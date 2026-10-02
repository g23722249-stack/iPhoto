Imports System.Threading.Tasks

' 設定 › 地點 (new in the .NET port): where the 地點 of a photo comes from, the attraction list and filling
' in the places of the photos already in the library. Made in code like the other added pages.
'   地名來源          Config.PlaceSource: 離線 (the township list) / 線上 (OpenStreetMap) -- saved with 儲存
'   景點資料          Places\Attractions.txt; 「更新景點資料」 downloads the current one from 交通部觀光署 (the
'                     only time this goes to the network) -- at once
'   地圖              Config.MapOffline: 線上 (OpenStreetMap through the tile cache) / 離線 (the offline map
'                     file, else the outline map) and Config.MapCacheMB -- saved with 儲存; 清除地圖快取,
'                     下載／更新／刪除 離線地圖檔 -- at once (Modules\MapHost.vb, OfflineMap.vb)
'   補寫…             PlaceFill.FillAll after a .Exif backup (Maintenance.BackupExif) -- at once
Partial Class frmSetup

    Private pagePlaces As Aqua.TabPage
    Private rbPlaceSource(1) As Aqua.RadioButton
    Private lblAttractions, lblFillState As Label
    Private WithEvents lblUpdateAttractions, lblFillPlaces As Label
    Private m_bolPlaceBusy As Boolean

    Private Sub PlacesPage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        pagePlaces = New Aqua.TabPage With {.Title = "地點", .BackColor = Color.White, .Font = pageGeneral.Font, .ForeColor = pageGeneral.ForeColor}
        Dim big As Font = chkPrivilege.Font
        Dim small As New Font(big.FontFamily, 11.0F)
        Dim link As New Font(big.FontFamily, 12.0F, FontStyle.Underline)
        Dim blue As Color = Color.FromArgb(42, 116, 208)
        Dim x As Integer = 40

        pagePlaces.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "照片的地點怎麼決定", .Location = New Point(x, 18), .BackColor = Color.Transparent})
        pagePlaces.Controls.Add(New Label With {
            .AutoSize = False, .Size = New Size(770, 44), .Location = New Point(x + 20, 46), .Font = small, .ForeColor = Color.DimGray, .BackColor = Color.Transparent,
            .Text = "照片有 GPS 時：200 公尺內有觀光景點就寫景點名稱（例如「九族文化村」），沒有就寫縣市與鄉鎮區。" & vbCrLf &
                    "只填空白的欄位，自己打的地點不會被蓋掉；.Exif 另外記下 Country、City（縣市加區）、Town（區）。"})
        Dim captions() As String = {"縣市鄉鎮用離線清單", "縣市鄉鎮上網查（OpenStreetMap）"}
        For i = 0 To 1
            rbPlaceSource(i) = PlaceRadio("PLACE", captions(i), small, x + 20 + i * 250, 94)
        Next
        rbPlaceSource(If(g_lpConfig.PlaceSource = PlaceNames.enumPlaceSource.psOnline, 1, 0)).Checked = True

        pagePlaces.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "景點資料（交通部觀光署）", .Location = New Point(x, 136), .BackColor = Color.Transparent})
        lblAttractions = New Label With {.AutoSize = False, .Size = New Size(320, 24), .Location = New Point(x + 300, 140), .Font = small, .ForeColor = Color.DimGray, .BackColor = Color.Transparent}
        pagePlaces.Controls.Add(lblAttractions)
        lblUpdateAttractions = New Label With {.AutoSize = True, .Font = link, .ForeColor = blue, .Cursor = Cursors.Hand, .Location = New Point(x + 640, 138), .BackColor = Color.Transparent, .Text = "更新景點資料"}
        pagePlaces.Controls.Add(lblUpdateAttractions)

        ' 地圖 (Modules\MapHost.vb, OfflineMap.vb): 線上 / 離線, the offline map file, the tiles kept
        pagePlaces.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "地圖", .Location = New Point(x, 180), .BackColor = Color.Transparent})
        rbMapMode(0) = PlaceRadio("MAPMODE", "線上（OpenStreetMap 街道地圖）", small, x + 80, 182)
        rbMapMode(1) = PlaceRadio("MAPMODE", "離線（不連網路：離線地圖檔或簡易地圖）", small, x + 380, 182)
        rbMapMode(If(g_lpConfig.MapOffline, 1, 0)).Checked = True
        lblMapHelp = New Label With {.AutoSize = True, .Font = link, .ForeColor = blue, .Cursor = Cursors.Hand, .Location = New Point(x + 600, 252), .BackColor = Color.Transparent, .Text = "地圖說明與流程…"}
        pagePlaces.Controls.Add(lblMapHelp)

        pagePlaces.Controls.Add(New Label With {.AutoSize = True, .Font = small, .ForeColor = Color.DimGray, .Text = "離線地圖檔", .Location = New Point(x + 20, 220), .BackColor = Color.Transparent})
        lblOfflineMap = New Label With {.AutoSize = True, .Font = small, .ForeColor = Color.FromArgb(64, 64, 64), .Location = New Point(x + 140, 220), .BackColor = Color.Transparent}
        pagePlaces.Controls.Add(lblOfflineMap)
        lblOfflineDownload = New Label With {.AutoSize = True, .Font = link, .ForeColor = blue, .Cursor = Cursors.Hand, .Location = New Point(x + 520, 218), .BackColor = Color.Transparent}
        pagePlaces.Controls.Add(lblOfflineDownload)
        lblOfflineDelete = New Label With {.AutoSize = True, .Font = link, .ForeColor = blue, .Cursor = Cursors.Hand, .Location = New Point(x + 690, 218), .BackColor = Color.Transparent, .Text = "刪除"}
        pagePlaces.Controls.Add(lblOfflineDelete)

        pagePlaces.Controls.Add(New Label With {.AutoSize = True, .Font = small, .ForeColor = Color.DimGray, .Text = "地圖快取上限", .Location = New Point(x + 20, 254), .BackColor = Color.Transparent})
        cboMapCache = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Font = small, .Location = New Point(x + 140, 250), .Width = 110}
        cboMapCache.Items.AddRange({"200 MB", "500 MB", "1 GB", "2 GB"})
        Dim mbs() As Integer = {200, 500, 1024, 2048}
        Dim at As Integer = Array.IndexOf(mbs, g_lpConfig.MapCacheMB)
        cboMapCache.SelectedIndex = If(at >= 0, at, 1)
        pagePlaces.Controls.Add(cboMapCache)
        lblMapCache = New Label With {.AutoSize = True, .Font = small, .ForeColor = Color.DimGray, .Location = New Point(x + 270, 254), .BackColor = Color.Transparent}
        pagePlaces.Controls.Add(lblMapCache)
        lblClearMapCache = New Label With {.AutoSize = True, .Font = link, .ForeColor = blue, .Cursor = Cursors.Hand, .Location = New Point(x + 420, 252), .BackColor = Color.Transparent, .Text = "清除地圖快取"}
        pagePlaces.Controls.Add(lblClearMapCache)

        pagePlaces.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "已經在相片庫的照片", .Location = New Point(x, 300), .BackColor = Color.Transparent})
        ' how many photos / videos the library has and how many photos have GPS: at the bottom left of the
        ' window, beside 放棄 / 儲存, so every page shows it (counted once the window is up)
        lblLibraryCounts = New Label With {
            .AutoSize = False, .Font = small, .ForeColor = Color.FromArgb(64, 64, 64), .BackColor = Color.Transparent, .Text = "",
            .Bounds = New Rectangle(tabSetup.Left, butExit.Top - 8, butExit.Left - tabSetup.Left - 16, 44), .TextAlign = ContentAlignment.MiddleLeft}
        Controls.Add(lblLibraryCounts)
        lblLibraryCounts.BringToFront()
        pagePlaces.Controls.Add(New Label With {
            .AutoSize = False, .Size = New Size(770, 44), .Location = New Point(x + 20, 328), .Font = small, .ForeColor = Color.DimGray, .BackColor = Color.Transparent,
            .Text = "從照片檔讀出 GPS 寫進 .Exif，並補上空白的地點、Country、City、Town；開始前會先備份所有 .Exif。" & vbCrLf &
                    "沒有 GPS 的照片（多半是舊相機拍的）不會改。"})
        lblFillPlaces = New Label With {.AutoSize = True, .Font = link, .ForeColor = blue, .Cursor = Cursors.Hand, .Location = New Point(x + 20, 376), .BackColor = Color.Transparent, .Text = "補寫全部照片的 GPS／地點…"}
        pagePlaces.Controls.Add(lblFillPlaces)
        lblFillState = New Label With {.AutoSize = False, .Size = New Size(770, 44), .Location = New Point(x + 20, 406), .Font = small, .ForeColor = Color.DimGray, .BackColor = Color.Transparent}
        pagePlaces.Controls.Add(lblFillState)
        tabSetup.TabPages.Add(pagePlaces)
        ShowAttractionState()
        ShowMapCacheState()
        ShowOfflineMapState()
        BeginInvoke(Sub() ShowLibraryCounts())
        Dim writable As Boolean = Not g_lpConfig.ReadOnly
        lblUpdateAttractions.Enabled = writable
        lblFillPlaces.Enabled = writable AndAlso g_lpConfig.AlbumCount > 0
    End Sub

    Private rbMapMode(1) As Aqua.RadioButton
    Private cboMapCache As ComboBox
    Private lblMapCache As Label
    Private WithEvents lblClearMapCache As Label
    Private WithEvents lblMapHelp As Label
    Private lblLibraryCounts As Label
    Private lblOfflineMap As Label
    Private WithEvents lblOfflineDownload, lblOfflineDelete As Label
    Private m_offlineCancel As Threading.CancellationTokenSource

    ''' <summary>「2026-10-01 版，131 MB（下載於 2026/10/1）」 / 「還沒有下載」, and the links for it.</summary>
    Private Sub ShowOfflineMapState()
        Dim i As OfflineMap.Info = OfflineMap.Current()
        If i Is Nothing Then
            lblOfflineMap.Text = "還沒有下載（台灣全區，約 130 MB）"
            lblOfflineDownload.Text = "下載離線地圖"
        Else
            Dim build As String = If(i.Build.Length = 8, $"{i.Build.Substring(0, 4)}-{i.Build.Substring(4, 2)}-{i.Build.Substring(6, 2)} 版，", "")
            lblOfflineMap.Text = build & (i.Bytes / 1048576.0).ToString("0") & " MB（下載於 " & i.Downloaded.ToString("yyyy/M/d") & "）"
            lblOfflineDownload.Text = "更新離線地圖"
        End If
        lblOfflineDelete.Visible = i IsNot Nothing
        lblOfflineDownload.Enabled = True
    End Sub

    ''' <summary>下載／更新離線地圖; while it runs the link says 取消下載.</summary>
    Private Async Sub lblOfflineDownload_Click(sender As Object, e As EventArgs) Handles lblOfflineDownload.Click
        If m_offlineCancel IsNot Nothing Then   ' 取消下載
            m_offlineCancel.Cancel()
            Return
        End If
        Dim had As Boolean = OfflineMap.Current() IsNot Nothing
        If Not frmQueryMsgBox.ShowMessage(If(had, "下載最新的台灣離線地圖，取代現在的？", "下載台灣離線地圖？") & vbCrLf &
                                          "約 130 MB，資料來自 OpenStreetMap（Protomaps 每天的版本），存在相片庫的 System\Maps。" & vbCrLf &
                                          "設定成「離線」時，地圖會用它顯示完整的街道與地名。", "離線地圖") Then Return
        m_offlineCancel = New Threading.CancellationTokenSource
        SetPlaceBusy(True)
        lblOfflineDownload.Text = "取消下載"
        lblOfflineDelete.Visible = False
        Dim report As Action(Of String) = Sub(s) BeginInvoke(Sub() lblOfflineMap.Text = s)
        Try
            Dim token = m_offlineCancel.Token
            Await Task.Run(Function() OfflineMap.DownloadAsync(report, token))
            ShowOfflineMapState()
        Catch ex As OperationCanceledException
            ShowOfflineMapState()
            If OfflineMap.Current() IsNot Nothing Then lblOfflineMap.Text = "已取消下載，原本的地圖照常使用" Else lblOfflineMap.Text = "已取消下載"
        Catch ex As Exception
            ShowOfflineMapState()
            frmMsgBox.ShowCriticalMessage("離線地圖下載失敗，原本的檔案照常使用" & vbCrLf & ex.Message, "離線地圖")
        Finally
            m_offlineCancel.Dispose()
            m_offlineCancel = Nothing
            SetPlaceBusy(False)
        End Try
    End Sub

    Private Sub lblOfflineDelete_Click(sender As Object, e As EventArgs) Handles lblOfflineDelete.Click
        If m_offlineCancel IsNot Nothing Then Return
        If Not frmQueryMsgBox.ShowMessage("刪除台灣離線地圖檔？" & vbCrLf & "離線時地圖會改成只顯示簡易地圖；之後可以再下載。", "離線地圖") Then Return
        OfflineMap.Delete()
        ShowOfflineMapState()
    End Sub

    ''' <summary>「照片 23,104 張、影片 721 個；有 GPS 的照片 8,122 張（35%）」 from the photo index. On the UI
    ''' thread: the database connection isn't shared with other threads.</summary>
    Private Sub ShowLibraryCounts()
        If lblLibraryCounts Is Nothing OrElse lblLibraryCounts.IsDisposed Then Return
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then
            lblLibraryCounts.Text = "（沒有相片庫資料庫）"
            Return
        End If
        Try
            Dim c As Database.LibraryCounts = g_lpDatabase.CountLibrary()
            Dim pct As String = If(c.Photos > 0, "（" & (c.PhotosWithGps * 100.0 / c.Photos).ToString("0") & "%）", "")
            lblLibraryCounts.Text = $"相片庫：照片 {c.Photos:#,0} 張、影片 {c.Videos:#,0} 個" & vbCrLf &
                                    $"有 GPS 的照片 {c.PhotosWithGps:#,0} 張" & pct &
                                    If(c.VideosWithGps > 0, $"、影片 {c.VideosWithGps:#,0} 個", "")
        Catch ex As Exception When TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is InvalidOperationException
            lblLibraryCounts.Text = "（無法讀取相片庫數量）"
        End Try
    End Sub

    Private Sub lblMapHelp_Click(sender As Object, e As EventArgs) Handles lblMapHelp.Click
        frmMapHelp.ShowHelp(Me)
    End Sub

    Private Function PlaceRadio(ByVal group As String, ByVal text As String, ByVal font As Font, ByVal x As Integer, ByVal y As Integer) As Aqua.RadioButton
        Dim rb As New Aqua.RadioButton With {
            .GroupName = group, .TextValue = text, .TextGap = rbStyle(0).TextGap, .Font = font, .BackColor = Color.Transparent,
            .ImageChecked = rbStyle(0).ImageChecked, .ImageUnChecked = rbStyle(0).ImageUnChecked,
            .ImageCheckDisabled = rbStyle(0).ImageCheckDisabled, .ImageUnCheckDisabled = rbStyle(0).ImageUnCheckDisabled,
            .Location = New Point(x, y)}
        pagePlaces.Controls.Add(rb)
        Return rb
    End Function

    ''' <summary>How much the map tile cache holds now (counted in the background).</summary>
    Private Sub ShowMapCacheState()
        lblMapCache.Text = "計算中…"
        Task.Run(Function() MapHost.CacheBytes()).ContinueWith(
            Sub(t)
                If IsDisposed Then Return
                BeginInvoke(Sub() lblMapCache.Text = "已存 " & (t.Result / 1024.0 / 1024.0).ToString("#,0.0") & " MB")
            End Sub)
    End Sub

    Private Sub lblClearMapCache_Click(sender As Object, e As EventArgs) Handles lblClearMapCache.Click
        If Not frmQueryMsgBox.ShowMessage("清除電腦裡存的地圖圖磚？" & vbCrLf & "線上時看過的地方會重新下載（快取只是讓地圖開得快、少連 OpenStreetMap）。", "清除地圖快取") Then Return
        MapHost.ClearCache()
        ShowMapCacheState()
    End Sub

    Private Sub ShowAttractionState()
        Dim n As Integer = Attractions.Count
        lblAttractions.Text = If(n = 0, "沒有景點資料（按「更新景點資料」下載）",
                                 $"{n:#,0} 個景點" & If(Attractions.Updated <> "", "，資料日期 " & Strings.Left(Attractions.Updated, 10), ""))
    End Sub

    ''' <summary>Called by butSave_Click before g_lpConfig.Save.</summary>
    Private Sub SavePlacesPage()
        If pagePlaces Is Nothing Then Return
        g_lpConfig.PlaceSource = If(rbPlaceSource(1).Checked, PlaceNames.enumPlaceSource.psOnline, PlaceNames.enumPlaceSource.psOffline)
        g_lpConfig.MapOffline = rbMapMode(1).Checked   ' maps opened from now on
        g_lpConfig.MapCacheMB = {200, 500, 1024, 2048}(Math.Max(0, cboMapCache.SelectedIndex))
    End Sub

    Private Sub SetPlaceBusy(ByVal busy As Boolean)
        m_bolPlaceBusy = busy
        lblUpdateAttractions.Enabled = Not busy AndAlso Not g_lpConfig.ReadOnly
        lblFillPlaces.Enabled = Not busy AndAlso Not g_lpConfig.ReadOnly AndAlso g_lpConfig.AlbumCount > 0
        butSave.Enabled = Not busy
        butExit.Enabled = Not busy
    End Sub

    Private Sub PlacesPage_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If m_bolPlaceBusy Then e.Cancel = True   ' a download or the fill is running: wait for it
    End Sub

    Private Sub lblUpdateAttractions_Click(sender As Object, e As EventArgs) Handles lblUpdateAttractions.Click
        If Not lblUpdateAttractions.Enabled OrElse m_bolPlaceBusy Then Return
        If Not frmQueryMsgBox.ShowMessage("連線到交通部觀光署下載最新的景點資料（約 3 MB）？" & vbCrLf & Attractions.SourceUrl, "更新景點資料") Then Return
        SetPlaceBusy(True)
        lblAttractions.Text = "下載中…"
        Task.Run(Function() Attractions.Download()).ContinueWith(
            Sub(t)
                BeginInvoke(Sub()
                                SetPlaceBusy(False)
                                ShowAttractionState()
                                If t.IsFaulted Then
                                    frmMsgBox.ShowCriticalMessage("景點資料更新失敗，舊的資料照常使用" & vbCrLf & t.Exception.GetBaseException().Message, "")
                                Else
                                    frmMsgBox.ShowExclamationMessage($"已更新：{t.Result.Kept:#,0} 個景點" &
                                        If(t.Result.Skipped > 0, $"（{t.Result.Skipped} 個名稱有 .Exif 存不下的字，沒有採用）", ""), "更新景點資料")
                                End If
                            End Sub)
            End Sub)
    End Sub

    Private Sub lblFillPlaces_Click(sender As Object, e As EventArgs) Handles lblFillPlaces.Click
        If Not lblFillPlaces.Enabled OrElse m_bolPlaceBusy Then Return
        If Not frmQueryMsgBox.ShowMessage("補寫全部照片的 GPS 與地點？" & vbCrLf &
                                          "只會填空白的欄位；開始前會先備份所有 .Exif。照片很多時要幾分鐘。", "補寫 GPS／地點") Then Return
        SavePlacesPage()   ' the source chosen here counts for this run
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        SetPlaceBusy(True)
        lblFillState.Text = "備份 .Exif…"
        Application.DoEvents()
        Dim backup As (Zip As String, Count As Integer) = Maintenance.BackupExif(mdb, roots)
        If backup.Zip = "" Then
            SetPlaceBusy(False)
            lblFillState.Text = ""
            frmMsgBox.ShowCriticalMessage(".Exif 備份失敗，沒有補寫", "")
            Return
        End If
        Task.Run(Function() PlaceFill.FillAll(mdb, roots, Sub(done, total) BeginInvoke(Sub() lblFillState.Text = $"補寫中… {done:#,0} / {total:#,0} 張"))).ContinueWith(
            Sub(t)
                BeginInvoke(Sub()
                                SetPlaceBusy(False)
                                If t.IsFaulted Then
                                    lblFillState.Text = "補寫中斷：" & t.Exception.GetBaseException().Message
                                    Return
                                End If
                                Dim r As PlaceFill.Result = t.Result
                                lblFillState.Text = $"看過 {r.Checked:#,0} 張，有 GPS 的 {r.WithGps:#,0} 張，補寫了 {r.Changed:#,0} 張（其中 {r.Attractions:#,0} 張的地點是景點）" &
                                                    If(r.Failed > 0, $"，{r.Failed} 張失敗：{r.LastError}", "") & vbCrLf &
                                                    ".Exif 備份：" & IO.Path.GetFileName(backup.Zip)
                                ShowLibraryCounts()   ' more photos have GPS now
                            End Sub)
            End Sub)
    End Sub

End Class
