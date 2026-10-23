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

End Module
