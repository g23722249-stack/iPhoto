Imports System.IO
Imports System.Text.Json
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms

' 設定拍攝地點 (new in the .NET port; PhotoLib PlacePick): pick a place three ways --
'   行政區   臺灣 › 縣市 › 鄉鎮市區 from Places\Places.txt (the township's centre point), or search them
'   景點     the attractions of Places\Attractions.txt, searched by name, optionally within a 縣市
'   地圖     OpenStreetMap in WebView2 (Map\map_pick.html, needs the internet): click to put the pin,
'            drag it; 200 m from an attraction the attraction is the 地點, else the 縣市區
' The right side shows what will be written (GPS, 地點, 國家, 縣市區, 鄉鎮).
'   Apply  (主視窗右鍵、批次修改資訊) writes it into the photos at once (PlacePick.Apply: backup of their
'          .Exif first, PhotoIndex follows); photos with a GPS position / a 地點 already are skipped /
'          kept unless the two boxes say otherwise.
'   Pick   (照片資訊) only hands the choice back; the form writes it with 好.
' Made in code (no designer file).
Friend Class frmPlacePicker
    Inherits Form

    Private Shared s_intLastTab As Integer = 0
    Private Shared s_strLastCounty As String = "臺中市"

    Private ReadOnly m_lpFiles As List(Of String)
    Private ReadOnly m_bolApply As Boolean
    Private m_intWithGps As Integer = -1          ' photos that have a GPS position (-1: still counting)
    Private m_lpChoice As PlacePick.Choice
    Private m_lpResult As PlacePick.Result
    Private m_lpTowns As List(Of PlaceNames.TownPoint)
    Private m_bolMapReady As Boolean
    Private m_intMapRun As Integer

    ' top
    Private ReadOnly pnlTop As New Panel
    Private ReadOnly picThumbs(4) As PictureBox
    Private ReadOnly lblCount As New Label, lblGpsInfo As New Label
    ' tabs
    Private ReadOnly butTabs(2) As Button
    Private ReadOnly pnlTabs(2) As Panel
    ' 行政區
    Private ReadOnly txtTown As New TextBox
    Private ReadOnly lstCountry As New ListBox, lstCounty As New ListBox, lstTown As New ListBox, lstTownHits As New ListBox
    Private ReadOnly lblTownHead As New Label
    ' 景點
    Private ReadOnly txtSpot As New TextBox
    Private ReadOnly cboSpotCity As New ComboBox
    Private ReadOnly lvSpots As New ListView
    ' 地圖
    Private ReadOnly txtGoto As New TextBox
    Private ReadOnly web As New WebView2
    Private ReadOnly lblMapStatus As New Label
    ' right
    Private ReadOnly lblOutSpot As New Label, lblOutCountry As New Label, lblOutCity As New Label, lblOutTown As New Label, lblOutGps As New Label
    Private ReadOnly lblOutNote As New Label, lblNoPick As New Label
    Private ReadOnly chkOverGps As New CheckBox, chkOverSpot As New CheckBox
    Private ReadOnly butOk As New Button, butCancel As New Button

    '==================================================================================================
    ' Opening
    '==================================================================================================
    ''' <summary>主視窗右鍵 / 批次修改資訊: pick a place and write it into <paramref name="files"/>. Nothing when
    ''' cancelled.</summary>
    Public Shared Function PickAndApply(ByVal owner As IWin32Window, ByVal files As List(Of String)) As PlacePick.Result
        If files Is Nothing OrElse files.Count = 0 Then Return Nothing
        Using f As New frmPlacePicker(files, True)
            f.ShowDialog(owner)
            Return f.m_lpResult
        End Using
    End Function

    ''' <summary>照片資訊: pick a place for <paramref name="file"/>; the caller writes it. Nothing when cancelled.</summary>
    Public Shared Function PickFor(ByVal owner As IWin32Window, ByVal file As String) As PlacePick.Choice
        Using f As New frmPlacePicker(New List(Of String) From {file}, False)
            If f.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
            Return f.m_lpChoice
        End Using
    End Function

    Private Sub New(ByVal files As List(Of String), ByVal apply As Boolean)
        m_lpFiles = files
        m_bolApply = apply
        Text = "設定拍攝地點"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(980, 640)
        BackColor = Color.White
        KeyPreview = True
        BuildTop()
        BuildTabs()
        BuildTownPanel()
        BuildSpotPanel()
        BuildMapPanel()
        BuildRight()
        AcceptButton = butOk
        CancelButton = butCancel
    End Sub

    Private Shared ReadOnly Grey As Color = Color.FromArgb(82, 96, 109)
    Private Shared ReadOnly Blue As Color = Color.FromArgb(29, 95, 180)

    Private Sub BuildTop()
        pnlTop.SetBounds(0, 0, 980, 66)
        pnlTop.BackColor = Color.White
        For i = 0 To picThumbs.Length - 1
            picThumbs(i) = New PictureBox With {.Bounds = New Rectangle(16 + i * 64, 12, 56, 42), .SizeMode = PictureBoxSizeMode.Zoom,
                                                .BackColor = Color.FromArgb(214, 221, 228), .Visible = i < m_lpFiles.Count}
            pnlTop.Controls.Add(picThumbs(i))
        Next
        Dim x As Integer = 16 + Math.Min(m_lpFiles.Count, picThumbs.Length) * 64 + 8
        If m_lpFiles.Count > picThumbs.Length Then
            pnlTop.Controls.Add(New Label With {.Text = "+" & (m_lpFiles.Count - picThumbs.Length), .AutoSize = False, .Bounds = New Rectangle(x - 4, 12, 44, 42),
                                                .TextAlign = ContentAlignment.MiddleCenter, .BackColor = Color.FromArgb(214, 221, 228), .ForeColor = Grey})
            x += 52
        End If
        lblCount.SetBounds(x, 12, 600, 22)
        lblCount.Font = New Font(Font, FontStyle.Bold)
        lblCount.Text = If(m_lpFiles.Count = 1, Path.GetFileName(m_lpFiles(0)), m_lpFiles.Count & " 張照片")
        lblGpsInfo.SetBounds(x, 34, 600, 20)
        lblGpsInfo.ForeColor = Grey
        lblGpsInfo.Text = "檢查照片是否已有 GPS…"
        pnlTop.Controls.AddRange({lblCount, lblGpsInfo})
        Dim line As New Label With {.Bounds = New Rectangle(0, 65, 980, 1), .BackColor = Color.FromArgb(227, 232, 237)}
        Controls.AddRange({pnlTop, line})
    End Sub

    Private Sub BuildTabs()
        Dim names = {"行政區", "景點", "地圖"}
        Dim x As Integer = 16
        For i = 0 To 2
            Dim k As Integer = i
            butTabs(i) = New Button With {.Text = names(i), .FlatStyle = FlatStyle.Flat, .Height = 32, .AutoSize = True, .Location = New Point(x, 78), .Cursor = Cursors.Hand}
            butTabs(i).FlatAppearance.BorderColor = Color.FromArgb(154, 165, 177)
            AddHandler butTabs(i).Click, Sub() ShowTab(k)
            Controls.Add(butTabs(i))
            x += TextRenderer.MeasureText(names(i), Font).Width + 40
            pnlTabs(i) = New Panel With {.Bounds = New Rectangle(16, 118, 640, 510), .Visible = False}
            Controls.Add(pnlTabs(i))
        Next
    End Sub

    Private Sub ShowTab(ByVal i As Integer)
        s_intLastTab = i
        For k = 0 To 2
            pnlTabs(k).Visible = (k = i)
            butTabs(k).BackColor = If(k = i, Blue, Color.White)
            butTabs(k).ForeColor = If(k = i, Color.White, Color.FromArgb(31, 41, 51))
        Next
        If i = 2 Then StartMap()
        Select Case i
            Case 0 : txtTown.Focus()
            Case 1 : txtSpot.Focus()
            Case 2 : txtGoto.Focus()
        End Select
    End Sub

    Private Function NewNote(ByVal text As String, ByVal y As Integer, ByVal parent As Control) As Label
        Dim l As New Label With {.Text = text, .ForeColor = Grey, .AutoSize = False, .Bounds = New Rectangle(0, y, 640, 20), .Font = New Font(Font.FontFamily, 9.0F)}
        parent.Controls.Add(l)
        Return l
    End Function

    '==================================================================================================
    ' 行政區
    '==================================================================================================
    Private Sub BuildTownPanel()
        Dim p As Panel = pnlTabs(0)
        p.Controls.Add(New Label With {.Text = "搜尋", .AutoSize = True, .Location = New Point(0, 6)})
        txtTown.SetBounds(46, 2, 594, 28)
        txtTown.PlaceholderText = "輸入縣市或鄉鎮，例如「西屯」「魚池」"
        lstCountry.SetBounds(0, 40, 110, 440)
        lstCounty.SetBounds(118, 40, 130, 440)
        lstTown.SetBounds(256, 40, 384, 440)
        lstTown.MultiColumn = True
        lstTown.ColumnWidth = 124
        lstTownHits.SetBounds(0, 40, 640, 440)
        lstTownHits.Visible = False
        For Each l In {lstCountry, lstCounty, lstTown, lstTownHits}
            l.IntegralHeight = False
        Next
        lstCountry.Items.Add("臺灣")
        lstCountry.SelectedIndex = 0
        p.Controls.AddRange({txtTown, lstCountry, lstCounty, lstTown, lstTownHits})
        NewNote("行政區的 GPS 用該區的中心點，是約略位置。資料只有臺灣的 368 個鄉鎮市區。", 488, p)

        m_lpTowns = PlaceNames.TownCentres()
        For Each c In PlaceNames.Counties
            If m_lpTowns.Any(Function(t) t.County = c) Then lstCounty.Items.Add(c)
        Next
        AddHandler lstCounty.SelectedIndexChanged, Sub() FillTowns()
        AddHandler lstTown.SelectedIndexChanged, Sub()
                                                     Dim t = TryCast(lstTown.SelectedItem, TownItem)
                                                     If t IsNot Nothing Then SetChoice(PlacePick.FromTown(t.Town))
                                                 End Sub
        AddHandler lstTownHits.SelectedIndexChanged, Sub()
                                                         Dim t = TryCast(lstTownHits.SelectedItem, TownItem)
                                                         If t IsNot Nothing Then SetChoice(PlacePick.FromTown(t.Town))
                                                     End Sub
        AddHandler txtTown.TextChanged, Sub() SearchTowns()
        Dim i As Integer = lstCounty.Items.IndexOf(s_strLastCounty)
        lstCounty.SelectedIndex = If(i >= 0, i, If(lstCounty.Items.Count > 0, 0, -1))
    End Sub

    ''' <summary>A township in a list box (shows its name; the hits list shows the county too).</summary>
    Private Class TownItem
        Public ReadOnly Town As PlaceNames.TownPoint
        Private ReadOnly m_bolFull As Boolean
        Public Sub New(ByVal t As PlaceNames.TownPoint, ByVal full As Boolean)
            Town = t
            m_bolFull = full
        End Sub
        Public Overrides Function ToString() As String
            Return If(m_bolFull, Town.County & " › " & Town.Town, Town.Town)
        End Function
    End Class

    Private Sub FillTowns()
        Dim c As String = TryCast(lstCounty.SelectedItem, String)
        lstTown.BeginUpdate()
        lstTown.Items.Clear()
        If c IsNot Nothing Then
            s_strLastCounty = c
            For Each t In m_lpTowns.Where(Function(x) x.County = c)
                lstTown.Items.Add(New TownItem(t, False))
            Next
        End If
        lstTown.EndUpdate()
    End Sub

    Private Sub SearchTowns()
        Dim q As String = txtTown.Text.Trim().Replace("台", "臺")
        Dim searching As Boolean = q <> ""
        lstTownHits.Visible = searching
        For Each l In {lstCountry, lstCounty, lstTown}
            l.Visible = Not searching
        Next
        If Not searching Then Return
        lstTownHits.BeginUpdate()
        lstTownHits.Items.Clear()
        For Each t In m_lpTowns.Where(Function(x) x.Name.Contains(q) OrElse ("臺灣" & x.Name).Contains(q))
            lstTownHits.Items.Add(New TownItem(t, True))
        Next
        If lstTownHits.Items.Count = 0 Then lstTownHits.Items.Add("找不到符合的縣市或鄉鎮")
        lstTownHits.EndUpdate()
    End Sub

    '==================================================================================================
    ' 景點
    '==================================================================================================
    Private Sub BuildSpotPanel()
        Dim p As Panel = pnlTabs(1)
        p.Controls.Add(New Label With {.Text = "搜尋", .AutoSize = True, .Location = New Point(0, 6)})
        txtSpot.SetBounds(46, 2, 424, 28)
        txtSpot.PlaceholderText = "景點名稱，例如「高美」「九族」"
        p.Controls.Add(New Label With {.Text = "縣市", .AutoSize = True, .Location = New Point(484, 6)})
        cboSpotCity.SetBounds(528, 2, 112, 28)
        cboSpotCity.DropDownStyle = ComboBoxStyle.DropDownList
        cboSpotCity.Items.Add("全部")
        cboSpotCity.Items.AddRange(PlaceNames.Counties)
        cboSpotCity.SelectedIndex = 0
        lvSpots.SetBounds(0, 40, 640, 440)
        lvSpots.View = View.Details
        lvSpots.FullRowSelect = True
        lvSpots.HideSelection = False
        lvSpots.MultiSelect = False
        lvSpots.Columns.Add("景點", 380)
        lvSpots.Columns.Add("縣市鄉鎮", 230)
        p.Controls.AddRange({txtSpot, cboSpotCity, lvSpots})
        NewNote("交通部觀光署景點 " & Attractions.Count.ToString("#,0") & " 個（Attractions.txt）；同名重複的只列一次。", 488, p)
        AddHandler txtSpot.TextChanged, Sub() SearchSpots()
        AddHandler cboSpotCity.SelectedIndexChanged, Sub() SearchSpots()
        AddHandler lvSpots.SelectedIndexChanged, Sub()
                                                     If lvSpots.SelectedItems.Count = 0 Then Return
                                                     Dim a = TryCast(lvSpots.SelectedItems(0).Tag, Attractions.Attraction)
                                                     If a IsNot Nothing Then SetChoice(PlacePick.FromAttraction(a))
                                                 End Sub
    End Sub

    Private Sub SearchSpots()
        Dim city As String = If(cboSpotCity.SelectedIndex <= 0, "", CStr(cboSpotCity.SelectedItem))
        Dim q As String = txtSpot.Text.Trim()
        lvSpots.BeginUpdate()
        lvSpots.Items.Clear()
        If q <> "" OrElse city <> "" Then
            For Each a In Attractions.Search(q, city)
                lvSpots.Items.Add(New ListViewItem({a.Name, a.City & a.Town}) With {.Tag = a})
            Next
            If lvSpots.Items.Count = 0 Then lvSpots.Items.Add(New ListViewItem({"找不到符合的景點", ""}) With {.ForeColor = Grey})
        End If
        lvSpots.EndUpdate()
    End Sub

    '==================================================================================================
    ' 地圖
    '==================================================================================================
    Private Sub BuildMapPanel()
        Dim p As Panel = pnlTabs(2)
        p.Controls.Add(New Label With {.Text = "跳到", .AutoSize = True, .Location = New Point(0, 6)})
        txtGoto.SetBounds(46, 2, 594, 28)
        txtGoto.PlaceholderText = "景點或鄉鎮名稱，按 Enter"
        web.SetBounds(0, 40, 640, 440)
        lblMapStatus.SetBounds(0, 40, 640, 440)
        lblMapStatus.TextAlign = ContentAlignment.MiddleCenter
        lblMapStatus.BackColor = Color.FromArgb(238, 241, 244)
        lblMapStatus.Text = "開啟地圖中…"
        p.Controls.AddRange({txtGoto, lblMapStatus, web})
        lblMapStatus.BringToFront()
        NewNote("點選的位置會對照景點：200 公尺內有景點就用景點名稱，否則用縣市區。", 488, p)
        AddHandler txtGoto.KeyDown, Sub(s, e)
                                        If e.KeyCode <> Keys.Enter Then Return
                                        e.SuppressKeyPress = True
                                        GotoPlace(txtGoto.Text.Trim())
                                    End Sub
    End Sub

    Private m_bolMapStarted As Boolean

    Private Async Sub StartMap()
        If m_bolMapStarted Then Return
        m_bolMapStarted = True
        Try
            Dim data As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto")
            Dim env As CoreWebView2Environment = Await CoreWebView2Environment.CreateAsync(Nothing, Path.Combine(data, "WebView2"))
            Await web.EnsureCoreWebView2Async(env)
            With web.CoreWebView2
                .Settings.AreDevToolsEnabled = False
                .Settings.IsStatusBarEnabled = False
                .Settings.AreDefaultContextMenusEnabled = False
                AddHandler .WebMessageReceived, AddressOf Map_Message
                AddHandler .NewWindowRequested, Sub(s, e)
                                                     e.Handled = True
                                                     Try
                                                         Process.Start(New ProcessStartInfo(e.Uri) With {.UseShellExecute = True})
                                                     Catch
                                                     End Try
                                                 End Sub
            End With
            MapHost.ServeAndNavigate(web.CoreWebView2, "map_pick.html")   ' (Modules\MapHost.vb)
            lblMapStatus.Visible = False
        Catch ex As Exception
            lblMapStatus.Text = "無法開啟地圖：" & ex.Message & vbCrLf & "可以改用「行政區」或「景點」。"
        End Try
    End Sub

    Private Sub Map_Message(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Dim msg As JsonElement
        Try
            msg = JsonDocument.Parse(e.WebMessageAsJson).RootElement
        Catch
            Return
        End Try
        Select Case msg.GetProperty("type").GetString()
            Case "help"   ' the 線上 / 離線 badge clicked
                frmMapHelp.ShowHelp(Me)
            Case "jserror"
                MapHost.LogPageError("輸入地點 " & msg.GetProperty("msg").GetString())
            Case "ready"
                m_bolMapReady = True
                ' the pin where the choice is, else where the (first) photo already is
                If m_lpChoice IsNot Nothing Then
                    PostMap("set", m_lpChoice.Lat, m_lpChoice.Lon, 15)
                Else
                    Dim lat, lon As Double
                    If PlaceNames.TryParseGps(ExistingGps(), lat, lon) Then PostMap("set", lat, lon, 15)
                End If
            Case "offline"
                lblMapStatus.Text = "地圖需要連上網路才能顯示。" & vbCrLf & "可以改用「行政區」或「景點」選擇地點。"
                lblMapStatus.Visible = True
            Case "pos"
                Dim lat As Double = msg.GetProperty("lat").GetDouble(), lon As Double = msg.GetProperty("lon").GetDouble()
                Dim run As Integer = Threading.Interlocked.Increment(m_intMapRun)
                lblNoPick.Text = "查詢地名…"
                ' PlaceNames may ask OpenStreetMap (設定 › 地點): off the window's thread
                Threading.Tasks.Task.Run(Function() PlacePick.FromPoint(lat, lon)).ContinueWith(
                    Sub(t)
                        If IsDisposed OrElse Not IsHandleCreated OrElse t.IsFaulted Then Return
                        BeginInvoke(Sub()
                                        If run = m_intMapRun Then SetChoice(t.Result)
                                    End Sub)
                    End Sub)
        End Select
    End Sub

    Private Sub PostMap(ByVal type As String, ByVal lat As Double, ByVal lon As Double, ByVal zoom As Integer)
        If Not m_bolMapReady OrElse web.CoreWebView2 Is Nothing Then Return
        web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(New Dictionary(Of String, Object) From {{"type", type}, {"lat", lat}, {"lon", lon}, {"zoom", zoom}}))
    End Sub

    ''' <summary>跳到: the first attraction, else township, matching the text.</summary>
    Private Sub GotoPlace(ByVal q As String)
        If q = "" Then Return
        Dim a As Attractions.Attraction = Attractions.Search(q, "", 1).FirstOrDefault()
        Dim qq As String = q.Replace("台", "臺")
        Dim t As PlaceNames.TownPoint = m_lpTowns.FirstOrDefault(Function(x) x.Name.Contains(qq))
        If t IsNot Nothing AndAlso (a Is Nothing OrElse t.Name.EndsWith(qq)) Then
            PostMap("goto", t.Lat, t.Lon, 13)
        ElseIf a IsNot Nothing Then
            PostMap("goto", a.Lat, a.Lon, 16)
        Else
            lblNoPick.Text = "找不到「" & q & "」"
        End If
    End Sub

    ''' <summary>The .Exif GPS of the first photo that has one ("" when none).</summary>
    Private Function ExistingGps() As String
        For Each f In m_lpFiles
            Dim exif As String = Path.ChangeExtension(f, gc_strExifPattern)
            If Not File.Exists(exif) Then Continue For
            Dim g As String = GetClearText(New Carbon.IniFile With {.FileName = exif}.SimpleGetValue("Exif", "GPS"))
            If g <> "" Then Return g
        Next
        Return If(m_lpFiles.Count > 0, PlaceNames.GpsOfPicture(m_lpFiles(0)), "")
    End Function

    '==================================================================================================
    ' Right: what will be written
    '==================================================================================================
    Private Sub BuildRight()
        Dim p As New Panel With {.Bounds = New Rectangle(672, 66, 308, 574), .BackColor = Color.FromArgb(247, 249, 251)}
        Controls.Add(p)
        p.Controls.Add(New Label With {.Text = "將寫入", .Font = New Font(Font, FontStyle.Bold), .AutoSize = True, .Location = New Point(18, 16)})
        Dim y As Integer = 48
        For Each row In {("地點", lblOutSpot), ("國家", lblOutCountry), ("縣市區", lblOutCity), ("鄉鎮", lblOutTown), ("GPS", lblOutGps)}
            p.Controls.Add(New Label With {.Text = row.Item1, .ForeColor = Grey, .AutoSize = False, .Bounds = New Rectangle(18, y, 62, 24)})
            row.Item2.SetBounds(82, y, 210, If(row.Item1 = "地點", 44, 24))
            p.Controls.Add(row.Item2)
            y += If(row.Item1 = "地點", 48, 28)
        Next
        lblOutSpot.Font = New Font(Font, FontStyle.Bold)
        lblOutGps.Font = New Font("Consolas", 10.0F)
        lblOutNote.SetBounds(18, y + 4, 276, 60)
        lblOutNote.ForeColor = Grey
        lblOutNote.Font = New Font(Font.FontFamily, 9.0F)
        lblNoPick.SetBounds(18, 48, 276, 60)
        lblNoPick.ForeColor = Grey
        lblNoPick.Text = "還沒選地點"
        p.Controls.AddRange({lblOutNote, lblNoPick})
        lblNoPick.BringToFront()

        chkOverGps.SetBounds(18, 330, 280, 44)
        chkOverGps.Text = "已有 GPS 的照片也改掉（相機記錄的位置通常較準）"
        chkOverSpot.SetBounds(18, 378, 280, 24)
        chkOverSpot.Text = "「地點」欄已有文字的也改掉"
        For Each c In {chkOverGps, chkOverSpot}
            c.Font = New Font(Font.FontFamily, 9.5F)
            c.Visible = m_bolApply
            AddHandler c.CheckedChanged, Sub() UpdateOk()
            p.Controls.Add(c)
        Next

        butCancel.SetBounds(96, 522, 80, 34)
        butCancel.Text = "取消"
        butCancel.DialogResult = DialogResult.Cancel
        butOk.SetBounds(184, 522, 108, 34)
        butOk.FlatStyle = FlatStyle.Flat
        butOk.BackColor = Blue
        butOk.ForeColor = Color.White
        butOk.FlatAppearance.BorderColor = Blue
        AddHandler butOk.Click, Sub() OkClicked()
        p.Controls.AddRange({butCancel, butOk})
        ShowChoice()
    End Sub

    Private Sub SetChoice(ByVal c As PlacePick.Choice)
        m_lpChoice = c
        ShowChoice()
    End Sub

    Private Sub ShowChoice()
        Dim c As PlacePick.Choice = m_lpChoice
        Dim has As Boolean = c IsNot Nothing
        For Each l In {lblOutSpot, lblOutCountry, lblOutCity, lblOutTown, lblOutGps, lblOutNote}
            l.Visible = has
        Next
        lblNoPick.Visible = Not has
        If has Then
            lblNoPick.Text = "還沒選地點"
            lblOutSpot.Text = If(c.Spot <> "", c.Spot, "（沒有名稱）")
            lblOutCountry.Text = c.Country
            lblOutCity.Text = c.City
            lblOutTown.Text = c.Town
            lblOutGps.Text = c.Gps
            Select Case c.Kind
                Case PlacePick.enumPickKind.pkTown : lblOutNote.Text = "行政區中心點（約略位置）"
                Case PlacePick.enumPickKind.pkAttraction : lblOutNote.Text = "景點的座標"
                Case Else
                    lblOutNote.Text = "地圖上點的位置" & If(c.NearMeters >= 0, "；200 公尺內最近的景點：" & c.Spot & "（" & c.NearMeters & " 公尺）", "")
                    If c.City = "" Then lblOutNote.Text &= vbCrLf & "這裡查不到臺灣的縣市鄉鎮，只會寫入 GPS。"
            End Select
            If Not m_bolApply AndAlso m_intWithGps > 0 Then lblOutNote.Text &= vbCrLf & "這張照片已有 GPS，按「好」後會被取代。"
        End If
        UpdateOk()
    End Sub

    Private Sub UpdateOk()
        Dim ro As Boolean = g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly
        butOk.Enabled = m_lpChoice IsNot Nothing AndAlso Not (m_bolApply AndAlso ro)
        butOk.BackColor = If(butOk.Enabled, Blue, Color.FromArgb(154, 165, 177))
        If Not m_bolApply Then
            butOk.Text = "選這個地點"
            Return
        End If
        Dim n As Integer = m_lpFiles.Count
        If m_intWithGps <= 0 OrElse chkOverGps.Checked Then
            butOk.Text = If(n = 1, "寫入", "寫入 " & n & " 張")
        Else
            butOk.Text = "寫入 " & (n - m_intWithGps) & " 張"
        End If
        butOk.Width = Math.Max(108, TextRenderer.MeasureText(butOk.Text, butOk.Font).Width + 28)
        butOk.Left = 292 - butOk.Width
        butCancel.Left = butOk.Left - 88
    End Sub

    Private Sub OkClicked()
        If m_lpChoice Is Nothing Then Return
        If Not m_bolApply Then
            DialogResult = DialogResult.OK
            Close()
            Return
        End If
        If m_intWithGps > 0 AndAlso Not chkOverGps.Checked AndAlso m_intWithGps >= m_lpFiles.Count Then
            frmMsgBox.ShowExclamationMessage("這些照片都已有 GPS。要改掉請勾選「已有 GPS 的照片也改掉」。", Text)
            Return
        End If
        UseWaitCursor = True
        Enabled = False
        Try
            Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
            Dim folder As String = If(String.IsNullOrEmpty(mdb), Path.Combine(Path.GetTempPath(), "iPhoto"), Maintenance.BackupFolder(mdb))
            m_lpResult = PlacePick.Apply(m_lpFiles, m_lpChoice, chkOverGps.Checked, chkOverSpot.Checked, g_lpDatabase, folder)
        Finally
            Enabled = True
            UseWaitCursor = False
        End Try
        Dim r As PlacePick.Result = m_lpResult
        If r.Written = 0 AndAlso r.Failed.Count > 0 AndAlso r.BackupZip = "" Then
            frmMsgBox.ShowCriticalMessage("備份 .Exif 失敗，這次沒有寫入。", Text)
            Return
        End If
        Dim msg As String = "已寫入 " & r.Written & " 張照片的地點「" & m_lpChoice.Spot & "」"
        If r.SkippedGps > 0 Then msg &= vbCrLf & r.SkippedGps & " 張已有 GPS，沒有改"
        If r.KeptSpot > 0 Then msg &= vbCrLf & r.KeptSpot & " 張保留原本的「地點」文字"
        If r.Failed.Count > 0 Then msg &= vbCrLf & r.Failed.Count & " 張寫入失敗：" & Path.GetFileName(r.Failed(0))
        frmMsgBox.ShowSmileMessage(msg, Text)
        DialogResult = DialogResult.OK
        Close()
    End Sub

    '==================================================================================================
    ' Starting: the thumbnails and the GPS count in the background, the tab used last
    '==================================================================================================
    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        ShowTab(s_intLastTab)
        Dim files As List(Of String) = m_lpFiles.ToList()
        Threading.Tasks.Task.Run(Function() files.Where(Function(f) PlacePick.HasGps(f)).Count()).ContinueWith(
            Sub(t)
                If IsDisposed OrElse Not IsHandleCreated OrElse t.IsFaulted Then Return
                BeginInvoke(Sub() GpsCounted(t.Result))
            End Sub)
        For i = 0 To Math.Min(files.Count, picThumbs.Length) - 1
            Dim k As Integer = i, f As String = files(i)
            Threading.Tasks.Task.Run(Function() Thumb(f)).ContinueWith(
                Sub(t)
                    If IsDisposed OrElse Not IsHandleCreated OrElse t.IsFaulted OrElse t.Result Is Nothing Then Return
                    BeginInvoke(Sub() picThumbs(k).Image = t.Result)
                End Sub)
        Next
    End Sub

    Private Sub GpsCounted(ByVal n As Integer)
        m_intWithGps = n
        If m_lpFiles.Count = 1 Then
            lblGpsInfo.Text = If(n > 0, "已有 GPS：" & ExistingGps(), "還沒有 GPS")
        Else
            lblGpsInfo.Text = If(n > 0, "其中 " & n & " 張已有 GPS（多半是相機記錄的）", "都還沒有 GPS")
        End If
        chkOverGps.Enabled = n > 0
        If n > 0 Then chkOverGps.Text = "已有 GPS 的 " & n & " 張也改掉（相機記錄的位置通常較準）"
        ShowChoice()
    End Sub

    Private Shared Function Thumb(ByVal file As String) As Image
        If GetMediaType(Nothing, file) <> enumPhotoMediaType.mdImage Then Return Nothing
        Try
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using img As Image = Image.FromStream(fs, False, False)
                    Dim scale As Double = Math.Min(112.0 / img.Width, 84.0 / img.Height)
                    Dim bmp As New Bitmap(Math.Max(1, CInt(img.Width * scale)), Math.Max(1, CInt(img.Height * scale)))
                    Using g As Graphics = Graphics.FromImage(bmp)
                        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                        g.DrawImage(img, 0, 0, bmp.Width, bmp.Height)
                    End Using
                    Const OrientationId As Integer = &H112
                    If img.PropertyIdList.Contains(OrientationId) Then
                        Select Case img.GetPropertyItem(OrientationId).Value(0)
                            Case 3 : bmp.RotateFlip(RotateFlipType.Rotate180FlipNone)
                            Case 6 : bmp.RotateFlip(RotateFlipType.Rotate90FlipNone)
                            Case 8 : bmp.RotateFlip(RotateFlipType.Rotate270FlipNone)
                        End Select
                    End If
                    Return bmp
                End Using
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            web.Dispose()
            For Each p In picThumbs
                p?.Image?.Dispose()
            Next
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
