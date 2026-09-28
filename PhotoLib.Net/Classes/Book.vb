Imports System.IO

' Port of 電子相簿\Lib\Class\Book.cls: a photo book ("攝影集") -- an *.Alm ini that lists photo files
' ([Photos] File=...) plus its [Create] / [Note] values. Shared logic with Class is in PhotoSet.
Public Class Book
    Inherits PhotoSet

    Private m_szFavoriteAlbumFile As String = ""

    Protected Overrides ReadOnly Property NoteFile As String
        Get
            Return m_szFavoriteAlbumFile
        End Get
    End Property

    ''' <summary>The .Alm file; setting it moves (renames) the file.</summary>
    Public Property FileName As String
        Get
            Return m_szFavoriteAlbumFile
        End Get
        Set(value As String)
            Dim fs As New Carbon.FileSystem
            fs.MoveFile(m_szFavoriteAlbumFile, value)
            m_szFavoriteAlbumFile = value
        End Set
    End Property

    Public Overrides ReadOnly Property Key As String
        Get
            Return m_szFavoriteAlbumFile
        End Get
    End Property

    Public Sub Construct(ByVal szFileDesc As String)
        Dim fs As New Carbon.FileSystem
        m_szFavoriteAlbumFile = szFileDesc
        m_szName = fs.AnalyseFile(fsBaseName, szFileDesc)
        m_bConstruct = True
        If Not fs.FileExists(szFileDesc) Then
            m_bReadOnly = True
            Clear()
        Else
            m_bReadOnly = fs.AttributeFile(Carbon.AttributeConstants.attReadonly, szFileDesc)
            Load()
        End If
    End Sub

    ''' <summary>(Re)reads the photo list from the .Alm, oldest first, and totals the file sizes.</summary>
    Public Overrides Function Load() As Boolean
        If Not CheckCreate() Then Return False
        Clear()
        Dim files() As String = Nothing
        Dim count As Integer = LoadProfileSectionValues(m_szFavoriteAlbumFile, "Photos", "File", files)
        For i = 0 To count - 1
            files(i) = files(i).Trim()
        Next
        count = SortPhotoFileByCreateDateTime(enumSortOrder.Ascending, files, count)
        Dim total As Long = 0
        For i = 0 To count - 1
            CreatePhotos(files(i))
            ' VB6's FileLen raised an error for a listed file that no longer exists; skip it instead
            If File.Exists(files(i)) Then total += New FileInfo(files(i)).Length
        Next
        If m_lpPhoto.Count > 0 Then
            m_bolLoad = True
            CalcFileLength(total, m_dblFileLength, m_strFileLengthUnit)
        Else
            m_dblFileLength = 0
            m_strFileLengthUnit = ""
        End If
        Return True
    End Function

    ''' <summary>Rewrites the whole .Alm (Create, Note, then the current photo list) in the ANSI code page.</summary>
    Public Function Save() As Boolean
        Dim fs As New Carbon.FileSystem
        Dim createDate As String = Create(enumCreate.crDate)
        Dim createTime As String = Create(enumCreate.crTime)
        Dim createSeq As Integer = CInt(Val(Create(enumCreate.crSeq)))
        Dim title As String = Note(enumNote.ntTitle)
        Dim noteDate As String = Note(enumNote.ntDate)
        Dim spot As String = Note(enumNote.ntSopt)
        Dim icon As String = Note(enumNote.ntIcon)
        Dim music As String = Note(enumNote.ntMusic)
        Dim remark As String = Note(enumNote.ntRemark)
        Dim cover As String = CoverPhoto

        If Not fs.DeleteFile(m_szFavoriteAlbumFile) Then
            MsgBox("刪除攝影集設定檔 " & m_szFavoriteAlbumFile & " 失敗...", MsgBoxStyle.Critical, "錯誤")
            Return False
        End If

        Using w As New StreamWriter(m_szFavoriteAlbumFile, False, AnsiText.Encoding)
            w.WriteLine("//建立資料")
            w.WriteLine("[Create]")
            w.WriteLine("Date=" & createDate)
            w.WriteLine("Time=" & createTime)
            w.WriteLine("Seq=" & createSeq)
            w.WriteLine(" ")
            w.WriteLine("//Note")
            w.WriteLine("[Note]")
            w.WriteLine("Title=" & title)
            w.WriteLine("Date=" & noteDate)
            w.WriteLine("Spot=" & spot)
            w.WriteLine("Icon=" & icon)
            w.WriteLine("Music=" & music)
            w.WriteLine("Remark=" & remark)
            If cover <> "" Then w.WriteLine("Cover=" & cover)   ' 設為相本封面 (new in the .NET port)
            w.WriteLine(" ")
            w.WriteLine("//相片")
            w.WriteLine("[Photos]")
            For Each p In m_lpPhoto
                w.WriteLine("File=" & p.FileDesc.Trim())
            Next
            w.WriteLine(" ")
        End Using
        Return True
    End Function

End Class
