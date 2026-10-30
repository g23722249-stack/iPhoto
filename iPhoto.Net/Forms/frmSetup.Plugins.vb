' 設定 › 外掛程式 is gone (2026-10), done in code so the converted designer file stays as ported. Its
' rows all named outside programs that are built in now or unused:
'   列印輸出 (txtApp(0)) -- the tool bar's 沖印 is gone; 列印 and 蒙太奇拼貼 (PhotoMontage) print;
'   影像編輯 (txtApp(1)) -- 相片後製 opens PhotoEdit (the program named here only when PhotoEdit is missing);
'   影片編輯 / 網頁建置 (txtApp(2) / (4)) -- never used; 電子郵件 (txtApp(3)) -- 郵件 became 蒙太奇拼貼;
'   燒錄軟體 (txtApp(5)) -- 燒錄 became 地點.
' The values stay in iPhoto.Ini; the text boxes still load and save them, so 儲存 keeps them as read.
Partial Class frmSetup

    Private Sub Plugins_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tabSetup.TabPages.Remove(pagePlugins)
    End Sub

End Class
