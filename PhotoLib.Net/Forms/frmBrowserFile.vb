' Port of Lib\Form\frmBrowserFile.frm: the Aqua file picker. Layout lives in the designer.
' The filter uses the common-dialog format "描述|*.Ext1;*.Ext2|描述|*.*"; the file list shows the
' extensions of the type chosen in cboPattern.
' Fixed from VB6: the answer is reset on every call and only set by a successful OK, so Cancel returns ""
' (VB6 kept the previous answer, and a failed "file must exist" check still returned the file).
Public Class frmBrowserFile

    Private Structure FilePattern
        Public Description As String
        Public Extension() As String
    End Structure

    Private m_strFileDesc As String = ""
    Private m_bolVerify As Boolean
    Private m_strPattern As String = ""
    Private ReadOnly m_lpPattern As New List(Of FilePattern)

    ''' <summary>The chosen file, or "" when cancelled. Verify = the file must exist.</summary>
    Public Function GetFile(ByVal Title As String, ByVal Pattern As String, Optional ByVal Verify As Boolean = False) As String
        m_strFileDesc = ""
        m_bolVerify = Verify
        m_strPattern = If(Pattern, "")
        Text = Title
        ShowDialog()
        Return m_strFileDesc
    End Function

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        txtFileName.Text = txtFileName.Text.Trim()
        If DeskTop1.Path.Trim() = "" Then Return
        If txtFileName.Text = "" Then Return

        ' no extension typed: use the first one of the chosen type
        Dim strFileName As String = txtFileName.Text
        If g_lpFileSystem.AnalyseFile(fsExtensionName, strFileName).Trim() = "" AndAlso cboPattern.SelectedIndex >= 0 Then
            Dim ext As String = m_lpPattern(cboPattern.SelectedIndex).Extension(0)
            If ext <> "*" Then strFileName = g_lpFileSystem.AnalyseFile(fsBaseName, strFileName) & "." & ext
        End If

        Dim strFileDesc As String = DeskTop1.Path.TrimEnd("\"c) & "\" & strFileName
        If m_bolVerify AndAlso Not g_lpFileSystem.FileExists(strFileDesc) Then
            frmMsgBox.ShowCriticalMessage("檔案不存在", "")
            Return
        End If

        m_strFileDesc = strFileDesc
        Close()
    End Sub

    Private Sub cboPattern_SelectedChanged(sender As Object, e As EventArgs) Handles cboPattern.SelectedChanged
        SetPattern()
    End Sub

    Private Sub FileListBox1_SelectedChanged(sender As Object, e As EventArgs) Handles FileListBox1.SelectedChanged
        If FileListBox1.File <> "" Then txtFileName.Text = FileListBox1.File
    End Sub

    Private Sub DeskTop1_SelectedChanged(sender As Object, e As EventArgs) Handles DeskTop1.SelectedChanged
        If DeskTop1.Path.Trim() <> "" Then FileListBox1.Path = DeskTop1.Path
    End Sub

    ' VB6 Form_Load (every show)
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then
            If m_strPattern.Trim() = "" OrElse m_strPattern.Trim() = "*.*" Then m_strPattern = "所有檔案(*.*)|*.*"

            ' "描述|樣式|描述|樣式..." -> pairs
            m_lpPattern.Clear()
            Dim parts() As String = m_strPattern.Split("|"c)
            For i As Integer = 0 To parts.Length - 2 Step 2
                m_lpPattern.Add(ToPattern(parts(i), parts(i + 1)))
            Next

            cboPattern.Clear()
            For Each p As FilePattern In m_lpPattern
                cboPattern.AddItem(p.Description, p.Description)
            Next
            If cboPattern.Count > 0 Then cboPattern.SelectedIndex = 0

            SetPattern()
            txtFileName.Text = ""
            DeskTop1_SelectedChanged(DeskTop1, EventArgs.Empty)
        End If
        MyBase.OnVisibleChanged(e)
    End Sub

    ''' <summary>"*.Jpg;*.Bmp" -> {"Jpg", "Bmp"}; "*.*" -> {"*"}.</summary>
    Private Shared Function ToPattern(ByVal szDescription As String, ByVal szPatterns As String) As FilePattern
        Dim p As New FilePattern With {.Description = szDescription.Trim()}
        p.Extension = szPatterns.Trim().Split(";"c).
            Select(Function(s) s.Replace(".", "").Replace("*", "").Trim()).
            Select(Function(s) If(s = "", "*", s)).ToArray()
        Return p
    End Function

    Private Sub SetPattern()
        If cboPattern.Count <= 0 OrElse cboPattern.SelectedIndex < 0 Then
            FileListBox1.Pattern = "*"
        Else
            FileListBox1.Pattern = String.Join(";", m_lpPattern(cboPattern.SelectedIndex).Extension)
        End If
    End Sub

    Private Sub txtFileName_Validation(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles txtFileName.Validation
        txtFileName.Text = txtFileName.Text.Trim()
    End Sub

End Class
