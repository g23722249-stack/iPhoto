Imports System.IO
Imports PhotoMontage

' 蒙太奇拼貼 (the tool bar's 12, was 郵件; new in the .NET port): PhotoMontage's editor
' (C:\專案\PhotoMontage, referenced as a library) runs in iPhoto's own process. Its 選取照片 shows this
' library through LibrarySource -- 相簿, 攝影集, 人物 and the Dock (only when it has photos), pictures only,
' the edited version of an edited photo (EditedPhotos) -- and its 匯出 lets the user pick the album the
' work goes into (選相簿…); frmMain.Montage.vb then indexes it and shows it.
Friend Module MontageLink

    Friend Const AlbumsKey As String = "ALBUMS", BooksKey As String = "BOOKS", PeopleKey As String = "PEOPLE", DockKey As String = "DOCK"

    ''' <summary>This library as PhotoMontage's photo source.</summary>
    Friend Class LibrarySource
        Implements IPhotoSource

        Private ReadOnly m_start As IReadOnlyList(Of String)

        ''' <param name="start">The node shown first: AlbumsKey or BooksKey, the section's Path, the album's Key.</param>
        Public Sub New(ByVal start As IReadOnlyList(Of String))
            m_start = start
        End Sub

        Public ReadOnly Property Title As String Implements IPhotoSource.Title
            Get
                Return "iPhoto"
            End Get
        End Property

        Public ReadOnly Property StartPath As IReadOnlyList(Of String) Implements IPhotoSource.StartPath
            Get
                Return m_start
            End Get
        End Property

        Public Function Roots() As IReadOnlyList(Of PhotoSourceNode) Implements IPhotoSource.Roots
            Dim list As New List(Of PhotoSourceNode) From {
                New PhotoSourceNode(AlbumsKey, "相簿") With {.Checkable = False, .Expanded = True, .Children = Function() Sections(enumExeMode.exeAlbums)},
                New PhotoSourceNode(BooksKey, "攝影集") With {.Checkable = False, .Children = Function() Sections(enumExeMode.exeFavorites)}}
            If g_lpFaces?.Catalog IsNot Nothing AndAlso g_lpFaces.Catalog.VisiblePersons.Count > 0 Then
                list.Add(New PhotoSourceNode(PeopleKey, "人物") With {.Checkable = False, .Children = AddressOf People})
            End If
            If g_lpDock IsNot Nothing AndAlso g_lpDock.Count > 0 Then
                list.Add(New PhotoSourceNode(DockKey, "Dock (" & g_lpDock.Count & ")") With {.Photos = AddressOf DockPhotos})
            End If
            Return list
        End Function
    End Class

    ''' <summary>The sections of 相簿 or 攝影集; checking one takes all its albums.</summary>
    Private Function Sections(ByVal mode As enumExeMode) As IReadOnlyList(Of PhotoSourceNode)
        Dim list As New List(Of PhotoSourceNode)
        Dim count As Integer = If(mode = enumExeMode.exeAlbums, g_lpStorage.AlbumCount, g_lpStorage.FavoriteCount)
        For i = 0 To count - 1
            Dim section As Albums = If(mode = enumExeMode.exeAlbums, g_lpStorage.Album(i), g_lpStorage.Favorite(i))
            list.Add(New PhotoSourceNode(section.Path, section.Name) With {.Children = Function() AlbumsOf(section)})
        Next
        Return list
    End Function

    Private Function AlbumsOf(ByVal section As Albums) As IReadOnlyList(Of PhotoSourceNode)
        Dim list As New List(Of PhotoSourceNode)
        section.Load()
        For i = 0 To section.ItemCount - 1
            Dim album As PhotoSet = section.Item(i)
            Dim folderAlbum As [Class] = TryCast(album, [Class])
            list.Add(New PhotoSourceNode(album.Key, album.DisplayName) With {
                .Photos = Function() PicturesOf(album),
                .Folder = If(folderAlbum Is Nothing OrElse g_lpConfig.ReadOnly, Nothing, folderAlbum.Path.TrimEnd("\"c))})
        Next
        Return list
    End Function

    ''' <summary>An album's or photo book's pictures, in its order (an album lists the edited version
    ''' of an edited photo).</summary>
    Private Function PicturesOf(ByVal album As PhotoSet) As IReadOnlyList(Of String)
        album.Load()
        Dim files As New List(Of String)
        For i = 0 To album.PhotoCount - 1
            files.Add(album.Photo(i).FileDesc)
        Next
        Return files.Where(AddressOf IsPicture).ToList()
    End Function

    Private Function People() As IReadOnlyList(Of PhotoSourceNode)
        Dim list As New List(Of PhotoSourceNode)
        Dim cat As FaceCatalog = g_lpFaces?.Catalog
        If cat Is Nothing Then Return list
        For Each p In cat.VisiblePersons
            Dim person As FaceCatalog.PersonEntry = p
            list.Add(New PhotoSourceNode("P" & person.PersonID, person.Name & " (" & person.PhotoCount & ")") With {
                .Photos = Function() g_lpFaces.PhotosOf(person).Where(AddressOf IsPicture).ToList()})
        Next
        Return list
    End Function

    Private Function DockPhotos() As IReadOnlyList(Of String)
        Dim files As New List(Of String)
        For i = 0 To g_lpDock.Count - 1
            files.Add(g_lpDock.FileName(i))
        Next
        Return files.Where(AddressOf IsPicture).ToList()
    End Function

    Private Function IsPicture(ByVal file As String) As Boolean
        Return Not String.IsNullOrEmpty(file) AndAlso GetMediaType(g_lpFileSystem, file) = enumPhotoMediaType.mdImage AndAlso IO.File.Exists(file)
    End Function

    ''' <summary>True when <paramref name="file"/> is inside one of the library's album roots.</summary>
    Friend Function InLibrary(ByVal file As String) As Boolean
        Dim folder As String = Path.GetDirectoryName(file)
        For i = 0 To g_lpConfig.AlbumCount - 1
            Dim root As String = g_lpConfig.AlbumPath(i).TrimEnd("\"c) & "\"
            If folder.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

End Module
