Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

' Port of TechnoSoft CoreMedia.Monitors / Monitor (D:\專案\RunTime\CoreMedia\CoClass\Monitors.cls,
' Monitor.cls): the EnumDisplayMonitors wrapper, rebuilt on Screen.AllScreens. Indexes are 0-based
' like VB6's Monitor(Index); all sizes are in pixels.
Namespace CoreMedia

    Public Class Monitors

        Private _screens As Screen()

        Public Sub New()
            Refresh()
        End Sub

        Public Sub Refresh()
            _screens = Screen.AllScreens
        End Sub

        Public ReadOnly Property MonitorCount As Integer
            Get
                Return _screens.Length
            End Get
        End Property

        Public ReadOnly Property Monitor(ByVal index As Integer) As Monitor
            Get
                Return New Monitor(_screens(index))
            End Get
        End Property

        Public ReadOnly Property MonitorForPoint(ByVal x As Integer, ByVal y As Integer) As Monitor
            Get
                Return New Monitor(Screen.FromPoint(New Point(x, y)))
            End Get
        End Property

        Public ReadOnly Property MonitorForWindow(ByVal hwnd As IntPtr) As Monitor
            Get
                Return New Monitor(Screen.FromHandle(hwnd))
            End Get
        End Property

        Public ReadOnly Property VirtualScreenLeft As Integer
            Get
                Return SystemInformation.VirtualScreen.Left
            End Get
        End Property

        Public ReadOnly Property VirtualScreenTop As Integer
            Get
                Return SystemInformation.VirtualScreen.Top
            End Get
        End Property

        Public ReadOnly Property VirtualScreenWidth As Integer
            Get
                Return SystemInformation.VirtualScreen.Width
            End Get
        End Property

        Public ReadOnly Property VirtualScreenHeight As Integer
            Get
                Return SystemInformation.VirtualScreen.Height
            End Get
        End Property

    End Class

    Public Class Monitor

        Private ReadOnly _screen As Screen

        Friend Sub New(ByVal s As Screen)
            _screen = s
        End Sub

        Public ReadOnly Property IsPrimary As Boolean
            Get
                Return _screen.Primary
            End Get
        End Property

        Public ReadOnly Property Name As String
            Get
                Return _screen.DeviceName
            End Get
        End Property

        Public ReadOnly Property Left As Integer
            Get
                Return _screen.Bounds.Left
            End Get
        End Property

        Public ReadOnly Property Top As Integer
            Get
                Return _screen.Bounds.Top
            End Get
        End Property

        Public ReadOnly Property Width As Integer
            Get
                Return _screen.Bounds.Width
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return _screen.Bounds.Height
            End Get
        End Property

        Public ReadOnly Property WorkLeft As Integer
            Get
                Return _screen.WorkingArea.Left
            End Get
        End Property

        Public ReadOnly Property WorkTop As Integer
            Get
                Return _screen.WorkingArea.Top
            End Get
        End Property

        Public ReadOnly Property WorkWidth As Integer
            Get
                Return _screen.WorkingArea.Width
            End Get
        End Property

        Public ReadOnly Property WorkHeight As Integer
            Get
                Return _screen.WorkingArea.Height
            End Get
        End Property

        ''' <summary>The monitor's full bounds in screen pixels.</summary>
        Public ReadOnly Property Bounds As Rectangle
            Get
                Return _screen.Bounds
            End Get
        End Property

    End Class

End Namespace
