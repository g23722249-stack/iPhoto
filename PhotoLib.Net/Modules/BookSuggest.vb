' 建立攝影集 (new in the .NET port): what the photos suggest for a new photo book --
'   name    the dates ("2024年3月", "2024", "2019～2024"), then the place most of them share, then the
'           person most of them show: "2024年3月 臺南市 阿嬤"; only characters a file name may have
'   date    the first and last day: "2024/3/15～2024/3/20" (one day: "2024/3/15")
'   place   the place at least a third of them share (地點, else 縣市)
'   cover   the best rated, then the earliest
' Reads the photos' .Exif (Photo); the caller passes the files.
Public Module BookSuggest

    Public Class Suggestion
        Public Name As String = ""
        Public DateText As String = ""
        Public Place As String = ""
        Public Cover As String = ""
    End Class

    Public Function Suggest(ByVal files As IEnumerable(Of String)) As Suggestion
        Dim photos As New List(Of (File As String, D As Date?, Spot As String, City As String, People As String(), Rank As Integer))
        For Each f In files
            If Not IO.File.Exists(f) Then Continue For
            Dim p As New Photo
            p.Construct(f)
            Dim d As Date
            Dim dt As Date? = If(Date.TryParseExact(PadNum(p.Exif(enumPhotoExif.peDate), 8), "yyyyMMdd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, d), d, CType(Nothing, Date?))
            photos.Add((f, dt, If(p.Exif(enumPhotoExif.peSpot), "").Trim(), If(p.Exif(enumPhotoExif.peCity), "").Trim(),
                        FaceNames.Split(p.Exif(enumPhotoExif.peCharacter)), CInt(Val(p.Exif(enumPhotoExif.peRanking)))))
        Next
        Dim s As New Suggestion
        If photos.Count = 0 Then Return s

        Dim dates = photos.Where(Function(x) x.D.HasValue).Select(Function(x) x.D.Value).OrderBy(Function(x) x).ToList()
        Dim parts As New List(Of String)
        If dates.Count > 0 Then
            Dim a As Date = dates.First(), b As Date = dates.Last()
            If a.Year = b.Year AndAlso a.Month = b.Month Then
                parts.Add($"{a.Year}年{a.Month}月")
            ElseIf a.Year = b.Year Then
                parts.Add($"{a.Year}")
            Else
                parts.Add($"{a.Year}～{b.Year}")
            End If
            s.DateText = If(a = b, $"{a:yyyy/M/d}", $"{a:yyyy/M/d}～{b:yyyy/M/d}")
        End If

        ' the place a third of them share: the spot, else the county / city
        For Each values In {photos.Select(Function(x) x.Spot), photos.Select(Function(x) x.City)}
            Dim top = values.Where(Function(v) v <> "").GroupBy(Function(v) v).OrderByDescending(Function(g) g.Count()).FirstOrDefault()
            If top IsNot Nothing AndAlso top.Count() * 3 >= photos.Count Then
                s.Place = top.Key
                Exit For
            End If
        Next
        If s.Place <> "" Then parts.Add(s.Place)

        ' the person half of them show
        Dim person = photos.SelectMany(Function(x) x.People.Distinct()).GroupBy(Function(n) n).OrderByDescending(Function(g) g.Count()).FirstOrDefault()
        If person IsNot Nothing AndAlso person.Count() * 2 >= photos.Count Then parts.Add(person.Key)

        s.Name = CleanName(String.Join(" ", parts))
        If s.Name = "" Then s.Name = "新的攝影集"
        s.Cover = photos.Where(Function(x) GetMediaType(Nothing, x.File) = enumPhotoMediaType.mdImage).
                         OrderByDescending(Function(x) x.Rank).ThenBy(Function(x) If(x.D, Date.MaxValue)).
                         Select(Function(x) x.File).FirstOrDefault()
        If s.Cover Is Nothing Then s.Cover = ""
        Return s
    End Function

    ''' <summary><paramref name="name"/> without what a file name can't have (CheckNameRule, and the rest
    ''' Windows refuses).</summary>
    Public Function CleanName(ByVal name As String) As String
        Dim bad As Char() = "\/:*?""<>|.".ToCharArray()
        Return New String(If(name, "").Select(Function(c) If(bad.Contains(c), " "c, c)).ToArray()).Trim()
    End Function

End Module
