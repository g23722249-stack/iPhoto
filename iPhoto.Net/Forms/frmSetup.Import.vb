' 設定 › 相片庫: 「輸入照片使用新畫面」 (Config.ImportNewStyle, iPhoto.Ini [Import] Style): the tool bar's
' 輸入 opens frmImportStudio, or -- unchecked -- the ported frmImport. Made in code (like the 面孔 page)
' so the designer file stays as ported; it borrows chkPrivilege's pictures.
Partial Class frmSetup

    Private chkImportNew As Aqua.CheckBox

    Private Sub ImportOption_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        chkImportNew = New Aqua.CheckBox With {
            .TextValue = "輸入照片使用新畫面", .TextGap = chkPrivilege.TextGap, .Font = chkPrivilege.Font, .ForeColor = chkPrivilege.ForeColor,
            .BackColor = Color.Transparent, .Location = New Point(430, 336),
            .ImageChecked = chkPrivilege.ImageChecked, .ImageUnChecked = chkPrivilege.ImageUnChecked,
            .ImageCheckDisabled = chkPrivilege.ImageCheckDisabled, .ImageUnCheckDisabled = chkPrivilege.ImageUnCheckDisabled,
            .Checked = g_lpConfig.ImportNewStyle}
        chkPrivilege.Parent.Controls.Add(chkImportNew)
        chkImportNew.BringToFront()
    End Sub

    ''' <summary>Called by butSave_Click before g_lpConfig.Save.</summary>
    Private Sub SaveImportOption()
        If chkImportNew Is Nothing Then Return
        g_lpConfig.ImportNewStyle = chkImportNew.Checked
    End Sub

End Class
