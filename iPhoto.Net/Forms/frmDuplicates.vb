' 尋找重複照片 (new in the .NET port; PhotoLib Duplicates): searches the albums on a worker thread,
' lists the groups, and shows each group's photos side by side with what tells them apart (size,
' picture size, rating, date, album). The best one is kept, the others ticked; 刪除勾選的 sends the
' ticked ones to the Recycle Bin with their side files and records (PhotoFiles.DeletePhoto).
' Opened from 設定 › 維護. Made in code (no designer file).
Friend Class frmDuplicates
    Inherits Form

    Private ReadOnly chkSimilar As New CheckBox
    Private ReadOnly butFind As New Aqua.FlashButton
    Private ReadOnly lblStatus As New Label
    Private ReadOnly lstGroups As New ListBox
    Private ReadOnly pnlFiles As New FlowLayoutPanel
    Private ReadOnly butDelete As New Aqua.FlashButton
    Private ReadOnly butSkip As New Aqua.FlashButton
    Private ReadOnly butClose As New Aqua.FlashButton
    Private m_lpCancel As Threading.CancellationTokenSource
    Private m_intDeleted As Integer

    Public Sub New()
        Text = "尋找重複照片"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.Sizable
        MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(1000, 640)
        MinimumSize = New Size(820, 520)
        BackColor = Color.White

        chkSimilar.SetBounds(16, 14, 520, 26)
        chkSimilar.Text = "也找非常相似的照片（連拍、另存成不同大小的同一張）"
        butFind.SetBounds(560, 10, 140, 34) : butFind.Text = "開始尋找"
        lblStatus.SetBounds(16, 50, 968, 24)
        lstGroups.SetBounds(16, 80, 300, 494)
        lstGroups.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom
        lstGroups.IntegralHeight = False
        pnlFiles.SetBounds(328, 80, 656, 494)
        pnlFiles.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right
        pnlFiles.AutoScroll = True
        pnlFiles.BackColor = Color.FromArgb(246, 247, 249)
        butDelete.SetBounds(328, 588, 280, 36) : butDelete.Text = "刪除勾選的（移到資源回收筒）"
        butSkip.SetBounds(620, 588, 130, 36) : butSkip.Text = "略過這組"
        butClose.SetBounds(884, 588, 100, 36) : butClose.Text = "關閉"
        For Each b In {butDelete, butSkip}
            b.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Next
        butClose.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        For Each b In {butFind, butDelete, butSkip, butClose}
            b.Font = New Font("華康細圓體", 12.0F)
        Next
        Controls.AddRange({chkSimilar, butFind, lblStatus, lstGroups, pnlFiles, butDelete, butSkip, butClose})
        lblStatus.Text = "按「開始尋找」，找出相片庫裡重複的照片。"
        SetButtons()

        AddHandler butFind.Click, Sub() FindOrStop()
        AddHandler lstGroups.SelectedIndexChanged, Sub() ShowGroup()
        AddHandler butDelete.Click, Sub() DeleteTicked()
        AddHandler butSkip.Click, Sub() RemoveGroup()
        AddHandler butClose.Click, Sub() Close()
    End Sub

    ''' <summary>How many photos were deleted (the caller refreshes what it shows).</summary>
    Public ReadOnly Property DeletedCount As Integer
        Get
            Return m_intDeleted
        End Get
    End Property

    '==================================================================================================
    ' Searching
    '==================================================================================================
    Private Sub FindOrStop()
        If m_lpCancel IsNot Nothing Then
            m_lpCancel.Cancel()
            Return
        End If
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Dim similar As Boolean = chkSimilar.Checked
        m_lpCancel = New Threading.CancellationTokenSource()
        Dim token As Threading.CancellationToken = m_lpCancel.Token
        butFind.Text = "停止"
        chkSimilar.Enabled = False
        lstGroups.Items.Clear()
        pnlFiles.Controls.Clear()
        lblStatus.Text = "列出照片…"
        SetButtons()
        Threading.Tasks.Task.Run(Function() Duplicates.Find(roots, similar,
                                                             Sub(what, done, total)
                                                                 If IsHandleCreated AndAlso Not IsDisposed Then BeginInvoke(Sub() lblStatus.Text = $"{what}… {done:#,0} / {total:#,0}")
                                                             End Sub, token)).
            ContinueWith(Sub(t)
                             If IsDisposed OrElse Not IsHandleCreated Then Return
                             BeginInvoke(Sub() FindDone(If(t.IsFaulted, New List(Of Duplicates.DupGroup), t.Result), token.IsCancellationRequested))
                         End Sub)
    End Sub

    Private Sub FindDone(ByVal groups As List(Of Duplicates.DupGroup), ByVal stopped As Boolean)
        m_lpCancel = Nothing
        butFind.Text = "重新尋找"
        chkSimilar.Enabled = True
        lstGroups.BeginUpdate()
        For Each g In groups
            lstGroups.Items.Add(g)
        Next
        lstGroups.EndUpdate()
        Dim photos As Integer = groups.Sum(Function(g) g.Files.Count - 1)
        lblStatus.Text = If(stopped, "已停止。", "") & If(groups.Count = 0, "沒有找到重複的照片。",
                         $"找到 {groups.Count} 組，可以刪掉 {photos} 張（每組預設保留評價最高、畫面最大的一張）。")
        If groups.Count > 0 Then lstGroups.SelectedIndex = 0
        SetButtons()
    End Sub

    '==================================================================================================
    ' One group
    '==================================================================================================
    Private ReadOnly Property Current As Duplicates.DupGroup
        Get
            Return TryCast(lstGroups.SelectedItem, Duplicates.DupGroup)
        End Get
    End Property

    Private Sub ShowGroup()
        For Each c As Control In pnlFiles.Controls.Cast(Of Control)().ToList()
            c.Dispose()
        Next
        pnlFiles.Controls.Clear()
        Dim g As Duplicates.DupGroup = Current
        If g IsNot Nothing Then
            For Each f In g.Files
                pnlFiles.Controls.Add(FileCard(f, g.Exact))
            Next
        End If
        SetButtons()
    End Sub

    ''' <summary>A card: the picture, 刪除 tick, name, album, picture and file size, rating, date.</summary>
    Private Function FileCard(ByVal f As Duplicates.DupFile, ByVal exact As Boolean) As Control
        Dim card As New Panel With {.Size = New Size(200, 292), .Margin = New Padding(8), .BackColor = If(f.Keep, Color.FromArgb(232, 243, 234), Color.White), .Tag = f}
        Dim pic As New PictureBox With {.Bounds = New Rectangle(8, 8, 184, 138), .SizeMode = PictureBoxSizeMode.Zoom, .BackColor = Color.FromArgb(235, 237, 240)}
        pic.Image = Thumb(f.File, 184, 138)
        Dim chk As New CheckBox With {.Bounds = New Rectangle(8, 150, 184, 24), .Text = If(f.Keep, "刪除（建議保留）", "刪除"), .Checked = Not f.Keep}
        Dim folder As String = IO.Path.GetFileName(IO.Path.GetDirectoryName(f.File))
        Dim info As New Label With {
            .Bounds = New Rectangle(8, 176, 184, 90), .ForeColor = Color.FromArgb(60, 66, 76),
            .Text = IO.Path.GetFileName(f.File) & vbCrLf & "相本：" & folder & vbCrLf &
                    If(f.Width > 0, $"{f.Width}×{f.Height}，", "") & $"{f.Size / 1024.0:#,0} KB" & vbCrLf &
                    If(f.Rating > 0, New String("★"c, f.Rating) & "  ", "") & f.Time.ToString("yyyy/MM/dd")}
        Dim open As New LinkLabel With {.Bounds = New Rectangle(8, 266, 184, 22), .Text = "以檔案總管開啟"}
        AddHandler open.LinkClicked, Sub() frmMain_1280x1024.ShowInExplorer(f.File)
        card.Controls.AddRange({pic, chk, info, open})
        Return card
    End Function

    Private Shared Function Thumb(ByVal file As String, ByVal w As Integer, ByVal h As Integer) As Image
        Try
            Using fs As New IO.FileStream(file, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite), img As Image = Image.FromStream(fs, False, False)
                Dim k As Double = Math.Min(w / img.Width, h / img.Height)
                Dim bmp As New Bitmap(Math.Max(1, CInt(img.Width * k)), Math.Max(1, CInt(img.Height * k)))
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    g.DrawImage(img, 0, 0, bmp.Width, bmp.Height)
                End Using
                Return bmp
            End Using
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing   ' a video, or unreadable: the grey box stays
        End Try
    End Function

    Private Sub SetButtons()
        Dim busy As Boolean = m_lpCancel IsNot Nothing
        Dim has As Boolean = Current IsNot Nothing
        butDelete.Enabled = has AndAlso Not busy AndAlso Not g_lpConfig.ReadOnly
        butSkip.Enabled = has AndAlso Not busy
    End Sub

    Private Sub DeleteTicked()
        Dim g As Duplicates.DupGroup = Current
        If g Is Nothing Then Return
        Dim ticked As New List(Of Duplicates.DupFile)
        For Each card As Control In pnlFiles.Controls
            Dim chk As CheckBox = card.Controls.OfType(Of CheckBox)().FirstOrDefault()
            If chk IsNot Nothing AndAlso chk.Checked Then ticked.Add(CType(card.Tag, Duplicates.DupFile))
        Next
        If ticked.Count = 0 Then Return
        If ticked.Count = g.Files.Count Then
            If Not frmQueryMsgBox.ShowMessage("這一組的照片全部勾選了，確定一張都不留？", "尋找重複照片", defaultNo:=True) Then Return
        ElseIf Not frmQueryMsgBox.ShowMessage($"刪除勾選的 {ticked.Count} 張？會移到資源回收筒。" & vbCrLf &
                                              String.Join(vbCrLf, ticked.Take(3).Select(Function(x) IO.Path.GetFileName(x.File))), "尋找重複照片") Then
            Return
        End If
        ' the pictures on the cards hold no file; still let go of them first
        For Each card As Control In pnlFiles.Controls
            Dim pic As PictureBox = card.Controls.OfType(Of PictureBox)().FirstOrDefault()
            If pic IsNot Nothing Then pic.Image = Nothing
        Next
        Dim failed As New List(Of String)
        UseWaitCursor = True
        Try
            For Each f In ticked
                If PhotoFiles.DeletePhoto(f.File, permanent:=False) Then m_intDeleted += 1 Else failed.Add(f.File)
            Next
            g_lpFaces?.RequestOrganizeSoon()
        Finally
            UseWaitCursor = False
        End Try
        If failed.Count > 0 Then frmMsgBox.ShowCriticalMessage(failed.Count & " 張刪除失敗（可能正在使用中）" & vbCrLf & IO.Path.GetFileName(failed(0)), "尋找重複照片")
        lblStatus.Text = $"已刪除 {m_intDeleted} 張。"
        RemoveGroup()
    End Sub

    Private Sub RemoveGroup()
        Dim i As Integer = lstGroups.SelectedIndex
        If i < 0 Then Return
        lstGroups.Items.RemoveAt(i)
        If lstGroups.Items.Count > 0 Then lstGroups.SelectedIndex = Math.Min(i, lstGroups.Items.Count - 1) Else ShowGroup()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        m_lpCancel?.Cancel()
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
