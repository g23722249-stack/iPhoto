' 年度回顧 (new in the .NET port): the photos of a year worth showing together, for PhotoMontage --
'   score    the rating counts most (★ x 3), then named people (up to 3, x 2), then faces at all (up to 4),
'            then having a 地點 (0.5)
'   spread   every month with photos gets a share by the square root of its count (a busy month gets
'            more, a quiet one still one), at least one each while there are picks to give; within a
'            month the days take turns, best first, so one outing doesn't fill the month
'   order    the picks by date
' Pictures only (no videos, nothing that is gone). Only reads; may run on a worker thread.
Public Module YearPicks

    Public Class Candidate
        Public File As String = ""
        ''' <summary>yyyyMMdd.</summary>
        Public DateText As String = ""
        Public Score As Double
    End Class

    Public Function Score(ByVal ranking As Integer, ByVal faces As Integer, ByVal named As Integer, ByVal hasPlace As Boolean) As Double
        Return Math.Max(0, Math.Min(5, ranking)) * 3 + Math.Min(named, 3) * 2 + Math.Min(faces, 4) + If(hasPlace, 0.5, 0)
    End Function

    ''' <summary>Up to <paramref name="count"/> of <paramref name="candidates"/>, spread over the months and
    ''' days, by date.</summary>
    Public Function Pick(ByVal candidates As IEnumerable(Of Candidate), ByVal count As Integer) As List(Of Candidate)
        Dim months = candidates.Where(Function(c) c.DateText.Length = 8).
                                GroupBy(Function(c) c.DateText.Substring(4, 2)).OrderBy(Function(g) g.Key).
                                Select(Function(g) g.ToList()).ToList()
        Dim result As New List(Of Candidate)
        If count <= 0 OrElse months.Count = 0 Then Return result

        ' shares: by the square root of each month's count, largest remainder; at least one each when
        ' there are enough picks for that
        Dim weights() As Double = months.Select(Function(m) Math.Sqrt(m.Count)).ToArray()
        Dim total As Double = weights.Sum()
        Dim shares(months.Count - 1) As Integer
        Dim exact(months.Count - 1) As Double
        For i = 0 To months.Count - 1
            exact(i) = count * weights(i) / total
            shares(i) = Math.Min(months(i).Count, CInt(Math.Floor(exact(i))))
            If count >= months.Count Then shares(i) = Math.Max(1, shares(i))
        Next
        Dim given As Integer = shares.Sum()
        ' too many (the minimum of one): take back from the months furthest above their exact share
        While given > count
            Dim k As Integer = Enumerable.Range(0, months.Count).Where(Function(i) shares(i) > 1).
                               OrderByDescending(Function(i) shares(i) - exact(i)).DefaultIfEmpty(-1).First()
            If k < 0 Then Exit While
            shares(k) -= 1 : given -= 1
        End While
        ' too few: to the months furthest below their exact share that still have photos
        While given < count
            Dim k As Integer = Enumerable.Range(0, months.Count).Where(Function(i) shares(i) < months(i).Count).
                               OrderByDescending(Function(i) exact(i) - shares(i)).DefaultIfEmpty(-1).First()
            If k < 0 Then Exit While
            shares(k) += 1 : given += 1
        End While

        For i = 0 To months.Count - 1
            result.AddRange(PickInMonth(months(i), shares(i)))
        Next
        Return result.OrderBy(Function(c) c.DateText).ThenBy(Function(c) c.File, StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    ''' <summary>The days take turns (the day with the best photo first), each giving its best photo left.</summary>
    Private Function PickInMonth(ByVal month As List(Of Candidate), ByVal share As Integer) As List(Of Candidate)
        Dim days = month.GroupBy(Function(c) c.DateText).
                         Select(Function(g) New Queue(Of Candidate)(g.OrderByDescending(Function(c) c.Score).ThenBy(Function(c) c.File))).
                         OrderByDescending(Function(q) q.Peek().Score).ToList()
        Dim picks As New List(Of Candidate)
        While picks.Count < share AndAlso days.Count > 0
            For Each q In days.ToList()
                If picks.Count >= share Then Exit For
                picks.Add(q.Dequeue())
                If q.Count = 0 Then days.Remove(q)
            Next
        End While
        Return picks
    End Function

End Module
