' Port of Sub Main in iPhoto\Module\LibMain.bas (VB6 project Startup = "Sub Main").
'
' Differences from VB6:
'   - frmMain_1440X900_NEW (picked for wide screens) is not ported: frmMain is always used.
'   - Screen sizes come from CoreMedia.Monitors (Techno.Net), which already reports pixels, so the
'     VB6 twips conversions (ScaleX / TwipsPerPixel) disappear.
'   - The app stays DPI-unaware like the VB6 one (Aqua skins are pixel artwork); Windows scales it.
'   - App.PrevInstance (checked in the VB6 main form's Load) is a named mutex here.
Friend Module Program

    <STAThread>
    Friend Sub Main()
        Dim createdNew As Boolean
        Using instanceLock As New Threading.Mutex(True, "iPhoto.Net.SingleInstance", createdNew)
            If Not createdNew Then
                ' started by 「現在重新開啟 iPhoto」 (RestartArg): wait for the old one to close
                If Not Environment.GetCommandLineArgs().Contains(RestartArg) Then Return
                Try
                    If Not instanceLock.WaitOne(TimeSpan.FromSeconds(20)) Then Return
                Catch ex As Threading.AbandonedMutexException
                    ' the old one ended without letting go: it is ours now
                End Try
            End If
            Run()
        End Using
    End Sub

    Private Const RestartArg As String = "/restart"

    ''' <summary>「現在重新開啟 iPhoto」 (設定 › 螢幕): starts a new iPhoto that waits for this one, then
    ''' closes this one. Works for iPhoto.exe and for "dotnet iPhoto.dll" (the test copies).</summary>
    Friend Sub RestartApplication()
        Dim exe As String = Environment.ProcessPath
        Dim args As String = RestartArg
        If String.Equals(IO.Path.GetFileNameWithoutExtension(exe), "dotnet", StringComparison.OrdinalIgnoreCase) Then
            args = """" & Reflection.Assembly.GetEntryAssembly().Location & """ " & RestartArg
        End If
        Try
            Process.Start(New ProcessStartInfo(exe, args) With {.UseShellExecute = False, .WorkingDirectory = Application.StartupPath})
        Catch ex As ComponentModel.Win32Exception
            frmMsgBox.ShowCriticalMessage("無法重新開啟 iPhoto，請自己關掉再開一次。" & vbCrLf & ex.Message, "重新開啟 iPhoto")
            Return
        End Try
        Application.OpenForms.Cast(Of Form)().FirstOrDefault(Function(f) TypeOf f Is frmMain)?.Close()
    End Sub

    Private Sub Run()
        Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' 華康細圓體 before any window uses it (installed for the user from the copy next to the exe)
        FontSetup.EnsureFont()
        Application.UseWaitCursor = True

        g_lpFileSystem = New Carbon.FileSystem
        g_lpConfig = New Config
        g_lpConfig.Construct(Application.StartupPath)

        ' VB6 skipped the splash on the developer's machine
        Dim logo As frmLogo = Nothing
        If Not String.Equals(Environment.MachineName, "WS-CYL-DELL", StringComparison.OrdinalIgnoreCase) Then
            logo = New frmLogo()
            logo.Show()
            Application.DoEvents()
        End If

        g_lpMonitors = New CoreMedia.Monitors
        g_intMonitorCount = g_lpMonitors.MonitorCount
        ' 設定 › 螢幕 (one screen: always single; two: 雙螢幕 unless the user chose 單螢幕)
        g_bolDualScreen = g_intMonitorCount > 1 AndAlso Not g_lpConfig.ScreenSingle
        ReDim emScreenRatio(g_intMonitorCount - 1)
        g_bolHaveWidthScreen = False
        g_intMainScreenIndex = 0
        For i As Integer = 0 To g_intMonitorCount - 1
            Dim m As CoreMedia.Monitor = g_lpMonitors.Monitor(i)
            If m.Width / m.Height < 1.5 Then
                emScreenRatio(i) = enumScreenRatio.rtNormal
            Else
                emScreenRatio(i) = enumScreenRatio.rtWidth
                g_bolHaveWidthScreen = True
                g_intMainScreenIndex = i   ' the main window goes to the (last) wide monitor, as in VB6
            End If
        Next

        ' the screen the main window was on last time wins (WindowPlacement, saved on every move)
        Dim savedScreen As Screen = WindowPlacement.SavedScreen(DualScreenSwap.MainKey)
        If savedScreen IsNot Nothing AndAlso WindowPlacement.ScreenIndex(savedScreen) >= 0 Then
            g_intMainScreenIndex = WindowPlacement.ScreenIndex(savedScreen)
        End If

        Dim mainForm As New frmMain()
        ' VB6: objForm.Show (its Form_Load runs, SetFormPosition included), then objForm.Move to fill the
        ' monitor -- this handler runs after the form's own Load handler. The window is sizable now, so
        ' it opens where it was left last time (WindowPlacement), else maximised on that monitor
        ' (working area, task bar visible), restoring to its designed size centred there.
        AddHandler mainForm.Load,
            Sub()
                Dim scr As Screen = Screen.AllScreens(g_intMainScreenIndex)
                Dim saved As Rectangle? = WindowPlacement.SavedBounds(DualScreenSwap.MainKey, scr)
                If saved.HasValue Then
                    mainForm.Bounds = saved.Value
                Else
                    Dim area As Rectangle = scr.WorkingArea
                    mainForm.Location = New Point(area.Left + Math.Max(0, (area.Width - mainForm.Width) \ 2),
                                                  area.Top + Math.Max(0, (area.Height - mainForm.Height) \ 2))
                End If
                If Not saved.HasValue OrElse WindowPlacement.SavedMaximized(DualScreenSwap.MainKey) Then
                    mainForm.WindowState = FormWindowState.Maximized
                End If
                mainForm.StartScreenSwap()
            End Sub

        ' VB6 LibMain.CreateMultiMonters (the photo viewer) runs in the main form's Load: the form keeps
        ' the viewer it drives (see frmMain.CreateMultiMonters).

        AddHandler mainForm.Shown,
            Sub()
                If logo IsNot Nothing Then logo.Close()
                Application.UseWaitCursor = False
                ' the video engine (LibVLC) loads its plug-ins in the background now, so the first
                ' click on a video plays at once instead of waiting ~3 s
                Aqua.MediaViewerControl.PreloadVideoEngine()
            End Sub
        ' dialogs and other windows opened from the main window open on its screen (LibScreens)
        Using New ChildWindowsFollowMain(mainForm)
            Application.Run(mainForm)
        End Using
    End Sub

End Module
