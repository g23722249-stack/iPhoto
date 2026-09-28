' Port of iPhoto\Form\frmDefaultPhotoInfo.frm: the default title / people / place / remark / keywords
' written into every imported photo's .Exif file. The values live in the (default) instance, as the
' VB6 module-level variables did across Unload.
Friend Class frmDefaultPhotoInfo

    Private m_strTitle As String = ""
    Private m_strCharacter As String = ""
    Private m_strSpot As String = ""
    Private m_strRemark As String = ""
    Private m_strKeyWord As String = ""

    Public ReadOnly Property DefaultInformation(ByVal Mode As enumPhotoExif) As String
        Get
            Select Case Mode
                Case enumPhotoExif.peTitle : Return m_strTitle
                Case enumPhotoExif.peCharacter : Return m_strCharacter
                Case enumPhotoExif.peSpot : Return m_strSpot
                Case enumPhotoExif.peRemark : Return m_strRemark
                Case enumPhotoExif.peKeyWord : Return m_strKeyWord
                Case Else : Return ""
            End Select
        End Get
    End Property

    Public Sub Clear()
        m_strTitle = ""
        m_strCharacter = ""
        m_strSpot = ""
        m_strRemark = ""
        m_strKeyWord = ""
    End Sub

    Public Sub ShowDefaultPhotoInfo()
        ShowDialog()
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        m_strTitle = txtTitle.Text
        m_strCharacter = txtCharacter.Text
        m_strSpot = txtSpot.Text
        m_strRemark = txtRemark.Text
        m_strKeyWord = txtKeyWord.Text
        Close()
    End Sub

    ' VB6 Form_Load (every show)
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then
            txtTitle.Text = m_strTitle
            txtCharacter.Text = m_strCharacter
            txtSpot.Text = m_strSpot
            txtRemark.Text = m_strRemark
            txtKeyWord.Text = m_strKeyWord

            Dim hasKeyWords As Boolean = g_lpFileSystem.FileExists(g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord))
            For Each b As Aqua.PngButton In imgKeyWords
                b.Visible = hasKeyWords
            Next
        End If
        MyBase.OnVisibleChanged(e)
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
