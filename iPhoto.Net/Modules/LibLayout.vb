' Keeps a row of controls spread across a panel as it grows: each child keeps the horizontal centre
' it has in the designer, as a fraction of the panel's designer width (the main window's tool bar,
' whose buttons would otherwise all stay bunched up on the left when the window is maximised).
' A caption registered with AddCaption sits centred under its picture, and each picture + caption
' pair is centred in the panel's height (a PngButton's hover-zoom headroom above its image is left
' out, so the icon itself is centred); the other children (the separators) are centred too.
' The positions themselves stay in the designer; this only scales and centres them.
Friend Class ProportionalLayout

    Private ReadOnly m_panel As Control
    Private ReadOnly m_centres As New Dictionary(Of Control, Double)
    Private ReadOnly m_captions As New Dictionary(Of Control, Control)   ' picture -> caption
    Private ReadOnly m_gaps As New Dictionary(Of Control, Integer)       ' picture -> designer gap to caption
    Private ReadOnly m_designTops As New Dictionary(Of Control, Integer) ' as in the designer, before any arranging

    ''' <summary>Records the children's current (designer) positions; call before the panel is resized.</summary>
    Public Sub New(ByVal panel As Control)
        m_panel = panel
        Dim w As Integer = Math.Max(1, panel.ClientSize.Width)
        For Each c As Control In panel.Controls
            m_centres(c) = (c.Left + c.Width / 2) / w
            m_designTops(c) = c.Top
        Next
        AddHandler panel.Resize, AddressOf Arrange
        AddHandler panel.Disposed, Sub() RemoveHandler panel.Resize, AddressOf Arrange
    End Sub

    ''' <summary>Puts <paramref name="caption"/> centred under <paramref name="picture"/> (keeping the
    ''' designer gap between them) instead of scaling it on its own.</summary>
    Public Sub AddCaption(ByVal picture As Control, ByVal caption As Control)
        m_centres.Remove(caption)
        m_captions(picture) = caption
        m_gaps(picture) = Math.Max(0, m_designTops(caption) - (m_designTops(picture) + picture.Height))
        AddHandler caption.SizeChanged, AddressOf Arrange   ' AutoSize labels change width with their text
        Arrange(Nothing, EventArgs.Empty)
    End Sub

    Private Sub Arrange(sender As Object, e As EventArgs)
        Dim w As Integer = m_panel.ClientSize.Width, h As Integer = m_panel.ClientSize.Height
        If w <= 0 OrElse h <= 0 Then Return
        ' one row height for every picture + caption pair (the tallest), so the pictures stay on one
        ' line even when captions of different scripts come out a few pixels taller or shorter
        Dim rowH As Integer = 0
        For Each kv In m_captions
            rowH = Math.Max(rowH, kv.Key.Height - Headroom(kv.Key) + m_gaps(kv.Key) + kv.Value.Height)
        Next
        m_panel.SuspendLayout()
        For Each kv In m_centres
            Dim c As Control = kv.Key
            Dim left As Integer = CInt(Math.Round(kv.Value * w - c.Width / 2))
            Dim caption As Control = Nothing
            If m_captions.TryGetValue(c, caption) Then
                Dim gap As Integer = m_gaps(c)
                ' centre what is visible at rest; the headroom above it may not leave the panel either
                Dim top As Integer = Math.Max(0, (h - rowH) \ 2 - Headroom(c))
                c.SetBounds(left, top, 0, 0, BoundsSpecified.Location)
                caption.SetBounds(c.Left + (c.Width - caption.Width) \ 2, c.Bottom + gap, 0, 0, BoundsSpecified.Location)
            Else
                c.SetBounds(left, (h - c.Height) \ 2, 0, 0, BoundsSpecified.Location)
            End If
        Next
        m_panel.ResumeLayout(False)
        m_panel.Invalidate(True)
    End Sub

    ''' <summary>The room a PngButton keeps above its image for the upward hover zoom (the image rests
    ''' at the bottom of the button); 0 for anything else.</summary>
    Private Shared Function Headroom(ByVal c As Control) As Integer
        Dim b As Aqua.PngButton = TryCast(c, Aqua.PngButton)
        If b Is Nothing OrElse b.Image Is Nothing OrElse b.HoverZoom <= 0 OrElse
           b.HoverZoomDirection <> Aqua.PngZoomDirection.Up Then Return 0
        Return Math.Max(0, b.Height - b.Image.Height)
    End Function

End Class
