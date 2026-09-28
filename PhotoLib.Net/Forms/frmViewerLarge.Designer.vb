<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmViewerLarge
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmViewerLarge))
        Me.imgUndoEnabled = New System.Windows.Forms.PictureBox()
        Me.imgUndoDisabled = New System.Windows.Forms.PictureBox()
        Me.mpViewerVideo = New Aqua.MediaViewerControl()
        Me.imgPlay = New System.Windows.Forms.PictureBox()
        Me.imgPause = New System.Windows.Forms.PictureBox()
        Me.picImageBar1 = New Aqua.ToolBar()
        Me.picPhoto = New System.Windows.Forms.Panel()
        Me.imgPhoto = New System.Windows.Forms.PictureBox()
        Me.picPaint = New System.Windows.Forms.PictureBox()
        Me.picImageBar2 = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Image1 = New System.Windows.Forms.PictureBox()
        Me.Image2 = New System.Windows.Forms.PictureBox()
        Me.Image3 = New System.Windows.Forms.PictureBox()
        Me.Image4 = New System.Windows.Forms.PictureBox()
        Me.shpZero = New System.Windows.Forms.Label()
        Me.Line2 = New System.Windows.Forms.Label()
        Me.lblUndo = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblSelPoint = New System.Windows.Forms.Label()
        Me.sliContrast = New Aqua.Slider()
        Me.sliBrightness = New Aqua.Slider()
        Me.imgUndo = New System.Windows.Forms.PictureBox()
        Me.pgImage = New Aqua.ProgressBar()
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
        Me.lblResolution = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.lblFileName = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.lblFileLength = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.imbNext = New Aqua.PngButton()
        Me.imbPrior = New Aqua.PngButton()
        Me.picPhoto.SuspendLayout()
        Me.picImageBar2.SuspendLayout()
        Me.picVideoBar.SuspendLayout()
        Me.picPage.SuspendLayout()
        Me.SuspendLayout()
        '
        'imgUndoEnabled
        '
        Me.imgUndoEnabled.Location = New System.Drawing.Point(976, 68)
        Me.imgUndoEnabled.Size = New System.Drawing.Size(32, 32)
        Me.imgUndoEnabled.Name = "imgUndoEnabled"
        Me.imgUndoEnabled.BackColor = System.Drawing.Color.Transparent
        Me.imgUndoEnabled.Image = CType(resources.GetObject("imgUndoEnabled.Image"), System.Drawing.Image)
        Me.imgUndoEnabled.Visible = False
        '
        'imgUndoDisabled
        '
        Me.imgUndoDisabled.Location = New System.Drawing.Point(908, 72)
        Me.imgUndoDisabled.Size = New System.Drawing.Size(32, 32)
        Me.imgUndoDisabled.Name = "imgUndoDisabled"
        Me.imgUndoDisabled.BackColor = System.Drawing.Color.Transparent
        Me.imgUndoDisabled.Image = CType(resources.GetObject("imgUndoDisabled.Image"), System.Drawing.Image)
        Me.imgUndoDisabled.Visible = False
        '
        'mpViewerVideo
        '
        Me.mpViewerVideo.Location = New System.Drawing.Point(0, 0)
        Me.mpViewerVideo.Size = New System.Drawing.Size(1280, 926)
        Me.mpViewerVideo.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.mpViewerVideo.AutoRewind = True
        Me.mpViewerVideo.Name = "mpViewerVideo"
        Me.mpViewerVideo.TabIndex = 9
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
        'picImageBar1
        '
        Me.picImageBar1.Location = New System.Drawing.Point(300, 950)
        Me.picImageBar1.Size = New System.Drawing.Size(671, 73)
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("左轉90°", CType(resources.GetObject("picImageBar1.Icon0"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("右轉90°", CType(resources.GetObject("picImageBar1.Icon1"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("紅眼", CType(resources.GetObject("picImageBar1.Icon2"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("裁切", CType(resources.GetObject("picImageBar1.Icon3"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("泛黃", CType(resources.GetObject("picImageBar1.Icon4"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("黑白", CType(resources.GetObject("picImageBar1.Icon5"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("柔焦", CType(resources.GetObject("picImageBar1.Icon6"), System.Drawing.Image)))
        Me.picImageBar1.Items.Add(New Aqua.ToolBar.ToolBarItem("清晰", CType(resources.GetObject("picImageBar1.Icon7"), System.Drawing.Image)))
        Me.picImageBar1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.picImageBar1.Visible = False
        Me.picImageBar1.Name = "picImageBar1"
        Me.picImageBar1.TabIndex = 4
        Me.picImageBar1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.picImageBar1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgPhoto
        '
        Me.imgPhoto.Location = New System.Drawing.Point(34, 48)
        Me.imgPhoto.Size = New System.Drawing.Size(315, 207)
        Me.imgPhoto.Name = "imgPhoto"
        Me.imgPhoto.TabIndex = 6
        Me.imgPhoto.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.imgPhoto.BackColor = System.Drawing.Color.Black
        Me.imgPhoto.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'picPaint
        '
        Me.picPaint.Location = New System.Drawing.Point(0, 0)
        Me.picPaint.Size = New System.Drawing.Size(87, 41)
        Me.picPaint.Name = "picPaint"
        Me.picPaint.TabIndex = 7
        Me.picPaint.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPaint.BackColor = System.Drawing.SystemColors.Window
        Me.picPaint.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPaint.Visible = False
        '
        'picPhoto
        '
        Me.picPhoto.Location = New System.Drawing.Point(0, 0)
        Me.picPhoto.Size = New System.Drawing.Size(1280, 949)
        Me.picPhoto.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picPhoto.Name = "picPhoto"
        Me.picPhoto.TabIndex = 5
        Me.picPhoto.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPhoto.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.picPhoto.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPhoto.Controls.Add(Me.imgPhoto)
        Me.picPhoto.Controls.Add(Me.picPaint)
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(58, 56)
        Me.Label1.Size = New System.Drawing.Size(89, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 3
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "亮度 / 對比"
        Me.Label1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label1.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Image1
        '
        Me.Image1.Location = New System.Drawing.Point(186, 2)
        Me.Image1.Size = New System.Drawing.Size(19, 19)
        Me.Image1.Name = "Image1"
        Me.Image1.BackColor = System.Drawing.Color.Transparent
        Me.Image1.Image = CType(resources.GetObject("Image1.Image"), System.Drawing.Image)
        '
        'Image2
        '
        Me.Image2.Location = New System.Drawing.Point(2, 2)
        Me.Image2.Size = New System.Drawing.Size(19, 19)
        Me.Image2.Name = "Image2"
        Me.Image2.BackColor = System.Drawing.Color.Transparent
        Me.Image2.Image = CType(resources.GetObject("Image2.Image"), System.Drawing.Image)
        '
        'Image3
        '
        Me.Image3.Location = New System.Drawing.Point(186, 34)
        Me.Image3.Size = New System.Drawing.Size(19, 19)
        Me.Image3.Name = "Image3"
        Me.Image3.BackColor = System.Drawing.Color.Transparent
        Me.Image3.Image = CType(resources.GetObject("Image3.Image"), System.Drawing.Image)
        '
        'Image4
        '
        Me.Image4.Location = New System.Drawing.Point(2, 34)
        Me.Image4.Size = New System.Drawing.Size(19, 19)
        Me.Image4.Name = "Image4"
        Me.Image4.BackColor = System.Drawing.Color.Transparent
        Me.Image4.Image = CType(resources.GetObject("Image4.Image"), System.Drawing.Image)
        '
        'shpZero
        '
        Me.shpZero.Location = New System.Drawing.Point(101, 27)
        Me.shpZero.Size = New System.Drawing.Size(3, 3)
        Me.shpZero.Name = "shpZero"
        Me.shpZero.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.shpZero.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Line2
        '
        Me.Line2.AutoSize = False
        Me.Line2.Location = New System.Drawing.Point(212, 0)
        Me.Line2.Size = New System.Drawing.Size(1, 74)
        Me.Line2.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Line2.Name = "Line2"
        '
        'lblUndo
        '
        Me.lblUndo.Location = New System.Drawing.Point(220, 48)
        Me.lblUndo.Size = New System.Drawing.Size(80, 19)
        Me.lblUndo.Name = "lblUndo"
        Me.lblUndo.TabIndex = 29
        Me.lblUndo.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.lblUndo.BackColor = System.Drawing.SystemColors.Window
        Me.lblUndo.Text = "無法復原"
        Me.lblUndo.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblUndo.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(318, 4)
        Me.Label3.Size = New System.Drawing.Size(48, 16)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 30
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "定位點"
        Me.Label3.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label3.Visible = False
        Me.Label3.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblSelPoint
        '
        Me.lblSelPoint.Location = New System.Drawing.Point(318, 26)
        Me.lblSelPoint.Size = New System.Drawing.Size(42, 19)
        Me.lblSelPoint.Name = "lblSelPoint"
        Me.lblSelPoint.TabIndex = 31
        Me.lblSelPoint.BackColor = System.Drawing.Color.Transparent
        Me.lblSelPoint.AutoSize = True
        Me.lblSelPoint.Text = "Label3"
        Me.lblSelPoint.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblSelPoint.Visible = False
        Me.lblSelPoint.Font = New System.Drawing.Font("Times New Roman", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'sliContrast
        '
        Me.sliContrast.Location = New System.Drawing.Point(22, 32)
        Me.sliContrast.Size = New System.Drawing.Size(161, 25)
        Me.sliContrast.Name = "sliContrast"
        Me.sliContrast.TabIndex = 2
        Me.sliContrast.TickStyle = Aqua.SliderTickMode.TopLeft
        Me.sliContrast.ShowTicks = False
        Me.sliContrast.Maximum = 30
        Me.sliContrast.Minimum = -30
        '
        'sliBrightness
        '
        Me.sliBrightness.Location = New System.Drawing.Point(22, 0)
        Me.sliBrightness.Size = New System.Drawing.Size(161, 25)
        Me.sliBrightness.Name = "sliBrightness"
        Me.sliBrightness.TabIndex = 1
        Me.sliBrightness.TickStyle = Aqua.SliderTickMode.BottomRight
        Me.sliBrightness.ShowTicks = False
        Me.sliBrightness.Maximum = 30
        Me.sliBrightness.Minimum = -30
        '
        'imgUndo
        '
        Me.imgUndo.Location = New System.Drawing.Point(238, 8)
        Me.imgUndo.Size = New System.Drawing.Size(32, 32)
        Me.imgUndo.Name = "imgUndo"
        Me.imgUndo.TabIndex = 28
        Me.imgUndo.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.imgUndo.Image = CType(resources.GetObject("imgUndo.Image"), System.Drawing.Image)
        Me.imgUndo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.imgUndo.BackColor = System.Drawing.SystemColors.Window
        Me.imgUndo.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'picImageBar2
        '
        Me.picImageBar2.Location = New System.Drawing.Point(972, 950)
        Me.picImageBar2.Size = New System.Drawing.Size(397, 73)
        Me.picImageBar2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.picImageBar2.Visible = False
        Me.picImageBar2.Name = "picImageBar2"
        Me.picImageBar2.TabIndex = 0
        Me.picImageBar2.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picImageBar2.BackColor = System.Drawing.SystemColors.Window
        Me.picImageBar2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picImageBar2.Controls.Add(Me.Image1)
        Me.picImageBar2.Controls.Add(Me.Image2)
        Me.picImageBar2.Controls.Add(Me.Image3)
        Me.picImageBar2.Controls.Add(Me.Image4)
        Me.picImageBar2.Controls.Add(Me.sliContrast)
        Me.picImageBar2.Controls.Add(Me.sliBrightness)
        Me.picImageBar2.Controls.Add(Me.imgUndo)
        Me.picImageBar2.Controls.Add(Me.Label1)
        Me.picImageBar2.Controls.Add(Me.shpZero)
        Me.picImageBar2.Controls.Add(Me.Line2)
        Me.picImageBar2.Controls.Add(Me.lblUndo)
        Me.picImageBar2.Controls.Add(Me.Label3)
        Me.picImageBar2.Controls.Add(Me.lblSelPoint)
        '
        'pgImage
        '
        Me.pgImage.Location = New System.Drawing.Point(0, 934)
        Me.pgImage.Size = New System.Drawing.Size(1280, 16)
        Me.pgImage.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pgImage.Visible = False
        Me.pgImage.Name = "pgImage"
        Me.pgImage.TabIndex = 8
        Me.pgImage.Maximum = 100
        '
        'Line3
        '
        Me.Line3.AutoSize = False
        Me.Line3.Location = New System.Drawing.Point(208, 0)
        Me.Line3.Size = New System.Drawing.Size(1, 74)
        Me.Line3.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Line3.Name = "Line3"
        '
        'lblDuration
        '
        Me.lblDuration.Location = New System.Drawing.Point(14, 4)
        Me.lblDuration.Size = New System.Drawing.Size(48, 16)
        Me.lblDuration.Name = "lblDuration"
        Me.lblDuration.TabIndex = 13
        Me.lblDuration.BackColor = System.Drawing.Color.Transparent
        Me.lblDuration.AutoSize = True
        Me.lblDuration.Text = "Label5"
        Me.lblDuration.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.lblDuration.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblCurrentPosition
        '
        Me.lblCurrentPosition.Location = New System.Drawing.Point(14, 28)
        Me.lblCurrentPosition.Size = New System.Drawing.Size(48, 16)
        Me.lblCurrentPosition.Name = "lblCurrentPosition"
        Me.lblCurrentPosition.TabIndex = 14
        Me.lblCurrentPosition.BackColor = System.Drawing.Color.Transparent
        Me.lblCurrentPosition.AutoSize = True
        Me.lblCurrentPosition.Text = "Label5"
        Me.lblCurrentPosition.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.lblCurrentPosition.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Image6
        '
        Me.Image6.Location = New System.Drawing.Point(484, 30)
        Me.Image6.Size = New System.Drawing.Size(16, 16)
        Me.Image6.Name = "Image6"
        Me.Image6.BackColor = System.Drawing.Color.Transparent
        Me.Image6.Image = CType(resources.GetObject("Image6.Image"), System.Drawing.Image)
        '
        'Image5
        '
        Me.Image5.Location = New System.Drawing.Point(298, 30)
        Me.Image5.Size = New System.Drawing.Size(16, 16)
        Me.Image5.Name = "Image5"
        Me.Image5.BackColor = System.Drawing.Color.Transparent
        Me.Image5.Image = CType(resources.GetObject("Image5.Image"), System.Drawing.Image)
        '
        'picVideo
        '
        Me.picVideo.Location = New System.Drawing.Point(226, 14)
        Me.picVideo.Size = New System.Drawing.Size(48, 48)
        Me.picVideo.Name = "picVideo"
        Me.picVideo.TabIndex = 11
        Me.picVideo.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picVideo.Image = CType(resources.GetObject("picVideo.Image"), System.Drawing.Image)
        Me.picVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.picVideo.BackColor = System.Drawing.SystemColors.Window
        Me.picVideo.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'chkAutoRewind
        '
        Me.chkAutoRewind.Location = New System.Drawing.Point(14, 52)
        Me.chkAutoRewind.Checked = True
        Me.chkAutoRewind.Name = "chkAutoRewind"
        Me.chkAutoRewind.TabIndex = 15
        Me.chkAutoRewind.Enabled = False
        Me.chkAutoRewind.Checked = True
        Me.chkAutoRewind.TextValue = "重複撥放"
        Me.chkAutoRewind.TextGap = 6
        Me.chkAutoRewind.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.chkAutoRewind.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'sliSound
        '
        Me.sliSound.Location = New System.Drawing.Point(316, 22)
        Me.sliSound.Size = New System.Drawing.Size(163, 33)
        Me.sliSound.Name = "sliSound"
        Me.sliSound.TabIndex = 17
        Me.sliSound.Maximum = 100
        Me.sliSound.Value = 100
        '
        'picVideoBar
        '
        Me.picVideoBar.Location = New System.Drawing.Point(300, 950)
        Me.picVideoBar.Size = New System.Drawing.Size(980, 73)
        Me.picVideoBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picVideoBar.Visible = False
        Me.picVideoBar.Name = "picVideoBar"
        Me.picVideoBar.TabIndex = 10
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
        Me.pgVideo.Location = New System.Drawing.Point(0, 927)
        Me.pgVideo.Size = New System.Drawing.Size(1280, 23)
        Me.pgVideo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pgVideo.Visible = False
        Me.pgVideo.Name = "pgVideo"
        Me.pgVideo.TabIndex = 12
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(2, 52)
        Me.Label2.Size = New System.Drawing.Size(105, 16)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 21
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "上一張/下一張"
        Me.Label2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label2.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblResolution
        '
        Me.lblResolution.Location = New System.Drawing.Point(174, 28)
        Me.lblResolution.Size = New System.Drawing.Size(118, 19)
        Me.lblResolution.Name = "lblResolution"
        Me.lblResolution.TabIndex = 22
        Me.lblResolution.BackColor = System.Drawing.Color.Transparent
        Me.lblResolution.AutoSize = True
        Me.lblResolution.Text = "Label3"
        Me.lblResolution.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblResolution.Font = New System.Drawing.Font("Times New Roman", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(120, 28)
        Me.Label4.Size = New System.Drawing.Size(48, 16)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 23
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "解析度"
        Me.Label4.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label4.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(110, 2)
        Me.Line1.Size = New System.Drawing.Size(1, 68)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'lblFileName
        '
        Me.lblFileName.Location = New System.Drawing.Point(174, 4)
        Me.lblFileName.Size = New System.Drawing.Size(118, 19)
        Me.lblFileName.Name = "lblFileName"
        Me.lblFileName.TabIndex = 24
        Me.lblFileName.BackColor = System.Drawing.Color.Transparent
        Me.lblFileName.AutoSize = True
        Me.lblFileName.Text = "Label3"
        Me.lblFileName.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblFileName.Font = New System.Drawing.Font("Times New Roman", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(120, 4)
        Me.Label7.Size = New System.Drawing.Size(48, 16)
        Me.Label7.Name = "Label7"
        Me.Label7.TabIndex = 25
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.AutoSize = True
        Me.Label7.Text = "檔  名"
        Me.Label7.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label7.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblFileLength
        '
        Me.lblFileLength.Location = New System.Drawing.Point(174, 52)
        Me.lblFileLength.Size = New System.Drawing.Size(118, 19)
        Me.lblFileLength.Name = "lblFileLength"
        Me.lblFileLength.TabIndex = 26
        Me.lblFileLength.BackColor = System.Drawing.Color.Transparent
        Me.lblFileLength.AutoSize = True
        Me.lblFileLength.Text = "Label3"
        Me.lblFileLength.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblFileLength.Font = New System.Drawing.Font("Times New Roman", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'Label9
        '
        Me.Label9.Location = New System.Drawing.Point(120, 52)
        Me.Label9.Size = New System.Drawing.Size(48, 16)
        Me.Label9.Name = "Label9"
        Me.Label9.TabIndex = 27
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.AutoSize = True
        Me.Label9.Text = "大  小"
        Me.Label9.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label9.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imbNext
        '
        Me.imbNext.Location = New System.Drawing.Point(54, 12)
        Me.imbNext.Size = New System.Drawing.Size(36, 30)
        Me.imbNext.Name = "imbNext"
        Me.imbNext.TabIndex = 20
        Me.imbNext.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imbNext.Image = CType(resources.GetObject("imbNext.Image"), System.Drawing.Image)
        Me.imbNext.HoverZoom = 0!
        Me.imbNext.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imbPrior
        '
        Me.imbPrior.Location = New System.Drawing.Point(20, 12)
        Me.imbPrior.Size = New System.Drawing.Size(36, 30)
        Me.imbPrior.Name = "imbPrior"
        Me.imbPrior.TabIndex = 19
        Me.imbPrior.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imbPrior.Image = CType(resources.GetObject("imbPrior.Image"), System.Drawing.Image)
        Me.imbPrior.HoverZoom = 0!
        Me.imbPrior.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'picPage
        '
        Me.picPage.Location = New System.Drawing.Point(0, 950)
        Me.picPage.Size = New System.Drawing.Size(299, 73)
        Me.picPage.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.picPage.Visible = False
        Me.picPage.Name = "picPage"
        Me.picPage.TabIndex = 18
        Me.picPage.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPage.BackColor = System.Drawing.SystemColors.Window
        Me.picPage.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPage.Controls.Add(Me.imbNext)
        Me.picPage.Controls.Add(Me.imbPrior)
        Me.picPage.Controls.Add(Me.Label2)
        Me.picPage.Controls.Add(Me.lblResolution)
        Me.picPage.Controls.Add(Me.Label4)
        Me.picPage.Controls.Add(Me.Line1)
        Me.picPage.Controls.Add(Me.lblFileName)
        Me.picPage.Controls.Add(Me.Label7)
        Me.picPage.Controls.Add(Me.lblFileLength)
        Me.picPage.Controls.Add(Me.Label9)
        '
        'frmViewerLarge
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1280, 1024)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.KeyPreview = True
        Me.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation
        Me.BackColor = System.Drawing.Color.Black
        Me.Text = "Form1"
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.Name = "frmViewerLarge"
        Me.Controls.Add(Me.pgImage)
        Me.Controls.Add(Me.imgUndoEnabled)
        Me.Controls.Add(Me.imgUndoDisabled)
        Me.Controls.Add(Me.mpViewerVideo)
        Me.Controls.Add(Me.imgPlay)
        Me.Controls.Add(Me.imgPause)
        Me.Controls.Add(Me.picImageBar1)
        Me.Controls.Add(Me.picPhoto)
        Me.Controls.Add(Me.picImageBar2)
        Me.Controls.Add(Me.picVideoBar)
        Me.Controls.Add(Me.pgVideo)
        Me.Controls.Add(Me.picPage)
        Me.picPage.ResumeLayout(False)
        Me.picVideoBar.ResumeLayout(False)
        Me.picImageBar2.ResumeLayout(False)
        Me.picPhoto.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents imgUndoEnabled As System.Windows.Forms.PictureBox
    Friend WithEvents imgUndoDisabled As System.Windows.Forms.PictureBox
    Friend WithEvents mpViewerVideo As Aqua.MediaViewerControl
    Friend WithEvents imgPlay As System.Windows.Forms.PictureBox
    Friend WithEvents imgPause As System.Windows.Forms.PictureBox
    Friend WithEvents picImageBar1 As Aqua.ToolBar
    Friend WithEvents picPhoto As System.Windows.Forms.Panel
    Friend WithEvents imgPhoto As System.Windows.Forms.PictureBox
    Friend WithEvents picPaint As System.Windows.Forms.PictureBox
    Friend WithEvents picImageBar2 As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Image1 As System.Windows.Forms.PictureBox
    Friend WithEvents Image2 As System.Windows.Forms.PictureBox
    Friend WithEvents Image3 As System.Windows.Forms.PictureBox
    Friend WithEvents Image4 As System.Windows.Forms.PictureBox
    Friend WithEvents shpZero As System.Windows.Forms.Label
    Friend WithEvents Line2 As System.Windows.Forms.Label
    Friend WithEvents lblUndo As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblSelPoint As System.Windows.Forms.Label
    Friend WithEvents sliContrast As Aqua.Slider
    Friend WithEvents sliBrightness As Aqua.Slider
    Friend WithEvents imgUndo As System.Windows.Forms.PictureBox
    Friend WithEvents pgImage As Aqua.ProgressBar
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
    Friend WithEvents lblResolution As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents lblFileName As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents lblFileLength As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents imbNext As Aqua.PngButton
    Friend WithEvents imbPrior As Aqua.PngButton
End Class
