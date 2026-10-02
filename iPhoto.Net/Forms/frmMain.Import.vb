' 輸入 with the new import window (frmImportStudio, new in the .NET port). It copies, writes the .Exif
' files, PhotoIndex and the faces itself; here the album imported last becomes the recent one (so the
' refresh after the tool bar opens it, as after frmImport) and the background face scan is asked to
' look at the new photos (the ones analysed in the window are skipped: their FacePhoto rows fit).
Partial Class frmMain

    Private Sub ImportWithStudio()
        Dim jobs As List(Of ImportJob)
        Using f As New frmImportStudio
            f.ShowDialog(Me)
            jobs = f.ImportedJobs
        End Using
        If jobs.Count = 0 Then Return
        Dim last As ImportJob = jobs(jobs.Count - 1)
        LibUserInterface.SaveRecentSectionValue(enumExeMode.exeAlbums, last.AlbumPath)
        LibUserInterface.SaveRecentKeyValue(enumExeMode.exeAlbums, last.ClassPath)
        FaceScanAfterImport()   ' frmMain.Faces.vb
    End Sub

End Class
