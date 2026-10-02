Imports System.Threading.Tasks

' 設定 › 維護 「重新產生 .Exif…」 (the link is made in frmSetup.Maintenance.vb): ExifRegen.RegenerateAll
' after a .Exif backup, on a worker thread; the result shows under the .Exif backup heading. Happens at
' once (not on 儲存); 設定 can't be closed while it runs.
Partial Class frmSetup

    Private Sub lblExifRegen_Click(sender As Object, e As EventArgs) Handles lblExifRegen.Click
        If Not lblExifRegen.Enabled OrElse m_bolExifRegenBusy Then Return
        If Not frmQueryMsgBox.ShowMessage("重新產生所有照片與影片的 .Exif？" & vbCrLf &
                                          "沒有 .Exif 的會建立；已有的只補上沒有或空白的欄位（日期時間、GPS、地點、Country、City、Town），" &
                                          "已經有內容的欄位不會改。開始前會先備份所有 .Exif；照片很多時要幾分鐘。", "重新產生 .Exif") Then Return
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        Dim mdb As String = MdbFile
        SetExifRegenBusy(True)
        lblExifBackup.Text = "備份 .Exif…"
        Application.DoEvents()
        Dim backup As (Zip As String, Count As Integer) = Maintenance.BackupExif(mdb, roots)
        If backup.Zip = "" Then
            SetExifRegenBusy(False)
            ShowBackupState()
            frmMsgBox.ShowCriticalMessage(".Exif 備份失敗，沒有重新產生", "")
            Return
        End If
        Task.Run(Function() ExifRegen.RegenerateAll(mdb, roots, Sub(done, total) BeginInvoke(Sub() lblExifBackup.Text = $"重新產生 .Exif… {done:#,0} / {total:#,0}"))).ContinueWith(
            Sub(t)
                BeginInvoke(Sub()
                                SetExifRegenBusy(False)
                                ShowBackupState()
                                If t.IsFaulted Then
                                    frmMsgBox.ShowCriticalMessage("重新產生 .Exif 中斷：" & t.Exception.GetBaseException().Message, "")
                                    Return
                                End If
                                Dim r As ExifRegen.Result = t.Result
                                frmMsgBox.ShowExclamationMessage($"看過 {r.Checked:#,0} 個檔案：新建 {r.Created:#,0} 個 .Exif，補寫 {r.Updated:#,0} 個（共 {r.Fields:#,0} 個欄位）" &
                                                                 If(r.Failed > 0, vbCrLf & $"{r.Failed} 個失敗，例如 {r.LastError}", "") & vbCrLf &
                                                                 "開始前的備份：" & IO.Path.GetFileName(backup.Zip), "重新產生 .Exif")
                            End Sub)
            End Sub)
    End Sub

    Private Sub SetExifRegenBusy(ByVal busy As Boolean)
        m_bolExifRegenBusy = busy
        lblExifRegen.Enabled = Not busy AndAlso Not g_lpConfig.ReadOnly
        lblExifNow.Enabled = Not busy AndAlso Not g_lpConfig.ReadOnly
        butSave.Enabled = Not busy
        butExit.Enabled = Not busy
    End Sub

    Private Sub ExifRegen_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If m_bolExifRegenBusy Then e.Cancel = True
    End Sub

End Class
