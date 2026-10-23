Imports System.IO
Imports System.Xml.Linq

' The face data kept outside iPhoto.mdb too (new in the .NET port), so the database can be made again
' from the files (Restore) -- the reason a photo's .Exif exists:
'   <photo's .Exif>, its entry      <Faces>: the analysis and each face, the person by name
'                                   (Database.FaceArchiveOf)
'   <database folder>\Faces.xml     the people (birth year, hidden, cover face) and the name cards;
'                                   its "exported" time is when the .Exif files were last brought up to date
' ConvertAll (設定 › 維護, once) turns every .Exif into the XML format and writes all the face data. From
' then on Export, run in the background (at start, every few minutes, on close), writes the photos
' whose face data changed since the last export. Before ConvertAll (no Faces.xml) Export does nothing,
' so the .Exif files aren't converted behind the user's back. Every call opens its own Database (worker
' threads); one export at a time.
Public Module FaceArchive

    Public Const FacesFileName As String = "Faces.xml"
    Private ReadOnly s_busy As New Threading.SemaphoreSlim(1, 1)

    Public Function FacesFile(ByVal strDatabase As String) As String
        Return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(strDatabase)), FacesFileName)
    End Function

    ''' <summary>When the .Exif files last got the face data; Nothing before ConvertAll (no Faces.xml).</summary>
    Public Function LastExport(ByVal strDatabase As String) As DateTime?
        Dim doc As XDocument = ReadFacesFile(strDatabase)
        Dim t As DateTime
        If doc Is Nothing OrElse Not DateTime.TryParse(CStr(doc.Root.Attribute("exported")), Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, t) Then Return Nothing
        Return t
    End Function

    Public Class Result
        Public Converted As Integer    ' old-format .Exif files made XML
        Public Written As Integer      ' photos whose face data was written
        Public Failed As Integer
        Public LastError As String = ""
    End Class

    ''' <summary>The photos whose face data changed since the last export get it in their .Exif, and
    ''' Faces.xml is written again. Nothing to do before ConvertAll, or while another export runs.</summary>
    Public Function Export(ByVal strDatabase As String) As Result
        Dim r As New Result
        Dim since As DateTime? = LastExport(strDatabase)
        If Not since.HasValue OrElse Not s_busy.Wait(0) Then Return r
        Try
            Run(strDatabase, since.Value.AddSeconds(-5), Nothing, r, Nothing, Nothing)   ' a little overlap: Jet's times are to the second
        Finally
            s_busy.Release()
        End Try
        Return r
    End Function

    ''' <summary>設定 › 維護: every .Exif under <paramref name="roots"/> in the XML format, then the face data of
    ''' every photo (removed from the .Exif of photos the database has none for), then Faces.xml. The caller
    ''' backs the .Exif files up first. <paramref name="progress"/> gets (step text, done, total).</summary>
    Public Function ConvertAll(ByVal strDatabase As String, ByVal roots As IEnumerable(Of String),
                               Optional ByVal progress As Action(Of String, Integer, Integer) = Nothing,
                               Optional ByVal cancel As Threading.CancellationToken = Nothing) As Result
        Dim r As New Result
        s_busy.Wait()
        Try
            Dim sides As New List(Of String)
            For Each root In roots.Where(Function(x) Directory.Exists(x)).Distinct(StringComparer.OrdinalIgnoreCase)
                Try
                    sides.AddRange(Directory.EnumerateFiles(root, "*." & gc_strExifPattern, SearchOption.AllDirectories))
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                End Try
            Next
            For i = 0 To sides.Count - 1
                If cancel.IsCancellationRequested Then Return r
                Try
                    If ExifFile.ConvertToXml(sides(i)) Then r.Converted += 1
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                    r.Failed += 1
                    r.LastError = Path.GetFileName(sides(i)) & "：" & ex.Message
                End Try
                If progress IsNot Nothing AndAlso (i Mod 100 = 0 OrElse i = sides.Count - 1) Then progress("轉換 .Exif", i + 1, sides.Count)
            Next
            Run(strDatabase, Nothing, Duplicates.AlbumPhotos(roots), r, progress, cancel)
        Finally
            s_busy.Release()
        End Try
        Return r
    End Function

    ''' <summary>Writes the face data of the changed photos (<paramref name="since"/>) or of every photo of
    ''' <paramref name="allPhotos"/> (also removing it where the database has none), then Faces.xml.</summary>
    Private Sub Run(ByVal strDatabase As String, ByVal since As DateTime?, ByVal allPhotos As List(Of String), ByVal r As Result,
                    ByVal progress As Action(Of String, Integer, Integer), ByVal cancel As Threading.CancellationToken)
        Dim start As DateTime = DateTime.Now
        Using db As New Database
            db.Construct(strDatabase)
            If Not db.FaceTablesReady Then Return
            Dim files As List(Of String) = If(allPhotos, db.FaceFilesChangedSince(since))
            For i = 0 To files.Count - 1
                If cancel.IsCancellationRequested Then Return
                Dim f As String = files(i)
                Try
                    If File.Exists(f) Then
                        Dim faces As XElement = db.FaceArchiveOf(f)
                        Dim x As New ExifFile(f)
                        If faces IsNot Nothing OrElse x.GetElement("Faces") IsNot Nothing Then
                            x.SetElement("Faces", faces)
                            r.Written += 1
                        End If
                    End If
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is Data.Common.DbException
                    r.Failed += 1
                    r.LastError = Path.GetFileName(f) & "：" & ex.Message
                End Try
                If progress IsNot Nothing AndAlso (i Mod 100 = 0 OrElse i = files.Count - 1) Then progress("寫入面孔資料", i + 1, files.Count)
            Next
            ' only when every photo got its data does the export time move on (a failed one is tried again)
            If r.Failed = 0 Then WriteFacesFile(strDatabase, db, start)
        End Using
    End Sub

    ''' <summary>The face tables made again from the .Exif files under <paramref name="roots"/> and Faces.xml
    ''' (replacing what the tables have). The caller asks first and has the face organizer run after.</summary>
    Public Function Restore(ByVal strDatabase As String, ByVal roots As IEnumerable(Of String),
                            Optional ByVal progress As Action(Of Integer, Integer) = Nothing) As (Photos As Integer, Faces As Integer, Persons As Integer)
        Dim photos As List(Of String) = Duplicates.AlbumPhotos(roots)
        Dim archived As New List(Of (File As String, Faces As XElement))
        For i = 0 To photos.Count - 1
            Dim faces As XElement = New ExifFile(photos(i)).GetElement("Faces")
            If faces IsNot Nothing Then archived.Add((photos(i), faces))
            If progress IsNot Nothing AndAlso i Mod 200 = 0 Then progress(i + 1, photos.Count * 2)
        Next
        Dim doc As XDocument = ReadFacesFile(strDatabase)
        Dim people As IEnumerable(Of XElement) = If(doc Is Nothing, Enumerable.Empty(Of XElement)(), doc.Root.Elements())
        Using db As New Database
            db.Construct(strDatabase)
            Return db.RestoreFaces(archived, people, Path.GetDirectoryName(FacesFile(strDatabase)),
                                   Sub(n) If progress IsNot Nothing AndAlso n Mod 100 = 0 Then progress(photos.Count + n * photos.Count \ Math.Max(1, archived.Count), photos.Count * 2))
        End Using
    End Function

    ''' <summary>How many photos have face data in their .Exif (a quick look: stops at <paramref name="atMost"/>).</summary>
    Public Function ArchivedPhotos(ByVal roots As IEnumerable(Of String), Optional ByVal atMost As Integer = 1) As Integer
        Dim n As Integer = 0
        For Each f In Duplicates.AlbumPhotos(roots)
            If New ExifFile(f).GetElement("Faces") IsNot Nothing Then
                n += 1
                If n >= atMost Then Exit For
            End If
        Next
        Return n
    End Function

    '==================================================================================================
    ' Faces.xml
    '==================================================================================================
    Private Function ReadFacesFile(ByVal strDatabase As String) As XDocument
        If String.IsNullOrEmpty(strDatabase) Then Return Nothing
        Dim path As String = FacesFile(strDatabase)
        If Not File.Exists(path) Then Return Nothing
        Try
            Dim doc As XDocument = XDocument.Load(path)
            Return If(doc.Root IsNot Nothing AndAlso doc.Root.Name.LocalName = "Faces", doc, Nothing)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is Xml.XmlException
            Return Nothing
        End Try
    End Function

    Private Sub WriteFacesFile(ByVal strDatabase As String, ByVal db As Database, ByVal exported As DateTime)
        Dim path As String = FacesFile(strDatabase)
        Dim root As New XElement("Faces", New XAttribute("version", "1"), New XAttribute("exported", exported.ToString("s")))
        root.Add(db.PeopleArchive(IO.Path.GetDirectoryName(path)))
        Dim temp As String = path & ".tmp"
        Using w As Xml.XmlWriter = Xml.XmlWriter.Create(temp, New Xml.XmlWriterSettings With {.Indent = True, .Encoding = New Text.UTF8Encoding(False)})
            Call New XDocument(New XDeclaration("1.0", "utf-8", Nothing), root).Save(w)
        End Using
        File.Move(temp, path, True)
    End Sub

End Module
