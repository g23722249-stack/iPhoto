Imports System.Threading

' 輸入照片 (frmImportStudio): does the import on a worker thread, album by album, photo by photo:
'   0. the new class folder with its olyalbum.inf and Note.Ini (as frmImport wrote them);
'   1. copy the file;
'   2. write its .Exif ([Create] / [Exif] keys exactly as frmMain.ImportAlbumFiles wrote them, but each
'      photo with its own title / people / place / keywords / remark);
'   3. its PhotoIndex row;
'   4. its faces (FacePhoto / FaceRegion under the new path), so they aren't analysed again.
' Usually one album (job); 「每天一本相簿」 gives one job per day. Every folder is checked before the
' first is made. A photo that fails is logged and skipped; the next one goes on. Pause waits between
' photos, Cancel stops after the photo in hand -- what was imported stays. The thread has its own
' Database: an OleDbConnection can't be shared between threads.
Public Class ImportRunner

    Public Enum enumPhase
        phFolder = 0
        phCopy = 1
        phExif = 2
        phIndex = 3
        phFaces = 4
    End Enum

    Public Class Failure
        Public Job As ImportJob
        Public Item As ImportItem
        Public Message As String = ""
    End Class

    ''' <summary>Raised on the UI thread after each photo (and once at the start).</summary>
    Public Event Progress As EventHandler
    ''' <summary>Raised on the UI thread for each line of the log.</summary>
    Public Event LogLine(ByVal strLine As String, ByVal bolWarning As Boolean)
    ''' <summary>Raised on the UI thread when the import ends (finished, cancelled or failed).</summary>
    Public Event Finished As EventHandler

    Private ReadOnly m_jobs As List(Of ImportJob)
    Private ReadOnly m_strDatabase As String
    Private m_context As SynchronizationContext
    Private m_thread As Thread
    Private ReadOnly m_resume As New ManualResetEventSlim(True)
    Private m_bolCancel As Boolean
    Private ReadOnly m_exifWritten As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly m_made As New HashSet(Of ImportJob)

    ''' <summary>Write PhotoIndex (iPhoto has a database) / the faces (the face tables exist).</summary>
    Public Property WriteIndex As Boolean = True
    Public Property WriteFaces As Boolean = True

    Public ReadOnly Counts(4) As Integer
    Public ReadOnly Property Total As Integer
    Public ReadOnly Property Done As Integer
    Public ReadOnly Property Current As ImportItem
    Public ReadOnly Property CurrentJob As ImportJob
    Public ReadOnly Imported As New List(Of ImportItem)
    Public ReadOnly Failures As New List(Of Failure)
    Public ReadOnly Property Cancelled As Boolean
    ''' <summary>Why the import couldn't start / stopped (a folder exists, can't be made ...); Nothing when fine.</summary>
    Public ReadOnly Property FatalError As String
    Public ReadOnly Property StartTime As DateTime

    Public Sub New(ByVal jobs As IEnumerable(Of ImportJob), ByVal strDatabase As String)
        m_jobs = jobs.ToList()
        m_strDatabase = strDatabase
    End Sub

    Public Sub New(ByVal job As ImportJob, ByVal strDatabase As String)
        Me.New({job}, strDatabase)
    End Sub

    Public ReadOnly Property Jobs As List(Of ImportJob)
        Get
            Return m_jobs
        End Get
    End Property

    ''' <summary>The albums whose folder was made.</summary>
    Public ReadOnly Property MadeJobs As List(Of ImportJob)
        Get
            Return m_jobs.Where(Function(j) m_made.Contains(j)).ToList()
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

    ''' <summary>Starts the import (UI thread: the events are raised on it). <paramref name="retry"/> True
    ''' imports again only the photos that failed (「重試失敗的」).</summary>
    Public Sub Start(Optional ByVal retry As Boolean = False)
        m_context = If(SynchronizationContext.Current, New SynchronizationContext())
        Dim work As List(Of (Job As ImportJob, Item As ImportItem)) = WorkList(retry)
        m_thread = New Thread(Sub() Run(work)) With {.IsBackground = True, .Name = "ImportRunner"}
        m_thread.Start()
    End Sub

    ''' <summary>Runs on the calling thread (tests).</summary>
    Public Sub RunNow(Optional ByVal retry As Boolean = False)
        m_context = New SynchronizationContext()
        Run(WorkList(retry))
    End Sub

    Private Function WorkList(ByVal retry As Boolean) As List(Of (Job As ImportJob, Item As ImportItem))
        m_bolCancel = False
        _Cancelled = False
        _FatalError = Nothing
        m_resume.Set()
        Dim work As New List(Of (Job As ImportJob, Item As ImportItem))
        If retry Then
            For Each f In Failures
                work.Add((f.Job, f.Item))
            Next
            Failures.Clear()
        Else
            For Each j In m_jobs
                ' a video sharing its .Exif with a picture comes after it
                For Each it In j.Included.OrderBy(Function(i) If(i.IsVideo AndAlso i.SharesExifWith <> "", 1, 0))
                    work.Add((j, it))
                Next
            Next
        End If
        _Total = work.Count
        _Done = 0
        Array.Clear(Counts, 0, Counts.Length)
        _StartTime = DateTime.Now
        Return work
    End Function

    Public Sub Pause()
        m_resume.Reset()
    End Sub

    Public Sub [Resume]()
        m_resume.Set()
    End Sub

    Public Sub Cancel()
        m_bolCancel = True
        m_resume.Set()
    End Sub

    ''' <summary>Seconds still to go, from the pace so far (-1 until there is a pace).</summary>
    Public Function SecondsLeft() As Integer
        If _Done <= 0 OrElse _Total <= _Done Then Return -1
        Dim per As Double = (DateTime.Now - _StartTime).TotalSeconds / _Done
        Return CInt(Math.Ceiling(per * (_Total - _Done)))
    End Function

    '==================================================================================================
    ' Worker thread
    '==================================================================================================
    Private Sub Run(ByVal work As List(Of (Job As ImportJob, Item As ImportItem)))
        Dim db As Database = Nothing
        Try
            Post(Sub() RaiseEvent Progress(Me, EventArgs.Empty))
            ' every new folder is checked before the first one is made
            For Each j In work.Select(Function(w) w.Job).Distinct().Where(Function(x) Not m_made.Contains(x))
                If IO.Directory.Exists(j.ClassPath) Then
                    _FatalError = "相簿資料夾已經存在：" & j.ClassPath
                    Return
                End If
            Next

            If WriteIndex OrElse WriteFaces Then
                db = New Database
                db.Construct(m_strDatabase)
                If Not db.Implement Then
                    Log("無法開啟資料庫，只複製照片與寫入 .Exif", True)
                    db.Dispose()
                    db = Nothing
                End If
            End If

            For Each w In work
                m_resume.Wait()
                If m_bolCancel Then
                    _Cancelled = True
                    Exit For
                End If
                _CurrentJob = w.Job
                _Current = w.Item
                If Not m_made.Contains(w.Job) Then
                    IO.Directory.CreateDirectory(w.Job.ClassPath)
                    WriteAlbumFiles(w.Job, w.Job.ClassPath)
                    m_made.Add(w.Job)
                    Counts(enumPhase.phFolder) += 1
                    Log(Stamp() & " 建立相簿 " & w.Job.Folder & "（" & w.Job.Title & "）", False)
                End If
                Try
                    ImportOne(db, w.Job, w.Item)
                    Imported.Add(w.Item)
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is InvalidOperationException OrElse
                                           TypeOf ex Is ArgumentException
                    Failures.Add(New Failure With {.Job = w.Job, .Item = w.Item, .Message = ex.Message})
                    Log(Stamp() & " " & w.Item.FileName & " 失敗：" & ex.Message, True)
                End Try
                _Done += 1
                Post(Sub() RaiseEvent Progress(Me, EventArgs.Empty))
            Next
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is Data.OleDb.OleDbException OrElse TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is NotSupportedException
            _FatalError = ex.Message
        Finally
            db?.Dispose()
            _Current = Nothing
            Post(Sub() RaiseEvent Finished(Me, EventArgs.Empty))
        End Try
    End Sub

    Private Shared Function Stamp() As String
        Return DateTime.Now.ToString("HH:mm:ss")
    End Function

    Private Sub ImportOne(ByVal db As Database, ByVal job As ImportJob, ByVal it As ImportItem)
        Dim dest As String = job.ClassPath & "\" & it.FileName
        Dim notes As New List(Of String)

        ' 1. copy (a retry finds the file already there)
        If Not IO.File.Exists(dest) Then IO.File.Copy(it.SourceFile, dest, False)
        Counts(enumPhase.phCopy) += 1

        ' 2. .Exif -- IMG_1.JPG and IMG_1.MOV share IMG_1.Exif: the first one written (pictures go first)
        '    keeps it, the other is not allowed to overwrite it
        Dim exif As String = IO.Path.ChangeExtension(dest, gc_strExifPattern)
        If m_exifWritten.Contains(exif) Then
            notes.Add("與同名檔案共用 .Exif，保留先寫入的資料")
        Else
            WriteExif(job, it, dest)
            m_exifWritten.Add(exif)
        End If
        Counts(enumPhase.phExif) += 1

        ' 3. PhotoIndex
        If db IsNot Nothing AndAlso WriteIndex Then
            Dim p As New Photo
            p.Construct(dest)
            db.WriteImportedPhoto(p)
            Counts(enumPhase.phIndex) += 1
        End If

        ' 4. faces
        If db IsNot Nothing AndAlso WriteFaces AndAlso it.IsPicture AndAlso it.Faces IsNot Nothing AndAlso db.FaceTablesReady Then
            Dim info As New IO.FileInfo(dest)
            Dim year As Integer = If(it.ShotDate.HasValue, it.ShotDate.Value.Year, info.LastWriteTime.Year)
            db.SaveImportedFaces(dest, info.Length, info.LastWriteTime, year, it.Faces)
            Counts(enumPhase.phFaces) += 1
            Dim n As Integer = it.VisibleFaces.Count()
            If n > 0 Then notes.Add(n & " 張臉")
        End If
        If it.IsVideo Then notes.Add("影片")
        Log(Stamp() & " " & it.FileName & " ✓" & If(notes.Count > 0, " " & String.Join("、", notes), ""), False)
    End Sub

    ''' <summary>The photo's .Exif next to <paramref name="strDest"/> (keys as frmMain.ImportAlbumFiles).</summary>
    Public Shared Sub WriteExif(ByVal job As ImportJob, ByVal it As ImportItem, ByVal strDest As String)
        Dim fs As New Carbon.FileSystem
        Dim strDateTime As String = If(it.ShotDateTime <> "", it.ShotDateTime, GetExifFileDateTime(fs, strDest))
        strDateTime = strDateTime.PadRight(14, "0"c)
        Dim ini As New Carbon.IniFile With {.FileName = IO.Path.ChangeExtension(strDest, gc_strExifPattern)}
        With ini
            .SimpleSetValue("Create", "Date", Mid(strDateTime, 1, 8))
            .SimpleSetValue("Create", "Time", Mid(strDateTime, 9, 6))
            .SimpleSetValue("Exif", "Date", Mid(strDateTime, 1, 8))
            .SimpleSetValue("Exif", "Time", Mid(strDateTime, 9, 6))
            .SimpleSetValue("Exif", "Title", it.Title)
            .SimpleSetValue("Exif", "Character", it.Character())
            If it.Gps <> "" Then .SimpleSetValue("Exif", "GPS", it.Gps)
            If it.Country <> "" Then .SimpleSetValue("Exif", "Country", it.Country)
            If it.City <> "" Then .SimpleSetValue("Exif", "City", it.City)
            If it.Town <> "" Then .SimpleSetValue("Exif", "Town", it.Town)
            .SimpleSetValue("Exif", "Spot", it.SpotFor(job.Spot))
            .SimpleSetValue("Exif", "KeyWord", it.KeyWord)
            .SimpleSetValue("Exif", "Remark", it.Remark)
        End With
    End Sub

    ''' <summary>olyalbum.inf and Note.Ini of the new class folder (as frmImport wrote them).</summary>
    Public Shared Sub WriteAlbumFiles(ByVal job As ImportJob, ByVal strFolder As String)
        Dim oly As New Carbon.IniFile With {.FileName = strFolder & "\" & gc_strOlympus_camedia_inf}
        oly.SimpleSetValue("AlbumInfo", "AlbumType", "2")

        Dim note As String = strFolder & "\" & gc_strNote
        If IO.File.Exists(note) Then IO.File.Delete(note)
        Dim ini As New Carbon.IniFile With {.FileName = note}
        ini.SimpleSetValue("Create", "Date", Date.Now.ToString("yyyyMMdd"))
        ini.SimpleSetValue("Create", "Time", Date.Now.ToString("HHmmss"))
        ini.SimpleSetValue("Note", "Title", job.Title.Trim())
        ini.SimpleSetValue("Note", "Description", job.Title.Trim())
        ini.SimpleSetValue("Note", "Date", job.DateText.Trim())
        ini.SimpleSetValue("Note", "Spot", job.Spot.Trim())
        ini.SimpleSetValue("Note", "Icon", If(job.Icon.Trim() = "", "icoPeople", job.Icon.Trim()))
        ini.SimpleSetValue("Note", "Remark", job.Remark.Replace(vbCrLf, vbTab))
    End Sub

    Private Sub Log(ByVal strLine As String, ByVal bolWarning As Boolean)
        Post(Sub() RaiseEvent LogLine(strLine, bolWarning))
    End Sub

    Private Sub Post(ByVal action As Action)
        Try
            m_context.Post(Sub(state) action(), Nothing)
        Catch ex As InvalidOperationException
            ' the window is gone
        End Try
    End Sub

End Class
