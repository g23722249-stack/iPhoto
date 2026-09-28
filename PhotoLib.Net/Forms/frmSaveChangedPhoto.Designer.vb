<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSaveChangedPhoto
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSaveChangedPhoto))
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.rbExecute_0 = New Aqua.RadioButton()
        Me.rbExecute_1 = New Aqua.RadioButton()
        Me.rbExecute_2 = New Aqua.RadioButton()
        Me.rbExecute_3 = New Aqua.RadioButton()
        Me.butOk = New Aqua.FlashButton()
        Me.butExit = New Aqua.FlashButton()
        Me.SuspendLayout()
        '
        'lblTitle
        '
        Me.lblTitle.Location = New System.Drawing.Point(32, 46)
        Me.lblTitle.Size = New System.Drawing.Size(374, 21)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.TabIndex = 1
        Me.lblTitle.BackColor = System.Drawing.Color.Transparent
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Text = "相片的內容已改變了，要如何處理呢？"
        Me.lblTitle.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'rbExecute_0
        '
        Me.rbExecute_0.Location = New System.Drawing.Point(74, 88)
        Me.rbExecute_0.Name = "rbExecute_0"
        Me.rbExecute_0.TabIndex = 2
        Me.rbExecute_0.TextValue = "放棄改變"
        Me.rbExecute_0.TextGap = 10
        '
        'rbExecute_1
        '
        Me.rbExecute_1.Location = New System.Drawing.Point(74, 119)
        Me.rbExecute_1.Name = "rbExecute_1"
        Me.rbExecute_1.TabIndex = 4
        Me.rbExecute_1.TextValue = "儲存"
        Me.rbExecute_1.TextGap = 10
        Me.rbExecute_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'rbExecute_2
        '
        Me.rbExecute_2.Location = New System.Drawing.Point(74, 151)
        Me.rbExecute_2.Name = "rbExecute_2"
        Me.rbExecute_2.TabIndex = 6
        Me.rbExecute_2.TextValue = "複製到剪貼簿"
        Me.rbExecute_2.TextGap = 10
        '
        'rbExecute_3
        '
        Me.rbExecute_3.Location = New System.Drawing.Point(74, 182)
        Me.rbExecute_3.Name = "rbExecute_3"
        Me.rbExecute_3.TabIndex = 8
        Me.rbExecute_3.TextValue = "另存新檔案"
        Me.rbExecute_3.TextGap = 10
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(225, 224)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 10
        Me.butOk.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(108, 224)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 11
        Me.butExit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmSaveChangedPhoto
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(435, 275)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.SizeMode = Aqua.ImageSizeMode.StretchImage
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = ""
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("Times New Roman", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmSaveChangedPhoto"
        Me.Controls.Add(Me.rbExecute_0)
        Me.Controls.Add(Me.rbExecute_1)
        Me.Controls.Add(Me.rbExecute_2)
        Me.Controls.Add(Me.rbExecute_3)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.lblTitle)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents rbExecute_0 As Aqua.RadioButton
    Friend WithEvents rbExecute_1 As Aqua.RadioButton
    Friend WithEvents rbExecute_2 As Aqua.RadioButton
    Friend WithEvents rbExecute_3 As Aqua.RadioButton
    Friend WithEvents butOk As Aqua.FlashButton
    Friend WithEvents butExit As Aqua.FlashButton
End Class
