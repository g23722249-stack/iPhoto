' Port of iPhoto\Form\frmDock.frm: the photos picked so far (g_lpDock), with load / save of a .dck list
' and "clear". Position (10, 10), the list's one-per-row layout: designer. Open it with
' "Using f As New frmDock"; the list is filled a moment after it shows (VB6 Timer1).
Friend Class frmDock

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ShowTitle()
        Timer1.Enabled = True
    End Sub

    Private Sub ShowTitle()
        Text = "Dock " & "共 " & g_lpDock.Count & " 張相片"
    End Sub

    '載入挑選的檔案
    Private Sub Image1_Click(sender As Object, e As EventArgs) Handles Image1.Click
        Dim strFileDesc As String = frmBrowserFile.GetFile("載入挑選的檔案", "Dock 檔 (*.dck)|*.dck", False).Trim()
        If strFileDesc = "" Then Return
        Enabled = False
        Application.UseWaitCursor = True
        Try
            g_lpDock.Restore(strFileDesc)
            Timer1_Tick(Timer1, EventArgs.Empty)
            ShowTitle()   ' VB6 left the old count in the title
        Finally
            Application.UseWaitCursor = False
            Enabled = True
        End Try
    End Sub

    '儲存挑選的檔案
    Private Sub Image2_Click(sender As Object, e As EventArgs) Handles Image2.Click
        Dim strFileDesc As String = frmBrowserFile.GetFile("載入挑選的檔案", "Dock 檔 (*.dck)|*.dck", False).Trim()
        If strFileDesc = "" Then Return
        If g_lpFileSystem.FileExists(strFileDesc) Then
            If Not frmQueryMsgBox.ShowMessage("是否覆蓋已經存在的檔案？", "注意") Then Return
        End If
        Enabled = False
        Application.UseWaitCursor = True
        Try
            g_lpDock.Save(strFileDesc)
        Finally
            Application.UseWaitCursor = False
            Enabled = True
        End Try
    End Sub

    Private Sub imgClear_Click(sender As Object, e As EventArgs) Handles imgClear.Click
        If g_lpDock.Count <= 0 Then Return
        If frmQueryMsgBox.ShowMessage("是否清空選取的相片？", "注意") Then
            g_lpDock.Clear()
            Close()
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False
        mlDock.Clear()
        For I As Integer = 0 To g_lpDock.Count - 1
            mlDock.AddItem(g_lpDock.FileName(I))
            Application.DoEvents()
        Next
    End Sub

End Class
