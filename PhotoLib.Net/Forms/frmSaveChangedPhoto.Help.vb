' Help windows for 相片已改變 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmSaveChangedPhoto

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("savechanged.0", rbExecute(0))
            .SetHelp("savechanged.1", rbExecute(1))
            .SetHelp("savechanged.2", rbExecute(2))
            .SetHelp("savechanged.3", rbExecute(3))
            .SetHelp("common.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class