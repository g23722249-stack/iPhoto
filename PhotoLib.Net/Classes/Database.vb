Imports System.Data
Imports System.Data.OleDb

' Port of 電子相簿\Lib\Class\Database.cls: the photo index in iPhoto.mdb (Jet 4.0), used for the
' calendar, "faces" and search. ADODB -> System.Data.OleDb (the app is built x86 because Jet is 32-bit).
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
Public Class Database
    Implements IDisposable

    Private Const mc_strTableName As String = "PhotoIndex"

    Private m_lpConnection As OleDbConnection
    Private m_bolImplement As Boolean = False

    ''' <summary>True once Construct opened the database.</summary>
    Public ReadOnly Property Implement As Boolean
        Get
            Return m_bolImplement
        End Get
    End Property

    Public ReadOnly Property Connection As OleDbConnection
        Get
            Return m_lpConnection
        End Get
    End Property

    ''' <summary>Opens the .mdb; a missing file or a failed open just leaves Implement = False (VB6).</summary>
    Public Sub Construct(ByVal szFileDesc As String)
        Close()
        m_bolImplement = False
        If String.IsNullOrEmpty(szFileDesc) OrElse Not IO.File.Exists(szFileDesc) Then Return
        Try
            m_lpConnection = New OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & szFileDesc)
            m_lpConnection.Open()
            m_bolImplement = True
        Catch
            Close()
        End Try
    End Sub

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
    ''' <summary>Empties PhotoIndex and re-adds every photo of every album root.</summary>
    Public Sub Rebuild(ByVal lpStorage As Storage)
        If Not m_bolImplement Then Return
        TruncateTable()
        Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
            For i = 0 To lpStorage.AlbumCount - 1
                Dim album As Albums = lpStorage.Album(i)
                album.Load()
                For j = 0 To album.ItemCount - 1
                    Dim c As [Class] = TryCast(album.Item(j), [Class])
                    If c Is Nothing Then Continue For
                    c.Load()
                    For k = 0 To c.PhotoCount - 1
                        InsertPhoto(c.Photo(k), tx)
                    Next
                Next
            Next
            tx.Commit()
        End Using
    End Sub

    ''' <summary>Replaces the photo's row (delete + insert), as after editing its details.</summary>
    Public Sub AddItem(ByVal lpPhoto As Photo)
        Try
            Delete(lpPhoto.FileDesc)
            Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                InsertPhoto(lpPhoto, tx)
                tx.Commit()
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub Delete(ByVal strFileDesc As String)
        Try
            Execute("Delete From " & mc_strTableName & " Where FileName = ?", strFileDesc)
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

    ''' <summary>VB6 MovePhotoToRecord: the date columns come from the photo's Exif date (yyyyMMdd).</summary>
    Private Sub InsertPhoto(ByVal p As Photo, ByVal tx As OleDbTransaction)
        Dim d As String = PadNum(p.Exif(enumPhotoExif.peDate), 8)
        Dim y As String = Mid(d, 1, 4), m As String = Mid(d, 5, 2), dd As String = Mid(d, 7, 2)
        Using cmd As New OleDbCommand("Insert Into " & mc_strTableName &
                " (FileName, Create_Date, Create_Year, Create_Month, Create_Day, Exif_Date, Exif_Year, Exif_Month, Exif_Day," &
                "  Exif_Title, Exif_Character, Exif_Spot, Exif_Remark, Exif_KeyWord, Exif_Ranking, ModifyTime)" &
                " Values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", m_lpConnection, tx)
            For Each v In {p.FileDesc, d, y, m, dd, d, y, m, dd,
                           p.Exif(enumPhotoExif.peTitle), p.Exif(enumPhotoExif.peCharacter), p.Exif(enumPhotoExif.peSpot),
                           p.Exif(enumPhotoExif.peRemark), p.Exif(enumPhotoExif.peKeyWord)}
                cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.VarWChar, .Value = If(v, "")})
            Next
            cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Integer, .Value = CInt(Val(p.Exif(enumPhotoExif.peRanking)))})
            cmd.Parameters.Add(New OleDbParameter With {.OleDbType = OleDbType.Date, .Value = DateTime.Now})
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Makes <paramref name="strFileDesc"/> the cover of face <paramref name="strFaceName"/>.</summary>
    Public Sub AddFaceCover(ByVal strFaceName As String, ByVal strFileDesc As String)
        Try
            Execute("Delete From FaceIndex Where FaceName = ?", strFaceName)
            Execute("Insert Into FaceIndex (FaceName, FileName) Values (?, ?)", strFaceName, strFileDesc)
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
            Using cmd As New OleDbCommand("Select FileName, FaceName From FaceIndex", m_lpConnection)
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        files.Add(AsText(r(0)))
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
        strFileDesc = FileNames("Select FileName From " & mc_strTableName & " Where InStr(Exif_Character, ?) > 0", strName)
        Return strFileDesc.Length
    End Function

    ''' <summary>Photos dated within the last 365 days.</summary>
    Public Function LoadOneYearFile(ByRef strFileDesc() As String) As Integer
        strFileDesc = FileNames("Select FileName From " & mc_strTableName & " Where Exif_Date > ?", DateTime.Now.AddDays(-365).ToString("yyyyMMdd"))
        Return strFileDesc.Length
    End Function

    ''' <summary>Five-star photos.</summary>
    Public Function LoadRankingFile(ByRef strFileDesc() As String) As Integer
        strFileDesc = FileNames("Select FileName From " & mc_strTableName & " Where Exif_Ranking = 5")
        Return strFileDesc.Length
    End Function

    Public Function LoadDateFile(ByVal intYear As Integer, ByVal intMonth As Integer, ByVal intDay As Integer, ByRef strFileDesc() As String) As Integer
        strFileDesc = FileNames("Select FileName From " & mc_strTableName & " Where Exif_Year = ? And Exif_Month = ? And Exif_Day = ?",
                                intYear.ToString("0000"), intMonth.ToString("00"), intDay.ToString("00"))
        Return strFileDesc.Length
    End Function

    ''' <summary>Files matching a WHERE clause built by the caller (frmSearch).</summary>
    Public Function SearchFileArrayList(ByVal strWhereSQL As String, ByRef strFileDesc() As String) As Integer
        strFileDesc = FileNames("Select FileName From " & mc_strTableName & " Where " & strWhereSQL)
        Return strFileDesc.Length
    End Function

    ''' <summary>All PhotoIndex columns for a WHERE clause (blank = every row); True if any row matched.
    ''' VB6 returned a live ADODB.Recordset, this returns a DataTable.</summary>
    Public Function SearchFileRecordSet(ByVal strWhereSQL As String, ByRef objRecordSet As DataTable) As Boolean
        objRecordSet = New DataTable(mc_strTableName)
        If Not m_bolImplement Then Return False
        Dim sql As String = "Select * From " & mc_strTableName & If(String.IsNullOrWhiteSpace(strWhereSQL), "", " Where " & strWhereSQL)
        Using da As New OleDbDataAdapter(sql, m_lpConnection)
            da.Fill(objRecordSet)
        End Using
        Return objRecordSet.Rows.Count > 0
    End Function

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    Private Sub Execute(ByVal sql As String, ParamArray values As Object())
        If Not m_bolImplement Then Return
        Using cmd As OleDbCommand = NewCommand(sql, values)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function FileNames(ByVal sql As String, ParamArray values As Object()) As String()
        Dim result As New List(Of String)
        If m_bolImplement Then
            Using cmd As OleDbCommand = NewCommand(sql, values)
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        result.Add(AsText(r(0)))
                    End While
                End Using
            End Using
        End If
        Return result.ToArray()
    End Function

    Private Function DistinctNumbers(ByVal sql As String, ParamArray values As Object()) As HashSet(Of Integer)
        Dim result As New HashSet(Of Integer)
        If m_bolImplement Then
            Using cmd As OleDbCommand = NewCommand(sql, values)
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        result.Add(CInt(Val(AsText(r(0)))))
                    End While
                End Using
            End Using
        End If
        Return result
    End Function

    Private Function NewCommand(ByVal sql As String, ByVal values As Object()) As OleDbCommand
        Dim cmd As New OleDbCommand(sql, m_lpConnection)
        For Each v In values
            cmd.Parameters.Add(New OleDbParameter With {.Value = If(v, DBNull.Value)})
        Next
        Return cmd
    End Function

    Private Shared Function AsText(ByVal v As Object) As String
        Return If(v Is Nothing OrElse v Is DBNull.Value, "", v.ToString())
    End Function

End Class
