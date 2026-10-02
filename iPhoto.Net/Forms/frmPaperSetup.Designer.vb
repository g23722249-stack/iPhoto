<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPaperSetup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPaperSetup))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Line2 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txtEdge_0 = New Aqua.TextBox()
        Me.txtEdge_1 = New Aqua.TextBox()
        Me.txtEdge_2 = New Aqua.TextBox()
        Me.txtEdge_3 = New Aqua.TextBox()
        Me.txtKeep_0 = New Aqua.TextBox()
        Me.txtKeep_1 = New Aqua.TextBox()
        Me.butExit = New Aqua.ThinButton()
        Me.butOk = New Aqua.ThinButton()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(19, 38)
        Me.Label1.Size = New System.Drawing.Size(100, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 1
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "邊界（mm）"
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Label1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(40, 72)
        Me.Label2.Size = New System.Drawing.Size(20, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 2
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "上"
        Me.Label2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(40, 104)
        Me.Label3.Size = New System.Drawing.Size(20, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 3
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "左"
        Me.Label3.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label3.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(178, 72)
        Me.Label4.Size = New System.Drawing.Size(20, 19)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 4
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "下"
        Me.Label4.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label4.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(178, 104)
        Me.Label5.Size = New System.Drawing.Size(20, 19)
        Me.Label5.Name = "Label5"
        Me.Label5.TabIndex = 5
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.AutoSize = True
        Me.Label5.Text = "右"
        Me.Label5.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label5.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(115, 46)
        Me.Line1.Size = New System.Drawing.Size(169, 1)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(19, 152)
        Me.Label6.Size = New System.Drawing.Size(140, 19)
        Me.Label6.Name = "Label6"
        Me.Label6.TabIndex = 10
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.AutoSize = True
        Me.Label6.Text = "滿版設定（mm）"
        Me.Label6.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Label6.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Line2
        '
        Me.Line2.AutoSize = False
        Me.Line2.Location = New System.Drawing.Point(158, 160)
        Me.Line2.Size = New System.Drawing.Size(126, 1)
        Me.Line2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line2.Name = "Line2"
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(30, 188)
        Me.Label7.Size = New System.Drawing.Size(30, 19)
        Me.Label7.Name = "Label7"
        Me.Label7.TabIndex = 11
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.AutoSize = True
        Me.Label7.Text = "X軸"
        Me.Label7.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label7.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label8
        '
        Me.Label8.Location = New System.Drawing.Point(168, 188)
        Me.Label8.Size = New System.Drawing.Size(30, 19)
        Me.Label8.Name = "Label8"
        Me.Label8.TabIndex = 12
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.AutoSize = True
        Me.Label8.Text = "Y軸"
        Me.Label8.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label8.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtEdge_0
        '
        Me.txtEdge_0.Location = New System.Drawing.Point(70, 70)
        Me.txtEdge_0.Size = New System.Drawing.Size(55, 26)
        Me.txtEdge_0.Name = "txtEdge_0"
        Me.txtEdge_0.TabIndex = 6
        Me.txtEdge_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtEdge_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtEdge_0.MaxLength = 0
        Me.txtEdge_0.Text = "0"
        Me.txtEdge_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtEdge_0.AutoSelect = True
        Me.txtEdge_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtEdge_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtEdge_0.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'txtEdge_1
        '
        Me.txtEdge_1.Location = New System.Drawing.Point(208, 70)
        Me.txtEdge_1.Size = New System.Drawing.Size(55, 26)
        Me.txtEdge_1.Name = "txtEdge_1"
        Me.txtEdge_1.TabIndex = 7
        Me.txtEdge_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtEdge_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtEdge_1.MaxLength = 0
        Me.txtEdge_1.Text = "0"
        Me.txtEdge_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtEdge_1.AutoSelect = True
        Me.txtEdge_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtEdge_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtEdge_1.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'txtEdge_2
        '
        Me.txtEdge_2.Location = New System.Drawing.Point(70, 100)
        Me.txtEdge_2.Size = New System.Drawing.Size(55, 26)
        Me.txtEdge_2.Name = "txtEdge_2"
        Me.txtEdge_2.TabIndex = 8
        Me.txtEdge_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtEdge_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtEdge_2.MaxLength = 0
        Me.txtEdge_2.Text = "0"
        Me.txtEdge_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtEdge_2.AutoSelect = True
        Me.txtEdge_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtEdge_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtEdge_2.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'txtEdge_3
        '
        Me.txtEdge_3.Location = New System.Drawing.Point(208, 100)
        Me.txtEdge_3.Size = New System.Drawing.Size(55, 26)
        Me.txtEdge_3.Name = "txtEdge_3"
        Me.txtEdge_3.TabIndex = 9
        Me.txtEdge_3.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtEdge_3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtEdge_3.MaxLength = 0
        Me.txtEdge_3.Text = "0"
        Me.txtEdge_3.Alignment = Aqua.AlignmentConstants.Center
        Me.txtEdge_3.AutoSelect = True
        Me.txtEdge_3.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtEdge_3.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtEdge_3.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'txtKeep_0
        '
        Me.txtKeep_0.Location = New System.Drawing.Point(70, 184)
        Me.txtKeep_0.Size = New System.Drawing.Size(55, 26)
        Me.txtKeep_0.Name = "txtKeep_0"
        Me.txtKeep_0.TabIndex = 13
        Me.txtKeep_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtKeep_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtKeep_0.MaxLength = 0
        Me.txtKeep_0.Text = "0"
        Me.txtKeep_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtKeep_0.AutoSelect = True
        Me.txtKeep_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtKeep_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtKeep_0.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'txtKeep_1
        '
        Me.txtKeep_1.Location = New System.Drawing.Point(208, 184)
        Me.txtKeep_1.Size = New System.Drawing.Size(55, 26)
        Me.txtKeep_1.Name = "txtKeep_1"
        Me.txtKeep_1.TabIndex = 14
        Me.txtKeep_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtKeep_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtKeep_1.MaxLength = 0
        Me.txtKeep_1.Text = "0"
        Me.txtKeep_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtKeep_1.AutoSelect = True
        Me.txtKeep_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtKeep_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtKeep_1.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(41, 246)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 15
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(158, 246)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 16
        Me.butOk.ForeColor = System.Drawing.Color.Black
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmPaperSetup
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(303, 291)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "版面設定"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmPaperSetup"
        Me.Controls.Add(Me.txtEdge_0)
        Me.Controls.Add(Me.txtEdge_1)
        Me.Controls.Add(Me.txtEdge_2)
        Me.Controls.Add(Me.txtEdge_3)
        Me.Controls.Add(Me.txtKeep_0)
        Me.Controls.Add(Me.txtKeep_1)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Line1)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Line2)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label8)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Line2 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtEdge_0 As Aqua.TextBox
    Friend WithEvents txtEdge_1 As Aqua.TextBox
    Friend WithEvents txtEdge_2 As Aqua.TextBox
    Friend WithEvents txtEdge_3 As Aqua.TextBox
    Friend WithEvents txtKeep_0 As Aqua.TextBox
    Friend WithEvents txtKeep_1 As Aqua.TextBox
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents butOk As Aqua.ThinButton
End Class
