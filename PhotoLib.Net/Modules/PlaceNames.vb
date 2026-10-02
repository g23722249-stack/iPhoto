Imports System.IO
Imports System.Net.Http

' Where a photo was taken, as the .Exif keeps it (new in the .NET port):
'   [Exif] GPS=24.165750,120.685190   decimal degrees, from the photo's own EXIF GPS (ShotInfo)
'   [Exif] Spot=台中市西屯區          the 地點 field: filled from the GPS when it is blank (what was typed
'                                     by hand is never overwritten)
' The place name comes from (設定: Config.PlaceSource)
'   psOffline (default)  the nearest entry of PlacesFile (a list of towns with their coordinates, next to
'                        iPhoto.exe) within OfflineKm -- nothing leaves the computer
'   psOnline             OpenStreetMap's Nominatim (the coordinates are sent there), at most one request a
'                        second as its rules ask, answers cached in OnlineCacheFile; offline when it fails
Public Module PlaceNames

    Public Enum enumPlaceSource
        psOffline = 0
        psOnline = 1
    End Enum

    ''' <summary>Farther than this from every known town: no name (a photo at sea, abroad ...).</summary>
    Public Const OfflineKm As Double = 20

    '==================================================================================================
    ' The GPS value of the .Exif
    '==================================================================================================
    Public Function FormatGps(ByVal lat As Double, ByVal lon As Double) As String
        Return lat.ToString("0.000000", Globalization.CultureInfo.InvariantCulture) & "," & lon.ToString("0.000000", Globalization.CultureInfo.InvariantCulture)
    End Function

    Public Function TryParseGps(ByVal s As String, ByRef lat As Double, ByRef lon As Double) As Boolean
        Dim parts() As String = If(s, "").Split(","c)
        If parts.Length <> 2 Then Return False
        Return Double.TryParse(parts(0).Trim(), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, lat) AndAlso
               Double.TryParse(parts(1).Trim(), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, lon) AndAlso
               Math.Abs(lat) <= 90 AndAlso Math.Abs(lon) <= 180 AndAlso Not (lat = 0 AndAlso lon = 0)
    End Function

    ''' <summary>The photo's GPS read from the picture itself, as the .Exif writes it; "" when it has none.</summary>
    Public Function GpsOfPicture(ByVal file As String) As String
        Dim s As ShotInfo.Shot = ShotInfo.Read(file)
        If s Is Nothing OrElse Not s.HasPlace Then Return ""
        Return FormatGps(s.Latitude.Value, s.Longitude.Value)
    End Function

    ''' <summary>The 地點 for a .Exif GPS value ("" when there is none or no name is known): an attraction
    ''' within 200 m (Attractions), else the county and district.</summary>
    Public Function PlaceOf(ByVal gps As String) As String
        Return Resolve(gps).Spot
    End Function

    ''' <summary>What the .Exif keeps of where a photo was taken:
    '''   [Exif] Spot=九族文化村          the attraction within 200 m, else the county + district
    '''   [Exif] Country=臺灣
    '''   [Exif] City=南投縣魚池鄉        county + district
    '''   [Exif] Town=魚池鄉              district</summary>
    Public Class PlaceInfo
        Public Spot As String = ""
        Public Country As String = ""
        Public City As String = ""
        Public Town As String = ""
    End Class

    ''' <summary>Taiwan's 22 counties / cities all have three characters (臺中市, 新竹縣 ...).</summary>
    Private ReadOnly TaiwanCounties As String() = {
        "臺北市", "新北市", "桃園市", "臺中市", "臺南市", "高雄市", "基隆市", "新竹市", "嘉義市", "新竹縣", "苗栗縣",
        "彰化縣", "南投縣", "雲林縣", "嘉義縣", "屏東縣", "宜蘭縣", "花蓮縣", "臺東縣", "澎湖縣", "金門縣", "連江縣"}

    ''' <summary>Where the GPS value is (all fields "" when it isn't a position or nothing is known).</summary>
    Public Function Resolve(ByVal gps As String) As PlaceInfo
        Dim info As New PlaceInfo
        Dim lat, lon As Double
        If Not TryParseGps(gps, lat, lon) Then Return info
        ' County and district from where the GPS really is (the township list / OpenStreetMap): the
        ' attraction data's own address is not always right (「桃山部落」 at 南庄 is listed under 和平區).
        Dim a As Attractions.Attraction = Attractions.Nearest(lat, lon)
        Dim name As String = PlaceOf(lat, lon)      ' 臺中市西屯區 (online: whatever OpenStreetMap says)
        If name = "" AndAlso a IsNot Nothing Then name = a.City & a.Town
        info.Spot = If(a IsNot Nothing, a.Name, name)
        info.City = name
        Dim county As String = TaiwanCounties.FirstOrDefault(Function(c) name.Replace("台", "臺").StartsWith(c))
        If county IsNot Nothing Then
            info.Country = "臺灣"
            info.Town = name.Substring(county.Length)
        End If
        Return info
    End Function

    Public Function PlaceOf(ByVal lat As Double, ByVal lon As Double) As String
        Dim source As enumPlaceSource = If(g_lpConfig Is Nothing, enumPlaceSource.psOffline, g_lpConfig.PlaceSource)
        If source = enumPlaceSource.psOnline Then
            Dim name As String = OnlinePlace(lat, lon)
            If name IsNot Nothing Then Return name
        End If
        Return OfflinePlace(lat, lon)
    End Function

    '==================================================================================================
    ' Offline: the nearest town of PlacesFile
    '==================================================================================================
    ''' <summary>"name TAB latitude TAB longitude" per line (UTF-8), e.g. 「臺中市西屯區	24.1800	120.6400」; a
    ''' name may have many points. The shipped file: 內政部國土測繪中心 鄉(鎮、市、區)界線 (政府資料開放授權),
    ''' a point every 0.01 degree inside each township plus the centre of each of its parts.</summary>
    Public ReadOnly Property PlacesFile As String
        Get
            Return Path.Combine(AppContext.BaseDirectory, "Places", "Places.txt")
        End Get
    End Property

    Private s_towns As List(Of (Name As String, Lat As Double, Lon As Double))
    Private ReadOnly s_townsLock As New Object

    Private Function Towns() As List(Of (Name As String, Lat As Double, Lon As Double))
        SyncLock s_townsLock
            If s_towns IsNot Nothing Then Return s_towns
            s_towns = New List(Of (Name As String, Lat As Double, Lon As Double))
            Try
                If File.Exists(PlacesFile) Then
                    For Each line In File.ReadLines(PlacesFile, Text.Encoding.UTF8)
                        Dim p() As String = line.Split(ChrW(9))
                        Dim lat, lon As Double
                        If p.Length >= 3 AndAlso p(0).Trim() <> "" AndAlso
                           Double.TryParse(p(1), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, lat) AndAlso
                           Double.TryParse(p(2), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, lon) Then
                            s_towns.Add((p(0).Trim(), lat, lon))
                        End If
                    Next
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
            Return s_towns
        End SyncLock
    End Function

    ''' <summary>A township of PlacesFile, for picking one by hand (輸入地點): its 縣市, 鄉鎮 and a point to
    ''' use as its position -- the township's point nearest the average of its points (so it lies inside it).</summary>
    Public Class TownPoint
        Public County As String = ""   ' 臺中市
        Public Town As String = ""     ' 西屯區
        Public Lat As Double
        Public Lon As Double
        Public ReadOnly Property Name As String
            Get
                Return County & Town
            End Get
        End Property
    End Class

    Private s_centres As List(Of TownPoint)

    ''' <summary>Every township of PlacesFile, by 縣市 in the usual order, then by name.</summary>
    Public Function TownCentres() As List(Of TownPoint)
        SyncLock s_townsLock
            If s_centres IsNot Nothing Then Return s_centres
        End SyncLock
        Dim result As New List(Of TownPoint)
        For Each g In Towns().GroupBy(Function(t) t.Name)
            Dim county As String = TaiwanCounties.FirstOrDefault(Function(c) g.Key.StartsWith(c))
            If county Is Nothing Then Continue For
            Dim mLat As Double = g.Average(Function(t) t.Lat), mLon As Double = g.Average(Function(t) t.Lon)
            Dim best = g.OrderBy(Function(t) (t.Lat - mLat) ^ 2 + (t.Lon - mLon) ^ 2).First()
            result.Add(New TownPoint With {.County = county, .Town = g.Key.Substring(county.Length), .Lat = best.Lat, .Lon = best.Lon})
        Next
        result = result.OrderBy(Function(t) Array.IndexOf(TaiwanCounties, t.County)).ThenBy(Function(t) t.Town, StringComparer.Create(New Globalization.CultureInfo("zh-TW"), False)).ToList()
        SyncLock s_townsLock
            s_centres = result
        End SyncLock
        Return result
    End Function

    ''' <summary>The 22 縣市 in the usual order.</summary>
    Public ReadOnly Property Counties As String()
        Get
            Return CType(TaiwanCounties.Clone(), String())
        End Get
    End Property

    ''' <summary>True when the offline list is there (else offline names are all "").</summary>
    Public ReadOnly Property OfflineAvailable As Boolean
        Get
            Return Towns().Count > 0
        End Get
    End Property

    Public Function OfflinePlace(ByVal lat As Double, ByVal lon As Double) As String
        Dim best As String = "", bestKm As Double = OfflineKm
        ' 0.2 degrees is more than OfflineKm at Taiwan's latitudes: the far points skip the trigonometry
        For Each t In Towns()
            If Math.Abs(t.Lat - lat) > 0.2 OrElse Math.Abs(t.Lon - lon) > 0.2 Then Continue For
            Dim km As Double = ShotInfo.DistanceKm(lat, lon, t.Lat, t.Lon)
            If km < bestKm Then bestKm = km : best = t.Name
        Next
        Return best
    End Function

    '==================================================================================================
    ' Online: OpenStreetMap Nominatim
    '==================================================================================================
    Public ReadOnly Property OnlineCacheFile As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "PlaceCache.txt")
        End Get
    End Property

    Private s_online As Dictionary(Of String, String)
    Private s_lastRequest As DateTime = DateTime.MinValue
    Private ReadOnly s_onlineLock As New Object
    Private ReadOnly s_http As New HttpClient With {.Timeout = TimeSpan.FromSeconds(10)}

    ''' <summary>The name OpenStreetMap gives (county + district level, in Chinese); Nothing when it can't
    ''' be asked (no network, refused), so the caller falls back to the offline list.</summary>
    Public Function OnlinePlace(ByVal lat As Double, ByVal lon As Double) As String
        ' about 100 m: nearby photos share one request
        Dim key As String = lat.ToString("0.000", Globalization.CultureInfo.InvariantCulture) & "," & lon.ToString("0.000", Globalization.CultureInfo.InvariantCulture)
        SyncLock s_onlineLock
            If s_online Is Nothing Then s_online = LoadOnlineCache()
            Dim cached As String = Nothing
            If s_online.TryGetValue(key, cached) Then Return cached
            Dim wait As TimeSpan = s_lastRequest.AddSeconds(1.1) - DateTime.Now
            If wait > TimeSpan.Zero Then Threading.Thread.Sleep(wait)
            s_lastRequest = DateTime.Now
            Try
                Dim url As String = "https://nominatim.openstreetmap.org/reverse?format=jsonv2&zoom=14&accept-language=zh-TW&lat=" &
                                    lat.ToString("0.######", Globalization.CultureInfo.InvariantCulture) & "&lon=" & lon.ToString("0.######", Globalization.CultureInfo.InvariantCulture)
                Using req As New HttpRequestMessage(HttpMethod.Get, url)
                    req.Headers.UserAgent.ParseAdd("iPhoto.Net/1.0 (personal photo album)")
                    Using resp As HttpResponseMessage = s_http.Send(req)
                        If Not resp.IsSuccessStatusCode Then Return Nothing
                        Using doc As Text.Json.JsonDocument = Text.Json.JsonDocument.Parse(resp.Content.ReadAsStream())
                            Dim name As String = NameFromAddress(doc.RootElement)
                            s_online(key) = name
                            File.AppendAllText(OnlineCacheFile, key & ChrW(9) & name & vbCrLf, Text.Encoding.UTF8)
                            Return name
                        End Using
                    End Using
                End Using
            Catch ex As Exception When TypeOf ex Is HttpRequestException OrElse TypeOf ex Is Threading.Tasks.TaskCanceledException OrElse
                                       TypeOf ex Is IOException OrElse TypeOf ex Is Text.Json.JsonException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return Nothing
            End Try
        End SyncLock
    End Function

    ''' <summary>「台中市西屯區」: the county / city and the district / township of Nominatim's address.</summary>
    Private Function NameFromAddress(ByVal root As Text.Json.JsonElement) As String
        Dim addr As Text.Json.JsonElement
        If Not root.TryGetProperty("address", addr) Then Return ""
        Dim county As String = FirstOf(addr, "city", "county", "state")
        Dim district As String = FirstOf(addr, "city_district", "suburb", "town", "village", "district")
        If district <> "" AndAlso county.EndsWith(district) Then district = ""
        Dim name As String = county & district
        If name = "" Then name = FirstOf(addr, "country")
        Return name
    End Function

    Private Function FirstOf(ByVal addr As Text.Json.JsonElement, ParamArray keys As String()) As String
        For Each key In keys
            Dim v As Text.Json.JsonElement
            If addr.TryGetProperty(key, v) AndAlso v.ValueKind = Text.Json.JsonValueKind.String AndAlso v.GetString() <> "" Then Return v.GetString()
        Next
        Return ""
    End Function

    Private Function LoadOnlineCache() As Dictionary(Of String, String)
        Dim d As New Dictionary(Of String, String)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(OnlineCacheFile))
            If File.Exists(OnlineCacheFile) Then
                For Each line In File.ReadLines(OnlineCacheFile, Text.Encoding.UTF8)
                    Dim tab As Integer = line.IndexOf(ChrW(9))
                    If tab > 0 Then d(line.Substring(0, tab)) = line.Substring(tab + 1)
                Next
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return d
    End Function

End Module
