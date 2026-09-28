' Port of iPhoto\Form\frmSetup.frm: the settings dialog (iPhoto.Ini through g_lpConfig).
'
' Layout, pages and captions live in the designer; open the form with "Using f As New frmSetup" (it is
' filled from g_lpConfig on Load). VB6's PageHead1_SelectedChanged, which showed/hid the PageSheets by
' hand, is gone: the pages are the TabControl tabSetup's own TabPages (pageGeneral, pageAlbums,
' pageAppearance, pagePlugins); its TabStripPadding puts the tab strip where VB6's PageHead was.
' Fixed from VB6: the 附加檔案 browse buttons flashed the 應用程式 button of the same index; the
' 雙螢幕切換 check box was shown but never saved.
Friend Class frmSetup

    Private m_lpChanged As Boolean

    ''' <summary>True when the settings were saved.</summary>
    Public ReadOnly Property Changed As Boolean
        Get
            Return m_lpChanged
        End Get
    End Property

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MoveProfileToScreen()
        imgRebuild.Visible = g_lpDatabase.Implement
        m_lpChanged = False
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        g_lpConfig.Refresh()
        Close()
    End Sub

    Private Sub butSave_Click(sender As Object, e As EventArgs) Handles butSave.Click
        g_lpConfig.Privilege(Config.enumPrivilege.privDeleteAlbumPhotos) = chkPrivilege.Checked

        '幻燈片
        g_lpConfig.Slide(Config.enumSlide.sliMusicPath) = txtMusic.Text
        g_lpConfig.Slide(Config.enumSlide.sliShowTimes) = CStr(sliSlide.Value)

        '相片庫
        Dim strArray(lstAlbum.Count - 1) As String
        For I As Integer = 0 To lstAlbum.Count - 1
            strArray(I) = lstAlbum.Item(I).Value
        Next
        g_lpConfig.AddAlbumFolders(strArray)

        '攝影集
        g_lpConfig.AddFavoriteFolders({txtFavorite.Text})

        '字型
        g_lpConfig.InterfaceFontName = txtFont.Text

        '風格
        g_lpConfig.Style = If(rbStyle(0).Checked, "0", "1")

        '雙螢幕
        If chkSwitchScreen.Enabled Then g_lpConfig.SwitchScreen = chkSwitchScreen.Checked

        '音效
        g_lpConfig.Sound(Config.enumSound.snImportFinish) = txtSound(0).Text
        g_lpConfig.Sound(Config.enumSound.snExportFinish) = txtSound(1).Text
        g_lpConfig.Sound(Config.enumSound.snButtonExit) = txtSound(2).Text
        g_lpConfig.Sound(Config.enumSound.snButtonEnter) = txtSound(3).Text
        g_lpConfig.Sound(Config.enumSound.snButtonClick) = txtSound(4).Text
        g_lpConfig.Sound(Config.enumSound.snOpenDialogBox) = txtSound(4).Text   ' VB6: the dialog sound follows the click sound

        '應用程式
        g_lpConfig.Application(Config.enumApplication.AppPrint) = txtApp(0).Text
        g_lpConfig.Application(Config.enumApplication.AppImageEdit) = txtApp(1).Text
        g_lpConfig.Application(Config.enumApplication.AppVideoEdit) = txtApp(2).Text
        g_lpConfig.Application(Config.enumApplication.AppMail) = txtApp(3).Text
        g_lpConfig.Application(Config.enumApplication.AppHomePage) = txtApp(4).Text
        g_lpConfig.Application(Config.enumApplication.AppBurn) = txtApp(5).Text

        '附加檔案
        g_lpConfig.Attached(Config.enumAttachedFile.filDatabase) = txtAttached(0).Text
        g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord) = txtAttached(1).Text
        g_lpConfig.Attached(Config.enumAttachedFile.filAddressBook) = txtAttached(2).Text
        g_lpConfig.Attached(Config.enumAttachedFile.filFaceCover) = txtAttached(3).Text

        SaveFacePage()   ' frmSetup.Faces.vb
        g_lpConfig.Save()
        m_lpChanged = True
        Close()
    End Sub

    ''' <summary>VB6 "BorderStyle = 1 : Wait : BorderStyle = 0" press effect of the small browse icons.</summary>
    Private Sub Flash(ByVal box As Control, ByVal ms As Integer)
        Dim pic As PictureBox = TryCast(box, PictureBox)
        Dim icon As Aqua.IconBox = TryCast(box, Aqua.IconBox)
        If pic IsNot Nothing Then pic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        If icon IsNot Nothing Then icon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Application.DoEvents()
        Darwin.Wait(ms)
        Application.DoEvents()
        If pic IsNot Nothing Then pic.BorderStyle = System.Windows.Forms.BorderStyle.None
        If icon IsNot Nothing Then icon.BorderStyle = System.Windows.Forms.BorderStyle.None
    End Sub

    '相片庫：新增、刪除、上移、下移
    Private Sub imgAlbums_Click(sender As Object, e As EventArgs) Handles imgAlbums_0.Click, imgAlbums_1.Click, imgAlbums_2.Click, imgAlbums_3.Click
        Dim Index As Integer = Array.IndexOf(imgAlbums, sender)
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgAlbums(Index), 20)

        Dim I As Integer = lstAlbum.SelectedIndex
        Select Case Index
            Case 0
                Dim strTemp As String = frmBrowserFolder.GetFolder("存放相片庫的資料夾").Trim()
                If strTemp = "" Then Return
                For J As Integer = 0 To lstAlbum.Count - 1
                    If String.Equals(lstAlbum.Item(J).Value.Trim(), strTemp, StringComparison.OrdinalIgnoreCase) Then Return
                Next
                If I >= 0 Then lstAlbum.Item(I).Selected = False
                lstAlbum.AddItem(strTemp, strTemp)
                lstAlbum.Item(lstAlbum.Count - 1).Selected = True

            Case 1
                If lstAlbum.Count > 0 AndAlso I >= 0 Then lstAlbum.RemoveItem(I)

            Case 2
                If I <= 0 OrElse lstAlbum.Count <= 1 Then Return
                SwapAlbum(I - 1, I)
                lstAlbum.SelectedIndex = I - 1

            Case 3
                If I < 0 OrElse I >= lstAlbum.Count - 1 OrElse lstAlbum.Count <= 1 Then Return
                SwapAlbum(I + 1, I)
                lstAlbum.SelectedIndex = I + 1
        End Select
    End Sub

    Private Sub SwapAlbum(ByVal a As Integer, ByVal b As Integer)
        Dim strTemp As String = lstAlbum.Item(a).Value
        lstAlbum.Item(a).Value = lstAlbum.Item(b).Value
        lstAlbum.Item(a).Text = lstAlbum.Item(b).Text
        lstAlbum.Item(b).Value = strTemp
        lstAlbum.Item(b).Text = strTemp
    End Sub

    Private Sub imgApp_Click(sender As Object, e As EventArgs) Handles imgApp_0.Click, imgApp_1.Click, imgApp_2.Click, imgApp_3.Click, imgApp_4.Click, imgApp_5.Click
        Dim Index As Integer = Array.IndexOf(imgApp, sender)
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgApp(Index), 20)
        txtApp(Index).Text = frmBrowserFile.GetFile("選擇執行檔", "執行檔 (*.Exe)|*.Exe", True).Trim()
    End Sub

    Private Sub imgAttached_Click(sender As Object, e As EventArgs) Handles imgAttached_0.Click, imgAttached_1.Click, imgAttached_2.Click, imgAttached_3.Click
        Dim Index As Integer = Array.IndexOf(imgAttached, sender)
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgAttached(Index), 20)

        Select Case Index
            Case 0 : txtAttached(Index).Text = frmBrowserFile.GetFile("選擇資料庫檔案", "Access 資料庫 (*.Mdb)|*.Mdb", True).Trim()
            Case 1 : txtAttached(Index).Text = frmBrowserFile.GetFile("選擇關鍵字檔案", "關鍵字 (*.Kwd)|*.Kwd", True).Trim()
            Case 2 : txtAttached(Index).Text = frmBrowserFile.GetFile("選擇通訊錄", "通訊錄 (*.Adb)|*.Adb", True).Trim()
            Case 3
                Dim strTemp As String = frmBrowserFolder.GetFolder("存放面孔封面的資料夾")
                If strTemp.Trim() <> "" Then txtAttached(Index).Text = strTemp
        End Select
    End Sub

    Private Sub imgFavorites_Click(sender As Object, e As EventArgs) Handles imgFavorites.Click
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgFavorites, 20)
        Dim strTemp As String = frmBrowserFolder.GetFolder("存放攝影集的資料夾")
        If strTemp.Trim() <> "" Then txtFavorite.Text = strTemp
    End Sub

    Private Sub imgMusic_Click(sender As Object, e As EventArgs) Handles imgMusic.Click
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgMusic, 20)
        Dim strTemp As String = frmBrowserFolder.GetFolder("存放音樂檔的資料夾")
        If strTemp.Trim() <> "" Then txtMusic.Text = strTemp
    End Sub

    Private Sub imgRebuild_Click(sender As Object, e As EventArgs) Handles imgRebuild.Click
        If Not g_lpDatabase.Implement Then Return
        Application.UseWaitCursor = True
        Try
            g_lpDatabase.Rebuild(g_lpStorage)
        Finally
            Application.UseWaitCursor = False
        End Try
        frmMsgBox.ShowExclamationMessage("重建完成" & vbCrLf & "建議重新啟動以釋放資源", "")
    End Sub

    Private Sub imgSound_Click(sender As Object, e As EventArgs) Handles imgSound_0.Click, imgSound_1.Click, imgSound_2.Click, imgSound_3.Click, imgSound_4.Click
        Dim Index As Integer = Array.IndexOf(imgSound, sender)
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgSound(Index), 20)
        txtSound(Index).Text = frmBrowserFile.GetFile("選擇音效檔", "音效檔 (*.WAV)|*.WAV", True).Trim()
    End Sub

    Private Sub MoveProfileToScreen()
        '權限
        chkPrivilege.Checked = g_lpConfig.Privilege(Config.enumPrivilege.privDeleteAlbumPhotos)

        '附加檔案
        txtAttached(0).Text = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        txtAttached(1).Text = g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord)
        txtAttached(2).Text = g_lpConfig.Attached(Config.enumAttachedFile.filAddressBook)
        txtAttached(3).Text = g_lpConfig.Attached(Config.enumAttachedFile.filFaceCover)

        '幻燈片
        sliSlide.Value = Math.Max(sliSlide.Minimum, Math.Min(sliSlide.Maximum, CInt(Val(g_lpConfig.Slide(Config.enumSlide.sliShowTimes)))))
        txtMusic.Text = g_lpConfig.Slide(Config.enumSlide.sliMusicPath)

        '相片庫
        lstAlbum.Clear()
        For I As Integer = 0 To g_lpConfig.AlbumCount - 1
            lstAlbum.AddItem(g_lpConfig.AlbumPath(I), g_lpConfig.AlbumPath(I))
        Next

        '攝影集（只有一個）
        For I As Integer = 0 To g_lpConfig.FavoriteCount - 1
            txtFavorite.Text = g_lpConfig.FavoritePath(I)
        Next

        '外觀
        txtFont.Text = g_lpConfig.InterfaceFontName
        If Val(g_lpConfig.Style) = 0 Then
            rbStyle(0).Checked = True
        Else
            rbStyle(1).Checked = True
        End If

        If g_intMonitorCount > 1 Then
            chkSwitchScreen.Checked = g_lpConfig.SwitchScreen
        Else
            chkSwitchScreen.Enabled = False   ' disabled, it draws its caption grey itself
        End If

        '音效
        txtSound(0).Text = g_lpConfig.Sound(Config.enumSound.snImportFinish)
        txtSound(1).Text = g_lpConfig.Sound(Config.enumSound.snExportFinish)
        txtSound(2).Text = g_lpConfig.Sound(Config.enumSound.snButtonExit)
        txtSound(3).Text = g_lpConfig.Sound(Config.enumSound.snButtonEnter)
        txtSound(4).Text = g_lpConfig.Sound(Config.enumSound.snButtonClick)

        '外掛應用程式
        txtApp(0).Text = g_lpConfig.Application(Config.enumApplication.AppPrint)
        txtApp(1).Text = g_lpConfig.Application(Config.enumApplication.AppImageEdit)
        txtApp(2).Text = g_lpConfig.Application(Config.enumApplication.AppVideoEdit)
        txtApp(3).Text = g_lpConfig.Application(Config.enumApplication.AppMail)
        txtApp(4).Text = g_lpConfig.Application(Config.enumApplication.AppHomePage)
        txtApp(5).Text = g_lpConfig.Application(Config.enumApplication.AppBurn)
    End Sub

End Class
