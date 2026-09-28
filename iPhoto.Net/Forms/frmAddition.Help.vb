' Help windows for 新增 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmAddition

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("addition.mode.0", rbMode(0))
            .SetHelp("addition.mode.1", rbMode(1))
            .SetHelp("addition.tree", tvList)
            .SetHelp("addition.name", Label1, txtPath)
            .SetHelp("common.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class