' Help windows for 指定列印頁次 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmPrintPages

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("printpages.0", rdPages(0))
            .SetHelp("printpages.1", rdPages(1))
            .SetHelp("printpages.2", rdPages(2), txtPages, Label1)
            .SetHelp("common.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class