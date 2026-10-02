' Help windows for the main window (HelpTip; texts in PhotoLib HelpTexts, keys "main.*"). Registered
' once the window is up (Shown: the face status label is made in Load). The old one-line tooltips of
' these controls (vb6ToolTip -- most of the tool box said 「設定」) are removed so only one shows.
Partial Class frmMain

    Private ReadOnly m_lpHelp As New HelpTip

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        For i = 0 To imgToolBox.Length - 1
            AddHelp(imgToolBox(i), "main.toolbox." & i)
            ' the caption under a tool-box button shows the button's help (and icon) too
            Dim entry As HelpTip.Entry = HelpTexts.Get("main.toolbox." & i)
            If entry IsNot Nothing AndAlso lblToolBox(i) IsNot Nothing Then
                m_lpHelp.SetHelp(lblToolBox(i), New HelpTip.Entry With {
                    .Title = entry.Title, .Text = entry.Text, .Hint = entry.Hint, .DisabledHint = entry.DisabledHint, .Icon = imgToolBox(i).Image})
                vb6ToolTip.SetToolTip(lblToolBox(i), Nothing)
            End If
        Next
        For i = 0 To imgButton.Length - 1
            AddHelp(imgButton(i), "main.button." & i)
        Next
        AddHelp(butMode, "main.mode")
        AddHelp(iTextBox1, "main.search")
        AddHelp(sliSize, "main.size")
        AddHelp(imgSubject, "main.subject")
        AddHelp(lblSelCount, "main.dock")
        AddHelp(lblFaceScan, "main.facescan")
        AddHelp(lblFaceGuide, "main.faceguide")
    End Sub

    Private Sub AddHelp(ByVal c As Control, ByVal key As String)
        If c Is Nothing Then Return
        Dim entry As HelpTip.Entry = HelpTexts.Get(key)
        If entry Is Nothing Then Return
        m_lpHelp.SetHelp(c, entry)
        vb6ToolTip.SetToolTip(c, Nothing)
    End Sub

    Private Sub Help_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_lpHelp.Dispose()
    End Sub

End Class
