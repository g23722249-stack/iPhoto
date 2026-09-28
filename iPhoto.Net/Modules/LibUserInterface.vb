' Port of iPhoto\Module\LibUserInterface.bas: fills the album / photo-book TreeViews and the MediaList,
' and remembers the last opened album/class.
'
' MSComctlLib.TreeView -> System.Windows.Forms.TreeView:
'   - the VB6 flat, key-indexed Nodes collection becomes TreeNode.Name + Nodes.Find(key, True)
'     (FindNode / AllNodes below); Node.Image/SelectedImage -> ImageKey/SelectedImageKey.
'   - the tree is lazy: every section gets one "FAKE_<key>" child so it shows a "+", and the Expand
'     handler swaps it for the real children (ExpandFake...).
' The Recent* values still go through SaveSetting/GetSetting under the VB6 application name, i.e.
' HKCU\Software\VB and VBA Program Settings\iPhoto, so the .NET build picks up where VB6 left off.
' Not ported: AddPhotoToMediaListPlus / DockedMediaListPlus (Aqua.MediaListPlus is not used by any
' ported form) and AddSubjectToListView (only frmSubject uses it; it moves to PhotoLib with that form).
Friend Module LibUserInterface

    Private Const AppName As String = "iPhoto"   ' VB6 App.EXEName

    '==================================================================================================
    ' TreeView helpers
    '==================================================================================================
    ''' <summary>VB6 lpTreeView.Nodes(key): the node with that key anywhere in the tree, or Nothing.</summary>
    Friend Function FindNode(ByVal lpTreeView As TreeView, ByVal szKey As String) As TreeNode
        If String.IsNullOrEmpty(szKey) Then Return Nothing
        Dim hits() As TreeNode = lpTreeView.Nodes.Find(szKey, True)
        Return If(hits.Length > 0, hits(0), Nothing)
    End Function

    ''' <summary>Every node, parents before children (VB6 iterated its flat Nodes collection).</summary>
    Friend Function AllNodes(ByVal lpTreeView As TreeView) As List(Of TreeNode)
        Dim list As New List(Of TreeNode)
        Dim walk As Action(Of TreeNodeCollection) = Nothing
        walk = Sub(nodes)
                   For Each n As TreeNode In nodes
                       list.Add(n)
                       walk(n.Nodes)
                   Next
               End Sub
        walk(lpTreeView.Nodes)
        Return list
    End Function

    ''' <summary>VB6 Nodes.Add(..., key, text, image, selectedImage); a node without its own
    ''' selected image shows its image when selected, as in MSComctl.</summary>
    Private Function AddNode(ByVal nodes As TreeNodeCollection, ByVal szKey As String, ByVal szText As String,
                             Optional ByVal szImage As String = "", Optional ByVal szSelImage As String = "") As TreeNode
        Dim n As TreeNode = nodes.Add(szKey, szText)
        If szImage <> "" Then
            n.ImageKey = szImage
            n.SelectedImageKey = If(szSelImage <> "", szSelImage, szImage)
        End If
        Return n
    End Function

    Private Function AddFakeChild(ByVal lpParent As TreeNode) As TreeNode
        Dim n As TreeNode = AddNode(lpParent.Nodes, "FAKE_" & lpParent.Name, "FAKE")
        n.Tag = -1
        Return n
    End Function

    ''' <summary>Removes the section's FAKE child; False when it was already expanded once.</summary>
    Private Function RemoveFakeChild(ByVal lpTreeView As TreeView, ByVal lpSectionNode As TreeNode) As Boolean
        Dim fake As TreeNode = FindNode(lpTreeView, "FAKE_" & lpSectionNode.Name)
        If fake Is Nothing Then Return False
        fake.Remove()
        Return True
    End Function

    '==================================================================================================
    ' Albums / favorites tree
    '==================================================================================================
    '//加入 Album/Favorite 至 Treeview
    Public Sub AddSectionFakeKeyToTreeView(ByVal enumExe As enumExeMode, ByVal lpStorage As Storage, ByVal lpTreeView As TreeView, Optional ByVal bolSpecialFolder As Boolean = False)
        Dim intCount As Integer

        lpTreeView.BeginUpdate()
        lpTreeView.Nodes.Clear()
        lpTreeView.ImageList = lpStorage.Config.ImageListSubject

        Select Case enumExe
            Case enumExeMode.exeAlbums : intCount = lpStorage.AlbumCount
            Case enumExeMode.exeFavorites : intCount = lpStorage.FavoriteCount
        End Select

        If enumExe = enumExeMode.exeFavorites AndAlso bolSpecialFolder Then
            AddNode(lpTreeView.Nodes, "FACE", "面孔", "icoFace").Tag = "FACE"
            AddNode(lpTreeView.Nodes, "ONEYEAR", "最近一年的照片", "icoAlbumFolder").Tag = "ONEYEAR"
            AddNode(lpTreeView.Nodes, "STAR", "五顆星評價", "icoStar").Tag = "STAR"
        End If
        For I As Integer = 0 To intCount - 1
            Dim lpSection As Albums = If(enumExe = enumExeMode.exeAlbums, lpStorage.Album(I), lpStorage.Favorite(I))
            Dim lpNode As TreeNode = AddNode(lpTreeView.Nodes, lpSection.Path, lpSection.Name, lpSection.Icon)
            lpNode.Tag = I
            AddFakeChild(lpNode)
        Next
        lpTreeView.EndUpdate()
    End Sub

    '//展開虛擬的類別至 TreeView
    Public Sub ExpandFakeSectionKeyToTreeView(ByVal enumExe As enumExeMode, ByVal lpStorage As Storage, ByVal lpTreeView As TreeView, ByVal lpSectionNode As TreeNode, ByVal iSectionIndex As Integer)
        If Not RemoveFakeChild(lpTreeView, lpSectionNode) Then Return
        Dim lpSection As Albums
        Select Case enumExe
            Case enumExeMode.exeAlbums : lpSection = lpStorage.Album(iSectionIndex)
            Case enumExeMode.exeFavorites : lpSection = lpStorage.Favorite(iSectionIndex)
            Case Else : Return
        End Select

        lpSection.Load()
        For I As Integer = 0 To lpSection.ItemCount - 1
            Dim lpKey As PhotoSet = lpSection.Item(I)
            AddNode(lpSectionNode.Nodes, lpKey.Key, lpKey.DisplayName, lpKey.Note(enumNote.ntIcon), lpKey.Note(enumNote.ntSelIcon)).Tag = I
        Next
    End Sub

    '//加入 Album/Favorite 路徑,虛擬的類別至 Treeview
    Public Sub AddStorageSectionPathFakeKeyToTreeView(ByVal enumExe As enumExeMode, ByVal lpConfig As Config, ByVal lpTreeView As TreeView)
        lpTreeView.BeginUpdate()
        lpTreeView.Nodes.Clear()
        lpTreeView.ImageList = lpConfig.ImageListStorage

        Select Case enumExe
            Case enumExeMode.exeAlbums
                For I As Integer = 0 To lpConfig.AlbumCount - 1
                    Dim n As TreeNode = AddNode(lpTreeView.Nodes, lpConfig.AlbumPath(I), lpConfig.AlbumPath(I), "icoFolderClose", "icoFolderOpen")
                    n.Tag = I
                    AddFakeChild(n)
                Next
            Case enumExeMode.exeFavorites
                For I As Integer = 0 To lpConfig.FavoriteCount - 1
                    Dim n As TreeNode = AddNode(lpTreeView.Nodes, lpConfig.FavoritePath(I), lpConfig.FavoritePath(I), "icoFolderClose", "icoFolderOpen")
                    n.Tag = I
                    AddFakeChild(n)
                Next
        End Select
        lpTreeView.EndUpdate()
    End Sub

    Private Function FolderIcon(ByVal enumExe As enumExeMode) As String
        Select Case enumExe
            Case enumExeMode.exeAlbums : Return "icoClass"
            Case enumExeMode.exeFavorites : Return "icoBook"
            Case Else : Return ""
        End Select
    End Function

    '//展開虛擬的 Class/Book 至 TreeView
    Public Sub ExpandFakeSectionKeyPathToTreeView(ByVal enumExe As enumExeMode, ByVal lpTreeView As TreeView, ByVal lpSectionNode As TreeNode)
        If Not RemoveFakeChild(lpTreeView, lpSectionNode) Then Return
        Dim strPath() As String = Nothing
        Dim intPath As Integer = frmResAlbum.ExactFolders(lpSectionNode.Name, strPath)
        For I As Integer = 0 To intPath - 1
            If Not IsSystemFolder(strPath(I)) Then
                AddNode(lpSectionNode.Nodes, strPath(I), g_lpFileSystem.AnalyseFile(fsBaseName, strPath(I)).Trim(), FolderIcon(enumExe))
            End If
        Next
    End Sub

    '//展開虛擬的 Photo 至 TreeView
    Public Sub ExpandFakeSectionKeyPathFakeChildToTreeView(ByVal enumExe As enumExeMode, ByVal lpTreeView As TreeView, ByVal lpSectionNode As TreeNode)
        If Not RemoveFakeChild(lpTreeView, lpSectionNode) Then Return
        Dim strPath() As String = Nothing
        Dim intPath As Integer = frmResAlbum.ExactFolders(lpSectionNode.Name, strPath)
        For I As Integer = 0 To intPath - 1
            If Not IsSystemFolder(strPath(I)) Then
                Dim n As TreeNode = AddNode(lpSectionNode.Nodes, strPath(I), g_lpFileSystem.AnalyseFile(fsBaseName, strPath(I)).Trim(), FolderIcon(enumExe))
                n.Tag = I
                AddFakeChild(n)
            End If
        Next
    End Sub

    '//展開虛擬的 Class/Book 至 TreeView
    Public Sub ExpandFakePhotoToTreeView(ByVal enumExe As enumExeMode, ByVal lpTreeView As TreeView, ByVal lpSectionNode As TreeNode)
        Dim lpSection As New Albums
        lpSection.Construct(If(enumExe = enumExeMode.exeFavorites, enumAlbums.alFavorite, enumAlbums.alGuide), lpSectionNode.Name)
        lpSection.Load()

        If Not RemoveFakeChild(lpTreeView, lpSectionNode) Then Return
        For I As Integer = 0 To lpSection.ItemCount - 1
            Dim lpKey As PhotoSet = lpSection.Item(I)
            AddNode(lpSectionNode.Nodes, lpKey.Key, lpKey.DisplayName.Trim(), "icoNote")
        Next
    End Sub

    '==================================================================================================
    ' MediaList
    '==================================================================================================
    ''' <summary>VB6 Format("20040424", "0000/00/00"): digits through a numeric picture; anything else as is.</summary>
    Private Function FormatDigits(ByVal szValue As String, ByVal szPicture As String) As String
        Dim n As Long
        If Long.TryParse(If(szValue, "").Trim(), n) Then Return n.ToString(szPicture)
        Return If(szValue, "")
    End Function

    ''' <summary>VB6 "If Val(ranking) = 0 Then 無評價 Else Val(ranking)".</summary>
    Friend Function RankingOf(ByVal lpPhoto As Photo) As Aqua.MediaItemRanking
        Dim v As Integer = CInt(Val(lpPhoto.Exif(enumPhotoExif.peRanking)))
        If v < 1 OrElse v > 5 Then Return Aqua.MediaItemRanking.NoRating
        Return CType(v, Aqua.MediaItemRanking)
    End Function

    '//將檔案加入 Aqua.MediaList
    Public Sub AddPhotoToMediaList(ByVal enumMode As enumExeMode, ByVal lpStorage As Storage, ByVal lpMediaList As Aqua.MediaList, ByVal iLimit As Integer, ByVal iSectionIndex As Integer, ByVal iKeyIndex As Integer)
        lpMediaList.Clear()
        lpMediaList.Limit = iLimit

        Dim lpKey As PhotoSet
        Select Case enumMode
            Case enumExeMode.exeAlbums : lpKey = lpStorage.Album(iSectionIndex).Item(iKeyIndex)
            Case enumExeMode.exeFavorites : lpKey = lpStorage.Favorite(iSectionIndex).Item(iKeyIndex)
            Case Else : Return
        End Select

        lpKey.Load()
        For I As Integer = 0 To lpKey.PhotoCount - 1
            Dim lpPhoto As Photo = lpKey.Photo(I)
            Dim strToolTipText As String = "時間：" & FormatDigits(lpPhoto.Exif(enumPhotoExif.peDate), "0000/00/00") & " " & FormatDigits(lpPhoto.Exif(enumPhotoExif.peTime), "00:00:00")
            strToolTipText &= vbCrLf & "人物：" & lpPhoto.Exif(enumPhotoExif.peCharacter)
            strToolTipText &= vbCrLf & "備註：" & lpPhoto.Exif(enumPhotoExif.peRemark)

            ' a video shows its Thumb\<name>.jpg when there is one
            Dim strShowFile As String = lpPhoto.FileDesc
            If GetMediaType(g_lpFileSystem, lpPhoto.FileDesc) = enumPhotoMediaType.mdVideo Then
                Dim strThumbFile As String = g_lpFileSystem.AnalyseFile(fsParentFolderName, lpPhoto.FileDesc) & "\Thumb\" & g_lpFileSystem.AnalyseFile(fsBaseName, lpPhoto.FileDesc) & ".jpg"
                If g_lpFileSystem.FileExists(strThumbFile) Then strShowFile = strThumbFile
            End If

            Try
                Dim lpItem As Aqua.MediaItem = lpMediaList.AddItem(strShowFile, strToolTipText)
                lpItem.Ranking = RankingOf(lpPhoto)
            Catch ex As Exception
                Debug.Print(ex.Message)   ' VB6: On Error Resume Next semantics via Err.Number check
            End Try
            ' VB6 scrolled to the end after every photo (so each one was loaded as it went by); the list
            ' now stays where it is and only loads what is on screen
            Application.DoEvents()
        Next
    End Sub

    '//Dock MediaList 的檔案
    Public Sub DockedMediaList(ByVal lpDock As Dock, ByVal lpMediaList As Aqua.MediaList)
        For I As Integer = 0 To lpMediaList.Count - 1
            Dim objItem As Aqua.MediaItem = lpMediaList.Item(I)
            objItem.Marked = lpDock.Selected(objItem.FileName)
        Next
    End Sub

    '==================================================================================================
    ' Recently opened album / class
    '==================================================================================================
    '//紀錄最近的 Album or Favorite By TreeView
    Public Sub SaveRecentSection(ByVal enumExe As enumExeMode, ByVal lpStorage As Storage, ByVal iSectionIndex As Integer)
        Select Case enumExe
            Case enumExeMode.exeAlbums : SaveSetting(AppName, "Recent", "Album", lpStorage.Album(iSectionIndex).Path)
            Case enumExeMode.exeFavorites : SaveSetting(AppName, "Recent", "Favorite", lpStorage.Favorite(iSectionIndex).Path)
        End Select
    End Sub

    Public Sub SaveRecentSectionValue(ByVal enumExe As enumExeMode, ByVal strSectionValue As String)
        Select Case enumExe
            Case enumExeMode.exeAlbums : SaveSetting(AppName, "Recent", "Album", strSectionValue)
            Case enumExeMode.exeFavorites : SaveSetting(AppName, "Recent", "Favorite", strSectionValue)
        End Select
    End Sub

    '//紀錄最近匯入的 Album By TreeView
    Public Sub SaveImportRecentSectionValue(ByVal strSectionValue As String)
        SaveSetting(AppName, "Import Recent", "Album", strSectionValue)
    End Sub

    ''' <summary>Selects and expands the node with that key (case-insensitive, as VB6 compared UCase$).</summary>
    Private Function SelectNodeByKey(ByVal lpTreeView As TreeView, ByVal szKey As String, ByVal bolExpand As Boolean) As Boolean
        If szKey.Trim() = "" Then Return False
        For Each n As TreeNode In AllNodes(lpTreeView)
            If String.Equals(n.Name, szKey, StringComparison.OrdinalIgnoreCase) Then
                lpTreeView.SelectedNode = n
                If bolExpand Then n.Expand()
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub ExpandFirstNode(ByVal lpTreeView As TreeView)
        If lpTreeView.Nodes.Count > 0 Then lpTreeView.Nodes(0).Expand()
    End Sub

    '//將 TreeView 設在最近匯入的 Album
    Public Function LoadTreeViewImportRecentSection(ByVal lpStorage As Storage, ByVal lpTreeView As TreeView) As Boolean
        Dim bolFind As Boolean = False
        Dim szSection As String = GetSetting(AppName, "Import Recent", "Album", "")
        If szSection.Trim() <> "" Then
            Dim strParentFolder As String = g_lpFileSystem.AnalyseFile(fsParentFolderName, szSection)
            If SelectNodeByKey(lpTreeView, strParentFolder, True) Then
                bolFind = SelectNodeByKey(lpTreeView, szSection, True)
            End If
        End If
        If Not bolFind Then ExpandFirstNode(lpTreeView)
        Return bolFind
    End Function

    '//紀錄最近的 Class or Book By TreeView
    Public Sub SaveRecentKey(ByVal enumExe As enumExeMode, ByVal lpStorage As Storage, ByVal iSectionIndex As Integer, ByVal iKeyIndex As Integer)
        Select Case enumExe
            Case enumExeMode.exeAlbums : SaveSetting(AppName, "Recent", "Class", CType(lpStorage.Album(iSectionIndex).Item(iKeyIndex), PhotoSet).Name)
            Case enumExeMode.exeFavorites : SaveSetting(AppName, "Recent", "Book", CType(lpStorage.Favorite(iSectionIndex).Item(iKeyIndex), PhotoSet).Name)
        End Select
    End Sub

    Public Sub SaveRecentKeyValue(ByVal enumExe As enumExeMode, ByVal strKeyValue As String)
        Select Case enumExe
            Case enumExeMode.exeAlbums : SaveSetting(AppName, "Recent", "Class", strKeyValue)
            Case enumExeMode.exeFavorites : SaveSetting(AppName, "Recent", "Book", strKeyValue)
        End Select
    End Sub

    '//將 TreeView 設在最近的 Album, Favorite
    Public Function LoadTreeViewRecentSection(ByVal enumExe As enumExeMode, ByVal lpStorage As Storage, ByVal lpTreeView As TreeView) As Boolean
        Dim szSection As String
        Select Case enumExe
            Case enumExeMode.exeFavorites : szSection = GetSetting(AppName, "Recent", "Favorite", "")
            Case Else : szSection = GetSetting(AppName, "Recent", "Album", "")
        End Select
        Dim bolFind As Boolean = SelectNodeByKey(lpTreeView, szSection, True)
        If Not bolFind Then ExpandFirstNode(lpTreeView)
        Return bolFind
    End Function

    '//將 TreeView 設在最近的 Class, Book（外面再 Call tvList_Click）
    Public Function LoadTreeViewRecentKey(ByVal enumExe As enumExeMode, ByVal lpStorage As Storage, ByVal lpTreeView As TreeView) As Boolean
        If Not LoadTreeViewRecentSection(enumExe, lpStorage, lpTreeView) Then Return False
        Dim szKey As String = ""
        Select Case enumExe
            Case enumExeMode.exeAlbums : szKey = GetSetting(AppName, "Recent", "Class", "")
            Case enumExeMode.exeFavorites : szKey = GetSetting(AppName, "Recent", "Book", "")
        End Select
        Return SelectNodeByKey(lpTreeView, szKey, False)
    End Function

End Module
