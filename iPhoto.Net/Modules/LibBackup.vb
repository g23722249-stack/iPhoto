' 同步備份 (the tool bar's imgToolBox(14), frmBackup): the whole 生活剪輯 folder mirrored to a backup drive
' or folder, so the copy works as it is on another computer (program, library, database, models).
'   Plan (Scan, nothing written)  a file the target lacks -> 新增; newer in the source, or another size ->
'                                 覆蓋; a file the target has and the source doesn't -> 刪除; else 略過
'                                 (two seconds apart is the same time: FAT / exFAT keep two-second times)
'   Run                           the old FolderSync Lost folder goes first, then the deletions (room
'                                 first), then the copies -- each to "~ibk~<name>" and renamed when
'                                 whole, so an unplugged drive never leaves half a file -- then folders
'                                 the source no longer has, and BackupInfoFile in the target
'   Skipped on both sides         temp files (*.tmp, ~$*, Thumbs.db), and System\ThumbCache when asked
'                                 (it can be made again); a ~ibk~* left by a broken-off run is deleted
'   Brake                         BackupPlan.ManyDeletions: frmBackup asks again before so many deletions
' The target is kept as the drive's volume label and serial plus the folder on it (BackupSettings), so
' a USB drive found under another letter is still the target, and another drive with the same letter
' isn't. Replaces the FolderSyncWPF-style backup of folder lists (moved deleted files to Lost).
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading

''' <summary>What a backup would do (Scan) and did (Run).</summary>
Friend Class BackupPlan
    Public Source As String = ""
    Public Target As String = ""
    Public ReadOnly Adds As New List(Of (Relative As String, Size As Long))
    Public ReadOnly Updates As New List(Of (Relative As String, Size As Long, OldSize As Long))
    Public ReadOnly Deletes As New List(Of (Relative As String, Size As Long))
    ''' <summary>Folders the target has and the source doesn't (deepest first).</summary>
    Public ReadOnly DeleteFolders As New List(Of String)
    ''' <summary>Folders the source has and the target doesn't (empty ones come with no file).</summary>
    Public ReadOnly NewFolders As New List(Of String)
    Public Skipped As Integer, SkippedBytes As Long
    ''' <summary>The old backup's Lost folder (FolderSync style), cleared first; "" when there is none.</summary>
    Public OldLost As String = ""
    Public OldLostFiles As Integer, OldLostBytes As Long
    ''' <summary>How many files the target held (for the deletion brake).</summary>
    Public TargetFiles As Integer

    Public ReadOnly Property CopyBytes As Long
        Get
            Return Adds.Sum(Function(a) a.Size) + Updates.Sum(Function(u) u.Size)
        End Get
    End Property

    ''' <summary>How much more room the target needs (what is copied, less what is overwritten or deleted).</summary>
    Public ReadOnly Property NeededBytes As Long
        Get
            Return Math.Max(0L, CopyBytes - Updates.Sum(Function(u) u.OldSize) - Deletes.Sum(Function(d) d.Size) - OldLostBytes)
        End Get
    End Property

    Public ReadOnly Property NothingToDo As Boolean
        Get
            Return Adds.Count = 0 AndAlso Updates.Count = 0 AndAlso Deletes.Count = 0 AndAlso DeleteFolders.Count = 0 AndAlso
                   NewFolders.Count = 0 AndAlso OldLost = ""
        End Get
    End Property

    ''' <summary>So many deletions that the source may not be what it should be (a drive not plugged in,
    ''' a folder emptied by mistake): more than a thousand, or more than a tenth of the backup.</summary>
    Public ReadOnly Property ManyDeletions As Boolean
        Get
            Return Deletes.Count > 1000 OrElse (TargetFiles >= 50 AndAlso Deletes.Count * 10 > TargetFiles)
        End Get
    End Property
End Class

