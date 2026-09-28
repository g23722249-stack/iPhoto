<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmToolTipText
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
        Me.Shape1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Shape1
        '
        Me.Shape1.Location = New System.Drawing.Point(0, 0)
        Me.Shape1.Size = New System.Drawing.Size(187, 61)
        Me.Shape1.Name = "Shape1"
        Me.Shape1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Shape1.BackColor = System.Drawing.Color.Transparent
        '
        'frmToolTipText
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(188, 62)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Text = "Form1"
        Me.ShowInTaskbar = False
        Me.Name = "frmToolTipText"
        Me.Controls.Add(Me.Shape1)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Shape1 As System.Windows.Forms.Label
End Class
