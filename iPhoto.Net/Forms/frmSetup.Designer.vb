<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSetup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSetup))
        butSave = New Aqua.FlashButton()
        butExit = New Aqua.FlashButton()
        tabSetup = New Aqua.TabControl()
        pageGeneral = New Aqua.TabPage()
        imgMusic = New PictureBox()
        imgAttached_1 = New PictureBox()
        imgAttached_2 = New PictureBox()
        imgAttached_0 = New PictureBox()
        imgRebuild = New PictureBox()
        imgAttached_3 = New PictureBox()
        sliSlide = New Aqua.Slider()
        txtAttached_2 = New Aqua.TextBox()
        txtAttached_3 = New Aqua.TextBox()
        txtAttached_0 = New Aqua.TextBox()
        txtAttached_1 = New Aqua.TextBox()
        txtMusic = New Aqua.TextBox()
        Label18 = New Label()
        Label19 = New Label()
        Label17 = New Label()
        Label21 = New Label()
        Label22 = New Label()
        Label25 = New Label()
        pageAlbums = New Aqua.TabPage()
        imgFavorites = New PictureBox()
        lstAlbum = New Aqua.ItemListBox()
        txtFavorite = New Aqua.TextBox()
        imgAlbums_0 = New Aqua.IconBox()
        imgAlbums_1 = New Aqua.IconBox()
        imgAlbums_2 = New Aqua.IconBox()
        imgAlbums_3 = New Aqua.IconBox()
        chkPrivilege = New Aqua.CheckBox()
        Label1 = New Label()
        Label2 = New Label()
        pageAppearance = New Aqua.TabPage()
        imgSound_0 = New PictureBox()
        imgFontName = New PictureBox()
        imgSound_1 = New PictureBox()
        imgSound_2 = New PictureBox()
        imgSound_3 = New PictureBox()
        imgSound_4 = New PictureBox()
        txtFont = New Aqua.TextBox()
        txtSound_0 = New Aqua.TextBox()
        txtSound_1 = New Aqua.TextBox()
        txtSound_2 = New Aqua.TextBox()
        txtSound_3 = New Aqua.TextBox()
        txtSound_4 = New Aqua.TextBox()
        rbStyle_0 = New Aqua.RadioButton()
        rbStyle_1 = New Aqua.RadioButton()
        chkSwitchScreen = New Aqua.CheckBox()
        Label4 = New Label()
        Line5 = New Label()
        Label16 = New Label()
        Line4 = New Label()
        Label15 = New Label()
        Line3 = New Label()
        Label14 = New Label()
        Line2 = New Label()
        Label3 = New Label()
        Label5 = New Label()
        Label6 = New Label()
        Label7 = New Label()
        pagePlugins = New Aqua.TabPage()
        imgApp_0 = New PictureBox()
        imgApp_1 = New PictureBox()
        imgApp_2 = New PictureBox()
        imgApp_3 = New PictureBox()
        imgApp_4 = New PictureBox()
        imgApp_5 = New PictureBox()
        txtApp_0 = New Aqua.TextBox()
        txtApp_1 = New Aqua.TextBox()
        txtApp_2 = New Aqua.TextBox()
        txtApp_3 = New Aqua.TextBox()
        txtApp_4 = New Aqua.TextBox()
        txtApp_5 = New Aqua.TextBox()
        Label8 = New Label()
        Label9 = New Label()
        Label10 = New Label()
        Label11 = New Label()
        Label12 = New Label()
        Label13 = New Label()
        vb6ToolTip = New ToolTip(components)
        tabSetup.SuspendLayout()
        pageGeneral.SuspendLayout()
        CType(imgMusic, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgAttached_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgAttached_2, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgAttached_0, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgRebuild, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgAttached_3, ComponentModel.ISupportInitialize).BeginInit()
        pageAlbums.SuspendLayout()
        CType(imgFavorites, ComponentModel.ISupportInitialize).BeginInit()
        pageAppearance.SuspendLayout()
        CType(imgSound_0, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgFontName, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgSound_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgSound_2, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgSound_3, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgSound_4, ComponentModel.ISupportInitialize).BeginInit()
        pagePlugins.SuspendLayout()
        CType(imgApp_0, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgApp_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgApp_2, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgApp_3, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgApp_4, ComponentModel.ISupportInitialize).BeginInit()
        CType(imgApp_5, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' butSave
        ' 
        butSave.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butSave.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butSave.Location = New Point(473, 566)
        butSave.Name = "butSave"
        butSave.Size = New Size(103, 27)
        butSave.SoundFileOfClick = Nothing
        butSave.SoundFileOfEnterFocus = Nothing
        butSave.SoundFileOfExitFocus = Nothing
        butSave.SoundFileOfMouseEnter = Nothing
        butSave.SoundFileOfMouseHover = Nothing
        butSave.SoundFileOfMouseLeave = Nothing
        butSave.TabIndex = 18
        butSave.Text = "儲存"
        ' 
        ' butExit
        ' 
        butExit.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        butExit.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        butExit.Location = New Point(352, 566)
        butExit.Name = "butExit"
        butExit.Size = New Size(103, 27)
        butExit.SoundFileOfClick = Nothing
        butExit.SoundFileOfEnterFocus = Nothing
        butExit.SoundFileOfExitFocus = Nothing
        butExit.SoundFileOfMouseEnter = Nothing
        butExit.SoundFileOfMouseHover = Nothing
        butExit.SoundFileOfMouseLeave = Nothing
        butExit.TabIndex = 17
        butExit.Text = "放棄"
        ' 
        ' tabSetup
        ' 
        tabSetup.BackColor = Color.White
        tabSetup.Controls.Add(pageGeneral)
        tabSetup.Controls.Add(pageAlbums)
        tabSetup.Controls.Add(pageAppearance)
        tabSetup.Controls.Add(pagePlugins)
        tabSetup.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        tabSetup.ForeColor = SystemColors.ControlText
        tabSetup.Location = New Point(34, 48)
        tabSetup.Name = "tabSetup"
        tabSetup.Size = New Size(860, 509)
        tabSetup.TabIndex = 20
        tabSetup.TabPages.Add(pageGeneral)
        tabSetup.TabPages.Add(pageAlbums)
        tabSetup.TabPages.Add(pageAppearance)
        tabSetup.TabPages.Add(pagePlugins)
        tabSetup.TabStop = False
        tabSetup.TabStripPadding = New Padding(10, 0, 10, 0)
        ' 
        ' pageGeneral
        ' 
        pageGeneral.BackColor = Color.White
        pageGeneral.Controls.Add(imgMusic)
        pageGeneral.Controls.Add(imgAttached_1)
        pageGeneral.Controls.Add(imgAttached_2)
        pageGeneral.Controls.Add(imgAttached_0)
        pageGeneral.Controls.Add(imgRebuild)
        pageGeneral.Controls.Add(imgAttached_3)
        pageGeneral.Controls.Add(sliSlide)
        pageGeneral.Controls.Add(txtAttached_2)
        pageGeneral.Controls.Add(txtAttached_3)
        pageGeneral.Controls.Add(txtAttached_0)
        pageGeneral.Controls.Add(txtAttached_1)
        pageGeneral.Controls.Add(txtMusic)
        pageGeneral.Controls.Add(Label18)
        pageGeneral.Controls.Add(Label19)
        pageGeneral.Controls.Add(Label17)
        pageGeneral.Controls.Add(Label21)
        pageGeneral.Controls.Add(Label22)
        pageGeneral.Controls.Add(Label25)
        pageGeneral.Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        pageGeneral.ForeColor = SystemColors.ControlText
        pageGeneral.Location = New Point(3, 30)
        pageGeneral.Name = "pageGeneral"
        pageGeneral.Size = New Size(854, 476)
        pageGeneral.TabIndex = 40
        pageGeneral.Title = "一般設定"
        ' 
        ' imgMusic
        ' 
        imgMusic.BackColor = Color.Transparent
        imgMusic.Image = CType(resources.GetObject("imgMusic.Image"), Image)
        imgMusic.Location = New Point(238, 70)
        imgMusic.Name = "imgMusic"
        imgMusic.Size = New Size(30, 21)
        imgMusic.TabIndex = 0
        imgMusic.TabStop = False
        ' 
        ' imgAttached_1
        ' 
        imgAttached_1.BackColor = Color.Transparent
        imgAttached_1.Image = CType(resources.GetObject("imgAttached_1.Image"), Image)
        imgAttached_1.Location = New Point(96, 202)
        imgAttached_1.Name = "imgAttached_1"
        imgAttached_1.Size = New Size(30, 21)
        imgAttached_1.TabIndex = 1
        imgAttached_1.TabStop = False
        ' 
        ' imgAttached_2
        ' 
        imgAttached_2.BackColor = Color.Transparent
        imgAttached_2.Image = CType(resources.GetObject("imgAttached_2.Image"), Image)
        imgAttached_2.Location = New Point(96, 268)
        imgAttached_2.Name = "imgAttached_2"
        imgAttached_2.Size = New Size(30, 21)
        imgAttached_2.TabIndex = 2
        imgAttached_2.TabStop = False
        ' 
        ' imgAttached_0
        ' 
        imgAttached_0.BackColor = Color.Transparent
        imgAttached_0.Image = CType(resources.GetObject("imgAttached_0.Image"), Image)
        imgAttached_0.Location = New Point(96, 136)
        imgAttached_0.Name = "imgAttached_0"
        imgAttached_0.Size = New Size(30, 21)
        imgAttached_0.TabIndex = 3
        imgAttached_0.TabStop = False
        ' 
        ' imgRebuild
        ' 
        imgRebuild.BackColor = Color.Transparent
        imgRebuild.Image = CType(resources.GetObject("imgRebuild.Image"), Image)
        imgRebuild.Location = New Point(130, 136)
        imgRebuild.Name = "imgRebuild"
        imgRebuild.Size = New Size(30, 21)
        imgRebuild.TabIndex = 4
        imgRebuild.TabStop = False
        vb6ToolTip.SetToolTip(imgRebuild, "重建資料庫")
        ' 
        ' imgAttached_3
        ' 
        imgAttached_3.BackColor = Color.Transparent
        imgAttached_3.Image = CType(resources.GetObject("imgAttached_3.Image"), Image)
        imgAttached_3.Location = New Point(174, 334)
        imgAttached_3.Name = "imgAttached_3"
        imgAttached_3.Size = New Size(30, 21)
        imgAttached_3.TabIndex = 5
        imgAttached_3.TabStop = False
        ' 
        ' sliSlide
        ' 
        sliSlide.Location = New Point(236, 22)
        sliSlide.Maximum = 20
        sliSlide.Minimum = 1
        sliSlide.Name = "sliSlide"
        sliSlide.Size = New Size(598, 39)
        sliSlide.SoundFileOfClick = Nothing
        sliSlide.SoundFileOfEnterFocus = Nothing
        sliSlide.SoundFileOfExitFocus = Nothing
        sliSlide.TabIndex = 0
        sliSlide.TickStyle = Aqua.SliderTickMode.TopLeft
        sliSlide.Value = 1
        ' 
        ' txtAttached_2
        ' 
        txtAttached_2.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtAttached_2.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtAttached_2.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtAttached_2.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtAttached_2.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtAttached_2.Location = New Point(28, 292)
        txtAttached_2.Name = "txtAttached_2"
        txtAttached_2.PasswordChar = ChrW(0)
        txtAttached_2.RowActive = True
        txtAttached_2.SelLength = 0
        txtAttached_2.SelStart = 0
        txtAttached_2.SelText = ""
        txtAttached_2.Size = New Size(806, 27)
        txtAttached_2.SoundFileOfEnterFocus = ""
        txtAttached_2.SoundFileOfExitFocus = ""
        txtAttached_2.TabIndex = 1
        txtAttached_2.TabStop = False
        ' 
        ' txtAttached_3
        ' 
        txtAttached_3.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtAttached_3.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtAttached_3.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtAttached_3.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtAttached_3.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtAttached_3.Location = New Point(28, 358)
        txtAttached_3.Name = "txtAttached_3"
        txtAttached_3.PasswordChar = ChrW(0)
        txtAttached_3.RowActive = True
        txtAttached_3.SelLength = 0
        txtAttached_3.SelStart = 0
        txtAttached_3.SelText = ""
        txtAttached_3.Size = New Size(806, 27)
        txtAttached_3.SoundFileOfEnterFocus = ""
        txtAttached_3.SoundFileOfExitFocus = ""
        txtAttached_3.TabIndex = 52
        txtAttached_3.TabStop = False
        ' 
        ' txtAttached_0
        ' 
        txtAttached_0.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtAttached_0.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtAttached_0.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtAttached_0.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtAttached_0.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtAttached_0.Location = New Point(28, 160)
        txtAttached_0.Name = "txtAttached_0"
        txtAttached_0.PasswordChar = ChrW(0)
        txtAttached_0.RowActive = True
        txtAttached_0.SelLength = 0
        txtAttached_0.SelStart = 0
        txtAttached_0.SelText = ""
        txtAttached_0.Size = New Size(806, 27)
        txtAttached_0.SoundFileOfEnterFocus = ""
        txtAttached_0.SoundFileOfExitFocus = ""
        txtAttached_0.TabIndex = 53
        txtAttached_0.TabStop = False
        ' 
        ' txtAttached_1
        ' 
        txtAttached_1.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtAttached_1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtAttached_1.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtAttached_1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtAttached_1.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtAttached_1.Location = New Point(28, 226)
        txtAttached_1.Name = "txtAttached_1"
        txtAttached_1.PasswordChar = ChrW(0)
        txtAttached_1.RowActive = True
        txtAttached_1.SelLength = 0
        txtAttached_1.SelStart = 0
        txtAttached_1.SelText = ""
        txtAttached_1.Size = New Size(806, 27)
        txtAttached_1.SoundFileOfEnterFocus = ""
        txtAttached_1.SoundFileOfExitFocus = ""
        txtAttached_1.TabIndex = 54
        txtAttached_1.TabStop = False
        ' 
        ' txtMusic
        ' 
        txtMusic.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtMusic.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtMusic.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtMusic.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtMusic.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtMusic.Location = New Point(28, 94)
        txtMusic.Name = "txtMusic"
        txtMusic.PasswordChar = ChrW(0)
        txtMusic.RowActive = True
        txtMusic.SelLength = 0
        txtMusic.SelStart = 0
        txtMusic.SelText = ""
        txtMusic.Size = New Size(806, 27)
        txtMusic.SoundFileOfEnterFocus = ""
        txtMusic.SoundFileOfExitFocus = ""
        txtMusic.TabIndex = 55
        txtMusic.TabStop = False
        ' 
        ' Label18
        ' 
        Label18.AutoSize = True
        Label18.BackColor = Color.Transparent
        Label18.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label18.ForeColor = SystemColors.WindowText
        Label18.Location = New Point(28, 28)
        Label18.Name = "Label18"
        Label18.Size = New Size(229, 19)
        Label18.TabIndex = 41
        Label18.Text = "幻燈片撥放時間（秒）　"
        ' 
        ' Label19
        ' 
        Label19.AutoSize = True
        Label19.BackColor = Color.Transparent
        Label19.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label19.ForeColor = SystemColors.WindowText
        Label19.Location = New Point(28, 70)
        Label19.Name = "Label19"
        Label19.Size = New Size(209, 19)
        Label19.TabIndex = 42
        Label19.Text = "幻燈片音樂檔存放位置"
        ' 
        ' Label17
        ' 
        Label17.AutoSize = True
        Label17.BackColor = Color.Transparent
        Label17.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label17.ForeColor = SystemColors.WindowText
        Label17.Location = New Point(28, 203)
        Label17.Name = "Label17"
        Label17.Size = New Size(69, 19)
        Label17.TabIndex = 43
        Label17.Text = "關鍵字"
        ' 
        ' Label21
        ' 
        Label21.AutoSize = True
        Label21.BackColor = Color.Transparent
        Label21.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label21.ForeColor = SystemColors.WindowText
        Label21.Location = New Point(28, 269)
        Label21.Name = "Label21"
        Label21.Size = New Size(69, 19)
        Label21.TabIndex = 44
        Label21.Text = "通訊錄"
        ' 
        ' Label22
        ' 
        Label22.AutoSize = True
        Label22.BackColor = Color.Transparent
        Label22.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label22.ForeColor = SystemColors.WindowText
        Label22.Location = New Point(28, 136)
        Label22.Name = "Label22"
        Label22.Size = New Size(69, 19)
        Label22.TabIndex = 45
        Label22.Text = "資料庫"
        ' 
        ' Label25
        ' 
        Label25.AutoSize = True
        Label25.BackColor = Color.Transparent
        Label25.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label25.ForeColor = SystemColors.WindowText
        Label25.Location = New Point(28, 336)
        Label25.Name = "Label25"
        Label25.Size = New Size(149, 19)
        Label25.TabIndex = 2
        Label25.Text = "面孔封面資料夾"
        ' 
        ' pageAlbums
        ' 
        pageAlbums.BackColor = Color.White
        pageAlbums.Controls.Add(imgFavorites)
        pageAlbums.Controls.Add(lstAlbum)
        pageAlbums.Controls.Add(txtFavorite)
        pageAlbums.Controls.Add(imgAlbums_0)
        pageAlbums.Controls.Add(imgAlbums_1)
        pageAlbums.Controls.Add(imgAlbums_2)
        pageAlbums.Controls.Add(imgAlbums_3)
        pageAlbums.Controls.Add(chkPrivilege)
        pageAlbums.Controls.Add(Label1)
        pageAlbums.Controls.Add(Label2)
        pageAlbums.Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        pageAlbums.ForeColor = SystemColors.ControlText
        pageAlbums.Location = New Point(3, 30)
        pageAlbums.Name = "pageAlbums"
        pageAlbums.Size = New Size(854, 476)
        pageAlbums.TabIndex = 21
        pageAlbums.Title = "相簿位置"
        ' 
        ' imgFavorites
        ' 
        imgFavorites.BackColor = Color.Transparent
        imgFavorites.Image = CType(resources.GetObject("imgFavorites.Image"), Image)
        imgFavorites.Location = New Point(166, 338)
        imgFavorites.Name = "imgFavorites"
        imgFavorites.Size = New Size(30, 21)
        imgFavorites.TabIndex = 0
        imgFavorites.TabStop = False
        ' 
        ' lstAlbum
        ' 
        lstAlbum.BackColor = SystemColors.Window
        lstAlbum.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        lstAlbum.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        lstAlbum.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        lstAlbum.ForeColor = SystemColors.ControlText
        lstAlbum.Location = New Point(12, 46)
        lstAlbum.Name = "lstAlbum"
        lstAlbum.SelectedIndex = -1
        lstAlbum.Size = New Size(840, 283)
        lstAlbum.SoundFileOfEnterFocus = ""
        lstAlbum.SoundFileOfExitFocus = ""
        lstAlbum.SoundFileOfMouseEnter = ""
        lstAlbum.SoundFileOfMouseLeave = ""
        lstAlbum.TabIndex = 3
        ' 
        ' txtFavorite
        ' 
        txtFavorite.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtFavorite.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtFavorite.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtFavorite.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtFavorite.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtFavorite.Location = New Point(14, 362)
        txtFavorite.Name = "txtFavorite"
        txtFavorite.PasswordChar = ChrW(0)
        txtFavorite.RowActive = True
        txtFavorite.SelLength = 0
        txtFavorite.SelStart = 0
        txtFavorite.SelText = ""
        txtFavorite.Size = New Size(838, 27)
        txtFavorite.SoundFileOfEnterFocus = ""
        txtFavorite.SoundFileOfExitFocus = ""
        txtFavorite.TabIndex = 4
        txtFavorite.TabStop = False
        ' 
        ' imgAlbums_0
        ' 
        imgAlbums_0.BackColor = Color.Transparent
        imgAlbums_0.Image = CType(resources.GetObject("imgAlbums_0.Image"), Image)
        imgAlbums_0.Location = New Point(140, 22)
        imgAlbums_0.Name = "imgAlbums_0"
        imgAlbums_0.Size = New Size(30, 21)
        imgAlbums_0.TabIndex = 5
        imgAlbums_0.TabStop = False
        imgAlbums_0.TransparencyKey = Color.FromArgb(CByte(255), CByte(0), CByte(255))
        ' 
        ' imgAlbums_1
        ' 
        imgAlbums_1.BackColor = Color.Transparent
        imgAlbums_1.Image = CType(resources.GetObject("imgAlbums_1.Image"), Image)
        imgAlbums_1.Location = New Point(178, 22)
        imgAlbums_1.Name = "imgAlbums_1"
        imgAlbums_1.Size = New Size(30, 21)
        imgAlbums_1.TabIndex = 6
        imgAlbums_1.TabStop = False
        imgAlbums_1.TransparencyKey = Color.FromArgb(CByte(255), CByte(0), CByte(255))
        ' 
        ' imgAlbums_2
        ' 
        imgAlbums_2.BackColor = Color.Transparent
        imgAlbums_2.Image = CType(resources.GetObject("imgAlbums_2.Image"), Image)
        imgAlbums_2.Location = New Point(216, 22)
        imgAlbums_2.Name = "imgAlbums_2"
        imgAlbums_2.Size = New Size(30, 21)
        imgAlbums_2.TabIndex = 7
        imgAlbums_2.TabStop = False
        imgAlbums_2.TransparencyKey = Color.FromArgb(CByte(255), CByte(0), CByte(255))
        ' 
        ' imgAlbums_3
        ' 
        imgAlbums_3.BackColor = Color.Transparent
        imgAlbums_3.Image = CType(resources.GetObject("imgAlbums_3.Image"), Image)
        imgAlbums_3.Location = New Point(254, 22)
        imgAlbums_3.Name = "imgAlbums_3"
        imgAlbums_3.Size = New Size(30, 21)
        imgAlbums_3.TabIndex = 8
        imgAlbums_3.TabStop = False
        imgAlbums_3.TransparencyKey = Color.FromArgb(CByte(255), CByte(0), CByte(255))
        ' 
        ' chkPrivilege
        ' 
        chkPrivilege.BackColor = Color.Transparent
        chkPrivilege.Checked = False
        chkPrivilege.ImageCheckDisabled = CType(resources.GetObject("chkPrivilege.ImageCheckDisabled"), Image)
        chkPrivilege.ImageChecked = CType(resources.GetObject("chkPrivilege.ImageChecked"), Image)
        chkPrivilege.ImageUnCheckDisabled = CType(resources.GetObject("chkPrivilege.ImageUnCheckDisabled"), Image)
        chkPrivilege.ImageUnChecked = CType(resources.GetObject("chkPrivilege.ImageUnChecked"), Image)
        chkPrivilege.Location = New Point(647, 24)
        chkPrivilege.Name = "chkPrivilege"
        chkPrivilege.TabIndex = 50
        chkPrivilege.TextValue = "可刪除相片庫的相片"
        chkPrivilege.TextGap = 8
        chkPrivilege.ForeColor = SystemColors.WindowText
        chkPrivilege.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.Transparent
        Label1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label1.ForeColor = SystemColors.WindowText
        Label1.Location = New Point(16, 340)
        Label1.Name = "Label1"
        Label1.Size = New Size(149, 19)
        Label1.TabIndex = 22
        Label1.Text = "攝影輯存放位置"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.BackColor = Color.Transparent
        Label2.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label2.ForeColor = SystemColors.WindowText
        Label2.Location = New Point(12, 24)
        Label2.Name = "Label2"
        Label2.Size = New Size(129, 19)
        Label2.TabIndex = 23
        Label2.Text = "照片存放位置"
        ' 
        ' Label20
        ' 
        ' 
        ' pageAppearance
        ' 
        pageAppearance.BackColor = Color.White
        pageAppearance.Controls.Add(imgSound_0)
        pageAppearance.Controls.Add(imgFontName)
        pageAppearance.Controls.Add(imgSound_1)
        pageAppearance.Controls.Add(imgSound_2)
        pageAppearance.Controls.Add(imgSound_3)
        pageAppearance.Controls.Add(imgSound_4)
        pageAppearance.Controls.Add(txtFont)
        pageAppearance.Controls.Add(txtSound_0)
        pageAppearance.Controls.Add(txtSound_1)
        pageAppearance.Controls.Add(txtSound_2)
        pageAppearance.Controls.Add(txtSound_3)
        pageAppearance.Controls.Add(txtSound_4)
        pageAppearance.Controls.Add(rbStyle_0)
        pageAppearance.Controls.Add(rbStyle_1)
        pageAppearance.Controls.Add(chkSwitchScreen)
        pageAppearance.Controls.Add(Label4)
        pageAppearance.Controls.Add(Line5)
        pageAppearance.Controls.Add(Label16)
        pageAppearance.Controls.Add(Line4)
        pageAppearance.Controls.Add(Label15)
        pageAppearance.Controls.Add(Line3)
        pageAppearance.Controls.Add(Label14)
        pageAppearance.Controls.Add(Line2)
        pageAppearance.Controls.Add(Label3)
        pageAppearance.Controls.Add(Label5)
        pageAppearance.Controls.Add(Label6)
        pageAppearance.Controls.Add(Label7)
        pageAppearance.Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        pageAppearance.ForeColor = SystemColors.ControlText
        pageAppearance.Location = New Point(3, 30)
        pageAppearance.Name = "pageAppearance"
        pageAppearance.Size = New Size(854, 476)
        pageAppearance.TabIndex = 24
        pageAppearance.Title = "外觀/音效"
        ' 
        ' imgSound_0
        ' 
        imgSound_0.BackColor = Color.Transparent
        imgSound_0.Image = CType(resources.GetObject("imgSound_0.Image"), Image)
        imgSound_0.Location = New Point(112, 112)
        imgSound_0.Name = "imgSound_0"
        imgSound_0.Size = New Size(30, 21)
        imgSound_0.TabIndex = 0
        imgSound_0.TabStop = False
        ' 
        ' imgFontName
        ' 
        imgFontName.BackColor = Color.Transparent
        imgFontName.Image = CType(resources.GetObject("imgFontName.Image"), Image)
        imgFontName.Location = New Point(112, 38)
        imgFontName.Name = "imgFontName"
        imgFontName.Size = New Size(30, 21)
        imgFontName.TabIndex = 1
        imgFontName.TabStop = False
        ' 
        ' imgSound_1
        ' 
        imgSound_1.BackColor = Color.Transparent
        imgSound_1.Image = CType(resources.GetObject("imgSound_1.Image"), Image)
        imgSound_1.Location = New Point(112, 168)
        imgSound_1.Name = "imgSound_1"
        imgSound_1.Size = New Size(30, 21)
        imgSound_1.TabIndex = 2
        imgSound_1.TabStop = False
        ' 
        ' imgSound_2
        ' 
        imgSound_2.BackColor = Color.Transparent
        imgSound_2.Image = CType(resources.GetObject("imgSound_2.Image"), Image)
        imgSound_2.Location = New Point(112, 224)
        imgSound_2.Name = "imgSound_2"
        imgSound_2.Size = New Size(30, 21)
        imgSound_2.TabIndex = 3
        imgSound_2.TabStop = False
        ' 
        ' imgSound_3
        ' 
        imgSound_3.BackColor = Color.Transparent
        imgSound_3.Image = CType(resources.GetObject("imgSound_3.Image"), Image)
        imgSound_3.Location = New Point(112, 282)
        imgSound_3.Name = "imgSound_3"
        imgSound_3.Size = New Size(30, 21)
        imgSound_3.TabIndex = 4
        imgSound_3.TabStop = False
        ' 
        ' imgSound_4
        ' 
        imgSound_4.BackColor = Color.Transparent
        imgSound_4.Image = CType(resources.GetObject("imgSound_4.Image"), Image)
        imgSound_4.Location = New Point(112, 338)
        imgSound_4.Name = "imgSound_4"
        imgSound_4.Size = New Size(30, 21)
        imgSound_4.TabIndex = 5
        imgSound_4.TabStop = False
        ' 
        ' txtFont
        ' 
        txtFont.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtFont.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtFont.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtFont.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtFont.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtFont.Location = New Point(26, 62)
        txtFont.Name = "txtFont"
        txtFont.PasswordChar = ChrW(0)
        txtFont.RowActive = True
        txtFont.SelLength = 0
        txtFont.SelStart = 0
        txtFont.SelText = ""
        txtFont.Size = New Size(810, 27)
        txtFont.SoundFileOfEnterFocus = ""
        txtFont.SoundFileOfExitFocus = ""
        txtFont.TabIndex = 5
        txtFont.TabStop = False
        ' 
        ' txtSound_0
        ' 
        txtSound_0.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtSound_0.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtSound_0.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtSound_0.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtSound_0.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtSound_0.Location = New Point(26, 136)
        txtSound_0.Name = "txtSound_0"
        txtSound_0.PasswordChar = ChrW(0)
        txtSound_0.RowActive = True
        txtSound_0.SelLength = 0
        txtSound_0.SelStart = 0
        txtSound_0.SelText = ""
        txtSound_0.Size = New Size(810, 27)
        txtSound_0.SoundFileOfEnterFocus = ""
        txtSound_0.SoundFileOfExitFocus = ""
        txtSound_0.TabIndex = 6
        txtSound_0.TabStop = False
        ' 
        ' txtSound_1
        ' 
        txtSound_1.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtSound_1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtSound_1.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtSound_1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtSound_1.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtSound_1.Location = New Point(26, 192)
        txtSound_1.Name = "txtSound_1"
        txtSound_1.PasswordChar = ChrW(0)
        txtSound_1.RowActive = True
        txtSound_1.SelLength = 0
        txtSound_1.SelStart = 0
        txtSound_1.SelText = ""
        txtSound_1.Size = New Size(810, 27)
        txtSound_1.SoundFileOfEnterFocus = ""
        txtSound_1.SoundFileOfExitFocus = ""
        txtSound_1.TabIndex = 7
        txtSound_1.TabStop = False
        ' 
        ' txtSound_2
        ' 
        txtSound_2.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtSound_2.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtSound_2.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtSound_2.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtSound_2.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtSound_2.Location = New Point(26, 248)
        txtSound_2.Name = "txtSound_2"
        txtSound_2.PasswordChar = ChrW(0)
        txtSound_2.RowActive = True
        txtSound_2.SelLength = 0
        txtSound_2.SelStart = 0
        txtSound_2.SelText = ""
        txtSound_2.Size = New Size(810, 27)
        txtSound_2.SoundFileOfEnterFocus = ""
        txtSound_2.SoundFileOfExitFocus = ""
        txtSound_2.TabIndex = 8
        txtSound_2.TabStop = False
        ' 
        ' txtSound_3
        ' 
        txtSound_3.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtSound_3.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtSound_3.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtSound_3.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtSound_3.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtSound_3.Location = New Point(26, 306)
        txtSound_3.Name = "txtSound_3"
        txtSound_3.PasswordChar = ChrW(0)
        txtSound_3.RowActive = True
        txtSound_3.SelLength = 0
        txtSound_3.SelStart = 0
        txtSound_3.SelText = ""
        txtSound_3.Size = New Size(810, 27)
        txtSound_3.SoundFileOfEnterFocus = ""
        txtSound_3.SoundFileOfExitFocus = ""
        txtSound_3.TabIndex = 9
        txtSound_3.TabStop = False
        ' 
        ' txtSound_4
        ' 
        txtSound_4.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtSound_4.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtSound_4.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtSound_4.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtSound_4.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtSound_4.Location = New Point(26, 362)
        txtSound_4.Name = "txtSound_4"
        txtSound_4.PasswordChar = ChrW(0)
        txtSound_4.RowActive = True
        txtSound_4.SelLength = 0
        txtSound_4.SelStart = 0
        txtSound_4.SelText = ""
        txtSound_4.Size = New Size(810, 27)
        txtSound_4.SoundFileOfEnterFocus = ""
        txtSound_4.SoundFileOfExitFocus = ""
        txtSound_4.TabIndex = 10
        txtSound_4.TabStop = False
        ' 
        ' rbStyle_0
        ' 
        rbStyle_0.BackColor = Color.Transparent
        rbStyle_0.Checked = True
        rbStyle_0.GroupName = "S"
        rbStyle_0.ImageCheckDisabled = CType(resources.GetObject("rbStyle_0.ImageCheckDisabled"), Image)
        rbStyle_0.ImageChecked = CType(resources.GetObject("rbStyle_0.ImageChecked"), Image)
        rbStyle_0.ImageUnCheckDisabled = CType(resources.GetObject("rbStyle_0.ImageUnCheckDisabled"), Image)
        rbStyle_0.ImageUnChecked = CType(resources.GetObject("rbStyle_0.ImageUnChecked"), Image)
        rbStyle_0.Location = New Point(606, 282)
        rbStyle_0.Name = "rbStyle_0"
        rbStyle_0.TabIndex = 48
        rbStyle_0.Visible = False
        rbStyle_0.TextValue = "簡易"
        rbStyle_0.TextGap = 6
        rbStyle_0.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' rbStyle_1
        ' 
        rbStyle_1.BackColor = Color.Transparent
        rbStyle_1.Checked = False
        rbStyle_1.GroupName = "S"
        rbStyle_1.ImageCheckDisabled = CType(resources.GetObject("rbStyle_1.ImageCheckDisabled"), Image)
        rbStyle_1.ImageChecked = CType(resources.GetObject("rbStyle_1.ImageChecked"), Image)
        rbStyle_1.ImageUnCheckDisabled = CType(resources.GetObject("rbStyle_1.ImageUnCheckDisabled"), Image)
        rbStyle_1.ImageUnChecked = CType(resources.GetObject("rbStyle_1.ImageUnChecked"), Image)
        rbStyle_1.Location = New Point(684, 282)
        rbStyle_1.Name = "rbStyle_1"
        rbStyle_1.TabIndex = 49
        rbStyle_1.Visible = False
        rbStyle_1.TextValue = "進階"
        rbStyle_1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' chkSwitchScreen
        ' 
        chkSwitchScreen.BackColor = Color.Transparent
        chkSwitchScreen.Checked = False
        chkSwitchScreen.ImageCheckDisabled = CType(resources.GetObject("chkSwitchScreen.ImageCheckDisabled"), Image)
        chkSwitchScreen.ImageChecked = CType(resources.GetObject("chkSwitchScreen.ImageChecked"), Image)
        chkSwitchScreen.ImageUnCheckDisabled = CType(resources.GetObject("chkSwitchScreen.ImageUnCheckDisabled"), Image)
        chkSwitchScreen.ImageUnChecked = CType(resources.GetObject("chkSwitchScreen.ImageUnChecked"), Image)
        chkSwitchScreen.Location = New Point(420, 20)
        chkSwitchScreen.Name = "chkSwitchScreen"
        chkSwitchScreen.TabIndex = 56
        chkSwitchScreen.TabStop = False
        chkSwitchScreen.Visible = False
        chkSwitchScreen.TextValue = "左右螢幕對調"
        chkSwitchScreen.TextGap = 6
        chkSwitchScreen.ForeColor = System.Drawing.SystemColors.WindowText
        chkSwitchScreen.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.BackColor = Color.Transparent
        Label4.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label4.Location = New Point(26, 112)
        Label4.Name = "Label4"
        Label4.Size = New Size(89, 19)
        Label4.TabIndex = 26
        Label4.Text = "匯入完成"
        ' 
        ' Line5
        ' 
        Line5.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(192))
        Line5.Location = New Point(406, 104)
        Line5.Name = "Line5"
        Line5.Size = New Size(362, 1)
        Line5.TabIndex = 57
        ' 
        ' Label16
        ' 
        Label16.AutoSize = True
        Label16.BackColor = Color.Transparent
        Label16.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label16.ForeColor = Color.FromArgb(CByte(128), CByte(128), CByte(128))
        Label16.Location = New Point(358, 90)
        Label16.Name = "Label16"
        Label16.Size = New Size(49, 19)
        Label16.TabIndex = 27
        Label16.Text = "音效"
        ' 
        ' Line4
        ' 
        Line4.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(192))
        Line4.Location = New Point(22, 102)
        Line4.Name = "Line4"
        Line4.Size = New Size(326, 1)
        Line4.TabIndex = 58
        ' 
        ' Label15
        ' 
        Label15.AutoSize = True
        Label15.BackColor = Color.Transparent
        Label15.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label15.Location = New Point(26, 38)
        Label15.Name = "Label15"
        Label15.Size = New Size(89, 19)
        Label15.TabIndex = 28
        Label15.Text = "顯示字型"
        ' 
        ' Line3
        ' 
        Line3.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(192))
        Line3.Location = New Point(402, 30)
        Line3.Name = "Line3"
        Line3.Size = New Size(366, 1)
        Line3.TabIndex = 59
        ' 
        ' Label14
        ' 
        Label14.AutoSize = True
        Label14.BackColor = Color.Transparent
        Label14.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label14.ForeColor = Color.FromArgb(CByte(128), CByte(128), CByte(128))
        Label14.Location = New Point(358, 20)
        Label14.Name = "Label14"
        Label14.Size = New Size(49, 19)
        Label14.TabIndex = 29
        Label14.Text = "外觀"
        ' 
        ' Line2
        ' 
        Line2.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(192))
        Line2.Location = New Point(20, 30)
        Line2.Name = "Line2"
        Line2.Size = New Size(332, 1)
        Line2.TabIndex = 60
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.BackColor = Color.Transparent
        Label3.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label3.Location = New Point(26, 168)
        Label3.Name = "Label3"
        Label3.Size = New Size(89, 19)
        Label3.TabIndex = 30
        Label3.Text = "匯出完成"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.BackColor = Color.Transparent
        Label5.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label5.Location = New Point(26, 224)
        Label5.Name = "Label5"
        Label5.Size = New Size(89, 19)
        Label5.TabIndex = 31
        Label5.Text = "滑鼠移入"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.BackColor = Color.Transparent
        Label6.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label6.Location = New Point(26, 282)
        Label6.Name = "Label6"
        Label6.Size = New Size(89, 19)
        Label6.TabIndex = 32
        Label6.Text = "滑鼠移出"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.BackColor = Color.Transparent
        Label7.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label7.Location = New Point(26, 338)
        Label7.Name = "Label7"
        Label7.Size = New Size(89, 19)
        Label7.TabIndex = 33
        Label7.Text = "滑鼠按下"
        ' 
        ' Label24
        ' 
        ' 
        ' Label23
        ' 
        ' 
        ' lblSwitchScreen
        ' 
        ' 
        ' pagePlugins
        ' 
        pagePlugins.BackColor = Color.White
        pagePlugins.Controls.Add(imgApp_0)
        pagePlugins.Controls.Add(imgApp_1)
        pagePlugins.Controls.Add(imgApp_2)
        pagePlugins.Controls.Add(imgApp_3)
        pagePlugins.Controls.Add(imgApp_4)
        pagePlugins.Controls.Add(imgApp_5)
        pagePlugins.Controls.Add(txtApp_0)
        pagePlugins.Controls.Add(txtApp_1)
        pagePlugins.Controls.Add(txtApp_2)
        pagePlugins.Controls.Add(txtApp_3)
        pagePlugins.Controls.Add(txtApp_4)
        pagePlugins.Controls.Add(txtApp_5)
        pagePlugins.Controls.Add(Label8)
        pagePlugins.Controls.Add(Label9)
        pagePlugins.Controls.Add(Label10)
        pagePlugins.Controls.Add(Label11)
        pagePlugins.Controls.Add(Label12)
        pagePlugins.Controls.Add(Label13)
        pagePlugins.Font = New Font("新細明體", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        pagePlugins.ForeColor = SystemColors.ControlText
        pagePlugins.Location = New Point(3, 30)
        pagePlugins.Name = "pagePlugins"
        pagePlugins.Size = New Size(854, 476)
        pagePlugins.TabIndex = 25
        pagePlugins.Title = "外掛程式"
        ' 
        ' imgApp_0
        ' 
        imgApp_0.BackColor = Color.Transparent
        imgApp_0.Image = CType(resources.GetObject("imgApp_0.Image"), Image)
        imgApp_0.Location = New Point(104, 24)
        imgApp_0.Name = "imgApp_0"
        imgApp_0.Size = New Size(30, 21)
        imgApp_0.TabIndex = 0
        imgApp_0.TabStop = False
        ' 
        ' imgApp_1
        ' 
        imgApp_1.BackColor = Color.Transparent
        imgApp_1.Image = CType(resources.GetObject("imgApp_1.Image"), Image)
        imgApp_1.Location = New Point(104, 85)
        imgApp_1.Name = "imgApp_1"
        imgApp_1.Size = New Size(30, 21)
        imgApp_1.TabIndex = 1
        imgApp_1.TabStop = False
        ' 
        ' imgApp_2
        ' 
        imgApp_2.BackColor = Color.Transparent
        imgApp_2.Image = CType(resources.GetObject("imgApp_2.Image"), Image)
        imgApp_2.Location = New Point(104, 146)
        imgApp_2.Name = "imgApp_2"
        imgApp_2.Size = New Size(30, 21)
        imgApp_2.TabIndex = 2
        imgApp_2.TabStop = False
        ' 
        ' imgApp_3
        ' 
        imgApp_3.BackColor = Color.Transparent
        imgApp_3.Image = CType(resources.GetObject("imgApp_3.Image"), Image)
        imgApp_3.Location = New Point(104, 208)
        imgApp_3.Name = "imgApp_3"
        imgApp_3.Size = New Size(30, 21)
        imgApp_3.TabIndex = 3
        imgApp_3.TabStop = False
        ' 
        ' imgApp_4
        ' 
        imgApp_4.BackColor = Color.Transparent
        imgApp_4.Image = CType(resources.GetObject("imgApp_4.Image"), Image)
        imgApp_4.Location = New Point(104, 269)
        imgApp_4.Name = "imgApp_4"
        imgApp_4.Size = New Size(30, 21)
        imgApp_4.TabIndex = 4
        imgApp_4.TabStop = False
        ' 
        ' imgApp_5
        ' 
        imgApp_5.BackColor = Color.Transparent
        imgApp_5.Image = CType(resources.GetObject("imgApp_5.Image"), Image)
        imgApp_5.Location = New Point(104, 330)
        imgApp_5.Name = "imgApp_5"
        imgApp_5.Size = New Size(30, 21)
        imgApp_5.TabIndex = 5
        imgApp_5.TabStop = False
        ' 
        ' txtApp_0
        ' 
        txtApp_0.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtApp_0.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtApp_0.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtApp_0.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtApp_0.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtApp_0.Location = New Point(14, 48)
        txtApp_0.Name = "txtApp_0"
        txtApp_0.PasswordChar = ChrW(0)
        txtApp_0.RowActive = True
        txtApp_0.SelLength = 0
        txtApp_0.SelStart = 0
        txtApp_0.SelText = ""
        txtApp_0.Size = New Size(838, 27)
        txtApp_0.SoundFileOfEnterFocus = ""
        txtApp_0.SoundFileOfExitFocus = ""
        txtApp_0.TabIndex = 11
        txtApp_0.TabStop = False
        ' 
        ' txtApp_1
        ' 
        txtApp_1.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtApp_1.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtApp_1.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtApp_1.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtApp_1.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtApp_1.Location = New Point(14, 109)
        txtApp_1.Name = "txtApp_1"
        txtApp_1.PasswordChar = ChrW(0)
        txtApp_1.RowActive = True
        txtApp_1.SelLength = 0
        txtApp_1.SelStart = 0
        txtApp_1.SelText = ""
        txtApp_1.Size = New Size(838, 27)
        txtApp_1.SoundFileOfEnterFocus = ""
        txtApp_1.SoundFileOfExitFocus = ""
        txtApp_1.TabIndex = 12
        txtApp_1.TabStop = False
        ' 
        ' txtApp_2
        ' 
        txtApp_2.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtApp_2.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtApp_2.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtApp_2.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtApp_2.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtApp_2.Location = New Point(14, 170)
        txtApp_2.Name = "txtApp_2"
        txtApp_2.PasswordChar = ChrW(0)
        txtApp_2.RowActive = True
        txtApp_2.SelLength = 0
        txtApp_2.SelStart = 0
        txtApp_2.SelText = ""
        txtApp_2.Size = New Size(838, 27)
        txtApp_2.SoundFileOfEnterFocus = ""
        txtApp_2.SoundFileOfExitFocus = ""
        txtApp_2.TabIndex = 13
        txtApp_2.TabStop = False
        ' 
        ' txtApp_3
        ' 
        txtApp_3.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtApp_3.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtApp_3.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtApp_3.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtApp_3.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtApp_3.Location = New Point(14, 232)
        txtApp_3.Name = "txtApp_3"
        txtApp_3.PasswordChar = ChrW(0)
        txtApp_3.RowActive = True
        txtApp_3.SelLength = 0
        txtApp_3.SelStart = 0
        txtApp_3.SelText = ""
        txtApp_3.Size = New Size(838, 27)
        txtApp_3.SoundFileOfEnterFocus = ""
        txtApp_3.SoundFileOfExitFocus = ""
        txtApp_3.TabIndex = 14
        txtApp_3.TabStop = False
        ' 
        ' txtApp_4
        ' 
        txtApp_4.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtApp_4.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtApp_4.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtApp_4.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtApp_4.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtApp_4.Location = New Point(14, 293)
        txtApp_4.Name = "txtApp_4"
        txtApp_4.PasswordChar = ChrW(0)
        txtApp_4.RowActive = True
        txtApp_4.SelLength = 0
        txtApp_4.SelStart = 0
        txtApp_4.SelText = ""
        txtApp_4.Size = New Size(838, 27)
        txtApp_4.SoundFileOfEnterFocus = ""
        txtApp_4.SoundFileOfExitFocus = ""
        txtApp_4.TabIndex = 15
        txtApp_4.TabStop = False
        ' 
        ' txtApp_5
        ' 
        txtApp_5.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        txtApp_5.BorderColor = Color.FromArgb(CByte(189), CByte(189), CByte(189))
        txtApp_5.BorderFocusColor = Color.FromArgb(CByte(159), CByte(182), CByte(244))
        txtApp_5.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        txtApp_5.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        txtApp_5.Location = New Point(14, 354)
        txtApp_5.Name = "txtApp_5"
        txtApp_5.PasswordChar = ChrW(0)
        txtApp_5.RowActive = True
        txtApp_5.SelLength = 0
        txtApp_5.SelStart = 0
        txtApp_5.SelText = ""
        txtApp_5.Size = New Size(838, 27)
        txtApp_5.SoundFileOfEnterFocus = ""
        txtApp_5.SoundFileOfExitFocus = ""
        txtApp_5.TabIndex = 16
        txtApp_5.TabStop = False
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.BackColor = Color.Transparent
        Label8.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label8.ForeColor = SystemColors.WindowText
        Label8.Location = New Point(16, 26)
        Label8.Name = "Label8"
        Label8.Size = New Size(89, 19)
        Label8.TabIndex = 34
        Label8.Text = "列印輸出"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.BackColor = Color.Transparent
        Label9.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label9.ForeColor = SystemColors.WindowText
        Label9.Location = New Point(16, 87)
        Label9.Name = "Label9"
        Label9.Size = New Size(89, 19)
        Label9.TabIndex = 35
        Label9.Text = "影像編輯"
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.BackColor = Color.Transparent
        Label10.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label10.ForeColor = SystemColors.WindowText
        Label10.Location = New Point(16, 148)
        Label10.Name = "Label10"
        Label10.Size = New Size(89, 19)
        Label10.TabIndex = 36
        Label10.Text = "影片編輯"
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.BackColor = Color.Transparent
        Label11.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label11.ForeColor = SystemColors.WindowText
        Label11.Location = New Point(16, 210)
        Label11.Name = "Label11"
        Label11.Size = New Size(89, 19)
        Label11.TabIndex = 37
        Label11.Text = "電子郵件"
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.BackColor = Color.Transparent
        Label12.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label12.ForeColor = SystemColors.WindowText
        Label12.Location = New Point(16, 271)
        Label12.Name = "Label12"
        Label12.Size = New Size(89, 19)
        Label12.TabIndex = 38
        Label12.Text = "網頁建置"
        ' 
        ' Label13
        ' 
        Label13.AutoSize = True
        Label13.BackColor = Color.Transparent
        Label13.Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        Label13.ForeColor = SystemColors.WindowText
        Label13.Location = New Point(16, 332)
        Label13.Name = "Label13"
        Label13.Size = New Size(89, 19)
        Label13.TabIndex = 39
        Label13.Text = "燒錄軟體"
        ' 
        ' frmSetup
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = SystemColors.Window
        ClientSize = New Size(930, 605)
        Controls.Add(butSave)
        Controls.Add(butExit)
        Controls.Add(tabSetup)
        Font = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ForeColor = SystemColors.ControlText
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Image = CType(resources.GetObject("$this.Image"), Image)
        MaxButton = False
        MenuFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        MinButton = False
        Name = "frmSetup"
        ShowInTaskbar = False
        SizeMode = Aqua.ImageSizeMode.Appose
        StartPosition = FormStartPosition.CenterScreen
        Text = "iPhoto 基本設置"
        TitleFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        WindowBorderStyle = Aqua.FormBorderStyle.Fixed
        tabSetup.ResumeLayout(False)
        pageGeneral.ResumeLayout(False)
        pageGeneral.PerformLayout()
        CType(imgMusic, ComponentModel.ISupportInitialize).EndInit()
        CType(imgAttached_1, ComponentModel.ISupportInitialize).EndInit()
        CType(imgAttached_2, ComponentModel.ISupportInitialize).EndInit()
        CType(imgAttached_0, ComponentModel.ISupportInitialize).EndInit()
        CType(imgRebuild, ComponentModel.ISupportInitialize).EndInit()
        CType(imgAttached_3, ComponentModel.ISupportInitialize).EndInit()
        pageAlbums.ResumeLayout(False)
        pageAlbums.PerformLayout()
        CType(imgFavorites, ComponentModel.ISupportInitialize).EndInit()
        pageAppearance.ResumeLayout(False)
        pageAppearance.PerformLayout()
        CType(imgSound_0, ComponentModel.ISupportInitialize).EndInit()
        CType(imgFontName, ComponentModel.ISupportInitialize).EndInit()
        CType(imgSound_1, ComponentModel.ISupportInitialize).EndInit()
        CType(imgSound_2, ComponentModel.ISupportInitialize).EndInit()
        CType(imgSound_3, ComponentModel.ISupportInitialize).EndInit()
        CType(imgSound_4, ComponentModel.ISupportInitialize).EndInit()
        pagePlugins.ResumeLayout(False)
        pagePlugins.PerformLayout()
        CType(imgApp_0, ComponentModel.ISupportInitialize).EndInit()
        CType(imgApp_1, ComponentModel.ISupportInitialize).EndInit()
        CType(imgApp_2, ComponentModel.ISupportInitialize).EndInit()
        CType(imgApp_3, ComponentModel.ISupportInitialize).EndInit()
        CType(imgApp_4, ComponentModel.ISupportInitialize).EndInit()
        CType(imgApp_5, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)

    End Sub

    Friend WithEvents butSave As Aqua.FlashButton
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents tabSetup As Aqua.TabControl
    Friend WithEvents pagePlugins As Aqua.TabPage
    Friend WithEvents imgApp_0 As System.Windows.Forms.PictureBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents imgApp_1 As System.Windows.Forms.PictureBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents imgApp_2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents imgApp_3 As System.Windows.Forms.PictureBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents imgApp_4 As System.Windows.Forms.PictureBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents imgApp_5 As System.Windows.Forms.PictureBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents txtApp_0 As Aqua.TextBox
    Friend WithEvents txtApp_1 As Aqua.TextBox
    Friend WithEvents txtApp_2 As Aqua.TextBox
    Friend WithEvents txtApp_3 As Aqua.TextBox
    Friend WithEvents txtApp_4 As Aqua.TextBox
    Friend WithEvents txtApp_5 As Aqua.TextBox
    Friend WithEvents pageAlbums As Aqua.TabPage
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents imgFavorites As System.Windows.Forms.PictureBox
    Friend WithEvents lstAlbum As Aqua.ItemListBox
    Friend WithEvents txtFavorite As Aqua.TextBox
    Friend WithEvents imgAlbums_0 As Aqua.IconBox
    Friend WithEvents imgAlbums_1 As Aqua.IconBox
    Friend WithEvents imgAlbums_2 As Aqua.IconBox
    Friend WithEvents imgAlbums_3 As Aqua.IconBox
    Friend WithEvents chkPrivilege As Aqua.CheckBox
    Friend WithEvents pageGeneral As Aqua.TabPage
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents imgMusic As System.Windows.Forms.PictureBox
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents imgAttached_1 As System.Windows.Forms.PictureBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents imgAttached_2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents imgAttached_0 As System.Windows.Forms.PictureBox
    Friend WithEvents imgRebuild As System.Windows.Forms.PictureBox
    Friend WithEvents Label25 As System.Windows.Forms.Label
    Friend WithEvents imgAttached_3 As System.Windows.Forms.PictureBox
    Friend WithEvents sliSlide As Aqua.Slider
    Friend WithEvents txtAttached_2 As Aqua.TextBox
    Friend WithEvents txtAttached_3 As Aqua.TextBox
    Friend WithEvents txtAttached_0 As Aqua.TextBox
    Friend WithEvents txtAttached_1 As Aqua.TextBox
    Friend WithEvents txtMusic As Aqua.TextBox
    Friend WithEvents pageAppearance As Aqua.TabPage
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents imgSound_0 As System.Windows.Forms.PictureBox
    Friend WithEvents Line5 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents Line4 As System.Windows.Forms.Label
    Friend WithEvents imgFontName As System.Windows.Forms.PictureBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Line3 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Line2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents imgSound_1 As System.Windows.Forms.PictureBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents imgSound_2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents imgSound_3 As System.Windows.Forms.PictureBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents imgSound_4 As System.Windows.Forms.PictureBox
    Friend WithEvents txtFont As Aqua.TextBox
    Friend WithEvents txtSound_0 As Aqua.TextBox
    Friend WithEvents txtSound_1 As Aqua.TextBox
    Friend WithEvents txtSound_2 As Aqua.TextBox
    Friend WithEvents txtSound_3 As Aqua.TextBox
    Friend WithEvents txtSound_4 As Aqua.TextBox
    Friend WithEvents rbStyle_0 As Aqua.RadioButton
    Friend WithEvents rbStyle_1 As Aqua.RadioButton
    Friend WithEvents chkSwitchScreen As Aqua.CheckBox
    Friend WithEvents vb6ToolTip As System.Windows.Forms.ToolTip
End Class
