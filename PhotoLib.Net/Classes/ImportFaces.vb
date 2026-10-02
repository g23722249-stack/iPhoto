' 輸入照片 (frmImportStudio): the faces of the pictures being imported, found before they are copied.
' Each face is matched against the people already in the library (their FaceTemplate averages, the
' same numbers FaceOrganizer uses): sure (>= the 設定 › 面孔 threshold and ahead of the second person by
' AutoMargin) -> ifAuto, from SuggestCosine up -> ifSuggested, else unknown. The unknown faces of the
' whole import are then grouped (ClusterCosine), so one new person is named once (「新面孔」).
' Analyse may run on a worker thread (the engine locks itself); the rest runs on the UI thread.
Public Class ImportFaceMatcher

    Private ReadOnly m_templates As Dictionary(Of Integer, List(Of Single()))
    Private ReadOnly m_names As New Dictionary(Of Integer, String)
    Private ReadOnly m_ids As New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)
    Private ReadOnly m_sngAuto As Single

    ''' <summary>Reads the people and templates through <paramref name="db"/> (UI thread's g_lpDatabase).</summary>
    Public Sub New(ByVal db As Database, ByVal intStrictness As Integer)
        m_sngAuto = FaceOrganizer.AutoCosineByStrictness(Math.Max(0, Math.Min(2, intStrictness)))
        m_templates = If(db IsNot Nothing AndAlso db.FaceTablesReady, db.LoadTemplateFeatures(), New Dictionary(Of Integer, List(Of Single())))
        If db IsNot Nothing AndAlso db.FaceTablesReady Then
            For Each p In db.LoadFacePersons()
                m_names(p.PersonID) = p.Name
                m_ids(p.Name) = p.PersonID
                If p.Hidden Then m_templates.Remove(p.PersonID)
            Next
        End If
        ' templates of people without a name can't be suggested
        For Each id In m_templates.Keys.Where(Function(k) Not m_names.ContainsKey(k)).ToList()
            m_templates.Remove(id)
        Next
    End Sub

    ''' <summary>For tests: templates and names given directly.</summary>
    Public Sub New(ByVal templates As Dictionary(Of Integer, List(Of Single())), ByVal names As Dictionary(Of Integer, String), ByVal sngAuto As Single)
        m_templates = templates
        For Each kv In names
            m_names(kv.Key) = kv.Value
            m_ids(kv.Value) = kv.Key
        Next
        m_sngAuto = sngAuto
    End Sub

    ''' <summary>The known person of a name (0 when the name is new).</summary>
    Public Function PersonOf(ByVal strName As String) As Integer
        Dim id As Integer
        Return If(m_ids.TryGetValue(If(strName, "").Trim(), id), id, 0)
    End Function

    ''' <summary>The known names (for the name box).</summary>
    Public ReadOnly Property Names As IEnumerable(Of String)
        Get
            Return m_names.Values
        End Get
    End Property

    ''' <summary>Finds and matches the faces of <paramref name="item"/> (a picture). Worker thread.</summary>
    Public Sub Analyse(ByVal engine As Quartz.FaceEngine, ByVal item As ImportItem)
        Dim found As List(Of Quartz.FaceEngine.Face) = Nothing
        Try
            found = engine.Analyze(item.SourceFile)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is OpenCvSharp.OpenCVException OrElse TypeOf ex Is ArgumentException
            item.FaceError = ex.Message
        End Try
        Dim list As New List(Of ImportFace)
        If found IsNot Nothing Then
            For Each f In found.OrderBy(Function(x) x.Box.X)
                Dim face As New ImportFace With {.Face = f}
                Match(face)
                list.Add(face)
            Next
        ElseIf item.FaceError Is Nothing Then
            item.FaceError = "無法讀取照片"
        End If
        item.Faces = list   ' last: the UI thread reads Faces IsNot Nothing as "done"
    End Sub

    ''' <summary>A face boxed by hand (<paramref name="box"/> in fractions); Nothing when the picture can't be read.</summary>
    Public Function AddManual(ByVal engine As Quartz.FaceEngine, ByVal item As ImportItem, ByVal box As RectangleF) As ImportFace
        Dim f As Quartz.FaceEngine.Face = engine.AnalyzeRegion(item.SourceFile, box)
        If f Is Nothing Then Return Nothing
        Dim face As New ImportFace With {.Face = f, .Manual = True}
        Match(face)
        ' a hand-boxed face is only ever suggested, never named by itself
        If face.State = ImportFace.enumImportFaceState.ifAuto Then face.State = ImportFace.enumImportFaceState.ifSuggested
        If item.Faces Is Nothing Then item.Faces = New List(Of ImportFace)
        item.Faces.Add(face)
        Return face
    End Function

    ''' <summary>Best person for the face (not one the user rejected).</summary>
    Public Sub Match(ByVal face As ImportFace)
        face.Clear()
        Dim feature As Single() = face.Face?.Feature
        If feature Is Nothing OrElse feature.Length <> Quartz.FaceEngine.FeatureLength Then Return
        Dim best As Integer = 0, bestS As Single = -1, secondS As Single = -1
        For Each kv In m_templates
            If face.Rejected.Contains(kv.Key) Then Continue For
            Dim s As Single = -1
            For Each t In kv.Value
                s = Math.Max(s, Quartz.FaceEngine.Cosine(feature, t))
            Next
            If s > bestS Then
                secondS = bestS : bestS = s : best = kv.Key
            ElseIf s > secondS Then
                secondS = s
            End If
        Next
        If best = 0 Then Return
        If bestS >= m_sngAuto AndAlso bestS - secondS >= FaceOrganizer.AutoMargin Then
            face.State = ImportFace.enumImportFaceState.ifAuto
        ElseIf bestS >= FaceOrganizer.SuggestCosine Then
            face.State = ImportFace.enumImportFaceState.ifSuggested
        Else
            Return
        End If
        face.PersonID = best
        face.Name = m_names(best)
        face.Similarity = bestS
    End Sub

    ''' <summary>Groups the unknown faces of the import (groups of 2 or more get 1, 2, ... biggest first).</summary>
    Public Shared Sub GroupUnknown(ByVal items As IEnumerable(Of ImportItem))
        Dim loose As New List(Of FaceRegion)
        Dim back As New Dictionary(Of FaceRegion, ImportFace)
        For Each it In items
            For Each f In it.VisibleFaces
                f.Group = 0
                If f.State <> ImportFace.enumImportFaceState.ifUnknown OrElse f.Face?.Feature Is Nothing Then Continue For
                Dim r As New FaceRegion With {.Feature = f.Face.Feature, .Score = f.Face.Score, .FileName = it.SourceFile}
                loose.Add(r)
                back(r) = f
            Next
        Next
        Dim n As Integer = 0
        For Each g In FaceOrganizer.Cluster(loose)
            n += 1
            For Each r In g
                back(r).Group = n
            Next
        Next
    End Sub

End Class
