' Port of Lib\Form\frmLoading.frm: a "please wait" box with a running bar, shown modeless while a long
' job runs on the UI thread (stays on top -- designer TopMost).
Public Class frmLoading

    Private m_lngColor As Aqua.ColorConstants

    Public Sub StartLoading(ByVal Message As String, Optional ByVal Color As Aqua.ColorConstants = Aqua.ColorConstants.Blue)
        m_lngColor = Color
        lblMessage.Text = Message
        Loading1.Color = m_lngColor
        Loading1.Play = True
        If Not Visible Then Show()
        Refresh()
        Application.DoEvents()
    End Sub

    Public WriteOnly Property Message As String
        Set(value As String)
            Loading1.Color = m_lngColor
            Loading1.Play = True
            lblMessage.Text = value
            Refresh()
            Application.DoEvents()
        End Set
    End Property

    ''' <summary>VB6 "Unload Me": the default instance is kept, just hidden.</summary>
    Public Sub EndLoading()
        Loading1.Play = False
        Hide()
    End Sub

End Class
