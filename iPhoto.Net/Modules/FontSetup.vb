Imports System.Drawing.Text
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

' The forms are drawn in 華康細圓體 (DFYuanLight-B5); without it Windows puts another font in and the
' texts no longer fit. At start (Program.Main, before any window) the font is looked for, and when it
' is missing the copy shipped next to iPhoto.exe (細圓體.TTC, both 華康細圓體 and 華康細圓體(P)) is
' installed for the current user -- as Windows' own "Install" does for a user without administrator
' rights: the file into %LOCALAPPDATA%\Microsoft\Windows\Fonts, a value under HKCU ...\Fonts, and
' AddFontResource so this process (and running programs, WM_FONTCHANGE) can use it at once.
Friend Module FontSetup

    Private Const FamilyName As String = "華康細圓體"          ' its zh-TW name (the forms ask for this)
    Private Const EnglishName As String = "DFYuanLight-B5"
    Private Const FontFile As String = "細圓體.TTC"
    Private Const RegistryName As String = "華康細圓體 & 華康細圓體(P) (TrueType)"

    ''' <summary>Makes sure 華康細圓體 is there; True when it is (already, or installed now).</summary>
    Public Function EnsureFont() As Boolean
        If IsInstalled() Then Return True
        Dim source As String = IO.Path.Combine(AppContext.BaseDirectory, FontFile)
        If Not IO.File.Exists(source) Then
            Warn("找不到字型「" & FamilyName & "」，iPhoto 資料夾裡也沒有 " & FontFile & "。" & vbCrLf & "畫面文字會改用其他字型顯示。")
            Return False
        End If
        Dim err As String = InstallForUser(source)
        If err Is Nothing AndAlso IsInstalled() Then Return True
        Warn("無法自動安裝字型「" & FamilyName & "」" & If(err IsNot Nothing, "：" & err, "。") & vbCrLf &
             "請在檔案總管對 iPhoto 資料夾裡的 " & FontFile & " 按右鍵選「安裝」。" & vbCrLf & "這次畫面文字會改用其他字型顯示。")
        Return False
    End Function

    ''' <summary>The family is among the installed fonts (by its zh-TW or English name).</summary>
    Private Function IsInstalled() As Boolean
        Using fonts As New InstalledFontCollection()
            For Each f In fonts.Families
                If String.Equals(f.Name, EnglishName, StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(f.GetName(&H404), FamilyName, StringComparison.Ordinal) Then Return True
            Next
        End Using
        Return False
    End Function

    ''' <summary>Per-user install; Nothing when it went through, else what failed.</summary>
    Private Function InstallForUser(ByVal source As String) As String
        Try
            Dim folder As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts")
            IO.Directory.CreateDirectory(folder)
            Dim target As String = IO.Path.Combine(folder, FontFile)
            If Not IO.File.Exists(target) Then IO.File.Copy(source, target)
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey("Software\Microsoft\Windows NT\CurrentVersion\Fonts")
                key.SetValue(RegistryName, target, RegistryValueKind.String)
            End Using
            If AddFontResource(target) = 0 Then Return "Windows 沒有接受這個字型檔"
            ' tell running programs (without waiting long for one that doesn't answer)
            Dim result As IntPtr
            SendMessageTimeout(New IntPtr(HWND_BROADCAST), WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, 1000, result)
            Return Nothing
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is Security.SecurityException
            Return ex.Message
        End Try
    End Function

    Private Sub Warn(ByVal text As String)
        MessageBox.Show(text, "iPhoto 字型", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Const HWND_BROADCAST As Integer = &HFFFF
    Private Const WM_FONTCHANGE As Integer = &H1D
    Private Const SMTO_ABORTIFHUNG As Integer = &H2

    <DllImport("gdi32.dll", CharSet:=CharSet.Unicode)>
    Private Function AddFontResource(ByVal lpFileName As String) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Function SendMessageTimeout(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr,
                                        ByVal flags As Integer, ByVal timeout As Integer, ByRef result As IntPtr) As IntPtr
    End Function

End Module
