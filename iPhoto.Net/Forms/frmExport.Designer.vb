<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmExport
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmExport))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.DeskTop1 = New Aqua.DirListBox()
        Me.txtPath = New Aqua.TextBox()
        Me.ddSize = New Aqua.DropDownList()
        Me.txtReName = New Aqua.TextBox()
        Me.butOk = New Aqua.ThinButton()
        Me.butExit = New Aqua.ThinButton()
        Me.chkSort = New Aqua.CheckBox()
        Me.ProgressBar1 = New Aqua.ProgressBar()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(14, 424)
        Me.Label1.Size = New System.Drawing.Size(40, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 8
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "位置"
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(229, 468)
        Me.Label2.Size = New System.Drawing.Size(40, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 9
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "尺寸"
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.Location = New System.Drawing.Point(11, 456)
        Me.Line1.Size = New System.Drawing.Size(742, 1)
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Name = "Line1"
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(229, 504)
        Me.Label3.Size = New System.Drawing.Size(40, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 11
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "名稱"
        '
        'DeskTop1
        '
        Me.DeskTop1.Location = New System.Drawing.Point(12, 36)
        Me.DeskTop1.Size = New System.Drawing.Size(741, 379)
        Me.DeskTop1.Name = "DeskTop1"
        Me.DeskTop1.TabIndex = 0
        Me.DeskTop1.ShowSpecialFolders = True
        Me.DeskTop1.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtPath
        '
        Me.txtPath.Location = New System.Drawing.Point(60, 422)
        Me.txtPath.Size = New System.Drawing.Size(561, 25)
        Me.txtPath.Name = "txtPath"
        Me.txtPath.TabIndex = 1
        Me.txtPath.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtPath.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtPath.MaxLength = 0
        Me.txtPath.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtPath.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtPath.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'ddSize
        '
        Me.ddSize.Location = New System.Drawing.Point(277, 466)
        Me.ddSize.Size = New System.Drawing.Size(259, 25)
        Me.ddSize.Name = "ddSize"
        Me.ddSize.TabIndex = 3
        Me.ddSize.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ddSize.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.ddSize.Items.Add(New Aqua.MenuItem("", "保留原始尺寸", 1, Nothing, Nothing))
        Me.ddSize.Items.Add(New Aqua.MenuItem("1280", "大型  (1280 x 1024)", 1, Nothing, Nothing))
        Me.ddSize.Items.Add(New Aqua.MenuItem("1024", "中型  (1024 x 768)", 1, Nothing, Nothing))
        Me.ddSize.Items.Add(New Aqua.MenuItem("640", "小型  (640 x 480)", 1, Nothing, Nothing))
        Me.ddSize.Items.Add(New Aqua.MenuItem("320", "縮圖  (320 x 240)", 1, Nothing, Nothing))
        Me.ddSize.SelectedIndex = 0
        '
        'txtReName
        '
        Me.txtReName.Location = New System.Drawing.Point(277, 502)
        Me.txtReName.Size = New System.Drawing.Size(257, 25)
        Me.txtReName.Name = "txtReName"
        Me.txtReName.TabIndex = 4
        Me.txtReName.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtReName.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtReName.MaxLength = 0
        Me.txtReName.Text = "Image*"
        Me.txtReName.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtReName.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtReName.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(390, 540)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 6
        Me.butOk.ForeColor = System.Drawing.Color.Black
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(272, 540)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 5
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'chkSort
        '
        Me.chkSort.Location = New System.Drawing.Point(632, 426)
        Me.chkSort.Name = "chkSort"
        Me.chkSort.TabIndex = 2
        Me.chkSort.Checked = True
        Me.chkSort.TextValue = "依時間排序"
        Me.chkSort.TextGap = 6
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(2, 582)
        Me.ProgressBar1.Size = New System.Drawing.Size(761, 21)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.TabIndex = 12
        '
        'frmExport
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(765, 605)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "匯出"
        Me.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmExport"
        Me.Controls.Add(Me.DeskTop1)
        Me.Controls.Add(Me.txtPath)
        Me.Controls.Add(Me.ddSize)
        Me.Controls.Add(Me.txtReName)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.chkSort)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Line1)
        Me.Controls.Add(Me.Label3)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents DeskTop1 As Aqua.DirListBox
    Friend WithEvents txtPath As Aqua.TextBox
    Friend WithEvents ddSize As Aqua.DropDownList
    Friend WithEvents txtReName As Aqua.TextBox
    Friend WithEvents butOk As Aqua.ThinButton
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents chkSort As Aqua.CheckBox
    Friend WithEvents ProgressBar1 As Aqua.ProgressBar
End Class
