' Background upkeep (PhotoLib), once the main window is up, on worker threads (each with its own
' database connection):
'   the .Exif backup        every Maintenance.ExifEveryDays days (it only reads the side files)
'   the face data           written into the changed photos' .Exif and Faces.xml (FaceArchive.Export): at
'                           start, every FaceExportMinutes and on close -- nothing before 設定 › 維護
'                           「轉換 .Exif 為新格式」 has been run once
'   the index               PhotoIndex rows that differ from their .Exif written again (IndexSync), at
'                           most once a day
' The database copy is made earlier, in LibMain.CreateServerObject, before the database is opened.
Partial Class frmMain

    Private Const FaceExportMinutes As Integer = 10
    Private WithEvents m_faceExportTimer As Timer

    Private Sub Maintenance_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If g_lpConfig.ReadOnly Then Return
        Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        If String.IsNullOrEmpty(mdb) OrElse Not IO.File.Exists(mdb) Then Return
        Dim roots As List(Of String) = AlbumRoots()   ' frmMain.Faces.vb
        Threading.Tasks.Task.Run(Sub()
                                     Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.BelowNormal
                                     If Maintenance.ExifBackupDue(mdb) Then Maintenance.BackupExif(mdb, roots)
                                     FaceArchive.Export(mdb)
                                     If IndexSync.Due(mdb) Then IndexSync.Run(mdb, roots)
                                 End Sub)
        m_faceExportTimer = New Timer With {.Interval = FaceExportMinutes * 60 * 1000}
        m_faceExportTimer.Start()
    End Sub

    Private Sub m_faceExportTimer_Tick(sender As Object, e As EventArgs) Handles m_faceExportTimer.Tick
        Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        Threading.Tasks.Task.Run(Sub()
                                     Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.BelowNormal
                                     FaceArchive.Export(mdb)
                                 End Sub)
    End Sub

    ''' <summary>The face data changed since the last export, before iPhoto goes (usually a few photos).</summary>
    Private Sub Maintenance_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_faceExportTimer?.Stop()
        If g_lpConfig.ReadOnly Then Return
        Try
            FaceArchive.Export(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase))
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is Data.Common.DbException
            ' the next start exports it
        End Try
    End Sub

End Class
