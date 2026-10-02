<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmInputString
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmInputString))
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.butOk = New Aqua.ThinButton()
        Me.butCancel = New Aqua.ThinButton()
        Me.TextBox1 = New Aqua.TextBox()
        Me.SuspendLayout()
        '
        'lblMessage
        '
        Me.lblMessage.Location = New System.Drawing.Point(16, 32)
        Me.lblMessage.Size = New System.Drawing.Size(100, 19)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.TabIndex = 2
        Me.lblMessage.BackColor = System.Drawing.Color.Transparent
        Me.lblMessage.AutoSize = True
        Me.lblMessage.Text = "請輸入名稱"
        Me.lblMessage.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblMessage.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(70, 100)
        Me.butOk.Size = New System.Drawing.Size(95, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 1
        Me.butOk.Text = "確定"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butOk.ForeColor = System.Drawing.Color.Black
        '
        'butCancel
        '
        Me.butCancel.Location = New System.Drawing.Point(186, 100)
        Me.butCancel.Size = New System.Drawing.Size(95, 27)
        Me.butCancel.Name = "butCancel"
        Me.butCancel.TabIndex = 4
        Me.butCancel.Text = "放棄"
        Me.butCancel.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butCancel.ForeColor = System.Drawing.Color.Black
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(16, 58)
        Me.TextBox1.Size = New System.Drawing.Size(313, 29)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.TabIndex = 3
        Me.TextBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.TextBox1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.TextBox1.MaxLength = 0
        Me.TextBox1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.TextBox1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.TextBox1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmInputString
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(351, 141)
        Me.TopMost = True
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.CloseButton = False
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = ""
        Me.Opacity = 240R
        Me.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmInputString"
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.butCancel)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.lblMessage)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents butOk As Aqua.ThinButton
    Friend WithEvents butCancel As Aqua.ThinButton
    Friend WithEvents TextBox1 As Aqua.TextBox
End Class
