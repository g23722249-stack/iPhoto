' 同步備份 (the tool bar's 同步備份, imgToolBox(14)): the whole 生活剪輯 folder mirrored to a backup drive or
' folder (Modules\LibBackup.vb has the rules) --
'   來源   生活剪輯 (where iPhoto is), can be changed
'   略過   the thumbnail cache, when ticked (temp files always)
'   目的   a drive (a card per drive: name, internal / USB / network, free space; backed up to
'          <drive>\<source folder name>) or any folder; kept by the drive's serial, so a USB drive under
'          another letter is still found
'   檢查   the drive there and writable, then the comparison (frmProgress): new / changed / to delete /
'          unchanged, with sizes, and whether the room is enough; nothing is written
'   開始備份  asks again before many deletions (BackupPlan.ManyDeletions), then runs in frmProgress
' Replaces the FolderSyncWPF-style window (a list of folders, deleted files moved to Lost).
' Made in code (no designer file).
Friend Class frmBackup
    Inherits Form

    Private ReadOnly lblLast As New Label
    Private ReadOnly txtSource As New TextBox
    Private ReadOnly butSource As New Button
    Private ReadOnly chkThumbs As New CheckBox
    Private ReadOnly pnlDrives As New FlowLayoutPanel
    Private ReadOnly lnkRefresh As New LinkLabel
    Private ReadOnly lblTarget As New Label
    Private ReadOnly lblCheck As New Label
    Private ReadOnly pnlMetrics(3) As Panel
    Private ReadOnly lblNumber(3) As Label
    Private ReadOnly lblSize(3) As Label
    Private ReadOnly butCheck As New Aqua.ThinButton, butStart As New Aqua.ThinButton, butClose As New Aqua.ThinButton
    Private ReadOnly m_lpHelp As New HelpTip

    Private m_strSource As String
    Private m_strTarget As String = ""
    Private m_lpPlan As BackupPlan

    Private Shared ReadOnly Accent As Color = Color.FromArgb(42, 116, 208)
    Private Shared ReadOnly MetricTitles As String() = {"新增", "覆蓋（較新或大小不同）", "刪除（來源已經沒有）", "略過（沒有異動）"}

    Public Sub New()
        Text = "同步備份"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False : MaximizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(720, 560)
        BackColor = Color.White

        Dim lbl = Function(t As String, y As Integer) New Label With {.Text = t, .AutoSize = True, .Location = New Point(16, y + 4), .ForeColor = Color.DimGray}
        lblLast.SetBounds(300, 8, 404, 20) : lblLast.TextAlign = ContentAlignment.TopRight : lblLast.ForeColor = Color.DimGray
        txtSource.SetBounds(80, 32, 500, 28) : txtSource.ReadOnly = True
        butSource.SetBounds(590, 31, 114, 30) : butSource.Text = "變更…"
        chkThumbs.SetBounds(80, 68, 624, 26)
        chkThumbs.Text = "縮圖快取（可重建；暫存檔一律略過）"
        pnlDrives.SetBounds(76, 102, 632, 150) : pnlDrives.AutoScroll = True
        lnkRefresh.SetBounds(16, 130, 60, 40) : lnkRefresh.Text = "重新" & vbCrLf & "整理"
        lblTarget.SetBounds(80, 256, 624, 24)
        Dim line As New Label With {.BorderStyle = BorderStyle.Fixed3D, .Bounds = New Rectangle(16, 290, 688, 2)}
        lblCheck.SetBounds(16, 300, 688, 44)
        For i = 0 To 3
            pnlMetrics(i) = New Panel With {.Bounds = New Rectangle(16 + i * 174, 350, 164, 86), .BackColor = Color.FromArgb(246, 247, 249)}
            Dim title As New Label With {.Text = MetricTitles(i), .Bounds = New Rectangle(8, 6, 150, 20), .ForeColor = Color.DimGray, .Font = New Font("Microsoft JhengHei UI", 9.0F)}
            lblNumber(i) = New Label With {.Bounds = New Rectangle(8, 26, 150, 32), .Font = New Font("Microsoft JhengHei UI", 15.0F, FontStyle.Bold), .Text = "—"}
            lblSize(i) = New Label With {.Bounds = New Rectangle(8, 60, 150, 20), .ForeColor = Color.DimGray, .Font = New Font("Microsoft JhengHei UI", 9.0F)}
            pnlMetrics(i).Controls.AddRange({title, lblNumber(i), lblSize(i)})
        Next
        For Each c As Control In {pnlMetrics(2), lblNumber(2), lblSize(2)}
            c.Cursor = Cursors.Hand
            AddHandler c.Click, Sub() ShowDeletions()
        Next

        butCheck.SetBounds(360, 510, 110, 34) : butCheck.Text = "檢查"
        butStart.SetBounds(478, 510, 118, 34) : butStart.Text = "開始備份"
        butClose.SetBounds(604, 510, 100, 34) : butClose.Text = "結束"
        For Each b In {butCheck, butStart, butClose}
            b.Font = New Font("華康細圓體", 12.0F)
            b.ForeColor = Color.Black
        Next
        Controls.AddRange({lblLast, lbl("來源", 32), txtSource, butSource, lbl("略過", 68), chkThumbs, lbl("目的", 102), lnkRefresh, pnlDrives,
                           lblTarget, line, lblCheck, pnlMetrics(0), pnlMetrics(1), pnlMetrics(2), pnlMetrics(3), butCheck, butStart, butClose})

        AddHandler butSource.Click, Sub() PickSource()
        AddHandler chkThumbs.CheckedChanged, Sub()
                                                 BackupSettings.SkipThumbs = chkThumbs.Checked
                                                 Invalidate_()
                                             End Sub
        AddHandler lnkRefresh.LinkClicked, Sub() FillDrives()
        AddHandler butCheck.Click, Sub() Check()
        AddHandler butStart.Click, Sub() StartBackup()
        AddHandler butClose.Click, Sub() Close()

        m_strSource = BackupSettings.Source()
        txtSource.Text = m_strSource
        chkThumbs.Checked = BackupSettings.SkipThumbs
        Dim last As String = BackupSettings.LastRun
        lblLast.Text = If(last = "", "還沒有備份過", "上次備份：" & last)
        LoadTarget()
        FillDrives()
        Invalidate_()

        m_lpHelp.SetHelp("backup.source", txtSource, butSource)
        m_lpHelp.SetHelp("backup.skip", chkThumbs)
        m_lpHelp.SetHelp("backup.target", pnlDrives, lblTarget)
        m_lpHelp.SetHelp("backup.check", butCheck)
        m_lpHelp.SetHelp("backup.start", butStart)
        m_lpHelp.SetHelp("common.close", butClose)
    End Sub

    '==================================================================================================
    ' Source and target
    '==================================================================================================
    Private Sub PickSource()
        Dim folder As String = frmBrowserFolder.GetFolder("選擇要備份的資料夾（通常是生活剪輯）").Trim()
        If folder = "" Then Return
        m_strSource = folder.TrimEnd("\"c)
        txtSource.Text = m_strSource
        BackupSettings.SetSource(m_strSource)
        FillDrives()
        Invalidate_()
    End Sub

    Private Sub LoadTarget()
        Dim label As String = "", missing As Boolean
        m_strTarget = BackupSettings.Target(label, missing)
        If missing Then
            m_strTarget = ""
            lblTarget.Text = $"上次的備份碟「{label}」沒有連接：接上後按左邊的「重新整理」，或選另一個位置。"
            lblTarget.ForeColor = Color.Firebrick
        Else
            ShowTarget()
        End If
    End Sub

    Private Sub ShowTarget()
        lblTarget.ForeColor = Color.FromArgb(60, 66, 76)
        lblTarget.Text = If(m_strTarget = "", "請選擇要備份到哪個磁碟或資料夾。", "備份到 " & m_strTarget)
    End Sub

    ''' <summary>A card per drive (not the source's), then 指定資料夾….</summary>
    Private Sub FillDrives()
        If m_strTarget = "" Then LoadTarget()
        For Each c As Control In pnlDrives.Controls.Cast(Of Control)().ToList()
            c.Dispose()
        Next
        pnlDrives.Controls.Clear()
        Dim sourceRoot As String = IO.Path.GetPathRoot(m_strSource)
        For Each d In BackupDrives.List(sourceRoot)
            Dim kind As String = If(d.Removable, "外接", If(d.Network, "網路", "內接"))
            Dim name As String = d.Root.TrimEnd("\"c) & " " & If(d.Label = "", "（未命名）", d.Label)
            Dim choice As BackupDrives.DriveChoice = d
            Dim selected As Boolean = m_strTarget <> "" AndAlso m_strTarget.StartsWith(d.Root, StringComparison.OrdinalIgnoreCase)
            pnlDrives.Controls.Add(Card(name, $"{kind} · 可用 {frmProgress.SizeText(d.Free)}", selected, Sub() PickDrive(choice)))
        Next
        Dim other As Boolean = m_strTarget <> "" AndAlso Not pnlDrives.Controls.Cast(Of Control)().Any(Function(c) CBool(c.Tag))
        pnlDrives.Controls.Add(Card(If(other, "其他資料夾", "指定資料夾…"), If(other, "按一下換別的", "NAS、網路磁碟、任何資料夾"), other, Sub() PickFolder()))
    End Sub

    Private Function Card(ByVal title As String, ByVal subtitle As String, ByVal selected As Boolean, ByVal onClick As Action) As Control
        Dim p As New Panel With {.Size = New Size(196, 62), .Margin = New Padding(4), .Cursor = Cursors.Hand, .Tag = selected,
                                 .BackColor = If(selected, Color.FromArgb(225, 236, 251), Color.FromArgb(246, 247, 249))}
        Dim t As New Label With {.Text = title, .Bounds = New Rectangle(10, 8, 180, 22), .Font = New Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold),
                                 .ForeColor = If(selected, Accent, Color.Black), .AutoEllipsis = True}
        Dim s As New Label With {.Text = subtitle, .Bounds = New Rectangle(10, 32, 180, 20), .ForeColor = Color.DimGray, .Font = New Font("Microsoft JhengHei UI", 9.0F), .AutoEllipsis = True}
        p.Controls.AddRange({t, s})
        For Each c As Control In {p, t, s}
            AddHandler c.Click, Sub() onClick()
        Next
        If selected Then AddHandler p.Paint, Sub(sender, e) e.Graphics.DrawRectangle(New Pen(Accent, 2), 1, 1, p.Width - 2, p.Height - 2)
        Return p
    End Function

    Private Sub PickDrive(ByVal d As BackupDrives.DriveChoice)
        Dim folder As String = IO.Path.Combine(d.Root, IO.Path.GetFileName(m_strSource))
        SetTarget(folder)
    End Sub

    Private Sub PickFolder()
        Dim folder As String = frmBrowserFolder.GetFolder("選擇備份的資料夾（生活剪輯會整個複製到這個資料夾裡）").Trim()
        If folder = "" Then Return
        SetTarget(folder.TrimEnd("\"c))
    End Sub

    ''' <summary>The target, kept by its drive's serial (a share by its path).</summary>
    Private Sub SetTarget(ByVal folder As String)
        Dim root As String = IO.Path.GetPathRoot(folder)
        Dim serial As String = If(root.StartsWith("\\"), "", BackupDrives.SerialOf(root))
        Dim label As String = ""
        Try
            If serial <> "" Then label = New IO.DriveInfo(root).VolumeLabel
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is ArgumentException
        End Try
        BackupSettings.SetTarget(folder, serial, label)
        m_strTarget = folder
        ShowTarget()
        FillDrives()
        Invalidate_()
    End Sub

    '==================================================================================================
    ' Checking and backing up
    '==================================================================================================
    ''' <summary>A choice changed: the comparison has to be done again.</summary>
    Private Sub Invalidate_()
        m_lpPlan = Nothing
        butStart.Enabled = False
        For i = 0 To 3
            lblNumber(i).Text = "—"
            lblSize(i).Text = ""
        Next
        pnlMetrics(2).BackColor = Color.FromArgb(246, 247, 249)
        lblCheck.ForeColor = Color.DimGray
        lblCheck.Text = "按「檢查」：先確認磁碟，再比對要新增、覆蓋、刪除哪些檔案（這一步不會寫入任何東西）。"
    End Sub

    Private Sub Check()
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        Invalidate_()
        If m_strTarget = "" Then LoadTarget()
        Dim problem As String = MirrorBackup.Problem(m_strSource, m_strTarget)
        If problem <> "" Then
            ShowProblem(problem)
            Return
        End If
        g_lpDatabase?.Checkpoint()   ' the database's latest changes into iPhoto.db itself
        Dim plan As BackupPlan = Nothing
        Dim skipThumbs As Boolean = chkThumbs.Checked
        Dim source As String = m_strSource, target As String = m_strTarget
        Dim err As Exception = Nothing
        Dim r As frmProgress.Outcome = frmProgress.Run(Me, "同步備份：比對中",
                                                       Sub(report, token) plan = MirrorBackup.Scan(source, target, skipThumbs, report, token), err)
        If r = frmProgress.Outcome.Cancelled Then Return
        If r = frmProgress.Outcome.Failed Then
            ShowProblem("比對失敗：" & err.Message)
            Return
        End If
        ShowPlan(plan)
    End Sub

    Private Sub ShowProblem(ByVal text As String)
        lblCheck.ForeColor = Color.Firebrick
        lblCheck.Text = "✗ " & text
    End Sub

    Private Sub ShowPlan(ByVal plan As BackupPlan)
        m_lpPlan = plan
        lblNumber(0).Text = plan.Adds.Count.ToString("#,0") : lblSize(0).Text = frmProgress.SizeText(plan.Adds.Sum(Function(a) a.Size))
        lblNumber(1).Text = plan.Updates.Count.ToString("#,0") : lblSize(1).Text = frmProgress.SizeText(plan.Updates.Sum(Function(u) u.Size))
        lblNumber(2).Text = plan.Deletes.Count.ToString("#,0")
        lblSize(2).Text = If(plan.Deletes.Count + plan.DeleteFolders.Count > 0 OrElse plan.OldLost <> "", "按一下看清單", "")
        lblNumber(3).Text = plan.Skipped.ToString("#,0") : lblSize(3).Text = frmProgress.SizeText(plan.SkippedBytes)
        pnlMetrics(2).BackColor = If(plan.ManyDeletions, Color.FromArgb(253, 236, 200), Color.FromArgb(246, 247, 249))

        Dim free As Long = BackupDrives.FreeSpace(plan.Target)
        Dim need As Long = plan.NeededBytes
        Dim lines As New List(Of String)
        If free >= 0 AndAlso need + 100L * 1024 * 1024 > free Then
            ShowProblem($"空間不夠：需要 {frmProgress.SizeText(need)}，{IO.Path.GetPathRoot(plan.Target)} 只剩 {frmProgress.SizeText(free)}。")
            Return
        End If
        lines.Add("✓ 檢查完成：" & IO.Path.GetPathRoot(plan.Target).TrimEnd("\"c) & " 已連接、可寫入" &
                  If(free >= 0, $"，空間足夠（需要 {frmProgress.SizeText(need)}，可用 {frmProgress.SizeText(free)}）", ""))
        Dim extra As New List(Of String)
        If plan.OldLost <> "" Then extra.Add($"會清掉舊的 Lost 資料夾（{plan.OldLostFiles:#,0} 個檔案，{frmProgress.SizeText(plan.OldLostBytes)}）")
        If plan.ManyDeletions Then extra.Add("要刪除的檔案很多，開始前會再確認一次")
        If plan.NothingToDo Then extra.Add("兩邊已經一樣，不需要備份")
        If extra.Count > 0 Then lines.Add(String.Join("；", extra) & "。")
        lblCheck.ForeColor = Color.FromArgb(40, 120, 60)
        lblCheck.Text = String.Join(vbCrLf, lines)
        butStart.Enabled = Not plan.NothingToDo
    End Sub

    Private Sub ShowDeletions()
        If m_lpPlan Is Nothing Then Return
        Dim items As New List(Of String)
        If m_lpPlan.OldLost <> "" Then items.Add($"[舊的 Lost 資料夾] {m_lpPlan.OldLost}（{m_lpPlan.OldLostFiles:#,0} 個檔案）")
        items.AddRange(m_lpPlan.Deletes.Select(Function(d) d.Relative & "　" & frmProgress.SizeText(d.Size)))
        items.AddRange(m_lpPlan.DeleteFolders.Select(Function(d) d & "\　（資料夾）"))
        If items.Count = 0 Then Return
        Using f As New Form With {.Text = "會刪除的檔案（備份碟上）", .Font = Font, .ClientSize = New Size(640, 420), .StartPosition = FormStartPosition.CenterParent,
                                  .MinimizeBox = False, .ShowInTaskbar = False, .BackColor = Color.White}
            Dim lst As New ListBox With {.Dock = DockStyle.Fill, .IntegralHeight = False, .HorizontalScrollbar = True}
            lst.Items.AddRange(items.Cast(Of Object)().ToArray())
            f.Controls.Add(lst)
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub StartBackup()
        Dim plan As BackupPlan = m_lpPlan
        If plan Is Nothing Then Return
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        If plan.ManyDeletions AndAlso Not frmQueryMsgBox.ShowMessage(
                $"這次要在備份碟上刪除 {plan.Deletes.Count:#,0} 個檔案（備份裡共 {plan.TargetFiles:#,0} 個）。" & vbCrLf &
                "如果是來源磁碟沒接好或資料夾被清空，請先按「否」檢查。" & vbCrLf & "確定要刪除並繼續？", "同步備份", defaultNo:=True) Then Return
        ' still the same drive and the same files? (time may have passed since 檢查)
        Dim problem As String = MirrorBackup.Problem(plan.Source, plan.Target)
        If problem <> "" Then
            ShowProblem(problem)
            butStart.Enabled = False
            Return
        End If
        g_lpDatabase?.Checkpoint()
        Dim outcome As BackupOutcome = Nothing
        Dim err As Exception = Nothing
        Dim r As frmProgress.Outcome = frmProgress.Run(Me, "同步備份", Sub(report, token) outcome = MirrorBackup.Run(plan, report, token), err)
        Dim where As String = IO.Path.GetPathRoot(plan.Target).TrimEnd("\"c)
        Select Case r
            Case frmProgress.Outcome.Done
                Dim label As String = ""
                Try
                    If Not where.StartsWith("\\") Then label = New IO.DriveInfo(where & "\").VolumeLabel
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is ArgumentException
                End Try
                BackupSettings.LastRun = Now.ToString("yyyy/MM/dd HH:mm") & "（" & If(label = "", "", label & " ") & where & "）"
                lblLast.Text = "上次備份：" & BackupSettings.LastRun
                g_lpConfig.PlaySound(Config.enumSound.snExportFinish)
                Dim msg As String = $"同步備份完成" & vbCrLf & $"複製 {outcome.Copied:#,0} 個（{frmProgress.SizeText(outcome.Bytes)}），刪除 {outcome.Deleted:#,0} 個"
                If outcome.Failed > 0 Then
                    frmMsgBox.ShowExclamationMessage(msg & vbCrLf & $"{outcome.Failed} 個失敗，例如：" & vbCrLf & String.Join(vbCrLf, outcome.Failures.Take(3)) &
                                                     vbCrLf & "（備份碟上的「" & MirrorBackup.BackupInfoFile & "」有全部清單）", "同步備份")
                Else
                    frmMsgBox.ShowSmileMessage(msg, "同步備份")
                End If
            Case frmProgress.Outcome.Cancelled
                frmMsgBox.ShowExclamationMessage("同步備份已取消。已經複製好的檔案會保留，下次備份會從沒做完的地方接著做。", "同步備份")
            Case frmProgress.Outcome.Failed
                frmMsgBox.ShowCriticalMessage("同步備份失敗" & vbCrLf & err.Message, "同步備份")
        End Select
        Invalidate_()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then m_lpHelp.Dispose()
        MyBase.Dispose(disposing)
    End Sub

End Class
