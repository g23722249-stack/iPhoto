' 拍攝資訊 in the full-screen viewer (new in the .NET port; PhotoLib ShotInfo): a 資訊 button at the
' top-left of the picture shows / hides a panel with the camera, lens, exposure, when and where; it
' stays on from photo to photo. Where the photo has a place, 在地圖上看 opens it on Google Maps in the
' browser (nothing is sent anywhere until then). Made in code so the designer file stays as ported.
Partial Class frmViewerLarge

    Private WithEvents picInfoBar As Aqua.ToolBar
    Private pnlInfo As Panel
    Private lblInfo As Label
    Private WithEvents lnkMap As LinkLabel
    Private m_bolInfoMode As Boolean
    Private m_lpShot As ShotInfo.Shot

    Private Sub EnsureInfoBar()
        If picInfoBar IsNot Nothing Then Return
        picInfoBar = New Aqua.ToolBar With {
            .Size = New Size(90, 40), .Anchor = AnchorStyles.Top Or AnchorStyles.Left, .Location = New Point(12, 12),
            .BackColor = Color.FromArgb(40, 40, 40), .ForeColor = Color.White, .HoverColor = Color.FromArgb(80, 80, 80),
            .Font = New Font(Me.Font.FontFamily, 12.0F), .Visible = False}
        picInfoBar.Items.Add(New Aqua.ToolBar.ToolBarItem("資訊", Nothing))
        pnlInfo = New Panel With {.Location = New Point(12, 56), .BackColor = Color.FromArgb(30, 30, 30), .Visible = False, .Padding = New Padding(10)}
        lblInfo = New Label With {.AutoSize = True, .ForeColor = Color.Gainsboro, .BackColor = Color.Transparent, .Font = New Font(Me.Font.FontFamily, 11.0F), .Location = New Point(10, 10)}
        lnkMap = New LinkLabel With {.AutoSize = True, .Text = "在地圖上看（Google 地圖）", .LinkColor = Color.FromArgb(120, 180, 250), .ActiveLinkColor = Color.White,
                                     .BackColor = Color.Transparent, .Font = New Font(Me.Font.FontFamily, 11.0F)}
        pnlInfo.Controls.Add(lblInfo)
        pnlInfo.Controls.Add(lnkMap)
        picPhoto.Controls.Add(picInfoBar)
        picPhoto.Controls.Add(pnlInfo)
        picInfoBar.BringToFront()
        pnlInfo.BringToFront()
    End Sub

    ''' <summary>Called by ShowPicture.</summary>
    Private Sub ShowInfoForPicture()
        EnsureInfoBar()
        picInfoBar.Visible = True
        m_lpShot = ShotInfo.Read(m_strFileName)
        UpdateInfoPanel()
    End Sub

    ''' <summary>Called by Clear (and for videos).</summary>
    Private Sub HideInfo()
        m_lpShot = Nothing
        If picInfoBar Is Nothing Then Return
        picInfoBar.Visible = False
        pnlInfo.Visible = False
    End Sub

    Private Sub UpdateInfoPanel()
        If pnlInfo Is Nothing Then Return
        pnlInfo.Visible = m_bolInfoMode AndAlso picInfoBar.Visible
        If Not pnlInfo.Visible Then Return
        Dim lines As List(Of String) = If(m_lpShot?.Lines(), New List(Of String))
        lblInfo.Text = If(lines.Count = 0, "這張照片沒有拍攝資訊", String.Join(vbCrLf, lines))
        lnkMap.Visible = m_lpShot IsNot Nothing AndAlso m_lpShot.HasPlace
        lnkMap.Location = New Point(10, lblInfo.Bottom + 6)
        Dim w As Integer = Math.Max(lblInfo.PreferredWidth, If(lnkMap.Visible, lnkMap.PreferredWidth, 0)) + 20
        Dim h As Integer = If(lnkMap.Visible, lnkMap.Bottom, lblInfo.Bottom) + 10
        pnlInfo.Size = New Size(w, h)
        pnlInfo.BringToFront()
    End Sub

    Private Sub picInfoBar_Click(sender As Object, Index As Integer) Handles picInfoBar.Click
        m_bolInfoMode = Not m_bolInfoMode
        picInfoBar.Items(0).Text = If(m_bolInfoMode, "隱藏資訊", "資訊")
        picInfoBar.Width = If(m_bolInfoMode, 120, 90)
        picInfoBar.SetButtonProperty()
        UpdateInfoPanel()
    End Sub

    Private Sub lnkMap_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnkMap.LinkClicked
        If m_lpShot Is Nothing OrElse Not m_lpShot.HasPlace Then Return
        Try
            Process.Start(New ProcessStartInfo(m_lpShot.MapUrl()) With {.UseShellExecute = True})
        Catch ex As ComponentModel.Win32Exception
            frmMsgBox.ShowCriticalMessage("無法開啟瀏覽器" & vbCrLf & ex.Message, "")
        End Try
    End Sub

End Class
