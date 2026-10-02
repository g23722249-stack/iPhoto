Imports System.IO

' 人物欄名字檢查 (new in the .NET port): the people fields (.Exif [Exif] Character) are typed by hand,
' so one person can end up under several spellings -- 陳慈佑 / 陳慈祐, "陳 慈祐", full-width letters,
' 慈祐 / 陳慈祐. Scan reads every .Exif under the album roots; Candidates groups the names that look like
' spellings of one another; Unify rewrites the chosen spelling into every photo that has another.
' The caller backs the .Exif files up first (Maintenance.BackupExif) and renames the face person.
Public Module NameCheck

    ''' <summary>A spelling and the .Exif files whose people field has it.</summary>
    Public Class NameUse
        Public Name As String = ""
        Public ReadOnly Files As New List(Of String)
    End Class

    ''' <summary>Spellings that may be one person, and why.</summary>
    Public Class NameGroup
        Public ReadOnly Names As New List(Of NameUse)
        Public ReadOnly Reasons As New List(Of String)
        ''' <summary>How alike the faces of these names are (0..1), -1 when fewer than two have faces.</summary>
        Public FaceSimilarity As Single = -1

        Public ReadOnly Property PhotoCount As Integer
            Get
                Return Names.Sum(Function(n) n.Files.Count)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Join("／", Names.Select(Function(n) n.Name)) & "（" & String.Join("＋", Names.Select(Function(n) n.Files.Count)) & " 張）"
        End Function
    End Class

    ''' <summary>Every spelling in the people fields under <paramref name="roots"/> (may run on a worker
    ''' thread; <paramref name="progress"/> gets (done, total)).</summary>
    Public Function Scan(ByVal roots As IEnumerable(Of String), Optional ByVal progress As Action(Of Integer, Integer) = Nothing) As Dictionary(Of String, NameUse)
        Dim files As New List(Of String)
        For Each root In roots.Where(Function(r) Directory.Exists(r)).Distinct(StringComparer.OrdinalIgnoreCase)
            Try
                files.AddRange(Directory.EnumerateFiles(root, "*." & gc_strExifPattern, SearchOption.AllDirectories))
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        Dim uses As New Dictionary(Of String, NameUse)(StringComparer.Ordinal)
        For i = 0 To files.Count - 1
            For Each n In FaceNames.Split(ReadCharacter(files(i)))
                Dim u As NameUse = Nothing
                If Not uses.TryGetValue(n, u) Then
                    u = New NameUse With {.Name = n}
                    uses(n) = u
                End If
                u.Files.Add(files(i))
            Next
            If progress IsNot Nothing AndAlso (i Mod 200 = 0 OrElse i = files.Count - 1) Then progress(i + 1, files.Count)
        Next
        Return uses
    End Function

    Private Function ReadCharacter(ByVal exifFile As String) As String
        Dim ini As New Carbon.IniFile With {.FileName = exifFile}
        Return GetClearText(ini.SimpleGetValue("Exif", "Character"))
    End Function

    '==================================================================================================
    ' Which spellings go together
    '==================================================================================================
    ''' <summary>The groups of spellings that look like one person, most photos first.
    ''' <paramref name="faceSimilarity"/> (optional) says how alike two names' faces are, -1 unknown.</summary>
    Public Function Candidates(ByVal uses As Dictionary(Of String, NameUse), Optional ByVal faceSimilarity As Func(Of String, String, Single) = Nothing) As List(Of NameGroup)
        Dim names As List(Of String) = uses.Keys.ToList()
        Dim parent As New Dictionary(Of String, String)
        For Each n In names
            parent(n) = n
        Next
        Dim find As Func(Of String, String) = Nothing
        find = Function(x As String) As String
                   While parent(x) <> x
                       parent(x) = parent(parent(x))
                       x = parent(x)
                   End While
                   Return x
               End Function
        Dim reasons As New Dictionary(Of String, List(Of String))
        For i = 0 To names.Count - 1
            For j = i + 1 To names.Count - 1
                Dim why As String = Similar(names(i), names(j))
                If why Is Nothing Then Continue For
                Dim a As String = find(names(i)), b As String = find(names(j))
                If a <> b Then parent(b) = a
                Dim key As String = names(i)
                If Not reasons.ContainsKey(key) Then reasons(key) = New List(Of String)
                reasons(key).Add(why)
            Next
        Next
        Dim groups As New List(Of NameGroup)
        For Each g In names.GroupBy(Function(n) find(n)).Where(Function(x) x.Count() >= 2)
            Dim ng As New NameGroup
            ng.Names.AddRange(g.Select(Function(n) uses(n)).OrderByDescending(Function(u) u.Files.Count))
            For Each n In g
                If reasons.ContainsKey(n) Then ng.Reasons.AddRange(reasons(n))
            Next
            Dim distinct = ng.Reasons.Distinct().ToList()
            ng.Reasons.Clear()
            ng.Reasons.AddRange(distinct)
            If faceSimilarity IsNot Nothing Then
                For a = 0 To ng.Names.Count - 1
                    For b = a + 1 To ng.Names.Count - 1
                        ng.FaceSimilarity = Math.Max(ng.FaceSimilarity, faceSimilarity(ng.Names(a).Name, ng.Names(b).Name))
                    Next
                Next
            End If
            groups.Add(ng)
        Next
        Return groups.OrderByDescending(Function(g) g.PhotoCount).ToList()
    End Function

    ''' <summary>Why two spellings look like one name; Nothing when they don't.</summary>
    Public Function Similar(ByVal a As String, ByVal b As String) As String
        If Normalize(a) = Normalize(b) Then Return "寫法不同（空白、全形／半形或大小寫）"
        ' one character apart (not the family name): 陳慈佑 / 陳慈祐
        If a.Length = b.Length AndAlso a.Length >= 3 Then
            Dim diff As Integer = -1, count As Integer = 0
            For k = 0 To a.Length - 1
                If a(k) <> b(k) Then diff = k : count += 1
            Next
            If count = 1 AndAlso diff > 0 Then Return "只差一個字：" & a(diff) & "／" & b(diff)
        End If
        ' the family name left out: 慈祐 / 陳慈祐
        Dim s As String = If(a.Length < b.Length, a, b), l As String = If(a.Length < b.Length, b, a)
        If s.Length >= 2 AndAlso l.Length = s.Length + 1 AndAlso l.EndsWith(s, StringComparison.Ordinal) AndAlso IsCjk(l(0)) Then
            Return "少了姓氏：" & s & "／" & l
        End If
        Return Nothing
    End Function

    ''' <summary>Without spaces, full-width letters and digits as half-width, lower case.</summary>
    Public Function Normalize(ByVal s As String) As String
        Dim sb As New Text.StringBuilder
        For Each ch In If(s, "")
            If Char.IsWhiteSpace(ch) OrElse ch = ChrW(&H3000) Then Continue For
            If ch >= ChrW(&HFF01) AndAlso ch <= ChrW(&HFF5E) Then ch = ChrW(AscW(ch) - &HFEE0)
            sb.Append(Char.ToLowerInvariant(ch))
        Next
        Return sb.ToString()
    End Function

    Private Function IsCjk(ByVal ch As Char) As Boolean
        Return ch >= ChrW(&H4E00) AndAlso ch <= ChrW(&H9FFF)
    End Function

    '==================================================================================================
    ' Rewriting
    '==================================================================================================
    ''' <summary>Every photo whose people field has <paramref name="from"/> gets <paramref name="target"/>
    ''' instead (just loses it when the target is there already); PhotoIndex follows. Returns how many
    ''' .Exif files changed.</summary>
    Public Function Unify(ByVal from As NameUse, ByVal target As String, ByVal db As Database) As Integer
        Dim changed As Integer = 0
        For Each exif In from.Files
            Dim ini As New Carbon.IniFile With {.FileName = exif}
            Dim before As String = GetClearText(ini.SimpleGetValue("Exif", "Character"))
            Dim after As String = FaceNames.Rename(before, from.Name, target)
            If after = before Then Continue For
            ini.SimpleSetValue("Exif", "Character", after)
            changed += 1
            Dim photo As String = PhotoOf(exif)
            If photo <> "" AndAlso db IsNot Nothing AndAlso db.Implement Then
                Dim p As New Photo
                p.Construct(photo)
                db.SetPhotoCharacter(p)
            End If
        Next
        Return changed
    End Function

    ''' <summary>Every photo whose people field has <paramref name="name"/> loses it (the other names stay);
    ''' PhotoIndex follows. Returns how many .Exif files changed.</summary>
    Public Function RemoveName(ByVal name As NameUse, ByVal db As Database) As Integer
        Dim changed As Integer = 0
        For Each exif In name.Files
            Dim ini As New Carbon.IniFile With {.FileName = exif}
            Dim before As String = GetClearText(ini.SimpleGetValue("Exif", "Character"))
            If Not FaceNames.Contains(before, name.Name) Then Continue For   ' Remove would also re-join the rest
            Dim after As String = FaceNames.Remove(before, name.Name)
            If after = before Then Continue For
            ini.SimpleSetValue("Exif", "Character", after)
            changed += 1
            Dim photo As String = PhotoOf(exif)
            If photo <> "" AndAlso db IsNot Nothing AndAlso db.Implement Then
                Dim p As New Photo
                p.Construct(photo)
                db.SetPhotoCharacter(p)
            End If
        Next
        Return changed
    End Function

    ''' <summary>The photo / video a .Exif belongs to ("" when it is gone).</summary>
    Private Function PhotoOf(ByVal exifFile As String) As String
        Dim folder As String = Path.GetDirectoryName(exifFile), baseName As String = Path.GetFileNameWithoutExtension(exifFile)
        Try
            Return Directory.GetFiles(folder, baseName & ".*").
                   FirstOrDefault(Function(f) Not f.EndsWith("." & gc_strExifPattern, StringComparison.OrdinalIgnoreCase) AndAlso
                                               String.Equals(Path.GetFileNameWithoutExtension(f), baseName, StringComparison.OrdinalIgnoreCase))
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return ""
    End Function

End Module
