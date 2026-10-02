Imports System.IO
Imports System.IO.Compression
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading

' 離線地圖檔 (設定 › 地點「下載／更新離線地圖」): Taiwan cut out of Protomaps' daily build of OpenStreetMap
' data (https://build.protomaps.com/YYYYMMDD.pmtiles, ~140 GB for the planet, ODbL: © OpenStreetMap
' contributors) into MapHost.OfflineMapFile (System\Maps\taiwan.pmtiles beside the database). This is what
' `pmtiles extract --bbox --maxzoom` does, done here so iPhoto needs no extra program: the build's
' directories are read with HTTP range requests, only the tiles of Taiwan's box up to zoom 14 are fetched
' (~130 MB), and a new PMTiles v3 archive is written. Drawn offline by protomaps-leaflet (lib/outline.js).
' PMTiles v3: https://github.com/protomaps/PMTiles/blob/main/spec/v3/spec.md
Friend Module OfflineMap

    Private Const BuildsUrl As String = "https://build-metadata.protomaps.dev/builds.json"
    Private Const BuildBase As String = "https://build.protomaps.com/"
    Friend Const MaxZoom As Integer = 14
    Private Const Parallel As Integer = 8        ' ranges at a time
    Private Const StallSeconds As Integer = 20   ' a range with no bytes this long is asked again
    ' Taiwan with Penghu, Kinmen, Matsu, Green and Orchid Islands, and some sea around them
    Private Const MinLon As Double = 117.8, MinLat As Double = 21.6, MaxLon As Double = 122.6, MaxLat As Double = 26.6

    ''' <summary>What the file is: the build it was cut from (yyyyMMdd) and when it was downloaded.</summary>
    Friend Class Info
        Public Build As String = ""
        Public Downloaded As Date
        Public Bytes As Long
    End Class

    ''' <summary>The file's info (from taiwan.pmtiles.info beside it); Nothing when there is no file.</summary>
    Friend Function Current() As Info
        Dim file As String = MapHost.OfflineMapFile
        If Not IO.File.Exists(file) Then Return Nothing
        Dim i As New Info With {.Bytes = New FileInfo(file).Length, .Downloaded = IO.File.GetLastWriteTime(file)}
        Try
            Dim lines() As String = IO.File.ReadAllLines(file & ".info")
            If lines.Length > 0 Then i.Build = lines(0).Trim()
        Catch ex As IOException
        Catch ex As UnauthorizedAccessException
        End Try
        Return i
    End Function

    ''' <summary>設定 › 地點「刪除」.</summary>
    Friend Sub Delete()
        For Each f In {MapHost.OfflineMapFile, MapHost.OfflineMapFile & ".info"}
            Try
                If IO.File.Exists(f) Then IO.File.Delete(f)
            Catch ex As IOException
            Catch ex As UnauthorizedAccessException
            End Try
        Next
    End Sub

    ''' <summary>Downloads the newest build's Taiwan into the offline map file (replacing it at the end).
    ''' <paramref name="progress"/>: a line of text for the settings page. Returns the new file's info.</summary>
    Friend Async Function DownloadAsync(ByVal progress As Action(Of String), ByVal cancel As CancellationToken) As Task(Of Info)
        Using http As New HttpClient(New SocketsHttpHandler With {.MaxConnectionsPerServer = Parallel + 1}) With {.Timeout = TimeSpan.FromMinutes(5)}
            http.DefaultRequestHeaders.UserAgent.ParseAdd("iPhoto.Net/1.0 (Windows photo album; offline map of Taiwan)")

            progress("找最新的地圖資料…")
            Dim build As String = Await LatestBuildAsync(http, cancel)
            Dim url As String = BuildBase & build & ".pmtiles"

            ' the header and the root directory (both in the first 16 KB)
            progress("讀取地圖目錄…")
            Dim first() As Byte = Await RangeAsync(http, url, 0, 16384, cancel)
            Dim h As Header = Header.Parse(first)
            If h.InternalCompression <> 2 Then Throw New InvalidDataException("地圖資料的目錄格式不支援（不是 gzip）")
            Dim root As List(Of Entry) = ReadDirectory(Slice(first, h.RootOffset, h.RootLength))
            Dim metadata() As Byte = Await RangeAsync(http, url, h.MetadataOffset, h.MetadataLength, cancel)

            ' the tile ids wanted: every tile of the box from zoom 0 to MaxZoom
            Dim wanted As New List(Of ULong)
            For z = 0 To Math.Min(MaxZoom, h.MaxZoom)
                Dim x0 As Integer = LonToX(MinLon, z), x1 As Integer = LonToX(MaxLon, z)
                Dim y0 As Integer = LatToY(MaxLat, z), y1 As Integer = LatToY(MinLat, z)
                For x = x0 To x1
                    For y = y0 To y1
                        wanted.Add(ZxyToTileId(z, x, y))
                    Next
                Next
            Next
            wanted.Sort()

            ' the tile entries for them, through the leaf directories that cover them
            Dim found As New List(Of Entry)
            Await CollectAsync(http, url, h, root, wanted, found, 0, progress, cancel)
            found.Sort(Function(a, b) a.TileId.CompareTo(b.TileId))

            ' the tile data: each distinct piece once, fetched in a few big ranges (pieces lie close together)
            Dim pieces = found.Select(Function(e) (e.Offset, e.Length)).Distinct().OrderBy(Function(p) p.Offset).ToList()
            Dim total As Long = pieces.Sum(Function(p) CLng(p.Length))
            Dim folder As String = MapHost.MapsFolder
            Directory.CreateDirectory(folder)
            Dim dataFile As String = Path.Combine(folder, "taiwan.download.data")
            Dim newOffset As New Dictionary(Of ULong, ULong)
            Dim written As Long = 0
            ' the ranges: runs of pieces with gaps under 256 KB, up to 2 MB each
            Dim runs As New List(Of (First As Integer, Last As Integer, Start As ULong, [End] As ULong))
            Dim i0 As Integer = 0
            While i0 < pieces.Count
                Dim j As Integer = i0
                Dim runStart As ULong = pieces(i0).Offset, runEnd As ULong = pieces(i0).Offset + CULng(pieces(i0).Length)
                While j + 1 < pieces.Count AndAlso pieces(j + 1).Offset <= runEnd + 262144UL AndAlso pieces(j + 1).Offset + CULng(pieces(j + 1).Length) - runStart <= 2097152UL
                    j += 1
                    runEnd = Math.Max(runEnd, pieces(j).Offset + CULng(pieces(j).Length))
                End While
                runs.Add((i0, j, runStart, runEnd))
                i0 = j + 1
            End While
            Try
                Using data As New FileStream(dataFile, FileMode.Create, FileAccess.ReadWrite)
                    ' Up to Parallel ranges at once, a new one starting as soon as one is in (some parts of the
                    ' build come slowly -- the CDN's cache misses -- and must not hold the rest up); written to
                    ' the file in order, each as soon as it and the ones before it are in.
                    Dim gate As New SemaphoreSlim(Parallel)
                    Dim got As Long = 0
                    Dim fetch = Async Function(r As (First As Integer, Last As Integer, Start As ULong, [End] As ULong)) As Task(Of Byte())
                                    Await gate.WaitAsync(cancel)
                                    Try
                                        Dim bytes() As Byte = Await RangeAsync(http, url, h.TileDataOffset + r.Start, CLng(r.End - r.Start), cancel)
                                        Dim now As Long = Interlocked.Add(got, bytes.Length)
                                        progress($"下載地圖資料… {Math.Min(100, now * 100 \ Math.Max(1, total))}%（{now / 1048576.0:0} / {total / 1048576.0:0} MB）")
                                        Return bytes
                                    Finally
                                        gate.Release()
                                    End Try
                                End Function
                    Dim tasks = runs.Select(Function(r) fetch(r)).ToList()
                    Try
                        For n = 0 To runs.Count - 1
                            Dim r = runs(n)
                            Dim chunk() As Byte = Await tasks(n)
                            tasks(n) = Nothing   ' (its bytes can go once written)
                            For k = r.First To r.Last
                                Dim p = pieces(k)
                                newOffset(p.Offset) = CULng(data.Position)
                                data.Write(chunk, CInt(p.Offset - r.Start), p.Length)
                                written += p.Length
                            Next
                        Next
                    Catch
                        ' one failed for good: let the others end before the work files go
                        Try
                            Task.WaitAll(tasks.Where(Function(t) t IsNot Nothing).Cast(Of Task)().ToArray(), 3000)
                        Catch
                        End Try
                        Throw
                    End Try

                ' the new archive: header, root directory, metadata, leaf directories, tile data
                progress("寫入離線地圖檔…")
                Dim entries As New List(Of Entry)(found.Select(Function(e) New Entry With {.TileId = e.TileId, .RunLength = e.RunLength, .Offset = newOffset(e.Offset), .Length = e.Length}))
                Dim rootBytes() As Byte = Nothing, leafBytes() As Byte = Nothing
                BuildDirectories(entries, rootBytes, leafBytes)
                Dim temp As String = Path.Combine(folder, "taiwan.download.pmtiles")
                Using outFile As New FileStream(temp, FileMode.Create, FileAccess.Write)
                    Dim o As New Header With {
                        .RootOffset = 127, .RootLength = CULng(rootBytes.Length),
                        .MetadataOffset = 127UL + CULng(rootBytes.Length), .MetadataLength = CULng(metadata.Length),
                        .LeafOffset = 127UL + CULng(rootBytes.Length) + CULng(metadata.Length), .LeafLength = CULng(leafBytes.Length),
                        .AddressedTiles = CULng(entries.Sum(Function(e) CLng(e.RunLength))), .TileEntries = CULng(entries.Count), .TileContents = CULng(pieces.Count),
                        .InternalCompression = 2, .TileCompression = h.TileCompression, .TileType = h.TileType,
                        .MinZoom = 0, .MaxZoom = CByte(Math.Min(MaxZoom, h.MaxZoom)),
                        .MinLonE7 = CInt(MinLon * 10000000.0), .MinLatE7 = CInt(MinLat * 10000000.0), .MaxLonE7 = CInt(MaxLon * 10000000.0), .MaxLatE7 = CInt(MaxLat * 10000000.0),
                        .CenterZoom = 7, .CenterLonE7 = CInt(120.9 * 10000000.0), .CenterLatE7 = CInt(23.7 * 10000000.0)}
                    o.TileDataOffset = o.LeafOffset + o.LeafLength
                    o.TileDataLength = CULng(data.Length)
                    outFile.Write(o.ToBytes())
                    outFile.Write(rootBytes)
                    outFile.Write(metadata)
                    outFile.Write(leafBytes)
                    data.Position = 0
                    data.CopyTo(outFile)
                End Using
                ' in place of the old one (MapHost reads it with FileShare.Delete, so an open map doesn't block this)
                IO.File.Move(temp, MapHost.OfflineMapFile, overwrite:=True)
                End Using
            Finally
                ' cancelled, failed or done: the work files go (the old map, if any, is still in place)
                For Each f In {dataFile, Path.Combine(folder, "taiwan.download.pmtiles")}
                    Try
                        If IO.File.Exists(f) Then IO.File.Delete(f)
                    Catch ex As IOException
                    Catch ex As UnauthorizedAccessException
                    End Try
                Next
            End Try
            IO.File.WriteAllLines(MapHost.OfflineMapFile & ".info", {build, Date.Now.ToString("s")})
            Return Current()
        End Using
    End Function

    ''' <summary>The newest build's key without ".pmtiles" (yyyyMMdd).</summary>
    Private Async Function LatestBuildAsync(ByVal http As HttpClient, ByVal cancel As CancellationToken) As Task(Of String)
        Dim json As String = Await http.GetStringAsync(BuildsUrl, cancel)
        Dim keys = Text.RegularExpressions.Regex.Matches(json, """key""\s*:\s*""(\d{8})\.pmtiles""").Select(Function(m) m.Groups(1).Value).ToList()
        If keys.Count = 0 Then Throw New InvalidDataException("找不到地圖資料的版本")
        Return keys.Max()
    End Function

    Private Async Function RangeAsync(ByVal http As HttpClient, ByVal url As String, ByVal offset As ULong, ByVal length As Long, ByVal cancel As CancellationToken) As Task(Of Byte())
        If length <= 0 Then Return Array.Empty(Of Byte)()
        For attempt = 1 To 5
            If attempt > 1 Then Await Task.Delay(500 * attempt, cancel)   ' (a network hiccup: again)
            ' a range that stalls (no bytes for 20 s) is dropped and asked again -- on a fresh connection
            ' (a throttled one was seen crawling at 50 KB/s while others ran at MB/s)
            Using stall As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancel)
                Try
                    Using req As New HttpRequestMessage(HttpMethod.Get, url)
                        req.Headers.Range = New RangeHeaderValue(CLng(offset), CLng(offset) + length - 1)
                        If attempt > 1 Then req.Headers.ConnectionClose = True
                        stall.CancelAfter(StallSeconds * 1000)
                        Using resp As HttpResponseMessage = Await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, stall.Token)
                            resp.EnsureSuccessStatusCode()
                            Dim buf(CInt(length) - 1) As Byte
                            Dim got As Integer = 0
                            Using s As Stream = Await resp.Content.ReadAsStreamAsync(stall.Token)
                                Do
                                    stall.CancelAfter(StallSeconds * 1000)
                                    If got >= buf.Length Then
                                        ' a server that ignores Range sends more than asked
                                        If resp.StatusCode <> Net.HttpStatusCode.PartialContent Then Throw New InvalidDataException("地圖伺服器不支援分段下載")
                                        Exit Do
                                    End If
                                    Dim n As Integer = Await s.ReadAsync(buf.AsMemory(got, buf.Length - got), stall.Token)
                                    If n <= 0 Then Exit Do
                                    got += n
                                Loop
                            End Using
                            If got < buf.Length Then Throw New HttpRequestException("地圖資料不完整")
                            Return buf
                        End Using
                    End Using
                Catch ex As OperationCanceledException When Not cancel.IsCancellationRequested AndAlso attempt < 5
                    ' stalled: again
                Catch ex As HttpRequestException When attempt < 5
                Catch ex As IOException When attempt < 5
                End Try
            End Using
        Next
        Throw New HttpRequestException("下載失敗（網路太慢或中斷）")
    End Function

    ''' <summary>The tile entries of <paramref name="dir"/> that hold a wanted id (runs cut to the wanted ids),
    ''' going down into leaf directories that may hold some.</summary>
    Private Async Function CollectAsync(ByVal http As HttpClient, ByVal url As String, ByVal h As Header, ByVal dir As List(Of Entry),
                                        ByVal wanted As List(Of ULong), ByVal found As List(Of Entry), ByVal depth As Integer,
                                        ByVal progress As Action(Of String), ByVal cancel As CancellationToken) As Task
        If depth > 4 Then Throw New InvalidDataException("地圖目錄太深")
        For i = 0 To dir.Count - 1
            Dim e As Entry = dir(i)
            Dim [end] As ULong = If(i + 1 < dir.Count, dir(i + 1).TileId, ULong.MaxValue)
            If e.RunLength = 0 Then
                ' a leaf directory: tiles from its id up to the next entry's
                If Not AnyIn(wanted, e.TileId, [end]) Then Continue For
                If depth = 0 Then progress("讀取地圖目錄…")
                Dim leaf() As Byte = Await RangeAsync(http, url, h.LeafOffset + e.Offset, e.Length, cancel)
                Await CollectAsync(http, url, h, ReadDirectory(leaf), wanted, found, depth + 1, progress, cancel)
            Else
                ' tiles e.TileId .. e.TileId + RunLength - 1 share this data: keep the wanted ones, as runs
                Dim s As Integer = LowerBound(wanted, e.TileId)
                Dim runEnd As ULong = e.TileId + CULng(e.RunLength)
                While s < wanted.Count AndAlso wanted(s) < runEnd
                    Dim startId As ULong = wanted(s), n As UInteger = 1
                    While s + CInt(n) < wanted.Count AndAlso wanted(s + CInt(n)) = startId + n AndAlso startId + n < runEnd
                        n += 1UI
                    End While
                    found.Add(New Entry With {.TileId = startId, .RunLength = n, .Offset = e.Offset, .Length = e.Length})
                    s += CInt(n)
                End While
            End If
        Next
    End Function

    Private Function LowerBound(ByVal list As List(Of ULong), ByVal value As ULong) As Integer
        Dim lo As Integer = 0, hi As Integer = list.Count
        While lo < hi
            Dim mid As Integer = (lo + hi) \ 2
            If list(mid) < value Then lo = mid + 1 Else hi = mid
        End While
        Return lo
    End Function

    Private Function AnyIn(ByVal list As List(Of ULong), ByVal from As ULong, ByVal [to] As ULong) As Boolean
        Dim i As Integer = LowerBound(list, from)
        Return i < list.Count AndAlso list(i) < [to]
    End Function

    '==================================================================================================
    ' PMTiles v3
    '==================================================================================================
    Private Class Entry
        Public TileId As ULong
        Public Offset As ULong
        Public Length As Integer
        Public RunLength As UInteger
    End Class

    Private Class Header
        Public RootOffset, RootLength, MetadataOffset, MetadataLength, LeafOffset, LeafLength, TileDataOffset, TileDataLength As ULong
        Public AddressedTiles, TileEntries, TileContents As ULong
        Public Clustered As Byte, InternalCompression As Byte, TileCompression As Byte, TileType As Byte, MinZoom As Byte, MaxZoom As Byte
        Public MinLonE7, MinLatE7, MaxLonE7, MaxLatE7 As Integer
        Public CenterZoom As Byte, CenterLonE7, CenterLatE7 As Integer

        Public Shared Function Parse(ByVal b() As Byte) As Header
            If b.Length < 127 OrElse Text.Encoding.ASCII.GetString(b, 0, 7) <> "PMTiles" OrElse b(7) <> 3 Then Throw New InvalidDataException("不是 PMTiles v3 地圖資料")
            Return New Header With {
                .RootOffset = BitConverter.ToUInt64(b, 8), .RootLength = BitConverter.ToUInt64(b, 16),
                .MetadataOffset = BitConverter.ToUInt64(b, 24), .MetadataLength = BitConverter.ToUInt64(b, 32),
                .LeafOffset = BitConverter.ToUInt64(b, 40), .LeafLength = BitConverter.ToUInt64(b, 48),
                .TileDataOffset = BitConverter.ToUInt64(b, 56), .TileDataLength = BitConverter.ToUInt64(b, 64),
                .Clustered = b(96), .InternalCompression = b(97), .TileCompression = b(98), .TileType = b(99), .MinZoom = b(100), .MaxZoom = b(101)}
        End Function

        Public Function ToBytes() As Byte()
            Dim b(126) As Byte
            Text.Encoding.ASCII.GetBytes("PMTiles").CopyTo(b, 0)
            b(7) = 3
            Dim put = Sub(at As Integer, v As ULong) BitConverter.GetBytes(v).CopyTo(b, at)
            Dim put32 = Sub(at As Integer, v As Integer) BitConverter.GetBytes(v).CopyTo(b, at)
            put(8, RootOffset) : put(16, RootLength) : put(24, MetadataOffset) : put(32, MetadataLength)
            put(40, LeafOffset) : put(48, LeafLength) : put(56, TileDataOffset) : put(64, TileDataLength)
            put(72, AddressedTiles) : put(80, TileEntries) : put(88, TileContents)
            b(96) = 0 : b(97) = InternalCompression : b(98) = TileCompression : b(99) = TileType : b(100) = MinZoom : b(101) = MaxZoom
            put32(102, MinLonE7) : put32(106, MinLatE7) : put32(110, MaxLonE7) : put32(114, MaxLatE7)
            b(118) = CenterZoom : put32(119, CenterLonE7) : put32(123, CenterLatE7)
            Return b
        End Function
    End Class

    Private Function Slice(ByVal b() As Byte, ByVal offset As ULong, ByVal length As ULong) As Byte()
        Dim r(CInt(length) - 1) As Byte
        Array.Copy(b, CInt(offset), r, 0, CInt(length))
        Return r
    End Function

    ''' <summary>A gzip'd directory: count, tile id deltas, run lengths, lengths, offsets (0 = right after the previous).</summary>
    Private Function ReadDirectory(ByVal gz() As Byte) As List(Of Entry)
        Dim raw() As Byte
        Using src As New MemoryStream(gz), z As New GZipStream(src, CompressionMode.Decompress), m As New MemoryStream
            z.CopyTo(m)
            raw = m.ToArray()
        End Using
        Dim pos As Integer = 0
        Dim n As Integer = CInt(ReadVarint(raw, pos))
        Dim list As New List(Of Entry)(n)
        Dim last As ULong = 0
        For i = 0 To n - 1
            last += ReadVarint(raw, pos)
            list.Add(New Entry With {.TileId = last})
        Next
        For i = 0 To n - 1
            list(i).RunLength = CUInt(ReadVarint(raw, pos))
        Next
        For i = 0 To n - 1
            list(i).Length = CInt(ReadVarint(raw, pos))
        Next
        For i = 0 To n - 1
            Dim v As ULong = ReadVarint(raw, pos)
            list(i).Offset = If(v = 0 AndAlso i > 0, list(i - 1).Offset + CULng(list(i - 1).Length), v - 1UL)
        Next
        Return list
    End Function

    Private Function WriteDirectory(ByVal entries As IList(Of Entry)) As Byte()
        Using m As New MemoryStream
            WriteVarint(m, CULng(entries.Count))
            Dim last As ULong = 0
            For Each e In entries
                WriteVarint(m, e.TileId - last) : last = e.TileId
            Next
            For Each e In entries : WriteVarint(m, e.RunLength) : Next
            For Each e In entries : WriteVarint(m, CULng(e.Length)) : Next
            For i = 0 To entries.Count - 1
                If i > 0 AndAlso entries(i).Offset = entries(i - 1).Offset + CULng(entries(i - 1).Length) Then
                    WriteVarint(m, 0)
                Else
                    WriteVarint(m, entries(i).Offset + 1UL)
                End If
            Next
            Using outer As New MemoryStream
                Using z As New GZipStream(outer, CompressionLevel.Optimal, leaveOpen:=True)
                    m.Position = 0
                    m.CopyTo(z)
                End Using
                Return outer.ToArray()
            End Using
        End Using
    End Function

    ''' <summary>The root directory, and the leaf directories when the entries don't fit in a 16 KB root.</summary>
    Private Sub BuildDirectories(ByVal entries As List(Of Entry), ByRef root() As Byte, ByRef leaves() As Byte)
        root = WriteDirectory(entries)
        leaves = Array.Empty(Of Byte)()
        If root.Length <= 16384 - 127 Then Return
        Dim leafSize As Integer = 4096
        Do
            Dim rootEntries As New List(Of Entry)
            Using m As New MemoryStream
                For i = 0 To entries.Count - 1 Step leafSize
                    Dim part = entries.GetRange(i, Math.Min(leafSize, entries.Count - i))
                    Dim bytes() As Byte = WriteDirectory(part)
                    rootEntries.Add(New Entry With {.TileId = part(0).TileId, .Offset = CULng(m.Position), .Length = bytes.Length, .RunLength = 0})
                    m.Write(bytes)
                Next
                root = WriteDirectory(rootEntries)
                leaves = m.ToArray()
            End Using
            If root.Length <= 16384 - 127 Then Return
            leafSize *= 2
        Loop
    End Sub

    Private Function ReadVarint(ByVal b() As Byte, ByRef pos As Integer) As ULong
        Dim result As ULong = 0
        Dim shift As Integer = 0
        Do
            Dim c As Byte = b(pos)
            pos += 1
            result = result Or (CULng(c And &H7F) << shift)
            If (c And &H80) = 0 Then Return result
            shift += 7
        Loop
    End Function

    Private Sub WriteVarint(ByVal s As Stream, ByVal v As ULong)
        While v >= &H80UL
            s.WriteByte(CByte((v And &H7FUL) Or &H80UL))
            v >>= 7
        End While
        s.WriteByte(CByte(v))
    End Sub

    ''' <summary>The Hilbert-curve tile id: the tiles of the zooms before, then along the curve.</summary>
    Private Function ZxyToTileId(ByVal z As Integer, ByVal x As Integer, ByVal y As Integer) As ULong
        Dim acc As ULong = 0
        For i = 0 To z - 1
            acc += 1UL << (2 * i)
        Next
        Dim n As Long = 1L << z
        Dim d As ULong = 0
        Dim px As Long = x, py As Long = y
        Dim s As Long = n \ 2
        While s > 0
            Dim rx As Integer = If((px And s) > 0, 1, 0)
            Dim ry As Integer = If((py And s) > 0, 1, 0)
            d += CULng(s) * CULng(s) * CULng((3 * rx) Xor ry)
            If ry = 0 Then
                If rx = 1 Then
                    px = n - 1 - px
                    py = n - 1 - py
                End If
                Dim t As Long = px : px = py : py = t
            End If
            s \= 2
        End While
        Return acc + d
    End Function

    Private Function LonToX(ByVal lon As Double, ByVal z As Integer) As Integer
        Return Math.Min((1 << z) - 1, Math.Max(0, CInt(Math.Floor((lon + 180.0) / 360.0 * (1 << z)))))
    End Function

    Private Function LatToY(ByVal lat As Double, ByVal z As Integer) As Integer
        Dim r As Double = lat * Math.PI / 180.0
        Dim y As Double = (1.0 - Math.Log(Math.Tan(r) + 1.0 / Math.Cos(r)) / Math.PI) / 2.0 * (1 << z)
        Return Math.Min((1 << z) - 1, Math.Max(0, CInt(Math.Floor(y))))
    End Function

End Module
