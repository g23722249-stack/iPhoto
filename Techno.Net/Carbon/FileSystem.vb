Option Strict On
Option Explicit On

Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

' Port of TechnoSoft Carbon.FileSystem (D:\專案\RunTime\Carbon\CoClass\FileSystem.cls), which
' wrapped Scripting.FileSystemObject. Same names, same "return False instead of throwing" contract,
' so ported code keeps calling  g_lpFileSystem.FileExists(...) / .AnalyseFile(fsBaseName, ...)
' unchanged. Import Carbon.AnalyseFileConstants to use the fs* members unqualified, as VB6 did.
Namespace Carbon

    ''' <summary>What AnalyseFile returns (VB6 values kept).</summary>
    Public Enum AnalyseFileConstants
        fsBaseName = 10          ' 檔案名稱(無副檔名)
        fsExtensionName = 20     ' 副檔名(不含點)
        fsFileName = 30          ' 檔名(含副檔名)
        fsParentFolderName = 40  ' 取得路徑
        fsFileVersion = 50       ' 版本(Win 程式)
        fsLastWriteDate = 110    ' yyyyMMdd
        fsLastWriteTime = 120    ' HHmmss
        fsCreateDate = 130       ' yyyyMMdd
        fsCreateTime = 140       ' HHmmss
        fsShortFileDsce = 200    ' 8.3 短路徑
    End Enum

    <Flags>
    Public Enum AttributeConstants
        attReadonly = &H1
        attHidden = &H2
        attSystem = &H4
        attDirectory = &H10
        attArchive = &H20
        attNormal = &H80
        attTemporary = &H100
        attCompressed = &H800
    End Enum

    Public Class FileSystem

        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)>
        Private Shared Function GetShortPathName(ByVal longPath As String, ByVal shortPath As StringBuilder, ByVal bufferSize As Integer) As Integer
        End Function

        Public Function DriveExists(ByVal driveSpec As String) As Boolean
            Try
                Return New DriveInfo(driveSpec).IsReady OrElse Directory.Exists(driveSpec)
            Catch
                Return False
            End Try
        End Function

        Public Function FileExists(ByVal fileName As String) As Boolean
            Return Not String.IsNullOrEmpty(fileName) AndAlso File.Exists(fileName)
        End Function

        Public Function FolderExists(ByVal folderSpec As String) As Boolean
            Return Not String.IsNullOrEmpty(folderSpec) AndAlso Directory.Exists(folderSpec)
        End Function

        ''' <summary>Deletes the file (read-only included, as FSO's Force:=True). True if it is gone afterwards.</summary>
        Public Function DeleteFile(ByVal fileName As String) As Boolean
            Try
                If FileExists(fileName) Then
                    File.SetAttributes(fileName, FileAttributes.Normal)
                    File.Delete(fileName)
                End If
            Catch
            End Try
            Return Not FileExists(fileName)
        End Function

        ''' <summary>True if the copy exists afterwards. VB6 quirk kept: with overwrite = False the call
        ''' returns True without copying when the destination does NOT exist yet.</summary>
        Public Function CopyFile(ByVal fileName As String, ByVal destination As String, Optional ByVal overwrite As Boolean = True) As Boolean
            If Not FileExists(fileName) Then Return False
            If Not overwrite AndAlso Not FileExists(destination) Then Return True
            Try
                File.Copy(fileName, destination, overwrite)
            Catch
                Return False
            End Try
            Return FileExists(destination)
        End Function

        Public Function MoveFile(ByVal fileName As String, ByVal destination As String) As Boolean
            If Not FileExists(fileName) Then Return False
            Try
                File.Move(fileName, destination)
            Catch
            End Try
            Return Not FileExists(fileName)
        End Function

        ''' <summary>Creates the folder and any missing parents. True if it exists afterwards.
        ''' Like VB6, a spec without any "\" is rejected.</summary>
        Public Function CreateFolder(ByVal folderSpec As String) As Boolean
            If String.IsNullOrEmpty(folderSpec) OrElse folderSpec.IndexOf("\"c) < 0 Then Return False
            Try
                Directory.CreateDirectory(folderSpec)
            Catch
                Return False
            End Try
            Return FolderExists(folderSpec)
        End Function

        Public Function DeleteFolder(ByVal folderSpec As String) As Boolean
            Try
                If FolderExists(folderSpec) Then Directory.Delete(folderSpec, recursive:=True)
            Catch
                Return False
            End Try
            Return Not FolderExists(folderSpec)
        End Function

        Public Function CopyFolder(ByVal folderSpec As String, ByVal destination As String, Optional ByVal overwrite As Boolean = True) As Boolean
            If Not FolderExists(folderSpec) Then Return False
            Try
                CopyTree(New DirectoryInfo(folderSpec), destination, overwrite)
            Catch
                Return False
            End Try
            Return FolderExists(destination)
        End Function

        Private Shared Sub CopyTree(ByVal src As DirectoryInfo, ByVal dest As String, ByVal overwrite As Boolean)
            Directory.CreateDirectory(dest)
            For Each f In src.GetFiles()
                f.CopyTo(Path.Combine(dest, f.Name), overwrite)
            Next
            For Each d In src.GetDirectories()
                CopyTree(d, Path.Combine(dest, d.Name), overwrite)
            Next
        End Sub

        Public Function MoveFolder(ByVal folderSpec As String, ByVal destination As String, Optional ByVal overwrite As Boolean = True) As Boolean
            If Not FolderExists(folderSpec) Then Return False
            Try
                If overwrite AndAlso FolderExists(destination) Then Directory.Delete(destination, recursive:=True)
                Directory.Move(folderSpec, destination)
            Catch
                Return False
            End Try
            Return FolderExists(destination) AndAlso Not FolderExists(folderSpec)
        End Function

        ''' <summary>Pieces of a file or folder name, or its dates/version (VB6: AnalyseFile). Works on names
        ''' that don't exist (the name parts are pure string handling, dates come back as zeros).</summary>
        Public Function AnalyseFile(ByVal analyseMode As AnalyseFileConstants, ByVal fileName As String) As String
            If fileName Is Nothing Then fileName = ""
            Dim isFile As Boolean = File.Exists(fileName)
            Dim isFolder As Boolean = Not isFile AndAlso Directory.Exists(fileName)
            Dim trimmed As String = If(isFolder, fileName.TrimEnd("\"c), fileName)
            Select Case analyseMode
                Case AnalyseFileConstants.fsBaseName
                    Return Path.GetFileNameWithoutExtension(trimmed)
                Case AnalyseFileConstants.fsExtensionName
                    Return If(isFolder, "", Path.GetExtension(trimmed).TrimStart("."c))
                Case AnalyseFileConstants.fsFileName
                    Return Path.GetFileName(trimmed)
                Case AnalyseFileConstants.fsParentFolderName
                    Return If(Path.GetDirectoryName(trimmed), "")
                Case AnalyseFileConstants.fsFileVersion
                    If Not isFile Then Return If(isFolder, "", "0")
                    Return If(FileVersionInfo.GetVersionInfo(fileName).FileVersion, "")
                Case AnalyseFileConstants.fsLastWriteDate, AnalyseFileConstants.fsLastWriteTime,
                     AnalyseFileConstants.fsCreateDate, AnalyseFileConstants.fsCreateTime
                    Dim isDate As Boolean = (analyseMode = AnalyseFileConstants.fsLastWriteDate OrElse analyseMode = AnalyseFileConstants.fsCreateDate)
                    If Not isFile AndAlso Not isFolder Then Return If(isDate, "00000000", "000000")
                    Dim written As Boolean = (analyseMode = AnalyseFileConstants.fsLastWriteDate OrElse analyseMode = AnalyseFileConstants.fsLastWriteTime)
                    Dim t As DateTime = If(written, File.GetLastWriteTime(fileName), File.GetCreationTime(fileName))
                    Return t.ToString(If(isDate, "yyyyMMdd", "HHmmss"))
                Case AnalyseFileConstants.fsShortFileDsce
                    If Not isFile AndAlso Not isFolder Then Return ""
                    Dim sb As New StringBuilder(1024)
                    Return If(GetShortPathName(fileName, sb, sb.Capacity) > 0, sb.ToString(), fileName)
            End Select
            Return ""
        End Function

        ''' <summary>True if the file exists and has any of the given attribute bits.</summary>
        Public Function AttributeFile(ByVal attributeMode As AttributeConstants, ByVal fileName As String) As Boolean
            If Not FileExists(fileName) Then Return False
            Return (CInt(File.GetAttributes(fileName)) And CInt(attributeMode)) <> 0
        End Function

        ''' <summary>True if the folder exists and has any of the given attribute bits.</summary>
        Public Function AttributeFolder(ByVal attributeMode As AttributeConstants, ByVal folderSpec As String) As Boolean
            If Not FolderExists(folderSpec) Then Return False
            Return (CInt(File.GetAttributes(folderSpec)) And CInt(attributeMode)) <> 0
        End Function

        Public Function GetAbsolutePathName(ByVal path As String) As String
            Return IO.Path.GetFullPath(path)
        End Function

    End Class

End Namespace
