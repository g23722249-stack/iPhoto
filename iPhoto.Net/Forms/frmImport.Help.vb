' Help windows for 輸入 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmImport

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("import.folder", DeskTop1)
            .SetHelp("import.files", FileListBox1)
            .SetHelp("import.preview", MediaItem1, Label1, Label2, Label3, Label4, lblFileName, lblFileLength, lblFileDateTime, lblResolution)
            .SetHelp("import.target", tvClass)
            .SetHelp("import.foldername", Label5, txtFolder)
            vb6ToolTip.SetToolTip(Label5, Nothing) : vb6ToolTip.SetToolTip(txtFolder, Nothing)
            .SetHelp("import.title", Label6, txtTitle)
            .SetHelp("import.date", Label7, txtDate)
            .SetHelp("import.spot", Label8, txtSpot)
            .SetHelp("import.remark", Label9, txtRemark)
            .SetHelp("import.subject", imgSubject)
            .SetHelp("import.default", Button1)
            .SetHelp("import.auto", chkAutoInfo)
            .SetHelp("import.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class