' Port of iPhoto\Form\frmDropFileList.frm: the docked files, sorted by date, to drag into a burning
' program (dragging any row drags all of them), save as a text list (opened in Notepad) or copy to a
' folder as Photo0001.jpg, Photo0002.jpg, ...
' Callers use "Using f As New frmDropFileList".
' Changed from VB6:
'   - "複製到資料夾" wiped the chosen folder without asking; it now asks first when the folder already
'     holds files (and still refuses a folder that overlaps an album / photo-book root, now in both
'     directions -- VB6 only caught a folder that contained a root).
'   - the numbering is zero-padded to max(4, digits of the count); VB6's Format call lost the pattern
'     for more than 4 files, giving Photo1, Photo10, ...
Friend Class frmDropFileList

    Public Sub ShowDropFileList()
        If g_lpDock.Count <= 0 Then Return
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ShowFormTitle()
        AddDockItem()
    End Sub

    Private Sub ShowFormTitle()
        Dim dblSize As Double = 0
        For I As Integer = 0 To g_lpDock.Count - 1
            Dim fi As New IO.FileInfo(g_lpDock.FileName(I))
            If fi.Exists Then dblSize += fi.Length / 1024
        Next
        dblSize = Math.Round(dblSize / 1024, 2)
        Text = "檔案清單：共 " & g_lpDock.Count & " 筆" & "  " & dblSize & " Mb"
    End Sub

    ''' <summary>VB6 Format(x, "0000/00/00"): digits through a picture, anything else as is.</summary>
    Private Shared Function FormatDigits(ByVal value As String, ByVal picture As String) As String
        Dim n As Long
        If Long.TryParse(If(value, "").Trim(), n) Then Return n.ToString(picture)
        Return If(value, "")
    End Function

    Public Sub AddDockItem()
        Viewer.BeginUpdate()
        Viewer.Items.Clear()
        For Each row As System.Data.DataRow In g_lpDock.SortRecordSet.Rows
            Dim item As ListViewItem = Viewer.Items.Add(FormatDigits(CStr(row("SortDate")), "0000/00/00") & " " & FormatDigits(CStr(row("SortTime")), "00:00:00"))
            item.SubItems.Add(CStr(row("FileDesc")).Trim())
        Next
        Viewer.EndUpdate()
    End Sub

    Private Function ListedFiles() As List(Of String)
        Return Viewer.Items.Cast(Of ListViewItem)().Select(Function(i) i.SubItems(1).Text).ToList()
    End Function

    Private Sub cmdSave_Click(sender As Object, e As EventArgs) Handles cmdSave.Click
        Dim strOutputFile As String = IO.Path.Combine(IO.Path.GetTempPath(), "iPhotoIndex.Lst")
        IO.File.WriteAllLines(strOutputFile, ListedFiles(), AnsiText.Encoding)
        Process.Start("notepad.exe", """" & strOutputFile & """")
    End Sub

    Private Shared Function Overlaps(ByVal a As String, ByVal b As String) As Boolean
        a = IO.Path.GetFullPath(a).TrimEnd("\"c) & "\"
        b = IO.Path.GetFullPath(b).TrimEnd("\"c) & "\"
        Return a.StartsWith(b, StringComparison.OrdinalIgnoreCase) OrElse b.StartsWith(a, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub cmdSaveToFolder_Click(sender As Object, e As EventArgs) Handles cmdSaveToFolder.Click
        Dim strFolderDesc As String = frmBrowserFolder.GetFolder("開啟資料夾")
        If strFolderDesc.Trim() = "" Then Return

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

        Dim objFileSystem As New Carbon.FileSystem
        If Not objFileSystem.FolderExists(strFolderDesc) Then
            If Not frmQueryMsgBox.ShowMessage("資料夾不存在" & vbCrLf & "是否建立？", "注意") Then Return
        ElseIf IO.Directory.EnumerateFileSystemEntries(strFolderDesc).Any() Then
            If Not frmQueryMsgBox.ShowMessage("資料夾內已有檔案" & vbCrLf & "是否清除後再複製？", "注意") Then Return
            ' 清除現有資料
            objFileSystem.DeleteFolder(strFolderDesc)
        End If
        If Not objFileSystem.CreateFolder(strFolderDesc) Then
            frmMsgBox.ShowCriticalMessage("建立資料夾失敗", "")
            Return
        End If

        Dim files As List(Of String) = ListedFiles()
        Dim digits As New String("0"c, Math.Max(4, files.Count.ToString().Length))
        Cursor = Cursors.WaitCursor
        Try
            For I As Integer = 0 To files.Count - 1
                Viewer.Items(I).Selected = True
                Viewer.Items(I).EnsureVisible()
                Application.DoEvents()
                Dim strSaveFileName As String = strFolderDesc & "\Photo" & (I + 1).ToString(digits) & "." &
                                                objFileSystem.AnalyseFile(Carbon.AnalyseFileConstants.fsExtensionName, files(I))
                objFileSystem.CopyFile(files(I), strSaveFileName)
            Next
        Finally
            Cursor = Cursors.Default
        End Try
        frmMsgBox.ShowExclamationMessage("儲存成功", "")
        Process.Start("explorer.exe", """" & strFolderDesc & """")
    End Sub

    Private Sub cmdUnload_Click(sender As Object, e As EventArgs) Handles cmdUnload.Click
        Close()
    End Sub

    ' VB6 OLEDrag: all listed files, as a file list (copy)
    Private Sub Viewer_ItemDrag(sender As Object, e As ItemDragEventArgs) Handles Viewer.ItemDrag
        If Viewer.Items.Count = 0 OrElse e.Button <> MouseButtons.Left Then Return
        Dim data As New DataObject(DataFormats.FileDrop, ListedFiles().ToArray())
        Viewer.DoDragDrop(data, DragDropEffects.Copy)
    End Sub

End Class
