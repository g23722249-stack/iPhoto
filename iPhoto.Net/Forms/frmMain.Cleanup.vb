' 整理清單 (the tool bar's 17): frmCleanup on the album shown (or the whole library). The albums that
' lost photos are listed again -- an edited picture deleted brings its original back (EditedPhotos) --
' and the list shows the album again.
Partial Class frmMain

    Private Sub ShowCleanup()
        Dim album As [Class] = TryCast(If(m_lpAppEnv.ExeMode = enumExeMode.exeAlbums, CurrentSection(), Nothing), [Class])
        Dim deleted As List(Of String)
        Using f As New frmCleanup(album?.Path.TrimEnd("\"c), album?.DisplayName)
            f.ShowDialog(Me)
            deleted = f.DeletedFiles.ToList()
        End Using
        If IsDisposed OrElse deleted.Count = 0 Then Return

        ' the albums the photos were in, where they are loaded, list their folders again
        Dim folders As New HashSet(Of String)(deleted.Select(Function(d) IO.Path.GetDirectoryName(d)), StringComparer.OrdinalIgnoreCase)
        For i = 0 To g_lpStorage.AlbumCount - 1
            Dim section As Albums = g_lpStorage.Album(i)
            For j = 0 To section.ItemCount - 1
                Dim c As [Class] = TryCast(section.Item(j), [Class])
                If c IsNot Nothing AndAlso folders.Contains(c.Path.TrimEnd("\"c)) Then c.Reload()
            Next
        Next
        RefreshCurrentList(Math.Max(0, mlList.SelectedIndex))
        SetDockProperty()   ' PhotoFiles.DeletePhoto took them out of the Dock
    End Sub

End Class
