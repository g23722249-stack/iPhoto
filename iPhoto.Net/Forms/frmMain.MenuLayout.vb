' The photo list's right-click menu in groups (new in the .NET port): the designer's items and the ones
' added in code (FileOps, Books, Vault) are put in this order once the window is up, the like ones in a
' submenu, so the menu is about a dozen lines instead of thirty:
'   全圖瀏覽 · 顯示資訊 · 相片後製… · 我的評價 ▶
'   照片 ▶        複製 · 左轉 90° · 右轉 90° · 恢復到最初狀態
'   地點與人物 ▶  設定拍攝地點… · 指定人物… · 在地圖上看拍攝地點 · 拍攝地點附近的照片
'   整理 ▶        加入 Dock · 移出 Dock · 移動到… · 設為相本封面 · 以檔案總管開啟
'   攝影集 ▶      加入攝影集 ▶ · 整份清單存成攝影集… · 這本攝影集 ▶ · 從攝影集移除
'   加密隱藏… / 解密顯示 · 刪除 ▶ (刪除檔案… · 永久刪除…) · 選取 ▶ (全選 · 全不選 · 反向選取)
' Items are found with FindMenu (AquaMenu.Item(name) only finds first-level ones). A group shows while
' any of its items shows, and is greyed when none of them can be used (UpdateMenuGroups).
Partial Class frmMain

    ''' <summary>The groups: name, caption, the items in it (a "-" is a line).</summary>
    Private Shared ReadOnly MenuGroups As (Name As String, Text As String, Items As String())() = {
        ("mnuGrpPicture", "照片", {"mnuCopy", "-", "mnuRotateClockwise", "mnuRotateCounterClockwise", "-", "mnuRevertToOriginal"}),
        ("mnuGrpPlace", "地點與人物", {"mnuSetPlace", "mnuAssignPeople", "-", "mnuMap", "mnuNearby"}),
        ("mnuGrpOrganize", "整理", {"mnuDockAdd", "mnuDockRemove", "-", "mnuMoveTo", "mnuSetCover", "-", "mnuExplorer"}),
        ("mnuGrpBook", "攝影集", {"mnuBookAdd", "mnuBookFromList", "mnuBookOps", "mnuRemoveFromBook"}),
        ("mnuGrpDelete", "刪除", {"mnuDeleteFile", "mnuDeleteForever"}),
        ("mnuGrpSelect", "選取", {"mnuSelectAll", "mnuSelectNone", "mnuSelectInvert"})}

    ''' <summary>The first level, top to bottom: an item, a group (its name), or "-".</summary>
    Private Shared ReadOnly MenuTop As String() = {
        "mnuView", "mnuShowInfo", "mnuEdit", "mnuMyRanking", "-",
        "mnuGrpPicture", "mnuGrpPlace", "mnuGrpOrganize", "mnuGrpBook", "-",
        "mnuEncrypt", "mnuDecrypt", "mnuGrpDelete", "-",
        "mnuGrpSelect"}

    Private m_bolMenuArranged As Boolean

    Private Sub MenuLayout_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        ArrangePopupMenu()
    End Sub

    ''' <summary>Puts AquaMenu1's items in the order above (once). An item keeps the items under it (我的評價's
    ''' stars, 加入攝影集's books ...), one level deeper inside a group. Items not named above go last.</summary>
    Private Sub ArrangePopupMenu()
        If m_bolMenuArranged Then Return
        m_bolMenuArranged = True
        Dim flat As List(Of Aqua.MenuItem) = AquaMenu1.Items.ToList()
        Dim orig As Integer() = flat.Select(Function(m) m.Level).ToArray()   ' the levels before any is changed
        Dim used As New HashSet(Of Aqua.MenuItem)
        Dim result As New List(Of Aqua.MenuItem)
        Dim sep As Integer = 0

        ' an item and those under it (by the original levels), at the new level
        Dim take = Sub(name As String, level As Integer)
                       Dim i As Integer = flat.FindIndex(Function(m) String.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase))
                       If i < 0 OrElse used.Contains(flat(i)) Then Return
                       Dim baseLevel As Integer = orig(i)
                       Dim k As Integer = i
                       Do
                           Dim m As Aqua.MenuItem = flat(k)
                           If Not used.Contains(m) Then
                               m.Level = level + (orig(k) - baseLevel)
                               result.Add(m)
                               used.Add(m)
                           End If
                           k += 1
                       Loop While k < flat.Count AndAlso orig(k) > baseLevel
                   End Sub
        Dim line = Sub(level As Integer)
                       sep += 1
                       result.Add(New Aqua.MenuItem("mnuLayoutSep" & sep, "-", level, Nothing, Nothing))
                   End Sub

        For Each entry As String In MenuTop
            If entry = "-" Then
                line(1)
                Continue For
            End If
            Dim g = MenuGroups.FirstOrDefault(Function(x) x.Name = entry)
            If g.Name Is Nothing Then
                Dim i As Integer = flat.FindIndex(Function(m) String.Equals(m.Name, entry, StringComparison.OrdinalIgnoreCase))
                If i >= 0 AndAlso flat(i).Text = "-" Then   ' a line of its own (mnuVaultSep: shown with the vault items)
                    flat(i).Level = 1
                    result.Add(flat(i))
                    used.Add(flat(i))
                Else
                    take(entry, 1)
                End If
                Continue For
            End If
            result.Add(New Aqua.MenuItem(g.Name, g.Text, 1, Nothing, Nothing))
            For Each it In g.Items
                If it = "-" Then line(2) Else take(it, 2)
            Next
        Next
        ' anything else (added later, or a line of the old order) -- lines dropped, items kept at the end
        For Each m In flat
            If used.Contains(m) OrElse m.Text = "-" Then Continue For
            m.Level = If(m.Level < 1, 1, m.Level)
            result.Add(m)
        Next

        AquaMenu1.Items.Clear()
        For Each m In result
            AquaMenu1.Items.Add(m)
        Next
    End Sub

    ''' <summary>Called last by SetMediaListPopupMenu: a group shows while any of its items does, greyed
    ''' when none of those can be used.</summary>
    Private Sub UpdateMenuGroups()
        For Each g In MenuGroups
            Dim grp As Aqua.MenuItem = AquaMenu1.FindMenu(g.Name)
            If grp Is Nothing Then Continue For
            Dim shown As New List(Of Aqua.MenuItem)
            For i = 0 To grp.Count - 1
                If grp(i).Visible AndAlso grp(i).Text <> "-" Then shown.Add(grp(i))
            Next
            grp.Visible = shown.Count > 0
            grp.Enabled = shown.Any(Function(m) m.Enabled)
        Next
    End Sub

End Class
