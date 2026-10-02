<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmQueryMsgBox
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmQueryMsgBox))
        Me.lblInterval = New System.Windows.Forms.Label()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.imgIcon = New System.Windows.Forms.PictureBox()
        Me.butOk = New Aqua.ThinButton()
        Me.butExit = New Aqua.ThinButton()
        Me.SuspendLayout()
        '
        'lblInterval
        '
        Me.lblInterval.Location = New System.Drawing.Point(38, 146)
        Me.lblInterval.Size = New System.Drawing.Size(21, 12)
        Me.lblInterval.Name = "lblInterval"
        Me.lblInterval.TabIndex = 4
        Me.lblInterval.Visible = False
        '
        'lblMessage
        '
        Me.lblMessage.Location = New System.Drawing.Point(86, 16)
        Me.lblMessage.Size = New System.Drawing.Size(120, 19)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.TabIndex = 3
        Me.lblMessage.BackColor = System.Drawing.Color.Transparent
        Me.lblMessage.AutoSize = True
        Me.lblMessage.Text = "資料夾不存在"
        Me.lblMessage.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblMessage.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgIcon
        '
        Me.imgIcon.Location = New System.Drawing.Point(24, 20)
        Me.imgIcon.Size = New System.Drawing.Size(32, 32)
        Me.imgIcon.Name = "imgIcon"
        Me.imgIcon.BackColor = System.Drawing.Color.Transparent
        Me.imgIcon.Image = CType(resources.GetObject("imgIcon.Image"), System.Drawing.Image)
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(186, 78)
        Me.butOk.Size = New System.Drawing.Size(91, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 0
        Me.butOk.ForeColor = System.Drawing.Color.Black
        Me.butOk.Text = "是"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(74, 78)
        Me.butExit.Size = New System.Drawing.Size(91, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 1
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Text = "否"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmQueryMsgBox
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(351, 123)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.SizeMode = Aqua.ImageSizeMode.StretchImage
        Me.CloseButton = False
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = ""
        Me.Opacity = 240R
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmQueryMsgBox"
        Me.Controls.Add(Me.imgIcon)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.lblInterval)
        Me.Controls.Add(Me.lblMessage)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblInterval As System.Windows.Forms.Label
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents imgIcon As System.Windows.Forms.PictureBox
    Friend WithEvents butOk As Aqua.ThinButton
    Friend WithEvents butExit As Aqua.ThinButton
End Class
