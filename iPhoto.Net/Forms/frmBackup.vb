' 同步備份 (new in the .NET version; opened by the tool bar's 同步備份 button, imgToolBox(14)): the
' FolderSyncWPF tool redone in iPhoto's style. A list of folders to back up, each with its own target
' (or one target for all), backed up one way -- see LibBackup.vb for the rules. Layout and captions
' live in the designer; the list and options are kept in the registry (BackupSettings).
' Callers use "Using f As New frmBackup".
Imports System.Threading
Imports System.Threading.Tasks

Friend Class frmBackup

    Private m_sources As New List(Of BackupSource)
    Private m_cts As CancellationTokenSource        ' set while a backup runs
    Private m_loading As Boolean                     ' filling the controls: not the user's edit
    Private ReadOnly m_log As New Queue(Of String)()
    Private Const LogLines As Integer = 400           ' the window keeps the last lines; sync_log.txt has all
    Private WithEvents m_logTimer As New System.Windows.Forms.Timer With {.Interval = 200}
    Private m_logDirty As Boolean

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim applyAll As Boolean, common As String = ""
        m_loading = True
        m_sources = BackupSettings.Load(applyAll, common)
        chkApplyAll.Checked = applyAll
        If applyAll Then txtTarget.Text = common
        m_loading = False
        RefreshList(0)
        ProgressBar1.Value = 0
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If m_cts IsNot Nothing Then
            ' a backup is running: stop it first (the window closes with 結束 afterwards)
            e.Cancel = True
            m_cts.Cancel()
            Return
        End If
        SaveSettings()
    End Sub

    '==================================================================================================
    ' The list
    '==================================================================================================
    Private Shared Function RowText(ByVal s As BackupSource) As String
        If s.TargetPath.Trim() = "" Then Return s.SourcePath & "  →  (尚未指定目的)"
        Try
            Return s.SourcePath & "  →  " & FolderBackup.TargetRootFor(s.SourcePath, s.TargetPath)   ' where it really goes
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException OrElse TypeOf ex Is IO.PathTooLongException
            Return s.SourcePath & "  →  " & s.TargetPath
        End Try
    End Function

    Private Sub RefreshList(ByVal selectIndex As Integer)
        lstSource.Clear()
        For Each s As BackupSource In m_sources
            lstSource.AddItem(s.SourcePath, RowText(s))
        Next
        If m_sources.Count > 0 Then lstSource.SelectedIndex = Math.Max(0, Math.Min(selectIndex, m_sources.Count - 1))
        ShowSelectedTarget()
    End Sub

    Private Sub RefreshRowTexts()
        For i As Integer = 0 To m_sources.Count - 1
            lstSource.Item(i).Text = RowText(m_sources(i))
        Next
        lstSource.Invalidate()
    End Sub

    ''' <summary>The target box shows the selected source's target (or, with 套用至所有來源, the common one).</summary>
    Private Sub ShowSelectedTarget()
        If chkApplyAll.Checked Then Return
        Dim i As Integer = lstSource.SelectedIndex
        m_loading = True
        txtTarget.Text = If(i >= 0 AndAlso i < m_sources.Count, m_sources(i).TargetPath, "")
        m_loading = False
    End Sub

    Private Sub lstSource_SelectedChanged(sender As Object, e As EventArgs) Handles lstSource.SelectedChanged
        ShowSelectedTarget()
    End Sub

    ' 新增、刪除來源
    Private Sub imgSource_Click(sender As Object, e As EventArgs) Handles imgSource_0.Click, imgSource_1.Click
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(CType(sender, Control), 20)
        If sender Is imgSource_0 Then
            Dim folder As String = frmBrowserFolder.GetFolder("選擇要備份的資料夾").Trim()
            If folder = "" Then Return
            For i As Integer = 0 To m_sources.Count - 1
                If String.Equals(m_sources(i).SourcePath.TrimEnd("\"c), folder.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then
                    lstSource.SelectedIndex = i
                    Return
                End If
            Next
            m_sources.Add(New BackupSource With {.SourcePath = folder, .TargetPath = If(chkApplyAll.Checked, txtTarget.Text.Trim(), "")})
            RefreshList(m_sources.Count - 1)
        Else
            Dim i As Integer = lstSource.SelectedIndex
            If i < 0 OrElse i >= m_sources.Count Then Return
            m_sources.RemoveAt(i)
            RefreshList(i)
        End If
        SaveSettings()
    End Sub

    '==================================================================================================
    ' The target
    '==================================================================================================
    Private Sub imgTarget_Click(sender As Object, e As EventArgs) Handles imgTarget.Click
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgTarget, 20)
        Dim folder As String = frmBrowserFolder.GetFolder("選擇備份的目的資料夾").Trim()
        If folder <> "" Then txtTarget.Text = folder   ' -> txtTarget_TextChanged
    End Sub

    Private Sub txtTarget_TextChanged(sender As Object, e As EventArgs) Handles txtTarget.TextChanged
        If m_loading Then Return
        Dim t As String = txtTarget.Text.Trim()
        If chkApplyAll.Checked Then
            For Each s As BackupSource In m_sources
                s.TargetPath = t
            Next
        Else
            Dim i As Integer = lstSource.SelectedIndex
            If i < 0 OrElse i >= m_sources.Count Then Return
            m_sources(i).TargetPath = t
        End If
        RefreshRowTexts()
    End Sub

    Private Sub txtTarget_Leave(sender As Object, e As EventArgs) Handles txtTarget.Leave
        SaveSettings()
    End Sub

    Private Sub chkApplyAll_CheckedChanged(sender As Object, e As EventArgs) Handles chkApplyAll.CheckedChanged
        If m_loading Then Return
        If chkApplyAll.Checked Then
            txtTarget_TextChanged(txtTarget, EventArgs.Empty)   ' the box's target now goes for every source
        Else
            ShowSelectedTarget()
        End If
        SaveSettings()
    End Sub

    Private Sub SaveSettings()
        BackupSettings.Save(m_sources, chkApplyAll.Checked, If(chkApplyAll.Checked, txtTarget.Text.Trim(), ""))
    End Sub

    '==================================================================================================
    ' Backing up
    '==================================================================================================
    Private Async Sub butStart_Click(sender As Object, e As EventArgs) Handles butStart.Click
        If m_cts IsNot Nothing Then Return
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        SaveSettings()
        If m_sources.Count = 0 Then
            frmMsgBox.ShowExclamationMessage("請先按「＋」新增要備份的資料夾。", "同步備份")
            Return
        End If
        For Each s As BackupSource In m_sources
            Dim problem As String = FolderBackup.Problem(s.SourcePath, s.TargetPath)
            If problem <> "" Then
                frmMsgBox.ShowExclamationMessage(problem, "同步備份")
                Return
            End If
        Next

        SetBusy(True)
        m_log.Clear()
        txtLog.Text = ""
        ProgressBar1.Value = 0
        m_cts = New CancellationTokenSource()
        Dim token As CancellationToken = m_cts.Token
        Dim progress As New Progress(Of BackupProgress)(AddressOf OnProgress)
        Dim changes As Integer = 0, failed As Integer = 0
        Try
            For Each s As BackupSource In m_sources.ToArray()
                Dim src As String = s.SourcePath, dst As String = s.TargetPath
                AppendLog("開始：" & src & "  →  " & FolderBackup.TargetRootFor(src, dst))
                Dim r As BackupResult = Await Task.Run(Function() FolderBackup.Run(src, dst, progress, token))
                AppendLog($"完成：新增 {r.Added}、覆蓋 {r.Updated}、移到 Lost {r.MovedToLost}" &
                          If(r.Failed > 0, $"、失敗 {r.Failed}", "") & "（日誌：" & r.LogFile & "）")
                changes += r.Changes
                failed += r.Failed
            Next
            FlushLog()
            g_lpConfig.PlaySound(Config.enumSound.snExportFinish)
            frmMsgBox.ShowSmileMessage("同步備份完成" & vbCrLf & $"共 {changes} 個變更" & If(failed > 0, $"，{failed} 個失敗（見日誌）", ""), "同步備份")
        Catch ex As OperationCanceledException
            AppendLog("已取消")
            FlushLog()
            frmMsgBox.ShowExclamationMessage("同步備份已取消。", "同步備份")
        Catch ex As Exception
            AppendLog("錯誤：" & ex.Message)
            FlushLog()
            frmMsgBox.ShowCriticalMessage("同步備份失敗" & vbCrLf & ex.Message, "同步備份")
        Finally
            m_cts.Dispose()
            m_cts = Nothing
            SetBusy(False)
        End Try
    End Sub

    Private Sub OnProgress(ByVal p As BackupProgress)
        ProgressBar1.Value = p.Percent
        If Not String.IsNullOrEmpty(p.Message) Then AppendLog(p.Message)
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        If m_cts IsNot Nothing Then
            m_cts.Cancel()   ' 取消 while running
        Else
            Close()
        End If
    End Sub

    Private Sub SetBusy(ByVal busy As Boolean)
        butStart.Enabled = Not busy
        butExit.Text = If(busy, "取消", "結束")
        For Each c As Control In New Control() {imgSource_0, imgSource_1, lstSource, imgTarget, txtTarget, chkApplyAll}
            c.Enabled = Not busy
        Next
        If busy Then m_logTimer.Start() Else m_logTimer.Stop()
    End Sub

    '==================================================================================================
    ' The log (a file per file is too much to put into the box one by one: batched every 200 ms)
    '==================================================================================================
    Private Sub AppendLog(ByVal text As String)
        m_log.Enqueue(DateTime.Now.ToString("HH:mm:ss") & "  " & text)
        While m_log.Count > LogLines
            m_log.Dequeue()
        End While
        m_logDirty = True
    End Sub

    Private Sub m_logTimer_Tick(sender As Object, e As EventArgs) Handles m_logTimer.Tick
        FlushLog()
    End Sub

    Private Sub FlushLog()
        If Not m_logDirty Then Return
        m_logDirty = False
        txtLog.Text = String.Join(vbCrLf, m_log.ToArray())
        txtLog.ScrollToEnd()   ' show the latest lines
    End Sub

    ''' <summary>A short frame around a clicked icon (as frmSetup's).</summary>
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

End Class
