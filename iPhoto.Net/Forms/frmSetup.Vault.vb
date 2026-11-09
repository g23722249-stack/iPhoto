' 設定 › 一般「金鑰密碼」(加密隱藏, new in the .NET port; PhotoVault): empty at every start; the right
' password (at first 123456) unlocks the encrypted photos until iPhoto ends, and brings 更改密碼…,
' 新的救援碼… and 上鎖. Takes effect at once (no 儲存 needed). Made in code, below 面孔封面資料夾.
Partial Class frmSetup

    Private lblVault As Label
    Private txtVaultPassword As TextBox
    Private butVaultUnlock As Aqua.ThinButton
    Private lblVaultState As Label
    Private lblVaultNote As Label
    Private butVaultChange As Aqua.ThinButton
    Private butVaultRecovery As Aqua.ThinButton
    Private butVaultLock As Aqua.ThinButton

    Private Sub Vault_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Const y As Integer = 412
        lblVault = New Label With {.Text = "金鑰密碼", .AutoSize = False, .BackColor = Color.Transparent,
                                   .Font = Label22.Font, .ForeColor = Label22.ForeColor, .Bounds = New Rectangle(28, y + 4, 96, 22)}
        txtVaultPassword = New TextBox With {.UseSystemPasswordChar = True, .Bounds = New Rectangle(128, y, 200, 27),
                                             .Font = New Font("Microsoft JhengHei UI", 11.0F)}
        butVaultUnlock = NewVaultButton("解鎖", 338, y, 80)
        lblVaultState = New Label With {.AutoSize = False, .BackColor = Color.Transparent, .Font = Label22.Font,
                                        .ForeColor = Color.FromArgb(46, 125, 50), .Bounds = New Rectangle(128, y + 4, 200, 22)}
        lblVaultNote = New Label With {.AutoSize = False, .BackColor = Color.Transparent, .Font = New Font("Microsoft JhengHei UI", 10.0F),
                                       .ForeColor = Color.Firebrick, .Bounds = New Rectangle(128, y + 34, 700, 22)}
        butVaultChange = NewVaultButton("更改密碼…", 338, y, 120)
        butVaultRecovery = NewVaultButton("新的救援碼…", 468, y, 130)
        butVaultLock = NewVaultButton("上鎖", 608, y, 80)
        pageGeneral.Controls.AddRange({lblVault, txtVaultPassword, butVaultUnlock, lblVaultState, lblVaultNote, butVaultChange, butVaultRecovery, butVaultLock})

        AddHandler butVaultUnlock.Click, Sub() VaultUnlock()
        AddHandler txtVaultPassword.KeyDown, Sub(s, ev)
                                                 If ev.KeyCode = Keys.Enter Then
                                                     ev.SuppressKeyPress = True
                                                     VaultUnlock()
                                                 End If
                                             End Sub
        AddHandler butVaultChange.Click, Sub() If frmVault.AskNewPassword(Me) Then ShowVaultState()
        AddHandler butVaultRecovery.Click, Sub()
                                               If Not frmQueryMsgBox.ShowMessage("產生新的救援碼？" & vbCrLf & "舊的救援碼就不能再用了。", "救援碼") Then Return
                                               Dim code As String = PhotoVault.NewRecoveryCode()
                                               If code IsNot Nothing Then frmVault.ShowRecoveryCode(Me, code)
                                           End Sub
        AddHandler butVaultLock.Click, Sub()
                                           PhotoVault.Lock()
                                           ShowVaultState()
                                       End Sub
        m_lpHelp.SetHelp("setup.vault", lblVault, txtVaultPassword, butVaultUnlock, lblVaultState)   ' frmSetup.Help.vb
        m_lpHelp.SetHelp("setup.vault.change", butVaultChange)
        m_lpHelp.SetHelp("setup.vault.recovery", butVaultRecovery)
        m_lpHelp.SetHelp("setup.vault.lock", butVaultLock)
        ShowVaultState()
    End Sub

    Private Function NewVaultButton(ByVal text As String, ByVal x As Integer, ByVal y As Integer, ByVal w As Integer) As Aqua.ThinButton
        Dim b As New Aqua.ThinButton With {.Text = text, .Font = New Font("華康細圓體", 13.0F), .ForeColor = Color.Black}
        b.SetBounds(x, y, w, 27)
        Return b
    End Function

    Private Sub VaultUnlock()
        ' the key is derived slowly on purpose (about a second): say so meanwhile
        lblVaultNote.ForeColor = Color.DimGray
        lblVaultNote.Text = "檢查密碼中…"
        txtVaultPassword.Enabled = False
        butVaultUnlock.Enabled = False
        Cursor = Cursors.WaitCursor
        pageGeneral.Refresh()
        Dim ok As Boolean
        Try
            ok = PhotoVault.Unlock(txtVaultPassword.Text)
        Finally
            Cursor = Cursors.Default
            txtVaultPassword.Enabled = True
            butVaultUnlock.Enabled = True
            lblVaultNote.ForeColor = Color.Firebrick
            lblVaultNote.Text = ""
        End Try
        txtVaultPassword.Text = ""
        If Not ok Then
            frmMsgBox.ShowExclamationMessage("金鑰密碼不正確。", "金鑰密碼")
            txtVaultPassword.Focus()
            Return
        End If
        ShowVaultState()
    End Sub

    Private Sub ShowVaultState()
        Dim open As Boolean = PhotoVault.IsUnlocked
        txtVaultPassword.Visible = Not open
        butVaultUnlock.Visible = Not open
        lblVaultState.Visible = open
        butVaultChange.Visible = open
        butVaultRecovery.Visible = open
        butVaultLock.Visible = open
        lblVaultState.Text = "已解鎖"
        lblVaultNote.Text = If(open AndAlso PhotoVault.UsesDefaultPassword, "密碼還是預設的 123456，任何人都猜得到：請按「更改密碼」。", "")
    End Sub

End Class
