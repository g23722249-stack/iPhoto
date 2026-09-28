Imports System.Drawing.Drawing2D

' 面孔功能使用說明 (new in the .NET port): a guide to face recognition with worked examples, opened from
' the face wall, 設定 › 面孔 and the viewer's face bar.
'
'   frmFaceGuide.ShowGuide(Me, "wall")                   ' from the main window
'   frmFaceGuide.ShowGuide(Me, "viewer", stayWithCaller:=True)   ' from the full-screen viewer
'
' Not modal, so the guide can stay open while you follow it in the main window (it doesn't dim the main
' window either -- ModalDimmer only reacts to modal dialogs). One guide at a time: asking again shows the
' asked topic in the open one. Owned by the main window (the top of the caller's owner chain) so it
' outlives a dialog it was opened from, and floats over the main window; the viewer keeps it for itself
' (stayWithCaller) because it covers the main window.
' Left: the table of contents (FaceGuideContent.Topics by group); right: the topic, drawn by GuideView
' (text, numbered steps, tips, figures from FaceGuideFigures, links to other topics).
Public Class frmFaceGuide
    Inherits Form

    Private Shared s_guide As frmFaceGuide

    Private ReadOnly m_toc As New TocView
    Private ReadOnly m_view As New GuideView

    ''' <summary>Shows the guide at topic <paramref name="topicKey"/> ("" = the first).</summary>
    Public Shared Sub ShowGuide(ByVal caller As Form, Optional ByVal topicKey As String = "", Optional ByVal stayWithCaller As Boolean = False)
        Dim owner As Form = caller
        If owner IsNot Nothing AndAlso Not stayWithCaller Then
            While owner.Owner IsNot Nothing
                owner = owner.Owner
            End While
        End If
        If s_guide Is Nothing OrElse s_guide.IsDisposed Then
            s_guide = New frmFaceGuide
            s_guide.ShowTopic(topicKey)
            s_guide.PlaceOver(If(owner, caller))
            If owner IsNot Nothing Then s_guide.Show(owner) Else s_guide.Show()
        Else
            If s_guide.Owner IsNot owner Then s_guide.Owner = owner
            s_guide.ShowTopic(topicKey)
            If s_guide.WindowState = FormWindowState.Minimized Then s_guide.WindowState = FormWindowState.Normal
            s_guide.Activate()
        End If
    End Sub

    Private Sub New()
        Text = "面孔功能使用說明"
        Font = New Font("Microsoft JhengHei UI", 9.75F)
        BackColor = Color.White
        ShowInTaskbar = False
        ShowIcon = False
        MinimizeBox = False
        StartPosition = FormStartPosition.Manual
        KeyPreview = True
        Size = New Size(1000, 740)
        MinimumSize = New Size(720, 480)

        m_toc.Dock = DockStyle.Left
        m_toc.Width = 236
        m_view.Dock = DockStyle.Fill
        Controls.Add(m_view)
        Controls.Add(m_toc)

        AddHandler m_toc.TopicPicked, Sub(t) ShowTopic(t.Key)
        AddHandler m_view.LinkClicked, Sub(key) ShowTopic(key)
    End Sub

    ''' <summary>Centred over <paramref name="f"/>, inside its screen's working area.</summary>
    Private Sub PlaceOver(ByVal f As Form)
        Dim wa As Rectangle = If(f IsNot Nothing, Screen.FromControl(f).WorkingArea, Screen.PrimaryScreen.WorkingArea)
        Dim w As Integer = Math.Min(Width, wa.Width - 40), h As Integer = Math.Min(Height, wa.Height - 40)
        Dim c As Point = If(f IsNot Nothing AndAlso f.WindowState <> FormWindowState.Minimized,
                            New Point(f.Left + f.Width \ 2, f.Top + f.Height \ 2), New Point(wa.Left + wa.Width \ 2, wa.Top + wa.Height \ 2))
        Bounds = New Rectangle(Math.Max(wa.Left, Math.Min(wa.Right - w, c.X - w \ 2)), Math.Max(wa.Top, Math.Min(wa.Bottom - h, c.Y - h \ 2)), w, h)
    End Sub

    Public Sub ShowTopic(ByVal key As String)
        Dim t As GuideTopic = If(FaceGuideContent.Topic(key), FaceGuideContent.Topics(0))
        m_toc.Current = t
        m_view.Topic = t
        m_view.Focus()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Select Case e.KeyCode
            Case Keys.Escape
                Close()
            Case Keys.Up, Keys.Down
                If e.Control OrElse e.Alt Then
                    Dim i As Integer = FaceGuideContent.Topics.IndexOf(m_toc.Current) + If(e.KeyCode = Keys.Down, 1, -1)
                    If i >= 0 AndAlso i < FaceGuideContent.Topics.Count Then ShowTopic(FaceGuideContent.Topics(i).Key)
                    e.Handled = True
                End If
        End Select
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        If s_guide Is Me Then s_guide = Nothing
        MyBase.OnFormClosed(e)
    End Sub

    '==================================================================================================
    ' Colours and fonts shared by the two views
    '==================================================================================================
    Private NotInheritable Class Look
        Public Shared ReadOnly Ink As Color = Color.FromArgb(27, 35, 48)
        Public Shared ReadOnly Body As Color = Color.FromArgb(52, 62, 78)
        Public Shared ReadOnly Muted As Color = Color.FromArgb(112, 122, 136)
        Public Shared ReadOnly Accent As Color = Color.FromArgb(42, 116, 208)
        Public Shared ReadOnly Rule As Color = Color.FromArgb(222, 227, 233)
        Public Shared ReadOnly TocBack As Color = Color.FromArgb(243, 245, 248)
        Public Shared ReadOnly TocHot As Color = Color.FromArgb(232, 237, 244)
        Public Shared ReadOnly TocSel As Color = Color.FromArgb(218, 231, 249)
        Public Shared ReadOnly TipBack As Color = Color.FromArgb(234, 243, 253)
        Public Shared ReadOnly NoteBack As Color = Color.FromArgb(254, 243, 230)
        Public Shared ReadOnly NoteBar As Color = Color.FromArgb(214, 120, 30)
        Public Const FontName As String = "Microsoft JhengHei UI"
    End Class

    '==================================================================================================
    ' Table of contents
    '==================================================================================================
    Private Class TocView
        Inherits Control

        Public Event TopicPicked(ByVal t As GuideTopic)

        Private Const HeaderHeight As Integer = 38
        Private Const RowHeight As Integer = 34

        Private ReadOnly m_rows As New List(Of (Header As String, Topic As GuideTopic, Top As Integer))
        Private ReadOnly m_headFont As New Font(Look.FontName, 9.0F, FontStyle.Bold)
        Private ReadOnly m_rowFont As New Font(Look.FontName, 10.5F)
        Private ReadOnly m_titleFont As New Font(Look.FontName, 13.0F, FontStyle.Bold)
        Private m_current As GuideTopic
        Private m_hot As GuideTopic
        Private m_scroll As Integer
        Private m_height As Integer

        Public Sub New()
            SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
            BackColor = Look.TocBack
            Cursor = Cursors.Hand
            Dim y As Integer = 64
            Dim group As String = Nothing
            For Each t In FaceGuideContent.Topics
                If t.Group <> group Then
                    group = t.Group
                    m_rows.Add((group, Nothing, y))
                    y += HeaderHeight
                End If
                m_rows.Add((Nothing, t, y))
                y += RowHeight
            Next
            m_height = y + 16
        End Sub

        Public Property Current As GuideTopic
            Get
                Return m_current
            End Get
            Set(value As GuideTopic)
                m_current = value
                ' keep it in view
                Dim row = m_rows.FirstOrDefault(Function(r) r.Topic Is value)
                If row.Topic IsNot Nothing Then
                    If row.Top - m_scroll < 56 Then m_scroll = Math.Max(0, row.Top - 64)
                    If row.Top + RowHeight - m_scroll > Height Then m_scroll = row.Top + RowHeight - Height + 8
                End If
                Invalidate()
            End Set
        End Property

        Private Function RowAt(ByVal y As Integer) As GuideTopic
            y += m_scroll
            For Each r In m_rows
                If r.Topic IsNot Nothing AndAlso y >= r.Top AndAlso y < r.Top + RowHeight Then Return r.Topic
            Next
            Return Nothing
        End Function

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            Dim t As GuideTopic = RowAt(e.Y)
            If t IsNot m_hot Then
                m_hot = t
                Cursor = If(t Is Nothing, Cursors.Default, Cursors.Hand)
                Invalidate()
            End If
        End Sub

        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)
            m_hot = Nothing
            Invalidate()
        End Sub

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            Dim t As GuideTopic = RowAt(e.Y)
            If t IsNot Nothing Then RaiseEvent TopicPicked(t)
        End Sub

        Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
            MyBase.OnMouseWheel(e)
            m_scroll = Math.Max(0, Math.Min(Math.Max(0, m_height - Height), m_scroll - e.Delta \ 3))
            Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            TextRenderer.DrawText(g, "面孔功能", m_titleFont, New Rectangle(20, 18 - m_scroll, Width - 30, 28), Look.Ink, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
            For Each r In m_rows
                Dim y As Integer = r.Top - m_scroll
                If y > Height OrElse y + RowHeight < 0 Then Continue For
                If r.Topic Is Nothing Then
                    TextRenderer.DrawText(g, r.Header, m_headFont, New Rectangle(20, y + 12, Width - 30, HeaderHeight - 12), Look.Muted, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
                    Continue For
                End If
                Dim rr As New Rectangle(8, y + 2, Width - 16, RowHeight - 4)
                If r.Topic Is m_current Then
                    Using b As New SolidBrush(Look.TocSel), p As GraphicsPath = RoundRect(rr, 6)
                        g.SmoothingMode = SmoothingMode.AntiAlias
                        g.FillPath(b, p)
                    End Using
                    Using b As New SolidBrush(Look.Accent)
                        g.FillRectangle(b, rr.X, rr.Y + 7, 3, rr.Height - 14)
                    End Using
                ElseIf r.Topic Is m_hot Then
                    Using b As New SolidBrush(Look.TocHot), p As GraphicsPath = RoundRect(rr, 6)
                        g.SmoothingMode = SmoothingMode.AntiAlias
                        g.FillPath(b, p)
                    End Using
                End If
                TextRenderer.DrawText(g, r.Topic.Title, m_rowFont, New Rectangle(rr.X + 14, rr.Y, rr.Width - 18, rr.Height),
                                      If(r.Topic Is m_current, Look.Accent, Look.Body), TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            Next
            Using p As New Pen(Look.Rule)
                g.DrawLine(p, Width - 1, 0, Width - 1, Height)
            End Using
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                m_headFont.Dispose()
                m_rowFont.Dispose()
                m_titleFont.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

    Private Shared Function RoundRect(ByVal r As Rectangle, ByVal radius As Integer) As GraphicsPath
        Dim p As New GraphicsPath
        Dim d As Integer = radius * 2
        p.AddArc(r.X, r.Y, d, d, 180, 90)
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        p.CloseFigure()
        Return p
    End Function

    '==================================================================================================
    ' The page
    '==================================================================================================
    ''' <summary>One topic, laid out top to bottom for the current width and drawn by hand; scrolls with
    ''' the wheel / scroll bar / keys. Links (to other topics, and the 上一篇 / 下一篇 line added at the end)
    ''' raise LinkClicked.</summary>
    Private Class GuideView
        Inherits ScrollableControl

        Public Event LinkClicked(ByVal key As String)

        Private Const MarginX As Integer = 40
        Private Const MaxTextWidth As Integer = 680

        Private ReadOnly m_title As New Font(Look.FontName, 19.0F, FontStyle.Bold)
        Private ReadOnly m_summary As New Font(Look.FontName, 11.0F)
        Private ReadOnly m_heading As New Font(Look.FontName, 13.0F, FontStyle.Bold)
        Private ReadOnly m_body As New Font(Look.FontName, 11.0F)
        Private ReadOnly m_bold As New Font(Look.FontName, 11.0F, FontStyle.Bold)
        Private ReadOnly m_small As New Font(Look.FontName, 9.5F)
        Private ReadOnly m_link As New Font(Look.FontName, 11.0F, FontStyle.Underline)

        Private m_topic As GuideTopic
        Private ReadOnly m_items As New List(Of Item)
        Private m_layoutWidth As Integer = -1
        Private m_hotLink As Item

        Private Enum ItemKind
            Text          ' Text in Font / Color within Rect
            Badge         ' a numbered circle
            Dot           ' a bullet
            Panel         ' a tip / note box: Color = back, Color2 = bar
            Figure        ' FaceGuideFigures.Draw(Figure) scaled to Rect
            Rule          ' a thin line
            Link          ' Text as a link to Target
        End Enum

        Private Class Item
            Public Kind As ItemKind
            Public Rect As Rectangle
            Public Text As String
            Public Font As Font
            Public Color As Color
            Public Color2 As Color
            Public Figure As String
            Public Target As String
        End Class

        Public Sub New()
            SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
            BackColor = Color.White
            AutoScroll = True
        End Sub

        Public Property Topic As GuideTopic
            Get
                Return m_topic
            End Get
            Set(value As GuideTopic)
                m_topic = value
                m_layoutWidth = -1
                AutoScrollPosition = Point.Empty
                Arrange()
                Invalidate()
            End Set
        End Property

        Protected Overrides Sub OnResize(e As EventArgs)
            MyBase.OnResize(e)
            If ClientSize.Width <> m_layoutWidth Then
                Arrange()
                Invalidate()
            End If
        End Sub

        '----------------------------------------------------------------------------------------------
        ' Layout
        '----------------------------------------------------------------------------------------------
        Private Const Flags As TextFormatFlags = TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix Or TextFormatFlags.TextBoxControl

        Private Function Measure(ByVal s As String, ByVal ft As Font, ByVal w As Integer) As Integer
            Return TextRenderer.MeasureText(s, ft, New Size(Math.Max(20, w), 0), Flags).Height
        End Function

        ''' <summary>Adds a wrapped text at (x, y) of width w; returns its height.</summary>
        Private Function AddText(ByVal s As String, ByVal ft As Font, ByVal c As Color, ByVal x As Integer, ByVal y As Integer, ByVal w As Integer) As Integer
            Dim h As Integer = Measure(s, ft, w)
            m_items.Add(New Item With {.Kind = ItemKind.Text, .Text = s, .Font = ft, .Color = c, .Rect = New Rectangle(x, y, w, h)})
            Return h
        End Function

        Private Sub Arrange()
            m_items.Clear()
            m_hotLink = Nothing
            m_layoutWidth = ClientSize.Width
            If m_topic Is Nothing Then Return
            ' the scroll bar may come or go: lay out for the width without it, a bar's worth narrower
            Dim full As Integer = Math.Max(200, ClientSize.Width - If(VerticalScroll.Visible, 0, SystemInformation.VerticalScrollBarWidth))
            Dim w As Integer = Math.Min(MaxTextWidth, full - MarginX * 2)
            Dim x As Integer = MarginX
            Dim y As Integer = 28

            y += AddText(m_topic.Title, m_title, Look.Ink, x, y, w) + 4
            If m_topic.Summary <> "" Then y += AddText(m_topic.Summary, m_summary, Look.Muted, x, y, w) + 2
            y += 12
            m_items.Add(New Item With {.Kind = ItemKind.Rule, .Rect = New Rectangle(x, y, w, 1), .Color = Look.Rule})
            y += 18

            For Each b In m_topic.Blocks
                Select Case b.Kind
                    Case GuideBlockKind.Heading
                        y += 14
                        y += AddText(b.Text, m_heading, Look.Ink, x, y, w) + 8

                    Case GuideBlockKind.Paragraph
                        y += AddText(b.Text, m_body, Look.Body, x, y, w) + 12

                    Case GuideBlockKind.Steps
                        For i = 0 To b.Items.Length - 1
                            m_items.Add(New Item With {.Kind = ItemKind.Badge, .Text = (i + 1).ToString(), .Rect = New Rectangle(x, y, 24, 24)})
                            Dim h As Integer = AddText(b.Items(i), m_body, Look.Body, x + 36, y + 2, w - 36)
                            y += Math.Max(26, h + 2) + 10
                        Next
                        y += 4

                    Case GuideBlockKind.Bullets
                        For Each s In b.Items
                            m_items.Add(New Item With {.Kind = ItemKind.Dot, .Rect = New Rectangle(x + 6, y + 9, 6, 6), .Color = Look.Accent})
                            y += AddText(s, m_body, Look.Body, x + 22, y, w - 22) + 8
                        Next
                        y += 6

                    Case GuideBlockKind.Tip, GuideBlockKind.Note
                        Dim tip As Boolean = b.Kind = GuideBlockKind.Tip
                        Dim pad As Integer = 14
                        Dim label As String = If(tip, "小技巧", "注意")
                        Dim labelH As Integer = Measure(label, m_bold, w)
                        Dim textH As Integer = Measure(b.Text, m_body, w - pad * 2 - 4)
                        Dim panelH As Integer = pad + labelH + 4 + textH + pad
                        m_items.Add(New Item With {.Kind = ItemKind.Panel, .Rect = New Rectangle(x, y, w, panelH),
                                                   .Color = If(tip, Look.TipBack, Look.NoteBack), .Color2 = If(tip, Look.Accent, Look.NoteBar)})
                        AddText(label, m_bold, If(tip, Look.Accent, Look.NoteBar), x + pad + 4, y + pad, w - pad * 2 - 4)
                        AddText(b.Text, m_body, Look.Body, x + pad + 4, y + pad + labelH + 4, w - pad * 2 - 4)
                        y += panelH + 16

                    Case GuideBlockKind.Figure
                        Dim dh As Integer = FaceGuideFigures.Height(b.Figure)
                        If dh = 0 Then Continue For
                        Dim fw As Integer = Math.Min(FaceGuideFigures.DesignWidth, w - 32)
                        Dim fh As Integer = CInt(dh * fw / FaceGuideFigures.DesignWidth)
                        Dim frame As New Rectangle(x, y + 4, w, fh + 32 + If(b.Text <> "", 24, 0))
                        m_items.Add(New Item With {.Kind = ItemKind.Panel, .Rect = frame, .Color = Color.FromArgb(250, 251, 252), .Color2 = Color.Empty})
                        m_items.Add(New Item With {.Kind = ItemKind.Figure, .Figure = b.Figure, .Rect = New Rectangle(x + (w - fw) \ 2, y + 20, fw, fh)})
                        If b.Text <> "" Then
                            Dim capW As Integer = w - 32
                            m_items.Add(New Item With {.Kind = ItemKind.Text, .Text = b.Text, .Font = m_small, .Color = Look.Muted,
                                                       .Rect = New Rectangle(x + 16, y + 20 + fh + 10, capW, Measure(b.Text, m_small, capW))})
                        End If
                        y += frame.Height + 20

                    Case GuideBlockKind.Link
                        y += AddLink("→ " & b.Text, b.Target, x, y) + 10

                    Case GuideBlockKind.Question
                        m_items.Add(New Item With {.Kind = ItemKind.Badge, .Text = "Q", .Rect = New Rectangle(x, y, 24, 24)})
                        y += Math.Max(26, AddText(b.Text, m_bold, Look.Ink, x + 36, y + 2, w - 36) + 2) + 4
                        y += AddText(b.Items(0), m_body, Look.Body, x + 36, y, w - 36) + 22
                End Select
            Next

            ' 上一篇 / 下一篇
            Dim all As List(Of GuideTopic) = FaceGuideContent.Topics
            Dim i0 As Integer = all.IndexOf(m_topic)
            y += 8
            m_items.Add(New Item With {.Kind = ItemKind.Rule, .Rect = New Rectangle(x, y, w, 1), .Color = Look.Rule})
            y += 14
            Dim navH As Integer = 0
            If i0 > 0 Then navH = AddLink("← " & all(i0 - 1).Title, all(i0 - 1).Key, x, y)
            If i0 >= 0 AndAlso i0 < all.Count - 1 Then
                Dim s As String = all(i0 + 1).Title & " →"
                Dim sw As Integer = TextRenderer.MeasureText(s, m_link, New Size(w, 0), Flags).Width
                navH = Math.Max(navH, AddLink(s, all(i0 + 1).Key, x + w - sw, y))
            End If
            y += navH + 36

            AutoScrollMinSize = New Size(0, y)
        End Sub

        Private Function AddLink(ByVal s As String, ByVal target As String, ByVal x As Integer, ByVal y As Integer) As Integer
            Dim sz As Size = TextRenderer.MeasureText(s, m_link, New Size(MaxTextWidth, 0), Flags)
            m_items.Add(New Item With {.Kind = ItemKind.Link, .Text = s, .Target = target, .Font = m_link, .Color = Look.Accent, .Rect = New Rectangle(x, y, sz.Width, sz.Height)})
            Return sz.Height
        End Function

        '----------------------------------------------------------------------------------------------
        ' Drawing
        '----------------------------------------------------------------------------------------------
        Protected Overrides Sub OnScroll(se As ScrollEventArgs)
            MyBase.OnScroll(se)
            Invalidate()
        End Sub

        Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
            MyBase.OnMouseWheel(e)
            Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            Dim dy As Integer = AutoScrollPosition.Y
            For Each it In m_items
                Dim r As Rectangle = it.Rect
                r.Offset(0, dy)
                If r.Bottom < e.ClipRectangle.Top OrElse r.Top > e.ClipRectangle.Bottom Then Continue For
                Select Case it.Kind
                    Case ItemKind.Text
                        TextRenderer.DrawText(g, it.Text, it.Font, r, it.Color, Flags)
                    Case ItemKind.Link
                        TextRenderer.DrawText(g, it.Text, it.Font, r, If(it Is m_hotLink, Color.FromArgb(20, 80, 160), it.Color), Flags)
                    Case ItemKind.Badge
                        g.SmoothingMode = SmoothingMode.AntiAlias
                        Using b As New SolidBrush(If(it.Text = "Q", Look.NoteBar, Look.Accent))
                            g.FillEllipse(b, r)
                        End Using
                        TextRenderer.DrawText(g, it.Text, m_bold, r, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)
                    Case ItemKind.Dot
                        g.SmoothingMode = SmoothingMode.AntiAlias
                        Using b As New SolidBrush(it.Color)
                            g.FillEllipse(b, r)
                        End Using
                    Case ItemKind.Panel
                        g.SmoothingMode = SmoothingMode.AntiAlias
                        Using p As GraphicsPath = RoundRect(r, 8), b As New SolidBrush(it.Color)
                            g.FillPath(b, p)
                            If it.Color2 = Color.Empty Then
                                Using pen As New Pen(Look.Rule)
                                    g.DrawPath(pen, p)
                                End Using
                            End If
                        End Using
                        If it.Color2 <> Color.Empty Then
                            Using b As New SolidBrush(it.Color2)
                                g.FillRectangle(b, r.X, r.Y + 6, 4, r.Height - 12)
                            End Using
                        End If
                    Case ItemKind.Figure
                        Dim state As GraphicsState = g.Save()
                        g.TranslateTransform(r.X, r.Y)
                        Dim k As Single = CSng(r.Width / FaceGuideFigures.DesignWidth)
                        g.ScaleTransform(k, k)
                        g.SetClip(New RectangleF(-4, -4, FaceGuideFigures.DesignWidth + 8, FaceGuideFigures.Height(it.Figure) + 8))
                        FaceGuideFigures.Draw(g, it.Figure)
                        g.Restore(state)
                    Case ItemKind.Rule
                        Using b As New SolidBrush(it.Color)
                            g.FillRectangle(b, r)
                        End Using
                End Select
            Next
        End Sub

        '----------------------------------------------------------------------------------------------
        ' Links and keys
        '----------------------------------------------------------------------------------------------
        Private Function LinkAt(ByVal p As Point) As Item
            p.Offset(0, -AutoScrollPosition.Y)
            Return m_items.FirstOrDefault(Function(it) it.Kind = ItemKind.Link AndAlso it.Rect.Contains(p))
        End Function

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            Dim hit As Item = LinkAt(e.Location)
            If hit IsNot m_hotLink Then
                m_hotLink = hit
                Cursor = If(hit Is Nothing, Cursors.Default, Cursors.Hand)
                Invalidate()
            End If
        End Sub

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            Focus()
            Dim hit As Item = LinkAt(e.Location)
            If hit IsNot Nothing AndAlso e.Button = MouseButtons.Left Then RaiseEvent LinkClicked(hit.Target)
        End Sub

        Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
            Select Case keyData
                Case Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End
                    Return True
            End Select
            Return MyBase.IsInputKey(keyData)
        End Function

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Control OrElse e.Alt Then Return
            Dim y As Integer = -AutoScrollPosition.Y
            Select Case e.KeyCode
                Case Keys.Up : y -= 40
                Case Keys.Down : y += 40
                Case Keys.PageUp : y -= ClientSize.Height - 40
                Case Keys.PageDown, Keys.Space : y += ClientSize.Height - 40
                Case Keys.Home : y = 0
                Case Keys.End : y = AutoScrollMinSize.Height
                Case Else : Return
            End Select
            AutoScrollPosition = New Point(0, Math.Max(0, y))
            Invalidate()
            e.Handled = True
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                For Each ft In {m_title, m_summary, m_heading, m_body, m_bold, m_small, m_link}
                    ft.Dispose()
                Next
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

End Class
