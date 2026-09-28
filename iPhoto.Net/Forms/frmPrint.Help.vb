' Help windows for 列印 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmPrint

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("print.printer", Label1, cboPrinter)
            .SetHelp("print.default", Label2, cboDefault)
            .SetHelp("print.type", Label3, cboType)
            .SetHelp("print.copies", Label7, txtPrintCount, UpDown1)
            .SetHelp("print.columns", Label4, Slider1, txtLimit)
            .SetHelp("print.mode.0", rdPrintMode(0))
            .SetHelp("print.mode.1", rdPrintMode(1))
            .SetHelp("print.photos", MediaList1)
            .SetHelp("print.exifdate", chkPrintExifDate)
            .SetHelp("print.outputtime", chkPrintOuputDateTime)
            .SetHelp("print.page", udPage, txtPage)
            .SetHelp("print.advanced", butAdviance)
            .SetHelp("print.paper", butPaper)
            .SetHelp("print.print", butPrint)
            .SetHelp("print.save", butSave)
            .SetHelp("common.close", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class