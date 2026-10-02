' 設定 › 螢幕 (new in the .NET port; Config.ScreenSingle, iPhoto.Ini [Interface] ScreenMode): 雙螢幕 (the
' main window on one screen, 全圖瀏覽 on another; the default) or 單螢幕 (全圖 inside the main window,
' frmMain.SingleScreen.vb). With one screen only 單螢幕 can be chosen. It takes effect at the
' next start: 儲存 then asks 「稍後再重開」 / 「現在重新開啟 iPhoto」 (Program.RestartApplication).
' Replaces VB6's 「左右螢幕對調」 (two screens now swap by dragging a window across).
Partial Class frmSetup

    Private pageScreen As Aqua.TabPage
    Private rbScreen(1) As Aqua.RadioButton   ' 0 雙螢幕, 1 單螢幕
    Private m_bolScreenSingleWas As Boolean

    Private Sub ScreenPage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        pageScreen = New Aqua.TabPage With {.Title = "螢幕", .BackColor = Color.White, .Font = pageGeneral.Font, .ForeColor = pageGeneral.ForeColor}
        Dim big As Font = chkPrivilege.Font
        Dim small As New Font(big.FontFamily, 11.0F)
        Dim x As Integer = 40
        Dim two As Boolean = g_intMonitorCount > 1

        pageScreen.Controls.Add(New Label With {.AutoSize = True, .Font = big, .Text = "螢幕模式", .Location = New Point(x, 28), .BackColor = Color.Transparent})
        rbScreen(0) = NewScreenRadio("雙螢幕（預設）", x + 20, 66)
        pageScreen.Controls.Add(NewScreenNote("一個螢幕放主視窗（縮圖），另一個放全圖瀏覽；兩邊可以拖曳互換，位置會記住。", x + 52, 98, small))
        rbScreen(1) = NewScreenRadio("單螢幕", x + 20, 140)
        pageScreen.Controls.Add(NewScreenNote("全圖在主視窗裡看：「縮圖」「全圖」切換，全圖上方有縮圖列（可拖到上下左右）。", x + 52, 172, small))
        Dim note As Label = NewScreenNote(If(two, "偵測到 " & g_intMonitorCount & " 個螢幕。", "目前只偵測到一個螢幕，只能用單螢幕。"), x + 20, 222, small)
        note.ForeColor = If(two, Color.FromArgb(29, 79, 143), Color.FromArgb(138, 75, 0))
        pageScreen.Controls.Add(note)
        pageScreen.Controls.Add(NewScreenNote("改了螢幕模式，要重新開啟 iPhoto 才會生效。", x + 20, 252, small))

        m_bolScreenSingleWas = Not two OrElse g_lpConfig.ScreenSingle
        rbScreen(If(m_bolScreenSingleWas, 1, 0)).Checked = True
        rbScreen(0).Enabled = two AndAlso Not g_lpConfig.ReadOnly
        rbScreen(1).Enabled = Not g_lpConfig.ReadOnly
        ' after 外觀 (the screen is about how things look)
        tabSetup.TabPages.Add(pageScreen)
    End Sub

    Private Function NewScreenRadio(ByVal text As String, ByVal x As Integer, ByVal y As Integer) As Aqua.RadioButton
        Dim rb As New Aqua.RadioButton With {
            .GroupName = "SCREEN", .TextValue = text, .TextGap = rbStyle(0).TextGap, .Font = chkPrivilege.Font, .BackColor = Color.Transparent,
            .ImageChecked = rbStyle(0).ImageChecked, .ImageUnChecked = rbStyle(0).ImageUnChecked,
            .ImageCheckDisabled = rbStyle(0).ImageCheckDisabled, .ImageUnCheckDisabled = rbStyle(0).ImageUnCheckDisabled,
            .Location = New Point(x, y)}
        pageScreen.Controls.Add(rb)
        Return rb
    End Function

    Private Shared Function NewScreenNote(ByVal text As String, ByVal x As Integer, ByVal y As Integer, ByVal f As Font) As Label
        Return New Label With {.AutoSize = False, .Size = New Size(700, 24), .Location = New Point(x, y), .Font = f, .ForeColor = Color.DimGray,
                               .BackColor = Color.Transparent, .Text = text}
    End Function

    ''' <summary>Called by butSave_Click before g_lpConfig.Save; True when the mode was changed (the
    ''' restart question follows).</summary>
    Private Function SaveScreenPage() As Boolean
        If pageScreen Is Nothing OrElse g_intMonitorCount <= 1 Then Return False   ' one screen: nothing to choose
        Dim wantSingle As Boolean = rbScreen(1).Checked
        g_lpConfig.ScreenSingle = wantSingle
        Return wantSingle <> m_bolScreenSingleWas
    End Function

    ''' <summary>After 儲存 changed the mode: 「稍後再重開」 or 「現在重新開啟 iPhoto」.</summary>
    Private Sub AskRestartForScreen()
        Dim now As String = If(g_lpConfig.ScreenSingle, "單螢幕", "雙螢幕")
        Dim was As String = If(g_bolDualScreen, "雙螢幕", "單螢幕")
        If frmRestartAsk.Ask(Owner, "螢幕模式改為「" & now & "」。",
                             "重新開啟 iPhoto 後才會生效；這次先照原本的「" & was & "」繼續用。") Then
            Program.RestartApplication()
        End If
    End Sub

End Class

''' <summary>The question after 設定 › 螢幕 changed: two buttons, 「稍後再重開」 and 「現在重新開啟 iPhoto」.</summary>
Friend Class frmRestartAsk
    Inherits Form

    Private m_bolNow As Boolean

    Public Shared Function Ask(ByVal owner As IWin32Window, ByVal line1 As String, ByVal line2 As String) As Boolean
        Using f As New frmRestartAsk(line1, line2)
            f.ShowDialog(owner)
            Return f.m_bolNow
        End Using
    End Function

    Private Sub New(ByVal line1 As String, ByVal line2 As String)
        Text = "螢幕模式"
        Font = New Font("華康細圓體", 12.0F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(500, 170)
        BackColor = Color.White
        Dim l1 As New Label With {.Text = line1, .AutoSize = False, .Bounds = New Rectangle(24, 20, 452, 28), .Font = New Font(Font.FontFamily, 13.0F)}
        Dim l2 As New Label With {.Text = line2, .AutoSize = False, .Bounds = New Rectangle(24, 54, 452, 48), .ForeColor = Color.FromArgb(62, 76, 89)}
        Dim later As New Aqua.ThinButton With {.Text = "稍後再重開", .Bounds = New Rectangle(105, 116, 150, 27), .Font = New Font("華康細圓體", 13.0F), .ForeColor = Color.Black}
        Dim now As New Aqua.ThinButton With {.Text = "現在重新開啟 iPhoto", .Bounds = New Rectangle(265, 116, 215, 27), .Font = New Font("華康細圓體", 13.0F), .ForeColor = Color.Black}
        AddHandler later.Click, Sub() Close()
        AddHandler now.Click, Sub()
                                  m_bolNow = True
                                  Close()
                              End Sub
        Controls.AddRange({l1, l2, later, now})
    End Sub

End Class
