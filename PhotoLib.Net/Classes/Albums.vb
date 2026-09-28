' Port of 電子相簿\Lib\Class\Albums.cls: one album root ("相片庫", items are Class folders) or one
' photo-book folder ("攝影集", items are Book files). Items load lazily on first access.
Public Class Albums

    Private m_enumAlbums As enumAlbums = enumAlbums.alGuide
    Private m_szAlbumPath As String = ""
    Private ReadOnly m_lpItem As New List(Of Object)
    Private m_szStorage As String = ""
    Private m_szName As String = ""
    Private m_bolLoad As Boolean = False

    Public Sub Clear()
        m_lpItem.Clear()
        m_bolLoad = False
    End Sub

    ''' <summary>ImageList key of the tree icon.</summary>
    Public ReadOnly Property Icon As String
        Get
            Return If(m_enumAlbums = enumAlbums.alFavorite, "icoAlbumBook", "icoAlbumFolder")
        End Get
    End Property

    Public ReadOnly Property Name As String
        Get
            Return m_szName
        End Get
    End Property

    Public ReadOnly Property Path As String
        Get
            Return m_szAlbumPath
        End Get
    End Property

    ''' <summary>The folder that holds this album root.</summary>
    Public ReadOnly Property Storage As String
        Get
            Return m_szStorage
        End Get
    End Property

    Public ReadOnly Property ItemCount As Integer
        Get
            Return m_lpItem.Count
        End Get
    End Property

    ''' <summary>A [Class] (album root) or a Book (photo-book folder); loads the items on first access.</summary>
    Public ReadOnly Property Item(ByVal index As Integer) As Object
        Get
            If index >= 0 AndAlso index >= m_lpItem.Count Then Load()
            If index < 0 OrElse index >= m_lpItem.Count Then
                MsgBox("取得 Albums.Item 的 Index 錯誤...", MsgBoxStyle.Critical, "錯誤")
                Return Nothing
            End If
            Return m_lpItem(index)
        End Get
    End Property

    Public Sub Construct(ByVal Albums As enumAlbums, ByVal PathSpec As String)
        m_enumAlbums = Albums
        m_szAlbumPath = PathSpec
        m_szStorage = ExactParentFolder(PathSpec)
        m_szName = ExactFolderName(PathSpec)
        If m_bolLoad Then
            m_bolLoad = False
            Load()
        End If
    End Sub

    Public Sub Refresh()
        Clear()
        Load()
    End Sub

    ''' <summary>Reads the items (once; Refresh reads again).</summary>
    Public Function Load() As Boolean
        If Not m_bolLoad Then
            Clear()
            Dim names() As String = Nothing
            If m_enumAlbums = enumAlbums.alGuide Then
                Dim count As Integer = ExactFolders(m_szAlbumPath, names)
                For i = 0 To count - 1
                    If Not IsSystemFolder(names(i)) Then CreateAlbumItem(names(i))
                Next
            Else
                Dim count As Integer = ExactFiles(m_szAlbumPath, "*." & gc_strFavoritesPattern, names, True)
                For i = 0 To count - 1
                    CreateFavoriteItem(names(i))
                Next
            End If
        End If
        m_bolLoad = True
        Return True
    End Function

    ''' <summary>Adds a new, empty item (folder or .Alm) and returns its index. The folder/file itself
    ''' is created by the caller, as in VB6.</summary>
    Public Function NewBook(ByVal BookName As String) As Integer
        If m_enumAlbums = enumAlbums.alGuide Then
            CreateAlbumItem(m_szAlbumPath & "\" & BookName)
        Else
            CreateFavoriteItem(m_szAlbumPath & "\" & BookName & "." & gc_strFavoritesPattern)
        End If
        Return m_lpItem.Count - 1
    End Function

    Private Sub CreateAlbumItem(ByVal strPathSpec As String)
        Dim c As New [Class]
        c.Construct(strPathSpec)
        m_lpItem.Add(c)
    End Sub

    Private Sub CreateFavoriteItem(ByVal strFileDesc As String)
        Dim b As New Book
        b.Construct(strFileDesc)
        m_lpItem.Add(b)
    End Sub

End Class
