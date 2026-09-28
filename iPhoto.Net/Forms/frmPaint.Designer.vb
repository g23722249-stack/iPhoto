<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPaint
    Inherits System.Windows.Forms.Form

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
        Me.picTemp = New System.Windows.Forms.PictureBox()
        Me.picEffect = New System.Windows.Forms.PictureBox()
        Me.SuspendLayout()
        '
        'picTemp
        '
        Me.picTemp.Location = New System.Drawing.Point(32, 24)
        Me.picTemp.Size = New System.Drawing.Size(635, 431)
        Me.picTemp.Name = "picTemp"
        Me.picTemp.TabIndex = 0
        Me.picTemp.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picTemp.BackColor = System.Drawing.SystemColors.Window
        Me.picTemp.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'picEffect
        '
        Me.picEffect.Location = New System.Drawing.Point(188, 286)
        Me.picEffect.Size = New System.Drawing.Size(635, 431)
        Me.picEffect.Name = "picEffect"
        Me.picEffect.TabIndex = 1
        Me.picEffect.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picEffect.BackColor = System.Drawing.SystemColors.Window
        Me.picEffect.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'frmPaint
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(989, 787)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation
        Me.Text = "Form1"
        Me.Name = "frmPaint"
        Me.Controls.Add(Me.picTemp)
        Me.Controls.Add(Me.picEffect)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents picTemp As System.Windows.Forms.PictureBox
    Friend WithEvents picEffect As System.Windows.Forms.PictureBox
End Class
