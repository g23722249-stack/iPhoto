Imports System.IO

' 編輯 with PhotoEdit (new in the .NET port; the tool bar's 編輯 and the photo list's right-click 編輯):
' PhotoEdit.exe beside iPhoto.exe (published with it, sharing Models\ and Stick\) is started with the
' photo -- an edited picture's original, so PhotoEdit brings back the edits it keeps beside it. Its own
' process: its memory (AI models), its DPI mode and window style stay apart from iPhoto's.
' What PhotoEdit exports beside the photo (<name>_edited.jpg) replaces it in the album (EditedPhotos):
' TakeInEdits looks in the folders of the photos opened from here and takes in what the album listing
' found elsewhere, then the index, the faces and the Dock follow -- when PhotoEdit closes and whenever
' iPhoto's main window is activated again. Without PhotoEdit.exe the tool bar's 編輯 starts the program
' iPhoto.Ini names, as before.
Friend Module PhotoEditLink

    Private Const ExeName As String = "PhotoEdit.exe"
    Private ReadOnly s_folders As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>PhotoEdit.exe beside iPhoto.exe (published together); when iPhoto runs from its build
    ''' folder (Visual Studio), the PhotoEdit project's own build (C:\專案\PhotoEdit\PhotoEdit\bin\Release or
    ''' Debug, the newer). The one beside iPhoto when there is none.</summary>
    Friend ReadOnly Property ExePath As String
        Get
            Dim beside As String = Path.Combine(AppContext.BaseDirectory, ExeName)
            If File.Exists(beside) Then Return beside
            Dim d As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
            For level = 1 To 6
                If d Is Nothing Then Exit For
                Dim built = {"Release", "Debug"}.Select(Function(cfg) New FileInfo(Path.Combine(d.FullName, "PhotoEdit", "PhotoEdit", "bin", cfg, "net8.0-windows", ExeName))).
                                                 Where(Function(f) f.Exists).OrderByDescending(Function(f) f.LastWriteTimeUtc).FirstOrDefault()
                If built IsNot Nothing Then Return built.FullName
                d = d.Parent
            Next
            Return beside
        End Get
    End Property

    Friend ReadOnly Property Available As Boolean
        Get
            Return File.Exists(ExePath)
        End Get
    End Property

    ''' <summary>Opens <paramref name="file"/> (its original when it is an edited picture) in PhotoEdit;
    ''' <paramref name="whenClosed"/> runs on <paramref name="owner"/>'s thread when PhotoEdit closes.
    ''' False when it couldn't be started.</summary>
    Friend Function Edit(ByVal file As String, ByVal owner As Control, ByVal whenClosed As Action) As Boolean
        Dim target As String = If(EditedPhotos.OriginalOf(file), file)
        If Not IO.File.Exists(target) Then Return False
        Try
            Dim exe As String = ExePath
            Dim p As Process = Process.Start(New ProcessStartInfo(exe, """" & target & """") With {
                .UseShellExecute = False, .WorkingDirectory = Path.GetDirectoryName(exe)})
            If p Is Nothing Then Return False
            SyncLock s_folders
                s_folders.Add(Path.GetDirectoryName(target))
            End SyncLock
            p.EnableRaisingEvents = True
            AddHandler p.Exited, Sub()
                                     p.Dispose()
                                     If owner.IsHandleCreated AndAlso Not owner.IsDisposed Then owner.BeginInvoke(whenClosed)
                                 End Sub
            Return True
        Catch ex As Exception When TypeOf ex Is ComponentModel.Win32Exception OrElse TypeOf ex Is InvalidOperationException
            Return False
        End Try
    End Function

    ''' <summary>Takes in the pictures PhotoEdit exported into the folders opened from here, and what the
    ''' album listing took in elsewhere: the replaced files' index rows and face data go, the edited
    ''' pictures' rows come, the Dock follows. The adoptions (none when nothing came).</summary>
    Friend Function TakeInEdits() As List(Of EditedPhotos.Adoption)
        If g_lpConfig Is Nothing OrElse g_lpConfig.ReadOnly Then Return New List(Of EditedPhotos.Adoption)
        Dim folders As List(Of String)
        SyncLock s_folders
            folders = s_folders.ToList()
        End SyncLock
        For Each albumFolder As String In folders
            If Directory.Exists(albumFolder) Then EditedPhotos.AdoptIn(albumFolder)   ' queued for TakeAdoptions
        Next
        Dim list As List(Of EditedPhotos.Adoption) = EditedPhotos.TakeAdoptions()
        Dim dockChanged As Boolean = False
        For Each a In list
            Dim edited As New Photo
            edited.Construct(a.Edited)
            For Each gone In {a.Original}.Concat(a.Superseded)
                g_lpDatabase?.Delete(gone)
                g_lpDatabase?.DeleteFacePhoto(gone)
                If g_lpDock IsNot Nothing AndAlso g_lpDock.RemoveItem(gone) Then
                    If Not g_lpDock.Selected(a.Edited) Then g_lpDock.AddItem(a.Edited, edited.Exif(enumPhotoExif.peDate), edited.Exif(enumPhotoExif.peTime))
                    dockChanged = True
                End If
            Next
            g_lpDatabase?.AddItem(edited)
        Next
        If dockChanged Then g_lpDock.Save()
        If list.Count > 0 Then g_lpFaces?.RequestOrganizeSoon()
        Return list
    End Function

End Module
