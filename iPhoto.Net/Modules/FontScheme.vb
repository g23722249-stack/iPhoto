Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

' 設定 › 外觀「顯示字型」 (Config.InterfaceFontName) for every window. The forms are designed in 華康細圓體;
' when the setting names another font -- or 華康細圓體 is missing and Config fell back to 標楷體 / 細明體 --
' every control in 華康細圓體 gets that font instead, the size and style kept. Each window is done as it
' is made (its WM_CREATE, before its Load runs, so what Load builds from a control's font follows), and
' a control added to it later as it is added. A thread hook (WH_CALLWNDPROC, the UI thread) sees every
' window; nothing is hooked when the setting is 華康細圓體 itself. A change in 設定 takes effect at the
' next start (frmSetup asks to restart).
Friend Module FontScheme

    Private Const DesignFamily As String = "華康細圓體"

    Private s_target As String            ' the family to use instead; Nothing: nothing to change
    Private s_designNames As HashSet(Of String)
    Private s_hook As IntPtr
    Private s_proc As HookProc
    Private ReadOnly s_watched As New ConditionalWeakTable(Of Control, Object)

    ''' <summary>Program.Run, once the settings are read and before the first window.</summary>
    Public Sub Install()
        If s_hook <> IntPtr.Zero OrElse g_lpConfig Is Nothing Then Return
        Dim wanted As String = If(g_lpConfig.InterfaceFontName, "").Trim()
        If wanted.Length = 0 Then Return
        s_designNames = NamesOf(DesignFamily)
        s_designNames.Add(DesignFamily)
        If s_designNames.Overlaps(NamesOf(wanted)) Then Return   ' the design font itself
        s_target = wanted
        s_proc = AddressOf OnHook
        s_hook = SetWindowsHookEx(WH_CALLWNDPROC, s_proc, IntPtr.Zero, GetCurrentThreadId())
    End Sub

    ''' <summary>The names GDI+ gives a family (in the UI language and in English); empty when it isn't there.</summary>
    Private Function NamesOf(ByVal family As String) As HashSet(Of String)
        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Try
            Using ff As New FontFamily(family)
                names.Add(ff.Name)
                names.Add(ff.GetName(&H404))
                names.Add(ff.GetName(&H409))
            End Using
        Catch ex As ArgumentException
        End Try
        Return names
    End Function

    Private Function OnHook(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
        If nCode >= 0 Then
            Dim msg As CWPSTRUCT = Marshal.PtrToStructure(Of CWPSTRUCT)(lParam)
            If msg.message = WM_CREATE Then
                Dim f As Form = TryCast(Control.FromHandle(msg.hwnd), Form)
                If f IsNot Nothing Then Apply(f)
            End If
        End If
        Return CallNextHookEx(s_hook, nCode, wParam, lParam)
    End Function

    ''' <summary><paramref name="c"/> and everything in it, and what is added to them later.</summary>
    Private Sub Apply(ByVal c As Control)
        Dim f As Font = c.Font
        ' a control that only follows its parent's font (the same object) changes with the parent
        Dim own As Boolean = c.Parent Is Nothing OrElse Not ReferenceEquals(f, c.Parent.Font)
        If own AndAlso s_designNames.Contains(If(f.OriginalFontName, f.Name)) Then
            c.Font = New Font(s_target, f.Size, f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont)
        End If
        For Each child As Control In c.Controls
            Apply(child)
        Next
        Dim seen As Object = Nothing
        If Not s_watched.TryGetValue(c, seen) Then
            s_watched.Add(c, Nothing)
            AddHandler c.ControlAdded, Sub(sender, e) Apply(e.Control)
        End If
    End Sub

    Private Delegate Function HookProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr

    Private Const WH_CALLWNDPROC As Integer = 4
    Private Const WM_CREATE As Integer = &H1

    <StructLayout(LayoutKind.Sequential)>
    Private Structure CWPSTRUCT
        Public lParam As IntPtr
        Public wParam As IntPtr
        Public message As Integer
        Public hwnd As IntPtr
    End Structure

    <DllImport("user32.dll")>
    Private Function SetWindowsHookEx(ByVal idHook As Integer, ByVal lpfn As HookProc, ByVal hMod As IntPtr, ByVal dwThreadId As Integer) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function CallNextHookEx(ByVal hhk As IntPtr, ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Private Function GetCurrentThreadId() As Integer
    End Function

End Module
