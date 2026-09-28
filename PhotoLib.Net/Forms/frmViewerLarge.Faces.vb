' Face mode of the full-screen viewer (new in the .NET port, face recognition P1).
'
' A small bar at the top-right of the picture: 「顯示面孔 / 隱藏面孔」 turns face mode on and off (it stays
' on from photo to photo), 「新增面孔」 turns the current selection box into a face. In face mode every
' face of the photo is boxed: named faces in white with the name under them, faces nobody has named in
' dashed yellow with 「這是誰？」.
'   - click a face (or its label): a name box opens under it, listing the known names as you type;
'     Enter saves (a blank name takes the name off), Esc cancels;
'   - right-click a face: 移除名字 / 這不是臉;  Delete: 這不是臉 for the selected face.
' Naming goes through g_lpFaces, which also writes the name into the photo's people field.
' The whole bar stays hidden when face recognition can't run (g_lpFaces Is Nothing) and for videos;
' the boxes are hidden while the picture has unsaved edits (they no longer fit the edited picture).
' The bar, name box and menu are made in code so the designer file stays as ported.
Partial Class frmViewerLarge

    Private Const FaceBarIndexToggle As Integer = 0
    Private Const FaceBarIndexAdd As Integer = 1
    Private Const FaceBarIndexGuide As Integer = 2   ' 說明: frmFaceGuide at 「在全圖瀏覽中命名」

    Private m_bolFaceMode As Boolean
    Private m_lpFaces As New List(Of FaceRegion)
    Private m_lpSelFace As FaceRegion
    Private m_lpPeople As List(Of String)

    Private WithEvents picFaceBar As Aqua.ToolBar
    Private lblFaceInfo As Label
    Private WithEvents cboFaceName As ComboBox
    Private WithEvents mnuFace As ContextMenuStrip
    Private WithEvents mnuFaceUnname As ToolStripMenuItem
    Private WithEvents mnuFaceNotFace As ToolStripMenuItem
    Private WithEvents mnuFaceReject As ToolStripMenuItem

    Private Shared ReadOnly NamedPen As Color = Color.White
    Private Shared ReadOnly UnnamedPen As Color = Color.FromArgb(242, 183, 5)
    Private Shared ReadOnly SelectedPen As Color = Color.FromArgb(63, 127, 208)

    '==================================================================================================
    ' Controls
    '==================================================================================================
    Private Sub FaceBar_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        EnsureFaceBar()
    End Sub

    ''' <summary>Makes the bar, name box and menu once. Also called from ShowPicture: the main window
    ''' hands the first photo over before the viewer is shown (loaded).</summary>
    Private Sub EnsureFaceBar()
        If picFaceBar IsNot Nothing Then Return
        picFaceBar = New Aqua.ToolBar With {
            .Size = New Size(330, 40), .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .BackColor = Color.FromArgb(40, 40, 40), .ForeColor = Color.White, .HoverColor = Color.FromArgb(80, 80, 80),
            .Font = New Font(Me.Font.FontFamily, 12.0F), .Visible = False}
        picFaceBar.Items.Add(New Aqua.ToolBar.ToolBarItem("顯示面孔", Nothing))
        picFaceBar.Items.Add(New Aqua.ToolBar.ToolBarItem("新增面孔", Nothing))
        picFaceBar.Items.Add(New Aqua.ToolBar.ToolBarItem("說明", Nothing))
        picFaceBar.Location = New Point(picPhoto.ClientSize.Width - picFaceBar.Width - 12, 12)

        lblFaceInfo = New Label With {
            .AutoSize = False, .Size = New Size(picFaceBar.Width, 22), .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .Location = New Point(picFaceBar.Left, picFaceBar.Bottom + 2), .TextAlign = ContentAlignment.MiddleRight,
            .ForeColor = Color.Silver, .BackColor = Color.Black, .Font = New Font(Me.Font.FontFamily, 10.0F), .Visible = False}

        cboFaceName = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDown, .Width = 180, .Visible = False,
            .AutoCompleteMode = AutoCompleteMode.SuggestAppend, .AutoCompleteSource = AutoCompleteSource.ListItems,
            .Font = New Font(Me.Font.FontFamily, 12.0F)}

        mnuFaceUnname = New ToolStripMenuItem("移除名字")
        mnuFaceNotFace = New ToolStripMenuItem("這不是臉")
        mnuFaceReject = New ToolStripMenuItem("不是此人")
        mnuFace = New ContextMenuStrip()
        mnuFace.Items.AddRange({mnuFaceReject, mnuFaceUnname, mnuFaceNotFace})

        picPhoto.Controls.Add(picFaceBar)
        picPhoto.Controls.Add(lblFaceInfo)
        picPhoto.Controls.Add(cboFaceName)
        picFaceBar.BringToFront()
        lblFaceInfo.BringToFront()
        cboFaceName.BringToFront()
        AddFaceBarHelp()   ' frmViewerLarge.Help.vb
    End Sub

    ''' <summary>Called by ShowPicture: shows the bar and, in face mode, the faces of the new photo.</summary>
    Private Sub ShowFacesForPicture()
        CloseNameBox()
        m_lpFaces = New List(Of FaceRegion)
        m_lpSelFace = Nothing
        If g_lpFaces IsNot Nothing Then EnsureFaceBar()
        Dim can As Boolean = g_lpFaces IsNot Nothing AndAlso picFaceBar IsNot Nothing AndAlso FaceLibrary.IsPicture(m_strFileName)
        If picFaceBar IsNot Nothing Then
            picFaceBar.Visible = can
            lblFaceInfo.Visible = can AndAlso m_bolFaceMode
        End If
        If Not can Then Return
        If m_bolFaceMode Then LoadFaces()
        UpdateFaceBar()
    End Sub

    ''' <summary>Called by ShowVideo / Clear.</summary>
    Private Sub HideFaceBar()
        CloseNameBox()
        m_lpFaces = New List(Of FaceRegion)
        m_lpSelFace = Nothing
        If picFaceBar IsNot Nothing Then
            picFaceBar.Visible = False
            lblFaceInfo.Visible = False
        End If
    End Sub

    Private Sub LoadFaces()
        Try
            UseWaitCursor = True
            m_lpFaces = g_lpFaces.FacesOf(m_strFileName)
        Finally
            UseWaitCursor = False
        End Try
        m_lpPeople = Nothing   ' names may have been added elsewhere
        imgPhoto.Invalidate()
    End Sub

    Private Sub UpdateFaceBar()
        If picFaceBar Is Nothing Then Return
        Dim caption As String = If(m_bolFaceMode, "隱藏面孔", "顯示面孔")
        If picFaceBar.Items(FaceBarIndexToggle).Text <> caption Then
            picFaceBar.Items(FaceBarIndexToggle).Text = caption
            picFaceBar.SetButtonProperty()
        End If
        lblFaceInfo.Visible = picFaceBar.Visible AndAlso m_bolFaceMode
        If Not m_bolFaceMode Then Return
        If Changed Then
            lblFaceInfo.Text = "照片已編輯：存檔後才會重新找臉"
        ElseIf m_lpFaces.Count = 0 Then
            lblFaceInfo.Text = "沒有找到臉（可框選後按「新增面孔」）"
        Else
            Dim toConfirm As Integer = m_lpFaces.Where(Function(f) f.NeedsConfirm).Count()
            Dim unnamed As Integer = m_lpFaces.Where(Function(f) f.PersonID = 0).Count()
            lblFaceInfo.Text = m_lpFaces.Count & " 張臉" & If(toConfirm > 0, " · " & toConfirm & " 張待確認", "") & If(unnamed > 0, " · " & unnamed & " 張未命名", "")
        End If
    End Sub

    Private Sub picFaceBar_Click(sender As Object, Index As Integer) Handles picFaceBar.Click
        Select Case Index
            Case FaceBarIndexToggle
                m_bolFaceMode = Not m_bolFaceMode
                CloseNameBox()
                m_lpSelFace = Nothing
                If m_bolFaceMode Then LoadFaces() Else imgPhoto.Invalidate()
                UpdateFaceBar()
            Case FaceBarIndexAdd
                AddFaceFromSelection()
            Case FaceBarIndexGuide
                ' owned by the viewer (it covers the main window); not modal, so naming goes on
                frmFaceGuide.ShowGuide(Me, "viewer", stayWithCaller:=True)
        End Select
    End Sub

    '==================================================================================================
    ' Drawing and hit-testing (imgPhoto shows the whole picture fitted, so fractions x its size)
    '==================================================================================================
    Private ReadOnly Property FacesShown As Boolean
        Get
            Return m_bolFaceMode AndAlso picFaceBar IsNot Nothing AndAlso picFaceBar.Visible AndAlso Not Changed AndAlso m_lpFaces.Count > 0
        End Get
    End Property

    Private Function FaceRect(ByVal f As FaceRegion) As Rectangle
        Dim w As Integer = imgPhoto.ClientSize.Width, h As Integer = imgPhoto.ClientSize.Height
        Return New Rectangle(CInt(f.Box.X * w), CInt(f.Box.Y * h), Math.Max(4, CInt(f.Box.Width * w)), Math.Max(4, CInt(f.Box.Height * h)))
    End Function

    ' Labels (P3): a certain name (confirmed / from the people field) in white; the program's own match
    ' as 「名字？」 and a suggestion as 「這是 名字 嗎？」, both with ✓ / ✕ buttons; nobody as 「這是誰？」.
    Private Const ConfirmButtonWidth As Integer = 24
    Private Shared ReadOnly MatchedPen As Color = Color.FromArgb(135, 190, 245)

    Private Function FaceLabel(ByVal f As FaceRegion) As String
        If f.IsCertain Then Return f.PersonName
        If f.PersonID <> 0 AndAlso f.State = FaceRegion.enumFaceState.fsAuto Then Return f.PersonName & "？"
        If f.PersonID <> 0 AndAlso f.State = FaceRegion.enumFaceState.fsSuggested Then Return "這是 " & f.PersonName & " 嗎？"
        Return "這是誰？"
    End Function

    Private Function FaceColor(ByVal f As FaceRegion) As Color
        If f.IsCertain Then Return NamedPen
        If f.PersonID <> 0 AndAlso f.State = FaceRegion.enumFaceState.fsAuto Then Return MatchedPen
        Return UnnamedPen
    End Function

    ''' <summary>The label under the face; with room for ✓ / ✕ at its right when the name needs confirming.</summary>
    Private Function FaceLabelRect(ByVal g As Graphics, ByVal f As FaceRegion, ByVal font As Font) As Rectangle
        Dim r As Rectangle = FaceRect(f)
        Dim sz As Size = TextRenderer.MeasureText(g, FaceLabel(f), font)
        Dim w As Integer = sz.Width + 12 + If(f.NeedsConfirm, ConfirmButtonWidth * 2 + 4, 0)
        Return New Rectangle(r.Left + (r.Width - w) \ 2, r.Bottom + 4, w, Math.Max(sz.Height + 2, 22))
    End Function

    Private Shared Function YesRect(ByVal label As Rectangle) As Rectangle
        Return New Rectangle(label.Right - ConfirmButtonWidth * 2 - 2, label.Top + 1, ConfirmButtonWidth, label.Height - 2)
    End Function

    Private Shared Function NoRect(ByVal label As Rectangle) As Rectangle
        Return New Rectangle(label.Right - ConfirmButtonWidth - 1, label.Top + 1, ConfirmButtonWidth, label.Height - 2)
    End Function

    ''' <summary>Called first thing by imgPhoto_Paint.</summary>
    Private Sub PaintFaces(ByVal g As Graphics)
        If Not FacesShown Then Return
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Using labelFont As New Font(Me.Font.FontFamily, 11.0F), back As New SolidBrush(Color.FromArgb(200, 20, 24, 30)),
              yesBrush As New SolidBrush(Color.FromArgb(46, 154, 91)), noBrush As New SolidBrush(Color.FromArgb(197, 58, 58))
            For Each f In m_lpFaces
                Dim r As Rectangle = FaceRect(f)
                Dim selected As Boolean = f Is m_lpSelFace
                Using pen As New Pen(If(selected, SelectedPen, FaceColor(f)), If(selected, 3.0F, 2.0F))
                    If Not f.IsCertain AndAlso Not selected Then pen.DashStyle = Drawing2D.DashStyle.Dash
                    Using shadow As New Pen(Color.FromArgb(120, 0, 0, 0), pen.Width + 2)
                        g.DrawRectangle(shadow, r)
                    End Using
                    g.DrawRectangle(pen, r)
                End Using
                If f Is m_lpSelFace AndAlso cboFaceName.Visible Then Continue For   ' the name box is there
                Dim lr As Rectangle = FaceLabelRect(g, f, labelFont)
                g.FillRectangle(back, lr)
                Dim textRect As Rectangle = If(f.NeedsConfirm, New Rectangle(lr.X, lr.Y, lr.Width - ConfirmButtonWidth * 2 - 4, lr.Height), lr)
                TextRenderer.DrawText(g, FaceLabel(f), labelFont, textRect, FaceColor(f),
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                If f.NeedsConfirm Then
                    Dim yes As Rectangle = YesRect(lr), no As Rectangle = NoRect(lr)
                    g.FillRectangle(yesBrush, yes)
                    g.FillRectangle(noBrush, no)
                    TextRenderer.DrawText(g, "✓", labelFont, yes, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                    TextRenderer.DrawText(g, "✕", labelFont, no, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                End If
            Next
        End Using
    End Sub

    Private Enum FaceHitPart
        fhNone = 0
        fhFace = 1     ' the box or the label text
        fhYes = 2      ' ✓
        fhNo = 3       ' ✕
    End Enum

    Private Function HitFace(ByVal p As Point, Optional ByRef part As FaceHitPart = FaceHitPart.fhNone) As FaceRegion
        part = FaceHitPart.fhNone
        If Not FacesShown Then Return Nothing
        Using g As Graphics = imgPhoto.CreateGraphics(), labelFont As New Font(Me.Font.FontFamily, 11.0F)
            ' the ✓ / ✕ buttons first: they sit on the label
            For Each f In m_lpFaces.Where(Function(x) x.NeedsConfirm)
                Dim lr As Rectangle = FaceLabelRect(g, f, labelFont)
                If YesRect(lr).Contains(p) Then part = FaceHitPart.fhYes : Return f
                If NoRect(lr).Contains(p) Then part = FaceHitPart.fhNo : Return f
            Next
            Dim hit As FaceRegion = m_lpFaces.Where(Function(f) FaceRect(f).Contains(p) OrElse FaceLabelRect(g, f, labelFont).Contains(p)).
                                              OrderBy(Function(f) FaceRect(f).Width).FirstOrDefault()
            If hit IsNot Nothing Then part = FaceHitPart.fhFace
            Return hit
        End Using
    End Function

    ''' <summary>Called first thing by imgPhoto_MouseDown; True when it handled the click (on a face).</summary>
    Private Function FaceMouseDown(ByVal e As MouseEventArgs) As Boolean
        Dim part As FaceHitPart
        Dim f As FaceRegion = HitFace(e.Location, part)
        If f Is Nothing Then
            If m_lpSelFace IsNot Nothing Then
                CloseNameBox()
                m_lpSelFace = Nothing
                imgPhoto.Invalidate()
            End If
            Return False
        End If
        InitialSelection()
        m_lpSelFace = f
        If e.Button = MouseButtons.Left AndAlso part = FaceHitPart.fhYes Then
            CloseNameBox()
            g_lpFaces.ConfirmFaces({f})
            AfterFaceChange()
        ElseIf e.Button = MouseButtons.Left AndAlso part = FaceHitPart.fhNo Then
            CloseNameBox()
            g_lpFaces.RejectFaces({f})
            AfterFaceChange()
        ElseIf e.Button = MouseButtons.Left Then
            OpenNameBox(f)
        ElseIf e.Button = MouseButtons.Right Then
            CloseNameBox()
            mnuFaceUnname.Enabled = f.IsCertain
            mnuFaceReject.Enabled = f.PersonID <> 0
            mnuFaceReject.Text = If(f.PersonID <> 0, "不是" & f.PersonName, "不是此人")
            imgPhoto.Invalidate()
            mnuFace.Show(imgPhoto, e.Location)
        End If
        Return True
    End Function

    ''' <summary>After ✓ / ✕ / naming: redraw, update the count, sort again when the user pauses.</summary>
    Private Sub AfterFaceChange()
        g_lpFaces.RequestOrganizeSoon()
        UpdateFaceBar()
        imgPhoto.Invalidate()
    End Sub

    '==================================================================================================
    ' Name box
    '==================================================================================================
    Private Sub OpenNameBox(ByVal f As FaceRegion)
        If m_lpPeople Is Nothing Then m_lpPeople = g_lpDatabase.LoadPeopleNames()
        cboFaceName.BeginUpdate()
        cboFaceName.Items.Clear()
        cboFaceName.Items.AddRange(m_lpPeople.Cast(Of Object)().ToArray())
        cboFaceName.EndUpdate()
        cboFaceName.Text = If(f.PersonID <> 0, f.PersonName, "")

        ' under the face, kept inside picPhoto
        Dim r As Rectangle = FaceRect(f)
        Dim p As Point = picPhoto.PointToClient(imgPhoto.PointToScreen(New Point(r.Left + r.Width \ 2 - cboFaceName.Width \ 2, r.Bottom + 4)))
        p.X = Math.Max(0, Math.Min(picPhoto.ClientSize.Width - cboFaceName.Width, p.X))
        p.Y = Math.Max(0, Math.Min(picPhoto.ClientSize.Height - cboFaceName.Height, p.Y))
        cboFaceName.Location = p
        cboFaceName.Visible = True
        cboFaceName.BringToFront()
        cboFaceName.Focus()
        cboFaceName.SelectAll()
        imgPhoto.Invalidate()
    End Sub

    Private Sub CloseNameBox()
        If cboFaceName Is Nothing OrElse Not cboFaceName.Visible Then Return
        cboFaceName.Visible = False
        imgPhoto.Invalidate()
    End Sub

    ''' <summary>True while the name box is open (Esc closes it, not the viewer).</summary>
    Private ReadOnly Property NameBoxOpen As Boolean
        Get
            Return cboFaceName IsNot Nothing AndAlso cboFaceName.Visible
        End Get
    End Property

    Private Sub cboFaceName_KeyDown(sender As Object, e As KeyEventArgs) Handles cboFaceName.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                e.Handled = True
                e.SuppressKeyPress = True
                SaveName()
            Case Keys.Escape
                e.Handled = True
                e.SuppressKeyPress = True
                CloseNameBox()
        End Select
    End Sub

    Private Sub cboFaceName_Leave(sender As Object, e As EventArgs) Handles cboFaceName.Leave
        CloseNameBox()
    End Sub

    Private Sub SaveName()
        Dim f As FaceRegion = m_lpSelFace
        Dim name As String = cboFaceName.Text.Trim()
        CloseNameBox()
        If f Is Nothing Then Return
        If name <> "" AndAlso Not CheckNameRule(name) Then
            frmMsgBox.ShowCriticalMessage("姓名含有不合法的字元", "錯誤")
            Return
        End If
        g_lpFaces.Name(f, name)
        g_lpFaces.RequestOrganizeSoon()
        If name <> "" AndAlso Not m_lpPeople.Contains(name) Then m_lpPeople = Nothing
        m_lpSelFace = NextUnnamed(f)
        UpdateFaceBar()
        imgPhoto.Invalidate()
    End Sub

    ''' <summary>The next face without a name after <paramref name="f"/> (left to right), for Tab-like naming.</summary>
    Private Function NextUnnamed(ByVal f As FaceRegion) As FaceRegion
        Dim i As Integer = m_lpFaces.IndexOf(f)
        For k = 1 To m_lpFaces.Count - 1
            Dim c As FaceRegion = m_lpFaces((i + k) Mod m_lpFaces.Count)
            If Not c.IsCertain Then Return c
        Next
        Return Nothing
    End Function

    '==================================================================================================
    ' Menu / keys / manual faces
    '==================================================================================================
    Private Sub mnuFaceUnname_Click(sender As Object, e As EventArgs) Handles mnuFaceUnname.Click
        If m_lpSelFace Is Nothing Then Return
        g_lpFaces.Unname(m_lpSelFace)
        g_lpFaces.RequestOrganizeSoon()
        UpdateFaceBar()
        imgPhoto.Invalidate()
    End Sub

    Private Sub mnuFaceReject_Click(sender As Object, e As EventArgs) Handles mnuFaceReject.Click
        If m_lpSelFace Is Nothing OrElse m_lpSelFace.PersonID = 0 Then Return
        g_lpFaces.RejectFaces({m_lpSelFace})
        AfterFaceChange()
    End Sub

    Private Sub mnuFaceNotFace_Click(sender As Object, e As EventArgs) Handles mnuFaceNotFace.Click
        RemoveSelectedFace()
    End Sub

    Private Sub FaceKeys_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Delete AndAlso Not NameBoxOpen AndAlso m_lpSelFace IsNot Nothing AndAlso FacesShown Then
            e.Handled = True
            RemoveSelectedFace()
        End If
    End Sub

    Private Sub RemoveSelectedFace()
        Dim f As FaceRegion = m_lpSelFace
        If f Is Nothing Then Return
        If f.IsCertain AndAlso Not frmQueryMsgBox.ShowMessage("「" & f.PersonName & "」會從這張照片的人物移除，確定這不是臉嗎？", "注意") Then Return
        g_lpFaces.MarkNotFace(f)
        g_lpFaces.RequestOrganizeSoon()
        m_lpFaces.Remove(f)
        m_lpSelFace = Nothing
        UpdateFaceBar()
        imgPhoto.Invalidate()
    End Sub

    Private Sub AddFaceFromSelection()
        If m_lpImage Is Nothing Then Return
        If Changed Then
            frmMsgBox.ShowCriticalMessage("照片已編輯，請先存檔再新增面孔", "注意")
            Return
        End If
        Dim r As Rectangle = SelectionOnPicture()
        If r.Width < 8 OrElse r.Height < 8 Then
            frmMsgBox.ShowCriticalMessage("請先在照片上框出臉（按住 Shift 為正方形），再按「新增面孔」", "新增面孔")
            Return
        End If
        Dim box As New RectangleF(CSng(r.X / m_lpImage.Width), CSng(r.Y / m_lpImage.Height), CSng(r.Width / m_lpImage.Width), CSng(r.Height / m_lpImage.Height))
        If Not m_bolFaceMode Then
            m_bolFaceMode = True
            LoadFaces()   ' also analyses the photo when it wasn't yet, so the new face has a FacePhoto row
        End If
        Dim f As FaceRegion
        Try
            UseWaitCursor = True
            f = g_lpFaces.AddManual(m_strFileName, box)
        Finally
            UseWaitCursor = False
        End Try
        InitialSelection()
        If f Is Nothing Then
            frmMsgBox.ShowCriticalMessage("無法新增這張臉", "錯誤")
            Return
        End If
        m_lpFaces.Add(f)
        m_lpSelFace = f
        UpdateFaceBar()
        OpenNameBox(f)
    End Sub

End Class
