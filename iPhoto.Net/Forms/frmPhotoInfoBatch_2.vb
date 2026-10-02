' Port of iPhoto\Form\frmPhotoInfoBatch_2.frm: step 2 of the batch edit -- thumbnails of every picture
' in the albums chosen in step 1 (sorted by album, date, time). Clicking one shows its .Exif fields in
' "目前選擇的照片", which are saved as soon as a field is left; "所有打勾的照片" writes title / people /
' place / keywords / remark to every selected thumbnail at once. The "▼" buttons drop a checklist of
' the keyword file's people or place entries.
'
' VB6 -> .NET:
'   - the vbAccelerator ExplorerBar -> barSearch (a scrolling Panel with the section headers laid out
'     in the designer); the vbAccelerator ListView -> ListView; the multi-select combo -> mnuKeyWords.
'   - the disconnected ADODB Recordset -> a sorted List(Of PhotoRecord) holding each Photo, so a field
'     is written straight to its .Exif file instead of re-reading the class folder every time.
' Fixed from VB6: the batch save left the form disabled (it ended with Me.Enabled = False, and exited
' early without re-enabling); the people/place lists had duplicates (IndexForKey <= 0 check).
' Callers use "Using f As New frmPhotoInfoBatch_2".
Friend Class frmPhotoInfoBatch_2

    Private Class PhotoRecord
        Public AlbumKey As String
        Public Photo As Photo
        Public Character As String
        Public ExifDate As String
        Public ExifTime As String
        Public ReadOnly Property FileDesc As String
            Get
                Return Photo.FileDesc
            End Get
        End Property
    End Class

    Private m_strAlbum() As String = {}
    Private ReadOnly m_records As New List(Of PhotoRecord)
    Private ReadOnly m_byFile As New Dictionary(Of String, PhotoRecord)(StringComparer.OrdinalIgnoreCase)
    Private m_current As PhotoRecord          ' the photo shown in "目前選擇的照片"
    Private m_bolShowing As Boolean            ' filling the fields -- don't write them back
    Private m_objKeyWords As KeyWord

    Private ReadOnly m_people As New List(Of String)
    Private ReadOnly m_places As New List(Of String)
    Private m_menuTarget As TextBox

    Public Sub ShowPhotos(ByVal strAlbum() As String, ByVal intCount As Integer)
        m_strAlbum = New String(Math.Max(0, intCount) - 1) {}
        Array.Copy(strAlbum, m_strAlbum, m_strAlbum.Length)
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_objKeyWords = New KeyWord
        m_objKeyWords.Construct(g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord))
        AddDropDownList()
        CreateRecordSet()
        Enabled = False
        Timer1.Enabled = True
    End Sub

    '==================================================================================================
    ' Keyword lists
    '==================================================================================================
    Private Sub AddKeys(ByVal list As List(Of String), ByVal section As String)
        Dim strArray() As String = Nothing
        Dim intCount As Integer = m_objKeyWords.Keys(section, strArray)
        For I As Integer = 0 To intCount - 1
            If Not list.Contains(strArray(I)) Then list.Add(strArray(I))
        Next
    End Sub

    Private Sub AddDropDownList()
        m_people.Clear()
        m_places.Clear()
        For Each s As String In {"家人", "寵物", "朋友/同學", "稱謂/關係"}
            AddKeys(m_people, s)
        Next
        For Each s As String In {"學校/公園", "旅遊/景點", "地名/地區"}
            AddKeys(m_places, s)
        Next
    End Sub

    Private Sub ShowKeyMenu(ByVal button As Control, ByVal target As TextBox, ByVal keys As List(Of String))
        If keys.Count = 0 Then Return
        m_menuTarget = target
        Dim current As New HashSet(Of String)(target.Text.Split(","c).Select(Function(s) s.Trim()))
        mnuKeyWords.Items.Clear()
        For Each k As String In keys
            mnuKeyWords.Items.Add(New ToolStripMenuItem(k) With {.Checked = current.Contains(k)})
        Next
        mnuKeyWords.Show(button, New Point(0, button.Height))
    End Sub

    Private Sub mnuKeyWords_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles mnuKeyWords.ItemClicked
        Dim item As ToolStripMenuItem = CType(e.ClickedItem, ToolStripMenuItem)
        item.Checked = Not item.Checked
        m_menuTarget.Text = String.Join(",", mnuKeyWords.Items.OfType(Of ToolStripMenuItem)().Where(Function(i) i.Checked).Select(Function(i) i.Text))
        ' the single-photo fields are saved when edited (VB6 Validate)
        If m_menuTarget Is txtCharacter OrElse m_menuTarget Is txtSpot Then UpdateSelectedItemExif()
    End Sub

    Private Sub mnuKeyWords_Closing(sender As Object, e As ToolStripDropDownClosingEventArgs) Handles mnuKeyWords.Closing
        ' stay open while ticking entries, as the VB6 checklist did
        If e.CloseReason = ToolStripDropDownCloseReason.ItemClicked Then e.Cancel = True
    End Sub

    Private Sub cboCharacter_Click(sender As Object, e As EventArgs) Handles cboCharacter.Click
        ShowKeyMenu(cboCharacter, txtCharacter, m_people)
    End Sub

    Private Sub cboBatchCharacter_Click(sender As Object, e As EventArgs) Handles cboBatchCharacter.Click
        ShowKeyMenu(cboBatchCharacter, txtBatchCharacter, m_people)
    End Sub

    Private Sub cboSpot_Click(sender As Object, e As EventArgs) Handles cboSpot.Click
        ShowKeyMenu(cboSpot, txtSpot, m_places)
    End Sub

    Private Sub cboBatchSpot_Click(sender As Object, e As EventArgs) Handles cboBatchSpot.Click
        ShowKeyMenu(cboBatchSpot, txtBatchSpot, m_places)
    End Sub

    '==================================================================================================
    ' Records and thumbnails
    '==================================================================================================
    ''' <summary>VB6 Format(x, "0000/00/00"): digits through a picture, anything else as is.</summary>
    Private Shared Function FormatDigits(ByVal value As String, ByVal picture As String) As String
        Dim n As Long
        If Long.TryParse(If(value, "").Trim(), n) Then Return n.ToString(picture)
        Return If(value, "")
    End Function

    Private Sub CreateRecordSet()
        m_records.Clear()
        m_byFile.Clear()
        For Each strKey As String In m_strAlbum
            Dim objClass As New [Class]
            objClass.Construct(strKey)
            objClass.Load()
            For J As Integer = 0 To objClass.PhotoCount - 1
                Dim p As Photo = objClass.Photo(J)
                If p.MediaType <> enumPhotoMediaType.mdImage Then Continue For
                If m_byFile.ContainsKey(p.FileDesc) Then Continue For
                Dim r As New PhotoRecord With {
                    .AlbumKey = strKey,
                    .Photo = p,
                    .Character = p.Exif(enumPhotoExif.peCharacter),
                    .ExifDate = FormatDigits(p.Exif(enumPhotoExif.peDate), "0000/00/00"),
                    .ExifTime = FormatDigits(p.Exif(enumPhotoExif.peTime), "00:00:00")}
                m_records.Add(r)
                m_byFile.Add(p.FileDesc, r)
            Next
        Next
        ' VB6 Sort = "AlbumKey, ExifDate, ExifTime"
        Dim sorted = m_records.OrderBy(Function(r) r.AlbumKey, StringComparer.Ordinal).
                               ThenBy(Function(r) r.ExifDate, StringComparer.Ordinal).
                               ThenBy(Function(r) r.ExifTime, StringComparer.Ordinal).ToList()
        m_records.Clear()
        m_records.AddRange(sorted)
    End Sub

    ''' <summary>The picture fitted in the icon size and centred; portraits are fitted sideways and
    ''' turned (VB6 RotatePicture), so they fill the landscape icon.</summary>
    Private Function MakeIcon(ByVal file As String) As Bitmap
        Dim size As Size = ilsIcons.ImageSize
        Dim info As New Quartz.ImageInfo With {.FileName = file}
        Dim thumb As New Quartz.Thumbnail With {.FileName = file, .FixedSize = True}
        Dim portrait As Boolean = info.Height > info.Width
        Dim pic As Bitmap = If(portrait, thumb.GetThumbnail(size.Height, size.Width), thumb.GetThumbnail(size.Width, size.Height))
        If pic Is Nothing Then Return Nothing
        If portrait Then pic.RotateFlip(RotateFlipType.Rotate90FlipNone)
        Dim icon As New Bitmap(size.Width, size.Height)
        Using g As Graphics = Graphics.FromImage(icon)
            g.Clear(Color.White)
            g.DrawImage(pic, (size.Width - pic.Width) \ 2, (size.Height - pic.Height) \ 2, pic.Width, pic.Height)
        End Using
        pic.Dispose()
        Return icon
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            ilsIcons.Images.Clear()
            lvwMain.BeginUpdate()
            lvwMain.Items.Clear()
            For Each r As PhotoRecord In m_records
                Dim icon As Bitmap = MakeIcon(r.FileDesc)
                If icon Is Nothing Then Continue For
                ilsIcons.Images.Add(r.FileDesc, icon)
                Dim strText As String = r.Character.Trim()
                If strText = "" Then strText = r.ExifDate.Trim()
                lvwMain.Items.Add(r.FileDesc, strText, r.FileDesc)
            Next
            lvwMain.EndUpdate()
        Finally
            Cursor = Cursors.Default
        End Try
        Enabled = True
        Text = Text & "  總共有 " & lvwMain.Items.Count & " 張照片"
    End Sub

    '==================================================================================================
    ' The selected photo
    '==================================================================================================
    Private Sub lvwMain_ItemSelectionChanged(sender As Object, e As ListViewItemSelectionChangedEventArgs) Handles lvwMain.ItemSelectionChanged
        If Not e.IsSelected OrElse Not Enabled Then Return
        ShowItem(e.Item)
    End Sub

    Private Sub ShowItem(ByVal item As ListViewItem)
        m_bolShowing = True
        Try
            txtTitle.Text = ""
            txtKeyword.Text = ""
            txtDateTime.Text = ""
            txtRemark.Text = ""
            txtCharacter.Text = ""
            txtSpot.Text = ""
            m_current = Nothing

            Dim r As PhotoRecord = Nothing
            If Not m_byFile.TryGetValue(item.Name, r) Then Return
            m_current = r
            Dim p As Photo = r.Photo
            txtTitle.Text = p.Exif(enumPhotoExif.peTitle)
            txtKeyword.Text = p.Exif(enumPhotoExif.peKeyWord)
            Dim d As Date
            Dim strDate As String = FormatDigits(p.Exif(enumPhotoExif.peDate), "0000/00/00")
            If Date.TryParseExact(strDate, "yyyy/MM/dd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, d) Then
                txtDateTime.Text = strDate & "-" & FormatDigits(p.Exif(enumPhotoExif.peTime), "00:00:00")
            Else
                txtDateTime.Text = IO.File.GetLastWriteTime(p.FileDesc).ToString("yyyy/MM/dd-HH:mm:ss")
            End If
            txtRemark.Text = p.Exif(enumPhotoExif.peRemark)
            txtCharacter.Text = p.Exif(enumPhotoExif.peCharacter)
            txtSpot.Text = p.Exif(enumPhotoExif.peSpot)
            ShowPlaceOf(p)   ' frmPhotoInfoBatch_2.Place.vb: 縣市區 after the 地點 label

            If txtTitle.Text.Trim() <> "" Then txtBatchTitle.Text = txtTitle.Text
            If txtKeyword.Text.Trim() <> "" Then txtBatchKeyword.Text = txtKeyword.Text
            If txtRemark.Text.Trim() <> "" Then txtBatchRemark.Text = txtRemark.Text
            If txtCharacter.Text.Trim() <> "" Then txtBatchCharacter.Text = txtCharacter.Text
            If txtSpot.Text.Trim() <> "" Then txtBatchSpot.Text = txtSpot.Text
        Finally
            m_bolShowing = False
        End Try
    End Sub

    Private Sub Fields_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles txtTitle.Validating, txtCharacter.Validating, txtDateTime.Validating, txtKeyword.Validating, txtRemark.Validating, txtSpot.Validating
        UpdateSelectedItemExif()
    End Sub

    Private Sub UpdateSelectedItemExif()
        If m_bolShowing OrElse m_current Is Nothing Then Return
        Dim p As Photo = m_current.Photo
        p.Exif(enumPhotoExif.peTitle) = txtTitle.Text.TrimEnd()
        p.Exif(enumPhotoExif.peKeyWord) = txtKeyword.Text.TrimEnd()
        ' "yyyy/MM/dd-HH:mm:ss" -> 8 + 6 digits; a half-typed date is left as it was
        Dim digits As New String(txtDateTime.Text.Where(Function(c) Char.IsDigit(c)).ToArray())
        If digits.Length = 14 Then
            p.Exif(enumPhotoExif.peDate) = digits.Substring(0, 8)
            p.Exif(enumPhotoExif.peTime) = digits.Substring(8, 6)
        End If
        p.Exif(enumPhotoExif.peRemark) = txtRemark.Text.TrimEnd()
        p.Exif(enumPhotoExif.peCharacter) = txtCharacter.Text.TrimEnd()
        p.Exif(enumPhotoExif.peSpot) = txtSpot.Text.TrimEnd()
    End Sub

    '==================================================================================================
    ' Batch
    '==================================================================================================
    Private Sub butBatchSave_Click(sender As Object, e As EventArgs) Handles butBatchSave.Click
        If lvwMain.SelectedItems.Count = 0 Then Return
        Enabled = False
        Cursor = Cursors.WaitCursor
        Dim k As Integer = 0
        Try
            For Each item As ListViewItem In lvwMain.SelectedItems
                Dim r As PhotoRecord = Nothing
                If Not m_byFile.TryGetValue(item.Name, r) Then Continue For
                Dim p As Photo = r.Photo
                p.Exif(enumPhotoExif.peTitle) = txtBatchTitle.Text.TrimEnd()
                p.Exif(enumPhotoExif.peKeyWord) = txtBatchKeyword.Text.TrimEnd()
                p.Exif(enumPhotoExif.peRemark) = txtBatchRemark.Text.TrimEnd()
                p.Exif(enumPhotoExif.peCharacter) = txtBatchCharacter.Text.TrimEnd()
                p.Exif(enumPhotoExif.peSpot) = txtBatchSpot.Text.TrimEnd()
                k += 1
            Next
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
        ' the fields of the photo on show may have changed
        If lvwMain.FocusedItem IsNot Nothing AndAlso lvwMain.FocusedItem.Selected Then ShowItem(lvwMain.FocusedItem)
        frmMsgBox.ShowSmileMessage("共修改 " & k & " 筆資料完成", "成功")
    End Sub

    Private Sub butCheckedAll_Click(sender As Object, e As EventArgs) Handles butCheckedAll.Click
        lvwMain.BeginUpdate()
        For Each item As ListViewItem In lvwMain.Items
            item.Selected = True
        Next
        lvwMain.EndUpdate()
    End Sub

    Private Sub butUnCheckedAll_Click(sender As Object, e As EventArgs) Handles butUnCheckedAll.Click
        lvwMain.BeginUpdate()
        For Each item As ListViewItem In lvwMain.Items
            item.Selected = False
        Next
        lvwMain.EndUpdate()
    End Sub

    Private Sub butUnload_Click(sender As Object, e As EventArgs) Handles butUnload.Click
        UpdateSelectedItemExif()
        If chkBuildIndex.Checked AndAlso g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement Then
            Enabled = False
            Dim done As Boolean
            Try
                Cursor = Cursors.WaitCursor
                done = RebuildPhotoIndex()   ' Modules\PhotoIndexTools.vb
            Finally
                Cursor = Cursors.Default
                Enabled = True
            End Try
            If done Then frmMsgBox.ShowExclamationMessage("重建完成" & vbCrLf & "建議重新啟動以釋放資源", "")
        End If
        Close()
    End Sub

End Class
