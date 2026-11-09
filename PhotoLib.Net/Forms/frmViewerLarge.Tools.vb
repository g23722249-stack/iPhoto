' The editing tools of 全圖瀏覽 (new in the .NET port; replaced 紅眼 / 泛黃 / 黑白 / 柔焦 / 清晰, which
' 相片後製 -- PhotoEdit -- does better and without touching the photo). The tool bar (picImageBar1):
'   0 左轉90°  1 右轉90°   the file itself, losslessly, through the main window (RotateRequested) -- or,
'                          while the picture has unsaved edits, as one more edit of it
'   2 裁切     a bar above the tool bar: the ratio (自由 / 原比例 / 4:3 / 3:2 / 16:9 / 1:1, turned to
'              portrait when the drag is taller than wide), the selection drawn with thirds, its size
'   3 拉直     a bar with an angle slider (±10°, tenths), the picture turned and cut live, a grid over it
'   4 自動調整  Quartz.ImageFilter.AutoEnhance
'   5 相片後製  6 加入 / 移出 Dock  7 刪除   asked of the main window (EditRequested / DockRequested /
'              DeleteRequested)
' Holding the right mouse button on an edited picture shows the original until it is let go. The
' edits are kept as before (undo, Changed); the main window saves them as <name>_edited.jpg, the original
' kept but hidden (PhotoLib EditedPhotos), when the photo is left.
' 評價 (stars, picImageBar2, where 定位點 was) and Docked are set by the main window for each photo.
Partial Class frmViewerLarge

    Public Event RotateRequested(ByVal sender As Object, ByVal angle As Integer) Implements IPhotoViewer.RotateRequested
    Public Event EditRequested As EventHandler Implements IPhotoViewer.EditRequested
    Public Event DockRequested As EventHandler Implements IPhotoViewer.DockRequested
    Public Event DeleteRequested As EventHandler Implements IPhotoViewer.DeleteRequested
    Public Event RatingChanged(ByVal sender As Object, ByVal rating As Integer) Implements IPhotoViewer.RatingChanged

    Private Enum ToolMode
        None
        Crop
        Straighten
    End Enum

    Private Const ItemDock As Integer = 6

    Private m_toolMode As ToolMode = ToolMode.None
    Private pnlTool As Panel
    Private lblToolTitle As Label
    Private lblToolInfo As Label
    Private ReadOnly m_ratioButtons As New List(Of CheckBox)
    Private sliAngle As Aqua.Slider
    Private butToolApply As Button, butToolCancel As Button, butToolReset As Button
    Private rtgRating As Aqua.RatingControl
    Private m_bolSettingRating As Boolean
    Private m_bolDocked As Boolean
    ''' <summary>The ratio width / height of the crop (0 = free).</summary>
    Private m_dblRatio As Double
    ''' <summary>拉直: the picture as shown when the tool opened (the preview is made from it).</summary>
    Private m_lpStraightenBase As Bitmap
    Private m_bolComparing As Boolean

    Private Shared ReadOnly Ratios As (Text As String, Value As Double)() = {
        ("自由", 0), ("原比例", -1), ("4:3", 4 / 3), ("3:2", 3 / 2), ("16:9", 16 / 9), ("1:1", 1)}

    Private Sub Tools_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' the bar above the tool bar, for 裁切 and 拉直
        pnlTool = New Panel With {.Visible = False, .BackColor = SystemColors.Window,
                                  .Bounds = New Rectangle(picImageBar1.Left, picImageBar1.Top - 52, picImageBar1.Width + picImageBar2.Width + 1, 48),
                                  .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left}
        Dim f As New Font("Microsoft JhengHei UI", 10.5F)
        lblToolTitle = New Label With {.AutoSize = False, .Bounds = New Rectangle(10, 14, 52, 22), .Font = New Font(f, FontStyle.Bold)}
        pnlTool.Controls.Add(lblToolTitle)
        Dim x As Integer = 66
        For Each r In Ratios
            Dim b As New CheckBox With {.Appearance = Appearance.Button, .Text = r.Text, .Tag = r.Value, .Font = f,
                                        .TextAlign = ContentAlignment.MiddleCenter, .Bounds = New Rectangle(x, 9, If(r.Text.Length > 2, 64, 52), 30)}
            AddHandler b.Click, Sub(s, ev) PickRatio(CType(s, CheckBox))
            m_ratioButtons.Add(b)
            pnlTool.Controls.Add(b)
            x += b.Width + 4
        Next
        sliAngle = New Aqua.Slider With {.Minimum = -100, .Maximum = 100, .Bounds = New Rectangle(66, 12, 320, 25), .ShowTicks = False}
        AddHandler sliAngle.ValueChanged, Sub(s, ev) PreviewStraighten()
        butToolReset = New Button With {.Text = "歸零", .Font = f, .Bounds = New Rectangle(394, 9, 60, 30)}
        AddHandler butToolReset.Click, Sub(s, ev) sliAngle.Value = 0
        pnlTool.Controls.Add(sliAngle)
        pnlTool.Controls.Add(butToolReset)
        lblToolInfo = New Label With {.AutoSize = False, .Font = f, .ForeColor = Color.DimGray, .TextAlign = ContentAlignment.MiddleLeft,
                                      .Bounds = New Rectangle(470, 9, pnlTool.Width - 470 - 200, 30), .Anchor = AnchorStyles.Left Or AnchorStyles.Top}
        pnlTool.Controls.Add(lblToolInfo)
        butToolApply = New Button With {.Font = f, .Bounds = New Rectangle(pnlTool.Width - 190, 9, 88, 30)}
        butToolCancel = New Button With {.Text = "取消", .Font = f, .Bounds = New Rectangle(pnlTool.Width - 96, 9, 88, 30)}
        AddHandler butToolApply.Click, Sub(s, ev) ApplyTool()
        AddHandler butToolCancel.Click, Sub(s, ev) EndTool()
        pnlTool.Controls.Add(butToolApply)
        pnlTool.Controls.Add(butToolCancel)
        Controls.Add(pnlTool)
        pnlTool.BringToFront()

        ' 評價, where 定位點 was (the crop bar shows the selection now)
        Label3.Visible = False
        lblSelPoint.Visible = False
        rtgRating = New Aqua.RatingControl With {.Bounds = New Rectangle(picImageBar2.Width - 96, 6, 92, 34)}
        Dim lblRating As New Label With {.Text = "評價", .AutoSize = True, .BackColor = Color.Transparent, .Location = New Point(picImageBar2.Width - 66, 50)}
        AddHandler rtgRating.RatingChanged, Sub(n)
                                                If Not m_bolSettingRating Then RaiseEvent RatingChanged(Me, n)
                                            End Sub
        picImageBar2.Controls.Add(rtgRating)
        picImageBar2.Controls.Add(lblRating)
    End Sub

    Public Property Rating As Integer Implements IPhotoViewer.Rating
        Get
            Return If(rtgRating Is Nothing, 0, rtgRating.Rating)
        End Get
        Set(value As Integer)
            If rtgRating Is Nothing Then Return
            m_bolSettingRating = True
            rtgRating.Rating = value
            m_bolSettingRating = False
        End Set
    End Property

    Public Property Docked As Boolean Implements IPhotoViewer.Docked
        Get
            Return m_bolDocked
        End Get
        Set(value As Boolean)
            m_bolDocked = value
            If picImageBar1.Count > ItemDock Then
                picImageBar1.Items(ItemDock).Text = If(value, "移出 Dock", "加入 Dock")
                picImageBar1.Invalidate()
            End If
        End Set
    End Property

    Private m_bolEditLocked As Boolean

    ''' <summary>The name shown for the file (an encrypted photo shows its own name, not the vault file's).</summary>
    Public Sub ShowName(ByVal name As String)
        m_strShownName = name
        lblFileName.Text = name
    End Sub

    ''' <summary>ShowName's name until Clear (the video's size, known later, shows the file's properties again).</summary>
    Private m_strShownName As String

    Public Property EditLocked As Boolean Implements IPhotoViewer.EditLocked
        Get
            Return m_bolEditLocked
        End Get
        Set(value As Boolean)
            m_bolEditLocked = value
            If value Then EndTool()
            picImageBar1.Enabled = Not value
        End Set
    End Property

    '==================================================================================================
    ' The tool bar
    '==================================================================================================
    Private Sub picImageBar1_Click(sender As Object, Index As Integer) Handles picImageBar1.Click
        If m_lpImage Is Nothing OrElse m_bolEditLocked Then Return
        If Index <> 2 AndAlso Index <> 3 Then EndTool()
        Select Case Index
            Case 0, 1
                Dim angle As Integer = If(Index = 0, -90, 90)
                If Changed Then
                    RotatePicture(angle)   ' one more edit of the edited picture
                Else
                    RaiseEvent RotateRequested(Me, angle)   ' the file, losslessly
                End If
            Case 2 : BeginTool(ToolMode.Crop)
            Case 3 : BeginTool(ToolMode.Straighten)
            Case 4
                Dim result As Image = Quartz.ImageFilter.AutoEnhance(m_lpImage, AddressOf ShowProgress)
                pgImage.Visible = False
                ApplyEdit(result, Nothing)
            Case 5 : RaiseEvent EditRequested(Me, EventArgs.Empty)
            Case 6 : RaiseEvent DockRequested(Me, EventArgs.Empty)
            Case 7 : RaiseEvent DeleteRequested(Me, EventArgs.Empty)
        End Select
    End Sub

    '==================================================================================================
    ' 裁切 / 拉直
    '==================================================================================================
    Private Sub BeginTool(ByVal mode As ToolMode)
        If m_toolMode = mode Then Return
        EndTool()
        m_toolMode = mode
        Dim crop As Boolean = mode = ToolMode.Crop
        For Each b In m_ratioButtons
            b.Visible = crop
        Next
        sliAngle.Visible = Not crop
        butToolReset.Visible = Not crop
        lblToolTitle.Text = If(crop, "比例", "角度")
        butToolApply.Text = If(crop, "裁切", "套用")
        If crop Then
            PickRatio(m_ratioButtons(0))
            lblToolInfo.Text = "在照片上拖曳，框出要保留的範圍"
        Else
            InitialSelection()
            m_lpStraightenBase = If(imgPhoto.Image Is Nothing, Nothing, New Bitmap(imgPhoto.Image))
            sliAngle.Value = 0
            lblToolInfo.Text = "0.0°　對齊格線，往左右拖曳"
        End If
        pnlTool.Visible = True
        pnlTool.BringToFront()
        imgPhoto.Invalidate()
    End Sub

    ''' <summary>The bar goes; a 拉直 preview not applied goes with it.</summary>
    Private Sub EndTool()
        If m_toolMode = ToolMode.None Then Return
        Dim wasStraighten As Boolean = m_toolMode = ToolMode.Straighten
        m_toolMode = ToolMode.None
        If pnlTool IsNot Nothing Then pnlTool.Visible = False
        If wasStraighten Then
            m_lpStraightenBase?.Dispose()
            m_lpStraightenBase = Nothing
            MovePictureToScreen()
        End If
        imgPhoto.Invalidate()
    End Sub

    Private Sub ApplyTool()
        Select Case m_toolMode
            Case ToolMode.Crop
                Dim r As Rectangle = SelectionOnPicture()
                If r.Width < 2 OrElse r.Height < 2 Then
                    lblToolInfo.Text = "請先在照片上拖曳框出範圍"
                    Return
                End If
                EndTool()
                CropPicture()
            Case ToolMode.Straighten
                Dim degrees As Double = sliAngle.Value / 10.0
                EndTool()
                If Math.Abs(degrees) >= 0.05 Then ApplyEdit(Quartz.ImageFilter.Straighten(m_lpImage, degrees), Nothing)
        End Select
    End Sub

    Private Sub PickRatio(ByVal b As CheckBox)
        For Each other In m_ratioButtons
            other.Checked = other Is b
        Next
        Dim v As Double = CDbl(b.Tag)
        m_dblRatio = If(v < 0, If(m_lpImage Is Nothing, 0, m_lpImage.Width / CDbl(m_lpImage.Height)), v)
        InitialSelection()
    End Sub

    ''' <summary>The selection's second corner under the crop ratio (portrait when dragged taller than wide),
    ''' kept inside the picture. False when no ratio applies.</summary>
    Private Function AdjustP2ToRatio(ByVal X As Integer, ByVal Y As Integer) As Boolean
        If m_toolMode <> ToolMode.Crop OrElse m_dblRatio <= 0 Then Return False
        Dim dx As Integer = X - m_P1.X, dy As Integer = Y - m_P1.Y
        Dim ratio As Double = m_dblRatio
        If Math.Abs(dy) > Math.Abs(dx) AndAlso ratio > 1 Then ratio = 1 / ratio   ' portrait
        Dim w As Double = Math.Abs(dx), h As Double = w / ratio
        If h < Math.Abs(dy) Then h = Math.Abs(dy) : w = h * ratio
        ' inside the picture
        Dim maxW As Double = If(dx >= 0, imgPhoto.ClientSize.Width - 1 - m_P1.X, m_P1.X)
        Dim maxH As Double = If(dy >= 0, imgPhoto.ClientSize.Height - 1 - m_P1.Y, m_P1.Y)
        Dim k As Double = Math.Min(1, Math.Min(If(w > 0, maxW / w, 1), If(h > 0, maxH / h, 1)))
        w *= k : h *= k
        m_P2 = New Point(m_P1.X + CInt(Math.Sign(If(dx = 0, 1, dx)) * w), m_P1.Y + CInt(Math.Sign(If(dy = 0, 1, dy)) * h))
        Return True
    End Function

    Private Sub ShowCropSize()
        If m_toolMode <> ToolMode.Crop Then Return
        Dim r As Rectangle = SelectionOnPicture()
        lblToolInfo.Text = If(r.Width > 0 AndAlso r.Height > 0, $"{r.Width} × {r.Height}", "在照片上拖曳，框出要保留的範圍")
    End Sub

    Private Sub PreviewStraighten()
        If m_toolMode <> ToolMode.Straighten OrElse m_lpStraightenBase Is Nothing Then Return
        Dim degrees As Double = sliAngle.Value / 10.0
        lblToolInfo.Text = degrees.ToString("+0.0;-0.0;0.0") & "°　對齊格線，往左右拖曳"
        Dim preview As Bitmap
        Using turned As Bitmap = Quartz.ImageFilter.Straighten(m_lpStraightenBase, degrees)
            preview = New Bitmap(m_lpStraightenBase.Width, m_lpStraightenBase.Height)
            Using g As Graphics = Graphics.FromImage(preview)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
                g.DrawImage(turned, 0, 0, preview.Width, preview.Height)
            End Using
        End Using
        SetDisplay(preview)
    End Sub

    ''' <summary>Called by imgPhoto_Paint: the crop selection's thirds, 拉直's grid.</summary>
    Private Sub PaintToolGuides(ByVal g As Graphics)
        Select Case m_toolMode
            Case ToolMode.Crop
                Dim r As Rectangle = SelectionOnScreen()
                If r.Width < 6 OrElse r.Height < 6 Then Return
                Using p As New Pen(Color.FromArgb(160, 255, 255, 255))
                    For i = 1 To 2
                        g.DrawLine(p, r.Left + r.Width * i \ 3, r.Top, r.Left + r.Width * i \ 3, r.Bottom)
                        g.DrawLine(p, r.Left, r.Top + r.Height * i \ 3, r.Right, r.Top + r.Height * i \ 3)
                    Next
                End Using
            Case ToolMode.Straighten
                Dim w As Integer = imgPhoto.ClientSize.Width, h As Integer = imgPhoto.ClientSize.Height
                Using p As New Pen(Color.FromArgb(120, 255, 255, 255))
                    For i = 1 To 7
                        g.DrawLine(p, w * i \ 8, 0, w * i \ 8, h)
                        g.DrawLine(p, 0, h * i \ 8, w, h * i \ 8)
                    Next
                End Using
        End Select
    End Sub

    '==================================================================================================
    ' Right button: the original while held
    '==================================================================================================
    ''' <summary>Called by imgPhoto_MouseDown; True when it showed the original.</summary>
    Private Function BeginCompare(ByVal e As MouseEventArgs) As Boolean
        If e.Button <> MouseButtons.Right OrElse Not Changed OrElse m_toolMode <> ToolMode.None Then Return False
        Dim original As Image = m_lpPicture(0)
        Dim shown As New Bitmap(Math.Max(1, imgPhoto.Width), Math.Max(1, imgPhoto.Height))
        Using g As Graphics = Graphics.FromImage(shown)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
            g.DrawImage(original, 0, 0, shown.Width, shown.Height)
        End Using
        SetDisplay(shown)
        lblResolution.Text = "原圖（放開滑鼠回到修改後）"
        m_bolComparing = True
        Return True
    End Function

    Private Sub imgPhoto_CompareUp(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseUp
        If Not m_bolComparing OrElse e.Button <> MouseButtons.Right Then Return
        m_bolComparing = False
        MovePictureToScreen()
    End Sub

End Class
