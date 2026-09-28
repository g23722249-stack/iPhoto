<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDock
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDock))
        Me.imgClear = New System.Windows.Forms.PictureBox()
        Me.Image1 = New System.Windows.Forms.PictureBox()
        Me.Image2 = New System.Windows.Forms.PictureBox()
        Me.mlDock = New Aqua.MediaList()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.vb6ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout()
        '
        'imgClear
        '
        Me.imgClear.Location = New System.Drawing.Point(2, 179)
        Me.imgClear.Size = New System.Drawing.Size(24, 24)
        Me.imgClear.Name = "imgClear"
        Me.imgClear.BackColor = System.Drawing.Color.Transparent
        Me.imgClear.Image = CType(resources.GetObject("imgClear.Image"), System.Drawing.Image)
        '
        'Image1
        '
        Me.Image1.Location = New System.Drawing.Point(2, 98)
        Me.Image1.Size = New System.Drawing.Size(24, 24)
        Me.Image1.Name = "Image1"
        Me.Image1.BackColor = System.Drawing.Color.Transparent
        Me.Image1.Image = CType(resources.GetObject("Image1.Image"), System.Drawing.Image)
        '
        'Image2
        '
        Me.Image2.Location = New System.Drawing.Point(2, 138)
        Me.Image2.Size = New System.Drawing.Size(24, 24)
        Me.Image2.Name = "Image2"
        Me.Image2.BackColor = System.Drawing.Color.Transparent
        Me.Image2.Image = CType(resources.GetObject("Image2.Image"), System.Drawing.Image)
        '
        'mlDock
        '
        Me.mlDock.Location = New System.Drawing.Point(28, 32)
        Me.mlDock.Size = New System.Drawing.Size(589, 171)
        Me.mlDock.Limit = 1
        Me.mlDock.BorderSize = 4
        Me.mlDock.Name = "mlDock"
        Me.mlDock.TabIndex = 0
        Me.mlDock.ShowCheckBox = False
        Me.mlDock.DropItem = False
        Me.mlDock.BackColor = System.Drawing.SystemColors.Window
        Me.mlDock.ForeColor = System.Drawing.SystemColors.ControlText
        Me.mlDock.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.mlDock.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.mlDock.Orientation = Aqua.OrientationMode.Horizontal
        Me.mlDock.DragItem = True
        Me.mlDock.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Timer1
        '
        Me.Timer1.Enabled = False
        Me.Timer1.Interval = 500
        '
        'frmDock
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(627, 211)

        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Location = New System.Drawing.Point(10, 10)
        Me.Text = "Dock"
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "Dock"
        Me.TopMost = True
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MenuFont = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmDock"
        Me.Controls.Add(Me.imgClear)
        Me.Controls.Add(Me.Image1)
        Me.Controls.Add(Me.Image2)
        Me.Controls.Add(Me.mlDock)
        Me.vb6ToolTip.SetToolTip(Me.imgClear, "清除選取的照片")
        Me.vb6ToolTip.SetToolTip(Me.Image1, "載入挑選的照片")
        Me.vb6ToolTip.SetToolTip(Me.Image2, "儲存挑選的照片")
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents imgClear As System.Windows.Forms.PictureBox
    Friend WithEvents Image1 As System.Windows.Forms.PictureBox
    Friend WithEvents Image2 As System.Windows.Forms.PictureBox
    Friend WithEvents mlDock As Aqua.MediaList
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents vb6ToolTip As System.Windows.Forms.ToolTip
End Class
