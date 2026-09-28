' 設定 › 面孔 (face recognition P4, new in the .NET port): the page is made in code so the designer file
' stays as ported; its check boxes and radio buttons borrow the pictures of the ported ones
' (chkPrivilege, rbStyle) so they look the same. Saved into iPhoto.Ini [Faces] with the rest (butSave).
'   啟用人物辨識             Config.FaceEnabled    (next start)
'   開啟時分析新照片         Config.FaceAutoScan   (next start; the status label can start a scan)
'   確認的名字寫進人物欄     Config.FaceWriteNames (now)
'   自動認人 寬鬆/平衡/嚴格  Config.FaceStrictness (now: the faces are sorted again)
'   清除面孔辨識資料…        at once, after a question: faces, people, templates, rejections and the
'                           picture cache; photos, people fields and name cards stay.
Partial Class frmSetup

    Private pageFaces As Aqua.TabPage
    Private chkFaceEnabled, chkFaceAutoScan, chkFaceWriteNames As Aqua.CheckBox
    Private rbFaceStrict(2) As Aqua.RadioButton
    Private lblFaceCounts As Label
    Private WithEvents lblFaceClear As Label
    Private WithEvents lblFaceGuide As Label

    Private Sub FacePage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        pageFaces = New Aqua.TabPage With {.Title = "面孔", .BackColor = Color.White, .Font = pageGeneral.Font, .ForeColor = pageGeneral.ForeColor}
        Dim big As Font = chkPrivilege.Font
        Dim small As New Font(big.FontFamily, 11.0F)
        Dim x As Integer = 40

        chkFaceEnabled = NewCheck("啟用人物辨識（下次開啟 iPhoto 生效）", x, 28)
        chkFaceAutoScan = NewCheck("開啟 iPhoto 時，在背景分析新增或修改過的照片", x, 66)
        chkFaceWriteNames = NewCheck("確認過的名字寫進照片的「人物」欄（搜尋、照片資訊都看得到）", x, 104)

        Dim lblStrict As New Label With {.AutoSize = True, .Font = big, .Text = "自動認人", .Location = New Point(x, 152), .BackColor = Color.Transparent}
        pageFaces.Controls.Add(lblStrict)
        Dim captions() As String = {"寬鬆", "平衡", "嚴格"}
        For i = 0 To 2
            Dim rb As New Aqua.RadioButton With {
                .GroupName = "FACE", .TextValue = captions(i), .TextGap = rbStyle(0).TextGap, .Font = big, .BackColor = Color.Transparent,
                .ImageChecked = rbStyle(0).ImageChecked, .ImageUnChecked = rbStyle(0).ImageUnChecked,
                .ImageCheckDisabled = rbStyle(0).ImageCheckDisabled, .ImageUnCheckDisabled = rbStyle(0).ImageUnCheckDisabled,
                .Location = New Point(x + 130 + i * 120, 150)}
            rbFaceStrict(i) = rb
            pageFaces.Controls.Add(rb)
        Next
        pageFaces.Controls.Add(New Label With {
            .AutoSize = False, .Size = New Size(700, 64), .Location = New Point(x, 190), .Font = small, .ForeColor = Color.DimGray,
            .BackColor = Color.Transparent,
            .Text = "程式自己認出多少臉：寬鬆約 6 成（其中約 7% 認錯）、平衡約 5 成（約 6%）、嚴格約 4 成（約 6%）。" & vbCrLf &
                    "程式認出的名字只顯示在面孔牆與全圖瀏覽，按 ✓ 確認後才會寫進照片的人物欄。"})

        lblFaceCounts = New Label With {.AutoSize = True, .Font = small, .Location = New Point(x, 280), .BackColor = Color.Transparent}
        pageFaces.Controls.Add(lblFaceCounts)
        lblFaceClear = New Label With {
            .AutoSize = True, .Font = New Font(big.FontFamily, 12.0F, FontStyle.Underline), .ForeColor = Color.FromArgb(178, 38, 30),
            .Cursor = Cursors.Hand, .Location = New Point(x, 320), .BackColor = Color.Transparent,
            .Text = "清除面孔辨識資料…"}
        pageFaces.Controls.Add(lblFaceClear)
        lblFaceGuide = New Label With {
            .AutoSize = True, .Font = New Font(big.FontFamily, 12.0F, FontStyle.Underline), .ForeColor = Color.FromArgb(42, 116, 208),
            .Cursor = Cursors.Hand, .Location = New Point(x + 330, 320), .BackColor = Color.Transparent,
            .Text = "面孔功能使用說明…"}
        pageFaces.Controls.Add(lblFaceGuide)

        tabSetup.TabPages.Add(pageFaces)
        MoveFacesToScreen()
    End Sub

    Private Function NewCheck(ByVal text As String, ByVal x As Integer, ByVal y As Integer) As Aqua.CheckBox
        Dim c As New Aqua.CheckBox With {
            .TextValue = text, .TextGap = chkPrivilege.TextGap, .Font = chkPrivilege.Font, .ForeColor = chkPrivilege.ForeColor,
            .BackColor = Color.Transparent, .Location = New Point(x, y),
            .ImageChecked = chkPrivilege.ImageChecked, .ImageUnChecked = chkPrivilege.ImageUnChecked,
            .ImageCheckDisabled = chkPrivilege.ImageCheckDisabled, .ImageUnCheckDisabled = chkPrivilege.ImageUnCheckDisabled}
        pageFaces.Controls.Add(c)
        Return c
    End Function

    Private Sub MoveFacesToScreen()
        chkFaceEnabled.Checked = g_lpConfig.FaceEnabled
        chkFaceAutoScan.Checked = g_lpConfig.FaceAutoScan
        chkFaceWriteNames.Checked = g_lpConfig.FaceWriteNames
        rbFaceStrict(g_lpConfig.FaceStrictness).Checked = True
        ShowFaceCounts()
        Dim ready As Boolean = g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.FaceTablesReady
        lblFaceClear.Enabled = ready AndAlso Not g_lpConfig.ReadOnly
        If Not ready Then lblFaceCounts.Text = "資料庫沒有面孔資料表，無法使用人物辨識"
    End Sub

    Private Sub ShowFaceCounts()
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.FaceTablesReady Then Return
        Dim c = g_lpDatabase.FaceCounts()
        lblFaceCounts.Text = $"已分析 {c.Photos:#,0} 張照片，找到 {c.Faces:#,0} 張臉，{c.Persons:#,0} 人"
    End Sub

    ''' <summary>Called by butSave_Click before g_lpConfig.Save.</summary>
    Private Sub SaveFacePage()
        If pageFaces Is Nothing Then Return
        g_lpConfig.FaceEnabled = chkFaceEnabled.Checked
        g_lpConfig.FaceAutoScan = chkFaceAutoScan.Checked
        g_lpConfig.FaceWriteNames = chkFaceWriteNames.Checked
        For i = 0 To 2
            If rbFaceStrict(i).Checked Then g_lpConfig.FaceStrictness = i
        Next
    End Sub

    ''' <summary>The guide at 「面孔設定」; owned by the main window, so it stays when 設定 is closed.</summary>
    Private Sub lblFaceGuide_Click(sender As Object, e As EventArgs) Handles lblFaceGuide.Click
        frmFaceGuide.ShowGuide(Me, "setup")
    End Sub

    Private Sub lblFaceClear_Click(sender As Object, e As EventArgs) Handles lblFaceClear.Click
        If Not lblFaceClear.Enabled Then Return
        If Not frmQueryMsgBox.ShowMessage("清除所有面孔辨識資料？" & vbCrLf &
                                          "找到的臉、人物、確認與「不是此人」的紀錄都會刪除，照片、人物欄和面孔名片不受影響。" & vbCrLf &
                                          "之後會重新分析全部照片。", "清除面孔辨識資料") Then Return
        Application.UseWaitCursor = True
        Try
            If g_lpFaces IsNot Nothing Then
                g_lpFaces.ClearAll()
                Dim roots As New List(Of String)
                For i = 0 To g_lpConfig.AlbumCount - 1
                    roots.Add(g_lpConfig.AlbumPath(i))
                Next
                If g_lpConfig.FaceAutoScan Then g_lpFaces.Start(roots)
            Else
                g_lpDatabase.ClearFaceTables()
            End If
        Finally
            Application.UseWaitCursor = False
        End Try
        ShowFaceCounts()
        frmMsgBox.ShowExclamationMessage("面孔辨識資料已清除", "")
    End Sub

End Class
