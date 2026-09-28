<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPhotoInfoBatch_1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPhotoInfoBatch_1))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.frmClass = New Aqua.Panel()
        Me.tvList = New System.Windows.Forms.TreeView()
        Me.mlList = New Aqua.MediaList()
        Me.butExit = New Aqua.FlashButton()
        Me.butNext = New Aqua.FlashButton()
        Me.frmClass.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(12, 40)
        Me.Label1.Size = New System.Drawing.Size(352, 21)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 6
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "步驟一：選取要設定相片資訊的相本"
        Me.Label1.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'tvList
        '
        Me.tvList.Location = New System.Drawing.Point(4, 4)
        Me.tvList.Size = New System.Drawing.Size(291, 764)
        Me.tvList.Name = "tvList"
        Me.tvList.TabIndex = 2
        Me.tvList.LabelEdit = False
        Me.tvList.HideSelection = False
        Me.tvList.Checkboxes = True
        Me.tvList.FullRowSelect = True
        Me.tvList.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmClass
        '
        Me.frmClass.Location = New System.Drawing.Point(10, 72)
        Me.frmClass.Size = New System.Drawing.Size(297, 771)
        Me.frmClass.Name = "frmClass"
        Me.frmClass.TabIndex = 1
        Me.frmClass.TabStop = False
        Me.frmClass.ForeColor = System.Drawing.SystemColors.ControlText
        Me.frmClass.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.frmClass.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.frmClass.PanelStyle = Aqua.PanelStyleMode.Container
        Me.frmClass.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.frmClass.Controls.Add(Me.tvList)
        '
        'mlList
        '
        Me.mlList.Location = New System.Drawing.Point(312, 72)
        Me.mlList.Size = New System.Drawing.Size(921, 771)
        Me.mlList.Name = "mlList"
        Me.mlList.TabIndex = 3
        Me.mlList.ShowCheckBox = False
        Me.mlList.DragItem = False
        Me.mlList.DropItem = False
        Me.mlList.BackColor = System.Drawing.SystemColors.Window
        Me.mlList.ForeColor = System.Drawing.SystemColors.ControlText
        Me.mlList.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.mlList.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.mlList.Limit = 4
        Me.mlList.BorderSize = 6
        Me.mlList.MarkAlignment = System.Drawing.ContentAlignment.BottomLeft
        Me.mlList.MarkPosition = Aqua.MediaItemMarkPosition.SnapToPhoto
        Me.mlList.MarkImage = CType(resources.GetObject("mlList.MarkImage"), System.Drawing.Image)
        Me.mlList.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(474, 852)
        Me.butExit.Size = New System.Drawing.Size(109, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 4
        Me.butExit.TabStop = False
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butNext
        '
        Me.butNext.Location = New System.Drawing.Point(612, 852)
        Me.butNext.Size = New System.Drawing.Size(109, 27)
        Me.butNext.Name = "butNext"
        Me.butNext.TabIndex = 5
        Me.butNext.TabStop = False
        Me.butNext.Enabled = False
        Me.butNext.Text = "下一步"
        Me.butNext.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmPhotoInfoBatch_1
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1249, 891)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "批次詳細資料修改"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmPhotoInfoBatch_1"
        Me.Controls.Add(Me.frmClass)
        Me.Controls.Add(Me.mlList)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butNext)
        Me.Controls.Add(Me.Label1)
        Me.frmClass.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents frmClass As Aqua.Panel
    Friend WithEvents tvList As System.Windows.Forms.TreeView
    Friend WithEvents mlList As Aqua.MediaList
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butNext As Aqua.FlashButton
End Class
