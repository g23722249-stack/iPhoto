<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSubject
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSubject))
        Me.Panel1 = New Aqua.Panel()
        Me.lvSubject = New System.Windows.Forms.ListView()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lvSubject
        '
        Me.lvSubject.Location = New System.Drawing.Point(2, 2)
        Me.lvSubject.Size = New System.Drawing.Size(433, 173)
        Me.lvSubject.Name = "lvSubject"
        Me.lvSubject.TabIndex = 0
        Me.lvSubject.LabelEdit = False
        Me.lvSubject.View = System.Windows.Forms.View.LargeIcon
        Me.lvSubject.LabelWrap = True
        Me.lvSubject.HideSelection = True
        Me.lvSubject.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lvSubject.BackColor = System.Drawing.SystemColors.Window
        Me.lvSubject.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Panel1
        '
        Me.Panel1.Location = New System.Drawing.Point(12, 34)
        Me.Panel1.Size = New System.Drawing.Size(437, 177)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.TabIndex = 2
        Me.Panel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Panel1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.Panel1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.Panel1.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Panel1.Controls.Add(Me.lvSubject)
        '
        'frmSubject
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(461, 223)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.Text = "主題"
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "主題"
        Me.Opacity = 230R
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmSubject"
        Me.Controls.Add(Me.Panel1)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel1 As Aqua.Panel
    Friend WithEvents lvSubject As System.Windows.Forms.ListView
End Class
