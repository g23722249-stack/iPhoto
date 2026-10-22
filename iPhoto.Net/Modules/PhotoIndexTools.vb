' 重建資料庫索引 (設定 and 批次修改資訊): PhotoIndex made again from every photo's .Exif. With
' Config.RebuildFillPlaces (設定 › 一般設定, the default) each .Exif first gets what it lacks from the picture --
' its GPS, and the 地點 named from it when blank (Database.Rebuild fillExif, PlaceNames) -- after a backup of
' the .Exif files (Maintenance). Without it nothing but the index is written: a 地點 or GPS cleared on purpose
' stays cleared.
' When the .Exif files hold the face data too (FaceArchive, after 設定 › 維護 「轉換 .Exif 為新格式」), the
' face tables can be made again from them as well -- after a question: it replaces what the tables have.
Friend Module PhotoIndexTools

    ''' <summary>False when the .Exif backup failed (nothing was changed then). <paramref name="fillPlaces"/>:
    ''' Nothing = Config.RebuildFillPlaces.</summary>
    Public Function RebuildPhotoIndex(Optional ByVal fillPlaces As Boolean? = Nothing) As Boolean
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return False
        Dim fill As Boolean = Not g_lpConfig.ReadOnly AndAlso If(fillPlaces, g_lpConfig.RebuildFillPlaces)
        Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        If fill Then
            Dim zip As String
            frmLoading.StartLoading("備份 .Exif 中…")
            Try
                zip = Maintenance.BackupExif(mdb, roots).Zip
            Finally
                frmLoading.EndLoading()
            End Try
            If zip = "" Then
                frmMsgBox.ShowCriticalMessage(".Exif 備份失敗，這次不重建索引。", "重建資料庫索引")
                Return False
            End If
        End If
        frmLoading.StartLoading(If(fill, "正在重建索引並補上拍攝地點…請耐心等候", "正在重建索引…請耐心等候"))
        Try
            g_lpDatabase.Rebuild(g_lpStorage, fillExif:=fill)
        Finally
            frmLoading.EndLoading()
        End Try
        RestoreFacesIfWanted(mdb, roots)
        Return True
    End Function

    ''' <summary>The face tables made again from the .Exif files, when they have face data and the user wants it.</summary>
    Private Sub RestoreFacesIfWanted(ByVal mdb As String, ByVal roots As List(Of String))
        If g_lpConfig.ReadOnly OrElse Not g_lpDatabase.FaceTablesReady OrElse Not FaceArchive.LastExport(mdb).HasValue Then Return
        If FaceArchive.ArchivedPhotos(roots) = 0 Then Return
        Dim counts = g_lpDatabase.FaceCounts()
        If Not frmQueryMsgBox.ShowMessage("也要從 .Exif 還原面孔資料嗎？" & vbCrLf &
                                          $"資料庫裡現在的面孔資料（{counts.Photos:#,0} 張照片、{counts.Faces:#,0} 張臉、{counts.Persons:#,0} 個人）會換成 .Exif 裡的。" & vbCrLf &
                                          "只有資料庫遺失、損壞或從舊的備份還原時才需要；資料庫正常時選「否」。", "重建資料庫索引") Then Return
        g_lpFaces?.Stop()
        Dim r As (Photos As Integer, Faces As Integer, Persons As Integer)
        frmLoading.StartLoading("正在從 .Exif 還原面孔資料…請耐心等候")
        Try
            FaceArchive.Export(mdb)   ' what the database has that the .Exif files don't have yet
            r = FaceArchive.Restore(mdb, roots)
        Finally
            frmLoading.EndLoading()
        End Try
        g_lpFaces?.Reload()
        frmMsgBox.ShowExclamationMessage($"已從 .Exif 還原：{r.Photos:#,0} 張照片、{r.Faces:#,0} 張臉、{r.Persons:#,0} 個人", "重建資料庫索引")
    End Sub

End Module