''' <summary>What a run did.</summary>
Friend Class BackupOutcome
    Public Copied As Integer, Deleted As Integer, Failed As Integer, Bytes As Long
    Public ReadOnly Failures As New List(Of String)
End Class

Friend Module MirrorBackup

    Public Const BackupInfoFile As String = "iPhoto 備份紀錄.txt"
    ' the copy in progress: "~ibk~<name>" beside its target. When whole, the old target is deleted and the
    ' copy renamed into its place -- not renamed over it: security software (PC-cillin here) kills a
    ' program that renames files over existing photos, as ransomware does (tested: stopped after two)
    Private Const TempPrefix As String = "~ibk~"
    Private Const OldLostFolder As String = "Lost"
    Private Const ThumbCacheFolder As String = "System\ThumbCache"
    Private Const TimeSlack As Double = 2.0   ' seconds

    ''' <summary>Why the backup can't run now ("" = it can): the source there, the target's drive plugged in
    ''' and writable, the two apart.</summary>
    Public Function Problem(ByVal source As String, ByVal target As String) As String
        If String.IsNullOrWhiteSpace(source) OrElse Not Directory.Exists(source) Then Return "找不到要備份的資料夾：" & source
        If String.IsNullOrWhiteSpace(target) Then Return "還沒有選擇備份到哪裡。"
        Dim root As String
        Try
            root = Path.GetPathRoot(Path.GetFullPath(target))
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException OrElse TypeOf ex Is PathTooLongException
            Return "備份位置不正確：" & target
        End Try
        If String.IsNullOrEmpty(root) OrElse Not Directory.Exists(root) Then Return "備份的磁碟（或網路位置）沒有連接：" & root
        Dim s As String = Path.GetFullPath(source).TrimEnd("\"c) & "\"
        Dim t As String = Path.GetFullPath(target).TrimEnd("\"c) & "\"
        If t.StartsWith(s, StringComparison.OrdinalIgnoreCase) OrElse s.StartsWith(t, StringComparison.OrdinalIgnoreCase) Then
            Return "備份位置不能在生活剪輯裡面（或反過來）。"
        End If
        ' writable: a small file in the target (or its nearest existing parent)
        Dim probeDir As String = t.TrimEnd("\"c)
        While Not Directory.Exists(probeDir) AndAlso Path.GetDirectoryName(probeDir) IsNot Nothing
            probeDir = Path.GetDirectoryName(probeDir)
        End While
        Dim probe As String = Path.Combine(probeDir, "~ibk_probe_" & Guid.NewGuid().ToString("N") & ".tmp")
        Try
            File.WriteAllText(probe, "x")
            File.Delete(probe)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return "備份位置不能寫入（唯讀或沒有權限）：" & probeDir
        End Try
        Return ""
    End Function

    ''' <summary>Left out on both sides.</summary>
    Private Function Skip(ByVal relative As String, ByVal skipThumbs As Boolean) As Boolean
        Dim name As String = Path.GetFileName(relative)
        If name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) OrElse name.StartsWith("~$") OrElse
           name.StartsWith(TempPrefix, StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(name, "Thumbs.db", StringComparison.OrdinalIgnoreCase) Then Return True
        If skipThumbs AndAlso (relative.StartsWith(ThumbCacheFolder & "\", StringComparison.OrdinalIgnoreCase) OrElse
                               String.Equals(relative, ThumbCacheFolder, StringComparison.OrdinalIgnoreCase)) Then Return True
        Return False
    End Function

    Private ReadOnly Walk As New EnumerationOptions With {
        .RecurseSubdirectories = True, .IgnoreInaccessible = True, .AttributesToSkip = FileAttributes.ReparsePoint}

    ''' <summary>Compares the source with the target; writes nothing. <paramref name="report"/> gets how
    ''' many files were looked at.</summary>
    Public Function Scan(ByVal source As String, ByVal target As String, ByVal skipThumbs As Boolean,
                         ByVal report As Action(Of ProgressState), ByVal token As CancellationToken) As BackupPlan
        source = Path.GetFullPath(source).TrimEnd("\"c)
        target = Path.GetFullPath(target).TrimEnd("\"c)
        Dim plan As New BackupPlan With {.Source = source, .Target = target}
        Dim seen As Long = 0
        Dim tick = Sub(item As String)
                       seen += 1
                       If seen Mod 500 = 0 Then report?.Invoke(New ProgressState With {.Step = "比對檔案…", .Item = item, .Done = seen})
                   End Sub

        Dim src As New Dictionary(Of String, FileInfo)(StringComparer.OrdinalIgnoreCase)
        For Each fi In New DirectoryInfo(source).EnumerateFiles("*", Walk)
            token.ThrowIfCancellationRequested()
            Dim rel As String = fi.FullName.Substring(source.Length + 1)
            If Not Skip(rel, skipThumbs) Then src(rel) = fi
            tick(rel)
        Next
        Dim srcDirs As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each di In New DirectoryInfo(source).EnumerateDirectories("*", Walk)
            Dim rel As String = di.FullName.Substring(source.Length + 1)
            If Not Skip(rel, skipThumbs) Then srcDirs.Add(rel)
        Next

        Dim dst As New Dictionary(Of String, FileInfo)(StringComparer.OrdinalIgnoreCase)
        Dim dstDirs As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If Directory.Exists(target) Then
            Dim lost As String = Path.Combine(target, OldLostFolder)
            If Directory.Exists(lost) AndAlso Not srcDirs.Contains(OldLostFolder) Then
                plan.OldLost = lost
                For Each fi In New DirectoryInfo(lost).EnumerateFiles("*", Walk)
                    plan.OldLostFiles += 1
                    plan.OldLostBytes += fi.Length
                Next
            End If
            For Each fi In New DirectoryInfo(target).EnumerateFiles("*", Walk)
                token.ThrowIfCancellationRequested()
                Dim rel As String = fi.FullName.Substring(target.Length + 1)
                If plan.OldLost <> "" AndAlso rel.StartsWith(OldLostFolder & "\", StringComparison.OrdinalIgnoreCase) Then Continue For
                If String.Equals(rel, BackupInfoFile, StringComparison.OrdinalIgnoreCase) Then Continue For
                If Skip(rel, skipThumbs) AndAlso Not Path.GetFileName(rel).StartsWith(TempPrefix, StringComparison.OrdinalIgnoreCase) Then Continue For
                dst(rel) = fi
                tick(rel)
            Next
            For Each di In New DirectoryInfo(target).EnumerateDirectories("*", Walk)
                Dim rel As String = di.FullName.Substring(target.Length + 1)
                If plan.OldLost <> "" AndAlso (String.Equals(rel, OldLostFolder, StringComparison.OrdinalIgnoreCase) OrElse
                                               rel.StartsWith(OldLostFolder & "\", StringComparison.OrdinalIgnoreCase)) Then Continue For
                If Not Skip(rel, skipThumbs) Then dstDirs.Add(rel)
            Next
        End If
        plan.TargetFiles = dst.Count

        For Each kv In src.OrderBy(Function(x) x.Key, StringComparer.OrdinalIgnoreCase)
            Dim t As FileInfo = Nothing
            If Not dst.TryGetValue(kv.Key, t) Then
                plan.Adds.Add((kv.Key, kv.Value.Length))
            ElseIf kv.Value.Length <> t.Length OrElse (kv.Value.LastWriteTimeUtc - t.LastWriteTimeUtc).TotalSeconds > TimeSlack Then
                plan.Updates.Add((kv.Key, kv.Value.Length, t.Length))
            Else
                plan.Skipped += 1
                plan.SkippedBytes += kv.Value.Length
            End If
        Next
        For Each kv In dst
            If Not src.ContainsKey(kv.Key) Then plan.Deletes.Add((kv.Key, kv.Value.Length))
        Next
        plan.DeleteFolders.AddRange(dstDirs.Where(Function(d) Not srcDirs.Contains(d)).OrderByDescending(Function(d) d.Length))
        plan.NewFolders.AddRange(srcDirs.Where(Function(d) Not dstDirs.Contains(d)).OrderBy(Function(d) d.Length))
        Return plan
    End Function

    ''' <summary>Does what <paramref name="plan"/> says; a file that fails is listed and skipped.</summary>
    Public Function Run(ByVal plan As BackupPlan, ByVal report As Action(Of ProgressState), ByVal token As CancellationToken) As BackupOutcome
        Dim outcome As New BackupOutcome
        Dim total As Long = plan.CopyBytes
        Dim files As Long = plan.Adds.Count + plan.Updates.Count
        Dim state As New ProgressState With {.Total = files, .BytesTotal = total}
        Directory.CreateDirectory(plan.Target)

        ' 1. the old Lost folder, then what the source no longer has (room first)
        If plan.OldLost <> "" Then
            state.Step = "清除舊的 Lost 資料夾…" : state.Item = plan.OldLost : report(state)
            Try
                ClearAttributes(plan.OldLost)
                Directory.Delete(plan.OldLost, recursive:=True)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                outcome.Failed += 1 : outcome.Failures.Add("Lost：" & ex.Message)
            End Try
        End If
        state.Step = "刪除來源已經沒有的檔案…"
        For Each d In plan.Deletes
            token.ThrowIfCancellationRequested()
            state.Item = d.Relative : report(state)
            Dim f As String = Path.Combine(plan.Target, d.Relative)
            Try
                If File.Exists(f) Then
                    File.SetAttributes(f, FileAttributes.Normal)
                    File.Delete(f)
                End If
                outcome.Deleted += 1
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                outcome.Failed += 1 : outcome.Failures.Add(d.Relative & "：" & ex.Message)
            End Try
        Next

        ' 2. new and changed files
        state.Step = "複製中…"
        Dim buffer(1024 * 1024 - 1) As Byte
        For Each c In plan.Adds.Select(Function(a) (a.Relative, a.Size)).Concat(plan.Updates.Select(Function(u) (u.Relative, u.Size)))
            token.ThrowIfCancellationRequested()
            Dim before As Long = state.BytesDone
            state.Item = c.Relative : state.ItemDone = 0 : state.ItemTotal = c.Size
            report(state)
            Try
                CopyOne(Path.Combine(plan.Source, c.Relative), Path.Combine(plan.Target, c.Relative), buffer,
                        Sub(n)
                            state.ItemDone = n
                            state.BytesDone = before + n
                            report(state)
                        End Sub, token)
                outcome.Copied += 1
                outcome.Bytes += c.Size
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                outcome.Failed += 1 : outcome.Failures.Add(c.Relative & "：" & ex.Message)
            End Try
            state.BytesDone = before + c.Size
            state.Done += 1
        Next

        ' 3. folders: the new (empty) ones, and the ones the source no longer has
        For Each d In plan.NewFolders
            Directory.CreateDirectory(Path.Combine(plan.Target, d))
        Next
        state.Step = "整理資料夾…" : state.Item = "" : state.ItemTotal = 0 : report(state)
        For Each d In plan.DeleteFolders
            Dim dir As String = Path.Combine(plan.Target, d)
            Try
                If Directory.Exists(dir) Then
                    ClearAttributes(dir)
                    Directory.Delete(dir, recursive:=True)
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                outcome.Failed += 1 : outcome.Failures.Add(d & "\：" & ex.Message)
            End Try
        Next

        ' 4. the record
        Try
            File.WriteAllLines(Path.Combine(plan.Target, BackupInfoFile), {
                "iPhoto 同步備份",
                "時間：" & Now.ToString("yyyy/MM/dd HH:mm:ss"),
                "來源：" & plan.Source,
                $"複製 {outcome.Copied} 個（{frmProgress.SizeText(outcome.Bytes)}）、刪除 {outcome.Deleted} 個、沒有異動 {plan.Skipped} 個、失敗 {outcome.Failed} 個"}.
                Concat(outcome.Failures.Take(200).Select(Function(x) "失敗：" & x)), Text.Encoding.UTF8)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return outcome
    End Function

    ''' <summary>A file copied to "~ibk~<name>" beside its target and renamed when whole, with the source's times. The
    ''' source is opened shared for writing too (the database is open in iPhoto).</summary>
    Private Sub CopyOne(ByVal source As String, ByVal target As String, ByVal buffer As Byte(), ByVal progress As Action(Of Long), ByVal token As CancellationToken)
        Directory.CreateDirectory(Path.GetDirectoryName(target))
        Dim temp As String = Path.Combine(Path.GetDirectoryName(target), TempPrefix & Path.GetFileName(target))
        Try
            Using input As New FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete, buffer.Length, FileOptions.SequentialScan),
                  output As New FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.SequentialScan)
                Dim done As Long = 0
                Dim lastReport As Long = 0
                Do
                    token.ThrowIfCancellationRequested()
                    Dim n As Integer = input.Read(buffer, 0, buffer.Length)
                    If n <= 0 Then Exit Do
                    output.Write(buffer, 0, n)
                    done += n
                    If done - lastReport >= 4 * 1024 * 1024 Then
                        progress(done)
                        lastReport = done
                    End If
                Loop
                progress(done)
            End Using
            Dim info As New FileInfo(source)
            File.SetLastWriteTimeUtc(temp, info.LastWriteTimeUtc)
            File.SetCreationTimeUtc(temp, info.CreationTimeUtc)
            If File.Exists(target) Then
                File.SetAttributes(target, FileAttributes.Normal)
                File.Delete(target)   ' then the copy into its place (not renamed over it, see TempPrefix)
            End If
            File.Move(temp, target)
        Catch
            Try
                If File.Exists(temp) Then File.Delete(temp)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
            Throw
        End Try
    End Sub

    Private Sub ClearAttributes(ByVal dir As String)
        For Each f In New DirectoryInfo(dir).EnumerateFiles("*", Walk)
            If (f.Attributes And FileAttributes.ReadOnly) <> 0 Then f.Attributes = FileAttributes.Normal
        Next
    End Sub

End Module

''' <summary>The drives a backup can go to, and finding one again by its volume label and serial.</summary>
Friend Module BackupDrives

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Function GetVolumeInformation(ByVal rootPath As String, ByVal volumeName As Text.StringBuilder, ByVal volumeNameSize As Integer,
                                          ByRef serial As UInteger, ByRef maxComponent As UInteger, ByRef flags As UInteger,
                                          ByVal fileSystem As Text.StringBuilder, ByVal fileSystemSize As Integer) As Boolean
    End Function

    Public Class DriveChoice
        Public Root As String = ""          ' E:\
        Public Label As String = ""
        Public Serial As String = ""        ' hex
        Public Removable As Boolean
        Public Network As Boolean
        Public Free As Long
        Public Total As Long
    End Class

    ''' <summary>The serial of the volume <paramref name="root"/> is on ("" when it can't be read).</summary>
    Public Function SerialOf(ByVal root As String) As String
        Dim serial As UInteger, maxComp As UInteger, flags As UInteger
        If GetVolumeInformation(root, New Text.StringBuilder(261), 261, serial, maxComp, flags, New Text.StringBuilder(261), 261) Then Return serial.ToString("X8")
        Return ""
    End Function

    ''' <summary>The ready drives except <paramref name="excludeRoot"/> (the source's).</summary>
    Public Function List(ByVal excludeRoot As String) As List(Of DriveChoice)
        Dim result As New List(Of DriveChoice)
        For Each d In DriveInfo.GetDrives()
            Try
                If Not d.IsReady OrElse d.DriveType = DriveType.CDRom Then Continue For
                If String.Equals(d.RootDirectory.FullName, excludeRoot, StringComparison.OrdinalIgnoreCase) Then Continue For
                result.Add(New DriveChoice With {
                    .Root = d.RootDirectory.FullName, .Label = d.VolumeLabel, .Serial = SerialOf(d.RootDirectory.FullName),
                    .Removable = d.DriveType = DriveType.Removable, .Network = d.DriveType = DriveType.Network,
                    .Free = d.AvailableFreeSpace, .Total = d.TotalSize})
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        Return result
    End Function

    ''' <summary>The root of the volume with <paramref name="serial"/> now ("" when it isn't plugged in).</summary>
    Public Function FindBySerial(ByVal serial As String) As String
        If serial = "" Then Return ""
        For Each d In DriveInfo.GetDrives()
            Try
                If d.IsReady AndAlso SerialOf(d.RootDirectory.FullName) = serial Then Return d.RootDirectory.FullName
            Catch ex As IOException
            End Try
        Next
        Return ""
    End Function

    ''' <summary>Free space where <paramref name="folder"/> is (-1 when unknown, e.g. a share).</summary>
    Public Function FreeSpace(ByVal folder As String) As Long
        Try
            Dim root As String = Path.GetPathRoot(Path.GetFullPath(folder))
            If root.StartsWith("\\") Then Return -1
            Return New DriveInfo(root).AvailableFreeSpace
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is UnauthorizedAccessException
            Return -1
        End Try
    End Function

End Module

''' <summary>同步備份's choices, kept with iPhoto's other settings (HKCU\Software\VB and VBA Program
''' Settings\iPhoto\Backup): the source, the target as volume label + serial + the folder on it (or a
''' plain folder, a share), what to skip, the last run.</summary>
Friend Module BackupSettings

    Private Const AppName As String = "iPhoto"
    Private Const Section As String = "Backup"

    ''' <summary>The folder chosen, else the library's: the one holding the database's System folder
    ''' (D:\生活剪輯 -- also when iPhoto runs from its build folder), else where iPhoto is.</summary>
    Friend Function Source() As String
        Dim s As String = GetSetting(AppName, Section, "MirrorSource", "")
        If s <> "" AndAlso Directory.Exists(s) Then Return s
        Try
            Dim db As String = If(g_lpConfig?.Attached(Config.enumAttachedFile.filDatabase), "")
            If db <> "" Then
                Dim d As DirectoryInfo = New FileInfo(Path.GetFullPath(db)).Directory
                While d IsNot Nothing
                    If String.Equals(d.Name, "System", StringComparison.OrdinalIgnoreCase) AndAlso d.Parent IsNot Nothing Then Return d.Parent.FullName.TrimEnd("\"c)
                    d = d.Parent
                End While
            End If
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException OrElse TypeOf ex Is PathTooLongException
        End Try
        Return Application.StartupPath.TrimEnd("\"c)
    End Function

    Friend Sub SetSource(ByVal value As String)
        SaveSetting(AppName, Section, "MirrorSource", value)
    End Sub

    ''' <summary>The target now: on the drive with the saved serial wherever it is plugged in; a plain
    ''' folder as it is. "" when none was chosen. <paramref name="missing"/>: the drive isn't plugged in.</summary>
    Friend Function Target(ByRef label As String, ByRef missing As Boolean) As String
        missing = False
        label = GetSetting(AppName, Section, "TargetLabel", "")
        Dim serial As String = GetSetting(AppName, Section, "TargetSerial", "")
        Dim folder As String = GetSetting(AppName, Section, "TargetFolder", "")
        If serial = "" Then Return folder
        Dim root As String = BackupDrives.FindBySerial(serial)
        If root = "" Then
            missing = True
            Return ""
        End If
        Return Path.Combine(root, GetSetting(AppName, Section, "TargetRelative", ""))
    End Function

    ''' <summary>The target: a drive's folder (kept by its serial) or any folder (serial "").</summary>
    Friend Sub SetTarget(ByVal folder As String, ByVal serial As String, ByVal label As String)
        Dim root As String = Path.GetPathRoot(folder)
        SaveSetting(AppName, Section, "TargetFolder", folder)
        SaveSetting(AppName, Section, "TargetSerial", serial)
        SaveSetting(AppName, Section, "TargetLabel", label)
        SaveSetting(AppName, Section, "TargetRelative", If(serial = "", "", folder.Substring(root.Length)))
    End Sub

    Friend Property SkipThumbs As Boolean
        Get
            Return GetSetting(AppName, Section, "SkipThumbs", "N") = "Y"
        End Get
        Set(value As Boolean)
            SaveSetting(AppName, Section, "SkipThumbs", If(value, "Y", "N"))
        End Set
    End Property

    Friend Property LastRun As String
        Get
            Return GetSetting(AppName, Section, "LastRun", "")
        End Get
        Set(value As String)
            SaveSetting(AppName, Section, "LastRun", value)
        End Set
    End Property

End Module
