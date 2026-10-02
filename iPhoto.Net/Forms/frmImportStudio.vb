Imports System.Threading
Imports System.Threading.Tasks

' 輸入照片 -- the new import window (new in the .NET port; the tool bar's 輸入 opens it unless 設定 ›
' 相片庫 chose the old one, frmImport, which is left exactly as ported). One window, three steps:
'   ① 選照片  pick a folder (or drop one); the photos are grouped by day, the ones already in the
'            library (same file) are left unchecked;                                   (this file)
'   ② 整理    the album (name, folder, date, place, where it goes, icon, remark) and each photo's own
'            title / people / place / keywords / remark, one or many at a time; the faces are found
'            in the background and matched against the people already known;  (frmImportStudio.Organize.vb)
'   ③ 匯入    copy, .Exif, PhotoIndex, faces on a worker thread with progress, pause and cancel.
'                                                                            (frmImportStudio.Progress.vb)
' The work itself is in PhotoLib (ImportJob / ImportSource / ImportFaceMatcher / ImportRunner).
' The controls are made here in code; frmMain reads ImportedJobs when the window is closed.
Friend Class frmImportStudio

    Private Const AppName As String = "iPhoto"   ' the registry key of LibUserInterface
    Private Const RecentKey As String = "Import Studio"

    Private m_items As New List(Of ImportItem)
    Private m_strSource As String = ""
    Private m_cache As ImportThumbCache
    Private m_library As ImportSource.LibraryIndex
    Private m_readCts As CancellationTokenSource
    Private m_intStep As Integer
    Private m_bolFacesPaused As Boolean
    Private ReadOnly m_imported As New List(Of ImportJob)
    Private m_intImportedCount As Integer

    ' shell
    Private stepBar As ImportStepBar
    Private pnlStep(2) As Panel
    Private WithEvents butCancel, butBack, butNext As Aqua.FlashButton
    Private lblFoot As Label

    ' step 1
    Private WithEvents tvSource As ImportFolderTree
    Private WithEvents lstQuick As ListBox
    Private WithEvents grid1 As ImportThumbGrid
    Private WithEvents butAll, butNone, butNewOnly As Aqua.FlashButton
    Private lblSource As Label
    Private picPreview As PictureBox
    Private lblPreview, lblBatch, lblDup, lblShared As Label
    Private WithEvents chkSplit As Aqua.CheckBox
    Private m_intPreviewVersion As Integer

    Public Sub New()
        InitializeComponent()
        BuildUI()
    End Sub

    ''' <summary>The albums made (in order); frmMain opens the last one.</summary>
    Public ReadOnly Property ImportedJobs As List(Of ImportJob)
        Get
            Return m_imported
        End Get
    End Property

    Public ReadOnly Property ImportedCount As Integer
        Get
            Return m_intImportedCount
        End Get
    End Property

    '==================================================================================================
    ' Layout
    '==================================================================================================
    Private Function NewLabel(ByVal parent As Control, ByVal text As String, ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer,
                              Optional ByVal size As Single = 0, Optional ByVal style As FontStyle = FontStyle.Regular, Optional ByVal color As Color = Nothing) As Label
        Dim l As New Label With {.AutoSize = False, .Text = text, .Location = New Point(x, y), .Size = New Size(w, h), .BackColor = Color.Transparent,
                                 .Font = New Font(Font.FontFamily, If(size > 0, size, Font.Size), style), .ForeColor = If(color.IsEmpty, Ink, color)}
        parent.Controls.Add(l)
        Return l
    End Function

    Private Function NewButton(ByVal parent As Control, ByVal text As String, ByVal x As Integer, ByVal y As Integer, ByVal w As Integer,
                               Optional ByVal primary As Boolean = False, Optional ByVal h As Integer = 27) As Aqua.FlashButton
        Dim b As New Aqua.FlashButton With {.AutoSize = False, .Text = text, .Location = New Point(x, y), .Size = New Size(w, h),
                                            .Font = New Font(Font.FontFamily, 11.0F), .Flash = primary,
                                            .Color = If(primary, Aqua.ColorConstants.Blue, Aqua.ColorConstants.Metal)}
        parent.Controls.Add(b)
        Return b
    End Function

    ''' <summary>A white box with a thin border (a panel of the Aqua look).</summary>
    Private Function NewBox(ByVal parent As Control, ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As Panel
        Dim p As New Panel With {.Location = New Point(x, y), .Size = New Size(w, h), .BackColor = Color.White}
        AddHandler p.Paint, Sub(s, e)
                                Using pen As New Pen(Line)
                                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1)
                                End Using
                            End Sub
        parent.Controls.Add(p)
        Return p
    End Function

    Private Function NewCheck(ByVal parent As Control, ByVal text As String, ByVal x As Integer, ByVal y As Integer) As Aqua.CheckBox
        ' the pictures of frmImport's check box (its resources are only read)
        Dim res As New ComponentModel.ComponentResourceManager(GetType(frmImport))
        Dim c As New Aqua.CheckBox With {
            .TextValue = text, .TextGap = 6, .Font = New Font(Font.FontFamily, 11.0F), .ForeColor = Ink, .BackColor = Color.Transparent,
            .Location = New Point(x, y),
            .ImageChecked = CType(res.GetObject("chkAutoInfo.ImageChecked"), Image), .ImageUnChecked = CType(res.GetObject("chkAutoInfo.ImageUnChecked"), Image),
            .ImageCheckDisabled = CType(res.GetObject("chkAutoInfo.ImageCheckDisabled"), Image), .ImageUnCheckDisabled = CType(res.GetObject("chkAutoInfo.ImageUnCheckDisabled"), Image)}
        parent.Controls.Add(c)
        Return c
    End Function

    Private Sub BuildUI()
        Try
            Image = CType(New ComponentModel.ComponentResourceManager(GetType(frmImport)).GetObject("$this.Image"), Image)
            SizeMode = Aqua.ImageSizeMode.Appose
        Catch ex As Exception When TypeOf ex Is Resources.MissingManifestResourceException OrElse TypeOf ex Is InvalidCastException
        End Try
        SuspendLayout()
        stepBar = New ImportStepBar With {.Location = New Point(0, 24), .Size = New Size(ClientSize.Width, 44), .Font = New Font(Font.FontFamily, 12.0F)}
        Controls.Add(stepBar)
        For i = 0 To 2
            pnlStep(i) = New Panel With {.Location = New Point(0, 68), .Size = New Size(ClientSize.Width, 696), .BackColor = Ground, .Visible = (i = 0)}
            Controls.Add(pnlStep(i))
        Next

        Dim foot As New Panel With {.Location = New Point(0, 764), .Size = New Size(ClientSize.Width, 48), .BackColor = Color.FromArgb(222, 226, 232)}
        AddHandler foot.Paint, Sub(s, e)
                                   Using p As New Pen(Color.FromArgb(181, 189, 200))
                                       e.Graphics.DrawLine(p, 0, 0, foot.Width, 0)
                                   End Using
                               End Sub
        Controls.Add(foot)
        butCancel = NewButton(foot, "取消", 14, 11, 110)
        lblFoot = NewLabel(foot, "", 140, 14, 740, 22, 11.0F, color:=Muted)
        lblFoot.TextAlign = ContentAlignment.MiddleRight
        butBack = NewButton(foot, "‹ 上一步", 896, 11, 120)
        butNext = NewButton(foot, "下一步：整理 ›", 1026, 11, 160, primary:=True)

        BuildStep1(pnlStep(0))
        BuildStep2(pnlStep(1))   ' frmImportStudio.Organize.vb
        BuildStep3(pnlStep(2))   ' frmImportStudio.Progress.vb
        ResumeLayout(False)
        AllowDrop = True
    End Sub

    Private Sub BuildStep1(ByVal p As Panel)
        ' sources
        NewLabel(p, "來源", 12, 8, 240, 22, 11.0F, FontStyle.Bold, Muted)
        Dim treeBox As Panel = NewBox(p, 10, 32, 250, 440)
        tvSource = New ImportFolderTree With {.Location = New Point(1, 1), .Size = New Size(248, 438), .Font = New Font(Font.FontFamily, 11.0F)}
        treeBox.Controls.Add(tvSource)
        NewLabel(p, "相機記憶卡與最近用過", 12, 480, 240, 22, 11.0F, FontStyle.Bold, Muted)
        Dim quickBox As Panel = NewBox(p, 10, 504, 250, 120)
        lstQuick = New ListBox With {.Location = New Point(1, 1), .Size = New Size(248, 118), .BorderStyle = BorderStyle.None,
                                     .Font = New Font(Font.FontFamily, 10.5F), .IntegralHeight = False, .HorizontalScrollbar = True}
        quickBox.Controls.Add(lstQuick)
        Dim drop As Label = NewLabel(p, "也可以把資料夾或照片" & vbCrLf & "直接拖到這個視窗", 10, 632, 250, 54, 10.5F, color:=Muted)
        drop.TextAlign = ContentAlignment.MiddleCenter
        AddHandler drop.Paint, Sub(s, e)
                                   Using pen As New Pen(Color.FromArgb(174, 184, 197), 2) With {.DashStyle = Drawing2D.DashStyle.Dash}
                                       e.Graphics.DrawRectangle(pen, 1, 1, drop.Width - 3, drop.Height - 3)
                                   End Using
                               End Sub

        ' thumbnails
        butAll = NewButton(p, "全選", 272, 8, 70, h:=25)
        butNone = NewButton(p, "全不選", 348, 8, 80, h:=25)
        butNewOnly = NewButton(p, "只選新照片", 434, 8, 110, h:=25)
        lblSource = NewLabel(p, "", 552, 10, 374, 22, 10.5F, color:=Muted)
        lblSource.TextAlign = ContentAlignment.MiddleRight
        lblSource.AutoEllipsis = True
        Dim gridBox As Panel = NewBox(p, 270, 38, 658, 648)
        grid1 = New ImportThumbGrid With {.Location = New Point(1, 1), .Size = New Size(656, 646), .Font = New Font(Font.FontFamily, 11.0F),
                                          .CellSize = New Size(144, 108), .ShowChecks = True, .PendingText = "正在讀取拍攝日期…"}
        gridBox.Controls.Add(grid1)

        ' this batch
        Dim prevBox As Panel = NewBox(p, 938, 8, 252, 250)
        picPreview = New PictureBox With {.Location = New Point(1, 1), .Size = New Size(250, 188), .SizeMode = PictureBoxSizeMode.Zoom, .BackColor = Color.FromArgb(226, 230, 235)}
        prevBox.Controls.Add(picPreview)
        lblPreview = NewLabel(prevBox, "", 8, 192, 238, 54, 10.0F, color:=Muted)

        NewLabel(p, "這一批", 940, 268, 240, 22, 11.0F, FontStyle.Bold, Muted)
        Dim batchBox As Panel = NewBox(p, 938, 292, 252, 118)
        lblBatch = NewLabel(batchBox, "還沒有選資料夾", 10, 8, 234, 104, 10.5F)

        lblDup = NewLabel(p, "", 940, 418, 250, 76, 10.5F, color:=Color.FromArgb(138, 90, 16))
        lblShared = NewLabel(p, "", 940, 498, 250, 96, 10.5F, color:=Color.FromArgb(160, 50, 40))
        chkSplit = NewCheck(p, "每天分成一本相簿", 940, 604)
        NewLabel(p, "不勾：這一批放進同一本相簿（和以前一樣）", 940, 634, 250, 44, 9.5F, color:=Muted)
        grid1.Cache = m_cache
    End Sub

    '==================================================================================================
    ' Open / close
    '==================================================================================================
    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_cache = New ImportThumbCache
        grid1.Cache = m_cache
        grid2.Cache = m_cache
        ' the background face scan waits while the window is open: the engine and the database are ours
        If g_lpFaces IsNot Nothing AndAlso g_lpFaces.IsRunning AndAlso Not g_lpFaces.IsPaused Then
            g_lpFaces.Pause()
            m_bolFacesPaused = True
        End If
        LoadStep2Data()   ' frmImportStudio.Organize.vb: people, albums
        tvSource.LoadRoots()
        FillQuickList()
        BuildLibraryIndex()
        ShowStep(0)

        Dim start As String = ImportFolderTree.CameraFolders().FirstOrDefault()
        If start Is Nothing Then start = RecentSources().FirstOrDefault(Function(f) IO.Directory.Exists(f))
        If start IsNot Nothing Then
            tvSource.SelectPath(start)
            If Not String.Equals(m_strSource, start, StringComparison.OrdinalIgnoreCase) Then LoadSource(start)
        End If
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If RunnerBusy() Then
            ' an import is running: ask it to stop; the window closes once it has (Progress.vb)
            e.Cancel = True
            If frmQueryMsgBox.ShowMessage("匯入還沒完成，要停止嗎？" & vbCrLf & "已經匯入的照片會留在相簿裡。", "停止匯入") Then StopRunnerAndClose()
            Return
        End If
        m_readCts?.Cancel()
        StopFaceAnalysis()
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If picCurrent IsNot Nothing Then picCurrent.Image = Nothing   ' the cache's bitmap
        m_cache?.Dispose()
        picPreview.Image?.Dispose()
        If m_bolFacesPaused AndAlso g_lpFaces IsNot Nothing Then g_lpFaces.Resume()
    End Sub

    Private Sub butCancel_Click(sender As Object, e As EventArgs) Handles butCancel.Click
        If m_intStep = 2 AndAlso RunnerBusy() Then
            CancelRunner()
        Else
            Close()
        End If
    End Sub

    Private Sub butBack_Click(sender As Object, e As EventArgs) Handles butBack.Click
        If m_intStep = 1 Then
            CommitInspector()
            ShowStep(0)
        ElseIf m_intStep = 2 Then
            PauseOrResumeRunner()
        End If
    End Sub

    Private Sub butNext_Click(sender As Object, e As EventArgs) Handles butNext.Click
        Select Case m_intStep
            Case 0
                If m_items.Where(Function(i) i.Include).Count() = 0 Then
                    frmMsgBox.ShowCriticalMessage("還沒有勾選要匯入的照片", "")
                    Return
                End If
                EnterStep2()
            Case 1
                StartImport()        ' Progress.vb
            Case 2
                If RunnerBusy() Then Return
                Close()
        End Select
    End Sub

    Private Sub ShowStep(ByVal i As Integer)
        m_intStep = i
        stepBar.Current = i
        For k = 0 To 2
            pnlStep(k).Visible = (k = i)
        Next
        Select Case i
            Case 0
                butBack.Visible = False
                butNext.Text = "下一步：整理 ›"
                butCancel.Text = "取消"
                UpdateStep1Info()
                grid1.Focus()
            Case 1
                butBack.Visible = True
                butBack.Text = "‹ 上一步"
                butCancel.Text = "取消"
                UpdateStep2Foot()
            Case 2
                butBack.Visible = True
                butBack.Text = "暫停"
                butNext.Text = "完成"
                butCancel.Text = "停止匯入"
        End Select
    End Sub

    '==================================================================================================
    ' Sources
    '==================================================================================================
    Private Shared Function RecentSources() As List(Of String)
        Return GetSetting(AppName, RecentKey, "Recent", "").Split("|"c).Where(Function(s) s.Trim() <> "").ToList()
    End Function

    Private Sub SaveRecentSource(ByVal strFolder As String)
        Dim list = RecentSources()
        list.RemoveAll(Function(s) String.Equals(s, strFolder, StringComparison.OrdinalIgnoreCase))
        list.Insert(0, strFolder)
        SaveSetting(AppName, RecentKey, "Recent", String.Join("|", list.Take(6)))
    End Sub

    Private Sub FillQuickList()
        lstQuick.Items.Clear()
        For Each f In ImportFolderTree.CameraFolders()
            lstQuick.Items.Add("相機：" & f)
        Next
        For Each f In RecentSources().Where(Function(x) IO.Directory.Exists(x))
            lstQuick.Items.Add(f)
        Next
    End Sub

    Private Sub lstQuick_Click(sender As Object, e As EventArgs) Handles lstQuick.Click
        If lstQuick.SelectedItem Is Nothing Then Return
        Dim f As String = CStr(lstQuick.SelectedItem)
        If f.StartsWith("相機：") Then f = f.Substring(3)
        If Not IO.Directory.Exists(f) Then Return
        tvSource.SelectPath(f)
        If Not String.Equals(m_strSource, f, StringComparison.OrdinalIgnoreCase) Then LoadSource(f)
    End Sub

    Private Sub tvSource_PathSelected(strPath As String) Handles tvSource.PathSelected
        If String.Equals(m_strSource, strPath, StringComparison.OrdinalIgnoreCase) Then Return
        LoadSource(strPath)
    End Sub

    Private Sub Form_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        If m_intStep = 0 AndAlso e.Data.GetDataPresent(DataFormats.FileDrop) Then e.Effect = DragDropEffects.Copy
    End Sub

    Private Sub Form_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
        Dim paths() As String = TryCast(e.Data.GetData(DataFormats.FileDrop), String())
        If paths Is Nothing OrElse paths.Length = 0 Then Return
        If paths.Length = 1 AndAlso IO.Directory.Exists(paths(0)) Then
            tvSource.SelectPath(paths(0))
            If Not String.Equals(m_strSource, paths(0), StringComparison.OrdinalIgnoreCase) Then LoadSource(paths(0))
            Return
        End If
        ' files (and folders) dropped: their photos, from wherever they are
        Dim patterns As String() = GetPhotoPatterns().Split(";"c).Select(Function(p) p.TrimStart("*"c).ToUpperInvariant()).ToArray()
        Dim files As New List(Of String)
        For Each p In paths
            If IO.Directory.Exists(p) Then
                files.AddRange(ImportSource.ListFiles(p))
            ElseIf IO.File.Exists(p) AndAlso patterns.Contains(IO.Path.GetExtension(p).ToUpperInvariant()) Then
                files.Add(p)
            End If
        Next
        If files.Count = 0 Then Return
        LoadFiles(files, "拖曳進來的 " & files.Count & " 個檔案")
    End Sub

    Private Sub LoadSource(ByVal strFolder As String)
        m_strSource = strFolder
        LoadFiles(ImportSource.ListFiles(strFolder), strFolder)
        If m_items.Count > 0 Then
            SaveRecentSource(strFolder)
        End If
    End Sub

    ''' <summary>A new batch: the files are shown at once, their dates / GPS are read in the background
    ''' and then they are grouped by day and compared with the library.</summary>
    Private Sub LoadFiles(ByVal files As List(Of String), ByVal strFrom As String)
        m_readCts?.Cancel()
        StopFaceAnalysis()
        m_items = files.Select(Function(f) New ImportItem With {.SourceFile = f}).ToList()
        m_bolAlbumEdited = False   ' Organize.vb: the album's defaults come from the new photos
        lblSource.Text = strFrom
        vb6ToolTip.SetToolTip(lblSource, strFrom)
        grid1.SetItems(m_items)
        If m_items.Count > 0 Then grid1.SelectOnly(m_items(0)) Else ShowPreview(Nothing)
        UpdateStep1Info()

        Dim cts As New CancellationTokenSource
        m_readCts = cts
        Dim items As List(Of ImportItem) = m_items
        Task.Run(Sub()
                     Dim fs As New Carbon.FileSystem
                     Dim n As Integer = 0
                     For Each it In items
                         If cts.IsCancellationRequested Then Return
                         ImportSource.ReadInfo(it, fs)
                         n += 1
                         If n Mod 25 = 0 Then Post(Sub() If Not cts.IsCancellationRequested Then UpdateStep1Info())
                     Next
                     Post(Sub()
                              If cts.IsCancellationRequested Then Return
                              MarkDuplicates()
                              grid1.RefreshLayout()
                              UpdateStep1Info()
                          End Sub)
                 End Sub)
    End Sub

    Private Sub Post(ByVal a As Action)
        Try
            If IsHandleCreated AndAlso Not IsDisposed Then BeginInvoke(a)
        Catch ex As InvalidOperationException
        End Try
    End Sub

    ''' <summary>Lists the library's photos by size once, in the background (a few seconds).</summary>
    Private Sub BuildLibraryIndex()
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Task.Run(Sub()
                     Dim idx As ImportSource.LibraryIndex = ImportSource.LibraryIndex.Build(roots)
                     Post(Sub()
                              m_library = idx
                              MarkDuplicates()
                              grid1.Invalidate()
                              UpdateStep1Info()
                          End Sub)
                 End Sub)
    End Sub

    ''' <summary>The photos already in the library: marked, and unchecked the first time they are found.</summary>
    Private Sub MarkDuplicates()
        If m_library Is Nothing OrElse m_items.Any(Function(i) Not i.InfoRead) Then Return
        Dim items As List(Of ImportItem) = m_items
        Dim idx As ImportSource.LibraryIndex = m_library
        Dim todo = items.Where(Function(i) i.DuplicateOf = "" AndAlso i.Size > 0).ToList()
        Task.Run(Sub()
                     Dim found As New List(Of (Item As ImportItem, Dup As String))
                     SyncLock idx
                         For Each it In todo
                             Dim d As String = idx.Find(it.SourceFile, it.Size)
                             If d <> "" Then found.Add((it, d))
                         Next
                     End SyncLock
                     If found.Count = 0 Then Return
                     Post(Sub()
                              If items IsNot m_items Then Return
                              For Each f In found
                                  f.Item.DuplicateOf = f.Dup
                                  f.Item.Include = False
                              Next
                              grid1.Invalidate()
                              UpdateStep1Info()
                          End Sub)
                 End Sub)
    End Sub

    '==================================================================================================
    ' Step 1: picking
    '==================================================================================================
    Private Sub butAll_Click(sender As Object, e As EventArgs) Handles butAll.Click
        For Each it In m_items
            it.Include = True
        Next
        grid1.Invalidate()
        UpdateStep1Info()
    End Sub

    Private Sub butNone_Click(sender As Object, e As EventArgs) Handles butNone.Click
        For Each it In m_items
            it.Include = False
        Next
        grid1.Invalidate()
        UpdateStep1Info()
    End Sub

    Private Sub butNewOnly_Click(sender As Object, e As EventArgs) Handles butNewOnly.Click
        For Each it In m_items
            it.Include = it.DuplicateOf = ""
        Next
        grid1.Invalidate()
        UpdateStep1Info()
    End Sub

    Private Sub grid1_IncludeChanged(sender As Object, e As EventArgs) Handles grid1.IncludeChanged
        UpdateStep1Info()
    End Sub

    Private Sub grid1_SelectionChanged(sender As Object, e As EventArgs) Handles grid1.SelectionChanged
        ShowPreview(grid1.FocusItem)
    End Sub

    Private Sub chkSplit_CheckedChanged(sender As Object, e As EventArgs) Handles chkSplit.CheckedChanged
        UpdateStep1Info()
    End Sub

    Private Sub ShowPreview(ByVal it As ImportItem)
        m_intPreviewVersion += 1
        Dim version As Integer = m_intPreviewVersion
        Dim old As Image = picPreview.Image
        picPreview.Image = Nothing
        old?.Dispose()
        If it Is Nothing Then
            lblPreview.Text = ""
            Return
        End If
        Dim len As Double, unit As String = ""
        CalcFileLength(it.Size, len, unit)
        Dim d As Date? = it.ShotDate
        lblPreview.Text = IO.Path.GetFileName(it.SourceFile) & vbCrLf &
                          If(d.HasValue, d.Value.ToString("yyyy/MM/dd HH:mm"), "") & If(it.Size > 0, " · " & len & " " & unit, "") &
                          If(it.GpsPlace <> "", vbCrLf & it.GpsPlace, "")
        Dim file As String = it.SourceFile
        Task.Run(Sub()
                     Dim bmp As Bitmap = Quartz.Thumbnail.ShellThumbnail(file, 250, 188)
                     Post(Sub()
                              If version <> m_intPreviewVersion OrElse IsDisposed Then
                                  bmp?.Dispose()
                              Else
                                  picPreview.Image = bmp
                              End If
                          End Sub)
                 End Sub)
    End Sub

    Private Sub UpdateStep1Info()
        If lblBatch Is Nothing Then Return
        Dim inc = m_items.Where(Function(i) i.Include).ToList()
        Dim reading As Integer = m_items.Where(Function(i) Not i.InfoRead).Count()
        If m_items.Count = 0 Then
            lblBatch.Text = If(m_strSource = "", "在左邊選一個資料夾", "這個資料夾沒有照片或影片")
        Else
            Dim bytes As Long = inc.Sum(Function(i) i.Size)
            Dim len As Double, unit As String = ""
            CalcFileLength(bytes, len, unit)
            Dim days = inc.Where(Function(i) i.ShotDate.HasValue).Select(Function(i) i.ShotDate.Value.Date).Distinct().OrderBy(Function(d) d).ToList()
            Dim places = inc.Where(Function(i) i.GpsPlace <> "").GroupBy(Function(i) i.GpsPlace).OrderByDescending(Function(g) g.Count()).Select(Function(g) g.Key).Take(2).ToList()
            Dim gpsCount As Integer = inc.Where(Function(i) i.Gps <> "").Count()
            Dim videos As Integer = inc.Where(Function(i) i.IsVideo).Count()
            Dim sb As New Text.StringBuilder
            sb.AppendLine("已選 " & inc.Count & " / " & m_items.Count & " 張" & If(bytes > 0, " · " & len & " " & unit, ""))
            If reading > 0 Then
                sb.AppendLine("正在讀取拍攝日期… 還有 " & reading & " 張")
            Else
                If days.Count > 0 Then sb.AppendLine("拍攝　" & If(days.Count = 1, days(0).ToString("yyyy/MM/dd"), days.First().ToString("MM/dd") & " – " & days.Last().ToString("MM/dd") & "（" & days.Count & " 天）"))
                If places.Count > 0 Then sb.AppendLine("地點　" & String.Join("、", places) & "（GPS " & gpsCount & " 張）")
                If videos > 0 Then sb.AppendLine("影片　" & videos & " 部")
            End If
            lblBatch.Text = sb.ToString().TrimEnd()
        End If

        Dim dups = m_items.Where(Function(i) i.DuplicateOf <> "").ToList()
        If dups.Count > 0 Then
            Dim where As String = IO.Path.GetFileName(IO.Path.GetDirectoryName(dups(0).DuplicateOf))
            lblDup.Text = "已經在相片庫：" & dups.Count & " 張和相簿裡的照片相同（例如在「" & where & "」），" &
                          If(dups.All(Function(d) Not d.Include), "已取消勾選。", "有 " & dups.Where(Function(d) d.Include).Count() & " 張仍勾選著。")
        ElseIf m_library Is Nothing AndAlso m_items.Count > 0 Then
            lblDup.Text = "正在比對相片庫裡已有的照片…"
        Else
            lblDup.Text = ""
        End If

        ' IMG_1.JPG + IMG_1.MOV would share IMG_1.Exif
        Dim probe As New ImportJob
        probe.Items.AddRange(m_items)
        probe.AssignFileNames()
        Dim shared_ = inc.Where(Function(i) i.SharesExifWith <> "").ToList()
        If shared_.Count > 0 Then
            lblShared.Text = "注意：有 " & shared_.Count & " 個檔案和另一個檔案同名（例如 " & shared_(0).FileName & " 和 " & shared_(0).SharesExifWith &
                             "），它們會共用同一個 .Exif，只保留照片的資料。縮圖上標「同名」。"
        Else
            lblShared.Text = ""
        End If

        Dim split As String = ""
        If chkSplit.Checked Then
            Dim n As Integer = inc.Where(Function(i) i.ShotDate.HasValue).Select(Function(i) i.ShotDate.Value.Date).Distinct().Count()
            split = "，每天一本（" & Math.Max(1, n) & " 本相簿）"
        ElseIf inc.Count > 0 Then
            split = "，放進同一本相簿"
        End If
        lblFoot.Text = If(inc.Count = 0, "勾選要匯入的照片", "已選 " & inc.Count & " 張" & split)
        grid1.Invalidate()
    End Sub

    ''' <summary>Keys that work anywhere in the window (not while typing).</summary>
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape AndAlso m_intStep <> 2 Then
            If TypeOf ActiveControl Is ComboBox Then Return MyBase.ProcessCmdKey(msg, keyData)
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
