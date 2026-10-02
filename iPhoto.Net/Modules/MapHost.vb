Imports System.IO
Imports System.Net.Http
Imports Microsoft.Web.WebView2.Core

' The map pages (Map\map.html for 地點, Map\map_pick.html for 輸入地點) as https://iphoto-map.local/<page>.
' WebView2 asks this module for every request to that host:
'   <file>             Map\<file> from the exe's embedded resources: the pages, Leaflet, markercluster,
'                      topojson-client, the outline map script and the county / town borders (Map\lib).
'                      Nothing comes from a CDN any more, so the page opens without the internet.
'   tiles/{z}/{x}/{y}.png
'                      an OpenStreetMap tile through a cache (TileFolder: System\Tiles of the library, so it
'                      goes along to another computer with D:\生活剪輯), as OSM's tile usage policy asks
'                      (https://operations.osmfoundation.org/policies/tiles/): a tile is kept as long as the
'                      server's Cache-Control / Expires say (7 days when they don't), then asked again with
'                      its ETag (304: kept longer). The policy forbids offline use of its tiles, so the
'                      cache only saves asking again while online: an expired tile that can't be fetched is
'                      not shown, and 設定 › 地點 地圖「離線」(Config.MapOffline) shows no OSM tiles at all.
'                      A tile not given is a 404: the outline map under the tiles (lib/outline.js) shows
'                      there. Only tiles the user is looking at are asked for (no prefetching -- the policy
'                      forbids bulk downloading). TrimCache keeps the cache under Config.MapCacheMB.
' (The pages were written to %LOCALAPPDATA%\iPhoto\Map and served by SetVirtualHostNameToFolderMapping, but
' newer WebView2 runtimes read such a folder from a sandboxed process that may not reach it: ERR_ACCESS_DENIED.)
' The address stays an https one, so the tile requests carry a proper referrer.
Friend Module MapHost

    Friend Const HostName As String = "iphoto-map.local"
    Private Const TileServer As String = "https://tile.openstreetmap.org/"
    Private Const DefaultKeepDays As Integer = 7   ' OSM's policy: at least 7 days when the headers don't say

    ' OpenStreetMap's tile policy: a clear, unique User-Agent; few connections (2 here)
    Private ReadOnly s_http As New HttpClient With {.Timeout = TimeSpan.FromSeconds(15)}
    Private ReadOnly s_gate As New Threading.SemaphoreSlim(2)
    Private ReadOnly s_files As New Dictionary(Of String, Byte())(StringComparer.OrdinalIgnoreCase)

    Sub New()
        s_http.DefaultRequestHeaders.UserAgent.ParseAdd("iPhoto.Net/1.0 (Windows photo album; map of the user's own photos)")
        s_http.DefaultRequestHeaders.Referrer = New Uri("https://" & HostName & "/")
    End Sub

    ''' <summary>With the library, so it goes along to another computer: Tiles beside the database's folder
    ''' (D:\生活剪輯\System\Database\iPhoto.mdb -> D:\生活剪輯\System\Tiles). No database set:
    ''' %LOCALAPPDATA%\iPhoto\Tiles.</summary>
    Friend ReadOnly Property TileFolder As String
        Get
            Dim mdb As String = If(g_lpConfig Is Nothing, "", g_lpConfig.Attached(Config.enumAttachedFile.filDatabase))
            Dim dbFolder As String = If(String.IsNullOrEmpty(mdb), Nothing, Path.GetDirectoryName(mdb))
            Dim parent As String = If(dbFolder Is Nothing, Nothing, Path.GetDirectoryName(dbFolder))
            If Not String.IsNullOrEmpty(parent) AndAlso Directory.Exists(parent) Then Return Path.Combine(parent, "Tiles")
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "Tiles")
        End Get
    End Property

    ''' <summary>Serves Map\* and the tiles, and navigates to <paramref name="page"/> (e.g. "map.html").</summary>
    Friend Sub ServeAndNavigate(ByVal core As CoreWebView2, ByVal page As String)
        Dim offline As Boolean = g_lpConfig IsNot Nothing AndAlso g_lpConfig.MapOffline
        core.AddWebResourceRequestedFilter("https://" & HostName & "/*", CoreWebView2WebResourceContext.All)
        AddHandler core.WebResourceRequested,
            Async Sub(sender As Object, e As CoreWebView2WebResourceRequestedEventArgs)
                Dim uri As Uri = Nothing
                If Not Uri.TryCreate(e.Request.Uri, UriKind.Absolute, uri) OrElse
                   Not String.Equals(uri.Host, HostName, StringComparison.OrdinalIgnoreCase) Then Return
                Dim rel As String = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart("/"c)
                If String.Equals(rel, OfflineMapPath, StringComparison.OrdinalIgnoreCase) Then
                    ' the offline map file, by byte ranges (the vector map reads only the parts it shows)
                    Dim range As String = If(e.Request.Headers.Contains("Range"), e.Request.Headers.GetHeader("Range"), "")
                    Dim deferral0 As CoreWebView2Deferral = e.GetDeferral()
                    Dim part As (Bytes As Byte(), Start As Long, Total As Long) = (Nothing, 0, 0)
                    Try
                        part = Await Task.Run(Function() ReadRange(OfflineMapFile, range))
                    Catch
                    End Try
                    Try
                        If part.Bytes Is Nothing Then
                            e.Response = NotFound(core)
                        Else
                            Dim last As Long = part.Start + part.Bytes.Length - 1
                            e.Response = core.Environment.CreateWebResourceResponse(New MemoryStream(part.Bytes), 206, "Partial Content",
                                "Content-Type: application/octet-stream" & vbCrLf & "Accept-Ranges: bytes" & vbCrLf &
                                $"Content-Range: bytes {part.Start}-{last}/{part.Total}" & vbCrLf & "Content-Length: " & part.Bytes.Length)
                        End If
                    Catch
                    End Try
                    Try
                        deferral0.Complete()
                    Catch
                    End Try
                    Return
                End If
                Dim tile() As Integer = TileOf(rel)
                If tile Is Nothing Then
                    Dim bytes() As Byte = FileBytes(rel)
                    e.Response = If(bytes Is Nothing, NotFound(core),
                                    core.Environment.CreateWebResourceResponse(New MemoryStream(bytes), 200, "OK", "Content-Type: " & ContentType(rel)))
                    Return
                End If
                If offline Then   ' no OSM tiles offline (the policy): the outline map alone
                    e.Response = NotFound(core)
                    Return
                End If
                ' a tile takes a while (the disk, the internet): answered later. Leaflet drops the tiles it no
                ' longer needs (zooming, panning) and WebView2 cancels those requests -- answering one then
                ' fails (E_ILLEGAL_METHOD_CALL), which is fine: nobody waits for it any more.
                Dim deferral As CoreWebView2Deferral = e.GetDeferral()
                Dim png() As Byte = Nothing
                Try
                    png = Await Task.Run(Function() GetTileAsync(tile(0), tile(1), tile(2)))
                Catch
                End Try
                Try
                    e.Response = If(png Is Nothing, NotFound(core),
                                    core.Environment.CreateWebResourceResponse(New MemoryStream(png), 200, "OK", "Content-Type: image/png"))
                Catch
                End Try
                Try
                    deferral.Complete()
                Catch
                End Try
            End Sub
        Dim query As New List(Of String)
        If offline Then query.Add("mode=offline")
        If IO.File.Exists(OfflineMapFile) Then query.Add("offlinemap=" & OfflineMapPath)   ' 離線地圖檔 downloaded
        core.Navigate("https://" & HostName & "/" & page & If(query.Count > 0, "?" & String.Join("&", query), ""))
    End Sub

    '==================================================================================================
    ' 離線地圖檔 (設定 › 地點「下載／更新離線地圖」, Modules\OfflineMap.vb): Taiwan from Protomaps' build of
    ' OpenStreetMap data (PMTiles), drawn by protomaps-leaflet when the map is offline
    '==================================================================================================
    Friend Const OfflineMapPath As String = "offline/taiwan.pmtiles"

    ''' <summary>Maps beside the database's folder (D:\生活剪輯\System\Maps), as the tile cache.</summary>
    Friend ReadOnly Property MapsFolder As String
        Get
            Return Path.Combine(Path.GetDirectoryName(TileFolder), "Maps")
        End Get
    End Property

    Friend ReadOnly Property OfflineMapFile As String
        Get
            Return Path.Combine(MapsFolder, "taiwan.pmtiles")
        End Get
    End Property

    ''' <summary>The bytes of an HTTP Range ("bytes=a-b", "bytes=a-", "bytes=-n"; none: the first 16 KB).</summary>
    Private Function ReadRange(ByVal file As String, ByVal range As String) As (Bytes As Byte(), Start As Long, Total As Long)
        If Not IO.File.Exists(file) Then Return (Nothing, 0, 0)
        Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim total As Long = fs.Length
            Dim start As Long = 0, last As Long = Math.Min(total, 16384) - 1
            Dim m = Text.RegularExpressions.Regex.Match(If(range, ""), "bytes=(\d*)-(\d*)")
            If m.Success Then
                If m.Groups(1).Value = "" AndAlso m.Groups(2).Value <> "" Then
                    start = Math.Max(0, total - CLng(m.Groups(2).Value)) : last = total - 1
                Else
                    start = If(m.Groups(1).Value = "", 0, CLng(m.Groups(1).Value))
                    last = If(m.Groups(2).Value = "", total - 1, Math.Min(total - 1, CLng(m.Groups(2).Value)))
                End If
            End If
            If start >= total OrElse last < start Then Return (Nothing, 0, total)
            Dim buf(CInt(last - start)) As Byte
            fs.Position = start
            Dim read As Integer = 0
            While read < buf.Length
                Dim n As Integer = fs.Read(buf, read, buf.Length - read)
                If n <= 0 Then Exit While
                read += n
            End While
            Return (buf, start, total)
        End Using
    End Function

    ''' <summary>A script error of a map page ({type:'jserror'}): to %TEMP%\iPhoto_map.log (DevTools are off).</summary>
    Friend Sub LogPageError(ByVal message As String)
        Try
            IO.File.AppendAllText(Path.Combine(Path.GetTempPath(), "iPhoto_map.log"), Date.Now.ToString("s") & " " & message & vbCrLf)
        Catch ex As IOException
        Catch ex As UnauthorizedAccessException
        End Try
    End Sub

    Private Function NotFound(ByVal core As CoreWebView2) As CoreWebView2WebResourceResponse
        Return core.Environment.CreateWebResourceResponse(Nothing, 404, "Not Found", "")
    End Function

    ''' <summary>"tiles/12/3425/1755.png" -> {12, 3425, 1755}; Nothing for anything else.</summary>
    Private Function TileOf(ByVal rel As String) As Integer()
        Dim p() As String = rel.Split("/"c)
        If p.Length <> 4 OrElse p(0) <> "tiles" OrElse Not p(3).EndsWith(".png", StringComparison.OrdinalIgnoreCase) Then Return Nothing
        Dim z, x, y As Integer
        If Not Integer.TryParse(p(1), z) OrElse Not Integer.TryParse(p(2), x) OrElse Not Integer.TryParse(p(3).Substring(0, p(3).Length - 4), y) Then Return Nothing
        If z < 0 OrElse z > 19 Then Return Nothing
        Dim n As Integer = 1 << z
        If x < 0 OrElse x >= n OrElse y < 0 OrElse y >= n Then Return Nothing
        Return {z, x, y}
    End Function

    ''' <summary>Map\<paramref name="rel"/> from the resources (kept after the first time); Nothing when not there.</summary>
    Private Function FileBytes(ByVal rel As String) As Byte()
        If rel = "" OrElse rel.Contains("..") Then Return Nothing
        SyncLock s_files
            Dim cached() As Byte = Nothing
            If s_files.TryGetValue(rel, cached) Then Return cached
            Using s As Stream = GetType(MapHost).Assembly.GetManifestResourceStream("iPhoto.Map\" & rel.Replace("/"c, "\"c))
                If s Is Nothing Then Return Nothing
                Using m As New MemoryStream
                    s.CopyTo(m)
                    s_files(rel) = m.ToArray()
                    Return s_files(rel)
                End Using
            End Using
        End SyncLock
    End Function

    Private Function ContentType(ByVal rel As String) As String
        Select Case Path.GetExtension(rel).ToLowerInvariant()
            Case ".html" : Return "text/html; charset=utf-8"
            Case ".js" : Return "text/javascript; charset=utf-8"
            Case ".css" : Return "text/css; charset=utf-8"
            Case ".json" : Return "application/json; charset=utf-8"
            Case ".png" : Return "image/png"
            Case Else : Return "application/octet-stream"
        End Select
    End Function

    ''' <summary>The tile from the cache while the server lets it be kept, else from OpenStreetMap (asked
    ''' with the cached copy's ETag: 304 keeps it for longer). Nothing when it can't be had -- an expired
    ''' copy is not shown (OSM's policy: no offline use of its tiles).
    ''' Beside each y.png a y.png.meta: the time it may be kept until (UTC ticks) and its ETag.</summary>
    Private Async Function GetTileAsync(ByVal z As Integer, ByVal x As Integer, ByVal y As Integer) As Task(Of Byte())
        Dim file As String = Path.Combine(TileFolder, z.ToString(), x.ToString(), y & ".png")
        Dim meta As String = file & ".meta"
        Dim cached As Byte() = Nothing, etag As String = Nothing
        If IO.File.Exists(file) Then
            Try
                cached = IO.File.ReadAllBytes(file)
                IO.File.SetLastAccessTimeUtc(file, Date.UtcNow)   ' TrimCache drops the least recently used first
                Dim keepUntil As Date = Date.MinValue
                If IO.File.Exists(meta) Then
                    Dim lines() As String = IO.File.ReadAllLines(meta)
                    Dim ticks As Long
                    If lines.Length > 0 AndAlso Long.TryParse(lines(0), ticks) Then keepUntil = New Date(ticks, DateTimeKind.Utc)
                    If lines.Length > 1 AndAlso lines(1) <> "" Then etag = lines(1)
                Else
                    keepUntil = IO.File.GetLastWriteTimeUtc(file).AddDays(DefaultKeepDays)
                End If
                If Date.UtcNow < keepUntil Then Return cached
            Catch ex As IOException
            Catch ex As UnauthorizedAccessException
            End Try
        End If
        Await s_gate.WaitAsync()
        Try
            Using request As New HttpRequestMessage(HttpMethod.Get, $"{TileServer}{z}/{x}/{y}.png")
                If cached IsNot Nothing AndAlso etag IsNot Nothing Then request.Headers.TryAddWithoutValidation("If-None-Match", etag)
                Using response As HttpResponseMessage = Await s_http.SendAsync(request)
                    If response.StatusCode = Net.HttpStatusCode.NotModified AndAlso cached IsNot Nothing Then
                        SaveMeta(meta, KeepUntil(response), If(response.Headers.ETag?.ToString(), etag))
                        Return cached
                    End If
                    If Not response.IsSuccessStatusCode Then Return Nothing
                    Dim png() As Byte = Await response.Content.ReadAsByteArrayAsync()
                    Try
                        Directory.CreateDirectory(Path.GetDirectoryName(file))
                        Dim temp As String = file & "." & Guid.NewGuid().ToString("N") & ".tmp"
                        IO.File.WriteAllBytes(temp, png)
                        IO.File.Move(temp, file, overwrite:=True)
                        SaveMeta(meta, KeepUntil(response), response.Headers.ETag?.ToString())
                    Catch ex As IOException
                    Catch ex As UnauthorizedAccessException
                    End Try
                    Return png
                End Using
            End Using
        Catch ex As HttpRequestException
            Return Nothing   ' no internet: the outline map shows (not the expired copy)
        Catch ex As TaskCanceledException
            Return Nothing
        Finally
            s_gate.Release()
        End Try
    End Function

    ''' <summary>Until when the server lets the tile be kept: Cache-Control max-age (less Age), else
    ''' Expires, else 7 days.</summary>
    Private Function KeepUntil(ByVal response As HttpResponseMessage) As Date
        Dim cc = response.Headers.CacheControl
        If cc IsNot Nothing AndAlso cc.MaxAge.HasValue Then
            Dim age As TimeSpan = If(response.Headers.Age, TimeSpan.Zero)
            Return Date.UtcNow + cc.MaxAge.Value - age
        End If
        If response.Content.Headers.Expires.HasValue Then Return response.Content.Headers.Expires.Value.UtcDateTime
        Return Date.UtcNow.AddDays(DefaultKeepDays)
    End Function

    Private Sub SaveMeta(ByVal meta As String, ByVal keepUntil As Date, ByVal etag As String)
        Try
            IO.File.WriteAllLines(meta, {keepUntil.Ticks.ToString(), If(etag, "")})
        Catch ex As IOException
        Catch ex As UnauthorizedAccessException
        End Try
    End Sub

    ''' <summary>The bytes the tile cache holds now.</summary>
    Friend Function CacheBytes() As Long
        Dim d As New DirectoryInfo(TileFolder)
        If Not d.Exists Then Return 0
        Try
            Return d.EnumerateFiles("*.png", SearchOption.AllDirectories).Sum(Function(f) f.Length)
        Catch ex As IOException
            Return 0
        Catch ex As UnauthorizedAccessException
            Return 0
        End Try
    End Function

    ''' <summary>Over Config.MapCacheMB: the least recently used tiles go until it is at 80% of it.
    ''' Runs in the background (the 地點 window closing).</summary>
    Friend Sub TrimCache()
        If g_lpConfig Is Nothing Then Return
        Dim limit As Long = CLng(g_lpConfig.MapCacheMB) * 1024 * 1024
        Task.Run(Sub()
                     Try
                         Dim d As New DirectoryInfo(TileFolder)
                         If Not d.Exists Then Return
                         Dim files = d.EnumerateFiles("*.png", SearchOption.AllDirectories).ToList()
                         Dim total As Long = files.Sum(Function(f) f.Length)
                         If total <= limit Then Return
                         For Each f In files.OrderBy(Function(x) x.LastAccessTimeUtc)
                             If total <= limit * 8 \ 10 Then Exit For
                             Try
                                 Dim len As Long = f.Length
                                 f.Delete()
                                 IO.File.Delete(f.FullName & ".meta")
                                 total -= len
                             Catch ex As IOException
                             Catch ex As UnauthorizedAccessException
                             End Try
                         Next
                     Catch ex As IOException
                     Catch ex As UnauthorizedAccessException
                     End Try
                 End Sub)
    End Sub

    ''' <summary>設定 › 地點「清除地圖快取」: every tile kept goes.</summary>
    Friend Sub ClearCache()
        Try
            If Directory.Exists(TileFolder) Then Directory.Delete(TileFolder, recursive:=True)
        Catch ex As IOException
        Catch ex As UnauthorizedAccessException
        End Try
    End Sub

End Module
