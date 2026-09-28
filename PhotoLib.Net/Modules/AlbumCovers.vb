Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices

' Covers of the cover wall (new in the .NET port): when a first-level node of the tree (an Albums --
' 「2025年」, a photo-book folder) is clicked, the list shows one cover per child (a Class folder /
' a Book), made of a few of its photos printed on paper and scattered like a pile on a table.
'
'   Cover file  <section folder>\<child name>.jpg     e.g. 依日期排序\2025年\01月12日.jpg
'               (next to the child folders / .Alm files: nothing reads pictures there -- Albums lists
'               folders, a Class lists its own folder, the face scan walks Class folders). When that
'               folder can't be written (read-only mode, a CD, no rights) it goes to CacheFolder.
'   Card file   CacheFolder\K<hash>.jpg -- the cover with the name and photo count under it, what the
'               list shows (Aqua.MediaList shows files). The hash covers what the card shows, so a
'               changed card is a new file.
' A cover is made again when the child changed after it (folder / .Alm newer than the cover file) or
' on request (右鍵 重新產生封面). Photos: the best rated first, then spread over the set (first,
' middle ..., last); videos are skipped -- unless there are only videos: then the prints are frames
' taken at random points of them (Quartz.VideoFrame), new ones each time the cover is made. The
' scatter is seeded by the child's name, so a cover made again from the same photos looks the same.
' Everything here only reads the photos and may run on a worker thread (no shared state).
Public Module AlbumCovers

    Public Const CoverWidth As Integer = 1000
    Public Const CoverHeight As Integer = 750
    Private Const CardWidth As Integer = 800
    Private Const CardHeight As Integer = 720
    Private Const PrintCount As Integer = 4
    ''' <summary>The layout was drawn for a 600 px cover; everything scales with the cover.</summary>
    Private Const K As Single = CoverWidth / 600.0F
    ''' <summary>Bumped when the card's look changes, so old cards in the cache aren't used.</summary>
    Private Const CardVersion As String = "2"
    Private ReadOnly CardFont As String = "Microsoft JhengHei"
    Private ReadOnly Paper As Color = Color.White

    ''' <summary>Cards, placeholders and covers that can't be written next to the albums.</summary>
    Public ReadOnly Property CacheFolder As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "CoverCache")
        End Get
    End Property

    '==================================================================================================
    ' Where a cover lives and whether it is current
    '==================================================================================================
    ''' <summary>The folder / .Alm the child stands for.</summary>
    Public Function SourceOf(ByVal child As PhotoSet) As String
        Return child.Key
    End Function

    ''' <summary>"<section folder>\<child name>.jpg".</summary>
    Public Function CoverFile(ByVal sectionPath As String, ByVal child As PhotoSet) As String
        Return Path.Combine(sectionPath, child.Name & ".jpg")
    End Function

    ''' <summary>The same cover in the cache (when the section folder can't be written).</summary>
    Private Function CachedCoverFile(ByVal coverFile As String) As String
        Return Path.Combine(CacheFolder, "C" & Hash(coverFile.ToUpperInvariant()) & ".jpg")
    End Function

    ''' <summary>Drops the cover of <paramref name="child"/> (next to the albums and in the cache), so the
    ''' cover wall makes it again -- after 設為相本封面.</summary>
    Public Sub ForgetCover(ByVal sectionPath As String, ByVal child As PhotoSet)
        Dim f As String = CoverFile(sectionPath, child)
        For Each x In {f, CachedCoverFile(f)}
            Try
                If File.Exists(x) Then File.Delete(x)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
    End Sub

    ''' <summary>The cover to show now: the file next to the albums, else the cached one; "" when there is
    ''' none or it is older than the child (<paramref name="current"/> = False then too).</summary>
    Public Function ExistingCover(ByVal coverFile As String, ByVal source As String, ByRef current As Boolean) As String
        current = False
        For Each f In {coverFile, CachedCoverFile(coverFile)}
            If Not File.Exists(f) Then Continue For
            current = File.GetLastWriteTime(f) >= SourceTime(source) AndAlso ImageWidth(f) >= CoverWidth
            Return f
        Next
        Return ""
    End Function

    ''' <summary>Width of a picture file, reading only its header; 0 when it can't be read.</summary>
    Private Function ImageWidth(ByVal file As String) As Integer
        Try
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), img As Image = Image.FromStream(fs, False, False)
                Return img.Width
            End Using
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is UnauthorizedAccessException
            Return 0
        End Try
    End Function

    Private Function SourceTime(ByVal source As String) As DateTime
        Try
            If Directory.Exists(source) Then Return Directory.GetLastWriteTime(source)
            If File.Exists(source) Then Return File.GetLastWriteTime(source)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return DateTime.MinValue
    End Function

    '==================================================================================================
    ' Making a cover
    '==================================================================================================
    ''' <summary>Makes and saves the cover of <paramref name="child"/> (loads it; call on a worker thread).
    ''' Returns the file written ("" when the child has no pictures or nothing could be written).</summary>
    Public Function MakeCover(ByVal child As PhotoSet, ByVal coverFile As String, ByVal readOnlyMode As Boolean) As String
        child.Load()
        Dim picks As List(Of CoverPick) = PickPhotos(child).Select(Function(f) New CoverPick(f)).ToList()
        If picks.Count = 0 Then picks = PickVideoFrames(child)   ' only videos in it
        If picks.Count = 0 Then Return ""
        Using cover As Bitmap = DrawCover(child.Name, picks)
            If cover Is Nothing Then Return ""
            If Not readOnlyMode AndAlso SaveJpeg(cover, coverFile) Then
                Try
                    File.Delete(CachedCoverFile(coverFile))   ' an older fallback copy
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                End Try
                Return coverFile
            End If
            Dim cached As String = CachedCoverFile(coverFile)
            Return If(SaveJpeg(cover, cached), cached, "")
        End Using
    End Function

    ''' <summary>Up to PrintCount pictures, the one for the top of the pile last: the best rated ones
    ''' first, the rest spread evenly over the set.</summary>
    Public Function PickPhotos(ByVal child As PhotoSet) As List(Of String)
        Dim pics As New List(Of (File As String, Rating As Integer, Order As Integer))
        For i = 0 To child.PhotoCount - 1
            Dim p As Photo = child.Photo(i)
            If p Is Nothing OrElse p.MediaType <> enumPhotoMediaType.mdImage OrElse Not File.Exists(p.FileDesc) Then Continue For
            Dim r As Integer = CInt(Val(p.Exif(enumPhotoExif.peRanking)))
            pics.Add((p.FileDesc, If(r >= 1 AndAlso r <= 5, r, 0), pics.Count))
        Next
        If pics.Count = 0 Then Return New List(Of String)

        Dim n As Integer = Math.Min(PrintCount, pics.Count)
        Dim chosen As New List(Of (File As String, Rating As Integer, Order As Integer))
        chosen.AddRange(pics.Where(Function(x) x.Rating > 0).OrderByDescending(Function(x) x.Rating).ThenBy(Function(x) x.Order).Take(n))
        ' fill up with photos spread over the set (first, middle ..., last), then any not taken yet
        Dim spread As New List(Of Integer)
        For j = 0 To n - 1
            spread.Add(If(n = 1, pics.Count \ 2, CInt(Math.Round(j * (pics.Count - 1) / (n - 1)))))
        Next
        For Each idx In spread.Concat(Enumerable.Range(0, pics.Count))
            If chosen.Count >= n Then Exit For
            Dim order As Integer = pics(idx).Order
            If Not chosen.Any(Function(x) x.Order = order) Then chosen.Add(pics(idx))
        Next

        ' the top print: the photo chosen with 設為相本封面, else the best rated one, else the middle of the set
        Dim chosenCover As String = child.CoverPhoto
        Dim coverPic = pics.FirstOrDefault(Function(x) chosenCover <> "" AndAlso String.Equals(x.File, chosenCover, StringComparison.OrdinalIgnoreCase))
        Dim hasCover As Boolean = coverPic.File IsNot Nothing
        If hasCover AndAlso Not chosen.Any(Function(c) c.Order = coverPic.Order) Then
            chosen.RemoveAt(chosen.Count - 1)
            chosen.Add(coverPic)
        End If
        Dim top = If(hasCover, coverPic,
                  If(chosen.Any(Function(c) c.Rating > 0), chosen.OrderByDescending(Function(c) c.Rating).ThenBy(Function(c) c.Order).First(),
                                                            chosen.OrderBy(Function(c) Math.Abs(c.Order - pics.Count \ 2)).First()))
        Dim rest = chosen.Where(Function(c) c.Order <> top.Order).OrderBy(Function(c) c.Order).Select(Function(c) c.File).ToList()
        rest.Add(top.File)
        Return rest
    End Function

    ''' <summary>A print on the pile: a picture file, or a frame of a video.</summary>
    Public Structure CoverPick
        Public File As String
        ''' <summary>Where in the video (0..1); -1 for a picture.</summary>
        Public Fraction As Double

        Public Sub New(ByVal file As String, Optional ByVal fraction As Double = -1)
            Me.File = file
            Me.Fraction = fraction
        End Sub

        Public ReadOnly Property IsVideo As Boolean
            Get
                Return Fraction >= 0
            End Get
        End Property
    End Structure

    ''' <summary>For a set with videos only: PrintCount frames at random points, the videos taken in turn
    ''' (one video gives all the prints, from different parts of it). The first video's frame on top.</summary>
    Public Function PickVideoFrames(ByVal child As PhotoSet) As List(Of CoverPick)
        Dim videos As New List(Of String)
        For i = 0 To child.PhotoCount - 1
            Dim p As Photo = child.Photo(i)
            If p IsNot Nothing AndAlso p.MediaType = enumPhotoMediaType.mdVideo AndAlso File.Exists(p.FileDesc) Then videos.Add(p.FileDesc)
        Next
        Dim picks As New List(Of CoverPick)
        If videos.Count = 0 Then Return picks
        Dim rnd As New Random()
        Dim perVideo As Integer = CInt(Math.Ceiling(PrintCount / Math.Min(PrintCount, videos.Count)))
        For slot = PrintCount - 1 To 0 Step -1   ' the last one added (slot 0: the first video) goes on top
            Dim v As Integer = slot Mod videos.Count
            Dim part As Integer = slot \ videos.Count   ' which part of this video
            ' a random point inside its part of 5 % .. 95 %, so frames of one video differ
            Dim f As Double = 0.05 + 0.9 * (part + 0.15 + rnd.NextDouble() * 0.7) / perVideo
            picks.Add(New CoverPick(videos(v), f))
        Next
        Return picks
    End Function

    ''' <summary>The pile: every print but the last scattered (±15°), the last on top, nearly straight.</summary>
    Public Function DrawCover(ByVal name As String, ByVal files As List(Of String)) As Bitmap
        Return DrawCover(name, files.Select(Function(f) New CoverPick(f)).ToList())
    End Function

    Public Function DrawCover(ByVal name As String, ByVal picks As List(Of CoverPick)) As Bitmap
        Dim rnd As New Random(Seed(name))
        Dim cover As New Bitmap(CoverWidth, CoverHeight, PixelFormat.Format24bppRgb)
        Dim drawn As Integer = 0
        Using g As Graphics = Graphics.FromImage(cover)
            g.Clear(Paper)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            For i = 0 To picks.Count - 1
                Dim top As Boolean = i = picks.Count - 1
                ' draw the random numbers even for a photo that can't be read, so the others keep their place
                Dim angle As Single = If(top, CSng(rnd.NextDouble() * 8 - 4), CSng(rnd.NextDouble() * 30 - 15))
                Dim dx As Single = If(top, 0, CSng(rnd.NextDouble() * 150 - 75)) * K
                Dim dy As Single = If(top, 6, CSng(rnd.NextDouble() * 90 - 45)) * K
                Dim side As Integer = CInt(If(top, 330, 290) * K)
                Using photo As Bitmap = If(picks(i).IsVideo, Quartz.VideoFrame.Grab(picks(i).File, picks(i).Fraction, side), LoadPhoto(picks(i).File, side))
                    If photo Is Nothing Then Continue For
                    DrawPrint(g, photo, CoverWidth / 2.0F + dx, CoverHeight / 2.0F + dy, angle, If(top, 12, 10) * K)
                    drawn += 1
                End Using
            Next
        End Using
        If drawn = 0 Then
            cover.Dispose()
            Return Nothing
        End If
        Return cover
    End Function

    ''' <summary>A print: the photo on slightly warm paper with a thin edge and a soft shadow, rotated by
    ''' <paramref name="angle"/> around (cx, cy).</summary>
    Private Sub DrawPrint(ByVal g As Graphics, ByVal photo As Bitmap, ByVal cx As Single, ByVal cy As Single, ByVal angle As Single, ByVal border As Single)
        Dim w As Single = photo.Width + border * 2, h As Single = photo.Height + border * 2
        Dim state As GraphicsState = g.Save()
        g.TranslateTransform(cx, cy)
        g.RotateTransform(angle)
        For s = 6 To 1 Step -1
            Dim r As Single = s * K
            Using b As New SolidBrush(Color.FromArgb(10, 0, 0, 0))
                g.FillRectangle(b, -w / 2 + 3 * K - r, -h / 2 + 5 * K - r, w + r * 2, h + r * 2)
            End Using
        Next
        Using paperBrush As New LinearGradientBrush(New RectangleF(-w / 2, -h / 2, w, h), Color.FromArgb(255, 255, 253), Color.FromArgb(238, 236, 230), LinearGradientMode.ForwardDiagonal)
            g.FillRectangle(paperBrush, -w / 2, -h / 2, w, h)
        End Using
        g.DrawImage(photo, -w / 2 + border, -h / 2 + border, photo.Width, photo.Height)
        Using gloss As New LinearGradientBrush(New RectangleF(-w / 2, -h / 2, w, h), Color.FromArgb(40, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.ForwardDiagonal)
            g.FillRectangle(gloss, -w / 2 + border, -h / 2 + border, photo.Width, photo.Height)
        End Using
        Using edge As New Pen(Color.FromArgb(60, 0, 0, 0), K)
            g.DrawRectangle(edge, -w / 2, -h / 2, w, h)
        End Using
        g.Restore(state)
    End Sub

    ''' <summary>The photo turned by its EXIF orientation, fitted within max x max; Nothing when it can't be read.</summary>
    Private Function LoadPhoto(ByVal file As String, ByVal max As Integer) As Bitmap
        Try
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                  img As Image = Image.FromStream(fs, False, False)
                If img.PropertyIdList.Contains(&H112) Then
                    Select Case img.GetPropertyItem(&H112).Value(0)
                        Case 3 : img.RotateFlip(RotateFlipType.Rotate180FlipNone)
                        Case 6 : img.RotateFlip(RotateFlipType.Rotate90FlipNone)
                        Case 8 : img.RotateFlip(RotateFlipType.Rotate270FlipNone)
                    End Select
                End If
                Dim fit As Double = Math.Min(max / img.Width, max / img.Height)
                Dim bmp As New Bitmap(Math.Max(1, CInt(img.Width * fit)), Math.Max(1, CInt(img.Height * fit)), PixelFormat.Format24bppRgb)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.DrawImage(img, 0, 0, bmp.Width, bmp.Height)
                End Using
                Return bmp
            End Using
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is ExternalException
            Return Nothing
        End Try
    End Function

    '==================================================================================================
    ' Cards for the list
    '==================================================================================================
    ''' <summary>"57 張", "2 部影片", "12 張 · 3 部影片", "沒有照片" (the set loaded).</summary>
    Public Function CountText(ByVal child As PhotoSet) As String
        Dim pictures As Integer = 0, videos As Integer = 0
        For i = 0 To child.PhotoCount - 1
            Select Case child.Photo(i).MediaType
                Case enumPhotoMediaType.mdVideo : videos += 1
                Case Else : pictures += 1
            End Select
        Next
        If pictures = 0 AndAlso videos = 0 Then Return "沒有照片"
        If videos = 0 Then Return pictures & " 張"
        If pictures = 0 Then Return videos & " 部影片"
        Return pictures & " 張 · " & videos & " 部影片"
    End Function

    ''' <summary>The card the list shows: the cover (or a placeholder when <paramref name="coverFile"/> is
    ''' "") with the name and <paramref name="subtitle"/> under it.</summary>
    Public Function Card(ByVal coverFile As String, ByVal title As String, ByVal subtitle As String) As String
        Dim stamp As String = If(coverFile <> "" AndAlso File.Exists(coverFile), File.GetLastWriteTimeUtc(coverFile).Ticks.ToString(), "-")
        Dim cardFile As String = Path.Combine(CacheFolder, "K" & Hash(CardVersion & "|" & coverFile.ToUpperInvariant() & "|" & stamp & "|" & title & "|" & subtitle) & ".jpg")
        If IO.File.Exists(cardFile) Then Return cardFile
        Using bmp As New Bitmap(CardWidth, CardHeight, PixelFormat.Format24bppRgb)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.Clear(Paper)
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.InterpolationMode = InterpolationMode.HighQualityBicubic
                Dim picH As Integer = CardWidth * CoverHeight \ CoverWidth
                Dim drawn As Boolean = False
                If coverFile <> "" Then
                    Try
                        Using fs As New FileStream(coverFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), img As Image = Image.FromStream(fs)
                            g.DrawImage(img, 0, 0, CardWidth, picH)
                            drawn = True
                        End Using
                    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is UnauthorizedAccessException
                    End Try
                End If
                If Not drawn Then DrawPlaceholder(g, New Rectangle(0, 0, CardWidth, picH))
                ' grey anti-aliasing, not ClearType: the list scales the card, and ClearType's coloured
                ' edges turn to blur when scaled (and JPEG smears them)
                g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
                Using f1 As New Font(CardFont, 38.0F, FontStyle.Bold, GraphicsUnit.Pixel), f2 As New Font(CardFont, 27.0F, FontStyle.Regular, GraphicsUnit.Pixel),
                      sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center, .Trimming = StringTrimming.EllipsisCharacter, .FormatFlags = StringFormatFlags.NoWrap},
                      b1 As New SolidBrush(Color.FromArgb(40, 46, 56)), b2 As New SolidBrush(Color.FromArgb(104, 112, 124))
                    g.DrawString(title, f1, b1, New RectangleF(12, picH + 4, CardWidth - 24, 62), sf)
                    g.DrawString(subtitle, f2, b2, New RectangleF(12, picH + 64, CardWidth - 24, 44), sf)
                End Using
            End Using
            Return If(SaveJpeg(bmp, cardFile, 95L), cardFile, "")
        End Using
    End Function

    ''' <summary>While the cover is being made / when there are no pictures: three empty prints.</summary>
    Private Sub DrawPlaceholder(ByVal g As Graphics, ByVal r As Rectangle)
        Dim cx As Single = r.X + r.Width / 2.0F, cy As Single = r.Y + r.Height / 2.0F
        For Each a In {-9.0F, 6.0F, -1.0F}
            Dim state As GraphicsState = g.Save()
            g.TranslateTransform(cx, cy)
            g.RotateTransform(a)
            Using b As New SolidBrush(Color.FromArgb(245, 246, 248)), p As New Pen(Color.FromArgb(214, 218, 224), 3.0F)
                g.FillRectangle(b, -220, -160, 440, 320)
                g.DrawRectangle(p, -220, -160, 440, 320)
            End Using
            g.Restore(state)
        Next
    End Sub

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    Private Function SaveJpeg(ByVal bmp As Bitmap, ByVal file As String, Optional ByVal quality As Long = 92L) As Boolean
        Dim tmp As String = file & ".tmp"
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(file))
            Dim codec = ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = ImageFormat.Jpeg.Guid)
            Using ep As New EncoderParameters(1)
                ep.Param(0) = New EncoderParameter(Encoder.Quality, quality)
                bmp.Save(tmp, codec, ep)
            End Using
            ' replace in one step, so the list never reads half a file
            If IO.File.Exists(file) Then IO.File.Delete(file)
            IO.File.Move(tmp, file)
            Return True
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is ExternalException
            Try
                If IO.File.Exists(tmp) Then IO.File.Delete(tmp)
            Catch ex2 As Exception When TypeOf ex2 Is IOException OrElse TypeOf ex2 Is UnauthorizedAccessException
            End Try
            Return False
        End Try
    End Function

    Private Function Seed(ByVal s As String) As Integer
        Dim h As Long = 17
        For Each ch In If(s, "")
            h = (h * 31 + AscW(ch)) Mod 2147483647L
        Next
        Return CInt(h)
    End Function

    ''' <summary>Two 32-bit FNV-1a hashes (forwards / backwards) -- enough to name cache files.</summary>
    Private Function Hash(ByVal s As String) As String
        s = If(s, "")
        Dim a As UInteger = 2166136261UI, b As UInteger = 2166136261UI
        For i = 0 To s.Length - 1
            a = CUInt((CULng(a Xor CUInt(AscW(s(i)))) * 16777619UL) And &HFFFFFFFFUL)
            b = CUInt((CULng(b Xor CUInt(AscW(s(s.Length - 1 - i)))) * 16777619UL) And &HFFFFFFFFUL)
        Next
        Return a.ToString("x8") & b.ToString("x8")
    End Function

End Module
