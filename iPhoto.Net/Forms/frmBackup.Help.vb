' Help windows for 同步備份 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmBackup

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("backup.sources", lblSource, lstSource)
            .SetHelp("backup.add", imgSource_0)
            .SetHelp("backup.remove", imgSource_1)
            .SetHelp("backup.target", lblTarget, txtTarget, imgTarget)
            .SetHelp("backup.applyall", chkApplyAll)
            .SetHelp("backup.log", lblLog, txtLog)
            .SetHelp("backup.start", butStart)
            .SetHelp("common.close", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class