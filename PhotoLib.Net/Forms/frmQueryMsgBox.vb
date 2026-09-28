' Port of Lib\Form\frmQueryMsgBox.frm: the Aqua-styled OK / Cancel question box.
' Form_Load runs whenever the form becomes visible -- see frmMsgBox.
Public Class frmQueryMsgBox

    Private m_szTitle As String = ""
    Private m_szPrompt As String = ""
    Private m_bolResult As Boolean
    Private m_bolBusy As Boolean
    Private m_szDesign As Size = Size.Empty

    Private m_bolDefaultNo As Boolean

    ''' <summary>True when the user pressed 是. <paramref name="defaultNo"/>: 否 has the focus (Enter
    ''' answers 否) -- for questions where 是 can't be undone.</summary>
    Public Function ShowMessage(ByVal szPrompt As String, ByVal szTitle As String, Optional ByVal defaultNo As Boolean = False) As Boolean
        m_bolResult = False
        m_bolDefaultNo = defaultNo
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

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If m_bolDefaultNo Then butExit.Select() Else butOk.Select()
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
        butOk.Location = New Point(CInt((lWidth - butOk.Width * 2.5) / 2), lHeight - gap.Height - butOk.Height)
        butExit.Location = New Point(butOk.Left + CInt(butOk.Width * 1.5), butOk.Top)
    End Sub

End Class
