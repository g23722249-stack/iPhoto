' 時間軸 in the main window's tree and the 地點 button of the tool bar (new in the .NET port), both from
' PhotoIndex (Database.Browse.vb):
'   時間軸     a node after 面孔 (both modes), filled when first opened: 那年今日 · one node per year ›
'              its months · 沒有日期. A year is one list, oldest first, with a heading per day ("3月15日
'              星期五  臺中市 · 12 張", Aqua.MediaList sections); a month scrolls to its first day (the year
'              is loaded once). The 時間軸 node itself shows the latest year.
'   地點       the tool bar's 地點 (imgToolBox 13, after 照片目錄; picture and caption in the designer) opens frmMap: the map, with the place
'              tree 縣市 › 鄉鎮區 › 景點 · 其他地點 (PlaceBrowse) on its left. A place picked there shows
'              its photos here by date, a heading per day.
' The lists are ordinary photo lists (exeFace: files from anywhere, as the face / search lists): viewer,
' rating, dock, file operations all work. A click on a heading selects that day's first photo.
Partial Class frmMain

    Private Const TimeKey As String = "TIME"
    Private Const TimeTodayKey As String = "TIME:TODAY"
    Private Const TimeNoDateKey As String = "TIME:NODATE"
    Private Const TimeYearPrefix As String = "TIME:Y"      ' TIME:Y2024
    Private Const TimeMonthPrefix As String = "TIME:M"     ' TIME:M2024-03
    Private Const BrowseWaitSuffix As String = ":WAIT"

    Private Shared ReadOnly TwCulture As New Globalization.CultureInfo("zh-TW")

    ''' <summary>The rows the list shows, by mlList index (a browse list), else empty.</summary>
    Private ReadOnly m_lpBrowseRows As New List(Of Database.IndexRow)
    ''' <summary>The year the list shows (時間軸), 0 when it shows something else.</summary>
    Private m_intBrowseYear As Integer

    ''' <summary>Called after the tree is (re)built, after 面孔.</summary>
    Private Sub AddBrowseNodes()
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return
        tvList.BeginUpdate()
        AddBrowseChild(AddBrowseTop(TimeKey, "時間軸", "icoFilm"), TimeKey & BrowseWaitSuffix, "…", "icoFilm")
        tvList.EndUpdate()
    End Sub

    Private Function AddBrowseTop(ByVal key As String, ByVal text As String, ByVal icon As String) As TreeNode
        Dim n As TreeNode = tvList.Nodes.Add(key, text)
        n.ImageKey = icon
        n.SelectedImageKey = icon
        n.Tag = key
        Return n
    End Function

    Private Shared Function AddBrowseChild(ByVal parent As TreeNode, ByVal key As String, ByVal text As String, ByVal icon As String) As TreeNode
        Dim c As TreeNode = parent.Nodes.Add(key, text)
        c.ImageKey = icon
        c.SelectedImageKey = icon
        c.Tag = key
        Return c
    End Function

    Private Shared Function BrowseKey(ByVal node As TreeNode) As String
        Dim key As String = If(node Is Nothing, Nothing, TryCast(node.Tag, String))
        If key Is Nothing OrElse Not key.StartsWith(TimeKey) Then Return Nothing
        Return key
    End Function

    Private Sub Browse_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvList.BeforeExpand
        If BrowseKey(e.Node) = TimeKey Then FillTimeNode(e.Node)
    End Sub

    ''' <summary>時間軸's children, once (the wait node goes).</summary>
    Private Sub FillTimeNode(ByVal n As TreeNode)
        If n.Nodes.Count <> 1 OrElse Not n.Nodes(0).Name.EndsWith(BrowseWaitSuffix) Then Return
        SetBusy(True)
        tvList.BeginUpdate()
        Try
            n.Nodes.Clear()
            AddBrowseChild(n, TimeTodayKey, "那年今日", "icoBirthday")
            Dim total As Integer = 0
            For Each y In g_lpDatabase.TimelineYears()
                Dim yn As TreeNode = AddBrowseChild(n, TimeYearPrefix & y.Key, y.Key & " 年 (" & y.Value.ToString("#,0") & ")", "icoFilm")
                For Each m In g_lpDatabase.TimelineMonths(y.Key)
                    If m.Key < 1 OrElse m.Key > 12 Then Continue For
                    AddBrowseChild(yn, TimeMonthPrefix & y.Key & "-" & m.Key.ToString("00"), m.Key & " 月 (" & m.Value.ToString("#,0") & ")", "icoFilm")
                Next
                total += y.Value
            Next
            Dim none As Integer = g_lpDatabase.NoDateCount()
            If none > 0 Then AddBrowseChild(n, TimeNoDateKey, "沒有日期 (" & none.ToString("#,0") & ")", "icoOther")
            n.Text = "時間軸 (" & (total + none).ToString("#,0") & ")"
        Finally
            tvList.EndUpdate()
            SetBusy(False)
        End Try
    End Sub

    ''' <summary>After places were written: the map window (when open) reads the places again.</summary>
    Private Sub RefreshPlaceNode()
        If m_frmMap IsNot Nothing AndAlso Not m_frmMap.IsDisposed Then m_frmMap.ReloadPlaces()
    End Sub
    ''' <summary>Called by ClearScreenAlbum.</summary>
    Private Sub EndBrowseView()
        m_lpBrowseRows.Clear()
        m_intBrowseYear = 0
    End Sub

    ''' <summary>Called first by tvList_Click (after 面孔): True when the node is 時間軸 or below.</summary>
    Private Function BrowseNodeClick(ByVal node As TreeNode) As Boolean
        Dim key As String = BrowseKey(node)
        If key Is Nothing Then Return False
        If key.EndsWith(BrowseWaitSuffix) Then Return True
        Select Case True
            Case key = TimeKey
                FillTimeNode(node)
                Dim last As TreeNode = node.Nodes.Cast(Of TreeNode)().LastOrDefault(Function(x) CStr(x.Tag).StartsWith(TimeYearPrefix))
                If last IsNot Nothing Then ShowTimelineYear(CInt(CStr(last.Tag).Substring(TimeYearPrefix.Length)), 0, key)
            Case key = TimeTodayKey : ShowOnThisDay(key)
            Case key = TimeNoDateKey : ShowNoDate(key)
            Case key.StartsWith(TimeYearPrefix) : ShowTimelineYear(CInt(key.Substring(TimeYearPrefix.Length)), 0, key)
            Case key.StartsWith(TimeMonthPrefix)
                Dim ym() As String = key.Substring(TimeMonthPrefix.Length).Split("-"c)
                ShowTimelineYear(CInt(ym(0)), CInt(ym(1)), key)
        End Select
        Return True
    End Function

    '==================================================================================================
    ' The lists
    '==================================================================================================
    Private Sub ShowTimelineYear(ByVal year As Integer, ByVal month As Integer, ByVal key As String)
        If m_intBrowseYear <> year Then
            Dim rows As List(Of Database.IndexRow) = Existing(g_lpDatabase.TimelineRows(year))
            FillSections(rows.GroupBy(Function(r) r.DateText).Select(Function(g) (DayTitle(g.ToList(), False, 0), g.ToList())))
            m_intBrowseYear = year
            txtTitle.Text = year & " 年"
            txtRemark.Text = rows.Count.ToString("#,0") & " 張照片，" & mlList.SectionCount & " 天"
        End If
        m_strShownKey = key
        ' a month: its first day at the top; the year: the start
        Dim s As Integer = 0
        If month > 0 Then
            s = -1
            For i = 0 To mlList.SectionCount - 1
                If m_lpBrowseRows(mlList.SectionFirstIndex(i)).Month >= month Then s = i : Exit For
            Next
        End If
        If s >= 0 AndAlso s < mlList.SectionCount Then
            mlList.SelectedIndex = mlList.SectionFirstIndex(s)
            mlList.ScrollToSection(s)
        End If
        If mlList.Visible Then mlList.Focus()
    End Sub

    ''' <summary>那年今日: today in the years before, newest first; nothing on the very day widens to ±3 days.</summary>
    Private Sub ShowOnThisDay(ByVal key As String)
        Dim today As Date = Date.Today
        Dim rows As New List(Of Database.IndexRow)
        Dim widened As Boolean = False
        For Each offset In {0, -1, 1, -2, 2, -3, 3}
            If offset <> 0 AndAlso rows.Count > 0 Then Exit For
            Dim d As Date = today.AddDays(offset)
            rows.AddRange(Existing(g_lpDatabase.OnThisDayRows(d.Month, d.Day, today.Year)))
            widened = offset <> 0
        Next
        If rows.Count = 0 Then
            ClearScreenAlbum()
            txtTitle.Text = "那年今日"
            txtRemark.Text = "往年的今天前後三天都沒有照片"
            m_strShownKey = key
            Return
        End If
        ' newest year first; within a year by date
        Dim groups = rows.GroupBy(Function(r) r.Year).OrderByDescending(Function(g) g.Key).
                          SelectMany(Function(g) g.OrderBy(Function(r) r.DateText).ThenBy(Function(r) r.FileName).GroupBy(Function(r) r.DateText)).
                          Select(Function(g) (DayTitle(g.ToList(), True, today.Year - g.First().Year), g.ToList())).ToList()
        FillSections(groups)
        txtTitle.Text = "那年今日"
        txtRemark.Text = today.ToString("M月d日", TwCulture) & If(widened, " 前後幾天", "") & "：" &
                         groups.Select(Function(g) g.Item2(0).Year).Distinct().Count() & " 個年份，" & rows.Count & " 張照片"
        m_strShownKey = key
        If mlList.Visible Then mlList.Focus()
    End Sub

    Private Sub ShowNoDate(ByVal key As String)
        Dim rows As List(Of Database.IndexRow) = Existing(g_lpDatabase.NoDateRows())
        ' by folder: photos without a date were usually imported together
        FillSections(rows.GroupBy(Function(r) IO.Path.GetDirectoryName(r.FileName)).
                          Select(Function(g) (IO.Path.GetFileName(g.Key) & vbTab & g.Count() & " 張", g.ToList())))
        txtTitle.Text = "沒有日期"
        txtRemark.Text = rows.Count & " 張照片沒有拍攝日期（可以用「批次修改資訊」補上）"
        m_strShownKey = key
    End Sub

    ''' <summary>A place picked in the map window's place tree: its photos, a heading per day.</summary>
    Private Sub ShowPlace(ByVal p As PlaceBrowse.PlaceNode)
        Dim rows As List(Of Database.IndexRow) = Existing(p.Rows)
        If PlaceViewerActive Then   ' two screens: into the viewer's strip, this list stays (frmMain.PlaceViewer.vb)
            ShowInPlaceViewer(p.Name, rows)
            Return
        End If
        FillSections(rows.GroupBy(Function(r) r.DateText).Select(Function(g) (DayTitle(g.ToList(), True, 0, p.Children.Count > 0 OrElse p.Name = PlaceBrowse.OtherPlaces), g.ToList())))
        txtTitle.Text = p.Name
        Dim dated = rows.Where(Function(r) r.ShotDate.HasValue).ToList()
        txtRemark.Text = rows.Count.ToString("#,0") & " 張照片" &
                         If(dated.Count > 0, "，" & dated.First().ShotDate.Value.ToString("yyyy/M/d") &
                                             If(dated.Count > 1 AndAlso dated.Last().DateText <> dated.First().DateText, " ～ " & dated.Last().ShotDate.Value.ToString("yyyy/M/d"), ""), "") &
                         "，去過 " & mlList.SectionCount & " 天"
        If mlList.Visible Then mlList.Focus()
    End Sub

    ''' <summary>The rows whose photo is still there.</summary>
    Private Shared Function Existing(ByVal rows As IEnumerable(Of Database.IndexRow)) As List(Of Database.IndexRow)
        Return rows.Where(Function(r) r.FileName <> "" AndAlso IO.File.Exists(r.FileName)).ToList()
    End Function

    ''' <summary>A day's heading: "3月15日 星期五" (with the year when <paramref name="withYear"/>), then in
    ''' grey how long ago (那年今日), where (the most common 地點) and how many.</summary>
    Private Shared Function DayTitle(ByVal rows As List(Of Database.IndexRow), ByVal withYear As Boolean, ByVal yearsAgo As Integer,
                                     Optional ByVal withPlace As Boolean = True) As String
        Dim d As Date? = rows(0).ShotDate
        Dim head As String = If(d.HasValue, d.Value.ToString(If(withYear, "yyyy年M月d日 dddd", "M月d日 dddd"), TwCulture), "沒有日期")
        Dim parts As New List(Of String)
        If yearsAgo > 0 Then parts.Add(yearsAgo & " 年前")
        If withPlace Then
            Dim place As String = PlaceBrowse.MainPlace(rows)
            If place <> "" Then parts.Add(place)
        End If
        parts.Add(rows.Count & " 張")
        Return head & vbTab & String.Join(" · ", parts)
    End Function

    ''' <summary>Fills mlList with one section per group (a heading, then its photos).</summary>
    Private Sub FillSections(ByVal groups As IEnumerable(Of (Title As String, Rows As List(Of Database.IndexRow))))
        Dim list = groups.Where(Function(g) g.Rows.Count > 0).ToList()
        SetBusy(True)
        Enabled = False
        Try
            ClearScreenAlbum()
            With m_lpAppEnv
                .ExeMode = enumExeMode.exeFace   ' a list of files from anywhere (as the face / search lists)
                .SectionIndex = -1
                .KeyIndex = -1
            End With
            mlList.Clear()
            sliSize.Value = LimitFor(list.Sum(Function(g) g.Rows.Count))
            mlList.BeginUpdate()   ' a year is thousands of photos: lay them out once at the end
            Try
                For Each g In list
                    mlList.AddSection(g.Title)
                    For Each r In g.Rows
                        Dim item As Aqua.MediaItem = mlList.AddItem(r.FileName)
                        item.Ranking = If(r.Ranking >= 1 AndAlso r.Ranking <= 5, CType(r.Ranking, Aqua.MediaItemRanking), Aqua.MediaItemRanking.NoRating)
                        m_lpBrowseRows.Add(r)
                    Next
                Next
            Finally
                mlList.EndUpdate()
            End Try
            DockedMediaList(g_lpDock, mlList)
            MoveNoteToScreen(m_lpAppEnv.ExeMode, -1, -1)
            If mlList.Count > 0 Then
                mlList.ScrollValue = mlList.ScrollMin
                mlList.SelectedIndex = 0
            End If
        Finally
            Enabled = True
            SetBusy(False)
        End Try
    End Sub

    Private Sub Browse_SectionClick(sectionIndex As Integer, e As MouseEventArgs) Handles mlList.SectionClick
        If ListShowsCards OrElse sectionIndex < 0 OrElse sectionIndex >= mlList.SectionCount Then Return
        Dim first As Integer = mlList.SectionFirstIndex(sectionIndex)
        If first < mlList.Count Then mlList.SelectedIndex = first
    End Sub

    '==================================================================================================
    ' 地點 (frmMap): a place of its tree, or photos picked on its map, show here, a heading per day
    '==================================================================================================
    Private m_frmMap As frmMap

    Private Sub ShowMap()
        If m_frmMap Is Nothing OrElse m_frmMap.IsDisposed Then
            m_frmMap = New frmMap()
            AddHandler m_frmMap.PhotosPicked, AddressOf Map_PhotosPicked
            AddHandler m_frmMap.PhotoOpened, AddressOf Map_PhotoOpened
            AddHandler m_frmMap.PlacePicked, AddressOf ShowPlace
            m_frmMap.Show(Me)
            BeginPlaceViewer()   ' two screens: the viewer goes into 地點 mode (frmMain.PlaceViewer.vb)
        Else
            If m_frmMap.WindowState = FormWindowState.Minimized Then m_frmMap.WindowState = FormWindowState.Normal
            m_frmMap.Activate()
        End If
    End Sub

    Private Sub Map_PhotosPicked(ByVal title As String, ByVal files As List(Of String))
        ' the index rows of those photos (for the date / place headings), by date
        Dim rows As List(Of Database.IndexRow) = Existing(g_lpDatabase.RowsOf(files))
        If PlaceViewerActive Then
            ShowInPlaceViewer(title, rows)
            Return
        End If
        FillSections(rows.GroupBy(Function(r) r.DateText).Select(Function(g) (DayTitle(g.ToList(), True, 0), g.ToList())))
        txtTitle.Text = title
        txtRemark.Text = rows.Count & " 張照片，" & mlList.SectionCount & " 天"
        If mlList.Visible Then mlList.Focus()
        Activate()
    End Sub

    ''' <summary>「在 iPhoto 顯示這一天」: the photo's year in the 時間軸, at the photo.</summary>
    Private Sub Map_PhotoOpened(ByVal file As String)
        Dim r As Database.IndexRow = g_lpDatabase.RowsOf({file}).FirstOrDefault()
        If r Is Nothing OrElse r.Year <= 0 Then Return
        If PlaceViewerActive Then   ' two screens: that day in the viewer's strip, at the photo
            Dim day As List(Of Database.IndexRow) = Existing(g_lpDatabase.TimelineRows(r.Year).Where(Function(x) x.DateText = r.DateText))
            ShowInPlaceViewer(If(r.ShotDate.HasValue, r.ShotDate.Value.ToString("yyyy年M月d日", TwCulture), r.DateText), day, file)
            Return
        End If
        Dim key As String = TimeMonthPrefix & r.Year & "-" & r.Month.ToString("00")
        Dim time() As TreeNode = tvList.Nodes.Find(TimeKey, False)
        If time.Length > 0 Then
            FillTimeNode(time(0))
            Dim hits() As TreeNode = time(0).Nodes.Find(key, True)
            If hits.Length > 0 Then tvList.SelectedNode = hits(0)
        End If
        ShowTimelineYear(r.Year, r.Month, key)
        Dim i As Integer = m_lpBrowseRows.FindIndex(Function(x) String.Equals(x.FileName, file, StringComparison.OrdinalIgnoreCase))
        If i >= 0 Then
            mlList.SelectedIndex = i
            Dim s As Integer = mlList.SectionOfItem(i)
            If s >= 0 Then mlList.ScrollToSection(s)
        End If
        Activate()
    End Sub

End Class
