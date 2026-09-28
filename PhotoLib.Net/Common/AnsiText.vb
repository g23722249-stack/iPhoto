Imports System.Runtime.InteropServices
Imports System.Text

' The system ANSI code page (Big5 on these machines). VB6 read and wrote text files (iPhoto.Ini,
' Note.Ini, *.Exif, *.Alm) with Open/Line Input/Print, i.e. in that code page; in .NET 8
' Encoding.Default is UTF-8, so every port of that file I/O must use AnsiText.Encoding instead.
Public Module AnsiText

    <DllImport("kernel32.dll")>
    Private Function GetACP() As Integer
    End Function

    Private _encoding As Encoding

    Public ReadOnly Property Encoding As Encoding
        Get
            If _encoding Is Nothing Then
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
                _encoding = System.Text.Encoding.GetEncoding(GetACP())
            End If
            Return _encoding
        End Get
    End Property

End Module
