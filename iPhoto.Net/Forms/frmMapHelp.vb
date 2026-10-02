Imports System.IO
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms

' 地圖說明與流程 (new in the .NET port): Map\help.html -- the two map modes, the flow, where the map comes
' from, the tile cache, why there are no street maps offline -- in WebView2, served by Modules\MapHost.vb
' like the maps (no internet needed). Opened from 設定 › 地點 and from the 線上 / 離線 badge on the map.
' Not modal; one at a time (asking again brings it to the front). Made in code (no designer file).
Friend Class frmMapHelp
    Inherits Form

    Private Shared s_help As frmMapHelp
    Private ReadOnly m_web As New WebView2 With {.Dock = DockStyle.Fill}

    Public Shared Sub ShowHelp(ByVal owner As Form)
        If s_help Is Nothing OrElse s_help.IsDisposed Then
            s_help = New frmMapHelp
            If owner IsNot Nothing Then
                Dim wa As Rectangle = Screen.FromControl(owner).WorkingArea
                s_help.Location = New Point(wa.Left + (wa.Width - s_help.Width) \ 2, wa.Top + (wa.Height - s_help.Height) \ 2)
                s_help.Show(owner)
            Else
                s_help.Show()
            End If
        Else
            If s_help.WindowState = FormWindowState.Minimized Then s_help.WindowState = FormWindowState.Normal
            s_help.Activate()
        End If
    End Sub

    Private Sub New()
        Text = "地圖說明與流程"
        Font = New Font("Microsoft JhengHei UI", 9.75F)
        BackColor = Color.FromArgb(244, 245, 247)
        ShowInTaskbar = False
        ShowIcon = False
        MinimizeBox = False
        StartPosition = FormStartPosition.Manual
        KeyPreview = True
        Size = New Size(980, 820)
        MinimumSize = New Size(640, 480)
        Controls.Add(m_web)
    End Sub

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Dim data As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto")
            Dim env As CoreWebView2Environment = Await CoreWebView2Environment.CreateAsync(Nothing, Path.Combine(data, "WebView2"))
            Await m_web.EnsureCoreWebView2Async(env)
            With m_web.CoreWebView2
                .Settings.AreDevToolsEnabled = False
                .Settings.IsStatusBarEnabled = False
                .Settings.AreDefaultContextMenusEnabled = False
                AddHandler .NewWindowRequested, Sub(s, a) a.Handled = True
            End With
            MapHost.ServeAndNavigate(m_web.CoreWebView2, "help.html")
        Catch ex As Exception
            Controls.Add(New Label With {.Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleCenter,
                                         .Text = "無法顯示說明：" & ex.Message})
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If e.KeyCode = Keys.Escape Then Close()
    End Sub

End Class
