Imports System.Drawing.Drawing2D
Imports System.Threading

' Controls of the new import window (frmImportStudio), drawn in code:
'   ImportThumbCache -- thumbnails of the source files, made on one background (STA) thread, newest
'                       request first (what is on screen), shared by both grids;
'   ImportThumbGrid  -- the photos as thumbnails, grouped by day, with a check box (step 1), badges
'                       (已有 / 影片 / faces / edited) and multi-select (Ctrl / Shift / Ctrl+A);
'   ImportFaceStage  -- one photo large, its faces boxed: green = the name goes into the people field,
'                       dashed yellow = 「是 X 嗎？」 with ✓ / ✕, dashed white = nobody; click a box to
'                       name it, drag on the photo to box a face by hand;
'   ImportStepBar    -- ① 選照片 ② 整理 ③ 匯入;
'   ImportFolderTree -- 桌面 / 圖片 / 下載 / the drives (a drive with a DCIM folder is marked 相機);
'   ImportChip       -- a rounded label (people, groups).
Friend Module ImportStudioColors
    Public ReadOnly Accent As Color = Color.FromArgb(42, 111, 203)
    Public ReadOnly AccentSoft As Color = Color.FromArgb(225, 236, 250)
    Public ReadOnly Green As Color = Color.FromArgb(35, 150, 95)
    Public ReadOnly GreenSoft As Color = Color.FromArgb(221, 241, 229)
    Public ReadOnly Amber As Color = Color.FromArgb(214, 150, 30)
    Public ReadOnly AmberSoft As Color = Color.FromArgb(251, 240, 218)
    Public ReadOnly Ground As Color = Color.FromArgb(236, 239, 243)
    Public ReadOnly Line As Color = Color.FromArgb(201, 208, 217)
    Public ReadOnly Muted As Color = Color.FromArgb(94, 105, 119)
    Public ReadOnly Ink As Color = Color.FromArgb(30, 36, 44)
    Public ReadOnly Stage As Color = Color.FromArgb(43, 48, 56)

    Public Function RoundRect(ByVal r As RectangleF, ByVal radius As Single) As GraphicsPath
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
End Module

'======================================================================================================
' Thumbnails
'======================================================================================================
Friend Class ImportThumbCache
    Implements IDisposable

    Public Const ThumbWidth As Integer = 160
    Public Const ThumbHeight As Integer = 120

    ''' <summary>Raised on the UI thread when a thumbnail is ready.</summary>
    Public Event Loaded(ByVal strFile As String)

    Private ReadOnly m_thumbs As New Dictionary(Of String, Bitmap)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly m_wanted As New List(Of String)
    Private ReadOnly m_lock As New Object
    Private ReadOnly m_signal As New AutoResetEvent(False)
    Private ReadOnly m_context As SynchronizationContext
    Private ReadOnly m_thread As Thread
    Private m_bolStop As Boolean

    Public Sub New()
        m_context = If(SynchronizationContext.Current, New SynchronizationContext())
        m_thread = New Thread(AddressOf Work) With {.IsBackground = True, .Name = "ImportThumbCache", .Priority = ThreadPriority.BelowNormal}
        m_thread.SetApartmentState(ApartmentState.STA)   ' the Shell's thumbnailers want STA
        m_thread.Start()
    End Sub

    Public Function TryGet(ByVal strFile As String, ByRef bmp As Bitmap) As Boolean
        SyncLock m_lock
            Return m_thumbs.TryGetValue(strFile, bmp)
        End SyncLock
    End Function

    ''' <summary>Asks for the thumbnail; the newest request is made first.</summary>
    Public Sub Request(ByVal strFile As String)
        SyncLock m_lock
            If m_thumbs.ContainsKey(strFile) Then Return
            m_wanted.Remove(strFile)
            m_wanted.Add(strFile)
        End SyncLock
        m_signal.Set()
    End Sub

    Private Sub Work()
        Do
            m_signal.WaitOne()
            Do
                If m_bolStop Then Return
                Dim f As String = Nothing
                SyncLock m_lock
                    If m_wanted.Count = 0 Then Exit Do
                    f = m_wanted(m_wanted.Count - 1)
                    m_wanted.RemoveAt(m_wanted.Count - 1)
                End SyncLock
                Dim bmp As Bitmap = Quartz.Thumbnail.ShellThumbnail(f, ThumbWidth, ThumbHeight)
                If bmp Is Nothing Then bmp = New Bitmap(1, 1)
                SyncLock m_lock
                    m_thumbs(f) = bmp
                End SyncLock
                Dim done As String = f
                Try
                    m_context.Post(Sub(s) RaiseEvent Loaded(done), Nothing)
                Catch ex As InvalidOperationException
                End Try
            Loop
        Loop Until m_bolStop
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        m_bolStop = True
        m_signal.Set()
        m_thread.Join(2000)
        SyncLock m_lock
            For Each b In m_thumbs.Values
                b.Dispose()
            Next
            m_thumbs.Clear()
        End SyncLock
    End Sub

End Class

