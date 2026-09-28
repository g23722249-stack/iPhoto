' Port of Lib\Form\frmQueryMsgBox.frm: the Aqua-styled OK / Cancel question box.
' Form_Load runs whenever the form becomes visible -- see frmMsgBox.
Public Class frmQueryMsgBox

    Private m_szTitle As String = ""
    Private m_szPrompt As String = ""
    Private m_bolResult As Boolean
    Private m_bolBusy As Boolean
    Private m_szDesign As Size = Size.Empty

    ''' <summary>True when the user pressed OK.</summary>
    Public Function ShowMessage(ByVal szPrompt As String, ByVal szTitle As String) As Boolean
        m_bolResult = False
        m_szPrompt = If(szPrompt, "")
        m_szTitle = If(szTitle, "")
        ShowDialog()
        Return m_bolResult
    End Function

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        m_bolResult = True
        Close()
    End Sub

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then
            If m_szDesign.IsEmpty Then m_szDesign = Size
            StartPosition = FormStartPosition.Manual
            SetScreenDefaultValue()
            SetScreenControl()
            m_bolBusy = Application.UseWaitCursor
            Application.UseWaitCursor = False
        Else
            Application.UseWaitCursor = m_bolBusy
        End If
        MyBase.OnVisibleChanged(e)
    End Sub

    Private Sub SetScreenDefaultValue()
        Text = m_szTitle
        lblMessage.Text = m_szPrompt
    End Sub

    Private Sub SetScreenControl()
        Dim gap As Size = lblInterval.Size
        Dim lWidth As Integer = Math.Max(gap.Width + imgIcon.Width + gap.Width + lblMessage.Width + gap.Width, m_szDesign.Width)
        Dim lHeight As Integer = Math.Max(gap.Height + Math.Max(lblMessage.Height, imgIcon.Height) + gap.Height + butOk.Height + gap.Height, m_szDesign.Height)
        ' the screen it is opening on (the app moves dialogs to its main window's screen), not the primary one
        Dim scr As Rectangle = Screen.FromControl(Me).Bounds
        Bounds = New Rectangle(scr.Left + (scr.Width - lWidth) \ 2, scr.Top + (scr.Height - lHeight) \ 2, lWidth, lHeight)

        Dim hasTitle As Boolean = Text.Trim() <> ""
        Dim lChdHeight As Integer = If(hasTitle, lHeight - butOk.Height - 2 * gap.Height, lHeight - butOk.Height - gap.Height)

        imgIcon.Location = New Point(gap.Width, If(hasTitle, gap.Height, 0) + (lChdHeight - imgIcon.Height) \ 2)
        butOk.Location = New Point(CInt((lWidth - butOk.Width * 2.5) / 2), lHeight - gap.Height - butOk.Height)
        butExit.Location = New Point(butOk.Left + CInt(butOk.Width * 1.5), butOk.Top)

        Dim top As Integer = gap.Height
        If lChdHeight > lblMessage.Height Then top = If(hasTitle, gap.Height, 0) + (lChdHeight - lblMessage.Height) \ 2
        lblMessage.Location = New Point(imgIcon.Right + gap.Width, top)
    End Sub

End Class
