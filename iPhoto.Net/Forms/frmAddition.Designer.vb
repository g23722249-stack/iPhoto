<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmAddition
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddition))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.frmClass = New Aqua.Panel()
        Me.tvList = New System.Windows.Forms.TreeView()
        Me.txtPath = New Aqua.TextBox()
        Me.butExit = New Aqua.FlashButton()
        Me.butOk = New Aqua.FlashButton()
        Me.rbMode_0 = New Aqua.RadioButton()
        Me.rbMode_1 = New Aqua.RadioButton()
        Me.frmClass.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(14, 402)
        Me.Label1.Size = New System.Drawing.Size(160, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 6
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "請輸入新相簿名稱"
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'tvList
        '
        Me.tvList.Location = New System.Drawing.Point(4, 4)
        Me.tvList.Size = New System.Drawing.Size(589, 314)
        Me.tvList.Name = "tvList"
        Me.tvList.TabIndex = 0
        Me.tvList.LabelEdit = False
        Me.tvList.HideSelection = False
        Me.tvList.FullRowSelect = True
        Me.tvList.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmClass
        '
        Me.frmClass.Location = New System.Drawing.Point(12, 74)
        Me.frmClass.Size = New System.Drawing.Size(595, 321)
        Me.frmClass.Name = "frmClass"
        Me.frmClass.TabIndex = 5
        Me.frmClass.ForeColor = System.Drawing.SystemColors.ControlText
        Me.frmClass.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.frmClass.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.frmClass.PanelStyle = Aqua.PanelStyleMode.Container
        Me.frmClass.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.frmClass.Controls.Add(Me.tvList)
        '
        'txtPath
        '
        Me.txtPath.Location = New System.Drawing.Point(180, 398)
        Me.txtPath.Size = New System.Drawing.Size(427, 25)
        Me.txtPath.Name = "txtPath"
        Me.txtPath.TabIndex = 1
        Me.txtPath.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtPath.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtPath.MaxLength = 0
        Me.txtPath.Text = "新資料夾"
        Me.txtPath.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtPath.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtPath.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(198, 434)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 2
        Me.butExit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(318, 434)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 3
        Me.butOk.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'rbMode_0
        '
        Me.rbMode_0.Location = New System.Drawing.Point(163, 42)
        Me.rbMode_0.Name = "rbMode_0"
        Me.rbMode_0.TabIndex = 7
        Me.rbMode_0.TabStop = False
        Me.rbMode_0.Checked = True
        Me.rbMode_0.TextValue = "新增相片庫"
        Me.rbMode_0.TextGap = 8
        Me.rbMode_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'rbMode_1
        '
        Me.rbMode_1.Location = New System.Drawing.Point(331, 42)
        Me.rbMode_1.Name = "rbMode_1"
        Me.rbMode_1.TabIndex = 9
        Me.rbMode_1.TabStop = False
        Me.rbMode_1.TextValue = "新增攝影集"
        Me.rbMode_1.TextGap = 8
        Me.rbMode_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'frmAddition
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(619, 481)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "新增相片庫"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmAddition"
        Me.Controls.Add(Me.frmClass)
        Me.Controls.Add(Me.txtPath)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.rbMode_0)
        Me.Controls.Add(Me.rbMode_1)
        Me.Controls.Add(Me.Label1)
        Me.frmClass.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents frmClass As Aqua.Panel
    Friend WithEvents tvList As System.Windows.Forms.TreeView
    Friend WithEvents txtPath As Aqua.TextBox
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butOk As Aqua.FlashButton
    Friend WithEvents rbMode_0 As Aqua.RadioButton
    Friend WithEvents rbMode_1 As Aqua.RadioButton
End Class
