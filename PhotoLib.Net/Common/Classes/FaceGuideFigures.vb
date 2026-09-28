Imports System.Drawing.Drawing2D
Imports System.Drawing.Text

' The pictures of the face guide (frmFaceGuide): simplified drawings of the real screens -- the face
' wall, the confirm view, the viewer's face boxes, the settings page -- with cartoon people instead of
' real photos. Each figure is drawn at a fixed design size (DesignWidth x Height(key)) and scaled down
' by the caller when the page is narrower. Text goes through DrawString (GDI+) so it scales with the
' figure.
Public NotInheritable Class FaceGuideFigures

    Public Const DesignWidth As Integer = 560

    Private Shared ReadOnly FontName As String = "Microsoft JhengHei UI"

    ''' <summary>Design height of figure <paramref name="key"/>; 0 for an unknown key.</summary>
    Public Shared Function Height(ByVal key As String) As Integer
        Select Case key
            Case "flow" : Return 118
            Case "status" : Return 132
            Case "labels" : Return 270
            Case "ages" : Return 170
            Case "wall" : Return 290
            Case "wallmenu" : Return 214
            Case "group", "confirm" : Return 280
            Case "namebox" : Return 270
            Case "merge" : Return 160
            Case "setup" : Return 262
            Case "strict" : Return 150
            Case "reject" : Return 230
            Case "addface" : Return 270
            Case Else : Return 0
        End Select
    End Function

    ''' <summary>Draws figure <paramref name="key"/> with its top-left at (0, 0) in design units.</summary>
    Public Shared Sub Draw(ByVal g As Graphics, ByVal key As String)
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        Select Case key
            Case "flow" : DrawFlow(g)
            Case "status" : DrawStatus(g)
            Case "labels" : DrawLabels(g)
            Case "ages" : DrawAges(g)
            Case "wall" : DrawWall(g)
            Case "wallmenu" : DrawWallMenu(g)
            Case "group" : DrawTicks(g, False)
            Case "confirm" : DrawTicks(g, True)
            Case "namebox" : DrawNameBox(g)
            Case "merge" : DrawMerge(g)
            Case "setup" : DrawSetup(g)
            Case "strict" : DrawStrict(g)
            Case "reject" : DrawReject(g)
            Case "addface" : DrawAddFace(g)
        End Select
    End Sub

    '==================================================================================================
    ' Colours and small helpers
    '==================================================================================================
    Private Shared ReadOnly Skins() As Color = {Color.FromArgb(241, 200, 170), Color.FromArgb(224, 172, 130), Color.FromArgb(250, 216, 190), Color.FromArgb(205, 150, 110)}
    Private Shared ReadOnly Hairs() As Color = {Color.FromArgb(58, 40, 30), Color.FromArgb(28, 28, 32), Color.FromArgb(122, 82, 44), Color.FromArgb(196, 196, 200), Color.FromArgb(92, 60, 40)}
    Private Shared ReadOnly Shirts() As Color = {Color.FromArgb(70, 130, 200), Color.FromArgb(220, 96, 96), Color.FromArgb(88, 168, 120), Color.FromArgb(230, 170, 60), Color.FromArgb(150, 112, 190), Color.FromArgb(96, 104, 120)}
    Private Shared ReadOnly Backs() As Color = {Color.FromArgb(222, 232, 246), Color.FromArgb(246, 228, 224), Color.FromArgb(226, 242, 230), Color.FromArgb(247, 240, 220), Color.FromArgb(236, 228, 246)}

    ' who is who in the figures: 媽媽 4 (long hair), 爸爸 5, 陳慈祐 2, 阿嬤 3 (grey hair)
    Private Shared ReadOnly LabelWho() As Integer = {4, 5, 3, 0}
    Private Shared ReadOnly WallWho() As Integer = {4, 5, 2, 3}
    Private Shared ReadOnly GroupWho() As Integer = {7, 6, 8}

    Private Shared ReadOnly Ink As Color = Color.FromArgb(40, 48, 60)
    Private Shared ReadOnly Gray As Color = Color.FromArgb(120, 128, 140)
    Private Shared ReadOnly Line As Color = Color.FromArgb(206, 212, 220)
    Private Shared ReadOnly Accent As Color = Color.FromArgb(42, 116, 208)
    Private Shared ReadOnly YesGreen As Color = Color.FromArgb(46, 154, 91)
    Private Shared ReadOnly NoRed As Color = Color.FromArgb(197, 58, 58)
    Private Shared ReadOnly Unnamed As Color = Color.FromArgb(242, 183, 5)
    Private Shared ReadOnly Matched As Color = Color.FromArgb(135, 190, 245)

    Private Shared Function F(ByVal size As Single, Optional ByVal style As FontStyle = FontStyle.Regular) As Font
        Return New Font(FontName, size, style, GraphicsUnit.Pixel)
    End Function

    Private Shared Sub Text(ByVal g As Graphics, ByVal s As String, ByVal size As Single, ByVal c As Color, ByVal x As Single, ByVal y As Single, Optional ByVal style As FontStyle = FontStyle.Regular)
        Using ft As Font = F(size, style), b As New SolidBrush(c)
            g.DrawString(s, ft, b, x, y)
        End Using
    End Sub

    Private Shared Sub TextIn(ByVal g As Graphics, ByVal s As String, ByVal size As Single, ByVal c As Color, ByVal r As RectangleF,
                              Optional ByVal align As StringAlignment = StringAlignment.Center, Optional ByVal style As FontStyle = FontStyle.Regular)
        Using ft As Font = F(size, style), b As New SolidBrush(c), sf As New StringFormat With {.Alignment = align, .LineAlignment = StringAlignment.Center}
            g.DrawString(s, ft, b, r, sf)
        End Using
    End Sub

    Private Shared Function TextWidth(ByVal g As Graphics, ByVal s As String, ByVal size As Single, Optional ByVal style As FontStyle = FontStyle.Regular) As Single
        Using ft As Font = F(size, style)
            Return g.MeasureString(s, ft, PointF.Empty, StringFormat.GenericTypographic).Width
        End Using
    End Function

    Private Shared Function Round(ByVal r As RectangleF, ByVal radius As Single) As GraphicsPath
        Dim p As New GraphicsPath
        Dim d As Single = Math.Min(radius * 2, Math.Min(r.Width, r.Height))
        If d <= 0 Then
            p.AddRectangle(r)
            Return p
        End If
        p.AddArc(r.X, r.Y, d, d, 180, 90)
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        p.CloseFigure()
        Return p
    End Function

    Private Shared Sub Box(ByVal g As Graphics, ByVal r As RectangleF, ByVal fill As Color, ByVal border As Color, Optional ByVal radius As Single = 6, Optional ByVal shadow As Boolean = False)
        If shadow Then
            Using p As GraphicsPath = Round(New RectangleF(r.X + 2, r.Y + 3, r.Width, r.Height), radius), b As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                g.FillPath(b, p)
            End Using
        End If
        Using p As GraphicsPath = Round(r, radius)
            Using b As New SolidBrush(fill)
                g.FillPath(b, p)
            End Using
            If border <> Color.Empty Then
                Using pen As New Pen(border)
                    g.DrawPath(pen, p)
                End Using
            End If
        End Using
    End Sub

    Private Shared Sub Arrow(ByVal g As Graphics, ByVal x1 As Single, ByVal y1 As Single, ByVal x2 As Single, ByVal y2 As Single, Optional ByVal c As Color = Nothing)
        If c = Color.Empty Then c = Gray
        Using pen As New Pen(c, 2.5F) With {.CustomEndCap = New AdjustableArrowCap(4, 4)}
            g.DrawLine(pen, x1, y1, x2, y2)
        End Using
    End Sub

    ''' <summary>A numbered circle marking a step in a figure.</summary>
    Private Shared Sub Badge(ByVal g As Graphics, ByVal n As Integer, ByVal cx As Single, ByVal cy As Single)
        Using b As New SolidBrush(Accent), pen As New Pen(Color.White, 2)
            g.FillEllipse(b, cx - 11, cy - 11, 22, 22)
            g.DrawEllipse(pen, cx - 11, cy - 11, 22, 22)
        End Using
        TextIn(g, n.ToString(), 12, Color.White, New RectangleF(cx - 11, cy - 11, 22, 22), StringAlignment.Center, FontStyle.Bold)
    End Sub

    ''' <summary>A mouse pointer with its tip at (x, y).</summary>
    Private Shared Sub Pointer(ByVal g As Graphics, ByVal x As Single, ByVal y As Single)
        Dim pts() As PointF = {New PointF(x, y), New PointF(x, y + 18), New PointF(x + 4.5F, y + 14), New PointF(x + 8, y + 21),
                               New PointF(x + 11, y + 19.5F), New PointF(x + 7.5F, y + 12.5F), New PointF(x + 13, y + 12.5F)}
        Using b As New SolidBrush(Color.White), pen As New Pen(Color.Black, 1.2F)
            g.FillPolygon(b, pts)
            g.DrawPolygon(pen, pts)
        End Using
    End Sub

    '==================================================================================================
    ' People
    '==================================================================================================
    ''' <summary>A cartoon person: head of radius <paramref name="r"/> centred at (cx, cy), shoulders below
    ''' down to <paramref name="bottom"/>. <paramref name="who"/> picks skin, hair and shirt.</summary>
    Private Shared Sub Person(ByVal g As Graphics, ByVal who As Integer, ByVal cx As Single, ByVal cy As Single, ByVal r As Single, ByVal bottom As Single, Optional ByVal side As Boolean = False)
        Dim skin As Color = Skins(who Mod Skins.Length), hair As Color = Hairs(who Mod Hairs.Length), shirt As Color = Shirts(who Mod Shirts.Length)
        ' shoulders, cut off at the bottom of the picture
        Dim state As GraphicsState = g.Save()
        g.IntersectClip(New RectangleF(cx - r * 3, cy - r * 3, r * 6, bottom - (cy - r * 3)))
        Using b As New SolidBrush(shirt), p As GraphicsPath = Round(New RectangleF(cx - r * 1.7F, cy + r * 1.05F, r * 3.4F, Math.Max(r, bottom - cy - r * 1.05F) + r), r * 0.9F)
            g.FillPath(b, p)
        End Using
        g.Restore(state)
        ' neck, head
        Using b As New SolidBrush(skin)
            g.FillRectangle(b, cx - r * 0.35F, cy + r * 0.6F, r * 0.7F, r * 0.6F)
            g.FillEllipse(b, cx - r, cy - r, r * 2, r * 2.1F)
        End Using
        ' hair: a cap over the top of the head (long at the sides for some)
        Using b As New SolidBrush(hair)
            g.FillPie(b, cx - r * 1.05F, cy - r * 1.12F, r * 2.1F, r * 1.7F, 180, 180)
            If who Mod 3 = 1 Then
                g.FillRectangle(b, cx - r * 1.05F, cy - r * 0.3F, r * 0.35F, r * 1.3F)
                g.FillRectangle(b, cx + r * 0.7F, cy - r * 0.3F, r * 0.35F, r * 1.3F)
            End If
        End Using
        ' eyes, smile (a side face: both eyes to one side)
        Dim eyeDx As Single = If(side, r * 0.15F, 0)
        Using b As New SolidBrush(Color.FromArgb(50, 40, 40)), pen As New Pen(Color.FromArgb(150, 80, 70), Math.Max(1, r * 0.09F))
            g.FillEllipse(b, cx - r * 0.42F + eyeDx, cy + r * 0.05F, r * 0.2F, r * 0.22F)
            If Not side Then g.FillEllipse(b, cx + r * 0.22F, cy + r * 0.05F, r * 0.2F, r * 0.22F)
            g.DrawArc(pen, cx - r * 0.35F + eyeDx, cy + r * 0.2F, r * 0.7F, r * 0.5F, 20, 140)
        End Using
    End Sub

    ''' <summary>A face picture as on a card or a tick tile: a person close up on a coloured ground.</summary>
    Private Shared Sub Avatar(ByVal g As Graphics, ByVal who As Integer, ByVal r As RectangleF)
        Dim state As GraphicsState = g.Save()
        g.SetClip(r)
        Using b As New SolidBrush(Backs(who Mod Backs.Length))
            g.FillRectangle(b, r)
        End Using
        Person(g, who, r.X + r.Width / 2, r.Y + r.Height * 0.46F, r.Width * 0.27F, r.Bottom + 4)
        g.Restore(state)
    End Sub

    ''' <summary>A person card of the face wall: picture, name, count.</summary>
    Private Shared Sub PersonCard(ByVal g As Graphics, ByVal who As Integer, ByVal x As Single, ByVal y As Single, ByVal w As Single, ByVal name As String, Optional ByVal count As String = "")
        Box(g, New RectangleF(x, y, w, w + 30), Color.White, Line, 6, True)
        Avatar(g, who, New RectangleF(x + 5, y + 5, w - 10, w - 10))
        TextIn(g, name, 12, Ink, New RectangleF(x, y + w - 4, w, 20), StringAlignment.Center, FontStyle.Bold)
        If count <> "" Then TextIn(g, count, 10, Gray, New RectangleF(x, y + w + 12, w, 16))
    End Sub

    ''' <summary>A group card: four small faces of one unnamed group.</summary>
    Private Shared Sub GroupCard(ByVal g As Graphics, ByVal who As Integer, ByVal x As Single, ByVal y As Single, ByVal w As Single, ByVal count As String)
        Box(g, New RectangleF(x, y, w, w + 30), Color.White, Line, 6, True)
        Dim half As Single = (w - 12) / 2
        For i = 0 To 3
            Avatar(g, who, New RectangleF(x + 5 + (i Mod 2) * (half + 2), y + 5 + (i \ 2) * (half + 2), half, half))
        Next
        TextIn(g, "未命名", 12, Gray, New RectangleF(x, y + w - 4, w, 20))
        TextIn(g, count, 10, Gray, New RectangleF(x, y + w + 12, w, 16))
    End Sub

    ''' <summary>A face tile with a tick box (group / confirm views).</summary>
    Private Shared Sub TickTile(ByVal g As Graphics, ByVal who As Integer, ByVal x As Single, ByVal y As Single, ByVal w As Single, ByVal ticked As Boolean)
        Box(g, New RectangleF(x, y, w, w), Color.White, If(ticked, Line, NoRed), 4, True)
        Avatar(g, who, New RectangleF(x + 4, y + 4, w - 8, w - 8))
        Dim cb As New RectangleF(x + 7, y + 7, 16, 16)
        Box(g, cb, If(ticked, Accent, Color.White), If(ticked, Accent, Gray), 3)
        If ticked Then
            Using pen As New Pen(Color.White, 2.2F)
                g.DrawLines(pen, {New PointF(cb.X + 3.5F, cb.Y + 8), New PointF(cb.X + 7, cb.Y + 11.5F), New PointF(cb.X + 12.5F, cb.Y + 4.5F)})
            End Using
        End If
    End Sub

    ''' <summary>A context menu; item <paramref name="hot"/> highlighted, "-" is a separator.</summary>
    Private Shared Function MenuBox(ByVal g As Graphics, ByVal x As Single, ByVal y As Single, ByVal w As Single, ByVal hot As Integer, ParamArray items As String()) As RectangleF
        Dim h As Single = 6
        For Each s In items
            h += If(s = "-", 9, 26)
        Next
        Dim r As New RectangleF(x, y, w, h + 6)
        Box(g, r, Color.White, Color.FromArgb(180, 186, 196), 4, True)
        Dim yy As Single = y + 6
        For i = 0 To items.Length - 1
            If items(i) = "-" Then
                Using pen As New Pen(Line)
                    g.DrawLine(pen, x + 10, yy + 4, x + w - 10, yy + 4)
                End Using
                yy += 9
                Continue For
            End If
            If i = hot Then Box(g, New RectangleF(x + 4, yy, w - 8, 24), Color.FromArgb(222, 236, 252), Color.FromArgb(150, 190, 236), 3)
            TextIn(g, items(i), 12, Ink, New RectangleF(x + 14, yy, w - 20, 24), StringAlignment.Near)
            yy += 26
        Next
        Return r
    End Function

    ''' <summary>A face box as the viewer draws it, with its label (and ✓ ✕ when it needs confirming).</summary>
    Private Shared Sub FaceBox(ByVal g As Graphics, ByVal r As RectangleF, ByVal c As Color, ByVal label As String, ByVal dashed As Boolean, Optional ByVal confirm As Boolean = False, Optional ByVal selected As Boolean = False)
        Using shadow As New Pen(Color.FromArgb(120, 0, 0, 0), 4), pen As New Pen(If(selected, Color.FromArgb(63, 127, 208), c), If(selected, 3, 2))
            If dashed AndAlso Not selected Then pen.DashStyle = DashStyle.Dash
            g.DrawRectangle(shadow, r.X, r.Y, r.Width, r.Height)
            g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height)
        End Using
        If label = "" Then Return
        Dim tw As Single = TextWidth(g, label, 12) + 14
        Dim w As Single = tw + If(confirm, 44, 0)
        Dim lr As New RectangleF(r.X + r.Width / 2 - w / 2, r.Bottom + 4, w, 20)
        Using b As New SolidBrush(Color.FromArgb(215, 20, 24, 30))
            g.FillRectangle(b, lr)
        End Using
        TextIn(g, label, 12, c, New RectangleF(lr.X, lr.Y, tw, lr.Height))
        If confirm Then
            Dim yes As New RectangleF(lr.Right - 42, lr.Y + 1, 20, 18), no As New RectangleF(lr.Right - 21, lr.Y + 1, 20, 18)
            Using b1 As New SolidBrush(YesGreen), b2 As New SolidBrush(NoRed)
                g.FillRectangle(b1, yes)
                g.FillRectangle(b2, no)
            End Using
            TextIn(g, "✓", 12, Color.White, yes, StringAlignment.Center, FontStyle.Bold)
            TextIn(g, "✕", 12, Color.White, no, StringAlignment.Center, FontStyle.Bold)
        End If
    End Sub

    ''' <summary>The viewer: black around a photo of a park with people in it. Returns the photo rectangle.</summary>
    Private Shared Function ViewerPhoto(ByVal g As Graphics, ByVal r As RectangleF) As RectangleF
        Using b As New SolidBrush(Color.FromArgb(18, 18, 20))
            g.FillRectangle(b, r)
        End Using
        Dim photo As New RectangleF(r.X + 14, r.Y + 14, r.Width - 28, r.Height - 28)
        Using sky As New LinearGradientBrush(photo, Color.FromArgb(150, 196, 236), Color.FromArgb(214, 234, 248), LinearGradientMode.Vertical)
            g.FillRectangle(sky, photo)
        End Using
        Using grass As New SolidBrush(Color.FromArgb(128, 176, 104)), tree As New SolidBrush(Color.FromArgb(96, 150, 90))
            g.FillRectangle(grass, photo.X, photo.Y + photo.Height * 0.62F, photo.Width, photo.Height * 0.38F)
            g.FillEllipse(tree, photo.X + photo.Width * 0.72F, photo.Y + photo.Height * 0.12F, photo.Width * 0.3F, photo.Height * 0.55F)
        End Using
        Return photo
    End Function

    ''' <summary>The viewer's face bar at the top-right of <paramref name="photo"/>.</summary>
    Private Shared Sub FaceBar(ByVal g As Graphics, ByVal photo As RectangleF, ByVal first As String, ByVal info As String, Optional ByVal hot As Integer = -1)
        Dim items() As String = {first, "新增面孔", "說明"}
        Dim w As Single = 196, x As Single = photo.Right - w - 8, y As Single = photo.Y + 8
        Box(g, New RectangleF(x, y, w, 26), Color.FromArgb(230, 40, 40, 40), Color.Empty, 4)
        Dim cellW() As Single = {74, 74, 48}
        Dim cx As Single = x
        For i = 0 To 2
            If i = hot Then Box(g, New RectangleF(cx + 2, y + 2, cellW(i) - 4, 22), Color.FromArgb(90, 90, 90), Color.Empty, 3)
            TextIn(g, items(i), 12, Color.White, New RectangleF(cx, y, cellW(i), 26))
            cx += cellW(i)
        Next
        If info <> "" Then
            Dim iw As Single = TextWidth(g, info, 10.5F) + 16
            Box(g, New RectangleF(x + w - iw, y + 29, iw, 19), Color.FromArgb(220, 0, 0, 0), Color.Empty, 3)
            TextIn(g, info, 10.5F, Color.FromArgb(225, 225, 225), New RectangleF(x + w - iw, y + 29, iw, 19))
        End If
    End Sub

    Private Shared Function HeadBox(ByVal cx As Single, ByVal cy As Single, ByVal r As Single) As RectangleF
        Return New RectangleF(cx - r * 1.2F, cy - r * 1.25F, r * 2.4F, r * 2.5F)
    End Function

    '==================================================================================================
    ' The figures
    '==================================================================================================
    Private Shared Sub DrawFlow(ByVal g As Graphics)
        Dim steps() As String = {"分析照片", "找出臉", "比對、分群", "你確認 ✓", "寫進人物欄"}
        Dim notes() As String = {"背景進行", "每張臉一組特徵", "猜名字、分群組", "按 ✓ 或命名", "照片資訊看得到"}
        Dim w As Single = 92, gap As Single = (DesignWidth - w * 5) / 4
        For i = 0 To 4
            Dim x As Single = i * (w + gap)
            Dim mine As Boolean = i = 3
            Box(g, New RectangleF(x, 20, w, 54), If(mine, Color.FromArgb(222, 236, 252), Color.FromArgb(246, 247, 249)), If(mine, Accent, Line), 8)
            TextIn(g, steps(i), 13, If(mine, Accent, Ink), New RectangleF(x, 22, w, 30), StringAlignment.Center, FontStyle.Bold)
            TextIn(g, notes(i), 10, Gray, New RectangleF(x, 48, w, 20))
            If i < 4 Then Arrow(g, x + w + 3, 47, x + w + gap - 3, 47)
        Next
        TextIn(g, "程式負責", 11, Gray, New RectangleF(0, 84, w * 3 + gap * 2, 20))
        Using pen As New Pen(Line, 1)
            g.DrawLine(pen, 10, 84, w * 3 + gap * 2 - 10, 84)
            g.DrawLine(pen, (w + gap) * 3 + 10, 84, DesignWidth - 10, 84)
        End Using
        TextIn(g, "由你決定", 11, Accent, New RectangleF((w + gap) * 3, 84, w * 2 + gap, 20), StringAlignment.Center, FontStyle.Bold)
    End Sub

    Private Shared Sub DrawStatus(ByVal g As Graphics)
        For row = 0 To 1
            Dim y As Single = 8 + row * 62
            Using b As New LinearGradientBrush(New RectangleF(0, y, DesignWidth, 40), Color.FromArgb(236, 238, 242), Color.FromArgb(214, 218, 224), LinearGradientMode.Vertical)
                g.FillRectangle(b, 0, y, DesignWidth, 40)
            End Using
            Text(g, If(row = 0, "分析中", "分析完成後"), 10, Gray, 8, y - 1)
            Text(g, "已選取 3 張照片", 13, Ink, 20, y + 14)
            Dim s As String = If(row = 0, "分析面孔 1,234 / 13,000", "面孔：12 人 · 35 群未命名")
            Dim sx As Single = 170
            Box(g, New RectangleF(sx - 6, y + 9, TextWidth(g, s, 13) + 14, 26), Color.FromArgb(255, 250, 222), Unnamed, 4)
            Text(g, s, 13, Ink, sx, y + 14)
            If row = 0 Then
                Pointer(g, sx + 110, y + 24)
                Box(g, New RectangleF(sx + 196, y + 8, 150, 26), Color.White, Line, 4, True)
                TextIn(g, "點一下暫停 / 繼續", 11.5F, Accent, New RectangleF(sx + 196, y + 8, 150, 26))
            Else
                Box(g, New RectangleF(sx + 196, y + 8, 186, 26), Color.White, Line, 4, True)
                TextIn(g, "左邊清單出現「面孔」", 11.5F, Accent, New RectangleF(sx + 196, y + 8, 186, 26))
            End If
        Next
    End Sub

    Private Shared Sub DrawLabels(ByVal g As Graphics)
        Dim photo As RectangleF = ViewerPhoto(g, New RectangleF(0, 0, DesignWidth, 270))
        Dim heads() As PointF = {New PointF(92, 108), New PointF(212, 118), New PointF(332, 104), New PointF(448, 124)}
        Dim rs() As Single = {26, 22, 25, 20}
        For i = 0 To 3
            Person(g, LabelWho(i), heads(i).X, heads(i).Y, rs(i), photo.Bottom)
        Next
        FaceBox(g, HeadBox(heads(0).X, heads(0).Y, rs(0)), Color.White, "媽媽", False)
        FaceBox(g, HeadBox(heads(1).X, heads(1).Y, rs(1)), Matched, "爸爸？", True, True)
        FaceBox(g, HeadBox(heads(2).X, heads(2).Y, rs(2)), Unnamed, "這是 阿嬤 嗎？", True, True)
        FaceBox(g, HeadBox(heads(3).X, heads(3).Y, rs(3)), Unnamed, "這是誰？", True)
        FaceBar(g, photo, "隱藏面孔", "4 張臉 · 2 張待確認 · 1 張未命名")
        Dim cap() As String = {"確定", "程式認出", "可能是", "還不知道"}
        For i = 0 To 3
            Dim r As RectangleF = HeadBox(heads(i).X, heads(i).Y, rs(i))
            Box(g, New RectangleF(r.X + r.Width / 2 - 32, photo.Bottom - 26, 64, 20), Color.FromArgb(235, 255, 255, 255), Color.Empty, 10)
            TextIn(g, cap(i), 11, Ink, New RectangleF(r.X + r.Width / 2 - 32, photo.Bottom - 26, 64, 20))
        Next
    End Sub

    Private Shared Sub DrawAges(ByVal g As Graphics)
        Dim ages() As String = {"0 歲", "1 歲", "3–4 歲", "7–8 歲", "13–15 歲", "21–30 歲"}
        Dim sizes() As Single = {15, 17, 19, 21, 23, 25}
        Using pen As New Pen(Line, 3)
            g.DrawLine(pen, 20, 118, DesignWidth - 20, 118)
        End Using
        Dim stepX As Single = (DesignWidth - 40) / 6
        For i = 0 To 5
            Dim cx As Single = 20 + stepX * i + stepX / 2
            Dim r As Single = sizes(i)
            Dim tile As New RectangleF(cx - r * 1.6F, 100 - r * 3.2F, r * 3.2F, r * 3.2F)
            Box(g, New RectangleF(tile.X - 3, tile.Y - 3, tile.Width + 6, tile.Height + 6), Color.White, Line, 6, True)
            Avatar(g, 2, tile)
            Using b As New SolidBrush(Accent)
                g.FillEllipse(b, cx - 5, 113, 10, 10)
            End Using
            TextIn(g, ages(i), 11.5F, Ink, New RectangleF(cx - stepX / 2, 126, stepX, 18), StringAlignment.Center, FontStyle.Bold)
        Next
        TextIn(g, "同一個人：每個年紀各自比對，小時候跟小時候比", 11, Gray, New RectangleF(0, 148, DesignWidth, 20))
    End Sub

    ''' <summary>The 面孔 node of the tree at the left of the wall figures.</summary>
    Private Shared Sub Tree(ByVal g As Graphics, ByVal r As RectangleF, ByVal hot As Integer)
        Box(g, r, Color.FromArgb(244, 246, 249), Line, 4)
        Dim rows() As String = {"▾ 面孔 (35)", "媽媽 (1,203)", "爸爸 (986)", "陳慈祐 (412)", "阿嬤 (230)", "未命名的臉 (35 群)"}
        For i = 0 To rows.Length - 1
            Dim y As Single = r.Y + 10 + i * 26
            Dim x As Single = r.X + If(i = 0, 8, 26)
            If i = hot Then Box(g, New RectangleF(r.X + 4, y - 2, r.Width - 8, 24), Color.FromArgb(214, 230, 250), Color.Empty, 3)
            If i > 0 Then
                Using b As New SolidBrush(If(i = rows.Length - 1, Unnamed, Accent))
                    g.FillEllipse(b, x - 12, y + 7, 7, 7)
                End Using
            End If
            Text(g, rows(i), 12, Ink, x, y + 2, If(i = 0, FontStyle.Bold, FontStyle.Regular))
        Next
    End Sub

    Private Shared Sub DrawWall(ByVal g As Graphics)
        Tree(g, New RectangleF(0, 0, 150, 290), 0)
        Dim names() As String = {"媽媽", "爸爸", "陳慈祐", "阿嬤"}
        Dim counts() As String = {"1,203 張", "986 張", "412 張", "230 張"}
        Dim w As Single = 84, gap As Single = 14, x0 As Single = 170
        For i = 0 To 3
            PersonCard(g, WallWho(i), x0 + i * (w + gap), 6, w, names(i), counts(i))
        Next
        Dim groups() As String = {"36 張臉", "21 張臉", "14 張臉"}
        For i = 0 To 2
            GroupCard(g, GroupWho(i), x0 + i * (w + gap), 150, w, groups(i))
        Next
        Box(g, New RectangleF(x0 + 3 * (w + gap), 170, w + 4, 74), Color.White, Line, 6, True)
        TextIn(g, "按兩下：" & vbLf & "人物 → 看照片" & vbLf & "群組 → 逐張命名", 10.5F, Accent, New RectangleF(x0 + 3 * (w + gap), 170, w + 4, 74))
    End Sub

    Private Shared Sub DrawWallMenu(ByVal g As Graphics)
        PersonCard(g, 4, 20, 20, 110, "媽媽", "1,203 張")
        Pointer(g, 96, 96)
        MenuBox(g, 150, 12, 220, 1, "看照片", "確認更多照片（128）…", "改名／合併…", "設定出生年…", "更換封面…", "-", "隱藏")
        Box(g, New RectangleF(390, 44, 166, 58), Color.FromArgb(234, 243, 253), Color.Empty, 6)
        TextIn(g, "程式替「媽媽」找到" & vbLf & "128 張臉，等你確認", 11.5F, Accent, New RectangleF(390, 44, 166, 58))
        Arrow(g, 390, 72, 374, 58, Accent)
    End Sub

    Private Shared Sub DrawTicks(ByVal g As Graphics, ByVal confirm As Boolean)
        Dim title As String = If(confirm, "媽媽 — 確認更多照片　這一批 40 張 · 共 128 張待確認", "這是誰？　12 張臉")
        Box(g, New RectangleF(0, 0, DesignWidth, 26), Color.FromArgb(244, 246, 249), Line, 4)
        Text(g, title, 12, Ink, 10, 5, FontStyle.Bold)
        Dim w As Single = 66, gap As Single = 8
        Dim odd As Integer = If(confirm, 5, 2)
        For i = 0 To 11
            Dim x As Single = (i Mod 4) * (w + gap), y As Single = 36 + (i \ 4) * (w + gap)
            Dim wrong As Boolean = i = odd OrElse (confirm AndAlso i = 10)
            TickTile(g, If(wrong, 3 + i, If(confirm, 4, 0)), x, y, w, Not wrong)
        Next
        Badge(g, 1, (odd Mod 4) * (w + gap) + w - 8, 36 + (odd \ 4) * (w + gap) + 8)
        Text(g, "不是的取消勾選", 11, NoRed, 0, 36 + 3 * (w + gap) + 2)
        Dim items() As String = If(confirm,
            {"勾選的是「媽媽」、其餘不是", "只確認勾選的（其餘之後再說）", "-", "全部勾選", "全部取消勾選", "-", "回到面孔牆"},
            {"將勾選的臉命名為…", "全部勾選", "全部取消勾選", "-", "回到面孔牆"})
        Dim m As RectangleF = MenuBox(g, 318, 44, 236, 0, items)
        Badge(g, 2, m.X - 2, m.Y + 18)
        Text(g, "再按右鍵", 11, Accent, m.X + 6, m.Bottom + 6)
    End Sub

    Private Shared Sub DrawNameBox(ByVal g As Graphics)
        Dim photo As RectangleF = ViewerPhoto(g, New RectangleF(0, 0, DesignWidth, 270))
        Person(g, 4, 150, 112, 26, photo.Bottom)
        Person(g, 2, 300, 118, 24, photo.Bottom)
        FaceBox(g, HeadBox(150, 112, 26), Color.White, "媽媽", False)
        Dim sel As RectangleF = HeadBox(300, 118, 24)
        FaceBox(g, sel, Color.White, "", False, False, True)
        ' the name box: a combo with the names that fit what was typed
        Dim cb As New RectangleF(sel.X + sel.Width / 2 - 80, sel.Bottom + 6, 160, 26)
        Box(g, cb, Color.White, Accent, 2)
        Text(g, "陳慈", 13, Ink, cb.X + 6, cb.Y + 4)
        Using pen As New Pen(Ink, 1.2F)
            Dim cx As Single = cb.X + 6 + TextWidth(g, "陳慈", 13) + 3
            g.DrawLine(pen, cx, cb.Y + 5, cx, cb.Bottom - 5)
        End Using
        Dim dd As New RectangleF(cb.X, cb.Bottom, cb.Width, 50)
        Box(g, dd, Color.White, Line, 0, True)
        Box(g, New RectangleF(dd.X + 1, dd.Y + 1, dd.Width - 2, 24), Color.FromArgb(0, 120, 215), Color.Empty, 0)
        Text(g, "陳慈祐", 12.5F, Color.White, dd.X + 6, dd.Y + 4)
        Text(g, "陳慈恩", 12.5F, Ink, dd.X + 6, dd.Y + 28)
        FaceBar(g, photo, "隱藏面孔", "2 張臉 · 1 張未命名")
        Box(g, New RectangleF(cb.Right + 16, cb.Y - 2, 150, 48), Color.White, Line, 6, True)
        TextIn(g, "Enter 儲存" & vbLf & "Esc 取消", 11.5F, Accent, New RectangleF(cb.Right + 16, cb.Y - 2, 150, 48))
    End Sub

    Private Shared Sub DrawMerge(ByVal g As Graphics)
        PersonCard(g, 2, 10, 14, 96, "陳慈佑", "35 張")
        TextIn(g, "+", 28, Gray, New RectangleF(112, 50, 30, 40))
        PersonCard(g, 2, 148, 14, 96, "陳慈祐", "377 張")
        Arrow(g, 262, 70, 318, 70, Accent)
        TextIn(g, "改名／合併", 11, Accent, New RectangleF(250, 40, 80, 20))
        PersonCard(g, 2, 330, 14, 96, "陳慈祐", "412 張")
        Box(g, New RectangleF(440, 40, 116, 60), Color.FromArgb(234, 243, 253), Color.Empty, 6)
        TextIn(g, "照片人物欄" & vbLf & "一起改好", 11.5F, Accent, New RectangleF(440, 40, 116, 60))
    End Sub

    Private Shared Sub DrawSetup(ByVal g As Graphics)
        Box(g, New RectangleF(0, 0, DesignWidth, 262), Color.White, Line, 6)
        Box(g, New RectangleF(12, -1, 64, 26), Color.White, Line, 4)
        TextIn(g, "面孔", 12, Ink, New RectangleF(12, 0, 64, 24), StringAlignment.Center, FontStyle.Bold)
        Dim checks() As String = {"啟用人物辨識（下次開啟 iPhoto 生效）", "開啟 iPhoto 時，在背景分析新增或修改過的照片", "確認過的名字寫進照片的「人物」欄"}
        For i = 0 To 2
            Dim y As Single = 38 + i * 32
            Box(g, New RectangleF(24, y + 2, 16, 16), Accent, Accent, 3)
            Using pen As New Pen(Color.White, 2.2F)
                g.DrawLines(pen, {New PointF(27.5F, y + 10), New PointF(31, y + 13.5F), New PointF(36.5F, y + 6.5F)})
            End Using
            Text(g, checks(i), 13, Ink, 48, y)
        Next
        Dim ry As Single = 138
        Text(g, "自動認人", 13, Ink, 24, ry)
        Dim opts() As String = {"寬鬆", "平衡", "嚴格"}
        For i = 0 To 2
            Dim x As Single = 120 + i * 90
            Using pen As New Pen(If(i = 1, Accent, Gray), 1.5F), b As New SolidBrush(Accent)
                g.DrawEllipse(pen, x, ry + 2, 16, 16)
                If i = 1 Then g.FillEllipse(b, x + 4, ry + 6, 8, 8)
            End Using
            Text(g, opts(i), 13, Ink, x + 22, ry)
        Next
        Text(g, "程式認出的名字按 ✓ 確認後才會寫進照片的人物欄。", 11, Gray, 24, ry + 30)
        Text(g, "已分析 13,012 張照片，找到 27,325 張臉，12 人", 11.5F, Ink, 24, ry + 60)
        Using ft As Font = F(12.5F, FontStyle.Underline), b As New SolidBrush(NoRed)
            g.DrawString("清除面孔辨識資料…", ft, b, 24, ry + 88)
        End Using
        Using ft As Font = F(12.5F, FontStyle.Underline), b As New SolidBrush(Accent)
            g.DrawString("面孔功能使用說明…", ft, b, 330, ry + 88)
        End Using
    End Sub

    Private Shared Sub DrawStrict(ByVal g As Graphics)
        Dim names() As String = {"寬鬆", "平衡", "嚴格"}
        Dim found() As Single = {0.6F, 0.5F, 0.4F}
        Dim wrong() As Single = {0.07F, 0.06F, 0.06F}
        Dim barX As Single = 56, barW As Single = 290
        For i = 0 To 2
            Dim y As Single = 12 + i * 38
            Text(g, names(i), 13, Ink, 6, y + 3, FontStyle.Bold)
            Box(g, New RectangleF(barX, y, barW, 26), Color.FromArgb(240, 242, 245), Color.Empty, 4)
            Dim fw As Single = barW * found(i), ww As Single = fw * wrong(i)
            Box(g, New RectangleF(barX, y, fw - ww, 26), Color.FromArgb(120, 190, 140), Color.Empty, 4)
            Using b As New SolidBrush(NoRed)
                g.FillRectangle(b, barX + fw - ww, y, ww, 26)
            End Using
            Text(g, $"認出約 {found(i) * 100:0}%，其中約 {wrong(i) * 100:0}% 認錯", 11.5F, Ink, barX + barW + 8, y + 4)
        Next
        Using b As New SolidBrush(Color.FromArgb(120, 190, 140))
            g.FillRectangle(b, barX, 128, 14, 12)
        End Using
        Text(g, "程式自己認出", 11, Gray, barX + 18, 125)
        Using b As New SolidBrush(NoRed)
            g.FillRectangle(b, barX + 120, 128, 14, 12)
        End Using
        Text(g, "其中認錯（按 ✕ 更正）", 11, Gray, barX + 138, 125)
        Text(g, "灰色：等你命名或確認", 11, Gray, barX + 300, 125)
    End Sub

    Private Shared Sub DrawReject(ByVal g As Graphics)
        For panel = 0 To 1
            Dim x As Single = panel * 290
            Dim photo As RectangleF = ViewerPhoto(g, New RectangleF(x, 0, 270, 230))
            Person(g, 0, x + 135, 96, 30, photo.Bottom)
            Dim hb As RectangleF = HeadBox(x + 135, 96, 30)
            If panel = 0 Then
                FaceBox(g, hb, Matched, "弟弟？", True, True)
                Dim w As Single = TextWidth(g, "弟弟？", 12) + 14 + 44
                Pointer(g, x + 135 + w / 2 - 12, hb.Bottom + 16)
                Badge(g, 1, x + 30, 40)
            Else
                FaceBox(g, hb, Color.White, "", False, False, True)
                Dim cb As New RectangleF(x + 135 - 70, hb.Bottom + 6, 140, 26)
                Box(g, cb, Color.White, Accent, 2)
                Text(g, "哥哥", 13, Ink, cb.X + 6, cb.Y + 4)
                Badge(g, 2, x + 30, 40)
            End If
        Next
        Arrow(g, 272, 115, 288, 115, Accent)
    End Sub

    Private Shared Sub DrawAddFace(ByVal g As Graphics)
        Dim photo As RectangleF = ViewerPhoto(g, New RectangleF(0, 0, DesignWidth, 270))
        Person(g, 5, 170, 120, 26, photo.Bottom)
        Person(g, 3, 330, 126, 24, photo.Bottom, True)
        FaceBox(g, HeadBox(170, 120, 26), Color.White, "爸爸", False)
        ' the selection being dragged round the side face
        Dim sel As RectangleF = HeadBox(330, 126, 24)
        sel.Inflate(4, 4)
        Using pen As New Pen(Color.White, 1.5F) With {.DashStyle = DashStyle.Dot}, back As New Pen(Color.Black, 1.5F)
            g.DrawRectangle(back, sel.X, sel.Y, sel.Width, sel.Height)
            g.DrawRectangle(pen, sel.X, sel.Y, sel.Width, sel.Height)
        End Using
        Pointer(g, sel.Right - 2, sel.Bottom - 2)
        Badge(g, 1, sel.X - 6, sel.Y - 6)
        FaceBar(g, photo, "隱藏面孔", "", 1)
        Badge(g, 2, photo.Right - 196 - 8 + 74 + 37, photo.Y + 8 + 36)
        Box(g, New RectangleF(photo.X + 12, photo.Bottom - 40, 250, 28), Color.FromArgb(235, 255, 255, 255), Color.Empty, 6)
        TextIn(g, "拖曳框出臉（Shift 為正方形）", 11.5F, Ink, New RectangleF(photo.X + 12, photo.Bottom - 40, 250, 28))
    End Sub

End Class
