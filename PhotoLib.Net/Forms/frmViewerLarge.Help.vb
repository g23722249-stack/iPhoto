' Help windows for the full-screen viewer (HelpTip; texts in HelpTexts, keys "viewer.*"): the editing
' tool bar, the face bar (made in frmViewerLarge.Faces.vb), prior / next, undo and the sliders.
Partial Class frmViewerLarge

    Private ReadOnly m_lpHelp As New HelpTip

    Private Sub Help_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For i = 0 To picImageBar1.Count - 1
            Dim entry As HelpTip.Entry = HelpTexts.Get("viewer.edit." & i)
            If entry IsNot Nothing Then m_lpHelp.SetToolBarHelp(picImageBar1, i, entry)
        Next
        AddHelp(imbPrior, "viewer.prior")
        AddHelp(imbNext, "viewer.next")
        AddHelp(imgUndo, "viewer.undo")
        AddHelp(lblUndo, "viewer.undo")
        AddHelp(sliBrightness, "viewer.brightness")
        AddHelp(sliContrast, "viewer.contrast")
        AddHelp(picVideo, "viewer.play")
        AddHelp(chkAutoRewind, "viewer.rewind")
        AddHelp(sliSound, "viewer.volume")
        AddHelp(pgVideo, "viewer.position")
        AddFaceBarHelp()
    End Sub

    ''' <summary>The face bar is made on demand (EnsureFaceBar); called from there and from Load.</summary>
    Private Sub AddFaceBarHelp()
        If picFaceBar Is Nothing Then Return
        For i = 0 To picFaceBar.Count - 1
            Dim entry As HelpTip.Entry = HelpTexts.Get("viewer.face." & i)
            If entry IsNot Nothing Then m_lpHelp.SetToolBarHelp(picFaceBar, i, entry)
        Next
    End Sub

    Private Sub AddHelp(ByVal c As Control, ByVal key As String)
        Dim entry As HelpTip.Entry = HelpTexts.Get(key)
        If c IsNot Nothing AndAlso entry IsNot Nothing Then m_lpHelp.SetHelp(c, entry)
    End Sub

    Private Sub Help_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        m_lpHelp.Dispose()   ' the main window keeps this viewer for the whole session
    End Sub

End Class
