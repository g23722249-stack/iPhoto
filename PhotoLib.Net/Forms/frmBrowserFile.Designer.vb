<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBrowserFile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBrowserFile))
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtFileName = New Aqua.TextBox()
        Me.DeskTop1 = New Aqua.DirListBox()
        Me.FileListBox1 = New Aqua.FileListBox()
        Me.cboPattern = New Aqua.DropDownList()
        Me.butOk = New Aqua.ThinButton()
        Me.butExit = New Aqua.ThinButton()
        Me.SuspendLayout()
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(14, 394)
        Me.Label3.Size = New System.Drawing.Size(80, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 7
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "檔案類型"
        Me.Label3.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(14, 428)
        Me.Label2.Size = New System.Drawing.Size(80, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 8
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "檔案名稱"
        Me.Label2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtFileName
        '
        Me.txtFileName.Location = New System.Drawing.Point(98, 424)
        Me.txtFileName.Size = New System.Drawing.Size(519, 29)
        Me.txtFileName.Name = "txtFileName"
        Me.txtFileName.TabIndex = 3
        Me.txtFileName.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtFileName.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtFileName.MaxLength = 0
        Me.txtFileName.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtFileName.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtFileName.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'DeskTop1
        '
        Me.DeskTop1.Location = New System.Drawing.Point(13, 36)
        Me.DeskTop1.Size = New System.Drawing.Size(421, 349)
        Me.DeskTop1.Name = "DeskTop1"
        Me.DeskTop1.TabIndex = 0
        Me.DeskTop1.ShowSpecialFolders = True
        Me.DeskTop1.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'FileListBox1
        '
        Me.FileListBox1.Location = New System.Drawing.Point(438, 36)
        Me.FileListBox1.Size = New System.Drawing.Size(179, 349)
        Me.FileListBox1.Name = "FileListBox1"
        Me.FileListBox1.TabIndex = 1
        Me.FileListBox1.Font = New System.Drawing.Font("Arial", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'cboPattern
        '
        Me.cboPattern.Location = New System.Drawing.Point(98, 392)
        Me.cboPattern.Size = New System.Drawing.Size(519, 25)
        Me.cboPattern.Name = "cboPattern"
        Me.cboPattern.TabIndex = 2
        Me.cboPattern.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cboPattern.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(323, 464)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 5
        Me.butOk.ForeColor = System.Drawing.Color.Black
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(203, 464)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 4
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmBrowserFile
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(629, 505)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "Form1"
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Opacity = 240R
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmBrowserFile"
        Me.Controls.Add(Me.txtFileName)
        Me.Controls.Add(Me.DeskTop1)
        Me.Controls.Add(Me.FileListBox1)
        Me.Controls.Add(Me.cboPattern)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtFileName As Aqua.TextBox
    Friend WithEvents DeskTop1 As Aqua.DirListBox
    Friend WithEvents FileListBox1 As Aqua.FileListBox
    Friend WithEvents cboPattern As Aqua.DropDownList
    Friend WithEvents butOk As Aqua.ThinButton
    Friend WithEvents butExit As Aqua.ThinButton
End Class
