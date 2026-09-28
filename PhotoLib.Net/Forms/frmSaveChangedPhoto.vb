' Port of Lib\Form\frmSaveChangedPhoto.frm: asks what to do with a photo edited in the viewer.
' The VB6 Public Enum lived in the form module (so it was global); here it sits beside the class.
Public Enum scpResult
    scpCancel = -1
    scpSave = 1
    scpCopy = 2
    scpSaveAs = 3
End Enum

Public Class frmSaveChangedPhoto

    Private m_enumResult As scpResult = scpResult.scpCancel

    Public ReadOnly Property Result As scpResult
        Get
            Return m_enumResult
        End Get
    End Property

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        Select Case True
            Case rbExecute_0.Checked : m_enumResult = scpResult.scpCancel
            Case rbExecute_1.Checked : m_enumResult = scpResult.scpSave
            Case rbExecute_2.Checked : m_enumResult = scpResult.scpCopy
            Case rbExecute_3.Checked : m_enumResult = scpResult.scpSaveAs
        End Select
        Close()
    End Sub

    ' VB6 Form_Load (every show -- see frmMsgBox)
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then
            lblTitle.Text = "相片的內容已改變了，要如何處理呢？"
            m_enumResult = scpResult.scpCancel
            rbExecute_0.Checked = True

            If g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly Then
                ' struck out; disabled, the radio button draws its caption grey itself
                rbExecute_1.Font = New Font(rbExecute_1.Font, rbExecute_1.Font.Style Or FontStyle.Strikeout)
                rbExecute_1.Enabled = False
            End If
        End If
        MyBase.OnVisibleChanged(e)
    End Sub

End Class
