' A progress window any long job can use (new in the .NET port; first used by 同步備份): the job runs on a
' worker thread and reports a ProgressState now and then; the window shows, ten times a second, the
' step, the item being done with its own bar (a file's bytes), the overall bar, and counts, size, speed
' and the time left. 取消 (or closing the window) cancels the job through its token.
'   Dim r As frmProgress.Outcome = frmProgress.Run(Me, "同步備份", Sub(report, token) ... report(New ProgressState With {...}) ...)
' Run returns when the job has ended: Done, Cancelled, or Failed (with Error).
' Made in code (no designer file).
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Where a job is (all optional: the window shows what is set).</summary>
Friend Class ProgressState
    ''' <summary>What is being done ("複製中", "刪除舊檔案").</summary>
    Public [Step] As String = ""
    ''' <summary>The item being done (a file's path).</summary>
    Public Item As String = ""
    ''' <summary>The item's own progress (bytes of a file); ItemTotal 0 = no item bar.</summary>
    Public ItemDone As Long, ItemTotal As Long
    ''' <summary>Items done / in all (files).</summary>
    Public Done As Long, Total As Long
    ''' <summary>Bytes done / in all; BytesTotal 0 = the overall bar follows Done / Total.</summary>
    Public BytesDone As Long, BytesTotal As Long

    Public Function Clone() As ProgressState
        Return DirectCast(MemberwiseClone(), ProgressState)
    End Function
End Class

Friend Class frmProgress
    Inherits Form

    Public Enum Outcome
        Done
        Cancelled
        Failed
    End Enum

    Private ReadOnly lblStep As New Label
    Private ReadOnly lblItem As New Label
    Private ReadOnly barItem As New ProgressBar
    Private ReadOnly barAll As New ProgressBar
    Private ReadOnly lblStats As New Label
    Private ReadOnly butCancel As New Aqua.ThinButton
    Private ReadOnly m_tmrShow As New System.Windows.Forms.Timer With {.Interval = 100}

    Private ReadOnly m_lpJob As Action(Of Action(Of ProgressState), CancellationToken)
    Private ReadOnly m_lpCancel As New CancellationTokenSource()
    Private ReadOnly m_lpLock As New Object
    Private m_lpLatest As ProgressState
    Private ReadOnly m_lpClock As New Diagnostics.Stopwatch
    Private m_bolEnded As Boolean

    Public Property Result As Outcome = Outcome.Done
    ''' <summary>What went wrong (Failed).</summary>
    Public Property [Error] As Exception

    ''' <summary>Runs <paramref name="job"/> on a worker thread with this window over <paramref name="owner"/>;
    ''' returns when it has ended. The job reports through its first argument (from any thread) and should
    ''' stop at its token (OperationCanceledException is a cancel, not a failure).</summary>
    Public Shared Function Run(ByVal owner As IWin32Window, ByVal title As String,
                               ByVal job As Action(Of Action(Of ProgressState), CancellationToken), Optional ByRef [error] As Exception = Nothing) As Outcome
        Using f As New frmProgress(title, job)
            f.ShowDialog(owner)
            [error] = f.Error
            Return f.Result
        End Using
    End Function

    Private Sub New(ByVal title As String, ByVal job As Action(Of Action(Of ProgressState), CancellationToken))
        m_lpJob = job
        Text = title
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False : MaximizeBox = False : ShowInTaskbar = False : ControlBox = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(560, 214)
        BackColor = Color.White

        lblStep.SetBounds(20, 16, 520, 24) : lblStep.Font = New Font("Microsoft JhengHei UI", 11.0F, FontStyle.Bold) : lblStep.Text = "準備中…"
        lblItem.SetBounds(20, 44, 520, 22) : lblItem.ForeColor = Color.DimGray : lblItem.AutoEllipsis = True
        barItem.SetBounds(20, 70, 520, 10) : barItem.Maximum = 1000
        barAll.SetBounds(20, 96, 520, 22) : barAll.Maximum = 1000
        lblStats.SetBounds(20, 126, 520, 44) : lblStats.ForeColor = Color.FromArgb(60, 66, 76)
        butCancel.SetBounds(432, 172, 108, 30) : butCancel.Text = "取消"
        butCancel.Font = New Font("華康細圓體", 12.0F) : butCancel.ForeColor = Color.Black
        Controls.AddRange({lblStep, lblItem, barItem, barAll, lblStats, butCancel})

        AddHandler butCancel.Click, Sub() AskCancel()
        AddHandler m_tmrShow.Tick, Sub() ShowLatest()
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        m_lpClock.Start()
        m_tmrShow.Start()
        Dim token As CancellationToken = m_lpCancel.Token
        Dim report As Action(Of ProgressState) = Sub(s)
                                                     SyncLock m_lpLock
                                                         m_lpLatest = s.Clone()
                                                     End SyncLock
                                                 End Sub
        Task.Run(Sub() m_lpJob(report, token), token).ContinueWith(
            Sub(t)
                If IsDisposed OrElse Not IsHandleCreated Then Return
                BeginInvoke(Sub() Ended(t))
            End Sub)
    End Sub

    Private Sub Ended(ByVal t As Task)
        m_bolEnded = True
        m_tmrShow.Stop()
        ShowLatest()
        If t.IsCanceled OrElse m_lpCancel.IsCancellationRequested Then
            Result = Outcome.Cancelled
        ElseIf t.IsFaulted Then
            Dim ex As Exception = t.Exception.GetBaseException()
            If TypeOf ex Is OperationCanceledException Then
                Result = Outcome.Cancelled
            Else
                Result = Outcome.Failed
                [Error] = ex
            End If
        Else
            Result = Outcome.Done
        End If
        Close()
    End Sub

    Private Sub AskCancel()
        If m_lpCancel.IsCancellationRequested Then Return
        m_lpCancel.Cancel()
        butCancel.Enabled = False
        lblStep.Text = "取消中…（做完目前這個檔案就停止）"
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If Not m_bolEnded Then
            e.Cancel = True   ' the job still runs: cancel it, the window closes when it has stopped
            AskCancel()
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            AskCancel()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub ShowLatest()
        Dim s As ProgressState
        SyncLock m_lpLock
            s = m_lpLatest
        End SyncLock
        If s Is Nothing Then Return
        If Not m_lpCancel.IsCancellationRequested AndAlso s.Step <> "" Then lblStep.Text = s.Step
        lblItem.Text = s.Item
        barItem.Visible = s.ItemTotal > 0
        If s.ItemTotal > 0 Then barItem.Value = Permille(s.ItemDone, s.ItemTotal)
        barAll.Value = If(s.BytesTotal > 0, Permille(s.BytesDone, s.BytesTotal), Permille(s.Done, s.Total))

        Dim parts As New List(Of String)
        If s.Total > 0 Then parts.Add($"{s.Done:#,0} / {s.Total:#,0} 個檔案")
        If s.BytesTotal > 0 Then parts.Add(SizeText(s.BytesDone) & " / " & SizeText(s.BytesTotal))
        Dim seconds As Double = m_lpClock.Elapsed.TotalSeconds
        Dim line2 As String = "經過 " & TimeText(seconds)
        If s.BytesTotal > 0 AndAlso s.BytesDone > 0 AndAlso seconds >= 2 Then
            Dim speed As Double = s.BytesDone / seconds
            line2 &= " · " & SizeText(CLng(speed)) & "/秒"
            If speed > 0 Then line2 &= " · 剩下約 " & TimeText((s.BytesTotal - s.BytesDone) / speed)
        End If
        lblStats.Text = String.Join(" · ", parts) & vbCrLf & line2
    End Sub

    Private Shared Function Permille(ByVal done As Long, ByVal total As Long) As Integer
        If total <= 0 Then Return 0
        Return CInt(Math.Max(0, Math.Min(1000, done * 1000L \ total)))
    End Function

    Friend Shared Function SizeText(ByVal bytes As Long) As String
        If bytes >= 1L << 30 Then Return (bytes / (1L << 30)).ToString("0.0") & " GB"
        If bytes >= 1L << 20 Then Return (bytes / (1L << 20)).ToString("0.0") & " MB"
        If bytes >= 1L << 10 Then Return (bytes / (1L << 10)).ToString("0") & " KB"
        Return bytes & " B"
    End Function

    Private Shared Function TimeText(ByVal seconds As Double) As String
        If Double.IsNaN(seconds) OrElse Double.IsInfinity(seconds) Then Return "—"
        Dim t As TimeSpan = TimeSpan.FromSeconds(Math.Max(0, seconds))
        If t.TotalHours >= 1 Then Return $"{CInt(Math.Floor(t.TotalHours))} 小時 {t.Minutes} 分"
        If t.TotalMinutes >= 1 Then Return $"{t.Minutes} 分 {t.Seconds} 秒"
        Return $"{t.Seconds} 秒"
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            m_tmrShow.Dispose()
            m_lpCancel.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
