' 同步備份 (new in the .NET version; the tool bar's imgToolBox(14) opens frmBackup). The rules come from
' the stand-alone FolderSyncWPF tool, done here in iPhoto's own style:
'   - one way, source -> target. A source keeps its path below the target without the drive letter:
'     D:\照片\黏土 -> <target>\照片\黏土 (a whole drive D:\ -> <target>\D, a share \\nas\photo\x ->
'     <target>\nas_photo\x);
'   - a file is copied when the target has none, or when the source's is newer;
'   - whatever the target has that the source no longer has is not deleted but moved to
'     <target root>\Lost\yyyyMMdd\<its relative path>;
'   - every run writes sync_log.txt in the target root.
' Fixed from FolderSyncWPF: files deleted in sub folders are moved to Lost too (it only looked at the
' top level); the reported target folder is the real one (it named <target>\<last folder> instead);
' an empty source no longer divides by zero; the list is kept in the registry with iPhoto's other
' settings instead of sources.json next to wherever the program was started from.
Imports System.IO
Imports System.Threading

''' <summary>One source folder and where it is backed up to.</summary>
Friend Class BackupSource
    Public Property SourcePath As String = ""
    Public Property TargetPath As String = ""
End Class

''' <summary>Progress of a backup run: done / total steps and what was just done (may be "").</summary>
Friend Structure BackupProgress
    Public Done As Integer
    Public Total As Integer
    Public Message As String
    Public ReadOnly Property Percent As Integer
        Get
            If Total <= 0 Then Return 100
            Return Math.Min(100, CInt(Done * 100L \ Total))
        End Get
    End Property
End Structure

