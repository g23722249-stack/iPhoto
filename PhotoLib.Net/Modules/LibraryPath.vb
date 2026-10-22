Imports System.IO

' Paths kept relative (new in the .NET port), so a library copied to another drive -- a USB stick
' with iPhoto beside 生活剪輯 -- still finds its photos when the drive letter changes. Each place that
' stores paths stores them relative to a folder that moves with it:
'   - iPhoto.mdb (PhotoIndex, FacePhoto, FaceRegion, FaceIndex): the folder of iPhoto.mdb
'   - 攝影集 .Alm and Dock .dck: the folder of that file
'   - iPhoto.Ini and the recent albums in the registry: the app folder, as "$LOCAL$\..." (Config)
' Only a path on the same drive as that folder is made relative ("..\..\Album\x.jpg"); one on another
' drive or a network share stays as it is, and an absolute path read back is used as it is, so files
' written before this keep working.
Public Module LibraryPath

    ''' <summary><paramref name="path"/> relative to <paramref name="baseFolder"/> when both are on the
    ''' same drive; otherwise (another drive, not a full path, blank) as it is.</summary>
    Public Function ToRelative(ByVal path As String, ByVal baseFolder As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse String.IsNullOrEmpty(baseFolder) OrElse Not IO.Path.IsPathFullyQualified(path) Then Return path
        Try
            Dim full As String = IO.Path.GetFullPath(path)
            If Not SameRoot(full, baseFolder) Then Return path
            Return IO.Path.GetRelativePath(baseFolder, full)
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException OrElse TypeOf ex Is PathTooLongException
            Return path
        End Try
    End Function

    ''' <summary>A path stored by <see cref="ToRelative"/> as a full path again; a full path is returned as it is.</summary>
    Public Function FromRelative(ByVal stored As String, ByVal baseFolder As String) As String
        If String.IsNullOrWhiteSpace(stored) OrElse String.IsNullOrEmpty(baseFolder) OrElse IO.Path.IsPathRooted(stored) Then Return stored
        Try
            Return IO.Path.GetFullPath(IO.Path.Combine(baseFolder, stored))
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException OrElse TypeOf ex Is PathTooLongException
            Return stored
        End Try
    End Function

    ''' <summary>True when both full paths are on the same drive (or network share).</summary>
    Public Function SameRoot(ByVal a As String, ByVal b As String) As Boolean
        Return String.Equals(IO.Path.GetPathRoot(a), IO.Path.GetPathRoot(b), StringComparison.OrdinalIgnoreCase)
    End Function

End Module
