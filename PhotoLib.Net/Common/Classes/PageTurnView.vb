Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

' 全圖瀏覽換照片的翻頁 (new in the .NET port; frmViewerLarge.PageTurn.vb). Laid over the viewer's
' picture area while the next photo loads and turns in; hidden the rest of the time. Two styles, both
' drawn with plain GDI+ (copies, mirror transform, polygon clips, gradient shadows) -- the same drawing
' as the demo page the user chose from:
'   Book   -- the middle of the picture area is a book's spine: going forward the right half of the old
'             photo turns over the spine and its back is the new photo's left half; going back the other
'             way round. The turning page darkens as it stands up and throws a shadow on the page below.
'   Corner -- the old photo is peeled off from the bottom-right corner along a straight fold that moves
'             to the top left; the lifted part shows the back of the photo paper. Going back plays the
'             same peel backwards, laying the prior photo down again.
' Frames are the whole picture area (black around the photo, as the viewer shows it), 32bppPArgb.
Public Class PageTurnView
    Inherits Control

    Public Enum enumTurnStyle
        tsBook = 0
        tsCorner = 1
    End Enum

    ''' <summary>The turn has ended (or was finished early); the view hides itself.</summary>
    Public Event Finished As EventHandler

    Private m_old As Bitmap          ' the picture before (shown still while the next one loads)
    Private m_new As Bitmap          ' the picture after
    ' big screens (4K) turn at half size: GDI+ draws a 3840 x 2160 frame far too slowly
    Public Const MaxTurnPixels As Integer = 2560 * 1600
    Private m_oldR, m_newR, m_buffer As Bitmap
    Private m_oldBox, m_newBox As RectangleF   ' where the photo is in each frame
    Private m_intDir As Integer = 1
    Private m_style As enumTurnStyle
    Private m_intMs As Integer = 550
    Private ReadOnly m_clock As New Diagnostics.Stopwatch
    Private WithEvents m_timer As New Timer With {.Interval = 10}
    Private WithEvents m_giveUp As New Timer With {.Interval = 3000}

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.Opaque, True)
        BackColor = Color.Black
        Visible = False
    End Sub

    Public ReadOnly Property IsTurning As Boolean
        Get
            Return m_new IsNot Nothing
        End Get
    End Property

    ''' <summary>True while the old picture is shown, waiting for the next one.</summary>
    Public ReadOnly Property HasStill As Boolean
        Get
            Return m_old IsNot Nothing AndAlso m_new Is Nothing
        End Get
    End Property

    ''' <summary>A frame of the area: black, with <paramref name="img"/> at <paramref name="bounds"/>.</summary>
    Public Shared Function Frame(ByVal area As Size, ByVal img As Image, ByVal bounds As Rectangle) As Bitmap
        Dim bmp As New Bitmap(Math.Max(1, area.Width), Math.Max(1, area.Height), PixelFormat.Format32bppPArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Black)
            If img IsNot Nothing Then g.DrawImage(img, bounds)
        End Using
        Return bmp
    End Function

    ''' <summary>Shows <paramref name="still"/> (owned from now on) until <see cref="Turn"/> or
    ''' <see cref="Cancel"/>; gives up by itself after a few seconds.</summary>
    Public Sub ShowStill(ByVal still As Bitmap, ByVal box As RectangleF)
        StopAll()
        m_old = still
        m_oldBox = box
        Visible = True
        BringToFront()
        m_giveUp.Start()
        Invalidate()
        Update()
    End Sub

    ''' <summary>Turns from the still picture to <paramref name="target"/> (owned from now on);
    ''' <paramref name="dir"/> +1 forward, -1 back.</summary>
    Public Sub Turn(ByVal target As Bitmap, ByVal box As RectangleF, ByVal dir As Integer, ByVal style As enumTurnStyle, ByVal ms As Integer)
        m_giveUp.Stop()
        If m_old Is Nothing Then
            target.Dispose()
            Return
        End If
        m_new = target
        m_newBox = box
        If CLng(target.Width) * target.Height > MaxTurnPixels Then
            m_oldR = Half(m_old)
            m_newR = Half(target)
            m_buffer = New Bitmap(m_oldR.Width, m_oldR.Height, PixelFormat.Format32bppPArgb)
        End If
        m_intDir = If(dir < 0, -1, 1)
        m_style = style
        m_intMs = Math.Max(100, ms)
        m_clock.Restart()
        m_timer.Start()
        Visible = True
        BringToFront()
        Invalidate()
    End Sub

    ''' <summary>The picture the turn is going to, taken over by the caller (the turn ends at once) --
    ''' the start of the next turn when the user goes on quickly. Nothing when not turning.</summary>
    Public Function TakeTarget(ByRef box As RectangleF) As Bitmap
        If m_new Is Nothing Then Return Nothing
        Dim t As Bitmap = m_new
        box = m_newBox
        m_new = Nothing
        StopAll()
        Return t
    End Function

    ''' <summary>Hides the view without turning (a video comes next, the viewer was cleared).</summary>
    Public Sub Cancel()
        StopAll()
        Visible = False
    End Sub

    Private Sub StopAll()
        m_timer.Stop()
        m_giveUp.Stop()
        m_old?.Dispose()
        m_old = Nothing
        m_new?.Dispose()
        m_new = Nothing
        For Each b In {m_oldR, m_newR, m_buffer}
            b?.Dispose()
        Next
        m_oldR = Nothing : m_newR = Nothing : m_buffer = Nothing
    End Sub

    Private Sub m_giveUp_Tick(sender As Object, e As EventArgs) Handles m_giveUp.Tick
        Cancel()
    End Sub

    Private Sub m_timer_Tick(sender As Object, e As EventArgs) Handles m_timer.Tick
        If m_clock.ElapsedMilliseconds >= m_intMs Then
            StopAll()
            Visible = False
            RaiseEvent Finished(Me, EventArgs.Empty)
            Return
        End If
        Invalidate()
        Update()
    End Sub

    ''' <summary>A half-size copy of a frame (for turning on big screens).</summary>
    Public Shared Function Half(ByVal frame As Bitmap) As Bitmap
        Dim bmp As New Bitmap(Math.Max(1, frame.Width \ 2), Math.Max(1, frame.Height \ 2), PixelFormat.Format32bppPArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.InterpolationMode = InterpolationMode.Bilinear
            g.PixelOffsetMode = PixelOffsetMode.Half
            g.DrawImage(frame, New Rectangle(0, 0, bmp.Width, bmp.Height))
        End Using
        Return bmp
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        If m_old Is Nothing Then
            g.Clear(Color.Black)
            Return
        End If
        If m_new Is Nothing Then
            Blit(g, m_old)
            Return
        End If
        Dim t As Double = Math.Min(1.0, m_clock.ElapsedMilliseconds / CDbl(m_intMs))
        If m_buffer IsNot Nothing Then
            Using bg As Graphics = Graphics.FromImage(m_buffer)
                Render(bg, m_oldR, m_newR, m_intDir, m_style, Ease(t), HalfBox(m_oldBox), HalfBox(m_newBox))
            End Using
            g.InterpolationMode = InterpolationMode.NearestNeighbor   ' exactly 2x: cheap, and the page is moving
            g.PixelOffsetMode = PixelOffsetMode.Half
            g.DrawImage(m_buffer, ClientRectangle)
        Else
            Render(g, m_old, m_new, m_intDir, m_style, Ease(t), m_oldBox, m_newBox)
        End If
    End Sub

    ''' <summary>A frame 1:1 at the top left (DrawImageUnscaled would scale by the bitmap's DPI).</summary>
    Private Shared Sub Blit(ByVal g As Graphics, ByVal img As Bitmap)
        g.DrawImage(img, New Rectangle(0, 0, img.Width, img.Height), New Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel)
    End Sub

    Private Shared Function HalfBox(ByVal r As RectangleF) As RectangleF
        Return New RectangleF(r.X / 2, r.Y / 2, r.Width / 2, r.Height / 2)
    End Function

    Public Shared Function Ease(ByVal t As Double) As Double
        Return If(t < 0.5, 4 * t * t * t, 1 - Math.Pow(-2 * t + 2, 3) / 2)
    End Function

    ''' <summary>One frame of a turn at <paramref name="t"/> (0..1, eased). Shared: the tests time it.</summary>
    Public Shared Sub Render(ByVal g As Graphics, ByVal oldP As Bitmap, ByVal newP As Bitmap, ByVal dir As Integer,
                             ByVal style As enumTurnStyle, ByVal t As Double,
                             Optional ByVal oldBox As RectangleF = Nothing, Optional ByVal newBox As RectangleF = Nothing)
        g.CompositingQuality = CompositingQuality.HighSpeed
        g.InterpolationMode = InterpolationMode.Bilinear
        g.PixelOffsetMode = PixelOffsetMode.HighSpeed
        g.SmoothingMode = SmoothingMode.None
        If style = enumTurnStyle.tsBook Then
            RenderBook(g, oldP, newP, dir, t)
        ElseIf dir > 0 Then
            RenderCorner(g, oldP, newP, oldBox, t)          ' the old photo peels off
        Else
            RenderCorner(g, newP, oldP, newBox, 1 - t)      ' the prior photo is laid back down
        End If
    End Sub

    '==================================================================================================
    ' 整頁翻
    '==================================================================================================
    Private Shared Sub RenderBook(ByVal g As Graphics, ByVal oldP As Bitmap, ByVal newP As Bitmap, ByVal dir As Integer, ByVal t As Double)
        Dim pageW As Integer = oldP.Width, pageH As Integer = oldP.Height
        Dim half As Integer = pageW \ 2, cx As Integer = half
        g.Clear(Color.Black)
        ' flat pages: forward -- old left half stays, new right half is underneath; back -- the mirror
        Dim stayX As Integer = If(dir > 0, 0, cx), underX As Integer = If(dir > 0, cx, 0)
        Dim stayW As Integer = If(stayX = 0, half, pageW - cx), underW As Integer = If(underX = 0, half, pageW - cx)
        g.DrawImage(oldP, New Rectangle(stayX, 0, stayW, pageH), New Rectangle(stayX, 0, stayW, pageH), GraphicsUnit.Pixel)
        g.DrawImage(newP, New Rectangle(underX, 0, underW, pageH), New Rectangle(underX, 0, underW, pageH), GraphicsUnit.Pixel)

        Dim ang As Double = t * Math.PI, c As Double = Math.Cos(ang), lift As Double = Math.Sin(ang)
        Dim frontSide As Boolean = ang < Math.PI / 2
        Dim w As Single = CSng(Math.Abs(c) * half)
        Dim grow As Double = 1 + 0.06 * lift
        Dim h As Single = CSng(pageH * grow), y As Single = (pageH - h) / 2

        ' shadow on the page below the turning one
        Dim shadowX As Integer = If(frontSide, underX, stayX)
        Dim reach As Single = w + CSng(80 * lift)
        Dim toward As Integer = If(frontSide, dir, -dir)
        FillBand(g, New Rectangle(shadowX, 0, half, pageH), cx, cx + toward * Math.Max(1.0F, reach),
                 Color.FromArgb(CInt(140 * lift), 0, 0, 0), Color.FromArgb(0, 0, 0, 0))

        If w > 0.5F Then
            Dim src As Bitmap, sx As Integer, dx As Single
            If frontSide Then
                src = oldP : sx = underX : dx = If(dir > 0, cx, cx - w)
            Else
                src = newP : sx = stayX : dx = If(dir > 0, cx - w, cx)
            End If
            Dim sw As Integer = If(sx = 0, half, pageW - cx)
            g.DrawImage(src, New RectangleF(dx, y, w, h), New RectangleF(sx, 0, sw, pageH), GraphicsUnit.Pixel)
            ' the turning page: dark near the spine the more it stands up, a little light on its edge
            Dim far As Single = If(dx < cx - 0.5F, dx, dx + w)
            Dim dark As Integer = CInt(255 * (0.12 + 0.5 * (1 - Math.Abs(c))))
            If Math.Abs(far - cx) >= 1 Then
                Using b As New LinearGradientBrush(New PointF(cx, 0), New PointF(far, 0), Color.Black, Color.Black)
                    Dim blend As New ColorBlend(3)
                    blend.Colors = {Color.FromArgb(dark, 0, 0, 0), Color.FromArgb(CInt(dark * 0.35), 0, 0, 0), Color.FromArgb(CInt(26 * lift), 255, 255, 255)}
                    blend.Positions = {0.0F, 0.7F, 1.0F}
                    b.InterpolationColors = blend
                    b.WrapMode = WrapMode.TileFlipX
                    g.FillRectangle(b, New RectangleF(dx, y, w, h))
                End Using
            End If
        End If

        ' the spine
        Using b As New LinearGradientBrush(New Point(cx - 10, 0), New Point(cx + 10, 0), Color.Black, Color.Black)
            Dim blend As New ColorBlend(3)
            blend.Colors = {Color.FromArgb(0, 0, 0, 0), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(0, 0, 0, 0)}
            blend.Positions = {0.0F, 0.5F, 1.0F}
            b.InterpolationColors = blend
            g.FillRectangle(b, cx - 10, 0, 20, pageH)
        End Using
    End Sub

    ''' <summary>A horizontal gradient from <paramref name="x0"/> (<paramref name="c0"/>) to
    ''' <paramref name="x1"/> (<paramref name="c1"/>), filled only over its own span inside <paramref name="area"/>.</summary>
    Private Shared Sub FillBand(ByVal g As Graphics, ByVal area As Rectangle, ByVal x0 As Single, ByVal x1 As Single, ByVal c0 As Color, ByVal c1 As Color)
        If c0.A = 0 OrElse Math.Abs(x1 - x0) < 1 Then Return
        Dim band As New RectangleF(Math.Min(x0, x1), area.Y, Math.Abs(x1 - x0), area.Height)
        band.Intersect(area)
        If band.Width <= 0 Then Return
        Using b As New LinearGradientBrush(New PointF(x0, 0), New PointF(x1, 0), c0, c1)
            g.FillRectangle(b, band)
        End Using
    End Sub

    '==================================================================================================
    ' 翻頁角
    '==================================================================================================
    ''' <summary><paramref name="top"/> peeled off <paramref name="under"/>: t = 0 flat, 1 gone.</summary>
    ''' <paramref name="box"/>: where the photo is in <paramref name="top"/> -- only the photo peels off,
    ''' not the black around it (that fades into the photo underneath instead).</summary>
    Private Shared Sub RenderCorner(ByVal g As Graphics, ByVal top As Bitmap, ByVal under As Bitmap, ByVal box As RectangleF, ByVal t As Double)
        If box.Width < 2 OrElse box.Height < 2 Then box = New RectangleF(0, 0, top.Width, top.Height)
        Dim X0 As Double = box.X, Y0 As Double = box.Y
        Dim W As Double = box.Width, H As Double = box.Height
        If t <= 0.001 Then
            Blit(g, top)
            Return
        End If
        If t >= 0.999 Then
            Blit(g, under)
            Return
        End If
        ' the corner C goes to P; the fold is the perpendicular bisector of C-P
        Const e As Double = 40
        Dim Cx As Double = X0 + W, Cy As Double = Y0 + H
        Dim Px As Double = Cx - t * (2 * W + e)
        Dim Py As Double = Cy - Math.Pow(t, 1.35) * (2 * H + e)   ' the corner rises a little after it starts moving left
        Dim Mx As Double = (Cx + Px) / 2, My As Double = (Cy + Py) / 2
        Dim nx As Double = Cx - Px, ny As Double = Cy - Py
        Dim nl As Double = Math.Sqrt(nx * nx + ny * ny)
        nx /= nl : ny /= nl                                       ' towards the corner: the folded side
        Dim side As Func(Of Double, Double, Double) = Function(x, yy) (x - Mx) * nx + (yy - My) * ny

        Dim rect As PointF() = {New PointF(box.Left, box.Top), New PointF(box.Right, box.Top), New PointF(box.Right, box.Bottom), New PointF(box.Left, box.Bottom)}
        Dim len As Single = CSng(top.Width + top.Height)
        Dim flat As PointF() = ClipHalf(rect, side, False)
        Dim lifted As PointF() = ClipHalf(rect, side, True)
        Dim flap As PointF() = lifted.Select(Function(p)
                                                 Dim d As Double = side(p.X, p.Y)
                                                 Return New PointF(CSng(p.X - 2 * d * nx), CSng(p.Y - 2 * d * ny))
                                             End Function).ToArray()
        Dim u As New PointF(CSng(-ny), CSng(nx))   ' along the fold

        ' 1. the photo underneath
        Blit(g, under)
        ' 2. the top frame except the lifted part; around the photo its black fades into the one underneath
        Dim state0 As GraphicsState = g.Save()
        If lifted.Length >= 3 Then
            Using p As New GraphicsPath
                p.AddPolygon(lifted)
                g.SetClip(p, CombineMode.Exclude)
            End Using
        End If
        Using outside As New Region(New Rectangle(0, 0, top.Width, top.Height))
            outside.Exclude(box)
            Using ia As New ImageAttributes
                ia.SetColorMatrix(New ColorMatrix With {.Matrix33 = CSng(1 - t)})
                Dim inPhoto As Region = g.Clip.Clone()
                inPhoto.Intersect(box)
                g.Clip = inPhoto
                Blit(g, top)
                g.Restore(state0)
                state0 = g.Save()
                g.SetClip(outside, CombineMode.Replace)
                g.DrawImage(top, New Rectangle(0, 0, top.Width, top.Height), 0, 0, top.Width, top.Height, GraphicsUnit.Pixel, ia)
                inPhoto.Dispose()
            End Using
        End Using
        g.Restore(state0)
        If flat.Length >= 3 Then   ' darker near the fold, where the paper starts to rise
            Dim state As GraphicsState = g.Save()
            Using p As New GraphicsPath
                p.AddPolygon(flat)
                g.SetClip(p)
            End Using
            FillStrip(g, Mx, My, -nx, -ny, u, 60, len, Color.FromArgb(72, 0, 0, 0))
            g.Restore(state)
        End If
        ' 3. the shadow the lifted part throws on the photo underneath
        If lifted.Length >= 3 Then
            Dim state As GraphicsState = g.Save()
            Using p As New GraphicsPath
                p.AddPolygon(lifted)
                g.SetClip(p)
            End Using
            FillStrip(g, Mx, My, nx, ny, u, 140, len, Color.FromArgb(140, 0, 0, 0))
            g.Restore(state)
        End If
        ' 4. the flap: the back of the photo paper, the picture showing through faintly (mirrored)
        If flap.Length >= 3 Then
            Dim state As GraphicsState = g.Save()
            Using p As New GraphicsPath
                p.AddPolygon(flap)
                g.SetClip(p)
                Dim k As Double = 2 * (Mx * nx + My * ny)
                Using m As New Matrix(CSng(1 - 2 * nx * nx), CSng(-2 * nx * ny), CSng(-2 * nx * ny), CSng(1 - 2 * ny * ny), CSng(k * nx), CSng(k * ny))
                    g.Transform = m
                    Blit(g, top)
                    g.ResetTransform()
                End Using
                Using paper As New SolidBrush(Color.FromArgb(236, 244, 241, 234))
                    g.FillPath(paper, p)
                End Using
                ' curl light: a little dark at the fold, a highlight, darker again at the far edge
                FillStrip(g, Mx, My, -nx, -ny, u, 320, len, Color.FromArgb(56, 0, 0, 0), Color.FromArgb(46, 255, 255, 255), Color.FromArgb(22, 0, 0, 0), Color.FromArgb(0, 0, 0, 0))
                g.Restore(state)
                Using pen As New Pen(Color.FromArgb(64, 0, 0, 0))
                    g.DrawPolygon(pen, flap)
                End Using
            End Using
        End If
    End Sub

    ''' <summary>A gradient strip starting on the fold line (through M) and reaching
    ''' <paramref name="dist"/> in direction (dx, dy): <paramref name="colors"/> from the fold outwards,
    ''' the last one fading to nothing when only one is given.</summary>
    Private Shared Sub FillStrip(ByVal g As Graphics, ByVal Mx As Double, ByVal My As Double, ByVal dx As Double, ByVal dy As Double,
                                 ByVal u As PointF, ByVal dist As Single, ByVal len As Single, ParamArray colors As Color())
        Dim a As New PointF(CSng(Mx), CSng(My)), b As New PointF(CSng(Mx + dx * dist), CSng(My + dy * dist))
        Dim quad As PointF() = {New PointF(a.X - u.X * len, a.Y - u.Y * len), New PointF(a.X + u.X * len, a.Y + u.Y * len),
                                New PointF(b.X + u.X * len, b.Y + u.Y * len), New PointF(b.X - u.X * len, b.Y - u.Y * len)}
        Using br As New LinearGradientBrush(a, b, colors(0), Color.FromArgb(0, colors(0)))
            If colors.Length > 1 Then
                Dim blend As New ColorBlend(colors.Length)
                blend.Colors = colors
                blend.Positions = Enumerable.Range(0, colors.Length).Select(Function(i) i / CSng(colors.Length - 1)).ToArray()
                br.InterpolationColors = blend
            End If
            g.FillPolygon(br, quad)
        End Using
    End Sub

    ''' <summary>The part of a convex polygon on one side of the fold (Sutherland-Hodgman, one edge).</summary>
    Private Shared Function ClipHalf(ByVal poly As PointF(), ByVal side As Func(Of Double, Double, Double), ByVal positive As Boolean) As PointF()
        Dim out As New List(Of PointF)
        For i = 0 To poly.Length - 1
            Dim a As PointF = poly(i), b As PointF = poly((i + 1) Mod poly.Length)
            Dim sa As Double = side(a.X, a.Y), sb As Double = side(b.X, b.Y)
            Dim ina As Boolean = If(positive, sa > 0, sa <= 0), inb As Boolean = If(positive, sb > 0, sb <= 0)
            If ina Then out.Add(a)
            If ina <> inb Then
                Dim k As Double = sa / (sa - sb)
                out.Add(New PointF(CSng(a.X + (b.X - a.X) * k), CSng(a.Y + (b.Y - a.Y) * k)))
            End If
        Next
        Return out.ToArray()
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            StopAll()
            m_timer.Dispose()
            m_giveUp.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
