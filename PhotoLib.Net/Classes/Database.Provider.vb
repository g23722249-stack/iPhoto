Imports System.Data
Imports System.Data.Common
Imports System.Data.OleDb
Imports System.Globalization
Imports Microsoft.Data.Sqlite

' The database behind Database (new in the .NET port): iPhoto.mdb through Jet (OLE DB, 32-bit only) or
' iPhoto.db through SQLite (Microsoft.Data.Sqlite), chosen by the file's extension in Construct. The rest
' of Database is written once for both:
'   - DbCmd: the SQL is written for Jet, with "?" parameters given as OleDbParameter (P / Txt / Pth); for
'     SQLite each "?" becomes @p1, @p2 ... and @@IDENTITY last_insert_rowid() (ToSqliteSql);
'   - SqliteTypedReader: SQLite keeps only integers, reals, text and blobs; a value read is turned back
'     into what Jet gives for the column (Int16, Byte, Single, DateTime, Boolean ...), by the column's
'     declared type, so what is made of it (FaceArchiveOf's XML, the face code) is the same either way;
'   - SqliteSchema: the tables iPhoto.mdb has, made by code: text compared without case (as Jet does), the
'     same relationships (FaceRegion -> FacePhoto on update/delete cascade ...), AUTOINCREMENT ids (Jet
'     never reuses an AutoNumber), and indexes on FileName.
' CopyMdbToSqlite makes iPhoto.db from iPhoto.mdb row by row (the ids kept). For now iPhoto still opens
' iPhoto.mdb: iPhoto.db is used by the tests that compare the two.
Partial Public Class Database

    Private m_bolSqlite As Boolean

    ''' <summary>True when the open database is SQLite (iPhoto.db), False for Jet (iPhoto.mdb).</summary>
    Public ReadOnly Property IsSqlite As Boolean
        Get
            Return m_bolSqlite
        End Get
    End Property

    ''' <summary>A SQLite database by its extension (.db, .sqlite, .sqlite3); anything else is Jet (.mdb).</summary>
    Public Shared Function IsSqliteFile(ByVal strFile As String) As Boolean
        Select Case IO.Path.GetExtension(If(strFile, "")).ToLowerInvariant()
            Case ".db", ".sqlite", ".sqlite3" : Return True
            Case Else : Return False
        End Select
    End Function

    ''' <summary>A connection to a SQLite file: relationships enforced, WAL (readers don't wait for the
    ''' writer: the UI, the face scan and an import each have a connection). No pooling: a closed
    ''' connection lets go of the file.</summary>
    Private Shared Function OpenSqlite(ByVal strFile As String, ByVal create As Boolean) As SqliteConnection
        Dim readOnlyLib As Boolean = Not create AndAlso g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly
        Dim cs As New SqliteConnectionStringBuilder With {
            .DataSource = strFile,
            .Mode = If(create, SqliteOpenMode.ReadWriteCreate, If(readOnlyLib, SqliteOpenMode.ReadOnly, SqliteOpenMode.ReadWrite)),
            .Pooling = False,
            .ForeignKeys = True,
            .DefaultTimeout = 30}   ' seconds a statement waits for another connection's write
        Dim cn As New SqliteConnection(cs.ToString())
        cn.Open()
        cn.CreateCollation(SortCollation, AddressOf CompareAsJet)
        If Not readOnlyLib Then
            Using cmd As SqliteCommand = cn.CreateCommand()
                cmd.CommandText = "PRAGMA journal_mode = WAL"
                cmd.ExecuteNonQuery()
                ' with WAL: a crash or a pulled USB stick never leaves a damaged file, at most the last commits are
                ' lost (Jet promised less) -- and a commit no longer waits for the disk (RestoreFaces: one per photo)
                cmd.CommandText = "PRAGMA synchronous = NORMAL"
                cmd.ExecuteNonQuery()
            End Using
        End If
        Return cn
    End Function

    ' Text ordered as Jet orders it instead of by code point: by the Big5 bytes, lower case -- Big5 lists the
    ' characters by stroke count (九族 中正 台灣 ... 劍湖山, 團 before 圖), as Taiwan sorts them. Checked against
    ' iPhoto.mdb's own order: every one of 22,953 neighbouring photos of the timeline. Only for Order By
    ' (SortedBy): the columns themselves stay NoCase, so = and the indexes work as before and another program
    ' can still open iPhoto.db.
    Private Const SortCollation As String = "IPhoto"
    Private Shared s_big5 As Text.Encoding

    Private Shared Function CompareAsJet(ByVal x As String, ByVal y As String) As Integer
        If s_big5 Is Nothing Then
            Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance)
            s_big5 = Text.Encoding.GetEncoding(950)
        End If
        Dim bx As Byte() = s_big5.GetBytes(x.ToLowerInvariant()), by As Byte() = s_big5.GetBytes(y.ToLowerInvariant())
        For i = 0 To Math.Min(bx.Length, by.Length) - 1
            If bx(i) <> by(i) Then Return CInt(bx(i)) - CInt(by(i))
        Next
        If bx.Length <> by.Length Then Return bx.Length - by.Length
        Return String.CompareOrdinal(x, y)   ' the same in Big5 (a character it hasn't): still one order
    End Function

    ''' <summary>An Order By list with the text column <paramref name="column"/> sorted as in Jet.</summary>
    Private Function SortedBy(ByVal orderBy As String, ByVal column As String) As String
        If Not m_bolSqlite Then Return orderBy
        Return Text.RegularExpressions.Regex.Replace(orderBy, "\b" & column & "\b(?!\s+Collate)", column & " Collate " & SortCollation, Text.RegularExpressions.RegexOptions.IgnoreCase)
    End Function

    '==================================================================================================
    ' Commands
    '==================================================================================================
    ''' <summary>A command on this database's connection (in <paramref name="tx"/> when given).</summary>
    Friend Function NewCmd(ByVal sql As String, Optional ByVal tx As DbTransaction = Nothing) As DbCmd
        Return New DbCmd(m_lpConnection, m_bolSqlite, sql, tx)
    End Function

    ''' <summary>The SQL of the Jet dialect as SQLite takes it: "?" -> @p1, @p2 ... (outside quotes) and
    ''' @@IDENTITY -> last_insert_rowid().</summary>
    Friend Shared Function ToSqliteSql(ByVal sql As String) As String
        Dim cached As String = Nothing
        If s_sqliteSql.TryGetValue(sql, cached) Then Return cached
        Dim sb As New Text.StringBuilder(sql.Length + 16)
        Dim inText As Boolean = False
        Dim n As Integer = 0
        For Each ch As Char In sql
            If ch = "'"c Then inText = Not inText   ' '' inside a literal toggles twice
            If ch = "?"c AndAlso Not inText Then
                n += 1
                sb.Append("@p").Append(n)
            Else
                sb.Append(ch)
            End If
        Next
        Dim result As String = Text.RegularExpressions.Regex.Replace(sb.ToString(), "@@IDENTITY", "last_insert_rowid()", Text.RegularExpressions.RegexOptions.IgnoreCase)
        s_sqliteSql.TryAdd(sql, result)
        Return result
    End Function
    Private Shared ReadOnly s_sqliteSql As New Concurrent.ConcurrentDictionary(Of String, String)

    ''' <summary>A statement with its "?" parameters, run on either kind of connection. The parameters are
    ''' OleDbParameter (type and value) as the Jet code always gave them; for SQLite only the values are
    ''' used. Each run makes the provider's command afresh, so the parameters can be changed in between.</summary>
    ''' <summary>DbCmd's parameters; Add hands the parameter back (as OleDbParameterCollection.Add), so a
    ''' command run for many rows can change its values in between.</summary>
    Friend NotInheritable Class ParameterList
        Inherits List(Of OleDbParameter)

        Public Shadows Function Add(ByVal parameter As OleDbParameter) As OleDbParameter
            MyBase.Add(parameter)
            Return parameter
        End Function
    End Class

    Friend NotInheritable Class DbCmd
        Implements IDisposable

        Private ReadOnly m_cn As DbConnection
        Private ReadOnly m_tx As DbTransaction
        Private ReadOnly m_sqlite As Boolean
        Private m_live As DbCommand

        Public Property CommandText As String
        Public ReadOnly Property Parameters As New ParameterList

        Friend Sub New(ByVal cn As DbConnection, ByVal sqlite As Boolean, ByVal sql As String, ByVal tx As DbTransaction)
            m_cn = cn
            m_sqlite = sqlite
            m_tx = tx
            CommandText = sql
        End Sub

        Private Function Live() As DbCommand
            m_live?.Dispose()
            m_live = m_cn.CreateCommand()
            m_live.Transaction = m_tx
            If m_sqlite Then
                m_live.CommandText = ToSqliteSql(CommandText)
                For i = 0 To Parameters.Count - 1
                    Dim dp As DbParameter = m_live.CreateParameter()
                    dp.ParameterName = "@p" & (i + 1)
                    dp.Value = If(Parameters(i).Value, DBNull.Value)
                    m_live.Parameters.Add(dp)
                Next
            Else
                m_live.CommandText = CommandText
                For Each prm In Parameters
                    m_live.Parameters.Add(CType(DirectCast(prm, ICloneable).Clone(), OleDbParameter))   ' a parameter belongs to one command
                Next
            End If
            Return m_live
        End Function

        Public Function ExecuteNonQuery() As Integer
            Return Live().ExecuteNonQuery()
        End Function

        ''' <summary>The first column of the first row (Nothing when there is no row), typed as Jet types it.</summary>
        Public Function ExecuteScalar() As Object
            If Not m_sqlite Then Return Live().ExecuteScalar()
            Using r As DbDataReader = ExecuteReader()
                Return If(r.Read(), r.GetValue(0), Nothing)
            End Using
        End Function

        Public Function ExecuteReader() As DbDataReader
            Dim r As DbDataReader = Live().ExecuteReader()
            Return If(m_sqlite, New SqliteTypedReader(r), r)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            m_live?.Dispose()
            m_live = Nothing
        End Sub
    End Class

    ''' <summary>Every row of <paramref name="r"/> into a new table (the columns' types as the reader gives them).</summary>
    Private Shared Function ReadTable(ByVal r As DbDataReader, ByVal name As String) As DataTable
        Dim t As New DataTable(name)
        For i = 0 To r.FieldCount - 1
            t.Columns.Add(r.GetName(i), r.GetFieldType(i))
        Next
        Dim values(r.FieldCount - 1) As Object
        t.BeginLoadData()
        While r.Read()
            r.GetValues(values)
            t.Rows.Add(values)
        End While
        t.EndLoadData()
        t.AcceptChanges()
        Return t
    End Function

    '==================================================================================================
    ' Errors
    '==================================================================================================
    ''' <summary>The other connection holds a lock: worth trying again in a moment.</summary>
    Private Shared Function IsLockError(ByVal ex As DbException) As Boolean
        Dim s As SqliteException = TryCast(ex, SqliteException)
        If s IsNot Nothing Then Return s.SqliteErrorCode = 5 OrElse s.SqliteErrorCode = 6   ' SQLITE_BUSY, SQLITE_LOCKED
        Dim o As OleDbException = TryCast(ex, OleDbException)
        If o Is Nothing Then Return False
        For Each e As OleDbError In o.Errors
            Select Case e.NativeError
                Case 3006, 3008, 3009, 3187, 3188, 3189, 3197, 3211, 3218, 3260, 3262 : Return True
                Case 3709 : Return True   ' "search key was not found": a cascade hit rows the other connection was replacing
            End Select
        Next
        Return ex.Message.IndexOf("locked", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               ex.Message.IndexOf("search key was not found", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>A row with that key is there already (Jet 3022, SQLite's UNIQUE / PRIMARY KEY constraint). Jet's
    ''' OLE DB error carries no Errors (only E_FAIL and the message), so the message is looked at too: the
    ''' NativeError test alone never matched, and a second AddFaceReject of the same pair failed.</summary>
    Private Shared Function IsDuplicateKey(ByVal ex As DbException) As Boolean
        Dim s As SqliteException = TryCast(ex, SqliteException)
        If s IsNot Nothing Then Return s.SqliteErrorCode = 19 AndAlso (s.SqliteExtendedErrorCode = 2067 OrElse s.SqliteExtendedErrorCode = 1555)
        Dim o As OleDbException = TryCast(ex, OleDbException)
        If o Is Nothing Then Return False
        Return o.Errors.Cast(Of OleDbError)().Any(Function(e) e.NativeError = 3022 OrElse e.SQLState = "3022") OrElse
               o.Message.IndexOf("duplicate values", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    '==================================================================================================
    ' Schema
    '==================================================================================================
    ''' <summary>True when the open database has the table.</summary>
    Private Function TableExists(ByVal table As String) As Boolean
        If m_bolSqlite Then
            Using cmd As DbCmd = NewCmd("Select Count(*) From sqlite_master Where type = 'table' And name = ? Collate NoCase")
                cmd.Parameters.Add(Txt(table))
                Return CInt(cmd.ExecuteScalar()) > 0
            End Using
        End If
        Using t As DataTable = DirectCast(m_lpConnection, OleDbConnection).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, {Nothing, Nothing, table, "TABLE"})
            Return t IsNot Nothing AndAlso t.Rows.Count > 0
        End Using
    End Function

    ''' <summary>True when <paramref name="table"/> has the column.</summary>
    Private Function HasColumn(ByVal table As String, ByVal column As String) As Boolean
        If m_bolSqlite Then
            Using cmd As DbCmd = NewCmd("Select Count(*) From pragma_table_info(?) Where name = ? Collate NoCase")
                cmd.Parameters.Add(Txt(table))
                cmd.Parameters.Add(Txt(column))
                Return CInt(cmd.ExecuteScalar()) > 0
            End Using
        End If
        Using t As DataTable = DirectCast(m_lpConnection, OleDbConnection).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, {Nothing, Nothing, table, column})
            Return t IsNot Nothing AndAlso t.Rows.Count > 0
        End Using
    End Function

    Private Const SqliteSchemaVersion As Integer = 1

    ' iPhoto.mdb's tables (Database.vb, Database.Faces.vb). The declared types are what SqliteTypedReader
    ' turns the values back into: INTEGER Int32, SMALLINT Int16, TINYINT Byte, FLOAT Single, DOUBLE Double,
    ' DATETIME DateTime, BOOLEAN Boolean, TEXT String, BLOB Byte(). Text is compared without case, as in Jet.
    Private Shared ReadOnly SqliteSchema As String() = {
        "Create Table PhotoIndex (
            FileName Text Collate NoCase Primary Key,
            Create_Date Text Collate NoCase, Create_Year Text Collate NoCase, Create_Month Text Collate NoCase, Create_Day Text Collate NoCase,
            Exif_Date Text Collate NoCase, Exif_Year Text Collate NoCase, Exif_Month Text Collate NoCase, Exif_Day Text Collate NoCase,
            Exif_Title Text Collate NoCase, Exif_Character Text Collate NoCase, Exif_Spot Text Collate NoCase,
            Exif_Remark Text Collate NoCase, Exif_KeyWord Text Collate NoCase,
            Exif_Ranking Integer Default 0,
            Exif_Sync Boolean Not Null Default 0,
            ModifyTime DateTime,
            Exif_Latitude Double, Exif_Longitude Double,
            Exif_Country Text Collate NoCase, Exif_City Text Collate NoCase, Exif_Town Text Collate NoCase)",
        "Create Index IX_PhotoIndex_ExifDate On PhotoIndex (Exif_Year, Exif_Month, Exif_Day)",
        "Create Table FacePhoto (
            FileName Text Collate NoCase Primary Key,
            FileSize Integer, FileTime DateTime, ShotYear SmallInt, Status TinyInt, FaceCount SmallInt,
            AnalyzeTime DateTime, EngineVer Text Collate NoCase)",
        "Create Index IX_FacePhoto_Status On FacePhoto (Status)",
        "Create Table FacePerson (
            PersonID Integer Primary Key AutoIncrement,
            FaceName Text Collate NoCase Not Null Unique,
            BirthYear SmallInt, CoverFaceID Integer,
            Hidden Boolean Not Null Default 0,
            CreateTime DateTime, UpdateTime DateTime)",
        "Create Table FaceRegion (
            FaceID Integer Primary Key AutoIncrement,
            FileName Text Collate NoCase Not Null References FacePhoto (FileName) On Update Cascade On Delete Cascade,
            X Float, Y Float, W Float, H Float, Landmarks Blob, Score Float, Embedding Blob,
            PersonID Integer References FacePerson (PersonID) On Delete Set Null,
            State TinyInt, Similarity Float, ShotYear SmallInt, CreateTime DateTime, UpdateTime DateTime)",
        "Create Index FK_FaceRegion_Photo On FaceRegion (FileName)",
        "Create Index IX_FaceRegion_PersonYear On FaceRegion (PersonID, ShotYear)",
        "Create Index IX_FaceRegion_State On FaceRegion (State)",
        "Create Table FaceReject (
            FaceID Integer Not Null References FaceRegion (FaceID) On Delete Cascade,
            PersonID Integer Not Null References FacePerson (PersonID) On Delete Cascade,
            CreateTime DateTime,
            Primary Key (FaceID, PersonID))",
        "Create Index FK_FaceReject_Person On FaceReject (PersonID)",
        "Create Table FaceTemplate (
            TemplateID Integer Primary Key AutoIncrement,
            PersonID Integer Not Null References FacePerson (PersonID) On Delete Cascade,
            YearFrom SmallInt Not Null, YearTo SmallInt Not Null, Embedding Blob, FaceCount Integer, UpdateTime DateTime,
            Unique (PersonID, YearFrom))",
        "Create Table FaceIndex (
            FaceName Text Collate NoCase Primary Key,
            FileName Text Collate NoCase)",
        "PRAGMA user_version = " & SqliteSchemaVersion}

    ''' <summary>Makes an empty iPhoto.db (every table, no rows); an existing file is not touched (False).</summary>
    Public Shared Function CreateSqlite(ByVal strFile As String) As Boolean
        If IO.File.Exists(strFile) Then Return False
        Using cn As SqliteConnection = OpenSqlite(strFile, create:=True)
            Using tx As SqliteTransaction = cn.BeginTransaction()
                For Each sql In SqliteSchema
                    Using cmd As SqliteCommand = cn.CreateCommand()
                        cmd.Transaction = tx
                        cmd.CommandText = sql
                        cmd.ExecuteNonQuery()
                    End Using
                Next
                tx.Commit()
            End Using
        End Using
        Return True
    End Function

    '==================================================================================================
    ' iPhoto.mdb -> iPhoto.db
    '==================================================================================================
    ' parents before children (the relationships)
    Private Shared ReadOnly CopyOrder As String() = {"PhotoIndex", "FacePhoto", "FacePerson", "FaceRegion", "FaceReject", "FaceTemplate", "FaceIndex"}
    Private Shared ReadOnly FileNameTables As String() = {"PhotoIndex", "FacePhoto", "FaceRegion", "FaceIndex"}

    ''' <summary>A new <paramref name="strDb"/> with every row of <paramref name="strMdb"/> (the ids kept). FileName
    ''' is stored relative to the new file's folder (as it was to the .mdb's). The .mdb is only read. Jet is
    ''' needed for it: an x86 process. Returns the rows copied by table; throws when it can't be done (the
    ''' half-made file is deleted).</summary>
    Public Shared Function CopyMdbToSqlite(ByVal strMdb As String, ByVal strDb As String,
                                           Optional ByVal progress As Action(Of String, Integer) = Nothing) As Dictionary(Of String, Integer)
        If IO.File.Exists(strDb) Then Throw New IO.IOException(strDb & " 已經存在")
        Dim mdbFolder As String = IO.Path.GetDirectoryName(IO.Path.GetFullPath(strMdb))
        Dim dbFolder As String = IO.Path.GetDirectoryName(IO.Path.GetFullPath(strDb))
        Dim counts As New Dictionary(Of String, Integer)
        Try
            CreateSqlite(strDb)
            Using src As New OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & strMdb & ";Mode=Read")
                src.Open()
                Using dst As SqliteConnection = OpenSqlite(strDb, create:=False)
                    Dim srcTables As DataTable = src.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, {Nothing, Nothing, Nothing, "TABLE"})
                    Dim have As New HashSet(Of String)(srcTables.Rows.Cast(Of DataRow)().Select(Function(r) CStr(r("TABLE_NAME"))), StringComparer.OrdinalIgnoreCase)
                    Using tx As SqliteTransaction = dst.BeginTransaction()
                        For Each table In CopyOrder
                            If Not have.Contains(table) Then Continue For
                            Dim dstCols As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                            Using cmd As SqliteCommand = dst.CreateCommand()
                                cmd.Transaction = tx
                                cmd.CommandText = "Select name From pragma_table_info('" & table & "')"
                                Using r = cmd.ExecuteReader()
                                    While r.Read() : dstCols.Add(r.GetString(0)) : End While
                                End Using
                            End Using
                            Dim n As Integer = 0
                            Using sel As New OleDbCommand("Select * From [" & table & "]", src)
                                Using r As OleDbDataReader = sel.ExecuteReader()
                                    Dim cols As New List(Of Integer)
                                    For i = 0 To r.FieldCount - 1
                                        If dstCols.Contains(r.GetName(i)) Then cols.Add(i)
                                    Next
                                    Dim fileCol As Integer = If(FileNameTables.Contains(table), cols.FindIndex(Function(i) r.GetName(i).Equals("FileName", StringComparison.OrdinalIgnoreCase)), -1)
                                    Using ins As SqliteCommand = dst.CreateCommand()
                                        ins.Transaction = tx
                                        ins.CommandText = "Insert Into " & table & " (" & String.Join(", ", cols.Select(Function(i) r.GetName(i))) & ") Values (" &
                                                          String.Join(", ", cols.Select(Function(i, k) "@p" & k)) & ")"
                                        For k = 0 To cols.Count - 1
                                            ins.Parameters.Add(New SqliteParameter("@p" & k, Nothing))
                                        Next
                                        While r.Read()
                                            For k = 0 To cols.Count - 1
                                                Dim v As Object = r.GetValue(cols(k))
                                                If k = fileCol AndAlso TypeOf v Is String Then
                                                    v = LibraryPath.ToRelative(LibraryPath.FromRelative(CStr(v), mdbFolder), dbFolder)
                                                End If
                                                ins.Parameters(k).Value = If(v, DBNull.Value)
                                            Next
                                            ins.ExecuteNonQuery()
                                            n += 1
                                            If progress IsNot Nothing AndAlso n Mod 1000 = 0 Then progress(table, n)
                                        End While
                                    End Using
                                End Using
                            End Using
                            counts(table) = n
                            progress?.Invoke(table, n)
                        Next
                        tx.Commit()
                    End Using
                End Using
            End Using
            Return counts
        Catch
            Try
                IO.File.Delete(strDb)
                IO.File.Delete(strDb & "-wal")
                IO.File.Delete(strDb & "-shm")
            Catch ex As IO.IOException
            End Try
            Throw
        End Try
    End Function

    '==================================================================================================
    ' SQLite values as Jet types them
    '==================================================================================================
    ''' <summary>A SQLite reader whose values are what Jet gives for the column (by its declared type; a
    ''' column made in the query, such as Count(*), by the value: integer Int32, real Double).</summary>
    Private NotInheritable Class SqliteTypedReader
        Inherits DbDataReader

        Private ReadOnly m_r As DbDataReader

        Public Sub New(ByVal r As DbDataReader)
            m_r = r
        End Sub

        ''' <summary>The .NET type of column <paramref name="i"/>; Nothing when it isn't declared.</summary>
        Private Function DeclaredType(ByVal i As Integer) As Type
            Dim t As String = If(m_r.GetDataTypeName(i), "").ToUpperInvariant()
            Select Case True
                Case t = "INTEGER" : Return GetType(Integer)
                Case t = "SMALLINT" : Return GetType(Short)
                Case t = "TINYINT" : Return GetType(Byte)
                Case t = "FLOAT" : Return GetType(Single)
                Case t = "DOUBLE" OrElse t = "REAL" : Return GetType(Double)
                Case t = "DATETIME" : Return GetType(DateTime)
                Case t = "BOOLEAN" : Return GetType(Boolean)
                Case t = "TEXT" OrElse t.Contains("CHAR") : Return GetType(String)
                Case t = "BLOB" : Return GetType(Byte())
                Case Else : Return Nothing
            End Select
        End Function

        Public Overrides Function GetValue(ByVal ordinal As Integer) As Object
            If m_r.IsDBNull(ordinal) Then Return DBNull.Value
            Dim v As Object = m_r.GetValue(ordinal)
            Dim t As Type = DeclaredType(ordinal)
            If t Is Nothing OrElse t Is v.GetType() Then Return v
            Select Case t
                Case GetType(Integer) : Return Convert.ToInt32(v, CultureInfo.InvariantCulture)
                Case GetType(Short) : Return Convert.ToInt16(v, CultureInfo.InvariantCulture)
                Case GetType(Byte) : Return Convert.ToByte(v, CultureInfo.InvariantCulture)
                Case GetType(Single) : Return Convert.ToSingle(v, CultureInfo.InvariantCulture)
                Case GetType(Double) : Return Convert.ToDouble(v, CultureInfo.InvariantCulture)
                Case GetType(Boolean) : Return Convert.ToInt64(v, CultureInfo.InvariantCulture) <> 0
                Case GetType(DateTime)
                    If TypeOf v Is String Then Return DateTime.Parse(CStr(v), CultureInfo.InvariantCulture, DateTimeStyles.None)
                    Return DateTime.FromOADate(Convert.ToDouble(v, CultureInfo.InvariantCulture))
                Case GetType(String) : Return Convert.ToString(v, CultureInfo.InvariantCulture)
                Case Else : Return v
            End Select
        End Function

        Public Overrides Function GetFieldType(ByVal ordinal As Integer) As Type
            Return If(DeclaredType(ordinal), m_r.GetFieldType(ordinal))
        End Function

        Public Overrides Function GetValues(ByVal values As Object()) As Integer
            Dim n As Integer = Math.Min(values.Length, FieldCount)
            For i = 0 To n - 1
                values(i) = GetValue(i)
            Next
            Return n
        End Function

        Default Public Overrides ReadOnly Property Item(ByVal ordinal As Integer) As Object
            Get
                Return GetValue(ordinal)
            End Get
        End Property

        Default Public Overrides ReadOnly Property Item(ByVal name As String) As Object
            Get
                Return GetValue(GetOrdinal(name))
            End Get
        End Property

        Public Overrides Function GetBoolean(ByVal ordinal As Integer) As Boolean
            Return Convert.ToBoolean(GetValue(ordinal))
        End Function
        Public Overrides Function GetByte(ByVal ordinal As Integer) As Byte
            Return Convert.ToByte(GetValue(ordinal))
        End Function
        Public Overrides Function GetInt16(ByVal ordinal As Integer) As Short
            Return Convert.ToInt16(GetValue(ordinal))
        End Function
        Public Overrides Function GetInt32(ByVal ordinal As Integer) As Integer
            Return Convert.ToInt32(GetValue(ordinal))
        End Function
        Public Overrides Function GetInt64(ByVal ordinal As Integer) As Long
            Return Convert.ToInt64(GetValue(ordinal))
        End Function
        Public Overrides Function GetFloat(ByVal ordinal As Integer) As Single
            Return Convert.ToSingle(GetValue(ordinal))
        End Function
        Public Overrides Function GetDouble(ByVal ordinal As Integer) As Double
            Return Convert.ToDouble(GetValue(ordinal))
        End Function
        Public Overrides Function GetDecimal(ByVal ordinal As Integer) As Decimal
            Return Convert.ToDecimal(GetValue(ordinal))
        End Function
        Public Overrides Function GetDateTime(ByVal ordinal As Integer) As DateTime
            Return Convert.ToDateTime(GetValue(ordinal))
        End Function
        Public Overrides Function GetString(ByVal ordinal As Integer) As String
            Return Convert.ToString(GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function
        Public Overrides Function GetChar(ByVal ordinal As Integer) As Char
            Return m_r.GetChar(ordinal)
        End Function
        Public Overrides Function GetGuid(ByVal ordinal As Integer) As Guid
            Return m_r.GetGuid(ordinal)
        End Function
        Public Overrides Function GetBytes(ByVal ordinal As Integer, ByVal dataOffset As Long, ByVal buffer As Byte(), ByVal bufferOffset As Integer, ByVal length As Integer) As Long
            Return m_r.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length)
        End Function
        Public Overrides Function GetChars(ByVal ordinal As Integer, ByVal dataOffset As Long, ByVal buffer As Char(), ByVal bufferOffset As Integer, ByVal length As Integer) As Long
            Return m_r.GetChars(ordinal, dataOffset, buffer, bufferOffset, length)
        End Function
        Public Overrides Function GetDataTypeName(ByVal ordinal As Integer) As String
            Return m_r.GetDataTypeName(ordinal)
        End Function
        Public Overrides Function GetName(ByVal ordinal As Integer) As String
            Return m_r.GetName(ordinal)
        End Function
        Public Overrides Function GetOrdinal(ByVal name As String) As Integer
            Return m_r.GetOrdinal(name)
        End Function
        Public Overrides Function IsDBNull(ByVal ordinal As Integer) As Boolean
            Return m_r.IsDBNull(ordinal)
        End Function
        Public Overrides ReadOnly Property FieldCount As Integer
            Get
                Return m_r.FieldCount
            End Get
        End Property
        Public Overrides ReadOnly Property HasRows As Boolean
            Get
                Return m_r.HasRows
            End Get
        End Property
        Public Overrides ReadOnly Property IsClosed As Boolean
            Get
                Return m_r.IsClosed
            End Get
        End Property
        Public Overrides ReadOnly Property RecordsAffected As Integer
            Get
                Return m_r.RecordsAffected
            End Get
        End Property
        Public Overrides ReadOnly Property Depth As Integer
            Get
                Return m_r.Depth
            End Get
        End Property
        Public Overrides Function NextResult() As Boolean
            Return m_r.NextResult()
        End Function
        Public Overrides Function Read() As Boolean
            Return m_r.Read()
        End Function
        Public Overrides Function GetEnumerator() As IEnumerator
            Return New DbEnumerator(Me, closeReader:=False)
        End Function
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then m_r.Dispose()
            MyBase.Dispose(disposing)
        End Sub
    End Class

End Class
