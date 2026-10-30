' 整理清單 (new in the .NET port; was 尋找重複照片): looks through the whole library or the album shown
' for photos probably not worth keeping, on a worker thread --
'   重複 / 相似   copies and near copies, bursts (PhotoLib Duplicates): the best of a group is kept, the
'                 others ticked;
'   模糊 / 太暗或過曝   (PhotoLib PhotoQuality) grouped by album, nothing ticked: the eye decides.
' The groups are listed; a group's photos show side by side with what tells them apart (size, picture
' size, rating, date, album, why it is listed). 刪除勾選的 sends the ticked ones to the Recycle Bin with
' their side files and records (PhotoFiles.DeletePhoto).
' Opened by the tool bar's 整理清單 (frmMain.Cleanup.vb) and 設定 › 維護. Made in code (no designer file).
Friend Class frmCleanup
    Inherits Form

    Private ReadOnly chkExact As New CheckBox
    Private ReadOnly chkSimilar As New CheckBox
    Private ReadOnly chkBlurry As New CheckBox
    Private ReadOnly chkExposure As New CheckBox
    Private ReadOnly optLibrary As New RadioButton
    Private ReadOnly optAlbum As New RadioButton
    Private ReadOnly butFind As New Aqua.ThinButton
    Private ReadOnly lblStatus As New Label
    Private ReadOnly lstGroups As New ListBox
    Private ReadOnly pnlFiles As New FlowLayoutPanel
    Private ReadOnly butDelete As New Aqua.ThinButton
    Private ReadOnly butSkip As New Aqua.ThinButton
    Private ReadOnly butClose As New Aqua.ThinButton
    Private ReadOnly m_strAlbum As String
    Private ReadOnly m_lpDeleted As New List(Of String)
    Private m_lpCancel As Threading.CancellationTokenSource

    ''' <param name="albumFolder">The album shown in the main window (a folder), offered as the range; Nothing: the library only.</param>
    Public Sub New(Optional ByVal albumFolder As String = Nothing, Optional ByVal albumName As String = Nothing)
        m_strAlbum = If(albumFolder IsNot Nothing AndAlso IO.Directory.Exists(albumFolder), albumFolder, Nothing)
        Text = "整理清單"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.Sizable
        MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(1000, 660)
        MinimumSize = New Size(860, 540)
        BackColor = Color.White

        chkExact.SetBounds(16, 12, 90, 26) : chkExact.Text = "重複" : chkExact.Checked = True
        chkSimilar.SetBounds(110, 12, 150, 26) : chkSimilar.Text = "相似（連拍）" : chkSimilar.Checked = True
        chkBlurry.SetBounds(264, 12, 80, 26) : chkBlurry.Text = "模糊" : chkBlurry.Checked = True
        chkExposure.SetBounds(348, 12, 140, 26) : chkExposure.Text = "太暗或過曝" : chkExposure.Checked = True
        optLibrary.SetBounds(16, 42, 140, 26) : optLibrary.Text = "整個相片庫"
        optAlbum.SetBounds(160, 42, 420, 26)
        optAlbum.Text = "目前的相簿" & If(String.IsNullOrEmpty(albumName), "", "（" & albumName & "）")
        optAlbum.Enabled = m_strAlbum IsNot Nothing
        If m_strAlbum IsNot Nothing Then optAlbum.Checked = True Else optLibrary.Checked = True
        butFind.SetBounds(600, 24, 140, 27) : butFind.Text = "開始整理"
        lblStatus.SetBounds(16, 76, 968, 24)
        lblStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lstGroups.SetBounds(16, 104, 300, 490)
        lstGroups.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom
        lstGroups.IntegralHeight = False
        pnlFiles.SetBounds(328, 104, 656, 490)
        pnlFiles.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right
        pnlFiles.AutoScroll = True
        pnlFiles.BackColor = Color.FromArgb(246, 247, 249)
        butDelete.SetBounds(328, 608, 280, 27) : butDelete.Text = "刪除勾選的（移到資源回收筒）"
        butSkip.SetBounds(620, 608, 130, 27) : butSkip.Text = "略過這組"
        butClose.SetBounds(884, 608, 100, 27) : butClose.Text = "關閉"
        For Each b In {butDelete, butSkip}
            b.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Next
        butClose.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        For Each b In {butFind, butDelete, butSkip, butClose}
            b.Font = New Font("華康細圓體", 12.0F)
            b.ForeColor = Color.Black
        Next
        Controls.AddRange({chkExact, chkSimilar, chkBlurry, chkExposure, optLibrary, optAlbum, butFind, lblStatus, lstGroups, pnlFiles, butDelete, butSkip, butClose})
        lblStatus.Text = "勾選要找的項目，按「開始整理」。找到的照片可以逐張決定刪除或保留。"
        SetButtons()

        AddHandler butFind.Click, Sub() FindOrStop()
        AddHandler lstGroups.SelectedIndexChanged, Sub() ShowGroup()
        AddHandler butDelete.Click, Sub() DeleteTicked()
        AddHandler butSkip.Click, Sub() RemoveGroup()
        AddHandler butClose.Click, Sub() Close()
    End Sub

    ''' <summary>How many photos were deleted.</summary>
    Public ReadOnly Property DeletedCount As Integer
        Get
            Return m_lpDeleted.Count
        End Get
    End Property

    ''' <summary>The photos deleted (the caller refreshes what it shows).</summary>
    Public ReadOnly Property DeletedFiles As IReadOnlyList(Of String)
        Get
            Return m_lpDeleted
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
        Dim exact As Boolean = chkExact.Checked, similar As Boolean = chkSimilar.Checked
        Dim wanted As New HashSet(Of PhotoQuality.Problem)
        If chkBlurry.Checked Then wanted.Add(PhotoQuality.Problem.Blurry)
        If chkExposure.Checked Then
            wanted.Add(PhotoQuality.Problem.Dark)
            wanted.Add(PhotoQuality.Problem.Bright)
        End If
        If Not exact AndAlso Not similar AndAlso wanted.Count = 0 Then
            lblStatus.Text = "請至少勾選一個項目。"
            Return
        End If
        Dim album As String = If(optAlbum.Checked, m_strAlbum, Nothing)
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        m_lpCancel = New Threading.CancellationTokenSource()
        Dim token As Threading.CancellationToken = m_lpCancel.Token
        butFind.Text = "停止"
        SetOptionsEnabled(False)
        lstGroups.Items.Clear()
        pnlFiles.Controls.Clear()
        lblStatus.Text = "列出照片…"
        SetButtons()
        Dim progress As Action(Of String, Integer, Integer) =
            Sub(what, done, total)
                If IsHandleCreated AndAlso Not IsDisposed Then BeginInvoke(Sub() lblStatus.Text = $"{what}… {done:#,0} / {total:#,0}")
            End Sub
        Threading.Tasks.Task.Run(Function()
                                     Dim files As List(Of String) = If(album IsNot Nothing, EditedPhotos.AlbumFiles(album).ToList(), Duplicates.AlbumPhotos(roots))
                                     Dim groups As New List(Of Duplicates.DupGroup)
                                     If exact OrElse similar Then groups.AddRange(Duplicates.FindIn(files, exact, similar, progress, token))
                                     If wanted.Count > 0 AndAlso Not token.IsCancellationRequested Then groups.AddRange(PhotoQuality.Find(files, wanted, progress, token))
                                     Return groups
                                 End Function).
            ContinueWith(Sub(t)
                             If IsDisposed OrElse Not IsHandleCreated Then Return
                             BeginInvoke(Sub() FindDone(If(t.IsFaulted, New List(Of Duplicates.DupGroup), t.Result), token.IsCancellationRequested))
                         End Sub)
    End Sub

    Private Sub SetOptionsEnabled(ByVal value As Boolean)
        For Each c As Control In {chkExact, chkSimilar, chkBlurry, chkExposure, optLibrary}
            c.Enabled = value
        Next
        optAlbum.Enabled = value AndAlso m_strAlbum IsNot Nothing
    End Sub

    Private Sub FindDone(ByVal groups As List(Of Duplicates.DupGroup), ByVal stopped As Boolean)
        m_lpCancel = Nothing
        butFind.Text = "重新整理"
        SetOptionsEnabled(True)
        lstGroups.BeginUpdate()
        For Each g In groups
            lstGroups.Items.Add(g)
        Next
        lstGroups.EndUpdate()
        Dim copies = groups.Where(Function(g) g.Title Is Nothing).ToList()
        Dim others = groups.Where(Function(g) g.Title IsNot Nothing).ToList()
        Dim parts As New List(Of String)
        If copies.Count > 0 Then parts.Add($"重複或相似 {copies.Count} 組（可以刪掉 {copies.Sum(Function(g) g.Files.Count - 1)} 張，每組預設保留評價最高、畫面最大的一張）")
        If others.Count > 0 Then parts.Add($"畫面有問題的 {others.Sum(Function(g) g.Files.Count)} 張（沒有預先勾選，請逐張決定）")
        lblStatus.Text = If(stopped, "已停止。", "") & If(groups.Count = 0, "沒有找到要整理的照片。", "找到" & String.Join("；", parts) & "。")
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
        Dim card As New Panel With {.Size = New Size(200, 310), .Margin = New Padding(8), .BackColor = If(f.Keep AndAlso f.Note Is Nothing, Color.FromArgb(232, 243, 234), Color.White), .Tag = f}
        Dim pic As New PictureBox With {.Bounds = New Rectangle(8, 8, 184, 138), .SizeMode = PictureBoxSizeMode.Zoom, .BackColor = Color.FromArgb(235, 237, 240)}
        pic.Image = Thumb(f.File, 184, 138)
        Dim chk As New CheckBox With {.Bounds = New Rectangle(8, 150, 184, 24), .Text = If(f.Keep AndAlso f.Note Is Nothing, "刪除（建議保留）", "刪除"), .Checked = Not f.Keep}
        Dim folder As String = IO.Path.GetFileName(IO.Path.GetDirectoryName(f.File))
        Dim info As New Label With {
            .Bounds = New Rectangle(8, 176, 184, 108), .ForeColor = Color.FromArgb(60, 66, 76),
            .Text = If(f.Note Is Nothing, "", f.Note & vbCrLf) & IO.Path.GetFileName(f.File) & vbCrLf & "相本：" & folder & vbCrLf &
                    If(f.Width > 0, $"{f.Width}×{f.Height}，", "") & $"{f.Size / 1024.0:#,0} KB" & vbCrLf &
                    If(f.Rating > 0, New String("★"c, f.Rating) & "  ", "") & f.Time.ToString("yyyy/MM/dd")}
        Dim open As New LinkLabel With {.Bounds = New Rectangle(8, 284, 184, 22), .Text = "以檔案總管開啟"}
        AddHandler open.LinkClicked, Sub() frmMain.ShowInExplorer(f.File)
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
        If ticked.Count = g.Files.Count AndAlso g.Title Is Nothing Then
            If Not frmQueryMsgBox.ShowMessage("這一組的照片全部勾選了，確定一張都不留？", "整理清單", defaultNo:=True) Then Return
        ElseIf Not frmQueryMsgBox.ShowMessage($"刪除勾選的 {ticked.Count} 張？會移到資源回收筒。" & vbCrLf &
                                              String.Join(vbCrLf, ticked.Take(3).Select(Function(x) IO.Path.GetFileName(x.File))), "整理清單") Then
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
                If PhotoFiles.DeletePhoto(f.File, permanent:=False) Then m_lpDeleted.Add(f.File) Else failed.Add(f.File)
            Next
            g_lpFaces?.RequestOrganizeSoon()
        Finally
            UseWaitCursor = False
        End Try
        If failed.Count > 0 Then frmMsgBox.ShowCriticalMessage(failed.Count & " 張刪除失敗（可能正在使用中）" & vbCrLf & IO.Path.GetFileName(failed(0)), "整理清單")
        lblStatus.Text = $"已刪除 {m_lpDeleted.Count} 張。"
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
