Imports System.IO

' PhotoIndex kept in line with the .Exif files (new in the .NET port): the .Exif is what counts, iPhoto
' writes it first, so a photo whose row differs (or has none) had its .Exif changed outside iPhoto -- put
' back from a backup, copied in with File Explorer, edited by another program -- and gets its row written
' again. Run in the background at start, at most once a day (Due); its own Database (worker thread).
Public Module IndexSync

    Public Const EveryHours As Integer = 24

    Public Class Result
        Public Checked As Integer
        Public Updated As Integer
        Public Failed As Integer
        Public LastError As String = ""
    End Class

    Private Function StampFile(ByVal strDatabase As String) As String
        Return Path.Combine(Maintenance.BackupFolder(strDatabase), "IndexSync.stamp")
    End Function

    ''' <summary>True when the last sync is more than EveryHours ago (or there was none).</summary>
    Public Function Due(ByVal strDatabase As String) As Boolean
        Dim f As String = StampFile(strDatabase)
        Return Not File.Exists(f) OrElse (DateTime.Now - File.GetLastWriteTime(f)).TotalHours >= EveryHours
    End Function

    ''' <summary>Every photo of the albums under <paramref name="roots"/> that has a .Exif: its PhotoIndex
    ''' row written again when it differs from the .Exif or is missing.</summary>
    Public Function Run(ByVal strDatabase As String, ByVal roots As IEnumerable(Of String),
                        Optional ByVal cancel As Threading.CancellationToken = Nothing) As Result
        Dim r As New Result
        Using db As New Database
            db.Construct(strDatabase)
            If Not db.Implement Then Return r
            Dim rows As Dictionary(Of String, Data.DataRow) = db.IndexRowsByFile()
            For Each f In Duplicates.AlbumPhotos(roots)
                If cancel.IsCancellationRequested Then Return r
                Try
                    If Not New ExifFile(f).Exists Then Continue For
                    r.Checked += 1
                    Dim p As New Photo
                    p.Construct(f)
                    Dim row As Data.DataRow = Nothing
                    If rows.TryGetValue(f, row) AndAlso Not db.IndexDiffers(p, row) Then Continue For
                    db.WriteImportedPhoto(p)   ' delete + insert, retried while the other connection holds a lock
                    r.Updated += 1
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is Data.OleDb.OleDbException
                    r.Failed += 1
                    r.LastError = Path.GetFileName(f) & "：" & ex.Message
                End Try
            Next
        End Using
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(StampFile(strDatabase)))
            File.WriteAllText(StampFile(strDatabase), DateTime.Now.ToString("s"))
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return r
    End Function

End Module
