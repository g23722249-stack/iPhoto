' 蒙太奇拼貼 (the tool bar's 12, was 郵件): PhotoMontage's editor (Modules\MontageLink.vb). The pictures
' selected in the list (two or more) come in at once; otherwise its 選取照片 opens on the album shown now.
' The work is exported into the album picked in its 匯出 (this album to begin with); a work saved in the
' library is indexed and, in the album shown, selected. 年度回顧 (frmMain.YearReview.vb) opens the editor
' the same way with the year's picks.
Partial Class frmMain

    Private Sub ShowMontage()
        Dim initial As New List(Of String)
        If mlList.SelectedIndices.Count > 1 Then
            For Each i In Targets(mlList.SelectedIndex)
                Dim f As String = mlList.Item(i).FileName
                If GetMediaType(g_lpFileSystem, f) = enumPhotoMediaType.mdImage Then initial.Add(f)
            Next
        End If
        OpenMontage(initial, "蒙太奇拼貼", PhotoMontage.Core.MontageMode.Collage)
    End Sub

    ''' <summary>PhotoMontage's editor with <paramref name="initial"/> (none: its 選取照片 opens first).</summary>
    Private Sub OpenMontage(ByVal initial As List(Of String), ByVal title As String, ByVal mode As PhotoMontage.Core.MontageMode)
        ' the album shown now: where 選取照片 opens and where the work goes unless another is picked
        Dim start As List(Of String) = Nothing
        Dim folder As String = Nothing
        Dim shown As PhotoSet = CurrentSection()
        If shown IsNot Nothing Then
            With m_lpAppEnv
                Dim book As Boolean = .ExeMode = enumExeMode.exeFavorites
                Dim section As Albums = If(book, g_lpStorage.Favorite(.SectionIndex), g_lpStorage.Album(.SectionIndex))
                start = New List(Of String) From {If(book, MontageLink.BooksKey, MontageLink.AlbumsKey), section.Path, shown.Key}
            End With
            If TypeOf shown Is [Class] AndAlso Not g_lpConfig.ReadOnly Then folder = DirectCast(shown, [Class]).Path.TrimEnd("\"c)
        End If

        Dim result As PhotoMontage.MontageResult = PhotoMontage.MontageEditor.ShowDialog(Me, New PhotoMontage.MontageOptions With {
            .Title = title,
            .Mode = mode,
            .InitialPhotos = initial,
            .PhotoSource = New MontageLink.LibrarySource(start),
            .ExportFolderPickerText = If(g_lpConfig.ReadOnly, Nothing, "選相簿…"),
            .DefaultExportFolder = folder,
            .CloseAfterExport = True,
            .ShowExportCompletedMessage = False})
        If IsDisposed OrElse Not result.Success Then Return
        TakeInMontage(result.OutputPath)
    End Sub

    ''' <summary>A work saved into an album: indexed, and selected when that album is shown.</summary>
    Private Sub TakeInMontage(ByVal file As String)
        If g_lpConfig.ReadOnly OrElse Not IO.File.Exists(file) OrElse Not MontageLink.InLibrary(file) Then
            frmMsgBox.ShowSmileMessage("作品已存到" & vbCrLf & file, "蒙太奇拼貼")
            Return
        End If
        Dim p As New Photo
        p.Construct(file)
        g_lpDatabase?.AddItem(p)

        Dim album As [Class] = TryCast(If(m_lpAppEnv.ExeMode = enumExeMode.exeAlbums, CurrentSection(), Nothing), [Class])
        If album IsNot Nothing AndAlso String.Equals(IO.Path.GetDirectoryName(file), album.Path.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then
            album.Reload()
            RefreshCurrentList(Math.Max(0, mlList.SelectedIndex))
            For i = 0 To mlList.Count - 1
                If String.Equals(mlList.Item(i).FileName, file, StringComparison.OrdinalIgnoreCase) Then
                    mlList.SelectedIndex = i
                    mlList.EnsureVisible(i)
                    Exit For
                End If
            Next
        Else
            frmMsgBox.ShowSmileMessage("作品已存到相簿" & vbCrLf & file, "蒙太奇拼貼")
        End If
    End Sub

End Class
