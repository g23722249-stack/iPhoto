' Help windows for 版面設定 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmPaperSetup

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("paper.edge", Label1, Label2, Label3, Label4, Label5, txtEdge(0), txtEdge(1), txtEdge(2), txtEdge(3))
            .SetHelp("paper.keep", Label6, Label7, Label8, txtKeep(0), txtKeep(1))
            .SetHelp("common.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class