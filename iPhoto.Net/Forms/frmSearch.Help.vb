' Help windows for 搜尋 (HelpTip; texts in PhotoLib HelpTexts, keys "search.*"). A field's caption
' and its box share the help; the keyword-book buttons next to the boxes have their own.
Partial Class frmSearch

    Private ReadOnly m_lpHelp As New HelpTip

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        With m_lpHelp
            .SetHelp("search.title", Label3, txtTitle)
            .SetHelp("search.character", Label6, txtCharacter)
            .SetHelp("search.spot", Label10, txtSpot)
            .SetHelp("search.remark", Label1, txtRemark)
            .SetHelp("search.keyword", Label2, txtKeyWord)
            .SetHelp("search.pick", imgKeyWords)
            For i = 0 To rbRange.Length - 1
                .SetHelp("search.range." & i, rbRange(i))
            Next
            .SetHelp("search.date", meStartDate, meEndDate, lblTo)
            .SetHelp("search.ranking", Label4, ddRanking)
            .SetHelp("search.media", Label5, ddMediaType)
            .SetHelp("search.go", butSearch)
            .SetHelp("search.exit", butExit)
        End With
    End Sub

    Private Sub Help_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_lpHelp.Dispose()
    End Sub

End Class
