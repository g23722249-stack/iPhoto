' 加密隱藏 in the main window (new in the .NET port; VaultFiles, PhotoVault):
'   - the padlock beside the thumbnail size buttons: closed = locked; a click asks the 金鑰密碼
'     (frmVault.AskUnlock), or, unlocked, locks again. 設定 › 一般 has the password too. Unlocked lasts
'     until iPhoto ends or 上鎖.
'   - unlocked: the photo list's right-click menu has 加密隱藏 (photos) / 解密顯示 (encrypted ones); an
'     album lists its encrypted photos after its own (a padlock in the corner), and the tree has
'     「加密照片」 with all of them by album.
'   - an encrypted photo can be looked at (全圖瀏覽, a video plays from a temporary file deleted after),
'     rated, decrypted and deleted (outright: an encrypted file in the Recycle Bin is no use); nothing
'     else -- the viewer's tool bar is off, the menu's other items are greyed, it never goes in the Dock.
'   - the first 加密隱藏 makes the 救援碼 (shown once) and, while the password is still 123456, asks
'     to change it first.
Partial Class frmMain

    Private Const VaultKey As String = "VAULT"
    Private picVault As PictureBox

    ''' <summary>The menu items an encrypted photo keeps.</summary>
    Private Shared ReadOnly VaultMenuAllowed As String() = {
        "mnuMyRanking", "mnuRankingNone", "mnuRankingLV1", "mnuRankingLV2", "mnuRankingLV3", "mnuRankingLV4", "mnuRankingLV5",
        "mnuView", "mnuShowInfo", "mnuDecrypt", "mnuDeleteFile", "mnuDeleteForever", "mnuSelectAll", "mnuSelectNone", "mnuSelectInvert"}

    Private Sub Vault_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        VaultFiles.Init()
        With AquaMenu1.Items
            Dim sel As Integer = -1
            For i = 0 To .Count - 1
                If String.Equals(.Item(i).Name, "mnuSelSep", StringComparison.OrdinalIgnoreCase) Then sel = i
            Next
            If sel < 0 Then sel = .Count
            .Insert(sel, New Aqua.MenuItem("mnuVaultSep", "-", 1, Nothing, Nothing))
            .Insert(sel + 1, New Aqua.MenuItem("mnuEncrypt", "加密隱藏…", 1, Nothing, Nothing))
            .Insert(sel + 2, New Aqua.MenuItem("mnuDecrypt", "解密顯示", 1, Nothing, Nothing))
        End With

        picVault = New PictureBox With {.Size = New Size(24, 24), .BackColor = Color.Transparent, .Cursor = Cursors.Hand,
                                        .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
                                        .Location = New Point(imgLargeSize.Left - 36, imgLargeSize.Top)}
        AddHandler picVault.Paint, AddressOf PaintVaultIcon
        AddHandler picVault.Click, Sub()
                                       If PhotoVault.IsUnlocked Then
                                           PhotoVault.Lock()
                                       Else
                                           frmVault.AskUnlock(Me)
                                       End If
                                   End Sub
        Controls.Add(picVault)
        picVault.BringToFront()
        AddHelp(picVault, "main.vault")   ' frmMain.Help.vb
        AddHandler PhotoVault.StateChanged, AddressOf Vault_StateChanged
    End Sub

    Private Sub Vault_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        RemoveHandler PhotoVault.StateChanged, AddressOf Vault_StateChanged
        PhotoVault.Lock()   ' the key forgotten, the temporary files gone
    End Sub

    Private Sub PaintVaultIcon(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Dim open As Boolean = PhotoVault.IsUnlocked
        Dim c As Color = If(open, Color.FromArgb(46, 125, 50), Color.FromArgb(96, 96, 96))
        Using pen As New Pen(c, 2.2F), br As New SolidBrush(c)
            ' the shackle: lifted to the left when open
            Dim sx As Single = If(open, 3.5F, 7.0F)
            g.DrawArc(pen, sx, 3.0F, 10.0F, 11.0F, 180, 180)
            g.DrawLine(pen, sx, 8.5F, sx, If(open, 8.5F, 12.0F))
            g.DrawLine(pen, sx + 10.0F, 8.5F, sx + 10.0F, 12.0F)
            g.FillRectangle(br, 4.0F, 11.5F, 16.0F, 10.0F)
        End Using
        g.FillEllipse(Brushes.White, 10.5F, 14.5F, 3.0F, 3.0F)
    End Sub

    ''' <summary>Unlocked or locked (here, from 設定, or at the end).</summary>
    Private Sub Vault_StateChanged(sender As Object, e As EventArgs)
        If IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New EventHandler(AddressOf Vault_StateChanged), sender, e)
            Return
        End If
        picVault?.Invalidate()
        Aqua.MediaViewerControl.ForgetProvidedThumbnails()
        AddVaultNode()
        If PhotoVault.IsUnlocked Then
            ' an album shown gets its encrypted photos -- once the password dialog (or 設定) is closed,
            ' not behind it
            If Form.ActiveForm IsNot Me Then
                m_bolVaultRefreshPending = True
                Return
            End If
            m_bolVaultRefreshPending = False
            If m_strShownKey IsNot Nothing AndAlso m_lpAppEnv.ExeMode = enumExeMode.exeAlbums AndAlso TypeOf CurrentSection() Is [Class] Then
                RefreshCurrentList(Math.Max(0, mlList.SelectedIndex))
            End If
            Return
        End If
        m_bolVaultRefreshPending = False
        ' locked: nothing encrypted stays in sight (the viewer lets go of a decrypted video first)
        If VaultCurrent() Then m_frmViewer?.Clear()
        If m_strShownKey = VaultKey Then
            ClearScreenAlbum()
            Return
        End If
        Dim anyShown As Boolean = False
        For i = 0 To mlList.Count - 1
            If PhotoVault.IsVaultItem(mlList.Item(i).FileName) Then anyShown = True : Exit For
        Next
        If anyShown Then
            m_frmViewer?.Clear()
            RefreshCurrentList(0)
        End If
    End Sub

    Private m_bolVaultRefreshPending As Boolean

    ''' <summary>Unlocked while a dialog was open: the list now.</summary>
    Private Sub Vault_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If m_bolVaultRefreshPending AndAlso PhotoVault.IsUnlocked Then BeginInvoke(New EventHandler(AddressOf Vault_StateChanged), Me, EventArgs.Empty)
    End Sub

    ''' <summary>An item's tooltip and rating again, after its information changed.</summary>
    Private Sub RefreshVaultItem(ByVal index As Integer)
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(mlList.Item(index).FileName)
        If meta Is Nothing Then Return
        Dim tip As String = "加密照片"
        If meta.ShotDate <> "" Then tip &= vbCrLf & "時間：" & If(meta.ShotDate.Length = 8, meta.ShotDate.Substring(0, 4) & "/" & meta.ShotDate.Substring(4, 2) & "/" & meta.ShotDate.Substring(6, 2), meta.ShotDate)
        If meta.Title <> "" Then tip &= vbCrLf & "標題：" & meta.Title
        mlList.Item(index).ToolTipText = tip
    End Sub

    ''' <summary>The current list item is an encrypted photo.</summary>
    Private Function VaultCurrent() As Boolean
        Return m_lpCurrentPhoto IsNot Nothing AndAlso PhotoVault.IsVaultItem(m_lpCurrentPhoto.FileDesc)
    End Function

    '==================================================================================================
    ' The tree: 加密照片
    '==================================================================================================
    ''' <summary>「加密照片」 at the end of the tree while unlocked (and there are any); gone when locked.</summary>
    Private Sub AddVaultNode()
        Dim n As TreeNode = tvList.Nodes.Cast(Of TreeNode)().FirstOrDefault(Function(x) TryCast(x.Tag, String) = VaultKey)
        If Not PhotoVault.IsUnlocked OrElse PhotoVault.ItemFiles().Count = 0 Then
            n?.Remove()
            Return
        End If
        If n Is Nothing Then
            n = New TreeNode("加密照片") With {.Name = VaultKey, .Tag = VaultKey, .ImageKey = "icoOther", .SelectedImageKey = "icoOther"}
            tvList.Nodes.Add(n)
        End If
        n.Text = "加密照片 (" & PhotoVault.ItemFiles().Count & ")"
    End Sub

    ''' <summary>Called first by tvList_Click: True when the node is 加密照片.</summary>
    Private Function VaultNodeClick(ByVal node As TreeNode) As Boolean
        If node Is Nothing OrElse TryCast(node.Tag, String) <> VaultKey Then Return False
        ShowVaultList()
        Return True
    End Function

    ''' <summary>Every encrypted photo, a heading per album.</summary>
    Private Sub ShowVaultList()
        If Not PhotoVault.IsUnlocked Then Return
        SetBusy(True)
        Enabled = False
        Try
            ClearScreenAlbum()
            With m_lpAppEnv
                .ExeMode = enumExeMode.exeFace   ' a list of files from anywhere
                .SectionIndex = -1
                .KeyIndex = -1
            End With
            Dim items As List(Of KeyValuePair(Of String, PhotoVault.VaultMeta)) = PhotoVault.Items()
            sliSize.Value = LimitFor(items.Count)
            mlList.BeginUpdate()
            Try
                For Each g In items.GroupBy(Function(kv) kv.Value.Album, StringComparer.OrdinalIgnoreCase).OrderBy(Function(x) x.Key, StringComparer.OrdinalIgnoreCase)
                    mlList.AddSection(IO.Path.GetFileName(PhotoVault.FullPath(g.Key).TrimEnd("\"c)) & vbTab & g.Count() & " 張")
                    For Each kv In g
                        AddVaultListItem(kv.Key, kv.Value)
                    Next
                Next
            Finally
                mlList.EndUpdate()
            End Try
            txtTitle.Text = "加密照片"
            txtRemark.Text = items.Count.ToString("#,0") & " 張，輸入金鑰密碼後才看得到"
            m_strShownKey = VaultKey
            If mlList.Count > 0 Then
                mlList.ScrollValue = mlList.ScrollMin
                mlList.SelectedIndex = 0
            End If
        Finally
            Enabled = True
            SetBusy(False)
        End Try
    End Sub

    Private Sub AddVaultListItem(ByVal item As String, ByVal meta As PhotoVault.VaultMeta)
        Dim tip As String = "加密照片"
        If meta.ShotDate <> "" Then tip &= vbCrLf & "時間：" & If(meta.ShotDate.Length = 8, meta.ShotDate.Substring(0, 4) & "/" & meta.ShotDate.Substring(4, 2) & "/" & meta.ShotDate.Substring(6, 2), meta.ShotDate)
        If meta.Title <> "" Then tip &= vbCrLf & "標題：" & meta.Title
        Dim it As Aqua.MediaItem = mlList.AddItem(item, tip)
        it.ToolTipTitle = meta.Name   ' not the vault file's name
        it.Ranking = If(meta.Ranking >= 1 AndAlso meta.Ranking <= 5, CType(meta.Ranking, Aqua.MediaItemRanking), Aqua.MediaItemRanking.NoRating)
    End Sub

    ''' <summary>After an album's photos (tvList_Click): its encrypted ones, while unlocked.</summary>
    Private Sub AddVaultItemsTo(ByVal section As PhotoSet)
        Dim album As [Class] = TryCast(section, [Class])
        If album Is Nothing OrElse Not PhotoVault.IsUnlocked Then Return
        Dim items As List(Of KeyValuePair(Of String, PhotoVault.VaultMeta)) = PhotoVault.ItemsIn(album.Path)
        If items.Count = 0 Then Return
        mlList.BeginUpdate()
        Try
            For Each kv In items
                AddVaultListItem(kv.Key, kv.Value)
            Next
        Finally
            mlList.EndUpdate()
        End Try
    End Sub

    '==================================================================================================
    ' The menu
    '==================================================================================================
    ''' <summary>The items SetVaultMenu greyed or hid for an encrypted photo, given back.</summary>
    Private ReadOnly m_vaultGreyed As New List(Of Aqua.MenuItem)

    ''' <summary>Called first by SetMediaListPopupMenu: what SetVaultMenu greyed last time is back (the
    ''' others then set it as usual).</summary>
    Private Sub ResetVaultMenu()
        For Each m In m_vaultGreyed
            m.Enabled = True
        Next
        m_vaultGreyed.Clear()
        AquaMenu1.Item("mnuDeleteForever").Visible = True
    End Sub

    ''' <summary>Called last by SetMediaListPopupMenu.</summary>
    Private Sub SetVaultMenu()
        Dim picked As List(Of Integer) = Targets(mlList.SelectedIndex)
        Dim vaulted As Integer = picked.Where(Function(i) PhotoVault.IsVaultItem(mlList.Item(i).FileName)).Count()
        Dim many As String = If(picked.Count > 1, "（" & picked.Count & " 張）", "")
        With AquaMenu1
            .Item("mnuEncrypt").Visible = PhotoVault.IsUnlocked AndAlso vaulted = 0
            .Item("mnuEncrypt").Enabled = Not g_lpConfig.ReadOnly
            .Item("mnuEncrypt").Text = "加密隱藏" & many & "…"
            .Item("mnuDecrypt").Visible = vaulted > 0
            .Item("mnuDecrypt").Enabled = Not g_lpConfig.ReadOnly
            .Item("mnuDecrypt").Text = "解密顯示" & If(vaulted > 1, "（" & vaulted & " 張）", "")
            .Item("mnuVaultSep").Visible = .Item("mnuEncrypt").Visible OrElse .Item("mnuDecrypt").Visible
            If vaulted = 0 Then Return
            ' an encrypted photo among them: only what it allows
            For i = 0 To .Items.Count - 1
                Dim m As Aqua.MenuItem = .Items(i)
                If m.Text = "-" OrElse VaultMenuAllowed.Contains(m.Name, StringComparer.OrdinalIgnoreCase) OrElse Not m.Enabled Then Continue For
                m.Enabled = False
                m_vaultGreyed.Add(m)
            Next
            .Item("mnuDeleteFile").Text = "刪除加密照片" & If(vaulted > 1, "（" & vaulted & " 張）", "") & "…"
            .Item("mnuDeleteForever").Visible = False
            .Item("mnuMyRanking").Enabled = Not g_lpConfig.ReadOnly
        End With
    End Sub

    ''' <summary>Called first by AquaMenu1_MenuSelected: True when done here.</summary>
    Private Function VaultMenuSelected(ByVal name As String, ByVal index As Integer) As Boolean
        Dim picked As List(Of Integer) = Targets(index)
        Dim vaulted As List(Of Integer) = picked.Where(Function(i) PhotoVault.IsVaultItem(mlList.Item(i).FileName)).ToList()
        Select Case name.ToUpperInvariant()
            Case "MNUENCRYPT" : EncryptPhotos(picked)
            Case "MNUDECRYPT" : DecryptPhotos(vaulted)
            Case "MNUSHOWINFO"
                If Not VaultCurrent() Then Return False
                Try
                    If VaultFiles.EditInfo(m_lpCurrentPhoto.FileDesc) Then RefreshVaultItem(index)
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is Security.Cryptography.CryptographicException OrElse TypeOf ex Is Xml.XmlException OrElse
                                           TypeOf ex Is InvalidOperationException
                    frmMsgBox.ShowCriticalMessage("無法修改加密照片的資訊" & vbCrLf & ex.Message, "顯示資訊")
                End Try
            Case "MNUDELETEFILE", "MNUDELETEFOREVER"
                If vaulted.Count = 0 Then Return False
                DeleteVaultItems(vaulted)
            Case "MNURANKINGNONE", "MNURANKINGLV1", "MNURANKINGLV2", "MNURANKINGLV3", "MNURANKINGLV4", "MNURANKINGLV5"
                If vaulted.Count = 0 Then Return False
                Dim level As Integer = If(name.EndsWith("None", StringComparison.OrdinalIgnoreCase), 0, CInt(name.Substring(name.Length - 1)))
                For Each i In picked
                    Dim f As String = mlList.Item(i).FileName
                    If PhotoVault.IsVaultItem(f) Then
                        VaultFiles.SetRanking(f, level)
                    Else
                        Dim p As New Photo
                        p.Construct(f)
                        p.Exif(enumPhotoExif.peRanking) = CStr(level)
                        g_lpDatabase.AddItem(p)
                    End If
                    mlList.Item(i).Ranking = CType(level, Aqua.MediaItemRanking)
                Next
            Case Else : Return False
        End Select
        Return True
    End Function

    '==================================================================================================
    ' 加密隱藏 / 解密顯示 / 刪除
    '==================================================================================================
    ''' <summary>The file a list item stands for: a video listed by its Thumb\ picture is the video.</summary>
    Private Shared Function MediaOfListFile(ByVal file As String) As String
        Dim folder As String = IO.Path.GetDirectoryName(file)
        If Not String.Equals(IO.Path.GetFileName(folder), "Thumb", StringComparison.OrdinalIgnoreCase) Then Return file
        Dim album As String = IO.Path.GetDirectoryName(folder), baseName As String = IO.Path.GetFileNameWithoutExtension(file)
        Try
            Dim video As String = IO.Directory.GetFiles(album, baseName & ".*").FirstOrDefault(
                Function(f) String.Equals(IO.Path.GetFileNameWithoutExtension(f), baseName, StringComparison.OrdinalIgnoreCase) AndAlso
                            GetMediaType(g_lpFileSystem, f) = enumPhotoMediaType.mdVideo)
            Return If(video, file)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return file
        End Try
    End Function

    Private Sub EncryptPhotos(ByVal indexes As List(Of Integer))
        If g_lpConfig.ReadOnly OrElse indexes.Count = 0 OrElse Not PhotoVault.IsUnlocked Then Return
        If PhotoVault.UsesDefaultPassword Then
            If frmQueryMsgBox.ShowMessage("金鑰密碼還是預設的 123456，任何人都猜得到。" & vbCrLf & "要先更改密碼嗎？", "加密隱藏") Then
                If Not frmVault.AskNewPassword(Me) Then Return
            End If
        End If
        Dim what As String = If(indexes.Count > 1, indexes.Count & " 張照片", "這張照片")
        If Not frmQueryMsgBox.ShowMessage("把" & what & "加密隱藏？" & vbCrLf &
                                          "加密後要輸入金鑰密碼才看得到；相片庫裡的原始檔案會刪除，" & vbCrLf &
                                          "只留加密的檔案（解密顯示可以完整還原）。", "加密隱藏") Then Return
        If Not PhotoVault.HasRecoveryCode Then
            Dim code As String = PhotoVault.NewRecoveryCode()
            If code IsNot Nothing Then frmVault.ShowRecoveryCode(Me, code)
        End If

        m_frmViewer?.Clear()
        Dim failed As New List(Of String)
        Dim section As PhotoSet = CurrentSection()
        SetBusy(True)
        Try
            For Each i In indexes.OrderByDescending(Function(x) x)
                Dim file As String = MediaOfListFile(mlList.Item(i).FileName)
                Try
                    VaultFiles.Encrypt(file)
                    section?.RemovePhoto(mlList.Item(i).FileName)
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is Security.Cryptography.CryptographicException OrElse TypeOf ex Is InvalidOperationException
                    failed.Add(IO.Path.GetFileName(file) & "：" & ex.Message)
                End Try
            Next
            g_lpFaces?.RequestOrganizeSoon()
            SetDockProperty()
        Finally
            SetBusy(False)
        End Try
        AfterVaultChange(indexes.Min())
        If failed.Count > 0 Then frmMsgBox.ShowCriticalMessage("加密失敗（原始檔案沒有變動）" & vbCrLf & String.Join(vbCrLf, failed.Take(3)), "加密隱藏")
    End Sub

    Private Sub DecryptPhotos(ByVal indexes As List(Of Integer))
        If g_lpConfig.ReadOnly OrElse indexes.Count = 0 Then Return
        m_frmViewer?.Clear()
        Dim failed As New List(Of String)
        SetBusy(True)
        Try
            For Each i In indexes.OrderByDescending(Function(x) x)
                Dim item As String = mlList.Item(i).FileName
                Try
                    VaultFiles.Decrypt(item)
                Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is Security.Cryptography.CryptographicException OrElse TypeOf ex Is InvalidOperationException
                    failed.Add(ex.Message)
                End Try
            Next
            SetDockProperty()
        Finally
            SetBusy(False)
        End Try
        PhotoVault.DeleteTempFiles()   ' a video the viewer had decrypted: it is a file of the album again
        AfterVaultChange(indexes.Min())
        If failed.Count > 0 Then frmMsgBox.ShowCriticalMessage("解密失敗" & vbCrLf & String.Join(vbCrLf, failed.Take(3)), "解密顯示")
    End Sub

    ''' <summary>Encrypted photos deleted outright, after a question.</summary>
    Private Sub DeleteVaultItems(ByVal indexes As List(Of Integer))
        If g_lpConfig.ReadOnly OrElse indexes.Count = 0 Then Return
        Dim what As String = If(indexes.Count > 1, indexes.Count & " 張加密照片", "這張加密照片")
        If Not frmQueryMsgBox.ShowMessage("是否確定刪除" & what & "？" & vbCrLf & "會直接刪除，無法還原。", "刪除加密照片", defaultNo:=True) Then Return
        m_frmViewer?.Clear()
        For Each i In indexes.OrderByDescending(Function(x) x)
            Dim item As String = mlList.Item(i).FileName
            mlList.RemoveItem(i)
            VaultFiles.Delete(item)
        Next
        PhotoVault.DeleteTempFiles()   ' a deleted video the viewer had decrypted
        AddVaultNode()
        SelectAfterRemove(indexes.Min())
    End Sub

    ''' <summary>Called first by DeletePhotoFiles: True when encrypted photos were among them (deleted here).</summary>
    Private Function VaultDeleteFiles(ByVal indexes As List(Of Integer)) As Boolean
        Dim vaulted As List(Of Integer) = indexes.Where(Function(i) PhotoVault.IsVaultItem(mlList.Item(i).FileName)).ToList()
        If vaulted.Count = 0 Then Return False
        DeleteVaultItems(vaulted)
        Return True
    End Function

    ''' <summary>The list again after photos went into or came out of the vault.</summary>
    Private Sub AfterVaultChange(ByVal index As Integer)
        Dim album As [Class] = TryCast(CurrentSection(), [Class])
        If album IsNot Nothing Then album.Reload()
        AddVaultNode()
        If m_strShownKey = VaultKey AndAlso PhotoVault.ItemFiles().Count = 0 Then
            ' all decrypted: the node is gone, so is its list
            ClearScreenAlbum()
            txtTitle.Text = "加密照片"
            txtRemark.Text = "加密的照片都已解密顯示，回到原本的相簿了。"
        ElseIf m_strShownKey = VaultKey Then
            ShowVaultList()
            If index < mlList.Count Then mlList.SelectedIndex = index
        Else
            RefreshCurrentList(Math.Max(0, index))
        End If
    End Sub

    '==================================================================================================
    ' 全圖瀏覽
    '==================================================================================================
    ''' <summary>Called first by ShowCurrentInViewer: True when the current photo is encrypted (shown here).</summary>
    Private Function ShowVaultInViewer(ByVal bolFirst As Boolean, ByVal bolLast As Boolean) As Boolean
        If Not VaultCurrent() Then Return False
        Dim item As String = m_lpCurrentPhoto.FileDesc
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(item)
        m_frmViewer.Clear()
        If meta Is Nothing Then Return True
        If meta.Video Then
            Dim temp As String = Nothing
            Try
                temp = PhotoVault.DecryptToTemp(item)
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is Security.Cryptography.CryptographicException OrElse TypeOf ex Is InvalidOperationException
            End Try
            If temp IsNot Nothing Then m_frmViewer.ShowVideo(temp, bolFirst, bolLast)
            ShowViewerState(item)
        Else
            ShowPictureInViewer(item, bolFirst, bolLast)   ' LoadPicture decrypts it
        End If
        Return True
    End Function

    ''' <summary>Called by ShowViewerState: an encrypted photo's rating and name; its tool bar off.</summary>
    Private Function ShowVaultViewerState(ByVal file As String) As Boolean
        Dim vault As Boolean = PhotoVault.IsVaultItem(file)
        m_frmViewer.EditLocked = vault
        If Not vault Then Return False
        Dim meta As PhotoVault.VaultMeta = PhotoVault.LoadMeta(file)
        m_frmViewer.Rating = If(meta Is Nothing, 0, meta.Ranking)
        m_frmViewer.Docked = False
        If meta IsNot Nothing Then TryCast(m_frmViewer, frmViewerLarge)?.ShowName(meta.Name & "（加密）")
        Return True
    End Function

End Class
