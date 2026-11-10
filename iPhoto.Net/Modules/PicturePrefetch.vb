Imports System.Drawing.Drawing2D
Imports System.Threading.Tasks

' 全圖瀏覽 reads the photos next to the one shown ahead of time (new in the .NET port): while a photo is
' looked at, the next and the prior one are decoded on a worker thread, with a copy already scaled to
' the viewer's picture area, so going on shows them at once. Decoding a 12-megapixel photo and scaling
' it with HighQualityBicubic takes 0.2 - 0.6 s on the UI thread otherwise (a 43-megapixel panorama:
' 0.4 s + 0.2 s), before the page turn can even start.
' At most Keep pictures are held; a photo changed on disk since (its time or size) is read again; a
' picture bigger than MaxPixels isn't read ahead (memory).
Friend Module PicturePrefetch

    Private Const Keep As Integer = 3
    Private Const MaxPixels As Long = 60_000_000L

    Private Class Entry
        Public Key As String
        Public Work As Task(Of (Full As Bitmap, Shown As Bitmap))
    End Class

    Private ReadOnly s_lock As New Object
    Private ReadOnly s_entries As New List(Of Entry)   ' oldest first

    ''' <summary>The file's identity: path, last-write time, length and the size it is shown in.</summary>
    Private Function KeyOf(ByVal file As String, ByVal canvas As Size) As String
        Try
            Dim fi As New IO.FileInfo(file)
            If Not fi.Exists Then Return Nothing
            Return fi.FullName.ToLowerInvariant() & "|" & fi.LastWriteTimeUtc.Ticks & "|" & fi.Length & "|" & canvas.Width & "x" & canvas.Height
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException
            Return Nothing
        End Try
    End Function

    ''' <summary>Starts reading <paramref name="files"/> (those not read yet) for a picture area of <paramref name="canvas"/>.</summary>
    Friend Sub Prefetch(ByVal files As IEnumerable(Of String), ByVal canvas As Size)
        If canvas.Width <= 0 OrElse canvas.Height <= 0 Then Return
        For Each f In files
            Dim key As String = KeyOf(f, canvas)
            If key Is Nothing Then Continue For
            SyncLock s_lock
                If s_entries.Any(Function(x) x.Key = key) Then Continue For
                Dim file As String = f
                s_entries.Add(New Entry With {.Key = key, .Work = Task.Run(Function() Read(file, canvas))})
                While s_entries.Count > Keep
                    Drop(s_entries(0))
                    s_entries.RemoveAt(0)
                End While
            End SyncLock
        Next
    End Sub

    ''' <summary>The picture of <paramref name="file"/> read ahead for <paramref name="canvas"/>, taken over
    ''' by the caller (full size and scaled to fit); False when it wasn't read ahead (or failed). Waits
    ''' for a read still running: it has a head start on reading the file now.</summary>
    Friend Function TryTake(ByVal file As String, ByVal canvas As Size, ByRef full As Bitmap, ByRef shown As Bitmap) As Boolean
        full = Nothing : shown = Nothing
        Dim key As String = KeyOf(file, canvas)
        If key Is Nothing Then Return False
        Dim e As Entry
        SyncLock s_lock
            e = s_entries.FirstOrDefault(Function(x) x.Key = key)
            If e Is Nothing Then Return False
            s_entries.Remove(e)
        End SyncLock
        Dim r As (Full As Bitmap, Shown As Bitmap)
        Try
            r = e.Work.GetAwaiter().GetResult()
        Catch ex As Exception
            Return False
        End Try
        If r.Full Is Nothing Then Return False
        full = r.Full
        shown = r.Shown
        Return True
    End Function

    ''' <summary>Everything read ahead goes (the list changed, the viewer closed).</summary>
    Friend Sub Clear()
        SyncLock s_lock
            For Each e In s_entries
                Drop(e)
            Next
            s_entries.Clear()
        End SyncLock
    End Sub

    Private Sub Drop(ByVal e As Entry)
        e.Work.ContinueWith(Sub(t)
                                If t.Status <> TaskStatus.RanToCompletion Then Return
                                t.Result.Full?.Dispose()
                                t.Result.Shown?.Dispose()
                            End Sub)
    End Sub

    ''' <summary>On a worker thread: the photo decoded (as frmMain.LoadPicture does) and a copy scaled to
    ''' fit <paramref name="canvas"/> (as frmViewerLarge.MovePictureToScreen does).</summary>
    Private Function Read(ByVal file As String, ByVal canvas As Size) As (Full As Bitmap, Shown As Bitmap)
        Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.BelowNormal
        Try
            Using fs As New IO.FileStream(file, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                Using img As Image = Image.FromStream(fs, False, False)
                    If CLng(img.Width) * img.Height > MaxPixels Then Return (Nothing, Nothing)
                    Dim full As New Bitmap(img)
                    ' the same fit as frmViewerLarge.MovePictureToScreen (it takes the copy only at its size)
                    Dim fit As New Quartz.FixSize
                    fit.SetPictureSize(full.Width, full.Height)
                    fit.SetCanvasSize(canvas.Width, canvas.Height)
                    fit.Resize()
                    Dim w As Integer = Math.Max(1, fit.Width), h As Integer = Math.Max(1, fit.Height)
                    Dim shown As New Bitmap(w, h)
                    Using g As Graphics = Graphics.FromImage(shown)
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic
                        g.DrawImage(full, 0, 0, w, h)
                    End Using
                    Return (full, shown)
                End Using
            End Using
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException OrElse
                                   TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is Runtime.InteropServices.ExternalException
            Return (Nothing, Nothing)
        End Try
    End Function

End Module
