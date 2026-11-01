' 設定 › 外觀「簡單的按鈕也顯示說明」(HelpTip.ShowSimple, new in the .NET port): the help windows of
' controls that say it all (取消, 上一張, a plain field -- HelpTexts.SimpleKeys) only with this ticked;
' off at first. Made in code, below 音效, so the converted designer file stays as ported.
Partial Class frmSetup

    Private chkHelpSimple As Aqua.CheckBox

    Private Sub HelpOption_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        chkHelpSimple = New Aqua.CheckBox With {
            .BackColor = Color.Transparent,
            .ImageChecked = chkSwitchScreen.ImageChecked, .ImageUnChecked = chkSwitchScreen.ImageUnChecked,
            .ImageCheckDisabled = chkSwitchScreen.ImageCheckDisabled, .ImageUnCheckDisabled = chkSwitchScreen.ImageUnCheckDisabled,
            .TextValue = "簡單的按鈕也顯示說明（例如「取消」「上一張」；複雜的功能一律顯示）",
            .TextGap = 6, .ForeColor = SystemColors.WindowText, .Font = chkSwitchScreen.Font,
            .Location = New Point(26, 414), .Size = New Size(800, 28), .TabStop = False,
            .Checked = HelpTip.ShowSimple}
        pageAppearance.Controls.Add(chkHelpSimple)
    End Sub

    ''' <summary>Called by butSave_Click.</summary>
    Private Sub SaveHelpOption()
        HelpTip.ShowSimple = chkHelpSimple.Checked
    End Sub

End Class
