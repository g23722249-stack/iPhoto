<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmImport
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmImport))
        Line1 = New Label()
        lblResolution = New Label()
        lblFileDateTime = New Label()
        lblFileLength = New Label()
        lblFileName = New Label()
        Label4 = New Label()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        imgSubject = New PictureBox()
        Label6 = New Label()
        Label7 = New Label()
        Label8 = New Label()
        Label9 = New Label()
        Line2 = New Label()
        Label5 = New Label()
        FileListBox1 = New Aqua.FileListBox()
        DeskTop1 = New Aqua.DirListBox()
        Panel1 = New Aqua.Panel()
        MediaItem1 = New Aqua.MediaItem()
        frmClass = New Aqua.Panel()
        tvClass = New TreeView()
        txtRemark = New Aqua.TextBox()
        txtSpot = New Aqua.TextBox()
        txtDate = New Aqua.TextBox()
        txtTitle = New Aqua.TextBox()
        butExit = New Aqua.FlashButton()
        butOk = New Aqua.FlashButton()
        txtFolder = New Aqua.TextBox()
        picSubject = New PictureBox()
        Button1 = New Aqua.FlashButton()
        chkAutoInfo = New Aqua.CheckBox()
        vb6ToolTip = New ToolTip(components)
        CType(imgSubject, ComponentModel.ISupportInitialize).BeginInit()
        Panel1.SuspendLayout()
        frmClass.SuspendLayout()
        CType(picSubject, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Line1
        ' 
        Line1.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        Line1.Location = New Point(236, 568)
        Line1.Name = "Line1"
        Line1.Size = New Size(1, 122)
        Line1.TabIndex = 30
        ' 
        ' lblResolution
        ' 
        lblResolution.BackColor = Color.Transparent
        lblResolution.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        lblResolution.Location = New Point(244, 627)
        lblResolution.Name = "lblResolution"
        lblResolution.Size = New Size(216, 21)
        lblResolution.TabIndex = 11
        ' 
        ' lblFileDateTime
        ' 
        lblFileDateTime.BackColor = Color.Transparent
        lblFileDateTime.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        lblFileDateTime.Location = New Point(244, 654)
        lblFileDateTime.Name = "lblFileDateTime"
        lblFileDateTime.Size = New Size(216, 45)
        lblFileDateTime.TabIndex = 12
        ' 
        ' lblFileLength
        ' 
        lblFileLength.BackColor = Color.Transparent
        lblFileLength.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        lblFileLength.Location = New Point(244, 601)
        lblFileLength.Name = "lblFileLength"
        lblFileLength.Size = New Size(216, 21)
        lblFileLength.TabIndex = 13
        ' 
        ' lblFileName
        ' 
        lblFileName.BackColor = Color.Transparent
        lblFileName.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        lblFileName.Location = New Point(244, 574)
        lblFileName.Name = "lblFileName"
        lblFileName.Size = New Size(216, 21)
        lblFileName.TabIndex = 14
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.BackColor = Color.Transparent
        Label4.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label4.Location = New Point(188, 627)
        Label4.Name = "Label4"
        Label4.Size = New Size(49, 19)
        Label4.TabIndex = 15
        Label4.Text = "尺寸"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.BackColor = Color.Transparent
        Label3.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label3.Location = New Point(188, 654)
        Label3.Name = "Label3"
        Label3.Size = New Size(49, 19)
        Label3.TabIndex = 16
        Label3.Text = "日期"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.BackColor = Color.Transparent
        Label2.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label2.Location = New Point(188, 601)
        Label2.Name = "Label2"
        Label2.Size = New Size(49, 19)
        Label2.TabIndex = 17
        Label2.Text = "大小"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.Transparent
        Label1.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label1.Location = New Point(188, 574)
        Label1.Name = "Label1"
        Label1.Size = New Size(49, 19)
        Label1.TabIndex = 18
        Label1.Text = "檔名"
        ' 
        ' imgSubject
        ' 
        imgSubject.BackColor = Color.Transparent
        imgSubject.Image = CType(resources.GetObject("imgSubject.Image"), Image)
        imgSubject.Location = New Point(788, 627)
        imgSubject.Name = "imgSubject"
        imgSubject.Size = New Size(24, 24)
        imgSubject.SizeMode = PictureBoxSizeMode.StretchImage
        imgSubject.TabIndex = 0
        imgSubject.TabStop = False
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.BackColor = Color.Transparent
        Label6.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label6.Location = New Point(778, 508)
        Label6.Name = "Label6"
        Label6.Size = New Size(49, 19)
        Label6.TabIndex = 21
        Label6.Text = "主題"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.BackColor = Color.Transparent
        Label7.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label7.Location = New Point(778, 538)
        Label7.Name = "Label7"
        Label7.Size = New Size(49, 19)
        Label7.TabIndex = 22
        Label7.Text = "日期"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.BackColor = Color.Transparent
        Label8.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label8.Location = New Point(778, 567)
        Label8.Name = "Label8"
        Label8.Size = New Size(49, 19)
        Label8.TabIndex = 23
        Label8.Text = "地點"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.BackColor = Color.Transparent
        Label9.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label9.Location = New Point(778, 597)
        Label9.Name = "Label9"
        Label9.Size = New Size(49, 19)
        Label9.TabIndex = 24
        Label9.Text = "註解"
        ' 
        ' Line2
        ' 
        Line2.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(192))
        Line2.Location = New Point(824, 475)
        Line2.Name = "Line2"
        Line2.Size = New Size(1, 208)
        Line2.TabIndex = 31
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.BackColor = Color.Transparent
        Label5.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label5.Location = New Point(778, 479)
        Label5.Name = "Label5"
        Label5.Size = New Size(49, 19)
        Label5.TabIndex = 27
        Label5.Text = "名稱"
        vb6ToolTip.SetToolTip(Label5, "資料夾名稱")
        ' 
        ' lblAutoInfo
        ' 
        ' 
        ' FileListBox1
        ' 
        FileListBox1.BackColor = SystemColors.Window
        FileListBox1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        FileListBox1.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        FileListBox1.Checkboxes = True
        FileListBox1.Font = New Font("Arial", 10.5F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        FileListBox1.Location = New Point(466, 34)
        FileListBox1.Name = "FileListBox1"
        FileListBox1.Pattern = "rm;mov;asf;avi;dat;mpeg;mpg;mp4;jpeg;jpg;bmp;png"
        FileListBox1.Size = New Size(306, 651)
        FileListBox1.TabIndex = 1
        FileListBox1.TabStop = False
        ' 
        ' DeskTop1
        ' 
        DeskTop1.BackColor = SystemColors.Window
        DeskTop1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        DeskTop1.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        DeskTop1.Font = New Font("華康細圓體", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        DeskTop1.Location = New Point(12, 34)
        DeskTop1.Name = "DeskTop1"
        DeskTop1.Size = New Size(447, 531)
        DeskTop1.TabIndex = 0
        DeskTop1.TabStop = False
        ' 
        ' Panel1
        ' 
        Panel1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        Panel1.BorderFocusColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        Panel1.Controls.Add(MediaItem1)
        Panel1.Font = New Font("新細明體", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Panel1.ForeColor = SystemColors.ControlText
        Panel1.Image = Nothing
        Panel1.Location = New Point(14, 572)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(169, 119)
        Panel1.SoundFileOfClick = Nothing
        Panel1.SoundFileOfEnterFocus = Nothing
        Panel1.SoundFileOfExitFocus = Nothing
        Panel1.SoundFileOfMouseEnter = Nothing
        Panel1.SoundFileOfMouseHover = Nothing
        Panel1.SoundFileOfMouseLeave = Nothing
        Panel1.TabIndex = 19
        ' 
        ' MediaItem1
        ' 
        MediaItem1.BackColor = Color.White
        MediaItem1.BorderSize = 4
        MediaItem1.CheckText = ""
        MediaItem1.DragItem = False
        MediaItem1.DropItem = False
        MediaItem1.Enabled = False
        MediaItem1.FileName = Nothing
        MediaItem1.Location = New Point(4, 4)
        MediaItem1.MarkImage = Nothing
        MediaItem1.Name = "MediaItem1"
        MediaItem1.Padding = New Padding(6)
        MediaItem1.ShowCheckBox = False
        MediaItem1.ShowRating = False
        MediaItem1.Size = New Size(161, 111)
        MediaItem1.TabIndex = 25
        ' 
        ' frmClass
        ' 
        frmClass.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        frmClass.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        frmClass.Controls.Add(tvClass)
        frmClass.Font = New Font("新細明體", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        frmClass.ForeColor = SystemColors.ControlText
        frmClass.Image = Nothing
        frmClass.Location = New Point(778, 34)
        frmClass.Name = "frmClass"
        frmClass.PanelStyle = Aqua.PanelStyleMode.Container
        frmClass.Size = New Size(303, 433)
        frmClass.SoundFileOfClick = Nothing
        frmClass.SoundFileOfEnterFocus = Nothing
        frmClass.SoundFileOfExitFocus = Nothing
        frmClass.SoundFileOfMouseEnter = Nothing
        frmClass.SoundFileOfMouseHover = Nothing
        frmClass.SoundFileOfMouseLeave = Nothing
        frmClass.TabIndex = 20
        ' 
        ' tvClass
        ' 
        tvClass.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        tvClass.FullRowSelect = True
        tvClass.HideSelection = False
        tvClass.Location = New Point(4, 4)
        tvClass.Name = "tvClass"
        tvClass.Size = New Size(297, 426)
        tvClass.TabIndex = 2
        ' 
        ' txtRemark
        ' 
        txtRemark.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtRemark.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtRemark.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtRemark.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtRemark.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtRemark.Location = New Point(830, 593)
        txtRemark.MaxLength = 32767
        txtRemark.Multiline = True
        txtRemark.Name = "txtRemark"
        txtRemark.PasswordChar = ChrW(0)
        txtRemark.RowActive = True
        txtRemark.SelLength = 0
        txtRemark.SelStart = 0
        txtRemark.SelText = ""
        txtRemark.Size = New Size(251, 93)
        txtRemark.SoundFileOfEnterFocus = ""
        txtRemark.SoundFileOfExitFocus = ""
        txtRemark.TabIndex = 6
        txtRemark.TabStop = False
        ' 
        ' txtSpot
        ' 
        txtSpot.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtSpot.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtSpot.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtSpot.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtSpot.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtSpot.Location = New Point(830, 563)
        txtSpot.MaxLength = 32767
        txtSpot.Name = "txtSpot"
        txtSpot.PasswordChar = ChrW(0)
        txtSpot.RowActive = True
        txtSpot.SelLength = 0
        txtSpot.SelStart = 0
        txtSpot.SelText = ""
        txtSpot.Size = New Size(251, 25)
        txtSpot.SoundFileOfEnterFocus = ""
        txtSpot.SoundFileOfExitFocus = ""
        txtSpot.TabIndex = 5
        txtSpot.TabStop = False
        ' 
        ' txtDate
        ' 
        txtDate.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtDate.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtDate.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtDate.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtDate.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtDate.Location = New Point(830, 534)
        txtDate.MaxLength = 32767
        txtDate.Name = "txtDate"
        txtDate.PasswordChar = ChrW(0)
        txtDate.RowActive = True
        txtDate.SelLength = 0
        txtDate.SelStart = 0
        txtDate.SelText = ""
        txtDate.Size = New Size(251, 25)
        txtDate.SoundFileOfEnterFocus = ""
        txtDate.SoundFileOfExitFocus = ""
        txtDate.TabIndex = 4
        txtDate.TabStop = False
        ' 
        ' txtTitle
        ' 
        txtTitle.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtTitle.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtTitle.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtTitle.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtTitle.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtTitle.Location = New Point(830, 504)
        txtTitle.MaxLength = 32767
        txtTitle.Name = "txtTitle"
        txtTitle.PasswordChar = ChrW(0)
        txtTitle.RowActive = True
        txtTitle.SelLength = 0
        txtTitle.SelStart = 0
        txtTitle.SelText = ""
        txtTitle.Size = New Size(251, 25)
        txtTitle.SoundFileOfEnterFocus = ""
        txtTitle.SoundFileOfExitFocus = ""
        txtTitle.TabIndex = 3
        txtTitle.TabStop = False
        txtTitle.Text = "新相簿"
        ' 
        ' butExit
        ' 
        butExit.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butExit.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butExit.Location = New Point(310, 700)
        butExit.Name = "butExit"
        butExit.Size = New Size(103, 27)
        butExit.SoundFileOfClick = Nothing
        butExit.SoundFileOfEnterFocus = Nothing
        butExit.SoundFileOfExitFocus = Nothing
        butExit.SoundFileOfMouseEnter = Nothing
        butExit.SoundFileOfMouseHover = Nothing
        butExit.SoundFileOfMouseLeave = Nothing
        butExit.TabIndex = 7
        butExit.Text = "取消"
        ' 
        ' butOk
        ' 
        butOk.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butOk.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butOk.Location = New Point(682, 700)
        butOk.Name = "butOk"
        butOk.Size = New Size(103, 27)
        butOk.SoundFileOfClick = Nothing
        butOk.SoundFileOfEnterFocus = Nothing
        butOk.SoundFileOfExitFocus = Nothing
        butOk.SoundFileOfMouseEnter = Nothing
        butOk.SoundFileOfMouseHover = Nothing
        butOk.SoundFileOfMouseLeave = Nothing
        butOk.TabIndex = 9
        butOk.Text = "好"
        ' 
        ' txtFolder
        ' 
        txtFolder.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtFolder.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtFolder.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtFolder.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtFolder.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtFolder.Location = New Point(830, 475)
        txtFolder.MaxLength = 32767
        txtFolder.Name = "txtFolder"
        txtFolder.PasswordChar = ChrW(0)
        txtFolder.RowActive = True
        txtFolder.SelLength = 0
        txtFolder.SelStart = 0
        txtFolder.SelText = ""
        txtFolder.Size = New Size(251, 25)
        txtFolder.SoundFileOfEnterFocus = ""
        txtFolder.SoundFileOfExitFocus = ""
        txtFolder.TabIndex = 26
        txtFolder.TabStop = False
        vb6ToolTip.SetToolTip(txtFolder, "資料夾名稱")
        ' 
        ' picSubject
        ' 
        picSubject.BorderStyle = BorderStyle.Fixed3D
        picSubject.Location = New Point(778, 475)
        picSubject.Name = "picSubject"
        picSubject.Size = New Size(37, 27)
        picSubject.TabIndex = 28
        picSubject.TabStop = False
        picSubject.Visible = False
        ' 
        ' Button1
        ' 
        Button1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Button1.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Button1.Location = New Point(430, 700)
        Button1.Name = "Button1"
        Button1.Size = New Size(235, 27)
        Button1.SoundFileOfClick = Nothing
        Button1.SoundFileOfEnterFocus = Nothing
        Button1.SoundFileOfExitFocus = Nothing
        Button1.SoundFileOfMouseEnter = Nothing
        Button1.SoundFileOfMouseHover = Nothing
        Button1.SoundFileOfMouseLeave = Nothing
        Button1.TabIndex = 8
        Button1.Text = "輸入預設的相片資訊"
        ' 
        ' chkAutoInfo
        ' 
        chkAutoInfo.BackColor = Color.Transparent
        chkAutoInfo.Checked = True
        chkAutoInfo.ImageCheckDisabled = CType(resources.GetObject("chkAutoInfo.ImageCheckDisabled"), Image)
        chkAutoInfo.ImageChecked = CType(resources.GetObject("chkAutoInfo.ImageChecked"), Image)
        chkAutoInfo.ImageUnCheckDisabled = CType(resources.GetObject("chkAutoInfo.ImageUnCheckDisabled"), Image)
        chkAutoInfo.ImageUnChecked = CType(resources.GetObject("chkAutoInfo.ImageUnChecked"), Image)
        chkAutoInfo.Location = New Point(16, 706)
        chkAutoInfo.Name = "chkAutoInfo"
        chkAutoInfo.TabIndex = 29
        chkAutoInfo.TextValue = "自動帶入相簿資訊"
        chkAutoInfo.TextGap = 6
        chkAutoInfo.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ' 
        ' frmImport
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = SystemColors.Window
        ClientSize = New Size(1095, 741)
        Controls.Add(imgSubject)
        Controls.Add(FileListBox1)
        Controls.Add(DeskTop1)
        Controls.Add(Panel1)
        Controls.Add(frmClass)
        Controls.Add(txtRemark)
        Controls.Add(txtSpot)
        Controls.Add(txtDate)
        Controls.Add(txtTitle)
        Controls.Add(butExit)
        Controls.Add(butOk)
        Controls.Add(txtFolder)
        Controls.Add(picSubject)
        Controls.Add(Button1)
        Controls.Add(chkAutoInfo)
        Controls.Add(Line1)
        Controls.Add(lblResolution)
        Controls.Add(lblFileDateTime)
        Controls.Add(lblFileLength)
        Controls.Add(lblFileName)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(Label6)
        Controls.Add(Label7)
        Controls.Add(Label8)
        Controls.Add(Label9)
        Controls.Add(Line2)
        Controls.Add(Label5)
        Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ForeColor = SystemColors.ControlText
        Image = CType(resources.GetObject("$this.Image"), Image)
        MaxButton = False
        MaximizeBox = False
        MenuFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        MinButton = False
        MinimizeBox = False
        Name = "frmImport"
        ShowInTaskbar = False
        SizeMode = Aqua.ImageSizeMode.Appose
        StartPosition = FormStartPosition.CenterScreen
        Text = "輸入"
        TitleFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        WindowBorderStyle = Aqua.FormBorderStyle.Fixed
        CType(imgSubject, ComponentModel.ISupportInitialize).EndInit()
        Panel1.ResumeLayout(False)
        frmClass.ResumeLayout(False)
        CType(picSubject, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents lblResolution As System.Windows.Forms.Label
    Friend WithEvents lblFileDateTime As System.Windows.Forms.Label
    Friend WithEvents lblFileLength As System.Windows.Forms.Label
    Friend WithEvents lblFileName As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents imgSubject As System.Windows.Forms.PictureBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Line2 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents FileListBox1 As Aqua.FileListBox
    Friend WithEvents DeskTop1 As Aqua.DirListBox
    Friend WithEvents Panel1 As Aqua.Panel
    Friend WithEvents MediaItem1 As Aqua.MediaItem
    Friend WithEvents frmClass As Aqua.Panel
    Friend WithEvents tvClass As System.Windows.Forms.TreeView
    Friend WithEvents txtRemark As Aqua.TextBox
    Friend WithEvents txtSpot As Aqua.TextBox
    Friend WithEvents txtDate As Aqua.TextBox
    Friend WithEvents txtTitle As Aqua.TextBox
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butOk As Aqua.FlashButton
    Friend WithEvents txtFolder As Aqua.TextBox
    Friend WithEvents picSubject As System.Windows.Forms.PictureBox
    Friend WithEvents Button1 As Aqua.FlashButton
    Friend WithEvents chkAutoInfo As Aqua.CheckBox
    Friend WithEvents vb6ToolTip As System.Windows.Forms.ToolTip
End Class
