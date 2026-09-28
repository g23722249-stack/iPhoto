' The cover wall (new in the .NET port): a click on a first-level node of the tree (「2025年」, a
' photo-book folder) opens it and fills the list with one cover per album under it -- a few of its
' photos scattered like prints on a table (PhotoLib AlbumCovers; the cover is saved as
' <node folder>\<album name>.jpg). Double-click a cover to open the album; right-click to make it again.
'
' Covers that exist and are newer than their album show at once; the others show an empty pile and
' are made on a worker thread, one album after another, each put in place as soon as it is ready.
' Clicking elsewhere (ClearScreenAlbum) drops what is still to come (m_intCoverRun).
' While the wall is up the photo handlers of mlList step aside (ListShowsCards), as for the face wall.
Partial Class frmMain_1280x1024

    Private m_bolCoverWall As Boolean
    Private m_lpCoverNode As TreeNode                  ' the first-level node the wall shows
    Private ReadOnly m_lpCoverItems As New List(Of PhotoSet)   ' by mlList index
    Private m_intCoverRun As Integer                   ' bumped to drop a running fill
    Private WithEvents mnuCover As ContextMenuStrip
    Private m_intCoverMenuIndex As Integer

    ''' <summary>True while mlList shows cards (face wall / groups, cover wall), not photos.</summary>
    Private ReadOnly Property ListShowsCards As Boolean
        Get
            Return FaceViewActive OrElse m_bolCoverWall
        End Get
    End Property

    ''' <summary>Called by ClearScreenAlbum.</summary>
    Private Sub EndCoverWall()
        m_bolCoverWall = False
        m_lpCoverNode = Nothing
        m_lpCoverItems.Clear()
        Threading.Interlocked.Increment(m_intCoverRun)
    End Sub

    ''' <summary>Called by tvList_Click for a first-level node: open it and show its covers.</summary>
    Private Sub ShowCoverWall(ByVal node As TreeNode)
        If Not node.IsExpanded Then node.Expand()   ' BeforeExpand fills the albums (and clears the list)
        EndCoverWall()
        mlList.Clear()

        Dim section As Albums = SectionOf(node)
        If section Is Nothing Then Return
        section.Load()
        m_bolCoverWall = True
        m_lpCoverNode = node
        Dim run As Integer = m_intCoverRun

        Dim jobs As New List(Of (Index As Integer, Child As PhotoSet, Cover As String, Title As String, Current As Boolean))
        For i = 0 To section.ItemCount - 1
            Dim child As PhotoSet = TryCast(section.Item(i), PhotoSet)
            If child Is Nothing Then Continue For
            Dim coverFile As String = AlbumCovers.CoverFile(section.Path, child)
            Dim current As Boolean
            Dim existing As String = AlbumCovers.ExistingCover(coverFile, AlbumCovers.SourceOf(child), current)
            Dim title As String = child.DisplayName
            Dim card As String = AlbumCovers.Card(existing, title, If(existing = "", "製作封面中…", ""))
            Dim item As Aqua.MediaItem = mlList.AddItem(card, 0, False, False, "")
            item.ShowRating = False
            item.ToolTipTitle = title
            item.ToolTipText = "按兩下開啟相本" & vbCrLf & "按右鍵可以重新產生封面"
            m_lpCoverItems.Add(child)
            jobs.Add((m_lpCoverItems.Count - 1, child, coverFile, title, current))
        Next
        txtTitle.Text = node.Text
        lblPhotoCounts.Text = m_lpCoverItems.Count & " 本相本"
        m_strShownKey = NodeKey(node)
        If mlList.Count > 0 Then mlList.ScrollValue = mlList.ScrollMin
        FillCovers(run, jobs.Select(Function(j) (j.Index, j.Child.Key, j.Cover, j.Title, j.Current, False)).ToList())
    End Sub

    Private Function SectionOf(ByVal node As TreeNode) As Albums
        If node Is Nothing OrElse Not TypeOf node.Tag Is Integer Then Return Nothing
        Dim i As Integer = CInt(node.Tag)
        Select Case m_lpAppEnv.ExeMode
            Case enumExeMode.exeAlbums : Return If(i < g_lpStorage.AlbumCount, g_lpStorage.Album(i), Nothing)
            Case enumExeMode.exeFavorites : Return If(i < g_lpStorage.FavoriteCount, g_lpStorage.Favorite(i), Nothing)
        End Select
        Return Nothing
    End Function

    ''' <summary>On a worker thread, album by album: count the photos, make the cover when it is missing or
    ''' older than the album (or the job says Force), make the card, and put it in the list.</summary>
    Private Sub FillCovers(ByVal run As Integer, ByVal jobs As List(Of (Index As Integer, Source As String, Cover As String, Title As String, Current As Boolean, Force As Boolean)))
        Dim favorites As Boolean = m_lpAppEnv.ExeMode = enumExeMode.exeFavorites
        Dim readOnlyMode As Boolean = g_lpConfig.ReadOnly
        Threading.Tasks.Task.Run(
            Sub()
                For Each j In jobs
                    If m_intCoverRun <> run OrElse IsDisposed Then Return
                    Try
                        ' a set of its own: the tree's objects stay with the UI thread
                        Dim child As PhotoSet
                        If favorites Then
                            Dim b As New Book
                            b.Construct(j.Source)
                            child = b
                        Else
                            Dim c As New [Class]
                            c.Construct(j.Source)
                            child = c
                        End If
                        child.Load()
                        Dim cover As String
                        If j.Current AndAlso Not j.Force Then
                            Dim current As Boolean
                            cover = AlbumCovers.ExistingCover(j.Cover, j.Source, current)
                        Else
                            cover = AlbumCovers.MakeCover(child, j.Cover, readOnlyMode)
                        End If
                        Dim subtitle As String = AlbumCovers.CountText(child)
                        Dim card As String = AlbumCovers.Card(cover, j.Title, subtitle)
                        If m_intCoverRun <> run OrElse IsDisposed Then Return
                        BeginInvoke(Sub() PutCover(run, j.Index, card))
                    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                               TypeOf ex Is ArgumentException OrElse TypeOf ex Is Runtime.InteropServices.ExternalException
                        ' this album keeps its empty pile
                    Catch ex As InvalidOperationException When IsDisposed OrElse Not IsHandleCreated
                        Return   ' the window closed
                    End Try
                Next
            End Sub)
    End Sub

    Private Sub PutCover(ByVal run As Integer, ByVal index As Integer, ByVal card As String)
        If run <> m_intCoverRun OrElse Not m_bolCoverWall OrElse card = "" OrElse index >= mlList.Count Then Return
        mlList.Item(index).FileName = card
    End Sub

    '==================================================================================================
    ' Mouse
    '==================================================================================================
    Private Sub CoverWall_ItemDblClick(index As Integer) Handles mlList.ItemDblClick
        If Not m_bolCoverWall Then Return
        OpenCoverAlbum(index)
    End Sub

    Private Sub OpenCoverAlbum(ByVal index As Integer)
        Dim node As TreeNode = m_lpCoverNode
        If node Is Nothing OrElse index < 0 OrElse index >= node.Nodes.Count Then Return
        tvList.SelectedNode = node.Nodes(index)
        tvList_Click()
    End Sub

    Private Sub CoverWall_ItemMouseDown(index As Integer, e As MouseEventArgs) Handles mlList.ItemMouseDown
        If Not m_bolCoverWall OrElse e.Button <> MouseButtons.Right Then Return
        m_intCoverMenuIndex = index
        If mnuCover Is Nothing Then
            mnuCover = New ContextMenuStrip()
            mnuCover.Items.Add("開啟相本", Nothing, Sub() OpenCoverAlbum(m_intCoverMenuIndex))
            mnuCover.Items.Add("以檔案總管開啟", Nothing, Sub()
                                                         If m_intCoverMenuIndex >= 0 AndAlso m_intCoverMenuIndex < m_lpCoverItems.Count Then ShowInExplorer(m_lpCoverItems(m_intCoverMenuIndex).Key)
                                                     End Sub)
            mnuCover.Items.Add(New ToolStripSeparator())
            mnuCover.Items.Add("重新產生封面", Nothing, Sub() RemakeCovers(m_intCoverMenuIndex))
            mnuCover.Items.Add("重新產生這裡全部的封面", Nothing, Sub() RemakeCovers(-1))
        End If
        mlList.PopupMenu(index, mnuCover)
    End Sub

    ''' <summary>Makes the cover of album <paramref name="index"/> again (-1: every album on the wall). The
    ''' fill starts over for the whole wall (a running one would put old cards back); the other albums
    ''' just get their cards again, after the asked ones.</summary>
    Private Sub RemakeCovers(ByVal index As Integer)
        Dim section As Albums = SectionOf(m_lpCoverNode)
        If section Is Nothing Then Return
        Dim jobs As New List(Of (Index As Integer, Source As String, Cover As String, Title As String, Current As Boolean, Force As Boolean))
        For i = 0 To m_lpCoverItems.Count - 1
            Dim child As PhotoSet = m_lpCoverItems(i)
            Dim title As String = child.DisplayName
            Dim coverFile As String = AlbumCovers.CoverFile(section.Path, child)
            Dim force As Boolean = index < 0 OrElse i = index
            Dim current As Boolean
            AlbumCovers.ExistingCover(coverFile, child.Key, current)
            If force AndAlso i < mlList.Count Then mlList.Item(i).FileName = AlbumCovers.Card("", title, "製作封面中…")
            jobs.Add((i, child.Key, coverFile, title, current, force))
        Next
        Threading.Interlocked.Increment(m_intCoverRun)
        FillCovers(m_intCoverRun, jobs.OrderByDescending(Function(j) j.Force).ToList())
    End Sub

    Private Sub CoverWall_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Threading.Interlocked.Increment(m_intCoverRun)
    End Sub

End Class
