Imports System.Runtime.InteropServices

' Dims a window while one of its feature dialogs is open (new in the .NET port; VB6 had two attempts,
' both left commented out: DarkScreen patterned the whole screen, SetOpacityWindow made the main window
' see-through instead of darker).
'
'   Private m_lpDimmer As ModalDimmer
'   m_lpDimmer = New ModalDimmer(Me)        ' in the main window's Load
'
' A modal dialog (ShowDialog) disables every other window of the application while it is open, so the
' window watched here gets WM_ENABLE(False) when any dialog opens and WM_ENABLE(True) when the last one
' closes: no call site has to do anything. On the first, a black, half-transparent, click-through window
' is laid exactly over the watched window, right under the dialog, and faded in; on the second it fades
' out. Nested dialogs keep the one overlay.
' Not dimmed (NotDimmed): message / question / input boxes, file and folder pickers, the subject icon
' picker, and the full-screen viewers and slide show -- by form type name, so a new dialog is dimmed by
' default. Nor are other windows (the viewer on a second screen) touched.
Public NotInheritable Class ModalDimmer
    Inherits NativeWindow
    Implements IDisposable

    ''' <summary>Dialogs that don't dim the window behind them (type names).</summary>
    Public Shared ReadOnly NotDimmed As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "frmMsgBox", "frmQueryMsgBox", "frmInputString", "frmSubject", "frmBrowserFile", "frmBrowserFolder",
        "frmSaveChangedPhoto", "frmLoading", "frmLogo",
        "frmViewerLarge", "frmViewerSmall", "frmSlideShow"}

    ''' <summary>How dark the overlay gets (0..1).</summary>
    Public Property Darkness As Double = 0.6
    ''' <summary>Fade in / out time (ms).</summary>
    Public Property FadeTime As Integer = 150

    <DllImport("user32.dll")>
    Private Shared Function IsWindowEnabled(hWnd As IntPtr) As Boolean
    End Function

    Private Const WM_ENABLE As Integer = &HA
    Private Const FadeStep As Integer = 15            ' ms per fade tick
    Private Const FindRetries As Integer = 10         ' the dialog may not be shown yet when WM_ENABLE comes

    Private ReadOnly m_owner As Form
    Private ReadOnly m_overlay As New Overlay
    Private WithEvents m_fade As New Timer With {.Interval = FadeStep}
    Private m_target As Double                       ' opacity the fade goes to
    Private m_intFindTries As Integer

    Public Sub New(ByVal owner As Form)
        m_owner = owner
        If owner.IsHandleCreated Then AssignHandle(owner.Handle)
        AddHandler owner.HandleCreated, Sub(s, e) AssignHandle(m_owner.Handle)
        AddHandler owner.HandleDestroyed, Sub(s, e) ReleaseHandle()
        AddHandler owner.LocationChanged, Sub(s, e) FollowOwner()
        AddHandler owner.SizeChanged, Sub(s, e) FollowOwner()
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg <> WM_ENABLE Then Return
        If m.WParam = IntPtr.Zero Then
            ' disabled: a dialog is opening; it is shown a moment later
            m_intFindTries = 0
            m_owner.BeginInvoke(New Action(AddressOf DimForDialog))
        Else
            FadeTo(0)
        End If
    End Sub

    ''' <summary>The dialog that disabled the owner: the last modal form opened (not one already handled).</summary>
    Private Function OpenDialog() As Form
        Dim found As Form = Nothing
        For Each f As Form In Application.OpenForms
            If f IsNot m_owner AndAlso f IsNot m_overlay AndAlso f.Modal AndAlso f.Visible Then found = f
        Next
        Return found
    End Function

    Private Sub DimForDialog()
        ' (Form.Enabled stays True: ShowDialog disables the window natively, so ask Windows)
        If m_owner.IsDisposed OrElse IsWindowEnabled(m_owner.Handle) OrElse Not m_owner.Visible OrElse m_owner.WindowState = FormWindowState.Minimized Then Return
        Dim dlg As Form = OpenDialog()
        If dlg Is Nothing Then
            ' not shown yet (or a system dialog such as MessageBox, which isn't a Form): look again shortly
            m_intFindTries += 1
            If m_intFindTries < FindRetries Then
                Dim t As New Timer With {.Interval = 30}
                AddHandler t.Tick, Sub(s, e)
                                       t.Dispose()
                                       DimForDialog()
                                   End Sub
                t.Start()
            End If
            Return
        End If
        If NotDimmed.Contains(dlg.GetType().Name) Then Return

        FollowOwner()
        ' owned by the watched window, so it stays above it when the application is switched back to
        If m_overlay.Owner IsNot m_owner Then m_overlay.Owner = m_owner
        If Not m_overlay.Visible Then
            m_overlay.Opacity = 0
            m_overlay.Show()
        End If
        m_overlay.PlaceUnder(dlg)
        FadeTo(Darkness)
    End Sub

    Private Sub FollowOwner()
        If m_overlay.IsDisposed Then Return
        m_overlay.Bounds = m_owner.Bounds
    End Sub

    Private Sub FadeTo(ByVal target As Double)
        m_target = target
        If Not m_overlay.Visible AndAlso target = 0 Then Return
        m_fade.Start()
    End Sub

    Private Sub m_fade_Tick(sender As Object, e As EventArgs) Handles m_fade.Tick
        Dim stepSize As Double = Darkness * FadeStep / Math.Max(FadeStep, FadeTime)
        Dim o As Double = m_overlay.Opacity
        o = If(o < m_target, Math.Min(m_target, o + stepSize), Math.Max(m_target, o - stepSize))
        m_overlay.Opacity = o
        If o = m_target Then
            m_fade.Stop()
            If o = 0 Then m_overlay.Hide()
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        m_fade.Dispose()
        m_overlay.Dispose()
        ReleaseHandle()
    End Sub

    '==================================================================================================
    ''' <summary>The black window: no border, no task-bar button, never activated, clicks go through to
    ''' the window under it (which is disabled, so Windows points at the dialog).</summary>
    Private Class Overlay
        Inherits Form

        <DllImport("user32.dll")>
        Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr, x As Integer, y As Integer, cx As Integer, cy As Integer, flags As UInteger) As Boolean
        End Function
        Private Const SWP_NOSIZE As UInteger = &H1, SWP_NOMOVE As UInteger = &H2, SWP_NOACTIVATE As UInteger = &H10

        Public Sub New()
            FormBorderStyle = FormBorderStyle.None
            ShowInTaskbar = False
            StartPosition = FormStartPosition.Manual
            BackColor = Color.Black
            Opacity = 0
        End Sub

        Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                Const WS_EX_TOOLWINDOW As Integer = &H80
                Const WS_EX_NOACTIVATE As Integer = &H8000000
                Const WS_EX_TRANSPARENT As Integer = &H20
                Const WS_EX_LAYERED As Integer = &H80000
                Dim cp As CreateParams = MyBase.CreateParams
                cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW Or WS_EX_NOACTIVATE Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
                Return cp
            End Get
        End Property

        ''' <summary>Right under <paramref name="dlg"/> in the z-order (so over the dimmed window).</summary>
        Public Sub PlaceUnder(ByVal dlg As Form)
            SetWindowPos(Handle, dlg.Handle, 0, 0, 0, 0, SWP_NOMOVE Or SWP_NOSIZE Or SWP_NOACTIVATE)
        End Sub
    End Class

End Class
