' Port of iPhoto\Form\frmAddition.frm: creates a new album folder (相片庫) or photo-book folder (攝影集)
' under one of the storage paths from the settings. The radio buttons switch between the two; the tree
' lists the storage paths, and the open one is where the folder is created.
' Callers use "Using f As New frmAddition" and read Changed afterwards.
Friend Class frmAddition

    Private m_bolChanged As Boolean
    Private m_lpExpNode As TreeNode
    Private m_bolTextChanged As Boolean
    Private m_enumExeMode As enumExeMode
    Private m_bolLoaded As Boolean

    ''' <summary>True when a folder was created (the caller then refreshes its tree).</summary>
    Public ReadOnly Property Changed As Boolean
        Get
            Return m_bolChanged
        End Get
    End Property

    Public Sub ShowAdditionAlbum()
        m_enumExeMode = enumExeMode.exeAlbums
        ShowDialog()
    End Sub

    Public Sub ShowAdditionFavorite()
        m_enumExeMode = enumExeMode.exeFavorites
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If m_enumExeMode = enumExeMode.exeFavorites Then
            rbMode_1.Checked = True
        Else
            rbMode_0.Checked = True
        End If
        ApplyMode()
        txtPath.Text = "新資料夾"
        m_bolTextChanged = False
        m_bolLoaded = True
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        txtPath.Text = txtPath.Text.Trim()
        If txtPath.Text = "" OrElse Not CheckNameRule(txtPath.Text) Then
            frmMsgBox.ShowCriticalMessage("新相簿名稱含有不合法的字元", "")
            txtPath.Focus()
            Return
        End If
        If m_lpExpNode Is Nothing Then
            frmMsgBox.ShowCriticalMessage("請先選擇要建立的位置", "")
            Return
        End If

        Dim strPath As String = m_lpExpNode.Name & "\" & txtPath.Text
        If g_lpFileSystem.FolderExists(strPath) Then
            frmMsgBox.ShowCriticalMessage("資料夾已存在", "")
            Return
        End If
        If Not g_lpFileSystem.CreateFolder(strPath) Then
            frmMsgBox.ShowCriticalMessage("建立資料夾失敗", "")
            Return
        End If

        m_bolChanged = True
        Close()
    End Sub

    Private Sub rbMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbMode_0.CheckedChanged, rbMode_1.CheckedChanged
        If Not m_bolLoaded OrElse Not CType(sender, Aqua.RadioButton).Checked Then Return
        ApplyMode()
    End Sub

    Private Sub ApplyMode()
        Dim mode As enumExeMode = If(rbMode_1.Checked, enumExeMode.exeFavorites, enumExeMode.exeAlbums)
        Text = If(mode = enumExeMode.exeFavorites, "新增攝影集", "新增相片庫")
        m_lpExpNode = Nothing
        LibUserInterface.AddStorageSectionPathFakeKeyToTreeView(mode, g_lpConfig, tvList)
        m_bolChanged = False
        If tvList.Nodes.Count > 0 Then
            tvList.Nodes(0).Expand()
            tvList.SelectedNode = tvList.Nodes(0)
        End If
    End Sub

    Private Sub tvList_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvList.NodeMouseClick
        If e.Button <> MouseButtons.Left Then Return
        If (tvList.HitTest(e.Location).Location And TreeViewHitTestLocations.PlusMinus) <> 0 Then Return
        tvList.SelectedNode = e.Node
        If e.Node.Parent IsNot Nothing AndAlso Not m_bolTextChanged Then txtPath.Text = e.Node.Text
    End Sub

    Private Sub tvList_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvList.BeforeExpand
        If m_lpExpNode IsNot Nothing AndAlso m_lpExpNode IsNot e.Node Then m_lpExpNode.Collapse()
        m_lpExpNode = e.Node
        LibUserInterface.ExpandFakeSectionKeyPathToTreeView(
            If(rbMode_1.Checked, enumExeMode.exeFavorites, enumExeMode.exeAlbums), tvList, e.Node)
    End Sub

    Private Sub tvList_GotFocus(sender As Object, e As EventArgs) Handles tvList.GotFocus
        frmClass.BorderColor = txtPath.BorderFocusColor
    End Sub

    Private Sub tvList_LostFocus(sender As Object, e As EventArgs) Handles tvList.LostFocus
        frmClass.BorderColor = txtPath.BorderColor
    End Sub

    Private Sub txtPath_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPath.KeyDown
        m_bolTextChanged = True
    End Sub

End Class
