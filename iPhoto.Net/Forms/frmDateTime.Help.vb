' Help windows for 照片時間調整 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmDateTime

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("datetime.file", Label7, Label2, txtYYYY(0), txtMM(0), txtDD(0), txtHH(0), txtNN(0), txtSS(0))
            .SetHelp("datetime.exif", Label1, Label3, txtYYYY(1), txtMM(1), txtDD(1), txtHH(1), txtNN(1), txtSS(1))
            .SetHelp("datetime.new", Label4, Label5, txtYYYY(2), txtMM(2), txtDD(2), txtHH(2), txtNN(2), txtSS(2))
            .SetHelp("datetime.mode.0", chkSaveMode(0))
            .SetHelp("datetime.mode.1", chkSaveMode(1))
            .SetHelp("datetime.save", butSave)
            .SetHelp("datetime.all", cmdSaveToFolder)
            .SetHelp("common.close", cmdCancel)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class