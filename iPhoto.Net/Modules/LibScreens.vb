' Window placement across screens (new in the .NET port):
'   - WindowPlacement: where the main window and the photo viewer were, saved with SaveSetting under the
'     VB6 application name (HKCU\Software\VB and VBA Program Settings\iPhoto\Window) and restored at the
'     next start.
'   - DualScreenSwap: with two screens the main window and the viewer each fill one. Moving either of
'     them onto the other's screen (dragging it by the title bar, Win+Shift+Arrow, ...) moves the other
'     one to the screen just left, so they trade places; a window that was maximised is maximised
'     again on its new screen. The placement is saved after every move.
'   - ChildWindowsFollowMain: every other window (dialogs, message boxes, tool windows) opens on the
'     main window's screen, unless it was opened from the viewer or the slide show.
Imports System.Runtime.InteropServices

Friend Module WindowPlacement

    Private Const AppName As String = "iPhoto"   ' VB6 App.EXEName, as LibUserInterface
    Private Const Section As String = "Window"

    ''' <summary>The screen whose device name was saved under <paramref name="key"/>; Nothing if it's gone.</summary>
    Friend Function SavedScreen(ByVal key As String) As Screen
        Dim device As String = GetSetting(AppName, Section, key & "Screen", "")
        If device = "" Then Return Nothing
        Return Screen.AllScreens.FirstOrDefault(Function(s) String.Equals(s.DeviceName, device, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>Index of the screen in Screen.AllScreens (= CoreMedia.Monitors); -1 if not found.</summary>
    Friend Function ScreenIndex(ByVal scr As Screen) As Integer
        If scr Is Nothing Then Return -1
        Return Array.FindIndex(Screen.AllScreens, Function(s) String.Equals(s.DeviceName, scr.DeviceName, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>The saved normal (restored) bounds, when they are still on the given screen.</summary>
    Friend Function SavedBounds(ByVal key As String, ByVal scr As Screen) As Rectangle?
        Dim parts() As String = GetSetting(AppName, Section, key & "Bounds", "").Split(","c)
        If parts.Length <> 4 Then Return Nothing
        Dim v(3) As Integer
        For i As Integer = 0 To 3
            If Not Integer.TryParse(parts(i), v(i)) Then Return Nothing
        Next
        Dim r As New Rectangle(v(0), v(1), v(2), v(3))
        If r.Width <= 0 OrElse r.Height <= 0 OrElse scr Is Nothing OrElse Not scr.WorkingArea.IntersectsWith(r) Then Return Nothing
        Return r
    End Function

    Friend Function SavedMaximized(ByVal key As String) As Boolean
        Return GetSetting(AppName, Section, key & "Maximized", "Y") = "Y"
    End Function

    ''' <summary>Saves the screen, the normal bounds and whether the window is maximised.</summary>
    Friend Sub Save(ByVal key As String, ByVal f As Form)
        If f Is Nothing OrElse f.IsDisposed OrElse f.WindowState = FormWindowState.Minimized Then Return
        Dim r As Rectangle = If(f.WindowState = FormWindowState.Normal, f.Bounds, f.RestoreBounds)
        SaveSetting(AppName, Section, key & "Screen", Screen.FromControl(f).DeviceName)
        SaveSetting(AppName, Section, key & "Bounds", $"{r.X},{r.Y},{r.Width},{r.Height}")
        SaveSetting(AppName, Section, key & "Maximized", If(f.WindowState = FormWindowState.Maximized, "Y", "N"))
    End Sub

    ''' <summary>Puts the window on <paramref name="scr"/>: its normal bounds moved there (and kept inside
    ''' its working area), maximised again if it was.</summary>
    Friend Sub MoveToScreen(ByVal f As Form, ByVal scr As Screen, ByVal maximize As Boolean)
        Dim wasMax As Boolean = f.WindowState = FormWindowState.Maximized
        Dim r As Rectangle = If(f.WindowState = FormWindowState.Normal, f.Bounds, f.RestoreBounds)
        Dim wa As Rectangle = scr.WorkingArea
        Dim w As Integer = Math.Min(r.Width, wa.Width), h As Integer = Math.Min(r.Height, wa.Height)
        If wasMax Then f.WindowState = FormWindowState.Normal
        f.Bounds = New Rectangle(wa.X + (wa.Width - w) \ 2, wa.Y + (wa.Height - h) \ 2, w, h)
        If maximize Then f.WindowState = FormWindowState.Maximized
    End Sub

