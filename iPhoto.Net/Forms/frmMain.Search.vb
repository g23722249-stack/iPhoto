' 搜尋 (the tool bar's 3): frmPhotoSearch asks; the photos found are shown here as a list from anywhere
' (FillSections, frmMain.Browse.vb) -- a year with fewer than SearchYearSection photos is one section,
' a year with more a section per day -- so viewing, rating, the Dock, 相片後製, 蒙太奇拼貼 and the
' right-click menu all work on them. A 搜尋結果 node at the end of the tree shows the last search again
' (read again from the index, so what changed since is in it).
Partial Class frmMain

    Private Const SearchKey As String = "SEARCH"
    ''' <summary>A year with fewer photos found than this is one section; with more, a section per day.</summary>
    Private Const SearchYearSection As Integer = 100

    Private m_lpLastQuery As PhotoSearch.Query
    Private m_strLastQueryText As String = ""

    Private Sub ShowSearch()
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then
            frmMsgBox.ShowExclamationMessage("搜尋要用到照片索引（設定 › 維護 › 重建索引）。", "搜尋")
            Return
        End If
        Dim album As [Class] = TryCast(If(m_lpAppEnv.ExeMode = enumExeMode.exeAlbums, CurrentSection(), Nothing), [Class])
        Dim records As List(Of PhotoSearch.Record)
        Using f As New frmPhotoSearch(album?.Path.TrimEnd("\"c), album?.DisplayName)
            If f.ShowDialog(Me) <> DialogResult.OK OrElse f.Query Is Nothing Then Return
            m_lpLastQuery = f.Query
            m_strLastQueryText = f.QueryText
            records = f.Records
        End Using
        ShowSearchResult(records)
    End Sub

    ''' <summary>The main window's quick search box: its words as 搜尋's box takes them (no 進階).</summary>
    Private Sub QuickSearch(ByVal text As String)
        If text.Trim() = "" OrElse g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return
        Dim q As PhotoSearch.Query = PhotoSearch.Parse(text, Date.Today)
        If q.IsEmpty Then Return
        m_lpLastQuery = q
        m_strLastQueryText = text.Trim()
        frmPhotoSearch.RememberText(text)
        ShowSearchResult()
    End Sub

    ''' <summary>The last search over <paramref name="records"/> (Nothing: read from the index now) in the list.</summary>
    Private Sub ShowSearchResult(Optional ByVal records As List(Of PhotoSearch.Record) = Nothing)
        If m_lpLastQuery Is Nothing Then Return
        Dim hits As List(Of PhotoSearch.Record)
        SetBusy(True)
        Try
            If records Is Nothing Then records = g_lpDatabase.SearchRecords()
            hits = PhotoSearch.Find(records, m_lpLastQuery).Where(Function(r) IO.File.Exists(r.File)).ToList()
        Finally
            SetBusy(False)
        End Try
        AddSearchNode()
        If hits.Count = 0 Then
            ClearScreenAlbum()
            txtTitle.Text = "搜尋：" & m_strLastQueryText
            txtRemark.Text = "沒有符合的照片"
            m_strShownKey = SearchKey
            SelectSearchNode()
            Return
        End If

        Dim groups As New List(Of (Title As String, Rows As List(Of Database.IndexRow)))
        For Each y In hits.Select(Function(h) h.Row).GroupBy(Function(r) If(r.ShotDate.HasValue, r.Year, 0)).OrderBy(Function(g) If(g.Key = 0, Integer.MaxValue, g.Key))
            Dim rows As List(Of Database.IndexRow) = y.ToList()
            If y.Key = 0 Then
                groups.Add(("沒有日期" & vbTab & rows.Count & " 張", rows))
            ElseIf rows.Count < SearchYearSection Then
                groups.Add((YearTitle(y.Key, rows), rows))
            Else
                For Each d In rows.GroupBy(Function(r) r.DateText)
                    groups.Add((DayTitle(d.ToList(), True, 0), d.ToList()))
                Next
            End If
        Next
        FillSections(groups)
        txtTitle.Text = "搜尋：" & m_strLastQueryText
        Dim years As Integer = hits.Where(Function(h) h.Row.ShotDate.HasValue).Select(Function(h) h.Row.Year).Distinct().Count()
        txtRemark.Text = hits.Count.ToString("#,0") & " 張照片" & If(years > 0, "，" & years & " 個年份", "")
        m_strShownKey = SearchKey
        SelectSearchNode()
        If mlList.Visible Then mlList.Focus()
    End Sub

    ''' <summary>A year's heading: "2024 年", then in grey the most common 地點 and how many.</summary>
    Private Shared Function YearTitle(ByVal year As Integer, ByVal rows As List(Of Database.IndexRow)) As String
        Dim parts As New List(Of String)
        Dim place As String = PlaceBrowse.MainPlace(rows)
        If place <> "" Then parts.Add(place)
        parts.Add(rows.Count & " 張")
        Return year & " 年" & vbTab & String.Join(" · ", parts)
    End Function

    ''' <summary>搜尋結果 at the end of the tree, after 時間軸 (once there has been a search).</summary>
    Private Sub AddSearchNode()
        If m_lpLastQuery Is Nothing Then Return
        Dim n As TreeNode = tvList.Nodes.Cast(Of TreeNode)().FirstOrDefault(Function(x) TryCast(x.Tag, String) = SearchKey)
        If n Is Nothing Then
            n = New TreeNode("搜尋結果") With {.Name = SearchKey, .Tag = SearchKey, .ImageKey = "icoOther", .SelectedImageKey = "icoOther"}
            tvList.Nodes.Add(n)   ' last: the 'first node' fallbacks pick an album
        End If
        n.ToolTipText = m_strLastQueryText
    End Sub

    Private Sub SelectSearchNode()
        Dim n As TreeNode = tvList.Nodes.Cast(Of TreeNode)().FirstOrDefault(Function(x) TryCast(x.Tag, String) = SearchKey)
        If n IsNot Nothing AndAlso tvList.SelectedNode IsNot n Then tvList.SelectedNode = n
    End Sub

    ''' <summary>Called first by tvList_Click: True when the node is 搜尋結果.</summary>
    Private Function SearchNodeClick(ByVal node As TreeNode) As Boolean
        If node Is Nothing OrElse TryCast(node.Tag, String) <> SearchKey Then Return False
        ShowSearchResult()
        Return True
    End Function

End Class
