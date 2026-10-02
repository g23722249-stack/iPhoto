Imports System.IO
Imports System.Security.Cryptography

' 輸入照片 (frmImportStudio): reading the source files. Everything here only reads and may run on a
' worker thread.
'   ListFiles      -- the photos / videos of a folder (the same types the old window listed);
'   ReadInfo       -- date, GPS and place, size of one file;
'   LibraryIndex   -- the photos already in the albums, by size, to find files imported before
'                     (same size, then the same SHA-256: byte for byte the same file);
'   SuggestFolder  -- the album folder / title / date from the photos' dates ("09月27日", "09月27日–28日").
Public Module ImportSource

    Public Function ListFiles(ByVal strFolder As String) As List(Of String)
        Dim files() As String = Nothing
        ExactFiles(strFolder, GetPhotoPatterns(), files, True)
        Return files.ToList()
    End Function

    ''' <summary>Fills the item's size, date, GPS and place (a file that can't be read keeps what it has).</summary>
    Public Sub ReadInfo(ByVal item As ImportItem, ByVal fs As Carbon.FileSystem)
        Try
            item.Size = New FileInfo(item.SourceFile).Length
            Dim type As enumPhotoMediaType = GetMediaType(fs, item.SourceFile)
            item.IsVideo = type = enumPhotoMediaType.mdVideo
            item.IsPicture = FaceLibrary.IsPicture(item.SourceFile)
            item.ShotDateTime = GetExifFileDateTime(fs, item.SourceFile)
            If item.IsPicture Then
                item.Gps = PlaceNames.GpsOfPicture(item.SourceFile)
                If item.Gps <> "" Then
                    Dim place As PlaceNames.PlaceInfo = PlaceNames.Resolve(item.Gps)   ' an attraction within 200 m first
                    item.GpsPlace = place.Spot
                    item.Country = place.Country
                    item.City = place.City
                    item.Town = place.Town
                End If
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException
            ' unreadable: imported as it is (the old window did the same)
        End Try
        item.InfoRead = True
    End Sub

    ''' <summary>"MM月dd日" of a date.</summary>
    Public Function DayText(ByVal d As Date) As String
        Return d.ToString("MM") & "月" & d.ToString("dd") & "日"
    End Function

    ''' <summary>The album's date text for the photos' days: "09月27日", "09月27日–28日", "09月30日–10月02日".</summary>
    Public Function DateRangeText(ByVal days As IEnumerable(Of Date)) As String
        Dim list = days.Select(Function(d) d.Date).Distinct().OrderBy(Function(d) d).ToList()
        If list.Count = 0 Then Return DayText(Date.Today)
        Dim first As Date = list.First(), last As Date = list.Last()
        If first = last Then Return DayText(first)
        If first.Year = last.Year AndAlso first.Month = last.Month Then Return DayText(first) & "–" & last.ToString("dd") & "日"
        Return DayText(first) & "–" & DayText(last)
    End Function

    ''' <summary>The place most of the photos were taken at ("" when none has GPS).</summary>
    Public Function CommonPlace(ByVal items As IEnumerable(Of ImportItem)) As String
        Dim g = items.Where(Function(i) i.GpsPlace <> "").GroupBy(Function(i) i.GpsPlace).OrderByDescending(Function(x) x.Count()).FirstOrDefault()
        Return If(g Is Nothing, "", g.Key)
    End Function

    '==================================================================================================
    ' Photos already in the library
    '==================================================================================================
    Public Class LibraryIndex

        Private ReadOnly m_bySize As New Dictionary(Of Long, List(Of String))
        Private ReadOnly m_hashes As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Lists the albums' photos under <paramref name="roots"/> (a few seconds for 20,000 files).</summary>
        Public Shared Function Build(ByVal roots As IEnumerable(Of String), Optional ByVal cancel As Threading.CancellationToken = Nothing) As LibraryIndex
            Dim index As New LibraryIndex
            For Each f In Duplicates.AlbumPhotos(roots)
                If cancel.IsCancellationRequested Then Exit For
                Try
                    Dim len As Long = New FileInfo(f).Length
                    Dim list As List(Of String) = Nothing
                    If Not index.m_bySize.TryGetValue(len, list) Then
                        list = New List(Of String)
                        index.m_bySize(len) = list
                    End If
                    list.Add(f)
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                End Try
            Next
            Return index
        End Function

        ''' <summary>The library photo that is the same file as <paramref name="strFile"/>, or "".</summary>
        Public Function Find(ByVal strFile As String, ByVal lngSize As Long) As String
            Dim list As List(Of String) = Nothing
            If lngSize <= 0 OrElse Not m_bySize.TryGetValue(lngSize, list) Then Return ""
            Dim h As String = HashOf(strFile)
            If h = "" Then Return ""
            For Each f In list
                If String.Equals(f, strFile, StringComparison.OrdinalIgnoreCase) Then Continue For
                Dim other As String = Nothing
                If Not m_hashes.TryGetValue(f, other) Then
                    other = HashOf(f)
                    m_hashes(f) = other
                End If
                If other = h Then Return f
            Next
            Return ""
        End Function

        Private Shared Function HashOf(ByVal f As String) As String
            Try
                Using s As New FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16), sha As SHA256 = SHA256.Create()
                    Return Convert.ToHexString(sha.ComputeHash(s))
                End Using
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                Return ""
            End Try
        End Function

    End Class

End Module
