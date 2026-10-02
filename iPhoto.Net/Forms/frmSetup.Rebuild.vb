' 設定 › 一般設定: beside 資料庫's 重建 button, 「重建索引時補上 GPS／地點」 (Config.RebuildFillPlaces,
' iPhoto.Ini [Place] RebuildFill). Checked: the rebuild first fills each photo's blank GPS / 地點 into its
' .Exif (after a backup); unchecked: only the index is made again, no .Exif is written. The 重建 button uses
' the box as it is; 儲存 keeps it for next time (and for 批次修改資訊's 重新編列索引檔).
Partial Class frmSetup

    Private chkRebuildFill As Aqua.CheckBox

    Private Sub RebuildOption_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        chkRebuildFill = New Aqua.CheckBox With {
            .TextValue = "重建索引時補上空白的 GPS／地點（會寫 .Exif）", .TextGap = chkPrivilege.TextGap,
            .Font = New Font(chkPrivilege.Font.FontFamily, 11.0F), .ForeColor = Color.DimGray,
            .BackColor = Color.Transparent, .Location = New Point(imgRebuild.Right + 16, imgRebuild.Top - 1),
            .ImageChecked = chkPrivilege.ImageChecked, .ImageUnChecked = chkPrivilege.ImageUnChecked,
            .ImageCheckDisabled = chkPrivilege.ImageCheckDisabled, .ImageUnCheckDisabled = chkPrivilege.ImageUnCheckDisabled,
            .Checked = g_lpConfig.RebuildFillPlaces}
        imgRebuild.Parent.Controls.Add(chkRebuildFill)
        chkRebuildFill.BringToFront()
        vb6ToolTip.SetToolTip(chkRebuildFill, "勾選：照片有 GPS 但「地點」空白時，重建時從 GPS 填上地點；.Exif 沒有 GPS 時從照片檔讀出來。只填空白，開始前會先備份 .Exif。" & vbCrLf &
                                              "不勾：只重建索引，完全不改 .Exif（刻意清空的地點或 GPS 會保持空白）。")
    End Sub

    ''' <summary>Called by butSave_Click before g_lpConfig.Save.</summary>
    Private Sub SaveRebuildOption()
        If chkRebuildFill Is Nothing Then Return
        g_lpConfig.RebuildFillPlaces = chkRebuildFill.Checked
    End Sub

End Class