'======================================================================================================
' Thumbnail grid
'======================================================================================================
Friend Class ImportThumbGrid
    Inherits Control

    Public Event SelectionChanged As EventHandler
    Public Event IncludeChanged As EventHandler

    Private m_items As New List(Of ImportItem)
    Private m_cache As ImportThumbCache
    Private ReadOnly m_selected As New HashSet(Of ImportItem)
    Private m_focus As ImportItem
    Private m_anchor As ImportItem
    Private ReadOnly m_scroll As New VScrollBar With {.Dock = DockStyle.Right, .SmallChange = 40}
    Private ReadOnly m_cells As New List(Of (Rect As Rectangle, Item As ImportItem))
    Private ReadOnly m_headers As New List(Of (Rect As Rectangle, Text As String, Sub_ As String))
    Private m_intContentHeight As Integer

    Public Property ShowChecks As Boolean = True
    Public Property ShowFaceBadges As Boolean
    Public Property CellSize As New Size(132, 99)
    Public Property GroupByDay As Boolean = True
    ''' <summary>Only the items it accepts are shown (Nothing = all).</summary>
    Public Property Filter As Func(Of ImportItem, Boolean)
    ''' <summary>Header while the dates are read.</summary>
    Public Property PendingText As String = ""

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or
                 ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
        BackColor = Color.FromArgb(252, 253, 254)
        Controls.Add(m_scroll)
        AddHandler m_scroll.ValueChanged, Sub() Invalidate()
    End Sub

    Public Property Cache As ImportThumbCache
        Get
            Return m_cache
        End Get
        Set(value As ImportThumbCache)
            If m_cache IsNot Nothing Then RemoveHandler m_cache.Loaded, AddressOf Cache_Loaded
            m_cache = value
            If m_cache IsNot Nothing Then AddHandler m_cache.Loaded, AddressOf Cache_Loaded
        End Set
    End Property

    Private Sub Cache_Loaded(ByVal strFile As String)
        For Each c In m_cells
            If String.Equals(c.Item.SourceFile, strFile, StringComparison.OrdinalIgnoreCase) Then
                Invalidate(New Rectangle(c.Rect.X, c.Rect.Y - m_scroll.Value, c.Rect.Width, c.Rect.Height))
                Exit For
            End If
        Next
    End Sub

    Public Sub SetItems(ByVal items As List(Of ImportItem))
        m_items = items
        m_selected.Clear()
        m_focus = Nothing
        m_anchor = Nothing
        m_scroll.Value = 0
        RefreshLayout()
    End Sub

    ''' <summary>The items shown, in their order.</summary>
    Public Function ShownItems() As List(Of ImportItem)
        Return m_cells.Select(Function(c) c.Item).ToList()
    End Function

    Public ReadOnly Property SelectedItems As List(Of ImportItem)
        Get
            Return m_cells.Select(Function(c) c.Item).Where(Function(i) m_selected.Contains(i)).ToList()
        End Get
    End Property

    Public ReadOnly Property FocusItem As ImportItem
        Get
            Return m_focus
        End Get
    End Property

    Public Sub SelectOnly(ByVal item As ImportItem)
        m_selected.Clear()
        If item IsNot Nothing Then m_selected.Add(item)
        m_focus = item
        m_anchor = item
        EnsureVisible(item)
        Invalidate()
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub SelectAll()
        For Each c In m_cells
            m_selected.Add(c.Item)
        Next
        If m_focus Is Nothing AndAlso m_cells.Count > 0 Then m_focus = m_cells(0).Item
        Invalidate()
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Moves the focus by <paramref name="delta"/> items (single selection).</summary>
    Public Sub MoveFocus(ByVal delta As Integer)
        Dim shown = ShownItems()
        If shown.Count = 0 Then Return
        Dim i As Integer = If(m_focus Is Nothing, -1, shown.IndexOf(m_focus))
        i = Math.Max(0, Math.Min(shown.Count - 1, i + delta))
        SelectOnly(shown(i))
    End Sub

    Public Sub RefreshLayout()
        m_cells.Clear()
        m_headers.Clear()
        Dim shown = m_items.Where(Function(i) Filter Is Nothing OrElse Filter(i)).ToList()
        Dim w As Integer = ClientSize.Width - m_scroll.Width - 8
        Dim cw As Integer = CellSize.Width + 8, ch As Integer = CellSize.Height + 8
        Dim cols As Integer = Math.Max(1, w \ cw)
        Dim x0 As Integer = 6 + Math.Max(0, (w - cols * cw) \ 2)
        Dim y As Integer = 6

        Dim groups As New List(Of (Title As String, Sub_ As String, Items As List(Of ImportItem)))
        If Not GroupByDay OrElse shown.Any(Function(i) Not i.InfoRead) Then
            groups.Add((If(GroupByDay, PendingText, ""), "", shown))
        Else
            For Each g In shown.GroupBy(Function(i) If(i.ShotDate.HasValue, i.ShotDate.Value.Date, Date.MinValue)).OrderBy(Function(x) x.Key)
                Dim title As String = If(g.Key = Date.MinValue, "沒有日期", ImportSource.DayText(g.Key) & "（" & "日一二三四五六"(g.Key.DayOfWeek) & "）")
                Dim place As String = ImportSource.CommonPlace(g)
                groups.Add((title, g.Count() & " 張" & If(place <> "", " · " & place, ""), g.ToList()))
            Next
        End If
        For Each g In groups
            If g.Title <> "" Then
                m_headers.Add((New Rectangle(6, y, w, 24), g.Title, g.Sub_))
                y += 28
            End If
            For k = 0 To g.Items.Count - 1
                Dim c As Integer = k Mod cols
                If k > 0 AndAlso c = 0 Then y += ch
                m_cells.Add((New Rectangle(x0 + c * cw, y, CellSize.Width, CellSize.Height), g.Items(k)))
            Next
            If g.Items.Count > 0 Then y += ch
            y += 4
        Next
        m_intContentHeight = y
        m_selected.RemoveWhere(Function(i) Not shown.Contains(i))
        If m_focus IsNot Nothing AndAlso Not shown.Contains(m_focus) Then m_focus = Nothing
        UpdateScroll()
        Invalidate()
    End Sub

    Private Sub UpdateScroll()
        Dim h As Integer = ClientSize.Height
        m_scroll.Minimum = 0
        m_scroll.LargeChange = Math.Max(1, h)
        m_scroll.Maximum = Math.Max(0, m_intContentHeight)
        m_scroll.Enabled = m_intContentHeight > h
        If m_scroll.Value > Math.Max(0, m_intContentHeight - h) Then m_scroll.Value = Math.Max(0, m_intContentHeight - h)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If m_items IsNot Nothing Then RefreshLayout()
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        If Not m_scroll.Enabled Then Return
        Dim v As Integer = m_scroll.Value - Math.Sign(e.Delta) * (CellSize.Height + 8)
        m_scroll.Value = Math.Max(0, Math.Min(Math.Max(0, m_intContentHeight - ClientSize.Height), v))
    End Sub

    Private Sub EnsureVisible(ByVal item As ImportItem)
        Dim c = m_cells.FirstOrDefault(Function(x) x.Item Is item)
        If c.Item Is Nothing OrElse Not m_scroll.Enabled Then Return
        Dim top As Integer = c.Rect.Y - 30, bottom As Integer = c.Rect.Bottom + 6
        If top < m_scroll.Value Then
            m_scroll.Value = Math.Max(0, top)
        ElseIf bottom > m_scroll.Value + ClientSize.Height Then
            m_scroll.Value = Math.Min(Math.Max(0, m_intContentHeight - ClientSize.Height), bottom - ClientSize.Height)
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Dim dy As Integer = -m_scroll.Value
        Using bold As New Font(Font, FontStyle.Bold), small As New Font(Font.FontFamily, Math.Max(8.0F, Font.Size - 2.0F)),
              tiny As New Font(Font.FontFamily, 8.0F)
            For Each h In m_headers
                Dim r As New Rectangle(h.Rect.X, h.Rect.Y + dy, h.Rect.Width, h.Rect.Height)
                If Not r.IntersectsWith(e.ClipRectangle) Then Continue For
                TextRenderer.DrawText(g, h.Text, bold, New Point(r.X, r.Y + 3), Ink)
                Dim tw As Integer = TextRenderer.MeasureText(h.Text, bold).Width
                TextRenderer.DrawText(g, h.Sub_, small, New Point(r.X + tw + 4, r.Y + 5), Muted)
            Next
            For Each c In m_cells
                Dim r As New Rectangle(c.Rect.X, c.Rect.Y + dy, c.Rect.Width, c.Rect.Height)
                If Not r.IntersectsWith(e.ClipRectangle) Then Continue For
                DrawCell(g, r, c.Item, small, tiny)
            Next
        End Using
        If m_cells.Count = 0 AndAlso m_headers.Count = 0 Then
            TextRenderer.DrawText(g, "這裡沒有照片", Font, ClientRectangle, Muted, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        End If
    End Sub

    Private Sub DrawCell(ByVal g As Graphics, ByVal r As Rectangle, ByVal it As ImportItem, ByVal small As Font, ByVal tiny As Font)
        Dim sel As Boolean = m_selected.Contains(it)
        If sel Then
            Using b As New SolidBrush(AccentSoft)
                g.FillRectangle(b, Rectangle.Inflate(r, 3, 3))
            End Using
        End If
        Using b As New SolidBrush(Color.FromArgb(226, 230, 235))
            g.FillRectangle(b, r)
        End Using
        Dim bmp As Bitmap = Nothing
        If m_cache IsNot Nothing Then
            If m_cache.TryGet(it.SourceFile, bmp) Then
                If bmp.Width > 1 Then
                    Dim s As Single = Math.Min(r.Width / CSng(bmp.Width), r.Height / CSng(bmp.Height))
                    Dim w As Integer = CInt(bmp.Width * s), h As Integer = CInt(bmp.Height * s)
                    Dim ir As New Rectangle(r.X + (r.Width - w) \ 2, r.Y + (r.Height - h) \ 2, w, h)
                    If ShowChecks AndAlso Not it.Include Then
                        Using ia As New Imaging.ImageAttributes
                            ia.SetColorMatrix(New Imaging.ColorMatrix With {.Matrix33 = 0.4F})
                            g.DrawImage(bmp, ir, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia)
                        End Using
                    Else
                        g.DrawImage(bmp, ir)
                    End If
                Else
                    TextRenderer.DrawText(g, IO.Path.GetFileName(it.SourceFile), tiny, r, Muted, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.WordBreak)
                End If
            Else
                m_cache.Request(it.SourceFile)
            End If
        End If
        Using p As New Pen(If(sel, Accent, Color.FromArgb(60, 0, 0, 0)), If(sel, 2.0F, 1.0F))
            g.DrawRectangle(p, r)
        End Using

        ' check box
        If ShowChecks Then
            Dim cb As New Rectangle(r.X + 4, r.Y + 4, 16, 16)
            Using b As New SolidBrush(If(it.Include, Accent, Color.White)), p As New Pen(If(it.Include, Color.White, Color.FromArgb(120, 130, 140)))
                g.FillRectangle(b, cb)
                g.DrawRectangle(p, cb)
            End Using
            If it.Include Then
                Using p As New Pen(Color.White, 2)
                    g.DrawLines(p, {New Point(cb.X + 3, cb.Y + 8), New Point(cb.X + 7, cb.Y + 12), New Point(cb.X + 13, cb.Y + 4)})
                End Using
            End If
        End If

        ' badges: bottom right, from the right
        Dim bx As Integer = r.Right - 3
        Dim badge = Sub(text As String, back As Color)
                        Dim sz As Size = TextRenderer.MeasureText(text, tiny)
                        Dim br As New Rectangle(bx - sz.Width - 2, r.Bottom - sz.Height - 3, sz.Width + 2, sz.Height)
                        Using b As New SolidBrush(back)
                            g.FillRectangle(b, br)
                        End Using
                        TextRenderer.DrawText(g, text, tiny, br, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                        bx = br.X - 3
                    End Sub
        If it.DuplicateOf <> "" Then badge("已有", Color.FromArgb(220, 138, 90, 16))
        If it.IsVideo Then badge("影片", Color.FromArgb(200, 20, 24, 30))
        If ShowFaceBadges AndAlso it.Faces IsNot Nothing Then
            Dim n As Integer = it.VisibleFaces.Count()
            Dim ask As Integer = it.VisibleFaces.Where(Function(f) f.State = ImportFace.enumImportFaceState.ifSuggested).Count()
            If n > 0 Then badge(n & " 臉" & If(ask > 0, " ?", ""), If(ask > 0, Color.FromArgb(220, 184, 116, 15), Color.FromArgb(200, 20, 24, 30)))
        End If
        If it.SharesExifWith <> "" AndAlso it.Include Then badge("同名", Color.FromArgb(220, 192, 57, 43))
        If it.Edited Then
            Using b As New SolidBrush(Color.FromArgb(245, 190, 63)), p As New Pen(Color.White)
                g.FillEllipse(b, r.Right - 12, r.Y + 4, 8, 8)
                g.DrawEllipse(p, r.Right - 12, r.Y + 4, 8, 8)
            End Using
        End If
    End Sub

    Private Function HitItem(ByVal pt As Point) As (Item As ImportItem, OnCheck As Boolean)
        Dim p As New Point(pt.X, pt.Y + m_scroll.Value)
        For Each c In m_cells
            If c.Rect.Contains(p) Then
                Dim cb As New Rectangle(c.Rect.X, c.Rect.Y, 26, 26)
                Return (c.Item, ShowChecks AndAlso cb.Contains(p))
            End If
        Next
        Return (Nothing, False)
    End Function

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Focus()
        Dim h = HitItem(e.Location)
        If h.Item Is Nothing Then Return
        If e.Button = MouseButtons.Left AndAlso h.OnCheck Then
            ' the check box of a selected photo toggles every selected one
            Dim targets As List(Of ImportItem) = If(m_selected.Contains(h.Item), SelectedItems, New List(Of ImportItem) From {h.Item})
            Dim value As Boolean = Not h.Item.Include
            For Each it In targets
                it.Include = value
            Next
            Invalidate()
            RaiseEvent IncludeChanged(Me, EventArgs.Empty)
            Return
        End If
        If e.Button = MouseButtons.Right AndAlso m_selected.Contains(h.Item) Then Return
        Dim shown = ShownItems()
        If (ModifierKeys And Keys.Shift) <> 0 AndAlso m_anchor IsNot Nothing Then
            Dim a As Integer = shown.IndexOf(m_anchor), b As Integer = shown.IndexOf(h.Item)
            If (ModifierKeys And Keys.Control) = 0 Then m_selected.Clear()
            For i = Math.Min(a, b) To Math.Max(a, b)
                m_selected.Add(shown(i))
            Next
        ElseIf (ModifierKeys And Keys.Control) <> 0 Then
            If Not m_selected.Remove(h.Item) Then m_selected.Add(h.Item)
            m_anchor = h.Item
        Else
            m_selected.Clear()
            m_selected.Add(h.Item)
            m_anchor = h.Item
        End If
        m_focus = h.Item
        Invalidate()
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Space
                Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Dim cols As Integer = Math.Max(1, (ClientSize.Width - m_scroll.Width - 8) \ (CellSize.Width + 8))
        Select Case e.KeyCode
            Case Keys.Left : MoveFocus(-1)
            Case Keys.Right : MoveFocus(1)
            Case Keys.Up : MoveFocus(-cols)
            Case Keys.Down : MoveFocus(cols)
            Case Keys.A
                If e.Control Then SelectAll()
            Case Keys.Space
                If ShowChecks AndAlso m_selected.Count > 0 Then
                    Dim sel = SelectedItems
                    Dim value As Boolean = Not sel(0).Include
                    For Each it In sel
                        it.Include = value
                    Next
                    Invalidate()
                    RaiseEvent IncludeChanged(Me, EventArgs.Empty)
                End If
        End Select
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

End Class

'======================================================================================================
' One photo with its faces
'======================================================================================================
Friend Class ImportFaceStage
    Inherits Control

    ''' <summary>A face was named, confirmed, rejected, removed or boxed.</summary>
    Public Event FacesChanged As EventHandler
    ''' <summary>A box was dragged on the photo (fractions of the picture).</summary>
    Public Event BoxDrawn(ByVal box As RectangleF)
    ''' <summary>‹ / › (-1 / +1).</summary>
    Public Event Navigate(ByVal intDelta As Integer)
    ''' <summary>Right-click on a face.</summary>
    Public Event FaceMenu(ByVal face As ImportFace, ByVal pt As Point)

    Private m_item As ImportItem
    Private m_picture As Bitmap
    Private m_intVersion As Integer
    Private m_selFace As ImportFace
    Private m_dragStart As Point?
    Private m_dragNow As Point
    Private ReadOnly m_zones As New List(Of (Rect As Rectangle, Face As ImportFace, Action As String))
    Private WithEvents m_name As ComboBox
    Private m_naming As ImportFace

    ''' <summary>Text of the bar at the bottom (file, date, n / N).</summary>
    Public Property Caption As String = ""
    ''' <summary>Shown in the bar on the right ("分析面孔中…", "拖曳可補畫臉框").</summary>
    Public Property Hint As String = ""
    Public Property FacesEnabled As Boolean = True
    Public Property Names As List(Of String) = New List(Of String)
    ''' <summary>Name of a known person (0 = new): set by the window.</summary>
    Public Property PersonOf As Func(Of String, Integer)

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or
                 ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
        BackColor = Stage
        m_name = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDown, .Width = 180, .Visible = False,
                                    .AutoCompleteMode = AutoCompleteMode.SuggestAppend, .AutoCompleteSource = AutoCompleteSource.ListItems}
        Controls.Add(m_name)
    End Sub

    Public ReadOnly Property Item As ImportItem
        Get
            Return m_item
        End Get
    End Property

    Public Property SelectedFace As ImportFace
        Get
            Return m_selFace
        End Get
        Set(value As ImportFace)
            m_selFace = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Shows <paramref name="item"/>: the picture is read in the background.</summary>
    Public Sub ShowItem(ByVal item As ImportItem)
        EndName(False)
        If item Is m_item AndAlso m_picture IsNot Nothing Then
            Invalidate()
            Return
        End If
        m_item = item
        m_selFace = Nothing
        Dim old As Bitmap = m_picture
        m_picture = Nothing
        old?.Dispose()
        m_intVersion += 1
        Invalidate()
        If item Is Nothing Then Return
        Dim version As Integer = m_intVersion
        Dim file As String = item.SourceFile, isPic As Boolean = item.IsPicture
        Dim maxW As Integer = Math.Max(800, Width), maxH As Integer = Math.Max(600, Height)
        Threading.Tasks.Task.Run(Sub()
                                     Dim bmp As Bitmap = Nothing
                                     Try
                                         If isPic Then
                                             ' as the faces were found: without the EXIF rotation (see FaceEngine)
                                             Using full As Bitmap = Quartz.ImageFile.LoadPicture(file)
                                                 Dim s As Double = Math.Min(1.0, Math.Min(maxW / full.Width, maxH / full.Height))
                                                 bmp = New Bitmap(full, Math.Max(1, CInt(full.Width * s)), Math.Max(1, CInt(full.Height * s)))
                                             End Using
                                         Else
                                             bmp = Quartz.Thumbnail.ShellThumbnail(file, maxW, maxH)
                                         End If
                                     Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is ArgumentException OrElse
                                                                TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is UnauthorizedAccessException
                                         bmp = Nothing
                                     End Try
                                     If IsDisposed Then
                                         bmp?.Dispose()
                                         Return
                                     End If
                                     Try
                                         BeginInvoke(Sub()
                                                         If version <> m_intVersion OrElse IsDisposed Then
                                                             bmp?.Dispose()
                                                         Else
                                                             m_picture = bmp
                                                             Invalidate()
                                                         End If
                                                     End Sub)
                                     Catch ex As InvalidOperationException
                                         bmp?.Dispose()
                                     End Try
                                 End Sub)
    End Sub

    Private Function PictureRect() As Rectangle
        Dim area As New Rectangle(8, 8, Width - 16, Height - 16 - 32)
        If m_picture Is Nothing OrElse area.Width <= 0 OrElse area.Height <= 0 Then Return area
        Dim s As Single = Math.Min(area.Width / CSng(m_picture.Width), area.Height / CSng(m_picture.Height))
        Dim w As Integer = CInt(m_picture.Width * s), h As Integer = CInt(m_picture.Height * s)
        Return New Rectangle(area.X + (area.Width - w) \ 2, area.Y + (area.Height - h) \ 2, w, h)
    End Function

    Private Function FaceRect(ByVal f As ImportFace, ByVal pr As Rectangle) As Rectangle
        Return New Rectangle(pr.X + CInt(f.Box.X * pr.Width), pr.Y + CInt(f.Box.Y * pr.Height),
                             Math.Max(6, CInt(f.Box.Width * pr.Width)), Math.Max(6, CInt(f.Box.Height * pr.Height)))
    End Function

    Private Shared Function LabelOf(ByVal f As ImportFace) As String
        Select Case f.State
            Case ImportFace.enumImportFaceState.ifConfirmed : Return "✓ " & f.Name
            Case ImportFace.enumImportFaceState.ifAuto : Return f.Name
            Case ImportFace.enumImportFaceState.ifSuggested : Return "是 " & f.Name & " 嗎？"
            Case Else : Return If(f.Group > 0, "新面孔 " & f.Group, "？ 這是誰")
        End Select
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.InterpolationMode = InterpolationMode.HighQualityBilinear
        m_zones.Clear()
        Dim pr As Rectangle = PictureRect()
        Using small As New Font(Font.FontFamily, Math.Max(8.0F, Font.Size - 1.5F))
            If m_picture IsNot Nothing Then
                g.DrawImage(m_picture, pr)
            ElseIf m_item IsNot Nothing Then
                TextRenderer.DrawText(g, "讀取照片中…", Font, pr, Color.Silver, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            Else
                TextRenderer.DrawText(g, "在左邊選一張照片", Font, pr, Color.Silver, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            End If

            If FacesEnabled AndAlso m_picture IsNot Nothing AndAlso m_item IsNot Nothing Then
                For Each f In m_item.VisibleFaces
                    DrawFace(g, f, FaceRect(f, pr), small)
                Next
            End If

            If m_dragStart.HasValue Then
                Using p As New Pen(Color.White, 1.5F) With {.DashStyle = DashStyle.Dash}
                    g.DrawRectangle(p, DragRect())
                End Using
            End If

            ' bottom bar
            Dim bar As New Rectangle(0, Height - 32, Width, 32)
            Using b As New SolidBrush(Color.FromArgb(24, 28, 34))
                g.FillRectangle(b, bar)
            End Using
            Dim prev As New Rectangle(Width - 120, bar.Y + 4, 26, 24), nxt As New Rectangle(Width - 34, bar.Y + 4, 26, 24)
            TextRenderer.DrawText(g, Caption, small, New Rectangle(10, bar.Y, Width - 400, bar.Height), Color.FromArgb(220, 226, 234),
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            TextRenderer.DrawText(g, Hint, small, New Rectangle(Width - 400, bar.Y, 270, bar.Height), Color.FromArgb(160, 170, 182),
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.Right)
            For Each z In {(prev, "‹"), (nxt, "›")}
                Using p As New Pen(Color.FromArgb(120, 130, 140))
                    g.DrawRectangle(p, z.Item1)
                End Using
                TextRenderer.DrawText(g, z.Item2, Font, z.Item1, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            Next
            m_zones.Add((prev, Nothing, "prev"))
            m_zones.Add((nxt, Nothing, "next"))
        End Using
    End Sub

    Private Sub DrawFace(ByVal g As Graphics, ByVal f As ImportFace, ByVal r As Rectangle, ByVal small As Font)
        Dim penColor As Color, dash As Boolean, back As Color, fore As Color = Color.White
        Select Case f.State
            Case ImportFace.enumImportFaceState.ifConfirmed, ImportFace.enumImportFaceState.ifAuto
                penColor = Color.FromArgb(76, 209, 138) : back = Green
            Case ImportFace.enumImportFaceState.ifSuggested
                penColor = Color.FromArgb(245, 190, 63) : back = Color.FromArgb(184, 116, 15) : dash = True
            Case Else
                penColor = Color.White : back = Color.White : fore = Stage : dash = True
        End Select
        Dim w As Single = If(f Is m_selFace, 3.0F, 2.0F)
        Using p As New Pen(If(f Is m_selFace, Color.FromArgb(111, 166, 240), penColor), w)
            If dash Then p.DashStyle = DashStyle.Dash
            g.DrawRectangle(p, r)
        End Using
        m_zones.Add((r, f, "face"))

        ' label (with ✓ / ✕ for the program's names)
        Dim text As String = LabelOf(f)
        Dim sz As Size = TextRenderer.MeasureText(text, small)
        Dim lr As New Rectangle(r.X, r.Bottom + 3, sz.Width + 6, sz.Height + 2)
        Dim buttons As Boolean = f.State = ImportFace.enumImportFaceState.ifSuggested OrElse f.State = ImportFace.enumImportFaceState.ifAuto
        Dim full As New Rectangle(lr.X, lr.Y, lr.Width + If(buttons, 44, 0), lr.Height)
        Using b As New SolidBrush(back)
            g.FillRectangle(b, full)
        End Using
        TextRenderer.DrawText(g, text, small, lr, fore, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        m_zones.Add((lr, f, "label"))
        If buttons Then
            Dim ok As New Rectangle(lr.Right, lr.Y, 22, lr.Height), no As New Rectangle(lr.Right + 22, lr.Y, 22, lr.Height)
            Using b As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
                g.FillRectangle(b, ok)
            End Using
            TextRenderer.DrawText(g, "✓", small, ok, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            TextRenderer.DrawText(g, "✕", small, no, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            m_zones.Add((ok, f, "ok"))
            m_zones.Add((no, f, "no"))
        End If
    End Sub

    Private Function DragRect() As Rectangle
        Dim a As Point = m_dragStart.Value, b As Point = m_dragNow
        Return New Rectangle(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y))
    End Function

    Private Function HitZone(ByVal pt As Point) As (Rect As Rectangle, Face As ImportFace, Action As String)
        ' last drawn is on top; small buttons before the face boxes
        For i = m_zones.Count - 1 To 0 Step -1
            If m_zones(i).Action <> "face" AndAlso m_zones(i).Rect.Contains(pt) Then Return m_zones(i)
        Next
        For i = m_zones.Count - 1 To 0 Step -1
            If m_zones(i).Rect.Contains(pt) Then Return m_zones(i)
        Next
        Return (Rectangle.Empty, Nothing, "")
    End Function

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Focus()
        EndName(True)
        Dim z = HitZone(e.Location)
        If e.Button = MouseButtons.Right Then
            If z.Face IsNot Nothing Then
                m_selFace = z.Face
                Invalidate()
                RaiseEvent FaceMenu(z.Face, e.Location)
            End If
            Return
        End If
        If e.Button <> MouseButtons.Left Then Return
        Select Case z.Action
            Case "prev" : RaiseEvent Navigate(-1)
            Case "next" : RaiseEvent Navigate(1)
            Case "ok"
                z.Face.Confirm()
                Changed()
            Case "no"
                z.Face.Reject()
                Changed()
            Case "face", "label"
                m_selFace = z.Face
                Invalidate()
                BeginName(z.Face)
            Case Else
                m_selFace = Nothing
                If FacesEnabled AndAlso m_item IsNot Nothing AndAlso m_item.IsPicture AndAlso m_picture IsNot Nothing AndAlso PictureRect().Contains(e.Location) Then
                    m_dragStart = e.Location
                    m_dragNow = e.Location
                End If
                Invalidate()
        End Select
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If m_dragStart.HasValue Then
            Dim pr As Rectangle = PictureRect()
            m_dragNow = New Point(Math.Max(pr.Left, Math.Min(pr.Right, e.X)), Math.Max(pr.Top, Math.Min(pr.Bottom, e.Y)))
            Invalidate()
        Else
            Dim z = HitZone(e.Location)
            Cursor = If(z.Action <> "", Cursors.Hand, If(FacesEnabled AndAlso m_picture IsNot Nothing AndAlso PictureRect().Contains(e.Location), Cursors.Cross, Cursors.Default))
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If Not m_dragStart.HasValue Then Return
        Dim r As Rectangle = DragRect()
        m_dragStart = Nothing
        Invalidate()
        If r.Width < 12 OrElse r.Height < 12 Then Return
        Dim pr As Rectangle = PictureRect()
        RaiseEvent BoxDrawn(New RectangleF((r.X - pr.X) / CSng(pr.Width), (r.Y - pr.Y) / CSng(pr.Height), r.Width / CSng(pr.Width), r.Height / CSng(pr.Height)))
    End Sub

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Left, Keys.Right, Keys.Delete
                Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Select Case e.KeyCode
            Case Keys.Left : RaiseEvent Navigate(-1)
            Case Keys.Right : RaiseEvent Navigate(1)
            Case Keys.Delete
                If m_selFace IsNot Nothing Then
                    m_selFace.State = ImportFace.enumImportFaceState.ifNotFace
                    m_selFace = Nothing
                    Changed()
                End If
        End Select
    End Sub

    Public Sub Changed()
        Invalidate()
        RaiseEvent FacesChanged(Me, EventArgs.Empty)
    End Sub

    '--------------------------------------------------------------------------------------------------
    ' The name box under a face
    '--------------------------------------------------------------------------------------------------
    Public Sub BeginName(ByVal f As ImportFace)
        If f Is Nothing OrElse m_picture Is Nothing Then Return
        m_naming = f
        Dim r As Rectangle = FaceRect(f, PictureRect())
        m_name.Font = New Font(Font.FontFamily, Math.Max(9.0F, Font.Size - 1))
        m_name.Items.Clear()
        m_name.Items.AddRange(Names.Distinct().OrderBy(Function(n) n, StringComparer.CurrentCulture).Cast(Of Object)().ToArray())
        m_name.Text = If(f.State = ImportFace.enumImportFaceState.ifUnknown, "", f.Name)
        m_name.Location = New Point(Math.Max(0, Math.Min(Width - m_name.Width, r.X)), Math.Min(Height - 60, r.Bottom + 26))
        m_name.Visible = True
        m_name.BringToFront()
        m_name.Focus()
        m_name.SelectAll()
    End Sub

    ''' <summary>Closes the name box; <paramref name="save"/> keeps what was typed.</summary>
    Public Sub EndName(ByVal save As Boolean)
        If m_naming Is Nothing Then
            m_name.Visible = False
            Return
        End If
        Dim f As ImportFace = m_naming
        m_naming = Nothing
        m_name.Visible = False
        If Not save Then Return
        Dim n As String = m_name.Text.Trim()
        If n = "" AndAlso f.State = ImportFace.enumImportFaceState.ifUnknown Then Return
        If n = f.Name AndAlso f.State = ImportFace.enumImportFaceState.ifConfirmed Then Return
        If n <> "" AndAlso Not CheckNameRule(n) Then Return
        If n <> "" AndAlso n <> f.Name AndAlso f.PersonID <> 0 AndAlso f.State <> ImportFace.enumImportFaceState.ifConfirmed Then f.Reject()
        f.SetName(n, If(PersonOf Is Nothing, 0, PersonOf(n)))
        If n <> "" AndAlso Not Names.Contains(n) Then Names.Add(n)
        Changed()
    End Sub

    Private Sub m_name_KeyDown(sender As Object, e As KeyEventArgs) Handles m_name.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            EndName(True)
            Focus()
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            EndName(False)
            Focus()
        End If
    End Sub

    Private Sub m_name_Leave(sender As Object, e As EventArgs) Handles m_name.Leave
        EndName(True)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            m_intVersion += 1
            m_picture?.Dispose()
            m_picture = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class

'======================================================================================================
' Steps
'======================================================================================================
Friend Class ImportStepBar
    Inherits Control

    Private ReadOnly m_steps As String() = {"選照片", "整理", "匯入"}
    Private m_intCurrent As Integer

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
        BackColor = Color.FromArgb(225, 229, 234)
    End Sub

    Public Property Current As Integer
        Get
            Return m_intCurrent
        End Get
        Set(value As Integer)
            m_intCurrent = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using bold As New Font(Font, FontStyle.Bold), small As New Font(Font.FontFamily, 9.0F, FontStyle.Bold), p As New Pen(Line)
            g.DrawLine(p, 0, Height - 1, Width, Height - 1)
            Dim widths = m_steps.Select(Function(s) 30 + TextRenderer.MeasureText(s, bold).Width).ToArray()
            Dim gap As Integer = 70
            Dim total As Integer = widths.Sum() + gap * (m_steps.Length - 1)
            Dim x As Integer = (Width - total) \ 2, cy As Integer = Height \ 2
            For i = 0 To m_steps.Length - 1
                Dim circle As New Rectangle(x, cy - 11, 22, 22)
                Dim done As Boolean = i < m_intCurrent, isOn As Boolean = i = m_intCurrent
                Using b As New SolidBrush(If(done, Green, If(isOn, Accent, Color.White))), bp As New Pen(If(done OrElse isOn, Color.Transparent, Color.FromArgb(181, 189, 200)))
                    g.FillEllipse(b, circle)
                    g.DrawEllipse(bp, circle)
                End Using
                TextRenderer.DrawText(g, If(done, "✓", (i + 1).ToString()), small, circle, If(done OrElse isOn, Color.White, Muted),
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                TextRenderer.DrawText(g, m_steps(i), If(isOn, bold, Font), New Point(x + 28, cy - 10), If(done, Green, If(isOn, Ink, Muted)))
                x += widths(i)
                If i < m_steps.Length - 1 Then
                    g.DrawLine(p, x + 10, cy, x + gap - 10, cy)
                    x += gap
                End If
            Next
        End Using
    End Sub

End Class

'======================================================================================================
' Folders
'======================================================================================================
Friend Class ImportFolderTree
    Inherits TreeView

    Public Event PathSelected(ByVal strPath As String)
    Private Const Dummy As String = "…"

    Public Sub New()
        HideSelection = False
        FullRowSelect = True
        ShowLines = False
        BorderStyle = BorderStyle.None
    End Sub

    Public Sub LoadRoots()
        BeginUpdate()
        Nodes.Clear()
        AddRoot("桌面", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
        AddRoot("圖片", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
        AddRoot("下載", IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"))
        For Each d In IO.DriveInfo.GetDrives()
            Try
                If Not d.IsReady Then Continue For
                Dim kind As String = If(d.DriveType = IO.DriveType.Removable, "卸除式磁碟", If(d.DriveType = IO.DriveType.CDRom, "光碟", "本機磁碟"))
                Dim label As String = If(d.VolumeLabel <> "", d.VolumeLabel, kind) & " (" & d.Name.TrimEnd("\"c) & ")"
                If IO.Directory.Exists(IO.Path.Combine(d.RootDirectory.FullName, "DCIM")) Then label &= "  · 相機"
                AddRoot(label, d.RootDirectory.FullName)
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        EndUpdate()
    End Sub

    ''' <summary>Removable drives with a DCIM folder (camera cards): their DCIM sub-folders.</summary>
    Public Shared Function CameraFolders() As List(Of String)
        Dim result As New List(Of String)
        For Each d In IO.DriveInfo.GetDrives()
            Try
                If Not d.IsReady OrElse d.DriveType <> IO.DriveType.Removable Then Continue For
                Dim dcim As String = IO.Path.Combine(d.RootDirectory.FullName, "DCIM")
                If Not IO.Directory.Exists(dcim) Then Continue For
                Dim subs() As String = Nothing
                ExactFolders(dcim, subs)
                result.AddRange(If(subs.Length > 0, subs, {dcim}))
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        Return result
    End Function

    Private Sub AddRoot(ByVal text As String, ByVal path As String)
        If String.IsNullOrEmpty(path) OrElse Not IO.Directory.Exists(path) Then Return
        Dim n As TreeNode = Nodes.Add(text)
        n.Tag = path
        n.Nodes.Add(Dummy)
    End Sub

    Protected Overrides Sub OnBeforeExpand(e As TreeViewCancelEventArgs)
        MyBase.OnBeforeExpand(e)
        Fill(e.Node)
    End Sub

    Private Sub Fill(ByVal n As TreeNode)
        If n.Nodes.Count <> 1 OrElse n.Nodes(0).Text <> Dummy OrElse n.Nodes(0).Tag IsNot Nothing Then Return
        n.Nodes.Clear()
        Dim subs() As String = Nothing
        ExactFolders(CStr(n.Tag), subs)
        For Each s In subs
            Dim c As TreeNode = n.Nodes.Add(IO.Path.GetFileName(s))
            c.Tag = s
            Try
                If IO.Directory.EnumerateDirectories(s).Any() Then c.Nodes.Add(Dummy)
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
    End Sub

    Protected Overrides Sub OnAfterSelect(e As TreeViewEventArgs)
        MyBase.OnAfterSelect(e)
        If e.Node?.Tag IsNot Nothing Then RaiseEvent PathSelected(CStr(e.Node.Tag))
    End Sub

    ''' <summary>Opens the tree down to <paramref name="strPath"/> and selects it (the deepest root that
    ''' holds it: 圖片 before the drive).</summary>
    Public Sub SelectPath(ByVal strPath As String)
        strPath = strPath.TrimEnd("\"c)
        Dim root As TreeNode = Nothing, best As Integer = -1
        For Each n As TreeNode In Nodes
            Dim p As String = CStr(n.Tag).TrimEnd("\"c)
            If (strPath.Equals(p, StringComparison.OrdinalIgnoreCase) OrElse strPath.StartsWith(p & "\", StringComparison.OrdinalIgnoreCase)) AndAlso p.Length > best Then
                root = n : best = p.Length
            End If
        Next
        If root Is Nothing Then Return
        Dim node As TreeNode = root
        Do While Not CStr(node.Tag).TrimEnd("\"c).Equals(strPath, StringComparison.OrdinalIgnoreCase)
            Fill(node)
            node.Expand()
            Dim nextNode As TreeNode = Nothing
            For Each c As TreeNode In node.Nodes
                Dim p As String = CStr(c.Tag).TrimEnd("\"c)
                If strPath.Equals(p, StringComparison.OrdinalIgnoreCase) OrElse strPath.StartsWith(p & "\", StringComparison.OrdinalIgnoreCase) Then nextNode = c : Exit For
            Next
            If nextNode Is Nothing Then Exit Do
            node = nextNode
        Loop
        SelectedNode = node
        node.EnsureVisible()
    End Sub

End Class

'======================================================================================================
' Chips
'======================================================================================================
Friend Class ImportChip
    Inherits Label

    Public Property ChipBack As Color = AccentSoft
    Public Property ChipBorder As Color = Color.FromArgb(169, 196, 234)
    Public Property Dashed As Boolean
    Public Property Payload As Object

    Public Sub New()
        AutoSize = False
        TextAlign = ContentAlignment.MiddleCenter
        Padding = New Padding(8, 1, 8, 1)
        Margin = New Padding(0, 2, 6, 2)
        Cursor = Cursors.Hand
        SetStyle(ControlStyles.SupportsTransparentBackColor Or ControlStyles.OptimizedDoubleBuffer, True)
        BackColor = Color.Transparent
    End Sub

    Public Sub FitText()
        Dim sz As Size = TextRenderer.MeasureText(Text, Font)
        Size = New Size(sz.Width + 18, sz.Height + 6)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim r As New RectangleF(0.5F, 0.5F, Width - 1.5F, Height - 1.5F)
        Using path As GraphicsPath = RoundRect(r, Height / 2.0F), b As New SolidBrush(ChipBack), p As New Pen(ChipBorder)
            If Dashed Then p.DashStyle = DashStyle.Dash
            e.Graphics.FillPath(b, path)
            e.Graphics.DrawPath(p, path)
        End Using
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)
    End Sub

End Class
