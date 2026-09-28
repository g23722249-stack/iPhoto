' Help windows for 照片目錄 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmPhotoIndex

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("photoindex.tree", tvList)
            .SetHelp("photoindex.columns", Label1, Slider1, txtLimit)
            .SetHelp("photoindex.note.0", chkPrintNote(0))
            .SetHelp("photoindex.note.1", chkPrintNote(1))
            .SetHelp("photoindex.printer", Label2, cboPrinter)
            .SetHelp("print.advanced", butAdviance)
            .SetHelp("print.paper", butPaper)
            .SetHelp("photoindex.preview", butPreview)
            .SetHelp("photoindex.page", UpDown1, lblPage)
            .SetHelp("photoindex.print", butPrint)
            .SetHelp("photoindex.save", butSave)
            .SetHelp("common.close", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class