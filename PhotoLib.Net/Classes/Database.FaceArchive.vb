Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Xml.Linq

' The face tables as XML (new in the .NET port), for FaceArchive: each photo's faces go into its .Exif
' entry (<Faces>), the people and the name cards into Faces.xml next to iPhoto.mdb -- so the face
' data can be made again from them when iPhoto.mdb is lost (RestoreFaces). People are named, not
' numbered (PersonID / FaceID are AutoNumbers, new every time); FaceTemplate is not kept (the face
' organizer works it out again).
'   <Faces fileSize fileTime shotYear status count analyzed engine>
'     <Face x y w h score state similarity person shotYear created updated>
'       <Landmarks>base64</Landmarks><Embedding>base64</Embedding><Rejected>name</Rejected>...
'     </Face>
'   </Faces>
Partial Public Class Database

    Private Shared ReadOnly Inv As CultureInfo = CultureInfo.InvariantCulture

    ''' <summary>The photos whose face data changed since <paramref name="since"/> (Nothing: every analysed
    ''' photo): faces added or changed, analysed again, 不是此人 marked, or their person renamed.</summary>
    Public Function FaceFilesChangedSince(ByVal since As DateTime?) As List(Of String)
        Dim result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If Not FaceTablesReady Then Return result.ToList()
        Dim add = Sub(sql As String)
                      Using cmd As New OleDbCommand(sql, m_lpConnection)
                          If since.HasValue Then cmd.Parameters.Add(P(OleDbType.Date, since.Value))
                          Using r As OleDbDataReader = cmd.ExecuteReader()
                              While r.Read()
                                  result.Add(FromStored(AsText(r(0))))
                              End While
                          End Using
                      End Using
                  End Sub
        If Not since.HasValue Then
            add("Select FileName From FacePhoto")
        Else
            add("Select Distinct FileName From FaceRegion Where UpdateTime >= ?")
            add("Select FileName From FacePhoto Where AnalyzeTime >= ?")
            add("Select Distinct r.FileName From FaceReject AS j Inner Join FaceRegion AS r On j.FaceID = r.FaceID Where j.CreateTime >= ?")
            add("Select Distinct r.FileName From FaceRegion AS r Inner Join FacePerson AS p On r.PersonID = p.PersonID Where p.UpdateTime >= ?")
        End If
        Return result.ToList()
    End Function

    ''' <summary>The photo's face data as a &lt;Faces&gt; element; Nothing when it was never analysed.</summary>
    Public Function FaceArchiveOf(ByVal strFileDesc As String) As XElement
        If Not FaceTablesReady Then Return Nothing
        Dim e As XElement = Nothing
        Using cmd As New OleDbCommand("Select FileSize, FileTime, ShotYear, Status, FaceCount, AnalyzeTime, EngineVer From FacePhoto Where FileName = ?", m_lpConnection)
            cmd.Parameters.Add(Pth(strFileDesc))
            Using r As OleDbDataReader = cmd.ExecuteReader()
                If Not r.Read() Then Return Nothing
                e = New XElement("Faces",
                                 Attr("fileSize", r(0)), Attr("fileTime", r(1)), Attr("shotYear", r(2)), Attr("status", r(3)),
                                 Attr("count", r(4)), Attr("analyzed", r(5)), Attr("engine", r(6)))
            End Using
        End Using
        Dim byFace As New Dictionary(Of Integer, XElement)
        Using cmd As New OleDbCommand("Select r.FaceID, r.X, r.Y, r.W, r.H, r.Score, r.State, r.Similarity, p.FaceName, r.ShotYear, r.CreateTime, r.UpdateTime, r.Landmarks, r.Embedding" &
                                      " From FaceRegion AS r Left Join FacePerson AS p On r.PersonID = p.PersonID Where r.FileName = ? Order By r.FaceID", m_lpConnection)
            cmd.Parameters.Add(Pth(strFileDesc))
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    Dim f As New XElement("Face",
                                          Attr("x", r(1)), Attr("y", r(2)), Attr("w", r(3)), Attr("h", r(4)), Attr("score", r(5)),
                                          Attr("state", r(6)), Attr("similarity", r(7)), Attr("person", r(8)), Attr("shotYear", r(9)),
                                          Attr("created", r(10)), Attr("updated", r(11)))
                    If TypeOf r(12) Is Byte() Then f.Add(New XElement("Landmarks", Convert.ToBase64String(CType(r(12), Byte()))))
                    If TypeOf r(13) Is Byte() Then f.Add(New XElement("Embedding", Convert.ToBase64String(CType(r(13), Byte()))))
                    e.Add(f)
                    byFace(CInt(r(0))) = f
                End While
            End Using
        End Using
        If byFace.Count > 0 AndAlso HasTable("FaceReject") Then
            Using cmd As New OleDbCommand("Select j.FaceID, p.FaceName From (FaceReject AS j Inner Join FaceRegion AS r On j.FaceID = r.FaceID)" &
                                          " Inner Join FacePerson AS p On j.PersonID = p.PersonID Where r.FileName = ?", m_lpConnection)
                cmd.Parameters.Add(Pth(strFileDesc))
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        Dim f As XElement = Nothing
                        If byFace.TryGetValue(CInt(r(0)), f) AndAlso AsText(r(1)) <> "" Then f.Add(New XElement("Rejected", AsText(r(1))))
                    End While
                End Using
            End Using
        End If
        Return e
    End Function

    ''' <summary>The people (and their cover face: photo + position) and the name cards, for Faces.xml;
    ''' paths relative to <paramref name="strFolder"/>.</summary>
    Public Function PeopleArchive(ByVal strFolder As String) As List(Of XElement)
        Dim result As New List(Of XElement)
        If FaceTablesReady Then
            Using cmd As New OleDbCommand("Select p.FaceName, p.BirthYear, p.Hidden, p.CreateTime, p.UpdateTime, r.FileName, r.X, r.Y" &
                                          " From FacePerson AS p Left Join FaceRegion AS r On p.CoverFaceID = r.FaceID Order By p.PersonID", m_lpConnection)
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        Dim person As New XElement("Person", Attr("name", r(0)), Attr("birthYear", r(1)), Attr("hidden", r(2)),
                                                   Attr("created", r(3)), Attr("updated", r(4)))
                        If Not IsDBNull(r(5)) Then
                            person.Add(New XElement("Cover", New XAttribute("file", LibraryPath.ToRelative(FromStored(AsText(r(5))), strFolder)),
                                                    Attr("x", r(6)), Attr("y", r(7))))
                        End If
                        result.Add(person)
                    End While
                End Using
            End Using
        End If
        If m_bolImplement Then
            Try
                Using cmd As New OleDbCommand("Select FaceName, FileName From FaceIndex", m_lpConnection)
                    Using r As OleDbDataReader = cmd.ExecuteReader()
                        While r.Read()
                            result.Add(New XElement("Card", Attr("name", r(0)), New XAttribute("file", LibraryPath.ToRelative(FromStored(AsText(r(1))), strFolder))))
                        End While
                    End Using
                End Using
            Catch ex As OleDbException
                ' no FaceIndex
            End Try
        End If
        Return result
    End Function

    ''' <summary>The face tables made again from <paramref name="photos"/> (each photo's &lt;Faces&gt;) and
    ''' <paramref name="people"/> (Faces.xml's elements, paths relative to <paramref name="strFolder"/>):
    ''' the tables are emptied first; the name cards are only put back when FaceIndex is empty.
    ''' FaceTemplate is left empty for the face organizer. Returns what was made.</summary>
    Public Function RestoreFaces(ByVal photos As IEnumerable(Of (File As String, Faces As XElement)), ByVal people As IEnumerable(Of XElement),
                                 ByVal strFolder As String, Optional ByVal progress As Action(Of Integer) = Nothing) As (Photos As Integer, Faces As Integer, Persons As Integer)
        If Not FaceTablesReady Then Return (0, 0, 0)
        ClearFaceTables()
        Dim ids As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        Dim personOf = Function(name As String) As Integer
                           Dim id As Integer
                           If ids.TryGetValue(name, id) Then Return id
                           Exec(Nothing, "Insert Into FacePerson (FaceName, Hidden, CreateTime, UpdateTime) Values (?, False, ?, ?)",
                                Txt(name), P(OleDbType.Date, DateTime.Now), P(OleDbType.Date, DateTime.Now))
                           id = LastIdentity(Nothing)
                           ids(name) = id
                           Return id
                       End Function
        Dim covers As New List(Of (PersonID As Integer, File As String, X As Single, Y As Single))
        Dim cards As New List(Of (Name As String, File As String))
        For Each item In people
            Select Case item.Name.LocalName
                Case "Person"
                    Dim name As String = CStr(item.Attribute("name"))
                    If String.IsNullOrEmpty(name) OrElse ids.ContainsKey(name) Then Continue For
                    Exec(Nothing, "Insert Into FacePerson (FaceName, BirthYear, Hidden, CreateTime, UpdateTime) Values (?, ?, ?, ?, ?)",
                         Txt(name), P(OleDbType.SmallInt, IntOrNull(item, "birthYear")), P(OleDbType.Boolean, CStr(item.Attribute("hidden")) = "true"),
                         P(OleDbType.Date, DateOr(item, "created")), P(OleDbType.Date, DateOr(item, "updated")))
                    ids(name) = LastIdentity(Nothing)
                    Dim c As XElement = item.Element("Cover")
                    If c IsNot Nothing Then covers.Add((ids(name), LibraryPath.FromRelative(CStr(c.Attribute("file")), strFolder), Sng(c, "x"), Sng(c, "y")))
                Case "Card"
                    cards.Add((CStr(item.Attribute("name")), LibraryPath.FromRelative(CStr(item.Attribute("file")), strFolder)))
            End Select
        Next

        Dim faceIDs As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)   ' file|x|y -> FaceID, for the covers
        Dim nPhotos As Integer = 0, nFaces As Integer = 0
        For Each ph In photos
            Dim fx As XElement = ph.Faces
            If fx Is Nothing Then Continue For
            Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                Exec(tx, "Insert Into FacePhoto (FileName, FileSize, FileTime, ShotYear, Status, FaceCount, AnalyzeTime, EngineVer) Values (?, ?, ?, ?, ?, ?, ?, ?)",
                     Pth(ph.File), P(OleDbType.Integer, IntOrNull(fx, "fileSize")), P(OleDbType.Date, DateOr(fx, "fileTime")),
                     P(OleDbType.SmallInt, IntOrNull(fx, "shotYear")), P(OleDbType.UnsignedTinyInt, IntOrNull(fx, "status")),
                     P(OleDbType.SmallInt, IntOrNull(fx, "count")), P(OleDbType.Date, DateOr(fx, "analyzed")), Txt(CStr(fx.Attribute("engine"))))
                For Each f In fx.Elements("Face")
                    Dim person As String = CStr(f.Attribute("person"))
                    Exec(tx, "Insert Into FaceRegion (FileName, X, Y, W, H, Landmarks, Score, Embedding, PersonID, State, Similarity, ShotYear, CreateTime, UpdateTime)" &
                             " Values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                         Pth(ph.File), P(OleDbType.Single, Sng(f, "x")), P(OleDbType.Single, Sng(f, "y")), P(OleDbType.Single, Sng(f, "w")), P(OleDbType.Single, Sng(f, "h")),
                         P(OleDbType.Binary, Bytes(f, "Landmarks")), P(OleDbType.Single, Sng(f, "score")), P(OleDbType.LongVarBinary, Bytes(f, "Embedding")),
                         P(OleDbType.Integer, If(String.IsNullOrEmpty(person), Nothing, CObj(personOf(person)))),
                         P(OleDbType.UnsignedTinyInt, IntOrNull(f, "state")), P(OleDbType.Single, SngOrNull(f, "similarity")), P(OleDbType.SmallInt, IntOrNull(f, "shotYear")),
                         P(OleDbType.Date, DateOr(f, "created")), P(OleDbType.Date, DateOr(f, "updated")))
                    Dim id As Integer = LastIdentity(tx)
                    faceIDs(ph.File & "|" & CStr(f.Attribute("x")) & "|" & CStr(f.Attribute("y"))) = id
                    For Each rj In f.Elements("Rejected")
                        If rj.Value <> "" Then Exec(tx, "Insert Into FaceReject (FaceID, PersonID, CreateTime) Values (?, ?, ?)",
                                                    P(OleDbType.Integer, id), P(OleDbType.Integer, personOf(rj.Value)), P(OleDbType.Date, DateOr(f, "updated")))   ' not new: not exported again
                    Next
                    nFaces += 1
                Next
                tx.Commit()
            End Using
            nPhotos += 1
            progress?.Invoke(nPhotos)
        Next

        For Each c In covers
            Dim id As Integer
            If faceIDs.TryGetValue(c.File & "|" & c.X.ToString("R", Inv) & "|" & c.Y.ToString("R", Inv), id) Then
                Exec(Nothing, "Update FacePerson Set CoverFaceID = ? Where PersonID = ?", P(OleDbType.Integer, id), P(OleDbType.Integer, c.PersonID))
            End If
        Next
        If cards.Count > 0 Then
            Dim empty As Boolean
            Using cmd As New OleDbCommand("Select Count(*) From FaceIndex", m_lpConnection)
                empty = CInt(cmd.ExecuteScalar()) = 0
            End Using
            If empty Then
                For Each c In cards.Where(Function(x) Not String.IsNullOrEmpty(x.Name))
                    Execute("Insert Into FaceIndex (FaceName, FileName) Values (?, ?)", c.Name, ToStored(c.File))
                Next
            End If
        End If
        Return (nPhotos, nFaces, ids.Count)
    End Function

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    Private Function LastIdentity(ByVal tx As OleDbTransaction) As Integer
        Using cmd As New OleDbCommand("Select @@IDENTITY", m_lpConnection, tx)
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    Private Function HasTable(ByVal table As String) As Boolean
        Try
            Using cmd As New OleDbCommand("Select Top 1 * From " & table, m_lpConnection)
                cmd.ExecuteScalar()
            End Using
            Return True
        Catch ex As OleDbException
            Return False
        End Try
    End Function

    ''' <summary>A column as an attribute (Nothing for a Null: no attribute); numbers and dates invariant.</summary>
    Private Shared Function Attr(ByVal name As String, ByVal v As Object) As XAttribute
        If v Is Nothing OrElse IsDBNull(v) Then Return Nothing
        Dim s As String
        Select Case True
            Case TypeOf v Is Single : s = CSng(v).ToString("R", Inv)
            Case TypeOf v Is Double : s = CDbl(v).ToString("R", Inv)
            Case TypeOf v Is DateTime : s = CDate(v).ToString("s", Inv)
            Case TypeOf v Is Boolean : s = If(CBool(v), "true", "false")
            Case Else : s = Convert.ToString(v, Inv)
        End Select
        Return New XAttribute(name, s)
    End Function

    Private Shared Function IntOrNull(ByVal e As XElement, ByVal name As String) As Object
        Dim n As Long
        Return If(Long.TryParse(CStr(e.Attribute(name)), NumberStyles.Integer, Inv, n), CObj(n), Nothing)
    End Function

    Private Shared Function Sng(ByVal e As XElement, ByVal name As String) As Single
        Dim f As Single
        Return If(Single.TryParse(CStr(e.Attribute(name)), NumberStyles.Float, Inv, f), f, 0.0F)
    End Function

    Private Shared Function SngOrNull(ByVal e As XElement, ByVal name As String) As Object
        Dim f As Single
        Return If(Single.TryParse(CStr(e.Attribute(name)), NumberStyles.Float, Inv, f), CObj(f), Nothing)
    End Function

    Private Shared Function DateOr(ByVal e As XElement, ByVal name As String) As DateTime
        Dim d As DateTime
        Return If(DateTime.TryParse(CStr(e.Attribute(name)), Inv, DateTimeStyles.None, d), d, DateTime.Now)
    End Function

    Private Shared Function Bytes(ByVal e As XElement, ByVal name As String) As Object
        Dim x As XElement = e.Element(name)
        If x Is Nothing OrElse x.Value = "" Then Return Nothing
        Try
            Return Convert.FromBase64String(x.Value)
        Catch ex As FormatException
            Return Nothing
        End Try
    End Function

End Class
