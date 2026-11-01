' What 全圖瀏覽's tools ask of the main window (frmViewerLarge.Tools.vb, new in the .NET port):
'   左轉 / 右轉   the file turned losslessly (RotatePhotos -> LibAlbum.RotatePicture), its Restore\ copy kept
'   相片後製      EditCurrentPhoto (PhotoEdit), unsaved viewer edits offered for saving first
'   加入 / 移出 Dock, 刪除 (asks), 評價 (the .Exif, the list's stars, the index)
' and saving the viewer's edits (裁切, 拉直, 自動調整, 亮度 / 對比) when the photo is left: as
' <original>_edited.jpg (the next free _editedN) with the photo's EXIF, taken in like PhotoEdit's (EditedPhotos):
' the album shows it, the original is kept but hidden, an earlier edit is superseded.
Partial Class frmMain

    Private Sub AddViewerToolHandlers()
        AddHandler m_frmViewer.RotateRequested, Sub(s, angle)
                                                    If mlList.SelectedIndex >= 0 Then RotatePhotos(New List(Of Integer) From {mlList.SelectedIndex}, angle)
                                                End Sub
        AddHandler m_frmViewer.EditRequested, Sub(s, e)
                                                  If m_frmViewer.Changed AndAlso Not AskSaveViewerEdits() Then Return
                                                  EditCurrentPhoto()   ' frmMain.Edit.vb
                                              End Sub
        AddHandler m_frmViewer.DockRequested, Sub(s, e)
                                                  Dim item As Aqua.MediaItem = mlList.SelectedItem
                                                  If item Is Nothing Then Return
                                                  item.Marked = Not item.Marked   ' mlList_ItemMarkChanged docks it
                                                  m_frmViewer.Docked = item.Marked
                                              End Sub
        AddHandler m_frmViewer.DeleteRequested, Sub(s, e)
                                                    If mlList.SelectedIndex >= 0 Then DeletePhotoFiles(New List(Of Integer) From {mlList.SelectedIndex})
                                                End Sub
        AddHandler m_frmViewer.RatingChanged, Sub(s, rating) RateCurrentPhoto(rating)
    End Sub

    ''' <summary>The viewer shows <paramref name="file"/>'s rating and whether it is in the Dock.</summary>
    Private Sub ShowViewerState(ByVal file As String)
        If m_frmViewer Is Nothing Then Return
        Dim p As New Photo
        p.Construct(file)
        m_frmViewer.Rating = CInt(Val(p.Exif(enumPhotoExif.peRanking)))
        m_frmViewer.Docked = g_lpDock IsNot Nothing AndAlso g_lpDock.Selected(file)
    End Sub

    Private Sub RateCurrentPhoto(ByVal rating As Integer)
        If m_lpCurrentPhoto Is Nothing OrElse g_lpConfig.ReadOnly Then Return
        m_lpCurrentPhoto.Exif(enumPhotoExif.peRanking) = CStr(rating)
        If mlList.SelectedItem IsNot Nothing Then mlList.SelectedItem.Ranking = CType(rating, Aqua.MediaItemRanking)
        g_lpDatabase?.AddItem(m_lpCurrentPhoto)
    End Sub

    ''' <summary>The viewer's unsaved edits: the usual question (放棄 / 儲存 / 剪貼簿 / 另存); False when 取消.</summary>
    Private Function AskSaveViewerEdits() As Boolean
        frmSaveChangedPhoto.ShowDialog()
        Select Case frmSaveChangedPhoto.Result
            Case scpResult.scpSave : SaveViewerEdits()
            Case scpResult.scpCopy
                Clipboard.Clear()
                Clipboard.SetImage(m_frmViewer.Photo)
        End Select
        Return True
    End Function

    ''' <summary>The viewer's picture as <original>_edited.jpg (the next free name), with the photo's EXIF;
    ''' taken in (after the list has moved on) like an export of PhotoEdit's.</summary>
    Private Sub SaveViewerEdits()
        If m_lpCurrentPhoto Is Nothing OrElse m_frmViewer Is Nothing OrElse g_lpConfig.ReadOnly Then Return
        Dim current As String = m_lpCurrentPhoto.FileDesc
        Dim original As String = If(EditedPhotos.OriginalOf(current), current)
        Dim folder As String = IO.Path.GetDirectoryName(original), baseName As String = IO.Path.GetFileNameWithoutExtension(original)
        Dim target As String = IO.Path.Combine(folder, baseName & "_edited.jpg")
        Dim n As Integer = 2
        While IO.File.Exists(target)
            target = IO.Path.Combine(folder, baseName & "_edited" & n & ".jpg")
            n += 1
        End While
        If Not SaveJpegWithExif(m_frmViewer.Photo, target, current) Then
            frmMsgBox.ShowCriticalMessage("儲存修改後的照片失敗" & vbCrLf & target, "全圖瀏覽")
            Return
        End If
        EditedPhotos.AdoptIn(folder)   ' queued; TakeInEdits (frmMain.Edit.vb) indexes it and lists it
        BeginInvoke(New Action(AddressOf TakeInEdits))
    End Sub

End Class
