Imports System.IO

' iPhoto.mdb -> iPhoto.db (new in the .NET port), once, at the first start of a version that has SQLite
' (Database.Provider.vb): iPhoto calls ToSqlite before it opens the database and from then on opens the
' file it returns (the settings point at it). In order:
'   1. the .mdb is opened once as before (Database.Construct): the full paths of an older one become
'      relative first, so the copy finds the photos from another drive letter too;
'   2. CopyMdbToSqlite makes iPhoto.db beside it, every row with its ids, the .mdb only read;
'   3. iPhoto.db is checked (integrity, relationships); anything wrong and it is thrown away: iPhoto goes
'      on with the .mdb and tries again next time;
'   4. the .mdb goes into Backup\ as iPhoto_before_SQLite_<time>.mdb -- kept, not used any more.
' An iPhoto.db already beside the .mdb (the settings couldn't be saved last time) is used as it is,
' unless the .mdb was changed after it was made: then it is set aside (.old-<time>) and made again.
' Settings still naming a .mdb that is gone while iPhoto.db is there: another iPhoto with its own
' iPhoto.Ini on the same library converted it (it moved the .mdb away); iPhoto.db is used.
' Nothing is done in read-only mode or for a SQLite file. Jet is needed for the .mdb: an x86 process.
Public Module DatabaseUpgrade

    Public Structure Result
        ''' <summary>The database to open now: the new iPhoto.db, or the one given when nothing was done.</summary>
        Public Database As String
        ''' <summary>True when iPhoto.db was made just now.</summary>
        Public Converted As Boolean
        ''' <summary>Why it couldn't be done ("" when it could, or there was nothing to do).</summary>
        Public [Error] As String
        ''' <summary>The rows copied, by table (and the views: 1 made, 0 left out).</summary>
        Public Counts As Dictionary(Of String, Integer)
    End Structure

    Public Function ToSqlite(ByVal strDatabase As String, Optional ByVal progress As Action(Of String, Integer) = Nothing) As Result
        Dim r As New Result With {.Database = strDatabase, .Error = ""}
        If String.IsNullOrEmpty(strDatabase) OrElse Database.IsSqliteFile(strDatabase) Then Return r
        Dim db As String = Path.ChangeExtension(strDatabase, ".db")
        If Not File.Exists(strDatabase) Then
            ' another iPhoto with its own settings on the same library (the Visual Studio build, a second
            ' copy) has converted it already: its iPhoto.db is the database now
            If File.Exists(db) Then r.Database = db
            Return r
        End If
        If g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly Then Return r
        Dim stamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
        Try
            If File.Exists(db) Then
                If File.GetLastWriteTimeUtc(strDatabase) <= File.GetCreationTimeUtc(db) Then
                    r.Database = db
                    Return r
                End If
                For Each suffix In {"", "-wal", "-shm"}
                    If File.Exists(db & suffix) Then File.Move(db & suffix, db & suffix & ".old-" & stamp)
                Next
            End If
            Using d As New Database
                d.Construct(strDatabase)   ' MigrateToRelativePaths
            End Using
            r.Counts = Database.CopyMdbToSqlite(strDatabase, db, progress)
            Dim problem As String = Database.CheckSqlite(db)
            If problem <> "" Then
                DeleteDatabase(db)
                r.Error = problem
                Return r
            End If
            r.Database = db
            r.Converted = True
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is Data.Common.DbException OrElse TypeOf ex Is InvalidOperationException
            DeleteDatabase(db)
            r.Error = ex.Message
            Return r
        End Try
        ' the .mdb out of the way, kept (a failure here only leaves it where it was)
        Try
            Dim folder As String = Maintenance.BackupFolder(strDatabase)
            Directory.CreateDirectory(folder)
            File.Move(strDatabase, Path.Combine(folder, "iPhoto_before_SQLite_" & stamp & ".mdb"))
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return r
    End Function

    Private Sub DeleteDatabase(ByVal db As String)
        For Each suffix In {"", "-wal", "-shm"}
            Try
                If File.Exists(db & suffix) Then File.Delete(db & suffix)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
    End Sub

End Module
