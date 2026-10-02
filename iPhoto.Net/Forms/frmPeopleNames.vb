' 全部人物名字 (new in the .NET port; PhotoLib NameCheck): every name in the people fields with how many
' photos have it, to rename one, merge several into one (rename to a name that is there already, or
' pick several and 改名／合併), or take a name out of the people fields. The .Exif files are backed up
' once before the first change (Maintenance.BackupExif); PhotoIndex follows (NameCheck.Unify /
' RemoveName); a face person with the old name is renamed / merged too, or hidden when its name is
' taken out. Opened from 設定 › 維護. Made in code (no designer file).
Friend Class frmPeopleNames
    Inherits Form

    Private ReadOnly lblStatus As New Label
    Private ReadOnly txtFilter As New TextBox
    Private ReadOnly lvNames As New ListView
    Private ReadOnly butRename As New Aqua.ThinButton
    Private ReadOnly butRemove As New Aqua.ThinButton
    Private ReadOnly butClose As New Aqua.ThinButton
    Private m_lpUses As Dictionary(Of String, NameCheck.NameUse)
    Private m_lpFaceNames As HashSet(Of String)
    Private m_bolBackedUp As Boolean
    Private m_intChanged As Integer
    Private m_intSortColumn As Integer = 1   ' most photos first

    Public Sub New()
        Text = "全部人物名字"
        Font = New Font("Microsoft JhengHei UI", 11.0F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(640, 600)
        BackColor = Color.White

        lblStatus.SetBounds(16, 12, 608, 26)
        Dim lblFilter As New Label With {.Text = "搜尋：", .AutoSize = True, .Location = New Point(16, 50)}
        txtFilter.SetBounds(72, 46, 200, 28)
        lvNames.SetBounds(16, 84, 420, 500)
        lvNames.View = View.Details
        lvNames.FullRowSelect = True
        lvNames.HideSelection = False
        lvNames.MultiSelect = True
        lvNames.Columns.Add("名字", 200)
        lvNames.Columns.Add("照片", 90, HorizontalAlignment.Right)
        lvNames.Columns.Add("面孔", 90, HorizontalAlignment.Center)
        butRename.SetBounds(452, 84, 172, 27) : butRename.Text = "改名／合併…"
        butRemove.SetBounds(452, 128, 172, 27) : butRemove.Text = "從人物欄移除…"
        butClose.SetBounds(452, 550, 172, 27) : butClose.Text = "關閉"
        For Each b In {butRename, butRemove, butClose}
            b.Font = New Font("華康細圓體", 13.0F)
            b.ForeColor = Color.Black
        Next
        Dim lblHelp As New Label With {
            .AutoSize = False, .Bounds = New Rectangle(452, 176, 172, 300), .ForeColor = Color.DimGray, .Font = New Font(Font.FontFamily, 10.0F),
            .Text = "按兩下名字可以改名。" & vbCrLf & vbCrLf &
                    "改成已經有的名字，兩個名字就會合併。" & vbCrLf & vbCrLf &
                    "選好幾個名字（Ctrl 或 Shift）再按「改名／合併」，會全部合併成同一個名字。" & vbCrLf & vbCrLf &
                    "「移除」只拿掉人物欄裡的這個名字，照片不會刪除。"}
        Controls.AddRange({lblStatus, lblFilter, txtFilter, lvNames, butRename, butRemove, lblHelp, butClose})
        SetButtons()

        AddHandler txtFilter.TextChanged, Sub() FillList()
        AddHandler lvNames.SelectedIndexChanged, Sub() SetButtons()
        AddHandler lvNames.DoubleClick, Sub() RenameSelected()
        AddHandler lvNames.ColumnClick, Sub(s, e)
                                            m_intSortColumn = e.Column
                                            FillList()
                                        End Sub
        AddHandler butRename.Click, Sub() RenameSelected()
        AddHandler butRemove.Click, Sub() RemoveSelected()
        AddHandler butClose.Click, Sub() Close()
    End Sub

    ''' <summary>How many .Exif files were changed.</summary>
    Public ReadOnly Property ChangedCount As Integer
        Get
            Return m_intChanged
        End Get
    End Property

    Private Function Roots() As List(Of String)
        Dim r As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            r.Add(g_lpConfig.AlbumPath(i))
        Next
        Return r
    End Function

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        lblStatus.Text = "讀取照片的人物欄…"
        Dim r As List(Of String) = Roots()
        UseWaitCursor = True
        Threading.Tasks.Task.Run(Function() NameCheck.Scan(r, Sub(done, total)
                                                                   If IsHandleCreated Then BeginInvoke(Sub() lblStatus.Text = $"讀取照片的人物欄… {done:#,0} / {total:#,0}")
                                                               End Sub)).
            ContinueWith(Sub(t)
                             If IsDisposed OrElse Not IsHandleCreated Then Return
                             BeginInvoke(Sub() ScanDone(t.Result))
                         End Sub)
    End Sub

    Private Sub ScanDone(ByVal uses As Dictionary(Of String, NameCheck.NameUse))
        UseWaitCursor = False
        m_lpUses = uses
        m_lpFaceNames = New HashSet(Of String)(If(g_lpFaces?.Catalog?.Persons.Select(Function(p) p.Name), Enumerable.Empty(Of String)()), StringComparer.CurrentCultureIgnoreCase)
        FillList()
        txtFilter.Focus()
    End Sub

    Private Sub FillList()
        If m_lpUses Is Nothing Then Return
        Dim f As String = txtFilter.Text.Trim()
        Dim names = m_lpUses.Values.Where(Function(u) u.Files.Count > 0 AndAlso (f = "" OrElse u.Name.Contains(f, StringComparison.CurrentCultureIgnoreCase)))
        Select Case m_intSortColumn
            Case 0 : names = names.OrderBy(Function(u) u.Name, StringComparer.Create(New Globalization.CultureInfo("zh-TW"), True))
            Case 2 : names = names.OrderByDescending(Function(u) m_lpFaceNames.Contains(u.Name)).ThenByDescending(Function(u) u.Files.Count)
            Case Else : names = names.OrderByDescending(Function(u) u.Files.Count).ThenBy(Function(u) u.Name)
        End Select
        Dim selected As New HashSet(Of String)(lvNames.SelectedItems.Cast(Of ListViewItem)().Select(Function(i) i.Text))
        lvNames.BeginUpdate()
        lvNames.Items.Clear()
        For Each u In names
            Dim it As New ListViewItem({u.Name, u.Files.Count.ToString("#,0"), If(m_lpFaceNames.Contains(u.Name), "✓", "")}) With {.Tag = u}
            it.Selected = selected.Contains(u.Name)
            lvNames.Items.Add(it)
        Next
        lvNames.EndUpdate()
        Dim total As Integer = m_lpUses.Values.Where(Function(u) u.Files.Count > 0).Count()
        lblStatus.Text = $"{total:#,0} 個名字" & If(f = "", "", $"，符合「{f}」的 {lvNames.Items.Count:#,0} 個") &
                         If(m_intChanged > 0, $"（已修改 {m_intChanged:#,0} 張照片）", "")
        SetButtons()
    End Sub

    Private ReadOnly Property Picked As List(Of NameCheck.NameUse)
        Get
            Return lvNames.SelectedItems.Cast(Of ListViewItem)().Select(Function(i) CType(i.Tag, NameCheck.NameUse)).ToList()
        End Get
    End Property

    Private Sub SetButtons()
        Dim n As Integer = lvNames.SelectedItems.Count
        Dim writable As Boolean = Not g_lpConfig.ReadOnly AndAlso m_lpUses IsNot Nothing
        butRename.Enabled = writable AndAlso n > 0
        butRename.Text = If(n > 1, "合併 " & n & " 個名字…", "改名／合併…")
        butRemove.Enabled = writable AndAlso n > 0
    End Sub

    ''' <summary>The .Exif files backed up once, before the first change; False when that failed.</summary>
    Private Function EnsureBackup() As Boolean
        If m_bolBackedUp Then Return True
        lblStatus.Text = "備份 .Exif 中…"
        lblStatus.Refresh()
        If Maintenance.BackupExif(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase), Roots()).Zip = "" Then
            frmMsgBox.ShowCriticalMessage(".Exif 備份失敗，這次不修改。", Text)
            Return False
        End If
        m_bolBackedUp = True
        Return True
    End Function

    Private Sub RenameSelected()
        Dim from As List(Of NameCheck.NameUse) = Picked
        If from.Count = 0 OrElse Not butRename.Enabled Then Return
        Dim first As NameCheck.NameUse = from.OrderByDescending(Function(u) u.Files.Count).First()
        frmInputString.Text = If(from.Count > 1, "合併成哪個名字？", "「" & first.Name & "」改成")
        Dim target As String = frmInputString.GetString(first.Name).Trim()
        If target = "" OrElse (from.Count = 1 AndAlso target = first.Name) Then Return
        If Not CheckNameRule(target) OrElse FaceNames.Split(target).Length <> 1 Then
            frmMsgBox.ShowCriticalMessage("名字含有不合法的字元（也不能有 , / 、 這些分隔符號）", Text)
            Return
        End If
        Dim existing As NameCheck.NameUse = m_lpUses.Values.FirstOrDefault(Function(u) u.Files.Count > 0 AndAlso String.Equals(u.Name, target, StringComparison.CurrentCultureIgnoreCase) AndAlso Not from.Contains(u))
        Dim others = from.Where(Function(u) u.Name <> target).ToList()
        If others.Count = 0 Then Return
        Dim count As Integer = others.Sum(Function(u) u.Files.Count)
        Dim what As String = $"把 {count:#,0} 張照片人物欄裡的「{String.Join("」、「", others.Select(Function(u) u.Name))}」改成「{target}」" &
                             If(existing IsNot Nothing, $"{vbCrLf}（「{target}」已經有 {existing.Files.Count:#,0} 張，會合併成同一個人）", "") & "？" &
                             If(m_bolBackedUp, "", vbCrLf & "（會先備份所有 .Exif）")
        If Not frmQueryMsgBox.ShowMessage(what, Text) Then Return
        UseWaitCursor = True
        Try
            If Not EnsureBackup() Then Return
            Dim keep As NameCheck.NameUse = If(existing, from.FirstOrDefault(Function(u) u.Name = target))
            If keep Is Nothing Then
                keep = New NameCheck.NameUse With {.Name = target}
                m_lpUses(target) = keep
            End If
            Dim n As Integer = 0
            For Each u In others
                n += NameCheck.Unify(u, target, g_lpDatabase)
                ' the face person with the old name becomes (or joins) the new one
                Dim p As FaceCatalog.PersonEntry = g_lpFaces?.Catalog?.Persons.FirstOrDefault(Function(x) String.Equals(x.Name, u.Name, StringComparison.CurrentCultureIgnoreCase))
                If p IsNot Nothing Then
                    g_lpFaces.RenamePerson(p, target)
                    m_lpFaceNames.Remove(u.Name)
                    m_lpFaceNames.Add(target)
                End If
                keep.Files.AddRange(u.Files.Except(keep.Files, StringComparer.OrdinalIgnoreCase).ToList())
                u.Files.Clear()
            Next
            m_intChanged += n
        Finally
            UseWaitCursor = False
        End Try
        txtFilter.Text = ""
        FillList()
        For Each it As ListViewItem In lvNames.Items
            it.Selected = it.Text = target
            If it.Selected Then it.EnsureVisible()
        Next
    End Sub

    Private Sub RemoveSelected()
        Dim names As List(Of NameCheck.NameUse) = Picked
        If names.Count = 0 OrElse Not butRemove.Enabled Then Return
        Dim count As Integer = names.Sum(Function(u) u.Files.Count)
        Dim faces = names.Where(Function(u) m_lpFaceNames.Contains(u.Name)).ToList()
        If Not frmQueryMsgBox.ShowMessage($"把「{String.Join("」、「", names.Select(Function(u) u.Name))}」從 {count:#,0} 張照片的人物欄拿掉？照片不會刪除。" &
                                          If(faces.Count > 0, vbCrLf & "面孔資料裡的「" & String.Join("」、「", faces.Select(Function(u) u.Name)) & "」會一起隱藏（可在 面孔 › 已隱藏的人 取消）。", "") &
                                          If(m_bolBackedUp, "", vbCrLf & "（會先備份所有 .Exif）"), Text) Then Return
        UseWaitCursor = True
        Try
            If Not EnsureBackup() Then Return
            Dim n As Integer = 0
            For Each u In names
                n += NameCheck.RemoveName(u, g_lpDatabase)
                Dim p As FaceCatalog.PersonEntry = g_lpFaces?.Catalog?.Persons.FirstOrDefault(Function(x) String.Equals(x.Name, u.Name, StringComparison.CurrentCultureIgnoreCase))
                If p IsNot Nothing AndAlso Not p.Hidden Then g_lpFaces.HidePerson(p)
                u.Files.Clear()
            Next
            m_intChanged += n
        Finally
            UseWaitCursor = False
        End Try
        FillList()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.Escape
                Close()
                Return True
            Case Keys.F2
                RenameSelected()
                Return True
            Case Keys.Delete
                If lvNames.Focused Then
                    RemoveSelected()
                    Return True
                End If
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
