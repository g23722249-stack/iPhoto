Option Strict On
Option Explicit On

Imports System.Runtime.InteropServices
Imports System.Text

' Port of TechnoSoft Carbon.IniFile (D:\專案\RunTime\Carbon\CoClass\IniFile.cls): the same
' Get/WritePrivateProfileString calls. The Unicode API reads iPhoto.Ini (saved in the system code
' page, Big5) correctly, because Windows converts non-Unicode INI files with the ANSI code page.
Namespace Carbon

    Public Class IniFile

        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)>
        Private Shared Function GetPrivateProfileString(ByVal section As String, ByVal key As String, ByVal def As String,
                                                        ByVal buffer As StringBuilder, ByVal size As Integer, ByVal path As String) As Integer
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, EntryPoint:="GetPrivateProfileStringW")>
        Private Shared Function GetPrivateProfileNames(ByVal section As String, ByVal key As String, ByVal def As String,
                                                       <Out> ByVal buffer As Char(), ByVal size As Integer, ByVal path As String) As Integer
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)>
        Private Shared Function WritePrivateProfileString(ByVal section As String, ByVal key As String, ByVal value As String, ByVal path As String) As Boolean
        End Function

        ''' <summary>Full path of the .ini file.</summary>
        Public Property FileName As String = ""

        ''' <summary>Trimmed value, or <paramref name="default"/> when the key is missing (VB6: SimpleGetValue).</summary>
        Public Function SimpleGetValue(ByVal section As String, ByVal key As String, Optional ByVal [default] As String = "") As String
            Dim sb As New StringBuilder(4096)
            GetPrivateProfileString(section, key, If([default], ""), sb, sb.Capacity, FileName)
            Return sb.ToString().Trim()
        End Function

        ''' <summary>Writes the trimmed value (cut at any embedded Chr(0), as VB6 did). True on success.</summary>
        Public Function SimpleSetValue(ByVal section As String, ByVal key As String, ByVal value As String) As Boolean
            Dim v As String = If(value, "")
            Dim nul As Integer = v.IndexOf(ChrW(0))
            If nul >= 0 Then v = v.Substring(0, nul)
            Return WritePrivateProfileString(section, key, v.Trim(), FileName)
        End Function

        Public Function DeleteKey(ByVal section As String, ByVal key As String) As Boolean
            Return WritePrivateProfileString(section, key, Nothing, FileName)
        End Function

        Public Function DeleteSection(ByVal section As String) As Boolean
            Return WritePrivateProfileString(section, Nothing, Nothing, FileName)
        End Function

        ''' <summary>Key names of a section, in file order. Note: a key repeated in the same section
        ''' (iPhoto.Ini's [Album] has several "Path=" lines) is returned once per occurrence.</summary>
        Public Function EnumerateKeys(ByVal section As String) As String()
            Return ReadNames(section)
        End Function

        Public Function EnumerateSections() As String()
            Return ReadNames(Nothing)
        End Function

        Private Function ReadNames(ByVal section As String) As String()
            Dim size As Integer = 8192
            Do
                Dim buf(size - 1) As Char
                Dim n As Integer = GetPrivateProfileNames(section, Nothing, "", buf, size, FileName)
                If n < size - 2 Then
                    Return New String(buf, 0, n).Split({ChrW(0)}, StringSplitOptions.RemoveEmptyEntries)
                End If
                size *= 2
            Loop
        End Function

    End Class

End Namespace
