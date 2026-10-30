Imports System.IO
Imports System.Threading.Tasks

' The photo lists' thumbnails kept on disk (Aqua.ThumbnailDiskCache), so an album opened again -- also
' after a restart -- shows them at once instead of decoding every photo anew. Config.ThumbCache (on by
' default) and Config.ThumbCacheMB, in 設定 › 維護. The cache is System\ThumbCache of the library, beside
' Tiles and FaceIndex, so it goes along with D:\生活剪輯 (its entries don't hold the drive letter); when
' iPhoto can't write there (a read-only medium), %LOCALAPPDATA%\iPhoto\ThumbCache.
Friend Module ThumbCache

    ''' <summary>D:\生活剪輯\System\Database\iPhoto.mdb -> D:\生活剪輯\System\ThumbCache.</summary>
    Friend ReadOnly Property Folder As String
        Get
            Dim local As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "ThumbCache")
            If g_lpConfig Is Nothing OrElse g_lpConfig.ReadOnly Then Return local
            Dim mdb As String = g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
            Dim dbFolder As String = If(String.IsNullOrEmpty(mdb), Nothing, Path.GetDirectoryName(mdb))
            Dim parent As String = If(dbFolder Is Nothing, Nothing, Path.GetDirectoryName(dbFolder))
            If Not String.IsNullOrEmpty(parent) AndAlso Directory.Exists(parent) Then Return Path.Combine(parent, "ThumbCache")
            Return local
        End Get
    End Property

    ''' <summary>The setting into effect (at start and after 設定's 儲存); over the limit, the least recently
    ''' used thumbnails go, in the background.</summary>
    Friend Sub Apply()
        Dim enabled As Boolean = g_lpConfig IsNot Nothing AndAlso g_lpConfig.ThumbCache
        Aqua.ThumbnailDiskCache.Folder = If(enabled, Folder, Nothing)
        If Not enabled Then Return
        Dim cache As String = Folder, limit As Long = CLng(g_lpConfig.ThumbCacheMB) * 1024 * 1024
        Task.Run(Sub()
                     Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.BelowNormal
                     Aqua.ThumbnailDiskCache.Trim(cache, limit)
                 End Sub)
    End Sub

    ''' <summary>The bytes the cache holds now, also when it is switched off (what 清除 would free).</summary>
    Friend Function Bytes() As Long
        Return Aqua.ThumbnailDiskCache.Bytes(Folder)
    End Function

    ''' <summary>設定 › 維護「清除縮圖快取」: every thumbnail kept goes.</summary>
    Friend Sub Clear()
        Aqua.ThumbnailDiskCache.Clear(Folder)
    End Sub

    '==================================================================================================
    ' 設定 › 維護「重建縮圖快取」「清除無效快取」: one job at a time on a worker thread, going on after 設定 is
    ' closed (and ended with iPhoto: what was made stays, a later 重建 goes on from there).
    '==================================================================================================
    Private s_job As Task
    Private s_cancel As Threading.CancellationTokenSource
    Private s_status As String = ""
    Private s_result As String = ""

    Friend ReadOnly Property Busy As Boolean
        Get
            Return s_job IsNot Nothing AndAlso Not s_job.IsCompleted
        End Get
    End Property

    ''' <summary>How far the job has come (for 設定 to show).</summary>
    Friend ReadOnly Property Status As String
        Get
            Return s_status
        End Get
    End Property

    ''' <summary>What the last job did, once: "" while one runs, before the first, and after it was taken (設定
    ''' shows it when the job ends while it is open, or the next time it opens).</summary>
    Friend Function TakeResult() As String
        If Busy OrElse s_resultTaken Then Return ""
        s_resultTaken = True
        Return s_result
    End Function
    Private s_resultTaken As Boolean = True

    Friend Sub StopJob()
        s_cancel?.Cancel()
    End Sub

    ''' <summary>清除無效快取: the entries whose photo is gone or changed (and old-format ones). False when a job runs.</summary>
    Friend Function StartRemoveInvalid() As Boolean
        Return Start(Sub(cache, cancel)
                         Dim r = Aqua.ThumbnailDiskCache.RemoveInvalid(cache, Sub(done, total) s_status = $"檢查中 {done:#,0} / {total:#,0}", cancel)
                         s_result = If(cancel.IsCancellationRequested, "已停止。", "") &
                                    $"清除無效快取 {r.Removed:#,0} 個（{r.RemovedBytes / 1024.0 / 1024.0:#,0.0} MB），有效的 {r.Valid:#,0} 個。"
                     End Sub)
    End Function

    ''' <summary>重建縮圖快取: 清除無效 first, then every photo and video of the library (PhotoIndex) gets the
    ''' thumbnails it lacks, at the sizes the valid entries are for and the sizes the photo list shows now
    ''' (<paramref name="shownImage"/> / <paramref name="shownVideo"/>, Size.Empty when it shows nothing).
    ''' False when a job runs or there is nothing to rebuild for.</summary>
    Friend Function StartRebuild(ByVal shownImage As Size, ByVal shownVideo As Size) As Boolean
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return False
        Dim files() As String = Nothing
        g_lpDatabase.SearchFileArrayList("1 = 1", files)
        If files Is Nothing OrElse files.Length = 0 Then Return False
        Dim limitMB As Integer = g_lpConfig.ThumbCacheMB
        Return Start(Sub(cache, cancel)
                         Dim check = Aqua.ThumbnailDiskCache.RemoveInvalid(cache, Sub(done, total) s_status = $"檢查中 {done:#,0} / {total:#,0}", cancel)
                         If shownImage.Width > 0 Then check.ImageBoxes.Add(shownImage)
                         If shownVideo.Width > 0 Then check.VideoBoxes.Add(shownVideo)
                         Dim made As Aqua.ThumbnailDiskCache.FillResult = Nothing
                         If Not cancel.IsCancellationRequested AndAlso check.ImageBoxes.Count + check.VideoBoxes.Count > 0 Then
                             made = Aqua.ThumbnailDiskCache.Fill(cache, files, check.ImageBoxes, check.VideoBoxes,
                                                                 Sub(done, total) s_status = $"重建中 {done:#,0} / {total:#,0}", cancel,
                                                                 Math.Max(1, Math.Min(4, Environment.ProcessorCount \ 2)))
                         End If
                         Dim mb As Double = Aqua.ThumbnailDiskCache.Bytes(cache) / 1024.0 / 1024.0
                         s_result = If(cancel.IsCancellationRequested, "已停止（做好的會留著，下次重建從這裡繼續）。" & vbCrLf, "") &
                                    $"清除無效快取 {check.Removed:#,0} 個。" & vbCrLf &
                                    If(made Is Nothing, "沒有可以依照的縮圖大小：先開一本相簿再重建。",
                                       $"新做 {made.Made:#,0} 張縮圖，原本就有 {made.Present:#,0} 張" &
                                       If(made.NotNeeded > 0, $"，{made.NotNeeded:#,0} 張照片本身的縮圖就夠用（不必存）", "") &
                                       If(made.Failed > 0, $"，{made.Failed:#,0} 張做不出來", "") & "。") & vbCrLf &
                                    $"快取現在 {mb:#,0.0} MB" &
                                    If(mb > limitMB, $"，超過上限 {limitMB:#,0} MB：下次開啟 iPhoto 時會刪掉最久沒用的，可以把上限調大。", "。")
                     End Sub)
    End Function

    Private Function Start(ByVal work As Action(Of String, Threading.CancellationToken)) As Boolean
        If Busy Then Return False
        Dim cache As String = Folder
        s_cancel = New Threading.CancellationTokenSource()
        Dim cancel As Threading.CancellationToken = s_cancel.Token
        s_status = "準備中…"
        s_result = ""
        s_resultTaken = False
        s_job = Task.Run(Sub()
                             Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.BelowNormal
                             Try
                                 work(cache, cancel)
                             Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                                 s_result = "縮圖快取的工作中斷：" & ex.Message
                             End Try
                         End Sub)
        Return True
    End Function

End Module
