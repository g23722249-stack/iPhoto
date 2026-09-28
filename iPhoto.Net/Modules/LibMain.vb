' Port of iPhoto\Module\LibMain.bas.
'   Sub Main                      -> Program.vb
'   globals (g_lpConfig ...)      -> PhotoLib.Net\Modules\Globals.vb (the shared Lib code uses them)
'   SetStyleOffice10 (vbAccelerator cFlatControl) is not needed with WinForms visual styles.
'   CreateMultiMonters (the photo viewer) -> frmMain_1280x1024.CreateMultiMonters, which keeps the viewer.
Friend Module LibMain

    ''' <summary>Creates the app-wide objects (VB6 called this from the main form's Load):
    ''' settings, the photo tree, the dock (restored from ~Dock.dck), the import list, the monitors
    ''' and the photo index database.</summary>
    Public Sub CreateServerObject(ByVal szPath As String)
        g_lpFileSystem = New Carbon.FileSystem

        g_lpConfig = New Config
        g_lpConfig.Construct(szPath)

        g_lpStorage = New Storage
        g_lpStorage.Construct(g_lpConfig)

        g_lpDock = New Dock
        g_lpDock.Restore()

        g_lpImport = New Import

        If g_lpMonitors Is Nothing Then
            g_lpMonitors = New CoreMedia.Monitors
            g_intMonitorCount = g_lpMonitors.MonitorCount
        End If

        ' a copy of iPhoto.mdb before it is opened (Maintenance: Backup\, the newest 10 kept)
        If Not g_lpConfig.ReadOnly Then Maintenance.BackupDatabase(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase))

        g_lpDatabase = New Database
        g_lpDatabase.Construct(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase))

        ' face recognition: Nothing when it is switched off (設定 › 面孔), the database has no face tables
        ' or the models are missing
        g_lpFaces = If(g_lpConfig.FaceEnabled, FaceLibrary.Create(g_lpDatabase, g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)), Nothing)
        If g_lpFaces IsNot Nothing Then
            g_lpFaces.WriteNames = g_lpConfig.FaceWriteNames
            g_lpFaces.Strictness = g_lpConfig.FaceStrictness
        End If
    End Sub

    Public Sub InitialProgramParameter()
        g_lpDock.Clear()
    End Sub

    Public Sub DestroyServerObject()
        If g_lpFaces IsNot Nothing Then g_lpFaces.Dispose()
        g_lpFaces = Nothing
        If g_lpDatabase IsNot Nothing Then g_lpDatabase.Dispose()
        g_lpDatabase = Nothing
        g_lpMonitors = Nothing
        g_lpImport = Nothing
        g_lpDock = Nothing
        g_lpFileSystem = Nothing
        g_lpConfig = Nothing
        g_lpStorage = Nothing
    End Sub

End Module
