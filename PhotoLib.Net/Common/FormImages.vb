Imports System.IO
Imports System.Reflection

' Loads the images carved out of the VB6 .frx files (Tools\Export-FrxImages.ps1), embedded per form
' as <prefix>.Forms.<Form>\<Control>[_<Index>]_<Property>.<ext>. Where VB6 wrote
'     imgButton(1).MaskImage = "frmMain.frx":235E76
' ported code writes
'     imgButton1.MaskImage = FormImages.Load(Me, "imgButton_1_MaskImage")
' The PNG (magenta already transparent) is preferred when the export produced one; otherwise the
' original BMP/ICO/JPG/GIF is used.
Public Module FormImages

    Private ReadOnly Extensions As String() = {".png", ".bmp", ".ico", ".jpg", ".gif", ".cur"}

    ''' <summary>Image <paramref name="name"/> of <paramref name="form"/>'s VB6 form (the form's VB_Name
    ''' is taken from its class name). Throws if it isn't embedded, so a typo fails loudly.</summary>
    Public Function Load(ByVal form As Object, ByVal name As String) As Image
        Return Load(form.GetType().Assembly, form.GetType().Name, name)
    End Function

    Public Function Load(ByVal asm As Assembly, ByVal formName As String, ByVal name As String) As Image
        For Each ext In Extensions
            Using s As Stream = Find(asm, formName, name & ext)
                If s Is Nothing Then Continue For
                If ext = ".ico" OrElse ext = ".cur" Then
                    Using ic As New Icon(s)
                        Return ic.ToBitmap()
                    End Using
                End If
                ' copy so the image doesn't depend on the (disposed) resource stream
                Using img As Image = Image.FromStream(s)
                    Return New Bitmap(img)
                End Using
            End Using
        Next
        Throw New FileNotFoundException($"Image '{name}' of form '{formName}' is not embedded in {asm.GetName().Name}.")
    End Function

    ''' <summary>An embedded .ico as an Icon (Form.Icon and friends).</summary>
    Public Function LoadIcon(ByVal form As Object, ByVal name As String) As Icon
        Dim asm As Assembly = form.GetType().Assembly
        Using s As Stream = Find(asm, form.GetType().Name, name & ".ico")
            If s Is Nothing Then Throw New FileNotFoundException($"Icon '{name}' of form '{form.GetType().Name}' is not embedded.")
            Return New Icon(s)
        End Using
    End Function

    Private Function Find(ByVal asm As Assembly, ByVal formName As String, ByVal fileName As String) As Stream
        Dim suffix As String = ".Forms." & formName & "\" & fileName
        For Each res In asm.GetManifestResourceNames()
            If res.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then Return asm.GetManifestResourceStream(res)
        Next
        Return Nothing
    End Function

End Module
