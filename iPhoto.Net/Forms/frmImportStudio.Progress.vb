' 輸入照片 › ③ 匯入 (frmImportStudio): ImportRunner on its worker thread; this page shows the photo in
' hand, the whole progress with the time left, one line per part of the work (album folder, copy,
' .Exif, PhotoIndex, faces) and the log. 暫停 / 繼續 between photos; 停止匯入 stops after the photo in
' hand (what was imported stays). At the end: a summary, 打開相簿 (closes the window, frmMain opens the
' album), 再匯入一批 (back to ①) and 重試失敗的 when some photos failed.
Partial Class frmImportStudio

    Private picCurrent As PictureBox
    Private lblRunTitle, lblRunWhere, lblRunFile, lblRunCount, lblSummary As Label
    Private barTotal As ImportBar
    Private lblPhase(4), lblPhaseCount(4), lblPhaseState(4) As Label
    Private barPhase(4) As ImportBar
    Private lstLog As ListBox
    Private pnlSummary As Panel
    Private WithEvents butOpen, butAgain, butRetry As Aqua.ThinButton
    Private WithEvents m_runner As ImportRunner
    Private m_bolCloseWhenStopped As Boolean

    Private Sub BuildStep3(ByVal p As Panel)
        Dim main As Panel = NewBox(p, 10, 10, 790, 676)
        picCurrent = New PictureBox With {.Location = New Point(24, 24), .Size = New Size(200, 150), .SizeMode = PictureBoxSizeMode.Zoom, .BackColor = Color.FromArgb(226, 230, 235)}
        main.Controls.Add(picCurrent)
        lblRunTitle = NewLabel(main, "", 244, 26, 520, 32, 16.0F, FontStyle.Bold)
        lblRunWhere = NewLabel(main, "", 244, 62, 520, 24, 11.0F, color:=Muted)
        lblRunWhere.AutoEllipsis = True
        lblRunFile = NewLabel(main, "", 244, 112, 330, 24, 11.0F)
        lblRunFile.AutoEllipsis = True
        lblRunCount = NewLabel(main, "", 574, 112, 190, 24, 11.0F, color:=Muted)
        lblRunCount.TextAlign = ContentAlignment.TopRight
        barTotal = New ImportBar With {.Location = New Point(244, 140), .Size = New Size(520, 16)}
        main.Controls.Add(barTotal)

        Dim names() As String = {"建立相簿資料夾（olyalbum.inf、Note.Ini）", "複製檔案", "寫入 .Exif", "同步 PhotoIndex", "寫入面孔資料"}
        For i = 0 To 4
            Dim y As Integer = 214 + i * 44
            lblPhaseState(i) = NewLabel(main, "", 24, y, 24, 24, 11.0F, FontStyle.Bold, Color.White)
            lblPhaseState(i).TextAlign = ContentAlignment.MiddleCenter
            lblPhase(i) = NewLabel(main, names(i), 58, y + 2, 360, 24, 11.5F)
            lblPhaseCount(i) = NewLabel(main, "", 420, y + 2, 120, 24, 11.0F, color:=Muted)
            lblPhaseCount(i).TextAlign = ContentAlignment.TopRight
            barPhase(i) = New ImportBar With {.Location = New Point(560, y + 6), .Size = New Size(200, 12)}
            main.Controls.Add(barPhase(i))
            Dim sep As New Label With {.Location = New Point(24, y + 36), .Size = New Size(740, 1), .BackColor = Color.FromArgb(225, 229, 234)}
            main.Controls.Add(sep)
        Next
        NewLabel(main, "匯入時會先暫停背景的面孔分析；關閉這個視窗後自動繼續。取消只會停在目前這張，已匯入的照片會留著。", 24, 440, 740, 48, 10.0F, color:=Muted)

        pnlSummary = NewBox(main, 24, 500, 740, 160)
        pnlSummary.BackColor = Color.FromArgb(242, 250, 245)
        pnlSummary.Visible = False
        lblSummary = NewLabel(pnlSummary, "", 16, 12, 708, 100, 11.5F)
        butOpen = NewButton(pnlSummary, "打開相簿", 16, 120, 120, primary:=True)
        butAgain = NewButton(pnlSummary, "再匯入一批", 146, 120, 120)
        butRetry = NewButton(pnlSummary, "重試失敗的", 276, 120, 120)

        NewLabel(p, "紀錄", 812, 10, 200, 22, 11.0F, FontStyle.Bold, Muted)
        Dim logBox As Panel = NewBox(p, 810, 34, 380, 652)
        lstLog = New ListBox With {.Location = New Point(1, 1), .Size = New Size(378, 650), .BorderStyle = BorderStyle.None,
                                   .Font = New Font("Consolas", 9.5F), .IntegralHeight = False, .HorizontalScrollbar = True,
                                   .DrawMode = DrawMode.OwnerDrawFixed, .ItemHeight = 18}
        AddHandler lstLog.DrawItem, AddressOf lstLog_DrawItem
        logBox.Controls.Add(lstLog)
    End Sub

    Private Sub lstLog_DrawItem(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        e.DrawBackground()
        Dim line As LogEntry = CType(lstLog.Items(e.Index), LogEntry)
        Dim font As Font = If(line.Text.Any(Function(c) AscW(c) > 255), New Font(Me.Font.FontFamily, 9.5F), lstLog.Font)
        TextRenderer.DrawText(e.Graphics, line.Text, font, e.Bounds, If(line.Warning, Color.FromArgb(184, 116, 15), Ink), TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
        If font IsNot lstLog.Font Then font.Dispose()
    End Sub

    Private Class LogEntry
        Public Text As String
        Public Warning As Boolean
        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    '==================================================================================================
    ' Running
    '==================================================================================================
    Private Function RunnerBusy() As Boolean
        Return m_runner IsNot Nothing AndAlso m_runner.IsRunning
    End Function

    Private Sub StartImport()
        Dim jobs As List(Of ImportJob) = BuildJobs()   ' Organize.vb
        If jobs Is Nothing Then Return
        StopFaceAnalysis()
        If Not m_included.All(Function(i) Not i.IsPicture OrElse i.Faces IsNot Nothing) AndAlso m_matcher IsNot Nothing Then
            AddLog("有些照片還沒分析完面孔，匯入後會在背景補分析", True)
        End If
        Dim target As String = jobs(0).AlbumPath
        SaveSetting(AppName, "Import Recent", "Album", target)   ' the album imported into (as frmImport kept it)

        lstLog.Items.Clear()
        pnlSummary.Visible = False
        lblRunTitle.Text = If(jobs.Count = 1, "正在匯入「" & jobs(0).Title & "」", "正在匯入 " & jobs.Count & " 本相簿")
        lblRunWhere.Text = String.Join("、", jobs.Select(Function(j) j.ClassPath))
        m_runner = New ImportRunner(jobs, g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)) With {
            .WriteIndex = g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement,
            .WriteFaces = g_lpFaces IsNot Nothing AndAlso g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.FaceTablesReady}
        ' the tool bar's database connection must not hold a lock the import needs
        If g_lpFaces IsNot Nothing AndAlso g_lpFaces.IsRunning AndAlso Not g_lpFaces.IsPaused Then
            g_lpFaces.Pause()
            m_bolFacesPaused = True
        End If
        ShowStep(2)
        butNext.Enabled = False
        m_runner.Start()
        ShowRunnerProgress()
    End Sub

    Private Sub AddLog(ByVal text As String, ByVal warning As Boolean)
        lstLog.Items.Add(New LogEntry With {.Text = text, .Warning = warning})
        lstLog.TopIndex = Math.Max(0, lstLog.Items.Count - 1)
    End Sub

    Private Sub m_runner_LogLine(strLine As String, bolWarning As Boolean) Handles m_runner.LogLine
        AddLog(strLine, bolWarning)
    End Sub

    Private Sub m_runner_Progress(sender As Object, e As EventArgs) Handles m_runner.Progress
        ShowRunnerProgress()
    End Sub

    Private Sub ShowRunnerProgress()
        If m_runner Is Nothing Then Return
        Dim r As ImportRunner = m_runner
        Dim total As Integer = Math.Max(1, r.Total)
        barTotal.Value = r.Done / CDbl(total)
        Dim left As Integer = r.SecondsLeft()
        lblRunCount.Text = r.Done & " / " & r.Total & If(left > 0, " · 約剩 " & If(left >= 90, (left \ 60) & " 分", left & " 秒"), "")
        Dim cur As ImportItem = r.Current
        If cur IsNot Nothing Then
            lblRunFile.Text = cur.FileName
            ShowCurrentPicture(cur)
        End If
        Dim folders As Integer = r.Jobs.Count
        Dim pics As Integer = r.Jobs.Sum(Function(j) j.Included.Where(Function(i) i.IsPicture AndAlso i.Faces IsNot Nothing).Count())
        Dim wanted() As Integer = {folders, r.Total, r.Total, If(r.WriteIndex, r.Total, 0), If(r.WriteFaces, pics, 0)}
        For i = 0 To 4
            Dim n As Integer = r.Counts(i)
            Dim w As Integer = wanted(i)
            lblPhaseCount(i).Text = If(w = 0, "略過", n & " / " & w)
            barPhase(i).Value = If(w = 0, 0, Math.Min(1.0, n / CDbl(w)))
            Dim state As String, back As Color
            If w = 0 Then
                state = "–" : back = Color.FromArgb(195, 202, 211)
            ElseIf n >= w Then
                state = "✓" : back = Green
            ElseIf r.IsRunning Then
                state = "▸" : back = Accent
            Else
                state = "!" : back = Amber
            End If
            lblPhaseState(i).Text = state
            lblPhaseState(i).BackColor = back
        Next
    End Sub

    Private Sub ShowCurrentPicture(ByVal it As ImportItem)
        Dim bmp As Bitmap = Nothing
        If m_cache IsNot Nothing AndAlso m_cache.TryGet(it.SourceFile, bmp) AndAlso bmp.Width > 1 Then
            picCurrent.Image = bmp   ' owned by the cache
        End If
    End Sub

    Private Sub m_runner_Finished(sender As Object, e As EventArgs) Handles m_runner.Finished
        Dim r As ImportRunner = m_runner
        ShowRunnerProgress()
        butNext.Enabled = True
        butBack.Visible = False
        butCancel.Text = "關閉"
        butNext.Text = "完成"
        lblRunFile.Text = ""
        For Each j In r.MadeJobs
            If Not m_imported.Contains(j) Then m_imported.Add(j)
        Next
        m_intImportedCount = m_imported.Sum(Function(j) j.Included.Where(Function(i) r.Imported.Contains(i)).Count())

        If r.FatalError IsNot Nothing Then
            lblRunTitle.Text = "匯入沒有完成"
            AddLog(r.FatalError, True)
        ElseIf r.Cancelled Then
            lblRunTitle.Text = "已停止匯入"
        Else
            lblRunTitle.Text = "匯入完成"
            g_lpConfig.PlaySound(Config.enumSound.snImportFinish)
        End If

        Dim videos As Integer = r.Imported.Where(Function(i) i.IsVideo).Count()
        Dim withPeople As Integer = r.Imported.Where(Function(i) i.Character() <> "").Count()
        Dim people As Integer = r.Imported.SelectMany(Function(i) FaceNames.Split(i.Character())).Distinct().Count()
        Dim sb As New Text.StringBuilder
        sb.AppendLine("已匯入 " & (r.Imported.Count - videos) & " 張照片" & If(videos > 0, "、" & videos & " 部影片", "") &
                      If(r.MadeJobs.Count > 1, "，" & r.MadeJobs.Count & " 本相簿", ""))
        If withPeople > 0 Then sb.AppendLine("有人名的 " & withPeople & " 張（" & people & " 位）")
        Dim skipped As Integer = m_items.Where(Function(i) Not i.Include AndAlso i.DuplicateOf <> "").Count()
        If skipped > 0 Then sb.AppendLine("略過 " & skipped & " 張已在相片庫的照片")
        If r.Cancelled Then sb.AppendLine("還有 " & (r.Total - r.Done) & " 張沒有匯入（按「再匯入一批」可以接著選）")
        sb.AppendLine(If(r.Failures.Count = 0, "沒有錯誤", r.Failures.Count & " 張失敗，詳見右邊的紀錄"))
        lblSummary.Text = sb.ToString().TrimEnd()
        butRetry.Visible = r.Failures.Count > 0
        butOpen.Enabled = r.MadeJobs.Count > 0
        pnlSummary.Visible = True
        If m_bolCloseWhenStopped Then Close()
    End Sub

    Private Sub CancelRunner()
        If Not RunnerBusy() Then Return
        m_runner.Cancel()
        butCancel.Enabled = False
        lblRunTitle.Text = "正在停止…（做完目前這張）"
    End Sub

    Private Sub StopRunnerAndClose()
        m_bolCloseWhenStopped = True
        CancelRunner()
    End Sub

    Private Sub PauseOrResumeRunner()
        If Not RunnerBusy() Then Return
        If m_runner.IsPaused Then
            m_runner.Resume()
            butBack.Text = "暫停"
            lblRunTitle.Text = lblRunTitle.Text.Replace("（已暫停）", "")
        Else
            m_runner.Pause()
            butBack.Text = "繼續"
            lblRunTitle.Text &= "（已暫停）"
        End If
    End Sub

    Private Sub butOpen_Click(sender As Object, e As EventArgs) Handles butOpen.Click
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub butRetry_Click(sender As Object, e As EventArgs) Handles butRetry.Click
        If m_runner Is Nothing OrElse RunnerBusy() Then Return
        pnlSummary.Visible = False
        butCancel.Enabled = True
        butCancel.Text = "停止匯入"
        butBack.Visible = True
        butBack.Text = "暫停"
        butNext.Enabled = False
        lblRunTitle.Text = "重試失敗的照片"
        m_runner.Start(retry:=True)
    End Sub

    ''' <summary>Back to ① for another batch; what was imported is kept for frmMain.</summary>
    Private Sub butAgain_Click(sender As Object, e As EventArgs) Handles butAgain.Click
        If RunnerBusy() Then Return
        butCancel.Enabled = True
        m_runner = Nothing
        m_items = New List(Of ImportItem)
        m_included = New List(Of ImportItem)
        m_bolAlbumEdited = False
        m_strSource = ""
        grid1.SetItems(m_items)
        grid2.SetItems(m_included)
        stage.ShowItem(Nothing)
        FillQuickList()
        BuildLibraryIndex()   ' the photos just imported are in the library now
        ShowStep(0)
        UpdateStep1Info()
    End Sub

End Class

''' <summary>A thin progress bar (0..1) drawn in the window's colours.</summary>
Friend Class ImportBar
    Inherits Control

    Private m_dblValue As Double

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
    End Sub

    Public Property Value As Double
        Get
            Return m_dblValue
        End Get
        Set(value As Double)
            value = Math.Max(0, Math.Min(1, value))
            If value = m_dblValue Then Return
            m_dblValue = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Dim r As New RectangleF(0.5F, 0.5F, Width - 1.5F, Height - 1.5F)
        Using path = RoundRect(r, Height / 2.0F), b As New SolidBrush(Color.FromArgb(213, 218, 225)), p As New Pen(Color.FromArgb(181, 189, 200))
            g.FillPath(b, path)
            If m_dblValue > 0 Then
                Dim w As Single = Math.Max(Height, CSng((Width - 1) * m_dblValue))
                Using fill = RoundRect(New RectangleF(0.5F, 0.5F, w - 1, Height - 1.5F), Height / 2.0F),
                      hb As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.WideUpwardDiagonal, Color.FromArgb(94, 156, 234), Color.FromArgb(63, 134, 222))
                    g.FillPath(hb, fill)
                End Using
            End If
            g.DrawPath(p, path)
        End Using
    End Sub

End Class
