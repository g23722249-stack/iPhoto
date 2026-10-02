' 地點全圖瀏覽 (new in the .NET port; two screens): while the 地點 window (frmMap) is open, the viewer on
' the other screen is in 地點 mode -- its thumbnail strip (frmViewerLarge.Strip.vb) holds the photos sent
' from the 地點 window (a place of its tree, photos picked on the map, the day of a marker), and the
' photo follows the strip, not this window's list (which is left as it was). Its right-click menu has
' the one-photo items of the list's: 照片資訊, 以檔案總管開啟, 複製, 設定拍攝地點, 指定人物, 我的評價,
' 左轉 / 右轉, 恢復到最初狀態, 加入 / 移出 Dock, 刪除檔案. Closing the 地點 window ends the mode: the
' strip goes and the viewer shows this window's photo again.
' The 地點 window opens maximised on this window's screen (it covers it); the two windows swap screens
' like the main window and the viewer (DualScreenSwap).
' One screen: the same, with the viewer in its own 全圖瀏覽 window over the 地點 window instead of on
' another screen (frmMain.SingleScreen.vb: ShowPlaceFull / HidePlaceFull, the 地圖 | 全圖 switch).
Partial Class frmMain

    Private m_mnuPlacePhoto As ContextMenuStrip
    Private m_mnuDock As ToolStripMenuItem, m_mnuRevert As ToolStripMenuItem, m_mnuCopy As ToolStripMenuItem

    ''' <summary>True while the viewer is in 地點 mode (the 地點 window open).</summary>
    Private ReadOnly Property PlaceViewerActive As Boolean
        Get
            Return m_bolPlaceMode AndAlso PlaceViewer IsNot Nothing AndAlso m_frmMap IsNot Nothing AndAlso Not m_frmMap.IsDisposed
        End Get
    End Property

    Private m_bolPlaceMode As Boolean

    ''' <summary>The viewer whose strip and right-click menu this file drives: the second-screen viewer
    ''' (雙螢幕), or the 全圖瀏覽 window's (單螢幕, SingleScreen.vb) while it shows 全圖 or 地點 is open.</summary>
    Private ReadOnly Property PlaceViewer As frmViewerLarge
        Get
            If g_bolDualScreen OrElse m_bolFullView OrElse m_bolPlaceMode Then Return TryCast(m_frmViewer, frmViewerLarge)
            Return Nothing
        End Get
    End Property

    Private Const PlaceViewerKey As String = "PlaceViewer"
    Private m_placeSwap As DualScreenSwap
    Private m_scrViewerHome As Screen   ' where the viewer was before 地點 mode (it goes back there)

    ''' <summary>Called by ShowMap right after the 地點 window is shown: it goes on this window's screen
    ''' (or the viewer's, when it was left there last time: the two had swapped), maximised; the viewer
    ''' goes into 地點 mode on the other screen.</summary>
    Private Sub BeginPlaceViewer()
        If Not g_bolDualScreen Then
            BeginPlaceSingle()   ' 單螢幕: 全圖 in its own window over the 地點 window (SingleScreen.vb)
            Return
        End If
        Dim v As frmViewerLarge = PlaceViewer
        If v Is Nothing Then Return
        m_bolPlaceMode = True
        EnsurePlacePhotoMenu()
        v.PhotoMenu = m_mnuPlacePhoto
        AddHandler v.StripPhotoSelected, AddressOf PlaceStrip_Selected
        AddHandler m_frmMap.FormClosing, AddressOf PlaceMap_Closing
        AddHandler m_frmMap.FormClosed, AddressOf PlaceMap_Closed
        If Not m_frmViewerForm.Visible Then m_frmViewerForm.Show()
        v.Clear()

        Dim mainScr As Screen = Screen.FromControl(Me)
        m_scrViewerHome = Screen.FromControl(m_frmViewerForm)
        Dim target As Screen = WindowPlacement.SavedScreen(DualScreenSwap.PlaceMapKey)
        If target Is Nothing OrElse Not (SameDevice(target, mainScr) OrElse SameDevice(target, m_scrViewerHome)) Then target = mainScr
        If m_screenSwap IsNot Nothing Then m_screenSwap.Paused = True   ' the main pair stays put while this pair moves
        Dim map As frmMap = m_frmMap
        ' after the window hook has put it on this window's screen (ChildWindowsFollowMain)
        BeginInvoke(Sub()
                        If map.IsDisposed Then Return
                        WindowPlacement.MoveToScreen(map, target, maximize:=True)
                        If SameDevice(target, m_scrViewerHome) Then WindowPlacement.MoveToScreen(m_frmViewerForm, mainScr, maximize:=True)
                        m_placeSwap = New DualScreenSwap(map, m_frmViewerForm, DualScreenSwap.PlaceMapKey, PlaceViewerKey)
                    End Sub)
    End Sub

    Private Shared Function SameDevice(ByVal a As Screen, ByVal b As Screen) As Boolean
        Return a IsNot Nothing AndAlso b IsNot Nothing AndAlso String.Equals(a.DeviceName, b.DeviceName, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub PlaceMap_Closing(sender As Object, e As FormClosingEventArgs)
        WindowPlacement.Save(DualScreenSwap.PlaceMapKey, CType(sender, Form))
    End Sub

    Private Sub PlaceMap_Closed(sender As Object, e As FormClosedEventArgs)
        Dim v As frmViewerLarge = PlaceViewer   ' before 地點 mode ends (單螢幕: PlaceViewer needs it)
        m_bolPlaceMode = False
        m_placeSwap?.Dispose()
        m_placeSwap = Nothing
        If v Is Nothing Then Return
        RemoveHandler v.StripPhotoSelected, AddressOf PlaceStrip_Selected
        v.PhotoMenu = Nothing
        v.HideStrip()
        If Not g_bolDualScreen Then
            EndPlaceSingle()   ' the 全圖瀏覽 window goes; the viewer waits for this window's 全圖
            Return
        End If
        ' the viewer back on its own screen (it may have swapped with the 地點 window), the main pair live again
        If m_scrViewerHome IsNot Nothing AndAlso Not SameDevice(Screen.FromControl(m_frmViewerForm), m_scrViewerHome) Then
            WindowPlacement.MoveToScreen(m_frmViewerForm, m_scrViewerHome, maximize:=True)
        End If
        m_screenSwap?.Resume()
        ' back to following this window's list
        If m_lpCurrentPhoto IsNot Nothing AndAlso mlList.SelectedIndex >= 0 Then
            ShowCurrentInViewer(mlList.SelectedIndex = 0, mlList.SelectedIndex = mlList.Count - 1)
        Else
            v.Clear()
        End If
    End Sub

    ''' <summary>Photos for the strip, a section per day: "2024年3月16日 星期六" + "8 張".</summary>
    Private Sub ShowInPlaceViewer(ByVal title As String, ByVal rows As List(Of Database.IndexRow), Optional ByVal selectFile As String = Nothing)
        Dim v As frmViewerLarge = PlaceViewer
        If v Is Nothing OrElse rows.Count = 0 Then Return
        ' a day per section, oldest day first (設定 › 全圖瀏覽 can turn it round); within a day as the index has them
        rows = rows.OrderBy(Function(r) r.DateText).ThenBy(Function(r) r.FileName, StringComparer.OrdinalIgnoreCase).ToList()
        If g_lpConfig.ViewerStripNewestFirst Then
            rows = rows.GroupBy(Function(r) r.DateText).Reverse().SelectMany(Function(g) g).ToList()
        End If
        Dim groups = rows.GroupBy(Function(r) r.DateText).
                          Select(Function(g) (DayTitle(g.ToList(), True, 0, False), g.Select(Function(r) r.FileName).ToList(), g.Select(Function(r) r.Ranking).ToList())).ToList()
        Dim at As Integer = If(selectFile Is Nothing, 0, Math.Max(0, rows.FindIndex(Function(r) String.Equals(r.FileName, selectFile, StringComparison.OrdinalIgnoreCase))))
        ' with the dates: the time line above the strip (a place may have years of photos)
        If Not g_bolDualScreen Then ShowPlaceFull()   ' 單螢幕: the 全圖瀏覽 window over the 地點 window
        If g_bolDualScreen AndAlso Not m_frmViewerForm.Visible Then m_frmViewerForm.Show()   ' closed meanwhile: back
        v.ShowStrip(title & "　" & rows.Count.ToString("#,0") & " 張", groups, at, rows.Select(Function(r) r.ShotDate).ToList())
        ShowStripPhoto(at, 0)
    End Sub

    Private Sub PlaceStrip_Selected(ByVal index As Integer)
        Dim v As frmViewerLarge = PlaceViewer
        ShowStripPhoto(index, If(v Is Nothing, 0, If(index < m_intStripShown, -1, 1)))
    End Sub

    Private m_intStripShown As Integer = -1

    ''' <summary>The strip's photo <paramref name="index"/> in the viewer (turning the page forward / back).</summary>
    Private Sub ShowStripPhoto(ByVal index As Integer, ByVal direction As Integer)
        Dim v As frmViewerLarge = PlaceViewer
        If v Is Nothing Then Return
        Dim file As String = v.StripFile(index)
        If file = "" Then Return
        m_intStripShown = index
        If direction <> 0 Then v.TurnDirection = direction
        Dim first As Boolean = index = 0, last As Boolean = index = v.StripCount - 1
        Select Case GetMediaType(g_lpFileSystem, file)
            Case enumPhotoMediaType.mdImage : ShowPictureInViewer(file, first, last)
            Case enumPhotoMediaType.mdVideo : m_frmViewer.ShowVideo(file, first, last)
            Case Else : m_frmViewer.Clear()
        End Select
    End Sub

    ''' <summary>The viewer's prior / next in 地點 mode go through the strip; True when it took the step.</summary>
    Private Function PlaceStripStep(ByVal delta As Integer) As Boolean
        If Not PlaceViewerActive Then Return False
        Dim v As frmViewerLarge = PlaceViewer
        Dim i As Integer = v.StripIndex + delta
        If i < 0 OrElse i >= v.StripCount Then Return True
        v.SelectStrip(i)
        ShowStripPhoto(i, delta)
        Return True
    End Function

    '==================================================================================================
    ' The right-click menu of the photo
    '==================================================================================================
    Private Sub EnsurePlacePhotoMenu()
        If m_mnuPlacePhoto IsNot Nothing Then Return
        m_mnuPlacePhoto = New ContextMenuStrip With {.Font = New Font("Microsoft JhengHei UI", 10.5F)}
        With m_mnuPlacePhoto.Items
            .Add("照片資訊…", Nothing, Sub() PlacePhotoInfo())
            .Add("以檔案總管開啟", Nothing, Sub() ShowInExplorer(PlaceFile))
            m_mnuCopy = New ToolStripMenuItem("複製", Nothing, Sub() PlaceCopy())
            .Add(m_mnuCopy)
            .Add(New ToolStripSeparator())
            .Add("設定拍攝地點…", Nothing, Sub() PlaceSetPlace())
            .Add("指定人物…", Nothing, Sub() PlaceAssignPeople())
            Dim rank As New ToolStripMenuItem("我的評價")
            rank.DropDownItems.Add("沒有評價", Nothing, Sub() PlaceRank(0))
            For k = 1 To 5
                Dim level As Integer = k
                rank.DropDownItems.Add(New String("★"c, k), Nothing, Sub() PlaceRank(level))
            Next
            .Add(rank)
            .Add(New ToolStripSeparator())
            .Add("左轉 90°", Nothing, Sub() PlaceRotate(-90))
            .Add("右轉 90°", Nothing, Sub() PlaceRotate(90))
            m_mnuRevert = New ToolStripMenuItem("恢復到最初狀態", Nothing, Sub() PlaceRevert())
            .Add(m_mnuRevert)
            .Add(New ToolStripSeparator())
            m_mnuDock = New ToolStripMenuItem("加入 Dock", Nothing, Sub() PlaceToggleDock())
            .Add(m_mnuDock)
            .Add("刪除檔案…", Nothing, Sub() PlaceDelete())
        End With
        AddHandler m_mnuPlacePhoto.Opening, Sub(s, e)
                                                Dim f As String = PlaceFile
                                                If f = "" Then
                                                    e.Cancel = True
                                                    Return
                                                End If
                                                Dim ro As Boolean = g_lpConfig.ReadOnly
                                                Dim picture As Boolean = GetMediaType(g_lpFileSystem, f) = enumPhotoMediaType.mdImage
                                                Dim p As New Photo
                                                p.Construct(f)
                                                m_mnuCopy.Enabled = picture
                                                m_mnuRevert.Enabled = Not ro AndAlso g_lpFileSystem.FileExists(p.RestoreFile)
                                                m_mnuDock.Text = If(g_lpDock.Selected(f), "移出 Dock", "加入 Dock")
                                                For Each it As ToolStripItem In m_mnuPlacePhoto.Items
                                                    Select Case it.Text
                                                        Case "設定拍攝地點…", "指定人物…", "我的評價", "刪除檔案…" : it.Enabled = Not ro
                                                        Case "左轉 90°", "右轉 90°" : it.Enabled = Not ro AndAlso picture
                                                    End Select
                                                Next
                                            End Sub
    End Sub

    ''' <summary>The photo the viewer shows in 地點 mode ("" when none).</summary>
    Private ReadOnly Property PlaceFile As String
        Get
            Dim v As frmViewerLarge = PlaceViewer
            If v Is Nothing Then Return ""
            Return v.StripFile(v.StripIndex)
        End Get
    End Property

    ''' <summary>Dialogs open on the viewer's screen.</summary>
    Private ReadOnly Property PlaceOwner As IWin32Window
        Get
            If m_frmFullView IsNot Nothing AndAlso m_frmFullView.Visible Then Return m_frmFullView   ' 單螢幕
            If Not g_bolDualScreen Then Return If(m_frmMap IsNot Nothing AndAlso Not m_frmMap.IsDisposed AndAlso m_frmMap.Visible, CType(m_frmMap, IWin32Window), Me)
            Return If(m_frmViewerForm IsNot Nothing AndAlso m_frmViewerForm.Visible, m_frmViewerForm, Me)
        End Get
    End Property

    ''' <summary>The photo again in the viewer and the strip (after it changed on disk or in its .Exif).</summary>
    Private Sub PlaceReshow()
        Dim v As frmViewerLarge = PlaceViewer
        If v Is Nothing Then Return
        v.RefreshStripItem(v.StripIndex)
        ShowStripPhoto(v.StripIndex, 0)
    End Sub

    Private Sub PlacePhotoInfo()
        Dim f As String = PlaceFile
        If f = "" Then Return
        Dim p As New Photo
        p.Construct(f)
        Using dlg As New frmPhotoInfo
            dlg.ShowPhotoInfo(p)
        End Using
        PlaceAfterChange(f)
    End Sub

    Private Sub PlaceCopy()
        Dim f As String = PlaceFile
        If f = "" OrElse GetMediaType(g_lpFileSystem, f) <> enumPhotoMediaType.mdImage Then Return
        Using bmp As Bitmap = LoadPicture(f)
            If bmp Is Nothing Then Return
            Clipboard.Clear()
            Clipboard.SetImage(bmp)
        End Using
    End Sub

    Private Sub PlaceSetPlace()
        Dim f As String = PlaceFile
        If f = "" Then Return
        Dim r As PlacePick.Result = frmPlacePicker.PickAndApply(PlaceOwner, New List(Of String) From {f})
        If r IsNot Nothing AndAlso r.Written > 0 Then RefreshPlaceNode()   ' the 地點 window's tree and markers
    End Sub

    Private Sub PlaceAssignPeople()
        Dim f As String = PlaceFile
        If f = "" Then Return
        If frmAssignPeople.Assign(PlaceOwner, New List(Of String) From {f}) Then ShowStripPhoto(PlaceViewer.StripIndex, 0)   ' the faces' names
    End Sub

    Private Sub PlaceRank(ByVal level As Integer)
        Dim f As String = PlaceFile
        If f = "" OrElse g_lpConfig.ReadOnly Then Return
        Dim p As New Photo
        p.Construct(f)
        p.Exif(enumPhotoExif.peRanking) = CStr(level)
        g_lpDatabase.AddItem(p)
        PlaceViewer.SetStripRanking(PlaceViewer.StripIndex, level)
        PlaceAfterChange(f)
    End Sub

    Private Sub PlaceRotate(ByVal angle As Long)
        Dim f As String = PlaceFile
        If f = "" OrElse g_lpConfig.ReadOnly Then Return
        m_frmViewer.Clear()   ' let go of the picture first
        Dim p As New Photo
        p.Construct(f)
        If Not p.Backup() OrElse Not RotatePicture(f, angle) Then frmMsgBox.ShowCriticalMessage("旋轉照片失敗...", "")
        PlaceReshow()
        PlaceAfterChange(f, thumbnail:=True)
    End Sub

    Private Sub PlaceRevert()
        Dim f As String = PlaceFile
        If f = "" OrElse g_lpConfig.ReadOnly Then Return
        Dim p As New Photo
        p.Construct(f)
        If Not g_lpFileSystem.FileExists(p.RestoreFile) Then Return
        m_frmViewer.Clear()
        If Not g_lpFileSystem.CopyFile(p.RestoreFile, f, True) Then
            frmMsgBox.ShowCriticalMessage("復原失敗...", "")
        Else
            g_lpFileSystem.DeleteFile(p.RestoreFile)
        End If
        PlaceReshow()
        PlaceAfterChange(f, thumbnail:=True)
    End Sub

    Private Sub PlaceToggleDock()
        Dim f As String = PlaceFile
        If f = "" Then Return
        Dim i As Integer = MainListIndexOf(f)
        If g_lpDock.Selected(f) Then
            If i >= 0 Then mlList.Item(i).Marked = False Else g_lpDock.RemoveItem(f)   ' a list item: mlList_ItemMarkChanged docks it
        Else
            If i >= 0 Then
                mlList.Item(i).Marked = True
            Else
                Dim p As New Photo
                p.Construct(f)
                g_lpDock.AddItem(f, p.Exif(enumPhotoExif.peDate), p.Exif(enumPhotoExif.peTime))
            End If
        End If
        SetDockProperty()
    End Sub

    Private Sub PlaceDelete()
        Dim f As String = PlaceFile
        If f = "" OrElse g_lpConfig.ReadOnly Then Return
        Dim what As String = If(GetMediaType(g_lpFileSystem, f) = enumPhotoMediaType.mdVideo, "這段影片", "這張照片")
        If Not frmQueryMsgBox.ShowMessage("是否確定刪除" & what & "？會移到資源回收筒。" & vbCrLf & IO.Path.GetFileName(f), "刪除檔案") Then Return
        Dim v As frmViewerLarge = PlaceViewer
        Dim index As Integer = v.StripIndex
        m_frmViewer.Clear()
        ' the main list lets go of it too
        Dim i As Integer = MainListIndexOf(f)
        If i >= 0 Then mlList.RemoveItem(i)
        v.RemoveStripItem(index)
        If Not PhotoFiles.DeletePhoto(f, False) Then
            frmMsgBox.ShowCriticalMessage("刪除失敗（檔案可能正在使用中）" & vbCrLf & f, "刪除檔案")
        Else
            g_lpFaces?.RequestOrganizeSoon()
            g_lpDock.RemoveItem(f)
            SetDockProperty()
            RefreshPlaceNode()
        End If
        If v.StripCount > 0 Then
            Dim nextIndex As Integer = Math.Min(index, v.StripCount - 1)
            v.SelectStrip(nextIndex)
            ShowStripPhoto(nextIndex, 0)
        End If
    End Sub

    Private Function MainListIndexOf(ByVal file As String) As Integer
        For i = 0 To mlList.Count - 1
            If String.Equals(mlList.Item(i).FileName, file, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>The main list's item of the photo follows (rating, thumbnail).</summary>
    Private Sub PlaceAfterChange(ByVal file As String, Optional ByVal thumbnail As Boolean = False)
        Dim i As Integer = MainListIndexOf(file)
        If i < 0 Then Return
        Dim p As New Photo
        p.Construct(file)
        mlList.Item(i).Ranking = RankingOf(p)
        If thumbnail Then
            mlList.Item(i).FileName = ""
            mlList.Item(i).FileName = file
        End If
        If i = mlList.SelectedIndex Then m_lpCurrentPhoto = p
    End Sub

End Class
