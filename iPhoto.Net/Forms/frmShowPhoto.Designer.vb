<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmShowPhoto
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmShowPhoto))
        Me.lblFileLength = New System.Windows.Forms.Label()
        Me.lblFileDateTime = New System.Windows.Forms.Label()
        Me.lblFilePath = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblCharacter = New System.Windows.Forms.Label()
        Me.lblSopt = New System.Windows.Forms.Label()
        Me.lblDateTime = New System.Windows.Forms.Label()
        Me.lblRemark = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.lblKeyWord = New System.Windows.Forms.Label()
        Me.lblFileName = New System.Windows.Forms.Label()
        Me.butExit = New Aqua.ThinButton()
        Me.Panel1 = New Aqua.Panel()
        Me.MediaItem1 = New Aqua.MediaItem()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblFileLength
        '
        Me.lblFileLength.Location = New System.Drawing.Point(124, 130)
        Me.lblFileLength.Size = New System.Drawing.Size(248, 19)
        Me.lblFileLength.Name = "lblFileLength"
        Me.lblFileLength.TabIndex = 2
        Me.lblFileLength.BackColor = System.Drawing.Color.Transparent
        Me.lblFileLength.Text = "資訊"
        Me.lblFileLength.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblFileDateTime
        '
        Me.lblFileDateTime.Location = New System.Drawing.Point(124, 99)
        Me.lblFileDateTime.Size = New System.Drawing.Size(248, 19)
        Me.lblFileDateTime.Name = "lblFileDateTime"
        Me.lblFileDateTime.TabIndex = 3
        Me.lblFileDateTime.BackColor = System.Drawing.Color.Transparent
        Me.lblFileDateTime.Text = "資訊"
        Me.lblFileDateTime.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblFilePath
        '
        Me.lblFilePath.Location = New System.Drawing.Point(124, 34)
        Me.lblFilePath.Size = New System.Drawing.Size(566, 21)
        Me.lblFilePath.Name = "lblFilePath"
        Me.lblFilePath.TabIndex = 4
        Me.lblFilePath.BackColor = System.Drawing.Color.Transparent
        Me.lblFilePath.Text = "資訊"
        Me.lblFilePath.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label10
        '
        Me.Label10.Location = New System.Drawing.Point(62, 223)
        Me.Label10.Size = New System.Drawing.Size(60, 19)
        Me.Label10.Name = "Label10"
        Me.Label10.TabIndex = 5
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.AutoSize = True
        Me.Label10.Text = "地點："
        Me.Label10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label8
        '
        Me.Label8.Location = New System.Drawing.Point(62, 161)
        Me.Label8.Size = New System.Drawing.Size(60, 19)
        Me.Label8.Name = "Label8"
        Me.Label8.TabIndex = 6
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.AutoSize = True
        Me.Label8.Text = "主題："
        Me.Label8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(62, 285)
        Me.Label4.Size = New System.Drawing.Size(60, 19)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 7
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "備註："
        Me.Label4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(62, 192)
        Me.Label6.Size = New System.Drawing.Size(60, 19)
        Me.Label6.Name = "Label6"
        Me.Label6.TabIndex = 8
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.AutoSize = True
        Me.Label6.Text = "人物："
        Me.Label6.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(62, 254)
        Me.Label7.Size = New System.Drawing.Size(60, 19)
        Me.Label7.Name = "Label7"
        Me.Label7.TabIndex = 9
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.AutoSize = True
        Me.Label7.Text = "時間："
        Me.Label7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(18, 90)
        Me.Line1.Size = New System.Drawing.Size(678, 1)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(62, 130)
        Me.Label3.Size = New System.Drawing.Size(60, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 10
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "大小："
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(22, 99)
        Me.Label2.Size = New System.Drawing.Size(100, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 11
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "修改日期："
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(62, 32)
        Me.Label1.Size = New System.Drawing.Size(60, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 12
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "檔案："
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblTitle
        '
        Me.lblTitle.Location = New System.Drawing.Point(124, 162)
        Me.lblTitle.Size = New System.Drawing.Size(248, 19)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.TabIndex = 14
        Me.lblTitle.BackColor = System.Drawing.Color.Transparent
        Me.lblTitle.Text = "資訊"
        Me.lblTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblCharacter
        '
        Me.lblCharacter.Location = New System.Drawing.Point(124, 193)
        Me.lblCharacter.Size = New System.Drawing.Size(248, 19)
        Me.lblCharacter.Name = "lblCharacter"
        Me.lblCharacter.TabIndex = 15
        Me.lblCharacter.BackColor = System.Drawing.Color.Transparent
        Me.lblCharacter.Text = "資訊"
        Me.lblCharacter.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblSopt
        '
        Me.lblSopt.Location = New System.Drawing.Point(124, 224)
        Me.lblSopt.Size = New System.Drawing.Size(248, 19)
        Me.lblSopt.Name = "lblSopt"
        Me.lblSopt.TabIndex = 16
        Me.lblSopt.BackColor = System.Drawing.Color.Transparent
        Me.lblSopt.Text = "資訊"
        Me.lblSopt.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblDateTime
        '
        Me.lblDateTime.Location = New System.Drawing.Point(124, 256)
        Me.lblDateTime.Size = New System.Drawing.Size(248, 19)
        Me.lblDateTime.Name = "lblDateTime"
        Me.lblDateTime.TabIndex = 17
        Me.lblDateTime.BackColor = System.Drawing.Color.Transparent
        Me.lblDateTime.Text = "資訊"
        Me.lblDateTime.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblRemark
        '
        Me.lblRemark.Location = New System.Drawing.Point(124, 287)
        Me.lblRemark.Size = New System.Drawing.Size(248, 19)
        Me.lblRemark.Name = "lblRemark"
        Me.lblRemark.TabIndex = 18
        Me.lblRemark.BackColor = System.Drawing.Color.Transparent
        Me.lblRemark.Text = "資訊"
        Me.lblRemark.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Label9
        '
        Me.Label9.Location = New System.Drawing.Point(42, 316)
        Me.Label9.Size = New System.Drawing.Size(80, 19)
        Me.Label9.Name = "Label9"
        Me.Label9.TabIndex = 19
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.AutoSize = True
        Me.Label9.Text = "關鍵字："
        Me.Label9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblKeyWord
        '
        Me.lblKeyWord.Location = New System.Drawing.Point(124, 318)
        Me.lblKeyWord.Size = New System.Drawing.Size(248, 19)
        Me.lblKeyWord.Name = "lblKeyWord"
        Me.lblKeyWord.TabIndex = 20
        Me.lblKeyWord.BackColor = System.Drawing.Color.Transparent
        Me.lblKeyWord.Text = "資訊"
        Me.lblKeyWord.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'lblFileName
        '
        Me.lblFileName.Location = New System.Drawing.Point(124, 60)
        Me.lblFileName.Size = New System.Drawing.Size(566, 21)
        Me.lblFileName.Name = "lblFileName"
        Me.lblFileName.TabIndex = 21
        Me.lblFileName.BackColor = System.Drawing.Color.Transparent
        Me.lblFileName.Text = "資訊"
        Me.lblFileName.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(302, 360)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 1
        Me.butExit.Text = "關閉"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butExit.ForeColor = System.Drawing.Color.Black
        '
        'MediaItem1
        '
        Me.MediaItem1.Location = New System.Drawing.Point(4, 4)
        Me.MediaItem1.Size = New System.Drawing.Size(305, 239)
        Me.MediaItem1.Name = "MediaItem1"
        Me.MediaItem1.TabIndex = 22
        Me.MediaItem1.ShowCheckBox = False
        Me.MediaItem1.DragItem = False
        Me.MediaItem1.DropItem = False
        Me.MediaItem1.BorderSize = 4
        '
        'Panel1
        '
        Me.Panel1.Location = New System.Drawing.Point(378, 98)
        Me.Panel1.Size = New System.Drawing.Size(313, 247)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.TabIndex = 13
        Me.Panel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Panel1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.Panel1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.Panel1.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Panel1.Controls.Add(Me.MediaItem1)
        '
        'frmShowPhoto
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(707, 401)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "相片詳細資料"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmShowPhoto"
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.lblFileLength)
        Me.Controls.Add(Me.lblFileDateTime)
        Me.Controls.Add(Me.lblFilePath)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Line1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.lblCharacter)
        Me.Controls.Add(Me.lblSopt)
        Me.Controls.Add(Me.lblDateTime)
        Me.Controls.Add(Me.lblRemark)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.lblKeyWord)
        Me.Controls.Add(Me.lblFileName)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblFileLength As System.Windows.Forms.Label
    Friend WithEvents lblFileDateTime As System.Windows.Forms.Label
    Friend WithEvents lblFilePath As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblCharacter As System.Windows.Forms.Label
    Friend WithEvents lblSopt As System.Windows.Forms.Label
    Friend WithEvents lblDateTime As System.Windows.Forms.Label
    Friend WithEvents lblRemark As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents lblKeyWord As System.Windows.Forms.Label
    Friend WithEvents lblFileName As System.Windows.Forms.Label
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents Panel1 As Aqua.Panel
    Friend WithEvents MediaItem1 As Aqua.MediaItem
End Class
