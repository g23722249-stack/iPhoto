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
    ''' <summary>Height of iForm's title bar: with a title the message and icon go below it.</summary>
    Private Const TitleBarHeight As Integer = 23

    Private Sub SetScreenControl()
        Dim gap As Size = lblInterval.Size
        Dim hasTitle As Boolean = Text.Trim() <> ""
        Dim titleH As Integer = If(hasTitle, TitleBarHeight, 0)
        Dim contentH As Integer = Math.Max(lblMessage.Height, imgIcon.Height)
        Dim lWidth As Integer = Math.Max(gap.Width + imgIcon.Width + gap.Width + lblMessage.Width + gap.Width, m_szDesign.Width)
        Dim lHeight As Integer = Math.Max(titleH + gap.Height + contentH + gap.Height + butOk.Height + gap.Height, m_szDesign.Height)
        ' the screen it is opening on (the app moves dialogs to its main window's screen), not the primary one
        Dim scr As Rectangle = Screen.FromControl(Me).Bounds
        Bounds = New Rectangle(scr.Left + (scr.Width - lWidth) \ 2, scr.Top + (scr.Height - lHeight) \ 2, lWidth, lHeight)

        ' the message and the icon centred between the title bar and the button(s)
        Dim areaTop As Integer = titleH + gap.Height \ 2
        Dim areaH As Integer = lHeight - gap.Height - butOk.Height - gap.Height \ 2 - areaTop
        imgIcon.Location = New Point(gap.Width, areaTop + Math.Max(0, (areaH - imgIcon.Height) \ 2))
        lblMessage.Location = New Point(imgIcon.Right + gap.Width, areaTop + Math.Max(0, (areaH - lblMessage.Height) \ 2))
        butOk.Location = New Point((lWidth - butOk.Width) \ 2, lHeight - gap.Height - butOk.Height)
    End Sub

End Class
