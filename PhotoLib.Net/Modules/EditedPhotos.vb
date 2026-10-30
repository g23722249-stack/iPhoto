Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

' Photos edited with PhotoEdit (new in the .NET port): the album shows the edited picture in place of the
' original, which stays as it was (PhotoEdit never changes it) but hidden.
'   - PhotoEdit exports beside the photo as <name>_edited.jpg (then _edited2, _edited3 ...; .png when the
'     picture is transparent). Such a file without a .Exif, next to an original <name>.<image ext>, is
'     taken in (Adopt): it gets the original's .Exif values (date, title, people, place ...) and
'     <EditedFrom name="original"/>; the original gets <EditedAs name="edited"/>; an earlier edited
'     version of the same original gets <EditedAs> the new one too.
'   - A file is hidden while its <EditedAs> file is there (HiddenBy): AlbumFiles, which the album list,
'     the index sync, the face scan and 尋找重複照片 use, leaves it out. Delete the edited file and the
'     original shows again.
'   - Editing an edited photo opens its original (OriginalOf): PhotoEdit brings back the edits it keeps
'     beside it (<photo>.pedit.json), so the next export goes on from them.
' What was taken in is also queued (TakeAdoptions) for iPhoto to update the index and the faces (the
' original's row and face data go, the edited one's row comes) on its own thread.
Public Module EditedPhotos

    Private Const EditedAsName As String = "EditedAs"
    Private Const EditedFromName As String = "EditedFrom"
    Private ReadOnly EditedPattern As New Regex("^(?<base>.+)_edited(?<n>\d*)$", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private ReadOnly ImageExts As String() = {".jpg", ".jpeg", ".png", ".bmp", ".gif"}

    ''' <summary>One edited picture taken in: <see cref="Original"/> (now hidden), <see cref="Edited"/>, and
    ''' earlier edited versions of it hidden now too.</summary>
    Public Class Adoption
        Public Original As String
        Public Edited As String
        Public Superseded As New List(Of String)
    End Class

    Private ReadOnly s_adopted As New Concurrent.ConcurrentQueue(Of Adoption)

    ''' <summary>The pictures taken in since the last call (by any thread), for the index and the faces.</summary>
    Public Function TakeAdoptions() As List(Of Adoption)
        Dim list As New List(Of Adoption)
        Dim a As Adoption = Nothing
        While s_adopted.TryDequeue(a)
            list.Add(a)
        End While
        Return list
    End Function

    ''' <summary>The photos and videos of an album folder as the album shows them: edited pictures taken in
    ''' first (unless read-only), the files they replace left out.</summary>
    Public Function AlbumFiles(ByVal folder As String) As String()
        Dim files() As String = Nothing
        ExactFiles(folder, GetPhotoPatterns(), files, True)
        If files.Length = 0 Then Return files
        If g_lpConfig Is Nothing OrElse Not g_lpConfig.ReadOnly Then AdoptIn(files)
        Return files.Where(Function(f) HiddenBy(f) Is Nothing).ToArray()
    End Function

    ''' <summary>The file that replaces <paramref name="file"/> in its album (its edited picture), or Nothing.</summary>
    Public Function HiddenBy(ByVal file As String) As String
        Return Linked(file, EditedAsName)
    End Function

    ''' <summary>The original of an edited picture, or Nothing (not edited, or the original is gone).</summary>
    Public Function OriginalOf(ByVal file As String) As String
        Return Linked(file, EditedFromName)
    End Function

    ''' <summary>The files in the same folder that <paramref name="file"/> hides (its original and earlier
    ''' edited versions), for moving them along.</summary>
    Public Function HiddenBehind(ByVal file As String) As List(Of String)
        Dim result As New List(Of String)
        Dim folder As String = Path.GetDirectoryName(file)
        Dim files() As String = Nothing
        ExactFiles(folder, GetPhotoPatterns(), files, True)
        For Each f In files
            If String.Equals(HiddenBy(f), file, StringComparison.OrdinalIgnoreCase) Then result.Add(f)
        Next
        Return result
    End Function

    ''' <summary>Points <paramref name="hidden"/> at <paramref name="by"/> (Nothing: shows it again) -- after
    ''' moving or renaming one of them.</summary>
    Public Sub SetHiddenBy(ByVal hidden As String, ByVal by As String)
        Dim exif As New ExifFile(hidden)
        exif.SetElement(EditedAsName, If(by Is Nothing, Nothing, New XElement(EditedAsName, New XAttribute("name", Path.GetFileName(by)))))
    End Sub

    ''' <summary>Points the edited picture <paramref name="edited"/> at its <paramref name="original"/>.</summary>
    Public Sub SetOriginal(ByVal edited As String, ByVal original As String)
        Dim exif As New ExifFile(edited)
        exif.SetElement(EditedFromName, If(original Is Nothing, Nothing, New XElement(EditedFromName, New XAttribute("name", Path.GetFileName(original)))))
    End Sub

    ''' <summary>The file named by the entry's element <paramref name="element"/>, in the same folder, when it is there.</summary>
    Private Function Linked(ByVal file As String, ByVal element As String) As String
        If String.IsNullOrEmpty(file) Then Return Nothing
        Dim x As XElement = New ExifFile(file).GetElement(element)
        Dim name As String = CStr(x?.Attribute("name"))
        If String.IsNullOrEmpty(name) OrElse name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then Return Nothing
        Dim target As String = Path.Combine(Path.GetDirectoryName(file), name)
        Return If(IO.File.Exists(target), target, Nothing)
    End Function

    ''' <summary>Takes in the edited pictures of a folder that aren't yet; what was taken in.</summary>
    Public Function AdoptIn(ByVal folder As String) As List(Of Adoption)
        Dim files() As String = Nothing
        ExactFiles(folder, GetPhotoPatterns(), files, True)
        Return AdoptIn(files)
    End Function

    Private Function AdoptIn(ByVal files() As String) As List(Of Adoption)
        Dim result As New List(Of Adoption)
        For Each edited In files
            Dim m As Match = EditedPattern.Match(Path.GetFileNameWithoutExtension(edited))
            If Not m.Success OrElse Array.IndexOf(ImageExts, Path.GetExtension(edited).ToLowerInvariant()) < 0 Then Continue For
            If New ExifFile(edited).Exists Then Continue For   ' taken in before (or a photo of its own)
            Dim baseName As String = m.Groups("base").Value
            ' the original: <base>.<image ext> in the same folder, the edited picture's own type first
            Dim original As String = files.Where(Function(f) Not String.Equals(f, edited, StringComparison.OrdinalIgnoreCase) AndAlso
                                                       String.Equals(Path.GetFileNameWithoutExtension(f), baseName, StringComparison.OrdinalIgnoreCase) AndAlso
                                                       Array.IndexOf(ImageExts, Path.GetExtension(f).ToLowerInvariant()) >= 0).
                                           OrderBy(Function(f) If(String.Equals(Path.GetExtension(f), Path.GetExtension(edited), StringComparison.OrdinalIgnoreCase), 0, 1)).
                                           FirstOrDefault()
            If original Is Nothing Then Continue For
            Try
                result.Add(Adopt(original, edited, files))
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                ' tried again the next time the album is listed
            End Try
        Next
        Return result
    End Function

    Private Function Adopt(ByVal original As String, ByVal edited As String, ByVal files() As String) As Adoption
        Dim a As New Adoption With {.Original = original, .Edited = edited}
        Dim src As New ExifFile(original)
        Dim values As New List(Of (Section As String, Key As String, Value As String))
        For Each section In {"Create", "Exif"}
            For Each key In src.EnumerateKeys(section)
                values.Add((section, key, src.SimpleGetValue(section, key)))
            Next
        Next
        Dim dst As New ExifFile(edited)
        If values.Count > 0 Then
            dst.SetValues(values)
        Else
            dst.SetValues({("Create", "Date", File.GetLastWriteTime(original).ToString("yyyyMMdd"))})
        End If
        SetOriginal(edited, original)
        ' earlier edited versions of this original are replaced too
        For Each f In files
            If String.Equals(f, edited, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(f, original, StringComparison.OrdinalIgnoreCase) Then Continue For
            If String.Equals(OriginalOf(f), original, StringComparison.OrdinalIgnoreCase) AndAlso HiddenBy(f) Is Nothing Then
                SetHiddenBy(f, edited)
                a.Superseded.Add(f)
            End If
        Next
        SetHiddenBy(original, edited)
        s_adopted.Enqueue(a)
        Return a
    End Function

End Module
