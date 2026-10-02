<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBuildBook
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBuildBook))
        Me.Label5 = New System.Windows.Forms.Label()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.frmClass = New Aqua.Panel()
        Me.tvList = New System.Windows.Forms.TreeView()
        Me.txtFolder = New Aqua.TextBox()
        Me.butOk = New Aqua.ThinButton()
        Me.butExit = New Aqua.ThinButton()
        Me.frmClass.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(388, 616)
        Me.Label5.Size = New System.Drawing.Size(40, 19)
        Me.Label5.Name = "Label5"
        Me.Label5.TabIndex = 1
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.AutoSize = True
        Me.Label5.Text = "名稱"
        Me.Label5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'lblInfo
        '
        Me.lblInfo.Location = New System.Drawing.Point(14, 616)
        Me.lblInfo.Size = New System.Drawing.Size(40, 19)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.TabIndex = 7
        Me.lblInfo.BackColor = System.Drawing.Color.Transparent
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Text = "相簿"
        Me.lblInfo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'tvList
        '
        Me.tvList.Location = New System.Drawing.Point(4, 4)
        Me.tvList.Size = New System.Drawing.Size(769, 564)
        Me.tvList.Name = "tvList"
        Me.tvList.TabIndex = 0
        Me.tvList.LabelEdit = False
        Me.tvList.HideSelection = False
        Me.tvList.FullRowSelect = True
        Me.tvList.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmClass
        '
        Me.frmClass.Location = New System.Drawing.Point(12, 34)
        Me.frmClass.Size = New System.Drawing.Size(775, 571)
        Me.frmClass.Name = "frmClass"
        Me.frmClass.TabIndex = 6
        Me.frmClass.ForeColor = System.Drawing.SystemColors.ControlText
        Me.frmClass.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.frmClass.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.frmClass.PanelStyle = Aqua.PanelStyleMode.Container
        Me.frmClass.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.frmClass.Controls.Add(Me.tvList)
        '
        'txtFolder
        '
        Me.txtFolder.Location = New System.Drawing.Point(436, 612)
        Me.txtFolder.Size = New System.Drawing.Size(351, 25)
        Me.txtFolder.Name = "txtFolder"
        Me.txtFolder.TabIndex = 2
        Me.txtFolder.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtFolder.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtFolder.MaxLength = 0
        Me.txtFolder.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtFolder.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtFolder.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(407, 652)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 4
        Me.butOk.ForeColor = System.Drawing.Color.Black
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(289, 652)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 3
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmBuildBook
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(799, 693)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "攝影集製作"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmBuildBook"
        Me.Controls.Add(Me.frmClass)
        Me.Controls.Add(Me.txtFolder)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.lblInfo)
        Me.frmClass.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents frmClass As Aqua.Panel
    Friend WithEvents tvList As System.Windows.Forms.TreeView
    Friend WithEvents txtFolder As Aqua.TextBox
    Friend WithEvents butOk As Aqua.ThinButton
    Friend WithEvents butExit As Aqua.ThinButton
End Class
