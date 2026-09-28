Imports System.IO

' Where every photo was taken (new in the .NET port; 拍攝地點附近的照片): the GPS of every album photo,
' read once (ShotInfo, header only) and kept in a cache by path, size and time, so after the first
' time only new / changed photos are read. May run on a worker thread.
Public Module GeoIndex

    Public ReadOnly Property CacheFile As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "GeoCache.txt")
        End Get
    End Property

    ''' <summary>The photos that have a place: file -> (latitude, longitude).</summary>
    Public Function Places(ByVal roots As IEnumerable(Of String), Optional ByVal progress As Action(Of Integer, Integer) = Nothing,
                           Optional ByVal cancel As Threading.CancellationToken = Nothing) As Dictionary(Of String, (Lat As Double, Lon As Double))
        Dim files As List(Of String) = Duplicates.AlbumPhotos(roots).Where(Function(f) GetMediaType(Nothing, f) = enumPhotoMediaType.mdImage).ToList()
        Dim cache As Dictionary(Of String, String) = LoadCache()
        Dim fresh As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim result As New Dictionary(Of String, (Lat As Double, Lon As Double))(StringComparer.OrdinalIgnoreCase)
        For i = 0 To files.Count - 1
            If cancel.IsCancellationRequested Then Exit For
            Dim f As String = files(i)
            Dim key As String
            Try
                Dim fi As New FileInfo(f)
                key = f & "|" & fi.Length & "|" & fi.LastWriteTimeUtc.Ticks
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                Continue For
            End Try
            Dim v As String = Nothing
            If Not cache.TryGetValue(key, v) Then
                Dim s As ShotInfo.Shot = ShotInfo.Read(f)
                v = If(s IsNot Nothing AndAlso s.HasPlace,
                       s.Latitude.Value.ToString("R", Globalization.CultureInfo.InvariantCulture) & "," & s.Longitude.Value.ToString("R", Globalization.CultureInfo.InvariantCulture), "-")
            End If
            fresh(key) = v
            If v <> "-" Then
                Dim parts() As String = v.Split(","c)
                result(f) = (Double.Parse(parts(0), Globalization.CultureInfo.InvariantCulture), Double.Parse(parts(1), Globalization.CultureInfo.InvariantCulture))
            End If
            If progress IsNot Nothing AndAlso (i Mod 100 = 0 OrElse i = files.Count - 1) Then progress(i + 1, files.Count)
        Next
        If Not cancel.IsCancellationRequested Then SaveCache(fresh)   ' only what is still there
        Return result
    End Function

    ''' <summary>The photos within <paramref name="km"/> of a place, nearest first.</summary>
    Public Function Nearby(ByVal places As Dictionary(Of String, (Lat As Double, Lon As Double)), ByVal lat As Double, ByVal lon As Double, ByVal km As Double) As List(Of String)
        Return places.Select(Function(kv) (File:=kv.Key, D:=ShotInfo.DistanceKm(lat, lon, kv.Value.Lat, kv.Value.Lon))).
                      Where(Function(x) x.D <= km).OrderBy(Function(x) x.D).Select(Function(x) x.File).ToList()
    End Function

    Private Function LoadCache() As Dictionary(Of String, String)
        Dim d As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Try
            If File.Exists(CacheFile) Then
                For Each line In File.ReadLines(CacheFile, Text.Encoding.UTF8)
                    Dim tab As Integer = line.LastIndexOf(ChrW(9))
                    If tab > 0 Then d(line.Substring(0, tab)) = line.Substring(tab + 1)
                Next
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return d
    End Function

    Private Sub SaveCache(ByVal d As Dictionary(Of String, String))
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(CacheFile))
            File.WriteAllLines(CacheFile, d.Select(Function(kv) kv.Key & ChrW(9) & kv.Value), Text.Encoding.UTF8)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

End Module
