' Port of iPhoto\Form\frmPrintPages.frm: which pages to print -- "ALL", the current page, or a list
' typed as "1,3,5" (digits, commas, spaces). Returns "" when cancelled.
' Callers use "Using f As New frmPrintPages".
Friend Class frmPrintPages

    Private m_intPage As Integer
    Private m_strResult As String = ""

    Public Function ShowPages(ByVal intPage As Integer) As String
        m_intPage = intPage
        m_strResult = ""
        ShowDialog()
        Return m_strResult
    End Function

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        rdPages_1.TextValue = "本頁：" & m_intPage
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        m_strResult = ""
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        If rdPages_0.Checked Then
            m_strResult = "ALL"
        ElseIf rdPages_1.Checked Then
            m_strResult = CStr(m_intPage)
        ElseIf rdPages_2.Checked Then
            m_strResult = txtPages.Text.Replace("，", ",").Replace(" ", "").Trim()
        End If
        Close()
    End Sub

    Private Sub rdPages_CheckedChanged(sender As Object, e As EventArgs) Handles rdPages_2.CheckedChanged
        If rdPages_2.Checked AndAlso Visible Then txtPages.Focus()
    End Sub

    Private Sub txtPages_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPages.KeyPress
        Select Case e.KeyChar
            Case "0"c To "9"c, ","c, " "c, ChrW(Keys.Back)
            Case Else
                e.Handled = True
        End Select
    End Sub

End Class
