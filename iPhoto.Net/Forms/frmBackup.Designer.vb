<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBackup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBackup))
        Me.lblSource = New System.Windows.Forms.Label()
        Me.lblTarget = New System.Windows.Forms.Label()
        Me.lblLog = New System.Windows.Forms.Label()
        Me.Line1 = New System.Windows.Forms.Label()
        Me.imgSource_0 = New Aqua.IconBox()
        Me.imgSource_1 = New Aqua.IconBox()
        Me.imgTarget = New System.Windows.Forms.PictureBox()
        Me.lstSource = New Aqua.ItemListBox()
        Me.txtTarget = New Aqua.TextBox()
        Me.chkApplyAll = New Aqua.CheckBox()
        Me.txtLog = New Aqua.EditBox()
        Me.butStart = New Aqua.ThinButton()
        Me.butExit = New Aqua.ThinButton()
        Me.ProgressBar1 = New Aqua.ProgressBar()
        CType(Me.imgTarget, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblSource
        '
        Me.lblSource.AutoSize = True
        Me.lblSource.BackColor = System.Drawing.Color.Transparent
        Me.lblSource.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblSource.Location = New System.Drawing.Point(14, 40)
        Me.lblSource.Name = "lblSource"
        Me.lblSource.Size = New System.Drawing.Size(88, 19)
        Me.lblSource.TabIndex = 10
        Me.lblSource.Text = "備份來源"
        '
        'lblTarget
        '
        Me.lblTarget.AutoSize = True
        Me.lblTarget.BackColor = System.Drawing.Color.Transparent
        Me.lblTarget.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblTarget.Location = New System.Drawing.Point(14, 270)
        Me.lblTarget.Name = "lblTarget"
        Me.lblTarget.Size = New System.Drawing.Size(88, 19)
        Me.lblTarget.TabIndex = 11
        Me.lblTarget.Text = "備份目的"
        '
        'lblLog
        '
        Me.lblLog.AutoSize = True
        Me.lblLog.BackColor = System.Drawing.Color.Transparent
        Me.lblLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblLog.Location = New System.Drawing.Point(14, 346)
        Me.lblLog.Name = "lblLog"
        Me.lblLog.Size = New System.Drawing.Size(48, 19)
        Me.lblLog.TabIndex = 12
        Me.lblLog.Text = "日誌"
        '
        'Line1
        '
        Me.Line1.AutoSize = False
        Me.Line1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Line1.Location = New System.Drawing.Point(11, 336)
        Me.Line1.Name = "Line1"
        Me.Line1.Size = New System.Drawing.Size(742, 1)
        '
        'imgSource_0
        '
        Me.imgSource_0.BackColor = System.Drawing.Color.Transparent
        Me.imgSource_0.Image = CType(resources.GetObject("imgSource_0.Image"), System.Drawing.Image)
        Me.imgSource_0.Location = New System.Drawing.Point(108, 39)
        Me.imgSource_0.Name = "imgSource_0"
        Me.imgSource_0.Size = New System.Drawing.Size(30, 21)
        Me.imgSource_0.TabIndex = 0
        Me.imgSource_0.TabStop = False
        Me.imgSource_0.TransparencyKey = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer))
        '
        'imgSource_1
        '
        Me.imgSource_1.BackColor = System.Drawing.Color.Transparent
        Me.imgSource_1.Image = CType(resources.GetObject("imgSource_1.Image"), System.Drawing.Image)
        Me.imgSource_1.Location = New System.Drawing.Point(142, 39)
        Me.imgSource_1.Name = "imgSource_1"
        Me.imgSource_1.Size = New System.Drawing.Size(30, 21)
        Me.imgSource_1.TabIndex = 1
        Me.imgSource_1.TabStop = False
        Me.imgSource_1.TransparencyKey = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer))
        '
        'imgTarget
        '
        Me.imgTarget.BackColor = System.Drawing.Color.Transparent
        Me.imgTarget.Image = CType(resources.GetObject("imgTarget.Image"), System.Drawing.Image)
        Me.imgTarget.Location = New System.Drawing.Point(108, 269)
        Me.imgTarget.Name = "imgTarget"
        Me.imgTarget.Size = New System.Drawing.Size(30, 21)
        Me.imgTarget.TabIndex = 3
        Me.imgTarget.TabStop = False
        '
        'lstSource
        '
        Me.lstSource.BackColor = System.Drawing.SystemColors.Window
        Me.lstSource.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.lstSource.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.lstSource.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.lstSource.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lstSource.Location = New System.Drawing.Point(12, 64)
        Me.lstSource.Name = "lstSource"
        Me.lstSource.Size = New System.Drawing.Size(741, 194)
        Me.lstSource.TabIndex = 2
        '
        'txtTarget
        '
        Me.txtTarget.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtTarget.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtTarget.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtTarget.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.txtTarget.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtTarget.Location = New System.Drawing.Point(12, 294)
        Me.txtTarget.MaxLength = 0
        Me.txtTarget.Name = "txtTarget"
        Me.txtTarget.Size = New System.Drawing.Size(741, 27)
        Me.txtTarget.TabIndex = 4
        '
        'chkApplyAll
        '
        Me.chkApplyAll.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.chkApplyAll.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.chkApplyAll.Location = New System.Drawing.Point(560, 270)
        Me.chkApplyAll.Name = "chkApplyAll"
        Me.chkApplyAll.TabIndex = 5
        Me.chkApplyAll.TextValue = "套用至所有來源"
        Me.chkApplyAll.TextGap = 6
        '
        'txtLog
        '
        Me.txtLog.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtLog.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtLog.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtLog.Font = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.txtLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtLog.Location = New System.Drawing.Point(12, 370)
        Me.txtLog.Locked = True
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLog.Size = New System.Drawing.Size(741, 158)
        Me.txtLog.TabIndex = 6
        Me.txtLog.TabStop = False
        '
        'butStart
        '
        Me.butStart.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butStart.ForeColor = System.Drawing.Color.Black
        Me.butStart.Location = New System.Drawing.Point(390, 540)
        Me.butStart.Name = "butStart"
        Me.butStart.Size = New System.Drawing.Size(103, 27)
        Me.butStart.TabIndex = 8
        Me.butStart.Text = "開始同步"
        '
        'butExit
        '
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butExit.ForeColor = System.Drawing.Color.Black
        Me.butExit.Location = New System.Drawing.Point(272, 540)
        Me.butExit.Name = "butExit"
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.TabIndex = 7
        Me.butExit.Text = "結束"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(2, 582)
        Me.ProgressBar1.Maximum = 100
        Me.ProgressBar1.Minimum = 0
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(761, 21)
        Me.ProgressBar1.TabIndex = 13
        '
        'frmBackup
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ClientSize = New System.Drawing.Size(765, 605)
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MinButton = False
        Me.Name = "frmBackup"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "同步備份"
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Controls.Add(Me.imgSource_0)
        Me.Controls.Add(Me.imgSource_1)
        Me.Controls.Add(Me.lstSource)
        Me.Controls.Add(Me.imgTarget)
        Me.Controls.Add(Me.txtTarget)
        Me.Controls.Add(Me.chkApplyAll)
        Me.Controls.Add(Me.txtLog)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butStart)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblSource)
        Me.Controls.Add(Me.lblTarget)
        Me.Controls.Add(Me.lblLog)
        Me.Controls.Add(Me.Line1)
        CType(Me.imgTarget, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblSource As System.Windows.Forms.Label
    Friend WithEvents lblTarget As System.Windows.Forms.Label
    Friend WithEvents lblLog As System.Windows.Forms.Label
    Friend WithEvents Line1 As System.Windows.Forms.Label
    Friend WithEvents imgSource_0 As Aqua.IconBox
    Friend WithEvents imgSource_1 As Aqua.IconBox
    Friend WithEvents imgTarget As System.Windows.Forms.PictureBox
    Friend WithEvents lstSource As Aqua.ItemListBox
    Friend WithEvents txtTarget As Aqua.TextBox
    Friend WithEvents chkApplyAll As Aqua.CheckBox
    Friend WithEvents txtLog As Aqua.EditBox
    Friend WithEvents butStart As Aqua.ThinButton
    Friend WithEvents butExit As Aqua.ThinButton
    Friend WithEvents ProgressBar1 As Aqua.ProgressBar
End Class
