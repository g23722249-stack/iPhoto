<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDropFileList
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDropFileList))
        Me.Panel1 = New Aqua.Panel()
        Me.Viewer = New System.Windows.Forms.ListView()
        Me.colDate = New System.Windows.Forms.ColumnHeader()
        Me.colFile = New System.Windows.Forms.ColumnHeader()
        Me.cmdUnload = New Aqua.FlashButton()
        Me.cmdSave = New Aqua.FlashButton()
        Me.cmdSaveToFolder = New Aqua.FlashButton()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Viewer
        '
        Me.Viewer.Location = New System.Drawing.Point(4, 2)
        Me.Viewer.Size = New System.Drawing.Size(560, 603)
        Me.Viewer.Name = "Viewer"
        Me.Viewer.TabIndex = 5
        Me.Viewer.LabelEdit = False
        Me.Viewer.View = System.Windows.Forms.View.Details
        Me.Viewer.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colDate, Me.colFile})
        Me.Viewer.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.Viewer.TabStop = False
        Me.Viewer.LabelWrap = True
        Me.Viewer.HideSelection = False
        Me.Viewer.AllowColumnReorder = True
        Me.Viewer.FullRowSelect = True
        Me.Viewer.GridLines = True
        Me.Viewer.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Viewer.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Viewer.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'Panel1
        '
        Me.Panel1.Location = New System.Drawing.Point(16, 38)
        Me.Panel1.Size = New System.Drawing.Size(567, 607)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.TabIndex = 4
        Me.Panel1.TabStop = False
        Me.Panel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Panel1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.Panel1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.Panel1.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Panel1.Controls.Add(Me.Viewer)
        '
        'colDate
        '
        Me.colDate.Text = "日期"
        Me.colDate.Width = 282
        '
        'colFile
        '
        Me.colFile.Text = "檔名"
        Me.colFile.Width = 964
        '
        'cmdUnload
        '
        Me.cmdUnload.Location = New System.Drawing.Point(447, 658)
        Me.cmdUnload.Size = New System.Drawing.Size(103, 27)
        Me.cmdUnload.Name = "cmdUnload"
        Me.cmdUnload.TabIndex = 2
        Me.cmdUnload.Text = "關閉"
        Me.cmdUnload.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'cmdSave
        '
        Me.cmdSave.Location = New System.Drawing.Point(50, 658)
        Me.cmdSave.Size = New System.Drawing.Size(179, 27)
        Me.cmdSave.Name = "cmdSave"
        Me.cmdSave.TabIndex = 0
        Me.cmdSave.Text = "存成檔案清單"
        Me.cmdSave.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'cmdSaveToFolder
        '
        Me.cmdSaveToFolder.Location = New System.Drawing.Point(248, 658)
        Me.cmdSaveToFolder.Size = New System.Drawing.Size(179, 27)
        Me.cmdSaveToFolder.Name = "cmdSaveToFolder"
        Me.cmdSaveToFolder.TabIndex = 1
        Me.cmdSaveToFolder.Text = "複製到資料夾"
        Me.cmdSaveToFolder.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmDropFileList
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(599, 697)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "檔案清單"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmDropFileList"
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.cmdUnload)
        Me.Controls.Add(Me.cmdSave)
        Me.Controls.Add(Me.cmdSaveToFolder)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel1 As Aqua.Panel
    Friend WithEvents Viewer As System.Windows.Forms.ListView
    Friend WithEvents colDate As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFile As System.Windows.Forms.ColumnHeader
    Friend WithEvents cmdUnload As Aqua.FlashButton
    Friend WithEvents cmdSave As Aqua.FlashButton
    Friend WithEvents cmdSaveToFolder As Aqua.FlashButton
End Class
