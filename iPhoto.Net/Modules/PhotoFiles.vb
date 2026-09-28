' Deleting a photo with everything that belongs to it (new in the .NET port), shared by the photo
' list's 刪除檔案 / 永久刪除 and 尋找重複照片: the photo, its .Exif, its Restore\ copy and a video's
' Thumb\ picture -- to the Recycle Bin or outright -- then its PhotoIndex row, face rows and Dock entry.
Friend Module PhotoFiles

    ''' <summary>False when the photo itself stayed (in use, no rights); the side files follow it only then.</summary>
    Public Function DeletePhoto(ByVal file As String, ByVal permanent As Boolean) As Boolean
        Dim remove As Func(Of String, Boolean) = If(permanent, New Func(Of String, Boolean)(AddressOf DeleteNow), New Func(Of String, Boolean)(AddressOf Recycle))
        If Not remove(file) Then Return False
        Dim folder As String = IO.Path.GetDirectoryName(file), baseName As String = IO.Path.GetFileNameWithoutExtension(file)
        remove(IO.Path.Combine(folder, baseName & "." & gc_strExifPattern))
        remove(IO.Path.Combine(folder, "Restore", IO.Path.GetFileName(file)))
        remove(IO.Path.Combine(folder, "Thumb", baseName & ".jpg"))
        g_lpDatabase?.Delete(file)
        g_lpDatabase?.DeleteFacePhoto(file)
        If g_lpDock IsNot Nothing AndAlso g_lpDock.RemoveItem(file) Then g_lpDock.Save()
        Return True
    End Function

    ''' <summary>Deleted outright (永久刪除); True when the file is gone (or wasn't there).</summary>
    Public Function DeleteNow(ByVal file As String) As Boolean
        If String.IsNullOrEmpty(file) OrElse Not IO.File.Exists(file) Then Return True
        Try
            IO.File.SetAttributes(file, IO.FileAttributes.Normal)
            IO.File.Delete(file)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return Not IO.File.Exists(file)
    End Function

    ''' <summary>To the Recycle Bin; True when the file is gone (or wasn't there).</summary>
    Public Function Recycle(ByVal file As String) As Boolean
        If String.IsNullOrEmpty(file) OrElse Not IO.File.Exists(file) Then Return True
        Try
            IO.File.SetAttributes(file, IO.FileAttributes.Normal)
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin, FileIO.UICancelOption.DoNothing)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is OperationCanceledException OrElse TypeOf ex Is Security.SecurityException
        End Try
        Return Not IO.File.Exists(file)
    End Function

End Module
