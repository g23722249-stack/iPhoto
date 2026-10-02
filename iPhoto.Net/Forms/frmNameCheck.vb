' 人物欄名字檢查 (new in the .NET port; PhotoLib NameCheck): reads every people field, lists the groups
' of spellings that look like one person (with why, and how alike their faces are), and rewrites the
' spelling chosen into the photos that have another. The .Exif files are backed up once before the
' first change (Maintenance.BackupExif); a face person with an old spelling is renamed / merged too.
' Opened from 設定 › 維護. Made in code (no designer file).
Friend Class frmNameCheck
    Inherits Form

    Private ReadOnly lblStatus As New Label
    Private ReadOnly lstGroups As New ListBox
    Private ReadOnly lblReason As New Label
    Private ReadOnly pnlNames As New FlowLayoutPanel
    Private ReadOnly butUnify As New Aqua.ThinButton
    Private ReadOnly butSkip As New Aqua.ThinButton
    Private ReadOnly butClose As New Aqua.ThinButton
    Private m_lpUses As Dictionary(Of String, NameCheck.NameUse)
    Private m_bolBackedUp As Boolean
    Private m_intChanged As Integer

    Public Sub New()
        Text = "人物欄名字檢查"
        Font = New Font("Microsoft JhengHei UI", 11.0F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(760, 500)
        BackColor = Color.White

        lblStatus.SetBounds(16, 12, 728, 26)
        lstGroups.SetBounds(16, 44, 330, 390)
        lstGroups.IntegralHeight = False
        lblReason.SetBounds(362, 44, 382, 110)
        lblReason.ForeColor = Color.DimGray
        pnlNames.SetBounds(362, 160, 382, 220)
        pnlNames.FlowDirection = FlowDirection.TopDown
        pnlNames.WrapContents = False
        pnlNames.AutoScroll = True
        butUnify.SetBounds(362, 392, 220, 27) : butUnify.Text = "統一成選取的名字"
        butSkip.SetBounds(594, 392, 150, 27) : butSkip.Text = "略過這組"
        butClose.SetBounds(644, 450, 100, 27) : butClose.Text = "關閉"
        For Each b In {butUnify, butSkip, butClose}
            b.Font = New Font("華康細圓體", 13.0F)
            b.ForeColor = Color.Black
        Next
        Controls.AddRange({lblStatus, lstGroups, lblReason, pnlNames, butUnify, butSkip, butClose})
        SetButtons()

        AddHandler lstGroups.SelectedIndexChanged, Sub() ShowGroup()
        AddHandler butUnify.Click, Sub() UnifyGroup()
        AddHandler butSkip.Click, Sub() RemoveGroup()
        AddHandler butClose.Click, Sub() Close()
    End Sub

    ''' <summary>How many .Exif files were changed (the caller refreshes what it shows).</summary>
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
        Dim groups As List(Of NameCheck.NameGroup) = NameCheck.Candidates(uses, FaceSimilarity())
        lstGroups.BeginUpdate()
        lstGroups.Items.Clear()
        For Each g In groups
            lstGroups.Items.Add(g)
        Next
        lstGroups.EndUpdate()
        lblStatus.Text = $"{uses.Count:#,0} 個名字，{groups.Count} 組可能是同一個人的不同寫法。"
        If groups.Count > 0 Then lstGroups.SelectedIndex = 0 Else lblReason.Text = "沒有發現寫法相近的名字。"
        SetButtons()
    End Sub

    ''' <summary>How alike two names' faces are (their templates), -1 when either has none.</summary>
    Private Function FaceSimilarity() As Func(Of String, String, Single)
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.FaceTablesReady Then Return Nothing
        Dim ids As Dictionary(Of String, Integer) = g_lpDatabase.LoadFacePersons().
            GroupBy(Function(p) p.Name, StringComparer.CurrentCultureIgnoreCase).ToDictionary(Function(g) g.Key, Function(g) g.First().PersonID, StringComparer.CurrentCultureIgnoreCase)
        Dim templates As Dictionary(Of Integer, List(Of Single())) = g_lpDatabase.LoadTemplateFeatures()
        Return Function(a As String, b As String) As Single
                   Dim ia, ib As Integer
                   If Not ids.TryGetValue(a, ia) OrElse Not ids.TryGetValue(b, ib) OrElse Not templates.ContainsKey(ia) OrElse Not templates.ContainsKey(ib) Then Return -1
                   Dim best As Single = -1
                   For Each x In templates(ia)
                       For Each y In templates(ib)
                           best = Math.Max(best, Quartz.FaceEngine.Cosine(x, y))
                       Next
                   Next
                   Return best
               End Function
    End Function

    Private ReadOnly Property Current As NameCheck.NameGroup
        Get
            Return TryCast(lstGroups.SelectedItem, NameCheck.NameGroup)
        End Get
    End Property

    Private Sub ShowGroup()
        pnlNames.Controls.Clear()
        Dim g As NameCheck.NameGroup = Current
        If g Is Nothing Then
            lblReason.Text = ""
            SetButtons()
            Return
        End If
        lblReason.Text = String.Join(vbCrLf, g.Reasons) &
                         If(g.FaceSimilarity >= 0, vbCrLf & $"臉部相似度 {g.FaceSimilarity:0.00}" & If(g.FaceSimilarity >= 0.55, "（很像，可能是同一人）", If(g.FaceSimilarity < 0.35, "（不太像，可能是不同的人）", "")), "") &
                         vbCrLf & "選要保留的寫法，其他寫法會改成它："
        For Each u In g.Names
            pnlNames.Controls.Add(New RadioButton With {.Text = $"{u.Name}（{u.Files.Count} 張）", .Tag = u, .AutoSize = True, .Checked = u Is g.Names(0)})
        Next
        SetButtons()
    End Sub

    Private Sub SetButtons()
        Dim has As Boolean = Current IsNot Nothing
        butUnify.Enabled = has AndAlso Not g_lpConfig.ReadOnly
        butSkip.Enabled = has
    End Sub

    Private Sub UnifyGroup()
        Dim g As NameCheck.NameGroup = Current
        If g Is Nothing Then Return
        Dim chosen As RadioButton = pnlNames.Controls.OfType(Of RadioButton)().FirstOrDefault(Function(r) r.Checked)
        If chosen Is Nothing Then Return
        Dim target As NameCheck.NameUse = CType(chosen.Tag, NameCheck.NameUse)
        Dim others = g.Names.Where(Function(u) u IsNot target).ToList()
        Dim count As Integer = others.Sum(Function(u) u.Files.Count)
        If Not frmQueryMsgBox.ShowMessage($"把 {count} 張照片人物欄裡的「{String.Join("」、「", others.Select(Function(u) u.Name))}」改成「{target.Name}」？" &
                                          If(m_bolBackedUp, "", vbCrLf & "（會先備份所有 .Exif）"), "人物欄名字檢查") Then Return
        UseWaitCursor = True
        Try
            If Not m_bolBackedUp Then
                lblStatus.Text = "備份 .Exif 中…"
                lblStatus.Refresh()
                If Maintenance.BackupExif(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase), Roots()).Zip = "" Then
                    frmMsgBox.ShowCriticalMessage(".Exif 備份失敗，這次不修改。", "人物欄名字檢查")
                    Return
                End If
                m_bolBackedUp = True
            End If
            Dim n As Integer = 0
            For Each u In others
                n += NameCheck.Unify(u, target.Name, g_lpDatabase)
                ' the face person with the old spelling becomes (or joins) the chosen one
                Dim p As FaceCatalog.PersonEntry = g_lpFaces?.Catalog?.Persons.FirstOrDefault(Function(x) String.Equals(x.Name, u.Name, StringComparison.CurrentCultureIgnoreCase))
                If p IsNot Nothing Then g_lpFaces.RenamePerson(p, target.Name)
                target.Files.AddRange(u.Files.Except(target.Files, StringComparer.OrdinalIgnoreCase).ToList())
                u.Files.Clear()
            Next
            m_intChanged += n
            lblStatus.Text = $"已修改 {n} 張照片的人物欄（{String.Join("、", others.Select(Function(u) u.Name))} → {target.Name}）。"
        Finally
            UseWaitCursor = False
        End Try
        RemoveGroup()
    End Sub

    Private Sub RemoveGroup()
        Dim i As Integer = lstGroups.SelectedIndex
        If i < 0 Then Return
        lstGroups.Items.RemoveAt(i)
        If lstGroups.Items.Count > 0 Then lstGroups.SelectedIndex = Math.Min(i, lstGroups.Items.Count - 1) Else ShowGroup()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
