' 輸入照片 (the new import window, frmImportStudio; new in the .NET port): what is going to be imported.
'   ImportJob  -- the album (where it goes, its name, date, place, icon, remark) and the photos;
'   ImportItem -- one source file: whether it is imported, what was read from it (date, GPS, size,
'                 already in the library?) and what the user typed for it (title, people, place,
'                 keywords, remark), plus the faces found in it;
'   ImportFace -- one face of a source picture, with the name the program suggests or the user gave.
' The old window (frmImport + g_lpImport + frmMain.ImportAlbumFiles) doesn't use any of this.
Public Class ImportFace

    Public Enum enumImportFaceState
        ifUnknown = 0       ' nobody (white box)
        ifAuto = 1          ' the program is sure (green): imported as confirmed, the name goes into the people field
        ifSuggested = 2     ' 「是 X 嗎？」 (yellow): kept as a suggestion only, unless the user confirms it
        ifConfirmed = 3     ' named / confirmed by the user
        ifNotFace = 9       ' the user said it isn't a face
    End Enum

    ''' <summary>Box / landmarks / score / feature from the engine (box in fractions of the picture).</summary>
    Public Face As Quartz.FaceEngine.Face
    ''' <summary>Boxed by hand.</summary>
    Public Manual As Boolean
    Public State As enumImportFaceState
    ''' <summary>The person in FacePerson; 0 for nobody or for a name that is new (added when imported).</summary>
    Public PersonID As Integer
    Public Name As String = ""
    Public Similarity As Single
    ''' <summary>People the user said this face is not (✕): written to FaceReject when imported.</summary>
    Public ReadOnly Rejected As New List(Of Integer)
    ''' <summary>Group of look-alike unknown faces in this import (「新面孔」); 0 = none.</summary>
    Public Group As Integer

    Public ReadOnly Property Box As RectangleF
        Get
            Return If(Face Is Nothing, RectangleF.Empty, Face.Box)
        End Get
    End Property

    ''' <summary>The name goes into the photo's people field (sure match or the user's own).</summary>
    Public ReadOnly Property WritesName As Boolean
        Get
            Return Name <> "" AndAlso (State = enumImportFaceState.ifAuto OrElse State = enumImportFaceState.ifConfirmed)
        End Get
    End Property

    ''' <summary>Gives the face a name (the user's): confirmed. A blank name makes it unknown again.</summary>
    Public Sub SetName(ByVal strName As String, ByVal intPersonID As Integer)
        strName = If(strName, "").Trim()
        If strName = "" Then
            Clear()
            Return
        End If
        Name = strName
        PersonID = intPersonID
        State = enumImportFaceState.ifConfirmed
        Group = 0
    End Sub

    ''' <summary>✓ on a suggestion / sure match.</summary>
    Public Sub Confirm()
        If Name <> "" Then State = enumImportFaceState.ifConfirmed
    End Sub

    ''' <summary>✕: not this person (remembered), back to unknown.</summary>
    Public Sub Reject()
        If PersonID <> 0 AndAlso Not Rejected.Contains(PersonID) Then Rejected.Add(PersonID)
        Clear()
    End Sub

    Public Sub Clear()
        Name = ""
        PersonID = 0
        Similarity = 0
        State = enumImportFaceState.ifUnknown
    End Sub

End Class

Public Class ImportItem

    Public SourceFile As String = ""
    ''' <summary>The file name in the new album folder (a second file of the same name gets "_2").</summary>
    Public FileName As String = ""
    Public Include As Boolean = True
    Public Size As Long
    ''' <summary>"yyyyMMddHHmmss" (GetExifFileDateTime: EXIF, else the file's times).</summary>
    Public ShotDateTime As String = ""
    Public Gps As String = ""
    ''' <summary>The place named from the GPS ("" when none).</summary>
    Public GpsPlace As String = ""
    Public IsPicture As Boolean
    Public IsVideo As Boolean
    ''' <summary>The library photo that is the same file (byte for byte); "" when none.</summary>
    Public DuplicateOf As String = ""
    ''' <summary>Another file of this import with the same base name (IMG_1.JPG / IMG_1.MOV): both would use
    ''' the same .Exif file.</summary>
    Public SharesExifWith As String = ""
    ''' <summary>True once the file's date / GPS / size were read.</summary>
    Public InfoRead As Boolean
    ''' <summary>[Exif] Country / City (county + district) / Town from the GPS (PlaceNames.Resolve).</summary>
    Public Country As String = ""
    Public City As String = ""
    Public Town As String = ""

    ' what the user typed (the people field is the faces' names plus People)
    Public Title As String = ""
    ''' <summary>Names typed by hand (people without a face in the picture).</summary>
    Public People As String = ""
    ''' <summary>Nothing = the place from the GPS (or the album's place when there is none).</summary>
    Public Spot As String = Nothing
    Public KeyWord As String = ""
    Public Remark As String = ""
    Public Edited As Boolean

    ''' <summary>Nothing until analysed (pictures only).</summary>
    Public Faces As List(Of ImportFace)
    Public FaceError As String

    Public ReadOnly Property ShotDate As Date?
        Get
            Dim d As Date
            If ShotDateTime.Length >= 14 AndAlso Date.TryParseExact(ShotDateTime.Substring(0, 14), "yyyyMMddHHmmss",
                                                                    Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, d) Then Return d
            If ShotDateTime.Length >= 8 AndAlso Date.TryParseExact(ShotDateTime.Substring(0, 8), "yyyyMMdd",
                                                                   Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, d) Then Return d
            Return Nothing
        End Get
    End Property

    ''' <summary>The faces shown (not the ones marked "not a face").</summary>
    Public ReadOnly Property VisibleFaces As IEnumerable(Of ImportFace)
        Get
            If Faces Is Nothing Then Return Enumerable.Empty(Of ImportFace)()
            Return Faces.Where(Function(f) f.State <> ImportFace.enumImportFaceState.ifNotFace)
        End Get
    End Property

    ''' <summary>The people field written into .Exif: the faces' names from left to right, then the names
    ''' typed by hand.</summary>
    Public Function Character() As String
        Dim s As String = ""
        For Each f In VisibleFaces.Where(Function(x) x.WritesName).OrderBy(Function(x) x.Box.X)
            s = FaceNames.Add(s, f.Name)
        Next
        For Each n In FaceNames.Split(People)
            s = FaceNames.Add(s, n)
        Next
        Return s
    End Function

    ''' <summary>The place written into .Exif.</summary>
    Public Function SpotFor(ByVal strAlbumSpot As String) As String
        If Spot IsNot Nothing Then Return Spot
        If GpsPlace <> "" Then Return GpsPlace
        Return If(strAlbumSpot, "")
    End Function

