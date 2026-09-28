' Help windows for Dock (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmDock

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("dock.list", mlDock)
            .SetHelp("dock.load", Image1)
            .SetHelp("dock.save", Image2)
            .SetHelp("dock.clear", imgClear)
            For Each c As Control In {Image1, Image2, imgClear} : vb6ToolTip.SetToolTip(c, Nothing) : Next
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class