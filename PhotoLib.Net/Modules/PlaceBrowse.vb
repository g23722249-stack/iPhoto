' The 地點 tree of the main window (new in the .NET port), from PhotoIndex rows (Database.PlaceRows):
'   縣市 › 鄉鎮區 › 景點      from Exif_City (縣市加區, e.g. 臺中市西屯區), Exif_Town (西屯區) and Exif_Spot
'   其他地點 › 地點           a 地點 typed by hand on a photo without GPS (no 縣市區)
' A 地點 that only repeats the 縣市區 (PlaceNames writes 臺中市西屯區 when no attraction is near) is no
' 景點 of its own: those photos show under the 區. Biggest first at every level.
Public Module PlaceBrowse

    Public Const OtherPlaces As String = "其他地點"

    Public Class PlaceNode
        Public Name As String = ""
        ''' <summary>Unique in the tree: the names from the top, joined with "|".</summary>
        Public Path As String = ""
        Public ReadOnly Rows As New List(Of Database.IndexRow)
        Public ReadOnly Children As New List(Of PlaceNode)

        Public Overrides Function ToString() As String
            Return Name & " (" & Rows.Count.ToString("#,0") & ")"
        End Function
    End Class

    ''' <summary>縣市 of a 縣市加區 name: 臺中市西屯區 minus 西屯區 is 臺中市.</summary>
    Public Function CountyOf(ByVal city As String, ByVal town As String) As String
        If town <> "" AndAlso city.Length > town.Length AndAlso city.EndsWith(town, StringComparison.Ordinal) Then Return city.Substring(0, city.Length - town.Length)
        Return city
    End Function

    Public Function Build(ByVal rows As IEnumerable(Of Database.IndexRow)) As List(Of PlaceNode)
        Dim top As New Dictionary(Of String, PlaceNode)(StringComparer.Ordinal)
        Dim other As New PlaceNode With {.Name = OtherPlaces, .Path = OtherPlaces}
        For Each r In rows
            If r.City <> "" Then
                Dim county As PlaceNode = Child(top, CountyOf(r.City, r.Town), "")
                county.Rows.Add(r)
                Dim townName As String = If(r.Town <> "", r.Town, r.City)
                Dim town As PlaceNode = ChildOf(county, townName)
                town.Rows.Add(r)
                If r.Spot <> "" AndAlso r.Spot <> r.City AndAlso r.Spot <> r.Town AndAlso r.Spot <> county.Name Then ChildOf(town, r.Spot).Rows.Add(r)
            ElseIf r.Spot <> "" Then
                other.Rows.Add(r)
                ChildOf(other, r.Spot).Rows.Add(r)
            End If
        Next
        Dim result As List(Of PlaceNode) = top.Values.ToList()
        If other.Rows.Count > 0 Then result.Add(other)
        For Each n In result
            Sort(n)
        Next
        Return result.OrderBy(Function(n) If(n Is other, 1, 0)).ThenByDescending(Function(n) n.Rows.Count).ToList()
    End Function

    Private Function Child(ByVal level As Dictionary(Of String, PlaceNode), ByVal name As String, ByVal parentPath As String) As PlaceNode
        Dim n As PlaceNode = Nothing
        If Not level.TryGetValue(name, n) Then
            n = New PlaceNode With {.Name = name, .Path = If(parentPath = "", name, parentPath & "|" & name)}
            level(name) = n
        End If
        Return n
    End Function

    Private Function ChildOf(ByVal parent As PlaceNode, ByVal name As String) As PlaceNode
        Dim n As PlaceNode = parent.Children.FirstOrDefault(Function(c) c.Name = name)
        If n Is Nothing Then
            n = New PlaceNode With {.Name = name, .Path = parent.Path & "|" & name}
            parent.Children.Add(n)
        End If
        Return n
    End Function

    Private Sub Sort(ByVal n As PlaceNode)
        n.Children.Sort(Function(a, b) If(b.Rows.Count <> a.Rows.Count, b.Rows.Count.CompareTo(a.Rows.Count), String.CompareOrdinal(a.Name, b.Name)))
        For Each c In n.Children
            Sort(c)
        Next
    End Sub

    ''' <summary>The node with <paramref name="path"/>, Nothing when the tree has none (any more).</summary>
    Public Function Find(ByVal nodes As IEnumerable(Of PlaceNode), ByVal path As String) As PlaceNode
        For Each n In nodes
            If n.Path = path Then Return n
            If path.StartsWith(n.Path & "|", StringComparison.Ordinal) Then Return Find(n.Children, path)
        Next
        Return Nothing
    End Function

    ''' <summary>The place a group of photos was taken at, for a heading: the most common 地點, else 縣市區.</summary>
    Public Function MainPlace(ByVal rows As IEnumerable(Of Database.IndexRow)) As String
        Dim best = rows.Select(Function(r) If(r.Spot <> "", r.Spot, r.City)).Where(Function(s) s <> "").
                        GroupBy(Function(s) s).OrderByDescending(Function(g) g.Count()).FirstOrDefault()
        Return If(best Is Nothing, "", best.Key)
    End Function

End Module
