' What the face wall shows (new in the .NET port, face recognition P2): the people with their faces
' and the groups of faces nobody is named on yet. Built on the scan thread after each sort
' (FaceLibrary.Catalog); the UI only reads it, and drops a group once it has been named.
Public Class FaceCatalog

    Public Class PersonEntry
        Public PersonID As Integer
        Public Name As String = ""
        Public BirthYear As Integer
        Public Hidden As Boolean
        ''' <summary>Faces confirmed / named by the user or assigned by the program.</summary>
        Public Faces As New List(Of FaceRegion)
        ''' <summary>Faces the program thinks are this person (not confirmed yet).</summary>
        Public Suggested As New List(Of FaceRegion)
        Public Cover As FaceRegion

        ''' <summary>Faces to confirm (✓ / ✕): the program's own matches first, then the suggestions,
        ''' each most alike first.</summary>
        Public ReadOnly Property ToConfirm As List(Of FaceRegion)
            Get
                ' NeedsConfirm on both: faces confirmed / rejected since the last sort drop out at once
                Return Faces.Where(Function(f) f.NeedsConfirm).OrderByDescending(Function(f) f.Similarity).
                       Concat(Suggested.Where(Function(f) f.NeedsConfirm).OrderByDescending(Function(f) f.Similarity)).ToList()
            End Get
        End Property

        ''' <summary>The photos the person is in (distinct files of the named faces).</summary>
        Public ReadOnly Property PhotoCount As Integer
            Get
                Return Faces.Select(Function(f) f.FileName).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            End Get
        End Property
    End Class

    Public ReadOnly Property Persons As New List(Of PersonEntry)
    Public ReadOnly Property Clusters As New List(Of List(Of FaceRegion))

    Public Shared Function Build(ByVal faces As List(Of FaceRegion), ByVal persons As List(Of Database.FacePersonInfo),
                                 ByVal clusters As List(Of List(Of FaceRegion))) As FaceCatalog
        Dim c As New FaceCatalog
        Dim byID As New Dictionary(Of Integer, PersonEntry)
        For Each p In persons
            Dim e As New PersonEntry With {.PersonID = p.PersonID, .Name = p.Name, .BirthYear = p.BirthYear, .Hidden = p.Hidden}
            byID(p.PersonID) = e
        Next
        For Each f In faces
            Dim e As PersonEntry = Nothing
            If f.PersonID = 0 OrElse Not byID.TryGetValue(f.PersonID, e) Then Continue For
            If f.IsNamed Then
                e.Faces.Add(f)
            ElseIf f.State = FaceRegion.enumFaceState.fsSuggested Then
                e.Suggested.Add(f)
            End If
        Next
        For Each p In persons
            Dim e As PersonEntry = byID(p.PersonID)
            If e.Faces.Count = 0 Then Continue For   ' a name nobody's face carries (yet)
            e.Cover = e.Faces.FirstOrDefault(Function(f) f.FaceID = p.CoverFaceID)
            If e.Cover Is Nothing Then
                ' the clearest face the user named, else the program's best
                e.Cover = e.Faces.OrderByDescending(Function(f) If(f.State = FaceRegion.enumFaceState.fsConfirmed, 1, 0)).
                                  ThenByDescending(Function(f) f.Score * f.Box.Width).First()
            End If
            c.Persons.Add(e)
        Next
        c.Persons.Sort(Function(a, b) b.PhotoCount.CompareTo(a.PhotoCount))
        c.Clusters.AddRange(If(clusters, FaceOrganizer.Cluster(faces)))
        Return c
    End Function

    Public Function Person(ByVal intPersonID As Integer) As PersonEntry
        Return Persons.FirstOrDefault(Function(p) p.PersonID = intPersonID)
    End Function

    ''' <summary>The people shown (not hidden).</summary>
    Public ReadOnly Property VisiblePersons As List(Of PersonEntry)
        Get
            Return Persons.Where(Function(p) Not p.Hidden).ToList()
        End Get
    End Property

End Class
