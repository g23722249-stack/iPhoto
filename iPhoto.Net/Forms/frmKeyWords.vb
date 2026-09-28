' Port of iPhoto\Form\frmKeyWords.frm: pick keywords from a keyword file by section (the sections are
' the cboSection items in the designer); double-click a keyword to append it, "+" adds the typed text to
' the current section and saves the file. Callers use "Using f As New frmKeyWords".
Friend Class frmKeyWords

    Private m_strKeyWords As String = ""
    Private m_strFileName As String = ""
    Private m_objKeyWords As KeyWord

    ''' <summary>Returns the chosen keywords ("" = cancelled).</summary>
    Public Function ShowKeyWords(ByVal FileName As String) As String
        m_strKeyWords = ""
        m_strFileName = FileName
        ShowDialog()
        Return m_strKeyWords
    End Function

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_objKeyWords = New KeyWord
        m_objKeyWords.Construct(m_strFileName)
        txtKeyWords.Text = ""
        If cboSection.Items.Count > 0 Then cboSection.SelectedIndex = 0
        cboSection_SelectedChanged(cboSection, EventArgs.Empty)
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        If txtKeyWords.Text.Trim() = "" Then
            txtKeyWords.Focus()
        Else
            m_strKeyWords = txtKeyWords.Text.Trim()
            Close()
        End If
    End Sub

    Private Function CurrentSection() As String
        Return If(cboSection.SelectedItem?.Name, "")
    End Function

    Private Sub cboSection_SelectedChanged(sender As Object, e As EventArgs) Handles cboSection.SelectedChanged
        If m_objKeyWords Is Nothing Then Return
        lstKeyWord.Clear()
        If CurrentSection() = "" Then Return
        Dim strArray() As String = Nothing
        Dim intCount As Integer = m_objKeyWords.Keys(CurrentSection(), strArray)
        For I As Integer = 0 To intCount - 1
            lstKeyWord.AddItem(strArray(I), strArray(I))
        Next
    End Sub

    Private Sub imgAddition_Click(sender As Object, e As EventArgs) Handles imgAddition.Click
        If txtKeyWords.Text.Trim() = "" OrElse CurrentSection() = "" Then Return
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        m_objKeyWords.AddItem(CurrentSection(), txtKeyWords.Text.Trim())
        m_objKeyWords.Save()
        cboSection_SelectedChanged(cboSection, EventArgs.Empty)
    End Sub

    Private Sub lstKeyWord_DoubleClick(sender As Object, e As EventArgs) Handles lstKeyWord.DoubleClick
        If lstKeyWord.SelectedItem Is Nothing Then Return
        If txtKeyWords.Text.Trim() = "" Then
            txtKeyWords.Text = lstKeyWord.SelectedItem.Text
        Else
            txtKeyWords.Text = txtKeyWords.Text & "," & lstKeyWord.SelectedItem.Text
        End If
    End Sub

End Class