End Class

Public Class ImportJob

    Public ReadOnly Items As New List(Of ImportItem)

    ''' <summary>The album folder (Config album root \ album) the new class folder goes into.</summary>
    Public AlbumPath As String = ""
    ''' <summary>The new class folder's name.</summary>
    Public Folder As String = ""
    Public Title As String = ""
    Public DateText As String = ""
    Public Spot As String = ""
    Public Icon As String = "icoPeople"
    Public Remark As String = ""

    Public ReadOnly Property ClassPath As String
        Get
            Return AlbumPath.TrimEnd("\"c) & "\" & Folder.Trim()
        End Get
    End Property

    Public ReadOnly Property Included As List(Of ImportItem)
        Get
            Return Items.Where(Function(i) i.Include).ToList()
        End Get
    End Property

    ''' <summary>「每天一本相簿」: one album per shooting day, each with its day as folder and date, the
    ''' day's own place (else this album's) and this album's icon / remark / where it goes. The title is
    ''' "<see cref="Title"/> 09月27日", or just the day when <paramref name="bolTitleIsDate"/> (the user
    ''' kept the date as the name). Photos without a date go with the first day.</summary>
    Public Function SplitByDay(ByVal bolTitleIsDate As Boolean) As List(Of ImportJob)
        Dim result As New List(Of ImportJob)
        Dim inc As List(Of ImportItem) = Included
        Dim firstDay As Date = inc.Where(Function(i) i.ShotDate.HasValue).Select(Function(i) i.ShotDate.Value.Date).DefaultIfEmpty(Date.Today).Min()
        For Each g In inc.GroupBy(Function(i) If(i.ShotDate.HasValue, i.ShotDate.Value.Date, firstDay)).OrderBy(Function(x) x.Key)
            Dim day As String = ImportSource.DayText(g.Key)
            Dim j As New ImportJob With {
                .AlbumPath = AlbumPath, .Folder = day, .DateText = day,
                .Title = If(bolTitleIsDate OrElse Title.Trim() = "", day, Title.Trim() & " " & day),
                .Icon = Icon, .Remark = Remark}
            Dim place As String = ImportSource.CommonPlace(g)
            j.Spot = If(place <> "", place, Spot)
            j.Items.AddRange(g)
            j.AssignFileNames()
            result.Add(j)
        Next
        Return result
    End Function

    ''' <summary>Gives every included item a file name that is unique in the new folder (the same name
    ''' from two source folders: the second becomes name_2.ext) and marks the ones that would share a .Exif.</summary>
    Public Sub AssignFileNames()
        Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each it In Items
            it.FileName = ""
            it.SharesExifWith = ""
        Next
        For Each it In Included
            Dim name As String = IO.Path.GetFileName(it.SourceFile)
            Dim baseName As String = IO.Path.GetFileNameWithoutExtension(name), ext As String = IO.Path.GetExtension(name)
            Dim n As Integer = 1
            Do While used.Contains(name)
                n += 1
                name = baseName & "_" & n & ext
            Loop
            used.Add(name)
            it.FileName = name
        Next
        For Each g In Included.GroupBy(Function(i) IO.Path.GetFileNameWithoutExtension(i.FileName), StringComparer.OrdinalIgnoreCase).Where(Function(x) x.Count() > 1)
            For Each it In g
                it.SharesExifWith = String.Join("、", g.Where(Function(o) o IsNot it).Select(Function(o) o.FileName))
            Next
        Next
    End Sub

End Class
