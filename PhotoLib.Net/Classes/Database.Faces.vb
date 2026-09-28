Imports System.Data
Imports System.Data.OleDb

' Face recognition tables of iPhoto.mdb (new in the .NET port; created by hand with a DDL script on
' 2026-09-28, see the plan): FacePerson, FacePhoto, FaceRegion, FaceTemplate, FaceReject.
'   FacePhoto  -- one row per analysed photo (FileName = Photo.FileDesc); deleting it deletes its faces.
'                 Deliberately NOT related to PhotoIndex: AddItem deletes and re-inserts PhotoIndex rows.
'   FaceRegion -- one row per face: box as fractions, SFace feature (LONGBINARY), person, state.
'   FacePerson -- one row per name (the same names as Exif_Character / FaceIndex.FaceName).
' Everything here is a no-op / empty when the database isn't open or the tables don't exist.
' Dates go in as OleDbType.Date: Jet rejects the DBTimeStamp that a DateTime parameter defaults to.
Partial Public Class Database

    Private m_intFaceTables As Integer = -1   ' -1 unknown, 0 missing, 1 present

    ''' <summary>True when the database is open and has the face tables.</summary>
    Public ReadOnly Property FaceTablesReady As Boolean
        Get
            If Not m_bolImplement Then Return False
            If m_intFaceTables < 0 Then
                Try
                    Dim t As DataTable = m_lpConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})
                    Dim names = t.Rows.Cast(Of DataRow)().Select(Function(r) r("TABLE_NAME").ToString()).ToList()
                    m_intFaceTables = If({"FacePerson", "FacePhoto", "FaceRegion"}.All(Function(n) names.Contains(n, StringComparer.OrdinalIgnoreCase)), 1, 0)
                Catch
                    m_intFaceTables = 0
                End Try
            End If
            Return m_intFaceTables = 1
        End Get
    End Property

    '==================================================================================================
    ' FacePhoto
    '==================================================================================================
    Public Structure FacePhotoState
        Public FileSize As Long
        Public FileTime As DateTime
        Public EngineVer As String
    End Structure

    ''' <summary>What was analysed, by file name (case-insensitive): every photo, or just <paramref name="strFileDesc"/>.</summary>
    Public Function LoadFacePhotoStates(Optional ByVal strFileDesc As String = Nothing) As Dictionary(Of String, FacePhotoState)
        Dim result As New Dictionary(Of String, FacePhotoState)(StringComparer.OrdinalIgnoreCase)
        If Not FaceTablesReady Then Return result
        Using cmd As New OleDbCommand("Select FileName, FileSize, FileTime, EngineVer From FacePhoto" &
                                      If(strFileDesc Is Nothing, "", " Where FileName = ?"), m_lpConnection)
            If strFileDesc IsNot Nothing Then cmd.Parameters.Add(Txt(strFileDesc))
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result(AsText(r(0))) = New FacePhotoState With {
                        .FileSize = If(IsDBNull(r(1)), -1L, CLng(r(1))),
                        .FileTime = If(IsDBNull(r(2)), DateTime.MinValue, CDate(r(2))),
                        .EngineVer = AsText(r(3))}
                End While
            End Using
        End Using
        Return result
    End Function

    Public Function FaceAnalyzedCount() As Integer
        If Not FaceTablesReady Then Return 0
        Using cmd As New OleDbCommand("Select Count(*) From FacePhoto", m_lpConnection)
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    ''' <summary>Replaces the photo's analysis: its FacePhoto row and all its faces (names given to them
    ''' before are dropped with them -- the file changed, so the old boxes don't fit any more).
    ''' <paramref name="faces"/> Nothing = the file couldn't be read (Status 2).</summary>
    Public Sub SaveFaceAnalysis(ByVal strFileDesc As String, ByVal lngSize As Long, ByVal dtFileTime As DateTime,
                                ByVal intShotYear As Integer, ByVal faces As List(Of Quartz.FaceEngine.Face))
        If Not FaceTablesReady Then Return
        WithRetry(Sub()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Exec(tx, "Delete From FacePhoto Where FileName = ?", Txt(strFileDesc))
                          Exec(tx, "Insert Into FacePhoto (FileName, FileSize, FileTime, ShotYear, Status, FaceCount, AnalyzeTime, EngineVer) Values (?, ?, ?, ?, ?, ?, ?, ?)",
                               Txt(strFileDesc), P(OleDbType.Integer, CInt(Math.Min(lngSize, Integer.MaxValue))), P(OleDbType.Date, dtFileTime),
                               P(OleDbType.SmallInt, CShort(intShotYear)), P(OleDbType.UnsignedTinyInt, CByte(If(faces Is Nothing, 2, 1))),
                               P(OleDbType.SmallInt, CShort(If(faces Is Nothing, 0, faces.Count))), P(OleDbType.Date, DateTime.Now), Txt(Quartz.FaceEngine.Version))
                          If faces IsNot Nothing Then
                              For Each f In faces
                                  InsertFaceRegion(tx, strFileDesc, f, FaceRegion.enumFaceState.fsUnnamed, intShotYear)
                              Next
                          End If
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    Private Function InsertFaceRegion(ByVal tx As OleDbTransaction, ByVal strFileDesc As String, ByVal f As Quartz.FaceEngine.Face,
                                      ByVal state As FaceRegion.enumFaceState, ByVal intShotYear As Integer) As Integer
        Dim marks(39) As Byte
        Buffer.BlockCopy(f.Landmarks.SelectMany(Function(p) {p.X, p.Y}).ToArray(), 0, marks, 0, 40)
        Exec(tx, "Insert Into FaceRegion (FileName, X, Y, W, H, Landmarks, Score, Embedding, State, ShotYear, CreateTime, UpdateTime) Values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
             Txt(strFileDesc), P(OleDbType.Single, f.Box.X), P(OleDbType.Single, f.Box.Y), P(OleDbType.Single, f.Box.Width), P(OleDbType.Single, f.Box.Height),
             P(OleDbType.Binary, marks), P(OleDbType.Single, f.Score), P(OleDbType.LongVarBinary, If(Quartz.FaceEngine.FeatureToBytes(f.Feature), CObj(DBNull.Value))),
             P(OleDbType.UnsignedTinyInt, CByte(state)), P(OleDbType.SmallInt, CShort(intShotYear)), P(OleDbType.Date, DateTime.Now), P(OleDbType.Date, DateTime.Now))
        Using cmd As New OleDbCommand("Select @@IDENTITY", m_lpConnection, tx)
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    '==================================================================================================
    ' FaceRegion
    '==================================================================================================
    ''' <summary>The faces of one photo (not the ones marked "not a face"), with their person's name.</summary>
    Public Function LoadFaceRegions(ByVal strFileDesc As String) As List(Of FaceRegion)
        Dim result As New List(Of FaceRegion)
        If Not FaceTablesReady Then Return result
        Using cmd As New OleDbCommand("Select r.FaceID, r.FileName, r.X, r.Y, r.W, r.H, r.Score, r.Embedding, r.PersonID, p.FaceName, r.State, r.Similarity, r.ShotYear" &
                                      " From FaceRegion AS r Left Join FacePerson AS p On r.PersonID = p.PersonID" &
                                      " Where r.FileName = ? And r.State <> 9 Order By r.X", m_lpConnection)
            cmd.Parameters.Add(Txt(strFileDesc))
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result.Add(New FaceRegion With {
                        .FaceID = CInt(r(0)), .FileName = AsText(r(1)),
                        .Box = New RectangleF(Sng(r(2)), Sng(r(3)), Sng(r(4)), Sng(r(5))),
                        .Score = Sng(r(6)),
                        .Feature = If(IsDBNull(r(7)), Nothing, Quartz.FaceEngine.BytesToFeature(CType(r(7), Byte()))),
                        .PersonID = If(IsDBNull(r(8)), 0, CInt(r(8))), .PersonName = AsText(r(9)),
                        .State = CType(If(IsDBNull(r(10)), 0, CInt(r(10))), FaceRegion.enumFaceState),
                        .Similarity = Sng(r(11)), .ShotYear = If(IsDBNull(r(12)), 0, CInt(r(12)))})
                End While
            End Using
        End Using
        Return result
    End Function

    ''' <summary>Adds a face the user boxed; returns its FaceID (0 when the photo was never analysed).</summary>
    Public Function AddManualFace(ByVal strFileDesc As String, ByVal f As Quartz.FaceEngine.Face, ByVal intShotYear As Integer) As Integer
        If Not FaceTablesReady Then Return 0
        Return WithRetry(Function()
                             Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                                 Using cmd As New OleDbCommand("Select Count(*) From FacePhoto Where FileName = ?", m_lpConnection, tx)
                                     cmd.Parameters.Add(Txt(strFileDesc))
                                     If CInt(cmd.ExecuteScalar()) = 0 Then Return 0
                                 End Using
                                 Dim id As Integer = InsertFaceRegion(tx, strFileDesc, f, FaceRegion.enumFaceState.fsManual, intShotYear)
                                 Exec(tx, "Update FacePhoto Set FaceCount = FaceCount + 1 Where FileName = ?", Txt(strFileDesc))
                                 tx.Commit()
                                 Return id
                             End Using
                         End Function)
    End Function

    ''' <summary>Sets a face's person (0 = nobody) and state.</summary>
    Public Sub SetFacePerson(ByVal intFaceID As Integer, ByVal intPersonID As Integer, ByVal state As FaceRegion.enumFaceState)
        If Not FaceTablesReady Then Return
        Exec(Nothing, "Update FaceRegion Set PersonID = ?, State = ?, UpdateTime = ? Where FaceID = ?",
             P(OleDbType.Integer, If(intPersonID = 0, CObj(DBNull.Value), intPersonID)), P(OleDbType.UnsignedTinyInt, CByte(state)),
             P(OleDbType.Date, DateTime.Now), P(OleDbType.Integer, intFaceID))
    End Sub

    ''' <summary>True when another face of the photo is (still) assigned to the person.</summary>
    Public Function PhotoHasPerson(ByVal strFileDesc As String, ByVal intPersonID As Integer, ByVal intExceptFaceID As Integer) As Boolean
        If Not FaceTablesReady Then Return False
        Using cmd As New OleDbCommand("Select Count(*) From FaceRegion Where FileName = ? And PersonID = ? And FaceID <> ? And State = 3", m_lpConnection)
            cmd.Parameters.Add(Txt(strFileDesc))
            cmd.Parameters.Add(P(OleDbType.Integer, intPersonID))
            cmd.Parameters.Add(P(OleDbType.Integer, intExceptFaceID))
            Return CInt(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    '==================================================================================================
    ' FacePerson
    '==================================================================================================
    ''' <summary>The person's ID, adding the person when the name is new (0 for a blank name).</summary>
    Public Function EnsureFacePerson(ByVal strName As String) As Integer
        strName = If(strName, "").Trim()
        If strName = "" OrElse Not FaceTablesReady Then Return 0
        For attempt = 1 To 2
            Using cmd As New OleDbCommand("Select PersonID From FacePerson Where FaceName = ?", m_lpConnection)
                cmd.Parameters.Add(Txt(strName))
                Dim v As Object = cmd.ExecuteScalar()
                If v IsNot Nothing AndAlso Not IsDBNull(v) Then Return CInt(v)
            End Using
            Try
                Exec(Nothing, "Insert Into FacePerson (FaceName, Hidden, CreateTime, UpdateTime) Values (?, False, ?, ?)",
                     Txt(strName), P(OleDbType.Date, DateTime.Now), P(OleDbType.Date, DateTime.Now))
                Using cmd As New OleDbCommand("Select @@IDENTITY", m_lpConnection)
                    Return CInt(cmd.ExecuteScalar())
                End Using
            Catch ex As OleDbException When attempt = 1 AndAlso ex.Errors.Cast(Of OleDbError)().Any(Function(e) e.NativeError = 3022)
                ' the other thread added the same name a moment ago: read its ID
                Threading.Thread.Sleep(LockWaitMs * 4)
            End Try
        Next
        Return 0
    End Function

    ''' <summary>Names for the name box: the face persons plus every name typed into Exif_Character,
    ''' sorted, without duplicates.</summary>
    Public Function LoadPeopleNames() As List(Of String)
        Dim names As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
        If m_bolImplement Then
            If FaceTablesReady Then
                For Each n In FileNames("Select FaceName From FacePerson Where Hidden = False")
                    names.Add(n.Trim())
                Next
            End If
            For Each s In FileNames("Select Distinct Exif_Character From " & mc_strTableName & " Where Exif_Character <> ''")
                For Each n In FaceNames.Split(s)
                    names.Add(n)
                Next
            Next
        End If
        names.Remove("")
        Return names.OrderBy(Function(n) n, StringComparer.CurrentCulture).ToList()
    End Function

    '==================================================================================================
    ' The whole library at once (FaceLibrary.Organize)
    '==================================================================================================
    ''' <summary>Every face except the ones marked "not a face", with feature and person name.</summary>
    Public Function LoadAllFaces() As List(Of FaceRegion)
        Dim result As New List(Of FaceRegion)
        If Not FaceTablesReady Then Return result
        Using cmd As New OleDbCommand("Select r.FaceID, r.FileName, r.X, r.Y, r.W, r.H, r.Score, r.Embedding, r.PersonID, p.FaceName, r.State, r.Similarity, r.ShotYear" &
                                      " From FaceRegion AS r Left Join FacePerson AS p On r.PersonID = p.PersonID Where r.State <> 9", m_lpConnection)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result.Add(New FaceRegion With {
                        .FaceID = CInt(r(0)), .FileName = AsText(r(1)),
                        .Box = New RectangleF(Sng(r(2)), Sng(r(3)), Sng(r(4)), Sng(r(5))),
                        .Score = Sng(r(6)),
                        .Feature = If(IsDBNull(r(7)), Nothing, Quartz.FaceEngine.BytesToFeature(CType(r(7), Byte()))),
                        .PersonID = If(IsDBNull(r(8)), 0, CInt(r(8))), .PersonName = AsText(r(9)),
                        .State = CType(If(IsDBNull(r(10)), 0, CInt(r(10))), FaceRegion.enumFaceState),
                        .Similarity = Sng(r(11)), .ShotYear = If(IsDBNull(r(12)), 0, CInt(r(12)))})
                End While
            End Using
        End Using
        Return result
    End Function

    Public Class FacePersonInfo
        Public PersonID As Integer
        Public Name As String = ""
        Public BirthYear As Integer
        Public CoverFaceID As Integer
        Public Hidden As Boolean
    End Class

    Public Function LoadFacePersons() As List(Of FacePersonInfo)
        Dim result As New List(Of FacePersonInfo)
        If Not FaceTablesReady Then Return result
        Using cmd As New OleDbCommand("Select PersonID, FaceName, BirthYear, CoverFaceID, Hidden From FacePerson", m_lpConnection)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result.Add(New FacePersonInfo With {
                        .PersonID = CInt(r(0)), .Name = AsText(r(1)),
                        .BirthYear = If(IsDBNull(r(2)), 0, CInt(r(2))), .CoverFaceID = If(IsDBNull(r(3)), 0, CInt(r(3))),
                        .Hidden = Not IsDBNull(r(4)) AndAlso CBool(r(4))})
                End While
            End Using
        End Using
        Return result
    End Function

    ''' <summary>The people field of every indexed photo, by file name.</summary>
    Public Function LoadCharacters() As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If Not m_bolImplement Then Return result
        Using cmd As New OleDbCommand("Select FileName, Exif_Character From " & mc_strTableName & " Where Exif_Character <> ''", m_lpConnection)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result(AsText(r(0))) = AsText(r(1))
                End While
            End Using
        End Using
        Return result
    End Function

    ''' <summary>"This face is not that person" pairs (FaceReject), as "faceID|personID".</summary>
    Public Function LoadFaceRejects() As HashSet(Of String)
        Dim result As New HashSet(Of String)
        If Not FaceTablesReady Then Return result
        Using cmd As New OleDbCommand("Select FaceID, PersonID From FaceReject", m_lpConnection)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result.Add(CInt(r(0)) & "|" & CInt(r(1)))
                End While
            End Using
        End Using
        Return result
    End Function

    Public Structure FaceAssignment
        Public FaceID As Integer
        Public ExpectedState As FaceRegion.enumFaceState   ' only applied while the face still has this state
        Public PersonID As Integer
        Public State As FaceRegion.enumFaceState
        Public Similarity As Single
    End Structure

    ''' <summary>Applies the assignments, 200 per transaction (short locks: the UI may be writing too);
    ''' each only when the face's state is still the expected one (the user may have named it
    ''' meanwhile). Returns the FaceIDs actually changed.</summary>
    Public Function SaveFaceAssignments(ByVal list As IEnumerable(Of FaceAssignment)) As HashSet(Of Integer)
        Dim changed As New HashSet(Of Integer)
        If Not FaceTablesReady Then Return changed
        For Each batch In list.Select(Function(a, i) (a, i)).GroupBy(Function(x) x.i \ 200, Function(x) x.a)
            Dim done As HashSet(Of Integer) =
                WithRetry(Function()
                              Dim ids As New HashSet(Of Integer)
                              Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                                  Using cmd As New OleDbCommand("Update FaceRegion Set PersonID = ?, State = ?, Similarity = ?, UpdateTime = ? Where FaceID = ? And State = ?", m_lpConnection, tx)
                                      Dim pPerson = cmd.Parameters.Add(P(OleDbType.Integer, Nothing))
                                      Dim pState = cmd.Parameters.Add(P(OleDbType.UnsignedTinyInt, Nothing))
                                      Dim pSim = cmd.Parameters.Add(P(OleDbType.Single, Nothing))
                                      Dim pTime = cmd.Parameters.Add(P(OleDbType.Date, Nothing))
                                      Dim pFace = cmd.Parameters.Add(P(OleDbType.Integer, Nothing))
                                      Dim pOld = cmd.Parameters.Add(P(OleDbType.UnsignedTinyInt, Nothing))
                                      For Each a In batch
                                          pPerson.Value = If(a.PersonID = 0, CObj(DBNull.Value), a.PersonID)
                                          pState.Value = CByte(a.State)
                                          pSim.Value = a.Similarity
                                          pTime.Value = DateTime.Now
                                          pFace.Value = a.FaceID
                                          pOld.Value = CByte(a.ExpectedState)
                                          If cmd.ExecuteNonQuery() > 0 Then ids.Add(a.FaceID)
                                      Next
                                  End Using
                                  tx.Commit()
                              End Using
                              Return ids
                          End Function)
            changed.UnionWith(done)
        Next
        Return changed
    End Function

    Public Structure FaceTemplateRow
        Public PersonID As Integer
        Public YearFrom As Integer
        Public YearTo As Integer
        Public Feature As Single()
        Public FaceCount As Integer
    End Structure

    ''' <summary>Replaces every FaceTemplate row (the age-bucket averages of each person).</summary>
    Public Sub ReplaceFaceTemplates(ByVal rows As IEnumerable(Of FaceTemplateRow))
        If Not FaceTablesReady Then Return
        WithRetry(Sub()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Exec(tx, "Delete From FaceTemplate")
                          For Each t In rows
                              Exec(tx, "Insert Into FaceTemplate (PersonID, YearFrom, YearTo, Embedding, FaceCount, UpdateTime) Values (?, ?, ?, ?, ?, ?)",
                                   P(OleDbType.Integer, t.PersonID), P(OleDbType.SmallInt, CShort(t.YearFrom)), P(OleDbType.SmallInt, CShort(t.YearTo)),
                                   P(OleDbType.LongVarBinary, Quartz.FaceEngine.FeatureToBytes(t.Feature)), P(OleDbType.Integer, t.FaceCount), P(OleDbType.Date, DateTime.Now))
                          Next
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    ''' <summary>Empties the face tables (children first). PhotoIndex and FaceIndex are not touched.</summary>
    Public Sub ClearFaceTables()
        If Not FaceTablesReady Then Return
        WithRetry(Sub()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          For Each tbl In {"FaceReject", "FaceTemplate", "FaceRegion", "FacePhoto", "FacePerson"}
                              Exec(tx, "Delete From " & tbl)
                          Next
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    ''' <summary>Photos analysed / faces / people, for 設定 › 面孔.</summary>
    Public Function FaceCounts() As (Photos As Integer, Faces As Integer, Persons As Integer)
        If Not FaceTablesReady Then Return (0, 0, 0)
        Dim n = Function(sql As String) As Integer
                    Using cmd As New OleDbCommand(sql, m_lpConnection)
                        Return CInt(cmd.ExecuteScalar())
                    End Using
                End Function
        Return (n("Select Count(*) From FacePhoto"), n("Select Count(*) From FaceRegion Where State <> 9"), n("Select Count(*) From FacePerson Where Hidden = False"))
    End Function

    ''' <summary>A photo moved to another folder: its face rows follow it.</summary>
    Public Sub RenameFacePhoto(ByVal strOld As String, ByVal strNew As String)
        If Not FaceTablesReady Then Return
        WithRetry(Sub()
                      ' FaceRegion.FileName must name a FacePhoto row (relationship without cascading
                      ' updates): the new row first, then the faces, then the old row
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Exec(tx, "Insert Into FacePhoto (FileName, FileSize, FileTime, ShotYear, Status, FaceCount, AnalyzeTime, EngineVer)" &
                                   " Select ?, FileSize, FileTime, ShotYear, Status, FaceCount, AnalyzeTime, EngineVer From FacePhoto Where FileName = ?",
                               Txt(strNew), Txt(strOld))
                          Exec(tx, "Update FaceRegion Set FileName = ? Where FileName = ?", Txt(strNew), Txt(strOld))
                          Exec(tx, "Delete From FacePhoto Where FileName = ?", Txt(strOld))
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    ''' <summary>Forgets a photo that was deleted: its faces, their 不是此人 records and its FacePhoto row.</summary>
    Public Sub DeleteFacePhoto(ByVal strFileDesc As String)
        If Not FaceTablesReady Then Return
        WithRetry(Sub()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Exec(tx, "Delete From FaceReject Where FaceID In (Select FaceID From FaceRegion Where FileName = ?)", Txt(strFileDesc))
                          Exec(tx, "Delete From FaceRegion Where FileName = ?", Txt(strFileDesc))
                          Exec(tx, "Delete From FacePhoto Where FileName = ?", Txt(strFileDesc))
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    ''' <summary>Every person's templates (age-bucket averages), by PersonID.</summary>
    Public Function LoadTemplateFeatures() As Dictionary(Of Integer, List(Of Single()))
        Dim result As New Dictionary(Of Integer, List(Of Single()))
        If Not FaceTablesReady Then Return result
        Using cmd As New OleDbCommand("Select PersonID, Embedding From FaceTemplate", m_lpConnection)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    If IsDBNull(r(0)) OrElse IsDBNull(r(1)) Then Continue While
                    Dim id As Integer = CInt(r(0))
                    If Not result.ContainsKey(id) Then result(id) = New List(Of Single())
                    result(id).Add(Quartz.FaceEngine.BytesToFeature(CType(r(1), Byte())))
                End While
            End Using
        End Using
        Return result
    End Function

    ''' <summary>Faces marked 我不認識 (State 8).</summary>
    Public Function StrangerCount() As Integer
        If Not FaceTablesReady Then Return 0
        Using cmd As New OleDbCommand("Select Count(*) From FaceRegion Where State = 8", m_lpConnection)
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    ''' <summary>Every 我不認識 face back to unnamed (grouped and matched again); returns how many.</summary>
    Public Function RestoreStrangers() As Integer
        If Not FaceTablesReady Then Return 0
        Return WithRetry(Function()
                             Using cmd As New OleDbCommand("Update FaceRegion Set State = 0, UpdateTime = ? Where State = 8", m_lpConnection)
                                 cmd.Parameters.Add(P(OleDbType.Date, DateTime.Now))
                                 Return cmd.ExecuteNonQuery()
                             End Using
                         End Function)
    End Function

    ''' <summary>Remembers "this face is not that person" (FaceReject): never suggested again.</summary>
    Public Sub AddFaceReject(ByVal intFaceID As Integer, ByVal intPersonID As Integer)
        If Not FaceTablesReady OrElse intPersonID = 0 Then Return
        Try
            Exec(Nothing, "Insert Into FaceReject (FaceID, PersonID, CreateTime) Values (?, ?, ?)",
                 P(OleDbType.Integer, intFaceID), P(OleDbType.Integer, intPersonID), P(OleDbType.Date, DateTime.Now))
        Catch ex As OleDbException When ex.Errors.Cast(Of OleDbError)().Any(Function(e) e.NativeError = 3022)
            ' already there
        End Try
    End Sub

    ''' <summary>Puts the photo's people field into its PhotoIndex row -- that column only, retrying on
    ''' locks. (AddItem deletes and re-inserts the row, and loses it when the insert is refused.)
    ''' A photo not indexed yet is added with AddItem.</summary>
    Public Sub SetPhotoCharacter(ByVal lpPhoto As Photo)
        If Not m_bolImplement Then Return
        Dim updated As Integer = 0
        WithRetry(Sub()
                      Using cmd As New OleDbCommand("Update " & mc_strTableName & " Set Exif_Character = ?, ModifyTime = ? Where FileName = ?", m_lpConnection)
                          cmd.Parameters.Add(Txt(lpPhoto.Exif(enumPhotoExif.peCharacter)))
                          cmd.Parameters.Add(P(OleDbType.Date, DateTime.Now))
                          cmd.Parameters.Add(Txt(lpPhoto.FileDesc))
                          updated = cmd.ExecuteNonQuery()
                      End Using
                  End Sub)
        If updated = 0 Then AddItem(lpPhoto)
    End Sub

    '==================================================================================================
    ' Person maintenance (UI thread)
    '==================================================================================================
    Public Sub RenameFacePerson(ByVal intPersonID As Integer, ByVal strName As String)
        Exec(Nothing, "Update FacePerson Set FaceName = ?, UpdateTime = ? Where PersonID = ?",
             Txt(strName), P(OleDbType.Date, DateTime.Now), P(OleDbType.Integer, intPersonID))
    End Sub

    ''' <summary>Moves every face of <paramref name="intFromID"/> to <paramref name="intToID"/> and deletes
    ''' the first person (its templates and rejections go with it).</summary>
    Public Sub MergeFacePerson(ByVal intFromID As Integer, ByVal intToID As Integer)
        WithRetry(Sub()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Exec(tx, "Update FaceRegion Set PersonID = ?, UpdateTime = ? Where PersonID = ?",
                               P(OleDbType.Integer, intToID), P(OleDbType.Date, DateTime.Now), P(OleDbType.Integer, intFromID))
                          Exec(tx, "Delete From FacePerson Where PersonID = ?", P(OleDbType.Integer, intFromID))
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    Public Sub SetFacePersonBirthYear(ByVal intPersonID As Integer, ByVal intYear As Integer)
        Exec(Nothing, "Update FacePerson Set BirthYear = ?, UpdateTime = ? Where PersonID = ?",
             P(OleDbType.SmallInt, If(intYear <= 0, CObj(DBNull.Value), CShort(intYear))), P(OleDbType.Date, DateTime.Now), P(OleDbType.Integer, intPersonID))
    End Sub

    Public Sub SetFacePersonHidden(ByVal intPersonID As Integer, ByVal bolHidden As Boolean)
        Exec(Nothing, "Update FacePerson Set Hidden = ?, UpdateTime = ? Where PersonID = ?",
             P(OleDbType.Boolean, bolHidden), P(OleDbType.Date, DateTime.Now), P(OleDbType.Integer, intPersonID))
    End Sub

    ''' <summary>The name card made with frmCover (FaceIndex), or "" when there is none.</summary>
    Public Function LoadFaceCover(ByVal strName As String) As String
        If Not m_bolImplement Then Return ""
        Using cmd As New OleDbCommand("Select FileName From FaceIndex Where FaceName = ?", m_lpConnection)
            cmd.Parameters.Add(Txt(strName))
            Return AsText(cmd.ExecuteScalar())
        End Using
    End Function

    ''' <summary>Moves a FaceIndex name card to a new name (the card file itself keeps its name).</summary>
    Public Sub RenameFaceCover(ByVal strOld As String, ByVal strNew As String)
        If Not m_bolImplement Then Return
        Dim file As String = LoadFaceCover(strOld)
        If file = "" OrElse LoadFaceCover(strNew) <> "" Then Return
        Exec(Nothing, "Update FaceIndex Set FaceName = ? Where FaceName = ?", Txt(strNew), Txt(strOld))
    End Sub

    '==================================================================================================
    ' Locks: the UI thread and the face scan thread write through separate connections, and Jet
    ' refuses a write to a page the other one has locked in a transaction ("currently locked").
    ' The writers below retry their whole work for a few seconds; an unfinished transaction rolls
    ' back when it is disposed, so a retry starts clean.
    '==================================================================================================
    Private Const LockRetries As Integer = 40
    Private Const LockWaitMs As Integer = 150

    Private Shared Function IsLockError(ByVal ex As OleDbException) As Boolean
        For Each e As OleDbError In ex.Errors
            Select Case e.NativeError
                Case 3006, 3008, 3009, 3187, 3188, 3189, 3197, 3211, 3218, 3260, 3262 : Return True
                Case 3709 : Return True   ' "search key was not found": a cascade hit rows the other connection was replacing
            End Select
        Next
        Return ex.Message.IndexOf("locked", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               ex.Message.IndexOf("search key was not found", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function WithRetry(Of T)(ByVal work As Func(Of T)) As T
        For attempt = 1 To LockRetries
            Try
                Return work()
            Catch ex As OleDbException When attempt < LockRetries AndAlso IsLockError(ex)
                Threading.Thread.Sleep(LockWaitMs)
            End Try
        Next
        Return work()   ' not reached
    End Function

    Private Shared Sub WithRetry(ByVal work As Action)
        WithRetry(Function()
                      work()
                      Return True
                  End Function)
    End Sub

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    ''' <summary>One statement. Outside a transaction it retries while the other connection holds a
    ''' lock; inside one the caller retries the whole transaction.</summary>
    Private Sub Exec(ByVal tx As OleDbTransaction, ByVal sql As String, ParamArray values As OleDbParameter())
        Dim run As Action = Sub()
                                Using cmd As New OleDbCommand(sql, m_lpConnection, tx)
                                    cmd.Parameters.AddRange(values)
                                    Try
                                        cmd.ExecuteNonQuery()
                                    Finally
                                        cmd.Parameters.Clear()   ' a parameter can belong to one command at a time (retry)
                                    End Try
                                End Using
                            End Sub
        If tx Is Nothing Then WithRetry(run) Else run()
    End Sub

    Private Shared Function P(ByVal type As OleDbType, ByVal value As Object) As OleDbParameter
        Return New OleDbParameter With {.OleDbType = type, .Value = If(value, DBNull.Value)}
    End Function

    Private Shared Function Txt(ByVal value As String) As OleDbParameter
        Return P(OleDbType.VarWChar, If(value, ""))
    End Function

    Private Shared Function Sng(ByVal v As Object) As Single
        Return If(v Is Nothing OrElse IsDBNull(v), 0.0F, CSng(v))
    End Function

End Class
