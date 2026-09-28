' Port of iPhoto\Form\frmPaperSetup.frm: the page margins (上/下/左/右) and the "滿版" space kept around
' each picture, in mm, of a MatrixPrint layout. Digits only; OK is enabled once something is typed.
' Callers use "Using f As New frmPaperSetup".
Friend Class frmPaperSetup

    Private m_objMatrixPrint As MatrixPrint
    Private m_bolChanged As Boolean

    ''' <summary>True when OK was pressed (the layout was changed).</summary>
    Public Function ShowPaperSetup(ByVal objMatrixPrint As MatrixPrint) As Boolean
        m_objMatrixPrint = objMatrixPrint
        m_bolChanged = False
        ShowDialog()
        Return m_bolChanged
    End Function

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If m_objMatrixPrint IsNot Nothing Then
            With m_objMatrixPrint
                txtEdge(0).Text = CStr(.Borderland(enumBorderland.arrHeader))
                txtEdge(1).Text = CStr(.Borderland(enumBorderland.arrFooter))
                txtEdge(2).Text = CStr(.Borderland(enumBorderland.arrLeft))
                txtEdge(3).Text = CStr(.Borderland(enumBorderland.arrRight))
                txtKeep(0).Text = CStr(.KeepWidth)
                txtKeep(1).Text = CStr(.KeepHeight)
            End With
        End If
        butOk.Enabled = False
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        If m_objMatrixPrint IsNot Nothing Then
            With m_objMatrixPrint
                .Borderland(enumBorderland.arrHeader) = CInt(Val(txtEdge(0).Text))
                .Borderland(enumBorderland.arrFooter) = CInt(Val(txtEdge(1).Text))
                .Borderland(enumBorderland.arrLeft) = CInt(Val(txtEdge(2).Text))
                .Borderland(enumBorderland.arrRight) = CInt(Val(txtEdge(3).Text))
                .KeepWidth = CInt(Val(txtKeep(0).Text))
                .KeepHeight = CInt(Val(txtKeep(1).Text))
            End With
        End If
        m_bolChanged = True
        Close()
    End Sub

    Private Sub Digits_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtEdge_0.KeyPress, txtEdge_1.KeyPress, txtEdge_2.KeyPress, txtEdge_3.KeyPress, txtKeep_0.KeyPress, txtKeep_1.KeyPress
        If Char.IsDigit(e.KeyChar) Then
            butOk.Enabled = True
        ElseIf e.KeyChar = ChrW(Keys.Back) Then
            butOk.Enabled = True       ' VB6 swallowed Backspace too, so a wrong digit couldn't be erased
        Else
            e.Handled = True
        End If
    End Sub

End Class
