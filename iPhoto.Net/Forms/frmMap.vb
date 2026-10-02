Imports System.IO
Imports System.Text.Json
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms

' 地點 (the tool bar's 地點 button; new in the .NET port):
'   left   the place tree 縣市 › 鄉鎮區 › 景點 · 其他地點 (PlaceBrowse, from PhotoIndex), with photo
'          counts and a 搜尋 box; a click shows the place's photos in the main window (PlacePicked) and
'          moves the map to them. Works without the internet.
'   right  every photo with a GPS position (PhotoIndex Exif_Latitude / Exif_Longitude) as clustered
'          markers on OpenStreetMap -- Leaflet in WebView2, the page is Map\map.html (embedded, served as
'          https://iphoto-map.local/ by MapHost, so the map tiles get a proper referrer). A marker shows the photo's thumbnail (made here, sent as a data url); 「顯示畫面中
'          的照片」 or a right-click on a cluster puts those photos in the main window (PhotosPicked),
'          「在 iPhoto 顯示這一天」 opens the photo's day in the 時間軸 (PhotoOpened). Needs the internet.
' The window stays open beside the main window. Made in code (no designer file).
Friend Class frmMap
    Inherits Aqua.iForm   ' the main window's look (iForm, brushed metal, the traffic-light buttons)

    Public Event PhotosPicked(ByVal title As String, ByVal files As List(Of String))
    Public Event PhotoOpened(ByVal file As String)
    Public Event PlacePicked(ByVal place As PlaceBrowse.PlaceNode)

    Private Const Edge As Integer = 12          ' as the main window: 12 from the sides
    Private Const MarginTop As Integer = 30           ' below the 23-pixel title bar
    Private Const MarginBottom As Integer = 18        ' room for the resize grip
    Private Const TreeWidth As Integer = 300

    Private ReadOnly m_web As New WebView2
    Private ReadOnly lblStatus As New Label
    Private ReadOnly txtFind As New Aqua.ITextBox
    Private ReadOnly pnlTree As New Aqua.Panel
    Private ReadOnly pnlMap As New Aqua.Panel
    Private ReadOnly tvPlaces As New TreeView
    Private m_lpRows As List(Of Database.IndexRow)
    Private m_lpPlaces As List(Of PlaceBrowse.PlaceNode)
    Private m_bolPageReady As Boolean

    Public Sub New()
        Dim mainRes As New ComponentModel.ComponentResourceManager(GetType(frmMain))
        Text = "地點"
        Font = New Font("華康細圓體", 12.0F)
        TitleFont = New Font("Times New Roman", 14.25F, FontStyle.Bold)
        Try
            Image = CType(mainRes.GetObject("$this.Image"), Image)   ' the main window's background
            SizeMode = Aqua.ImageSizeMode.StretchImage
        Catch ex As Exception When TypeOf ex Is Resources.MissingManifestResourceException OrElse TypeOf ex Is InvalidCastException
            BackColor = Color.FromArgb(236, 239, 243)
        End Try
        Shadow = False
        WindowBorderStyle = Aqua.FormBorderStyle.Sizable
        StartPosition = FormStartPosition.CenterScreen
        ClientSize = New Size(1280, 780)   ' the size when it is restored
        MinimumSize = New Size(760, 440)
        WindowState = FormWindowState.Maximized
        ShowInTaskbar = True

        ' left: 搜尋 (as the main window's search box), the place tree in a frame (as its tree)
        txtFind.SetBounds(Edge, MarginTop, TreeWidth, 23)
        txtFind.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        txtFind.Font = New Font("華康細圓體", 12.0F)
        txtFind.BackColor = Color.White
        txtFind.ParheliaColor = Color.FromArgb(159, 182, 244)
        Try
            txtFind.Icon = CType(mainRes.GetObject("iTextBox1.Icon"), Image)   ' the magnifier
        Catch ex As Exception When TypeOf ex Is Resources.MissingManifestResourceException OrElse TypeOf ex Is InvalidCastException
        End Try
        pnlTree.PanelStyle = Aqua.PanelStyleMode.Container
        pnlTree.BorderColor = Color.FromArgb(189, 189, 189)
        pnlTree.BorderFocusColor = Color.FromArgb(159, 182, 244)
        pnlTree.BackColor = Color.White
        pnlTree.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        tvPlaces.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        tvPlaces.Font = New Font("華康細圓體", 14.25F)
        tvPlaces.FullRowSelect = True
        tvPlaces.HideSelection = False
        tvPlaces.BorderStyle = BorderStyle.None
        If g_lpConfig IsNot Nothing Then tvPlaces.ImageList = g_lpConfig.ImageListSubject   ' the main tree's icons
        pnlTree.Controls.Add(tvPlaces)
        Controls.Add(txtFind)
        Controls.Add(pnlTree)
        AddHandler tvPlaces.NodeMouseClick, Sub(s, e)
                                                If e.Button <> MouseButtons.Left Then Return
                                                If (tvPlaces.HitTest(e.Location).Location And TreeViewHitTestLocations.PlusMinus) <> 0 Then Return
                                                tvPlaces.SelectedNode = e.Node
                                                PickPlace(e.Node)
                                            End Sub
        AddHandler tvPlaces.KeyDown, Sub(s, e)
                                         If e.KeyCode = Keys.Enter AndAlso tvPlaces.SelectedNode IsNot Nothing Then
                                             e.SuppressKeyPress = True
                                             PickPlace(tvPlaces.SelectedNode)
                                         End If
                                     End Sub
        AddHandler txtFind.KeyDown, Sub(s, e)
                                        If e.KeyCode <> Keys.Enter Then Return
                                        e.SuppressKeyPress = True
                                        FindPlace(txtFind.Text.Trim())
                                    End Sub
        AddHandler txtFind.ImageClick, Sub() FindPlace(txtFind.Text.Trim())
        AddHandler tvPlaces.GotFocus, Sub() pnlTree.BorderColor = pnlTree.BorderFocusColor
        AddHandler tvPlaces.LostFocus, Sub() pnlTree.BorderColor = Color.FromArgb(189, 189, 189)

        ' right: the map in a frame (as the main window's thumbnails)
        pnlMap.PanelStyle = Aqua.PanelStyleMode.Container
        pnlMap.BorderColor = Color.FromArgb(189, 189, 189)
        pnlMap.BorderFocusColor = Color.FromArgb(159, 182, 244)
        pnlMap.BackColor = Color.FromArgb(238, 241, 244)
        pnlMap.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        m_web.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lblStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lblStatus.TextAlign = ContentAlignment.MiddleCenter
        lblStatus.BackColor = Color.FromArgb(238, 241, 244)
        lblStatus.Text = "開啟地圖中…"
        pnlMap.Controls.Add(m_web)
        pnlMap.Controls.Add(lblStatus)
        lblStatus.BringToFront()
        Controls.Add(pnlMap)
        LayoutFrames()
    End Sub

    ''' <summary>The two frames for the current size (then their anchors keep them).</summary>
    Private Sub LayoutFrames()
        Dim h As Integer = Math.Max(100, ClientSize.Height - MarginTop - MarginBottom)
        pnlTree.SetBounds(Edge, MarginTop + 23 + 8, TreeWidth, h - 23 - 8)
        tvPlaces.SetBounds(4, 4, pnlTree.Width - 8, pnlTree.Height - 8)
        Dim mapLeft As Integer = Edge + TreeWidth + 10
        pnlMap.SetBounds(mapLeft, MarginTop, Math.Max(100, ClientSize.Width - mapLeft - Edge), h)
        m_web.SetBounds(4, 4, pnlMap.Width - 8, pnlMap.Height - 8)
        lblStatus.Bounds = m_web.Bounds
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If Owner IsNot Nothing Then Icon = Owner.Icon
        UpdateMaximizedBounds()
        If WindowState = FormWindowState.Maximized Then
            ' it was maximised (New) before MaximizedBounds was set: over the task bar -- again, inside it
            WindowState = FormWindowState.Normal
            WindowState = FormWindowState.Maximized
        End If
        LoadPlaces()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        MapHost.TrimCache()   ' the tiles kept stay under 設定 › 地點's limit (in the background)
    End Sub

    ''' <summary>A borderless window maximises over the whole monitor, task bar included: keep it to the
    ''' working area of the monitor it is on (as the main window does).</summary>
    Private Sub UpdateMaximizedBounds()
        Dim scr As Screen = Screen.FromControl(Me)
        Dim wa As Rectangle = scr.WorkingArea
        MaximizedBounds = New Rectangle(wa.X - scr.Bounds.X, wa.Y - scr.Bounds.Y, wa.Width, wa.Height)
    End Sub

    Protected Overrides Sub OnLocationChanged(e As EventArgs)
        If WindowState = FormWindowState.Normal Then UpdateMaximizedBounds()
        MyBase.OnLocationChanged(e)
    End Sub
    '==================================================================================================
    ' The place tree
    '==================================================================================================
    Private Sub LoadPlaces()
        Dim selected As String = TryCast(tvPlaces.SelectedNode?.Tag, PlaceBrowse.PlaceNode)?.Path
        m_lpPlaces = If(g_lpDatabase Is Nothing, New List(Of PlaceBrowse.PlaceNode), PlaceBrowse.Build(g_lpDatabase.PlaceRows()))
        tvPlaces.BeginUpdate()
        tvPlaces.Nodes.Clear()
        For Each p In m_lpPlaces
            AddPlaceNodes(tvPlaces.Nodes, p)
        Next
        tvPlaces.EndUpdate()
        If selected IsNot Nothing Then
            Dim n As TreeNode = FindNode(tvPlaces.Nodes, Function(x) CType(x.Tag, PlaceBrowse.PlaceNode).Path = selected)
            If n IsNot Nothing Then tvPlaces.SelectedNode = n
        End If
        If m_lpPlaces.Count = 0 Then tvPlaces.Nodes.Add("（還沒有照片設定了地點）")
    End Sub

    ''' <summary>After places were written (設定拍攝地點): the tree and the markers again.</summary>
    Public Sub ReloadPlaces()
        LoadPlaces()
        If m_bolPageReady Then SendPoints(keepView:=True)
    End Sub

    Private Shared Sub AddPlaceNodes(ByVal nodes As TreeNodeCollection, ByVal p As PlaceBrowse.PlaceNode)
        Dim n As TreeNode = nodes.Add(p.ToString())
        n.Tag = p
        n.ImageKey = If(p.Name = PlaceBrowse.OtherPlaces, "icoOther", "icoTravel")
        n.SelectedImageKey = n.ImageKey
        For Each c In p.Children
            AddPlaceNodes(n.Nodes, c)
        Next
    End Sub

    Private Shared Function FindNode(ByVal nodes As TreeNodeCollection, ByVal match As Func(Of TreeNode, Boolean)) As TreeNode
        For Each n As TreeNode In nodes
            If n.Tag IsNot Nothing AndAlso match(n) Then Return n
            Dim hit As TreeNode = FindNode(n.Nodes, match)
            If hit IsNot Nothing Then Return hit
        Next
        Return Nothing
    End Function

    ''' <summary>搜尋: the next place (after the selected one) whose name has the text.</summary>
    Private Sub FindPlace(ByVal q As String)
        If q = "" Then Return
        q = q.Replace("台", "臺")
        Dim all As New List(Of TreeNode)
        Dim walk As Action(Of TreeNodeCollection) = Nothing
        walk = Sub(nodes)
                   For Each n As TreeNode In nodes
                       If n.Tag IsNot Nothing Then all.Add(n)
                       walk(n.Nodes)
                   Next
               End Sub
        walk(tvPlaces.Nodes)
        Dim start As Integer = all.IndexOf(tvPlaces.SelectedNode) + 1
        For k = 0 To all.Count - 1
            Dim n As TreeNode = all((start + k) Mod all.Count)
            If CType(n.Tag, PlaceBrowse.PlaceNode).Name.Replace("台", "臺").Contains(q) Then
                tvPlaces.SelectedNode = n
                n.EnsureVisible()
                PickPlace(n)
                Return
            End If
        Next
        Text = "地點 — 找不到「" & q & "」"
    End Sub

    ''' <summary>A place clicked: its photos in the main window, the map moved to the ones with GPS.</summary>
    Private Sub PickPlace(ByVal n As TreeNode)
        Dim p As PlaceBrowse.PlaceNode = TryCast(n?.Tag, PlaceBrowse.PlaceNode)
        If p Is Nothing Then Return
        RaiseEvent PlacePicked(p)
        Dim gps = p.Rows.Where(Function(r) r.Latitude.HasValue AndAlso r.Longitude.HasValue).ToList()
        If gps.Count > 0 AndAlso m_bolPageReady Then
            Post(New Dictionary(Of String, Object) From {{"type", "fit"},
                 {"s", gps.Min(Function(r) r.Latitude.Value)}, {"w", gps.Min(Function(r) r.Longitude.Value)},
                 {"n", gps.Max(Function(r) r.Latitude.Value)}, {"e", gps.Max(Function(r) r.Longitude.Value)}})
        End If
        Activate()   ' the main window was filled; this one stays in front
    End Sub

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If Owner IsNot Nothing Then Icon = Owner.Icon
        Try
            Dim data As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto")
            Dim env As CoreWebView2Environment = Await CoreWebView2Environment.CreateAsync(Nothing, Path.Combine(data, "WebView2"))
            Await m_web.EnsureCoreWebView2Async(env)
            With m_web.CoreWebView2
                .Settings.AreDevToolsEnabled = False
                .Settings.IsStatusBarEnabled = False
                .Settings.AreDefaultContextMenusEnabled = False
                AddHandler .WebMessageReceived, AddressOf Web_Message
                AddHandler .NewWindowRequested, AddressOf Web_NewWindow
            End With
            MapHost.ServeAndNavigate(m_web.CoreWebView2, "map.html")
            lblStatus.Visible = False
        Catch ex As Exception
            ' no WebView2 runtime, or it failed to start
            lblStatus.Text = "無法開啟地圖：" & ex.Message & vbCrLf & vbCrLf &
                             "地圖需要 Microsoft Edge WebView2 執行階段（Windows 11 內建）。"
        End Try
    End Sub

    ''' <summary>Links (the OpenStreetMap credit) open in the browser, not in a new WebView window.</summary>
    Private Sub Web_NewWindow(sender As Object, e As CoreWebView2NewWindowRequestedEventArgs)
        e.Handled = True
        Try
            Process.Start(New ProcessStartInfo(e.Uri) With {.UseShellExecute = True})
        Catch
        End Try
    End Sub

    Private Sub Web_Message(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Dim msg As JsonElement
        Try
            msg = JsonDocument.Parse(e.WebMessageAsJson).RootElement
        Catch
            Return
        End Try
        Select Case msg.GetProperty("type").GetString()
            Case "ready"
                m_bolPageReady = True
                SendPoints()
            Case "thumb" : SendThumb(msg.GetProperty("id").GetInt32())
            Case "help" : frmMapHelp.ShowHelp(Me)   ' the 線上 / 離線 badge clicked
            Case "jserror" : MapHost.LogPageError("地點 " & msg.GetProperty("msg").GetString())
            Case "pick"
                Dim ids As List(Of Integer) = msg.GetProperty("ids").EnumerateArray().Select(Function(x) x.GetInt32()).ToList()
                Pick(ids, msg.GetProperty("all").GetBoolean())
            Case "open"
                Dim r As Database.IndexRow = RowOf(msg.GetProperty("id").GetInt32())
                If r IsNot Nothing Then RaiseEvent PhotoOpened(r.FileName)
        End Select
    End Sub

    Private Function RowOf(ByVal id As Integer) As Database.IndexRow
        If m_lpRows Is Nothing OrElse id < 0 OrElse id >= m_lpRows.Count Then Return Nothing
        Return m_lpRows(id)
    End Function

    ''' <summary>The markers: [lat, lon, id, year, "2024/3/15 · 九族文化村"]; id = index in m_lpRows.</summary>
    Private Sub SendPoints(Optional ByVal keepView As Boolean = False)
        If g_lpDatabase Is Nothing Then Return
        m_lpRows = g_lpDatabase.GpsRows().Where(Function(r) File.Exists(r.FileName)).ToList()
        Dim points As New List(Of Object())
        For i = 0 To m_lpRows.Count - 1
            Dim r As Database.IndexRow = m_lpRows(i)
            Dim d As Date? = r.ShotDate
            Dim text As String = If(d.HasValue, d.Value.ToString("yyyy/M/d"), "沒有日期")
            Dim place As String = If(r.Spot <> "", r.Spot, r.City)
            If place <> "" Then text &= " · " & place
            points.Add({Math.Round(r.Latitude.Value, 6), Math.Round(r.Longitude.Value, 6), i, If(d.HasValue, d.Value.Year, 0), text})
        Next
        Post(New Dictionary(Of String, Object) From {{"type", "points"}, {"points", points}, {"keep", keepView}})
        Text = "地點 — " & m_lpRows.Count.ToString("#,0") & " 張照片有拍攝位置"
    End Sub

    ''' <summary>A 200 x 150 thumbnail, made on a worker thread ("" when the photo can't be read).</summary>
    Private Sub SendThumb(ByVal id As Integer)
        Dim r As Database.IndexRow = RowOf(id)
        If r Is Nothing Then Return
        Dim file As String = r.FileName
        Threading.Tasks.Task.Run(Function() MakeThumb(file)).ContinueWith(
            Sub(t)
                If IsDisposed OrElse Not IsHandleCreated Then Return
                BeginInvoke(Sub()
                                If m_web.CoreWebView2 Is Nothing Then Return
                                Post(New Dictionary(Of String, Object) From {{"type", "thumb"}, {"id", id}, {"url", If(t.IsFaulted, "", t.Result)}})
                            End Sub)
            End Sub)
    End Sub

    Private Shared Function MakeThumb(ByVal file As String) As String
        Try
            Using src As Image = LoadRotated(file)
                If src Is Nothing Then Return ""
                Dim scale As Double = Math.Min(400.0 / src.Width, 300.0 / src.Height)
                Dim w As Integer = Math.Max(1, CInt(src.Width * Math.Min(1.0, scale)))
                Dim h As Integer = Math.Max(1, CInt(src.Height * Math.Min(1.0, scale)))
                Using bmp As New Bitmap(w, h)
                    Using g As Graphics = Graphics.FromImage(bmp)
                        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                        g.DrawImage(src, 0, 0, w, h)
                    End Using
                    Using ms As New MemoryStream()
                        bmp.Save(ms, Imaging.ImageFormat.Jpeg)
                        Return "data:image/jpeg;base64," & Convert.ToBase64String(ms.ToArray())
                    End Using
                End Using
            End Using
        Catch
            Return ""
        End Try
    End Function

    ''' <summary>The picture turned as its EXIF orientation says (as the viewer shows it).</summary>
    Private Shared Function LoadRotated(ByVal file As String) As Image
        Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim img As Image = Image.FromStream(fs, False, False)
            Dim bmp As New Bitmap(img)
            Const OrientationId As Integer = &H112
            If img.PropertyIdList.Contains(OrientationId) Then
                Select Case img.GetPropertyItem(OrientationId).Value(0)
                    Case 3 : bmp.RotateFlip(RotateFlipType.Rotate180FlipNone)
                    Case 6 : bmp.RotateFlip(RotateFlipType.Rotate90FlipNone)
                    Case 8 : bmp.RotateFlip(RotateFlipType.Rotate270FlipNone)
                End Select
            End If
            img.Dispose()
            Return bmp
        End Using
    End Function

    Private Sub Pick(ByVal ids As List(Of Integer), ByVal wholeView As Boolean)
        Dim rows As List(Of Database.IndexRow) = ids.Select(Function(i) RowOf(i)).Where(Function(r) r IsNot Nothing).ToList()
        If rows.Count = 0 Then Return
        Dim place As String = PlaceBrowse.MainPlace(rows)
        Dim title As String = If(place <> "", "地圖：" & place & If(wholeView, " 一帶", " 附近"), "地圖上選的照片")
        RaiseEvent PhotosPicked(title, rows.Select(Function(r) r.FileName).ToList())
        ' (one screen: the main window brings itself forward with its list; two: this window stays in front)
    End Sub

    Private Sub Post(ByVal message As Object)
        m_web.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(message))
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then m_web.Dispose()
        MyBase.Dispose(disposing)
    End Sub

End Class
