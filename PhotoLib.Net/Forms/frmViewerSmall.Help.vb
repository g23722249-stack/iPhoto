' Help windows for 全圖瀏覽（小螢幕） (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmViewerSmall

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("viewer.prior", imbPrior)
            .SetHelp("viewer.next", imbNext)
            .SetHelp("viewer.play", picVideo)
            .SetHelp("viewer.rewind", chkAutoRewind)
            .SetHelp("viewer.volume", sliSound)
            .SetHelp("viewer.position", pgVideo)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class