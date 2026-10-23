Imports System.Data
Imports System.Data.Common
Imports System.Data.OleDb

' Port of 電子相簿\Lib\Class\Database.cls: the photo index in iPhoto.mdb (Jet 4.0), used for the
' calendar, "faces" and search. ADODB -> System.Data.OleDb (Jet, 32-bit only; a 64-bit process uses ACE: MdbProvider).
' The same tables can be in a SQLite iPhoto.db instead (Database.Provider.vb, chosen by the file's
' extension); the code below is written once for both.
'
' Tables: PhotoIndex (FileName, Create_*/Exif_Date|Year|Month|Day as text, Exif_Title/Character/Spot/
'         Remark/KeyWord text, Exif_Ranking integer, ModifyTime date) and FaceIndex (FaceName, FileName).
'
' Differences from VB6:
'   - Values are passed as parameters, not pasted into the SQL: a name containing ' broke VB6's queries.
'     SearchFileArrayList / SearchFileRecordSet still take a WHERE clause built by the caller (frmSearch).
'   - SearchFileRecordSet hands back a DataTable instead of a live ADODB.Recordset.
'   - SetYearChanged / SetMonthChanged ask once (SELECT DISTINCT) instead of once per month / per day.
'   - TruncateTable deletes every row; VB6's "Where FileName <> Null" is never true in Jet, so it
'     deleted nothing (Rebuild isn't called by iPhoto -- CreateServerObject has it commented out).
'   - FileName (PhotoIndex, FacePhoto, FaceRegion, FaceIndex) is stored relative to the folder of
'     iPhoto.mdb when on the same drive (LibraryPath), so the library works from another drive letter.
'     Callers still pass and get full paths: ToStored / FromStored convert at every FileName here.
'     Construct converts the full paths of an older .mdb once (MigrateToRelativePaths).
Public Class Database
    Implements IDisposable

    Private Const mc_strTableName As String = "PhotoIndex"

    Private m_lpConnection As DbConnection   ' OleDbConnection (iPhoto.mdb) or SqliteConnection (iPhoto.db): Database.Provider.vb
    Private m_bolImplement As Boolean = False
    Private m_strBaseFolder As String = ""   ' the folder of iPhoto.mdb: FileName is relative to it

    ''' <summary>True once Construct opened the database.</summary>
    Public ReadOnly Property Implement As Boolean
        Get
            Return m_bolImplement
        End Get
    End Property

    Public ReadOnly Property Connection As DbConnection
        Get
            Return m_lpConnection
        End Get
    End Property

    ''' <summary>Opens the .mdb (or a SQLite .db: Database.Provider.vb); a missing file or a failed open just
    ''' leaves Implement = False (VB6).</summary>
    Public Sub Construct(ByVal szFileDesc As String)
        Close()
        m_bolImplement = False
        If String.IsNullOrEmpty(szFileDesc) OrElse Not IO.File.Exists(szFileDesc) Then Return
        m_strBaseFolder = IO.Path.GetDirectoryName(IO.Path.GetFullPath(szFileDesc))
        m_bolSqlite = IsSqliteFile(szFileDesc)
        If m_bolSqlite Then
            ' made by CreateSqlite with every column, and relative FileName from the start
            Try
                m_lpConnection = OpenSqlite(szFileDesc, create:=False)
                m_bolImplement = True
            Catch ex As Exception When TypeOf ex Is DbException OrElse TypeOf ex Is InvalidOperationException
                Close()
                Return
            End Try
            m_bolGpsColumns = True
            m_bolPlaceColumns = True
            m_bolRelativeNames = True
            Return
        End If
        MigrateToRelativePaths(szFileDesc)   ' before the shared connection: it needs the file to itself
        Try
            m_lpConnection = New OleDbConnection(MdbConnectionString(szFileDesc))
            m_lpConnection.Open()
            m_bolImplement = True
        Catch
            Close()
            Return
        End Try
        EnsureGpsColumns()
        EnsurePlaceColumns()
        ' converted (or nothing to convert): relative; otherwise full paths, as the rows still are
        m_bolRelativeNames = PendingSteps(DirectCast(m_lpConnection, OleDbConnection)).Count = 0
    End Sub

    '==================================================================================================
    ' Relative FileName (new in the .NET port)
    '==================================================================================================
    Private m_bolRelativeNames As Boolean = True   ' False: the .mdb still has full paths to convert

    ''' <summary>A full path as FileName stores it (relative to the folder of iPhoto.mdb when on its drive).
    ''' Full paths as they are while the .mdb still holds full ones (see MigrateToRelativePaths).</summary>
    Public Function ToStored(ByVal strFileDesc As String) As String
        If Not m_bolRelativeNames Then Return strFileDesc
        Return LibraryPath.ToRelative(strFileDesc, m_strBaseFolder)
    End Function

    ''' <summary>A FileName value as a full path again.</summary>
    Public Function FromStored(ByVal strStored As String) As String
        Return LibraryPath.FromRelative(strStored, m_strBaseFolder)
    End Function

    ''' <summary>The full paths an older iPhoto.mdb holds become relative, in a few statements per table
    ''' (no row by row: FileName isn't indexed). For each folder the .mdb is in, innermost first, the
    ''' rows under it get "..\" once per level instead of the folder: with iPhoto.mdb in
    ''' D:\生活剪輯\System\Database, D:\生活剪輯\Album\x.jpg becomes ..\..\Album\x.jpg. Rows of a library
    ''' copied to another drive letter (E:\生活剪輯\...) still read D:\生活剪輯\...: for every folder but
    ''' the drive itself the drive letter isn't compared, so they are converted too. A path elsewhere on
    ''' another drive stays as it is.
    ''' All four tables in one transaction, on a connection of its own opened exclusively: a table left
    ''' with full paths while the others are relative would no longer be found (FileName links them), and
    ''' shared, a transaction this size runs out of locks ("File sharing lock count exceeded", ~75,000
    ''' rows). When it can't be done (read-only, the file in use, a relationship that refuses the update)
    ''' nothing changes; Construct then keeps FileName as full paths until a later start converts it.</summary>
    Private Sub MigrateToRelativePaths(ByVal szFileDesc As String)
        If g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly Then Return
        Try
            Using cn As New OleDbConnection(MdbConnectionString(szFileDesc, ";Mode=Share Exclusive"))
                cn.Open()
                Dim pending As Dictionary(Of String, List(Of MigrationStep)) = PendingSteps(cn)
                If pending.Count = 0 Then Return
                Using tx As OleDbTransaction = cn.BeginTransaction()
                    For Each kv In pending
                        For Each s In kv.Value
                            Using cmd As New OleDbCommand("Update " & kv.Key & " Set FileName = ? & Mid(FileName, ?) Where FileName Like ?", cn, tx)
                                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.VarWChar, .Value = s.Dots})
                                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Integer, .Value = s.Prefix.Length + 1})
                                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.VarWChar, .Value = s.LikePattern})
                                cmd.ExecuteNonQuery()
                            End Using
                        Next
                    Next
                    tx.Commit()
                End Using
            End Using
        Catch ex As Exception When TypeOf ex Is DbException OrElse TypeOf ex Is InvalidOperationException
            ' in use, or rolled back: still full paths everywhere
        End Try
    End Sub

    ''' <summary>One Update of MigrateToRelativePaths: the rows under Prefix (with any drive letter when
    ''' AnyDrive) get Dots instead of it.</summary>
    Private Structure MigrationStep
        Public Dots As String
        Public Prefix As String
        Public AnyDrive As Boolean

        Public ReadOnly Property LikePattern As String
            Get
                Dim escaped As String = Prefix.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")   ' taken literally
                Return If(AnyDrive, "_" & escaped.Substring(1), escaped) & "%"
            End Get
        End Property

        ''' <summary>The same test as LikePattern (Jet's Like ignores case too), done here.</summary>
        Public Function Matches(ByVal strFileName As String) As Boolean
            If strFileName.Length < Prefix.Length Then Return False
            If Not AnyDrive Then Return strFileName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            Return String.Compare(strFileName, 1, Prefix, 1, Prefix.Length - 1, StringComparison.OrdinalIgnoreCase) = 0
        End Function
    End Structure

    ''' <summary>The Updates, one or two per folder the .mdb is in, innermost first.</summary>
    Private Function MigrationSteps() As List(Of MigrationStep)
        Dim steps As New List(Of MigrationStep)
        Dim d As String = m_strBaseFolder
        Dim level As Integer = 0
        While Not String.IsNullOrEmpty(d)
            Dim prefix As String = d.TrimEnd("\"c) & "\"
            Dim dots As String = String.Concat(Enumerable.Repeat("..\", level))
            Dim parent As String = IO.Path.GetDirectoryName(d)
            steps.Add(New MigrationStep With {.Dots = dots, .Prefix = prefix})   ' this drive
            ' any drive letter, but not for the drive itself (that would take in a whole other drive)
            If Not String.IsNullOrEmpty(parent) AndAlso prefix(1) = ":"c Then
                steps.Add(New MigrationStep With {.Dots = dots, .Prefix = prefix, .AnyDrive = True})
            End If
            d = parent
            level += 1
        End While
        Return steps
    End Function

    ''' <summary>By table, the Updates that have rows to convert (in their order). The full paths are read
    ''' with one plain Like per table and matched here: a Where with a parameter per folder took Jet
    ''' minutes. After the conversion only the few full paths elsewhere are read.</summary>
    Private Function PendingSteps(ByVal cn As OleDbConnection) As Dictionary(Of String, List(Of MigrationStep))
        Dim steps As List(Of MigrationStep) = MigrationSteps()
        Dim result As New Dictionary(Of String, List(Of MigrationStep))
        For Each table In {mc_strTableName, "FacePhoto", "FaceRegion", "FaceIndex"}
            Dim used As New HashSet(Of Integer)
            Try
                Using cmd As New OleDbCommand("Select Distinct FileName From " & table & " Where FileName Like '_:\%' Or FileName Like '\\%'", cn)
                    Using r As DbDataReader = cmd.ExecuteReader()
                        While r.Read() AndAlso used.Count < steps.Count
                            Dim f As String = AsText(r(0))
                            Dim i As Integer = steps.FindIndex(Function(s) s.Matches(f))   ' the first that takes it
                            If i >= 0 Then used.Add(i)
                        End While
                    End Using
                End Using
            Catch ex As DbException
                Continue For   ' not there
            End Try
            If used.Count > 0 Then result(table) = used.OrderBy(Function(i) i).Select(Function(i) steps(i)).ToList()
        Next
        Return result
    End Function

    ' [Exif] Country / City / Town (PlaceNames.Resolve) as PhotoIndex.Exif_Country / Exif_City / Exif_Town,
    ' so 地點 searches find 「新社」 in a photo whose 地點 is 「薰衣草森林」. Added like the GPS columns.
    Private m_bolPlaceColumns As Boolean

    Public ReadOnly Property HasPlaceColumns As Boolean
        Get
            Return m_bolPlaceColumns
        End Get
    End Property

    Private Sub EnsurePlaceColumns()
        Dim cols As String() = {"Exif_Country", "Exif_City", "Exif_Town"}
        m_bolPlaceColumns = cols.All(Function(c) HasColumn(c))
        If m_bolPlaceColumns OrElse (g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly) Then Return
        Try
            For Each c In cols
                If Not HasColumn(c) Then Execute("Alter Table " & mc_strTableName & " Add Column " & c & " Text(80)")
            Next
            m_bolPlaceColumns = True
        Catch ex As DbException
            ' as with the GPS columns: the index goes on without them
        End Try
    End Sub

    '==================================================================================================
    ' Where the photo was taken (new in the .NET port): PhotoIndex.Exif_Latitude / Exif_Longitude
    ' (Double, empty when the photo has no GPS), from the .Exif's [Exif] GPS -- numbers, so a place can
    ' be searched with a range. An older iPhoto.mdb gets the two columns the first time it is opened
    ' (iPhoto has backed it up just before, Maintenance.BackupDatabase).
    '==================================================================================================
    Private m_bolGpsColumns As Boolean

    ''' <summary>True when PhotoIndex has the GPS columns (added by Construct when it can).</summary>
    Public ReadOnly Property HasGpsColumns As Boolean
        Get
            Return m_bolGpsColumns
        End Get
    End Property

    Private Sub EnsureGpsColumns()
        m_bolGpsColumns = HasColumn("Exif_Latitude") AndAlso HasColumn("Exif_Longitude")
        If m_bolGpsColumns OrElse (g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly) Then Return
        Try
            If Not HasColumn("Exif_Latitude") Then Execute("Alter Table " & mc_strTableName & " Add Column Exif_Latitude Double")
            If Not HasColumn("Exif_Longitude") Then Execute("Alter Table " & mc_strTableName & " Add Column Exif_Longitude Double")
            m_bolGpsColumns = True
        Catch ex As DbException
            ' a read-only file, or another program has the table open: the index goes on without them
        End Try
    End Sub

    Private Function HasColumn(ByVal name As String) As Boolean
        Return HasColumn(mc_strTableName, name)
    End Function

    Public Sub Close()
        If m_lpConnection IsNot Nothing Then
            m_lpConnection.Dispose()
            m_lpConnection = Nothing
        End If
        m_bolImplement = False
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Close()
    End Sub

    '==================================================================================================
    ' Writing
    '==================================================================================================
    ''' <summary>Empties PhotoIndex and re-adds every photo of every album root. With
    ''' <paramref name="fillExif"/> each photo's .Exif first gets what it lacks from the picture itself:
    ''' [Exif] GPS, and the 地點 (Spot) named from it when the 地點 is blank (PlaceNames; a 地點 typed by
    ''' hand is kept). The caller backs the .Exif files up before (Maintenance.BackupExif).
    ''' <paramref name="progress"/> gets (done, total).</summary>
    Public Sub Rebuild(ByVal lpStorage As Storage, Optional ByVal fillExif As Boolean = False,
                       Optional ByVal progress As Action(Of Integer, Integer) = Nothing)
        If Not m_bolImplement Then Return
        ' the photos first (for the progress), then one transaction for the rows
        Dim photos As New List(Of Photo)
        For i = 0 To lpStorage.AlbumCount - 1
            Dim album As Albums = lpStorage.Album(i)
            album.Load()
            For j = 0 To album.ItemCount - 1
                Dim c As [Class] = TryCast(album.Item(j), [Class])
                If c Is Nothing Then Continue For
                c.Load()
                For k = 0 To c.PhotoCount - 1
                    photos.Add(c.Photo(k))
                Next
            Next
        Next
        TruncateTable()
        Using tx As DbTransaction = m_lpConnection.BeginTransaction()
            For n = 0 To photos.Count - 1
                If fillExif Then FillPlace(photos(n))
                InsertPhoto(photos(n), tx)
                If progress IsNot Nothing AndAlso (n Mod 50 = 0 OrElse n = photos.Count - 1) Then progress(n + 1, photos.Count)
            Next
            tx.Commit()
        End Using
    End Sub

    ''' <summary>The photo's .Exif gets its GPS from the picture when it has none, and 地點 / Country /
    ''' City / Town from the GPS (PlaceNames.Resolve: an attraction within 200 m first) -- each only when
    ''' it is blank: what is there, typed by hand or not, is never replaced. True when anything was written.</summary>
    Public Shared Function FillPlace(ByVal p As Photo) As Boolean
        If p.MediaType <> enumPhotoMediaType.mdImage Then Return False
        ' no .Exif yet: make a whole one (dates and every key, as an import does), not one with only the place
        If Not New ExifFile(p.FileDesc).Exists Then
            If PlaceNames.GpsOfPicture(p.FileDesc) = "" Then Return False
            Dim created As Boolean
            Return ExifRegen.RegenerateOne(p.FileDesc, New Carbon.FileSystem, created) > 0
        End If
        Dim changed As Boolean = False
        Dim gps As String = p.Exif(enumPhotoExif.peGps)
        If gps = "" Then
            gps = PlaceNames.GpsOfPicture(p.FileDesc)
            If gps = "" Then Return False
            p.Exif(enumPhotoExif.peGps) = gps
            changed = True
        End If
        Dim fields = {enumPhotoExif.peSpot, enumPhotoExif.peCountry, enumPhotoExif.peCity, enumPhotoExif.peTown}
        If fields.All(Function(f) p.Exif(f) <> "") Then Return changed
        Dim info As PlaceNames.PlaceInfo = PlaceNames.Resolve(gps)
        Dim values = {info.Spot, info.Country, info.City, info.Town}
        For i = 0 To fields.Length - 1
            If values(i) <> "" AndAlso p.Exif(fields(i)) = "" Then
                p.Exif(fields(i)) = values(i)
                changed = True
            End If
        Next
        Return changed
    End Function

    ''' <summary>Replaces the photo's row (delete + insert), as after editing its details.</summary>
    Public Sub AddItem(ByVal lpPhoto As Photo)
        Try
            Delete(lpPhoto.FileDesc)
            Using tx As DbTransaction = m_lpConnection.BeginTransaction()
                InsertPhoto(lpPhoto, tx)
                tx.Commit()
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub Delete(ByVal strFileDesc As String)
        Try
            Execute("Delete From " & mc_strTableName & " Where FileName = ?", ToStored(strFileDesc))
        Catch
            ' VB6: On Error Resume Next
        End Try
    End Sub

    Private Sub TruncateTable()
        Try
            Execute("Delete From " & mc_strTableName)
        Catch
        End Try
    End Sub

    ''' <summary>VB6 MovePhotoToRecord: the date columns come from the photo's Exif date (yyyyMMdd); with the
    ''' GPS columns also where it was taken (Nothing when the photo has no [Exif] GPS).</summary>
    Private Sub InsertPhoto(ByVal p As Photo, ByVal tx As DbTransaction)
        Dim d As String = PadNum(p.Exif(enumPhotoExif.peDate), 8)
        Dim y As String = Mid(d, 1, 4), m As String = Mid(d, 5, 2), dd As String = Mid(d, 7, 2)
        Dim gpsCols As String = If(m_bolGpsColumns, ", Exif_Latitude, Exif_Longitude", "") & If(m_bolPlaceColumns, ", Exif_Country, Exif_City, Exif_Town", "")
        Dim gpsVals As String = If(m_bolGpsColumns, ", ?, ?", "") & If(m_bolPlaceColumns, ", ?, ?, ?", "")
        Using cmd As DbCmd = NewCmd("Insert Into " & mc_strTableName &
                " (FileName, Create_Date, Create_Year, Create_Month, Create_Day, Exif_Date, Exif_Year, Exif_Month, Exif_Day," &
                "  Exif_Title, Exif_Character, Exif_Spot, Exif_Remark, Exif_KeyWord, Exif_Ranking, ModifyTime" & gpsCols & ")" &
                " Values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?" & gpsVals & ")", tx)
            For Each v In {ToStored(p.FileDesc), d, y, m, dd, d, y, m, dd,
                           p.Exif(enumPhotoExif.peTitle), p.Exif(enumPhotoExif.peCharacter), p.Exif(enumPhotoExif.peSpot),
                           p.Exif(enumPhotoExif.peRemark), p.Exif(enumPhotoExif.peKeyWord)}
                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.VarWChar, .Value = If(v, "")})
            Next
            cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Integer, .Value = CInt(Val(p.Exif(enumPhotoExif.peRanking)))})
            cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Date, .Value = DateTime.Now})
            If m_bolGpsColumns Then
                Dim lat, lon As Double
                Dim has As Boolean = PlaceNames.TryParseGps(p.Exif(enumPhotoExif.peGps), lat, lon)
                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Double, .Value = If(has, CObj(lat), DBNull.Value)})
                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Double, .Value = If(has, CObj(lon), DBNull.Value)})
            End If
            If m_bolPlaceColumns Then
                For Each f In {enumPhotoExif.peCountry, enumPhotoExif.peCity, enumPhotoExif.peTown}
                    cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.VarWChar, .Value = Left(If(p.Exif(f), ""), 80)})
                Next
            End If
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    ''' <summary>Makes <paramref name="strFileDesc"/> the cover of face <paramref name="strFaceName"/>.</summary>
    Public Sub AddFaceCover(ByVal strFaceName As String, ByVal strFileDesc As String)
        Try
            Execute("Delete From FaceIndex Where FaceName = ?", strFaceName)
            Execute("Insert Into FaceIndex (FaceName, FileName) Values (?, ?)", strFaceName, ToStored(strFileDesc))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    '==================================================================================================
    ' Calendar
    '==================================================================================================
    ''' <summary>Colours the 12 months of <paramref name="objMonth"/>: hint colour where the year has photos.</summary>
    Public Sub SetYearChanged(ByVal objMonth As Aqua.Month, ByVal intYear As Integer, ByVal defColor As Color, ByVal hintColor As Color)
        Dim withPhotos As HashSet(Of Integer) = DistinctNumbers("Select Distinct Exif_Month From " & mc_strTableName & " Where Exif_Year = ?", intYear.ToString("0000"))
        For i = 1 To 12
            objMonth.SetMonthColor(i, If(withPhotos.Contains(i), hintColor, defColor))
        Next
    End Sub

    ''' <summary>Colours the days of <paramref name="objCalendar"/>: hint colour where the month has photos.</summary>
    Public Sub SetMonthChanged(ByVal objCalendar As Aqua.Calendar, ByVal intYear As Integer, ByVal intMonth As Integer, ByVal defColor As Color, ByVal hintColor As Color)
        Dim withPhotos As HashSet(Of Integer) = DistinctNumbers("Select Distinct Exif_Day From " & mc_strTableName & " Where Exif_Year = ? And Exif_Month = ?",
                                                                intYear.ToString("0000"), intMonth.ToString("00"))
        For i = 1 To 31
            objCalendar.SetDayColor(i, If(withPhotos.Contains(i), hintColor, defColor))
        Next
    End Sub

    '==================================================================================================
    ' Queries returning file lists (VB6 filled a ByRef array and returned its count)
    '==================================================================================================
    Public Function LoadFaceIndex(ByRef strFileDesc() As String, ByRef strName() As String) As Integer
        Dim files As New List(Of String), names As New List(Of String)
        If m_bolImplement Then
            Using cmd As DbCmd = NewCmd("Select FileName, FaceName From FaceIndex")
                Using r As DbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        files.Add(CoverFile(FromStored(AsText(r(0)))))
                        names.Add(AsText(r(1)))
                    End While
                End Using
            End Using
        End If
        strFileDesc = files.ToArray()
        strName = names.ToArray()
        Return files.Count
    End Function

    ''' <summary>Photos whose people (Exif_Character) contain <paramref name="strName"/>.</summary>
    Public Function LoadFaceFile(ByVal strName As String, ByRef strFileDesc() As String) As Integer
        strFileDesc = Paths("Select FileName From " & mc_strTableName & " Where InStr(Exif_Character, ?) > 0", strName)
        Return strFileDesc.Length
    End Function

    ''' <summary>Photos dated within the last 365 days.</summary>
    Public Function LoadOneYearFile(ByRef strFileDesc() As String) As Integer
        strFileDesc = Paths("Select FileName From " & mc_strTableName & " Where Exif_Date > ?", DateTime.Now.AddDays(-365).ToString("yyyyMMdd"))
        Return strFileDesc.Length
    End Function

    ''' <summary>Five-star photos.</summary>
    Public Function LoadRankingFile(ByRef strFileDesc() As String) As Integer
        strFileDesc = Paths("Select FileName From " & mc_strTableName & " Where Exif_Ranking = 5")
        Return strFileDesc.Length
    End Function

    Public Function LoadDateFile(ByVal intYear As Integer, ByVal intMonth As Integer, ByVal intDay As Integer, ByRef strFileDesc() As String) As Integer
        strFileDesc = Paths("Select FileName From " & mc_strTableName & " Where Exif_Year = ? And Exif_Month = ? And Exif_Day = ?",
                                intYear.ToString("0000"), intMonth.ToString("00"), intDay.ToString("00"))
        Return strFileDesc.Length
    End Function

    ''' <summary>Files matching a WHERE clause built by the caller (frmSearch).</summary>
    Public Function SearchFileArrayList(ByVal strWhereSQL As String, ByRef strFileDesc() As String) As Integer
        strFileDesc = Paths("Select FileName From " & mc_strTableName & " Where " & strWhereSQL)
        Return strFileDesc.Length
    End Function

    ''' <summary>All PhotoIndex columns for a WHERE clause (blank = every row); True if any row matched.
    ''' VB6 returned a live ADODB.Recordset, this returns a DataTable.</summary>
    Public Function SearchFileRecordSet(ByVal strWhereSQL As String, ByRef objRecordSet As DataTable) As Boolean
        objRecordSet = New DataTable(mc_strTableName)
        If Not m_bolImplement Then Return False
        Dim sql As String = "Select * From " & mc_strTableName & If(String.IsNullOrWhiteSpace(strWhereSQL), "", " Where " & strWhereSQL)
        Using cmd As DbCmd = NewCmd(sql)
            Using r As DbDataReader = cmd.ExecuteReader()
                objRecordSet = ReadTable(r, mc_strTableName)
            End Using
        End Using
        If objRecordSet.Columns.Contains("FileName") Then
            For Each row As DataRow In objRecordSet.Rows
                If row("FileName") IsNot DBNull.Value Then row("FileName") = FromStored(CStr(row("FileName")))
            Next
            objRecordSet.AcceptChanges()
        End If
        Return objRecordSet.Rows.Count > 0
    End Function

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    Private Sub Execute(ByVal sql As String, ParamArray values As Object())
        If Not m_bolImplement Then Return
        Using cmd As DbCmd = NewCommand(sql, values)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function FileNames(ByVal sql As String, ParamArray values As Object()) As String()
        Dim result As New List(Of String)
        If m_bolImplement Then
            Using cmd As DbCmd = NewCommand(sql, values)
                Using r As DbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        result.Add(AsText(r(0)))
                    End While
                End Using
            End Using
        End If
        Return result.ToArray()
    End Function

    ''' <summary><see cref="FileNames"/> for a FileName column: the values as full paths.</summary>
    Private Function Paths(ByVal sql As String, ParamArray values As Object()) As String()
        Return FileNames(sql, values).Select(Function(s) FromStored(s)).ToArray()
    End Function

    Private Function DistinctNumbers(ByVal sql As String, ParamArray values As Object()) As HashSet(Of Integer)
        Dim result As New HashSet(Of Integer)
        If m_bolImplement Then
            Using cmd As DbCmd = NewCommand(sql, values)
                Using r As DbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        result.Add(CInt(Val(AsText(r(0)))))
                    End While
                End Using
            End Using
        End If
        Return result
    End Function

    Private Function NewCommand(ByVal sql As String, ByVal values As Object()) As DbCmd
        Dim cmd As DbCmd = NewCmd(sql)
        For Each v In values
            cmd.Parameters.Add(New OleDbParameter With {.Value = If(v, DBNull.Value)})
        Next
        Return cmd
    End Function

    Private Shared Function AsText(ByVal v As Object) As String
        Return If(v Is Nothing OrElse v Is DBNull.Value, "", v.ToString())
    End Function

End Class
