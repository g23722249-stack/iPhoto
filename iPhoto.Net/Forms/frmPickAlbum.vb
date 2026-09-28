' Picks an album folder (移動到…, new in the .NET port): the 相片庫 tree -- each album root's folders
' and their albums -- as in the main window. Made in code (no designer file).
Friend Class frmPickAlbum
    Inherits Form

    Private ReadOnly lblPrompt As New Label
    Private ReadOnly tvAlbums As New TreeView
    Private ReadOnly butOk As New Aqua.FlashButton
    Private ReadOnly butCancel As New Aqua.FlashButton
    Private m_strPicked As String

    Public Sub New()
        Text = "移動到…"
        Font = New Font("Microsoft JhengHei UI", 11.0F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(420, 520)
        BackColor = Color.White

        lblPrompt.SetBounds(16, 12, 390, 24)
        tvAlbums.SetBounds(16, 44, 388, 410)
        tvAlbums.HideSelection = False
        butOk.SetBounds(70, 470, 140, 36) : butOk.Text = "移到這裡"
        butCancel.SetBounds(226, 470, 114, 36) : butCancel.Text = "放棄"
        For Each b In {butOk, butCancel}
            b.Font = New Font("華康細圓體", 13.0F)
        Next
        Controls.AddRange({lblPrompt, tvAlbums, butOk, butCancel})

        AddHandler tvAlbums.AfterSelect, Sub() butOk.Enabled = IsAlbum(tvAlbums.SelectedNode)
        AddHandler tvAlbums.NodeMouseDoubleClick, Sub(s, e)
                                                      If IsAlbum(e.Node) Then Accept()
                                                  End Sub
        AddHandler butOk.Click, Sub() Accept()
        AddHandler butCancel.Click, Sub() Close()
    End Sub

    ''' <summary>The album folder picked; Nothing when cancelled.</summary>
    Public Function Pick(ByVal prompt As String) As String
        lblPrompt.Text = prompt
        tvAlbums.BeginUpdate()
        For i = 0 To g_lpStorage.AlbumCount - 1
            Dim section As Albums = g_lpStorage.Album(i)
            section.Load()
            Dim n As TreeNode = tvAlbums.Nodes.Add(section.Name)
            For k = 0 To section.ItemCount - 1
                Dim c As [Class] = TryCast(section.Item(k), [Class])
                If c IsNot Nothing Then n.Nodes.Add(New TreeNode(c.DisplayName) With {.Tag = c.Path})
            Next
        Next
        tvAlbums.EndUpdate()
        butOk.Enabled = False
        ShowDialog()
        Return m_strPicked
    End Function

    Private Shared Function IsAlbum(ByVal n As TreeNode) As Boolean
        Return n IsNot Nothing AndAlso TypeOf n.Tag Is String
    End Function

    Private Sub Accept()
        If Not IsAlbum(tvAlbums.SelectedNode) Then Return
        m_strPicked = CStr(tvAlbums.SelectedNode.Tag)
        Close()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
