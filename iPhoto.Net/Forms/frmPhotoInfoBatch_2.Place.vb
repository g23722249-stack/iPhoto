' 照片資訊 (batch): the photo's 縣市區 (.Exif City, PlaceNames.Resolve) shown after the 地點 label -- read
' only here; it is changed in the one-photo 照片資訊 or filled by 重新產生 .Exif.
Partial Class frmPhotoInfoBatch_2

    Private m_strSpotCaption As String

    Private Sub ShowPlaceOf(ByVal p As Photo)
        If m_strSpotCaption Is Nothing Then m_strSpotCaption = lblSpot.Text.Trim()
        Dim city As String = p.Exif(enumPhotoExif.peCity)
        lblSpot.Text = m_strSpotCaption & If(city <> "", "　（" & city & "）", "")
    End Sub

    '==================================================================================================
    ' 選擇… beside the batch 地點 (frmPlacePicker): the place is written into the selected photos at once
    ' (GPS, 地點, 國家, 縣市區, 鄉鎮; PhotoIndex follows), not with 批次儲存.
    '==================================================================================================
    Private Sub BatchPlace_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim lnk As New LinkLabel With {.Text = "選擇地點…", .AutoSize = True, .BackColor = Color.Transparent, .Font = lblBatchSpot.Font,
                                       .LinkColor = Color.FromArgb(29, 95, 180), .Enabled = Not g_lpConfig.ReadOnly}
        lblBatchSpot.AutoSize = False
        lblBatchSpot.Width = 120
        barSearch.Controls.Add(lnk)
        lnk.Location = New Point(txtBatchSpot.Left + txtBatchSpot.Width + cboBatchSpot.Width - lnk.PreferredWidth, lblBatchSpot.Top)
        lnk.BringToFront()
        AddHandler lnk.LinkClicked, Sub() PickBatchPlace()
    End Sub

    Private Sub PickBatchPlace()
        Dim files As New List(Of String)
        For Each item As ListViewItem In lvwMain.SelectedItems
            Dim r As PhotoRecord = Nothing
            If m_byFile.TryGetValue(item.Name, r) Then files.Add(r.FileDesc)
        Next
        If files.Count = 0 Then
            frmMsgBox.ShowCriticalMessage("請先在左邊選取要設定地點的照片", "設定拍攝地點")
            Return
        End If
        Dim result As PlacePick.Result = frmPlacePicker.PickAndApply(Me, files)
        If result Is Nothing OrElse result.Written = 0 Then Return
        ' the fields of the photo on show have changed
        If lvwMain.FocusedItem IsNot Nothing AndAlso lvwMain.FocusedItem.Selected Then ShowItem(lvwMain.FocusedItem)
    End Sub

End Class
