Imports System.Text.RegularExpressions

' 搜尋 (new in the .NET port; replaces VB6's frmSearch / frmSearchResult): one box, every word must match.
' A word is looked for in every text the photo has -- 主題, 人物 (the people field and the names given to
' its faces), 地點 (spot, county, town, country), 備註, 關鍵字, the file name and the album's name.
' Words that are not text:
'   2024  2024/3  2024-03-15  2024年3月  20240315    the date (year / month / day)
'   3月                                              that month, any year
'   今年  去年                                        this / last year
'   春天 夏天 秋天 冬天                               the months of the season (春 3-5, 夏 6-8, 秋 9-11, 冬 12-2)
'   ★4  ★★★★  4星  4顆星                            rated at least that
'   -詞                                              not containing it
'   "兩個 詞"                                         the words together
' 進階 adds what a word can't say: a date range, ratings, photos / videos, with or without a GPS position
' or people, only the Dock's photos, one album section or photo book, and the old field-by-field boxes.
' Only reads; Records comes from Database.SearchRecords.
Public Module PhotoSearch

    ''' <summary>One indexed photo and everything a word can match.</summary>
    Public Class Record
        Public Row As Database.IndexRow
        Public Title As String = ""
        Public People As String = ""
        ''' <summary>The names given to the photo's faces (face recognition).</summary>
        Public FaceNames As String = ""   ' comma-separated, as the people field
        Public Place As String = ""
        Public Remark As String = ""
        Public KeyWord As String = ""
        Public ReadOnly Property File As String
            Get
                Return Row.FileName
            End Get
        End Property
        Public ReadOnly Property HasGps As Boolean
            Get
                Return Row.Latitude.HasValue AndAlso Row.Longitude.HasValue
            End Get
        End Property
        Public ReadOnly Property HasPeople As Boolean
            Get
                Return People.Trim() <> "" OrElse FaceNames <> ""
            End Get
        End Property
    End Class

    Public Enum Presence
        Any
        Yes
        No
    End Enum

    ''' <summary>What to look for: the box's words (Parse) and 進階's choices.</summary>
    Public Class Query
        Public ReadOnly Terms As New List(Of String)
        Public ReadOnly Excludes As New List(Of String)
        ''' <summary>The dates the words named (yyyyMMdd prefixes: "2024", "202403", "20240315"); any of them.</summary>
        Public ReadOnly DatePrefixes As New List(Of String)
        ''' <summary>Months of any year (3月, a season); empty = any.</summary>
        Public ReadOnly Months As New HashSet(Of Integer)
        Public MinRanking As Integer
        ' 進階
        Public DateFrom As String = ""   ' yyyyMMdd, "" = open
        Public DateTo As String = ""
        ''' <summary>0 all, 1 pictures, 2 videos.</summary>
        Public Media As Integer
        Public Gps As Presence
        Public Persons As Presence
        ''' <summary>Only these files (the Dock, a photo book); Nothing = any.</summary>
        Public Within As HashSet(Of String)
        ''' <summary>Only files under this folder (an album section, an album); Nothing = any.</summary>
        Public Under As String
        Public FieldTitle As String = "", FieldPeople As String = "", FieldPlace As String = "", FieldRemark As String = "", FieldKeyWord As String = ""

        ''' <summary>True when nothing at all narrows the search.</summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Terms.Count = 0 AndAlso Excludes.Count = 0 AndAlso DatePrefixes.Count = 0 AndAlso Months.Count = 0 AndAlso
                       MinRanking = 0 AndAlso DateFrom = "" AndAlso DateTo = "" AndAlso Media = 0 AndAlso Gps = Presence.Any AndAlso
                       Persons = Presence.Any AndAlso Within Is Nothing AndAlso Under Is Nothing AndAlso
                       {FieldTitle, FieldPeople, FieldPlace, FieldRemark, FieldKeyWord}.All(Function(f) f.Trim() = "")
            End Get
        End Property
    End Class

    Private ReadOnly Seasons As New Dictionary(Of String, Integer()) From {
        {"春天", {3, 4, 5}}, {"春季", {3, 4, 5}}, {"夏天", {6, 7, 8}}, {"夏季", {6, 7, 8}},
        {"秋天", {9, 10, 11}}, {"秋季", {9, 10, 11}}, {"冬天", {12, 1, 2}}, {"冬季", {12, 1, 2}}}
    ''' <summary>2024, 2024/3, 2024-3-15 (年 / 月 / 日 are turned into / first).</summary>
    Private ReadOnly YmdPattern As New Regex("^(?<y>(19|20)\d\d)(?:[/\-.](?<m>\d{1,2})(?:[/\-.](?<d>\d{1,2}))?)?$")
    Private ReadOnly CompactDate As New Regex("^(?<y>(19|20)\d\d)(?<m>\d\d)(?<d>\d\d)$")
    Private ReadOnly MonthOnly As New Regex("^(?<m>\d{1,2})月$")
    Private ReadOnly StarsPattern As New Regex("^(?:★+|★(?<n>[1-5])|(?<n>[1-5])(?:顆)?星)$")

    ''' <summary>The box's words into <paramref name="q"/> (a new Query when Nothing); <paramref name="today"/>
    ''' for 今年 / 去年.</summary>
    Public Function Parse(ByVal text As String, ByVal today As Date, Optional ByVal q As Query = Nothing) As Query
        If q Is Nothing Then q = New Query
        For Each word In Words(text)
            Dim exclude As Boolean = word.Length > 1 AndAlso word.StartsWith("-")
            Dim w As String = If(exclude, word.Substring(1), word)
            If exclude Then
                q.Excludes.Add(w)
                Continue For
            End If
            Dim m As Match = CompactDate.Match(w)
            If Not m.Success Then m = YmdPattern.Match(w.Replace("年", "/").Replace("月", "/").TrimEnd("日"c).TrimEnd("/"c))
            If m.Success Then
                Dim y As String = m.Groups("y").Value
                Dim mo As Integer = If(m.Groups("m").Success, CInt(m.Groups("m").Value), 0)
                Dim d As Integer = If(m.Groups("d").Success, CInt(m.Groups("d").Value), 0)
                If mo <= 12 AndAlso d <= 31 Then
                    q.DatePrefixes.Add(y & If(mo > 0, mo.ToString("00"), "") & If(mo > 0 AndAlso d > 0, d.ToString("00"), ""))
                    Continue For
                End If
            End If
            m = MonthOnly.Match(w)
            If m.Success AndAlso CInt(m.Groups("m").Value) >= 1 AndAlso CInt(m.Groups("m").Value) <= 12 Then
                q.Months.Add(CInt(m.Groups("m").Value))
                Continue For
            End If
            If w = "今年" Then q.DatePrefixes.Add(today.Year.ToString("0000")) : Continue For
            If w = "去年" Then q.DatePrefixes.Add((today.Year - 1).ToString("0000")) : Continue For
            If Seasons.ContainsKey(w) Then
                q.Months.UnionWith(Seasons(w))
                Continue For
            End If
            m = StarsPattern.Match(w)
            If m.Success Then
                q.MinRanking = Math.Max(q.MinRanking, If(m.Groups("n").Success, CInt(m.Groups("n").Value), Math.Min(5, w.Length)))
                Continue For
            End If
            q.Terms.Add(w)
        Next
        Return q
    End Function

    ''' <summary>The words of <paramref name="text"/>: split at spaces (also full-width), "quoted words" kept together.</summary>
    Public Function Words(ByVal text As String) As List(Of String)
        Dim result As New List(Of String)
        For Each m As Match In Regex.Matches(If(text, "").Replace("　"c, " "c).Replace(ChrW(&H201C), """"c).Replace(ChrW(&H201D), """"c), """([^""]*)""|(\S+)")
            Dim w As String = If(m.Groups(1).Success, m.Groups(1).Value, m.Groups(2).Value).Trim()
            If w <> "" Then result.Add(w)
        Next
        Return result
    End Function

    Public Function Matches(ByVal r As Record, ByVal q As Query) As Boolean
        Dim row As Database.IndexRow = r.Row
        ' dates
        If q.DatePrefixes.Count > 0 AndAlso Not q.DatePrefixes.Any(Function(p) row.DateText.StartsWith(p)) Then Return False
        If q.Months.Count > 0 AndAlso Not q.Months.Contains(row.Month) Then Return False
        If q.DateFrom <> "" AndAlso String.CompareOrdinal(row.DateText, q.DateFrom) < 0 Then Return False
        If q.DateTo <> "" AndAlso String.CompareOrdinal(row.DateText, q.DateTo) > 0 Then Return False
        If (q.DateFrom <> "" OrElse q.DateTo <> "" OrElse q.DatePrefixes.Count > 0 OrElse q.Months.Count > 0) AndAlso Not row.ShotDate.HasValue Then Return False
        ' choices
        If q.MinRanking > 0 AndAlso row.Ranking < q.MinRanking Then Return False
        If q.Media <> 0 Then
            Dim t As enumPhotoMediaType = GetMediaType(Nothing, row.FileName)
            If q.Media = 1 AndAlso t <> enumPhotoMediaType.mdImage Then Return False
            If q.Media = 2 AndAlso t <> enumPhotoMediaType.mdVideo Then Return False
        End If
        If q.Gps <> Presence.Any AndAlso r.HasGps <> (q.Gps = Presence.Yes) Then Return False
        If q.Persons <> Presence.Any AndAlso r.HasPeople <> (q.Persons = Presence.Yes) Then Return False
        If q.Within IsNot Nothing AndAlso Not q.Within.Contains(row.FileName) Then Return False
        If q.Under IsNot Nothing AndAlso Not row.FileName.StartsWith(q.Under.TrimEnd("\"c) & "\", StringComparison.OrdinalIgnoreCase) Then Return False
        ' the field boxes
        If Not Has(r.Title, q.FieldTitle) OrElse Not Has(r.People & " " & r.FaceNames, q.FieldPeople) OrElse
           Not Has(r.Place, q.FieldPlace) OrElse Not Has(r.Remark, q.FieldRemark) OrElse Not Has(r.KeyWord, q.FieldKeyWord) Then Return False
        ' the words
        If q.Terms.Count > 0 OrElse q.Excludes.Count > 0 Then
            Dim all As String = AllText(r)
            For Each t In q.Terms
                If all.IndexOf(t, StringComparison.CurrentCultureIgnoreCase) < 0 Then Return False
            Next
            For Each t In q.Excludes
                If all.IndexOf(t, StringComparison.CurrentCultureIgnoreCase) >= 0 Then Return False
            Next
        End If
        Return True
    End Function

    Private Function Has(ByVal value As String, ByVal want As String) As Boolean
        If String.IsNullOrWhiteSpace(want) Then Return True
        Return If(value, "").IndexOf(want.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0
    End Function

    ''' <summary>Every text of the photo in one string (a word may be in any of them).</summary>
    Public Function AllText(ByVal r As Record) As String
        Dim file As String = r.Row.FileName
        Dim folder As String = IO.Path.GetFileName(IO.Path.GetDirectoryName(file))
        Return String.Join(ChrW(1), {r.Title, r.People, r.FaceNames, r.Place, r.Remark, r.KeyWord, IO.Path.GetFileName(file), folder})
    End Function

    ''' <summary>The photos of <paramref name="records"/> that match, by date (no date last).</summary>
    Public Function Find(ByVal records As IEnumerable(Of Record), ByVal q As Query) As List(Of Record)
        Return records.Where(Function(r) Matches(r, q)).
                       OrderBy(Function(r) If(r.Row.ShotDate.HasValue, 0, 1)).ThenBy(Function(r) r.Row.DateText).
                       ThenBy(Function(r) r.Row.FileName, StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    ''' <summary>The words to suggest for <paramref name="prefix"/> (people, places, keywords, albums), best first.</summary>
    Public Function Suggest(ByVal vocabulary As IEnumerable(Of String), ByVal prefix As String, ByVal max As Integer) As List(Of String)
        If String.IsNullOrWhiteSpace(prefix) Then Return New List(Of String)
        Dim p As String = prefix.Trim()
        Return vocabulary.Where(Function(v) v.IndexOf(p, StringComparison.CurrentCultureIgnoreCase) >= 0 AndAlso
                                            Not String.Equals(v, p, StringComparison.CurrentCultureIgnoreCase)).
                          OrderBy(Function(v) If(v.StartsWith(p, StringComparison.CurrentCultureIgnoreCase), 0, 1)).
                          ThenBy(Function(v) v.Length).ThenBy(Function(v) v, StringComparer.CurrentCulture).
                          Take(max).ToList()
    End Function

    ''' <summary>The words the records hold, for Suggest: people (field and faces), places, keywords.</summary>
    Public Function Vocabulary(ByVal records As IEnumerable(Of Record)) As List(Of String)
        Dim words As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
        For Each r In records
            For Each n In FaceNames.Split(r.People).Concat(FaceNames.Split(r.FaceNames))
                words.Add(n)
            Next
            For Each part In {r.Row.Spot, r.Row.City, r.Row.Town}
                If part <> "" Then words.Add(part)
            Next
            For Each k In r.KeyWord.Split({" "c, ","c, "，"c, "、"c, ";"c, "；"c, "　"c}, StringSplitOptions.RemoveEmptyEntries)
                words.Add(k.Trim())
            Next
        Next
        ' names typed with other separators ("甲;乙", "甲/乙") are suggested one by one
        Return words.SelectMany(Function(w) w.Split({";"c, "；"c, "/"c, "／"c}, StringSplitOptions.RemoveEmptyEntries)).
                     Select(Function(w) w.Trim()).Where(Function(w) w <> "").Distinct(StringComparer.CurrentCultureIgnoreCase).ToList()
    End Function

End Module
