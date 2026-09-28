' Port of Lib\Form\frmMsgBox.frm: the Aqua-styled message box (critical / smile / question / exclamation).
' VB6 unloaded the form after every message, so its Form_Load ran on each show; the .NET default
' instance lives on (ShowDialog + Close only hides it), so that code runs whenever the form becomes visible.
Public Class frmMsgBox

    Private m_szTitle As String = ""
    Private m_szPrompt As String = ""
    Private m_bolBusy As Boolean
    Private m_szDesign As Size = Size.Empty

    Public Sub ShowCriticalMessage(ByVal szPrompt As String, ByVal szTitle As String)
        ShowWithIcon(szPrompt, szTitle, imgCritical)
    End Sub

    Public Sub ShowSmileMessage(ByVal szPrompt As String, ByVal szTitle As String)
        ShowWithIcon(szPrompt, szTitle, imgSuccess)
    End Sub

    Public Sub ShowQuestionMessage(ByVal szPrompt As String, ByVal szTitle As String)
        ShowWithIcon(szPrompt, szTitle, imgQuestion)
    End Sub

    Public Sub ShowExclamationMessage(ByVal szPrompt As String, ByVal szTitle As String)
        ShowWithIcon(szPrompt, szTitle, imgExclamation)
    End Sub

    Private Sub ShowWithIcon(ByVal szPrompt As String, ByVal szTitle As String, ByVal icon As PictureBox)
        m_szPrompt = If(szPrompt, "")
        m_szTitle = If(szTitle, "")
        imgIcon.Image = icon.Image
        ShowDialog()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        Close()
    End Sub

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then
            ' VB6 Form_Load
            If m_szDesign.IsEmpty Then m_szDesign = Size
            StartPosition = FormStartPosition.Manual
            SetScreenDefaultValue()
            SetScreenControl()
            m_bolBusy = Application.UseWaitCursor
            Application.UseWaitCursor = False
        Else
            ' VB6 Form_Unload
            Application.UseWaitCursor = m_bolBusy
        End If
        MyBase.OnVisibleChanged(e)
    End Sub

    Private Sub SetScreenDefaultValue()
        Text = m_szTitle
        lblMessage.Text = m_szPrompt
    End Sub

    ''' <summary>Grows the box around the message (never below the designed size) and centres it.</summary>
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
        butOk.Location = New Point((lWidth - butOk.Width) \ 2, lHeight - gap.Height - butOk.Height)

        Dim top As Integer = gap.Height
        If lChdHeight > lblMessage.Height Then top = If(hasTitle, gap.Height, 0) + (lChdHeight - lblMessage.Height) \ 2
        lblMessage.Location = New Point(imgIcon.Right + gap.Width, top)
    End Sub

End Class
