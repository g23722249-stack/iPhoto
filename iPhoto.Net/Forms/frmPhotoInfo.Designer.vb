<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPhotoInfo
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPhotoInfo))
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.lblPath = New System.Windows.Forms.Label()
        Me.lblFileName = New System.Windows.Forms.Label()
        Me.lblFileDateTime = New System.Windows.Forms.Label()
        Me.lblFileLength = New System.Windows.Forms.Label()
        Me.butExit = New Aqua.ThinButton()
        Me.butOk = New Aqua.ThinButton()
        Me.txtCharacter = New Aqua.TextBox()
        Me.txtTitle = New Aqua.TextBox()
        Me.txtKeyWord = New Aqua.TextBox()
        Me.txtRemark = New Aqua.TextBox()
        Me.txtSpot = New Aqua.TextBox()
        Me.meDate = New Aqua.MaskEdit()
        Me.meTime = New Aqua.MaskEdit()
        Me.imgKeyWords_0 = New Aqua.PngButton()
        Me.imgKeyWords_1 = New Aqua.PngButton()
        Me.imgKeyWords_2 = New Aqua.PngButton()
        Me.imgKeyWords_3 = New Aqua.PngButton()
        Me.imgKeyWords_4 = New Aqua.PngButton()
        Me.SuspendLayout()
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(62, 40)
        Me.Label5.Size = New System.Drawing.Size(60, 19)
        Me.Label5.Name = "Label5"
        Me.Label5.TabIndex = 16
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.AutoSize = True
        Me.Label5.Text = "位置："
        Me.Label5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(62, 72)
        Me.Label1.Size = New System.Drawing.Size(60, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 17
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "檔名："
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(22, 103)
        Me.Label2.Size = New System.Drawing.Size(100, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 18
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "修改日期："
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(62, 135)
        Me.Label3.Size = New System.Drawing.Size(60, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 19
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "大小："
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(20, 164)
        Me.Line1.Size = New System.Drawing.Size(410, 1)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(62, 272)
        Me.Label7.Size = New System.Drawing.Size(60, 19)
        Me.Label7.Name = "Label7"
        Me.Label7.TabIndex = 6
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.AutoSize = True
        Me.Label7.Text = "時間："
        Me.Label7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(62, 209)
        Me.Label6.Size = New System.Drawing.Size(60, 19)
        Me.Label6.Name = "Label6"
        Me.Label6.TabIndex = 2
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.AutoSize = True
        Me.Label6.Text = "人物："
        Me.Label6.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(62, 304)
        Me.Label4.Size = New System.Drawing.Size(60, 19)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 9
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "備註："
        Me.Label4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label8
        '
        Me.Label8.Location = New System.Drawing.Point(62, 177)
        Me.Label8.Size = New System.Drawing.Size(60, 19)
        Me.Label8.Name = "Label8"
        Me.Label8.TabIndex = 0
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.AutoSize = True
        Me.Label8.Text = "主題："
        Me.Label8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label9
        '
        Me.Label9.Location = New System.Drawing.Point(42, 336)
        Me.Label9.Size = New System.Drawing.Size(80, 19)
        Me.Label9.Name = "Label9"
        Me.Label9.TabIndex = 11
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.AutoSize = True
        Me.Label9.Text = "關鍵字："
        Me.Label9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label10
        '
        Me.Label10.Location = New System.Drawing.Point(62, 240)
        Me.Label10.Size = New System.Drawing.Size(60, 19)
        Me.Label10.Name = "Label10"
        Me.Label10.TabIndex = 4
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.AutoSize = True
        Me.Label10.Text = "地點："
        Me.Label10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblPath
        '
        Me.lblPath.Location = New System.Drawing.Point(124, 40)
        Me.lblPath.Size = New System.Drawing.Size(304, 19)
        Me.lblPath.Name = "lblPath"
        Me.lblPath.TabIndex = 20
        Me.lblPath.BackColor = System.Drawing.Color.Transparent
        Me.lblPath.Text = "資訊"
        Me.lblPath.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblFileName
        '
        Me.lblFileName.Location = New System.Drawing.Point(124, 72)
        Me.lblFileName.Size = New System.Drawing.Size(304, 19)
        Me.lblFileName.Name = "lblFileName"
        Me.lblFileName.TabIndex = 21
        Me.lblFileName.BackColor = System.Drawing.Color.Transparent
        Me.lblFileName.Text = "資訊"
        Me.lblFileName.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblFileDateTime
        '
        Me.lblFileDateTime.Location = New System.Drawing.Point(124, 103)
        Me.lblFileDateTime.Size = New System.Drawing.Size(304, 19)
        Me.lblFileDateTime.Name = "lblFileDateTime"
        Me.lblFileDateTime.TabIndex = 22
        Me.lblFileDateTime.BackColor = System.Drawing.Color.Transparent
        Me.lblFileDateTime.Text = "資訊"
        Me.lblFileDateTime.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblFileLength
        '
        Me.lblFileLength.Location = New System.Drawing.Point(124, 135)
        Me.lblFileLength.Size = New System.Drawing.Size(304, 19)
        Me.lblFileLength.Name = "lblFileLength"
        Me.lblFileLength.TabIndex = 23
        Me.lblFileLength.BackColor = System.Drawing.Color.Transparent
        Me.lblFileLength.Text = "資訊"
        Me.lblFileLength.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(114, 374)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 13
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butExit.ForeColor = System.Drawing.Color.Black
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(234, 374)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 14
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butOk.ForeColor = System.Drawing.Color.Black
        '
        'txtCharacter
        '
        Me.txtCharacter.Location = New System.Drawing.Point(124, 209)
        Me.txtCharacter.Size = New System.Drawing.Size(243, 24)
        Me.txtCharacter.Name = "txtCharacter"
        Me.txtCharacter.TabIndex = 3
        Me.txtCharacter.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtCharacter.MaxLength = 0
        Me.txtCharacter.AutoSelect = True
        Me.txtCharacter.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtCharacter.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtCharacter.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtTitle
        '
        Me.txtTitle.Location = New System.Drawing.Point(124, 177)
        Me.txtTitle.Size = New System.Drawing.Size(243, 24)
        Me.txtTitle.Name = "txtTitle"
        Me.txtTitle.TabIndex = 1
        Me.txtTitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtTitle.MaxLength = 0
        Me.txtTitle.AutoSelect = True
        Me.txtTitle.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtTitle.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtTitle.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtKeyWord
        '
        Me.txtKeyWord.Location = New System.Drawing.Point(124, 336)
        Me.txtKeyWord.Size = New System.Drawing.Size(243, 24)
        Me.txtKeyWord.Name = "txtKeyWord"
        Me.txtKeyWord.TabIndex = 12
        Me.txtKeyWord.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtKeyWord.MaxLength = 0
        Me.txtKeyWord.AutoSelect = True
        Me.txtKeyWord.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtKeyWord.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtKeyWord.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtRemark
        '
        Me.txtRemark.Location = New System.Drawing.Point(124, 304)
        Me.txtRemark.Size = New System.Drawing.Size(243, 24)
        Me.txtRemark.Name = "txtRemark"
        Me.txtRemark.TabIndex = 10
        Me.txtRemark.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtRemark.MaxLength = 0
        Me.txtRemark.AutoSelect = True
        Me.txtRemark.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtRemark.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtRemark.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtSpot
        '
        Me.txtSpot.Location = New System.Drawing.Point(124, 241)
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
        'meDate
        '
        Me.meDate.Location = New System.Drawing.Point(124, 272)
        Me.meDate.Size = New System.Drawing.Size(123, 24)
        Me.meDate.Name = "meDate"
        Me.meDate.TabIndex = 7
        Me.meDate.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.meDate.Mask = "####/##/##"
        Me.meDate.Text = "0000/00/00"
        Me.meDate.Alignment = Aqua.AlignmentConstants.Center
        Me.meDate.AutoSelect = True
        Me.meDate.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.meDate.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.meDate.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'meTime
        '
        Me.meTime.Location = New System.Drawing.Point(256, 272)
        Me.meTime.Size = New System.Drawing.Size(111, 24)
        Me.meTime.Name = "meTime"
        Me.meTime.TabIndex = 8
        Me.meTime.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.meTime.Mask = "##:##:##"
        Me.meTime.Text = "00:00:00"
        Me.meTime.Alignment = Aqua.AlignmentConstants.Center
        Me.meTime.AutoSelect = True
        Me.meTime.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.meTime.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.meTime.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_0
        '
        Me.imgKeyWords_0.Location = New System.Drawing.Point(370, 178)
        Me.imgKeyWords_0.Size = New System.Drawing.Size(21, 21)
        Me.imgKeyWords_0.Name = "imgKeyWords_0"
        Me.imgKeyWords_0.TabIndex = 24
        Me.imgKeyWords_0.TabStop = False
        Me.imgKeyWords_0.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_0.Image = CType(resources.GetObject("imgKeyWords_0.Image"), System.Drawing.Image)
        Me.imgKeyWords_0.HoverZoom = 0!
        Me.imgKeyWords_0.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_1
        '
        Me.imgKeyWords_1.Location = New System.Drawing.Point(370, 212)
        Me.imgKeyWords_1.Size = New System.Drawing.Size(21, 21)
        Me.imgKeyWords_1.Name = "imgKeyWords_1"
        Me.imgKeyWords_1.TabIndex = 25
        Me.imgKeyWords_1.TabStop = False
        Me.imgKeyWords_1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_1.Image = CType(resources.GetObject("imgKeyWords_1.Image"), System.Drawing.Image)
        Me.imgKeyWords_1.HoverZoom = 0!
        Me.imgKeyWords_1.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_2
        '
        Me.imgKeyWords_2.Location = New System.Drawing.Point(370, 244)
        Me.imgKeyWords_2.Size = New System.Drawing.Size(21, 21)
        Me.imgKeyWords_2.Name = "imgKeyWords_2"
        Me.imgKeyWords_2.TabIndex = 26
        Me.imgKeyWords_2.TabStop = False
        Me.imgKeyWords_2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_2.Image = CType(resources.GetObject("imgKeyWords_2.Image"), System.Drawing.Image)
        Me.imgKeyWords_2.HoverZoom = 0!
        Me.imgKeyWords_2.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_3
        '
        Me.imgKeyWords_3.Location = New System.Drawing.Point(370, 306)
        Me.imgKeyWords_3.Size = New System.Drawing.Size(21, 21)
        Me.imgKeyWords_3.Name = "imgKeyWords_3"
        Me.imgKeyWords_3.TabIndex = 27
        Me.imgKeyWords_3.TabStop = False
        Me.imgKeyWords_3.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_3.Image = CType(resources.GetObject("imgKeyWords_3.Image"), System.Drawing.Image)
        Me.imgKeyWords_3.HoverZoom = 0!
        Me.imgKeyWords_3.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgKeyWords_4
        '
        Me.imgKeyWords_4.Location = New System.Drawing.Point(370, 338)
        Me.imgKeyWords_4.Size = New System.Drawing.Size(21, 21)
        Me.imgKeyWords_4.Name = "imgKeyWords_4"
        Me.imgKeyWords_4.TabIndex = 28
        Me.imgKeyWords_4.TabStop = False
        Me.imgKeyWords_4.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgKeyWords_4.Image = CType(resources.GetObject("imgKeyWords_4.Image"), System.Drawing.Image)
        Me.imgKeyWords_4.HoverZoom = 0!
        Me.imgKeyWords_4.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmPhotoInfo
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(451, 417)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "詳細資料"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmPhotoInfo"
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.txtCharacter)
        Me.Controls.Add(Me.txtTitle)
        Me.Controls.Add(Me.txtKeyWord)
        Me.Controls.Add(Me.txtRemark)
        Me.Controls.Add(Me.txtSpot)
        Me.Controls.Add(Me.meDate)
        Me.Controls.Add(Me.meTime)
        Me.Controls.Add(Me.imgKeyWords_0)
        Me.Controls.Add(Me.imgKeyWords_1)
        Me.Controls.Add(Me.imgKeyWords_2)
        Me.Controls.Add(Me.imgKeyWords_3)
        Me.Controls.Add(Me.imgKeyWords_4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Line1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.lblPath)
        Me.Controls.Add(Me.lblFileName)
        Me.Controls.Add(Me.lblFileDateTime)
        Me.Controls.Add(Me.lblFileLength)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents lblPath As System.Windows.Forms.Label
    Friend WithEvents lblFileName As System.Windows.Forms.Label
    Friend WithEvents lblFileDateTime As System.Windows.Forms.Label
    Friend WithEvents lblFileLength As System.Windows.Forms.Label
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents butOk As Aqua.ThinButton
    Friend WithEvents txtCharacter As Aqua.TextBox
    Friend WithEvents txtTitle As Aqua.TextBox
    Friend WithEvents txtKeyWord As Aqua.TextBox
    Friend WithEvents txtRemark As Aqua.TextBox
    Friend WithEvents txtSpot As Aqua.TextBox
    Friend WithEvents meDate As Aqua.MaskEdit
    Friend WithEvents meTime As Aqua.MaskEdit
    Friend WithEvents imgKeyWords_0 As Aqua.PngButton
    Friend WithEvents imgKeyWords_1 As Aqua.PngButton
    Friend WithEvents imgKeyWords_2 As Aqua.PngButton
    Friend WithEvents imgKeyWords_3 As Aqua.PngButton
    Friend WithEvents imgKeyWords_4 As Aqua.PngButton
End Class
