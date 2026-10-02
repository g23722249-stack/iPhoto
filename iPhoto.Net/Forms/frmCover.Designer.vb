<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmCover
    Inherits Aqua.iForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCover))
        Me.lblSelPoint = New System.Windows.Forms.Label()
        Me.Image1 = New System.Windows.Forms.PictureBox()
        Me.Panel1 = New Aqua.Panel()
        Me.picPhoto = New System.Windows.Forms.Panel()
        Me.imgPhoto = New System.Windows.Forms.PictureBox()
        Me.picPaint = New System.Windows.Forms.PictureBox()
        Me.picTemp = New System.Windows.Forms.PictureBox()
        Me.Panel2 = New Aqua.Panel()
        Me.Picture1 = New System.Windows.Forms.Panel()
        Me.Shape1 = New System.Windows.Forms.Label()
        Me.Text1 = New System.Windows.Forms.TextBox()
        Me.picFace = New System.Windows.Forms.PictureBox()
        Me.butExit = New Aqua.ThinButton()
        Me.butOk = New Aqua.ThinButton()
        Me.Panel1.SuspendLayout()
        Me.picPhoto.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Picture1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblSelPoint
        '
        Me.lblSelPoint.Location = New System.Drawing.Point(30, 390)
        Me.lblSelPoint.Size = New System.Drawing.Size(8, 16)
        Me.lblSelPoint.Name = "lblSelPoint"
        Me.lblSelPoint.TabIndex = 6
        Me.lblSelPoint.BackColor = System.Drawing.Color.Transparent
        Me.lblSelPoint.AutoSize = True
        Me.lblSelPoint.Font = New System.Drawing.Font("細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Image1
        '
        Me.Image1.Location = New System.Drawing.Point(36, 398)
        Me.Image1.Size = New System.Drawing.Size(413, 321)
        Me.Image1.Name = "Image1"
        Me.Image1.BackColor = System.Drawing.Color.Transparent
        Me.Image1.Visible = False
        '
        'imgPhoto
        '
        Me.imgPhoto.Location = New System.Drawing.Point(34, 34)
        Me.imgPhoto.Size = New System.Drawing.Size(315, 207)
        Me.imgPhoto.Name = "imgPhoto"
        Me.imgPhoto.TabIndex = 3
        Me.imgPhoto.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.imgPhoto.BackColor = System.Drawing.SystemColors.Window
        Me.imgPhoto.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgPhoto.Cursor = System.Windows.Forms.Cursors.Cross
        '
        'picPaint
        '
        Me.picPaint.Location = New System.Drawing.Point(0, 0)
        Me.picPaint.Size = New System.Drawing.Size(87, 41)
        Me.picPaint.Name = "picPaint"
        Me.picPaint.TabIndex = 4
        Me.picPaint.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPaint.BackColor = System.Drawing.SystemColors.Window
        Me.picPaint.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPaint.Visible = False
        '
        'picTemp
        '
        Me.picTemp.Location = New System.Drawing.Point(112, 78)
        Me.picTemp.Size = New System.Drawing.Size(315, 207)
        Me.picTemp.Name = "picTemp"
        Me.picTemp.TabIndex = 7
        Me.picTemp.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picTemp.BackColor = System.Drawing.SystemColors.Window
        Me.picTemp.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picTemp.Visible = False
        '
        'picPhoto
        '
        Me.picPhoto.Location = New System.Drawing.Point(2, 2)
        Me.picPhoto.Size = New System.Drawing.Size(469, 343)
        Me.picPhoto.Name = "picPhoto"
        Me.picPhoto.TabIndex = 2
        Me.picPhoto.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picPhoto.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.picPhoto.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picPhoto.Controls.Add(Me.imgPhoto)
        Me.picPhoto.Controls.Add(Me.picPaint)
        Me.picPhoto.Controls.Add(Me.picTemp)
        '
        'Panel1
        '
        Me.Panel1.Location = New System.Drawing.Point(28, 38)
        Me.Panel1.Size = New System.Drawing.Size(473, 347)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.TabIndex = 1
        Me.Panel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Panel1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.Panel1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.Panel1.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Panel1.Controls.Add(Me.picPhoto)
        '
        'Shape1
        '
        Me.Shape1.Location = New System.Drawing.Point(20, 20)
        Me.Shape1.Size = New System.Drawing.Size(335, 251)
        Me.Shape1.Name = "Shape1"
        Me.Shape1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Shape1.BackColor = System.Drawing.Color.Transparent
        '
        'Text1
        '
        Me.Text1.Location = New System.Drawing.Point(20, 282)
        Me.Text1.Size = New System.Drawing.Size(335, 48)
        Me.Text1.Name = "Text1"
        Me.Text1.TabIndex = 9
        Me.Text1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.Text1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.Text1.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.Text1.Text = "請輸入姓名"
        Me.Text1.Font = New System.Drawing.Font("華康細圓體", 36!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'picFace
        '
        Me.picFace.Location = New System.Drawing.Point(26, 26)
        Me.picFace.Size = New System.Drawing.Size(323, 239)
        Me.picFace.Name = "picFace"
        Me.picFace.TabIndex = 10
        Me.picFace.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picFace.BackColor = System.Drawing.SystemColors.Window
        Me.picFace.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picFace.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        '
        'Picture1
        '
        Me.Picture1.Location = New System.Drawing.Point(2, 2)
        Me.Picture1.Size = New System.Drawing.Size(375, 343)
        Me.Picture1.Name = "Picture1"
        Me.Picture1.TabIndex = 8
        Me.Picture1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.Picture1.BackColor = System.Drawing.SystemColors.Window
        Me.Picture1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Picture1.Controls.Add(Me.Text1)
        Me.Picture1.Controls.Add(Me.picFace)
        Me.Picture1.Controls.Add(Me.Shape1)
        '
        'Panel2
        '
        Me.Panel2.Location = New System.Drawing.Point(506, 38)
        Me.Panel2.Size = New System.Drawing.Size(379, 347)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.TabIndex = 5
        Me.Panel2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Panel2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.Panel2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.Panel2.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Panel2.Controls.Add(Me.Picture1)
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(340, 408)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 11
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butExit.ForeColor = System.Drawing.Color.Black
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(460, 408)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 12
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butOk.ForeColor = System.Drawing.Color.Black
        '
        'frmCover
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(907, 458)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.SizeMode = Aqua.ImageSizeMode.StretchImage
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "面孔"
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmCover"
        Me.Controls.Add(Me.Image1)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.lblSelPoint)
        Me.Picture1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.picPhoto.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblSelPoint As System.Windows.Forms.Label
    Friend WithEvents Image1 As System.Windows.Forms.PictureBox
    Friend WithEvents Panel1 As Aqua.Panel
    Friend WithEvents picPhoto As System.Windows.Forms.Panel
    Friend WithEvents imgPhoto As System.Windows.Forms.PictureBox
    Friend WithEvents picPaint As System.Windows.Forms.PictureBox
    Friend WithEvents picTemp As System.Windows.Forms.PictureBox
    Friend WithEvents Panel2 As Aqua.Panel
    Friend WithEvents Picture1 As System.Windows.Forms.Panel
    Friend WithEvents Shape1 As System.Windows.Forms.Label
    Friend WithEvents Text1 As System.Windows.Forms.TextBox
    Friend WithEvents picFace As System.Windows.Forms.PictureBox
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents butOk As Aqua.ThinButton
End Class
