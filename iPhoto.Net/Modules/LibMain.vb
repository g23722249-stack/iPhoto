' Port of iPhoto\Module\LibMain.bas.
'   Sub Main                      -> Program.vb
'   globals (g_lpConfig ...)      -> PhotoLib.Net\Modules\Globals.vb (the shared Lib code uses them)
'   SetStyleOffice10 (vbAccelerator cFlatControl) is not needed with WinForms visual styles.
'   CreateMultiMonters (the photo viewer) -> frmMain.CreateMultiMonters, which keeps the viewer.
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

        ' iPhoto.mdb becomes iPhoto.db (SQLite) once; then a copy of the database before it is opened
        ' (Maintenance: Backup\, the newest 10 kept)
        If Not g_lpConfig.ReadOnly Then
            UpgradeDatabase()
            Maintenance.BackupDatabase(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase))
        End If

        g_lpDatabase = New Database
        g_lpDatabase.Construct(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase))
        ' name cards made outside the library are copied into it once (Database.UseFaceCoverFolder)
        g_lpDatabase.UseFaceCoverFolder(g_lpConfig.Attached(Config.enumAttachedFile.filFaceCover))
        ThumbCache.Apply()   ' the photo lists' thumbnails kept on disk (設定 › 維護)

        ' face recognition: Nothing when it is switched off (設定 › 面孔), the database has no face tables
        ' or the models are missing
        g_lpFaces = If(g_lpConfig.FaceEnabled, FaceLibrary.Create(g_lpDatabase, g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)), Nothing)
        If g_lpFaces IsNot Nothing Then
            g_lpFaces.WriteNames = g_lpConfig.FaceWriteNames
            g_lpFaces.Strictness = g_lpConfig.FaceStrictness
        End If
    End Sub

    ''' <summary>The first start of a version with SQLite: iPhoto.mdb is copied into iPhoto.db (DatabaseUpgrade,
    ''' a few seconds), the .mdb kept in Backup\, and the settings point at iPhoto.db from then on. When it
    ''' can't be done iPhoto goes on with the .mdb this time and tries again at the next start.</summary>
    Private Sub UpgradeDatabase()
        Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        If String.IsNullOrEmpty(mdb) OrElse Database.IsSqliteFile(mdb) Then Return
        Dim r As DatabaseUpgrade.Result
        Dim converting As Boolean = IO.File.Exists(mdb)   ' else only to find an iPhoto.db made by another iPhoto
        If converting Then frmLoading.StartLoading("正在把資料庫轉換為新格式（只有第一次）…")
        Try
            r = DatabaseUpgrade.ToSqlite(mdb)
        Finally
            If converting Then frmLoading.EndLoading()
        End Try
        If Not String.Equals(r.Database, mdb, StringComparison.OrdinalIgnoreCase) Then
            g_lpConfig.Attached(Config.enumAttachedFile.filDatabase) = r.Database
            g_lpConfig.Save()
        End If
        If r.Error <> "" Then
            MessageBox.Show("資料庫轉換為新格式沒有成功，這次繼續使用 " & IO.Path.GetFileName(mdb) & "，下次開啟 iPhoto 會再試一次。" & vbCrLf & vbCrLf & r.Error,
                            "iPhoto", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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
