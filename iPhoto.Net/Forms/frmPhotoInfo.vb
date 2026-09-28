' Port of iPhoto\Form\frmPhotoInfo.frm: one photo's file details and its .Exif fields (title, people,
' place, date, time, remark, keywords); OK writes them back and updates the search database. The "…"
' buttons pick keywords (shown only when the keyword file exists); everything is read-only in
' read-only mode. Callers use "Using f As New frmPhotoInfo".
Friend Class frmPhotoInfo

    Private m_lpPhoto As Photo
    Private ReadOnly m_toolTip As New ToolTip()

    Public Sub ShowPhotoInfo(ByVal lpPhoto As Photo)
        m_lpPhoto = lpPhoto
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblPath.Text = m_lpPhoto.FolderDesc
        m_toolTip.SetToolTip(lblPath, m_lpPhoto.FolderDesc)
        lblFileName.Text = m_lpPhoto.Name
        Dim fi As New IO.FileInfo(m_lpPhoto.FileDesc)
        If fi.Exists Then
            lblFileDateTime.Text = fi.LastWriteTime.ToString("yyyy/M/d tt hh:mm:ss")
            lblFileLength.Text = CLng(fi.Length / 1024) & " K"
        End If

        txtTitle.Text = m_lpPhoto.Exif(enumPhotoExif.peTitle)
        txtCharacter.Text = m_lpPhoto.Exif(enumPhotoExif.peCharacter)
        txtSpot.Text = m_lpPhoto.Exif(enumPhotoExif.peSpot)
        meDate.Value = m_lpPhoto.Exif(enumPhotoExif.peDate)
        meTime.Value = m_lpPhoto.Exif(enumPhotoExif.peTime)
        txtRemark.Text = m_lpPhoto.Exif(enumPhotoExif.peRemark)
        txtKeyWord.Text = m_lpPhoto.Exif(enumPhotoExif.peKeyWord)

        If g_lpConfig.ReadOnly Then
            For Each b As Aqua.PngButton In imgKeyWords
                b.Visible = False
            Next
            For Each c As Control In New Control() {txtTitle, txtCharacter, txtSpot, meDate, meTime, txtRemark, txtKeyWord, butOk}
                c.Enabled = False
            Next
        Else
            Dim hasKeyWords As Boolean = g_lpFileSystem.FileExists(g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord))
            For Each b As Aqua.PngButton In imgKeyWords
                b.Visible = hasKeyWords
            Next
        End If
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_toolTip.Dispose()
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        m_lpPhoto.Exif(enumPhotoExif.peTitle) = txtTitle.Text
        m_lpPhoto.Exif(enumPhotoExif.peCharacter) = txtCharacter.Text
        m_lpPhoto.Exif(enumPhotoExif.peSpot) = txtSpot.Text
        m_lpPhoto.Exif(enumPhotoExif.peDate) = Convert.ToString(meDate.Value).Trim()
        m_lpPhoto.Exif(enumPhotoExif.peTime) = Convert.ToString(meTime.Value).Trim()
        m_lpPhoto.Exif(enumPhotoExif.peRemark) = txtRemark.Text
        m_lpPhoto.Exif(enumPhotoExif.peKeyWord) = txtKeyWord.Text

        If g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement Then g_lpDatabase.AddItem(m_lpPhoto)
        Close()
    End Sub

    Private Sub imgKeyWords_Click(sender As Object, e As EventArgs) Handles imgKeyWords_0.Click, imgKeyWords_1.Click, imgKeyWords_2.Click, imgKeyWords_3.Click, imgKeyWords_4.Click
        Dim Index As Integer = Array.IndexOf(imgKeyWords, sender)
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        Dim strKeyWords As String
        Using f As New frmKeyWords
            strKeyWords = f.ShowKeyWords(g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord))
        End Using
        If strKeyWords.Trim() = "" Then Return

        Dim target As Aqua.TextBox = {txtTitle, txtCharacter, txtSpot, txtRemark, txtKeyWord}(Index)
        target.Text = strKeyWords
        target.Focus()
    End Sub

End Class
