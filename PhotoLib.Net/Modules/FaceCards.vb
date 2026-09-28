Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO

' Pictures for the face wall (new in the .NET port, face recognition P2). Aqua.MediaList shows files,
' so each face / card is saved once as a JPEG in the cache folder next to iPhoto.mdb (FaceCache\);
' the file name carries what the picture shows (face, count, name), so a changed card gets a new
' file and the old one is simply no longer used. The folder can be deleted at any time.
'   F<FaceID>.jpg                  -- the face, square, 160 px
'   P<PersonID>_<face>_<n>_<h>.jpg -- person card 320 x 240: round face, name, photo count
'   C<FaceID>_<n>.jpg              -- unnamed group card: round face, 「這是誰？」, face count
' The photo is read without its EXIF orientation, like the face boxes.
Public Module FaceCards

    Private Const CardWidth As Integer = 320
    Private Const CardHeight As Integer = 240
    Private Const CropSize As Integer = 160
    Private ReadOnly CardFont As String = "Microsoft JhengHei"

    ''' <summary>The face cut out of its photo; "" when the photo can't be read.</summary>
    Public Function FaceCrop(ByVal strCacheFolder As String, ByVal f As FaceRegion) As String
        Dim file As String = Path.Combine(strCacheFolder, "F" & f.FaceID & ".jpg")
        If IO.File.Exists(file) Then Return file
        Using bmp As Bitmap = CropBitmap(f, CropSize)
            If bmp Is Nothing Then Return ""
            Return If(SaveJpeg(bmp, file), file, "")
        End Using
    End Function

    ''' <summary>A person's card for the face wall.</summary>
    Public Function PersonCard(ByVal strCacheFolder As String, ByVal p As FaceCatalog.PersonEntry) As String
        Dim file As String = Path.Combine(strCacheFolder, $"P{p.PersonID}_{p.Cover.FaceID}_{p.PhotoCount}_{NameHash(p.Name)}.jpg")
        If IO.File.Exists(file) Then Return file
        Return MakeCard(file, p.Cover, p.Name, p.PhotoCount & " 張照片", Color.FromArgb(27, 35, 48))
    End Function

    ''' <summary>A card for a group of faces nobody is named on.</summary>
    Public Function GroupCard(ByVal strCacheFolder As String, ByVal group As List(Of FaceRegion)) As String
        Dim face As FaceRegion = group(0)   ' the clearest (groups are built from the best score down)
        Dim file As String = Path.Combine(strCacheFolder, $"C{face.FaceID}_{group.Count}.jpg")
        If IO.File.Exists(file) Then Return file
        Return MakeCard(file, face, "這是誰？", group.Count & " 張臉", Color.FromArgb(178, 75, 18))
    End Function

    Private Function MakeCard(ByVal file As String, ByVal face As FaceRegion, ByVal title As String, ByVal subtitle As String, ByVal titleColor As Color) As String
        Using crop As Bitmap = CropBitmap(face, 150),
              card As New Bitmap(CardWidth, CardHeight, PixelFormat.Format24bppRgb)
            Using g As Graphics = Graphics.FromImage(card)
                g.Clear(Color.White)
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.InterpolationMode = InterpolationMode.HighQualityBicubic
                Dim circle As New Rectangle((CardWidth - 150) \ 2, 12, 150, 150)
                If crop IsNot Nothing Then
                    Using path As New GraphicsPath()
                        path.AddEllipse(circle)
                        g.SetClip(path)
                        g.DrawImage(crop, circle)
                        g.ResetClip()
                    End Using
                Else
                    Using b As New SolidBrush(Color.FromArgb(230, 233, 238))
                        g.FillEllipse(b, circle)
                    End Using
                End If
                Using pen As New Pen(Color.FromArgb(200, 205, 213), 2)
                    g.DrawEllipse(pen, circle)
                End Using
                Using f1 As New Font(CardFont, 17, FontStyle.Bold), f2 As New Font(CardFont, 11)
                    TextRenderer.DrawText(g, title, f1, New Rectangle(0, 168, CardWidth, 36), titleColor,
                                          TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
                    TextRenderer.DrawText(g, subtitle, f2, New Rectangle(0, 204, CardWidth, 26), Color.FromArgb(100, 110, 124),
                                          TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                End Using
            End Using
            Return If(SaveJpeg(card, file), file, "")
        End Using
    End Function

    ''' <summary>A square around the face (1.6 x the box), <paramref name="size"/> px; Nothing when the
    ''' photo can't be read.</summary>
    Private Function CropBitmap(ByVal f As FaceRegion, ByVal size As Integer) As Bitmap
        Try
            Using fs As New FileStream(f.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                  src As Image = Image.FromStream(fs, False, False)
                Dim cx As Single = (f.Box.X + f.Box.Width / 2) * src.Width
                Dim cy As Single = (f.Box.Y + f.Box.Height / 2) * src.Height
                Dim side As Single = Math.Max(f.Box.Width * src.Width, f.Box.Height * src.Height) * 1.6F
                side = Math.Min(side, Math.Min(src.Width, src.Height))
                Dim x As Single = Math.Max(0, Math.Min(src.Width - side, cx - side / 2))
                Dim y As Single = Math.Max(0, Math.Min(src.Height - side, cy - side / 2))
                Dim bmp As New Bitmap(size, size, PixelFormat.Format24bppRgb)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.DrawImage(src, New Rectangle(0, 0, size, size), New RectangleF(x, y, side, side), GraphicsUnit.Pixel)
                End Using
                Return bmp
            End Using
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    Private Function SaveJpeg(ByVal bmp As Bitmap, ByVal file As String) As Boolean
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(file))
            Dim codec = ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = ImageFormat.Jpeg.Guid)
            Using ep As New EncoderParameters(1)
                ep.Param(0) = New EncoderParameter(Encoder.Quality, 90L)
                bmp.Save(file, codec, ep)
            End Using
            Return True
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is Runtime.InteropServices.ExternalException
            Return False
        End Try
    End Function

    Private Function NameHash(ByVal s As String) As String
        Dim h As UInteger = 2166136261UI
        For Each ch In If(s, "")
            h = CUInt((CULng(h Xor CUInt(AscW(ch))) * 16777619UL) And &HFFFFFFFFUL)
        Next
        Return h.ToString("x8")
    End Function

End Module
