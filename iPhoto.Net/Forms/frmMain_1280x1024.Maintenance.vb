' The .Exif backup (PhotoLib Maintenance): every Maintenance.ExifEveryDays days, once the main window is
' up, on a worker thread (it only reads the side files). The database copy is made earlier, in
' LibMain.CreateServerObject, before the database is opened.
Partial Class frmMain_1280x1024

    Private Sub Maintenance_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If g_lpConfig.ReadOnly Then Return
        Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        If String.IsNullOrEmpty(mdb) OrElse Not IO.File.Exists(mdb) OrElse Not Maintenance.ExifBackupDue(mdb) Then Return
        Dim roots As List(Of String) = AlbumRoots()   ' frmMain_1280x1024.Faces.vb
        Threading.Tasks.Task.Run(Sub()
                                     Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.BelowNormal
                                     Maintenance.BackupExif(mdb, roots)
                                 End Sub)
    End Sub

End Class
