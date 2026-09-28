' Port of iPhoto\Form\frmDateTime.frm: shows a photo's file date (row 0) and .Exif date (row 1) and
' writes the date typed in row 2 to the file times (creation / last access / last write -- VB6 SetFileTime
' via OpenFile) and/or the .Exif date, for this photo or every photo of its folder.
' Callers use "Using f As New frmDateTime".
Friend Class frmDateTime

    Private m_objClass As PhotoSet
    Private m_objPhoto As Photo

    Public Sub ShowDateTime(ByVal objClass As PhotoSet, ByVal objPhoto As Photo)
        m_objClass = objClass
        m_objPhoto = objPhoto
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblFileName.Text = m_objPhoto.FileDesc
        LoadFileDateTime(m_objPhoto.FileDesc)
        LoadExifDateTime(m_objPhoto)
        LoadDefaultDateTime()
    End Sub

    Private Sub LoadFileDateTime(ByVal strFile As String)
        Dim d As Date = IO.File.GetCreationTime(strFile)
        txtYYYY(0).Text = d.ToString("yyyy")
        txtMM(0).Text = d.ToString("MM")
        txtDD(0).Text = d.ToString("dd")
        txtHH(0).Text = d.ToString("HH")
        txtNN(0).Text = d.ToString("mm")
        txtSS(0).Text = d.ToString("ss")
    End Sub

    Private Shared Function MidText(ByVal s As String, ByVal start As Integer, ByVal length As Integer) As String
        s = If(s, "")
        If start >= s.Length Then Return ""
        Return s.Substring(start, Math.Min(length, s.Length - start))
    End Function

    Private Sub LoadExifDateTime(ByVal objPhoto As Photo)
        Dim strDate As String = objPhoto.Exif(enumPhotoExif.peDate)
        Dim strTime As String = objPhoto.Exif(enumPhotoExif.peTime)
        txtYYYY(1).Text = MidText(strDate, 0, 4)
        txtMM(1).Text = MidText(strDate, 4, 2)
        txtDD(1).Text = MidText(strDate, 6, 2)
        txtHH(1).Text = MidText(strTime, 0, 2)
        txtNN(1).Text = MidText(strTime, 2, 2)
        txtSS(1).Text = MidText(strTime, 4, 2)
    End Sub

    Private Sub LoadDefaultDateTime()
        txtYYYY(2).Text = txtYYYY(1).Text
        txtMM(2).Text = txtMM(1).Text
        txtDD(2).Text = txtDD(1).Text
        txtHH(2).Text = txtHH(1).Text
        txtNN(2).Text = txtNN(1).Text
        txtSS(2).Text = txtSS(1).Text
    End Sub

    ''' <summary>Normalises row 2 and returns the date it holds; Nothing when it isn't a valid date/time.</summary>
    Private Function CheckDateTime() As Date?
        txtYYYY(2).Text = CInt(Val(txtYYYY(2).Text)).ToString("0000")
        txtMM(2).Text = CInt(Val(txtMM(2).Text)).ToString("00")
        txtDD(2).Text = CInt(Val(txtDD(2).Text)).ToString("00")
        txtHH(2).Text = CInt(Val(txtHH(2).Text)).ToString("00")
        txtNN(2).Text = CInt(Val(txtNN(2).Text)).ToString("00")
        txtSS(2).Text = CInt(Val(txtSS(2).Text)).ToString("00")
        Dim d As Date
        If Not Date.TryParseExact(txtYYYY(2).Text & txtMM(2).Text & txtDD(2).Text & txtHH(2).Text & txtNN(2).Text & txtSS(2).Text,
                                  "yyyyMMddHHmmss", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, d) Then
            frmMsgBox.ShowCriticalMessage("輸入日期格式錯誤", "")
            Return Nothing
        End If
        Return d
    End Function

    Private Sub SaveTo(ByVal lpPhoto As Photo, ByVal d As Date)
        If chkSaveMode(0).Checked Then ModifyFileDateTime(lpPhoto.FileDesc, d)   ' 修改檔案的建立日期
        If chkSaveMode(1).Checked Then                                          ' 修改 EXIF 資訊
            lpPhoto.Exif(enumPhotoExif.peDate) = d.ToString("yyyyMMdd")
            lpPhoto.Exif(enumPhotoExif.peTime) = d.ToString("HHmmss")
        End If
    End Sub

    Private Sub ModifyFileDateTime(ByVal strFile As String, ByVal d As Date)
        Try
            IO.File.SetCreationTime(strFile, d)
            IO.File.SetLastAccessTime(strFile, d)
            IO.File.SetLastWriteTime(strFile, d)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            ' VB6 ignored a file it couldn't open
        End Try
    End Sub

    Private Sub butSave_Click(sender As Object, e As EventArgs) Handles butSave.Click
        Dim d As Date? = CheckDateTime()
        If Not d.HasValue Then Return
        SaveTo(m_objPhoto, d.Value)
        Close()
    End Sub

    Private Sub cmdSaveToFolder_Click(sender As Object, e As EventArgs) Handles cmdSaveToFolder.Click
        Dim d As Date? = CheckDateTime()
        If Not d.HasValue Then Return
        Cursor = Cursors.WaitCursor
        Try
            For I As Integer = 0 To m_objClass.PhotoCount - 1
                SaveTo(m_objClass.Photo(I), d.Value)
            Next
        Finally
            Cursor = Cursors.Default
        End Try
        Close()
    End Sub

    Private Sub cmdCancel_Click(sender As Object, e As EventArgs) Handles cmdCancel.Click
        Close()
    End Sub

End Class
