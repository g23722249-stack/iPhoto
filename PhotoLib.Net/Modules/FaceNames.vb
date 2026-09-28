' The people field of a photo (.Exif [Exif] Character, PhotoIndex.Exif_Character) as a list of names.
' It is typed by hand, so the separators vary: "陳海秀,陳慈佑", "爸/媽/陳惠雯", full-width "，" or "、".
' New in the .NET port (face recognition writes names back into it).
Public Module FaceNames

    Private ReadOnly Separators As Char() = {","c, "/"c, "，"c, "、"c}

    Public Function Split(ByVal strCharacter As String) As String()
        Return If(strCharacter, "").Split(Separators, StringSplitOptions.RemoveEmptyEntries).
               Select(Function(n) n.Trim()).Where(Function(n) n <> "").ToArray()
    End Function

    Public Function Contains(ByVal strCharacter As String, ByVal strName As String) As Boolean
        Return Split(strCharacter).Any(Function(n) String.Equals(n, If(strName, "").Trim(), StringComparison.CurrentCultureIgnoreCase))
    End Function

    ''' <summary>The field with <paramref name="strName"/> added at the end (unchanged when already in it),
    ''' joined with the separator the field already uses ("," when it has none).</summary>
    Public Function Add(ByVal strCharacter As String, ByVal strName As String) As String
        strName = If(strName, "").Trim()
        strCharacter = If(strCharacter, "").Trim()
        If strName = "" OrElse Contains(strCharacter, strName) Then Return strCharacter
        If strCharacter = "" Then Return strName
        Return strCharacter & SeparatorOf(strCharacter) & strName
    End Function

    ''' <summary>The field without <paramref name="strName"/>, keeping its separator.</summary>
    Public Function Remove(ByVal strCharacter As String, ByVal strName As String) As String
        Dim sep As String = SeparatorOf(strCharacter)
        Return String.Join(sep, Split(strCharacter).Where(Function(n) Not String.Equals(n, If(strName, "").Trim(), StringComparison.CurrentCultureIgnoreCase)))
    End Function

    ''' <summary>The field with <paramref name="strOld"/> replaced by <paramref name="strNew"/> in place
    ''' (just removed when the new name is already in it).</summary>
    Public Function Rename(ByVal strCharacter As String, ByVal strOld As String, ByVal strNew As String) As String
        If Not Contains(strCharacter, strOld) Then Return If(strCharacter, "")
        If Contains(strCharacter, strNew) Then Return Remove(strCharacter, strOld)
        Dim sep As String = SeparatorOf(strCharacter)
        Return String.Join(sep, Split(strCharacter).Select(Function(n) If(String.Equals(n, strOld.Trim(), StringComparison.CurrentCultureIgnoreCase), strNew.Trim(), n)))
    End Function

    Private Function SeparatorOf(ByVal strCharacter As String) As String
        Dim i As Integer = If(strCharacter, "").IndexOfAny(Separators)
        Return If(i < 0, ",", strCharacter(i).ToString())
    End Function

End Module
