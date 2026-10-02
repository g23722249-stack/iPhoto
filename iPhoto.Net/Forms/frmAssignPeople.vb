' 指定人物 (new in the .NET port): the main list's right-click on one or several photos. Shows every
' face found in them (FaceLibrary.FacesOf: analysed on the spot when not done yet) with a tick box;
' type or pick a name and the ticked faces get it (FaceLibrary.NameFaces: confirmed, the name into the
' people field as 設定 › 面孔 says). A photo with no face found can get the name straight into its people
' field (.Exif and PhotoIndex). A face already named shows its name; ticking it renames it.
' Ticked at first: the one face of a photo that has only one and that nobody has confirmed yet.
' Without face recognition (設定 › 面孔 off) the name only goes into the people fields.
' Made in code (no designer file).
Friend Class frmAssignPeople
    Inherits Form

    Private ReadOnly m_lpFiles As List(Of String)
    Private ReadOnly m_lpFaces As New List(Of FaceRegion)          ' by face card
    Private ReadOnly m_lpChecks As New List(Of CheckBox)           ' by face card
    Private ReadOnly m_lpNoFace As New List(Of String)
    Private m_bolLoaded As Boolean
    Private m_bolChanged As Boolean
    Private m_bolClosing As Boolean   ' closed while the faces were still being found

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        m_bolClosing = True
        MyBase.OnFormClosing(e)
    End Sub

    Private ReadOnly lblHead As New Label
    Private ReadOnly cboName As New ComboBox
    Private ReadOnly pnlFaces As New FlowLayoutPanel
    Private ReadOnly chkNoFace As New CheckBox
    Private ReadOnly lblHint As New Label
    Private ReadOnly butOk As New Button, butCancel As New Button

    Private Shared ReadOnly Grey As Color = Color.FromArgb(82, 96, 109)
    Private Shared ReadOnly Blue As Color = Color.FromArgb(29, 95, 180)

    ''' <summary>Opens the dialog for <paramref name="files"/>; True when anything was named.</summary>
    Public Shared Function Assign(ByVal owner As IWin32Window, ByVal files As List(Of String)) As Boolean
        If files Is Nothing OrElse files.Count = 0 Then Return False
        Using f As New frmAssignPeople(files)
            f.ShowDialog(owner)
            Return f.m_bolChanged
        End Using
    End Function

    Private Sub New(ByVal files As List(Of String))
        m_lpFiles = files
        Text = "指定人物"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(820, 600)
        BackColor = Color.White

        lblHead.SetBounds(16, 12, 788, 24)
        lblHead.Font = New Font(Font, FontStyle.Bold)
        lblHead.Text = If(files.Count = 1, IO.Path.GetFileName(files(0)), files.Count & " 張照片")
        Dim lblName As New Label With {.Text = "這是誰？", .AutoSize = True, .Location = New Point(16, 48)}
        cboName.SetBounds(100, 44, 300, 30)
        cboName.DropDownStyle = ComboBoxStyle.DropDown
        cboName.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        cboName.AutoCompleteSource = AutoCompleteSource.ListItems
        Dim names = If(g_lpFaces?.Catalog?.VisiblePersons.OrderByDescending(Function(p) p.PhotoCount).Select(Function(p) p.Name), Enumerable.Empty(Of String)())
        cboName.Items.AddRange(names.Distinct().Cast(Of Object)().ToArray())
        lblHint.SetBounds(412, 48, 392, 24)
        lblHint.ForeColor = Grey
        lblHint.Font = New Font(Font.FontFamily, 9.0F)
        lblHint.Text = "勾選要指定成這個人的臉（按兩下臉也可以勾）"

        pnlFaces.SetBounds(16, 84, 788, 424)
        pnlFaces.AutoScroll = True
        pnlFaces.BorderStyle = BorderStyle.FixedSingle
        pnlFaces.BackColor = Color.FromArgb(247, 249, 251)
        pnlFaces.Padding = New Padding(6)

        chkNoFace.SetBounds(16, 516, 788, 26)
        chkNoFace.Checked = True
        chkNoFace.Visible = False

        butCancel.SetBounds(612, 552, 90, 34)
        butCancel.Text = "取消"
        butCancel.DialogResult = DialogResult.Cancel
        butOk.SetBounds(710, 552, 94, 34)
        butOk.Text = "指定"
        butOk.FlatStyle = FlatStyle.Flat
        butOk.BackColor = Blue
        butOk.ForeColor = Color.White
        butOk.FlatAppearance.BorderColor = Blue
        butOk.Enabled = False
        AddHandler butOk.Click, Sub() OkClicked()
        AddHandler cboName.TextChanged, Sub() UpdateOk()
        Controls.AddRange({lblHead, lblName, cboName, lblHint, pnlFaces, chkNoFace, butCancel, butOk})
        AcceptButton = butOk
        CancelButton = butCancel
    End Sub

    Private ReadOnly Property FacesOn As Boolean
        Get
            Return g_lpFaces IsNot Nothing AndAlso g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.FaceTablesReady
        End Get
    End Property

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        cboName.Focus()
        If Not FacesOn Then
            m_lpNoFace.AddRange(m_lpFiles)
            lblHint.Text = "面孔辨識沒有開啟：名字只寫進人物欄"
            FinishLoading()
            Return
        End If
        ' the faces, photo by photo (a photo not analysed yet takes a moment); the window stays usable
        Dim cache As String = g_lpFaces.CacheFolder
        UseWaitCursor = True
        Try
            For i = 0 To m_lpFiles.Count - 1
                If IsDisposed OrElse m_bolClosing Then Return
                lblHint.Text = $"找臉中… {i + 1} / {m_lpFiles.Count}"
                Dim file As String = m_lpFiles(i)
                Dim faces As List(Of FaceRegion) = g_lpFaces.FacesOf(file).
                    Where(Function(f) f.State <> FaceRegion.enumFaceState.fsNotFace).ToList()
                If faces.Count = 0 Then
                    m_lpNoFace.Add(file)
                Else
                    Dim only As Boolean = faces.Count = 1
                    For Each f In faces
                        AddCard(cache, f, only AndAlso f.State <> FaceRegion.enumFaceState.fsConfirmed)
                    Next
                End If
                Application.DoEvents()
            Next
        Finally
            If Not IsDisposed Then UseWaitCursor = False
        End Try
        If Not IsDisposed Then FinishLoading()
    End Sub

    Private Sub AddCard(ByVal cache As String, ByVal f As FaceRegion, ByVal ticked As Boolean)
        Dim card As New Panel With {.Size = New Size(120, 150), .Margin = New Padding(6), .BackColor = Color.White}
        Dim pic As New PictureBox With {.Bounds = New Rectangle(10, 6, 100, 100), .SizeMode = PictureBoxSizeMode.Zoom, .BackColor = Color.FromArgb(214, 221, 228), .Cursor = Cursors.Hand}
        Dim crop As String = FaceCards.FaceCrop(cache, f)
        If crop <> "" Then
            Try
                Using s As New IO.FileStream(crop, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                    Using img As Image = Image.FromStream(s)
                        pic.Image = New Bitmap(img)
                    End Using
                End Using
            Catch
            End Try
        End If
        Dim who As String = If(f.PersonID = 0, "未命名",
                               f.PersonName & If(f.State = FaceRegion.enumFaceState.fsConfirmed, "", "（程式認出）"))
        Dim chk As New CheckBox With {.Bounds = New Rectangle(6, 110, 112, 36), .Text = who, .Checked = ticked, .Font = New Font(Font.FontFamily, 9.0F),
                                      .ForeColor = If(f.PersonID = 0, Grey, Color.FromArgb(31, 41, 51))}
        AddHandler pic.DoubleClick, Sub() chk.Checked = Not chk.Checked
        AddHandler chk.CheckedChanged, Sub() UpdateOk()
        Dim tip As New ToolTip
        tip.SetToolTip(pic, IO.Path.GetFileName(f.FileName))
        card.Controls.AddRange({pic, chk})
        pnlFaces.Controls.Add(card)
        m_lpFaces.Add(f)
        m_lpChecks.Add(chk)
    End Sub

    Private Sub FinishLoading()
        m_bolLoaded = True
        If m_lpFaces.Count = 0 AndAlso FacesOn Then
            pnlFaces.Controls.Add(New Label With {.Text = "這些照片沒有找到臉。", .AutoSize = True, .ForeColor = Grey, .Margin = New Padding(12)})
        End If
        If Not FacesOn Then
            pnlFaces.Controls.Add(New Label With {.Text = "面孔辨識沒有開啟（設定 › 面孔）。", .AutoSize = True, .ForeColor = Grey, .Margin = New Padding(12)})
        End If
        If m_lpNoFace.Count > 0 Then
            chkNoFace.Visible = True
            chkNoFace.Text = If(FacesOn, m_lpNoFace.Count & " 張沒有找到臉的照片：名字直接寫進人物欄", "名字寫進這 " & m_lpNoFace.Count & " 張照片的人物欄")
            AddHandler chkNoFace.CheckedChanged, Sub() UpdateOk()
        End If
        If FacesOn Then lblHint.Text = "勾選要指定成這個人的臉（按兩下臉也可以勾）；" & m_lpFaces.Count & " 張臉"
        UpdateOk()
    End Sub

    Private ReadOnly Property Ticked As List(Of FaceRegion)
        Get
            Dim r As New List(Of FaceRegion)
            For i = 0 To m_lpChecks.Count - 1
                If m_lpChecks(i).Checked Then r.Add(m_lpFaces(i))
            Next
            Return r
        End Get
    End Property

    Private Sub UpdateOk()
        Dim ro As Boolean = g_lpConfig IsNot Nothing AndAlso g_lpConfig.ReadOnly
        Dim n As Integer = Ticked.Count
        Dim plain As Integer = If(chkNoFace.Visible AndAlso chkNoFace.Checked, m_lpNoFace.Count, 0)
        butOk.Enabled = m_bolLoaded AndAlso Not ro AndAlso cboName.Text.Trim() <> "" AndAlso (n > 0 OrElse plain > 0)
        butOk.BackColor = If(butOk.Enabled, Blue, Color.FromArgb(154, 165, 177))
        butOk.Text = If(n + plain > 0, "指定（" & If(n > 0, n & " 張臉", "") & If(n > 0 AndAlso plain > 0, "＋", "") & If(plain > 0, plain & " 張照片", "") & "）", "指定")
        butOk.Width = Math.Max(94, TextRenderer.MeasureText(butOk.Text, butOk.Font).Width + 28)
        butOk.Left = 804 - butOk.Width
        butCancel.Left = butOk.Left - 98
    End Sub

    Private Sub OkClicked()
        Dim name As String = cboName.Text.Trim()
        If name = "" Then Return
        If Not CheckNameRule(name) OrElse FaceNames.Split(name).Length <> 1 Then
            frmMsgBox.ShowCriticalMessage("名字含有不合法的字元（也不能有 , / 、 這些分隔符號）", Text)
            Return
        End If
        Dim faces As List(Of FaceRegion) = Ticked
        Dim plain As List(Of String) = If(chkNoFace.Visible AndAlso chkNoFace.Checked, m_lpNoFace, New List(Of String))
        UseWaitCursor = True
        Enabled = False
        Try
            If faces.Count > 0 Then
                g_lpFaces.NameFaces(faces, name)
                ' 設定 › 面孔 「寫回人物欄」 off: FaceLibrary leaves the field alone -- asked for here, so write it
                If Not g_lpFaces.WriteNames Then plain = plain.Concat(faces.Select(Function(f) f.FileName)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            End If
            For Each file In plain
                Dim p As New Photo
                p.Construct(file)
                Dim before As String = p.Exif(enumPhotoExif.peCharacter)
                Dim after As String = FaceNames.Add(before, name)
                If after = before Then Continue For
                p.Exif(enumPhotoExif.peCharacter) = after
                If g_lpDatabase IsNot Nothing AndAlso g_lpDatabase.Implement Then g_lpDatabase.SetPhotoCharacter(p)
            Next
            g_lpFaces?.RequestOrganizeSoon()
            m_bolChanged = True
        Finally
            Enabled = True
            UseWaitCursor = False
        End Try
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            For Each c In pnlFaces.Controls.OfType(Of Panel)()
                For Each p In c.Controls.OfType(Of PictureBox)()
                    p.Image?.Dispose()
                Next
            Next
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
