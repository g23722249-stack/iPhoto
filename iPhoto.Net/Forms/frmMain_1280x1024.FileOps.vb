' File operations on the photo list's right-click menu (new in the .NET port; the items are added to
' AquaMenu1 in code so the designer file stays as ported). The list takes Ctrl / Shift / Ctrl+A
' multi-selection (Aqua MediaList.MultiSelect); the items below work on every selected photo when the
' right-clicked one is among them, else on that one (rotate, copy, info, view stay one-photo).
'   以檔案總管開啟    the photo's folder with the photo selected (also on the face views and the cover wall)
'   我的評價           (multi) every selected photo gets the rating
'   加入 / 移出 Dock   (multi) marks / unmarks the photos (as a double-click does for one)
'   移動到…            (multi, not in 攝影集) into another album folder: the photo with its .Exif, Restore\
'                      copy and video Thumb\ picture; PhotoIndex, face rows, the Dock and the photo books
'                      (.Alm) follow the new path
'   設為相本封面       the photo goes on top of the album's cover (PhotoSet.CoverPhoto; the cover is made again)
'   刪除檔案…          (multi) after a question: to the Recycle Bin, with the side files; PhotoIndex rows,
'                      face rows and Dock entries go too. Also the Delete key (which, as in VB6, needs
'                      設定's DeleteAlbumPhotos).
'   永久刪除…          as 刪除檔案, but deleted outright (no Recycle Bin); the question defaults to 否.
'   從攝影集移除        (攝影集 only, multi) the photos leave the book (.Alm); the photos themselves stay.
Partial Class frmMain_1280x1024

    Private Sub FileOps_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        mlList.MultiSelect = True
        With AquaMenu1.Items
            Dim info As Integer = -1
            For i = 0 To .Count - 1
                If String.Equals(.Item(i).Name, "mnuShowInfo", StringComparison.OrdinalIgnoreCase) Then info = i
            Next
            Dim explorer As New Aqua.MenuItem("mnuExplorer", "以檔案總管開啟", 1, Nothing, Nothing)
            If info >= 0 Then .Insert(info + 1, explorer) Else .Add(explorer)
            Dim at As Integer = .IndexOf(explorer) + 1
            .Insert(at, New Aqua.MenuItem("mnuMap", "在地圖上看拍攝地點", 1, Nothing, Nothing))
            .Insert(at + 1, New Aqua.MenuItem("mnuNearby", "拍攝地點附近的照片", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuDockSep", "-", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuDockAdd", "加入 Dock", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuDockRemove", "移出 Dock", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuMoveTo", "移動到…", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuSetCover", "設為相本封面", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuFileSep", "-", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuRemoveFromBook", "從攝影集移除", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuDeleteFile", "刪除檔案…", 1, Nothing, Nothing))
            .Add(New Aqua.MenuItem("mnuDeleteForever", "永久刪除…", 1, Nothing, Nothing))
        End With
    End Sub

    ''' <summary>The photos an item of the menu works on: all selected when <paramref name="index"/> is
    ''' one of them, else just it. Indexes, lowest first.</summary>
    Private Function Targets(ByVal index As Integer) As List(Of Integer)
        Dim sel As List(Of Integer) = mlList.SelectedIndices
        If sel.Count > 1 AndAlso sel.Contains(index) Then Return sel
        Return If(index >= 0 AndAlso index < mlList.Count, New List(Of Integer) From {index}, New List(Of Integer))
    End Function

    ''' <summary>Called by SetMediaListPopupMenu.</summary>
    Private Sub SetFileOpsMenu()
        Dim book As Boolean = m_lpAppEnv.ExeMode = enumExeMode.exeFavorites
        Dim ro As Boolean = g_lpConfig.ReadOnly
        Dim n As Integer = Targets(mlList.SelectedIndex).Count
        Dim many As String = If(n > 1, "（" & n & " 張）", "")
        Dim docked As Integer = Targets(mlList.SelectedIndex).Where(Function(i) mlList.Item(i).Marked).Count()
        With AquaMenu1
            .Item("mnuExplorer").Enabled = True
            Dim shot As ShotInfo.Shot = If(m_lpCurrentPhoto Is Nothing, Nothing, ShotInfo.Read(m_lpCurrentPhoto.FileDesc))
            .Item("mnuMap").Enabled = shot IsNot Nothing AndAlso shot.HasPlace
            .Item("mnuNearby").Enabled = shot IsNot Nothing AndAlso shot.HasPlace
            .Item("mnuDockAdd").Text = "加入 Dock" & many
            .Item("mnuDockAdd").Enabled = docked < n
            .Item("mnuDockRemove").Text = "移出 Dock" & many
            .Item("mnuDockRemove").Enabled = docked > 0
            .Item("mnuMoveTo").Text = "移動到…" & many
            .Item("mnuMoveTo").Visible = Not book
            .Item("mnuMoveTo").Enabled = Not ro
            .Item("mnuSetCover").Enabled = Not ro AndAlso n = 1 AndAlso CurrentSection() IsNot Nothing AndAlso
                                          (m_lpAppEnv.ExeMode = enumExeMode.exeAlbums OrElse book)
            .Item("mnuRemoveFromBook").Text = "從攝影集移除" & many
            .Item("mnuRemoveFromBook").Visible = book
            .Item("mnuRemoveFromBook").Enabled = book AndAlso Not ro
            .Item("mnuDeleteFile").Text = "刪除檔案" & many & "…"
            .Item("mnuDeleteFile").Enabled = Not ro
            .Item("mnuDeleteForever").Text = "永久刪除" & many & "…"
            .Item("mnuDeleteForever").Enabled = Not ro
            .Item("mnuMyRanking").Text = "我的評價" & many
            .Item("mnuRotateClockwise").Text = "左轉 90°" & many
            .Item("mnuRotateCounterClockwise").Text = "右轉 90°" & many
            If n > 1 Then
                ' several: turnable when any of them is a picture (the one right-clicked may be a video)
                Dim anyPicture As Boolean = Targets(mlList.SelectedIndex).Any(Function(i) GetMediaType(g_lpFileSystem, mlList.Item(i).FileName) = enumPhotoMediaType.mdImage)
                .Item("mnuRotateClockwise").Enabled = anyPicture AndAlso Not ro
                .Item("mnuRotateCounterClockwise").Enabled = anyPicture AndAlso Not ro
            End If
        End With
    End Sub

    ''' <summary>Called by AquaMenu1_MenuSelected for the items above; True when it was one of them.</summary>
    Private Function FileOpsMenuSelected(ByVal name As String, ByVal index As Integer) As Boolean
        Select Case name.ToUpperInvariant()
            Case "MNUEXPLORER" : ShowInExplorer(m_lpCurrentPhoto.FileDesc)
            Case "MNUMAP" : ShowOnMap(m_lpCurrentPhoto.FileDesc)
            Case "MNUNEARBY" : ShowNearby(m_lpCurrentPhoto.FileDesc)
            Case "MNUDOCKADD" : SetDocked(Targets(index), True)
            Case "MNUDOCKREMOVE" : SetDocked(Targets(index), False)
            Case "MNUMOVETO" : MovePhotos(Targets(index))
            Case "MNUSETCOVER" : SetAlbumCover(index)
            Case "MNUDELETEFILE" : DeletePhotoFiles(Targets(index))
            Case "MNUDELETEFOREVER" : DeletePhotoFiles(Targets(index), permanent:=True)
            Case "MNUREMOVEFROMBOOK" : RemoveFromBook(Targets(index))
            Case "MNUROTATECLOCKWISE", "MNUROTATECOUNTERCLOCKWISE"
                Dim many As List(Of Integer) = Targets(index)
                If many.Count <= 1 Then Return False   ' one photo: AquaMenu1_MenuSelected as before
                ' VB6: the "左轉 90°" item (mnuRotateClockwise) turns by -90, "右轉 90°" by +90
                RotatePhotos(many, If(name.Equals("mnuRotateClockwise", StringComparison.OrdinalIgnoreCase), -90, 90))
            Case "MNURANKINGNONE", "MNURANKINGLV1", "MNURANKINGLV2", "MNURANKINGLV3", "MNURANKINGLV4", "MNURANKINGLV5"
                Dim many As List(Of Integer) = Targets(index)
                If many.Count <= 1 Then Return False   ' one photo: AquaMenu1_MenuSelected as before
                Dim level As Integer = If(name.EndsWith("None", StringComparison.OrdinalIgnoreCase), 0, CInt(name.Substring(name.Length - 1)))
                For Each i In many
                    Dim p As New Photo
                    p.Construct(mlList.Item(i).FileName)
                    p.Exif(enumPhotoExif.peRanking) = CStr(level)
                    mlList.Item(i).Ranking = CType(level, Aqua.MediaItemRanking)
                    g_lpDatabase.AddItem(p)
                Next
            Case Else : Return False
        End Select
        Return True
    End Function

    ''' <summary>左轉 / 右轉 on every selected photo (videos are passed over): each keeps its Restore\ copy
    ''' first, as for one photo, so 恢復到最初狀態 still works.</summary>
    Private Sub RotatePhotos(ByVal indexes As List(Of Integer), ByVal angle As Long)
        If g_lpConfig.ReadOnly Then Return
        If m_frmViewer IsNot Nothing Then m_frmViewer.Clear()   ' the viewer may hold the focused one
        Dim failed As New List(Of String)
        SetBusy(True)
        Try
            For Each i In indexes
                Dim p As New Photo
                p.Construct(mlList.Item(i).FileName)
                If p.MediaType <> enumPhotoMediaType.mdImage Then Continue For
                If Not p.Backup() OrElse Not RotatePicture(p.FileDesc, angle) Then
                    failed.Add(p.FileDesc)
                    Continue For
                End If
                mlList.Item(i).FileName = ""
                mlList.Item(i).FileName = p.FileDesc   ' the thumbnail again
            Next
        Finally
            SetBusy(False)
        End Try
        If failed.Count > 0 Then frmMsgBox.ShowCriticalMessage(failed.Count & " 張旋轉失敗" & vbCrLf & IO.Path.GetFileName(failed(0)), "旋轉照片")
        ' the focused photo in the viewer, as after rotating one
        Dim cur As Integer = mlList.SelectedIndex
        If cur >= 0 AndAlso m_lpCurrentPhoto IsNot Nothing AndAlso m_lpCurrentPhoto.MediaType = enumPhotoMediaType.mdImage Then
            ShowPictureInViewer(m_lpCurrentPhoto.FileDesc, cur = 0, cur = mlList.Count - 1)
        End If
    End Sub

    ''' <summary>「已選取 N 張」 while more than one photo is selected.</summary>
    Private Sub FileOps_SelectionChanged(sender As Object, e As EventArgs) Handles mlList.SelectionChanged
        If ListShowsCards Then Return
        Dim n As Integer = mlList.SelectedIndices.Count
        If n > 1 Then lblPhotoCounts.Text = "已選取 " & n & " 張 · 共 " & mlList.Count & " 張" Else ShowPhotoIndex()
    End Sub

    '==================================================================================================
    ' 以檔案總管開啟
    '==================================================================================================
    ''' <summary>Explorer at the file's folder with the file selected (a folder: opened, selected in its parent).</summary>
    Friend Shared Sub ShowInExplorer(ByVal path As String)
        If String.IsNullOrEmpty(path) Then Return
        If Not IO.File.Exists(path) AndAlso Not IO.Directory.Exists(path) Then
            frmMsgBox.ShowCriticalMessage("找不到檔案" & vbCrLf & path, "以檔案總管開啟")
            Return
        End If
        Try
            Process.Start(New ProcessStartInfo("explorer.exe", "/select,""" & path & """") With {.UseShellExecute = True})
        Catch ex As Exception When TypeOf ex Is ComponentModel.Win32Exception OrElse TypeOf ex Is InvalidOperationException
            frmMsgBox.ShowCriticalMessage("無法開啟檔案總管" & vbCrLf & ex.Message, "以檔案總管開啟")
        End Try
    End Sub

    '==================================================================================================
    ' 拍攝地點 (ShotInfo / GeoIndex)
    '==================================================================================================
    Private Sub ShowOnMap(ByVal file As String)
        Dim s As ShotInfo.Shot = ShotInfo.Read(file)
        If s Is Nothing OrElse Not s.HasPlace Then Return
        Try
            Process.Start(New ProcessStartInfo(s.MapUrl()) With {.UseShellExecute = True})
        Catch ex As ComponentModel.Win32Exception
            frmMsgBox.ShowCriticalMessage("無法開啟瀏覽器" & vbCrLf & ex.Message, "在地圖上看拍攝地點")
        End Try
    End Sub

    Private Const NearbyKm As Double = 1.0
    Private m_intNearbyRun As Integer

    ''' <summary>The album photos taken within NearbyKm of this one, nearest first. Every photo's place is
    ''' read once (GeoIndex, cached): the first time takes a while, with the progress below the list.</summary>
    Private Sub ShowNearby(ByVal file As String)
        Dim s As ShotInfo.Shot = ShotInfo.Read(file)
        If s Is Nothing OrElse Not s.HasPlace Then Return
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Dim run As Integer = Threading.Interlocked.Increment(m_intNearbyRun)
        lblPhotoCounts.Text = "讀取照片的拍攝地點…"
        Threading.Tasks.Task.Run(Function() GeoIndex.Places(roots, Sub(done, total)
                                                                     If run = m_intNearbyRun AndAlso IsHandleCreated Then BeginInvoke(Sub() lblPhotoCounts.Text = $"讀取拍攝地點… {done:#,0} / {total:#,0}")
                                                                 End Sub)).
            ContinueWith(Sub(t)
                             If run <> m_intNearbyRun OrElse IsDisposed OrElse Not IsHandleCreated OrElse t.IsFaulted Then Return
                             BeginInvoke(Sub() NearbyReady(s, GeoIndex.Nearby(t.Result, s.Latitude.Value, s.Longitude.Value, NearbyKm), t.Result.Count))
                         End Sub)
    End Sub

    Private Sub NearbyReady(ByVal s As ShotInfo.Shot, ByVal files As List(Of String), ByVal withPlace As Integer)
        With m_lpAppEnv
            .ExeMode = enumExeMode.exeFace   ' a list of files from anywhere (as the face / search lists)
            .SectionIndex = -1
            .KeyIndex = -1
        End With
        ClearScreenAlbum()
        MoveFilesToMediaList(files.ToArray(), files.Count)
        txtTitle.Text = "拍攝地點附近的照片"
        txtRemark.Text = $"{s.PlaceText()} 方圓 {NearbyKm:0.#} 公里內：{files.Count} 張（{withPlace:#,0} 張照片有拍攝地點）"
        If mlList.Visible Then mlList.Focus()
    End Sub

    '==================================================================================================
    ' Dock
    '==================================================================================================
    Private Sub SetDocked(ByVal indexes As List(Of Integer), ByVal docked As Boolean)
        For Each i In indexes
            If mlList.Item(i).Marked <> docked Then mlList.Item(i).Marked = docked   ' mlList_ItemMarkChanged docks it
        Next
    End Sub

    '==================================================================================================
    ' 設為相本封面
    '==================================================================================================
    Private Sub SetAlbumCover(ByVal index As Integer)
        Dim section As PhotoSet = CurrentSection()
        If section Is Nothing OrElse index < 0 OrElse index >= mlList.Count Then Return
        Dim file As String = mlList.Item(index).FileName
        If GetMediaType(g_lpFileSystem, file) <> enumPhotoMediaType.mdImage Then
            frmMsgBox.ShowCriticalMessage("只有照片可以當封面", "設為相本封面")
            Return
        End If
        section.CoverPhoto = file
        ' the folder / book the album is in: where its cover lives (AlbumCovers)
        Dim sectionPath As String = IO.Path.GetDirectoryName(section.Key.TrimEnd("\"c))
        AlbumCovers.ForgetCover(sectionPath, section)
        frmMsgBox.ShowSmileMessage("「" & section.DisplayName & "」的封面會用這張照片，下次打開封面牆時更新。", "設為相本封面")
    End Sub

    '==================================================================================================
    ' 移動到…
    '==================================================================================================
    Private Sub MovePhotos(ByVal indexes As List(Of Integer))
        If g_lpConfig.ReadOnly OrElse indexes.Count = 0 Then Return
        Dim target As String
        Using f As New frmPickAlbum
            target = f.Pick(If(indexes.Count > 1, "把 " & indexes.Count & " 張照片移到哪一本相本？", "把這張照片移到哪一本相本？"))
        End Using
        If String.IsNullOrEmpty(target) Then Return
        If m_frmViewer IsNot Nothing Then m_frmViewer.Clear()

        Dim moved As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim failed As New List(Of String)
        SetBusy(True)
        Try
            For Each i In indexes
                Dim src As String = mlList.Item(i).FileName
                If String.Equals(IO.Path.GetDirectoryName(src), target.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then Continue For   ' already there
                Dim dst As String = MoveOnePhoto(src, target)
                If dst Is Nothing Then failed.Add(src) Else moved(src) = dst
            Next
            FixBooks(moved)
        Finally
            SetBusy(False)
        End Try

        ' the photos left this album (not a face / search list: those show them wherever they are)
        Dim section As PhotoSet = CurrentSection()
        For Each i In indexes.OrderByDescending(Function(x) x)
            Dim src As String = mlList.Item(i).FileName
            If Not moved.ContainsKey(src) Then Continue For
            If m_lpAppEnv.ExeMode = enumExeMode.exeAlbums Then
                mlList.RemoveItem(i)
                section?.RemovePhoto(src)
            Else
                mlList.Item(i).FileName = moved(src)
            End If
        Next
        If mlList.Count > 0 Then SelectAfterRemove(indexes.Min())
        If failed.Count > 0 Then
            frmMsgBox.ShowCriticalMessage(failed.Count & " 張沒有移動（檔案可能正在使用中，或目的地無法寫入）" & vbCrLf & IO.Path.GetFileName(failed(0)), "移動到…")
        End If
    End Sub

    ''' <summary>Moves one photo with its side files into <paramref name="folder"/> (a free name when it is
    ''' taken there) and updates what refers to it; the new path, or Nothing when it didn't move.</summary>
    Private Function MoveOnePhoto(ByVal src As String, ByVal folder As String) As String
        If Not IO.File.Exists(src) Then Return Nothing
        Dim srcFolder As String = IO.Path.GetDirectoryName(src), srcBase As String = IO.Path.GetFileNameWithoutExtension(src)
        Dim ext As String = IO.Path.GetExtension(src)
        Dim base As String = srcBase, k As Integer = 1
        While IO.File.Exists(IO.Path.Combine(folder, base & ext)) OrElse IO.File.Exists(IO.Path.Combine(folder, base & "." & gc_strExifPattern))
            k += 1
            base = srcBase & " (" & k & ")"
        End While
        Dim dst As String = IO.Path.Combine(folder, base & ext)
        Try
            IO.File.Move(src, dst)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
        ' side files (a failure here leaves the photo moved; the side file stays behind)
        TryMove(IO.Path.Combine(srcFolder, srcBase & "." & gc_strExifPattern), IO.Path.Combine(folder, base & "." & gc_strExifPattern))
        TryMove(IO.Path.Combine(srcFolder, "Restore", srcBase & ext), IO.Path.Combine(folder, "Restore", base & ext))
        TryMove(IO.Path.Combine(srcFolder, "Thumb", srcBase & ".jpg"), IO.Path.Combine(folder, "Thumb", base & ".jpg"))
        ' what refers to it
        g_lpDatabase.Delete(src)
        Dim p As New Photo
        p.Construct(dst)
        g_lpDatabase.AddItem(p)
        g_lpDatabase.RenameFacePhoto(src, dst)
        If g_lpDock.RemoveItem(src) Then
            g_lpDock.AddItem(dst, p.Exif(enumPhotoExif.peDate), p.Exif(enumPhotoExif.peTime))
            g_lpDock.Save()
        End If
        Return dst
    End Function

    Private Shared Sub TryMove(ByVal src As String, ByVal dst As String)
        If Not IO.File.Exists(src) Then Return
        Try
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(dst))
            IO.File.Move(src, dst)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

    ''' <summary>Photo books (.Alm) listing a moved photo get its new path.</summary>
    Private Sub FixBooks(ByVal moved As Dictionary(Of String, String))
        If moved.Count = 0 Then Return
        For r = 0 To g_lpConfig.FavoriteCount - 1
            Dim root As String = g_lpConfig.FavoritePath(r)
            If Not IO.Directory.Exists(root) Then Continue For
            Dim books As IEnumerable(Of String)
            Try
                books = IO.Directory.GetFiles(root, "*." & gc_strFavoritesPattern, IO.SearchOption.AllDirectories)
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
                Continue For
            End Try
            For Each alm In books
                Try
                    Dim lines() As String = IO.File.ReadAllLines(alm, AnsiText.Encoding)
                    Dim changed As Boolean = False
                    For i = 0 To lines.Length - 1
                        If Not lines(i).StartsWith("File=", StringComparison.OrdinalIgnoreCase) Then Continue For
                        Dim dst As String = Nothing
                        If moved.TryGetValue(lines(i).Substring(5).Trim(), dst) Then
                            lines(i) = "File=" & dst
                            changed = True
                        End If
                    Next
                    If changed Then IO.File.WriteAllLines(alm, lines, AnsiText.Encoding)
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
                End Try
            Next
        Next
    End Sub

    '==================================================================================================
    ' 刪除檔案 / 永久刪除 / 從攝影集移除
    '==================================================================================================
    ''' <summary>Deletes the photos of list items <paramref name="indexes"/>, after a question: to the
    ''' Recycle Bin, or with <paramref name="permanent"/> outright (the question then defaults to 否).</summary>
    Private Sub DeletePhotoFiles(ByVal indexes As List(Of Integer), Optional ByVal permanent As Boolean = False)
        If g_lpConfig.ReadOnly OrElse indexes.Count = 0 Then Return
        Dim files As List(Of String) = indexes.Select(Function(i) mlList.Item(i).FileName).ToList()
        Dim what As String
        If files.Count > 1 Then
            what = files.Count & " 個檔案"
        Else
            what = If(GetMediaType(g_lpFileSystem, files(0)) = enumPhotoMediaType.mdVideo, "這段影片", "這張照片")
        End If
        Dim also As String = If(m_lpAppEnv.ExeMode = enumExeMode.exeFavorites, vbCrLf & "（相片庫裡的原始檔案也會一起刪除）", "")
        Dim names As String = String.Join(vbCrLf, files.Take(3).Select(Function(f) IO.Path.GetFileName(f))) & If(files.Count > 3, vbCrLf & "…", "")
        If permanent Then
            If Not frmQueryMsgBox.ShowMessage("是否確定永久刪除" & what & "？" & vbCrLf & "刪除後無法從資源回收筒還原。" & vbCrLf & names & also,
                                              "永久刪除", defaultNo:=True) Then Return
        ElseIf Not frmQueryMsgBox.ShowMessage("是否確定刪除" & what & "？會移到資源回收筒。" & vbCrLf & names & also, "刪除檔案") Then
            Return
        End If

        ' let go of the files first: the viewer's picture / player
        If m_frmViewer IsNot Nothing Then m_frmViewer.Clear()
        Dim section As PhotoSet = CurrentSection()
        Dim failed As New List(Of String)
        SetBusy(True)
        Try
            For Each i In indexes.OrderByDescending(Function(x) x)
                Dim file As String = mlList.Item(i).FileName
                mlList.RemoveItem(i)   ' also lets go of the thumbnail
                If Not PhotoFiles.DeletePhoto(file, permanent) Then
                    failed.Add(file)
                    Continue For
                End If
                section?.RemovePhoto(file)
            Next
            If TypeOf section Is Book Then CType(section, Book).Save()
            g_lpFaces?.RequestOrganizeSoon()
            SetDockProperty()
        Finally
            SetBusy(False)
        End Try
        If failed.Count > 0 Then
            frmMsgBox.ShowCriticalMessage("刪除失敗（檔案可能正在使用中）" & vbCrLf & String.Join(vbCrLf, failed.Take(3)), "刪除檔案")
            RefreshCurrentList(indexes.Min())   ' the files that stayed come back
            Return
        End If
        SelectAfterRemove(indexes.Min())
    End Sub

    ''' <summary>攝影集: the photos leave the book; the files stay where they are.</summary>
    Private Sub RemoveFromBook(ByVal indexes As List(Of Integer))
        If indexes.Count = 0 Then Return
        Dim book As Book = TryCast(CurrentSection(), Book)
        If book Is Nothing Then Return
        Dim what As String = If(indexes.Count > 1, indexes.Count & " 張照片", "這張照片")
        If Not frmQueryMsgBox.ShowMessage("把" & what & "從攝影集「" & book.Name & "」移除？" & vbCrLf & "照片本身不會刪除。", "從攝影集移除") Then Return
        If m_frmViewer IsNot Nothing Then m_frmViewer.Clear()
        For Each i In indexes.OrderByDescending(Function(x) x)
            Dim file As String = mlList.Item(i).FileName
            mlList.RemoveItem(i)
            book.RemovePhoto(file)
        Next
        book.Save()
        SelectAfterRemove(indexes.Min())
    End Sub

    ''' <summary>After items left the list: select the one now at the first place (or the last).</summary>
    Private Sub SelectAfterRemove(ByVal index As Integer)
        If mlList.Count = 0 Then
            m_lpCurrentPhoto = Nothing
            ShowPhotoIndex()
            Return
        End If
        mlList.SelectedIndex = Math.Min(index, mlList.Count - 1)
        mlList.Focus()
    End Sub

    ''' <summary>A failed delete: the album is shown again (the files still there come back).</summary>
    Private Sub RefreshCurrentList(ByVal index As Integer)
        m_strShownKey = Nothing
        If tvList.SelectedNode IsNot Nothing Then tvList_Click()
        If index < mlList.Count Then mlList.SelectedIndex = index
    End Sub

End Class
