Imports System.IO
Imports System.IO.Compression

' Keeping the data safe (new in the .NET port), all into <database folder>\Backup:
'   iPhoto_yyyyMMdd_HHmmss.mdb   a copy of iPhoto.mdb made at every start before it is opened (unless it
'                                hasn't changed since the newest copy); the newest DatabaseKeep are kept.
'   Exif_yyyyMMdd_HHmmss.zip     every .Exif side file under the album roots (entry names are the full
'                                paths without the drive colon, "D\生活剪輯\Album\...\x.Exif", so a file
'                                can be put back where it was); made every ExifEveryDays days in the
'                                background, or on request (設定 › 維護); the newest ExifKeep are kept.
' And the records of photos that are gone (deleted / moved outside iPhoto): PhotoIndex rows and face
' data (FindOrphans / CleanOrphans, 設定 › 維護, after a question).
Public Module Maintenance

    Public Const DatabaseKeep As Integer = 10
    Public Const ExifEveryDays As Integer = 7
    Public Const ExifKeep As Integer = 8

    Public Function BackupFolder(ByVal strDatabase As String) As String
        Return Path.Combine(Path.GetDirectoryName(strDatabase), "Backup")
    End Function

    '==================================================================================================
    ' iPhoto.mdb
    '==================================================================================================
    ''' <summary>Copies the database into Backup (before it is opened). Nothing is copied when the newest
    ''' copy is the same (size and time). Returns the copy made, or "" (none needed / failed).</summary>
    Public Function BackupDatabase(ByVal strDatabase As String) As String
        Try
            If String.IsNullOrEmpty(strDatabase) OrElse Not File.Exists(strDatabase) Then Return ""
            Dim folder As String = BackupFolder(strDatabase)
            Directory.CreateDirectory(folder)
            Dim src As New FileInfo(strDatabase)
            Dim newest As FileInfo = Backups(folder, "iPhoto_*.mdb").FirstOrDefault()
            If newest IsNot Nothing AndAlso newest.Length = src.Length AndAlso newest.LastWriteTimeUtc = src.LastWriteTimeUtc Then Return ""
            Dim target As String = Path.Combine(folder, "iPhoto_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".mdb")
            ' shared read: another program may have it open; the copy keeps the source's time (see above)
            Using input As New FileStream(strDatabase, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                  output As New FileStream(target & ".tmp", FileMode.Create, FileAccess.Write)
                input.CopyTo(output)
            End Using
            File.Move(target & ".tmp", target)
            File.SetLastWriteTimeUtc(target, src.LastWriteTimeUtc)
            Prune(folder, "iPhoto_*.mdb", DatabaseKeep)
            Return target
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return ""
        End Try
    End Function

    '==================================================================================================
    ' .Exif side files
    '==================================================================================================
    ''' <summary>When the newest .Exif backup was made (Nothing when there is none).</summary>
    Public Function LastExifBackup(ByVal strDatabase As String) As DateTime?
        Dim newest As FileInfo = Backups(BackupFolder(strDatabase), "Exif_*.zip").FirstOrDefault()
        Return If(newest Is Nothing, CType(Nothing, DateTime?), newest.LastWriteTime)
    End Function

    Public Function ExifBackupDue(ByVal strDatabase As String) As Boolean
        Dim last As DateTime? = LastExifBackup(strDatabase)
        Return Not last.HasValue OrElse (DateTime.Now - last.Value).TotalDays >= ExifEveryDays
    End Function

    ''' <summary>Zips every .Exif under <paramref name="roots"/> into Backup (may run on a worker thread).
    ''' Returns the zip and how many files went in; ("", 0) when nothing could be written.</summary>
    Public Function BackupExif(ByVal strDatabase As String, ByVal roots As IEnumerable(Of String)) As (Zip As String, Count As Integer)
        Dim folder As String = BackupFolder(strDatabase)
        Dim target As String = Path.Combine(folder, "Exif_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".zip")
        Dim count As Integer = 0
        Try
            Directory.CreateDirectory(folder)
            Using zip As ZipArchive = ZipFile.Open(target & ".tmp", ZipArchiveMode.Create)
                For Each root In roots.Where(Function(r) Directory.Exists(r)).Distinct(StringComparer.OrdinalIgnoreCase)
                    For Each file In SafeFiles(root, "*.Exif")
                        Try
                            zip.CreateEntryFromFile(file, EntryName(file), CompressionLevel.Optimal)
                            count += 1
                        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                            ' one unreadable file doesn't stop the backup
                        End Try
                    Next
                Next
            End Using
            If File.Exists(target) Then File.Delete(target)
            File.Move(target & ".tmp", target)
            Prune(folder, "Exif_*.zip", ExifKeep)
            Return (target, count)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Try
                If File.Exists(target & ".tmp") Then File.Delete(target & ".tmp")
            Catch ex2 As Exception When TypeOf ex2 Is IOException OrElse TypeOf ex2 Is UnauthorizedAccessException
            End Try
            Return ("", 0)
        End Try
    End Function

    ''' <summary>"D:\a\b.Exif" -> "D/a/b.Exif" (a UNC path keeps its server and share).</summary>
    Private Function EntryName(ByVal file As String) As String
        Dim full As String = Path.GetFullPath(file)
        If full.Length >= 2 AndAlso full(1) = ":"c Then full = full(0) & full.Substring(2)
        Return full.TrimStart("\"c).Replace("\"c, "/"c)
    End Function

    ''' <summary>Files below <paramref name="root"/>, skipping folders that can't be read.</summary>
    Private Iterator Function SafeFiles(ByVal root As String, ByVal pattern As String) As IEnumerable(Of String)
        Dim pending As New Stack(Of String)
        pending.Push(root)
        While pending.Count > 0
            Dim dir As String = pending.Pop()
            Dim files() As String = Nothing, subs() As String = Nothing
            Try
                files = Directory.GetFiles(dir, pattern)
                subs = Directory.GetDirectories(dir)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                Continue While
            End Try
            For Each f In files
                Yield f
            Next
            For Each s In subs
                pending.Push(s)
            Next
        End While
    End Function

    '==================================================================================================
    ' Records of photos that are gone
    '==================================================================================================
    ''' <summary>Photo files named in PhotoIndex / the face tables that no longer exist.</summary>
    Public Function FindOrphans(ByVal db As Database) As (Index As List(Of String), Faces As List(Of String))
        Dim index As New List(Of String), faces As New List(Of String)
        If db Is Nothing OrElse Not db.Implement Then Return (index, faces)
        Dim all() As String = Nothing
        db.SearchFileArrayList("1 = 1", all)
        index.AddRange(all.Where(Function(f) f <> "" AndAlso Not File.Exists(f)))
        If db.FaceTablesReady Then faces.AddRange(db.LoadFacePhotoStates().Keys.Where(Function(f) Not File.Exists(f)))
        Return (index, faces)
    End Function

    ''' <summary>Removes those records; returns how many photos' records went.</summary>
    Public Function CleanOrphans(ByVal db As Database, ByVal orphans As (Index As List(Of String), Faces As List(Of String))) As Integer
        For Each f In orphans.Index
            db.Delete(f)
        Next
        For Each f In orphans.Faces
            db.DeleteFacePhoto(f)
        Next
        Return orphans.Index.Union(orphans.Faces, StringComparer.OrdinalIgnoreCase).Count()
    End Function

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    ''' <summary>The backups matching <paramref name="pattern"/>, newest first (by name: the time is in it).</summary>
    Public Function Backups(ByVal folder As String, ByVal pattern As String) As List(Of FileInfo)
        If Not Directory.Exists(folder) Then Return New List(Of FileInfo)
        Return New DirectoryInfo(folder).GetFiles(pattern).OrderByDescending(Function(f) f.Name, StringComparer.Ordinal).ToList()
    End Function

    Private Sub Prune(ByVal folder As String, ByVal pattern As String, ByVal keep As Integer)
        For Each old In Backups(folder, pattern).Skip(keep)
            Try
                old.Delete()
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
    End Sub

End Module
