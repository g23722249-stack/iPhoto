' Help windows for 詳細資料 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmPhotoInfo

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("field.fileinfo", Label5, Label1, Label2, Label3, lblPath, lblFileName, lblFileDateTime, lblFileLength)
            .SetHelp("field.datetime", Label7, meDate, meTime)
            .SetHelp("field.title", Label8, txtTitle)
            .SetHelp("field.character", Label6, txtCharacter)
            .SetHelp("field.spot", Label10, txtSpot)
            .SetHelp("field.remark", Label4, txtRemark)
            .SetHelp("field.keyword", Label9, txtKeyWord)
            .SetHelp("field.pick", imgKeyWords)
            .SetHelp("common.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class