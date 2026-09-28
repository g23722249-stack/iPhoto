' Marks VB6 features whose code is not ported yet. The callers keep the VB6 flow (and the VB6
' method signatures on the forms), so porting a form later means replacing its stub, not the caller.
Public Module PortStatus

    ''' <summary>Says the feature is not ported yet instead of showing a form that has controls but no code
    ''' (an unported modal Aqua dialog has no working close button, so it would trap the user).</summary>
    Public Sub NotPorted(ByVal what As String)
        Dim busy As Boolean = Application.UseWaitCursor
        Application.UseWaitCursor = False
        MessageBox.Show("此功能尚未移植到 .NET 版：" & vbCrLf & vbCrLf & what, "iPhoto",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
        Application.UseWaitCursor = busy
    End Sub

End Module
