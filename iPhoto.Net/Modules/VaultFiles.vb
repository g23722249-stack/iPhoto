Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Security.Cryptography
Imports System.Xml.Linq

' 加密隱藏 / 解密顯示 (new in the .NET port), with PhotoVault (PhotoLib) doing the encryption:
'   加密隱藏  the photo as the album shows it -- with the original and earlier versions an edited picture
'             hides, each one's .Exif entry, Restore\ copy, PhotoEdit's .pedit.json / .pedx and a video's
'             Thumb\ picture -- goes encrypted into the vault (<library>\System\Vault). Each file is
'             decrypted once and compared before the plain one is deleted. Its PhotoIndex row, face rows,
'             Dock entry, photo book entries and disk thumbnails go; which books and whether it was docked
'             are kept (encrypted) to bring them back.
'   解密顯示  everything back where it was, as it was.
' While unlocked the list shows an item by its .ipx; Aqua.MediaViewerControl.ProvidedPicture gets its
' picture from ProvidePicture (decrypted in memory, with a lock in the corner, never on disk).
Friend Module VaultFiles

    ''' <summary>D:\生活剪輯\System\Database\iPhoto.db -> D:\生活剪輯\System\Vault (as ThumbCache).</summary>
    Friend ReadOnly Property Folder As String
        Get
            Dim db As String = If(g_lpConfig?.Attached(Config.enumAttachedFile.filDatabase), "")
            Dim dbFolder As String = If(String.IsNullOrEmpty(db), Nothing, Path.GetDirectoryName(Path.GetFullPath(db)))
            Dim parent As String = If(dbFolder Is Nothing, Nothing, Path.GetDirectoryName(dbFolder))
            If String.IsNullOrEmpty(parent) Then parent = If(dbFolder, Application.StartupPath)
            Return Path.Combine(parent, "Vault")
        End Get
    End Property

    ''' <summary>At start: the folder, the list's picture source, temporary files of an earlier run gone.</summary>
    Friend Sub Init()
        PhotoVault.Folder = Folder
        Aqua.MediaViewerControl.ProvidedPicture = AddressOf ProvidePicture
        PhotoVault.DeleteTempFiles()
    End Sub

    '==================================================================================================
    ' Pictures
    '==================================================================================================
    Private Const ListPictureMax As Integer = 1280

    ''' <summary>The list's picture of an item: at most 1280 pixels, a lock in the corner (worker threads).</summary>
    Private Function ProvidePicture(ByVal file As String) As Stream
        If Not PhotoVault.IsVaultItem(file) OrElse Not PhotoVault.IsUnlocked Then Return Nothing
        Try
            Using src As Bitmap = LoadPicture(file, ListPictureMax)
                If src Is Nothing Then Return Nothing
                DrawLock(src)
                Dim ms As New MemoryStream
                src.Save(ms, ImageFormat.Png)
                ms.Position = 0
                Return ms
            End Using
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is CryptographicException OrElse
                                   TypeOf ex Is ArgumentException OrElse TypeOf ex Is InvalidOperationException OrElse
                                   TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    ''' <summary>The item's picture (a video's: its Thumb\ picture, else a plain frame), no larger than
    ''' <paramref name="maxSide"/> when given; Nothing when it can't be read.</summary>
    Friend Function LoadPicture(ByVal item As String, Optional ByVal maxSide As Integer = 0) As Bitmap
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        If meta Is Nothing Then Return Nothing
        Dim source As String = item
        If meta.Video Then
            Dim thumb As PhotoVault.VaultSide = meta.Members(0).Sides.FirstOrDefault(Function(s) s.Path.StartsWith("Thumb\", StringComparison.OrdinalIgnoreCase))
            If thumb Is Nothing Then Return VideoFrame()
            source = PhotoVault.BlobPath(thumb.Blob)
        End If
        Using ms As MemoryStream = PhotoVault.DecryptToMemory(source)
            Using img As Image = Image.FromStream(ms)
                Dim scale As Double = 1
                If maxSide > 0 Then scale = Math.Min(1.0, maxSide / CDbl(Math.Max(img.Width, img.Height)))
                Dim w As Integer = Math.Max(1, CInt(img.Width * scale)), h As Integer = Math.Max(1, CInt(img.Height * scale))
                Dim bmp As New Bitmap(w, h, PixelFormat.Format32bppArgb)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality
                    g.DrawImage(img, 0, 0, w, h)
                End Using
                If meta.Video Then DrawPlay(bmp)
                Return bmp
            End Using
        End Using
    End Function

    Private Function VideoFrame() As Bitmap
        Dim bmp As New Bitmap(640, 360)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.FromArgb(40, 40, 40))
        End Using
        DrawPlay(bmp)
        Return bmp
    End Function

    Private Sub DrawPlay(ByVal bmp As Bitmap)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            Dim r As Single = Math.Min(bmp.Width, bmp.Height) * 0.12F
            Dim cx As Single = bmp.Width / 2.0F, cy As Single = bmp.Height / 2.0F
            Using b As New SolidBrush(Color.FromArgb(150, 0, 0, 0))
                g.FillEllipse(b, cx - r, cy - r, 2 * r, 2 * r)
            End Using
            g.FillPolygon(Brushes.White, {New PointF(cx - r * 0.35F, cy - r * 0.5F), New PointF(cx - r * 0.35F, cy + r * 0.5F), New PointF(cx + r * 0.55F, cy)})
        End Using
    End Sub

    ''' <summary>A padlock in the lower right corner: it is an encrypted photo.</summary>
    Private Sub DrawLock(ByVal bmp As Bitmap)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            Dim s As Single = Math.Max(18.0F, Math.Min(bmp.Width, bmp.Height) * 0.14F)
            Dim x As Single = bmp.Width - s * 1.25F, y As Single = bmp.Height - s * 1.35F
            Using back As New SolidBrush(Color.FromArgb(170, 0, 0, 0))
                g.FillEllipse(back, x - s * 0.2F, y - s * 0.1F, s * 1.4F, s * 1.4F)
            End Using
            Using pen As New Pen(Color.White, s * 0.11F)
                g.DrawArc(pen, x + s * 0.28F, y + s * 0.12F, s * 0.44F, s * 0.5F, 180, 180)
                g.DrawLine(pen, x + s * 0.28F, y + s * 0.37F, x + s * 0.28F, y + s * 0.52F)
                g.DrawLine(pen, x + s * 0.72F, y + s * 0.37F, x + s * 0.72F, y + s * 0.52F)
            End Using
            g.FillRectangle(Brushes.White, x + s * 0.18F, y + s * 0.52F, s * 0.64F, s * 0.5F)
            Using hole As New SolidBrush(Color.FromArgb(200, 0, 0, 0))
                g.FillEllipse(hole, x + s * 0.44F, y + s * 0.64F, s * 0.12F, s * 0.14F)
            End Using
        End Using
    End Sub

    '==================================================================================================
    ' 加密隱藏
    '==================================================================================================
    ''' <summary>The side files of one file, relative to its folder (those that exist).</summary>
    Private Function SidesOf(ByVal file As String) As List(Of String)
        Dim folder As String = Path.GetDirectoryName(file), name As String = Path.GetFileName(file), baseName As String = Path.GetFileNameWithoutExtension(file)
        Dim video As Boolean = GetMediaType(g_lpFileSystem, file) = enumPhotoMediaType.mdVideo
        Dim list As New List(Of String) From {Path.Combine("Restore", name), name & ".pedit.json"}
        If video Then list.Add(Path.Combine("Thumb", baseName & ".jpg")) Else list.Add(baseName & ".pedx")
        Return list.Where(Function(r) IO.File.Exists(Path.Combine(folder, r))).ToList()
    End Function

    ''' <summary>Encrypts <paramref name="source"/> into the vault file <paramref name="blob"/> and reads it
    ''' back: throws unless the decrypted content is the same.</summary>
    Private Sub EncryptVerified(ByVal source As String, ByVal blob As String)
        PhotoVault.EncryptFile(source, blob)
        Dim want As Byte(), got As Byte()
        Using fs As New FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read)
            want = SHA256.HashData(fs)
        End Using
        Using sha As SHA256 = SHA256.Create()
            Using cs As New CryptoStream(Stream.Null, sha, CryptoStreamMode.Write)
                PhotoVault.DecryptTo(blob, cs)
                cs.FlushFinalBlock()
            End Using
            got = sha.Hash
        End Using
        If Not want.SequenceEqual(got) Then Throw New CryptographicException("加密後讀回的內容不同：" & source)
    End Sub

    ''' <summary>The photo books (.Alm) listing <paramref name="file"/>.</summary>
    Private Function BooksWith(ByVal file As String) As List(Of String)
        Dim result As New List(Of String)
        For r = 0 To g_lpConfig.FavoriteCount - 1
            Dim root As String = g_lpConfig.FavoritePath(r)
            If Not Directory.Exists(root) Then Continue For
            Try
                For Each alm In Directory.GetFiles(root, "*." & gc_strFavoritesPattern, SearchOption.AllDirectories)
                    Dim folder As String = Path.GetDirectoryName(alm)
                    For Each ln As String In IO.File.ReadAllLines(alm, AnsiText.Encoding)
                        If ln.StartsWith("File=", StringComparison.OrdinalIgnoreCase) AndAlso
                           String.Equals(LibraryPath.FromRelative(ln.Substring(5).Trim(), folder), file, StringComparison.OrdinalIgnoreCase) Then
                            result.Add(alm)
                            Exit For
                        End If
                    Next
                Next
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        Return result
    End Function

    ''' <summary>加密隱藏 of one list file; the item's .ipx. Throws (IOException, CryptographicException,
    ''' UnauthorizedAccessException) when it can't be done -- then nothing changed.</summary>
    Friend Function Encrypt(ByVal file As String) As String
        Dim folder As String = Path.GetDirectoryName(file)
        Dim members As New List(Of String) From {file}
        members.AddRange(EditedPhotos.HiddenBehind(file).Where(Function(f) Not String.Equals(f, file, StringComparison.OrdinalIgnoreCase)))
        Dim p As New Photo
        p.Construct(file)
        Dim meta As New PhotoVault.VaultMeta With {
            .Name = Path.GetFileName(file), .Album = PhotoVault.StoredPath(folder),
            .Video = p.MediaType = enumPhotoMediaType.mdVideo, .Ranking = CInt(Val(p.Exif(enumPhotoExif.peRanking))),
            .ShotDate = p.Exif(enumPhotoExif.peDate), .Title = p.Exif(enumPhotoExif.peTitle), .Encrypted = DateTime.Now,
            .Docked = g_lpDock IsNot Nothing AndAlso g_lpDock.Selected(file)}
        Dim books As List(Of String) = BooksWith(file)
        meta.Books = books.Select(Function(b) PhotoVault.StoredPath(b)).ToList()

        Dim item As String = PhotoVault.NewItemPath()
        Dim sidePlain As New List(Of String)
        Try
            Dim n As Integer = 0
            For Each m In members
                Dim blob As String
                If m Is file Then
                    blob = Path.GetFileName(item)
                Else
                    n += 1
                    blob = PhotoVault.SideBlobName(item, n)
                End If
                EncryptVerified(m, PhotoVault.BlobPath(blob))
                Dim entry As XElement = ExifFile.GetEntry(m)
                Dim mem As New PhotoVault.VaultMember With {.Name = Path.GetFileName(m), .Blob = blob,
                                                            .Exif = entry?.ToString(SaveOptions.DisableFormatting)}
                Dim mFolder As String = Path.GetDirectoryName(m)
                For Each rel In SidesOf(m)
                    n += 1
                    Dim sideBlob As String = PhotoVault.SideBlobName(item, n)
                    EncryptVerified(Path.Combine(mFolder, rel), PhotoVault.BlobPath(sideBlob))
                    mem.Sides.Add(New PhotoVault.VaultSide With {.Path = rel, .Blob = sideBlob})
                    sidePlain.Add(Path.Combine(mFolder, rel))
                Next
                meta.Members.Add(mem)
            Next
            PhotoVault.SaveMeta(item, meta)
            ' the photo itself first: in use, it stays and nothing else changes
            If Not PhotoFiles.DeleteNow(file) Then Throw New IOException("檔案正在使用中：" & file)
        Catch
            PhotoVault.DeleteItemFiles(item)
            Throw
        End Try

        ' the plain files and every trace of them
        For Each m In members
            If m IsNot file Then PhotoFiles.DeleteNow(m)
            Try
                If ExifFile.RemoveEntry(m) Then PhotoFiles.DeleteNow(ExifFile.SidecarOf(m))
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
            g_lpDatabase?.Delete(m)
            g_lpDatabase?.DeleteFacePhoto(m)
        Next
        For Each s In sidePlain
            PhotoFiles.DeleteNow(s)
        Next
        If g_lpDock IsNot Nothing AndAlso g_lpDock.RemoveItem(file) Then g_lpDock.Save()
        For Each alm In books
            Try
                Dim b As New Book
                b.Construct(alm)
                b.RemovePhoto(file)
                b.Save()
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        ForgetThumbnails(members.Concat(sidePlain).ToList())
        Return item
    End Function

    ''' <summary>The disk thumbnails of files that are gone into the vault (in the background).</summary>
    Private Sub ForgetThumbnails(ByVal files As List(Of String))
        Dim cache As String = Aqua.ThumbnailDiskCache.Folder
        If String.IsNullOrEmpty(cache) Then cache = ThumbCache.Folder
        Threading.Tasks.Task.Run(Sub() Aqua.ThumbnailDiskCache.RemoveFiles(cache, files))
    End Sub

    '==================================================================================================
    ' 解密顯示, rating, delete
    '==================================================================================================
    ''' <summary>The file an item becomes again (its album may not exist any more: it is made).</summary>
    Friend Function OriginalPath(ByVal item As String) As String
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        If meta Is Nothing Then Return Nothing
        Return Path.Combine(PhotoVault.FullPath(meta.Album), meta.Name)
    End Function

    ''' <summary>解密顯示: every file back as it was; the photo's path. Throws when it can't be done (a file
    ''' of the name already there, the item damaged) -- the files written so far are removed then.</summary>
    Friend Function Decrypt(ByVal item As String) As String
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        If meta Is Nothing Then Throw New CryptographicException("加密照片無法讀取")
        Dim folder As String = PhotoVault.FullPath(meta.Album)
        Dim targets As New List(Of (Blob As String, Target As String))
        For Each m In meta.Members
            targets.Add((m.Blob, Path.Combine(folder, m.Name)))
            For Each s In m.Sides
                targets.Add((s.Blob, Path.Combine(folder, s.Path)))
            Next
        Next
        Dim taken As String = targets.Select(Function(t) t.Target).FirstOrDefault(Function(t) IO.File.Exists(t))
        If taken IsNot Nothing Then Throw New IOException("相簿裡已經有同名的檔案：" & taken)

        Dim written As New List(Of String)
        Try
            For Each t In targets
                Directory.CreateDirectory(Path.GetDirectoryName(t.Target))
                PhotoVault.DecryptFile(PhotoVault.BlobPath(t.Blob), t.Target)
                written.Add(t.Target)
            Next
        Catch
            For Each w In written
                PhotoFiles.DeleteNow(w)
            Next
            Throw
        End Try

        For Each m In meta.Members
            If m.Exif IsNot Nothing Then
                Try
                    ExifFile.PutEntry(Path.Combine(folder, m.Name), XElement.Parse(m.Exif))
                Catch ex As Exception When TypeOf ex Is System.Xml.XmlException OrElse TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                End Try
            End If
        Next
        Dim file As String = Path.Combine(folder, meta.Name)
        Dim p As New Photo
        p.Construct(file)
        If CInt(Val(p.Exif(enumPhotoExif.peRanking))) <> meta.Ranking Then p.Exif(enumPhotoExif.peRanking) = CStr(meta.Ranking)
        g_lpDatabase?.AddItem(p)
        If meta.Docked AndAlso g_lpDock IsNot Nothing Then
            g_lpDock.AddItem(file, p.Exif(enumPhotoExif.peDate), p.Exif(enumPhotoExif.peTime))
            g_lpDock.Save()
        End If
        For Each stored In meta.Books
            Dim alm As String = PhotoVault.FullPath(stored)
            If Not IO.File.Exists(alm) Then Continue For
            Try
                Dim b As New Book
                b.Construct(alm)
                b.AddPhoto(file)
                b.Save()
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        PhotoVault.DeleteItemFiles(item)
        g_lpFaces?.RequestOrganizeSoon()
        Return file
    End Function

    ''' <summary>顯示資訊 of an encrypted photo: its .Exif entry decrypted into a side file of its own in a
    ''' temporary folder, frmPhotoInfo on that, what it wrote encrypted again, the folder deleted at once.
    ''' True when changed.</summary>
    Friend Function EditInfo(ByVal item As String) As Boolean
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        If meta Is Nothing OrElse meta.Members.Count = 0 Then Throw New Security.Cryptography.CryptographicException("加密照片的資料讀不出來")
        Dim mem As PhotoVault.VaultMember = meta.Members(0)
        Dim folder As String = Path.Combine(Path.GetTempPath(), "iPhoto.Vault", "info-" & Guid.NewGuid().ToString("N"))
        Dim temp As String = Path.Combine(folder, meta.Name)
        Try
            Directory.CreateDirectory(folder)
            If mem.Exif IsNot Nothing Then ExifFile.PutEntry(temp, XElement.Parse(mem.Exif))
            Dim p As New Photo
            p.Construct(temp)
            Dim saved As Boolean
            Using f As New frmPhotoInfo
                saved = f.ShowVaultInfo(p, meta.Name)
            End Using
            If Not saved Then Return False
            mem.Exif = ExifFile.GetEntry(temp)?.ToString(SaveOptions.DisableFormatting)
            meta.Title = p.Exif(enumPhotoExif.peTitle)
            meta.ShotDate = p.Exif(enumPhotoExif.peDate)
            PhotoVault.SaveMeta(item, meta)
            Return True
        Finally
            Try
                If Directory.Exists(folder) Then Directory.Delete(folder, True)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        End Try
    End Function

    Friend Sub SetRanking(ByVal item As String, ByVal level As Integer)
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        If meta Is Nothing OrElse meta.Ranking = level Then Return
        meta.Ranking = level
        PhotoVault.SaveMeta(item, meta)
    End Sub

    Friend Function Ranking(ByVal item As String) As Integer
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        Return If(meta Is Nothing, 0, meta.Ranking)
    End Function

    ''' <summary>Deletes an item outright (an encrypted file in the Recycle Bin would be of no use).</summary>
    Friend Sub Delete(ByVal item As String)
        PhotoVault.DeleteItemFiles(item)
    End Sub

End Module
