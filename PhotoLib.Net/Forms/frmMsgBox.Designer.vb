<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMsgBox
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMsgBox))
        Me.lblInterval = New System.Windows.Forms.Label()
        Me.imgCritical = New System.Windows.Forms.PictureBox()
        Me.imgQuestion = New System.Windows.Forms.PictureBox()
        Me.imgExclamation = New System.Windows.Forms.PictureBox()
        Me.imgSuccess = New System.Windows.Forms.PictureBox()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.imgIcon = New System.Windows.Forms.PictureBox()
        Me.butOk = New Aqua.FlashButton()
        Me.SuspendLayout()
        '
        'lblInterval
        '
        Me.lblInterval.Location = New System.Drawing.Point(18, 134)
        Me.lblInterval.Size = New System.Drawing.Size(21, 12)
        Me.lblInterval.Name = "lblInterval"
        Me.lblInterval.TabIndex = 3
        Me.lblInterval.Visible = False
        '
        'imgCritical
        '
        Me.imgCritical.Location = New System.Drawing.Point(28, 166)
        Me.imgCritical.Size = New System.Drawing.Size(32, 32)
        Me.imgCritical.Name = "imgCritical"
        Me.imgCritical.BackColor = System.Drawing.Color.Transparent
        Me.imgCritical.Image = CType(resources.GetObject("imgCritical.Image"), System.Drawing.Image)
        '
        'imgQuestion
        '
        Me.imgQuestion.Location = New System.Drawing.Point(74, 168)
        Me.imgQuestion.Size = New System.Drawing.Size(32, 32)
        Me.imgQuestion.Name = "imgQuestion"
        Me.imgQuestion.BackColor = System.Drawing.Color.Transparent
        Me.imgQuestion.Image = CType(resources.GetObject("imgQuestion.Image"), System.Drawing.Image)
        '
        'imgExclamation
        '
        Me.imgExclamation.Location = New System.Drawing.Point(128, 166)
        Me.imgExclamation.Size = New System.Drawing.Size(32, 32)
        Me.imgExclamation.Name = "imgExclamation"
        Me.imgExclamation.BackColor = System.Drawing.Color.Transparent
        Me.imgExclamation.Image = CType(resources.GetObject("imgExclamation.Image"), System.Drawing.Image)
        '
        'imgSuccess
        '
        Me.imgSuccess.Location = New System.Drawing.Point(172, 238)
        Me.imgSuccess.Size = New System.Drawing.Size(32, 32)
        Me.imgSuccess.Name = "imgSuccess"
        Me.imgSuccess.BackColor = System.Drawing.Color.Transparent
        Me.imgSuccess.Image = CType(resources.GetObject("imgSuccess.Image"), System.Drawing.Image)
        '
        'lblMessage
        '
        Me.lblMessage.Location = New System.Drawing.Point(82, 16)
        Me.lblMessage.Size = New System.Drawing.Size(120, 19)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.TabIndex = 2
        Me.lblMessage.BackColor = System.Drawing.Color.Transparent
        Me.lblMessage.AutoSize = True
        Me.lblMessage.Text = "資料夾不存在"
        Me.lblMessage.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblMessage.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgIcon
        '
        Me.imgIcon.Location = New System.Drawing.Point(26, 38)
        Me.imgIcon.Size = New System.Drawing.Size(32, 32)
        Me.imgIcon.Name = "imgIcon"
        Me.imgIcon.BackColor = System.Drawing.Color.Transparent
        Me.imgIcon.Image = CType(resources.GetObject("imgIcon.Image"), System.Drawing.Image)
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(124, 82)
        Me.butOk.Size = New System.Drawing.Size(95, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 0
        Me.butOk.Text = "確定"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmMsgBox
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(351, 123)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
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
        Me.Name = "frmMsgBox"
        Me.Controls.Add(Me.imgCritical)
        Me.Controls.Add(Me.imgQuestion)
        Me.Controls.Add(Me.imgExclamation)
        Me.Controls.Add(Me.imgSuccess)
        Me.Controls.Add(Me.imgIcon)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.lblInterval)
        Me.Controls.Add(Me.lblMessage)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblInterval As System.Windows.Forms.Label
    Friend WithEvents imgCritical As System.Windows.Forms.PictureBox
    Friend WithEvents imgQuestion As System.Windows.Forms.PictureBox
    Friend WithEvents imgExclamation As System.Windows.Forms.PictureBox
    Friend WithEvents imgSuccess As System.Windows.Forms.PictureBox
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents imgIcon As System.Windows.Forms.PictureBox
    Friend WithEvents butOk As Aqua.FlashButton
End Class
