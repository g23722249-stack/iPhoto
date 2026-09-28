' Sorts the faces of the library into people (new in the .NET port, face recognition P2). Runs on the
' FaceLibrary scan thread with that thread's own Database, after the photos are analysed:
'
'   1. seeds    -- a photo with exactly one face whose people field holds exactly one name: that face
'                  is the person (State auto). The field is read from the photo's .Exif file (PhotoIndex
'                  doesn't list every photo). The people field is the user's own record of who is in
'                  the photo, so this is where recognition starts without asking anything.
'   2. outliers -- a seeded face far from the rest of its person (cosine < OutlierCosine to the others'
'                  average, persons with 5+ faces) goes back to unnamed: the one face found was often
'                  somebody else in the picture.
'   3. templates-- for each person, the average feature of each age bucket (FaceTemplate), from the
'                  ANCHORS only: seeds and faces the user confirmed. Faces the program matched never feed
'                  the templates (they snowballed: one run fed the next) and are matched again each run. Small
'                  children change quickly, adults slowly, so buckets are 1 / 2 / 3 / 10 years by age
'                  when the birth year is known, 5 years of the calendar otherwise.
'   4. matching -- every other face against every person: the best template of the person, a
'                  little less for buckets far from the photo's year. Auto when the best person scores
'                  >= AutoCosine and beats the second by AutoMargin, suggested ("這是 X 嗎？") from
'                  SuggestCosine up. Measured on the whole library (27,325 faces, the people fields as
'                  the answer): 0.60 / 0.08 matches about half the faces, 6% of them wrong -- so the
'                  program's matches are shown but never written into people fields; only the user's
'                  confirmation is. Brothers and sisters look alike, hence the margin.
'   5. clusters -- the faces still unnamed, grouped greedily around running averages (ClusterCosine:
'                  0.55 kept groups 96% one person on this library, 0.45 let one group grow to 1,382
'                  faces of several people); groups of one are left out of the face wall.
' Faces the user confirmed / named / boxed are never changed; FaceReject pairs are never suggested.
Friend Class FaceOrganizer

    Public Const AutoCosine As Single = 0.6F
    ''' <summary>AutoCosine for 設定 › 面孔 寬鬆 / 平衡 / 嚴格 (on this library: 61% / 51% / 44% of the
    ''' faces matched, 7% / 6% / 6% of those wrong).</summary>
    Public Shared ReadOnly AutoCosineByStrictness As Single() = {0.55F, 0.6F, 0.65F}
    Public Const AutoMargin As Single = 0.08F
    Public Const SuggestCosine As Single = 0.4F
    Public Const OutlierCosine As Single = 0.2F
    Public Const ClusterCosine As Single = 0.55F
    Private Const YearPenalty As Single = 0.01F      ' per year between the photo and the bucket
    Private Const MaxYearPenalty As Single = 0.1F

    Public Class Result
        Public Assignments As New List(Of Database.FaceAssignment)
        Public Templates As New List(Of Database.FaceTemplateRow)
        Public Clusters As New List(Of List(Of FaceRegion))
    End Class

    ''' <summary>Works out the new state of every face. <paramref name="ensurePerson"/> returns the ID of
    ''' a name (adding the person when new); <paramref name="faces"/> is updated in place.</summary>
    Public Shared Function Organize(ByVal faces As List(Of FaceRegion),
                                    ByVal persons As List(Of Database.FacePersonInfo),
                                    ByVal characterOf As Func(Of String, String),
                                    ByVal rejects As HashSet(Of String),
                                    ByVal ensurePerson As Func(Of String, Integer),
                                    Optional ByVal autoThreshold As Single = AutoCosine) As Result
        Dim result As New Result
        Dim before As Dictionary(Of Integer, (PersonID As Integer, State As FaceRegion.enumFaceState)) =
            faces.ToDictionary(Function(f) f.FaceID, Function(f) (f.PersonID, f.State))
        Dim usable = faces.Where(Function(f) f.Feature IsNot Nothing AndAlso f.Feature.Length = Quartz.FaceEngine.FeatureLength).ToList()

        ' the program's own matches and suggestions are recomputed from scratch
        For Each f In usable.Where(Function(x) x.State = FaceRegion.enumFaceState.fsSuggested OrElse (x.State = FaceRegion.enumFaceState.fsAuto AndAlso Not IsSeed(x)))
            f.PersonID = 0
            f.State = FaceRegion.enumFaceState.fsUnnamed
        Next

        ' 1. seeds
        Dim byPhoto = usable.GroupBy(Function(f) f.FileName, StringComparer.OrdinalIgnoreCase)
        For Each g In byPhoto
            If g.Count() <> 1 Then Continue For
            Dim f As FaceRegion = g.First()
            If f.State <> FaceRegion.enumFaceState.fsUnnamed Then Continue For
            Dim seedNames() As String = FaceNames.Split(characterOf(f.FileName))
            If seedNames.Length <> 1 OrElse Not CheckNameRule(seedNames(0)) Then Continue For
            Dim id As Integer = ensurePerson(seedNames(0))
            If id = 0 OrElse rejects.Contains(f.FaceID & "|" & id) Then Continue For
            f.PersonID = id
            f.PersonName = seedNames(0)
            f.State = FaceRegion.enumFaceState.fsAuto
            f.Similarity = 1
        Next

        ' 2. outliers among the seeds
        For Each g In usable.Where(Function(f) IsAnchor(f)).GroupBy(Function(f) f.PersonID)
            If g.Count() < 5 Then Continue For
            Dim all As Single() = Average(g.Select(Function(f) f.Feature))
            For Each f In g.Where(Function(x) IsSeed(x)).ToList()
                If Quartz.FaceEngine.Cosine(f.Feature, all) < OutlierCosine Then
                    f.PersonID = 0
                    f.PersonName = ""
                    f.State = FaceRegion.enumFaceState.fsUnnamed
                End If
            Next
        Next

        ' 3. templates (anchors only)
        Dim birth As Dictionary(Of Integer, Integer) = persons.ToDictionary(Function(p) p.PersonID, Function(p) p.BirthYear)
        Dim templates As New Dictionary(Of Integer, List(Of Database.FaceTemplateRow))
        For Each g In usable.Where(Function(f) IsAnchor(f)).GroupBy(Function(f) f.PersonID)
            Dim b As Integer = 0
            birth.TryGetValue(g.Key, b)
            Dim list As New List(Of Database.FaceTemplateRow)
            For Each grp In g.GroupBy(Function(f) Bucket(f.ShotYear, b))
                list.Add(New Database.FaceTemplateRow With {
                    .PersonID = g.Key, .YearFrom = grp.Key.Item1, .YearTo = grp.Key.Item2,
                    .Feature = Average(grp.Select(Function(f) f.Feature)), .FaceCount = grp.Count()})
            Next
            templates(g.Key) = list
            result.Templates.AddRange(list)
        Next
        Dim hidden As HashSet(Of Integer) = persons.Where(Function(p) p.Hidden).Select(Function(p) p.PersonID).ToHashSet()
        Dim names As Dictionary(Of Integer, String) = persons.ToDictionary(Function(p) p.PersonID, Function(p) p.Name)
        For Each f In usable.Where(Function(x) x.IsNamed AndAlso x.PersonName <> "")
            names(f.PersonID) = f.PersonName   ' people added by this run's seeds
        Next

        ' 4. matching (faces boxed by hand too, while nobody is assigned to them)
        For Each f In usable.Where(Function(x) x.State = FaceRegion.enumFaceState.fsUnnamed OrElse x.State = FaceRegion.enumFaceState.fsManual)
            Dim wasState As FaceRegion.enumFaceState = f.State
            Dim best As Integer = 0, bestS As Single = -1, secondS As Single = -1
            For Each kv In templates
                If hidden.Contains(kv.Key) OrElse rejects.Contains(f.FaceID & "|" & kv.Key) Then Continue For
                Dim s As Single = -1
                For Each t In kv.Value
                    Dim gap As Integer = If(f.ShotYear = 0, 0, Math.Max(0, Math.Max(t.YearFrom - f.ShotYear, f.ShotYear - t.YearTo)))
                    s = Math.Max(s, Quartz.FaceEngine.Cosine(f.Feature, t.Feature) - Math.Min(MaxYearPenalty, YearPenalty * gap))
                Next
                If s > bestS Then
                    secondS = bestS : bestS = s : best = kv.Key
                ElseIf s > secondS Then
                    secondS = s
                End If
            Next
            If best = 0 Then Continue For
            If bestS >= autoThreshold AndAlso bestS - secondS >= AutoMargin Then
                f.PersonID = best
                f.PersonName = If(names.ContainsKey(best), names(best), "")
                f.State = FaceRegion.enumFaceState.fsAuto
                f.Similarity = bestS
            ElseIf bestS >= SuggestCosine AndAlso wasState = FaceRegion.enumFaceState.fsUnnamed Then   ' a hand-boxed face stays manual
                f.PersonID = best
                f.PersonName = If(names.ContainsKey(best), names(best), "")
                f.State = FaceRegion.enumFaceState.fsSuggested
                f.Similarity = bestS
            End If
        Next

        ' what changed
        For Each f In usable
            Dim was = before(f.FaceID)
            If was.PersonID <> f.PersonID OrElse was.State <> f.State Then
                result.Assignments.Add(New Database.FaceAssignment With {
                    .FaceID = f.FaceID, .ExpectedState = was.State, .PersonID = f.PersonID, .State = f.State, .Similarity = f.Similarity})
            End If
        Next

        ' 5. clusters of the faces nobody is named or suggested for
        result.Clusters = Cluster(usable)
        Return result
    End Function

    ''' <summary>The faces nobody is named on, grouped (groups of 2 or more, biggest first).</summary>
    Public Shared Function Cluster(ByVal faces As IEnumerable(Of FaceRegion)) As List(Of List(Of FaceRegion))
        ' only faces nobody is even suggested for: a suggested face is reviewed under its person
        ' (「確認更多照片」), not in an unnamed group as well; nor faces the user doesn't know (我不認識)
        Dim loose = faces.Where(Function(f) f.PersonID = 0 AndAlso f.Feature IsNot Nothing AndAlso f.State <> FaceRegion.enumFaceState.fsStranger).
                          OrderByDescending(Function(f) f.Score).ToList()
        Dim centres As New List(Of Single())
        Dim sums As New List(Of Single())   ' running sum of each group's features
        Dim members As New List(Of List(Of FaceRegion))
        For Each f In loose
            Dim bestC As Integer = -1, bestS As Single = ClusterCosine
            For c = 0 To centres.Count - 1
                Dim s As Single = Quartz.FaceEngine.Cosine(f.Feature, centres(c))
                If s >= bestS Then bestS = s : bestC = c
            Next
            If bestC < 0 Then
                centres.Add(CType(f.Feature.Clone(), Single()))
                sums.Add(CType(f.Feature.Clone(), Single()))
                members.Add(New List(Of FaceRegion) From {f})
            Else
                members(bestC).Add(f)
                Dim sum As Single() = sums(bestC)
                For k = 0 To sum.Length - 1
                    sum(k) += f.Feature(k)
                Next
                centres(bestC) = Average({sum})
            End If
        Next
        Return members.Where(Function(m) m.Count >= 2).OrderByDescending(Function(m) m.Count).ToList()
    End Function

    ''' <summary>A face the program took from the people field (one face, one name): Similarity 1.</summary>
    Public Shared Function IsSeed(ByVal f As FaceRegion) As Boolean
        Return f.State = FaceRegion.enumFaceState.fsAuto AndAlso f.Similarity >= 0.999F
    End Function

    ''' <summary>A face the templates are built from: a seed, or a face the user confirmed / named.</summary>
    Public Shared Function IsAnchor(ByVal f As FaceRegion) As Boolean
        Return f.PersonID <> 0 AndAlso (f.State = FaceRegion.enumFaceState.fsConfirmed OrElse IsSeed(f))
    End Function

    ''' <summary>(first year, last year) of the age bucket of a photo from <paramref name="shotYear"/>.</summary>
    Public Shared Function Bucket(ByVal shotYear As Integer, ByVal birthYear As Integer) As Tuple(Of Integer, Integer)
        If shotYear <= 0 Then Return Tuple.Create(0, 0)
        If birthYear <= 0 OrElse birthYear > shotYear Then
            Dim y0 As Integer = shotYear - shotYear Mod 5
            Return Tuple.Create(y0, y0 + 4)
        End If
        Dim age As Integer = shotYear - birthYear
        Dim a0 As Integer, len As Integer
        If age <= 2 Then
            a0 = age : len = 1
        ElseIf age <= 12 Then
            a0 = 3 + ((age - 3) \ 2) * 2 : len = 2
        ElseIf age <= 20 Then
            a0 = 13 + ((age - 13) \ 3) * 3 : len = 3
        Else
            a0 = 21 + ((age - 21) \ 10) * 10 : len = 10
        End If
        Return Tuple.Create(birthYear + a0, birthYear + a0 + len - 1)
    End Function

    ''' <summary>L2-normalised average of features.</summary>
    Public Shared Function Average(ByVal features As IEnumerable(Of Single())) As Single()
        Dim c(Quartz.FaceEngine.FeatureLength - 1) As Single
        For Each f In features
            For k = 0 To c.Length - 1
                c(k) += f(k)
            Next
        Next
        Dim norm As Double = Math.Sqrt(c.Sum(Function(x) CDbl(x) * x))
        If norm > 0 Then
            For k = 0 To c.Length - 1
                c(k) = CSng(c(k) / norm)
            Next
        End If
        Return c
    End Function

End Class