End Module

Friend Class DualScreenSwap
    Implements IDisposable

    Friend Const MainKey As String = "Main"
    Friend Const ViewerKey As String = "Viewer"
    Friend Const PlaceMapKey As String = "PlaceMap"   ' the 地點 window, paired with the viewer while it is open

    ''' <summary>While True moves are ignored (another pair is using one of the windows); Resume takes the
    ''' windows' current screens as their homes again.</summary>
    Public Property Paused As Boolean

    Public Sub [Resume]()
        Paused = False
        Remember(m_main)
        If m_viewer.Form.Visible Then Remember(m_viewer)
    End Sub

    ''' <summary>The screen each window was last settled on, and whether it was maximised there.</summary>
    Private Class Home
        Public Form As Form
        Public Key As String
        Public Screen As Screen
        Public Maximized As Boolean
    End Class

    Private ReadOnly m_main As Home
    Private ReadOnly m_viewer As Home
    Private ReadOnly m_timer As New Timer With {.Interval = 300}
    Private m_busy As Boolean

    Public Sub New(ByVal main As Form, ByVal viewer As Form, Optional ByVal mainKey As String = DualScreenSwap.MainKey, Optional ByVal viewerKey As String = DualScreenSwap.ViewerKey)
        m_main = New Home With {.Form = main, .Key = mainKey}
        m_viewer = New Home With {.Form = viewer, .Key = viewerKey}
        Remember(m_main)
        Remember(m_viewer)
        For Each f As Form In {main, viewer}
            AddHandler f.LocationChanged, AddressOf Moved
            AddHandler f.SizeChanged, AddressOf Moved
        Next
        AddHandler m_timer.Tick, AddressOf Settle
    End Sub

    Private Sub Remember(ByVal h As Home)
        h.Screen = Screen.FromControl(h.Form)
        h.Maximized = h.Form.WindowState = FormWindowState.Maximized
    End Sub

    ' A move (or a restore when a maximised window is dragged) -- wait until it has settled.
    Private Sub Moved(sender As Object, e As EventArgs)
        If m_busy OrElse Paused Then Return
        m_timer.Stop()
        m_timer.Start()
    End Sub

    Private Sub Settle(sender As Object, e As EventArgs)
        If Control.MouseButtons <> MouseButtons.None Then Return      ' still dragging
        m_timer.Stop()
        If Paused OrElse m_main.Form.IsDisposed OrElse m_viewer.Form.IsDisposed Then Return
        If m_main.Form.WindowState = FormWindowState.Minimized OrElse m_viewer.Form.WindowState = FormWindowState.Minimized Then Return
        m_busy = True
        Try
            If Not TrySwap(m_main, m_viewer) Then TrySwap(m_viewer, m_main)
            Remember(m_main)
            If m_viewer.Form.Visible Then Remember(m_viewer)
            WindowPlacement.Save(m_main.Key, m_main.Form)
            If m_viewer.Form.Visible Then WindowPlacement.Save(m_viewer.Key, m_viewer.Form)
        Finally
            m_busy = False
        End Try
    End Sub

    ''' <summary>When <paramref name="moved"/> now sits on the other window's screen, the other one goes
    ''' to the screen <paramref name="moved"/> came from.</summary>
    Private Function TrySwap(ByVal moved As Home, ByVal other As Home) As Boolean
        If Screen.AllScreens.Length < 2 OrElse Not other.Form.Visible Then Return False
        Dim now As Screen = Screen.FromControl(moved.Form)
        If SameScreen(now, moved.Screen) OrElse Not SameScreen(now, other.Screen) Then Return False
        ' a maximised window that was dragged comes out restored; maximise it again where it landed
        If moved.Maximized AndAlso moved.Form.WindowState = FormWindowState.Normal Then moved.Form.WindowState = FormWindowState.Maximized
        WindowPlacement.MoveToScreen(other.Form, moved.Screen, other.Maximized)
        Return True
    End Function

    Private Shared Function SameScreen(ByVal a As Screen, ByVal b As Screen) As Boolean
        Return a IsNot Nothing AndAlso b IsNot Nothing AndAlso String.Equals(a.DeviceName, b.DeviceName, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        m_timer.Stop()
        m_timer.Dispose()
        For Each f As Form In {m_main.Form, m_viewer.Form}
            RemoveHandler f.LocationChanged, AddressOf Moved
            RemoveHandler f.SizeChanged, AddressOf Moved
        Next
    End Sub

End Class

''' <summary>
''' Keeps the windows opened from the main window on the main window's screen. The forms place
''' themselves in many ways -- CenterScreen (which WinForms puts on the *active* window's screen, the
''' viewer's if that was clicked last), WindowsDefaultLocation / Manual (always the primary screen), the
''' message boxes centring on the primary screen -- so instead of each form, one thread hook sees every
''' top-level window of the UI thread just before it is first shown (WM_SHOWWINDOW) and, when it is on
''' another screen than the main window, moves it there keeping its relative position (centred stays
''' centred). Nothing is visible yet, so it doesn't flicker. Windows owned by the viewer or the slide
''' show (and those themselves) are left alone: they belong on the viewer's screen.
''' </summary>
Friend NotInheritable Class ChildWindowsFollowMain
    Implements IDisposable

    Private Const WH_CALLWNDPROC As Integer = 4
    Private Const WM_SHOWWINDOW As Integer = &H18
    Private Const GW_OWNER As UInteger = 4
    Private Const GWL_STYLE As Integer = -16
    Private Const WS_CHILD As Integer = &H40000000
    Private Const SWP_NOSIZE As UInteger = &H1
    Private Const SWP_NOZORDER As UInteger = &H4
    Private Const SWP_NOACTIVATE As UInteger = &H10

    <StructLayout(LayoutKind.Sequential)>
    Private Structure CWPSTRUCT
        Public lParam As IntPtr
        Public wParam As IntPtr
        Public message As Integer
        Public hwnd As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left, Top, Right, Bottom As Integer
    End Structure

    Private Delegate Function HookProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowsHookEx(ByVal idHook As Integer, ByVal lpfn As HookProc, ByVal hMod As IntPtr, ByVal dwThreadId As Integer) As IntPtr
    End Function
    <DllImport("user32.dll")>
    Private Shared Function UnhookWindowsHookEx(ByVal hhk As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")>
    Private Shared Function CallNextHookEx(ByVal hhk As IntPtr, ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function
    <DllImport("kernel32.dll")>
    Private Shared Function GetCurrentThreadId() As Integer
    End Function
    <DllImport("user32.dll")>
    Private Shared Function GetWindow(ByVal hWnd As IntPtr, ByVal uCmd As UInteger) As IntPtr
    End Function
    <DllImport("user32.dll")>
    Private Shared Function GetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer) As Integer
    End Function
    <DllImport("user32.dll")>
    Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function
    <DllImport("user32.dll")>
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal x As Integer, ByVal y As Integer,
                                         ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInteger) As Boolean
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetClassName(ByVal hWnd As IntPtr, ByVal lpClassName As Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    Private ReadOnly m_main As Form
    Private ReadOnly m_proc As HookProc   ' kept referenced: the native hook calls it for the app's lifetime
    Private m_hook As IntPtr

    Public Sub New(ByVal main As Form)
        m_main = main
        m_proc = AddressOf OnHook
        m_hook = SetWindowsHookEx(WH_CALLWNDPROC, m_proc, IntPtr.Zero, GetCurrentThreadId())
    End Sub

    Private Function OnHook(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
        ' called for every message sent to any window of the UI thread: look at the message number
        ' alone first (CWPSTRUCT.message follows lParam and wParam) and marshal the rest only for ours
        If nCode >= 0 AndAlso Marshal.ReadInt32(lParam, 2 * IntPtr.Size) = WM_SHOWWINDOW Then
            Try
                Dim msg As CWPSTRUCT = Marshal.PtrToStructure(Of CWPSTRUCT)(lParam)
                ' wParam <> 0: being shown; lParam = 0: by ShowWindow (not a parent being restored)
                If msg.wParam <> IntPtr.Zero AndAlso msg.lParam = IntPtr.Zero Then Place(msg.hwnd)
            Catch
                ' never let an exception unwind through the native hook chain
            End Try
        End If
        Return CallNextHookEx(m_hook, nCode, wParam, lParam)
    End Function

    Private Sub Place(ByVal hwnd As IntPtr)
        If m_main.IsDisposed OrElse Not m_main.IsHandleCreated OrElse Not m_main.Visible Then Return   ' e.g. the splash
        If hwnd = m_main.Handle OrElse (GetWindowLong(hwnd, GWL_STYLE) And WS_CHILD) <> 0 Then Return
        Dim f As Form = TryCast(Control.FromHandle(hwnd), Form)
        If f Is Nothing AndAlso Not IsDialogClass(hwnd) Then Return   ' menus, tooltips, drop-downs ...
        If BelongsToViewer(hwnd) Then Return

        Dim r As RECT
        If Not GetWindowRect(hwnd, r) Then Return
        Dim bounds As New Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top)
        Dim target As Screen = Screen.FromHandle(m_main.Handle)
        Dim source As Screen = Screen.FromRectangle(bounds)
        If String.Equals(source.DeviceName, target.DeviceName, StringComparison.OrdinalIgnoreCase) Then Return

        If f IsNot Nothing AndAlso f.WindowState = FormWindowState.Maximized Then
            ' a maximised window can't just be moved: restore it there once it is up
            f.BeginInvoke(Sub() WindowPlacement.MoveToScreen(f, target, maximize:=True))
            Return
        End If
        Dim src As Rectangle = source.WorkingArea, dst As Rectangle = target.WorkingArea
        ' the same relative spot: the centre keeps its proportional position in the working area
        Dim cx As Double = dst.X + (bounds.X + bounds.Width / 2.0 - src.X) * dst.Width / src.Width
        Dim cy As Double = dst.Y + (bounds.Y + bounds.Height / 2.0 - src.Y) * dst.Height / src.Height
        Dim x As Integer = CInt(cx - bounds.Width / 2.0), y As Integer = CInt(cy - bounds.Height / 2.0)
        x = Math.Max(dst.Left, Math.Min(x, dst.Right - bounds.Width))
        y = Math.Max(dst.Top, Math.Min(y, dst.Bottom - bounds.Height))
        SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE Or SWP_NOZORDER Or SWP_NOACTIVATE)
    End Sub

    ''' <summary>The viewer or the slide show itself, or a window owned (directly or not) by one of them.</summary>
    Private Shared Function BelongsToViewer(ByVal hwnd As IntPtr) As Boolean
        Dim h As IntPtr = hwnd
        While h <> IntPtr.Zero
            Dim c As Control = Control.FromHandle(h)
            If TypeOf c Is PhotoLib.frmViewerLarge OrElse TypeOf c Is PhotoLib.frmViewerSmall OrElse TypeOf c Is PhotoLib.frmSlideShow Then Return True
            h = GetWindow(h, GW_OWNER)
        End While
        Return False
    End Function

    ''' <summary>A native dialog: MessageBox, the print / file dialogs.</summary>
    Private Shared Function IsDialogClass(ByVal hwnd As IntPtr) As Boolean
        Dim sb As New Text.StringBuilder(64)
        GetClassName(hwnd, sb, sb.Capacity)
        Return sb.ToString() = "#32770"
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If m_hook <> IntPtr.Zero Then
            UnhookWindowsHookEx(m_hook)
            m_hook = IntPtr.Zero
        End If
    End Sub

End Class
