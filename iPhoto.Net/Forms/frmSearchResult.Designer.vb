<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSearchResult
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSearchResult))
        Me.imgInfo = New System.Windows.Forms.PictureBox()
        Me.Grid1 = New Aqua.Grid()
        Me.butDock = New Aqua.ThinButton()
        Me.butExit = New Aqua.ThinButton()
        Me.butFavorite = New Aqua.ThinButton()
        Me.butResearch = New Aqua.ThinButton()
        Me.SuspendLayout()
        '
        'imgInfo
        '
        Me.imgInfo.Location = New System.Drawing.Point(80, 680)
        Me.imgInfo.Size = New System.Drawing.Size(12, 12)
        Me.imgInfo.Name = "imgInfo"
        Me.imgInfo.BackColor = System.Drawing.Color.Transparent
        Me.imgInfo.Image = CType(resources.GetObject("imgInfo.Image"), System.Drawing.Image)
        '
        'Grid1
        '
        Me.Grid1.Location = New System.Drawing.Point(12, 34)
        Me.Grid1.Size = New System.Drawing.Size(935, 571)
        Me.Grid1.Columns.Add(New Aqua.GridColumn("  ", 24, Aqua.AlignmentConstants.LeftJustify, False, Aqua.SortOrder.None, Nothing))
        Me.Grid1.Columns.Add(New Aqua.GridColumn("  ", 24, Aqua.AlignmentConstants.LeftJustify, False, Aqua.SortOrder.None, Nothing))
        Me.Grid1.Columns.Add(New Aqua.GridColumn("名稱", 107, Aqua.AlignmentConstants.LeftJustify, False, Aqua.SortOrder.None, Nothing))
        Me.Grid1.Columns.Add(New Aqua.GridColumn("日期", 200, Aqua.AlignmentConstants.LeftJustify, False, Aqua.SortOrder.None, Nothing))
        Me.Grid1.Columns.Add(New Aqua.GridColumn("位置", 0, Aqua.AlignmentConstants.LeftJustify, False, Aqua.SortOrder.None, Nothing))
        Me.Grid1.Name = "Grid1"
        Me.Grid1.TabIndex = 0
        Me.Grid1.BackColor = System.Drawing.SystemColors.Window
        Me.Grid1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Grid1.Parhelia = True
        Me.Grid1.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butDock
        '
        Me.butDock.Location = New System.Drawing.Point(480, 618)
        Me.butDock.Size = New System.Drawing.Size(151, 27)
        Me.butDock.Name = "butDock"
        Me.butDock.TabIndex = 2
        Me.butDock.Text = "加入 Dockbar"
        Me.butDock.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butDock.ForeColor = System.Drawing.Color.Black
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(172, 618)
        Me.butExit.Size = New System.Drawing.Size(135, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 3
        Me.butExit.Text = "關閉"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butExit.ForeColor = System.Drawing.Color.Black
        '
        'butFavorite
        '
        Me.butFavorite.Location = New System.Drawing.Point(652, 618)
        Me.butFavorite.Size = New System.Drawing.Size(135, 27)
        Me.butFavorite.Name = "butFavorite"
        Me.butFavorite.TabIndex = 4
        Me.butFavorite.Text = "成立攝影輯"
        Me.butFavorite.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butFavorite.ForeColor = System.Drawing.Color.Black
        '
        'butResearch
        '
        Me.butResearch.Location = New System.Drawing.Point(320, 618)
        Me.butResearch.Size = New System.Drawing.Size(135, 27)
        Me.butResearch.Name = "butResearch"
        Me.butResearch.TabIndex = 5
        Me.butResearch.Text = "重新尋找"
        Me.butResearch.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butResearch.ForeColor = System.Drawing.Color.Black
        '
        'frmSearchResult
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(959, 663)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "尋找結果"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmSearchResult"
        Me.Controls.Add(Me.imgInfo)
        Me.Controls.Add(Me.Grid1)
        Me.Controls.Add(Me.butDock)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butFavorite)
        Me.Controls.Add(Me.butResearch)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents imgInfo As System.Windows.Forms.PictureBox
    Friend WithEvents Grid1 As Aqua.Grid
    Friend WithEvents butDock As Aqua.ThinButton
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents butFavorite As Aqua.ThinButton
    Friend WithEvents butResearch As Aqua.ThinButton
End Class
