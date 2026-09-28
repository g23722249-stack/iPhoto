' Port of Lib\Form\frmLogo.frm: the splash picture; its magenta background is see-through (designer
' TransparencyKey, VB6 cut a window region from the picture with Quartz.Region) and Shape1 blinks
' every Timer1 tick (VB6 showed it for 100 ms).
Public Class frmLogo

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Shape1.Visible = True
        Shape1.Refresh()
        Darwin.Wait(100)
        Shape1.Visible = False
    End Sub

End Class
