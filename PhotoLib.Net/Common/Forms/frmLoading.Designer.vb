<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmLoading
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLoading))
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.Loading1 = New Aqua.Loading()
        Me.SuspendLayout()
        '
        'lblMessage
        '
        Me.lblMessage.Location = New System.Drawing.Point(0, 48)
        Me.lblMessage.Size = New System.Drawing.Size(575, 23)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.TabIndex = 2
        Me.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.lblMessage.BackColor = System.Drawing.Color.Transparent
        Me.lblMessage.Text = "Label1"
        Me.lblMessage.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblMessage.Font = New System.Drawing.Font("Tahoma", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'Loading1
        '
        Me.Loading1.Location = New System.Drawing.Point(16, 20)
        Me.Loading1.Size = New System.Drawing.Size(541, 16)
        Me.Loading1.Name = "Loading1"
        Me.Loading1.TabIndex = 1
        '
        'frmLoading
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(577, 89)
        Me.TopMost = True
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
        Me.Opacity = 235R
        Me.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("Tahoma", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmLoading"
        Me.Controls.Add(Me.Loading1)
        Me.Controls.Add(Me.lblMessage)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents Loading1 As Aqua.Loading
End Class
