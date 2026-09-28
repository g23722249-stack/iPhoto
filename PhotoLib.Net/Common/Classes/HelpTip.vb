Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

' A help window for the controls of a form (new in the .NET port; VB6 had an empty frmToolTipText for
' this and drew plain balloon tooltips): a small window with an icon, a title, a description and an
' optional hint line that appears when the mouse rests on a control.
'
'   Private m_lpHelp As New HelpTip
'   m_lpHelp.SetHelp(imgToolBox_0, HelpTexts.Get("main.toolbox.0"))          ' icon = the control's Image
'   m_lpHelp.SetToolBarHelp(picImageBar1, 0, HelpTexts.Get("viewer.edit.0")) ' an Aqua.ToolBar button
'
' Shown 500 ms after the mouse stops on the control, hidden when it leaves, on a click, or after 20 s.
' It also appears over a DISABLED control (WinForms sends a disabled control no mouse events: the parent's
' MouseMove is watched instead) and then shows the entry's DisabledHint -- why it can't be used now.
' The window never takes the focus (WS_EX_NOACTIVATE) and stays inside the screen's working area.
Public Class HelpTip
    Implements IDisposable

    ''' <summary>One help entry.</summary>
    Public Class Entry
        Public Title As String = ""
        Public Text As String = ""
        ''' <summary>Smaller line under the text (a tip, a shortcut, a condition); optional.</summary>
        Public Hint As String = ""
        ''' <summary>Shown instead of Hint while the control is disabled; optional.</summary>
        Public DisabledHint As String = ""
        ''' <summary>Overrides the icon taken from the control; optional.</summary>
        Public Icon As Image
    End Class

    Public Property InitialDelay As Integer = 500
    Public Property AutoPopDelay As Integer = 20000
    ''' <summary>False turns every help window off (e.g. a setting).</summary>
    Public Property Active As Boolean = True

    Private ReadOnly m_entries As New Dictionary(Of Control, Entry)
    Private ReadOnly m_bars As New Dictionary(Of Aqua.ToolBar, Dictionary(Of Integer, Entry))
    Private ReadOnly m_parents As New HashSet(Of Control)
    Private ReadOnly m_window As New HelpWindow
    Private WithEvents m_showTimer As New Timer
    Private WithEvents m_hideTimer As New Timer

    ' what the mouse rests on now
    Private m_hoverControl As Control
    Private m_hoverBar As Aqua.ToolBar
    Private m_hoverIndex As Integer = -1

    '==================================================================================================
    ' Registration
    '==================================================================================================
    ' every hooked control -> the registered control whose help it shows: a composite control (an
    ' Aqua.TextBox is a frame around a real text box) shows its help over any of its parts
    Private ReadOnly m_owner As New Dictionary(Of Control, Control)

    Public Sub SetHelp(ByVal c As Control, ByVal e As Entry)
        If c Is Nothing Then Return
        If e Is Nothing Then
            m_entries.Remove(c)
            Return
        End If
        If Not m_entries.ContainsKey(c) Then
            Hook(c, c)
            WatchParent(c.Parent)
            AddHandler c.ParentChanged, Sub(s, a) WatchParent(CType(s, Control).Parent)
        End If
        m_entries(c) = e
    End Sub

    ''' <summary>The help of HelpTexts key <paramref name="key"/> on each control (e.g. a field's caption and
    ''' its box); an unknown key or a missing control is skipped.</summary>
    Public Sub SetHelp(ByVal key As String, ParamArray controls As Control())
        Dim e As Entry = HelpTexts.Get(key)
        If e Is Nothing Then Return
        For Each c In controls
            If c IsNot Nothing Then SetHelp(c, e)
        Next
    End Sub

    ''' <summary>Mouse events of <paramref name="ctrl"/> and all its children (now and added later) go to
    ''' <paramref name="owner"/>'s help.</summary>
    Private Sub Hook(ByVal ctrl As Control, ByVal owner As Control)
        If m_owner.ContainsKey(ctrl) Then Return
        m_owner(ctrl) = owner
        AddHandler ctrl.MouseEnter, AddressOf Control_MouseEnter
        AddHandler ctrl.MouseLeave, AddressOf Control_MouseLeave
        AddHandler ctrl.MouseDown, AddressOf Control_MouseDown
        AddHandler ctrl.ControlAdded, Sub(s, a) Hook(a.Control, owner)
        For Each child As Control In ctrl.Controls
            Hook(child, owner)
        Next
    End Sub

    ''' <summary>Help for button <paramref name="index"/> of an Aqua.ToolBar (its buttons aren't controls).</summary>
    Public Sub SetToolBarHelp(ByVal bar As Aqua.ToolBar, ByVal index As Integer, ByVal e As Entry)
        If bar Is Nothing OrElse e Is Nothing Then Return
        If Not m_bars.ContainsKey(bar) Then
            m_bars(bar) = New Dictionary(Of Integer, Entry)
            AddHandler bar.ButtonMouseEnter, AddressOf Bar_ButtonMouseEnter
            AddHandler bar.ButtonMouseLeave, AddressOf Bar_ButtonMouseLeave
            AddHandler bar.MouseDown, AddressOf Control_MouseDown
            AddHandler bar.MouseLeave, AddressOf Bar_MouseLeave
        End If
        m_bars(bar)(index) = e
    End Sub

    ''' <summary>Disabled controls get no mouse events: their parent's MouseMove finds them.</summary>
    Private Sub WatchParent(ByVal p As Control)
        If p Is Nothing OrElse m_parents.Contains(p) Then Return
        m_parents.Add(p)
        AddHandler p.MouseMove, AddressOf Parent_MouseMove
        AddHandler p.MouseLeave, AddressOf Parent_MouseLeave
    End Sub

    '==================================================================================================
    ' Mouse
    '==================================================================================================
    Private Sub Control_MouseEnter(sender As Object, e As EventArgs)
        Dim owner As Control = Nothing
        If Not m_owner.TryGetValue(CType(sender, Control), owner) Then Return
        If m_hoverControl Is owner Then Return   ' moved from one part of the control to another
        Hover(owner, Nothing, -1)
    End Sub

    Private Sub Control_MouseLeave(sender As Object, e As EventArgs)
        Dim owner As Control = Nothing
        If Not m_owner.TryGetValue(CType(sender, Control), owner) OrElse m_hoverControl IsNot owner Then Return
        ' still over the control (the mouse went into one of its parts): keep the help
        If owner.IsHandleCreated AndAlso owner.RectangleToScreen(owner.ClientRectangle).Contains(Cursor.Position) Then Return
        Unhover()
    End Sub

    Private Sub Control_MouseDown(sender As Object, e As MouseEventArgs)
        Unhover()
    End Sub

    Private Sub Parent_MouseMove(sender As Object, e As MouseEventArgs)
        Dim p As Control = CType(sender, Control)
        Dim child As Control = p.GetChildAtPoint(e.Location, GetChildAtPointSkip.Invisible Or GetChildAtPointSkip.Transparent)
        If child IsNot Nothing AndAlso Not child.Enabled AndAlso m_entries.ContainsKey(child) Then
            If m_hoverControl IsNot child Then Hover(child, Nothing, -1)
        ElseIf m_hoverControl IsNot Nothing AndAlso Not m_hoverControl.Enabled Then
            Unhover()
        End If
    End Sub

    Private Sub Parent_MouseLeave(sender As Object, e As EventArgs)
        If m_hoverControl IsNot Nothing AndAlso Not m_hoverControl.Enabled Then Unhover()
    End Sub

    Private Sub Bar_ButtonMouseEnter(sender As Object, index As Integer)
        Hover(Nothing, CType(sender, Aqua.ToolBar), index)
    End Sub

    Private Sub Bar_ButtonMouseLeave(sender As Object, index As Integer)
        If m_hoverBar Is sender AndAlso m_hoverIndex = index Then Unhover()
    End Sub

    Private Sub Bar_MouseLeave(sender As Object, e As EventArgs)
        If m_hoverBar Is sender Then Unhover()
    End Sub

    Private Sub Hover(ByVal c As Control, ByVal bar As Aqua.ToolBar, ByVal index As Integer)
        Unhover()
        If Not Active Then Return
        m_hoverControl = c
        m_hoverBar = bar
        m_hoverIndex = index
        m_showTimer.Interval = Math.Max(1, InitialDelay)
        m_showTimer.Start()
    End Sub

    Private Sub Unhover()
        m_showTimer.Stop()
        m_hideTimer.Stop()
        m_hoverControl = Nothing
        m_hoverBar = Nothing
        m_hoverIndex = -1
        If m_window.Visible Then m_window.Hide()
    End Sub

    Private Sub m_showTimer_Tick(sender As Object, e As EventArgs) Handles m_showTimer.Tick
        m_showTimer.Stop()
        Dim entry As Entry = Nothing, icon As Image = Nothing, disabled As Boolean = False
        Dim anchor As Rectangle   ' screen rectangle the window goes under
        If m_hoverControl IsNot Nothing Then
            If Not m_entries.TryGetValue(m_hoverControl, entry) OrElse Not m_hoverControl.Visible Then Return
            icon = If(entry.Icon, ImageOf(m_hoverControl))
            disabled = Not m_hoverControl.Enabled
            anchor = m_hoverControl.RectangleToScreen(m_hoverControl.ClientRectangle)
        ElseIf m_hoverBar IsNot Nothing Then
            Dim items As Dictionary(Of Integer, Entry) = Nothing
            If Not m_bars.TryGetValue(m_hoverBar, items) OrElse Not items.TryGetValue(m_hoverIndex, entry) Then Return
            icon = If(entry.Icon, If(m_hoverIndex < m_hoverBar.Count, m_hoverBar.GetIcon(m_hoverIndex), Nothing))
            disabled = Not m_hoverBar.Enabled
            Dim p As Point = Cursor.Position
            anchor = New Rectangle(p.X - 8, p.Y - 8, 16, 24)
        Else
            Return
        End If
        m_window.ShowEntry(entry, icon, disabled, anchor)
        m_hideTimer.Interval = Math.Max(1000, AutoPopDelay)
        m_hideTimer.Start()
    End Sub

    Private Sub m_hideTimer_Tick(sender As Object, e As EventArgs) Handles m_hideTimer.Tick
        m_hideTimer.Stop()
        m_window.Hide()
    End Sub

    ''' <summary>The picture a control shows (PictureBox, Aqua image buttons ...), if it has one.</summary>
    Private Shared Function ImageOf(ByVal c As Control) As Image
        Dim pic As PictureBox = TryCast(c, PictureBox)
        If pic IsNot Nothing Then Return pic.Image
        Dim prop = c.GetType().GetProperty("Image", GetType(Image))
        If prop IsNot Nothing AndAlso prop.CanRead AndAlso prop.GetIndexParameters().Length = 0 Then
            Try
                Return CType(prop.GetValue(c), Image)
            Catch ex As Exception When TypeOf ex Is Reflection.TargetInvocationException OrElse TypeOf ex Is InvalidOperationException
                Return Nothing
            End Try
        End If
        Return Nothing
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        m_showTimer.Dispose()
        m_hideTimer.Dispose()
        m_window.Dispose()
    End Sub

    '==================================================================================================
    ' The window
    '==================================================================================================
    Private Class HelpWindow
        Inherits Form

        Private Const MaxWidth As Integer = 360
        Private Const MinWidth As Integer = 240
        Private Const Pad As Integer = 12
        Private Const IconSize As Integer = 32
        Private Const HeaderGap As Integer = 10

        Private Shared ReadOnly Border As Color = Color.FromArgb(158, 165, 175)
        Private Shared ReadOnly HeaderTop As Color = Color.FromArgb(246, 247, 249)
        Private Shared ReadOnly HeaderBottom As Color = Color.FromArgb(221, 227, 234)
        Private Shared ReadOnly TitleColor As Color = Color.FromArgb(27, 35, 48)
        Private Shared ReadOnly TextColor As Color = Color.FromArgb(60, 70, 86)
        Private Shared ReadOnly HintColor As Color = Color.FromArgb(42, 116, 208)
        Private Shared ReadOnly DisabledColor As Color = Color.FromArgb(178, 75, 18)

        Private ReadOnly m_titleFont As New Font("Microsoft JhengHei UI", 11.0F, FontStyle.Bold)
        Private ReadOnly m_textFont As New Font("Microsoft JhengHei UI", 9.75F)
        Private ReadOnly m_hintFont As New Font("Microsoft JhengHei UI", 9.0F)

        Private m_entry As Entry
        Private m_icon As Image
        Private m_disabled As Boolean
        Private m_headerHeight As Integer
        Private m_titleRect, m_textRect, m_hintRect As Rectangle

        Public Sub New()
            FormBorderStyle = FormBorderStyle.None
            ShowInTaskbar = False
            StartPosition = FormStartPosition.Manual
            TopMost = True
            BackColor = Color.White
            DoubleBuffered = True
        End Sub

        Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                Const WS_EX_TOOLWINDOW As Integer = &H80
                Const WS_EX_TOPMOST As Integer = &H8
                Const WS_EX_NOACTIVATE As Integer = &H8000000
                Const CS_DROPSHADOW As Integer = &H20000
                Dim cp As CreateParams = MyBase.CreateParams
                cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW Or WS_EX_TOPMOST Or WS_EX_NOACTIVATE
                cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
                Return cp
            End Get
        End Property

        Protected Overrides Sub WndProc(ByRef m As Message)
            Const WM_MOUSEACTIVATE As Integer = &H21
            Const MA_NOACTIVATE As Integer = 3
            If m.Msg = WM_MOUSEACTIVATE Then
                m.Result = New IntPtr(MA_NOACTIVATE)
                Return
            End If
            MyBase.WndProc(m)
        End Sub

        Public Sub ShowEntry(ByVal e As Entry, ByVal icon As Image, ByVal disabled As Boolean, ByVal anchor As Rectangle)
            m_entry = e
            m_icon = icon
            m_disabled = disabled
            Arrange()
            ' under the control, else above it; always inside the working area of its screen
            Dim wa As Rectangle = Screen.FromRectangle(anchor).WorkingArea
            Dim x As Integer = Math.Max(wa.Left + 4, Math.Min(wa.Right - Width - 4, anchor.Left))
            Dim y As Integer = anchor.Bottom + 6
            If y + Height > wa.Bottom - 4 Then y = Math.Max(wa.Top + 4, anchor.Top - Height - 6)
            Location = New Point(x, y)
            Invalidate()
            If Not Visible Then Show() Else Refresh()
        End Sub

        Private ReadOnly Property HintText As String
            Get
                If m_disabled AndAlso m_entry.DisabledHint <> "" Then Return "目前無法使用：" & m_entry.DisabledHint
                Return m_entry.Hint
            End Get
        End Property

        Private Sub Arrange()
            Dim hasIcon As Boolean = m_icon IsNot Nothing
            Dim titleX As Integer = Pad + If(hasIcon, IconSize + HeaderGap, 0)
            Dim flags As TextFormatFlags = TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix
            ' width: whatever of title, text and hint wants most, between MinWidth and MaxWidth; the
            ' heights are then measured at that width
            Dim widest As Integer = MaxWidth - Pad * 2
            Dim titleW As Integer = TextRenderer.MeasureText(m_entry.Title, m_titleFont, New Size(MaxWidth - titleX - Pad, 0), flags).Width
            Dim textW As Integer = If(m_entry.Text = "", 0, TextRenderer.MeasureText(m_entry.Text, m_textFont, New Size(widest, 0), flags).Width)
            Dim hintW As Integer = If(HintText = "", 0, TextRenderer.MeasureText(HintText, m_hintFont, New Size(widest, 0), flags).Width)
            Dim w As Integer = Math.Max(MinWidth, Math.Min(MaxWidth, Math.Max(titleX + titleW + Pad, Math.Max(textW, hintW) + Pad * 2)))
            Dim bodyW As Integer = w - Pad * 2
            Dim titleSz As Size = TextRenderer.MeasureText(m_entry.Title, m_titleFont, New Size(w - titleX - Pad, 0), flags)
            Dim textSz As Size = If(m_entry.Text = "", Size.Empty, TextRenderer.MeasureText(m_entry.Text, m_textFont, New Size(bodyW, 0), flags))
            Dim hintSz As Size = If(HintText = "", Size.Empty, TextRenderer.MeasureText(HintText, m_hintFont, New Size(bodyW, 0), flags))

            m_headerHeight = Math.Max(If(hasIcon, IconSize, 0), titleSz.Height) + Pad * 2 - 4
            m_titleRect = New Rectangle(titleX, (m_headerHeight - titleSz.Height) \ 2, w - titleX - Pad, titleSz.Height)
            Dim y As Integer = m_headerHeight + 8
            m_textRect = New Rectangle(Pad, y, bodyW, textSz.Height)
            If textSz.Height > 0 Then y += textSz.Height + 6
            m_hintRect = New Rectangle(Pad, y, bodyW, hintSz.Height)
            If hintSz.Height > 0 Then y += hintSz.Height + 4
            Size = New Size(w, y + Pad - 4)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            If m_entry Is Nothing Then Return
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            Dim header As New Rectangle(0, 0, Width, m_headerHeight)
            Using b As New LinearGradientBrush(header, HeaderTop, HeaderBottom, LinearGradientMode.Vertical)
                g.FillRectangle(b, header)
            End Using
            Using p As New Pen(Color.FromArgb(206, 212, 220))
                g.DrawLine(p, 0, m_headerHeight, Width, m_headerHeight)
            End Using
            If m_icon IsNot Nothing Then DrawIcon(g, m_icon, New Rectangle(Pad, (m_headerHeight - IconSize) \ 2, IconSize, IconSize))

            Dim flags As TextFormatFlags = TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix
            TextRenderer.DrawText(g, m_entry.Title, m_titleFont, m_titleRect, TitleColor, flags Or TextFormatFlags.VerticalCenter)
            If m_entry.Text <> "" Then TextRenderer.DrawText(g, m_entry.Text, m_textFont, m_textRect, TextColor, flags)
            If HintText <> "" Then
                TextRenderer.DrawText(g, HintText, m_hintFont, m_hintRect, If(m_disabled AndAlso m_entry.DisabledHint <> "", DisabledColor, HintColor), flags)
            End If
            Using p As New Pen(Border)
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
            End Using
        End Sub

        ''' <summary>The icon fitted into <paramref name="r"/>, magenta keyed out (the ported toolbar icons
        ''' use it as their transparent colour).</summary>
        Private Shared Sub DrawIcon(ByVal g As Graphics, ByVal img As Image, ByVal r As Rectangle)
            Dim k As Double = Math.Min(r.Width / img.Width, r.Height / img.Height)
            Dim w As Integer = Math.Max(1, CInt(img.Width * k)), h As Integer = Math.Max(1, CInt(img.Height * k))
            Dim dest As New Rectangle(r.X + (r.Width - w) \ 2, r.Y + (r.Height - h) \ 2, w, h)
            Using ia As New ImageAttributes()
                ia.SetColorKey(Color.FromArgb(255, 0, 255), Color.FromArgb(255, 0, 255))
                g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia)
            End Using
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                m_titleFont.Dispose()
                m_textFont.Dispose()
                m_hintFont.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

End Class
