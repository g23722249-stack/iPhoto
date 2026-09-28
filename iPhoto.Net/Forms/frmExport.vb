' Port of iPhoto\Form\frmExport.frm: copies the docked files to a folder under new names (txtReName, "*"
' = the running number), optionally sorted by date and shrunk to one of the ddSize sizes (the list
' items are in the designer).
' Callers use "Using f As New frmExport".
' Fixed from VB6:
'   - "依時間排序" had no effect (the sort was skipped when ticked, and the export didn't use the sorted
'     list anyway); ticked now exports in date order, unticked in dock order.
'   - resizing stretched every picture to exactly 1280x1024 etc.; it now fits the picture inside that
'     size keeping its shape, and never enlarges a smaller one. A file that isn't a picture is copied.
'   - declining "create the folder?" still exported into the missing folder.
'   - a folder inside an album / photo-book root was not caught (only one containing a root).
Friend Class frmExport

    Private Class ExportItem
        Public FileDesc As String
        Public CreateDate As String
        Public CreateTime As String
    End Class

    Private ReadOnly m_objExport As New List(Of ExportItem)

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Text = "匯出：" & g_lpDock.Count & " 張相片"
        DeskTop1_SelectedChanged(DeskTop1, EventArgs.Empty)

        m_objExport.Clear()
        For I As Integer = 0 To g_lpDock.Count - 1
            m_objExport.Add(New ExportItem With {
                .FileDesc = g_lpDock.FileName(I),
                .CreateDate = GetCreateDate(g_lpDock.FileName(I), g_lpDock.CreateDate(I)),
                .CreateTime = GetCreateTime(g_lpDock.FileName(I), g_lpDock.CreateTime(I))})
        Next
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = Math.Max(1, g_lpDock.Count)
        ProgressBar1.Value = 0
    End Sub

    Private Sub DeskTop1_SelectedChanged(sender As Object, e As EventArgs) Handles DeskTop1.SelectedChanged
        txtPath.Text = DeskTop1.Path
    End Sub

    Private Shared Function GetCreateDate(ByVal strFileDesc As String, ByVal strDate As String) As String
        If Val(strDate) = 0 Then Return IO.File.GetLastWriteTime(strFileDesc).ToString("yyyyMMdd")
        Return CLng(Val(strDate)).ToString("00000000")
    End Function

    Private Shared Function GetCreateTime(ByVal strFileDesc As String, ByVal strTime As String) As String
        If Val(strTime) = 0 Then Return IO.File.GetLastWriteTime(strFileDesc).ToString("HHmmss")
        Return CLng(Val(strTime)).ToString("000000")
    End Function

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Shared Function Overlaps(ByVal a As String, ByVal b As String) As Boolean
        a = IO.Path.GetFullPath(a).TrimEnd("\"c) & "\"
        b = IO.Path.GetFullPath(b).TrimEnd("\"c) & "\"
        Return a.StartsWith(b, StringComparison.OrdinalIgnoreCase) OrElse b.StartsWith(a, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        Dim strFolderDesc As String = GetClearFolderDesc(txtPath.Text).Trim()
        If strFolderDesc = "" Then
            frmMsgBox.ShowCriticalMessage("請選取資料夾", "")
            Return
        End If

        If txtReName.Text.Trim() = "" Then
            txtReName.Text = "Image*"
        ElseIf Not txtReName.Text.Contains("*") Then
            txtReName.Text = txtReName.Text.Trim() & "*"
        End If
        Do While txtReName.Text.Contains("**")
            txtReName.Text = txtReName.Text.Replace("**", "*")
        Loop
        If txtReName.Text.IndexOfAny(IO.Path.GetInvalidFileNameChars().Where(Function(c) c <> "*"c).ToArray()) >= 0 Then
            frmMsgBox.ShowCriticalMessage("名稱含有不合法的字元", "")
            txtReName.Focus()
            Return
        End If

        For I As Integer = 0 To g_lpStorage.AlbumCount - 1
            If Overlaps(g_lpStorage.Album(I).Path, strFolderDesc) Then
                frmMsgBox.ShowCriticalMessage("資料夾與相片庫重複", "")
                Return
            End If
        Next
        For I As Integer = 0 To g_lpStorage.FavoriteCount - 1
            If Overlaps(g_lpStorage.Favorite(I).Path, strFolderDesc) Then
                frmMsgBox.ShowCriticalMessage("資料夾與攝影集重複", "")
                Return
            End If
        Next

        If Not g_lpFileSystem.FolderExists(strFolderDesc) Then
            If Not frmQueryMsgBox.ShowMessage("資料夾不存在" & vbCrLf & "是否建立？", "注意") Then Return
            If Not g_lpFileSystem.CreateFolder(strFolderDesc) Then
                frmMsgBox.ShowCriticalMessage("建立資料夾失敗", "")
                Return
            End If
        End If

        ExportFiles(strFolderDesc, SortExportFile())
        Focus()
    End Sub

    Private Function SortExportFile() As List(Of ExportItem)
        If Not chkSort.Checked Then Return m_objExport.ToList()
        Return m_objExport.OrderBy(Function(x) x.CreateDate, StringComparer.Ordinal).
                           ThenBy(Function(x) x.CreateTime, StringComparer.Ordinal).ToList()
    End Function

    ''' <summary>The picture fitted inside the chosen size (never enlarged), saved as JPEG quality 100;
    ''' False when it isn't a picture GDI+ can read.</summary>
    Private Shared Function SaveResized(ByVal source As String, ByVal target As String, ByVal maxW As Integer, ByVal maxH As Integer) As Boolean
        Dim src As Bitmap
        Try
            src = Quartz.LoadPicture(source)
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is IO.IOException
            Return False
        End Try
        If src Is Nothing Then Return False
        Using src
            ' a portrait fits the same box turned sideways (1280x1024 -> 1024x1280)
            If src.Height > src.Width Then
                Dim t As Integer = maxW : maxW = maxH : maxH = t
            End If
            Using fitted As Bitmap = Quartz.ImageFilter.Fit(src, maxW, maxH, False)
                Return Quartz.SavePicture(fitted, target, 100)
            End Using
        End Using
    End Function

    Private Sub ExportFiles(ByVal strFolderDesc As String, ByVal items As List(Of ExportItem))
        Dim size As Integer = CInt(Val(If(ddSize.SelectedItem?.Name, "")))
        Dim digits As New String("0"c, items.Count.ToString().Length)
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            ProgressBar1.Value = 0
            For I As Integer = 0 To items.Count - 1
                ProgressBar1.Value = Math.Min(ProgressBar1.Maximum, I + 1)
                Application.DoEvents()
                Dim source As String = items(I).FileDesc
                Dim strFileName As String = txtReName.Text.Replace("*", (I + 1).ToString(digits))
                Dim strFileDesc As String = strFolderDesc & "\" & strFileName & "." & g_lpFileSystem.AnalyseFile(Carbon.AnalyseFileConstants.fsExtensionName, source)
                Dim ok As Boolean
                Select Case size
                    Case 1280 : ok = SaveResized(source, strFileDesc, 1280, 1024)
                    Case 1024 : ok = SaveResized(source, strFileDesc, 1024, 768)
                    Case 640 : ok = SaveResized(source, strFileDesc, 640, 480)
                    Case 320 : ok = SaveResized(source, strFileDesc, 320, 240)
                    Case Else : ok = False
                End Select
                If Not ok Then ok = g_lpFileSystem.CopyFile(source, strFileDesc)
                If Not ok Then frmMsgBox.ShowCriticalMessage("儲存檔案 " & source & " 失敗", "")
            Next
        Finally
            ProgressBar1.Value = 0
            Cursor = Cursors.Default
            Enabled = True
        End Try
        g_lpConfig.PlaySound(Config.enumSound.snExportFinish)
    End Sub

End Class