''' <summary>What one source's run did.</summary>
Friend Class BackupResult
    Public Property TargetRoot As String = ""
    Public Property LogFile As String = ""
    Public Property Added As Integer
    Public Property Updated As Integer
    Public Property MovedToLost As Integer
    Public Property Failed As Integer
    Public ReadOnly Property Changes As Integer
        Get
            Return Added + Updated + MovedToLost
        End Get
    End Property
End Class

Friend NotInheritable Class FolderBackup

    Public Const LogFileName As String = "sync_log.txt"
    Public Const LostFolderName As String = "Lost"

    Private Sub New()
    End Sub

    ''' <summary>Where <paramref name="source"/> goes below <paramref name="target"/> (see the header).</summary>
    Public Shared Function TargetRootFor(ByVal source As String, ByVal target As String) As String
        source = Path.GetFullPath(source)
        Dim root As String = Path.GetPathRoot(source)
        Dim relative As String = source.Substring(root.Length).Trim(Path.DirectorySeparatorChar)
        Dim isDrive As Boolean = root.Length >= 2 AndAlso root(1) = ":"c
        Dim rootName As String
        If isDrive Then
            rootName = root.Substring(0, 1)
        Else
            rootName = root.Trim("\"c).Replace("\"c, "_"c)
            If rootName = "" Then rootName = "UNC"
        End If
        If relative = "" Then Return Path.Combine(target, rootName)          ' a whole drive / share
        If isDrive Then Return Path.Combine(target, relative)
        Return Path.Combine(target, rootName, relative)
    End Function

    ''' <summary>Why the pair can't be backed up, or "" when it can.</summary>
    Public Shared Function Problem(ByVal source As String, ByVal target As String) As String
        If String.IsNullOrWhiteSpace(source) Then Return "沒有指定來源資料夾"
        If Not Directory.Exists(source) Then Return "來源資料夾不存在：" & source
        If String.IsNullOrWhiteSpace(target) Then Return "沒有指定目的資料夾：" & source
        Dim s As String = Path.GetFullPath(source).TrimEnd("\"c) & "\"
        Dim t As String = Path.GetFullPath(TargetRootFor(source, target)).TrimEnd("\"c) & "\"
        If t.StartsWith(s, StringComparison.OrdinalIgnoreCase) OrElse s.StartsWith(t, StringComparison.OrdinalIgnoreCase) Then
            Return "目的不能在來源資料夾裡面（或反過來）：" & source
        End If
        Return ""
    End Function

    ''' <summary>Backs <paramref name="source"/> up below <paramref name="target"/>. Runs on the calling
    ''' thread (call it from a background task); reports through <paramref name="progress"/>.</summary>
    Public Shared Function Run(ByVal source As String, ByVal target As String,
                               ByVal progress As IProgress(Of BackupProgress), ByVal token As CancellationToken) As BackupResult
        Dim result As New BackupResult()
        source = Path.GetFullPath(source).TrimEnd("\"c)
        Dim targetRoot As String = TargetRootFor(source, target)
        result.TargetRoot = targetRoot
        result.LogFile = Path.Combine(targetRoot, LogFileName)
        Dim log As New Text.StringBuilder()
        Dim state As New BackupProgress()
        Dim report = Sub(msg As String)
                         If msg <> "" Then log.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & " - " & msg)
                         state.Message = msg
                         If progress IsNot Nothing Then progress.Report(state)
                     End Sub

        Dim sourceFiles As String() = Directory.GetFiles(source, "*", SearchOption.AllDirectories)
        If Not Directory.Exists(targetRoot) Then
            Directory.CreateDirectory(targetRoot)
            report("建立目的資料夾：" & targetRoot)
        End If
        Dim targetFiles As Integer = Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories).Length
        state.Total = sourceFiles.Length + targetFiles
        report("")

        ' 1. new and newer files (folders come with their files; empty ones below)
        For Each file As String In sourceFiles
            token.ThrowIfCancellationRequested()
            Dim relative As String = file.Substring(source.Length + 1)
            Dim dest As String = Path.Combine(targetRoot, relative)
            Try
                Dim srcInfo As New FileInfo(file)
                Dim dstInfo As New FileInfo(dest)
                If Not dstInfo.Exists Then
                    Directory.CreateDirectory(Path.GetDirectoryName(dest))
                    IO.File.Copy(file, dest, overwrite:=False)
                    result.Added += 1
                    state.Done += 1 : report("新增檔案：" & relative)
                    Continue For
                ElseIf srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc Then
                    If dstInfo.IsReadOnly Then dstInfo.IsReadOnly = False
                    IO.File.Copy(file, dest, overwrite:=True)
                    result.Updated += 1
                    state.Done += 1 : report("覆蓋檔案：" & relative)
                    Continue For
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                result.Failed += 1
                state.Done += 1 : report("失敗：" & relative & "（" & ex.Message & "）")
                Continue For
            End Try
            state.Done += 1 : report("")
        Next
        For Each dir As String In Directory.GetDirectories(source, "*", SearchOption.AllDirectories)
            Dim dest As String = Path.Combine(targetRoot, dir.Substring(source.Length + 1))
            If Not Directory.Exists(dest) Then Directory.CreateDirectory(dest)
        Next

        ' 2. what the source no longer has -> Lost\yyyyMMdd, at every level (FolderSyncWPF only looked at the top)
        Dim lostRoot As String = Path.Combine(targetRoot, LostFolderName, DateTime.Now.ToString("yyyyMMdd"))
        MoveExtras(source, targetRoot, targetRoot, lostRoot, result, state, report, token)

        IO.File.WriteAllText(result.LogFile, log.ToString())
        state.Done = state.Total : report("")
        Return result
    End Function

    Private Shared Sub MoveExtras(ByVal source As String, ByVal targetRoot As String, ByVal targetDir As String, ByVal lostRoot As String,
                                  ByVal result As BackupResult, ByRef state As BackupProgress, ByVal report As Action(Of String),
                                  ByVal token As CancellationToken)
        Dim relDir As String = targetDir.Substring(targetRoot.Length).TrimStart("\"c)
        Dim atRoot As Boolean = (relDir = "")
        For Each file As String In Directory.GetFiles(targetDir)
            token.ThrowIfCancellationRequested()
            Dim name As String = Path.GetFileName(file)
            state.Done += 1
            If atRoot AndAlso String.Equals(name, LogFileName, StringComparison.OrdinalIgnoreCase) Then report("") : Continue For
            If IO.File.Exists(Path.Combine(source, relDir, name)) Then report("") : Continue For
            MoveToLost(file, Path.Combine(relDir, name), lostRoot, result, report)
        Next
        For Each dir As String In Directory.GetDirectories(targetDir)
            token.ThrowIfCancellationRequested()
            Dim name As String = Path.GetFileName(dir)
            If atRoot AndAlso String.Equals(name, LostFolderName, StringComparison.OrdinalIgnoreCase) Then Continue For
            If Directory.Exists(Path.Combine(source, relDir, name)) Then
                MoveExtras(source, targetRoot, dir, lostRoot, result, state, report, token)
            Else
                state.Done += Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length   ' the whole folder goes at once
                MoveToLost(dir, Path.Combine(relDir, name), lostRoot, result, report)
            End If
        Next
    End Sub

    Private Shared Sub MoveToLost(ByVal item As String, ByVal relative As String, ByVal lostRoot As String,
                                  ByVal result As BackupResult, ByVal report As Action(Of String))
        Dim dest As String = Path.Combine(lostRoot, relative)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(dest))
            If IO.File.Exists(dest) OrElse Directory.Exists(dest) Then
                dest = Path.Combine(Path.GetDirectoryName(dest), Path.GetFileNameWithoutExtension(dest) & "_" & DateTime.Now.ToString("HHmmssfff") & Path.GetExtension(dest))
            End If
            If Directory.Exists(item) Then Directory.Move(item, dest) Else IO.File.Move(item, dest)
            result.MovedToLost += 1
            report("來源已刪除，移到 Lost：" & relative)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            result.Failed += 1
            report("移到 Lost 失敗：" & relative & "（" & ex.Message & "）")
        End Try
    End Sub

End Class

''' <summary>The backup list and options, kept with iPhoto's other settings
''' (HKCU\Software\VB and VBA Program Settings\iPhoto\Backup).</summary>
Friend Module BackupSettings

    Private Const AppName As String = "iPhoto"
    Private Const Section As String = "Backup"

    Friend Function Load(ByRef applyToAll As Boolean, ByRef commonTarget As String) As List(Of BackupSource)
        Dim list As New List(Of BackupSource)
        Dim count As Integer = CInt(Val(GetSetting(AppName, Section, "Count", "0")))
        For i As Integer = 0 To count - 1
            Dim s As String = GetSetting(AppName, Section, "Source" & i, "")
            If s <> "" Then list.Add(New BackupSource With {.SourcePath = s, .TargetPath = GetSetting(AppName, Section, "Target" & i, "")})
        Next
        applyToAll = GetSetting(AppName, Section, "ApplyToAll", "N") = "Y"
        commonTarget = GetSetting(AppName, Section, "CommonTarget", "")
        Return list
    End Function

    Friend Sub Save(ByVal list As List(Of BackupSource), ByVal applyToAll As Boolean, ByVal commonTarget As String)
        Dim old As Integer = CInt(Val(GetSetting(AppName, Section, "Count", "0")))
        SaveSetting(AppName, Section, "Count", CStr(list.Count))
        For i As Integer = 0 To list.Count - 1
            SaveSetting(AppName, Section, "Source" & i, list(i).SourcePath)
            SaveSetting(AppName, Section, "Target" & i, list(i).TargetPath)
        Next
        For i As Integer = list.Count To old - 1   ' entries left from a longer list
            Try
                DeleteSetting(AppName, Section, "Source" & i)
                DeleteSetting(AppName, Section, "Target" & i)
            Catch ex As ArgumentException
            End Try
        Next
        SaveSetting(AppName, Section, "ApplyToAll", If(applyToAll, "Y", "N"))
        SaveSetting(AppName, Section, "CommonTarget", commonTarget)
    End Sub

End Module
