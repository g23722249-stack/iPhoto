' 設定 › 外掛: the 燒錄軟體 row (VB6 txtApp(5) / imgApp(5) / Label13) is taken off the page -- the tool
' bar's 燒錄 button became 地點 (2026-10). Done in code so the converted designer file stays as ported.
Partial Class frmSetup

    Private Sub Plugins_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each c As Control In {Label13, imgApp_5, txtApp_5}
            c.Parent?.Controls.Remove(c)
            c.Visible = False
        Next
    End Sub

End Class
