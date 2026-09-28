<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSearch
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSearch))
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.lblTo = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.lblSearching = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.ddRanking = New Aqua.DropDownList()
        Me.ddMediaType = New Aqua.DropDownList()
        Me.butExit = New Aqua.FlashButton()
        Me.butSearch = New Aqua.FlashButton()
        Me.txtRemark = New Aqua.TextBox()
        Me.txtKeyWord = New Aqua.TextBox()
        Me.meStartDate = New Aqua.MaskEdit()
        Me.meEndDate = New Aqua.MaskEdit()
        Me.rbRange_0 = New Aqua.RadioButton()
        Me.rbRange_1 = New Aqua.RadioButton()
        Me.rbRange_2 = New Aqua.RadioButton()
        Me.Loading1 = New Aqua.Loading()
        Me.txtTitle = New Aqua.TextBox()
        Me.txtCharacter = New Aqua.TextBox()
        Me.txtSpot = New Aqua.TextBox()
        Me.imgKeyWords_0 = New Aqua.PngButton()
        Me.imgKeyWords_1 = New Aqua.PngButton()
        Me.imgKeyWords_2 = New Aqua.PngButton()
        Me.imgKeyWords_3 = New Aqua.PngButton()
        Me.imgKeyWords_4 = New Aqua.PngButton()
        Me.SuspendLayout()
        '
        '
        '
        '
        '
        '
        '
        '
        '
        '
        '
        '
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(122, 312)
        Me.Label2.Size = New System.Drawing.Size(80, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 8
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "關鍵字："
        Me.Label2.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(140, 172)
        Me.Label3.Size = New System.Drawing.Size(60, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 0
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "主題："
        Me.Label3.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(142, 347)
        Me.Label4.Size = New System.Drawing.Size(60, 19)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 10
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "評價："
        Me.Label4.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(142, 382)
        Me.Label5.Size = New System.Drawing.Size(60, 19)
        Me.Label5.Name = "Label5"
        Me.Label5.TabIndex = 12
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.AutoSize = True
        Me.Label5.Text = "媒體："
        Me.Label5.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'lblTo
        '
        Me.lblTo.Location = New System.Drawing.Point(352, 112)
        Me.lblTo.Size = New System.Drawing.Size(20, 19)
        Me.lblTo.Name = "lblTo"
        Me.lblTo.TabIndex = 20
        Me.lblTo.BackColor = System.Drawing.Color.Transparent
        Me.lblTo.AutoSize = True
        Me.lblTo.Text = "To"
        Me.lblTo.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(88, 152)
        Me.Line1.Size = New System.Drawing.Size(426, 1)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'lblSearching
        '
        Me.lblSearching.Location = New System.Drawing.Point(34, 418)
        Me.lblSearching.Size = New System.Drawing.Size(90, 19)
        Me.lblSearching.Name = "lblSearching"
        Me.lblSearching.TabIndex = 27
        Me.lblSearching.BackColor = System.Drawing.Color.Transparent
        Me.lblSearching.AutoSize = True
        Me.lblSearching.Text = "Searching"
        Me.lblSearching.Visible = False
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(142, 277)
        Me.Label1.Size = New System.Drawing.Size(60, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 6
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "備註："
        Me.Label1.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(140, 207)
        Me.Label6.Size = New System.Drawing.Size(60, 19)
        Me.Label6.Name = "Label6"
        Me.Label6.TabIndex = 2
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.AutoSize = True
        Me.Label6.Text = "人物："
        Me.Label6.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Label10
        '
        Me.Label10.Location = New System.Drawing.Point(138, 242)
        Me.Label10.Size = New System.Drawing.Size(60, 19)
        Me.Label10.Name = "Label10"
        Me.Label10.TabIndex = 4
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.AutoSize = True
        Me.Label10.Text = "地點："
        Me.Label10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'ddRanking
        '
        Me.ddRanking.Location = New System.Drawing.Point(200, 345)
        Me.ddRanking.Size = New System.Drawing.Size(211, 24)
        Me.ddRanking.Name = "ddRanking"
        Me.ddRanking.TabIndex = 11
        Me.ddRanking.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ddRanking.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.ddRanking.Items.Add(New Aqua.MenuItem("", "全部的相片", 1, Nothing, Nothing))
        Me.ddRanking.Items.Add(New Aqua.MenuItem("1", "", 1, CType(resources.GetObject("imgUnSelRanking_0.Image"), System.Drawing.Image), CType(resources.GetObject("imgSelRanking_0.Image"), System.Drawing.Image)))
        Me.ddRanking.Items.Add(New Aqua.MenuItem("2", "", 1, CType(resources.GetObject("imgUnSelRanking_1.Image"), System.Drawing.Image), CType(resources.GetObject("imgSelRanking_1.Image"), System.Drawing.Image)))
        Me.ddRanking.Items.Add(New Aqua.MenuItem("3", "", 1, CType(resources.GetObject("imgUnSelRanking_2.Image"), System.Drawing.Image), CType(resources.GetObject("imgSelRanking_2.Image"), System.Drawing.Image)))
        Me.ddRanking.Items.Add(New Aqua.MenuItem("4", "", 1, CType(resources.GetObject("imgUnSelRanking_3.Image"), System.Drawing.Image), CType(resources.GetObject("imgSelRanking_3.Image"), System.Drawing.Image)))
        Me.ddRanking.Items.Add(New Aqua.MenuItem("5", "", 1, CType(resources.GetObject("imgUnSelRanking_4.Image"), System.Drawing.Image), CType(resources.GetObject("imgSelRanking_4.Image"), System.Drawing.Image)))
        '
        'ddMediaType
        '
        Me.ddMediaType.Location = New System.Drawing.Point(200, 380)
        Me.ddMediaType.Size = New System.Drawing.Size(211, 24)
        Me.ddMediaType.Name = "ddMediaType"
        Me.ddMediaType.TabIndex = 13
        Me.ddMediaType.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ddMediaType.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.ddMediaType.Items.Add(New Aqua.MenuItem("", "全部的相片", 1, Nothing, Nothing))
        Me.ddMediaType.Items.Add(New Aqua.MenuItem("0", "圖形檔", 1, CType(resources.GetObject("imgImage.Image"), System.Drawing.Image), Nothing))
        Me.ddMediaType.Items.Add(New Aqua.MenuItem("1", "影像檔", 1, CType(resources.GetObject("imgVideo.Image"), System.Drawing.Image), Nothing))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(146, 456)
        Me.butExit.Size = New System.Drawing.Size(135, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 14
        Me.butExit.Text = "結束"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butSearch
        '
        Me.butSearch.Location = New System.Drawing.Point(316, 454)
        Me.butSearch.Size = New System.Drawing.Size(135, 27)
        Me.butSearch.Name = "butSearch"
        Me.butSearch.TabIndex = 15
        Me.butSearch.Text = "尋找相片"
        Me.butSearch.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtRemark
        '
        Me.txtRemark.Location = New System.Drawing.Point(200, 274)
        Me.txtRemark.Size = New System.Drawing.Size(243, 24)
        Me.txtRemark.Name = "txtRemark"
        Me.txtRemark.TabIndex = 7
        Me.txtRemark.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtRemark.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtRemark.MaxLength = 0
        Me.txtRemark.AutoSelect = True
        Me.txtRemark.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtRemark.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtRemark.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtKeyWord
        '
        Me.txtKeyWord.Location = New System.Drawing.Point(200, 309)
        Me.txtKeyWord.Size = New System.Drawing.Size(243, 24)
        Me.txtKeyWord.Name = "txtKeyWord"
        Me.txtKeyWord.TabIndex = 9
        Me.txtKeyWord.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtKeyWord.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtKeyWord.MaxLength = 0
        Me.txtKeyWord.AutoSelect = True
        Me.txtKeyWord.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtKeyWord.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtKeyWord.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'meStartDate
        '
        Me.meStartDate.Location = New System.Drawing.Point(228, 110)
        Me.meStartDate.Size = New System.Drawing.Size(115, 24)
        Me.meStartDate.Name = "meStartDate"
        Me.meStartDate.TabIndex = 18
        Me.meStartDate.TabStop = False
        Me.meStartDate.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.meStartDate.Mask = "####/##/##"
        Me.meStartDate.Text = "0000/00/00"
        Me.meStartDate.Alignment = Aqua.AlignmentConstants.Center
        Me.meStartDate.AutoSelect = True
        Me.meStartDate.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.meStartDate.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.meStartDate.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'meEndDate
        '
        Me.meEndDate.Location = New System.Drawing.Point(384, 110)
        Me.meEndDate.Size = New System.Drawing.Size(115, 24)
        Me.meEndDate.Name = "meEndDate"
        Me.meEndDate.TabIndex = 19
        Me.meEndDate.TabStop = False
        Me.meEndDate.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.meEndDate.Mask = "####/##/##"
        Me.meEndDate.Text = "0000/00/00"
        Me.meEndDate.Alignment = Aqua.AlignmentConstants.Center
        Me.meEndDate.AutoSelect = True
        Me.meEndDate.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.meEndDate.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.meEndDate.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'rbRange_0
        '
        Me.rbRange_0.Location = New System.Drawing.Point(118, 46)
        Me.rbRange_0.Name = "rbRange_0"
        Me.rbRange_0.TabIndex = 21
        Me.rbRange_0.TabStop = False
        Me.rbRange_0.Checked = True
        Me.rbRange_0.TextValue = "全部的相片"
        Me.rbRange_0.TextGap = 8
        Me.rbRange_0.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'rbRange_1
        '
        Me.rbRange_1.Location = New System.Drawing.Point(118, 79)
        Me.rbRange_1.Name = "rbRange_1"
        Me.rbRange_1.TabIndex = 22
        Me.rbRange_1.TabStop = False
        Me.rbRange_1.TextValue = "最近 12 個月內的相片"
        Me.rbRange_1.TextGap = 8
        Me.rbRange_1.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'rbRange_2
        '
        Me.rbRange_2.Location = New System.Drawing.Point(118, 112)
        Me.rbRange_2.Name = "rbRange_2"
        Me.rbRange_2.TabIndex = 23
        Me.rbRange_2.TabStop = False
        Me.rbRange_2.TextValue = "指定日期"
        Me.rbRange_2.TextGap = 8
        Me.rbRange_2.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'Loading1
        '
        Me.Loading1.Location = New System.Drawing.Point(130, 420)
        Me.Loading1.Size = New System.Drawing.Size(431, 16)
        Me.Loading1.Name = "Loading1"
        Me.Loading1.TabIndex = 26
        Me.Loading1.TabStop = False
        Me.Loading1.Visible = False
        '
        'txtTitle
        '
        Me.txtTitle.Location = New System.Drawing.Point(200, 168)
        Me.txtTitle.Size = New System.Drawing.Size(243, 24)
        Me.txtTitle.Name = "txtTitle"
        Me.txtTitle.TabIndex = 1
        Me.txtTitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtTitle.MaxLength = 0
        Me.txtTitle.AutoSelect = True
        Me.txtTitle.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtTitle.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtTitle.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtCharacter
        '
        Me.txtCharacter.Location = New System.Drawing.Point(200, 203)
        Me.txtCharacter.Size = New System.Drawing.Size(243, 24)
        Me.txtCharacter.Name = "txtCharacter"
        Me.txtCharacter.TabIndex = 3
        Me.txtCharacter.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtCharacter.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtCharacter.MaxLength = 0
        Me.txtCharacter.AutoSelect = True
        Me.txtCharacter.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtCharacter.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtCharacter.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtSpot
        '
        Me.txtSpot.Location = New System.Drawing.Point(200, 239)
        Me.txtSpot.Size = New System.Drawing.Size(243, 24)
        Me.txtSpot.Name = "txtSpot"
        Me.txtSpot.TabIndex = 5
        Me.txtSpot.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtSpot.MaxLength = 0
        Me.txtSpot.AutoSelect = True
        Me.txtSpot.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtSpot.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtSpot.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_0
        '
        Me.imgKeyWords_0.Location = New System.Drawing.Point(448, 168)
        Me.imgKeyWords_0.Size = New System.Drawing.Size(1, 1)
        Me.imgKeyWords_0.Name = "imgKeyWords_0"
        Me.imgKeyWords_0.TabIndex = 28
        Me.imgKeyWords_0.TabStop = False
        Me.imgKeyWords_0.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_0.Image = CType(resources.GetObject("imgKeyWords_0.Image"), System.Drawing.Image)
        Me.imgKeyWords_0.HoverZoom = 0!
        Me.imgKeyWords_0.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_1
        '
        Me.imgKeyWords_1.Location = New System.Drawing.Point(448, 203)
        Me.imgKeyWords_1.Size = New System.Drawing.Size(1, 1)
        Me.imgKeyWords_1.Name = "imgKeyWords_1"
        Me.imgKeyWords_1.TabIndex = 29
        Me.imgKeyWords_1.TabStop = False
        Me.imgKeyWords_1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_1.Image = CType(resources.GetObject("imgKeyWords_1.Image"), System.Drawing.Image)
        Me.imgKeyWords_1.HoverZoom = 0!
        Me.imgKeyWords_1.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_2
        '
        Me.imgKeyWords_2.Location = New System.Drawing.Point(448, 239)
        Me.imgKeyWords_2.Size = New System.Drawing.Size(1, 1)
        Me.imgKeyWords_2.Name = "imgKeyWords_2"
        Me.imgKeyWords_2.TabIndex = 30
        Me.imgKeyWords_2.TabStop = False
        Me.imgKeyWords_2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_2.Image = CType(resources.GetObject("imgKeyWords_2.Image"), System.Drawing.Image)
        Me.imgKeyWords_2.HoverZoom = 0!
        Me.imgKeyWords_2.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_3
        '
        Me.imgKeyWords_3.Location = New System.Drawing.Point(448, 274)
        Me.imgKeyWords_3.Size = New System.Drawing.Size(1, 1)
        Me.imgKeyWords_3.Name = "imgKeyWords_3"
        Me.imgKeyWords_3.TabIndex = 31
        Me.imgKeyWords_3.TabStop = False
        Me.imgKeyWords_3.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_3.Image = CType(resources.GetObject("imgKeyWords_3.Image"), System.Drawing.Image)
        Me.imgKeyWords_3.HoverZoom = 0!
        Me.imgKeyWords_3.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_4
        '
        Me.imgKeyWords_4.Location = New System.Drawing.Point(448, 310)
        Me.imgKeyWords_4.Size = New System.Drawing.Size(1, 1)
        Me.imgKeyWords_4.Name = "imgKeyWords_4"
        Me.imgKeyWords_4.TabIndex = 32
        Me.imgKeyWords_4.TabStop = False
        Me.imgKeyWords_4.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_4.Image = CType(resources.GetObject("imgKeyWords_4.Image"), System.Drawing.Image)
        Me.imgKeyWords_4.HoverZoom = 0!
        Me.imgKeyWords_4.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmSearch
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(593, 509)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "搜尋"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmSearch"
        Me.Controls.Add(Me.ddRanking)
        Me.Controls.Add(Me.ddMediaType)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butSearch)
        Me.Controls.Add(Me.txtRemark)
        Me.Controls.Add(Me.txtKeyWord)
        Me.Controls.Add(Me.meStartDate)
        Me.Controls.Add(Me.meEndDate)
        Me.Controls.Add(Me.rbRange_0)
        Me.Controls.Add(Me.rbRange_1)
        Me.Controls.Add(Me.rbRange_2)
        Me.Controls.Add(Me.Loading1)
        Me.Controls.Add(Me.txtTitle)
        Me.Controls.Add(Me.txtCharacter)
        Me.Controls.Add(Me.txtSpot)
        Me.Controls.Add(Me.imgKeyWords_0)
        Me.Controls.Add(Me.imgKeyWords_1)
        Me.Controls.Add(Me.imgKeyWords_2)
        Me.Controls.Add(Me.imgKeyWords_3)
        Me.Controls.Add(Me.imgKeyWords_4)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.lblTo)
        Me.Controls.Add(Me.Line1)
        Me.Controls.Add(Me.lblSearching)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label10)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents lblTo As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents lblSearching As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents ddRanking As Aqua.DropDownList
    Friend WithEvents ddMediaType As Aqua.DropDownList
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butSearch As Aqua.FlashButton
    Friend WithEvents txtRemark As Aqua.TextBox
    Friend WithEvents txtKeyWord As Aqua.TextBox
    Friend WithEvents meStartDate As Aqua.MaskEdit
    Friend WithEvents meEndDate As Aqua.MaskEdit
    Friend WithEvents rbRange_0 As Aqua.RadioButton
    Friend WithEvents rbRange_1 As Aqua.RadioButton
    Friend WithEvents rbRange_2 As Aqua.RadioButton
    Friend WithEvents Loading1 As Aqua.Loading
    Friend WithEvents txtTitle As Aqua.TextBox
    Friend WithEvents txtCharacter As Aqua.TextBox
    Friend WithEvents txtSpot As Aqua.TextBox
    Friend WithEvents imgKeyWords_0 As Aqua.PngButton
    Friend WithEvents imgKeyWords_1 As Aqua.PngButton
    Friend WithEvents imgKeyWords_2 As Aqua.PngButton
    Friend WithEvents imgKeyWords_3 As Aqua.PngButton
    Friend WithEvents imgKeyWords_4 As Aqua.PngButton
End Class
