' Port of Lib\Form\frmLogo.frm: the splash picture; its magenta background is see-through (designer
' TransparencyKey, VB6 cut a window region from the picture with Quartz.Region) and Shape1 blinks
' every Timer1 tick (VB6 showed it for 100 ms).
Public Class frmLogo

    ''' <summary>The picture once, at its own size, on the see-through colour. The designer's
    ''' BackgroundImageLayout (Tile) and white BackColor showed a sliver of the picture again -- a bit of
    ''' the orange breast at the right of the beak -- whenever the window came out wider than the picture
    ''' (a borderless window can be widened by Windows), and white where it came out taller.</summary>
    Private Sub frmLogo_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundImageLayout = ImageLayout.None
        BackColor = TransparencyKey
        If BackgroundImage IsNot Nothing Then
            MinimumSize = Size.Empty
            ClientSize = BackgroundImage.Size
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Shape1.Visible = True
        Shape1.Refresh()
        Darwin.Wait(100)
        Shape1.Visible = False
    End Sub

End Class
