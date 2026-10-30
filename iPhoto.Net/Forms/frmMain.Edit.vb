' 編輯 (the tool bar's, and the photo list's right-click 編輯, frmMain.FileOps.vb) with PhotoEdit
' (Modules\PhotoEditLink.vb): the photo selected now -- one, a picture -- opens in PhotoEdit. What it
' exports beside the photo takes the photo's place in the album (EditedPhotos); when PhotoEdit closes,
' and whenever this window is activated again, that is taken in and the list shows the edited picture.
Partial Class frmMain

    ''' <summary>Opens the photo selected now in PhotoEdit.</summary>
    Private Sub EditCurrentPhoto()
        If m_lpCurrentPhoto Is Nothing Then
            frmMsgBox.ShowExclamationMessage("請先選一張照片。", "相片後製")
            Return
        End If
        If m_lpCurrentPhoto.MediaType <> enumPhotoMediaType.mdImage Then
            frmMsgBox.ShowExclamationMessage("影片不能用 PhotoEdit 修飾。", "相片後製")
            Return
        End If
        If Not PhotoEditLink.Available Then
            frmMsgBox.ShowExclamationMessage("找不到 PhotoEdit：" & vbCrLf & PhotoEditLink.ExePath & vbCrLf & "（發佈時 Publish.ps1 會一起放進去）", "相片後製")
            Return
        End If
        If Not PhotoEditLink.Edit(m_lpCurrentPhoto.FileDesc, Me, AddressOf TakeInEdits) Then
            frmMsgBox.ShowCriticalMessage("無法啟動 PhotoEdit" & vbCrLf & PhotoEditLink.ExePath, "相片後製")
        End If
    End Sub

    Private Sub Edit_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If g_lpConfig IsNot Nothing Then TakeInEdits()
    End Sub

    ''' <summary>What PhotoEdit exported is taken in (index, faces, Dock); the list shows the edited
    ''' pictures in place of their originals (the album listed again, or the items repointed).</summary>
    Private Sub TakeInEdits()
        If IsDisposed OrElse g_lpConfig Is Nothing Then Return
        Dim list As List(Of EditedPhotos.Adoption) = PhotoEditLink.TakeInEdits()
        If list.Count = 0 Then Return
        Dim replaced As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each a In list
            replaced(a.Original) = a.Edited
            For Each s In a.Superseded
                replaced(s) = a.Edited
            Next
        Next
        Dim selectFile As String = Nothing
        If mlList.SelectedIndex >= 0 AndAlso mlList.SelectedIndex < mlList.Count Then
            Dim cur As String = mlList.Item(mlList.SelectedIndex).FileName
            selectFile = If(replaced.ContainsKey(cur), replaced(cur), cur)
        End If
        Dim album As [Class] = TryCast(If(m_lpAppEnv.ExeMode = enumExeMode.exeAlbums, CurrentSection(), Nothing), [Class])
        If album IsNot Nothing AndAlso list.Any(Function(a) String.Equals(IO.Path.GetDirectoryName(a.Edited), album.Path.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase)) Then
            ' the album listed again: the originals leave it, the edited pictures come in at their dates
            album.Reload()
            RefreshCurrentList(Math.Max(0, mlList.SelectedIndex))
        Else
            For i = 0 To mlList.Count - 1
                Dim dst As String = Nothing
                If replaced.TryGetValue(mlList.Item(i).FileName, dst) Then mlList.Item(i).FileName = dst
            Next
        End If
        If selectFile IsNot Nothing Then
            For i = 0 To mlList.Count - 1
                If String.Equals(mlList.Item(i).FileName, selectFile, StringComparison.OrdinalIgnoreCase) Then
                    mlList.SelectedIndex = i
                    Exit For
                End If
            Next
        End If
        SetDockProperty()
    End Sub

End Class
