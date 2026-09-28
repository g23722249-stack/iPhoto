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
    Private Const FaceHiddenKey As String = "FACE:HIDDEN"
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
            Dim hidden As Integer = cat.Persons.Where(Function(p) p.Hidden).Count()
            If hidden > 0 Then AddFaceChild(n, FaceHiddenKey, "已隱藏的人 (" & hidden & ")")
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
        If m_enumFaceView = FaceView.fvWall Then ShowFaceWall(m_enumWallKind)   ' group / confirm views are left alone: the user is ticking faces
        ' a 面孔 node clicked before the first sort was done: show it now
        If m_strFacePending IsNot Nothing AndAlso tvList.SelectedNode IsNot Nothing AndAlso
           String.Equals(TryCast(tvList.SelectedNode.Tag, String), m_strFacePending, StringComparison.Ordinal) Then
            m_strFacePending = Nothing
            FaceNodeClick(tvList.SelectedNode)
        End If
        Dim cat As FaceCatalog = g_lpFaces?.Catalog
        If lblFaceScan IsNot Nothing AndAlso cat IsNot Nothing AndAlso Not lblFaceScan.Text.StartsWith("分析面孔") Then
            lblFaceScan.Text = "面孔：" & cat.VisiblePersons.Count & " 人 · " & cat.Clusters.Count & " 群未命名"
            vb6ToolTip.SetToolTip(lblFaceScan, g_lpFaces.LastOrganizeInfo)
        End If
    End Sub

    ''' <summary>The 面孔 node clicked while the first sort after starting was still running (no catalog
    ''' yet): Organized shows it when the sort is done, if it is still the selected node.</summary>
    Private m_strFacePending As String

    ''' <summary>Called first by tvList_Click: True when the node is 面孔 or one of its children.</summary>
    Private Function FaceNodeClick(ByVal node As TreeNode) As Boolean
        Dim key As String = TryCast(node.Tag, String)
        ShowFaceGuideLink(node)
        m_strFacePending = Nothing
        If key Is Nothing OrElse Not key.StartsWith(FaceNodeKey) Then Return False
        If g_lpFaces Is Nothing Then Return True
        If g_lpFaces.Catalog Is Nothing Then
            ' the first sort after starting iPhoto isn't done: say so, and show the wall when it is
            ClearScreenAlbum()
            m_strFacePending = key
            txtTitle.Text = "面孔"
            lblPhotoCounts.Text = "整理面孔中…完成後自動顯示"
            If Not g_lpFaces.IsRunning Then g_lpFaces.RequestOrganize()   ' a running scan sorts when it ends
            Return True
        End If
        Select Case True
            Case key = FaceNodeKey : ShowFaceWall(WallKind.wkAll)
            Case key = FaceGroupsKey : ShowFaceWall(WallKind.wkGroups)
            Case key = FaceHiddenKey : ShowFaceWall(WallKind.wkHidden)
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
    ''' <summary>What the face wall shows: everybody and the unnamed groups, only the groups, or the hidden people.</summary>
    Private Enum WallKind
        wkAll = 0
        wkGroups = 1
        wkHidden = 2
    End Enum
    Private m_enumWallKind As WallKind
    Private ReadOnly m_lpWallItems As New List(Of Object)    ' PersonEntry or List(Of FaceRegion), by mlList index
    Private m_lpGroup As List(Of FaceRegion)
    Private m_lpGroupFaces As New List(Of FaceRegion)          ' by mlList index in the group view
    Private WithEvents mnuFacePerson As ContextMenuStrip
    Private mnuFaceHide As ToolStripMenuItem
    Private WithEvents mnuFaceGroupCard As ContextMenuStrip
    Private WithEvents mnuFaceGroup As ContextMenuStrip
    Private WithEvents mnuFaceConfirm As ContextMenuStrip
    Private mnuFaceConfirmYes As ToolStripMenuItem
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

    Private Sub ShowFaceWall(ByVal kind As WallKind)
        Dim cat As FaceCatalog = g_lpFaces.Catalog
        If cat Is Nothing Then Return
        SetBusy(True)
        Try
            BeginFaceList(FaceView.fvWall)
            m_enumWallKind = kind
            Dim cache As String = g_lpFaces.CacheFolder
            If kind <> WallKind.wkGroups Then
                For Each p In If(kind = WallKind.wkHidden, cat.Persons.Where(Function(x) x.Hidden).ToList(), cat.VisiblePersons)
                    Dim card As String = g_lpDatabase.LoadFaceCover(p.Name)   ' a name card made with frmCover wins
                    If card = "" OrElse Not IO.File.Exists(card) Then card = FaceCards.PersonCard(cache, p)
                    If card = "" Then Continue For
                    Dim pending As Integer = p.ToConfirm.Count
                    AddFaceItem(card, p.Name & "：" & p.PhotoCount & " 張照片" & If(pending > 0, "，" & pending & " 張待確認", "") & vbCrLf &
                                      If(p.Hidden, "已隱藏：按右鍵「取消隱藏」", "按兩下看照片，按右鍵確認更多照片 / 改名 / 合併 / 找合照"), False)
                    m_lpWallItems.Add(p)
                Next
            End If
            For Each g In If(kind = WallKind.wkHidden, Enumerable.Empty(Of List(Of FaceRegion))(), cat.Clusters.Take(MaxGroupsShown))
                Dim card As String = FaceCards.GroupCard(cache, g)
                If card = "" Then Continue For
                AddFaceItem(card, g.Count & " 張可能是同一人的臉" & vbCrLf & "按兩下逐張確認後命名", False)
                m_lpWallItems.Add(g)
            Next
            Select Case kind
                Case WallKind.wkGroups : txtTitle.Text = "未命名的臉" : m_strShownKey = FaceGroupsKey
                Case WallKind.wkHidden : txtTitle.Text = "已隱藏的人" : m_strShownKey = FaceHiddenKey
                Case Else : txtTitle.Text = "面孔" : m_strShownKey = FaceNodeKey
            End Select
            lblPhotoCounts.Text = If(kind = WallKind.wkHidden, m_lpWallItems.Count & " 人已隱藏",
                                     cat.VisiblePersons.Count & " 人 · " & cat.Clusters.Count & " 群未命名")
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
            m_strShownKey = FacePersonPrefix & p.PersonID
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
                mnuFaceHide.Text = If(p.Hidden, "取消隱藏", "隱藏")
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
        mnuFacePerson.Items.Add("找合照…", Nothing, Sub() FindTogether(WallPerson))
        mnuFacePerson.Items.Add(New ToolStripSeparator())
        mnuFaceHide = New ToolStripMenuItem("隱藏", Nothing, Sub() HideWallPerson())
        mnuFacePerson.Items.Add(mnuFaceHide)

        mnuFaceGroupCard = New ContextMenuStrip()
        mnuFaceGroupCard.Items.Add("逐張確認…", Nothing, Sub() FaceWall_ItemDblClick(m_intFaceMenuIndex))
        mnuFaceGroupCard.Items.Add("全部命名為…", Nothing, Sub() NameGroup(CType(m_lpWallItems(m_intFaceMenuIndex), List(Of FaceRegion))))
        mnuFaceGroupCard.Items.Add(New ToolStripSeparator())
        mnuFaceGroupCard.Items.Add("我不認識…", Nothing, Sub() MarkStrangers(CType(m_lpWallItems(m_intFaceMenuIndex), List(Of FaceRegion))))

        mnuFaceGroup = New ContextMenuStrip()
        mnuFaceGroup.Items.Add("全圖瀏覽", Nothing, Sub() ShowFaceFullView(m_intFaceMenuIndex))
        mnuFaceGroup.Items.Add("以檔案總管開啟", Nothing, Sub() ShowFaceInExplorer(m_intFaceMenuIndex))
        mnuFaceGroup.Items.Add(New ToolStripSeparator())
        mnuFaceGroup.Items.Add("將勾選的臉命名為…", Nothing, Sub() NameCheckedFaces())
        mnuFaceGroup.Items.Add("勾選的臉我不認識…", Nothing, Sub() MarkCheckedStrangers())
        mnuFaceGroup.Items.Add(New ToolStripSeparator())
        mnuFaceGroup.Items.Add("全部勾選", Nothing, Sub() mlList.CheckAll(True))
        mnuFaceGroup.Items.Add("全部取消勾選", Nothing, Sub() mlList.CheckAll(False))
        mnuFaceGroup.Items.Add(New ToolStripSeparator())
        mnuFaceGroup.Items.Add("回到面孔牆", Nothing, Sub()
                                                     SelectFaceTreeNode(FaceNodeKey)
                                                     ShowFaceWall(WallKind.wkAll)
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
            ShowFaceWall(WallKind.wkAll)
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
            txtTitle.Text = p.Name
            m_strShownKey = FacePersonPrefix & p.PersonID & " — 確認更多照片"
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
        mnuFaceConfirmYes = New ToolStripMenuItem("勾選的是這個人、其餘不是", Nothing, Sub() ApplyConfirm(True))
        mnuFaceConfirm.Items.Add("全圖瀏覽", Nothing, Sub() ShowFaceFullView(m_intFaceMenuIndex))
        mnuFaceConfirm.Items.Add("以檔案總管開啟", Nothing, Sub() ShowFaceInExplorer(m_intFaceMenuIndex))
        mnuFaceConfirm.Items.Add(New ToolStripSeparator())
        mnuFaceConfirm.Items.Add(mnuFaceConfirmYes)
        mnuFaceConfirm.Items.Add("只確認勾選的（其餘之後再說）", Nothing, Sub() ApplyConfirm(False))
        mnuFaceConfirm.Items.Add(New ToolStripSeparator())
        mnuFaceConfirm.Items.Add("全部勾選", Nothing, Sub() mlList.CheckAll(True))
        mnuFaceConfirm.Items.Add("全部取消勾選", Nothing, Sub() mlList.CheckAll(False))
        mnuFaceConfirm.Items.Add(New ToolStripSeparator())
        mnuFaceConfirm.Items.Add("回到面孔牆", Nothing, Sub()
                                                       SelectFaceTreeNode(FaceNodeKey)
                                                       ShowFaceWall(WallKind.wkAll)
                                                   End Sub)
    End Sub

    Private Sub mnuFaceConfirm_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles mnuFaceConfirm.Opening
        If m_lpConfirmPerson Is Nothing Then Return
        mnuFaceConfirmYes.Text = "勾選的是「" & m_lpConfirmPerson.Name & "」、其餘不是"
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
        ShowFaceWall(WallKind.wkAll)
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
            If f.Result Then ShowFaceWall(m_enumWallKind)
        End Using
    End Sub

    ''' <summary>隱藏 / 取消隱藏 (on the 已隱藏的人 wall).</summary>
    Private Sub HideWallPerson()
        Dim p As FaceCatalog.PersonEntry = WallPerson
        If p Is Nothing Then Return
        If p.Hidden Then
            g_lpFaces.UnhidePerson(p)
        Else
            If Not frmQueryMsgBox.ShowMessage("隱藏「" & p.Name & "」？照片的人物欄不會改，之後也不會再自動認出這個人。" & vbCrLf &
                                              "可以在「面孔 › 已隱藏的人」取消隱藏。", "隱藏") Then Return
            g_lpFaces.HidePerson(p)
        End If
        Dim kind As WallKind = m_enumWallKind
        If kind = WallKind.wkHidden AndAlso Not g_lpFaces.Catalog.Persons.Any(Function(x) x.Hidden) Then kind = WallKind.wkAll   ' nobody hidden any more
        Dim n() As TreeNode = tvList.Nodes.Find(FaceNodeKey, False)
        If n.Length > 0 Then FillFaceNode(n(0))
        SelectFaceTreeNode(If(kind = WallKind.wkHidden, FaceHiddenKey, If(kind = WallKind.wkGroups, FaceGroupsKey, FaceNodeKey)))
        ShowFaceWall(kind)
    End Sub

    '==================================================================================================
    ' 找合照 -- the photos two or more people are all in (faces or people field: FaceLibrary.PhotosOf)
    '==================================================================================================
    Private Sub FindTogether(ByVal first As FaceCatalog.PersonEntry)
        If first Is Nothing OrElse g_lpFaces?.Catalog Is Nothing Then Return
        Dim others = g_lpFaces.Catalog.VisiblePersons.Where(Function(x) x IsNot first).ToList()
        If others.Count = 0 Then Return
        Dim picked As List(Of FaceCatalog.PersonEntry)
        Using f As New frmPickPeople
            picked = f.Pick("找「" & first.Name & "」和誰的合照？", others)
        End Using
        If picked Is Nothing OrElse picked.Count = 0 Then Return
        Dim people As New List(Of FaceCatalog.PersonEntry) From {first}
        people.AddRange(picked)
        SetBusy(True)
        Dim files() As String
        Try
            Dim together As IEnumerable(Of String) = g_lpFaces.PhotosOf(first)
            For Each p In picked
                together = together.Intersect(g_lpFaces.PhotosOf(p), StringComparer.OrdinalIgnoreCase)
            Next
            files = together.ToArray()
        Finally
            SetBusy(False)
        End Try
        Dim names As String = String.Join("、", people.Select(Function(x) x.Name))
        If files.Length = 0 Then
            frmMsgBox.ShowSmileMessage("沒有找到 " & names & " 的合照", "找合照")
            Return
        End If
        With m_lpAppEnv
            .ExeMode = enumExeMode.exeFace
            .SectionIndex = -1
            .KeyIndex = -1
        End With
        ClearScreenAlbum()
        MoveFilesToMediaList(files, files.Length)   ' PhotosOf gives them oldest first
        txtTitle.Text = names & " 的合照"
        txtRemark.Text = files.Length & " 張照片"
        If mlList.Visible Then mlList.Focus()
    End Sub

    '==================================================================================================
    ' 我不認識 -- faces of people the user doesn't know (a stranger in a group photo, a poster): kept,
    ' but never grouped or matched again (FaceLibrary.MarkStrangers, state 8). Photos and people
    ' fields don't change. 設定 › 面孔 brings them all back; naming one in the viewer brings that one.
    '==================================================================================================
    Private Sub MarkStrangers(ByVal faces As List(Of FaceRegion))
        If faces Is Nothing OrElse faces.Count = 0 Then Return
        If Not frmQueryMsgBox.ShowMessage("這 " & faces.Count & " 張臉都標成「我不認識」？" & vbCrLf &
                                          "之後不會再出現在「未命名的臉」，程式也不會再拿它們認人；照片和人物欄不變。" & vbCrLf &
                                          "標錯了可以在 設定 › 面孔 恢復，或在全圖瀏覽點臉直接輸入名字。", "我不認識") Then Return
        SetBusy(True)
        Try
            g_lpFaces.MarkStrangers(faces)
        Finally
            SetBusy(False)
        End Try
        Dim n() As TreeNode = tvList.Nodes.Find(FaceNodeKey, False)
        If n.Length > 0 Then FillFaceNode(n(0))
        If m_enumFaceView = FaceView.fvGroup AndAlso m_lpGroup IsNot Nothing Then
            ' what is left of the group (the unticked faces), else back to the wall
            m_lpGroup.RemoveAll(Function(f) f.State = FaceRegion.enumFaceState.fsStranger)
            If m_lpGroup.Count > 0 Then
                ShowGroupFaces(m_lpGroup)
                Return
            End If
        End If
        SelectFaceTreeNode(FaceNodeKey)
        ShowFaceWall(m_enumWallKind)
    End Sub

    Private Sub MarkCheckedStrangers()
        Dim picked As New List(Of FaceRegion)
        For i = 0 To Math.Min(mlList.Count, m_lpGroupFaces.Count) - 1
            If mlList.Item(i).Checked Then picked.Add(m_lpGroupFaces(i))
        Next
        If picked.Count = 0 Then
            frmMsgBox.ShowCriticalMessage("請先勾選不認識的臉", "注意")
            Return
        End If
        MarkStrangers(picked)
    End Sub

    '==================================================================================================
    ' 全圖瀏覽 from the group / confirm views: the whole photo of a face, face mode on and the face
    ' selected (frmViewerLarge.ShowFace); the viewer's prior / next go through the faces of the view.
    ' Faces named / confirmed / rejected in the viewer drop out of the view when it closes.
    '==================================================================================================
    Private Sub ShowFaceFullView(ByVal index As Integer)
        If m_frmViewer Is Nothing OrElse index < 0 OrElse index >= m_lpGroupFaces.Count Then Return
        If mlList.SelectedIndex <> index Then mlList.SelectedIndex = index
        ShowFaceInViewer(index)
        If m_frmViewerForm.Visible Then Return   ' two screens: the viewer is already up on the other one
        m_frmViewerForm.ShowDialog(Me)
        RefreshFaceViewAfterViewer()
    End Sub

    Private Sub ShowFaceInViewer(ByVal index As Integer)
        If m_frmViewer Is Nothing OrElse index < 0 OrElse index >= m_lpGroupFaces.Count Then Return
        Dim f As FaceRegion = m_lpGroupFaces(index)
        If Not IO.File.Exists(f.FileName) Then
            m_frmViewer.Clear()
            Return
        End If
        ShowPictureInViewer(f.FileName, index = 0, index = m_lpGroupFaces.Count - 1)
        TryCast(m_frmViewer, frmViewerLarge)?.ShowFace(f.FaceID)
    End Sub

    ''' <summary>The photo the face is in, selected in Explorer.</summary>
    Private Sub ShowFaceInExplorer(ByVal index As Integer)
        If index < 0 OrElse index >= m_lpGroupFaces.Count Then Return
        ShowInExplorer(m_lpGroupFaces(index).FileName)   ' frmMain_1280x1024.FileOps.vb
    End Sub
    ''' <summary>The viewer's prior / next move the selection; with two screens a click on a face does too.</summary>
    Private Sub FaceView_SelectedChanged(sender As Object, e As EventArgs) Handles mlList.SelectedChanged
        If m_enumFaceView <> FaceView.fvGroup AndAlso m_enumFaceView <> FaceView.fvConfirm Then Return
        If m_frmViewerForm Is Nothing OrElse Not m_frmViewerForm.Visible Then Return
        ShowFaceInViewer(mlList.SelectedIndex)
    End Sub

    ''' <summary>After the viewer: faces whose state changed there (named, ✓, ✕, 不是此人, 這不是臉) take
    ''' their new state and leave the view.</summary>
    Private Sub RefreshFaceViewAfterViewer()
        If m_enumFaceView <> FaceView.fvGroup AndAlso m_enumFaceView <> FaceView.fvConfirm Then Return
        Dim changed As Boolean = False
        For Each file In m_lpGroupFaces.Select(Function(f) f.FileName).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            Dim now As Dictionary(Of Integer, FaceRegion) = g_lpFaces.FacesOf(file).ToDictionary(Function(f) f.FaceID)
            For Each f In m_lpGroupFaces.Where(Function(x) String.Equals(x.FileName, file, StringComparison.OrdinalIgnoreCase))
                Dim cur As FaceRegion = Nothing
                Dim state As FaceRegion.enumFaceState = If(now.TryGetValue(f.FaceID, cur), cur.State, FaceRegion.enumFaceState.fsNotFace)
                Dim person As Integer = If(cur Is Nothing, 0, cur.PersonID)
                If state <> f.State OrElse person <> f.PersonID Then
                    f.State = state
                    f.PersonID = person
                    f.PersonName = If(cur Is Nothing, "", cur.PersonName)
                    changed = True
                End If
            Next
        Next
        If Not changed Then Return
        g_lpFaces.RequestOrganizeSoon()
        If m_enumFaceView = FaceView.fvConfirm Then
            ShowConfirmFaces(m_lpConfirmPerson)   ' ToConfirm drops what no longer needs confirming
        ElseIf m_lpGroup IsNot Nothing Then
            m_lpGroup.RemoveAll(Function(f) f.PersonID <> 0 OrElse f.State = FaceRegion.enumFaceState.fsStranger OrElse f.State = FaceRegion.enumFaceState.fsNotFace)
            If m_lpGroup.Count > 0 Then
                ShowGroupFaces(m_lpGroup)
            Else
                SelectFaceTreeNode(FaceNodeKey)
                ShowFaceWall(m_enumWallKind)
            End If
        End If
    End Sub

End Class
