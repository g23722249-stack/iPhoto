' Help windows for 預設的相片資料 (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmDefaultPhotoInfo

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("field.title", Label8, txtTitle)
            .SetHelp("field.character", Label6, txtCharacter)
            .SetHelp("field.spot", Label10, txtSpot)
            .SetHelp("field.remark", Label4, txtRemark)
            .SetHelp("field.keyword", Label9, txtKeyWord)
            .SetHelp("field.pick", imgKeyWords)
            .SetHelp("defaultinfo.ok", butOk)
            .SetHelp("common.cancel", butExit)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class