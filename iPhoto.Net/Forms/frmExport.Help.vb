' Help windows for 匯出 (HelpTip; texts in PhotoLib HelpTexts, keys "export.*").
Partial Class frmExport

    Private ReadOnly m_lpHelp As New HelpTip

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        With m_lpHelp
            .SetHelp("export.folder", Label1, DeskTop1, txtPath)
            .SetHelp("export.size", Label2, ddSize)
            .SetHelp("export.name", Label3, txtReName)
            .SetHelp("export.sort", chkSort)
            .SetHelp("export.ok", butOk)
            .SetHelp("export.cancel", butExit)
        End With
    End Sub

    Private Sub Help_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_lpHelp.Dispose()
    End Sub

End Class
