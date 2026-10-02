' The photo list's right-click: 設定拍攝地點… (frmPlacePicker) and 指定人物… (frmAssignPeople) on the
' photos the menu works on (all selected when the right-clicked one is among them, else that one), and
' 全選 / 全不選 / 反向選取 of the list's selection. A middle click adds / removes a photo, as Ctrl+click
' (Aqua MediaList). New in the .NET port.
Partial Class frmMain

    ''' <summary>Called by SetFileOpsMenu.</summary>
    Private Sub SetAssignMenu(ByVal many As String, ByVal ro As Boolean)
        With AquaMenu1
            .Item("mnuSetPlace").Text = "設定拍攝地點" & many & "…"
            .Item("mnuSetPlace").Enabled = Not ro
            .Item("mnuAssignPeople").Text = "指定人物" & many & "…"
            .Item("mnuAssignPeople").Enabled = Not ro
            Dim n As Integer = mlList.SelectedIndices.Count
            .Item("mnuSelectAll").Enabled = n < mlList.Count
            .Item("mnuSelectNone").Enabled = n > 1
            .Item("mnuSelectInvert").Enabled = mlList.Count > 1
        End With
    End Sub

    Private Function FilesAt(ByVal indexes As List(Of Integer)) As List(Of String)
        Return indexes.Where(Function(i) i >= 0 AndAlso i < mlList.Count).Select(Function(i) mlList.Item(i).FileName).
                       Where(Function(f) Not String.IsNullOrEmpty(f)).ToList()
    End Function

    Private Sub SetPlaceOf(ByVal indexes As List(Of Integer))
        Dim r As PlacePick.Result = frmPlacePicker.PickAndApply(Me, FilesAt(indexes))
        If r Is Nothing OrElse r.Written = 0 Then Return
        RefreshPlaceNode()   ' frmMain.Browse.vb: the 地點 tree is built again when opened
    End Sub

    Private Sub AssignPeopleOf(ByVal indexes As List(Of Integer))
        If Not frmAssignPeople.Assign(Me, FilesAt(indexes)) Then Return
        ' the 面孔 tree follows when the background sort is done (m_lpFaceScan_Organized)
    End Sub

End Class
