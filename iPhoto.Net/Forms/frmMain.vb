' Port of iPhoto\Form\frmMain.frm (VB_Name frmMain_1280x1024; renamed frmMain in .NET, 2026-10): the main window's code. Controls are in
' .Designer.vb; the VB6 control arrays (imgButton, imgToolBox, lblToolBox) are properties in .Vb6.vb.
'
' Differences from VB6 (see PORTING.md):
'   - App.PrevInstance moved to Program.Main; Screen.MousePointer -> SetBusy (Application.UseWaitCursor).
'   - The viewer: VB6 declared WithEvents frmViewerLarge/frmViewerSmall here, but the form's own
'     CreateMultiMonters had that code commented out, so m_frmViewer stayed Nothing ("全圖瀏覽" raised
'     error 91) while LibMain.CreateMultiMonters built a viewer nobody used. CreateMultiMonters below now
'     does what LibMain's did and keeps the viewer here: one screen -> hidden, shown modally by
'     "全圖瀏覽"; two screens -> shown on the other screen, following the selected photo.
'   - tvList_Click is the MSComctl Click: a mouse click on a node (not on its +/-), wired from NodeMouseClick.
'   - The Embed text boxes and imgSubject get their own clicks (VB6 Embed boxes / Image controls were
'     windowless, so the clicks reached Form_MouseDown).
'   - VB6 bugs fixed: the Delete key removed the *next* photo from a book; clicking the tree in calendar
'     mode opened the batch-info dialog (imgButton 1) instead of standard mode (3); the class title
'     typed in txtTitle never reached the tree node; the search text went into the SQL unescaped.
Friend Class frmMain

    Private Enum enumImageButton
        ibAddition = 0
        ibModeStand = 1
        ibModeCalendar = 2
        ibModeList = 3
        ibNoteAll = 4
        ibNoteClear = 5
        ibDelete = -1
        ibSyncDate = 6
    End Enum

    Private m_lpExpNode As TreeNode
    Private m_lpCurrentPhoto As Photo
    Private m_lpAppEnv As udfApplication

    ' frmViewerLarge (screens 1280+ wide) or frmViewerSmall; m_frmViewerForm is the same object as a Form
    Private m_frmViewer As IPhotoViewer
    Private m_frmViewerForm As Form

    Private Shared ReadOnly LabelHover As Color = Color.FromArgb(&H40, &H40, &H40)
    Private Shared ReadOnly LabelNormal As Color = Color.FromArgb(&H80, &H80, &H80)
    Private Shared ReadOnly NoPhotoColor As Color = Color.FromArgb(&HC0, &HC0, &HC0)

    '==================================================================================================
    ' Helpers for VB6 run-time features
    '==================================================================================================
    ''' <summary>VB6 Screen.MousePointer = vbHourglass / vbDefault.</summary>
    Private Shared Sub SetBusy(ByVal bolBusy As Boolean)
        Application.UseWaitCursor = bolBusy
        Cursor.Current = If(bolBusy, Cursors.WaitCursor, Cursors.Default)
    End Sub

    ''' <summary>VB6 LoadPicture: a copy, so the file is not left locked (it may be rotated or restored next).
    ''' Nothing when the picture can't be read (damaged / unsupported file, or no memory left for it).</summary>
    Private Shared Function LoadPicture(ByVal szFile As String) As Bitmap
        Try
            Return Quartz.LoadPicture(szFile)
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException OrElse
                                   TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    ''' <summary>Hands the photo to the viewer. The viewer drops the picture it has first, so the old and
    ''' the new full-size pictures are not in memory together (32-bit process).</summary>
    Private Sub ShowPictureInViewer(ByVal szFile As String, ByVal bolFirst As Boolean, ByVal bolLast As Boolean)
        If m_frmViewer Is Nothing Then Return
        m_frmViewer.Clear()
        Dim pic As Bitmap = LoadPicture(szFile)
        If pic IsNot Nothing Then m_frmViewer.ShowPicture(szFile, pic, bolFirst, bolLast)
    End Sub

    ''' <summary>VB6 quartz.SavePicture(picture, file, quality): JPEG at the given quality.</summary>
    Private Shared Function SavePicture(ByVal img As Image, ByVal szFile As String, ByVal lQuality As Long) As Boolean
        Try
            Dim codec = Imaging.ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = Imaging.ImageFormat.Jpeg.Guid)
            Using ps As New Imaging.EncoderParameters(1)
                ps.Param(0) = New Imaging.EncoderParameter(Imaging.Encoder.Quality, lQuality)
                img.Save(szFile, codec, ps)
            End Using
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>The class (albums mode) or book (favorites mode) the tree has open, or Nothing.</summary>
    Private Function CurrentSection() As PhotoSet
        With m_lpAppEnv
            If .SectionIndex < 0 OrElse .KeyIndex < 0 Then Return Nothing
            Select Case .ExeMode
                Case enumExeMode.exeAlbums : Return g_lpStorage.Album(.SectionIndex).Item(.KeyIndex)
                Case enumExeMode.exeFavorites : Return g_lpStorage.Favorite(.SectionIndex).Item(.KeyIndex)
                Case Else : Return Nothing
            End Select
        End With
    End Function

    ''' <summary>VB6 picFocus.SetFocus: parks the focus away from the note boxes.</summary>
    Private Sub DropFocus()
        ActiveControl = Nothing
    End Sub

    Private Function ServerReady() As Boolean
        Return g_lpStorage IsNot Nothing AndAlso g_lpDatabase IsNot Nothing
    End Function

    '==================================================================================================
    ' Form
    '==================================================================================================
    Private Sub SetImageButtonState(ByVal enumButton As enumImageButton, ByVal Value As Boolean)
        Select Case enumButton
            Case enumImageButton.ibAddition
                If g_lpConfig.ReadOnly Then Value = False
                imgButton(0).Enabled = Value
            Case enumImageButton.ibModeStand : imgButton(1).Enabled = Value
            Case enumImageButton.ibModeCalendar : imgButton(2).Enabled = Value
            Case enumImageButton.ibModeList : imgButton(3).Enabled = Value
            Case enumImageButton.ibNoteAll : imgButton(4).Enabled = Value
            Case enumImageButton.ibNoteClear : imgButton(5).Enabled = Value
            Case enumImageButton.ibSyncDate
                If g_lpConfig.ReadOnly Then Value = False
                imgButton(6).Enabled = Value
            Case enumImageButton.ibDelete
                If g_lpConfig.ReadOnly Then Value = False
                imgButton(7).Enabled = Value
        End Select
    End Sub

    ' The window is sizable (designer: BorderStyle Sizable, MinimumSize = the designed size); the
    ' panels follow through their designer Anchors and the tool-bar buttons spread with its width.
    Private m_toolBarLayout As ProportionalLayout

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' App.PrevInstance: Program.Main
        m_toolBarLayout = New ProportionalLayout(pnlToolBar)
        For I As Integer = 0 To imgToolBox.Length - 1
            If imgToolBox(I) IsNot Nothing AndAlso lblToolBox(I) IsNot Nothing Then m_toolBarLayout.AddCaption(imgToolBox(I), lblToolBox(I))
        Next
        SetFormPosition()
        CreateServerObject(Application.StartupPath)
        CreateMultiMonters()
        InitialApplication()
        InitialScreenControlPorperty()
        InitialScreenControlPrivate()
        imgButton_Click(imgButton(3), EventArgs.Empty)   '標準模式
        butMode.SelectedIndex = 0
        ' VB6 also deleted C:\Log\Aqua\Aqua.Log here -- the VB6 Aqua OCX's log; Aqua.Net writes none.
        StartFaceScan()   ' frmMain.Faces.vb
    End Sub

    ''' <summary>A borderless window maximises over the whole monitor, task bar included; keep it to
    ''' the working area of the monitor it is on. (MaximizedBounds is relative to that monitor.)</summary>
    Private Sub UpdateMaximizedBounds()
        Dim scr As Screen = Screen.FromControl(Me)
        Dim wa As Rectangle = scr.WorkingArea
        MaximizedBounds = New Rectangle(wa.X - scr.Bounds.X, wa.Y - scr.Bounds.Y, wa.Width, wa.Height)
    End Sub

    Protected Overrides Sub OnLocationChanged(e As EventArgs)
        If WindowState = FormWindowState.Normal Then UpdateMaximizedBounds()
        MyBase.OnLocationChanged(e)
    End Sub

    ''' <summary>butMode (整理 / 攝影集) stays centred under the thumbnails whatever the window size --
    ''' under where the designer put them: dragging the tree / thumbnails split doesn't move it.
    ''' Done after the form's own layout pass: moving it while the anchored controls are still being
    ''' laid out makes WinForms re-anchor it against a half-updated size (it drifted up).</summary>
    Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
        MyBase.OnLayout(levent)
        If butMode Is Nothing OrElse mlList Is Nothing Then Return
        ApplyClassWidth()   ' tree / thumbnails split (frmMain.Splitter.vb)
        Dim x As Integer = ListHomeLeft + (mlList.Right - ListHomeLeft - butMode.Width) \ 2
        If butMode.Left <> x Then butMode.Left = x
        FitFaceScanLabel()   ' the face label in the title bar (frmMain.Faces.vb)
    End Sub

    ' Two screens: moving the main window onto the viewer's screen (or the viewer onto this one) swaps
    ' them; both places are saved for the next start (LibScreens).
    Private m_screenSwap As DualScreenSwap

    ''' <summary>Called once the window is on its screen (Program, after Load).</summary>
    Friend Sub StartScreenSwap()
        If m_screenSwap IsNot Nothing OrElse m_frmViewerForm Is Nothing OrElse Not g_bolDualScreen Then Return
        m_screenSwap = New DualScreenSwap(Me, m_frmViewerForm)
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        WindowPlacement.Save(DualScreenSwap.MainKey, Me)
        If m_frmViewerForm IsNot Nothing AndAlso m_frmViewerForm.Visible AndAlso g_bolDualScreen Then
            WindowPlacement.Save(DualScreenSwap.ViewerKey, m_frmViewerForm)
        End If
        m_screenSwap?.Dispose()
        m_screenSwap = Nothing
    End Sub

    Private Sub SetFormPosition()
        Dim scr As Rectangle = Screen.PrimaryScreen.Bounds
        StartPosition = FormStartPosition.Manual
        Location = New Point(scr.Left + (scr.Width - Width) \ 2, scr.Top + 6)
    End Sub

    Private Sub InitialApplication()
        With m_lpAppEnv
            .ExeMode = enumExeMode.exeAlbums
            .SectionIndex = -1
            .KeyIndex = -1
        End With
    End Sub

    ''' <summary>VB6 LibMain.CreateMultiMonters + this form's own (commented-out) copy: builds the viewer
    ''' for the screen it will use -- hidden on a single screen, shown on the other screen otherwise.</summary>
    Private Sub CreateMultiMonters()
        Dim cMonTo As CoreMedia.Monitor
        If Not g_bolDualScreen Then   ' one screen, or 設定 › 螢幕 單螢幕: the viewer lives in this window (SingleScreen.vb)
            cMonTo = g_lpMonitors.Monitor(Math.Min(g_intMainScreenIndex, g_intMonitorCount - 1))
        Else
            '多螢幕: the screen the viewer was on last time, if it's still there and not the main one
            Dim idx As Integer = WindowPlacement.ScreenIndex(WindowPlacement.SavedScreen(DualScreenSwap.ViewerKey))
            If idx < 0 OrElse idx >= g_intMonitorCount OrElse idx = g_intMainScreenIndex Then idx = If(g_intMainScreenIndex = 0, 1, 0)
            cMonTo = g_lpMonitors.Monitor(idx)
        End If

        If cMonTo.Width >= 1280 Then
            m_frmViewer = New frmViewerLarge
        Else
            m_frmViewer = New frmViewerSmall
        End If
        m_frmViewerForm = CType(m_frmViewer, Form)
        AddHandler m_frmViewer.ShowPriorPhoto, AddressOf Viewer_ShowPriorPhoto
        AddHandler m_frmViewer.ShowNextPhoto, AddressOf Viewer_ShowNextPhoto

        m_frmViewerForm.StartPosition = FormStartPosition.Manual
        m_frmViewerForm.Bounds = cMonTo.Bounds       ' the maximised window opens on this screen
        m_frmViewer.Create(cMonTo.Width, cMonTo.Height)
        If g_bolDualScreen Then m_frmViewerForm.Show()
        m_frmViewer.Clear()

        ' 全圖瀏覽 on the right-click menu: two screens too (it brings back a viewer that was closed)
        If Not g_bolDualScreen Then sliSize.Value = 3
        AquaMenu1.Item("mnuView").Visible = True
    End Sub

    ''' <summary>雙螢幕: the viewer again on its screen after Esc or its close button hid it (the right-click
    ''' 全圖瀏覽, Enter, a 地點 pick). False with one screen.</summary>
    Private Function ShowDualViewer() As Boolean
        If Not g_bolDualScreen OrElse m_frmViewerForm Is Nothing OrElse m_frmViewerForm.IsDisposed Then Return False
        If Not m_frmViewerForm.Visible Then m_frmViewerForm.Show()
        ' 地點 mode: it shows its strip's photo (ShowInPlaceViewer); else this list's
        If Not PlaceViewerActive AndAlso Not ListShowsCards AndAlso mlList.SelectedIndex >= 0 Then
            ShowCurrentInViewer(mlList.SelectedIndex = 0, mlList.SelectedIndex = mlList.Count - 1)
        End If
        Return True
    End Function

    ''' <summary>VB6 also set picFocus.Enabled, the Embed look of the note boxes, butMode's width and
    ''' pnlImport's place (over pnlToolBar) here -- those are designer properties now.</summary>
    Private Sub InitialScreenControlPorperty()
        SetImageButtonState(enumImageButton.ibAddition, True)
        SetImageButtonState(enumImageButton.ibModeStand, True)
        SetImageButtonState(enumImageButton.ibModeCalendar, True)
        SetImageButtonState(enumImageButton.ibModeList, True)
        SetImageButtonState(enumImageButton.ibNoteAll, False)
        SetImageButtonState(enumImageButton.ibNoteClear, False)
        SetImageButtonState(enumImageButton.ibSyncDate, False)
        SetImageButtonState(enumImageButton.ibDelete, False)

        '日曆
        InitialDateControl()

        iTextBox1.Visible = g_lpDatabase.Implement

        SetToolBarProperty()
        SetDockProperty()
    End Sub

    Private Sub SetToolBarProperty()
        For I As Integer = 0 To 8
            imgToolBox(I).Enabled = True
            lblToolBox(I).Enabled = True
        Next

        imgToolBox(9).Enabled = Not g_lpConfig.ReadOnly
        lblToolBox(9).Enabled = Not g_lpConfig.ReadOnly
        imgToolBox(10).Enabled = g_lpFileSystem.FileExists(g_lpConfig.Application(Config.enumApplication.AppImageEdit))
        imgToolBox(11).Enabled = g_lpFileSystem.FileExists(g_lpConfig.Application(Config.enumApplication.AppPrint))
        imgToolBox(12).Enabled = g_lpFileSystem.FileExists(g_lpConfig.Application(Config.enumApplication.AppMail))
        imgToolBox(13).Enabled = True   ' 地點 (was 燒錄)

        For I As Integer = 10 To imgToolBox.Length - 1
            lblToolBox(I).Enabled = imgToolBox(I).Enabled
        Next
    End Sub

    ''' <summary>VB6 Form_MouseDown / iForm1_MouseDown: a click on an Embed note box turns it into an
    ''' editor; a click on the subject icon opens the icon picker.</summary>
    Private Sub Form_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown
        If e.Button <> MouseButtons.Left Then Return
        For Each box As Aqua.TextBox In {txtTitle, txtDate, txtSpot, txtRemark}
            If box.Bounds.Contains(e.Location) Then
                ActivateNoteBox(box)
                Return
            End If
        Next
    End Sub

    Private Sub NoteBox_MouseDown(sender As Object, e As MouseEventArgs) Handles txtTitle.MouseDown, txtDate.MouseDown, txtSpot.MouseDown, txtRemark.MouseDown
        Dim box As Aqua.TextBox = CType(sender, Aqua.TextBox)
        If e.Button = MouseButtons.Left AndAlso box.Embed Then ActivateNoteBox(box)
    End Sub

    Private Sub ActivateNoteBox(ByVal box As Aqua.TextBox)
        If g_lpConfig.ReadOnly Then Return
        If m_lpAppEnv.SectionIndex < 0 Then Return
        If m_lpAppEnv.KeyIndex < 0 Then Return

        frmClass.Enabled = False
        Try
            box.Embed = False
            Application.DoEvents()
            box.Focus()
        Finally
            frmClass.Enabled = True
        End Try
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If m_frmViewerForm IsNot Nothing Then m_frmViewerForm.Dispose()
        m_frmViewerForm = Nothing
        m_frmViewer = Nothing
        m_lpCurrentPhoto = Nothing
        m_lpExpNode = Nothing

        DestroyServerObject()

        For Each lpForm As Form In Application.OpenForms.Cast(Of Form)().ToList()
            If lpForm IsNot Me Then lpForm.Close()
        Next
    End Sub

    '==================================================================================================
    ' Popup menu
    '==================================================================================================
    Private Sub AquaMenu1_MenuOpen(sender As Object, e As EventArgs) Handles AquaMenu1.MenuOpen
        Try
            mlList.Focus()
            Application.DoEvents()
        Catch
        End Try
    End Sub

    Private Sub AquaMenu1_MenuSelected(sender As Object, Menu As Aqua.MenuItem) Handles AquaMenu1.MenuSelected
        If mlList.Count <= 0 Then Return
        If mlList.SelectedItem Is Nothing OrElse mlList.SelectedItem.FileName.Trim() = "" Then Return
        If m_lpCurrentPhoto Is Nothing Then Return

        Dim Index As Integer = mlList.SelectedIndex
        Dim objMediaItem As Aqua.MediaItem = mlList.SelectedItem
        Dim bolFirst As Boolean = (Index = 0)
        Dim bolLast As Boolean = (Index = mlList.Count - 1)

        If FileOpsMenuSelected(Menu.Name, Index) Then Return   ' frmMain.FileOps.vb
        Select Case Menu.Name.ToUpperInvariant()
            Case "MNUCOPY"
                If m_lpCurrentPhoto.MediaType = enumPhotoMediaType.mdImage Then
                    Clipboard.Clear()
                    Using bmp As Bitmap = LoadPicture(m_lpCurrentPhoto.FileDesc)
                        If bmp IsNot Nothing Then Clipboard.SetImage(bmp)
                    End Using
                End If

            Case "MNUROTATECLOCKWISE", "MNUROTATECOUNTERCLOCKWISE"
                If Not m_lpCurrentPhoto.Backup() Then
                    frmMsgBox.ShowCriticalMessage("建立復原照片失敗...", "")
                    Exit Select
                End If
                ' VB6: the "左轉 90°" item (mnuRotateClockwise) turns by -90, "右轉 90°" by +90
                Dim angle As Long = If(Menu.Name.Equals("mnuRotateClockwise", StringComparison.OrdinalIgnoreCase), -90, 90)
                If Not RotatePicture(m_lpCurrentPhoto.FileDesc, angle) Then
                    frmMsgBox.ShowCriticalMessage("旋轉照片失敗...", "")
                    Exit Select
                End If
                objMediaItem.FileName = ""
                objMediaItem.FileName = m_lpCurrentPhoto.FileDesc
                mlList.SelectedIndex = Index
                ShowPictureInViewer(m_lpCurrentPhoto.FileDesc, bolFirst, bolLast)

            Case "MNUSHOWINFO"
                Using f As New frmPhotoInfo : f.ShowPhotoInfo(m_lpCurrentPhoto) : End Using

            Case "MNURANKINGNONE", "MNURANKINGLV1", "MNURANKINGLV2", "MNURANKINGLV3", "MNURANKINGLV4", "MNURANKINGLV5"
                Dim level As Integer = If(Menu.Name.EndsWith("None", StringComparison.OrdinalIgnoreCase), 0, CInt(Menu.Name.Substring(Menu.Name.Length - 1)))
                m_lpCurrentPhoto.Exif(enumPhotoExif.peRanking) = CStr(level)
                objMediaItem.Ranking = CType(level, Aqua.MediaItemRanking)
                g_lpDatabase.AddItem(m_lpCurrentPhoto)

            Case "MNUREVERTTOORIGINAL"
                If g_lpFileSystem.FileExists(m_lpCurrentPhoto.RestoreFile) Then
                    If Not g_lpFileSystem.CopyFile(m_lpCurrentPhoto.RestoreFile, objMediaItem.FileName, True) Then
                        frmMsgBox.ShowCriticalMessage("復原失敗...", "")
                    Else
                        objMediaItem.KeepChange = False
                        objMediaItem.FileName = ""
                        objMediaItem.FileName = m_lpCurrentPhoto.FileDesc
                        g_lpFileSystem.DeleteFile(m_lpCurrentPhoto.RestoreFile)
                        mlList.SelectedIndex = Index
                        ShowPictureInViewer(m_lpCurrentPhoto.FileDesc, bolFirst, bolLast)
                    End If
                End If

            Case "MNUVIEW"
                If m_frmViewer Is Nothing Then Exit Select
                If ShowDualViewer() Then Exit Select   ' two screens: the viewer back on its screen
                If EnterFullView() Then Return   ' 全圖 inside this window (SingleScreen.vb)
                ShowCurrentInViewer(bolFirst, bolLast)
                m_frmViewerForm.ShowDialog(Me)
                ' the viewer's prior/next buttons may have moved the selection; nothing else to do
        End Select
        mlList.Focus()
    End Sub

    Private Sub SetMediaListPopupMenu(ByVal objPhoto As Photo)
        With AquaMenu1
            If objPhoto.MediaType = enumPhotoMediaType.mdImage Then
                .Item("mnuCopy").Enabled = True
                .Item("mnuRotateClockwise").Enabled = Not g_lpConfig.ReadOnly
                .Item("mnuRotateCounterClockwise").Enabled = Not g_lpConfig.ReadOnly
            Else
                .Item("mnuCopy").Enabled = False
                .Item("mnuRotateClockwise").Enabled = False
                .Item("mnuRotateCounterClockwise").Enabled = False
            End If

            .Item("mnuShowInfo").Enabled = True

            If g_lpConfig.ReadOnly Then
                .Item("mnuMyRanking").Enabled = False
                .Item("mnuRevertToOriginal").Enabled = False
            Else
                .Item("mnuMyRanking").Enabled = True
                .Item("mnuRevertToOriginal").Enabled = g_lpFileSystem.FileExists(objPhoto.RestoreFile)
            End If
        End With
        SetFileOpsMenu()   ' frmMain.FileOps.vb
    End Sub

    '==================================================================================================
    ' Albums / photo books switch and tree
    '==================================================================================================
    Private Sub butMode_SelectedChanged(sender As Object, e As EventArgs) Handles butMode.SelectedChanged
        If Not ServerReady() Then Return   ' the designer sets SelectedIndex before Form_Load
        m_lpExpNode = Nothing

        With m_lpAppEnv
            .ExeMode = If(butMode.SelectedIndex = 0, enumExeMode.exeAlbums, enumExeMode.exeFavorites)
            .SectionIndex = -1
            .KeyIndex = -1
        End With

        LibUserInterface.AddSectionFakeKeyToTreeView(m_lpAppEnv.ExeMode, g_lpStorage, tvList)
        AddFaceNode()   ' frmMain.Faces.vb (last, so the 'first node' fallbacks still pick an album)
        AddBrowseNodes()   ' 時間軸 / 地點: frmMain.Browse.vb
        If tvList.Nodes.Count > 0 Then
            If LibUserInterface.LoadTreeViewRecentSection(m_lpAppEnv.ExeMode, g_lpStorage, tvList) Then
                tvList.Focus()
            End If
        End If
    End Sub

    Private Sub tvList_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvList.NodeMouseClick
        If e.Button <> MouseButtons.Left Then Return
        If (tvList.HitTest(e.Location).Location And TreeViewHitTestLocations.PlusMinus) <> 0 Then Return
        tvList.SelectedNode = e.Node
        ' the list already shows this node (and nothing replaced it since): nothing to load again
        If m_strShownKey IsNot Nothing AndAlso m_strShownKey = NodeKey(e.Node) Then Return
        tvList_Click()
    End Sub

    ''' <summary>What the list shows, as NodeKey of its tree node; Nothing when it shows something else
    ''' (search results, a face group, ...). Cleared by ClearScreenAlbum, set once a node's content is up.</summary>
    Private m_strShownKey As String

    ''' <summary>A tree node's identity: the 面孔 keys, else the mode and the node's path.</summary>
    Private Function NodeKey(ByVal node As TreeNode) As String
        If node Is Nothing Then Return ""
        Dim tag As String = TryCast(node.Tag, String)
        If tag IsNot Nothing Then Return tag
        Return butMode.SelectedIndex & "|" & node.FullPath
    End Function

    Private Sub tvList_Click()
        If tvList.SelectedNode Is Nothing Then Return
        If FaceNodeClick(tvList.SelectedNode) Then Return   ' the 面孔 node and its children
        If BrowseNodeClick(tvList.SelectedNode) Then Return   ' 時間軸 / 地點 (frmMain.Browse.vb)

        If m_lpAppEnv.ExeMode = enumExeMode.exeCalendar Then
            imgButton_Click(imgButton(3), EventArgs.Empty)   ' VB6 passed 1 (batch info) -- 3 is standard mode
        End If

        With m_lpAppEnv
            .ExeMode = If(butMode.SelectedIndex = 0, enumExeMode.exeAlbums, enumExeMode.exeFavorites)
            .SectionIndex = -1
            .KeyIndex = -1
            If tvList.SelectedNode.Parent Is Nothing Then
                .SectionIndex = CInt(tvList.SelectedNode.Tag)
                .KeyIndex = -1
            Else
                .SectionIndex = CInt(tvList.SelectedNode.Parent.Tag)
                .KeyIndex = CInt(tvList.SelectedNode.Tag)
            End If
        End With

        If tvList.Nodes.Count <= 0 Then Return
        SetImageButtonState(enumImageButton.ibDelete, True)

        '紀錄最近開啟的相本
        With m_lpAppEnv
            Select Case .ExeMode
                Case enumExeMode.exeAlbums
                    If g_lpStorage.AlbumCount > 0 Then LibUserInterface.SaveRecentSection(.ExeMode, g_lpStorage, .SectionIndex)
                Case enumExeMode.exeFavorites
                    If g_lpStorage.FavoriteCount > 0 Then LibUserInterface.SaveRecentSection(.ExeMode, g_lpStorage, .SectionIndex)
            End Select
            LibUserInterface.SaveRecentKeyValue(.ExeMode, "")   '避免下一次進來自動開啟相本
        End With
        '清除畫面的資料
        ClearScreenAlbum()
        If tvList.SelectedNode.Parent Is Nothing Then
            ShowCoverWall(tvList.SelectedNode)   ' frmMain.Covers.vb: open it, one cover per album
            Return
        End If

        '搬相片至畫面上
        SetBusy(True)
        Enabled = False
        Try
            Dim lpKey As PhotoSet = CurrentSection()
            If lpKey Is Nothing Then
                MessageBox.Show("不正確的執行模式...", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            lpKey.Load()
            Dim intCount As Integer = lpKey.PhotoCount

            mlList.Clear()

            If g_bolDualScreen Then
                sliSize.Value = LimitFor(intCount)
                Application.DoEvents()
            End If

            AddPhotoToMediaList(m_lpAppEnv.ExeMode, g_lpStorage, mlList, sliSize.Value, m_lpAppEnv.SectionIndex, m_lpAppEnv.KeyIndex)

            DockedMediaList(g_lpDock, mlList)
            MoveNoteToScreen(m_lpAppEnv.ExeMode, m_lpAppEnv.SectionIndex, m_lpAppEnv.KeyIndex)
            If mlList.Count > 0 Then
                mlList.ScrollValue = mlList.ScrollMin
                mlList.SelectedIndex = 0
            End If
            m_strShownKey = NodeKey(tvList.SelectedNode)
            Enabled = True
            If mlList.Visible Then mlList.Focus()
        Finally
            Enabled = True
            SetBusy(False)
        End Try
    End Sub

    ''' <summary>Thumbnails per row for a photo count (VB6 Select Case, including its gap at 37..47).</summary>
    Private Shared Function LimitFor(ByVal intCount As Integer) As Integer
        Select Case intCount
            Case Is <= 6 : Return 2
            Case Is <= 12 : Return 3
            Case Is <= 24 : Return 4
            Case Is <= 36 : Return 6
            Case Is >= 48 : Return 7
            Case Else : Return 6
        End Select
    End Function

    Private Sub tvList_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvList.BeforeExpand
        Dim Node As TreeNode = e.Node
        If Node.Parent IsNot Nothing Then Return   ' only sections have children
        If TypeOf Node.Tag Is String Then Return   ' the 面孔 node (its children are filled already)
        If m_lpExpNode IsNot Nothing AndAlso m_lpExpNode IsNot Node Then m_lpExpNode.Collapse()
        m_lpExpNode = Node
        LibUserInterface.ExpandFakeSectionKeyToTreeView(m_lpAppEnv.ExeMode, g_lpStorage, tvList, Node, CInt(Node.Tag))
        ClearScreenAlbum()

        With m_lpAppEnv
            .SectionIndex = CInt(Node.Tag)
            .KeyIndex = -1
        End With
    End Sub

    Private Sub tvList_GotFocus(sender As Object, e As EventArgs) Handles tvList.GotFocus
        frmClass.BorderColor = mlList.BorderFocusColor   ' VB6 also set frmClass.Parhelia (no Aqua.Net equivalent)
    End Sub

    Private Sub tvList_LostFocus(sender As Object, e As EventArgs) Handles tvList.LostFocus
        frmClass.BorderColor = mlList.BorderColor
    End Sub

    '==================================================================================================
    ' Buttons above the list (imgButton 0..7)
    '==================================================================================================
    Private Sub imgButton_Click(sender As Object, e As EventArgs) Handles imgButton_0.Click, imgButton_1.Click, imgButton_2.Click, imgButton_3.Click, imgButton_4.Click, imgButton_5.Click, imgButton_6.Click, imgButton_7.Click
        Dim Index As Integer = Array.IndexOf(imgButton, sender)

        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)

        imgButton(Index).BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Application.DoEvents()
        Darwin.Wait(50)
        Application.DoEvents()
        imgButton(Index).BorderStyle = System.Windows.Forms.BorderStyle.None

        Select Case Index
            Case 0 '新增
                Using f As New frmAddition
                    Select Case butMode.SelectedIndex
                        Case 0 : f.ShowAdditionAlbum()
                        Case 1 : f.ShowAdditionFavorite()
                    End Select
                    If f.Changed Then RefreshTree()
                End Using

            Case 1 '批次修改資訊
                Using f As New frmPhotoInfoBatch_1 : f.ShowDialog(Me) : End Using

            ' VB6 also re-sized frmClass / tvList to the same height (mlList - picClass) in both modes;
            ' that size is in the designer now. pnlDateMode lies over the note boxes (z-order: designer).
            Case 2 '日曆模式
                pnlDateMode.Visible = True

            Case 3 '標準模式
                pnlDateMode.Visible = False
                If tvList.CanFocus Then tvList.Focus()

            Case 4 '全選
                For I As Integer = 0 To mlList.Count - 1
                    mlList.Item(I).Marked = True
                    Dim lpPhoto As New Photo
                    lpPhoto.Construct(mlList.Item(I).FileName)
                    g_lpDock.AddItem(mlList.Item(I).FileName, lpPhoto.Exif(enumPhotoExif.peDate), lpPhoto.Exif(enumPhotoExif.peTime))
                Next
                SetDockProperty()

            Case 5 '全不選
                For I As Integer = 0 To mlList.Count - 1
                    mlList.Item(I).Marked = False
                    g_lpDock.RemoveItem(mlList.Item(I).FileName)
                Next
                SetDockProperty()

            Case 6 '調整日期
                If m_lpAppEnv.ExeMode = enumExeMode.exeAlbums Then
                    Dim lpSection As PhotoSet = CurrentSection()
                    If lpSection IsNot Nothing Then
                        Using f As New frmDateTime : f.ShowDateTime(lpSection, m_lpCurrentPhoto) : End Using
                    End If
                End If

            Case 7 '刪除
                DeleteSelectedNode()
        End Select
    End Sub

    ''' <summary>After the album / book list changed on disk: reload it and reopen the recent album.</summary>
    Private Sub RefreshTree()
        g_lpStorage.Refresh()
        butMode_SelectedChanged(butMode, EventArgs.Empty)
        If tvList.Nodes.Count > 0 Then
            If LibUserInterface.LoadTreeViewRecentSection(m_lpAppEnv.ExeMode, g_lpStorage, tvList) Then
                tvList.Focus()
            End If
        End If
    End Sub

    Private Sub DeleteSelectedNode()
        If m_lpAppEnv.SectionIndex < 0 Then Return
        Dim node As TreeNode = tvList.SelectedNode
        If node Is Nothing Then Return

        If node.Parent Is Nothing Then
            If Not frmQueryMsgBox.ShowMessage("是否確定刪除相片庫？", "") Then Return
            If Not g_lpFileSystem.DeleteFolder(node.Name) Then
                frmMsgBox.ShowCriticalMessage("刪除相片庫失敗...", "")
                Return
            End If
        Else
            If g_lpFileSystem.FileExists(node.Name) Then
                If Not frmQueryMsgBox.ShowMessage("是否確定刪除攝影集？", "") Then Return
                If Not g_lpFileSystem.DeleteFile(node.Name) Then
                    frmMsgBox.ShowCriticalMessage("刪除攝影集失敗...", "")
                    Return
                End If
            ElseIf g_lpFileSystem.FolderExists(node.Name) Then
                If Not frmQueryMsgBox.ShowMessage("是否確定刪除相簿？", "") Then Return
                If Not g_lpFileSystem.DeleteFolder(node.Name) Then
                    frmMsgBox.ShowCriticalMessage("刪除相簿失敗...", "")
                    Return
                End If
            Else
                MessageBox.Show("無法辨識的資料" & vbCrLf & node.Name, "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            If m_frmViewer IsNot Nothing Then m_frmViewer.Clear()
            mlList.Clear()
        End If
        RefreshTree()
    End Sub

    Private Sub imgSubject_Click(sender As Object, e As EventArgs) Handles imgSubject.Click
        If g_lpConfig.ReadOnly Then Return
        If m_lpAppEnv.KeyIndex < 0 Then Return
        If tvList.SelectedNode Is Nothing Then Return

        imgSubject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Application.DoEvents()
        Darwin.Wait(80)
        imgSubject.BorderStyle = System.Windows.Forms.BorderStyle.None
        Application.DoEvents()

        Dim pt As Point = picSubject.PointToScreen(Point.Empty)
        Dim strIcon As String = frmSubject.ShowSubject(pt.X, pt.Y)
        If strIcon.Trim() = "" Then Return

        imgSubject.Tag = strIcon
        Dim objImageList As ImageList = g_lpConfig.ImageListSubject
        If objImageList.Images.ContainsKey(strIcon) Then imgSubject.Image = objImageList.Images(strIcon)
        imgSubject.SizeMode = PictureBoxSizeMode.StretchImage

        Dim lpKey As PhotoSet = CurrentSection()
        If lpKey Is Nothing Then Return
        Dim node As TreeNode = FindNode(tvList, lpKey.Key)
        If node IsNot Nothing Then
            node.ImageKey = strIcon
            node.SelectedImageKey = strIcon
        End If
        lpKey.Note(enumNote.ntIcon) = strIcon
    End Sub

    '==================================================================================================
    ' Tool box (imgToolBox 0..14; 14 同步備份 is new, placed before 13; 13 was 燒錄, now 地點)
    '==================================================================================================
    Private Sub imgToolBox_Click(sender As Object, e As EventArgs) Handles imgToolBox_0.Click, imgToolBox_1.Click, imgToolBox_2.Click, imgToolBox_3.Click, imgToolBox_4.Click, imgToolBox_5.Click, imgToolBox_6.Click, imgToolBox_7.Click, imgToolBox_8.Click, imgToolBox_9.Click, imgToolBox_10.Click, imgToolBox_11.Click, imgToolBox_12.Click, imgToolBox_13.Click, imgToolBox_14.Click
        Dim Index = Array.IndexOf(imgToolBox, sender)
        Dim strArray() As String = Nothing
        Dim I As Integer

        imgToolBox(Index).Enabled = False
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        Select Case Index
            Case 0 '基本設置
                Using f As New frmSetup
                    f.ShowDialog(Me)
                    If f.Changed Then
                        SetToolBarProperty
                        SetDockProperty
                        ApplyFaceSettings   ' frmMain.Faces.vb
                    End If
                End Using

            Case 1 'Dock
                If g_lpDock.Count > 0 Then
                    Using f As New frmDock : f.ShowDialog(Me) : End Using
                    '清空 Dock
                    If g_lpDock.Count <= 0 Then
                        For I = 0 To mlList.Count - 1
                            mlList.Item(I).Marked = False
                        Next
                        SetDockProperty
                    End If
                End If

            Case 2 '建立攝影集
                If g_lpDock.Count <= 0 Then Return   ' VB6 left the button disabled here too (SetDockProperty agrees)
                ReDim strArray(g_lpDock.Count - 1)
                For I = 0 To g_lpDock.Count - 1
                    strArray(I) = g_lpDock.FileName(I)
                Next
                Using f As New frmBuildBook : f.BuildBook(strArray, g_lpDock.Count) : End Using
                If m_lpAppEnv.ExeMode = enumExeMode.exeFavorites Then
                    g_lpStorage.Refresh
                    butMode_SelectedChanged(butMode, EventArgs.Empty)
                    If LoadTreeViewRecentKey(enumExeMode.exeAlbums, g_lpStorage, tvList) Then
                        tvList_Click
                    End If
                End If

            Case 3 '搜尋
                Using f As New frmSearch : f.ShowDialog(Me) : End Using
                SetDockProperty
                mlList.Focus

            Case 4 '匯出
                If g_lpDock.Count > 0 Then
                    Using f As New frmExport : f.ShowDialog(Me) : End Using
                End If

            Case 5 '輸入
                If g_lpConfig.ImportNewStyle Then
                    ImportWithStudio   ' the new window, frmMain.Import.vb (設定 › 相片庫 can go back to frmImport)
                Else
                    Using f As New frmImport
                        f.ShowDialog(Me)
                    End Using
                    ImportAlbumFiles
                    If g_lpImport.Count > 0 Then
                        '打開匯入的相簿
                        SaveRecentSectionValue(enumExeMode.exeAlbums, g_lpImport.AlbumPath)
                        SaveRecentKeyValue(enumExeMode.exeAlbums, g_lpImport.ClassPath)
                    End If
                End If
                If m_lpAppEnv.ExeMode = enumExeMode.exeAlbums Then
                    g_lpStorage.Refresh
                    butMode_SelectedChanged(butMode, EventArgs.Empty)
                    If LoadTreeViewRecentKey(enumExeMode.exeAlbums, g_lpStorage, tvList) Then
                        tvList_Click
                    End If
                End If

            Case 6 '幻燈片
                Using f As New frmSlideShow : f.SlideShow(Val(g_lpConfig.Slide(Config.enumSlide.sliShowTimes)), m_lpAppEnv.SectionIndex, m_lpAppEnv.KeyIndex, mlList.SelectedIndex) : End Using

            Case 7 '索引圖
                Using f As New frmPhotoIndex : f.ShowPhotoIndex(m_lpAppEnv.ExeMode) : End Using

            Case 8 '列印
                I = g_lpDock.SortFiles(strArray)
                Using f As New frmPrint : f.ShowPrintPhoto(strArray, I) : End Using

            Case 9 '照片資訊
                Using f As New frmPhotoInfoBatch_1 : f.ShowDialog(Me) : End Using

            Case 13 '地點 (was 燒錄): the map with the place tree, frmMain.Browse.vb
                ShowMap()

            Case 14 '同步備份 (new in the .NET version, see frmBackup)
                Using f As New frmBackup : f.ShowDialog(Me) : End Using

            Case Else
                Dim strFileDesc = ""
                Select Case Index
                    Case 10 : strFileDesc = g_lpConfig.Application(Config.enumApplication.AppImageEdit) '編輯
                    Case 11 : strFileDesc = g_lpConfig.Application(Config.enumApplication.AppPrint)     '沖印
                    Case 12 : strFileDesc = g_lpConfig.Application(Config.enumApplication.AppMail)      '郵件
                End Select
                If Not g_lpFileSystem.FileExists(strFileDesc) Then Return

                Try
                    Process.Start(New ProcessStartInfo(strFileDesc) With {.UseShellExecute = True, .WindowStyle = ProcessWindowStyle.Maximized})
                Catch ex As Exception
                    frmMsgBox.ShowCriticalMessage("無法啟動" & vbCrLf & strFileDesc, "")
                End Try

                If g_lpDock.Count > 0 Then
                    Using f As New frmDock : f.ShowDialog(Me) : End Using
                End If
        End Select
        imgToolBox(Index).Enabled = True
    End Sub

    Private Sub ImportAlbumFiles()
        If g_lpImport.Count <= 0 Then Return

        Enabled = False
        SetBusy(True)
        Try
            pnlImport.Visible = True   ' over pnlToolBar (designer z-order)

            lblImportTitle.Text = g_lpImport.Title
            lblImportDate.Text = g_lpImport.DateTime
            lblImportCount.Text = g_lpImport.Count & " 張相片"
            Dim strFolder As String = g_lpImport.ImportPath

            pgImport.Minimum = 0
            pgImport.Maximum = g_lpImport.Count
            pgImport.Value = 0

            Dim lpProfile As New Carbon.IniFile
            For I As Integer = 0 To g_lpImport.Count - 1
                '寫入資訊檔
                pgImport.Value = I
                Application.DoEvents()
                miImport.FileName = g_lpImport.Item(I)
                Dim strFileName As String = strFolder & "\" & g_lpFileSystem.AnalyseFile(fsFileName, g_lpImport.Item(I))
                Dim strExifFileName As String = strFolder & "\" & g_lpFileSystem.AnalyseFile(fsBaseName, g_lpImport.Item(I)) & "." & gc_strExifPattern

                g_lpFileSystem.CopyFile(g_lpImport.Item(I), strFileName)
                Dim strDateTime As String = GetExifFileDateTime(g_lpFileSystem, strFileName)

                With lpProfile
                    .FileName = strExifFileName
                    .SimpleSetValue("Create", "Date", Mid(strDateTime, 1, 8))
                    .SimpleSetValue("Create", "Time", Mid(strDateTime, 9, 6))

                    .SimpleSetValue("Exif", "Date", Mid(strDateTime, 1, 8))
                    .SimpleSetValue("Exif", "Time", Mid(strDateTime, 9, 6))

                    .SimpleSetValue("Exif", "Title", frmDefaultPhotoInfo.DefaultInformation(enumPhotoExif.peTitle))
                    .SimpleSetValue("Exif", "Character", frmDefaultPhotoInfo.DefaultInformation(enumPhotoExif.peCharacter))
                    ' where it was taken (new in the .NET port, PlaceNames): the picture's GPS, and the 地點
                    ' named from it unless a 地點 was given for the whole import
                    Dim strGps As String = PlaceNames.GpsOfPicture(strFileName)
                    Dim place As New PlaceNames.PlaceInfo
                    If strGps <> "" Then
                        .SimpleSetValue("Exif", "GPS", strGps)
                        place = PlaceNames.Resolve(strGps)   ' an attraction within 200 m, else county + district
                        If place.Country <> "" Then .SimpleSetValue("Exif", "Country", place.Country)
                        If place.City <> "" Then .SimpleSetValue("Exif", "City", place.City)
                        If place.Town <> "" Then .SimpleSetValue("Exif", "Town", place.Town)
                    End If
                    Dim strSpot As String = frmDefaultPhotoInfo.DefaultInformation(enumPhotoExif.peSpot)
                    If If(strSpot, "").Trim() = "" AndAlso strGps <> "" Then strSpot = place.Spot
                    .SimpleSetValue("Exif", "Spot", strSpot)
                    .SimpleSetValue("Exif", "KeyWord", frmDefaultPhotoInfo.DefaultInformation(enumPhotoExif.peKeyWord))
                    .SimpleSetValue("Exif", "Remark", frmDefaultPhotoInfo.DefaultInformation(enumPhotoExif.peRemark))
                End With
                '寫入 Database
                If g_lpDatabase.Implement Then
                    Dim lpPhoto As New Photo
                    lpPhoto.Construct(strFileName)
                    WritePhotoToDatabase(g_lpDatabase, lpPhoto)
                End If
            Next

            g_lpConfig.PlaySound(Config.enumSound.snImportFinish)
            pnlImport.Visible = False
            FaceScanAfterImport()   ' frmMain.Faces.vb
        Finally
            SetBusy(False)
            Enabled = True
        End Try
    End Sub

    Private Sub imgToolBox_MouseEnter(sender As Object, e As EventArgs) Handles imgToolBox_0.MouseEnter, imgToolBox_1.MouseEnter, imgToolBox_2.MouseEnter, imgToolBox_3.MouseEnter, imgToolBox_4.MouseEnter, imgToolBox_5.MouseEnter, imgToolBox_6.MouseEnter, imgToolBox_7.MouseEnter, imgToolBox_8.MouseEnter, imgToolBox_9.MouseEnter, imgToolBox_10.MouseEnter, imgToolBox_11.MouseEnter, imgToolBox_12.MouseEnter, imgToolBox_13.MouseEnter, imgToolBox_14.MouseEnter
        g_lpConfig.PlaySound(Config.enumSound.snButtonEnter)
        lblToolBox(Array.IndexOf(imgToolBox, sender)).ForeColor = LabelHover
    End Sub

    Private Sub imgToolBox_MouseLeave(sender As Object, e As EventArgs) Handles imgToolBox_0.MouseLeave, imgToolBox_1.MouseLeave, imgToolBox_2.MouseLeave, imgToolBox_3.MouseLeave, imgToolBox_4.MouseLeave, imgToolBox_5.MouseLeave, imgToolBox_6.MouseLeave, imgToolBox_7.MouseLeave, imgToolBox_8.MouseLeave, imgToolBox_9.MouseLeave, imgToolBox_10.MouseLeave, imgToolBox_11.MouseLeave, imgToolBox_12.MouseLeave, imgToolBox_13.MouseLeave, imgToolBox_14.MouseLeave
        lblToolBox(Array.IndexOf(imgToolBox, sender)).ForeColor = LabelNormal
    End Sub

    '==================================================================================================
    ' Search box
    '==================================================================================================
    Private Sub iTextBox1_ImageClick(sender As Object, e As EventArgs) Handles iTextBox1.ImageClick
        '尋找
        If iTextBox1.Text.Trim() = "" Then Return

        With m_lpAppEnv
            .ExeMode = enumExeMode.exeSearch
            .SectionIndex = -1
            .KeyIndex = -1
        End With

        Dim word As String = RTrim(iTextBox1.Text).Replace("'", "''")
        Dim strSQL As String = String.Join(" Or ",
            {"Exif_Title", "Exif_Character", "Exif_Spot", "Exif_Remark", "Exif_KeyWord"}.Concat(If(g_lpDatabase.HasPlaceColumns, New String() {"Exif_City"}, New String() {})).Select(Function(c) c & " Like '%" & word & "%'"))

        SetBusy(True)
        Enabled = False
        Try
            Dim strFileName() As String = Nothing
            Dim intCount As Integer = g_lpDatabase.SearchFileArrayList(strSQL, strFileName)
            If intCount > 0 Then MoveFilesToMediaList(strFileName, intCount)
        Finally
            SetBusy(False)
            SetImageButtonState(enumImageButton.ibDelete, False)
            Enabled = True
        End Try

        If mlList.Visible Then mlList.Focus()
    End Sub

    Private Sub iTextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles iTextBox1.KeyDown
        If e.KeyCode = Keys.Return Then
            e.SuppressKeyPress = True
            iTextBox1_ImageClick(iTextBox1, EventArgs.Empty)
        End If
    End Sub

    '==================================================================================================
    ' Viewer (frmViewerLarge / frmViewerSmall)
    '==================================================================================================
    Private Sub Viewer_ShowPriorPhoto(sender As Object, e As EventArgs)
        If PlaceStripStep(-1) Then Return   ' 地點 mode: through the viewer's strip (frmMain.PlaceViewer.vb)
        If mlList.SelectedIndex <= 0 Then Return
        mlList.SelectedIndex -= 1
    End Sub

    Private Sub Viewer_ShowNextPhoto(sender As Object, e As EventArgs)
        If PlaceStripStep(1) Then Return
        If mlList.SelectedIndex >= mlList.Count - 1 Then Return
        mlList.SelectedIndex += 1
    End Sub

    ''' <summary>Shows the selected photo / video in the viewer (a new copy of the picture each time:
    ''' the viewer owns and edits it).</summary>
    Private Sub ShowCurrentInViewer(ByVal bolFirst As Boolean, ByVal bolLast As Boolean)
        If m_frmViewer Is Nothing OrElse m_lpCurrentPhoto Is Nothing Then Return
        Select Case m_lpCurrentPhoto.MediaType
            Case enumPhotoMediaType.mdImage : ShowPictureInViewer(m_lpCurrentPhoto.FileDesc, bolFirst, bolLast)
            Case enumPhotoMediaType.mdVideo : m_frmViewer.ShowVideo(m_lpCurrentPhoto.FileDesc, bolFirst, bolLast)
            Case Else : m_frmViewer.Clear()
        End Select
    End Sub

    '==================================================================================================
    ' Media list
    '==================================================================================================
    Private Sub mlList_BeforeSelectedChanged(sender As Object, e As EventArgs) Handles mlList.BeforeSelectedChanged
        If ListShowsCards Then Return   ' face tiles / covers: no photo of the list is being edited
        If mlList.Count <= 0 Then Return
        If m_frmViewer Is Nothing Then Return
        If Not m_frmViewer.Changed Then Return
        If mlList.SelectedItem Is Nothing OrElse mlList.SelectedItem.FileName.Trim() = "" Then Return

        frmSaveChangedPhoto.ShowDialog()
        Select Case frmSaveChangedPhoto.Result
            Case scpResult.scpCancel

            Case scpResult.scpSave
                Dim objMediaItem As Aqua.MediaItem = mlList.SelectedItem
                If Not m_lpCurrentPhoto.Backup() Then
                    frmMsgBox.ShowCriticalMessage("建立復原照片失敗...", "")
                    Return
                End If
                If Not g_lpFileSystem.DeleteFile(m_lpCurrentPhoto.FileDesc) Then Return
                If Not SavePicture(m_frmViewer.Photo, m_lpCurrentPhoto.FileDesc, 100) Then Return
                objMediaItem.FileName = m_lpCurrentPhoto.FileDesc

            Case scpResult.scpCopy
                Clipboard.Clear()
                Clipboard.SetImage(m_frmViewer.Photo)

            Case scpResult.scpSaveAs
                Dim strFileDesc As String = frmBrowserFile.GetFile("另存新檔", "圖形檔 (*.Jpg)|*.Jpg", False).Trim()
                If strFileDesc = "" Then Return
                If Not g_lpFileSystem.DeleteFile(strFileDesc) Then Return
                If Not SavePicture(m_frmViewer.Photo, strFileDesc, 100) Then
                    frmMsgBox.ShowCriticalMessage("另存新檔失敗...", "")
                End If
        End Select
    End Sub

    Private Sub mlList_ItemDblClick(index As Integer) Handles mlList.ItemDblClick
        If ListShowsCards Then Return   ' face wall: FaceWall_ItemDblClick
        If mlList.SelectedItem Is Nothing Then Return
        If Not g_bolDualScreen AndAlso EnterFullView() Then Return   ' 單螢幕: 全圖 here (SingleScreen.vb); Dock is on the right-click menu
        mlList.SelectedItem.Marked = Not mlList.SelectedItem.Marked
    End Sub

    Private Sub mlList_ItemKeyDown(Index As Integer, e As KeyEventArgs) Handles mlList.ItemKeyDown
        If ListShowsCards Then Return
        If e.KeyCode = Keys.Enter AndAlso Not g_bolDualScreen Then   ' 單螢幕: 全圖 (SingleScreen.vb)
            If EnterFullView() Then e.Handled = True
            Return
        End If
        If e.KeyCode = Keys.Enter AndAlso m_frmViewerForm IsNot Nothing AndAlso Not m_frmViewerForm.Visible Then   ' 雙螢幕: a closed viewer back
            If ShowDualViewer() Then e.Handled = True
            Return
        End If
        If e.KeyCode <> Keys.Delete Then Return
        '移除 Photo
        Select Case m_lpAppEnv.ExeMode
            Case enumExeMode.exeAlbums
                ' VB6 only took the photo off the list (the file stayed): now it goes to the Recycle Bin,
                ' as 右鍵 刪除檔案 (which asks); the key still needs 設定's DeleteAlbumPhotos
                If Not g_lpConfig.Privilege(Config.enumPrivilege.privDeleteAlbumPhotos) Then Return
                DeletePhotoFiles(Targets(Index))   ' frmMain.FileOps.vb (all selected ones)
                Return
            Case enumExeMode.exeFavorites
                If Not frmQueryMsgBox.ShowMessage("是否確定刪除攝影集的相片？", "") Then Return
            Case Else
                Return
        End Select
        Dim lpSection As PhotoSet = CurrentSection()
        If lpSection Is Nothing Then Return

        ' VB6 removed the item first and then read Item(Index) -- i.e. the next photo's file name
        Dim strFileName As String = mlList.Item(Index).FileName
        mlList.RemoveItem(Index)
        lpSection.RemovePhoto(strFileName)
        ' VB6 then called lpSection.Save: a Book rewrites its .Alm list; a Class has no Save (the VB6
        ' call raised error 438), so for an album the photo only leaves the list, as before.
        If TypeOf lpSection Is Book Then CType(lpSection, Book).Save()
    End Sub

    Private Sub mlList_ItemMarkChanged(Index As Integer) Handles mlList.ItemMarkChanged
        If ListShowsCards Then Return   ' face cards are not photos: never docked
        If mlList.Count <= 0 Then Return
        If mlList.Item(Index).FileName.Trim() = "" Then Return

        ' VB6 took the date from the *selected* photo whenever there was one, so marking another item
        ' (double-click / the item's mark) docked it under the wrong date
        Dim lpPhoto As Photo = m_lpCurrentPhoto
        If lpPhoto Is Nothing OrElse Not String.Equals(lpPhoto.FileDesc, mlList.Item(Index).FileName, StringComparison.OrdinalIgnoreCase) Then
            lpPhoto = New Photo
            lpPhoto.Construct(mlList.Item(Index).FileName)
        End If

        If mlList.Item(Index).Marked Then
            g_lpDock.AddItem(mlList.Item(Index).FileName, lpPhoto.Exif(enumPhotoExif.peDate), lpPhoto.Exif(enumPhotoExif.peTime))
        Else
            g_lpDock.RemoveItem(mlList.Item(Index).FileName)
        End If
        g_lpDock.Save()
        SetDockProperty()
    End Sub

    Private Sub mlList_ItemMouseDown(Index As Integer, e As MouseEventArgs) Handles mlList.ItemMouseDown
        If ListShowsCards Then Return   ' face wall: FaceWall_ItemMouseDown
        If m_lpCurrentPhoto Is Nothing Then Return
        If e.Button = MouseButtons.Right Then
            If Not mlList.IsSelected(Index) Then mlList.SelectedIndex = Index   ' inside a multi-selection it stays
            SetMediaListPopupMenu(m_lpCurrentPhoto)
            mlList.PopupMenu(Index, AquaMenu1)
        Else
            AquaMenu1.CloseMenu()
        End If
    End Sub

    Private Sub mlList_ItemSelected(Index As Integer) Handles mlList.ItemSelected
        If ListShowsCards Then Return
        m_lpCurrentPhoto = New Photo
        m_lpCurrentPhoto.Construct(mlList.Item(Index).FileName)
    End Sub

    Private Sub mlList_SelectedChanged(sender As Object, e As EventArgs) Handles mlList.SelectedChanged
        If ListShowsCards Then Return
        Dim bolFirst As Boolean = (mlList.SelectedIndex = 0)
        Dim bolLast As Boolean = (mlList.SelectedIndex = mlList.Count - 1)

        If m_frmViewer IsNot Nothing Then
            If mlList.Count <= 0 Then
                m_frmViewer.Clear()
            ElseIf m_frmViewerForm.Visible AndAlso Not PlaceViewerActive Then   ' 地點 mode: the viewer follows its strip
                ' the page turns forward or back as the list moved (frmViewerLarge.PageTurn.vb)
                Dim large As frmViewerLarge = TryCast(m_frmViewer, frmViewerLarge)
                If large IsNot Nothing Then large.TurnDirection = If(mlList.SelectedIndex < m_intViewerIndex, -1, 1)
                ShowCurrentInViewer(bolFirst, bolLast)
            End If
        End If
        m_intViewerIndex = mlList.SelectedIndex
        ShowPhotoIndex()
    End Sub

    Private m_intViewerIndex As Integer = -1   ' the list position shown last (which way the page turns)

    Private Sub ShowPhotoIndex()
        lblPhotoCounts.Text = (mlList.SelectedIndex + 1) & " of " & mlList.Count & " 張相片"
    End Sub

    Private Sub SetDockProperty()
        Dim bolValue As Boolean
        If g_lpDock.Count <= 0 Then
            lblSelCount.Text = "未選取任何相片"
            bolValue = False
        Else
            lblSelCount.Text = "已選取 " & g_lpDock.Count & " 張相片 共 " & g_lpDock.FileLength & " " & g_lpDock.FileLengthUnit
            bolValue = True
        End If
        For Each i As Integer In {1, 2, 4, 8}   ' Dock, 建立攝影集, 匯出, 列印
            imgToolBox(i).Enabled = bolValue
            lblToolBox(i).Enabled = bolValue
        Next
    End Sub

    Private Sub MoveFilesToMediaList(ByVal strFileName() As String, ByVal intCount As Integer)
        ClearScreenAlbum()
        mlList.Clear()
        sliSize.Value = LimitFor(intCount)
        Application.DoEvents()

        For I As Integer = 0 To intCount - 1
            If g_lpFileSystem.FileExists(strFileName(I)) Then
                Dim lpPhoto As New Photo
                lpPhoto.Construct(strFileName(I))
                Dim lpItem As Aqua.MediaItem = mlList.AddItem(strFileName(I))
                lpItem.Ranking = RankingOf(lpPhoto)
                Application.DoEvents()   ' no scrolling to each new photo (VB6 did): only what is on screen loads
            End If
        Next

        DockedMediaList(g_lpDock, mlList)
        MoveNoteToScreen(m_lpAppEnv.ExeMode, -1, -1)
        If mlList.Count > 0 Then
            mlList.ScrollValue = mlList.ScrollMin
            mlList.SelectedIndex = 0
        End If
    End Sub

    '==================================================================================================
    ' Class / book notes (title, date, place, remark, subject icon)
    '==================================================================================================
    Private Sub ClearScreenAlbum()
        m_strShownKey = Nothing
        EndFaceView()   ' frmMain.Faces.vb
        EndCoverWall()  ' frmMain.Covers.vb
        EndBrowseView() ' frmMain.Browse.vb
        mlList.Clear()

        txtTitle.Text = ""
        txtDate.Text = ""
        txtSpot.Text = ""
        txtRemark.Text = ""
        Application.DoEvents()

        imgSubject.Image = Nothing
        imgSubject.Visible = False
        lblPhotoCounts.Text = ""
        lblFileLength.Text = ""

        SetImageButtonState(enumImageButton.ibAddition, True)
        SetImageButtonState(enumImageButton.ibModeStand, True)
        SetImageButtonState(enumImageButton.ibModeCalendar, True)
        SetImageButtonState(enumImageButton.ibModeList, True)
        SetImageButtonState(enumImageButton.ibNoteAll, False)
        SetImageButtonState(enumImageButton.ibNoteClear, False)
        SetImageButtonState(enumImageButton.ibSyncDate, False)
        SetImageButtonState(enumImageButton.ibDelete, False)
        If m_frmViewer IsNot Nothing Then m_frmViewer.Clear()
    End Sub

    Private Sub MoveNoteToScreen(ByVal enumMode As enumExeMode, ByVal iSectionIndex As Integer, ByVal iKeyIndex As Integer)
        Dim lpSection As PhotoSet = Nothing
        If iSectionIndex >= 0 AndAlso iKeyIndex >= 0 Then
            Select Case enumMode
                Case enumExeMode.exeAlbums : lpSection = g_lpStorage.Album(iSectionIndex).Item(iKeyIndex)
                Case enumExeMode.exeFavorites : lpSection = g_lpStorage.Favorite(iSectionIndex).Item(iKeyIndex)
            End Select
        End If

        If lpSection IsNot Nothing Then
            lpSection.Load()

            txtTitle.Text = lpSection.Note(enumNote.ntTitle)
            txtDate.Text = lpSection.Note(enumNote.ntDate)
            txtSpot.Text = lpSection.Note(enumNote.ntSopt)
            txtRemark.Text = lpSection.Note(enumNote.ntRemark)
            Application.DoEvents()

            lblFileLength.Text = lpSection.FileLength & " " & lpSection.FileLengthUnit
            Dim strIcon As String = lpSection.Note(enumNote.ntIcon)
            imgSubject.Tag = strIcon
            Dim lpImageList As ImageList = g_lpStorage.Config.ImageListSubject
            imgSubject.Image = If(lpImageList.Images.ContainsKey(strIcon), lpImageList.Images(strIcon), Nothing)
            imgSubject.Visible = True
            imgSubject.Refresh()
        Else
            txtTitle.Text = ""
            txtDate.Text = ""
            txtSpot.Text = ""
            txtRemark.Text = ""
            imgSubject.Image = Nothing
        End If
        ShowPhotoIndex()

        SetImageButtonState(enumImageButton.ibAddition, True)
        SetImageButtonState(enumImageButton.ibModeStand, True)
        SetImageButtonState(enumImageButton.ibModeCalendar, True)
        SetImageButtonState(enumImageButton.ibModeList, True)
        SetImageButtonState(enumImageButton.ibNoteAll, True)
        SetImageButtonState(enumImageButton.ibNoteClear, True)
        SetImageButtonState(enumImageButton.ibSyncDate, True)
        SetImageButtonState(enumImageButton.ibDelete, True)
    End Sub

    Private Sub NoteBox_KeyDown(sender As Object, e As KeyEventArgs) Handles txtTitle.KeyDown, txtDate.KeyDown, txtSpot.KeyDown, txtRemark.KeyDown
        If e.KeyCode = Keys.Return OrElse e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            DropFocus()   ' -> ExitFocus
        End If
    End Sub

    ''' <summary>VB6 txtTitle/txtDate/txtSpot/txtRemark_ExitFocus: save the note, back to Embed look.</summary>
    Private Sub NoteBox_ExitFocus(sender As Object, e As EventArgs) Handles txtTitle.ExitFocus, txtDate.ExitFocus, txtSpot.ExitFocus, txtRemark.ExitFocus
        Dim box As Aqua.TextBox = CType(sender, Aqua.TextBox)
        If box.Embed Then Return   ' already handled (Enter/Escape leave through here too)
        WriteNoteInput(box)
        box.Embed = True
        If ActiveControl Is box Then DropFocus()

        If box Is txtTitle Then
            ' VB6 tested "KeyIndex >= 0 Then GoTo ExitProcess", so the node never got the new title
            Dim lpKey As PhotoSet = CurrentSection()
            If lpKey Is Nothing Then Return
            Dim lpNode As TreeNode = FindNode(tvList, lpKey.Key)
            If lpNode IsNot Nothing Then lpNode.Text = lpKey.DisplayName
        End If
    End Sub

    Private Sub WriteNoteInput(ByVal lpControl As Aqua.TextBox)
        If m_lpAppEnv.KeyIndex < 0 Then Return
        If tvList.SelectedNode Is Nothing Then Return
        Try
            Dim lpSection As PhotoSet = CurrentSection()
            If lpSection Is Nothing OrElse lpSection.ReadOnly Then Return
            lpSection.Load()

            If lpControl Is txtTitle Then
                lpSection.Note(enumNote.ntTitle) = lpControl.Text
            ElseIf lpControl Is txtDate Then
                lpSection.Note(enumNote.ntDate) = lpControl.Text
            ElseIf lpControl Is txtSpot Then
                lpSection.Note(enumNote.ntSopt) = lpControl.Text
            ElseIf lpControl Is txtRemark Then
                lpSection.Note(enumNote.ntRemark) = lpControl.Text
            End If
        Catch
            ' VB6: On Error GoTo FailExit
        End Try
    End Sub

    '==================================================================================================
    ' Thumbnail size
    '==================================================================================================
    Private Sub sliSize_ValueChanged(sender As Object, e As EventArgs) Handles sliSize.ValueChanged
        SetBusy(True)
        mlList.Limit = sliSize.Value
        SetBusy(False)
    End Sub

    Private Sub imgLargeSize_Click(sender As Object, e As EventArgs) Handles imgLargeSize.Click
        If sliSize.Value > sliSize.Minimum Then sliSize.Value -= 1
    End Sub

    Private Sub imgSmallSize_Click(sender As Object, e As EventArgs) Handles imgSmallSize.Click
        If sliSize.Value < sliSize.Maximum Then sliSize.Value += 1
    End Sub

    '==================================================================================================
    ' Calendar mode
    '==================================================================================================
    ''' <summary>Opens the calendar on this month. (VB6 also placed pnlDateMode, picPrior/picNext/lblYear
    ''' and set the arrows' tooltips here -- all in the designer now.)</summary>
    Private Sub InitialDateControl()
        Dim intYear As Integer = Date.Now.Year
        Dim intMonth As Integer = Date.Now.Month

        lblYear.Text = CStr(intYear)
        Month1.Month = intMonth

        If Calendar1.Year = intYear Then
            Calendar1.SetYearMonth(intYear, intMonth)
            Calendar1_YearChanged(Calendar1, EventArgs.Empty)
        Else
            Calendar1.SetYearMonth(intYear, intMonth)
        End If
        lblYear_Click(lblYear, EventArgs.Empty)
    End Sub

    Private Sub lblYear_Click(sender As Object, e As EventArgs) Handles lblYear.Click
        Calendar1.Visible = False
        Month1.Visible = True
        lblYear.Text = CStr(Calendar1.Year)
    End Sub

    Private Sub Month1_DblClick(sender As Object, e As EventArgs) Handles Month1.DoubleClick
        Month1.Visible = False
        Dim bolExecute As Boolean = (Calendar1.Month = Month1.Month)
        Calendar1.Visible = True
        MoveDataToCalendar1(CInt(Val(lblYear.Text)), Month1.Month)
        lblYear.Text = Calendar1.Year & " / " & Calendar1.Month

        If bolExecute Then Calendar1_MonthChanged(Calendar1, EventArgs.Empty)
    End Sub

    Private Sub MoveDataToCalendar1(ByVal inyYear As Integer, ByVal intMonth As Integer)
        Calendar1.SetYearMonth(inyYear, intMonth)
    End Sub

    Private Sub picPrior_Click(sender As Object, e As EventArgs) Handles picPrior.Click
        If Month1.Visible Then
            '月模式
            Calendar1.Year -= 1
            lblYear.Text = CStr(Calendar1.Year)
        Else
            '日模式
            If Calendar1.Month = 1 Then
                Calendar1.SetYearMonth(Calendar1.Year - 1, 12)
            Else
                Calendar1.Month -= 1
            End If
            lblYear.Text = Calendar1.Year & " / " & Calendar1.Month
        End If
    End Sub

    Private Sub picPrior_DblClick(sender As Object, e As EventArgs) Handles picPrior.DoubleClick
        If Month1.Visible Then
            Calendar1.Year -= 4
            lblYear.Text = CStr(Calendar1.Year)
        Else
            picPrior_Click(sender, e)
        End If
    End Sub

    Private Sub picNext_Click(sender As Object, e As EventArgs) Handles picNext.Click
        If Month1.Visible Then
            Calendar1.Year += 1
            lblYear.Text = CStr(Calendar1.Year)
        Else
            If Calendar1.Month = 12 Then
                Calendar1.SetYearMonth(Calendar1.Year + 1, 1)
            Else
                Calendar1.Month += 1
            End If
            lblYear.Text = Calendar1.Year & " / " & Calendar1.Month
        End If
    End Sub

    Private Sub picNext_DblClick(sender As Object, e As EventArgs) Handles picNext.DoubleClick
        If Month1.Visible Then
            Calendar1.Year += 4
            lblYear.Text = CStr(Calendar1.Year)
        Else
            picNext_Click(sender, e)
        End If
    End Sub

    Private Sub Calendar1_YearChanged(sender As Object, e As EventArgs) Handles Calendar1.YearChanged
        If Not ServerReady() Then Return
        g_lpDatabase.SetYearChanged(Month1, Calendar1.Year, NoPhotoColor, Color.Black)
    End Sub

    Private Sub Calendar1_MonthChanged(sender As Object, e As EventArgs) Handles Calendar1.MonthChanged
        If Not ServerReady() Then Return
        g_lpDatabase.SetMonthChanged(Calendar1, Calendar1.Year, Calendar1.Month, NoPhotoColor, Color.Black)
    End Sub

    Private Sub Calendar1_Click(day As Integer) Handles Calendar1.DayClick
        With m_lpAppEnv
            .ExeMode = enumExeMode.exeCalendar
            .SectionIndex = -1
            .KeyIndex = -1
        End With

        SetBusy(True)
        Enabled = False
        Try
            Dim strFileName() As String = Nothing
            Dim intCount As Integer = g_lpDatabase.LoadDateFile(Calendar1.Year, Calendar1.Month, Calendar1.Day, strFileName)
            If intCount > 0 Then MoveFilesToMediaList(strFileName, intCount)
        Finally
            SetBusy(False)
            Enabled = True
        End Try

        If mlList.Visible Then mlList.Focus()
    End Sub

    Private Sub InitialScreenControlPrivate()
        If Not g_lpConfig.ReadOnly Then Return

        '屬性
        txtTitle.Enabled = False
        txtDate.Enabled = False
        txtSpot.Enabled = False
        txtRemark.Enabled = False
        imgSubject.Enabled = False

        '工具列
        imgToolBox(0).Enabled = False
        imgToolBox(2).Enabled = False
        imgToolBox(5).Enabled = False
    End Sub

End Class
