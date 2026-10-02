' 設定 › 維護 (new in the .NET port; PhotoLib Maintenance): what is backed up and when, the backup
' folder, a .Exif backup on request, and clearing the records of photos that are gone. Made in code so
' the designer file stays as ported. Everything here happens at once (not on 儲存).
Partial Class frmSetup

    Private pageMaint As Aqua.TabPage
    Private lblDbBackup, lblExifBackup As Label
    Private WithEvents lblOpenBackups, lblExifNow, lblCleanOrphans, lblNameCheck, lblDuplicates, lblExifRegen, lblPeopleNames As Label
    Private m_bolExifRegenBusy As Boolean

    Private Sub MaintPage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        pageMaint = New Aqua.TabPage With {.Title = "維護", .BackColor = Color.White, .Font = pageGeneral.Font, .ForeColor = pageGeneral.ForeColor}
        Dim big As Font = chkPrivilege.Font
        Dim small As New Font(big.FontFamily, 11.0F)
        Dim x As Integer = 40

        AddHeading("資料庫備份", x, 24, big)
        lblDbBackup = AddNote(x + 20, 56, small)
        AddHeading(".Exif 備份（照片的標題、人物、評價等資訊）", x, 120, big)
        lblExifBackup = AddNote(x + 20, 152, small)
        lblExifNow = AddLink("立即備份 .Exif", x + 20, 200, big, Color.FromArgb(42, 116, 208))
        lblOpenBackups = AddLink("開啟備份資料夾", x + 250, 200, big, Color.FromArgb(42, 116, 208))
        lblExifRegen = AddLink("重新產生 .Exif…", x + 480, 200, big, Color.FromArgb(42, 116, 208))   ' frmSetup.ExifRegen.vb

        AddHeading("清理失效紀錄", x, 256, big)
        pageMaint.Controls.Add(New Label With {
            .AutoSize = False, .Size = New Size(700, 44), .Location = New Point(x + 20, 288), .Font = small, .ForeColor = Color.DimGray,
            .BackColor = Color.Transparent,
            .Text = "在 iPhoto 以外刪除或搬走的照片，資料庫裡的索引和面孔資料還會留著；" & vbCrLf & "檢查後會先告訴你有幾筆，確定才清除（照片檔案不受影響）。"})
        lblCleanOrphans = AddLink("檢查並清理…", x + 20, 340, big, Color.FromArgb(42, 116, 208))
        lblNameCheck = AddLink("人物欄名字檢查…", x + 250, 340, big, Color.FromArgb(42, 116, 208))
        lblDuplicates = AddLink("尋找重複照片…", x + 480, 340, big, Color.FromArgb(42, 116, 208))
        lblPeopleNames = AddLink("全部人物名字…", x + 250, 380, big, Color.FromArgb(42, 116, 208))

        tabSetup.TabPages.Add(pageMaint)
        ShowBackupState()
    End Sub

    Private Sub AddHeading(ByVal text As String, ByVal x As Integer, ByVal y As Integer, ByVal f As Font)
        pageMaint.Controls.Add(New Label With {.AutoSize = True, .Font = f, .Text = text, .Location = New Point(x, y), .BackColor = Color.Transparent})
    End Sub

    Private Function AddNote(ByVal x As Integer, ByVal y As Integer, ByVal f As Font) As Label
        Dim l As New Label With {.AutoSize = False, .Size = New Size(700, 44), .Location = New Point(x, y), .Font = f, .ForeColor = Color.DimGray, .BackColor = Color.Transparent}
        pageMaint.Controls.Add(l)
        Return l
    End Function

    Private Function AddLink(ByVal text As String, ByVal x As Integer, ByVal y As Integer, ByVal f As Font, ByVal c As Color) As Label
        Dim l As New Label With {
            .AutoSize = True, .Font = New Font(f.FontFamily, 12.0F, FontStyle.Underline), .ForeColor = c, .Cursor = Cursors.Hand,
            .Location = New Point(x, y), .BackColor = Color.Transparent, .Text = text}
        pageMaint.Controls.Add(l)
        Return l
    End Function

    Private ReadOnly Property MdbFile As String
        Get
            Return g_lpConfig.Attached(Config.enumAttachedFile.filDatabase)
        End Get
    End Property

    Private Sub ShowBackupState()
        Dim mdb As String = MdbFile
        Dim ok As Boolean = Not String.IsNullOrEmpty(mdb) AndAlso IO.File.Exists(mdb)
        Dim folder As String = If(ok, Maintenance.BackupFolder(mdb), "")
        Dim dbs = If(ok, Maintenance.Backups(folder, "iPhoto_*.mdb"), New List(Of IO.FileInfo))
        Dim zips = If(ok, Maintenance.Backups(folder, "Exif_*.zip"), New List(Of IO.FileInfo))
        lblDbBackup.Text = $"每次開啟 iPhoto 時自動備份 iPhoto.mdb，保留最近 {Maintenance.DatabaseKeep} 份。" & vbCrLf &
                           If(dbs.Count = 0, "還沒有備份。", $"最近一次：{dbs(0).CreationTime:yyyy/MM/dd HH:mm}（共 {dbs.Count} 份）")
        lblExifBackup.Text = $"每 {Maintenance.ExifEveryDays} 天在背景自動備份一次所有 .Exif，保留最近 {Maintenance.ExifKeep} 份。" & vbCrLf &
                             If(zips.Count = 0, "還沒有備份。", $"最近一次：{zips(0).LastWriteTime:yyyy/MM/dd HH:mm}（共 {zips.Count} 份）")
        Dim writable As Boolean = ok AndAlso Not g_lpConfig.ReadOnly
        lblExifNow.Enabled = writable
        lblOpenBackups.Enabled = ok AndAlso IO.Directory.Exists(folder)
        lblCleanOrphans.Enabled = writable AndAlso g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement
        lblNameCheck.Enabled = writable
        lblPeopleNames.Enabled = writable
        lblDuplicates.Enabled = writable
        lblExifRegen.Enabled = writable AndAlso Not m_bolExifRegenBusy AndAlso g_lpConfig.AlbumCount > 0
        If Not ok Then lblDbBackup.Text = "找不到資料庫檔案，無法備份。"
    End Sub

    Private Sub lblOpenBackups_Click(sender As Object, e As EventArgs) Handles lblOpenBackups.Click
        If Not lblOpenBackups.Enabled Then Return
        Try
            Process.Start(New ProcessStartInfo(Maintenance.BackupFolder(MdbFile)) With {.UseShellExecute = True})
        Catch ex As ComponentModel.Win32Exception
            frmMsgBox.ShowCriticalMessage("無法開啟資料夾" & vbCrLf & ex.Message, "")
        End Try
    End Sub

    Private Sub lblExifNow_Click(sender As Object, e As EventArgs) Handles lblExifNow.Click
        If Not lblExifNow.Enabled Then Return
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Application.UseWaitCursor = True
        Dim result As (Zip As String, Count As Integer)
        Try
            result = Maintenance.BackupExif(MdbFile, roots)
        Finally
            Application.UseWaitCursor = False
        End Try
        ShowBackupState()
        If result.Zip = "" Then
            frmMsgBox.ShowCriticalMessage(".Exif 備份失敗", "")
        Else
            frmMsgBox.ShowExclamationMessage($"已備份 {result.Count:#,0} 個 .Exif" & vbCrLf & IO.Path.GetFileName(result.Zip), ".Exif 備份")
        End If
    End Sub

    ''' <summary>Duplicate photos in the albums (frmDuplicates).</summary>
    Private Sub lblDuplicates_Click(sender As Object, e As EventArgs) Handles lblDuplicates.Click
        If Not lblDuplicates.Enabled Then Return
        Using f As New frmDuplicates
            f.ShowDialog(Me)
        End Using
    End Sub

    ''' <summary>Spellings of one name in the people fields (frmNameCheck).</summary>
    Private Sub lblNameCheck_Click(sender As Object, e As EventArgs) Handles lblNameCheck.Click
        If Not lblNameCheck.Enabled Then Return
        Using f As New frmNameCheck
            f.ShowDialog(Me)
        End Using
        ShowBackupState()
    End Sub

    ''' <summary>Every name in the people fields: rename, merge, take out (frmPeopleNames).</summary>
    Private Sub lblPeopleNames_Click(sender As Object, e As EventArgs) Handles lblPeopleNames.Click
        If Not lblPeopleNames.Enabled Then Return
        Using f As New frmPeopleNames
            f.ShowDialog(Me)
        End Using
        ShowBackupState()
    End Sub

    Private Sub lblCleanOrphans_Click(sender As Object, e As EventArgs) Handles lblCleanOrphans.Click
        If Not lblCleanOrphans.Enabled Then Return
        Application.UseWaitCursor = True
        Dim orphans As (Index As List(Of String), Faces As List(Of String))
        Try
            orphans = Maintenance.FindOrphans(g_lpDatabase)
        Finally
            Application.UseWaitCursor = False
        End Try
        If orphans.Index.Count = 0 AndAlso orphans.Faces.Count = 0 Then
            frmMsgBox.ShowExclamationMessage("沒有失效的紀錄", "清理失效紀錄")
            Return
        End If
        Dim sample As String = String.Join(vbCrLf, orphans.Index.Union(orphans.Faces, StringComparer.OrdinalIgnoreCase).Take(3).Select(Function(f) IO.Path.GetFileName(f)))
        If Not frmQueryMsgBox.ShowMessage($"找到已不存在的照片：索引 {orphans.Index.Count:#,0} 筆、面孔資料 {orphans.Faces.Count:#,0} 張照片。" & vbCrLf &
                                          "例如：" & vbCrLf & sample & vbCrLf & "清除這些紀錄嗎？（照片檔案不受影響）", "清理失效紀錄") Then Return
        Application.UseWaitCursor = True
        Dim n As Integer
        Try
            n = Maintenance.CleanOrphans(g_lpDatabase, orphans)
            g_lpFaces?.RequestOrganize()
        Finally
            Application.UseWaitCursor = False
        End Try
        frmMsgBox.ShowExclamationMessage($"已清除 {n:#,0} 張照片的失效紀錄", "清理失效紀錄")
    End Sub

End Class
