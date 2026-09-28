' Port of Lib\Form\frmViewerSmall.frm: the full-screen viewer for screens narrower than 1280 -- the same
' picture / video display as frmViewerLarge without the editing tools (so Changed is always False).
'
' Layout: in the designer (design size 1024 x 768), with Anchors instead of VB6 SetToolBarPosition.
' Quartz.FixSize -> Techno.Net; Windows Media Player -> Aqua.MediaViewerControl.
' Fixed from VB6: ShowVideo keeps the first/last flags for the prior/next buttons; the file info is read
' for the media type being shown.
Public Class frmViewerSmall
    Implements IPhotoViewer

    Private Enum enumMediaMode
        mmImage = 0
        mmVideo = 1
    End Enum

    Public Event ShowPriorPhoto As EventHandler Implements IPhotoViewer.ShowPriorPhoto
    Public Event ShowNextPhoto As EventHandler Implements IPhotoViewer.ShowNextPhoto

    Private m_lpImage As Image
    Private m_bolFirstPhoto As Boolean
    Private m_bolLastPhoto As Boolean
    Private m_enumMediaMode As enumMediaMode
    Private m_strFileName As String = ""

    '==================================================================================================
    ' IPhotoViewer
    '==================================================================================================
    Public ReadOnly Property Changed As Boolean Implements IPhotoViewer.Changed
        Get
            Return False
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

        If m_lpImage IsNot Nothing Then m_lpImage.Dispose()
        m_lpImage = Nothing
        SetDisplay(Nothing)
        m_strFileName = ""
        Timer1.Enabled = False

        imbPrior.Enabled = False
        imbNext.Enabled = False

        picPage.Visible = False
        picVideoBar.Visible = False
        pgVideo.Visible = False
        imgPhoto.Visible = False
        mpViewerVideo.Visible = False

        BackColor = Color.Black
    End Sub

    Public Sub ShowPicture(ByVal FileName As String, ByVal Picture As Image, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean) Implements IPhotoViewer.ShowPicture
        Clear()
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
        picPage.Visible = True
        imgPhoto.Visible = True
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
        If e.KeyCode = Keys.Escape Then
            e.Handled = True
            mpViewerVideo.Pause()
            If Modal Then Close() Else Hide()   ' VB6 "Unload Me" -- see frmViewerLarge.CloseViewer
        End If
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.CloseReason = CloseReason.UserClosing AndAlso Not Modal Then
            e.Cancel = True
            Hide()
        End If
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
        picImageBar.Visible = True
        mpViewerVideo.Clear()
        mpViewerVideo.Visible = False

        picPhoto.Visible = True
        picPage.Visible = True
        pgImage.Visible = False
    End Sub

    Private Sub SetControlVideoMode()
        picImageBar.Visible = False
        picPhoto.Visible = False
        picPage.Visible = True
        pgImage.Visible = False

        pgVideo.CurrentPosition = 0
        pgVideo.Visible = True
        picVideoBar.Visible = True
        lblDuration.Text = ""
        lblCurrentPosition.Text = ""
        ShowPlayState()
    End Sub

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

        lblResolution.Text = m_lpImage.Width & " x " & m_lpImage.Height
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

        Dim mb As Double = New IO.FileInfo(m_strFileName).Length / 1024 / 1024
        lblFileLength.Text = (Math.Truncate(mb * 100) / 100) & " M"
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
        MoveFilePropertyToScreen()
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
