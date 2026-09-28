' Common part of Class (a folder of photos) and Book (a photo book, *.Alm), which in VB6 were two
' near-identical classes: the photo list and the [Create] / [Note] values kept in an ini file (the
' folder's Note.Ini for a Class, the .Alm file itself for a Book). The public members keep the VB6
' names and parameterised properties (Photo(i), Note(ntTitle), Create(crDate)) so ported callers
' don't change.
Public MustInherit Class PhotoSet

    Protected ReadOnly m_lpPhoto As New List(Of Photo)
    Protected m_szName As String = ""
    Protected m_bConstruct As Boolean = False
    Protected m_bolLoad As Boolean = False
    Protected m_bReadOnly As Boolean = False
    Protected m_dblFileLength As Double
    Protected m_strFileLengthUnit As String = ""

    ''' <summary>The ini that holds [Create] and [Note].</summary>
    Protected MustOverride ReadOnly Property NoteFile As String

    Public MustOverride ReadOnly Property Key As String

    Public MustOverride Function Load() As Boolean

    Public ReadOnly Property Name As String
        Get
            Return m_szName
        End Get
    End Property

    ''' <summary>Name, followed by the note title when it differs.</summary>
    Public ReadOnly Property DisplayName As String
        Get
            Dim title As String = Note(enumNote.ntTitle)
            If m_szName.Trim() = If(title, "").Trim() Then Return If(title, "").Trim()
            Return (m_szName & " " & title).Trim()
        End Get
    End Property

    Public ReadOnly Property FileLength As Double
        Get
            Return m_dblFileLength
        End Get
    End Property

    Public ReadOnly Property FileLengthUnit As String
        Get
            Return m_strFileLengthUnit
        End Get
    End Property

    Public ReadOnly Property [ReadOnly] As Boolean
        Get
            Return m_bReadOnly
        End Get
    End Property

    Public ReadOnly Property PhotoCount As Integer
        Get
            Return m_lpPhoto.Count
        End Get
    End Property

    Public ReadOnly Property Photo(ByVal Index As Integer) As Photo
        Get
            If Index < 0 OrElse Index >= m_lpPhoto.Count Then
                MsgBox("取得 Photo 的 Index 錯誤...", MsgBoxStyle.Critical, "錯誤")
                Return Nothing
            End If
            Return m_lpPhoto(Index)
        End Get
    End Property

    Public Property Create(ByVal Mode As enumCreate) As String
        Get
            If Not CheckCreate() Then Return Nothing
            Dim ini As New Carbon.IniFile With {.FileName = NoteFile}
            Select Case Mode
                Case enumCreate.crDate : Return ini.SimpleGetValue("Create", "Date")
                Case enumCreate.crTime : Return ini.SimpleGetValue("Create", "Time")
                Case enumCreate.crSeq : Return ini.SimpleGetValue("Create", "Seq")
            End Select
            Return ""
        End Get
        Set(value As String)
            If Not CheckCreate() Then Return
            Dim ini As New Carbon.IniFile With {.FileName = NoteFile}
            Select Case Mode
                Case enumCreate.crDate : ini.SimpleSetValue("Create", "Date", value)
                Case enumCreate.crTime : ini.SimpleSetValue("Create", "Time", value)
                Case enumCreate.crSeq : ini.SimpleSetValue("Create", "Seq", value)
            End Select
        End Set
    End Property

    Public Property Note(ByVal Mode As enumNote) As String
        Get
            If Not CheckCreate() Then Return Nothing
            Dim ini As New Carbon.IniFile With {.FileName = NoteFile}
            Select Case Mode
                Case enumNote.ntTitle : Return ini.SimpleGetValue("Note", "Title")
                Case enumNote.ntDate : Return ini.SimpleGetValue("Note", "Date")
                Case enumNote.ntSopt : Return ini.SimpleGetValue("Note", "Spot")
                Case enumNote.ntIcon
                    Dim icon As String = ini.SimpleGetValue("Note", "Icon")
                    Return If(icon.Trim().Length = 0, "icoOther", icon)
                Case enumNote.ntSelIcon : Return "icoFilm"
                Case enumNote.ntMusic : Return ini.SimpleGetValue("Note", "Music")
                Case enumNote.ntRemark : Return ini.SimpleGetValue("Note", "Remark").Replace(vbCrLf, vbTab)
            End Select
            Return ""
        End Get
        Set(value As String)
            If Not CheckCreate() Then Return
            Dim ini As New Carbon.IniFile With {.FileName = NoteFile}
            Select Case Mode
                Case enumNote.ntTitle : ini.SimpleSetValue("Note", "Title", value)
                Case enumNote.ntDate : ini.SimpleSetValue("Note", "Date", value)
                Case enumNote.ntSopt : ini.SimpleSetValue("Note", "Spot", value)
                Case enumNote.ntIcon : ini.SimpleSetValue("Note", "Icon", value)
                Case enumNote.ntSelIcon
                Case enumNote.ntMusic : ini.SimpleSetValue("Note", "Music", value)
                Case enumNote.ntRemark : ini.SimpleSetValue("Note", "Remark", If(value, "").Replace(vbCrLf, vbTab))
            End Select
        End Set
    End Property

    ''' <summary>The photo chosen for the album's cover (設為相本封面, new in the .NET port): its full path,
    ''' kept as [Note] Cover in the note ini / .Alm; "" when none was chosen.</summary>
    Public Property CoverPhoto As String
        Get
            If Not CheckCreate() Then Return ""
            Dim ini As New Carbon.IniFile With {.FileName = NoteFile}
            Return If(ini.SimpleGetValue("Note", "Cover"), "").Trim()
        End Get
        Set(value As String)
            If Not CheckCreate() Then Return
            Dim ini As New Carbon.IniFile With {.FileName = NoteFile}
            ini.SimpleSetValue("Note", "Cover", If(value, ""))
        End Set
    End Property

    Public Sub Clear()
        m_lpPhoto.Clear()
    End Sub

    Public Sub RemovePhoto(ByVal FileDesc As String)
        Dim i As Integer = IndexOfPhoto(FileDesc)
        If i >= 0 Then m_lpPhoto.RemoveAt(i)
    End Sub

    ''' <summary>Adds the file if it exists and isn't in the set yet (used when building a photo book).</summary>
    Public Function AddPhoto(ByVal FileDesc As String) As Photo
        Dim i As Integer = CreatePhotos(FileDesc)
        Return If(i >= 0, m_lpPhoto(i), Nothing)
    End Function

    ''' <summary>Index of the file's Photo, adding it first when the file exists; -1 if it doesn't.</summary>
    Protected Function CreatePhotos(ByVal szFileDesc As String) As Integer
        If Not IO.File.Exists(szFileDesc) Then Return -1
        Dim i As Integer = IndexOfPhoto(szFileDesc)
        If i >= 0 Then Return i
        Dim p As New Photo
        p.Construct(szFileDesc)
        m_lpPhoto.Add(p)
        Return m_lpPhoto.Count - 1
    End Function

    Private Function IndexOfPhoto(ByVal fileDesc As String) As Integer
        Dim target As String = If(fileDesc, "").Trim()
        For i = 0 To m_lpPhoto.Count - 1
            If String.Equals(m_lpPhoto(i).FileDesc.Trim(), target, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Return -1
    End Function

    Protected Function CheckCreate() As Boolean
        If Not m_bConstruct Then MsgBox("Class 物件尚未執行 Create...", MsgBoxStyle.Critical, "錯誤")
        Return m_bConstruct
    End Function

End Class
