' The bottom row of 全圖瀏覽 in photo mode, laid out to the window's width (new in the .NET port): the
' designer placed three fixed pieces for a 1280-wide screen -- picPage 0..299, the tool bar 300..971,
' the brightness / contrast / undo panel from 972, 397 wide -- so a wider screen showed black to the
' right of them and a 1280 one cut off the undo panel. Now the file panel keeps its width on the left,
' the undo panel keeps what its contents need on the right, and the tool bar (which spreads its
' buttons over its width) takes everything in between. The video bar already stretches (designer anchor).
Partial Class frmViewerLarge

    Private Const BarGap As Integer = 1            ' the thin black line between the pieces
    Private Const EditPanelWidth As Integer = 372  ' picImageBar2: sliders, undo, the selection point label

    Private Sub BottomBar_Layout(sender As Object, e As EventArgs) Handles MyBase.Load, MyBase.Resize
        LayoutBottomBar()
    End Sub

    Private Sub LayoutBottomBar()
        If picPage Is Nothing OrElse picImageBar1 Is Nothing OrElse picImageBar2 Is Nothing Then Return
        Dim w As Integer = ContentWidth   ' the window less the 地點 strip (frmViewerLarge.Strip.vb)
        If w <= 0 Then Return
        Dim y As Integer = picImageBar1.Top, h As Integer = picImageBar1.Height
        Dim editLeft As Integer = Math.Max(picPage.Right + BarGap + 200, w - EditPanelWidth)
        picImageBar2.Bounds = New Rectangle(editLeft, picImageBar2.Top, Math.Max(EditPanelWidth, w - editLeft), picImageBar2.Height)
        Dim toolLeft As Integer = picPage.Right + BarGap
        picImageBar1.Bounds = New Rectangle(toolLeft, y, Math.Max(200, editLeft - BarGap - toolLeft), h)
        ' 「無法復原」 / 「復原：10」 did not fit the ported 80 pixels
        If lblUndo IsNot Nothing AndAlso lblUndo.Width < 96 Then lblUndo.Width = 96
    End Sub

End Class
