' Help windows for 照片資訊的修改（步驟二） (HelpTip; texts in PhotoLib HelpTexts). Registered on the first Shown -- some
' of these forms are default instances shown again and again -- and released with the form.
Partial Class frmPhotoInfoBatch_2

    Private ReadOnly m_lpHelp As New HelpTip
    Private m_bolHelpReady As Boolean

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If m_bolHelpReady Then Return
        m_bolHelpReady = True
        With m_lpHelp
            .SetHelp("batch2.list", lvwMain)
            .SetHelp("batch2.one", lblBarOne)
            .SetHelp("field.title", lblTitle, txtTitle)
            .SetHelp("field.character", lblCharacter, txtCharacter)
            .SetHelp("field.datetime", lblDateTime, txtDateTime)
            .SetHelp("field.spot", lblSpot, txtSpot)
            .SetHelp("field.keyword", lblKeyword, txtKeyword)
            .SetHelp("field.remark", lblRemark, txtRemark)
            .SetHelp("batch2.pick", cboCharacter, cboSpot, cboBatchCharacter, cboBatchSpot)
            .SetHelp("batch2.batch", lblBarBatch, lblBatchTitle, txtBatchTitle, lblBatchCharacter, txtBatchCharacter, lblBatchSpot, txtBatchSpot, lblBatchKeyword, txtBatchKeyword, lblBatchRemark, txtBatchRemark)
            .SetHelp("batch2.save", butBatchSave)
            .SetHelp("batch2.checkall", butCheckedAll)
            .SetHelp("batch2.uncheckall", butUnCheckedAll)
            .SetHelp("batch2.unload", butUnload)
            .SetHelp("batch2.index", chkBuildIndex)
        End With
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()
    End Sub

End Class