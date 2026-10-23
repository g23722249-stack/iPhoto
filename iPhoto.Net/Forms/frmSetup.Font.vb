' 設定 › 外觀「顯示字型」 (Config.InterfaceFontName, iPhoto.Ini [Interface] FontName): the 「…」 beside it
' picks one of the installed fonts (it did nothing before); 儲存 keeps it -- a name Windows doesn't have
' is refused -- and when it changed asks to restart: every window takes it at the next start (FontScheme).
Partial Class frmSetup

    Private m_strFontWas As String

    Private Sub FontOption_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_strFontWas = g_lpConfig.InterfaceFontName
        imgFontName.Cursor = Cursors.Hand
    End Sub

    Private Sub imgFontName_Click(sender As Object, e As EventArgs) Handles imgFontName.Click
        g_lpConfig.PlaySound(Config.enumSound.snOpenDialogBox)
        Flash(imgFontName, 20)
        Using dlg As New FontDialog With {.ShowEffects = False, .ShowColor = False, .AllowVerticalFonts = False,
                                          .AllowSimulations = False, .FontMustExist = True}
            If FontExists(txtFont.Text.Trim()) Then dlg.Font = New Font(txtFont.Text.Trim(), 12.0F)
            If dlg.ShowDialog(Me) = DialogResult.OK Then txtFont.Text = dlg.Font.Name
        End Using
    End Sub

    ''' <summary>Called by butSave_Click before g_lpConfig.Save; True when the font changed.</summary>
    Private Function SaveFontOption() As Boolean
        Dim name As String = txtFont.Text.Trim()
        If name.Length = 0 Then name = m_strFontWas
        If Not FontExists(name) Then
            frmMsgBox.ShowExclamationMessage("找不到字型「" & name & "」，顯示字型維持「" & m_strFontWas & "」。", "顯示字型")
            name = m_strFontWas
        End If
        g_lpConfig.InterfaceFontName = name
        Return Not String.Equals(name, m_strFontWas, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>After 儲存 changed the font: 「稍後再重開」 or 「現在重新開啟 iPhoto」.</summary>
    Private Sub AskRestartForFont()
        If frmRestartAsk.Ask(Owner, "顯示字型改為「" & g_lpConfig.InterfaceFontName & "」。",
                             "重新開啟 iPhoto 後所有畫面才會換成這個字型。") Then
            Program.RestartApplication()
        End If
    End Sub

    Private Shared Function FontExists(ByVal name As String) As Boolean
        If String.IsNullOrWhiteSpace(name) Then Return False
        Try
            Using ff As New FontFamily(name)
                Return True
            End Using
        Catch ex As ArgumentException
            Return False
        End Try
    End Function

End Class
