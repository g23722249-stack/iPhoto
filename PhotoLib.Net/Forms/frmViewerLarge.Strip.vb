' 地點 mode of 全圖瀏覽 (new in the .NET port): while the 地點 window (iPhoto frmMap) is open, the viewer
' gets a thumbnail strip with the photos sent from there (a heading per day), and the main window steers
' the viewer from the strip instead of from its own list.
'   - Everything the designer put on the form is moved into pnlContent (same bounds, so the anchors hold);
'     the strip docks beside it. Hidden strip: pnlContent fills the window as before.
'   - The strip's grip (⋮⋮) is dragged to any edge of the window: the strip docks there and
'     g_lpConfig.ViewerStripDock remembers it (iPhoto.Ini [Viewer] StripDock).
'   - A right click on the photo opens PhotoMenu (set by the main window: 照片資訊, 設定拍攝地點,
'     指定人物, 我的評價 ...), unless face mode took the click.
Partial Class frmViewerLarge

    ''' <summary>A photo of the strip was clicked (or chosen with the arrow keys).</summary>
    Public Event StripPhotoSelected(ByVal index As Integer)

    ''' <summary>The menu a right click on the photo opens (Nothing: no menu).</summary>
    Public Property PhotoMenu As ContextMenuStrip

    Private pnlContent As Panel
    Private ReadOnly pnlStrip As New Panel
    Private ReadOnly pnlGrip As New Panel
    Private ReadOnly lblStripTitle As New Label
    Private ReadOnly mlStrip As New Aqua.MediaList
    Private m_bolStripFilling As Boolean

    Private ReadOnly Property ContentWidth As Integer
        Get
            Return If(pnlContent IsNot Nothing, pnlContent.ClientSize.Width, ClientSize.Width)
        End Get
    End Property

    Protected Overrides Sub OnLoad(e As EventArgs)
        BuildStrip()   ' before the Load handlers lay out the bottom bar
        MyBase.OnLoad(e)
    End Sub

    Private Sub BuildStrip()
        If pnlContent IsNot Nothing Then Return
        SuspendLayout()
        pnlContent = New Panel With {.Bounds = New Rectangle(Point.Empty, ClientSize), .BackColor = Color.Black}
        For Each c As Control In Controls.Cast(Of Control)().ToList()
            Dim b As Rectangle = c.Bounds
            Controls.Remove(c)
            pnlContent.Controls.Add(c)
            c.Bounds = b
        Next
        Controls.Add(pnlContent)
        pnlContent.Dock = DockStyle.Fill
        AddHandler pnlContent.Resize, Sub() LayoutBottomBar()

        pnlStrip.BackColor = Color.FromArgb(22, 26, 31)
        pnlStrip.Visible = False
        pnlGrip.BackColor = Color.FromArgb(32, 37, 43)
        pnlGrip.Cursor = Cursors.SizeAll
        AddHandler pnlGrip.Paint, AddressOf Grip_Paint
        AddHandler pnlGrip.MouseDown, AddressOf Grip_MouseDown
        AddHandler pnlGrip.MouseMove, AddressOf Grip_MouseMove
        AddHandler pnlGrip.MouseUp, AddressOf Grip_MouseUp
        lblStripTitle.ForeColor = Color.FromArgb(154, 164, 174)
        lblStripTitle.BackColor = Color.Transparent
        lblStripTitle.AutoSize = False
        lblStripTitle.TextAlign = ContentAlignment.MiddleLeft
        lblStripTitle.Font = New Font("Microsoft JhengHei UI", 9.5F)
        AddHandler lblStripTitle.MouseDown, AddressOf Grip_MouseDown
        AddHandler lblStripTitle.MouseMove, AddressOf Grip_MouseMove
        AddHandler lblStripTitle.MouseUp, AddressOf Grip_MouseUp
        lblStripTitle.Cursor = Cursors.SizeAll
        pnlGrip.Controls.Add(lblStripTitle)

        mlStrip.BackColor = Color.FromArgb(22, 26, 31)
        mlStrip.ShowCheckBox = False
        mlStrip.ShowRating = True
        mlStrip.DragItem = False
        mlStrip.DropItem = False
        mlStrip.SectionForeColor = Color.FromArgb(230, 233, 236)
        mlStrip.SectionSubColor = Color.FromArgb(154, 164, 174)
        mlStrip.SectionHeaderHeight = 28
        AddHandler mlStrip.SelectedChanged, Sub()
                                                If mlStrip.SelectedIndex >= 0 Then UpdateTimelineCurrent()   ' frmViewerLarge.Timeline.vb
                                                If m_bolStripFilling OrElse mlStrip.SelectedIndex < 0 Then Return
                                                RaiseEvent StripPhotoSelected(mlStrip.SelectedIndex)
                                            End Sub
        pnlStrip.Controls.Add(mlStrip)
        BuildTimeline()   ' between the grip and the thumbnails
        pnlStrip.Controls.Add(pnlGrip)
        Controls.Add(pnlStrip)
        pnlContent.BringToFront()   ' docking: the strip takes its edge, the content fills the rest
        ApplyStripDock()
        ResumeLayout(True)

        AddHandler imgPhoto.MouseUp, AddressOf PhotoMenu_MouseUp
        AddHandler picPhoto.MouseUp, AddressOf PhotoMenu_MouseUp
    End Sub

    '==================================================================================================
    ' Showing photos in the strip
    '==================================================================================================
    ''' <summary>True while the strip is up (地點 mode).</summary>
    Public ReadOnly Property StripVisible As Boolean
        Get
            Return pnlStrip.Visible
        End Get
    End Property

    ''' <summary>Fills the strip: one section per group (title: "2024年3月16日 星期六" & vbTab & "8 張"),
    ''' <paramref name="title"/> above it, and shows it. Selects <paramref name="select"/> without raising
    ''' StripPhotoSelected (the caller shows that photo). <paramref name="dates"/> (one per photo, in the
    ''' strip's order): the time line above the strip (frmViewerLarge.Timeline.vb); Nothing = none.</summary>
    Public Sub ShowStrip(ByVal title As String, ByVal groups As IEnumerable(Of (Title As String, Files As List(Of String), Rankings As List(Of Integer))),
                         Optional ByVal [select] As Integer = 0, Optional ByVal dates As IList(Of Date?) = Nothing)
        If pnlContent Is Nothing Then BuildStrip()
        m_bolStripFilling = True
        Try
            m_strStripTitle = title
            m_lpAllGroups = groups.Where(Function(g) g.Files.Count > 0).ToList()
            m_lpAllDates = If(dates Is Nothing, Nothing, dates.ToList())
            m_intRangeFrom = -1 : m_intRangeTo = -1
            ' (a local: Visible reads False while the strip panel is still hidden)
            Dim withTime As Boolean = m_lpAllDates IsNot Nothing AndAlso tlStrip.SetDates(m_lpAllDates)
            tlStrip.Visible = withTime
            If Not withTime Then m_lpAllDates = Nothing
            pnlStrip.Visible = True
            ApplyStripDock()
            FillStripItems(Math.Max(0, [select]))
        Finally
            m_bolStripFilling = False
        End Try
    End Sub

    ''' <summary>The strip goes away (the 地點 window closed).</summary>
    Public Sub HideStrip()
        m_bolStripFilling = True
        Try
            mlStrip.Clear()
        Finally
            m_bolStripFilling = False
        End Try
        m_lpAllGroups = Nothing
        m_lpAllDates = Nothing
        m_lpShownGlobal.Clear()
        tlStrip.Visible = False
        lnkAllTime.Visible = False
        pnlStrip.Visible = False
        MovePictureToScreen()
    End Sub

    Public ReadOnly Property StripCount As Integer
        Get
            Return mlStrip.Count
        End Get
    End Property

    Public ReadOnly Property StripIndex As Integer
        Get
            Return mlStrip.SelectedIndex
        End Get
    End Property

    Public Function StripFile(ByVal index As Integer) As String
        Return If(index >= 0 AndAlso index < mlStrip.Count, mlStrip.Item(index).FileName, "")
    End Function

    ''' <summary>Moves the strip's selection to <paramref name="index"/> without raising StripPhotoSelected.</summary>
    Public Sub SelectStrip(ByVal index As Integer)
        If index < 0 OrElse index >= mlStrip.Count Then Return
        m_bolStripFilling = True
        Try
            mlStrip.SelectedIndex = index
        Finally
            m_bolStripFilling = False
        End Try
    End Sub

    Public Sub SetStripRanking(ByVal index As Integer, ByVal level As Integer)
        If index < 0 OrElse index >= mlStrip.Count Then Return
        mlStrip.Item(index).Ranking = If(level >= 1 AndAlso level <= 5, CType(level, Aqua.MediaItemRanking), Aqua.MediaItemRanking.NoRating)
    End Sub

    ''' <summary>The photo at <paramref name="index"/> again (rotated / restored).</summary>
    Public Sub RefreshStripItem(ByVal index As Integer)
        If index < 0 OrElse index >= mlStrip.Count Then Return
        Dim f As String = mlStrip.Item(index).FileName
        mlStrip.Item(index).FileName = ""
        mlStrip.Item(index).FileName = f
    End Sub

    ''' <summary>The photo at <paramref name="index"/> left the strip (deleted).</summary>
    Public Sub RemoveStripItem(ByVal index As Integer)
        If index < 0 OrElse index >= mlStrip.Count Then Return
        m_bolStripFilling = True
        Try
            If m_lpAllGroups Is Nothing OrElse index >= m_lpShownGlobal.Count Then
                mlStrip.RemoveAt(index)
                Return
            End If
            ' out of everything the strip was given (the time line counts it no more), then again
            Dim g As Integer = m_lpShownGlobal(index), at As Integer = 0
            For i = 0 To m_lpAllGroups.Count - 1
                Dim grp = m_lpAllGroups(i)
                If g < at + grp.Files.Count Then
                    grp.Files.RemoveAt(g - at)
                    If grp.Rankings IsNot Nothing AndAlso g - at < grp.Rankings.Count Then grp.Rankings.RemoveAt(g - at)
                    If grp.Files.Count = 0 Then m_lpAllGroups.RemoveAt(i)
                    Exit For
                End If
                at += grp.Files.Count
            Next
            If m_lpAllDates IsNot Nothing Then
                m_lpAllDates.RemoveAt(g)
                tlStrip.SetDates(m_lpAllDates)
            End If
            FillStripItems(g)
        Finally
            m_bolStripFilling = False
        End Try
    End Sub

    '==================================================================================================
    ' Docking: drag the grip to an edge
    '==================================================================================================
    Private Const StripThickH As Integer = 150   ' top / bottom: the strip's height
    Private Const StripThickV As Integer = 220   ' left / right: its width
    Private Const GripSize As Integer = 26
    Private Const TimelineThickH As Integer = 64   ' the time line under the title (strip at the top / bottom)
    Private Const TimelineThickV As Integer = 84   ' beside the thumbnails (strip at the left / right)

    ''' <summary>True: the strip of 全圖 in the main window (單螢幕), which remembers its own place
    ''' (Config.ViewerStripDockSingle); False: the 地點 strip (Config.ViewerStripDock).</summary>
    Public Property StripSingleScreen As Boolean

    Private Property StripDock As DockStyle
        Get
            If g_lpConfig Is Nothing Then Return If(StripSingleScreen, DockStyle.Top, DockStyle.Bottom)
            Return If(StripSingleScreen, g_lpConfig.ViewerStripDockSingle, g_lpConfig.ViewerStripDock)
        End Get
        Set(value As DockStyle)
            If g_lpConfig Is Nothing Then Return
            If StripSingleScreen Then g_lpConfig.ViewerStripDockSingle = value Else g_lpConfig.ViewerStripDock = value
        End Set
    End Property

    Private Sub ApplyStripDock()
        Dim d As DockStyle = StripDock
        Dim side As Boolean = d = DockStyle.Left OrElse d = DockStyle.Right
        Dim withTime As Boolean = m_lpAllDates IsNot Nothing   ' the 地點 strip's time line (frmViewerLarge.Timeline.vb)
        SuspendLayout()
        pnlStrip.Dock = d
        If side Then pnlStrip.Width = StripThickV + If(withTime, TimelineThickV, 0) Else pnlStrip.Height = StripThickH + If(withTime, TimelineThickH, 0)
        pnlGrip.Dock = DockStyle.Top
        pnlGrip.Height = GripSize
        lblStripTitle.SetBounds(30, 0, If(side, StripThickV + TimelineThickV, 1200) - 34, GripSize)
        ' the time line: under the title (top / bottom), or on the strip's outer side (left / right)
        tlStrip.Vertical = side
        tlStrip.Dock = If(Not side, DockStyle.Top, If(d = DockStyle.Left, DockStyle.Left, DockStyle.Right))
        If side Then tlStrip.Width = TimelineThickV Else tlStrip.Height = TimelineThickH
        ' docking goes from the back: the title first, then the time line, the thumbnails fill the rest
        tlStrip.SendToBack()
        pnlGrip.SendToBack()
        mlStrip.Dock = DockStyle.Fill
        mlStrip.BringToFront()
        mlStrip.ScrollDirection = If(side, Aqua.MediaScrollDirection.Vertical, Aqua.MediaScrollDirection.Horizontal)
        mlStrip.Limit = If(side, 1, 1)   ' one per row (side) / one per column (top, bottom): items fill the strip's thickness
        mlStrip.AspectRatio = "4:3"
        ResumeLayout(True)
        pnlGrip.Invalidate()
        If m_lpImage IsNot Nothing AndAlso m_enumMediaMode = enumMediaMode.mmImage Then MovePictureToScreen()
    End Sub

    Private Sub Grip_Paint(sender As Object, e As PaintEventArgs)
        ' ⋮⋮ the handle
        Using b As New SolidBrush(Color.FromArgb(120, 130, 140))
            For col = 0 To 1
                For row = 0 To 2
                    e.Graphics.FillEllipse(b, 10 + col * 6, 6 + row * 5, 3, 3)
                Next
            Next
        End Using
        Using p As New Pen(Color.FromArgb(38, 43, 49))
            e.Graphics.DrawLine(p, 0, pnlGrip.Height - 1, pnlGrip.Width, pnlGrip.Height - 1)
        End Using
    End Sub

    Private m_bolGripDrag As Boolean
    Private m_ptGripStart As Point
    Private m_frmDockHint As Form

    Private Sub Grip_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then Return
        m_bolGripDrag = True
        m_ptGripStart = Cursor.Position
        CType(sender, Control).Capture = True
    End Sub

    Private Sub Grip_MouseMove(sender As Object, e As MouseEventArgs)
        If Not m_bolGripDrag Then Return
        Dim p As Point = Cursor.Position
        If Math.Abs(p.X - m_ptGripStart.X) + Math.Abs(p.Y - m_ptGripStart.Y) < 12 Then Return
        ShowDockHint(DockAt(PointToClient(p)))
    End Sub

    Private Sub Grip_MouseUp(sender As Object, e As MouseEventArgs)
        If Not m_bolGripDrag Then Return
        m_bolGripDrag = False
        CType(sender, Control).Capture = False
        Dim moved As Boolean = m_frmDockHint IsNot Nothing AndAlso m_frmDockHint.Visible
        HideDockHint()
        If Not moved Then Return
        Dim d As DockStyle = DockAt(PointToClient(Cursor.Position))
        If g_lpConfig IsNot Nothing AndAlso d <> StripDock Then
            StripDock = d
            ApplyStripDock()
            If mlStrip.SelectedIndex >= 0 Then mlStrip.EnsureVisible(mlStrip.SelectedIndex)
        End If
    End Sub

    ''' <summary>The edge nearest to a point of the window.</summary>
    Private Function DockAt(ByVal p As Point) As DockStyle
        Dim w As Integer = Math.Max(1, ClientSize.Width), h As Integer = Math.Max(1, ClientSize.Height)
        Dim fx As Double = p.X / w, fy As Double = p.Y / h
        Dim dl As Double = fx, dr As Double = 1 - fx, dt As Double = fy, db As Double = 1 - fy
        Dim m As Double = Math.Min(Math.Min(dl, dr), Math.Min(dt, db))
        If m = dt Then Return DockStyle.Top
        If m = db Then Return DockStyle.Bottom
        If m = dl Then Return DockStyle.Left
        Return DockStyle.Right
    End Function

    ''' <summary>A see-through blue band where the strip would go.</summary>
    Private Sub ShowDockHint(ByVal d As DockStyle)
        If m_frmDockHint Is Nothing Then
            m_frmDockHint = New Form With {.FormBorderStyle = FormBorderStyle.None, .ShowInTaskbar = False, .StartPosition = FormStartPosition.Manual,
                                           .BackColor = Color.FromArgb(29, 95, 180), .Opacity = 0.35, .TopMost = True}
        End If
        Dim c As Rectangle = RectangleToScreen(ClientRectangle)
        Dim side As Boolean = d = DockStyle.Left OrElse d = DockStyle.Right
        Dim r As Rectangle
        Select Case d
            Case DockStyle.Top : r = New Rectangle(c.X, c.Y, c.Width, StripThickH)
            Case DockStyle.Left : r = New Rectangle(c.X, c.Y, StripThickV, c.Height)
            Case DockStyle.Right : r = New Rectangle(c.Right - StripThickV, c.Y, StripThickV, c.Height)
            Case Else : r = New Rectangle(c.X, c.Bottom - StripThickH, c.Width, StripThickH)
        End Select
        m_frmDockHint.Bounds = r
        If Not m_frmDockHint.Visible Then m_frmDockHint.Show(Me)
    End Sub

    Private Sub HideDockHint()
        If m_frmDockHint IsNot Nothing Then m_frmDockHint.Hide()
    End Sub

    '==================================================================================================
    ' Right click on the photo
    '==================================================================================================
    Private Sub PhotoMenu_MouseUp(sender As Object, e As MouseEventArgs)
        If e.Button <> MouseButtons.Right OrElse PhotoMenu Is Nothing OrElse m_strFileName = "" Then Return
        If sender Is imgPhoto AndAlso m_bolFaceTookClick Then Return   ' a face's own menu (frmViewerLarge.Faces.vb)
        PhotoMenu.Show(CType(sender, Control), e.Location)
    End Sub

    ''' <summary>Set by imgPhoto_MouseDown: face mode handled the click (on a face box).</summary>
    Private m_bolFaceTookClick As Boolean

    ''' <summary>The photo on show ("" when none).</summary>
    Public ReadOnly Property CurrentFile As String
        Get
            Return m_strFileName
        End Get
    End Property

End Class
