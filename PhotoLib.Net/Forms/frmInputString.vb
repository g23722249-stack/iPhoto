' Port of Lib\Form\frmInputString.frm: asks for one line of text (stays on top -- designer TopMost).
' Returns the default when closed without OK, as VB6 did.
Public Class frmInputString

    Private m_strDefault As String = ""

    Public Function GetString(ByVal strDefault As String) As String
        m_strDefault = If(strDefault, "")
        ShowDialog()
        Return m_strDefault
    End Function

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        If TextBox1.Text.Trim() = "" Then
            TextBox1.Focus()
            Return
        End If
        m_strDefault = TextBox1.Text.Trim()
        Close()
    End Sub

    ''' <summary>放棄 (and Esc): closes without taking the text -- GetString returns the default, as
    ''' when the window is closed any other way.</summary>
    Private Sub butCancel_Click(sender As Object, e As EventArgs) Handles butCancel.Click
        Close()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    ' VB6 Form_Load (every show)
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then TextBox1.Text = m_strDefault
        MyBase.OnVisibleChanged(e)
    End Sub

End Class
