<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPrint
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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPrint))
        Label1 = New Label()
        Label2 = New Label()
        Label3 = New Label()
        Label7 = New Label()
        picContainer = New Aqua.PicturePanel()
        picShadow = New PictureBox()
        picPaper = New Panel()
        picPicture_0 = New PictureBox()
        PictureBox2 = New Aqua.PicturePanel()
        Slider1 = New Aqua.Slider()
        txtLimit = New Aqua.TextBox()
        Label4 = New Label()
        lblLimitInfo = New Label()
        lblPaper = New Label()
        cboPrinter = New Aqua.DropDownList()
        cboDefault = New Aqua.DropDownList()
        cboType = New Aqua.DropDownList()
        txtPrintCount = New Aqua.TextBox()
        UpDown1 = New Aqua.UpDown()
        chkPrintOuputDateTime = New Aqua.CheckBox()
        butSave = New Aqua.FlashButton()
        butPrint = New Aqua.FlashButton()
        butPaper = New Aqua.FlashButton()
        butExit = New Aqua.FlashButton()
        butAdviance = New Aqua.FlashButton()
        Timer1 = New Timer(components)
        udPage = New Aqua.UpDown()
        txtPage = New Aqua.TextBox()
        MediaList1 = New Aqua.MediaList()
        rdPrintMode_0 = New Aqua.RadioButton()
        rdPrintMode_1 = New Aqua.RadioButton()
        chkPrintExifDate = New Aqua.CheckBox()
        picSave = New PictureBox()
        picContainer.SuspendLayout()
        CType(picShadow, ComponentModel.ISupportInitialize).BeginInit()
        picPaper.SuspendLayout()
        CType(picPicture_0, ComponentModel.ISupportInitialize).BeginInit()
        PictureBox2.SuspendLayout()
        CType(picSave, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.Transparent
        Label1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label1.ForeColor = SystemColors.WindowText
        Label1.Location = New Point(674, 638)
        Label1.Name = "Label1"
        Label1.Size = New Size(89, 19)
        Label1.TabIndex = 8
        Label1.Text = "印表機："
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.BackColor = Color.Transparent
        Label2.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label2.ForeColor = SystemColors.WindowText
        Label2.Location = New Point(694, 668)
        Label2.Name = "Label2"
        Label2.Size = New Size(69, 19)
        Label2.TabIndex = 12
        Label2.Text = "預設："
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.BackColor = Color.Transparent
        Label3.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label3.ForeColor = SystemColors.WindowText
        Label3.Location = New Point(694, 698)
        Label3.Name = "Label3"
        Label3.Size = New Size(69, 19)
        Label3.TabIndex = 14
        Label3.Text = "樣式："
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.BackColor = Color.Transparent
        Label7.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label7.ForeColor = SystemColors.WindowText
        Label7.Location = New Point(1034, 860)
        Label7.Name = "Label7"
        Label7.Size = New Size(109, 19)
        Label7.TabIndex = 18
        Label7.Text = "列印份數："
        ' 
        ' Label6
        ' 
        ' 
        ' Label5
        ' 
        ' 
        ' Label8
        ' 
        ' 
        ' Label9
        ' 
        ' 
        ' picContainer
        ' 
        picContainer.BackColor = SystemColors.Window
        picContainer.Controls.Add(picShadow)
        picContainer.Controls.Add(picPaper)
        picContainer.Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        picContainer.ForeColor = SystemColors.WindowText
        picContainer.Image = CType(resources.GetObject("picContainer.Image"), Image)
        picContainer.Location = New Point(18, 60)
        picContainer.Name = "picContainer"
        picContainer.Size = New Size(621, 859)
        picContainer.SizeMode = Aqua.ImageSizeMode.Fill
        picContainer.TabIndex = 7
        picContainer.Transparency = True
        picContainer.TransparencyKey = Color.FromArgb(CByte(255), CByte(0), CByte(255))
        ' 
        ' picShadow
        ' 
        picShadow.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        picShadow.ForeColor = SystemColors.WindowText
        picShadow.Location = New Point(8, 88)
        picShadow.Name = "picShadow"
        picShadow.Size = New Size(97, 125)
        picShadow.TabIndex = 25
        picShadow.TabStop = False
        ' 
        ' picPaper
        ' 
        picPaper.AllowDrop = True
        picPaper.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        picPaper.Controls.Add(picPicture_0)
        picPaper.Font = New Font("華康細圓體", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        picPaper.ForeColor = SystemColors.WindowText
        picPaper.Location = New Point(28, 14)
        picPaper.Name = "picPaper"
        picPaper.Size = New Size(386, 574)
        picPaper.TabIndex = 24
        ' 
        ' picPicture_0
        ' 
        picPicture_0.BackColor = SystemColors.Window
        picPicture_0.BorderStyle = BorderStyle.Fixed3D
        picPicture_0.ForeColor = SystemColors.WindowText
        picPicture_0.Location = New Point(48, 60)
        picPicture_0.Name = "picPicture_0"
        picPicture_0.Size = New Size(147, 101)
        picPicture_0.TabIndex = 26
        picPicture_0.TabStop = False
        picPicture_0.Visible = False
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = SystemColors.Window
        PictureBox2.Controls.Add(Slider1)
        PictureBox2.Controls.Add(txtLimit)
        PictureBox2.Controls.Add(Label4)
        PictureBox2.Controls.Add(lblLimitInfo)
        PictureBox2.Controls.Add(lblPaper)
        PictureBox2.Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        PictureBox2.ForeColor = SystemColors.WindowText
        PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), Image)
        PictureBox2.Location = New Point(664, 728)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(531, 121)
        PictureBox2.SizeMode = Aqua.ImageSizeMode.Fill
        PictureBox2.TabIndex = 9
        PictureBox2.Transparency = True
        PictureBox2.TransparencyKey = Color.FromArgb(CByte(255), CByte(0), CByte(255))
        ' 
        ' Slider1
        ' 
        Slider1.Location = New Point(94, 10)
        Slider1.Maximum = 8
        Slider1.Minimum = 1
        Slider1.Name = "Slider1"
        Slider1.Size = New Size(371, 35)
        Slider1.SoundFileOfClick = Nothing
        Slider1.SoundFileOfEnterFocus = Nothing
        Slider1.SoundFileOfExitFocus = Nothing
        Slider1.TabIndex = 0
        Slider1.TickStyle = Aqua.SliderTickMode.BottomRight
        Slider1.Value = 2
        ' 
        ' txtLimit
        ' 
        txtLimit.Alignment = Aqua.AlignmentConstants.Center
        txtLimit.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtLimit.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtLimit.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtLimit.Enabled = False
        txtLimit.Font = New Font("Times New Roman", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtLimit.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtLimit.Location = New Point(480, 16)
        txtLimit.Name = "txtLimit"
        txtLimit.PasswordChar = ChrW(0)
        txtLimit.RowActive = True
        txtLimit.SelLength = 0
        txtLimit.SelStart = 0
        txtLimit.SelText = ""
        txtLimit.Size = New Size(33, 25)
        txtLimit.SoundFileOfEnterFocus = ""
        txtLimit.SoundFileOfExitFocus = ""
        txtLimit.TabIndex = 16
        txtLimit.TabStop = False
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.BackColor = Color.Transparent
        Label4.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label4.ForeColor = SystemColors.WindowText
        Label4.Location = New Point(30, 18)
        Label4.Name = "Label4"
        Label4.Size = New Size(69, 19)
        Label4.TabIndex = 15
        Label4.Text = "欄數："
        ' 
        ' lblLimitInfo
        ' 
        lblLimitInfo.BackColor = Color.Transparent
        lblLimitInfo.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        lblLimitInfo.ForeColor = Color.FromArgb(CByte(128), CByte(128), CByte(128))
        lblLimitInfo.Location = New Point(8, 68)
        lblLimitInfo.Name = "lblLimitInfo"
        lblLimitInfo.Size = New Size(514, 19)
        lblLimitInfo.TabIndex = 17
        lblLimitInfo.Text = "已選取 362 張照片    將列印成 13 頁"
        lblLimitInfo.TextAlign = ContentAlignment.TopCenter
        ' 
        ' lblPaper
        ' 
        lblPaper.BackColor = Color.Transparent
        lblPaper.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        lblPaper.ForeColor = Color.FromArgb(CByte(128), CByte(128), CByte(128))
        lblPaper.Location = New Point(8, 94)
        lblPaper.Name = "lblPaper"
        lblPaper.Size = New Size(514, 19)
        lblPaper.TabIndex = 23
        lblPaper.Text = "已選取 362 張照片    將列印成 13 頁"
        lblPaper.TextAlign = ContentAlignment.TopCenter
        ' 
        ' cboPrinter
        ' 
        cboPrinter.ActiveControl = True
        cboPrinter.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        cboPrinter.ForeColor = SystemColors.ControlText
        cboPrinter.Location = New Point(756, 636)
        cboPrinter.Name = "cboPrinter"
        cboPrinter.SelectedIndex = -1
        cboPrinter.Size = New Size(423, 21)
        cboPrinter.SoundFileOfClick = ""
        cboPrinter.SoundFileOfEnterFocus = ""
        cboPrinter.SoundFileOfExitFocus = ""
        cboPrinter.SoundFileOfMouseEnter = ""
        cboPrinter.SoundFileOfMouseHover = ""
        cboPrinter.SoundFileOfMouseLeave = ""
        cboPrinter.TabIndex = 10
        cboPrinter.TabStop = False
        ' 
        ' cboDefault
        ' 
        cboDefault.ActiveControl = True
        cboDefault.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        cboDefault.ForeColor = SystemColors.ControlText
        cboDefault.Items.Add(New Aqua.MenuItem("H", "照片", 1, Nothing, Nothing))
        cboDefault.Location = New Point(756, 666)
        cboDefault.Name = "cboDefault"
        cboDefault.SelectedIndex = -1
        cboDefault.Size = New Size(423, 21)
        cboDefault.SoundFileOfClick = ""
        cboDefault.SoundFileOfEnterFocus = ""
        cboDefault.SoundFileOfExitFocus = ""
        cboDefault.SoundFileOfMouseEnter = ""
        cboDefault.SoundFileOfMouseHover = ""
        cboDefault.SoundFileOfMouseLeave = ""
        cboDefault.TabIndex = 11
        cboDefault.TabStop = False
        ' 
        ' cboType
        ' 
        cboType.ActiveControl = True
        cboType.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        cboType.ForeColor = SystemColors.ControlText
        cboType.Items.Add(New Aqua.MenuItem("H", "橫式照片 4x3 輸出", 1, Nothing, Nothing))
        cboType.Items.Add(New Aqua.MenuItem("V", "直式 3x4 照片輸出", 1, Nothing, Nothing))
        cboType.Location = New Point(756, 696)
        cboType.Name = "cboType"
        cboType.SelectedIndex = -1
        cboType.Size = New Size(423, 21)
        cboType.SoundFileOfClick = ""
        cboType.SoundFileOfEnterFocus = ""
        cboType.SoundFileOfExitFocus = ""
        cboType.SoundFileOfMouseEnter = ""
        cboType.SoundFileOfMouseHover = ""
        cboType.SoundFileOfMouseLeave = ""
        cboType.TabIndex = 13
        cboType.TabStop = False
        ' 
        ' txtPrintCount
        ' 
        txtPrintCount.Alignment = Aqua.AlignmentConstants.Center
        txtPrintCount.AutoSelect = True
        txtPrintCount.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtPrintCount.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtPrintCount.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtPrintCount.Font = New Font("Times New Roman", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtPrintCount.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtPrintCount.Location = New Point(1134, 858)
        txtPrintCount.Name = "txtPrintCount"
        txtPrintCount.PasswordChar = ChrW(0)
        txtPrintCount.RowActive = True
        txtPrintCount.SelLength = 0
        txtPrintCount.SelStart = 0
        txtPrintCount.SelText = ""
        txtPrintCount.Size = New Size(33, 25)
        txtPrintCount.SoundFileOfEnterFocus = ""
        txtPrintCount.SoundFileOfExitFocus = ""
        txtPrintCount.TabIndex = 19
        txtPrintCount.TabStop = False
        txtPrintCount.Text = "1"
        ' 
        ' UpDown1
        ' 
        UpDown1.Color = Aqua.ColorConstants.Blue
        UpDown1.Location = New Point(1172, 856)
        UpDown1.Maximum = 9
        UpDown1.Name = "UpDown1"
        UpDown1.Orientation = Aqua.OrientationMode.Vertical
        UpDown1.Size = New Size(15, 29)
        UpDown1.Style = Aqua.UpDownStyle.Independence
        UpDown1.TabIndex = 20
        UpDown1.TabStop = False
        ' 
        ' chkPrintOuputDateTime
        ' 
        chkPrintOuputDateTime.BackColor = Color.Transparent
        chkPrintOuputDateTime.Checked = True
        chkPrintOuputDateTime.ImageCheckDisabled = CType(resources.GetObject("chkPrintOuputDateTime.ImageCheckDisabled"), Image)
        chkPrintOuputDateTime.ImageChecked = CType(resources.GetObject("chkPrintOuputDateTime.ImageChecked"), Image)
        chkPrintOuputDateTime.ImageUnCheckDisabled = CType(resources.GetObject("chkPrintOuputDateTime.ImageUnCheckDisabled"), Image)
        chkPrintOuputDateTime.ImageUnChecked = CType(resources.GetObject("chkPrintOuputDateTime.ImageUnChecked"), Image)
        chkPrintOuputDateTime.Location = New Point(680, 890)
        chkPrintOuputDateTime.Name = "chkPrintOuputDateTime"
        chkPrintOuputDateTime.TabIndex = 21
        chkPrintOuputDateTime.TabStop = False
        chkPrintOuputDateTime.TextValue = "列印輸出日期/時間"
        chkPrintOuputDateTime.TextGap = 8
        chkPrintOuputDateTime.ForeColor = SystemColors.WindowText
        chkPrintOuputDateTime.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' butSave
        ' 
        butSave.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butSave.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butSave.Location = New Point(600, 940)
        butSave.Name = "butSave"
        butSave.Size = New Size(111, 27)
        butSave.SoundFileOfClick = Nothing
        butSave.SoundFileOfEnterFocus = Nothing
        butSave.SoundFileOfExitFocus = Nothing
        butSave.SoundFileOfMouseEnter = Nothing
        butSave.SoundFileOfMouseHover = Nothing
        butSave.SoundFileOfMouseLeave = Nothing
        butSave.TabIndex = 3
        butSave.Text = "存檔"
        ' 
        ' butPrint
        ' 
        butPrint.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butPrint.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butPrint.Location = New Point(874, 942)
        butPrint.Name = "butPrint"
        butPrint.Size = New Size(111, 27)
        butPrint.SoundFileOfClick = Nothing
        butPrint.SoundFileOfEnterFocus = Nothing
        butPrint.SoundFileOfExitFocus = Nothing
        butPrint.SoundFileOfMouseEnter = Nothing
        butPrint.SoundFileOfMouseHover = Nothing
        butPrint.SoundFileOfMouseLeave = Nothing
        butPrint.TabIndex = 5
        butPrint.Text = "列印"
        ' 
        ' butPaper
        ' 
        butPaper.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butPaper.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butPaper.Location = New Point(237, 942)
        butPaper.Name = "butPaper"
        butPaper.Size = New Size(155, 27)
        butPaper.SoundFileOfClick = Nothing
        butPaper.SoundFileOfEnterFocus = Nothing
        butPaper.SoundFileOfExitFocus = Nothing
        butPaper.SoundFileOfMouseEnter = Nothing
        butPaper.SoundFileOfMouseHover = Nothing
        butPaper.SoundFileOfMouseLeave = Nothing
        butPaper.TabIndex = 1
        butPaper.Text = "紙張設定"
        ' 
        ' butExit
        ' 
        butExit.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butExit.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butExit.Location = New Point(737, 942)
        butExit.Name = "butExit"
        butExit.Size = New Size(111, 27)
        butExit.SoundFileOfClick = Nothing
        butExit.SoundFileOfEnterFocus = Nothing
        butExit.SoundFileOfExitFocus = Nothing
        butExit.SoundFileOfMouseEnter = Nothing
        butExit.SoundFileOfMouseHover = Nothing
        butExit.SoundFileOfMouseLeave = Nothing
        butExit.TabIndex = 4
        butExit.Text = "結束"
        ' 
        ' butAdviance
        ' 
        butAdviance.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butAdviance.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butAdviance.Location = New Point(419, 942)
        butAdviance.Name = "butAdviance"
        butAdviance.Size = New Size(155, 27)
        butAdviance.SoundFileOfClick = Nothing
        butAdviance.SoundFileOfEnterFocus = Nothing
        butAdviance.SoundFileOfExitFocus = Nothing
        butAdviance.SoundFileOfMouseEnter = Nothing
        butAdviance.SoundFileOfMouseHover = Nothing
        butAdviance.SoundFileOfMouseLeave = Nothing
        butAdviance.TabIndex = 2
        butAdviance.Text = "進階選項"
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 300
        ' 
        ' udPage
        ' 
        udPage.Color = Aqua.ColorConstants.Blue
        udPage.Location = New Point(60, 930)
        udPage.Maximum = 4
        udPage.Name = "udPage"
        udPage.Orientation = Aqua.OrientationMode.Horizontal
        udPage.Size = New Size(45, 15)
        udPage.Style = Aqua.UpDownStyle.Independence
        udPage.TabIndex = 27
        udPage.TabStop = False
        ' 
        ' txtPage
        ' 
        txtPage.Alignment = Aqua.AlignmentConstants.Center
        txtPage.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtPage.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtPage.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtPage.Font = New Font("Times New Roman", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtPage.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtPage.Location = New Point(20, 924)
        txtPage.Name = "txtPage"
        txtPage.PasswordChar = ChrW(0)
        txtPage.RowActive = True
        txtPage.SelLength = 0
        txtPage.SelStart = 0
        txtPage.SelText = ""
        txtPage.Size = New Size(33, 25)
        txtPage.SoundFileOfEnterFocus = ""
        txtPage.SoundFileOfExitFocus = ""
        txtPage.TabIndex = 28
        txtPage.TabStop = False
        txtPage.Text = "1"
        ' 
        ' MediaList1
        ' 
        MediaList1.BackColor = SystemColors.Window
        MediaList1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        MediaList1.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        MediaList1.BorderSize = 8
        MediaList1.DropItem = False
        MediaList1.Font = New Font("新細明體", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        MediaList1.ForeColor = SystemColors.ControlText
        MediaList1.ItemSize = New Size(200, 180)
        MediaList1.Limit = 6
        MediaList1.Location = New Point(652, 60)
        MediaList1.MarkImage = Nothing
        MediaList1.Name = "MediaList1"
        MediaList1.SelectedIndex = -1
        MediaList1.SelectedItem = Nothing
        MediaList1.ShowCheckBox = False
        MediaList1.ShowRating = False
        MediaList1.Size = New Size(553, 565)
        MediaList1.TabIndex = 29
        MediaList1.TabStop = False
        ' 
        ' rdPrintMode_0
        ' 
        rdPrintMode_0.BackColor = Color.Transparent
        rdPrintMode_0.Checked = True
        rdPrintMode_0.ImageCheckDisabled = CType(resources.GetObject("rdPrintMode_0.ImageCheckDisabled"), Image)
        rdPrintMode_0.ImageChecked = CType(resources.GetObject("rdPrintMode_0.ImageChecked"), Image)
        rdPrintMode_0.ImageUnCheckDisabled = CType(resources.GetObject("rdPrintMode_0.ImageUnCheckDisabled"), Image)
        rdPrintMode_0.ImageUnChecked = CType(resources.GetObject("rdPrintMode_0.ImageUnChecked"), Image)
        rdPrintMode_0.Location = New Point(387, 32)
        rdPrintMode_0.Name = "rdPrintMode_0"
        rdPrintMode_0.TabIndex = 30
        rdPrintMode_0.TabStop = False
        rdPrintMode_0.TextValue = "自動設定列印的照片"
        rdPrintMode_0.TextGap = 8
        rdPrintMode_0.ForeColor = SystemColors.WindowText
        rdPrintMode_0.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' rdPrintMode_1
        ' 
        rdPrintMode_1.BackColor = Color.Transparent
        rdPrintMode_1.Checked = False
        rdPrintMode_1.ImageCheckDisabled = CType(resources.GetObject("rdPrintMode_1.ImageCheckDisabled"), Image)
        rdPrintMode_1.ImageChecked = CType(resources.GetObject("rdPrintMode_1.ImageChecked"), Image)
        rdPrintMode_1.ImageUnCheckDisabled = CType(resources.GetObject("rdPrintMode_1.ImageUnCheckDisabled"), Image)
        rdPrintMode_1.ImageUnChecked = CType(resources.GetObject("rdPrintMode_1.ImageUnChecked"), Image)
        rdPrintMode_1.Location = New Point(631, 32)
        rdPrintMode_1.Name = "rdPrintMode_1"
        rdPrintMode_1.TabIndex = 32
        rdPrintMode_1.TabStop = False
        rdPrintMode_1.TextValue = "手動設定列印的照片"
        rdPrintMode_1.TextGap = 8
        rdPrintMode_1.ForeColor = SystemColors.WindowText
        rdPrintMode_1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' chkPrintExifDate
        ' 
        chkPrintExifDate.BackColor = Color.Transparent
        chkPrintExifDate.Checked = True
        chkPrintExifDate.ImageCheckDisabled = CType(resources.GetObject("chkPrintExifDate.ImageCheckDisabled"), Image)
        chkPrintExifDate.ImageChecked = CType(resources.GetObject("chkPrintExifDate.ImageChecked"), Image)
        chkPrintExifDate.ImageUnCheckDisabled = CType(resources.GetObject("chkPrintExifDate.ImageUnCheckDisabled"), Image)
        chkPrintExifDate.ImageUnChecked = CType(resources.GetObject("chkPrintExifDate.ImageUnChecked"), Image)
        chkPrintExifDate.Location = New Point(680, 860)
        chkPrintExifDate.Name = "chkPrintExifDate"
        chkPrintExifDate.TabIndex = 35
        chkPrintExifDate.TextValue = "列印拍攝日期（只支援數位相片）"
        chkPrintExifDate.TextGap = 8
        chkPrintExifDate.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' picSave
        ' 
        picSave.BackColor = SystemColors.Window
        picSave.ForeColor = SystemColors.WindowText
        picSave.Location = New Point(306, 998)
        picSave.Name = "picSave"
        picSave.Size = New Size(61, 21)
        picSave.TabIndex = 34
        picSave.TabStop = False
        picSave.Visible = False
        ' 
        ' frmPrint
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = SystemColors.Window
        ClientSize = New Size(1223, 993)
        Controls.Add(picContainer)
        Controls.Add(PictureBox2)
        Controls.Add(cboPrinter)
        Controls.Add(cboDefault)
        Controls.Add(cboType)
        Controls.Add(txtPrintCount)
        Controls.Add(UpDown1)
        Controls.Add(chkPrintOuputDateTime)
        Controls.Add(butSave)
        Controls.Add(butPrint)
        Controls.Add(butPaper)
        Controls.Add(butExit)
        Controls.Add(butAdviance)
        Controls.Add(udPage)
        Controls.Add(txtPage)
        Controls.Add(MediaList1)
        Controls.Add(rdPrintMode_0)
        Controls.Add(rdPrintMode_1)
        Controls.Add(chkPrintExifDate)
        Controls.Add(picSave)
        Controls.Add(Label1)
        Controls.Add(Label2)
        Controls.Add(Label3)
        Controls.Add(Label7)
        Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ForeColor = SystemColors.ControlText
        Image = CType(resources.GetObject("$this.Image"), Image)
        MaxButton = False
        MenuFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        MinButton = False
        Name = "frmPrint"
        ShowInTaskbar = False
        SizeMode = Aqua.ImageSizeMode.Appose
        StartPosition = FormStartPosition.CenterScreen
        Text = "照片列印"
        TitleFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        WindowBorderStyle = Aqua.FormBorderStyle.Fixed
        picContainer.ResumeLayout(False)
        CType(picShadow, ComponentModel.ISupportInitialize).EndInit()
        picPaper.ResumeLayout(False)
        CType(picPicture_0, ComponentModel.ISupportInitialize).EndInit()
        PictureBox2.ResumeLayout(False)
        PictureBox2.PerformLayout()
        CType(picSave, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents picContainer As Aqua.PicturePanel
    Friend WithEvents picShadow As System.Windows.Forms.PictureBox
    Friend WithEvents picPaper As System.Windows.Forms.Panel
    Friend WithEvents picPicture_0 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As Aqua.PicturePanel
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents lblLimitInfo As System.Windows.Forms.Label
    Friend WithEvents lblPaper As System.Windows.Forms.Label
    Friend WithEvents Slider1 As Aqua.Slider
    Friend WithEvents txtLimit As Aqua.TextBox
    Friend WithEvents cboPrinter As Aqua.DropDownList
    Friend WithEvents cboDefault As Aqua.DropDownList
    Friend WithEvents cboType As Aqua.DropDownList
    Friend WithEvents txtPrintCount As Aqua.TextBox
    Friend WithEvents UpDown1 As Aqua.UpDown
    Friend WithEvents chkPrintOuputDateTime As Aqua.CheckBox
    Friend WithEvents butSave As Aqua.FlashButton
    Friend WithEvents butPrint As Aqua.FlashButton
    Friend WithEvents butPaper As Aqua.FlashButton
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butAdviance As Aqua.FlashButton
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents udPage As Aqua.UpDown
    Friend WithEvents txtPage As Aqua.TextBox
    Friend WithEvents MediaList1 As Aqua.MediaList
    Friend WithEvents rdPrintMode_0 As Aqua.RadioButton
    Friend WithEvents rdPrintMode_1 As Aqua.RadioButton
    Friend WithEvents chkPrintExifDate As Aqua.CheckBox
    Friend WithEvents picSave As System.Windows.Forms.PictureBox
End Class
