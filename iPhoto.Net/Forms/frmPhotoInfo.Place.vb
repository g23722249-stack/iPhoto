' 照片資訊: the .Exif's Country / City / Town (PlaceNames.Resolve, 重新產生 .Exif) under 地點 -- two rows
' 「國家 / 鄉鎮」 and 「縣市區」, editable like the other fields and saved with 好 (SavePlaceFields, called
' by butOk_Click). Made in code so the designer file stays as ported: the rows below 地點 move down.
Partial Class frmPhotoInfo

    Private txtCountry, txtTown, txtCity As Aqua.TextBox

    Private Sub PlaceRows_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Const rowGap As Integer = 32
        Dim y As Integer = txtSpot.Bottom + 8
        ' everything from the time row down (labels, boxes, keyword buttons, OK / Cancel) moves down two rows
        For Each c As Control In Controls.Cast(Of Control)().Where(Function(x) x.Top >= y - 2).ToList()
            c.Top += rowGap * 2
        Next
        ClientSize = New Size(ClientSize.Width, ClientSize.Height + rowGap * 2)

        txtCountry = NewPlaceBox(txtSpot.Left, y, 90)
        AddPlaceLabel("國家：", y)
        Dim townLabel As New Label With {.AutoSize = False, .Size = New Size(TextRenderer.MeasureText("鄉鎮：", Label10.Font).Width + 2, 19), .Text = "鄉鎮：", .TextAlign = Label10.TextAlign,
                                         .Font = Label10.Font, .ForeColor = Label10.ForeColor, .BackColor = Label10.BackColor,
                                         .Location = New Point(txtCountry.Right + 6, y + 3)}
        Controls.Add(townLabel)
        txtTown = NewPlaceBox(townLabel.Right, y, txtSpot.Right - townLabel.Right)
        txtCity = NewPlaceBox(txtSpot.Left, y + rowGap, txtSpot.Width)
        AddPlaceLabel("縣市區：", y + rowGap)

        txtCountry.Text = m_lpPhoto.Exif(enumPhotoExif.peCountry)
        txtCity.Text = m_lpPhoto.Exif(enumPhotoExif.peCity)
        txtTown.Text = m_lpPhoto.Exif(enumPhotoExif.peTown)
        For Each t In {txtCountry, txtTown, txtCity}
            t.Enabled = Not g_lpConfig.ReadOnly
        Next
        m_toolTip.SetToolTip(txtCity, "縣市加鄉鎮市區，例如「臺中市西屯區」；照片有 GPS 時由「重新產生 .Exif」自動填入")

        ' 選擇… (frmPlacePicker): fills 地點 / 國家 / 縣市區 / 鄉鎮 and keeps the GPS for 好
        Dim butPick As New Button With {.Text = "選擇…", .Font = New Font("Microsoft JhengHei UI", 9.5F), .Height = txtSpot.Height, .Width = 70,
                                        .FlatStyle = FlatStyle.Flat, .ForeColor = Color.FromArgb(29, 95, 180), .BackColor = Color.White,
                                        .Enabled = Not g_lpConfig.ReadOnly}
        butPick.FlatAppearance.BorderColor = Color.FromArgb(29, 95, 180)
        txtSpot.Width -= butPick.Width + 6
        butPick.Location = New Point(txtSpot.Right + 6, txtSpot.Top)
        Controls.Add(butPick)
        butPick.BringToFront()
        m_toolTip.SetToolTip(butPick, "從行政區、景點或地圖選擇拍攝地點（也會寫入 GPS）")
        AddHandler butPick.Click, Sub() PickPlace()
    End Sub

    ''' <summary>The GPS of the place picked with 選擇… ("" when none was picked): written with 好.</summary>
    Private m_strPickedGps As String = ""

    Private Sub PickPlace()
        Dim c As PlacePick.Choice = frmPlacePicker.PickFor(Me, m_lpPhoto.FileDesc)
        If c Is Nothing Then Return
        txtSpot.Text = c.Spot
        txtCountry.Text = c.Country
        txtCity.Text = c.City
        txtTown.Text = c.Town
        m_strPickedGps = c.Gps
    End Sub

    Private Function NewPlaceBox(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer) As Aqua.TextBox
        Dim t As New Aqua.TextBox With {.Location = New Point(x, y), .Size = New Size(w, txtSpot.Height), .Font = txtSpot.Font,
                                        .BorderColor = txtSpot.BorderColor, .BorderFocusColor = txtSpot.BorderFocusColor, .BackColor = txtSpot.BackColor}
        Controls.Add(t)
        Return t
    End Function

    Private Sub AddPlaceLabel(ByVal text As String, ByVal y As Integer)
        ' right-aligned to the other labels' right edge (地點： is Label10)
        Dim l As New Label With {.AutoSize = False, .Text = text, .TextAlign = ContentAlignment.TopRight,
                                 .Font = Label10.Font, .ForeColor = Label10.ForeColor, .BackColor = Label10.BackColor,
                                 .Size = New Size(90, Label10.Height), .Location = New Point(Label10.Right - 90, y + 3)}
        Controls.Add(l)
    End Sub

    ''' <summary>Called by butOk_Click before the database row is written.</summary>
    Private Sub SavePlaceFields()
        If txtCity Is Nothing Then Return
        m_lpPhoto.Exif(enumPhotoExif.peCountry) = txtCountry.Text.Trim()
        m_lpPhoto.Exif(enumPhotoExif.peCity) = txtCity.Text.Trim()
        m_lpPhoto.Exif(enumPhotoExif.peTown) = txtTown.Text.Trim()
        If m_strPickedGps <> "" Then m_lpPhoto.Exif(enumPhotoExif.peGps) = m_strPickedGps
    End Sub

End Class
