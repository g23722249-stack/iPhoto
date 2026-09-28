' Port of Lib\Form\frmBrowserFolder.frm: the Aqua folder picker. Layout lives in the designer.
' Fixed from VB6: the answer is reset on every call and only set by a successful OK, so Cancel returns ""
' (VB6 kept the previous answer, and a declined "create it?" still returned the missing folder).
Public Class frmBrowserFolder

    Private m_strFolderDesc As String = ""

    ''' <summary>The chosen (existing or just created) folder, or "" when cancelled.</summary>
    Public Function GetFolder(ByVal Title As String) As String
        m_strFolderDesc = ""
        Text = Title
        ShowDialog()
        Return m_strFolderDesc
    End Function

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        Dim strFolder As String = GetClearFolderDesc(txtFolder.Text)
        If strFolder.Trim() = "" Then Return

        If Not g_lpFileSystem.FolderExists(strFolder) Then
            If Not frmQueryMsgBox.ShowMessage("資料夾不存在" & vbCrLf & "是否建立？", "注意") Then Return
            If Not g_lpFileSystem.CreateFolder(strFolder) Then
                frmMsgBox.ShowCriticalMessage("建立資料夾失敗", "")
                Return
            End If
        End If

        m_strFolderDesc = strFolder
        Close()
    End Sub

    Private Sub DeskTop1_SelectedChanged(sender As Object, e As EventArgs) Handles DeskTop1.SelectedChanged
        txtFolder.Text = DeskTop1.Path
    End Sub

    ' VB6 Form_Load (every show)
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Visible Then DeskTop1_SelectedChanged(DeskTop1, EventArgs.Empty)
        MyBase.OnVisibleChanged(e)
    End Sub

    Private Sub txtFolder_Validation(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles txtFolder.Validation
        txtFolder.Text = GetClearFolderDesc(txtFolder.Text)
    End Sub

End Class
