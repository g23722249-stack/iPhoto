' Port of iPhoto\Form\frmPhotoInfoBatch_1.frm: step 1 of the batch edit -- tick the albums (class
' folders) whose photos are to be edited; clicking one previews its photos. Ticking a root ticks all
' its albums. "下一步" opens step 2 (frmPhotoInfoBatch_2) with the ticked albums.
' Callers use "Using f As New frmPhotoInfoBatch_1".
' Fixed from VB6: opening a root with its "+" (instead of clicking its name) showed the placeholder
' "FAKE" node, and "下一步" with nothing ticked raised an error.
Friend Class frmPhotoInfoBatch_1

    Private m_iSectionIndex As Integer
    Private m_iKeyIndex As Integer
    Private m_lpExpNode As TreeNode
    Private m_lpSelNode As TreeNode
    Private ReadOnly m_enumExeMode As enumExeMode = enumExeMode.exeAlbums
    Private m_bolChecking As Boolean

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_lpExpNode = Nothing
        LibUserInterface.AddSectionFakeKeyToTreeView(m_enumExeMode, g_lpStorage, tvList)
        If tvList.Nodes.Count > 0 Then tvList.SelectedNode = tvList.Nodes(0)
    End Sub

    Private Sub Form_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        tvList.Focus()
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butNext_Click(sender As Object, e As EventArgs) Handles butNext.Click
        Dim strAlbum As New List(Of String)
        For Each n As TreeNode In LibUserInterface.AllNodes(tvList)
            If n.Parent IsNot Nothing AndAlso n.Checked AndAlso Not n.Name.StartsWith("FAKE_") Then strAlbum.Add(n.Name)
        Next
        If strAlbum.Count = 0 Then Return
        Using f As New frmPhotoInfoBatch_2
            f.ShowPhotos(strAlbum.ToArray(), strAlbum.Count)
        End Using
    End Sub

    Private Sub tvList_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvList.NodeMouseClick
        If e.Button <> MouseButtons.Left Then Return
        Dim hit As TreeViewHitTestLocations = tvList.HitTest(e.Location).Location
        If (hit And (TreeViewHitTestLocations.PlusMinus Or TreeViewHitTestLocations.StateImage)) <> 0 Then Return
        tvList.SelectedNode = e.Node
        NodeClick(e.Node)
    End Sub

    Private Sub NodeClick(ByVal node As TreeNode)
        If m_lpSelNode Is node Then Return
        If node.Parent Is Nothing Then
            ' 最頂層
            m_lpSelNode = Nothing
            ExpandNodes(node)
            node.Expand()
            Return
        End If
        m_iSectionIndex = CInt(node.Parent.Tag)
        m_iKeyIndex = CInt(node.Tag)
        m_lpSelNode = node

        ' 搬相片至畫面上
        Cursor = Cursors.WaitCursor
        Try
            Dim lpKey As PhotoSet = g_lpStorage.Album(m_iSectionIndex).Item(m_iKeyIndex)
            lpKey.Load()
            mlList.Clear()
            AddPhotoToMediaList(enumExeMode.exeAlbums, g_lpStorage, mlList, mlList.Limit, m_iSectionIndex, m_iKeyIndex)
            If mlList.Count > 0 Then mlList.ScrollValue = mlList.ScrollMin
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>Only one root stays open; its albums are read on first open.</summary>
    Private Sub ExpandNodes(ByVal node As TreeNode)
        If m_lpExpNode IsNot Nothing AndAlso m_lpExpNode IsNot node Then m_lpExpNode.Collapse()
        m_lpExpNode = node
        LibUserInterface.ExpandFakeSectionKeyToTreeView(m_enumExeMode, g_lpStorage, tvList, node, CInt(node.Tag))
        m_iSectionIndex = CInt(node.Tag)
        m_iKeyIndex = -1
    End Sub

    Private Sub tvList_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvList.BeforeExpand
        If e.Node.Parent Is Nothing Then ExpandNodes(e.Node)
    End Sub

    Private Sub tvList_GotFocus(sender As Object, e As EventArgs) Handles tvList.GotFocus
        frmClass.BorderColor = frmClass.BorderFocusColor
    End Sub

    Private Sub tvList_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvList.AfterCheck
        If m_bolChecking Then Return
        Dim node As TreeNode = e.Node
        If node.Parent Is Nothing Then
            ' 最頂層: open it and tick / untick all its albums
            ExpandNodes(node)
            node.Expand()
            tvList.SelectedNode = node
            m_bolChecking = True
            Try
                For Each child As TreeNode In node.Nodes
                    child.Checked = node.Checked
                Next
            Finally
                m_bolChecking = False
            End Try
        End If
        butNext.Enabled = LibUserInterface.AllNodes(tvList).Any(Function(n) n.Parent IsNot Nothing AndAlso n.Checked)
    End Sub

End Class
