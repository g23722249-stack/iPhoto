<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmViewerSmall
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmViewerSmall))
        Me.mpViewerVideo = New Aqua.MediaViewerControl()
        Me.imgPlay = New System.Windows.Forms.PictureBox()
        Me.imgPause = New System.Windows.Forms.PictureBox()
        Me.pgImage = New Aqua.ProgressBar()
        Me.picPhoto = New System.Windows.Forms.Panel()
        Me.imgPhoto = New System.Windows.Forms.PictureBox()
        Me.picPaint = New System.Windows.Forms.PictureBox()
        Me.picVideoBar = New System.Windows.Forms.Panel()
        Me.Line3 = New System.Windows.Forms.Label()
        Me.lblDuration = New System.Windows.Forms.Label()
        Me.lblCurrentPosition = New System.Windows.Forms.Label()
        Me.Image6 = New System.Windows.Forms.PictureBox()
        Me.Image5 = New System.Windows.Forms.PictureBox()
        Me.picVideo = New System.Windows.Forms.PictureBox()
        Me.chkAutoRewind = New Aqua.CheckBox()
        Me.sliSound = New Aqua.Slider()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.pgVideo = New Aqua.TimeLine()
        Me.picPage = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.imbNext = New Aqua.PngButton()
        Me.imbPrior = New Aqua.PngButton()
        Me.picImageBar = New System.Windows.Forms.Panel()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.lblFileLength = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.lblFileName = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lblResolution = New System.Windows.Forms.Label()
        Me.picPhoto.SuspendLayout()
        Me.picVideoBar.SuspendLayout()
        Me.picPage.SuspendLayout()
        Me.picImageBar.SuspendLayout()
        Me.SuspendLayout()
        '
        'mpViewerVideo
        '
        Me.mpViewerVideo.Location = New System.Drawing.Point(0, 0)
        Me.mpViewerVideo.Size = New System.Drawing.Size(1024, 670)
        Me.mpViewerVideo.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.mpViewerVideo.AutoRewind = True
        Me.mpViewerVideo.Name = "mpViewerVideo"
        Me.mpViewerVideo.TabIndex = 3
        Me.mpViewerVideo.Visible = False
        Me.mpViewerVideo.Enabled = True
        '
        'imgPlay
        '
        Me.imgPlay.Location = New System.Drawing.Point(970, 8)
        Me.imgPlay.Size = New System.Drawing.Size(48, 48)
        Me.imgPlay.Name = "imgPlay"
        Me.imgPlay.BackColor = System.Drawing.Color.Transparent
        Me.imgPlay.Image = CType(resources.GetObject("imgPlay.Image"), System.Drawing.Image)
        Me.imgPlay.Visible = False
        '
        'imgPause
        '
        Me.imgPause.Location = New System.Drawing.Point(900, 10)
        Me.imgPause.Size = New System.Drawing.Size(48, 48)
        Me.imgPause.Name = "imgPause"
        Me.imgPause.BackColor = System.Drawing.Color.Transparent
        Me.imgPause.Image = CType(resources.GetObject("imgPause.Image"), System.Drawing.Image)
        Me.imgPause.Visible = False
        '
        'pgImage
        '
        Me.pgImage.Location = New System.Drawing.Point(0, 678)
        Me.pgImage.Size = New System.Drawing.Size(1024, 16)
        Me.pgImage.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pgImage.Visible = False
        Me.pgImage.Name = "pgImage"
        Me.pgImage.TabIndex = 16
        Me.pgImage.Maximum = 100
        '
        'imgPhoto
        '
        Me.imgPhoto.Location = New System.Drawing.Point(34, 48)
        Me.imgPhoto.Size = New System.Drawing.Size(315, 207)
        Me.imgPhoto.Name = "imgPhoto"
        Me.imgPhoto.TabIndex = 1
        Me.imgPhoto.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.imgPhoto.BackColor = System.Drawing.Color.Black
        Me.imgPhoto.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'picPaint
        '
        Me.picPaint.Location = New System.Drawing.Point(0, 0)
        Me.picPaint.Size = New System.Drawing.Size(87, 41)
        Me.picPaint.Name = "picPaint"
        Me.picPaint.TabIndex = 2
        Me.picPaint.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPaint.BackColor = System.Drawing.SystemColors.Window
        Me.picPaint.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPaint.Visible = False
        '
        'picPhoto
        '
        Me.picPhoto.Location = New System.Drawing.Point(0, 0)
        Me.picPhoto.Size = New System.Drawing.Size(1024, 693)
        Me.picPhoto.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picPhoto.Name = "picPhoto"
        Me.picPhoto.TabIndex = 0
        Me.picPhoto.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPhoto.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.picPhoto.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPhoto.Controls.Add(Me.imgPhoto)
        Me.picPhoto.Controls.Add(Me.picPaint)
        '
        'Line3
        '
        Me.Line3.AutoSize = False
        Me.Line3.Location = New System.Drawing.Point(118, 0)
        Me.Line3.Size = New System.Drawing.Size(1, 74)
        Me.Line3.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Line3.Name = "Line3"
        '
        'lblDuration
        '
        Me.lblDuration.Location = New System.Drawing.Point(4, 10)
        Me.lblDuration.Size = New System.Drawing.Size(72, 21)
        Me.lblDuration.Name = "lblDuration"
        Me.lblDuration.TabIndex = 7
        Me.lblDuration.BackColor = System.Drawing.Color.Transparent
        Me.lblDuration.AutoSize = True
        Me.lblDuration.Text = "Label5"
        Me.lblDuration.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.lblDuration.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblCurrentPosition
        '
        Me.lblCurrentPosition.Location = New System.Drawing.Point(4, 40)
        Me.lblCurrentPosition.Size = New System.Drawing.Size(72, 21)
        Me.lblCurrentPosition.Name = "lblCurrentPosition"
        Me.lblCurrentPosition.TabIndex = 8
        Me.lblCurrentPosition.BackColor = System.Drawing.Color.Transparent
        Me.lblCurrentPosition.AutoSize = True
        Me.lblCurrentPosition.Text = "Label5"
        Me.lblCurrentPosition.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.lblCurrentPosition.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Image6
        '
        Me.Image6.Location = New System.Drawing.Point(366, 28)
        Me.Image6.Size = New System.Drawing.Size(16, 16)
        Me.Image6.Name = "Image6"
        Me.Image6.BackColor = System.Drawing.Color.Transparent
        Me.Image6.Image = CType(resources.GetObject("Image6.Image"), System.Drawing.Image)
        '
        'Image5
        '
        Me.Image5.Location = New System.Drawing.Point(180, 28)
        Me.Image5.Size = New System.Drawing.Size(16, 16)
        Me.Image5.Name = "Image5"
        Me.Image5.BackColor = System.Drawing.Color.Transparent
        Me.Image5.Image = CType(resources.GetObject("Image5.Image"), System.Drawing.Image)
        '
        'picVideo
        '
        Me.picVideo.Location = New System.Drawing.Point(124, 12)
        Me.picVideo.Size = New System.Drawing.Size(48, 48)
        Me.picVideo.Name = "picVideo"
        Me.picVideo.TabIndex = 5
        Me.picVideo.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picVideo.Image = CType(resources.GetObject("picVideo.Image"), System.Drawing.Image)
        Me.picVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.picVideo.BackColor = System.Drawing.SystemColors.Window
        Me.picVideo.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'chkAutoRewind
        '
        Me.chkAutoRewind.Location = New System.Drawing.Point(396, 26)
        Me.chkAutoRewind.Checked = True
        Me.chkAutoRewind.Name = "chkAutoRewind"
        Me.chkAutoRewind.TabIndex = 9
        Me.chkAutoRewind.Enabled = False
        Me.chkAutoRewind.Checked = True
        Me.chkAutoRewind.TextValue = "重複撥放"
        Me.chkAutoRewind.TextGap = 6
        Me.chkAutoRewind.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.chkAutoRewind.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'sliSound
        '
        Me.sliSound.Location = New System.Drawing.Point(196, 20)
        Me.sliSound.Size = New System.Drawing.Size(163, 33)
        Me.sliSound.Name = "sliSound"
        Me.sliSound.TabIndex = 11
        Me.sliSound.Maximum = 100
        Me.sliSound.Value = 100
        '
        'picVideoBar
        '
        Me.picVideoBar.Location = New System.Drawing.Point(170, 694)
        Me.picVideoBar.Size = New System.Drawing.Size(854, 73)
        Me.picVideoBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picVideoBar.Visible = False
        Me.picVideoBar.Name = "picVideoBar"
        Me.picVideoBar.TabIndex = 4
        Me.picVideoBar.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picVideoBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.picVideoBar.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picVideoBar.Controls.Add(Me.Image6)
        Me.picVideoBar.Controls.Add(Me.Image5)
        Me.picVideoBar.Controls.Add(Me.picVideo)
        Me.picVideoBar.Controls.Add(Me.chkAutoRewind)
        Me.picVideoBar.Controls.Add(Me.sliSound)
        Me.picVideoBar.Controls.Add(Me.Line3)
        Me.picVideoBar.Controls.Add(Me.lblDuration)
        Me.picVideoBar.Controls.Add(Me.lblCurrentPosition)
        '
        'Timer1
        '
        Me.Timer1.Enabled = False
        Me.Timer1.Interval = 100
        '
        'pgVideo
        '
        Me.pgVideo.Location = New System.Drawing.Point(0, 671)
        Me.pgVideo.Size = New System.Drawing.Size(1024, 23)
        Me.pgVideo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pgVideo.Visible = False
        Me.pgVideo.Name = "pgVideo"
        Me.pgVideo.TabIndex = 6
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(4, 44)
        Me.Label2.Size = New System.Drawing.Size(151, 21)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 15
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "上一張/下一張"
        Me.Label2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label2.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(164, 0)
        Me.Line1.Size = New System.Drawing.Size(1, 72)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'imbNext
        '
        Me.imbNext.Location = New System.Drawing.Point(82, 8)
        Me.imbNext.Size = New System.Drawing.Size(36, 30)
        Me.imbNext.Name = "imbNext"
        Me.imbNext.TabIndex = 14
        Me.imbNext.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imbNext.Image = CType(resources.GetObject("imbNext.Image"), System.Drawing.Image)
        Me.imbNext.HoverZoom = 0!
        Me.imbNext.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imbPrior
        '
        Me.imbPrior.Location = New System.Drawing.Point(48, 8)
        Me.imbPrior.Size = New System.Drawing.Size(36, 30)
        Me.imbPrior.Name = "imbPrior"
        Me.imbPrior.TabIndex = 13
        Me.imbPrior.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imbPrior.Image = CType(resources.GetObject("imbPrior.Image"), System.Drawing.Image)
        Me.imbPrior.HoverZoom = 0!
        Me.imbPrior.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'picPage
        '
        Me.picPage.Location = New System.Drawing.Point(0, 694)
        Me.picPage.Size = New System.Drawing.Size(169, 73)
        Me.picPage.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.picPage.Visible = False
        Me.picPage.Name = "picPage"
        Me.picPage.TabIndex = 12
        Me.picPage.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPage.BackColor = System.Drawing.SystemColors.Window
        Me.picPage.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPage.Controls.Add(Me.imbNext)
        Me.picPage.Controls.Add(Me.imbPrior)
        Me.picPage.Controls.Add(Me.Label2)
        Me.picPage.Controls.Add(Me.Line1)
        '
        'Label9
        '
        Me.Label9.Location = New System.Drawing.Point(10, 38)
        Me.Label9.Size = New System.Drawing.Size(70, 21)
        Me.Label9.Name = "Label9"
        Me.Label9.TabIndex = 18
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.AutoSize = True
        Me.Label9.Text = "大  小"
        Me.Label9.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label9.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblFileLength
        '
        Me.lblFileLength.Location = New System.Drawing.Point(90, 38)
        Me.lblFileLength.Size = New System.Drawing.Size(61, 21)
        Me.lblFileLength.Name = "lblFileLength"
        Me.lblFileLength.TabIndex = 19
        Me.lblFileLength.BackColor = System.Drawing.Color.Transparent
        Me.lblFileLength.AutoSize = True
        Me.lblFileLength.Text = "Label3"
        Me.lblFileLength.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblFileLength.Font = New System.Drawing.Font("新細明體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(280, 8)
        Me.Label7.Size = New System.Drawing.Size(70, 21)
        Me.Label7.Name = "Label7"
        Me.Label7.TabIndex = 20
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.AutoSize = True
        Me.Label7.Text = "檔  名"
        Me.Label7.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label7.Visible = False
        Me.Label7.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblFileName
        '
        Me.lblFileName.Location = New System.Drawing.Point(360, 8)
        Me.lblFileName.Size = New System.Drawing.Size(61, 21)
        Me.lblFileName.Name = "lblFileName"
        Me.lblFileName.TabIndex = 21
        Me.lblFileName.BackColor = System.Drawing.Color.Transparent
        Me.lblFileName.AutoSize = True
        Me.lblFileName.Text = "Label3"
        Me.lblFileName.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblFileName.Visible = False
        Me.lblFileName.Font = New System.Drawing.Font("新細明體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(10, 8)
        Me.Label4.Size = New System.Drawing.Size(69, 21)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 22
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "解析度"
        Me.Label4.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label4.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblResolution
        '
        Me.lblResolution.Location = New System.Drawing.Point(88, 8)
        Me.lblResolution.Size = New System.Drawing.Size(61, 21)
        Me.lblResolution.Name = "lblResolution"
        Me.lblResolution.TabIndex = 23
        Me.lblResolution.BackColor = System.Drawing.Color.Transparent
        Me.lblResolution.AutoSize = True
        Me.lblResolution.Text = "Label3"
        Me.lblResolution.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblResolution.Font = New System.Drawing.Font("新細明體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'picImageBar
        '
        Me.picImageBar.Location = New System.Drawing.Point(170, 694)
        Me.picImageBar.Size = New System.Drawing.Size(854, 73)
        Me.picImageBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picImageBar.Visible = False
        Me.picImageBar.Name = "picImageBar"
        Me.picImageBar.TabIndex = 17
        Me.picImageBar.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picImageBar.BackColor = System.Drawing.SystemColors.Window
        Me.picImageBar.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picImageBar.Controls.Add(Me.Label9)
        Me.picImageBar.Controls.Add(Me.lblFileLength)
        Me.picImageBar.Controls.Add(Me.Label7)
        Me.picImageBar.Controls.Add(Me.lblFileName)
        Me.picImageBar.Controls.Add(Me.Label4)
        Me.picImageBar.Controls.Add(Me.lblResolution)
        '
        'frmViewerSmall
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1024, 768)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.KeyPreview = True
        Me.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation
        Me.BackColor = System.Drawing.Color.Black
        Me.Text = "Form1"
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.Name = "frmViewerSmall"
        Me.Controls.Add(Me.mpViewerVideo)
        Me.Controls.Add(Me.imgPlay)
        Me.Controls.Add(Me.imgPause)
        Me.Controls.Add(Me.pgImage)
        Me.Controls.Add(Me.picPhoto)
        Me.Controls.Add(Me.picVideoBar)
        Me.Controls.Add(Me.pgVideo)
        Me.Controls.Add(Me.picPage)
        Me.Controls.Add(Me.picImageBar)
        Me.picImageBar.ResumeLayout(False)
        Me.picPage.ResumeLayout(False)
        Me.picVideoBar.ResumeLayout(False)
        Me.picPhoto.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents mpViewerVideo As Aqua.MediaViewerControl
    Friend WithEvents imgPlay As System.Windows.Forms.PictureBox
    Friend WithEvents imgPause As System.Windows.Forms.PictureBox
    Friend WithEvents pgImage As Aqua.ProgressBar
    Friend WithEvents picPhoto As System.Windows.Forms.Panel
    Friend WithEvents imgPhoto As System.Windows.Forms.PictureBox
    Friend WithEvents picPaint As System.Windows.Forms.PictureBox
    Friend WithEvents picVideoBar As System.Windows.Forms.Panel
    Friend WithEvents Line3 As System.Windows.Forms.Label
    Friend WithEvents lblDuration As System.Windows.Forms.Label
    Friend WithEvents lblCurrentPosition As System.Windows.Forms.Label
    Friend WithEvents Image6 As System.Windows.Forms.PictureBox
    Friend WithEvents Image5 As System.Windows.Forms.PictureBox
    Friend WithEvents picVideo As System.Windows.Forms.PictureBox
    Friend WithEvents chkAutoRewind As Aqua.CheckBox
    Friend WithEvents sliSound As Aqua.Slider
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents pgVideo As Aqua.TimeLine
    Friend WithEvents picPage As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents imbNext As Aqua.PngButton
    Friend WithEvents imbPrior As Aqua.PngButton
    Friend WithEvents picImageBar As System.Windows.Forms.Panel
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents lblFileLength As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents lblFileName As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents lblResolution As System.Windows.Forms.Label
End Class
