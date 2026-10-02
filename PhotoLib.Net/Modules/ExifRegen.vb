' 設定 › 維護 「重新產生 .Exif」 (new in the .NET port): every photo / video of the albums gets its .Exif
' side file back in shape --
'   no .Exif      it is made, with the keys an import writes (frmMain.ImportAlbumFiles / ImportRunner);
'   a .Exif       each of those keys that is missing or blank is written; a key with a value is never
'                 changed (typed titles, people, places stay as they are).
' What can be worked out from the file is filled in: the date and time (the picture's EXIF, else the
' file's times -- or, for [Exif], the [Create] date already in the file), the GPS read from the picture,
' and 地點 / Country / City / Town from the GPS (PlaceNames.Resolve: an attraction within 200 m first).
' Title / Character / KeyWord / Remark can't be worked out: a missing one is added empty. GPS / Country /
' City / Town are only added with a value (as an import does).
' A photo whose .Exif was made or changed gets its PhotoIndex row written again. IMG_1.JPG and IMG_1.MOV
' share IMG_1.Exif: the picture goes first. The caller backs the .Exif files up first
' (Maintenance.BackupExif). Worker thread: it opens its own Database.
Public Module ExifRegen

    Public Class Result
        Public Checked As Integer      ' photos and videos looked at
        Public Created As Integer      ' .Exif files made
        Public Updated As Integer      ' .Exif files with a key added or filled
        Public Fields As Integer       ' keys written in all
        Public Failed As Integer
        Public LastError As String = ""
    End Class

    ''' <summary>Fills in one file's .Exif; returns how many keys were written (0 = nothing to do) and
    ''' whether the .Exif had to be made.</summary>
    Public Function RegenerateOne(ByVal strFile As String, ByVal fs As Carbon.FileSystem, ByRef created As Boolean) As Integer
        Dim exif As String = IO.Path.ChangeExtension(strFile, gc_strExifPattern)
        created = Not IO.File.Exists(exif)
        Dim ini As New Carbon.IniFile With {.FileName = exif}
        Dim keys As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)
        For Each sec In {"Create", "Exif"}
            keys(sec) = New HashSet(Of String)(If(created, New String() {}, If(ini.EnumerateKeys(sec), New String() {})), StringComparer.OrdinalIgnoreCase)
        Next
        Dim written As Integer = 0

        ' blank = missing or only spaces
        Dim valueOf = Function(sec As String, key As String) As String
                          Return If(keys(sec).Contains(key), ini.SimpleGetValue(sec, key), "")
                      End Function
        Dim put = Sub(sec As String, key As String, value As String, addEmpty As Boolean)
                      If valueOf(sec, key) <> "" Then Return
                      If value = "" AndAlso (Not addEmpty OrElse keys(sec).Contains(key)) Then Return
                      ini.SimpleSetValue(sec, key, value)
                      keys(sec).Add(key)
                      written += 1
                  End Sub

        ' date and time
        Dim shot As String = Nothing
        Dim shotOf = Function() As String
                         If shot Is Nothing Then shot = GetExifFileDateTime(fs, strFile).PadRight(14, "0"c)
                         Return shot
                     End Function
        Dim createDate As String = valueOf("Create", "Date"), createTime As String = valueOf("Create", "Time")
        put("Create", "Date", If(createDate <> "", createDate, Mid(shotOf(), 1, 8)), True)
        put("Create", "Time", If(createTime <> "", createTime, Mid(shotOf(), 9, 6)), True)
        put("Exif", "Date", If(valueOf("Create", "Date") <> "", valueOf("Create", "Date"), Mid(shotOf(), 1, 8)), True)
        put("Exif", "Time", If(valueOf("Create", "Time") <> "", valueOf("Create", "Time"), Mid(shotOf(), 9, 6)), True)
        put("Exif", "Title", "", True)
        put("Exif", "Character", "", True)

        ' where
        Dim gps As String = valueOf("Exif", "GPS")
        If gps = "" AndAlso FaceLibrary.IsPicture(strFile) Then gps = PlaceNames.GpsOfPicture(strFile)
        put("Exif", "GPS", gps, False)
        Dim place As New PlaceNames.PlaceInfo
        If gps <> "" AndAlso {"Spot", "Country", "City", "Town"}.Any(Function(k) valueOf("Exif", k) = "") Then place = PlaceNames.Resolve(gps)
        put("Exif", "Spot", place.Spot, True)
        put("Exif", "Country", place.Country, False)
        put("Exif", "City", place.City, False)
        put("Exif", "Town", place.Town, False)

        put("Exif", "KeyWord", "", True)
        put("Exif", "Remark", "", True)
        Return written
    End Function

    ''' <summary>Every photo and video of the albums under <paramref name="roots"/>.
    ''' <paramref name="progress"/> gets (done, total) every few files.</summary>
    Public Function RegenerateAll(ByVal strDatabase As String, ByVal roots As IEnumerable(Of String),
                                  Optional ByVal progress As Action(Of Integer, Integer) = Nothing,
                                  Optional ByVal cancel As Threading.CancellationToken = Nothing) As Result
        Dim r As New Result
        Dim fs As New Carbon.FileSystem
        ' a video sharing its .Exif with a picture comes after it
        Dim files As List(Of String) = Duplicates.AlbumPhotos(roots).
            OrderBy(Function(f) IO.Path.ChangeExtension(f, Nothing), StringComparer.OrdinalIgnoreCase).
            ThenBy(Function(f) If(FaceLibrary.IsPicture(f), 0, 1)).ToList()
        Dim db As New Database
        Try
            db.Construct(strDatabase)
            For i = 0 To files.Count - 1
                If cancel.IsCancellationRequested Then Exit For
                Try
                    Dim created As Boolean
                    Dim n As Integer = RegenerateOne(files(i), fs, created)
                    If created Then
                        r.Created += 1
                    ElseIf n > 0 Then
                        r.Updated += 1
                    End If
                    r.Fields += n
                    If n > 0 AndAlso db.Implement Then
                        Dim p As New Photo
                        p.Construct(files(i))
                        db.WriteImportedPhoto(p)
                    End If
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is ArgumentException
                    r.Failed += 1
                    r.LastError = IO.Path.GetFileName(files(i)) & "：" & ex.Message
                End Try
                r.Checked += 1
                If progress IsNot Nothing AndAlso (i Mod 50 = 0 OrElse i = files.Count - 1) Then progress(i + 1, files.Count)
            Next
        Finally
            db.Dispose()
        End Try
        Return r
    End Function

End Module
