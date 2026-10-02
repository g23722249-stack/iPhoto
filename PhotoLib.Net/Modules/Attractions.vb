Imports System.IO
Imports System.IO.Compression
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json

' Tourist attractions of Taiwan (new in the .NET port; PlaceNames uses them first): a photo taken within
' MatchKm of one gets its name as 地點 (「九族文化村」) instead of the county and district.
'   Places\Attractions.txt  next to iPhoto.exe, UTF-8: comment lines start with "#", then one attraction a
'                           line: name TAB latitude TAB longitude TAB county TAB district TAB AttractionID.
'   Source: 交通部觀光署 觀光資訊資料庫 景點 (Attraction-json.zip, 政府資料開放授權), made from its
'           AttractionList.json by ConvertJson -- here on 設定 › 地點 「更新景點資料」 (the only time iPhoto
'           goes to the network for it), or ahead of time for the file that ships.
' The .Exif files are written in the ANSI code page (Big5): an attraction whose name has a character
' Big5 lacks (「萡子寮漁港」) is left out, so its photos get the county and district instead of "?".
Public Module Attractions

    Public Const MatchKm As Double = 0.2
    Public Const SourceUrl As String = "https://media.taiwan.net.tw/XMLReleaseAll_public/v2.0/Zh_tw/Attraction-json.zip"

    Public Class Attraction
        Public Name As String = ""
        Public Lat As Double
        Public Lon As Double
        Public City As String = ""   ' 臺中市
        Public Town As String = ""   ' 西屯區
    End Class

    Public ReadOnly Property AttractionsFile As String
        Get
            Return Path.Combine(AppContext.BaseDirectory, "Places", "Attractions.txt")
        End Get
    End Property

    Private s_list As List(Of Attraction)
    Private s_strUpdated As String = ""
    Private ReadOnly s_lock As New Object

    Private Function All() As List(Of Attraction)
        SyncLock s_lock
            If s_list IsNot Nothing Then Return s_list
            s_list = New List(Of Attraction)
            s_strUpdated = ""
            Try
                If File.Exists(AttractionsFile) Then
                    For Each line In File.ReadLines(AttractionsFile, Encoding.UTF8)
                        If line.StartsWith("#") Then
                            If line.StartsWith("#updated ") Then s_strUpdated = line.Substring(9).Trim()
                            Continue For
                        End If
                        Dim p() As String = line.Split(ChrW(9))
                        Dim lat, lon As Double
                        If p.Length >= 5 AndAlso p(0).Trim() <> "" AndAlso
                           Double.TryParse(p(1), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, lat) AndAlso
                           Double.TryParse(p(2), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, lon) Then
                            s_list.Add(New Attraction With {.Name = p(0).Trim(), .Lat = lat, .Lon = lon, .City = p(3).Trim(), .Town = p(4).Trim()})
                        End If
                    Next
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
            Return s_list
        End SyncLock
    End Function

    ''' <summary>Reads the file again (after an update).</summary>
    Public Sub Reload()
        SyncLock s_lock
            s_list = Nothing
        End SyncLock
    End Sub

    Public ReadOnly Property Count As Integer
        Get
            Return All().Count
        End Get
    End Property

    ''' <summary>When the source data was published ("" when unknown).</summary>
    Public ReadOnly Property Updated As String
        Get
            All()
            Return s_strUpdated
        End Get
    End Property

    ''' <summary>The nearest attraction within <paramref name="maxKm"/>, or Nothing.</summary>
    Public Function Nearest(ByVal lat As Double, ByVal lon As Double, Optional ByVal maxKm As Double = MatchKm) As Attraction
        Dim best As Attraction = Nothing, bestKm As Double = maxKm
        Dim box As Double = maxKm / 100 + 0.001    ' about maxKm in degrees, a little more
        For Each a In All()
            If Math.Abs(a.Lat - lat) > box OrElse Math.Abs(a.Lon - lon) > box * 1.2 Then Continue For
            Dim km As Double = ShotInfo.DistanceKm(lat, lon, a.Lat, a.Lon)
            If km <= bestKm Then bestKm = km : best = a
        Next
        Return best
    End Function

    ''' <summary>Attractions whose name (or 縣市 / 鄉鎮) contains every word of <paramref name="query"/>
    ''' (split on spaces; 台 = 臺), within <paramref name="city"/> when given; one per name (the file lists
    ''' some twice, from two agencies), names starting with the query first. At most <paramref name="max"/>.</summary>
    Public Function Search(ByVal query As String, Optional ByVal city As String = "", Optional ByVal max As Integer = 300) As List(Of Attraction)
        Dim words() As String = If(query, "").Replace("台", "臺").Split({" "c, "　"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim hits As New List(Of Attraction)
        For Each a In All()
            If city <> "" AndAlso a.City <> city Then Continue For
            Dim text As String = (a.Name & " " & a.City & a.Town).Replace("台", "臺")
            If Not words.All(Function(w) text.Contains(w, StringComparison.OrdinalIgnoreCase)) Then Continue For
            If seen.Add(a.Name) Then hits.Add(a)
        Next
        Dim first As String = If(words.Length > 0, words(0), "")
        Return hits.OrderBy(Function(a) If(first <> "" AndAlso a.Name.Replace("台", "臺").StartsWith(first), 0, 1)).ThenBy(Function(a) a.Name.Length).Take(max).ToList()
    End Function

    '==================================================================================================
    ' Making the file
    '==================================================================================================
    ''' <summary>Writes <paramref name="outFile"/> from AttractionList.json; returns how many attractions
    ''' were kept and how many were left out (no position, or a name Big5 can't hold).</summary>
    Public Function ConvertJson(ByVal json As Stream, ByVal outFile As String) As (Kept As Integer, Skipped As Integer)
        Dim big5 As Encoding = Encoding.GetEncoding(AnsiText.Encoding.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
        Dim kept As Integer = 0, skipped As Integer = 0
        Dim sb As New StringBuilder
        Using doc As JsonDocument = JsonDocument.Parse(json)
            Dim root As JsonElement = doc.RootElement
            Dim updated As String = ""
            Dim u As JsonElement
            If root.TryGetProperty("UpdateTime", u) AndAlso u.ValueKind = JsonValueKind.String Then updated = u.GetString()
            sb.AppendLine("# 交通部觀光署 觀光資訊資料庫 景點（政府資料開放授權）" & SourceUrl)
            sb.AppendLine("# 名稱 TAB 緯度 TAB 經度 TAB 縣市 TAB 鄉鎮市區 TAB 景點代碼")
            sb.AppendLine("#updated " & updated)
            For Each a In root.GetProperty("Attractions").EnumerateArray()
                Dim name As String = JStr(a, "AttractionName").Trim()
                Dim lat As Double = JNum(a, "PositionLat"), lon As Double = JNum(a, "PositionLon")
                Dim city As String = "", town As String = ""
                Dim pa As JsonElement
                If a.TryGetProperty("PostalAddress", pa) AndAlso pa.ValueKind = JsonValueKind.Object Then
                    city = JStr(pa, "City").Trim()
                    town = JStr(pa, "Town").Trim()
                End If
                If name = "" OrElse lat = 0 OrElse lon = 0 OrElse name.IndexOfAny({ChrW(9), ChrW(10), ChrW(13)}) >= 0 OrElse Not Fits(big5, name) Then
                    skipped += 1
                    Continue For
                End If
                sb.Append(name).Append(ChrW(9)).
                   Append(lat.ToString("0.000000", Globalization.CultureInfo.InvariantCulture)).Append(ChrW(9)).
                   Append(lon.ToString("0.000000", Globalization.CultureInfo.InvariantCulture)).Append(ChrW(9)).
                   Append(city).Append(ChrW(9)).Append(town).Append(ChrW(9)).Append(JStr(a, "AttractionID")).AppendLine()
                kept += 1
            Next
        End Using
        Directory.CreateDirectory(Path.GetDirectoryName(outFile))
        Dim temp As String = outFile & ".new"
        File.WriteAllText(temp, sb.ToString(), New UTF8Encoding(False))
        File.Move(temp, outFile, True)
        Return (kept, skipped)
    End Function

    ''' <summary>設定 › 地點 「更新景點資料」: downloads the current list from 觀光署 and replaces the file.
    ''' Throws when it can't (no network, the format changed); the old file stays then.</summary>
    Public Function Download() As (Kept As Integer, Skipped As Integer)
        Using http As New HttpClient With {.Timeout = TimeSpan.FromMinutes(2)}
            http.DefaultRequestHeaders.UserAgent.ParseAdd("iPhoto.Net/1.0 (personal photo album)")
            Using zipStream As Stream = http.GetStreamAsync(SourceUrl).GetAwaiter().GetResult(), buffer As New MemoryStream
                zipStream.CopyTo(buffer)
                buffer.Position = 0
                Using zip As New ZipArchive(buffer, ZipArchiveMode.Read)
                    Dim entry As ZipArchiveEntry = zip.Entries.FirstOrDefault(Function(e) e.Name.Equals("AttractionList.json", StringComparison.OrdinalIgnoreCase))
                    If entry Is Nothing Then Throw New InvalidDataException("下載的檔案裡沒有 AttractionList.json")
                    Using s As Stream = entry.Open()
                        Dim result = ConvertJson(s, AttractionsFile)
                        Reload()
                        Return result
                    End Using
                End Using
            End Using
        End Using
    End Function

    Private Function Fits(ByVal big5 As Encoding, ByVal s As String) As Boolean
        Try
            big5.GetBytes(s)
            Return True
        Catch ex As EncoderFallbackException
            Return False
        End Try
    End Function

    Private Function JStr(ByVal e As JsonElement, ByVal key As String) As String
        Dim v As JsonElement
        Return If(e.TryGetProperty(key, v) AndAlso v.ValueKind = JsonValueKind.String, v.GetString(), "")
    End Function

    Private Function JNum(ByVal e As JsonElement, ByVal key As String) As Double
        Dim v As JsonElement
        Return If(e.TryGetProperty(key, v) AndAlso v.ValueKind = JsonValueKind.Number, v.GetDouble(), 0)
    End Function

End Module
