<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPhotoIndex
    Inherits Aqua.AquaForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPhotoIndex))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblPage = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.frmClass = New Aqua.Panel()
        Me.tvList = New System.Windows.Forms.TreeView()
        Me.chkPrintNote_0 = New Aqua.CheckBox()
        Me.chkPrintNote_1 = New Aqua.CheckBox()
        Me.Slider1 = New Aqua.Slider()
        Me.butExit = New Aqua.ThinButton()
        Me.butPreview = New Aqua.ThinButton()
        Me.butPrint = New Aqua.ThinButton()
        Me.butSave = New Aqua.ThinButton()
        Me.UpDown1 = New Aqua.UpDown()
        Me.picContainer = New Aqua.PicturePanel()
        Me.picPaper = New System.Windows.Forms.PictureBox()
        Me.picShadow = New System.Windows.Forms.PictureBox()
        Me.cboPrinter = New Aqua.DropDownList()
        Me.butAdviance = New Aqua.ThinButton()
        Me.butPaper = New Aqua.ThinButton()
        Me.txtLimit = New Aqua.TextBox()
        Me.picCanvas_0 = New System.Windows.Forms.PictureBox()
        Me.frmClass.SuspendLayout()
        Me.picContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(34, 724)
        Me.Label1.Size = New System.Drawing.Size(60, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 10
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "欄數："
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'lblPage
        '
        Me.lblPage.Location = New System.Drawing.Point(916, 790)
        Me.lblPage.Size = New System.Drawing.Size(90, 19)
        Me.lblPage.Name = "lblPage"
        Me.lblPage.TabIndex = 16
        Me.lblPage.BackColor = System.Drawing.Color.Transparent
        Me.lblPage.AutoSize = True
        Me.lblPage.Text = "0 of 0 張"
        Me.lblPage.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(14, 686)
        Me.Label2.Size = New System.Drawing.Size(80, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 22
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "印表機："
        Me.Label2.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'tvList
        '
        Me.tvList.Location = New System.Drawing.Point(4, 4)
        Me.tvList.Size = New System.Drawing.Size(489, 630)
        Me.tvList.Name = "tvList"
        Me.tvList.TabIndex = 9
        Me.tvList.LabelEdit = False
        Me.tvList.TabStop = False
        Me.tvList.HideSelection = False
        Me.tvList.FullRowSelect = True
        Me.tvList.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmClass
        '
        Me.frmClass.Location = New System.Drawing.Point(14, 36)
        Me.frmClass.Size = New System.Drawing.Size(495, 637)
        Me.frmClass.Name = "frmClass"
        Me.frmClass.TabIndex = 8
        Me.frmClass.TabStop = False
        Me.frmClass.ForeColor = System.Drawing.SystemColors.ControlText
        Me.frmClass.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.frmClass.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.frmClass.PanelStyle = Aqua.PanelStyleMode.Container
        Me.frmClass.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.frmClass.Controls.Add(Me.tvList)
        '
        'chkPrintNote_0
        '
        Me.chkPrintNote_0.Location = New System.Drawing.Point(102, 766)
        Me.chkPrintNote_0.Name = "chkPrintNote_0"
        Me.chkPrintNote_0.TabIndex = 13
        Me.chkPrintNote_0.TabStop = False
        Me.chkPrintNote_0.Checked = True
        Me.chkPrintNote_0.TextValue = "列印印圖時間"
        Me.chkPrintNote_0.TextGap = 10
        Me.chkPrintNote_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'chkPrintNote_1
        '
        Me.chkPrintNote_1.Location = New System.Drawing.Point(266, 766)
        Me.chkPrintNote_1.Name = "chkPrintNote_1"
        Me.chkPrintNote_1.TabIndex = 14
        Me.chkPrintNote_1.TabStop = False
        Me.chkPrintNote_1.Checked = True
        Me.chkPrintNote_1.TextValue = "列印照片張數"
        Me.chkPrintNote_1.TextGap = 12
        Me.chkPrintNote_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Slider1
        '
        Me.Slider1.Location = New System.Drawing.Point(98, 712)
        Me.Slider1.Size = New System.Drawing.Size(343, 41)
        Me.Slider1.Name = "Slider1"
        Me.Slider1.TabIndex = 0
        Me.Slider1.TickStyle = Aqua.SliderTickMode.BottomRight
        Me.Slider1.Maximum = 16
        Me.Slider1.Minimum = 6
        Me.Slider1.Value = 10
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(848, 816)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 6
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Text = "結束"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butPreview
        '
        Me.butPreview.Location = New System.Drawing.Point(488, 816)
        Me.butPreview.Size = New System.Drawing.Size(103, 27)
        Me.butPreview.Name = "butPreview"
        Me.butPreview.TabIndex = 3
        Me.butPreview.ForeColor = System.Drawing.Color.Black
        Me.butPreview.Text = "預覽"
        Me.butPreview.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butPrint
        '
        Me.butPrint.Location = New System.Drawing.Point(608, 816)
        Me.butPrint.Size = New System.Drawing.Size(103, 27)
        Me.butPrint.Enabled = False
        Me.butPrint.Name = "butPrint"
        Me.butPrint.TabIndex = 4
        Me.butPrint.ForeColor = System.Drawing.Color.Black
        Me.butPrint.Text = "列印"
        Me.butPrint.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butSave
        '
        Me.butSave.Location = New System.Drawing.Point(728, 816)
        Me.butSave.Size = New System.Drawing.Size(103, 27)
        Me.butSave.Enabled = False
        Me.butSave.Name = "butSave"
        Me.butSave.TabIndex = 5
        Me.butSave.ForeColor = System.Drawing.Color.Black
        Me.butSave.Text = "存檔"
        Me.butSave.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'UpDown1
        '
        Me.UpDown1.Location = New System.Drawing.Point(1040, 790)
        Me.UpDown1.Size = New System.Drawing.Size(39, 21)
        Me.UpDown1.Name = "UpDown1"
        Me.UpDown1.TabIndex = 15
        Me.UpDown1.TabStop = False
        Me.UpDown1.Enabled = False
        Me.UpDown1.Minimum = 1
        Me.UpDown1.Maximum = 4
        Me.UpDown1.Orientation = Aqua.OrientationMode.Horizontal
        '
        'picPaper
        '
        Me.picPaper.Location = New System.Drawing.Point(140, 14)
        Me.picPaper.Size = New System.Drawing.Size(386, 574)
        Me.picPaper.Name = "picPaper"
        Me.picPaper.TabIndex = 19
        Me.picPaper.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPaper.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.picPaper.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPaper.TabStop = False
        '
        'picShadow
        '
        Me.picShadow.Location = New System.Drawing.Point(120, 88)
        Me.picShadow.Size = New System.Drawing.Size(97, 125)
        Me.picShadow.Name = "picShadow"
        Me.picShadow.TabIndex = 20
        Me.picShadow.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picShadow.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.picShadow.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picShadow.TabStop = False
        '
        'picContainer
        '
        Me.picContainer.Location = New System.Drawing.Point(520, 36)
        Me.picContainer.Size = New System.Drawing.Size(561, 747)
        Me.picContainer.Name = "picContainer"
        Me.picContainer.TabIndex = 18
        Me.picContainer.BackColor = System.Drawing.SystemColors.Window
        Me.picContainer.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picContainer.Image = CType(resources.GetObject("picContainer.Image"), System.Drawing.Image)
        Me.picContainer.SizeMode = Aqua.ImageSizeMode.Fill
        Me.picContainer.Transparency = True
        Me.picContainer.TransparencyKey = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.picContainer.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.picContainer.Controls.Add(Me.picPaper)
        Me.picContainer.Controls.Add(Me.picShadow)
        '
        'cboPrinter
        '
        Me.cboPrinter.Location = New System.Drawing.Point(96, 684)
        Me.cboPrinter.Size = New System.Drawing.Size(383, 23)
        Me.cboPrinter.Name = "cboPrinter"
        Me.cboPrinter.TabIndex = 21
        Me.cboPrinter.TabStop = False
        Me.cboPrinter.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cboPrinter.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butAdviance
        '
        Me.butAdviance.Location = New System.Drawing.Point(316, 816)
        Me.butAdviance.Size = New System.Drawing.Size(155, 27)
        Me.butAdviance.Name = "butAdviance"
        Me.butAdviance.TabIndex = 2
        Me.butAdviance.ForeColor = System.Drawing.Color.Black
        Me.butAdviance.Text = "進階選項"
        Me.butAdviance.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butPaper
        '
        Me.butPaper.Location = New System.Drawing.Point(144, 816)
        Me.butPaper.Size = New System.Drawing.Size(155, 27)
        Me.butPaper.Name = "butPaper"
        Me.butPaper.TabIndex = 1
        Me.butPaper.ForeColor = System.Drawing.Color.Black
        Me.butPaper.Text = "紙張設定"
        Me.butPaper.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtLimit
        '
        Me.txtLimit.Location = New System.Drawing.Point(448, 720)
        Me.txtLimit.Size = New System.Drawing.Size(33, 25)
        Me.txtLimit.Name = "txtLimit"
        Me.txtLimit.TabIndex = 23
        Me.txtLimit.TabStop = False
        Me.txtLimit.Enabled = False
        Me.txtLimit.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtLimit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtLimit.MaxLength = 0
        Me.txtLimit.Alignment = Aqua.AlignmentConstants.Center
        Me.txtLimit.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtLimit.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtLimit.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'picCanvas_0
        '
        Me.picCanvas_0.Location = New System.Drawing.Point(1082, 290)
        Me.picCanvas_0.Size = New System.Drawing.Size(40, 154)
        Me.picCanvas_0.Visible = False
        Me.picCanvas_0.Name = "picCanvas_0"
        Me.picCanvas_0.TabIndex = 17
        Me.picCanvas_0.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picCanvas_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.picCanvas_0.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picCanvas_0.TabStop = False
        Me.picCanvas_0.Visible = False
        '
        'frmPhotoIndex
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1095, 861)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "照片目錄"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmPhotoIndex"
        Me.Controls.Add(Me.frmClass)
        Me.Controls.Add(Me.chkPrintNote_0)
        Me.Controls.Add(Me.chkPrintNote_1)
        Me.Controls.Add(Me.Slider1)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butPreview)
        Me.Controls.Add(Me.butPrint)
        Me.Controls.Add(Me.butSave)
        Me.Controls.Add(Me.UpDown1)
        Me.Controls.Add(Me.picContainer)
        Me.Controls.Add(Me.cboPrinter)
        Me.Controls.Add(Me.butAdviance)
        Me.Controls.Add(Me.butPaper)
        Me.Controls.Add(Me.txtLimit)
        Me.Controls.Add(Me.picCanvas_0)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblPage)
        Me.Controls.Add(Me.Label2)
        Me.picContainer.ResumeLayout(False)
        Me.frmClass.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblPage As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents frmClass As Aqua.Panel
    Friend WithEvents tvList As System.Windows.Forms.TreeView
    Friend WithEvents chkPrintNote_0 As Aqua.CheckBox
    Friend WithEvents chkPrintNote_1 As Aqua.CheckBox
    Friend WithEvents Slider1 As Aqua.Slider
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents butPreview As Aqua.ThinButton
    Friend WithEvents butPrint As Aqua.ThinButton
    Friend WithEvents butSave As Aqua.ThinButton
    Friend WithEvents UpDown1 As Aqua.UpDown
    Friend WithEvents picContainer As Aqua.PicturePanel
    Friend WithEvents picPaper As System.Windows.Forms.PictureBox
    Friend WithEvents picShadow As System.Windows.Forms.PictureBox
    Friend WithEvents cboPrinter As Aqua.DropDownList
    Friend WithEvents butAdviance As Aqua.ThinButton
    Friend WithEvents butPaper As Aqua.ThinButton
    Friend WithEvents txtLimit As Aqua.TextBox
    Friend WithEvents picCanvas_0 As System.Windows.Forms.PictureBox
End Class
