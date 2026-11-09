' The 金鑰密碼 dialogs of 加密隱藏 (new in the .NET port; PhotoVault, VaultFiles). Made in code.
'   AskUnlock        the password (or, through 「用救援碼解鎖」, the 救援碼 -- then a new password)
'   AskNewPassword   a new password, twice
'   ShowRecoveryCode the 救援碼, once, to copy or write down
Friend Class frmVault
    Inherits Form

    Private ReadOnly lblPrompt As New Label
    Private ReadOnly txtFirst As New TextBox
    Private ReadOnly txtSecond As New TextBox
    Private ReadOnly lblSecond As New Label
    Private ReadOnly lblError As New Label
    Private ReadOnly lnkRecovery As New LinkLabel
    Private ReadOnly butOk As New Aqua.ThinButton
    Private ReadOnly butCancel As New Aqua.ThinButton
    Private m_check As Func(Of Boolean)
    Private m_bolRecovery As Boolean
    Private m_cancel As Action   ' the second button does this instead of closing

    Private Sub New(ByVal title As String)
        Text = title
        Font = New Font("Microsoft JhengHei UI", 11.0F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        BackColor = Color.White
        ClientSize = New Size(440, 210)
        lblPrompt.SetBounds(20, 14, 400, 48)
        txtFirst.SetBounds(20, 66, 400, 27)
        lblSecond.SetBounds(20, 100, 400, 22)
        txtSecond.SetBounds(20, 124, 400, 27)
        lblError.SetBounds(20, 100, 400, 22)
        lblError.ForeColor = Color.Firebrick
        lnkRecovery.SetBounds(20, 170, 170, 24)
        lnkRecovery.Text = "忘記密碼？用救援碼解鎖"
        lnkRecovery.Visible = False
        butOk.SetBounds(196, 166, 110, 27) : butOk.Text = "確定"
        butCancel.SetBounds(316, 166, 104, 27) : butCancel.Text = "放棄"
        For Each b In {butOk, butCancel}
            b.Font = New Font("華康細圓體", 13.0F)
            b.ForeColor = Color.Black
        Next
        lblSecond.Visible = False : txtSecond.Visible = False
        Controls.AddRange({lblPrompt, txtFirst, lblSecond, txtSecond, lblError, lnkRecovery, butOk, butCancel})
        AcceptButton = Nothing
        AddHandler butOk.Click, Sub() Accept()
        AddHandler butCancel.Click, Sub()
                                        If m_cancel IsNot Nothing Then m_cancel() Else Close()
                                    End Sub
        AddHandler txtFirst.TextChanged, Sub() lblError.Text = ""
        AddHandler txtSecond.TextChanged, Sub() lblError.Text = ""
    End Sub

    Private Sub Accept()
        If m_check Is Nothing OrElse m_check() Then
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.Escape
                Close()
                Return True
            Case Keys.Enter
                If txtSecond.Visible AndAlso txtFirst.Focused Then
                    txtSecond.Focus()
                Else
                    Accept()
                End If
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub ShowError(ByVal text As String)
        lblError.ForeColor = Color.Firebrick
        lblError.Text = text
        lblError.Top = If(txtSecond.Visible, 154, 100)
        txtFirst.SelectAll()
        txtFirst.Focus()
    End Sub

    '==================================================================================================
    ''' <summary>The password (or the 救援碼, then a new password); True when unlocked.</summary>
    Public Shared Function AskUnlock(ByVal owner As IWin32Window) As Boolean
        If PhotoVault.IsUnlocked Then Return True
        Dim recovered As Boolean
        Using f As New frmVault("加密照片")
            f.lblPrompt.Text = "請輸入金鑰密碼，顯示加密隱藏的照片。"
            f.txtFirst.UseSystemPasswordChar = True
            f.lnkRecovery.Visible = PhotoVault.HasRecoveryCode
            AddHandler f.lnkRecovery.LinkClicked, Sub()
                                                      f.m_bolRecovery = True
                                                      f.lnkRecovery.Visible = False
                                                      f.lblPrompt.Text = "請輸入救援碼（大小寫、橫線都可以不管）。" & vbCrLf & "解鎖後要設定新的金鑰密碼。"
                                                      f.txtFirst.UseSystemPasswordChar = False
                                                      f.txtFirst.Text = ""
                                                      f.txtFirst.Focus()
                                                  End Sub
            f.m_check = Function()
                            Dim ok As Boolean
                            ' the key is derived slowly on purpose (about a second): say so meanwhile
                            f.lblError.ForeColor = Color.DimGray
                            f.lblError.Text = If(f.m_bolRecovery, "檢查救援碼中…", "檢查密碼中…")
                            f.butOk.Enabled = False
                            f.Refresh()
                            f.Cursor = Cursors.WaitCursor
                            Try
                                ok = If(f.m_bolRecovery, PhotoVault.UnlockWithRecovery(f.txtFirst.Text), PhotoVault.Unlock(f.txtFirst.Text))
                            Finally
                                f.Cursor = Cursors.Default
                                f.butOk.Enabled = True
                            End Try
                            If Not ok Then f.ShowError(If(f.m_bolRecovery, "救援碼不正確", "密碼不正確"))
                            Return ok
                        End Function
            If f.ShowDialog(owner) <> DialogResult.OK Then Return False
            recovered = f.m_bolRecovery
        End Using
        If recovered Then AskNewPassword(owner, "已經用救援碼解鎖。請設定新的金鑰密碼。")
        Return True
    End Function

    ''' <summary>A new password, twice (unlocked only); True when changed.</summary>
    Public Shared Function AskNewPassword(ByVal owner As IWin32Window, Optional ByVal prompt As String = Nothing) As Boolean
        If Not PhotoVault.IsUnlocked Then Return False
        Using f As New frmVault("更改金鑰密碼")
            f.ClientSize = New Size(440, 250)
            f.lblPrompt.Text = If(prompt, "請輸入新的金鑰密碼。")
            f.txtFirst.UseSystemPasswordChar = True
            f.txtSecond.UseSystemPasswordChar = True
            f.lblSecond.Text = "再輸入一次"
            f.lblSecond.Visible = True : f.txtSecond.Visible = True
            f.butOk.Top = 206 : f.butCancel.Top = 206
            f.m_check = Function()
                            Dim pw As String = f.txtFirst.Text
                            If pw.Length < 4 Then f.ShowError("密碼至少要 4 個字") : Return False
                            If pw <> f.txtSecond.Text Then f.ShowError("兩次輸入的密碼不一樣") : Return False
                            If pw = PhotoVault.DefaultPassword Then f.ShowError("請不要用預設的 123456") : Return False
                            f.Cursor = Cursors.WaitCursor
                            Try
                                Return PhotoVault.ChangePassword(pw)
                            Finally
                                f.Cursor = Cursors.Default
                            End Try
                        End Function
            Return f.ShowDialog(owner) = DialogResult.OK
        End Using
    End Function

    ''' <summary>The 救援碼, shown once.</summary>
    Public Shared Sub ShowRecoveryCode(ByVal owner As IWin32Window, ByVal code As String)
        Using f As New frmVault("救援碼")
            f.ClientSize = New Size(440, 250)
            f.lblPrompt.SetBounds(20, 14, 400, 66)
            f.lblPrompt.Text = "忘記金鑰密碼時，只有這組救援碼能打開加密的照片。" & vbCrLf &
                               "它只會顯示這一次：請抄下來或複製，放在安全的地方。"
            f.txtFirst.SetBounds(20, 92, 400, 30)
            f.txtFirst.Font = New Font("Consolas", 14.0F, FontStyle.Bold)
            f.txtFirst.ReadOnly = True
            f.txtFirst.BackColor = Color.White
            f.txtFirst.TextAlign = HorizontalAlignment.Center
            f.txtFirst.Text = code
            f.lblError.ForeColor = Color.DimGray
            f.lblError.SetBounds(20, 132, 400, 44)
            f.lblError.Text = "沒有救援碼又忘了密碼，加密的照片就再也打不開。"
            f.butCancel.Text = "複製"
            f.butOk.Text = "我記下了"
            f.butOk.Top = 206 : f.butCancel.Top = 206
            f.m_cancel = Sub()
                             Clipboard.SetText(code)
                             f.lblError.Text = "已複製到剪貼簿。"
                         End Sub
            f.ShowDialog(owner)
        End Using
    End Sub

End Class
