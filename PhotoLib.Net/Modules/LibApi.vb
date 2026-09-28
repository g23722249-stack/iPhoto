' Port of Lib\Module\LibApi.bas: of its Win32 wrappers the ported code only needs these two (callers
' of SetOpacityWindow were already commented out in VB6); the rest (DarkScreen, special folders,
' AdvanceShell, GetTextBoxLineCount, IsDesignMode, MoveWindow, DrawRotatedText) have direct .NET
' equivalents (Environment.GetFolderPath, Process.Start, TextBox.Lines, DesignMode, Bounds,
' Graphics.RotateTransform) and nothing calls them.
Public Module LibApi

    ''' <summary>Keeps the window above the others (VB6 SetWindowPos HWND_TOPMOST).</summary>
    Public Sub SetTopMostWindow(ByVal form As Form, ByVal topMost As Boolean)
        form.TopMost = topMost
    End Sub

    ''' <summary>Window transparency, 0 (invisible) .. 255 (opaque) (VB6 SetLayeredWindowAttributes).</summary>
    Public Sub SetOpacityWindow(ByVal form As Form, ByVal alpha As Integer)
        form.Opacity = Math.Max(0, Math.Min(255, alpha)) / 255.0
    End Sub

End Module
