Imports System.Threading
Imports System.Threading.Tasks

' 輸入照片 › ② 整理 (frmImportStudio): the album card on top, the photos on the left, the photo with its
' faces in the middle, its details on the right.
'   - album card: name / folder / date / place / where it goes / icon / remark. They start from the
'     photos (date range, the place most photos were taken at, the album imported into last time); the
'     folder follows the date until it is typed over (「跟著日期」).
'   - details: one photo or several (Ctrl / Shift / Ctrl+A in the grid). With several, a field whose
'     values differ says so in its label and is left empty; what is typed goes into every selected
'     photo when the field is left or the selection changes. 「→ 全部」 copies the field to every photo.
'     The people field is the faces' names (chips) plus the names typed (people without a face).
'   - faces: every picture is analysed in the background (the one shown first) and matched against the
'     people already known (ImportFaceMatcher); the strip under the photo lists the people of the batch
'     and the groups of new faces, each named with one click.
Partial Class frmImportStudio

    ' album card
    Private WithEvents picIcon As PictureBox
    Private WithEvents txtTitle, txtFolder, txtDate, txtSpot, txtRemark As Aqua.TextBox
    Private WithEvents lblFolderAuto As Label
    Private WithEvents cboAlbum As ComboBox
    Private lblSplitInfo As Label
    Private m_strIcon As String = "icoPeople"
    Private m_bolFolderFollows As Boolean = True
    Private m_bolAlbumEdited As Boolean          ' the user typed into the card (keep it when going back and forth)
    Private m_strAutoTitle As String = ""

    ' photos, stage, details
    Private WithEvents grid2 As ImportThumbGrid
    Private lblCount2 As Label
    Private WithEvents chipUnnamed As ImportChip
    Private WithEvents stage As ImportFaceStage
    Private flpPeople As FlowLayoutPanel
    Private lblFaceProgress As Label
    Private lblScope As Label
    Private lblPTitle, lblPPeople, lblPSpot, lblPKey, lblPRemark As Label
    Private WithEvents txtPTitle, txtPPeople, txtPSpot, txtPKey, txtPRemark As Aqua.TextBox
    Private flpFaces As FlowLayoutPanel
    Private WithEvents butKeyWords As Aqua.FlashButton
    Private WithEvents butAllTitle, butAllSpot, butAllKey, butAllRemark As Aqua.FlashButton
    Private lblNote As Label
    Private WithEvents mnuFace As ContextMenuStrip
    Private WithEvents mnuNotFace, mnuUnname, mnuReject As ToolStripMenuItem
    Private WithEvents m_peopleTimer As New System.Windows.Forms.Timer With {.Interval = 250}

    Private m_included As New List(Of ImportItem)
    Private m_sel As New List(Of ImportItem)
    Private m_bolLoading As Boolean
    Private ReadOnly m_dirty As New HashSet(Of Aqua.TextBox)
    Private m_matcher As ImportFaceMatcher
    Private m_names As New List(Of String)
    Private m_faceCts As CancellationTokenSource
    Private m_faceTask As Task
    Private m_priority As ImportItem
    Private m_strPersonFilter As String

    Private ReadOnly Property Engine As Quartz.FaceEngine
        Get
            Return If(g_lpFaces Is Nothing, Nothing, g_lpFaces.Engine)
        End Get
    End Property

    '==================================================================================================
    ' Layout
    '==================================================================================================
    Private Function NewText(ByVal parent As Control, ByVal x As Integer, ByVal y As Integer, ByVal w As Integer,
                             Optional ByVal h As Integer = 28, Optional ByVal multi As Boolean = False) As Aqua.TextBox
        Dim t As New Aqua.TextBox With {.Location = New Point(x, y), .Size = New Size(w, h), .Font = New Font(Font.FontFamily, 12.0F),
                                        .BorderColor = Color.FromArgb(189, 189, 189), .BorderFocusColor = Color.FromArgb(159, 182, 244),
                                        .BackColor = Color.White}
        If multi Then
            t.Multiline = True
            t.ScrollBars = ScrollBars.Vertical
        End If
        parent.Controls.Add(t)
        Return t
    End Function

    Private Sub BuildStep2(ByVal p As Panel)
        ' ---- album card
        Dim card As Panel = NewBox(p, 0, 0, p.Width, 92)
        picIcon = New PictureBox With {.Location = New Point(14, 14), .Size = New Size(48, 48), .SizeMode = PictureBoxSizeMode.Zoom, .Cursor = Cursors.Hand, .BackColor = Color.Transparent}
        card.Controls.Add(picIcon)
        NewLabel(card, "圖示", 14, 64, 48, 20, 9.0F, color:=Muted).TextAlign = ContentAlignment.TopCenter
        NewLabel(card, "相簿名稱", 76, 6, 200, 20, 10.0F, color:=Muted)
        txtTitle = NewText(card, 76, 26, 300)
        NewLabel(card, "資料夾", 390, 6, 60, 20, 10.0F, color:=Muted)
        lblFolderAuto = NewLabel(card, "跟著日期", 450, 6, 110, 20, 9.5F, FontStyle.Underline, Accent)
        lblFolderAuto.Cursor = Cursors.Hand
        txtFolder = NewText(card, 390, 26, 170)
        NewLabel(card, "日期", 574, 6, 160, 20, 10.0F, color:=Muted)
        txtDate = NewText(card, 574, 26, 190)
        NewLabel(card, "地點", 778, 6, 200, 20, 10.0F, color:=Muted)
        txtSpot = NewText(card, 778, 26, 190)
        NewLabel(card, "存放在", 982, 6, 200, 20, 10.0F, color:=Muted)
        cboAlbum = New ComboBox With {.Location = New Point(982, 27), .Size = New Size(204, 28), .DropDownStyle = ComboBoxStyle.DropDownList,
                                      .Font = New Font(Font.FontFamily, 11.0F), .DropDownWidth = 360}
        card.Controls.Add(cboAlbum)
        NewLabel(card, "註解", 76, 62, 40, 22, 10.0F, color:=Muted)
        txtRemark = NewText(card, 116, 58, 648, 28)
        lblSplitInfo = NewLabel(card, "", 778, 62, 408, 24, 10.0F, color:=Muted)

        ' ---- photos
        Dim left As Panel = NewBox(p, 6, 100, 268, 590)
        lblCount2 = NewLabel(left, "", 8, 6, 120, 22, 10.5F, color:=Muted)
        chipUnnamed = New ImportChip With {.Text = "只看沒人名的", .Font = New Font(Font.FontFamily, 9.5F), .ForeColor = Ink, .Location = New Point(136, 5)}
        chipUnnamed.FitText()
        chipUnnamed.Left = left.Width - chipUnnamed.Width - 8
        left.Controls.Add(chipUnnamed)
        grid2 = New ImportThumbGrid With {.Location = New Point(1, 32), .Size = New Size(266, 557), .Font = New Font(Font.FontFamily, 9.5F),
                                          .CellSize = New Size(72, 54), .ShowChecks = False, .ShowFaceBadges = True}
        left.Controls.Add(grid2)

        ' ---- stage + people strip
        stage = New ImportFaceStage With {.Location = New Point(282, 100), .Size = New Size(596, 506), .Font = New Font(Font.FontFamily, 11.0F)}
        stage.PersonOf = Function(n) If(m_matcher Is Nothing, 0, m_matcher.PersonOf(n))
        p.Controls.Add(stage)
        Dim strip As Panel = NewBox(p, 282, 610, 596, 80)
        NewLabel(strip, "這批的人物", 8, 6, 100, 20, 9.5F, FontStyle.Bold, Muted)
        lblFaceProgress = NewLabel(strip, "", 300, 6, 288, 20, 9.5F, color:=Muted)
        lblFaceProgress.TextAlign = ContentAlignment.TopRight
        flpPeople = New FlowLayoutPanel With {.Location = New Point(6, 28), .Size = New Size(584, 50), .AutoScroll = True, .WrapContents = True, .BackColor = Color.White}
        strip.Controls.Add(flpPeople)

        mnuReject = New ToolStripMenuItem("不是這個人")
        mnuUnname = New ToolStripMenuItem("移除名字")
        mnuNotFace = New ToolStripMenuItem("這不是臉")
        mnuFace = New ContextMenuStrip With {.Font = New Font(Font.FontFamily, 11.0F)}
        mnuFace.Items.AddRange({mnuReject, mnuUnname, mnuNotFace})

        ' ---- details
        Dim right As Panel = NewBox(p, 886, 100, 308, 590)
        lblScope = NewLabel(right, "這張照片", 12, 8, 284, 24, 11.5F, FontStyle.Bold)
        Dim y As Integer = 40
        lblPTitle = NewLabel(right, "標題", 12, y, 200, 20, 10.0F, color:=Muted)
        butAllTitle = NewButton(right, "→ 全部", 226, y - 3, 70, h:=22)
        txtPTitle = NewText(right, 12, y + 20, 284)
        y += 56
        lblPPeople = NewLabel(right, "人物", 12, y, 280, 20, 10.0F, color:=Muted)
        flpFaces = New FlowLayoutPanel With {.Location = New Point(12, y + 20), .Size = New Size(284, 58), .AutoScroll = True, .BackColor = Color.White}
        right.Controls.Add(flpFaces)
        txtPPeople = NewText(right, 12, y + 80, 284)
        txtPPeople.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        txtPPeople.AutoCompleteSource = AutoCompleteSource.CustomSource
        vb6ToolTip.SetToolTip(txtPPeople, "照片裡沒露臉、或沒被找到臉的人，用「,」分開")
        y += 116
        lblPSpot = NewLabel(right, "地點", 12, y, 200, 20, 10.0F, color:=Muted)
        butAllSpot = NewButton(right, "→ 全部", 226, y - 3, 70, h:=22)
        txtPSpot = NewText(right, 12, y + 20, 284)
        y += 56
        lblPKey = NewLabel(right, "關鍵字", 12, y, 140, 20, 10.0F, color:=Muted)
        butKeyWords = NewButton(right, "…", 152, y - 3, 34, h:=22)
        butAllKey = NewButton(right, "→ 全部", 226, y - 3, 70, h:=22)
        txtPKey = NewText(right, 12, y + 20, 284)
        y += 56
        lblPRemark = NewLabel(right, "註解", 12, y, 200, 20, 10.0F, color:=Muted)
        butAllRemark = NewButton(right, "→ 全部", 226, y - 3, 70, h:=22)
        txtPRemark = NewText(right, 12, y + 20, 284, 84, multi:=True)
        y += 112
        lblNote = NewLabel(right, "", 12, y, 284, 590 - y - 6, 9.5F, color:=Muted)
        lblNote.Text = "多選：按住 Ctrl 或 Shift 點縮圖，Ctrl+A 全選；填好的欄位會套用到所有選取的照片。" & vbCrLf & vbCrLf &
                       "人物：綠色＝會寫進人物欄；黃色＝程式的建議，按 ✓ 才算；點臉框可以打名字。"
        For Each t In {txtPTitle, txtPPeople, txtPSpot, txtPKey, txtPRemark}
            AddHandler t.TextChanged, AddressOf Detail_TextChanged
            AddHandler t.Leave, AddressOf Detail_Leave
        Next
    End Sub

    ''' <summary>Form load: known people, the albums to import into.</summary>
    Private Sub LoadStep2Data()
        If g_lpFaces IsNot Nothing AndAlso g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.FaceTablesReady Then
            Try
                m_matcher = New ImportFaceMatcher(g_lpDatabase, g_lpConfig.FaceStrictness)
            Catch ex As Data.OleDb.OleDbException
                m_matcher = Nothing
            End Try
        End If
        stage.FacesEnabled = m_matcher IsNot Nothing
        If g_lpDatabase IsNot Nothing Then
            Try
                m_names = g_lpDatabase.LoadPeopleNames()
            Catch ex As Data.OleDb.OleDbException
                m_names = New List(Of String)
            End Try
        End If
        stage.Names = m_names
        Dim ac As New AutoCompleteStringCollection
        ac.AddRange(m_names.ToArray())
        txtPPeople.AutoCompleteCustomSource = ac

        ' where it goes: every album of every library root
        cboAlbum.Items.Clear()
        For i = 0 To g_lpConfig.AlbumCount - 1
            Dim root As String = g_lpConfig.AlbumPath(i)
            Dim sections() As String = Nothing
            ExactFolders(root, sections)
            For Each s In sections.Where(Function(f) Not IsSystemFolder(f))
                cboAlbum.Items.Add(New AlbumTarget With {.Path = s, .Text = IO.Path.GetFileName(root.TrimEnd("\"c)) & " › " & IO.Path.GetFileName(s)})
            Next
        Next
        Dim recent As String = GetSetting(AppName, "Import Recent", "Album", "")
        Dim idx As Integer = -1
        For i = 0 To cboAlbum.Items.Count - 1
            If String.Equals(CType(cboAlbum.Items(i), AlbumTarget).Path, recent, StringComparison.OrdinalIgnoreCase) Then idx = i
        Next
        If idx < 0 AndAlso cboAlbum.Items.Count > 0 Then idx = cboAlbum.Items.Count - 1
        cboAlbum.SelectedIndex = idx
        ShowAlbumIcon()
    End Sub

    Private Class AlbumTarget
        Public Path As String = ""
        Public Text As String = ""
        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    '==================================================================================================
    ' Entering the step
    '==================================================================================================
    Private Sub EnterStep2()
        m_included = m_items.Where(Function(i) i.Include).ToList()
        Dim probe As New ImportJob
        probe.Items.AddRange(m_items)
        probe.AssignFileNames()
        m_strPersonFilter = Nothing
        chipUnnamed.Tag = Nothing
        grid2.Filter = Nothing
        grid2.SetItems(m_included)
        FillAlbumDefaults()
        ShowStep(1)
        Dim first As ImportItem = If(m_included.Contains(grid1.FocusItem), grid1.FocusItem, m_included.FirstOrDefault())
        grid2.SelectOnly(first)
        StartFaceAnalysis()
        RefreshPeopleStrip()
        grid2.Focus()
    End Sub

    ''' <summary>The card's values from the photos, unless the user already typed into it.</summary>
    Private Sub FillAlbumDefaults()
        Dim days = m_included.Where(Function(i) i.ShotDate.HasValue).Select(Function(i) i.ShotDate.Value)
        Dim range As String = ImportSource.DateRangeText(days)
        Dim first As String = ImportSource.DayText(If(days.Any(), days.Min(), Date.Today))
        m_bolLoading = True
        If Not m_bolAlbumEdited Then
            txtTitle.Text = range
            txtDate.Text = range
            txtSpot.Text = ImportSource.CommonPlace(m_included)
            txtRemark.Text = ""
            m_bolFolderFollows = True
        End If
        m_strAutoTitle = range
        If m_bolFolderFollows Then txtFolder.Text = first
        m_bolLoading = False
        UpdateFolderState()
    End Sub

    Private Sub UpdateFolderState()
        Dim split As Boolean = chkSplit.Checked
        txtFolder.Enabled = Not split
        txtDate.Enabled = Not split
        lblFolderAuto.Text = If(split, "每天一本", If(m_bolFolderFollows, "跟著日期", "改回日期"))
        If split Then
            Dim n As Integer = m_included.Where(Function(i) i.ShotDate.HasValue).Select(Function(i) i.ShotDate.Value.Date).Distinct().Count()
            lblSplitInfo.Text = "每天一本：會建立 " & Math.Max(1, n) & " 本相簿，資料夾與日期用各自的日子"
        Else
            lblSplitInfo.Text = ""
        End If
    End Sub

    Private Sub AlbumCard_TextChanged(sender As Object, e As EventArgs) Handles txtTitle.TextChanged, txtDate.TextChanged, txtSpot.TextChanged, txtRemark.TextChanged
        If m_bolLoading Then Return
        m_bolAlbumEdited = True
    End Sub

    Private Sub txtFolder_TextChanged(sender As Object, e As EventArgs) Handles txtFolder.TextChanged
        If m_bolLoading Then Return
        m_bolAlbumEdited = True
        m_bolFolderFollows = False
        UpdateFolderState()
    End Sub

    Private Sub lblFolderAuto_Click(sender As Object, e As EventArgs) Handles lblFolderAuto.Click
        If chkSplit.Checked Then Return
        m_bolFolderFollows = True
        FillAlbumDefaults()
    End Sub

    Private Sub picIcon_Click(sender As Object, e As EventArgs) Handles picIcon.Click
        Dim pt As Point = picIcon.PointToScreen(New Point(0, picIcon.Height))
        Dim key As String
        Using f As New frmSubject
            key = f.ShowSubject(pt.X, pt.Y)
        End Using
        If key.Trim() = "" Then Return
        m_strIcon = key
        ShowAlbumIcon()
    End Sub

    Private Sub ShowAlbumIcon()
        Dim il As ImageList = g_lpConfig.ImageListSubject
        picIcon.Image = If(il IsNot Nothing AndAlso il.Images.ContainsKey(m_strIcon), il.Images(m_strIcon), Nothing)
        vb6ToolTip.SetToolTip(picIcon, "相簿的圖示：" & frmResAlbum.SubjectName(m_strIcon) & "（點一下更換）")
    End Sub

    '==================================================================================================
    ' Selection and details
    '==================================================================================================
    Private Sub grid2_SelectionChanged(sender As Object, e As EventArgs) Handles grid2.SelectionChanged
        CommitInspector()
        m_sel = grid2.SelectedItems
        Dim focus As ImportItem = grid2.FocusItem
        m_priority = focus
        stage.ShowItem(focus)
        UpdateStageText()
        LoadInspector()
    End Sub

    Private Sub UpdateStageText()
        Dim it As ImportItem = stage.Item
        If it Is Nothing Then
            stage.Caption = ""
            stage.Hint = ""
        Else
            Dim d As Date? = it.ShotDate
            Dim shown = grid2.ShownItems()
            stage.Caption = it.FileName & If(d.HasValue, "  ·  " & d.Value.ToString("yyyy/MM/dd HH:mm"), "") & "    " & (shown.IndexOf(it) + 1) & " / " & shown.Count
            If m_matcher Is Nothing OrElse Not it.IsPicture Then
                stage.Hint = If(it.IsVideo, "影片", "")
            ElseIf it.Faces Is Nothing Then
                stage.Hint = "分析面孔中…"
            ElseIf Not it.VisibleFaces.Any() Then
                stage.Hint = "沒找到臉 · 拖曳可框出一張臉"
            Else
                stage.Hint = "點臉框打名字 · 拖曳可補框"
            End If
        End If
        stage.Invalidate()
        Dim unnamed As Integer = m_included.Where(Function(i) i.Character() = "").Count()
        lblCount2.Text = m_included.Count & " 張" & If(unnamed > 0, " · " & unnamed & " 張沒人名", "")
    End Sub

    ''' <summary>The common value of a field over the selection (Nothing when they differ).</summary>
    Private Function Common(ByVal getter As Func(Of ImportItem, String)) As String
        If m_sel.Count = 0 Then Return ""
        Dim v As String = getter(m_sel(0))
        For Each it In m_sel
            If getter(it) <> v Then Return Nothing
        Next
        Return v
    End Function

    Private Function SpotShown(ByVal it As ImportItem) As String
        Return it.SpotFor(txtSpot.Text.Trim())
    End Function

    Private Sub LoadInspector()
        m_bolLoading = True
        m_dirty.Clear()
        Dim n As Integer = m_sel.Count
        lblScope.Text = If(n = 0, "沒有選照片", If(n = 1, "這張照片", "選取的 " & n & " 張照片"))
        Dim fields = {(txtPTitle, lblPTitle, "標題", CType(Function(i As ImportItem) i.Title, Func(Of ImportItem, String))),
                      (txtPPeople, lblPPeople, If(n = 1, "人物（臉＋其他人）", "人物：加上這些人"), CType(Function(i As ImportItem) i.People, Func(Of ImportItem, String))),
                      (txtPSpot, lblPSpot, "地點", CType(AddressOf SpotShown, Func(Of ImportItem, String))),
                      (txtPKey, lblPKey, "關鍵字", CType(Function(i As ImportItem) i.KeyWord, Func(Of ImportItem, String))),
                      (txtPRemark, lblPRemark, "註解", CType(Function(i As ImportItem) i.Remark, Func(Of ImportItem, String)))}
        For Each f In fields
            Dim v As String = Common(f.Item4)
            f.Item1.Text = If(v, "")
            f.Item1.Enabled = n > 0
            f.Item2.Text = f.Item3 & If(v Is Nothing, "（多個值）", "")
        Next
        If n = 1 Then
            Dim it As ImportItem = m_sel(0)
            If it.Spot Is Nothing Then lblPSpot.Text = "地點" & If(it.GpsPlace <> "", "（GPS）", If(txtSpot.Text.Trim() <> "", "（同相簿）", ""))
        End If
        m_bolLoading = False
        RefreshFaceChips()
    End Sub

    ''' <summary>The faces of the photo shown, as chips over the people box.</summary>
    Private Sub RefreshFaceChips()
        flpFaces.SuspendLayout()
        For Each c As Control In flpFaces.Controls.Cast(Of Control)().ToList()
            c.Dispose()
        Next
        flpFaces.Controls.Clear()
        Dim it As ImportItem = If(m_sel.Count = 1, m_sel(0), Nothing)
        Dim small As New Font(Font.FontFamily, 10.0F)
        If it Is Nothing Then
            flpFaces.Controls.Add(New Label With {.AutoSize = True, .Font = small, .ForeColor = Muted,
                                                  .Text = If(m_sel.Count > 1, "臉的名字要一張一張看；下面打的名字會加到每一張", "")})
        ElseIf it.Faces Is Nothing AndAlso it.IsPicture AndAlso m_matcher IsNot Nothing Then
            flpFaces.Controls.Add(New Label With {.AutoSize = True, .Font = small, .ForeColor = Muted, .Text = "分析面孔中…"})
        Else
            For Each f In it.VisibleFaces.OrderBy(Function(x) x.Box.X)
                Dim chip As New ImportChip With {.Font = small, .ForeColor = Ink, .Payload = f}
                Select Case f.State
                    Case ImportFace.enumImportFaceState.ifConfirmed, ImportFace.enumImportFaceState.ifAuto
                        chip.Text = f.Name : chip.ChipBack = GreenSoft : chip.ChipBorder = Color.FromArgb(159, 208, 178)
                    Case ImportFace.enumImportFaceState.ifSuggested
                        chip.Text = f.Name & "？" : chip.ChipBack = AmberSoft : chip.ChipBorder = Color.FromArgb(230, 198, 136) : chip.Dashed = True
                    Case Else
                        chip.Text = If(f.Group > 0, "新面孔 " & f.Group, "這是誰？") : chip.ChipBack = Color.White : chip.ChipBorder = Line : chip.Dashed = True
                End Select
                chip.FitText()
                AddHandler chip.Click, Sub(s, e)
                                           Dim face As ImportFace = CType(CType(s, ImportChip).Payload, ImportFace)
                                           stage.SelectedFace = face
                                           stage.BeginName(face)
                                       End Sub
                flpFaces.Controls.Add(chip)
            Next
            If flpFaces.Controls.Count = 0 Then
                flpFaces.Controls.Add(New Label With {.AutoSize = True, .Font = small, .ForeColor = Muted,
                                                      .Text = If(it.IsPicture AndAlso m_matcher IsNot Nothing, "沒有找到臉", "")})
            End If
        End If
        flpFaces.ResumeLayout()
    End Sub

    Private Sub Detail_TextChanged(sender As Object, e As EventArgs)
        If m_bolLoading Then Return
        m_dirty.Add(CType(sender, Aqua.TextBox))
    End Sub

    Private Sub Detail_Leave(sender As Object, e As EventArgs)
        CommitInspector()
    End Sub

    ''' <summary>What was typed into the details goes into every selected photo.</summary>
    Private Sub CommitInspector()
        If m_dirty.Count = 0 OrElse m_sel.Count = 0 Then
            m_dirty.Clear()
            Return
        End If
        For Each t In m_dirty.ToList()
            Dim v As String = t.Text.Trim()
            For Each it In m_sel
                If t Is txtPTitle Then
                    it.Title = v
                ElseIf t Is txtPPeople Then
                    it.People = If(m_sel.Count = 1, v, FaceNames.Split(v).Aggregate(it.People, Function(acc, n) FaceNames.Add(acc, n)))
                ElseIf t Is txtPSpot Then
                    it.Spot = If(v = "", Nothing, v)
                ElseIf t Is txtPKey Then
                    it.KeyWord = v
                ElseIf t Is txtPRemark Then
                    it.Remark = t.Text.Replace(vbCrLf, " ").Trim()
                End If
                it.Edited = True
            Next
        Next
        m_dirty.Clear()
        grid2.Invalidate()
        UpdateStageText()
        UpdateStep2Foot()
    End Sub

    ''' <summary>「→ 全部」: the field of the photo shown goes into every photo of the import.</summary>
    Private Sub CopyToAll(ByVal t As Aqua.TextBox)
        CommitInspector()
        Dim v As String = t.Text.Trim()
        If v = "" AndAlso Not frmQueryMsgBox.ShowMessage("把全部 " & m_included.Count & " 張照片的這一欄都清空？", "套用到全部") Then Return
        For Each it In m_included
            If t Is txtPTitle Then
                it.Title = v
            ElseIf t Is txtPSpot Then
                it.Spot = If(v = "", Nothing, v)
            ElseIf t Is txtPKey Then
                it.KeyWord = v
            ElseIf t Is txtPRemark Then
                it.Remark = t.Text.Replace(vbCrLf, " ").Trim()
            End If
            it.Edited = True
        Next
        grid2.Invalidate()
        LoadInspector()
        UpdateStep2Foot()
        lblFoot.Text = "已套用到全部 " & m_included.Count & " 張"
    End Sub

    Private Sub butAllTitle_Click(sender As Object, e As EventArgs) Handles butAllTitle.Click
        CopyToAll(txtPTitle)
    End Sub

    Private Sub butAllSpot_Click(sender As Object, e As EventArgs) Handles butAllSpot.Click
        CopyToAll(txtPSpot)
    End Sub

    Private Sub butAllKey_Click(sender As Object, e As EventArgs) Handles butAllKey.Click
        CopyToAll(txtPKey)
    End Sub

    Private Sub butAllRemark_Click(sender As Object, e As EventArgs) Handles butAllRemark.Click
        CopyToAll(txtPRemark)
    End Sub

    Private Sub butKeyWords_Click(sender As Object, e As EventArgs) Handles butKeyWords.Click
        Dim file As String = g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord)
        If Not IO.File.Exists(file) Then
            frmMsgBox.ShowExclamationMessage("找不到關鍵字檔（設定 › 一般）", "")
            Return
        End If
        Dim words As String
        Using f As New frmKeyWords
            words = f.ShowKeyWords(file)
        End Using
        If words.Trim() = "" Then Return
        txtPKey.Text = If(txtPKey.Text.Trim() = "", words, txtPKey.Text.Trim() & "," & words)
        m_dirty.Add(txtPKey)
        CommitInspector()
    End Sub

    Private Sub chipUnnamed_Click(sender As Object, e As EventArgs) Handles chipUnnamed.Click
        Dim on_ As Boolean = chipUnnamed.Tag Is Nothing
        chipUnnamed.Tag = If(on_, CObj(True), Nothing)
        chipUnnamed.ChipBack = If(on_, Accent, AccentSoft)
        chipUnnamed.ForeColor = If(on_, Color.White, Ink)
        chipUnnamed.Invalidate()
        m_strPersonFilter = Nothing
        ApplyGridFilter()
    End Sub

    Private Sub ApplyGridFilter()
        If chipUnnamed.Tag IsNot Nothing Then
            grid2.Filter = Function(i) i.Character() = ""
        ElseIf m_strPersonFilter IsNot Nothing Then
            Dim n As String = m_strPersonFilter
            grid2.Filter = Function(i) i.VisibleFaces.Any(Function(f) f.Name = n) OrElse FaceNames.Contains(i.People, n)
        Else
            grid2.Filter = Nothing
        End If
        grid2.RefreshLayout()
        Dim shown = grid2.ShownItems()
        If shown.Count > 0 AndAlso Not shown.Contains(grid2.FocusItem) Then grid2.SelectOnly(shown(0))
        RefreshPeopleStrip()
    End Sub

    Private Sub UpdateStep2Foot()
        If m_intStep <> 1 Then Return
        Dim ask As Integer = m_included.Sum(Function(i) i.VisibleFaces.Where(Function(f) f.State = ImportFace.enumImportFaceState.ifSuggested).Count())
        Dim edited As Integer = m_included.Where(Function(i) i.Edited).Count()
        lblFoot.Text = If(edited > 0, "已改 " & edited & " 張", "") & If(ask > 0, If(edited > 0, " · ", "") & ask & " 張臉等你確認（沒確認的不會寫進人物欄）", "")
        butNext.Text = "開始匯入 " & m_included.Count & " 張 ›"
    End Sub

    '==================================================================================================
    ' Faces
    '==================================================================================================
    ''' <summary>Analyses the pictures of the import that aren't yet (the one shown first).</summary>
    Private Sub StartFaceAnalysis()
        If m_matcher Is Nothing OrElse Engine Is Nothing Then
            lblFaceProgress.Text = If(g_lpFaces Is Nothing, "人物辨識沒有啟用（設定 › 面孔）", "")
            Return
        End If
        If m_faceTask IsNot Nothing AndAlso Not m_faceTask.IsCompleted Then Return
        Dim todo As List(Of ImportItem) = m_included.Where(Function(i) i.IsPicture AndAlso i.Faces Is Nothing).ToList()
        If todo.Count = 0 Then
            FacesAnalysed()
            Return
        End If
        Dim cts As New CancellationTokenSource
        m_faceCts = cts
        Dim eng As Quartz.FaceEngine = Engine
        Dim matcher As ImportFaceMatcher = m_matcher
        m_faceTask = Task.Run(Sub()
                                  Do While todo.Count > 0 AndAlso Not cts.IsCancellationRequested
                                      Dim p As ImportItem = m_priority
                                      Dim it As ImportItem = If(p IsNot Nothing AndAlso todo.Contains(p), p, todo(0))
                                      todo.Remove(it)
                                      If it.Faces IsNot Nothing Then Continue Do
                                      matcher.Analyse(eng, it)
                                      Dim done As ImportItem = it
                                      Post(Sub() FaceAnalysed(done))
                                  Loop
                                  If Not cts.IsCancellationRequested Then Post(AddressOf FacesAnalysed)
                              End Sub)
        UpdateFaceProgress()
    End Sub

    Private Sub StopFaceAnalysis()
        m_faceCts?.Cancel()
        Try
            m_faceTask?.Wait(3000)
        Catch ex As AggregateException
        End Try
        m_faceTask = Nothing
    End Sub

    Private Sub FaceAnalysed(ByVal it As ImportItem)
        If IsDisposed Then Return
        UpdateFaceProgress()
        grid2.Invalidate()
        If it Is stage.Item Then
            UpdateStageText()
            RefreshFaceChips()
        End If
        m_peopleTimer.Stop()
        m_peopleTimer.Start()
    End Sub

    ''' <summary>All analysed: the unknown faces of the batch are grouped.</summary>
    Private Sub FacesAnalysed()
        If IsDisposed Then Return
        ImportFaceMatcher.GroupUnknown(m_included)
        UpdateFaceProgress()
        RefreshPeopleStrip()
        If stage.Item IsNot Nothing Then
            UpdateStageText()
            RefreshFaceChips()
        End If
        UpdateStep2Foot()
    End Sub

    Private Sub UpdateFaceProgress()
        Dim pics = m_included.Where(Function(i) i.IsPicture).ToList()
        Dim done As Integer = pics.Where(Function(i) i.Faces IsNot Nothing).Count()
        If done < pics.Count Then
            lblFaceProgress.Text = "面孔分析 " & done & " / " & pics.Count
        Else
            lblFaceProgress.Text = "找到 " & pics.Sum(Function(i) i.VisibleFaces.Count()) & " 張臉"
        End If
    End Sub

    Private Sub m_peopleTimer_Tick(sender As Object, e As EventArgs) Handles m_peopleTimer.Tick
        m_peopleTimer.Stop()
        RefreshPeopleStrip()
        UpdateStep2Foot()
    End Sub

    ''' <summary>The people of the batch (named / suggested) and the groups of new faces.</summary>
    Private Sub RefreshPeopleStrip()
        If flpPeople Is Nothing Then Return
        flpPeople.SuspendLayout()
        For Each c As Control In flpPeople.Controls.Cast(Of Control)().ToList()
            c.Dispose()
        Next
        flpPeople.Controls.Clear()
        Dim font As New Font(Me.Font.FontFamily, 10.0F)
        Dim byName As New Dictionary(Of String, (Photos As Integer, Ask As Integer))
        For Each it In m_included
            For Each g In it.VisibleFaces.Where(Function(f) f.Name <> "").GroupBy(Function(f) f.Name)
                Dim v As (Photos As Integer, Ask As Integer) = Nothing
                byName.TryGetValue(g.Key, v)
                byName(g.Key) = (v.Photos + 1, v.Ask + g.Where(Function(f) f.State = ImportFace.enumImportFaceState.ifSuggested).Count())
            Next
        Next
        For Each kv In byName.OrderByDescending(Function(x) x.Value.Photos)
            Dim chip As New ImportChip With {.Font = font, .ForeColor = Ink, .Payload = kv.Key,
                                             .Text = kv.Key & " · " & kv.Value.Photos & If(kv.Value.Ask > 0, "（" & kv.Value.Ask & " 待確認）", "")}
            If kv.Key = m_strPersonFilter Then
                chip.ChipBack = Accent : chip.ForeColor = Color.White : chip.ChipBorder = Accent
            ElseIf kv.Value.Ask = kv.Value.Photos Then
                chip.ChipBack = AmberSoft : chip.ChipBorder = Color.FromArgb(230, 198, 136) : chip.Dashed = True
            Else
                chip.ChipBack = GreenSoft : chip.ChipBorder = Color.FromArgb(159, 208, 178)
            End If
            chip.FitText()
            vb6ToolTip.SetToolTip(chip, "只看有 " & kv.Key & " 的照片（再按一次取消）")
            AddHandler chip.Click, AddressOf PersonChip_Click
            flpPeople.Controls.Add(chip)
        Next
        For Each g In m_included.SelectMany(Function(i) i.VisibleFaces).Where(Function(f) f.Group > 0 AndAlso f.State = ImportFace.enumImportFaceState.ifUnknown).
                      GroupBy(Function(f) f.Group).OrderBy(Function(x) x.Key)
            Dim chip As New ImportChip With {.Font = font, .ForeColor = Color.FromArgb(120, 80, 10), .Payload = g.Key,
                                             .Text = "新面孔 " & g.Key & " · " & g.Count() & " 張，按這裡命名",
                                             .ChipBack = AmberSoft, .ChipBorder = Color.FromArgb(224, 178, 90), .Dashed = True}
            chip.FitText()
            AddHandler chip.Click, AddressOf GroupChip_Click
            flpPeople.Controls.Add(chip)
        Next
        If flpPeople.Controls.Count = 0 Then
            flpPeople.Controls.Add(New Label With {.AutoSize = True, .Font = font, .ForeColor = Muted,
                                                   .Text = If(m_matcher Is Nothing, "人物辨識沒有啟用", "還沒有認出任何人")})
        End If
        flpPeople.ResumeLayout()
    End Sub

    Private Sub PersonChip_Click(sender As Object, e As EventArgs)
        Dim n As String = CStr(CType(sender, ImportChip).Payload)
        m_strPersonFilter = If(m_strPersonFilter = n, Nothing, n)
        If m_strPersonFilter IsNot Nothing AndAlso chipUnnamed.Tag IsNot Nothing Then chipUnnamed_Click(chipUnnamed, EventArgs.Empty) : m_strPersonFilter = n
        ApplyGridFilter()
    End Sub

    ''' <summary>Names every face of a group of new faces at once.</summary>
    Private Sub GroupChip_Click(sender As Object, e As EventArgs)
        Dim group As Integer = CInt(CType(sender, ImportChip).Payload)
        Dim faces = m_included.SelectMany(Function(i) i.VisibleFaces).Where(Function(f) f.Group = group AndAlso f.State = ImportFace.enumImportFaceState.ifUnknown).ToList()
        If faces.Count = 0 Then Return
        Dim name As String
        Using f As New frmInputString
            name = f.GetString("").Trim()
        End Using
        If name = "" OrElse Not CheckNameRule(name) Then Return
        Dim pid As Integer = If(m_matcher Is Nothing, 0, m_matcher.PersonOf(name))
        For Each face In faces
            face.SetName(name, pid)
        Next
        If Not m_names.Contains(name) Then m_names.Add(name)
        FacesEdited()
        lblFoot.Text = faces.Count & " 張臉命名為「" & name & "」"
    End Sub

    Private Sub FacesEdited()
        stage.Invalidate()
        grid2.Invalidate()
        RefreshFaceChips()
        RefreshPeopleStrip()
        UpdateStageText()
        UpdateStep2Foot()
    End Sub

    Private Sub stage_FacesChanged(sender As Object, e As EventArgs) Handles stage.FacesChanged
        FacesEdited()
    End Sub

    Private Sub stage_Navigate(intDelta As Integer) Handles stage.Navigate
        grid2.MoveFocus(intDelta)
    End Sub

    Private Sub stage_BoxDrawn(box As RectangleF) Handles stage.BoxDrawn
        Dim it As ImportItem = stage.Item
        If it Is Nothing OrElse m_matcher Is Nothing OrElse Engine Is Nothing Then Return
        If it.Faces Is Nothing Then
            lblFoot.Text = "這張還在分析面孔，等一下再框"
            Return
        End If
        Dim face As ImportFace
        UseWaitCursor = True
        Try
            face = m_matcher.AddManual(Engine, it, box)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is OpenCvSharp.OpenCVException OrElse TypeOf ex Is ArgumentException
            face = Nothing
        Finally
            UseWaitCursor = False
        End Try
        If face Is Nothing Then
            lblFoot.Text = "這個範圍讀不出臉，框大一點再試"
            Return
        End If
        FacesEdited()
        stage.SelectedFace = face
        stage.BeginName(face)
    End Sub

    Private m_menuFace As ImportFace

    Private Sub stage_FaceMenu(face As ImportFace, pt As Point) Handles stage.FaceMenu
        m_menuFace = face
        mnuReject.Text = If(face.Name <> "", "不是 " & face.Name, "不是這個人")
        mnuReject.Enabled = face.PersonID <> 0
        mnuUnname.Enabled = face.Name <> ""
        mnuFace.Show(stage, pt)
    End Sub

    Private Sub mnuReject_Click(sender As Object, e As EventArgs) Handles mnuReject.Click
        m_menuFace?.Reject()
        stage.Changed()
    End Sub

    Private Sub mnuUnname_Click(sender As Object, e As EventArgs) Handles mnuUnname.Click
        If m_menuFace Is Nothing Then Return
        If m_menuFace.PersonID <> 0 AndAlso m_menuFace.State <> ImportFace.enumImportFaceState.ifConfirmed Then m_menuFace.Reject() Else m_menuFace.Clear()
        stage.Changed()
    End Sub

    Private Sub mnuNotFace_Click(sender As Object, e As EventArgs) Handles mnuNotFace.Click
        If m_menuFace Is Nothing Then Return
        m_menuFace.State = ImportFace.enumImportFaceState.ifNotFace
        stage.SelectedFace = Nothing
        stage.Changed()
    End Sub

    '==================================================================================================
    ' Checks before the import
    '==================================================================================================
    ''' <summary>The albums to make, or Nothing (and a message) when the card isn't right.</summary>
    Private Function BuildJobs() As List(Of ImportJob)
        CommitInspector()
        Dim target As AlbumTarget = TryCast(cboAlbum.SelectedItem, AlbumTarget)
        If target Is Nothing Then
            frmMsgBox.ShowCriticalMessage("請選擇相簿要存放在哪裡（設定 › 相片庫 裡加入相片庫）", "")
            Return Nothing
        End If
        Dim split As Boolean = chkSplit.Checked
        If Not split Then
            If txtFolder.Text.Trim() = "" Then
                frmMsgBox.ShowCriticalMessage("請輸入相簿資料夾名稱", "")
                txtFolder.Focus()
                Return Nothing
            End If
            If Not CheckNameRule(txtFolder.Text.Trim()) Then
                frmMsgBox.ShowCriticalMessage("資料夾名稱不能有 \ * / . "" 這些字元", "")
                txtFolder.Focus()
                Return Nothing
            End If
            If txtDate.Text.Trim() = "" Then
                frmMsgBox.ShowCriticalMessage("請輸入相簿的日期", "")
                txtDate.Focus()
                Return Nothing
            End If
        End If
        If txtTitle.Text.Trim() = "" Then
            frmMsgBox.ShowCriticalMessage("請輸入相簿名稱", "")
            txtTitle.Focus()
            Return Nothing
        End If

        Dim job As New ImportJob With {
            .AlbumPath = target.Path, .Folder = txtFolder.Text.Trim(), .Title = txtTitle.Text.Trim(), .DateText = txtDate.Text.Trim(),
            .Spot = txtSpot.Text.Trim(), .Icon = m_strIcon, .Remark = txtRemark.Text}
        job.Items.AddRange(m_included)
        job.AssignFileNames()
        Dim jobs As List(Of ImportJob) = If(split, job.SplitByDay(txtTitle.Text.Trim() = m_strAutoTitle), New List(Of ImportJob) From {job})
        Dim exists = jobs.Where(Function(j) IO.Directory.Exists(j.ClassPath)).ToList()
        If exists.Count > 0 Then
            frmMsgBox.ShowCriticalMessage("相簿已存在：" & String.Join("、", exists.Select(Function(j) j.Folder)) & vbCrLf & "請換一個資料夾名稱", "")
            If Not split Then txtFolder.Focus()
            Return Nothing
        End If
        Return jobs
    End Function

End Class
