Imports System.IO
Imports System.IO.Compression

' 輸入地點 (new in the .NET port; frmPlacePicker): a place picked by hand -- a 鄉鎮市區 (PlaceNames.
' TownCentres), an attraction (Attractions) or a point on the map -- written to photos:
'   .Exif [Exif]  GPS=lat,lon  Spot=地點  Country=臺灣  City=縣市區  Town=鄉鎮
'   PhotoIndex    the photo's row again (Database.AddItem: GPS columns and place columns follow)
' By default a photo that already has a GPS position (in its .Exif or the picture's own EXIF, usually
' the camera's) is left alone, and a 地點 already typed is kept; the caller can turn both off. The .Exif
' files about to change are zipped first (Backup), next to the database's other backups.
Public Module PlacePick

    Public Enum enumPickKind
        pkTown = 0
        pkAttraction = 1
        pkMap = 2
    End Enum

    ''' <summary>What goes into the photos.</summary>
    Public Class Choice
        Public Kind As enumPickKind
        Public Spot As String = ""
        Public Country As String = ""
        Public City As String = ""     ' 縣市區
        Public Town As String = ""
        Public Lat As Double
        Public Lon As Double
        ''' <summary>For a map point: the attraction named and how far it is (m); -1 when none.</summary>
        Public NearMeters As Integer = -1

        Public ReadOnly Property Gps As String
            Get
                Return PlaceNames.FormatGps(Lat, Lon)
            End Get
        End Property
    End Class

    Public Function FromTown(ByVal t As PlaceNames.TownPoint) As Choice
        Return New Choice With {.Kind = enumPickKind.pkTown, .Spot = t.Name, .Country = "臺灣", .City = t.Name, .Town = t.Town, .Lat = t.Lat, .Lon = t.Lon}
    End Function

    ''' <summary>An attraction: its name as 地點; 縣市區 from where it really is (PlaceNames, as the automatic
    ''' fill does), else the attraction data's own address.</summary>
    Public Function FromAttraction(ByVal a As Attractions.Attraction) As Choice
        Dim c As New Choice With {.Kind = enumPickKind.pkAttraction, .Spot = a.Name, .Lat = a.Lat, .Lon = a.Lon}
        Dim info As PlaceNames.PlaceInfo = PlaceNames.Resolve(c.Gps)
        c.City = If(info.City <> "", info.City, a.City & a.Town)
        c.Town = If(info.Town <> "", info.Town, a.Town)
        c.Country = If(info.Country <> "", info.Country, "臺灣")
        Return c
    End Function

    ''' <summary>A point on the map: the attraction within 200 m as 地點, else the 縣市區 (PlaceNames.Resolve).</summary>
    Public Function FromPoint(ByVal lat As Double, ByVal lon As Double) As Choice
        Dim c As New Choice With {.Kind = enumPickKind.pkMap, .Lat = lat, .Lon = lon}
        Dim info As PlaceNames.PlaceInfo = PlaceNames.Resolve(c.Gps)
        c.Spot = info.Spot : c.Country = info.Country : c.City = info.City : c.Town = info.Town
        Dim a As Attractions.Attraction = Attractions.Nearest(lat, lon)
        If a IsNot Nothing AndAlso a.Name = info.Spot Then c.NearMeters = CInt(ShotInfo.DistanceKm(lat, lon, a.Lat, a.Lon) * 1000)
        Return c
    End Function

    '==================================================================================================
    ' Writing
    '==================================================================================================
    ''' <summary>True when the photo already has a position: its .Exif GPS, or the picture's own EXIF.</summary>
    Public Function HasGps(ByVal strFile As String) As Boolean
        Dim exif As String = Path.ChangeExtension(strFile, gc_strExifPattern)
        If File.Exists(exif) Then
            Dim lat, lon As Double
            If PlaceNames.TryParseGps(GetClearText(New Carbon.IniFile With {.FileName = exif}.SimpleGetValue("Exif", "GPS")), lat, lon) Then Return True
        End If
        Return PlaceNames.GpsOfPicture(strFile) <> ""
    End Function

    Public Class Result
        Public Written As Integer
        Public SkippedGps As Integer
        Public KeptSpot As Integer
        Public ReadOnly Failed As New List(Of String)
        Public BackupZip As String = ""
    End Class

    ''' <summary>Writes <paramref name="c"/> into <paramref name="files"/> (photos or videos). A photo with a
    ''' GPS position is skipped unless <paramref name="overwriteGps"/>; a 地點 already there is kept unless
    ''' <paramref name="overwriteSpot"/> (the GPS, country, 縣市區 and 鄉鎮 are written all the same: they
    ''' describe the position). <paramref name="backupFolder"/> (optional): where the .Exif files are
    ''' zipped first; nothing is written when that fails.</summary>
    Public Function Apply(ByVal files As IEnumerable(Of String), ByVal c As Choice, ByVal overwriteGps As Boolean, ByVal overwriteSpot As Boolean,
                          ByVal db As Database, Optional ByVal backupFolder As String = "") As Result
        Dim r As New Result
        Dim todo As New List(Of String)
        For Each f In files.Distinct(StringComparer.OrdinalIgnoreCase)
            If Not File.Exists(f) Then
                r.Failed.Add(f)
            ElseIf Not overwriteGps AndAlso HasGps(f) Then
                r.SkippedGps += 1
            Else
                todo.Add(f)
            End If
        Next
        If todo.Count = 0 Then Return r
        If backupFolder <> "" Then
            r.BackupZip = Backup(todo, backupFolder)
            If r.BackupZip Is Nothing Then
                r.Failed.AddRange(todo)
                r.BackupZip = ""
                Return r
            End If
        End If
        Dim fs As New Carbon.FileSystem
        For Each f In todo
            Try
                Dim exif As String = Path.ChangeExtension(f, gc_strExifPattern)
                If Not File.Exists(exif) Then
                    Dim created As Boolean
                    ExifRegen.RegenerateOne(f, fs, created)   ' a whole .Exif (dates and all), as an import makes
                End If
                Dim p As New Photo
                p.Construct(f)
                p.Exif(enumPhotoExif.peGps) = c.Gps
                p.Exif(enumPhotoExif.peCountry) = c.Country
                p.Exif(enumPhotoExif.peCity) = c.City
                p.Exif(enumPhotoExif.peTown) = c.Town
                If overwriteSpot OrElse p.Exif(enumPhotoExif.peSpot).Trim() = "" Then
                    p.Exif(enumPhotoExif.peSpot) = c.Spot
                Else
                    r.KeptSpot += 1
                End If
                If db IsNot Nothing AndAlso db.Implement Then db.AddItem(p)
                r.Written += 1
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                r.Failed.Add(f)
            End Try
        Next
        Return r
    End Function

    ''' <summary>The .Exif files of <paramref name="files"/> (those that exist) zipped into
    ''' <paramref name="folder"/>\Exif_Place_yyyyMMdd_HHmmss.zip, each under its full path; "" when there was
    ''' nothing to keep, Nothing when it failed.</summary>
    Public Function Backup(ByVal files As IEnumerable(Of String), ByVal folder As String) As String
        Dim exifs = files.Select(Function(f) Path.ChangeExtension(f, gc_strExifPattern)).Where(Function(x) File.Exists(x)).ToList()
        If exifs.Count = 0 Then Return ""
        Try
            Directory.CreateDirectory(folder)
            Dim zip As String = Path.Combine(folder, "Exif_Place_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".zip")
            Dim n As Integer = 1
            While File.Exists(zip)
                n += 1
                zip = Path.Combine(folder, "Exif_Place_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & "_" & n & ".zip")
            End While
            Using z As ZipArchive = ZipFile.Open(zip, ZipArchiveMode.Create)
                For Each x In exifs
                    z.CreateEntryFromFile(x, x.Replace(":", "").TrimStart("\"c))
                Next
            End Using
            Return zip
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

End Module
