' The time line of the 地點 strip (new in the .NET port): a place may have hundreds of photos over many
' years, so above (or beside) the strip a bar chart of when they were taken -- a bar per month for up to
' 3 years, per quarter for up to 8, else per year; the bar of the photo on show is orange.
'   click a bar      the strip goes to the first photo of that time (a time outside the range: all again)
'   drag over bars   the strip keeps only that time; 「全部」 in the strip's title brings everything back
' Shown when the caller gives the photos' dates (ShowStrip's dates: the 地點 strip), not for 單螢幕 全圖.
' Design: https://claude.ai/artifact/MKb9gTRevHBDEgRDHr5cho (縮圖列的時間軸)
Partial Class frmViewerLarge

    Private ReadOnly tlStrip As New StripTimeline
    Private ReadOnly lnkAllTime As New LinkLabel
    ' everything ShowStrip was given (the strip may show part of it: the time range)
    Private m_lpAllGroups As List(Of (Title As String, Files As List(Of String), Rankings As List(Of Integer)))
    Private m_lpAllDates As List(Of Date?)          ' per photo, in the strip's order; Nothing = no time line
    Private ReadOnly m_lpShownGlobal As New List(Of Integer)   ' strip index -> index in the whole list
    Private m_strStripTitle As String = ""
    Private m_intRangeFrom As Integer = -1, m_intRangeTo As Integer = -1

    Private Sub BuildTimeline()
        tlStrip.Visible = False
        AddHandler tlStrip.BinClicked, AddressOf Timeline_BinClicked
        AddHandler tlStrip.RangeChosen, AddressOf Timeline_RangeChosen
        lnkAllTime.Text = "全部"
        lnkAllTime.AutoSize = True
        lnkAllTime.LinkColor = Color.FromArgb(245, 194, 107)
        lnkAllTime.ActiveLinkColor = Color.White
        lnkAllTime.BackColor = Color.Transparent
        lnkAllTime.Font = New Font("Microsoft JhengHei UI", 9.5F)
        lnkAllTime.Visible = False
        AddHandler lnkAllTime.LinkClicked, Sub() SetTimeRange(-1, -1)
        pnlGrip.Controls.Add(lnkAllTime)
        lnkAllTime.BringToFront()
        pnlStrip.Controls.Add(tlStrip)
    End Sub

    ''' <summary>(Re)fills the strip from m_lpAllGroups, keeping the photos in the time range.
    ''' <paramref name="selectGlobal"/>: the photo to select (index in the whole list), or the nearest kept.</summary>
    Private Sub FillStripItems(ByVal selectGlobal As Integer)
        Dim filtered As Boolean = m_intRangeFrom >= 0 AndAlso m_lpAllDates IsNot Nothing
        m_lpShownGlobal.Clear()
        mlStrip.Clear()
        mlStrip.BeginUpdate()
        Try
            Dim g As Integer = 0
            For Each grp In m_lpAllGroups
                Dim sectionAdded As Boolean = False
                For k = 0 To grp.Files.Count - 1
                    Dim here As Integer = g
                    g += 1
                    If filtered Then
                        Dim b As Integer = tlStrip.BinOf(m_lpAllDates(here))
                        If b < m_intRangeFrom OrElse b > m_intRangeTo Then Continue For
                    End If
                    If Not sectionAdded Then
                        mlStrip.AddSection(grp.Title)
                        sectionAdded = True
                    End If
                    Dim item As Aqua.MediaItem = mlStrip.AddItem(grp.Files(k))
                    Dim r As Integer = If(grp.Rankings IsNot Nothing AndAlso k < grp.Rankings.Count, grp.Rankings(k), 0)
                    item.Ranking = If(r >= 1 AndAlso r <= 5, CType(r, Aqua.MediaItemRanking), Aqua.MediaItemRanking.NoRating)
                    m_lpShownGlobal.Add(here)
                Next
            Next
        Finally
            mlStrip.EndUpdate()
        End Try
        ' the title: the place and how many, and the range when there is one
        lblStripTitle.Text = m_strStripTitle & If(filtered, "　只看 " & tlStrip.RangeName(m_intRangeFrom, m_intRangeTo) & "（" & mlStrip.Count & " 張）", "")
        lnkAllTime.Visible = filtered
        Dim textW As Integer = TextRenderer.MeasureText(lblStripTitle.Text, lblStripTitle.Font).Width + 8
        If filtered Then
            ' the title stops where 「全部」 starts (it would cover it)
            lnkAllTime.Left = Math.Max(40, Math.Min(pnlGrip.Width - lnkAllTime.Width - 8, lblStripTitle.Left + textW + 6))
            lnkAllTime.Top = (pnlGrip.Height - lnkAllTime.Height) \ 2
            lblStripTitle.Width = Math.Max(10, lnkAllTime.Left - lblStripTitle.Left - 4)
            lnkAllTime.BringToFront()
        Else
            lblStripTitle.Width = Math.Max(10, pnlGrip.Width - lblStripTitle.Left - 4)
        End If
        tlStrip.SetRange(If(filtered, m_intRangeFrom, -1), If(filtered, m_intRangeTo, -1))
        If mlStrip.Count = 0 Then Return
        Dim at As Integer = m_lpShownGlobal.FindIndex(Function(x) x >= selectGlobal)
        If at < 0 Then at = mlStrip.Count - 1
        mlStrip.SelectedIndex = at
        UpdateTimelineCurrent()
    End Sub

    ''' <summary>The orange bar: the time of the photo on show.</summary>
    Private Sub UpdateTimelineCurrent()
        If m_lpAllDates Is Nothing OrElse mlStrip.SelectedIndex < 0 OrElse mlStrip.SelectedIndex >= m_lpShownGlobal.Count Then Return
        tlStrip.CurrentBin = tlStrip.BinOf(m_lpAllDates(m_lpShownGlobal(mlStrip.SelectedIndex)))
    End Sub

    ''' <summary>A bar clicked: the first photo of that time (in the strip's order); outside the range, all again.</summary>
    Private Sub Timeline_BinClicked(ByVal bin As Integer)
        If m_lpAllDates Is Nothing Then Return
        Dim target As Integer = -1
        For i = 0 To m_lpAllDates.Count - 1
            If tlStrip.BinOf(m_lpAllDates(i)) = bin Then target = i : Exit For
        Next
        If target < 0 Then Return   ' an empty bar
        If m_intRangeFrom >= 0 AndAlso (bin < m_intRangeFrom OrElse bin > m_intRangeTo) Then
            m_intRangeFrom = -1 : m_intRangeTo = -1
            m_bolStripFilling = True
            Try
                FillStripItems(target)
            Finally
                m_bolStripFilling = False
            End Try
        End If
        Dim at As Integer = m_lpShownGlobal.IndexOf(target)
        If at < 0 Then Return
        mlStrip.SelectedIndex = at   ' raises StripPhotoSelected: the main window shows it
        mlStrip.EnsureVisible(at)
    End Sub

    Private Sub Timeline_RangeChosen(ByVal fromBin As Integer, ByVal toBin As Integer)
        SetTimeRange(fromBin, toBin)
    End Sub

    ''' <summary>Keeps only the photos of bins <paramref name="fromBin"/>..<paramref name="toBin"/> (-1: all),
    ''' and shows the first of them.</summary>
    Private Sub SetTimeRange(ByVal fromBin As Integer, ByVal toBin As Integer)
        If m_lpAllGroups Is Nothing OrElse (fromBin >= 0 AndAlso m_lpAllDates Is Nothing) Then Return
        Dim keep As Integer = If(mlStrip.SelectedIndex >= 0 AndAlso mlStrip.SelectedIndex < m_lpShownGlobal.Count, m_lpShownGlobal(mlStrip.SelectedIndex), 0)
        m_intRangeFrom = fromBin : m_intRangeTo = toBin
        Dim first As Integer = keep
        If fromBin >= 0 Then
            first = -1
            For i = 0 To m_lpAllDates.Count - 1
                Dim b As Integer = tlStrip.BinOf(m_lpAllDates(i))
                If b >= fromBin AndAlso b <= toBin Then first = i : Exit For
            Next
            If first < 0 Then Return   ' nothing there
        End If
        m_bolStripFilling = True
        Try
            FillStripItems(first)
        Finally
            m_bolStripFilling = False
        End Try
        If mlStrip.SelectedIndex >= 0 Then
            mlStrip.EnsureVisible(mlStrip.SelectedIndex)
            RaiseEvent StripPhotoSelected(mlStrip.SelectedIndex)
        End If
    End Sub

End Class

''' <summary>The bars of the 地點 strip's time line (frmViewerLarge.Timeline.vb). Lies along the strip:
''' left to right (strip at the top / bottom) or top to bottom (strip at the left / right).</summary>
Friend Class StripTimeline
    Inherits Control

    Public Event BinClicked(ByVal bin As Integer)
    Public Event RangeChosen(ByVal fromBin As Integer, ByVal toBin As Integer)

    Private m_intYear0 As Integer, m_intYears As Integer = 1
    Private m_intUnit As Integer = 1                ' months per bar: 1, 3 or 12
    Private m_counts As Integer() = {}
    Private m_intCurrent As Integer = -1
    Private m_intFrom As Integer = -1, m_intTo As Integer = -1
    Private m_intDragFrom As Integer = -1, m_intDragTo As Integer = -1
    Private m_intHover As Integer = -1
    Private ReadOnly m_tip As New ToolTip

    Public Sub New()
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
        BackColor = Color.FromArgb(22, 26, 31)
        Font = New Font("Microsoft JhengHei UI", 8.5F)
        Cursor = Cursors.Hand
    End Sub

    Public Property Vertical As Boolean

    ''' <summary>The photos' dates (Nothing for a photo without one): sets the years, the scale and the bars.
    ''' False when no photo has a date (then there is no time line).</summary>
    Public Function SetDates(ByVal dates As IEnumerable(Of Date?)) As Boolean
        Dim known = dates.Where(Function(d) d.HasValue).Select(Function(d) d.Value).ToList()
        If known.Count = 0 Then Return False
        m_intYear0 = known.Min().Year
        m_intYears = known.Max().Year - m_intYear0 + 1
        m_intUnit = If(m_intYears <= 3, 1, If(m_intYears <= 8, 3, 12))
        ReDim m_counts(m_intYears * 12 \ m_intUnit - 1)
        For Each d In known
            m_counts(BinOf(d)) += 1
        Next
        m_intCurrent = -1 : m_intFrom = -1 : m_intTo = -1
        Invalidate()
        Return True
    End Function

    Public ReadOnly Property ScaleText As String
        Get
            Return If(m_intUnit = 1, "每格一個月", If(m_intUnit = 3, "每格一季", "每格一年"))
        End Get
    End Property

    Public Function BinOf(ByVal d As Date?) As Integer
        If Not d.HasValue OrElse m_counts.Length = 0 Then Return -1
        Dim b As Integer = ((d.Value.Year - m_intYear0) * 12 + d.Value.Month - 1) \ m_intUnit
        Return If(b < 0 OrElse b >= m_counts.Length, -1, b)
    End Function

    Public Function BinName(ByVal b As Integer) As String
        Dim y As Integer = m_intYear0 + b * m_intUnit \ 12, m As Integer = (b * m_intUnit) Mod 12
        If m_intUnit = 12 Then Return y & " 年"
        If m_intUnit = 3 Then Return y & " 年第 " & (m \ 3 + 1) & " 季"
        Return y & " 年 " & (m + 1) & " 月"
    End Function

    Public Function RangeName(ByVal a As Integer, ByVal b As Integer) As String
        Return If(a = b, BinName(a), BinName(a) & " ～ " & BinName(b))
    End Function

    Public Property CurrentBin As Integer
        Get
            Return m_intCurrent
        End Get
        Set(value As Integer)
            If m_intCurrent = value Then Return
            m_intCurrent = value
            Invalidate()
        End Set
    End Property

    Public Sub SetRange(ByVal a As Integer, ByVal b As Integer)
        m_intFrom = a : m_intTo = b
        Invalidate()
    End Sub

    '==================================================================================================
    ' Drawing: the bars along the long side, the years beside them
    '==================================================================================================
    Private Const LabelBand As Integer = 16   ' the years: below (horizontal) / left (vertical) -- in pixels
    Private Const Pad As Integer = 6

    ''' <summary>The bars' area: along the long side, less the years' band.</summary>
    Private ReadOnly Property BarArea As Rectangle
        Get
            If Vertical Then Return New Rectangle(Pad + 34, Pad, Math.Max(1, Width - Pad * 2 - 34), Math.Max(1, Height - Pad * 2))
            Return New Rectangle(Pad, Pad, Math.Max(1, Width - Pad * 2), Math.Max(1, Height - Pad - LabelBand - 2))
        End Get
    End Property

    Private Function BinRect(ByVal b As Integer) As RectangleF
        Dim a As Rectangle = BarArea
        Dim n As Integer = Math.Max(1, m_counts.Length)
        If Vertical Then
            Dim h As Single = a.Height / CSng(n)
            Return New RectangleF(a.X, a.Y + b * h, a.Width, h)
        End If
        Dim w As Single = a.Width / CSng(n)
        Return New RectangleF(a.X + b * w, a.Y, w, a.Height)
    End Function

    Private Function BinAt(ByVal p As Point) As Integer
        If m_counts.Length = 0 Then Return -1
        Dim a As Rectangle = BarArea
        Dim f As Double = If(Vertical, (p.Y - a.Y) / CDbl(a.Height), (p.X - a.X) / CDbl(a.Width))
        Return Math.Max(0, Math.Min(m_counts.Length - 1, CInt(Math.Floor(f * m_counts.Length))))
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        If m_counts.Length = 0 Then Return
        Dim g As Graphics = e.Graphics
        Dim max As Integer = Math.Max(1, m_counts.Max())
        Dim lo As Integer = If(m_intDragFrom >= 0, Math.Min(m_intDragFrom, m_intDragTo), m_intFrom)
        Dim hi As Integer = If(m_intDragFrom >= 0, Math.Max(m_intDragFrom, m_intDragTo), m_intTo)
        Using inRange As New SolidBrush(Color.FromArgb(46, 245, 194, 107)), barBlue As New SolidBrush(Color.FromArgb(90, 143, 214)),
              barRange As New SolidBrush(Color.FromArgb(245, 194, 107)), barCur As New SolidBrush(Color.FromArgb(245, 158, 11)),
              barEmpty As New SolidBrush(Color.FromArgb(44, 51, 59)), hover As New SolidBrush(Color.FromArgb(30, 255, 255, 255))
            For b = 0 To m_counts.Length - 1
                Dim r As RectangleF = BinRect(b)
                Dim inR As Boolean = lo >= 0 AndAlso b >= lo AndAlso b <= hi
                If inR Then g.FillRectangle(inRange, r)
                If b = m_intHover Then g.FillRectangle(hover, r)
                Dim c As Integer = m_counts(b)
                Dim len As Single = If(c = 0, 2.0F, Math.Max(3.0F, CSng(If(Vertical, r.Width, r.Height) * c / max)))
                Dim gap As Single = If(m_counts.Length > 60, 0.0F, 1.0F)
                Dim bar As RectangleF = If(Vertical, New RectangleF(r.X, r.Y + gap / 2, len, Math.Max(1.0F, r.Height - gap)),
                                                     New RectangleF(r.X + gap / 2, r.Bottom - len, Math.Max(1.0F, r.Width - gap), len))
                g.FillRectangle(If(b = m_intCurrent, barCur, If(c = 0, barEmpty, If(inR, barRange, barBlue))), bar)
            Next
            ' the years
            Using pen As New Pen(Color.FromArgb(58, 66, 75)), txt As New SolidBrush(Color.FromArgb(154, 164, 174))
                Dim every As Integer = If(m_intYears > 12, 2, 1)
                For y = 0 To m_intYears - 1
                    If y Mod every <> 0 AndAlso y <> m_intYears - 1 Then Continue For
                    Dim r As RectangleF = BinRect(y * 12 \ m_intUnit)
                    If Vertical Then
                        g.DrawLine(pen, Pad, r.Y, Width - Pad, r.Y)
                        g.DrawString((m_intYear0 + y).ToString(), Font, txt, Pad, r.Y + 1)
                    Else
                        g.DrawLine(pen, r.X, Height - LabelBand - 2, r.X, Height - 2)
                        g.DrawString((m_intYear0 + y).ToString(), Font, txt, r.X + 2, Height - LabelBand - 1)
                    End If
                Next
            End Using
        End Using
    End Sub

    '==================================================================================================
    ' Mouse: a click jumps, a drag chooses a range
    '==================================================================================================
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button <> MouseButtons.Left OrElse m_counts.Length = 0 Then Return
        m_intDragFrom = BinAt(e.Location)
        m_intDragTo = m_intDragFrom
        Capture = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If m_counts.Length = 0 Then Return
        Dim b As Integer = BinAt(e.Location)
        If m_intDragFrom >= 0 Then
            If b <> m_intDragTo Then
                m_intDragTo = b
                Invalidate()
            End If
        ElseIf b <> m_intHover Then
            m_intHover = b
            m_tip.SetToolTip(Me, BinName(b) & "：" & m_counts(b) & " 張")
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If m_intDragFrom < 0 Then Return
        Capture = False
        Dim a As Integer = Math.Min(m_intDragFrom, m_intDragTo), b As Integer = Math.Max(m_intDragFrom, m_intDragTo)
        m_intDragFrom = -1 : m_intDragTo = -1
        Invalidate()
        If a = b Then RaiseEvent BinClicked(a) Else RaiseEvent RangeChosen(a, b)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If m_intHover >= 0 Then
            m_intHover = -1
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then m_tip.Dispose()
        MyBase.Dispose(disposing)
    End Sub

End Class
