' 設定 › 地點 「補寫全部照片的 GPS／地點」 (new in the .NET port): every picture of the albums gets, in its
' .Exif, the GPS read from the picture and 地點 / Country / City / Town from it (Database.FillPlace:
' an attraction within 200 m first, else the county and district) -- only where the field is blank;
' nothing typed is replaced. A photo whose .Exif changed gets its PhotoIndex row written again.
' The caller backs the .Exif files up first (Maintenance.BackupExif). Runs on a worker thread: it opens
' its own Database.
Public Module PlaceFill

    Public Class Result
        Public Checked As Integer        ' pictures looked at
        Public WithGps As Integer        ' of them with a GPS position
        Public Changed As Integer        ' .Exif files written
        Public Attractions As Integer    ' of them now with an attraction as 地點
        Public Failed As Integer
        Public LastError As String = ""
    End Class

    ''' <summary><paramref name="progress"/> gets (done, total) every few photos.</summary>
    Public Function FillAll(ByVal strDatabase As String, ByVal roots As IEnumerable(Of String),
                            Optional ByVal progress As Action(Of Integer, Integer) = Nothing,
                            Optional ByVal cancel As Threading.CancellationToken = Nothing) As Result
        Dim r As New Result
        Dim files As List(Of String) = Duplicates.AlbumPhotos(roots).Where(Function(f) FaceLibrary.IsPicture(f)).ToList()
        Dim db As New Database
        Try
            db.Construct(strDatabase)
            For i = 0 To files.Count - 1
                If cancel.IsCancellationRequested Then Exit For
                Try
                    Dim p As New Photo
                    p.Construct(files(i))
                    Dim had As String = p.Exif(enumPhotoExif.peSpot)
                    If Database.FillPlace(p) Then
                        r.Changed += 1
                        If had = "" AndAlso p.Exif(enumPhotoExif.peSpot) <> p.Exif(enumPhotoExif.peCity) Then r.Attractions += 1
                        If db.Implement Then db.WriteImportedPhoto(p)
                    End If
                    If p.Exif(enumPhotoExif.peGps) <> "" Then r.WithGps += 1
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is ArgumentException
                    r.Failed += 1
                    r.LastError = IO.Path.GetFileName(files(i)) & "：" & ex.Message
                End Try
                r.Checked += 1
                If progress IsNot Nothing AndAlso (i Mod 50 = 0 OrElse i = files.Count - 1) Then progress(i + 1, files.Count)
            Next
        Finally
            db.Dispose()
        End Try
        Return r
    End Function

End Module
