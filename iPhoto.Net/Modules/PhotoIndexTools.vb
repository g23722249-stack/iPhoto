' 重建資料庫索引 (設定 and 批次修改資訊): PhotoIndex made again from every photo's .Exif. With
' Config.RebuildFillPlaces (設定 › 一般設定, the default) each .Exif first gets what it lacks from the picture --
' its GPS, and the 地點 named from it when blank (Database.Rebuild fillExif, PlaceNames) -- after a backup of
' the .Exif files (Maintenance). Without it nothing but the index is written: a 地點 or GPS cleared on purpose
' stays cleared.
Friend Module PhotoIndexTools

    ''' <summary>False when the .Exif backup failed (nothing was changed then). <paramref name="fillPlaces"/>:
    ''' Nothing = Config.RebuildFillPlaces.</summary>
    Public Function RebuildPhotoIndex(Optional ByVal fillPlaces As Boolean? = Nothing) As Boolean
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return False
        Dim fill As Boolean = Not g_lpConfig.ReadOnly AndAlso If(fillPlaces, g_lpConfig.RebuildFillPlaces)
        If fill Then
            Dim roots As New List(Of String)
            For i = 0 To g_lpConfig.AlbumCount - 1
                roots.Add(g_lpConfig.AlbumPath(i))
            Next
            Dim zip As String
            frmLoading.StartLoading("備份 .Exif 中…")
            Try
                zip = Maintenance.BackupExif(g_lpConfig.Attached(Config.enumAttachedFile.filDatabase), roots).Zip
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
        Return True
    End Function

End Module
