' Face recognition in the main window (new in the .NET port). Made in code so the designer file stays
' as ported.
'   P1 -- once the window is up, g_lpFaces analyses the whole photo library in the background; the
'         progress shows next to 「已選取 N 張照片」 and a click on it pauses / resumes. The scan stops
'         when the window closes.
'   P2 -- the 面孔 node at the end of the tree (both modes), filled from g_lpFaces.Catalog after each
'         sort: one child per person and 「未命名的臉」. In the thumbnail list (mlList):
'           面孔           the face wall: a card per person, then a card per unnamed group
'           a person       that person's photos, as a normal photo list (viewer, rating, dock work)
'           a group card   the group's faces, all ticked: untick the ones that aren't the person,
'                          then right-click 「將勾選的臉命名為…」
'         Right-click a person card: 改名／合併…, 設定出生年…, 更換封面… (frmCover), 隱藏.
'         While the wall or a group is shown, the photo handlers of mlList step aside (FaceViewActive).
Partial Class frmMain_1280x1024

    Private WithEvents m_lpFaceScan As FaceLibrary
    Private WithEvents lblFaceScan As Label
    Private WithEvents lblFaceGuide As Label

    ''' <summary>Called at the end of Form_Load.</summary>
    Private Sub StartFaceScan()
        If g_lpFaces Is Nothing Then Return
        lblFaceScan = New Label With {
            .AutoSize = True, .BackColor = Color.Transparent, .ForeColor = lblSelCount.ForeColor, .Font = lblSelCount.Font,
            .Anchor = lblSelCount.Anchor, .Location = New Point(lblSelCount.Right + 24, lblSelCount.Top),
            .Cursor = Cursors.Hand, .Text = "準備分析面孔…"}
        vb6ToolTip.SetToolTip(lblFaceScan, "點一下暫停 / 繼續")
        Controls.Add(lblFaceScan)
        lblFaceScan.BringToFront()

        ' 「？ 面孔使用說明」 above the face wall, left of the quick search; shown with the 面孔 views
        lblFaceGuide = New Label With {
            .AutoSize = True, .BackColor = Color.Transparent, .ForeColor = Color.FromArgb(42, 116, 208),
            .Font = New Font(lblSelCount.Font, FontStyle.Underline), .Cursor = Cursors.Hand, .Visible = False,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Text = "？ 面孔使用說明"}
        Controls.Add(lblFaceGuide)
        lblFaceGuide.Location = New Point(iTextBox1.Left - lblFaceGuide.PreferredWidth - 20, iTextBox1.Top + (iTextBox1.Height - lblFaceGuide.PreferredHeight) \ 2)
        lblFaceGuide.BringToFront()

        m_lpFaceScan = g_lpFaces
        If g_lpConfig.FaceAutoScan Then
            m_lpFaceScan.Start(AlbumRoots())
        Else
            ' 設定 › 面孔: no automatic scan -- sort what was analysed before, and let the label start a scan
            lblFaceScan.Text = "面孔：按這裡分析新照片"
            vb6ToolTip.SetToolTip(lblFaceScan, "分析相片庫裡新增或修改過的照片")
            m_lpFaceScan.RequestOrganize()
        End If
    End Sub

    Private Function AlbumRoots() As List(Of String)
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Return roots
    End Function

    ''' <summary>Called when an import has finished: the new photos are analysed in the background.</summary>
    Private Sub FaceScanAfterImport()
        If m_lpFaceScan Is Nothing Then Return
        m_lpFaceScan.RequestScan(AlbumRoots())
        If lblFaceScan IsNot Nothing AndAlso Not lblFaceScan.Text.StartsWith("分析面孔") Then lblFaceScan.Text = "準備分析新匯入的照片…"
    End Sub

    ''' <summary>Called when 設定 was saved: what can change while iPhoto runs is applied now (turning face
    ''' recognition on / off takes effect at the next start).</summary>
    Private Sub ApplyFaceSettings()
        If g_lpFaces Is Nothing Then Return
        g_lpFaces.WriteNames = g_lpConfig.FaceWriteNames
        If g_lpFaces.Strictness <> g_lpConfig.FaceStrictness Then
            g_lpFaces.Strictness = g_lpConfig.FaceStrictness
            g_lpFaces.RequestOrganize()
        End If
    End Sub

    Private Sub m_lpFaceScan_Progress(intDone As Integer, intTotal As Integer) Handles m_lpFaceScan.Progress
        If lblFaceScan Is Nothing Then Return
        If intDone >= intTotal Then
            lblFaceScan.Text = "面孔：" & intTotal.ToString("#,0") & " 張照片已分析"
        Else
            lblFaceScan.Text = "分析面孔 " & intDone.ToString("#,0") & " / " & intTotal.ToString("#,0") & If(m_lpFaceScan.IsPaused, "（已暫停）", "")
        End If
    End Sub

    Private Sub m_lpFaceScan_Finished(intAnalysed As Integer, strError As String) Handles m_lpFaceScan.Finished
        If lblFaceScan Is Nothing Then Return
        If strError IsNot Nothing Then
            lblFaceScan.Text = "面孔分析失敗：" & strError
        ElseIf Not lblFaceScan.Text.StartsWith("面孔：") Then
            lblFaceScan.Text = "面孔分析已停止"
        End If
        If m_lpFaceScan IsNot Nothing AndAlso m_lpFaceScan.FailedCount > 0 Then
            lblFaceScan.Text &= "（" & m_lpFaceScan.FailedCount & " 張無法分析）"
            vb6ToolTip.SetToolTip(lblFaceScan, m_lpFaceScan.LastFailure)
        End If
    End Sub

    Private Sub lblFaceScan_Click(sender As Object, e As EventArgs) Handles lblFaceScan.Click
        If m_lpFaceScan Is Nothing Then Return
        If Not m_lpFaceScan.IsRunning Then
            ' idle: a click analyses what is new (the only way when 設定 turned the automatic scan off)
            m_lpFaceScan.Start(AlbumRoots())
            lblFaceScan.Text = "準備分析面孔…"
            vb6ToolTip.SetToolTip(lblFaceScan, "點一下暫停 / 繼續")
            Return
        End If
        If m_lpFaceScan.IsPaused Then
            m_lpFaceScan.Resume()
            lblFaceScan.Text = lblFaceScan.Text.Replace("（已暫停）", "")
        Else
            m_lpFaceScan.Pause()
            If Not lblFaceScan.Text.EndsWith("（已暫停）") Then lblFaceScan.Text &= "（已暫停）"
        End If
    End Sub

    Private Sub FaceScan_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' while the window (and so the thread's way back to it) is still there
        m_lpFaceScan?.Stop()
        m_lpFaceScan = Nothing
    End Sub

    '==================================================================================================
    ' P2: the 面孔 tree node
    '==================================================================================================
    Private Const FaceNodeKey As String = "FACE"
    Private Const FaceGroupsKey As String = "FACE:GROUPS"
    Private Const FacePersonPrefix As String = "FACE:P"
    ''' <summary>Unnamed groups shown on the wall (the biggest; the rest wait until these are named).</summary>
    Private Const MaxGroupsShown As Integer = 60

    ''' <summary>Called after the tree is (re)built: adds 面孔 as the last node.</summary>
    Private Sub AddFaceNode()
        If g_lpFaces Is Nothing Then Return
        Dim n As TreeNode = tvList.Nodes.Add(FaceNodeKey, "面孔")
        n.ImageKey = "icoFace"
        n.SelectedImageKey = "icoFace"
        n.Tag = FaceNodeKey
        FillFaceNode(n)
    End Sub

    Private Sub FillFaceNode(ByVal n As TreeNode)
        Dim selectedKey As String = If(tvList.SelectedNode IsNot Nothing AndAlso tvList.SelectedNode.Parent Is n, tvList.SelectedNode.Name, Nothing)
        tvList.BeginUpdate()
        n.Nodes.Clear()
        Dim cat As FaceCatalog = g_lpFaces.Catalog
        If cat Is Nothing Then
            n.Text = "面孔"
            AddFaceChild(n, "FACE:WAIT", "（整理中…）")
        Else
            n.Text = If(cat.Clusters.Count > 0, "面孔 (" & cat.Clusters.Count & ")", "面孔")
            For Each p In cat.VisiblePersons
                AddFaceChild(n, FacePersonPrefix & p.PersonID, p.Name & " (" & p.PhotoCount & ")")
            Next
            If cat.Clusters.Count > 0 Then AddFaceChild(n, FaceGroupsKey, "未命名的臉 (" & cat.Clusters.Count & " 群)")
        End If
        tvList.EndUpdate()
        If selectedKey IsNot Nothing Then
            Dim again() As TreeNode = n.Nodes.Find(selectedKey, False)
            If again.Length > 0 Then tvList.SelectedNode = again(0)
        End If
    End Sub

    Private Shared Sub AddFaceChild(ByVal parent As TreeNode, ByVal key As String, ByVal text As String)
        Dim c As TreeNode = parent.Nodes.Add(key, text)
        c.ImageKey = "icoFace"
        c.SelectedImageKey = "icoFace"
        c.Tag = key
    End Sub

    Private Sub m_lpFaceScan_Organized(sender As Object, e As EventArgs) Handles m_lpFaceScan.Organized
        Dim n() As TreeNode = tvList.Nodes.Find(FaceNodeKey, False)
        If n.Length > 0 Then FillFaceNode(n(0))
        If m_enumFaceView = FaceView.fvWall Then ShowFaceWall(m_bolWallGroupsOnly)   ' group / confirm views are left alone: the user is ticking faces
        Dim cat As FaceCatalog = g_lpFaces?.Catalog
        If lblFaceScan IsNot Nothing AndAlso cat IsNot Nothing AndAlso Not lblFaceScan.Text.StartsWith("分析面孔") Then
            lblFaceScan.Text = "面孔：" & cat.VisiblePersons.Count & " 人 · " & cat.Clusters.Count & " 群未命名"
            vb6ToolTip.SetToolTip(lblFaceScan, g_lpFaces.LastOrganizeInfo)
        End If
    End Sub

    ''' <summary>Called first by tvList_Click: True when the node is 面孔 or one of its children.</summary>
    Private Function FaceNodeClick(ByVal node As TreeNode) As Boolean
        Dim key As String = TryCast(node.Tag, String)
        ShowFaceGuideLink(node)
        If key Is Nothing OrElse Not key.StartsWith(FaceNodeKey) Then Return False
        If g_lpFaces Is Nothing OrElse g_lpFaces.Catalog Is Nothing Then Return True
        Select Case True
            Case key = FaceNodeKey : ShowFaceWall(False)
            Case key = FaceGroupsKey : ShowFaceWall(True)
            Case key.StartsWith(FacePersonPrefix)
                Dim p As FaceCatalog.PersonEntry = g_lpFaces.Catalog.Person(CInt(Val(key.Substring(FacePersonPrefix.Length))))
                If p IsNot Nothing Then ShowPersonPhotos(p)
        End Select
        Return True
    End Function

    '==================================================================================================
    ' The guide (frmFaceGuide): the link shows while a 面孔 node is selected
    '==================================================================================================
    Private Sub ShowFaceGuideLink(ByVal node As TreeNode)
        If lblFaceGuide Is Nothing Then Return
        Dim key As String = If(node Is Nothing, Nothing, TryCast(node.Tag, String))
        lblFaceGuide.Visible = key IsNot Nothing AndAlso key.StartsWith(FaceNodeKey)
    End Sub

    Private Sub FaceGuide_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvList.AfterSelect
        ShowFaceGuideLink(e.Node)
    End Sub

    ''' <summary>Opens the guide at what the list shows now; it stays open beside the main window.</summary>
    Private Sub lblFaceGuide_Click(sender As Object, e As EventArgs) Handles lblFaceGuide.Click
        Dim topic As String
        Select Case m_enumFaceView
            Case FaceView.fvGroup : topic = "group"
            Case FaceView.fvConfirm : topic = "confirm"
            Case Else : topic = If(g_lpFaces?.Catalog Is Nothing OrElse g_lpFaces.Catalog.Persons.Count = 0, "intro", "wall")
        End Select
        frmFaceGuide.ShowGuide(Me, topic)
    End Sub

    '==================================================================================================
    ' P2: the face wall in mlList
    '==================================================================================================
    Private Enum FaceView
        fvNone = 0
        fvWall = 1     ' person / group cards
        fvGroup = 2    ' the faces of one unnamed group, with check boxes
        fvConfirm = 3  ' a person's faces to confirm (P3), with check boxes
    End Enum

    Private m_enumFaceView As FaceView = FaceView.fvNone
    Private m_bolWallGroupsOnly As Boolean
    Private ReadOnly m_lpWallItems As New List(Of Object)    ' PersonEntry or List(Of FaceRegion), by mlList index
    Private m_lpGroup As List(Of FaceRegion)
    Private m_lpGroupFaces As New List(Of FaceRegion)          ' by mlList index in the group view
    Private WithEvents mnuFacePerson As ContextMenuStrip
    Private WithEvents mnuFaceGroupCard As ContextMenuStrip
    Private WithEvents mnuFaceGroup As ContextMenuStrip
    Private WithEvents mnuFaceConfirm As ContextMenuStrip
    Private m_lpConfirmPerson As FaceCatalog.PersonEntry
    Private m_intFaceMenuIndex As Integer

    ''' <summary>True while mlList shows face cards or group faces (not photos).</summary>
    Private ReadOnly Property FaceViewActive As Boolean
        Get
            Return m_enumFaceView <> FaceView.fvNone
        End Get
    End Property

    ''' <summary>Called by ClearScreenAlbum: whatever comes next is a photo list.</summary>
    Private Sub EndFaceView()
        m_enumFaceView = FaceView.fvNone
        m_lpWallItems.Clear()
        m_lpGroupFaces.Clear()
        m_lpGroup = Nothing
        m_lpConfirmPerson = Nothing
    End Sub

    Private Sub BeginFaceList(ByVal view As FaceView)
        ClearScreenAlbum()
        m_enumFaceView = view
        With m_lpAppEnv
            .ExeMode = enumExeMode.exeFace
            .SectionIndex = -1
            .KeyIndex = -1
        End With
        mlList.Clear()
    End Sub

    Private Function AddFaceItem(ByVal file As String, ByVal tip As String, ByVal withCheckBox As Boolean) As Aqua.MediaItem
        Dim item As Aqua.MediaItem = mlList.AddItem(file, 0, withCheckBox, withCheckBox, "")
        item.ShowRating = False
        item.ToolTipTitle = ""
        item.ToolTipText = tip
        Return item
    End Function

    Private Sub ShowFaceWall(ByVal bolGroupsOnly As Boolean)
        Dim cat As FaceCatalog = g_lpFaces.Catalog
        If cat Is Nothing Then Return
        SetBusy(True)
        Try
            BeginFaceList(FaceView.fvWall)
            m_bolWallGroupsOnly = bolGroupsOnly
            Dim cache As String = g_lpFaces.CacheFolder
            If Not bolGroupsOnly Then
                For Each p In cat.VisiblePersons
                    Dim card As String = g_lpDatabase.LoadFaceCover(p.Name)   ' a name card made with frmCover wins
                    If card = "" OrElse Not IO.File.Exists(card) Then card = FaceCards.PersonCard(cache, p)
                    If card = "" Then Continue For
                    Dim pending As Integer = p.ToConfirm.Count
                    AddFaceItem(card, p.Name & "：" & p.PhotoCount & " 張照片" & If(pending > 0, "，" & pending & " 張待確認", "") & vbCrLf &
                                      "按兩下看照片，按右鍵確認更多照片 / 改名 / 合併", False)
                    m_lpWallItems.Add(p)
                Next
            End If
            For Each g In cat.Clusters.Take(MaxGroupsShown)
                Dim card As String = FaceCards.GroupCard(cache, g)
                If card = "" Then Continue For
                AddFaceItem(card, g.Count & " 張可能是同一人的臉" & vbCrLf & "按兩下逐張確認後命名", False)
                m_lpWallItems.Add(g)
            Next
            txtTitle.Text = If(bolGroupsOnly, "未命名的臉", "面孔")
            lblPhotoCounts.Text = cat.VisiblePersons.Count & " 人 · " & cat.Clusters.Count & " 群未命名"
            If mlList.Count > 0 Then mlList.ScrollValue = mlList.ScrollMin
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub ShowPersonPhotos(ByVal p As FaceCatalog.PersonEntry)
        SetBusy(True)
        Try
            With m_lpAppEnv
                .ExeMode = enumExeMode.exeFace
                .SectionIndex = -1
                .KeyIndex = -1
            End With
            Dim files() As String = g_lpFaces.PhotosOf(p)
            ClearScreenAlbum()
            If files.Length > 0 Then MoveFilesToMediaList(files, files.Length)
            txtTitle.Text = p.Name
            Dim pending As Integer = p.ToConfirm.Count
            txtRemark.Text = files.Length & " 張照片" & If(p.BirthYear > 0, "，" & p.BirthYear & " 年出生", "") &
                             If(pending > 0, "；" & pending & " 張臉待確認（面孔牆右鍵「確認更多照片」）", "")
        Finally
            SetBusy(False)
        End Try
        If mlList.Visible Then mlList.Focus()
    End Sub

    Private Sub ShowGroupFaces(ByVal g As List(Of FaceRegion))
        SetBusy(True)
        Try
            BeginFaceList(FaceView.fvGroup)
            m_lpGroup = g
            Dim cache As String = g_lpFaces.CacheFolder
            For Each f In g
                Dim crop As String = FaceCards.FaceCrop(cache, f)
                If crop = "" Then Continue For
                AddFaceItem(crop, IO.Path.GetFileName(f.FileName) & If(f.ShotYear > 0, "（" & f.ShotYear & "）", ""), True)
                m_lpGroupFaces.Add(f)
            Next
            txtTitle.Text = "這是誰？"
            txtRemark.Text = "不是同一個人的臉請取消勾選，再按右鍵「將勾選的臉命名為…」"
            lblPhotoCounts.Text = m_lpGroupFaces.Count & " 張臉"
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub SelectFaceTreeNode(ByVal key As String)
        Dim hits() As TreeNode = tvList.Nodes.Find(key, True)
        If hits.Length > 0 Then tvList.SelectedNode = hits(0)
    End Sub

    Private Sub FaceWall_ItemDblClick(index As Integer) Handles mlList.ItemDblClick
        If m_enumFaceView <> FaceView.fvWall OrElse index < 0 OrElse index >= m_lpWallItems.Count Then Return
        Dim p As FaceCatalog.PersonEntry = TryCast(m_lpWallItems(index), FaceCatalog.PersonEntry)
        If p IsNot Nothing Then
            SelectFaceTreeNode(FacePersonPrefix & p.PersonID)
            ShowPersonPhotos(p)
        Else
            ShowGroupFaces(CType(m_lpWallItems(index), List(Of FaceRegion)))
        End If
    End Sub

    Private Sub FaceWall_ItemMouseDown(index As Integer, e As MouseEventArgs) Handles mlList.ItemMouseDown
        If e.Button <> MouseButtons.Right OrElse Not FaceViewActive Then Return
        m_intFaceMenuIndex = index
        EnsureFaceMenus()
        EnsureConfirmMenu()
        If m_enumFaceView = FaceView.fvGroup Then
            mlList.PopupMenu(index, mnuFaceGroup)
        ElseIf m_enumFaceView = FaceView.fvConfirm Then
            mlList.PopupMenu(index, mnuFaceConfirm)
        ElseIf index >= 0 AndAlso index < m_lpWallItems.Count Then
            Dim p As FaceCatalog.PersonEntry = TryCast(m_lpWallItems(index), FaceCatalog.PersonEntry)
            If p IsNot Nothing Then
                Dim pending As Integer = p.ToConfirm.Count
                mnuFacePerson.Items(1).Text = "確認更多照片" & If(pending > 0, "（" & pending & "）…", "")
                mnuFacePerson.Items(1).Enabled = pending > 0
            End If
            mlList.PopupMenu(index, If(p IsNot Nothing, mnuFacePerson, mnuFaceGroupCard))
        End If
    End Sub

    Private Sub EnsureFaceMenus()
        If mnuFacePerson IsNot Nothing Then Return
        mnuFacePerson = New ContextMenuStrip()
        mnuFacePerson.Items.Add("看照片", Nothing, Sub() FaceWall_ItemDblClick(m_intFaceMenuIndex))
        mnuFacePerson.Items.Add("確認更多照片…", Nothing, Sub() ShowConfirmFaces(WallPerson))
        mnuFacePerson.Items.Add("改名／合併…", Nothing, Sub() RenameWallPerson())
        mnuFacePerson.Items.Add("設定出生年…", Nothing, Sub() SetWallBirthYear())
        mnuFacePerson.Items.Add("更換封面…", Nothing, Sub() ChangeWallCover())
        mnuFacePerson.Items.Add(New ToolStripSeparator())
        mnuFacePerson.Items.Add("隱藏", Nothing, Sub() HideWallPerson())

        mnuFaceGroupCard = New ContextMenuStrip()
        mnuFaceGroupCard.Items.Add("逐張確認…", Nothing, Sub() FaceWall_ItemDblClick(m_intFaceMenuIndex))
        mnuFaceGroupCard.Items.Add("全部命名為…", Nothing, Sub() NameGroup(CType(m_lpWallItems(m_intFaceMenuIndex), List(Of FaceRegion))))

        mnuFaceGroup = New ContextMenuStrip()
        mnuFaceGroup.Items.Add("將勾選的臉命名為…", Nothing, Sub() NameCheckedFaces())
        mnuFaceGroup.Items.Add("全部勾選", Nothing, Sub() mlList.CheckAll(True))
        mnuFaceGroup.Items.Add("全部取消勾選", Nothing, Sub() mlList.CheckAll(False))
        mnuFaceGroup.Items.Add(New ToolStripSeparator())
        mnuFaceGroup.Items.Add("回到面孔牆", Nothing, Sub()
                                                     SelectFaceTreeNode(FaceNodeKey)
                                                     ShowFaceWall(False)
                                                 End Sub)
    End Sub

    '==================================================================================================
    ' P3: 確認更多照片 -- a person's faces the program matched or suggested, a batch at a time, all ticked:
    ' untick the ones that aren't the person, then 「勾選的是 X、其餘不是」 (or confirm only the ticked
    ' ones and leave the rest for later). Double-click a face to tick / untick it.
    '==================================================================================================
    Private Const ConfirmBatch As Integer = 40

    Private Sub ShowConfirmFaces(ByVal p As FaceCatalog.PersonEntry)
        If p Is Nothing Then Return
        Dim todo As List(Of FaceRegion) = p.ToConfirm
        If todo.Count = 0 Then
            frmMsgBox.ShowSmileMessage("「" & p.Name & "」沒有待確認的臉了", "確認更多照片")
            SelectFaceTreeNode(FaceNodeKey)
            ShowFaceWall(False)
            Return
        End If
        SetBusy(True)
        Try
            BeginFaceList(FaceView.fvConfirm)
            m_lpConfirmPerson = p
            Dim cache As String = g_lpFaces.CacheFolder
            For Each f In todo.Take(ConfirmBatch)
                Dim crop As String = FaceCards.FaceCrop(cache, f)
                If crop = "" Then Continue For
                Dim how As String = If(f.State = FaceRegion.enumFaceState.fsAuto, "程式認出", "可能是") & " " & p.Name & $"（相似度 {f.Similarity:0.00}）"
                AddFaceItem(crop, how & vbCrLf & IO.Path.GetFileName(f.FileName) & If(f.ShotYear > 0, "，" & f.ShotYear & " 年", ""), True)
                m_lpGroupFaces.Add(f)
            Next
            txtTitle.Text = p.Name & " — 確認更多照片"
            txtRemark.Text = "不是「" & p.Name & "」的臉請取消勾選（或按兩下），再按右鍵「勾選的是 " & p.Name & "、其餘不是」"
            lblPhotoCounts.Text = "這一批 " & m_lpGroupFaces.Count & " 張 · 共 " & todo.Count & " 張待確認"
            If mlList.Count > 0 Then mlList.ScrollValue = mlList.ScrollMin
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub ConfirmView_ItemDblClick(index As Integer) Handles mlList.ItemDblClick
        If m_enumFaceView <> FaceView.fvConfirm AndAlso m_enumFaceView <> FaceView.fvGroup Then Return
        If index < 0 OrElse index >= mlList.Count Then Return
        mlList.Item(index).Checked = Not mlList.Item(index).Checked
    End Sub

    Private Sub EnsureConfirmMenu()
        If mnuFaceConfirm IsNot Nothing Then Return
        mnuFaceConfirm = New ContextMenuStrip()
        mnuFaceConfirm.Items.Add("勾選的是這個人、其餘不是", Nothing, Sub() ApplyConfirm(True))
        mnuFaceConfirm.Items.Add("只確認勾選的（其餘之後再說）", Nothing, Sub() ApplyConfirm(False))
        mnuFaceConfirm.Items.Add(New ToolStripSeparator())
        mnuFaceConfirm.Items.Add("全部勾選", Nothing, Sub() mlList.CheckAll(True))
        mnuFaceConfirm.Items.Add("全部取消勾選", Nothing, Sub() mlList.CheckAll(False))
        mnuFaceConfirm.Items.Add(New ToolStripSeparator())
        mnuFaceConfirm.Items.Add("回到面孔牆", Nothing, Sub()
                                                       SelectFaceTreeNode(FaceNodeKey)
                                                       ShowFaceWall(False)
                                                   End Sub)
    End Sub

    Private Sub mnuFaceConfirm_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles mnuFaceConfirm.Opening
        If m_lpConfirmPerson Is Nothing Then Return
        mnuFaceConfirm.Items(0).Text = "勾選的是「" & m_lpConfirmPerson.Name & "」、其餘不是"
    End Sub

    ''' <summary>Ticked faces are the person (confirmed, name into the people field); with
    ''' <paramref name="bolRejectRest"/> the unticked ones are "not this person" (never suggested
    ''' again). Then the next batch.</summary>
    Private Sub ApplyConfirm(ByVal bolRejectRest As Boolean)
        Dim p As FaceCatalog.PersonEntry = m_lpConfirmPerson
        If p Is Nothing Then Return
        Dim yes As New List(Of FaceRegion), no As New List(Of FaceRegion)
        For i = 0 To Math.Min(mlList.Count, m_lpGroupFaces.Count) - 1
            If mlList.Item(i).Checked Then yes.Add(m_lpGroupFaces(i)) Else no.Add(m_lpGroupFaces(i))
        Next
        If bolRejectRest AndAlso no.Count > 0 AndAlso yes.Count = 0 AndAlso
           Not frmQueryMsgBox.ShowMessage("這一批全部都不是「" & p.Name & "」嗎？", "確認") Then Return
        SetBusy(True)
        Try
            g_lpFaces.ConfirmFaces(yes)
            If bolRejectRest Then g_lpFaces.RejectFaces(no)
        Finally
            SetBusy(False)
        End Try
        g_lpFaces.RequestOrganizeSoon()
        If Not bolRejectRest AndAlso yes.Count = 0 Then Return   ' nothing done: stay on this batch
        ShowConfirmFaces(p)   ' the next batch (confirmed / rejected faces have dropped out)
    End Sub

    Private ReadOnly Property WallPerson As FaceCatalog.PersonEntry
        Get
            If m_intFaceMenuIndex < 0 OrElse m_intFaceMenuIndex >= m_lpWallItems.Count Then Return Nothing
            Return TryCast(m_lpWallItems(m_intFaceMenuIndex), FaceCatalog.PersonEntry)
        End Get
    End Property

    ''' <summary>frmInputString with a title; "" when cancelled (it returns the default then).</summary>
    Private Shared Function AskText(ByVal title As String, ByVal defaultText As String) As String
        frmInputString.Text = title
        Dim s As String = frmInputString.GetString(defaultText).Trim()
        Return If(s = defaultText, "", s)
    End Function

    Private Shared Function AskName(ByVal title As String, ByVal defaultText As String) As String
        Dim name As String = AskText(title, defaultText)
        If name <> "" AndAlso Not CheckNameRule(name) Then
            frmMsgBox.ShowCriticalMessage("姓名含有不合法的字元", "錯誤")
            Return ""
        End If
        Return name
    End Function

    Private Sub NameGroup(ByVal faces As List(Of FaceRegion))
        Dim name As String = AskName("這些臉是誰？", "")
        If name = "" Then Return
        SetBusy(True)
        Try
            g_lpFaces.NameFaces(faces, name)
        Finally
            SetBusy(False)
        End Try
        SelectFaceTreeNode(FaceNodeKey)
        ShowFaceWall(False)
    End Sub

    Private Sub NameCheckedFaces()
        Dim picked As New List(Of FaceRegion)
        For i = 0 To Math.Min(mlList.Count, m_lpGroupFaces.Count) - 1
            If mlList.Item(i).Checked Then picked.Add(m_lpGroupFaces(i))
        Next
        If picked.Count = 0 Then
            frmMsgBox.ShowCriticalMessage("請先勾選要命名的臉", "注意")
            Return
        End If
        NameGroup(picked)
    End Sub

    Private Sub RenameWallPerson()
        Dim p As FaceCatalog.PersonEntry = WallPerson
        If p Is Nothing Then Return
        Dim name As String = AskName("改名（輸入已有的名字會合併成同一人）", p.Name)
        If name = "" Then Return
        Dim other As FaceCatalog.PersonEntry = g_lpFaces.Catalog.Persons.FirstOrDefault(Function(x) x IsNot p AndAlso String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase))
        If other IsNot Nothing AndAlso Not frmQueryMsgBox.ShowMessage("「" & p.Name & "」會合併到「" & other.Name & "」，照片的人物欄也會一起改，確定嗎？", "合併") Then Return
        SetBusy(True)
        Try
            g_lpFaces.RenamePerson(p, name)
        Finally
            SetBusy(False)
        End Try
        lblFaceScan.Text = "整理面孔中…"
    End Sub

    Private Sub SetWallBirthYear()
        Dim p As FaceCatalog.PersonEntry = WallPerson
        If p Is Nothing Then Return
        Dim s As String = AskText(p.Name & " 的出生年（西元，0 = 不知道）", If(p.BirthYear > 0, p.BirthYear.ToString(), "0"))
        If s = "" Then Return
        Dim y As Integer
        If Not Integer.TryParse(s, y) OrElse (y <> 0 AndAlso (y < 1900 OrElse y > DateTime.Now.Year)) Then
            frmMsgBox.ShowCriticalMessage("請輸入 1900 到今年之間的西元年，或 0", "錯誤")
            Return
        End If
        g_lpFaces.SetBirthYear(p, y)
        lblFaceScan.Text = "整理面孔中…"
    End Sub

    Private Sub ChangeWallCover()
        Dim p As FaceCatalog.PersonEntry = WallPerson
        If p Is Nothing OrElse p.Cover Is Nothing Then Return
        Using f As New frmCover
            f.Text1.Text = p.Name
            f.SetCover(p.Cover.FileName)
            If f.Result Then ShowFaceWall(m_bolWallGroupsOnly)
        End Using
    End Sub

    Private Sub HideWallPerson()
        Dim p As FaceCatalog.PersonEntry = WallPerson
        If p Is Nothing Then Return
        If Not frmQueryMsgBox.ShowMessage("隱藏「" & p.Name & "」？照片的人物欄不會改，之後也不會再自動認出這個人。", "隱藏") Then Return
        g_lpFaces.HidePerson(p)
        ShowFaceWall(m_bolWallGroupsOnly)
        Dim n() As TreeNode = tvList.Nodes.Find(FaceNodeKey, False)
        If n.Length > 0 Then FillFaceNode(n(0))
    End Sub

End Class
