Imports System.Threading

' Face recognition for the whole photo library (new in the .NET port):
'   - a background thread walks every album root (Config album paths -> album -> class folder -> photos,
'     the same tree Storage shows), analyses the photos that are new or changed since their FacePhoto
'     row and saves the faces. It has its own Database (Jet connection): an OleDbConnection can't be
'     shared between threads. It runs below normal priority and can be paused.
'   - the UI thread names faces, boxes faces by hand and analyses the shown photo on demand, through
'     g_lpDatabase. Naming writes the name into the photo's people field (.Exif Character) and
'     updates PhotoIndex, so search, tooltips and the info forms see it.
' Create returns Nothing when face recognition can't run (no database, no face tables, no models).
Public Class FaceLibrary
    Implements IDisposable

    ''' <summary>Raised on the UI thread while the library is being analysed (done of total photos).</summary>
    Public Event Progress(ByVal intDone As Integer, ByVal intTotal As Integer)
    ''' <summary>Raised on the UI thread when a scan ends (finished, stopped or failed).</summary>
    Public Event Finished(ByVal intAnalysed As Integer, ByVal strError As String)

    Private ReadOnly m_lpEngine As Quartz.FaceEngine
    Private ReadOnly m_strDatabase As String
    Private m_thread As Thread
    Private m_context As SynchronizationContext
    Private ReadOnly m_resume As New ManualResetEventSlim(True)
    Private m_bolStop As Boolean

    Private Shared ReadOnly ImageExtensions As String() = {".jpg", ".jpeg", ".png", ".bmp", ".gif"}

    Private Sub New(ByVal engine As Quartz.FaceEngine, ByVal strDatabase As String)
        m_lpEngine = engine
        m_strDatabase = strDatabase
    End Sub

    ''' <summary>The face library, or Nothing when <paramref name="lpDatabase"/> isn't open, has no face
    ''' tables, or the model files are missing.</summary>
    Public Shared Function Create(ByVal lpDatabase As Database, ByVal strDatabase As String) As FaceLibrary
        If lpDatabase Is Nothing OrElse Not lpDatabase.FaceTablesReady Then Return Nothing
        If Not Quartz.FaceEngine.ModelsAvailable(Quartz.FaceEngine.DefaultModelFolder) Then Return Nothing
        Try
            Return New FaceLibrary(New Quartz.FaceEngine(Quartz.FaceEngine.DefaultModelFolder), strDatabase)
        Catch ex As Exception When TypeOf ex Is DllNotFoundException OrElse TypeOf ex Is BadImageFormatException OrElse
                                   TypeOf ex Is TypeInitializationException OrElse TypeOf ex Is OpenCvSharp.OpenCVException
            Return Nothing   ' native OpenCV missing / wrong bitness, or a damaged model
        End Try
    End Function

    Public ReadOnly Property Engine As Quartz.FaceEngine
        Get
            Return m_lpEngine
        End Get
    End Property

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return m_thread IsNot Nothing AndAlso m_thread.IsAlive
        End Get
    End Property

    Public ReadOnly Property IsPaused As Boolean
        Get
            Return Not m_resume.IsSet
        End Get
    End Property

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
        m_lpEngine.Dispose()
        m_resume.Dispose()
    End Sub

    '==================================================================================================
    ' Background scan
    '==================================================================================================
    ''' <summary>Starts analysing the photos under <paramref name="albumRoots"/> (Config album paths).
    ''' Call from the UI thread: the events are raised on it.</summary>
    Public Sub Start(ByVal albumRoots As IEnumerable(Of String))
        If IsRunning Then Return
        StartThread(albumRoots.ToArray())
    End Sub

    ''' <summary>Sorts the faces into people again in the background (after the user named faces):
    ''' right away, or after the scan / sort that is running now.</summary>
    ''' <summary>Analyses what is new under <paramref name="albumRoots"/> (after an import): right away, or
    ''' when the scan / sort running now is done. UI thread.</summary>
    Public Sub RequestScan(ByVal albumRoots As IEnumerable(Of String))
        Dim roots As String() = albumRoots.ToArray()
        If IsRunning Then
            Interlocked.Exchange(m_lpRootsAgain, roots)
        Else
            StartThread(roots)
        End If
    End Sub

    Private m_lpRootsAgain As String()

    ''' <summary>設定 › 面孔 自動認人: 0 寬鬆, 1 平衡, 2 嚴格 (the next sort uses it).</summary>
    Public Property Strictness As Integer = 1

    ''' <summary>Deletes every face, person, template and rejection (the photos, their people fields and
    ''' the FaceIndex name cards stay) and the face wall's picture cache. The scan is stopped first.</summary>
    Public Sub ClearAll()
        [Stop]()
        g_lpDatabase.ClearFaceTables()
        m_lpCatalog = Nothing
        Try
            If IO.Directory.Exists(CacheFolder) Then IO.Directory.Delete(CacheFolder, True)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            ' a card still open somewhere: the folder is only a cache
        End Try
    End Sub

    Public Sub RequestOrganize()
        If IsRunning Then
            m_bolOrganizeAgain = True
        Else
            StartThread(Nothing)
        End If
    End Sub

    Private m_bolOrganizeAgain As Boolean
    Private WithEvents m_organizeTimer As System.Windows.Forms.Timer

    ''' <summary>RequestOrganize a few seconds after the last call (UI thread): naming faces one after
    ''' another in the viewer sorts once, when the user pauses.</summary>
    Public Sub RequestOrganizeSoon()
        If m_organizeTimer Is Nothing Then m_organizeTimer = New System.Windows.Forms.Timer With {.Interval = 5000}
        m_organizeTimer.Stop()
        m_organizeTimer.Start()
    End Sub

    Private Sub m_organizeTimer_Tick(sender As Object, e As EventArgs) Handles m_organizeTimer.Tick
        m_organizeTimer.Stop()
        RequestOrganize()
    End Sub

    Private Sub StartThread(ByVal roots As String())
        m_context = If(SynchronizationContext.Current, New SynchronizationContext())
        m_bolStop = False
        m_resume.Set()
        m_thread = New Thread(Sub() Scan(roots)) With {.IsBackground = True, .Priority = ThreadPriority.BelowNormal, .Name = "FaceLibrary.Scan"}
        m_thread.Start()
    End Sub

    Public Sub Pause()
        m_resume.Reset()
    End Sub

    Public Sub [Resume]()
        m_resume.Set()
    End Sub

    ''' <summary>Stops the scan and waits (up to a few seconds) for the photo in hand to finish.</summary>
    Public Sub [Stop]()
        m_bolStop = True
        m_resume.Set()
        If m_thread IsNot Nothing AndAlso m_thread.IsAlive Then m_thread.Join(5000)
        m_thread = Nothing
    End Sub

    Private m_intFailed As Integer
    Private m_strLastFailure As String

    ''' <summary>Photos the last scan skipped because of an error, and the last such error.</summary>
    Public ReadOnly Property FailedCount As Integer
        Get
            Return m_intFailed
        End Get
    End Property

    Public ReadOnly Property LastFailure As String
        Get
            Return m_strLastFailure
        End Get
    End Property

    ''' <summary>The thread: analyses the photos under <paramref name="roots"/> (Nothing = none), then sorts
    ''' the faces into people (unless stopped).</summary>
    Private Sub Scan(ByVal roots As String())
        Dim analysed As Integer = 0
        Dim errorText As String = Nothing
        Dim db As New Database
        Try
            db.Construct(m_strDatabase)
            If Not db.FaceTablesReady Then Throw New InvalidOperationException("資料庫沒有面孔資料表")
            Do
                If roots IsNot Nothing Then
                    analysed = AnalyseAll(db, roots)
                    Dim n As Integer = analysed
                    Post(Sub() RaiseEvent Finished(n, Nothing))
                End If
                ' Jet writes the UI thread's changes lazily and every connection reads through its own page
                ' cache (a few seconds): after a change made in the UI, wait a moment and sort on a fresh
                ' connection, so the sort sees what the user just did.
                Dim fromUI As Boolean = roots Is Nothing
                Do
                    m_bolOrganizeAgain = False
                    If fromUI Then Thread.Sleep(1500)
                    If m_bolStop Then Exit Do
                    db.Dispose()
                    db = New Database
                    db.Construct(m_strDatabase)
                    OrganizeWith(db)
                    fromUI = True   ' another round was asked for from the UI meanwhile
                Loop While m_bolOrganizeAgain
                ' photos imported while this scan ran (RequestScan)
                roots = Interlocked.Exchange(m_lpRootsAgain, Nothing)
            Loop While roots IsNot Nothing AndAlso Not m_bolStop
        Catch ex As Exception When TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is InvalidOperationException OrElse
                                   TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            errorText = ex.Message
            Dim err As String = errorText
            Post(Sub() RaiseEvent Finished(analysed, err))
        Finally
            db.Dispose()
        End Try
    End Sub

    Private Function AnalyseAll(ByVal db As Database, ByVal roots As String()) As Integer
        m_intFailed = 0
        m_strLastFailure = Nothing
        Dim analysed As Integer = 0
        Dim files As List(Of String) = ListPhotos(roots)
        Dim states As Dictionary(Of String, Database.FacePhotoState) = db.LoadFacePhotoStates()
        Dim todo As List(Of String) = files.Where(Function(f) NeedsAnalysis(f, states)).ToList()
        Dim total As Integer = files.Count
        Dim done As Integer = total - todo.Count
        Post(Sub() RaiseEvent Progress(done, total))

        For Each f In todo
            m_resume.Wait()
            If m_bolStop Then Exit For
            Try
                AnalyseAndSave(db, f)
                analysed += 1
            Catch ex As Exception When TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is IO.IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is OpenCvSharp.OpenCVException
                ' the same photo saved by the UI thread at this moment, the file just went away, or a
                ' picture OpenCV chokes on: skipped, tried again on the next scan
                m_intFailed += 1
                m_strLastFailure = IO.Path.GetFileName(f) & "：" & ex.Message
            End Try
            done += 1
            If done Mod 10 = 0 OrElse done = total Then
                Dim d As Integer = done
                Post(Sub() RaiseEvent Progress(d, total))
            End If
        Next
        Return analysed
    End Function

    '==================================================================================================
    ' Sorting faces into people (scan thread)
    '==================================================================================================
    ''' <summary>Raised on the UI thread when the faces have been sorted into people (the face wall and
    ''' the 面孔 tree node read <see cref="Catalog"/>).</summary>
    Public Event Organized As EventHandler

    Private m_lpCatalog As FaceCatalog

    ''' <summary>The people and unnamed groups from the last sort; Nothing until the first one ends.</summary>
    Public ReadOnly Property Catalog As FaceCatalog
        Get
            Return m_lpCatalog
        End Get
    End Property

    ''' <summary>Put names into the photos' people fields (.Exif Character + PhotoIndex) when a face is
    ''' named or assigned. False keeps the names in the face tables only (設定 › 面孔, and tests on a copy).</summary>
    Public Property WriteNames As Boolean = True

    ''' <summary>How long the last sort took and what it did (for the status line and tests).</summary>
    Public ReadOnly Property LastOrganizeInfo As String
        Get
            Return m_strOrganizeInfo
        End Get
    End Property
    Private m_strOrganizeInfo As String = ""

    ''' <summary>A photo's people field, from its .Exif file.</summary>
    Private Shared Function CharacterOf(ByVal strFileDesc As String) As String
        Dim p As New Photo
        p.Construct(strFileDesc)
        Return p.Exif(enumPhotoExif.peCharacter)
    End Function

    Private Sub OrganizeWith(ByVal db As Database)
        Dim sw As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
        Dim faces As List(Of FaceRegion) = db.LoadAllFaces()
        Dim persons As List(Of Database.FacePersonInfo) = db.LoadFacePersons()
        Dim result As FaceOrganizer.Result = FaceOrganizer.Organize(faces, persons, AddressOf CharacterOf, db.LoadFaceRejects(),
                                                                    Function(n) db.EnsureFacePerson(n),
                                                                    FaceOrganizer.AutoCosineByStrictness(Math.Max(0, Math.Min(2, Strictness))))
        Dim changed As HashSet(Of Integer) = db.SaveFaceAssignments(result.Assignments)
        db.ReplaceFaceTemplates(result.Templates)

        ' The program's own matches are NOT written into the photos' people fields: on this library about
        ' 6% of them are wrong (see FaceOrganizer). A name goes into a people field when the user names
        ' or confirms the face (Name / NameFaces).

        ' the in-memory faces may differ from the table where the user got there first: reload them
        If changed.Count <> result.Assignments.Count Then faces = db.LoadAllFaces()

        Dim catalog As FaceCatalog = FaceCatalog.Build(faces, db.LoadFacePersons(), If(changed.Count <> result.Assignments.Count, Nothing, result.Clusters))
        m_lpCatalog = catalog
        m_strOrganizeInfo = $"{faces.Count} 張臉、{catalog.Persons.Count} 人、{catalog.Clusters.Count} 個未命名群組；" &
                            $"本次改動 {changed.Count} 張臉，{sw.ElapsedMilliseconds / 1000.0:0.0} 秒"
        Post(Sub() RaiseEvent Organized(Me, EventArgs.Empty))
    End Sub

    ''' <summary>Every picture of every class folder of every album of the roots (Storage's tree).</summary>
    Private Shared Function ListPhotos(ByVal roots As String()) As List(Of String)
        Dim result As New List(Of String)
        For Each root In roots
            Dim albums() As String = Nothing
            For i = 0 To ExactFolders(root, albums) - 1
                If IsSystemFolder(albums(i)) Then Continue For
                Dim classes() As String = Nothing
                For j = 0 To ExactFolders(albums(i), classes) - 1
                    If IsSystemFolder(classes(j)) Then Continue For
                    Dim photos() As String = Nothing
                    For k = 0 To ExactFiles(classes(j), "*.JPG;*.JPEG;*.PNG;*.BMP;*.GIF", photos, True) - 1
                        result.Add(photos(k))
                    Next
                Next
            Next
        Next
        Return result
    End Function

    Private Shared Function NeedsAnalysis(ByVal f As String, ByVal states As Dictionary(Of String, Database.FacePhotoState)) As Boolean
        Dim s As New Database.FacePhotoState
        If Not states.TryGetValue(f, s) Then Return True
        If s.EngineVer <> Quartz.FaceEngine.Version Then Return True
        Try
            Dim info As New IO.FileInfo(f)
            Return info.Length <> s.FileSize OrElse Math.Abs((info.LastWriteTime - s.FileTime).TotalSeconds) > 2
        Catch
            Return False
        End Try
    End Function

    Private Sub AnalyseAndSave(ByVal db As Database, ByVal f As String)
        Dim info As New IO.FileInfo(f)
        Dim faces As List(Of Quartz.FaceEngine.Face) = m_lpEngine.Analyze(f)
        db.SaveFaceAnalysis(f, info.Length, info.LastWriteTime, ShotYear(f), faces)
    End Sub

    Private Sub Post(ByVal action As Action)
        Try
            m_context.Post(Sub(state) action(), Nothing)
        Catch ex As InvalidOperationException
            ' the window that started the scan is gone
        End Try
    End Sub

    ''' <summary>The year the photo was taken: its .Exif date, else the file's time.</summary>
    Public Shared Function ShotYear(ByVal strFileDesc As String) As Integer
        Dim p As New Photo
        p.Construct(strFileDesc)
        Dim d As String = PadNum(p.Exif(enumPhotoExif.peDate), 8)
        Dim y As Integer = CInt(Val(Left(d, 4)))
        If y >= 1900 AndAlso y <= DateTime.Now.Year + 1 Then Return y
        Return IO.File.GetLastWriteTime(strFileDesc).Year
    End Function

    '==================================================================================================
    ' UI thread (g_lpDatabase)
    '==================================================================================================
    ''' <summary>True for the picture types the engine analyses.</summary>
    Public Shared Function IsPicture(ByVal strFileDesc As String) As Boolean
        Return ImageExtensions.Contains(IO.Path.GetExtension(If(strFileDesc, "")).ToLowerInvariant())
    End Function

    ''' <summary>The faces of the photo, analysing it first when it is new or changed (about 0.2 s).</summary>
    Public Function FacesOf(ByVal strFileDesc As String) As List(Of FaceRegion)
        If Not IsPicture(strFileDesc) OrElse Not g_lpDatabase.FaceTablesReady Then Return New List(Of FaceRegion)
        Dim states = g_lpDatabase.LoadFacePhotoStates(strFileDesc)
        If NeedsAnalysis(strFileDesc, states) Then
            Try
                AnalyseAndSave(g_lpDatabase, strFileDesc)
            Catch ex As Exception When TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is IO.IOException OrElse
                                       TypeOf ex Is OpenCvSharp.OpenCVException
                ' the scan thread is saving it right now: show what is there
            End Try
        End If
        Return g_lpDatabase.LoadFaceRegions(strFileDesc)
    End Function

    ''' <summary>Gives the face a name (a new name adds the person) and puts the name into the photo's
    ''' people field. A blank name takes the name off (see <see cref="Unname"/>).</summary>
    Public Sub Name(ByVal face As FaceRegion, ByVal strName As String)
        strName = If(strName, "").Trim()
        If strName = "" Then
            Unname(face)
            Return
        End If
        ' confirming the program's own match / a seed keeps the person; only a confirmed face with the
        ' same name has nothing to do
        If face.State = FaceRegion.enumFaceState.fsConfirmed AndAlso String.Equals(face.PersonName, strName, StringComparison.CurrentCultureIgnoreCase) Then Return
        If face.PersonID <> 0 AndAlso Not String.Equals(face.PersonName, strName, StringComparison.CurrentCultureIgnoreCase) Then Unname(face)
        Dim id As Integer = g_lpDatabase.EnsureFacePerson(strName)
        g_lpDatabase.SetFacePerson(face.FaceID, id, FaceRegion.enumFaceState.fsConfirmed)
        face.PersonID = id
        face.PersonName = strName
        face.State = FaceRegion.enumFaceState.fsConfirmed
        UpdateCharacter(g_lpDatabase, face.FileName, Function(s) FaceNames.Add(s, strName))
    End Sub

    ''' <summary>Takes the name off the face; the name also leaves the photo's people field unless
    ''' another face of the photo still carries it.</summary>
    Public Sub Unname(ByVal face As FaceRegion)
        If face.PersonID = 0 Then Return
        Dim oldName As String = face.PersonName
        ' the name leaves the people field only when the user put it there through a face (named /
        ' confirmed). The program's matches never went into the field, and a seed's name was typed by
        ' hand (the one face found may even be somebody else in the photo).
        Dim wasNamed As Boolean = face.State = FaceRegion.enumFaceState.fsConfirmed
        Dim keep As Boolean = g_lpDatabase.PhotoHasPerson(face.FileName, face.PersonID, face.FaceID)
        g_lpDatabase.SetFacePerson(face.FaceID, 0, FaceRegion.enumFaceState.fsUnnamed)
        face.PersonID = 0
        face.PersonName = ""
        face.State = FaceRegion.enumFaceState.fsUnnamed
        If wasNamed AndAlso Not keep Then UpdateCharacter(g_lpDatabase, face.FileName, Function(s) FaceNames.Remove(s, oldName))
    End Sub

    ''' <summary>"This is not a face": the box is never shown again (its name, if any, is taken off first).</summary>
    Public Sub MarkNotFace(ByVal face As FaceRegion)
        Unname(face)
        g_lpDatabase.SetFacePerson(face.FaceID, 0, FaceRegion.enumFaceState.fsNotFace)
        face.State = FaceRegion.enumFaceState.fsNotFace
    End Sub

    ''' <summary>A face the user boxed (<paramref name="box"/> in fractions of the photo). Nothing when the
    ''' photo can't be read or was never analysed.</summary>
    Public Function AddManual(ByVal strFileDesc As String, ByVal box As RectangleF) As FaceRegion
        Dim f As Quartz.FaceEngine.Face = m_lpEngine.AnalyzeRegion(strFileDesc, box)
        If f Is Nothing Then Return Nothing
        Dim year As Integer = ShotYear(strFileDesc)
        Dim id As Integer = g_lpDatabase.AddManualFace(strFileDesc, f, year)
        If id = 0 Then Return Nothing
        Return New FaceRegion With {.FaceID = id, .FileName = strFileDesc, .Box = f.Box, .Score = f.Score, .Feature = f.Feature,
                                    .State = FaceRegion.enumFaceState.fsManual, .ShotYear = year}
    End Function

    '==================================================================================================
    ' People (UI thread; the face wall). Each change asks for a new sort in the background.
    '==================================================================================================
    ''' <summary>The face wall's picture folder (FaceCache next to iPhoto.mdb).</summary>
    Public ReadOnly Property CacheFolder As String
        Get
            Return IO.Path.Combine(IO.Path.GetDirectoryName(m_strDatabase), "FaceCache")
        End Get
    End Property

    ''' <summary>Names every face of <paramref name="faces"/> (a group from the face wall).</summary>
    Public Sub NameFaces(ByVal faces As IEnumerable(Of FaceRegion), ByVal strName As String)
        For Each f In faces
            Name(f, strName)
        Next
        m_lpCatalog?.Clusters.RemoveAll(Function(c) c.All(Function(f) f.IsNamed))
        RequestOrganize()
    End Sub

    ''' <summary>✓: the program's match / suggestion is right. Each face becomes confirmed and its name
    ''' goes into the photo's people field; confirmed faces feed the person's templates from now on.</summary>
    Public Sub ConfirmFaces(ByVal faces As IEnumerable(Of FaceRegion))
        For Each f In faces.Where(Function(x) x.PersonID <> 0 AndAlso x.PersonName <> "").ToList()
            Name(f, f.PersonName)
        Next
    End Sub

    ''' <summary>✕: the face is not the person it was matched / suggested / named as. Remembered
    ''' (FaceReject), so that person is never suggested for it again; the face goes back to unnamed (a
    ''' confirmed name also leaves the people field).</summary>
    Public Sub RejectFaces(ByVal faces As IEnumerable(Of FaceRegion))
        For Each f In faces.Where(Function(x) x.PersonID <> 0).ToList()
            g_lpDatabase.AddFaceReject(f.FaceID, f.PersonID)
            If f.State = FaceRegion.enumFaceState.fsConfirmed Then
                Unname(f)
            Else
                g_lpDatabase.SetFacePerson(f.FaceID, 0, FaceRegion.enumFaceState.fsUnnamed)
                f.PersonID = 0
                f.PersonName = ""
                f.State = FaceRegion.enumFaceState.fsUnnamed
            End If
        Next
    End Sub

    ''' <summary>Renames a person; when <paramref name="strNewName"/> is another person's name the two
    ''' become one (all faces go to that person). The name changes in every photo's people field that
    ''' has it (typed by hand too) and on the FaceIndex name card.</summary>
    Public Sub RenamePerson(ByVal p As FaceCatalog.PersonEntry, ByVal strNewName As String)
        strNewName = strNewName.Trim()
        If strNewName = "" OrElse strNewName = p.Name Then Return
        Dim target As Database.FacePersonInfo = g_lpDatabase.LoadFacePersons().
            FirstOrDefault(Function(x) x.PersonID <> p.PersonID AndAlso String.Equals(x.Name, strNewName, StringComparison.CurrentCultureIgnoreCase))
        If target IsNot Nothing Then
            g_lpDatabase.MergeFacePerson(p.PersonID, target.PersonID)
            strNewName = target.Name
        Else
            g_lpDatabase.RenameFacePerson(p.PersonID, strNewName)
        End If
        For Each file In PhotosOf(p)
            UpdateCharacter(g_lpDatabase, file, Function(s) FaceNames.Rename(s, p.Name, strNewName))
        Next
        g_lpDatabase.RenameFaceCover(p.Name, strNewName)
        RequestOrganize()
    End Sub

    Public Sub SetBirthYear(ByVal p As FaceCatalog.PersonEntry, ByVal intYear As Integer)
        g_lpDatabase.SetFacePersonBirthYear(p.PersonID, intYear)
        p.BirthYear = intYear
        RequestOrganize()
    End Sub

    ''' <summary>Hides a person from the face wall and from matching (the names stay in the photos).</summary>
    Public Sub HidePerson(ByVal p As FaceCatalog.PersonEntry)
        g_lpDatabase.SetFacePersonHidden(p.PersonID, True)
        p.Hidden = True
        RequestOrganize()
    End Sub

    ''' <summary>The person's photos: the ones with a face of theirs and the ones whose people field names
    ''' them (typed by hand, e.g. with the face turned away), oldest first.</summary>
    Public Function PhotosOf(ByVal p As FaceCatalog.PersonEntry) As String()
        Dim files As New HashSet(Of String)(p.Faces.Select(Function(f) f.FileName), StringComparer.OrdinalIgnoreCase)
        Dim byName() As String = Nothing
        g_lpDatabase.LoadFaceFile(p.Name, byName)
        For Each f In byName
            Dim ph As New Photo
            ph.Construct(f)
            If FaceNames.Contains(ph.Exif(enumPhotoExif.peCharacter), p.Name) Then files.Add(f)
        Next
        Dim list() As String = files.Where(Function(f) IO.File.Exists(f)).ToArray()
        SortPhotoFileByCreateDateTime(enumSortOrder.Ascending, list, list.Length)
        Return list
    End Function

    ''' <summary>Changes a photo's people field (.Exif) and its PhotoIndex row through <paramref name="db"/>
    ''' (g_lpDatabase on the UI thread, the scan thread's own one there).</summary>
    Private Sub UpdateCharacter(ByVal db As Database, ByVal strFileDesc As String, ByVal change As Func(Of String, String))
        If Not WriteNames Then Return
        Dim p As New Photo
        p.Construct(strFileDesc)
        Dim before As String = p.Exif(enumPhotoExif.peCharacter)
        Dim after As String = change(before)
        If after = before Then Return
        p.Exif(enumPhotoExif.peCharacter) = after
        If db IsNot Nothing AndAlso db.Implement Then db.SetPhotoCharacter(p)
    End Sub

End Class
