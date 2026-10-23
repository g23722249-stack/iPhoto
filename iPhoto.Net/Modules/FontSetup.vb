Imports System.Runtime.InteropServices
Imports Microsoft.Win32

' The forms are drawn in 華康細圓體 (DFYuanLight-B5); without it Windows puts another font in and the
' texts no longer fit. At start (Program.Main, before any window) the font is looked for, and when it
' is missing the copy shipped next to iPhoto.exe (細圓體.TTC, both 華康細圓體 and 華康細圓體(P)) is
' installed for the current user -- as Windows' own "Install" does for a user without administrator
' rights: the file into %LOCALAPPDATA%\Microsoft\Windows\Fonts, a value under HKCU ...\Fonts, and
' AddFontResource so this process (and running programs, WM_FONTCHANGE) can use it at once.
' A font installed but not loaded in this session (another user's install, a stick moved between
' computers) is loaded from the file the Fonts keys name before anything is asked.
Friend Module FontSetup

    Private Const FamilyName As String = "華康細圓體"          ' its zh-TW name (the forms ask for this)
    Private Const EnglishName As String = "DFYuanLight-B5"
    Private Const FontFile As String = "細圓體.TTC"
    Private Const RegistryName As String = "華康細圓體 & 華康細圓體(P) (TrueType)"

    ''' <summary>Makes sure 華康細圓體 is there; True when it is (already, or installed now).</summary>
    Public Function EnsureFont() As Boolean
        If IsInstalled() Then Return True
        ' installed (by Windows' 「安裝」 for all users or for this one, or by an earlier start) but not loaded
        ' in this session: loaded for this process from the file it was installed as, no question asked
        If LoadRegistered() AndAlso IsInstalled() Then Return True
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

    ''' <summary>Windows has the family: a font asked for by its zh-TW or English name is really that one
    ''' (GetTextFace; Windows answers with another face when it substitutes). Asked of GDI, not GDI+: GDI+
    ''' keeps the font list it read first and misses a font loaded after (AddFontResource), which made
    ''' this say "missing" right after the install.</summary>
    Private Function IsInstalled() As Boolean
        Return {FamilyName, EnglishName}.Any(Function(n) FaceOf(n).Equals(FamilyName, StringComparison.Ordinal) OrElse
                                                         FaceOf(n).Equals(EnglishName, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>The face Windows uses when asked for <paramref name="name"/>.</summary>
    Private Function FaceOf(ByVal name As String) As String
        Dim dc As IntPtr = GetDC(IntPtr.Zero)
        Dim font As IntPtr = CreateFont(-16, 0, 0, 0, 400, 0, 0, 0, CHINESEBIG5_CHARSET, 0, 0, 0, 0, name)
        Try
            Dim old As IntPtr = SelectObject(dc, font)
            Dim sb As New Text.StringBuilder(64)
            GetTextFace(dc, sb.Capacity, sb)
            SelectObject(dc, old)
            Return sb.ToString()
        Finally
            DeleteObject(font)
            ReleaseDC(IntPtr.Zero, dc)
        End Try
    End Function

    ''' <summary>The font files the Fonts keys (all users, this user) list for 華康細圓體 / 細圓體.TTC, loaded
    ''' into this process (AddFontResource). True when one was.</summary>
    Private Function LoadRegistered() As Boolean
        Dim loaded As Boolean = False
        For Each root In {Registry.LocalMachine, Registry.CurrentUser}
            Try
                Using key As RegistryKey = root.OpenSubKey("Software\Microsoft\Windows NT\CurrentVersion\Fonts")
                    If key Is Nothing Then Continue For
                    For Each value In key.GetValueNames()
                        Dim file As String = Convert.ToString(key.GetValue(value))
                        If value.IndexOf(FamilyName, StringComparison.Ordinal) < 0 AndAlso value.IndexOf("DFYuanLight", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
                           Not String.Equals(IO.Path.GetFileName(file), FontFile, StringComparison.OrdinalIgnoreCase) Then Continue For
                        ' all users' entries hold the file name only: it is in %WINDIR%\Fonts
                        If Not IO.Path.IsPathRooted(file) Then file = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), file)
                        If IO.File.Exists(file) AndAlso AddFontResource(file) > 0 Then loaded = True
                    Next
                End Using
            Catch ex As Exception When TypeOf ex Is Security.SecurityException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IO.IOException
            End Try
        Next
        Return loaded
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

    Private Const CHINESEBIG5_CHARSET As Integer = 136

    <DllImport("gdi32.dll", CharSet:=CharSet.Unicode, EntryPoint:="CreateFontW")>
    Private Function CreateFont(ByVal height As Integer, ByVal width As Integer, ByVal escapement As Integer, ByVal orientation As Integer,
                                ByVal weight As Integer, ByVal italic As Integer, ByVal underline As Integer, ByVal strikeOut As Integer,
                                ByVal charSet As Integer, ByVal outPrecision As Integer, ByVal clipPrecision As Integer,
                                ByVal quality As Integer, ByVal pitchAndFamily As Integer, ByVal face As String) As IntPtr
    End Function

    <DllImport("gdi32.dll", CharSet:=CharSet.Unicode, EntryPoint:="GetTextFaceW")>
    Private Function GetTextFace(ByVal dc As IntPtr, ByVal count As Integer, ByVal face As Text.StringBuilder) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Function SelectObject(ByVal dc As IntPtr, ByVal obj As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Function DeleteObject(ByVal obj As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function GetDC(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function ReleaseDC(ByVal hWnd As IntPtr, ByVal dc As IntPtr) As Integer
    End Function

    <DllImport("gdi32.dll", CharSet:=CharSet.Unicode)>
    Private Function AddFontResource(ByVal lpFileName As String) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Function SendMessageTimeout(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr,
                                        ByVal flags As Integer, ByVal timeout As Integer, ByRef result As IntPtr) As IntPtr
    End Function

End Module
