' 搜尋 (the tool bar's 3; replaces the VB6 port's frmSearch / frmSearchResult): one box -- every word
' must match, in any of the photo's texts (PhotoLib PhotoSearch says which words mean a date, a season,
' a rating) -- with suggestions while typing (people, places, keywords, albums; the recent searches when
' the box is empty) and 進階 for what a word can't say. The photos are read from the index once, when
' the window opens (Records); frmMain.Search.vb shows the result in the photo list.
' The last search is kept (Shared) for the next time the window opens. Made in code (no designer file).
Friend Class frmPhotoSearch
    Inherits Form

    Private Shared s_text As String = ""
    Private Shared s_advanced As Boolean
    Private Shared s_state As AdvancedState

    Private ReadOnly txtQuery As New TextBox
    Private ReadOnly lstSuggest As New ListBox
    Private ReadOnly butSearch As New Aqua.ThinButton
    Private ReadOnly lblHint As New Label
    Private ReadOnly pnlRecent As New FlowLayoutPanel
    Private ReadOnly chkAdvanced As New CheckBox
    Private ReadOnly pnlAdvanced As New Panel
    Private ReadOnly cboDate As New ComboBox, dtFrom As New DateTimePicker, dtTo As New DateTimePicker, cboSeason As New ComboBox
    Private ReadOnly cboRank As New ComboBox, cboMedia As New ComboBox, cboGps As New ComboBox, cboPeople As New ComboBox
    Private ReadOnly chkDock As New CheckBox
    Private ReadOnly cboScope As New ComboBox
    Private ReadOnly txtField(4) As TextBox
    Private ReadOnly butKeyWords As New Button
    Private ReadOnly butClear As New Aqua.ThinButton
    Private ReadOnly butCancel As New Aqua.ThinButton

    Private ReadOnly m_lpScopes As New List(Of (Text As String, Under As String, Book As Book))
    Private m_lpRecords As List(Of PhotoSearch.Record)
    Private m_lpVocabulary As List(Of String)
    Private m_lpQuery As PhotoSearch.Query
    Private m_bolPicking As Boolean

    Private Const BaseHeight As Integer = 196, AdvancedHeight As Integer = 268
    Private Shared ReadOnly DateChoices As String() = {"不限", "今年", "去年", "最近一年", "自訂"}
    Private Shared ReadOnly SeasonChoices As String() = {"不限", "春（3～5 月）", "夏（6～8 月）", "秋（9～11 月）", "冬（12～2 月）"}
    Private Shared ReadOnly SeasonMonths As Integer()() = {Nothing, New Integer() {3, 4, 5}, New Integer() {6, 7, 8}, New Integer() {9, 10, 11}, New Integer() {12, 1, 2}}

    ''' <summary>進階's choices, kept between searches.</summary>
    Private Class AdvancedState
        Public DateIndex As Integer, From As Date, [To] As Date, Season As Integer
        Public Rank As Integer, Media As Integer, Gps As Integer, People As Integer, Dock As Boolean, Scope As String
        Public Fields(4) As String
    End Class

    ''' <param name="albumFolder">The album shown in the main window, offered as a 範圍; Nothing: none.</param>
    Public Sub New(Optional ByVal albumFolder As String = Nothing, Optional ByVal albumName As String = Nothing)
        Text = "搜尋"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False : MaximizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(680, BaseHeight)
        BackColor = Color.White
        KeyPreview = True

        txtQuery.SetBounds(16, 16, 520, 30)
        txtQuery.Font = New Font("Microsoft JhengHei UI", 12.0F)
        butSearch.SetBounds(546, 15, 118, 32) : butSearch.Text = "搜尋"
        lstSuggest.SetBounds(16, 48, 520, 10)
        lstSuggest.Visible = False
        lstSuggest.IntegralHeight = True
        lstSuggest.BorderStyle = BorderStyle.FixedSingle
        lblHint.SetBounds(16, 54, 648, 22)
        lblHint.ForeColor = Color.DimGray
        lblHint.Text = "例：阿嬤 2024 台南 ★4　每個詞都要符合；-詞 排除；也認得 2024/3、3月、今年、夏天。"
        pnlRecent.SetBounds(12, 80, 652, 30)
        pnlRecent.WrapContents = False
        pnlRecent.AutoScroll = False
        chkAdvanced.SetBounds(16, 116, 200, 26) : chkAdvanced.Text = "進階條件"

        pnlAdvanced.SetBounds(16, 146, 648, AdvancedHeight - 8)
        pnlAdvanced.Visible = False
        BuildAdvanced(albumFolder, albumName)

        butClear.Text = "清除條件"
        butCancel.Text = "取消"
        For Each b In {butSearch, butClear, butCancel}
            b.Font = New Font("華康細圓體", 12.0F)
            b.ForeColor = Color.Black
        Next
        PlaceButtons()

        Controls.AddRange({txtQuery, butSearch, lblHint, pnlRecent, chkAdvanced, pnlAdvanced, butClear, butCancel})
        Controls.Add(lstSuggest)
        lstSuggest.BringToFront()

        AddHandler butSearch.Click, Sub() DoSearch()
        AddHandler butCancel.Click, Sub() Close()
        AddHandler butClear.Click, Sub() ClearAll()
        AddHandler chkAdvanced.CheckedChanged, Sub() ShowAdvanced(chkAdvanced.Checked)
        AddHandler txtQuery.TextChanged, Sub() UpdateSuggestions()
        AddHandler txtQuery.KeyDown, AddressOf Query_KeyDown
        AddHandler txtQuery.LostFocus, Sub() If Not lstSuggest.Focused Then lstSuggest.Visible = False
        AddHandler lstSuggest.MouseClick, Sub() AcceptSuggestion()
        AddHandler cboDate.SelectedIndexChanged, Sub()
                                                     dtFrom.Enabled = cboDate.SelectedIndex = 4
                                                     dtTo.Enabled = cboDate.SelectedIndex = 4
                                                 End Sub
        AddHandler butKeyWords.Click, Sub() PickKeyWords()

        Restore()
        FillRecent()
    End Sub

    ''' <summary>The photos read from the index (also for the result).</summary>
    Public ReadOnly Property Records As List(Of PhotoSearch.Record)
        Get
            Return m_lpRecords
        End Get
    End Property

    ''' <summary>What to look for (after OK).</summary>
    Public ReadOnly Property Query As PhotoSearch.Query
        Get
            Return m_lpQuery
        End Get
    End Property

    ''' <summary>The search in words, for the list's title: the box, then 進階's choices.</summary>
    Public ReadOnly Property QueryText As String
        Get
            Dim parts As New List(Of String)
            If txtQuery.Text.Trim() <> "" Then parts.Add(txtQuery.Text.Trim())
            If chkAdvanced.Checked Then
                If cboDate.SelectedIndex = 4 Then
                    parts.Add(dtFrom.Value.ToString("yyyy/M/d") & "～" & dtTo.Value.ToString("yyyy/M/d"))
                ElseIf cboDate.SelectedIndex > 0 Then
                    parts.Add(cboDate.Text)
                End If
                For Each c In {cboSeason, cboRank, cboMedia, cboGps, cboPeople}
                    If c.SelectedIndex > 0 Then parts.Add(c.Text)
                Next
                If chkDock.Checked Then parts.Add("Dock")
                If cboScope.SelectedIndex > 0 Then parts.Add(cboScope.Text)
                For i = 0 To 4
                    If txtField(i).Text.Trim() <> "" Then parts.Add(FieldNames(i) & "：" & txtField(i).Text.Trim())
                Next
            End If
            Return String.Join("　", parts)
        End Get
    End Property

    Private Shared ReadOnly FieldNames As String() = {"主題", "人物", "地點", "備註", "關鍵字"}

    '==================================================================================================
    ' Layout
    '==================================================================================================
    Private Sub BuildAdvanced(ByVal albumFolder As String, ByVal albumName As String)
        Dim lbl = Function(t As String, x As Integer, y As Integer) New Label With {.Text = t, .AutoSize = True, .Location = New Point(x, y + 4)}
        For Each c In {cboDate, cboSeason, cboRank, cboMedia, cboGps, cboPeople, cboScope}
            c.DropDownStyle = ComboBoxStyle.DropDownList
        Next
        cboDate.Items.AddRange(DateChoices) : cboDate.SetBounds(60, 0, 120, 28)
        dtFrom.SetBounds(190, 0, 130, 28) : dtTo.SetBounds(350, 0, 130, 28)
        For Each d In {dtFrom, dtTo}
            d.Format = DateTimePickerFormat.Custom
            d.CustomFormat = "yyyy/MM/dd"
        Next
        cboSeason.Items.AddRange(SeasonChoices) : cboSeason.SetBounds(530, 0, 118, 28)
        cboRank.Items.AddRange({"不限", "★1 以上", "★2 以上", "★3 以上", "★4 以上", "★5"}) : cboRank.SetBounds(60, 36, 120, 28)
        cboMedia.Items.AddRange({"照片和影片", "只有照片", "只有影片"}) : cboMedia.SetBounds(240, 36, 130, 28)
        cboGps.Items.AddRange({"不限", "有 GPS 位置", "沒有 GPS 位置"}) : cboGps.SetBounds(60, 72, 140, 28)
        cboPeople.Items.AddRange({"不限", "有人物", "沒有人物"}) : cboPeople.SetBounds(260, 72, 110, 28)
        chkDock.SetBounds(392, 72, 250, 28) : chkDock.Text = "只找 Dock 裡的照片"

        ' 範圍: the library, the album shown, each section of the library, each photo book
        m_lpScopes.Add(("整個相片庫", Nothing, Nothing))
        If albumFolder IsNot Nothing Then m_lpScopes.Add(("目前的相簿：" & albumName, albumFolder, Nothing))
        For i = 0 To g_lpStorage.AlbumCount - 1
            m_lpScopes.Add(("相片庫：" & g_lpStorage.Album(i).Name, g_lpStorage.Album(i).Path, Nothing))
        Next
        For i = 0 To g_lpStorage.FavoriteCount - 1
            Dim section As Albums = g_lpStorage.Favorite(i)
            section.Load()
            For j = 0 To section.ItemCount - 1
                Dim b As Book = TryCast(section.Item(j), Book)
                If b IsNot Nothing Then m_lpScopes.Add(("攝影集：" & b.DisplayName, Nothing, b))
            Next
        Next
        For Each s In m_lpScopes
            cboScope.Items.Add(s.Text)
        Next
        cboScope.SetBounds(60, 108, 588, 28)

        For i = 0 To 4
            txtField(i) = New TextBox
            Dim col As Integer = i Mod 2, row As Integer = i \ 2
            txtField(i).SetBounds(60 + col * 330, 148 + row * 34, If(i = 4, 226, 258), 28)
        Next
        butKeyWords.SetBounds(290, 148 + 2 * 34, 30, 28) : butKeyWords.Text = "…"

        pnlAdvanced.Controls.AddRange({lbl("日期", 0, 0), cboDate, dtFrom, lbl("～", 326, 0), dtTo, lbl("季節", 488, 0), cboSeason,
                                       lbl("評價", 0, 36), cboRank, lbl("媒體", 196, 36), cboMedia,
                                       lbl("位置", 0, 72), cboGps, lbl("人物", 216, 72), cboPeople, chkDock,
                                       lbl("範圍", 0, 108), cboScope,
                                       lbl(FieldNames(0), 0, 148), txtField(0), lbl(FieldNames(1), 330, 148), txtField(1),
                                       lbl(FieldNames(2), 0, 182), txtField(2), lbl(FieldNames(3), 330, 182), txtField(3),
                                       lbl(FieldNames(4), 0, 216), txtField(4), butKeyWords})
    End Sub

    Private Sub ShowAdvanced(ByVal show As Boolean)
        pnlAdvanced.Visible = show
        ClientSize = New Size(ClientSize.Width, BaseHeight + If(show, AdvancedHeight, 0))
        PlaceButtons()
    End Sub

    Private Sub PlaceButtons()
        Dim y As Integer = ClientSize.Height - 44
        butClear.SetBounds(16, y, 120, 30)
        butCancel.SetBounds(546, y, 118, 30)
    End Sub

    '==================================================================================================
    ' Suggestions and recent searches
    '==================================================================================================
    Private Sub EnsureRecords()
        If m_lpRecords IsNot Nothing Then Return
        UseWaitCursor = True
        Try
            m_lpRecords = If(g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement, g_lpDatabase.SearchRecords(), New List(Of PhotoSearch.Record))
            Dim words As List(Of String) = PhotoSearch.Vocabulary(m_lpRecords)
            Dim albums As New HashSet(Of String)(m_lpRecords.Select(Function(r) IO.Path.GetFileName(IO.Path.GetDirectoryName(r.File))), StringComparer.CurrentCultureIgnoreCase)
            m_lpVocabulary = words.Union(albums, StringComparer.CurrentCultureIgnoreCase).Where(Function(w) w <> "").ToList()
        Finally
            UseWaitCursor = False
        End Try
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Refresh()
        EnsureRecords()
        txtQuery.Focus()
        txtQuery.SelectAll()
    End Sub

    ''' <summary>The word being typed (after the last space).</summary>
    Private Function LastWord() As String
        Dim t As String = txtQuery.Text
        If t.EndsWith(" ") OrElse t.EndsWith("　") Then Return ""
        Dim i As Integer = Math.Max(t.LastIndexOf(" "c), t.LastIndexOf("　"c))
        Return t.Substring(i + 1).TrimStart("-"c)
    End Function

    Private Sub UpdateSuggestions()
        If m_bolPicking OrElse m_lpVocabulary Is Nothing Then Return
        Dim items As List(Of String) = PhotoSearch.Suggest(m_lpVocabulary, LastWord(), 8)
        lstSuggest.BeginUpdate()
        lstSuggest.Items.Clear()
        For Each s In items
            lstSuggest.Items.Add(s)
        Next
        lstSuggest.EndUpdate()
        lstSuggest.Height = Math.Max(1, items.Count) * lstSuggest.ItemHeight + 4
        lstSuggest.Visible = items.Count > 0 AndAlso txtQuery.Focused
    End Sub

    Private Sub Query_KeyDown(sender As Object, e As KeyEventArgs)
        If lstSuggest.Visible Then
            Select Case e.KeyCode
                Case Keys.Down
                    lstSuggest.SelectedIndex = Math.Min(lstSuggest.Items.Count - 1, lstSuggest.SelectedIndex + 1)
                    e.Handled = True : e.SuppressKeyPress = True
                    Return
                Case Keys.Up
                    lstSuggest.SelectedIndex = Math.Max(-1, lstSuggest.SelectedIndex - 1)
                    e.Handled = True : e.SuppressKeyPress = True
                    Return
                Case Keys.Tab, Keys.Enter
                    If lstSuggest.SelectedIndex >= 0 Then
                        AcceptSuggestion()
                        e.Handled = True : e.SuppressKeyPress = True
                        Return
                    End If
                Case Keys.Escape
                    lstSuggest.Visible = False
                    e.Handled = True : e.SuppressKeyPress = True
                    Return
            End Select
        End If
        If e.KeyCode = Keys.Enter Then
            e.Handled = True : e.SuppressKeyPress = True
            DoSearch()
        End If
    End Sub

    ''' <summary>The suggestion picked takes the place of the word being typed.</summary>
    Private Sub AcceptSuggestion()
        Dim s As String = TryCast(lstSuggest.SelectedItem, String)
        If s Is Nothing Then Return
        Dim t As String = txtQuery.Text
        Dim i As Integer = Math.Max(t.LastIndexOf(" "c), t.LastIndexOf("　"c))
        Dim head As String = t.Substring(0, i + 1)
        Dim minus As String = If(t.Substring(i + 1).StartsWith("-"), "-", "")
        If s.Contains(" ") Then s = """" & s & """"
        m_bolPicking = True
        txtQuery.Text = head & minus & s & " "
        m_bolPicking = False
        txtQuery.SelectionStart = txtQuery.Text.Length
        lstSuggest.Visible = False
        txtQuery.Focus()
    End Sub

    Private Shared ReadOnly Property HistoryFile As String
        Get
            Return IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "SearchHistory.txt")
        End Get
    End Property

    Private Shared Function LoadHistory() As List(Of String)
        Try
            If IO.File.Exists(HistoryFile) Then Return IO.File.ReadAllLines(HistoryFile, System.Text.Encoding.UTF8).Where(Function(l) l.Trim() <> "").Take(10).ToList()
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return New List(Of String)
    End Function

    Private Shared Sub SaveHistory(ByVal text As String)
        If text.Trim() = "" Then Return
        Dim list As List(Of String) = LoadHistory()
        list.RemoveAll(Function(l) String.Equals(l, text.Trim(), StringComparison.CurrentCultureIgnoreCase))
        list.Insert(0, text.Trim())
        Try
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(HistoryFile))
            IO.File.WriteAllLines(HistoryFile, list.Take(10), System.Text.Encoding.UTF8)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

    ''' <summary>A search typed elsewhere (the main window's quick box): the box's text next time, and
    ''' in the recent searches.</summary>
    Friend Shared Sub RememberText(ByVal text As String)
        s_text = text.Trim()
        SaveHistory(text)
    End Sub

    Private Sub FillRecent()
        pnlRecent.Controls.Clear()
        Dim list As List(Of String) = LoadHistory()
        If list.Count = 0 Then Return
        pnlRecent.Controls.Add(New Label With {.Text = "最近：", .AutoSize = True, .ForeColor = Color.DimGray, .Margin = New Padding(3, 5, 0, 0)})
        For Each h In list.Take(6)
            Dim link As New LinkLabel With {.Text = h, .AutoSize = True, .Margin = New Padding(6, 5, 6, 0), .LinkColor = Color.FromArgb(42, 116, 208)}
            AddHandler link.LinkClicked, Sub()
                                             txtQuery.Text = h
                                             DoSearch()
                                         End Sub
            pnlRecent.Controls.Add(link)
        Next
    End Sub

    '==================================================================================================
    ' 進階
    '==================================================================================================
    Private Sub PickKeyWords()
        Dim words As String
        Using f As New frmKeyWords
            words = f.ShowKeyWords(g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord))
        End Using
        If words.Trim() <> "" Then txtField(4).Text = words.Trim()
    End Sub

    Private Sub ClearAll()
        txtQuery.Text = ""
        cboDate.SelectedIndex = 0 : cboSeason.SelectedIndex = 0
        dtFrom.Value = Date.Today.AddYears(-1) : dtTo.Value = Date.Today
        For Each c In {cboRank, cboMedia, cboGps, cboPeople, cboScope}
            c.SelectedIndex = 0
        Next
        chkDock.Checked = False
        For Each t In txtField
            t.Text = ""
        Next
        txtQuery.Focus()
    End Sub

    Private Sub Restore()
        ClearAll()
        txtQuery.Text = s_text
        If s_state IsNot Nothing Then
            cboDate.SelectedIndex = s_state.DateIndex
            dtFrom.Value = s_state.From : dtTo.Value = s_state.To
            cboSeason.SelectedIndex = s_state.Season
            cboRank.SelectedIndex = s_state.Rank : cboMedia.SelectedIndex = s_state.Media
            cboGps.SelectedIndex = s_state.Gps : cboPeople.SelectedIndex = s_state.People
            chkDock.Checked = s_state.Dock
            Dim k As Integer = cboScope.Items.IndexOf(s_state.Scope)
            cboScope.SelectedIndex = If(k >= 0, k, 0)
            For i = 0 To 4
                txtField(i).Text = s_state.Fields(i)
            Next
        End If
        chkAdvanced.Checked = s_advanced
        ShowAdvanced(s_advanced)
    End Sub

    Private Sub Remember()
        s_text = txtQuery.Text
        s_advanced = chkAdvanced.Checked
        s_state = New AdvancedState With {
            .DateIndex = cboDate.SelectedIndex, .From = dtFrom.Value, .To = dtTo.Value, .Season = cboSeason.SelectedIndex,
            .Rank = cboRank.SelectedIndex, .Media = cboMedia.SelectedIndex, .Gps = cboGps.SelectedIndex, .People = cboPeople.SelectedIndex,
            .Dock = chkDock.Checked, .Scope = cboScope.Text}
        For i = 0 To 4
            s_state.Fields(i) = txtField(i).Text
        Next
    End Sub

    ''' <summary>The box's words and (when shown) 進階's choices as a Query.</summary>
    Private Function BuildQuery() As PhotoSearch.Query
        Dim today As Date = Date.Today
        Dim q As PhotoSearch.Query = PhotoSearch.Parse(txtQuery.Text, today)
        If Not chkAdvanced.Checked Then Return q
        Select Case cboDate.SelectedIndex
            Case 1 : q.DatePrefixes.Add(today.Year.ToString("0000"))
            Case 2 : q.DatePrefixes.Add((today.Year - 1).ToString("0000"))
            Case 3 : q.DateFrom = today.AddYears(-1).ToString("yyyyMMdd")
            Case 4
                Dim a As Date = dtFrom.Value.Date, b As Date = dtTo.Value.Date
                If a > b Then
                    Dim t As Date = a
                    a = b : b = t
                End If
                q.DateFrom = a.ToString("yyyyMMdd") : q.DateTo = b.ToString("yyyyMMdd")
        End Select
        If cboSeason.SelectedIndex > 0 Then q.Months.UnionWith(SeasonMonths(cboSeason.SelectedIndex))
        q.MinRanking = Math.Max(q.MinRanking, cboRank.SelectedIndex)
        q.Media = cboMedia.SelectedIndex
        q.Gps = CType(cboGps.SelectedIndex, PhotoSearch.Presence)
        q.Persons = CType(cboPeople.SelectedIndex, PhotoSearch.Presence)
        If chkDock.Checked Then
            q.Within = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i = 0 To g_lpDock.Count - 1
                q.Within.Add(g_lpDock.FileName(i))
            Next
        End If
        If cboScope.SelectedIndex > 0 Then
            Dim s = m_lpScopes(cboScope.SelectedIndex)
            If s.Under IsNot Nothing Then q.Under = s.Under
            If s.Book IsNot Nothing Then
                s.Book.Load()
                Dim files As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                For i = 0 To s.Book.PhotoCount - 1
                    files.Add(s.Book.Photo(i).FileDesc)
                Next
                If q.Within IsNot Nothing Then files.IntersectWith(q.Within)
                q.Within = files
            End If
        End If
        q.FieldTitle = txtField(0).Text : q.FieldPeople = txtField(1).Text : q.FieldPlace = txtField(2).Text
        q.FieldRemark = txtField(3).Text : q.FieldKeyWord = txtField(4).Text
        Return q
    End Function

    Private Sub DoSearch()
        lstSuggest.Visible = False
        Dim q As PhotoSearch.Query = BuildQuery()
        If q.IsEmpty Then
            frmMsgBox.ShowExclamationMessage("請輸入要找的字，或設定進階條件。", "搜尋")
            txtQuery.Focus()
            Return
        End If
        EnsureRecords()
        m_lpQuery = q
        Remember()
        SaveHistory(txtQuery.Text)
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape AndAlso Not lstSuggest.Visible Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
