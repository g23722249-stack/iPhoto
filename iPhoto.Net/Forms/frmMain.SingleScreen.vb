' 單螢幕 (new in the .NET port; one screen, or 設定 › 螢幕 單螢幕 -- g_bolDualScreen False): 全圖 in a
' window of its own instead of a viewer on another screen. Design: https://claude.ai/artifact/XkWG38HChmXYrrLqobYBRk
'   縮圖 | 全圖   a switch left of the search box. 全圖 opens frmFullView -- an Aqua iForm window like
'                地點, maximised over this one -- holding the viewer (the same frmViewerLarge as on a
'                second screen, so editing, 資訊, 面孔, the page turn all stay). Double-click a photo or
'                press Enter for 全圖; Esc, its red button or 縮圖 goes back.
'   the strip     the viewer's thumbnail strip (frmViewerLarge.Strip.vb) holds this list's photos, a
'                section per day; it moves with this list's selection both ways. Dragged to any edge it
'                stays there (Config.ViewerStripDockSingle, default the top; the 地點 strip below too).
'   right click   on the photo: the one-photo menu (frmMain.PlaceViewer.vb).
' 地點 (the 地點 window open, frmMain.PlaceViewer.vb): the same 全圖瀏覽 window holds the 地點 strip (with its
' time line) over the 地點 window -- a click on the map, its tree or 「在 iPhoto 顯示這一天」 opens it; Esc,
' the red button or the 地點 window's 地圖 | 全圖 switch goes back to the map with the strip kept; closing
' the 地點 window ends it. 全圖 in this window meanwhile closes the 地點 window first.
Partial Class frmMain

    Private WithEvents butView As Aqua.Buttons
    Private m_frmFullView As frmFullView
    Private m_bolFullView As Boolean
    Private m_strStripSignature As String = ""

    ''' <summary>The 單螢幕 全圖 viewer (Nothing on two screens, or with the small viewer).</summary>
    Private ReadOnly Property InWindowViewer As frmViewerLarge
        Get
            If g_bolDualScreen Then Return Nothing
            Return TryCast(m_frmViewer, frmViewerLarge)
        End Get
    End Property

    Private Sub SingleScreen_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If g_bolDualScreen Then Return
        ' after Form_Load (CreateMultiMonters made the viewer)
        BeginInvoke(Sub() SetUpSingleScreen())
    End Sub

    Private Sub SetUpSingleScreen()
        If InWindowViewer Is Nothing Then Return   ' a screen under 1280 wide: the small viewer, shown as before
        butView = New Aqua.Buttons With {.Font = New Font("華康細圓體", 12.0F), .BackColor = butMode.BackColor, .ForeColor = butMode.ForeColor,
                                         .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Size = New Size(150, 25)}
        butView.Items.Add(New Aqua.Buttons.ButtonItem("縮圖", Nothing))
        butView.Items.Add(New Aqua.Buttons.ButtonItem("全圖", Nothing))
        butView.SelectedIndex = 0
        butView.Location = New Point(iTextBox1.Left - butView.Width - 14, iTextBox1.Top - 1)
        Controls.Add(butView)
        butView.BringToFront()
        vb6ToolTip.SetToolTip(butView, "縮圖：照片排成縮圖　全圖：開全圖瀏覽視窗看大圖（按兩下照片、Enter；Esc 回來）")
        ' the face guide link sat just left of the search box: now left of the switch
        If lblFaceGuide IsNot Nothing Then lblFaceGuide.Left = butView.Left - lblFaceGuide.PreferredWidth - 12
        FitFaceScanLabel()   ' and the face label left of those (frmMain.Faces.vb)

        Dim v As frmViewerLarge = InWindowViewer
        v.StripSingleScreen = True
        AddHandler v.StripPhotoSelected, AddressOf FullStrip_Selected
        AddHandler v.CloseRequested, Sub() FullView_CloseRequested()
        AddHandler Application.Idle, AddressOf FullView_Idle
    End Sub

    Private Sub SingleScreen_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        RemoveHandler Application.Idle, AddressOf FullView_Idle
    End Sub

    Private Sub butView_SelectedChanged(sender As Object, e As EventArgs) Handles butView.SelectedChanged
        If butView.SelectedIndex = 1 Then
            If Not EnterFullView() Then butView.SelectedIndex = 0
        ElseIf m_bolFullView Then
            LeaveFullView()
        End If
    End Sub

    ''' <summary>全圖 (double-click, Enter, the switch, 全圖瀏覽 on the menu). False when the list has no
    ''' photos to show (empty, or the face wall / album covers).</summary>
    Private Function EnterFullView() As Boolean
        Dim v As frmViewerLarge = InWindowViewer
        If v Is Nothing OrElse ListShowsCards OrElse mlList.Count = 0 Then Return False
        If PlaceViewerActive Then m_frmMap.Close()   ' 地點 mode ends: the viewer is this list's again
        If mlList.SelectedIndex < 0 Then mlList.SelectedIndex = 0
        EnsureFullViewWindow()
        EnsurePlacePhotoMenu()   ' the one-photo right-click menu (frmMain.PlaceViewer.vb)
        v.PhotoMenu = m_mnuPlacePhoto
        m_bolFullView = True
        v.Show()
        m_frmFullView.ShowOver(Me)
        FillFullStrip()
        ShowCurrentInViewer(mlList.SelectedIndex = 0, mlList.SelectedIndex = mlList.Count - 1)
        If butView IsNot Nothing AndAlso butView.SelectedIndex <> 1 Then butView.SelectedIndex = 1
        v.Focus()
        Return True
    End Function

    Private Sub LeaveFullView()
        If Not m_bolFullView Then Return
        m_bolFullView = False
        Dim v As frmViewerLarge = InWindowViewer
        If v IsNot Nothing Then
            v.HideStrip()
            v.Clear()
        End If
        m_frmFullView?.Hide()
        If butView IsNot Nothing AndAlso butView.SelectedIndex <> 0 Then butView.SelectedIndex = 0
        Activate()
        If mlList.SelectedIndex >= 0 Then mlList.EnsureVisible(mlList.SelectedIndex)
        mlList.Focus()
    End Sub

    ''' <summary>The 全圖瀏覽 window, made once: the viewer goes into it.</summary>
    Private Sub EnsureFullViewWindow()
        If m_frmFullView IsNot Nothing Then Return
        m_frmFullView = New frmFullView(InWindowViewer)
        AddHandler m_frmFullView.CloseRequested, Sub() FullView_CloseRequested()
    End Sub

    ''' <summary>Esc in the viewer, the 全圖瀏覽 window's red button: back to the map in 地點 mode, else to 縮圖.</summary>
    Private Sub FullView_CloseRequested()
        If m_bolPlaceFull Then HidePlaceFull() Else LeaveFullView()
    End Sub

    '==================================================================================================
    ' 地點 (單螢幕)
    '==================================================================================================
    Private WithEvents butMapView As Aqua.Buttons
    Private m_bolPlaceFull As Boolean

    ''' <summary>From BeginPlaceViewer, the 地點 window just shown: 地點 mode, and its 地圖 | 全圖 switch.</summary>
    Private Sub BeginPlaceSingle()
        Dim v As frmViewerLarge = InWindowViewer
        If v Is Nothing Then Return   ' the small viewer: picked photos go into this window's list, as before
        If m_bolFullView Then LeaveFullView()
        m_bolPlaceMode = True
        EnsureFullViewWindow()
        EnsurePlacePhotoMenu()
        v.PhotoMenu = m_mnuPlacePhoto
        v.HideStrip()
        v.Clear()
        AddHandler v.StripPhotoSelected, AddressOf PlaceStrip_Selected
        AddHandler m_frmMap.FormClosed, AddressOf PlaceMap_Closed

        ' 地圖 | 全圖 at the right of the 地點 window's title bar (as 縮圖 | 全圖 in this one)
        butMapView = New Aqua.Buttons With {.Font = New Font("華康細圓體", 12.0F), .BackColor = butMode.BackColor, .ForeColor = butMode.ForeColor,
                                            .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Size = New Size(150, 25)}
        butMapView.Items.Add(New Aqua.Buttons.ButtonItem("地圖", Nothing))
        butMapView.Items.Add(New Aqua.Buttons.ButtonItem("全圖", Nothing))
        butMapView.SelectedIndex = 0
        butMapView.Location = New Point(m_frmMap.ClientSize.Width - butMapView.Width - 14, iTextBox1.Top - 1)
        m_frmMap.Controls.Add(butMapView)
        butMapView.BringToFront()
        vb6ToolTip.SetToolTip(butMapView, "地圖：選地點　全圖：看選到的照片（點地圖上的照片也會開；Esc 回地圖）")
    End Sub

    Private Sub butMapView_SelectedChanged(sender As Object, e As EventArgs) Handles butMapView.SelectedChanged
        If butMapView.SelectedIndex = 1 Then
            If m_bolPlaceFull Then Return   ' set by ShowPlaceFull itself (the strip is filled after)
            ' nothing picked yet: stay on the map
            If PlaceViewer Is Nothing OrElse PlaceViewer.StripCount = 0 Then butMapView.SelectedIndex = 0 Else ShowPlaceFull()
        ElseIf m_bolPlaceFull Then
            HidePlaceFull()
        End If
    End Sub

    ''' <summary>The 全圖瀏覽 window over the 地點 window (ShowInPlaceViewer, the switch).</summary>
    Private Sub ShowPlaceFull()
        Dim v As frmViewerLarge = InWindowViewer
        If v Is Nothing OrElse m_frmMap Is Nothing OrElse m_frmMap.IsDisposed Then Return
        m_bolPlaceFull = True
        v.Show()
        m_frmFullView.Text = "全圖瀏覽　－　地點"
        m_frmFullView.ShowOver(Me)   ' (activated: over the 地點 window)
        If butMapView IsNot Nothing AndAlso butMapView.SelectedIndex <> 1 Then butMapView.SelectedIndex = 1
        v.Focus()
        ' a click in the 地點 window (its tree, the map) activates that window again when it ends: once more after it
        BeginInvoke(Sub()
                        If Not m_bolPlaceFull OrElse m_frmFullView Is Nothing OrElse Not m_frmFullView.Visible Then Return
                        m_frmFullView.Activate()
                        v.Focus()
                    End Sub)
    End Sub

    ''' <summary>Back to the map; the strip stays for the next 全圖.</summary>
    Private Sub HidePlaceFull()
        If Not m_bolPlaceFull Then Return
        m_bolPlaceFull = False
        m_frmFullView?.Hide()
        If butMapView IsNot Nothing AndAlso Not butMapView.IsDisposed AndAlso butMapView.SelectedIndex <> 0 Then butMapView.SelectedIndex = 0
        If m_frmMap IsNot Nothing AndAlso Not m_frmMap.IsDisposed Then m_frmMap.Activate()
    End Sub

    ''' <summary>From PlaceMap_Closed: the 全圖瀏覽 window back to this window's 全圖.</summary>
    Private Sub EndPlaceSingle()
        HidePlaceFull()
        butMapView = Nothing   ' it went with the 地點 window
        InWindowViewer?.Clear()
        If m_frmFullView IsNot Nothing Then m_frmFullView.Text = "全圖瀏覽"
        Activate()
    End Sub

    ''' <summary>The strip: this list's photos, a section per day (dates from the photo index).</summary>
    Private Sub FillFullStrip()
        Dim v As frmViewerLarge = InWindowViewer
        If v Is Nothing Then Return
        Dim files As New List(Of String)
        For i = 0 To mlList.Count - 1
            files.Add(mlList.Item(i).FileName)
        Next
        Dim byFile As New Dictionary(Of String, Database.IndexRow)(StringComparer.OrdinalIgnoreCase)
        If g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement Then
            For Each r In g_lpDatabase.RowsOf(files)
                byFile(r.FileName) = r
            Next
        End If
        ' the list's own order; a new section where the day changes
        Dim groups As New List(Of (Title As String, Files As List(Of String), Rankings As List(Of Integer)))
        Dim lastDay As String = Nothing
        Dim dayRows As New List(Of Database.IndexRow)
        Dim flush = Sub()
                        If dayRows.Count = 0 Then Return
                        groups.Add((DayTitle(dayRows, True, 0, False), dayRows.Select(Function(x) x.FileName).ToList(), dayRows.Select(Function(x) x.Ranking).ToList()))
                        dayRows = New List(Of Database.IndexRow)
                    End Sub
        For Each f In files
            Dim r As Database.IndexRow = Nothing
            If Not byFile.TryGetValue(f, r) Then r = New Database.IndexRow With {.FileName = f, .DateText = "00000000"}
            If lastDay IsNot Nothing AndAlso r.DateText <> lastDay Then flush()
            ' the list's own rating (it may be newer than the index)
            dayRows.Add(New Database.IndexRow With {.FileName = f, .DateText = r.DateText, .Ranking = r.Ranking})
            lastDay = r.DateText
        Next
        flush()
        Dim title As String = If(txtTitle.Text.Trim() <> "", txtTitle.Text.Trim(), If(tvList.SelectedNode?.Text, ""))
        v.ShowStrip(title & "　" & files.Count.ToString("#,0") & " 張", groups, Math.Max(0, mlList.SelectedIndex))
        m_strStripSignature = StripSignature()
    End Sub

    Private Function StripSignature() As String
        If mlList.Count = 0 Then Return "0"
        Return mlList.Count & "|" & mlList.Item(0).FileName & "|" & mlList.Item(mlList.Count - 1).FileName
    End Function

    ''' <summary>The list was filled again while in 全圖 (another album picked with 顯示相簿, a photo
    ''' deleted, ...): the strip follows; a face wall / covers / an empty list ends 全圖.</summary>
    Private Sub FullView_Idle(sender As Object, e As EventArgs)
        If Not m_bolFullView Then Return
        If ListShowsCards OrElse mlList.Count = 0 Then
            LeaveFullView()
            Return
        End If
        If StripSignature() = m_strStripSignature Then Return
        FillFullStrip()
        If mlList.SelectedIndex < 0 Then mlList.SelectedIndex = 0 Else ShowCurrentInViewer(mlList.SelectedIndex = 0, mlList.SelectedIndex = mlList.Count - 1)
    End Sub

    ''' <summary>A photo of the strip clicked: this list moves there (which shows it in the viewer).</summary>
    Private Sub FullStrip_Selected(ByVal index As Integer)
        If Not m_bolFullView OrElse index < 0 OrElse index >= mlList.Count Then Return
        If index <> mlList.SelectedIndex Then mlList.SelectedIndex = index
    End Sub

    ''' <summary>This list moved (the viewer's prior / next, a key): the strip follows.</summary>
    Private Sub FullView_SelectedChanged(sender As Object, e As EventArgs) Handles mlList.SelectedChanged
        If Not m_bolFullView OrElse ListShowsCards Then Return
        InWindowViewer?.SelectStrip(mlList.SelectedIndex)
    End Sub

End Class
