' Port of iPhoto\Form\frmShowPhoto.frm: one photo's thumbnail, ranking and information (from its .Exif
' file), opened from the search results.
Friend Class frmShowPhoto

    Private m_strFileDesc As String = ""

    Public Sub ShowPhoto(ByVal FileDesc As String)
        m_strFileDesc = FileDesc
        ShowDialog()
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    ' VB6 Form_Load (every show)
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then MovePhotoToScreen()
        MyBase.OnVisibleChanged(e)
    End Sub

    Private Sub MovePhotoToScreen()
        Dim fs As Carbon.FileSystem = g_lpFileSystem
        Dim strExifFile As String = fs.AnalyseFile(fsParentFolderName, m_strFileDesc) & "\" & fs.AnalyseFile(fsBaseName, m_strFileDesc) & "." & gc_strExifPattern

        lblFilePath.Text = fs.AnalyseFile(fsParentFolderName, m_strFileDesc)
        lblFileName.Text = fs.AnalyseFile(fsFileName, m_strFileDesc)
        If IO.File.Exists(m_strFileDesc) Then
            lblFileDateTime.Text = IO.File.GetLastWriteTime(m_strFileDesc).ToString()
            lblFileLength.Text = CLng(New IO.FileInfo(m_strFileDesc).Length / 1024) & " K"
        Else
            lblFileDateTime.Text = ""
            lblFileLength.Text = ""
        End If

        Dim ini As New Carbon.IniFile
        ini.FileName = strExifFile
        lblTitle.Text = ini.SimpleGetValue("Exif", "Title")
        lblCharacter.Text = ini.SimpleGetValue("Exif", "Character")
        lblSopt.Text = ini.SimpleGetValue("Exif", "Spot")
        lblDateTime.Text = FormatDigits(ini.SimpleGetValue("Exif", "Date"), "0000/00/00") & " " & FormatDigits(ini.SimpleGetValue("Exif", "Time"), "00:00:00")
        lblRemark.Text = ini.SimpleGetValue("Exif", "Remark")
        lblKeyWord.Text = ini.SimpleGetValue("Exif", "KeyWord")

        Dim intRanking As Integer = CInt(Val(ini.SimpleGetValue("Exif", "Ranking")))
        MediaItem1.FileName = m_strFileDesc
        MediaItem1.Ranking = If(intRanking <= 0 OrElse intRanking > 5, Aqua.MediaItemRanking.NoRating, CType(intRanking, Aqua.MediaItemRanking))
    End Sub

    Private Shared Function FormatDigits(ByVal szValue As String, ByVal szPicture As String) As String
        Dim n As Long
        If Long.TryParse(If(szValue, "").Trim(), n) Then Return n.ToString(szPicture)
        Return If(szValue, "")
    End Function

End Class
