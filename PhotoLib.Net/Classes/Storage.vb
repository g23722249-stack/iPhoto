' Port of 電子相簿\Lib\Class\Storage.cls: the top of the photo tree. Every sub-folder of each
' Config album root becomes an Albums (相片庫); every sub-folder of each favourite root becomes an
' Albums of photo books (攝影集).
'
'   Storage ┬ Album ┬ Class ┬ Photo ...      (folders)
'           └ Favorite ┬ Book ┬ Photo ...    (*.Alm files listing photos)
Public Class Storage

    Private m_lpConfig As Config
    Private ReadOnly m_lpAlbums As New List(Of Albums)
    Private ReadOnly m_lpFavorite As New List(Of Albums)

    Public ReadOnly Property Config As Config
        Get
            Return m_lpConfig
        End Get
    End Property

    Public Sub Construct(ByVal lpConfig As Config)
        m_lpConfig = lpConfig
        LoadAlbums()
        LoadFavorites()
    End Sub

    Public Sub Refresh()
        LoadAlbums()
        LoadFavorites()
    End Sub

    Public ReadOnly Property AlbumCount As Integer
        Get
            Return m_lpAlbums.Count
        End Get
    End Property

    Public ReadOnly Property Album(ByVal Index As Integer) As Albums
        Get
            If Index < 0 OrElse Index >= m_lpAlbums.Count Then
                MsgBox("取得 Album 的 Index 錯誤...", MsgBoxStyle.Critical, "錯誤")
                Return Nothing
            End If
            Return m_lpAlbums(Index)
        End Get
    End Property

    Public ReadOnly Property FavoriteCount As Integer
        Get
            Return m_lpFavorite.Count
        End Get
    End Property

    Public ReadOnly Property Favorite(ByVal Index As Integer) As Albums
        Get
            If Index < 0 OrElse Index >= m_lpFavorite.Count Then
                MsgBox("取得 Favorite 的 Index 錯誤...", MsgBoxStyle.Critical, "錯誤")
                Return Nothing
            End If
            Return m_lpFavorite(Index)
        End Get
    End Property

    Private Sub LoadAlbums()
        m_lpAlbums.Clear()
        For i = 0 To m_lpConfig.AlbumCount - 1
            Dim folders() As String = Nothing
            Dim count As Integer = ExactFolders(m_lpConfig.AlbumPath(i), folders)
            For j = 0 To count - 1
                If Not IsSystemFolder(folders(j)) Then m_lpAlbums.Add(NewAlbums(enumAlbums.alGuide, folders(j)))
            Next
        Next
    End Sub

    Private Sub LoadFavorites()
        m_lpFavorite.Clear()
        For i = 0 To m_lpConfig.FavoriteCount - 1
            Dim folders() As String = Nothing
            Dim count As Integer = ExactFolders(m_lpConfig.FavoritePath(i), folders)
            For j = 0 To count - 1
                If Not IsSystemFolder(folders(j)) Then m_lpFavorite.Add(NewAlbums(enumAlbums.alFavorite, folders(j)))
            Next
        Next
    End Sub

    Private Shared Function NewAlbums(ByVal kind As enumAlbums, ByVal path As String) As Albums
        Dim a As New Albums
        a.Construct(kind, path)
        Return a
    End Function

End Class
