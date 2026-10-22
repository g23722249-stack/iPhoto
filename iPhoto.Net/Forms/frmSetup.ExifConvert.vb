Imports System.Threading.Tasks

' 設定 › 維護 「轉換 .Exif 為新格式…」 (the link is made in frmSetup.Maintenance.vb): FaceArchive.ConvertAll
' after a .Exif backup, on a worker thread -- every .Exif in the XML format (an entry per file of the
' name), then the face data of every photo into its .Exif and Faces.xml. From then on iPhoto keeps them up
' to date by itself (frmMain.Maintenance.vb), and 重建 can make the face tables again from them. Run
' again later, it writes all the face data afresh. Shares 重新產生 .Exif's busy flag: one at a time, and
' 設定 can't be closed while it runs.
Partial Class frmSetup

    ''' <summary>The link's text and the note beside it: converted (and when the face data was last
    ''' written) or not yet. Called by ShowBackupState.</summary>
    Private Sub ShowExifFormat(ByVal writable As Boolean)
        If lblExifConvert Is Nothing Then Return
        Dim last As DateTime? = If(String.IsNullOrEmpty(MdbFile), Nothing, FaceArchive.LastExport(MdbFile))
        lblExifConvert.Text = If(last.HasValue, "重新寫入全部面孔資料…", "轉換 .Exif 為新格式…")
        lblExifFormat.Text = If(last.HasValue, $"新格式；面孔資料最近寫入 .Exif：{last.Value:yyyy/MM/dd HH:mm}", "尚未轉換：照片資料還是舊格式，面孔資料只在資料庫裡")
        lblExifConvert.Enabled = writable AndAlso Not m_bolExifRegenBusy AndAlso g_lpConfig.AlbumCount > 0 AndAlso
                                 g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement
    End Sub

    Private Sub lblExifConvert_Click(sender As Object, e As EventArgs) Handles lblExifConvert.Click
        If Not lblExifConvert.Enabled OrElse m_bolExifRegenBusy Then Return
        Dim mdb As String = MdbFile
        Dim again As Boolean = FaceArchive.LastExport(mdb).HasValue
        If Not frmQueryMsgBox.ShowMessage(If(again,
                                             "把資料庫裡所有的面孔資料重新寫入每張照片的 .Exif？" & vbCrLf & "平常不需要：iPhoto 每 10 分鐘會自動寫入有改過的。",
                                             "把所有 .Exif 轉成新格式，並把面孔資料寫進去？" & vbCrLf &
                                             "同名的照片和影片（例如 IMG_1.JPG 和 IMG_1.MP4）會各有自己的資料；之後面孔資料也能從 .Exif 救回來。" & vbCrLf &
                                             "開始前會先備份所有 .Exif；照片很多時要幾分鐘。") , "轉換 .Exif") Then Return
        Dim roots As New List(Of String)
        For i = 0 To g_lpConfig.AlbumCount - 1
            roots.Add(g_lpConfig.AlbumPath(i))
        Next
        SetExifRegenBusy(True)
        lblExifConvert.Enabled = False
        lblExifBackup.Text = "備份 .Exif…"
        Application.DoEvents()
        Dim backup As (Zip As String, Count As Integer) = Maintenance.BackupExif(mdb, roots)
        If backup.Zip = "" Then
            SetExifRegenBusy(False)
            ShowBackupState()
            frmMsgBox.ShowCriticalMessage(".Exif 備份失敗，沒有轉換", "")
            Return
        End If
        Task.Run(Function() FaceArchive.ConvertAll(mdb, roots, Sub(stepText, done, total) BeginInvoke(Sub() lblExifBackup.Text = $"{stepText}… {done:#,0} / {total:#,0}"))).ContinueWith(
            Sub(t)
                BeginInvoke(Sub()
                                SetExifRegenBusy(False)
                                ShowBackupState()
                                If t.IsFaulted Then
                                    frmMsgBox.ShowCriticalMessage("轉換 .Exif 中斷：" & t.Exception.GetBaseException().Message, "")
                                    Return
                                End If
                                Dim r As FaceArchive.Result = t.Result
                                frmMsgBox.ShowExclamationMessage($"轉成新格式 {r.Converted:#,0} 個 .Exif，寫入 {r.Written:#,0} 張照片的面孔資料" &
                                                                 If(r.Failed > 0, vbCrLf & $"{r.Failed} 個失敗，例如 {r.LastError}（下次會再試）", "") & vbCrLf &
                                                                 "開始前的備份：" & IO.Path.GetFileName(backup.Zip), "轉換 .Exif")
                            End Sub)
            End Sub)
    End Sub

End Class
