' 設定 › 全圖瀏覽 (new in the .NET port): the page turn when 全圖瀏覽 moves to another photo
' (Config.ViewerPageTurn / ViewerPageTurnSpeed, iPhoto.Ini [Viewer]). Made in code like the 面孔 page,
' with the pictures of the ported radio buttons (rbStyle).
Partial Class frmSetup

    Private pageViewer As Aqua.TabPage
    Private rbTurn(3) As Aqua.RadioButton
    Private rbTurnSpeed(2) As Aqua.RadioButton

    Private Sub ViewerPage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        pageViewer = New Aqua.TabPage With {.Title = "全圖瀏覽", .BackColor = Color.White, .Font = pageGeneral.Font, .ForeColor = pageGeneral.ForeColor}
        Dim big As Font = chkPrivilege.Font
        Dim small As New Font(big.FontFamily, 11.0F)
        Dim x As Integer = 40

        pageViewer.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "換照片時的翻頁效果", .Location = New Point(x, 28), .BackColor = Color.Transparent})
        Dim turns() As String = {"兩種輪流（依秒數：雙數秒整頁翻、單數秒翻頁角）", "整頁翻", "翻頁角", "不要翻頁"}
        For i = 0 To 3
            rbTurn(i) = NewRadio("TURN", turns(i), x + 20, 64 + i * 36)
        Next
        pageViewer.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "翻頁速度", .Location = New Point(x, 222), .BackColor = Color.Transparent})
        Dim speeds() As String = {"快", "中", "慢"}
        For i = 0 To 2
            rbTurnSpeed(i) = NewRadio("TURNSPEED", speeds(i), x + 130 + i * 100, 220)
        Next
        pageViewer.Controls.Add(New Label With {
            .AutoSize = False, .Size = New Size(720, 92), .Location = New Point(x, 300), .Font = small, .ForeColor = Color.DimGray,
            .BackColor = Color.Transparent,
            .Text = "整頁翻：照片中間當書脊，右半頁翻過去（上一張往反方向翻）。" & vbCrLf &
                    "翻頁角：從右下角掀起，露出下一張（上一張是把前一張蓋回來）。" & vbCrLf &
                    "幻燈片播放也會用這裡選的翻頁（夾在原本的轉場效果中，約三分之一）。" & vbCrLf &
                    "影片不翻頁；Windows 關閉「顯示動畫」時也不翻頁。"})

        ' 地點 mode (frmMain.PlaceViewer.vb): the order of the thumbnail strip
        pageViewer.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "地點縮圖列的順序", .Location = New Point(x, 262), .BackColor = Color.Transparent})
        rbStripOrder(0) = NewRadio("STRIPORDER", "由舊到新", x + 220, 260)
        rbStripOrder(1) = NewRadio("STRIPORDER", "由新到舊", x + 360, 260)
        pageViewer.Controls.Add(New Label With {
            .AutoSize = False, .Size = New Size(300, 26), .Location = New Point(x + 500, 264), .Font = small, .ForeColor = Color.DimGray,
            .BackColor = Color.Transparent, .Text = "每一天一段；同一天依檔名"})

        tabSetup.TabPages.Add(pageViewer)
        rbTurn(CInt(g_lpConfig.ViewerPageTurn)).Checked = True
        rbTurnSpeed(g_lpConfig.ViewerPageTurnSpeed).Checked = True
        rbStripOrder(If(g_lpConfig.ViewerStripNewestFirst, 1, 0)).Checked = True
    End Sub

    Private rbStripOrder(1) As Aqua.RadioButton

    Private Function NewRadio(ByVal group As String, ByVal text As String, ByVal x As Integer, ByVal y As Integer) As Aqua.RadioButton
        Dim rb As New Aqua.RadioButton With {
            .GroupName = group, .TextValue = text, .TextGap = rbStyle(0).TextGap, .Font = chkPrivilege.Font, .BackColor = Color.Transparent,
            .ImageChecked = rbStyle(0).ImageChecked, .ImageUnChecked = rbStyle(0).ImageUnChecked,
            .ImageCheckDisabled = rbStyle(0).ImageCheckDisabled, .ImageUnCheckDisabled = rbStyle(0).ImageUnCheckDisabled,
            .Location = New Point(x, y)}
        pageViewer.Controls.Add(rb)
        Return rb
    End Function

    ''' <summary>Called by butSave_Click before g_lpConfig.Save.</summary>
    Private Sub SaveViewerPage()
        If pageViewer Is Nothing Then Return
        For i = 0 To 3
            If rbTurn(i).Checked Then g_lpConfig.ViewerPageTurn = CType(i, Config.enumPageTurn)
        Next
        For i = 0 To 2
            If rbTurnSpeed(i).Checked Then g_lpConfig.ViewerPageTurnSpeed = i
        Next
        g_lpConfig.ViewerStripNewestFirst = rbStripOrder(1).Checked
    End Sub

End Class
