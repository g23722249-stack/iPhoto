' Port of Lib\Form\frmViewerLarge.frm: the full-screen viewer for screens 1280 wide or more -- a picture
' with simple editing (rotate, crop, red eye, sepia, grey, soften, sharpen, brightness / contrast, undo),
' or a video with play / pause, position and volume.
'
' Layout: in the designer (design size 1280 x 1024). VB6 placed the bars from the screen size at run
' time (SetToolBarPosition / SetControlPhotoMode / SetControlVideoMode); the same positions are now
' designer values with Anchors, so the maximised form lays itself out on any screen.
' The toolbar buttons (左轉90° ... 清晰) are picImageBar1.Items in the designer.
' Quartz (StdPicture filters, FixSize) -> Techno.Net Quartz.ImageFilter / Quartz.FixSize.
' Windows Media Player -> Aqua.MediaViewerControl (Play / Pause / Stop / Duration / CurrentPosition).
'
' Fixed from VB6: crop / red eye use the selection whichever way it was dragged, and red eye works in
' picture pixels (VB6 passed screen coordinates); the zoom factor comes from the size actually shown
' (VB6 compared the height with the canvas width); ShowVideo keeps the first/last flags for the
' prior/next buttons; the file info is read for the media type being shown; Shift (square selection)
' also holds on mouse up. Moving a brightness / contrast slider is one undo step per drag, and at most
' MaxUndo steps are kept -- VB6 kept a full-size copy for every slider tick.
Public Class frmViewerLarge
    Implements IPhotoViewer

    Private Enum enumMediaMode
        mmImage = 0
        mmVideo = 1
    End Enum

    Private Const MaxUndo As Integer = 10

    Public Event ShowPriorPhoto As EventHandler Implements IPhotoViewer.ShowPriorPhoto
    Public Event ShowNextPhoto As EventHandler Implements IPhotoViewer.ShowNextPhoto

    Private m_P1, m_P2 As Point                   ' selection, in imgPhoto (display) pixels
    Private m_blnDrawing As Boolean

    Private ReadOnly m_lpPicture As New List(Of Image)   ' undo stack (oldest first)
    Private m_lpImage As Image                    ' the picture being edited, full size
    Private m_lastEdit As String                  ' slider being dragged (one undo step per drag)
    Private m_intLastBrightness As Integer
    Private m_intLastContrast As Integer
    Private m_dblScale As Double = 1
    Private m_bolFirstPhoto As Boolean
    Private m_bolLastPhoto As Boolean
    Private m_enumMediaMode As enumMediaMode
    Private m_strFileName As String = ""

    '==================================================================================================
    ' IPhotoViewer
    '==================================================================================================
    Public ReadOnly Property Changed As Boolean Implements IPhotoViewer.Changed
        Get
            Return m_lpPicture.Count > 0
        End Get
    End Property

    Public ReadOnly Property Photo As Image Implements IPhotoViewer.Photo
        Get
            Return m_lpImage
        End Get
    End Property

    Public Sub Create(ByVal Width As Integer, ByVal Height As Integer) Implements IPhotoViewer.Create
        If WindowState = FormWindowState.Normal Then Size = New Size(Width, Height)
    End Sub

    Public Sub Clear() Implements IPhotoViewer.Clear
        Try
            mpViewerVideo.Clear()
        Catch
        End Try

        DisposeImage(m_lpImage)   ' first: the slider handlers below do nothing without a picture
        m_lpImage = Nothing
        ClearUndo()
        m_lastEdit = Nothing

        m_intLastBrightness = 0
        m_intLastContrast = 0
        sliBrightness.Value = 0
        sliContrast.Value = 0

        SetDisplay(Nothing)
        m_strFileName = ""
        Timer1.Enabled = False

        imbPrior.Enabled = False
        imbNext.Enabled = False

        HideFaceBar()
        HideInfo()   ' frmViewerLarge.Info.vb
        picPage.Visible = False
        picImageBar1.Visible = False
        picImageBar2.Visible = False
        picVideoBar.Visible = False
        pgVideo.Visible = False
        imgPhoto.Visible = False
        mpViewerVideo.Visible = False

        BackColor = Color.Black
    End Sub

    Public Sub ShowPicture(ByVal FileName As String, ByVal Picture As Image, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean) Implements IPhotoViewer.ShowPicture
        Clear()
        InitialSelection()
        SetControlPhotoMode()

        m_strFileName = FileName
        m_lpImage = Picture
        m_bolFirstPhoto = FirstPhoto
        m_bolLastPhoto = LastPhoto
        m_enumMediaMode = enumMediaMode.mmImage

        MovePictureToScreen()
        MoveFilePropertyToScreen()

        imbPrior.Enabled = Not m_bolFirstPhoto
        imbNext.Enabled = Not m_bolLastPhoto

        picImageBar1.Enabled = True
        picImageBar2.Enabled = True
        picPage.Visible = True
        imgPhoto.Visible = True
        ShowFacesForPicture()   ' frmViewerLarge.Faces.vb
        ShowInfoForPicture()    ' frmViewerLarge.Info.vb
    End Sub

    Public Sub ShowVideo(ByVal FileName As String, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean) Implements IPhotoViewer.ShowVideo
        Clear()
        SetControlVideoMode()

        m_strFileName = FileName
        m_bolFirstPhoto = FirstPhoto
        m_bolLastPhoto = LastPhoto
        m_enumMediaMode = enumMediaMode.mmVideo

        mpViewerVideo.AutoRewind = chkAutoRewind.Checked
        mpViewerVideo.Volume = sliSound.Value
        mpViewerVideo.FileName = FileName
        mpViewerVideo.Visible = True
        mpViewerVideo.Play()

        MoveFilePropertyToScreen()

        imbPrior.Enabled = Not m_bolFirstPhoto
        imbNext.Enabled = Not m_bolLastPhoto
        picPage.Visible = True
    End Sub

    '==================================================================================================
    ' Form
    '==================================================================================================
    Private Sub Form_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape AndAlso Not NameBoxOpen Then   ' Esc in the face name box only closes it
            e.Handled = True
            CloseViewer()
        End If
    End Sub

    ''' <summary>VB6 "Unload Me". The main window keeps this viewer for the whole session, so it is only
    ''' hidden (a modal "全圖瀏覽" returns; the second-screen viewer just goes away).</summary>
    Private Sub CloseViewer()
        mpViewerVideo.Pause()
        If Modal Then Close() Else Hide()
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.CloseReason = CloseReason.UserClosing AndAlso Not Modal Then
            e.Cancel = True
            Hide()
        End If
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If Not Modal Then Clear()
    End Sub

    Private Sub imbNext_Click(sender As Object, e As EventArgs) Handles imbNext.Click
        RaiseEvent ShowNextPhoto(Me, EventArgs.Empty)
    End Sub

    Private Sub imbPrior_Click(sender As Object, e As EventArgs) Handles imbPrior.Click
        RaiseEvent ShowPriorPhoto(Me, EventArgs.Empty)
    End Sub

    '==================================================================================================
    ' Screen
    '==================================================================================================
    Private Sub SetControlPhotoMode()
        pgVideo.Visible = False
        picVideoBar.Visible = False
        mpViewerVideo.Clear()
        mpViewerVideo.Visible = False

        picPhoto.Visible = True
        picPage.Visible = True
        picImageBar1.Visible = True
        picImageBar2.Visible = True
        pgImage.Visible = False

        imgUndo.Image = imgUndoDisabled.Image
        imgUndo.Enabled = False
        lblUndo.Text = "無法復原"
    End Sub

    Private Sub SetControlVideoMode()
        picPhoto.Visible = False
        picPage.Visible = True
        picImageBar1.Visible = False
        picImageBar2.Visible = False
        pgImage.Visible = False

        pgVideo.CurrentPosition = 0
        pgVideo.Visible = True
        picVideoBar.Visible = True
        lblDuration.Text = ""
        lblCurrentPosition.Text = ""
        ShowPlayState()
    End Sub

    ''' <summary>Shrinks the picture to fit picPhoto (never enlarges it) and centres it.</summary>
    Private Sub MovePictureToScreen()
        If m_lpImage Is Nothing Then Return

        Dim fit As New Quartz.FixSize
        fit.SetPictureSize(m_lpImage.Width, m_lpImage.Height)
        fit.SetCanvasSize(picPhoto.ClientSize.Width, picPhoto.ClientSize.Height)
        fit.Resize()
        Dim w As Integer = Math.Max(1, fit.Width), h As Integer = Math.Max(1, fit.Height)
        imgPhoto.Bounds = New Rectangle((picPhoto.ClientSize.Width - w) \ 2, (picPhoto.ClientSize.Height - h) \ 2, w, h)

        Dim shown As New Bitmap(w, h)
        Using g As Graphics = Graphics.FromImage(shown)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(m_lpImage, 0, 0, w, h)
        End Using
        SetDisplay(shown)
        m_dblScale = w / m_lpImage.Width

        If m_lpPicture.Count > 0 Then
            imgUndo.Image = imgUndoEnabled.Image
            imgUndo.Enabled = True
            lblUndo.Text = "復原：" & m_lpPicture.Count
        End If
        lblResolution.Text = m_lpImage.Width & " x " & m_lpImage.Height
        UpdateFaceBar()   ' faces hide while the picture has unsaved edits
    End Sub

    Private Sub SetDisplay(ByVal img As Image)
        Dim old As Image = imgPhoto.Image
        imgPhoto.Image = img
        If old IsNot Nothing Then old.Dispose()
    End Sub

    Private Sub MoveFilePropertyToScreen()
        If m_strFileName.Trim() = "" Then
            lblResolution.Text = ""
            lblFileLength.Text = ""
            Return
        End If
        lblFileName.Text = IO.Path.GetFileName(m_strFileName)

        Dim sz As Size = Size.Empty
        Select Case m_enumMediaMode
            Case enumMediaMode.mmImage : If m_lpImage IsNot Nothing Then sz = m_lpImage.Size
            Case enumMediaMode.mmVideo : sz = mpViewerVideo.VideoSize
        End Select
        lblResolution.Text = sz.Width & " x " & sz.Height

        ' VB6: MB cut (not rounded) to two decimals
        Dim mb As Double = New IO.FileInfo(m_strFileName).Length / 1024 / 1024
        lblFileLength.Text = (Math.Truncate(mb * 100) / 100) & " M"
    End Sub

    '==================================================================================================
    ' Selection (VB6 drew an XOR rectangle on imgPhoto)
    '==================================================================================================
    Private Sub InitialSelection()
        m_P1 = Point.Empty
        m_P2 = Point.Empty
        m_blnDrawing = False
        lblSelPoint.Text = ""
        imgPhoto.Invalidate()
    End Sub

    Private Function SelectionOnScreen() As Rectangle
        Return Rectangle.FromLTRB(Math.Min(m_P1.X, m_P2.X), Math.Min(m_P1.Y, m_P2.Y), Math.Max(m_P1.X, m_P2.X), Math.Max(m_P1.Y, m_P2.Y))
    End Function

    ''' <summary>The selection in picture pixels.</summary>
    Private Function SelectionOnPicture() As Rectangle
        Dim r As Rectangle = SelectionOnScreen()
        Return Rectangle.FromLTRB(CInt(Int(r.Left / m_dblScale)), CInt(Int(r.Top / m_dblScale)), CInt(Int(r.Right / m_dblScale)), CInt(Int(r.Bottom / m_dblScale)))
    End Function

    Private Sub imgPhoto_MouseDown(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseDown
        If FaceMouseDown(e) Then Return   ' a click on a face box (face mode)
        If e.Button = MouseButtons.Left Then
            m_blnDrawing = True
            m_P1 = e.Location
            m_P2 = m_P1
            imgPhoto.Invalidate()
        Else
            InitialSelection()
        End If
    End Sub

    Private Sub imgPhoto_MouseMove(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseMove
        If e.Button <> MouseButtons.Left OrElse Not m_blnDrawing OrElse imgPhoto.Image Is Nothing Then Return
        AdjustP2(e.X, e.Y, ModifierKeys)
        imgPhoto.Invalidate()
        Dim r As Rectangle = SelectionOnPicture()
        lblSelPoint.Text = "(" & r.Left & "," & r.Top & ")～(" & r.Right & "," & r.Bottom & ")"
    End Sub

    Private Sub imgPhoto_MouseUp(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseUp
        If e.Button = MouseButtons.Left AndAlso m_blnDrawing Then
            AdjustP2(e.X, e.Y, ModifierKeys)
            imgPhoto.Invalidate()
        End If
    End Sub

    Private Sub imgPhoto_Paint(sender As Object, e As PaintEventArgs) Handles imgPhoto.Paint
        PaintFaces(e.Graphics)
        Dim r As Rectangle = SelectionOnScreen()
        If r.Width <= 0 AndAlso r.Height <= 0 Then Return
        Using black As New Pen(Color.Black), white As New Pen(Color.White) With {.DashStyle = Drawing2D.DashStyle.Dash}
            e.Graphics.DrawRectangle(black, r)
            e.Graphics.DrawRectangle(white, r)
        End Using
    End Sub

    ''' <summary>Shift = square, otherwise free (VB6 AdjustP2; its Ctrl mode was never enabled).</summary>
    Private Sub AdjustP2(ByVal X As Integer, ByVal Y As Integer, ByVal keys As Keys)
        If (keys And Keys.Shift) = Keys.Shift Then
            If Math.Abs(X - m_P1.X) <= Math.Abs(Y - m_P1.Y) Then
                m_P2.X = X
                m_P2.Y = If(Y > m_P1.Y, m_P1.Y + Math.Abs(X - m_P1.X), m_P1.Y - Math.Abs(X - m_P1.X))
            Else
                m_P2.X = If(X > m_P1.X, m_P1.X + Math.Abs(Y - m_P1.Y), m_P1.X - Math.Abs(Y - m_P1.Y))
                m_P2.Y = Y
            End If
        Else
            m_P2 = New Point(X, Y)
        End If
    End Sub

    Private Sub picPhoto_Click(sender As Object, e As EventArgs) Handles picPhoto.Click
        InitialSelection()
    End Sub

    '==================================================================================================
    ' Editing
    '==================================================================================================
    Private Sub picImageBar1_Click(sender As Object, Index As Integer) Handles picImageBar1.Click
        If m_lpImage Is Nothing Then Return
        Select Case Index
            Case 0 : RotatePicture(-90)   '左轉 90 度
            Case 1 : RotatePicture(90)    '右轉 90 度
            Case 3 : CropPicture()
            Case 2, 4, 5, 6, 7 : FilterPicture(Index)
        End Select
    End Sub

    Private Sub RotatePicture(ByVal lngAngle As Integer)
        picPhoto.Visible = False
        ApplyEdit(Quartz.ImageFilter.Rotate(m_lpImage, lngAngle), Nothing)
        InitialSelection()
        picPhoto.Visible = True
    End Sub

    Private Sub CropPicture()
        Dim r As Rectangle = SelectionOnPicture()
        If r.Width <= 0 OrElse r.Height <= 0 Then Return
        ApplyEdit(Quartz.ImageFilter.Crop(m_lpImage, r), Nothing)
        InitialSelection()
    End Sub

    Private Sub FilterPicture(ByVal Index As Integer)
        Dim result As Image = Nothing
        Select Case Index
            Case 2 '紅眼：需要先框選
                Dim r As Rectangle = SelectionOnPicture()
                If r.Width > 0 AndAlso r.Height > 0 Then result = Quartz.ImageFilter.RedEye(m_lpImage, r, AddressOf ShowProgress)
            Case 4 : result = Quartz.ImageFilter.Colourise(m_lpImage, Color.FromArgb(251, 209, 123), AddressOf ShowProgress) '泛黃
            Case 5 : result = Quartz.ImageFilter.GrayScale(m_lpImage, AddressOf ShowProgress) '黑白
            Case 6 : result = Quartz.ImageFilter.Soften(m_lpImage, False, AddressOf ShowProgress) '柔焦
            Case 7 : result = Quartz.ImageFilter.Sharpen(m_lpImage, False, AddressOf ShowProgress) '清晰
        End Select
        pgImage.Visible = False   ' VB6 m_lpQuartz_Complete
        If result IsNot Nothing Then ApplyEdit(result, Nothing)
    End Sub

    ''' <summary>VB6 m_lpQuartz_InitProgress / Progress.</summary>
    Private Sub ShowProgress(ByVal percent As Integer)
        If Not pgImage.Visible Then
            pgImage.Value = 0
            pgImage.Visible = True
            pgImage.BringToFront()
        End If
        If percent <> pgImage.Value Then
            pgImage.Value = Math.Max(pgImage.Minimum, Math.Min(pgImage.Maximum, percent))
            pgImage.Refresh()
        End If
    End Sub

    Private Sub sliBrightness_ValueChanged(sender As Object, e As EventArgs) Handles sliBrightness.ValueChanged
        If m_lpImage Is Nothing Then Return
        ApplyEdit(Quartz.ImageFilter.ContrastAndBrightness(m_lpImage, 0, sliBrightness.Value - m_intLastBrightness), "brightness")
        m_intLastBrightness = sliBrightness.Value
    End Sub

    Private Sub sliContrast_ValueChanged(sender As Object, e As EventArgs) Handles sliContrast.ValueChanged
        If m_lpImage Is Nothing Then Return
        ApplyEdit(Quartz.ImageFilter.ContrastAndBrightness(m_lpImage, sliContrast.Value - m_intLastContrast, 0), "contrast")
        m_intLastContrast = sliContrast.Value
    End Sub

    ''' <summary>Makes the edited picture current and keeps the previous one for undo (VB6
    ''' BackupUndoPicture). Repeated moves of the same slider share one undo step.</summary>
    Private Sub ApplyEdit(ByVal edited As Image, ByVal editKey As String)
        If editKey IsNot Nothing AndAlso editKey = m_lastEdit AndAlso m_lpPicture.Count > 0 Then
            DisposeImage(m_lpImage)   ' an intermediate step of the same drag
        Else
            m_lpPicture.Add(m_lpImage)
            If m_lpPicture.Count > MaxUndo Then
                DisposeImage(m_lpPicture(0))
                m_lpPicture.RemoveAt(0)
            End If
        End If
        m_lastEdit = editKey
        m_lpImage = edited
        MovePictureToScreen()
    End Sub

    Private Sub imgUndo_Click(sender As Object, e As EventArgs) Handles imgUndo.Click, lblUndo.Click
        If m_lpPicture.Count <= 0 Then Return
        DisposeImage(m_lpImage)
        m_lpImage = m_lpPicture(m_lpPicture.Count - 1)
        m_lpPicture.RemoveAt(m_lpPicture.Count - 1)
        m_lastEdit = Nothing

        If m_lpPicture.Count <= 0 Then
            imgUndo.Image = imgUndoDisabled.Image
            imgUndo.Enabled = False
            lblUndo.Text = "無法復原"
        End If
        MovePictureToScreen()
    End Sub

    Private Sub ClearUndo()
        For Each img As Image In m_lpPicture
            DisposeImage(img)
        Next
        m_lpPicture.Clear()
    End Sub

    Private Shared Sub DisposeImage(ByVal img As Image)
        If img IsNot Nothing Then img.Dispose()
    End Sub

    '==================================================================================================
    ' Video
    '==================================================================================================
    Private Sub ShowPlayState()
        picVideo.Image = If(mpViewerVideo.PlayState <> Aqua.MediaPlayState.Playing, imgPlay.Image, imgPause.Image)
    End Sub

    Private Sub chkAutoRewind_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoRewind.CheckedChanged
        mpViewerVideo.AutoRewind = chkAutoRewind.Checked
    End Sub

    Private Sub mpViewerVideo_MediaOpened(sender As Object, e As EventArgs) Handles mpViewerVideo.MediaOpened
        lblDuration.Text = "長度 = " & GetVideoDuration(CLng(mpViewerVideo.Duration))
        pgVideo.Duration = CInt(mpViewerVideo.Duration * 1000)
        MoveFilePropertyToScreen()   ' the video size is known now
    End Sub

    Private Sub mpViewerVideo_PlayStateChanged(sender As Object, e As EventArgs) Handles mpViewerVideo.PlayStateChanged
        Timer1.Enabled = (mpViewerVideo.PlayState = Aqua.MediaPlayState.Playing)
        ShowPlayState()
        If mpViewerVideo.VideoSize.Width > 0 AndAlso lblResolution.Text.StartsWith("0 ") Then MoveFilePropertyToScreen()
    End Sub

    Private Sub picVideo_Click(sender As Object, e As EventArgs) Handles picVideo.Click
        If mpViewerVideo.PlayState <> Aqua.MediaPlayState.Playing Then
            mpViewerVideo.Play()
        Else
            mpViewerVideo.Pause()
        End If
    End Sub

    Private Sub sliSound_ValueChanged(sender As Object, e As EventArgs) Handles sliSound.ValueChanged
        mpViewerVideo.Volume = sliSound.Value
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If mpViewerVideo.PlayState <> Aqua.MediaPlayState.Playing Then Return
        lblCurrentPosition.Text = "位置 = " & GetVideoDuration(CLng(mpViewerVideo.CurrentPosition))
        pgVideo.CurrentPosition = CInt(mpViewerVideo.CurrentPosition * 1000)
    End Sub

    Private Sub pgVideo_SlideChanged(sender As Object, e As EventArgs) Handles pgVideo.SlideChanged
        mpViewerVideo.CurrentPosition = pgVideo.CurrentPosition / 1000
    End Sub

End Class
