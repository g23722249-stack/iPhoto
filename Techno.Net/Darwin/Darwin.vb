Option Strict On
Option Explicit On

Imports System.Runtime.InteropServices
Imports System.Windows.Forms

' Port of the TechnoSoft Darwin library's global functions (Darwin Export.cls) that iPhoto calls as
' Darwin.Wait(...) / Darwin.ShowWindow(...). A VB Module's members are promoted to its namespace,
' so those calls compile unchanged against Namespace Darwin.
Namespace Darwin

    Public Module DarwinExport

        <DllImport("user32.dll")>
        Private Function ShowWindowAsync(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
        End Function

        Private Const SW_SHOW As Integer = 5
        Private Const SW_SHOWMAXIMIZED As Integer = 3
        Private Const SW_SHOWMINIMIZED As Integer = 2

        ''' <summary>Busy-waits while pumping messages, exactly like VB6's GetTickCount/DoEvents loop.
        ''' iPhoto only uses it for 50-100 ms click flashes; for anything longer prefer a Timer or
        ''' Await Task.Delay, since DoEvents re-enters the message loop.</summary>
        Public Sub Wait(ByVal milliseconds As Integer)
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Do While sw.ElapsedMilliseconds <= milliseconds
                Application.DoEvents()
                Threading.Thread.Sleep(1)
            Loop
        End Sub

        Public Sub ShowWindow(ByVal hwnd As IntPtr, Optional ByVal state As FormWindowState = FormWindowState.Normal)
            Select Case state
                Case FormWindowState.Maximized : ShowWindowAsync(hwnd, SW_SHOWMAXIMIZED)
                Case FormWindowState.Minimized : ShowWindowAsync(hwnd, SW_SHOWMINIMIZED)
                Case Else : ShowWindowAsync(hwnd, SW_SHOW)
            End Select
        End Sub

    End Module

End Namespace
