<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDateTime
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDateTime))
        Me.lblFileName = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtYYYY_0 = New Aqua.TextBox()
        Me.txtMM_0 = New Aqua.TextBox()
        Me.txtDD_0 = New Aqua.TextBox()
        Me.txtHH_0 = New Aqua.TextBox()
        Me.txtNN_0 = New Aqua.TextBox()
        Me.txtSS_0 = New Aqua.TextBox()
        Me.txtYYYY_1 = New Aqua.TextBox()
        Me.txtMM_1 = New Aqua.TextBox()
        Me.txtDD_1 = New Aqua.TextBox()
        Me.txtHH_1 = New Aqua.TextBox()
        Me.txtNN_1 = New Aqua.TextBox()
        Me.txtSS_1 = New Aqua.TextBox()
        Me.txtYYYY_2 = New Aqua.TextBox()
        Me.txtMM_2 = New Aqua.TextBox()
        Me.txtDD_2 = New Aqua.TextBox()
        Me.txtHH_2 = New Aqua.TextBox()
        Me.txtNN_2 = New Aqua.TextBox()
        Me.txtSS_2 = New Aqua.TextBox()
        Me.butSave = New Aqua.FlashButton()
        Me.cmdSaveToFolder = New Aqua.FlashButton()
        Me.chkSaveMode_0 = New Aqua.CheckBox()
        Me.chkSaveMode_1 = New Aqua.CheckBox()
        Me.cmdCancel = New Aqua.FlashButton()
        Me.SuspendLayout()
        '
        'lblFileName
        '
        Me.lblFileName.Location = New System.Drawing.Point(4, 32)
        Me.lblFileName.Size = New System.Drawing.Size(150, 19)
        Me.lblFileName.Name = "lblFileName"
        Me.lblFileName.TabIndex = 10
        Me.lblFileName.BackColor = System.Drawing.Color.Transparent
        Me.lblFileName.AutoSize = True
        Me.lblFileName.Text = "C:\Autoexec.Bat"
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(14, 68)
        Me.Label7.Size = New System.Drawing.Size(120, 19)
        Me.Label7.Name = "Label7"
        Me.Label7.TabIndex = 12
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.AutoSize = True
        Me.Label7.Text = "檔案建立日期"
        Me.Label7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(308, 68)
        Me.Label2.Size = New System.Drawing.Size(40, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.TabIndex = 15
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.AutoSize = True
        Me.Label2.Text = "時間"
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(44, 98)
        Me.Label1.Size = New System.Drawing.Size(90, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 20
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "Exif 日期"
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(308, 98)
        Me.Label3.Size = New System.Drawing.Size(40, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.TabIndex = 23
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.AutoSize = True
        Me.Label3.Text = "時間"
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(34, 128)
        Me.Label4.Size = New System.Drawing.Size(100, 19)
        Me.Label4.Name = "Label4"
        Me.Label4.TabIndex = 27
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.AutoSize = True
        Me.Label4.Text = "調整的日期"
        Me.Label4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(308, 128)
        Me.Label5.Size = New System.Drawing.Size(40, 19)
        Me.Label5.Name = "Label5"
        Me.Label5.TabIndex = 28
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.AutoSize = True
        Me.Label5.Text = "時間"
        Me.Label5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'txtYYYY_0
        '
        Me.txtYYYY_0.Location = New System.Drawing.Point(138, 64)
        Me.txtYYYY_0.Size = New System.Drawing.Size(65, 25)
        Me.txtYYYY_0.Name = "txtYYYY_0"
        Me.txtYYYY_0.TabIndex = 11
        Me.txtYYYY_0.TabStop = False
        Me.txtYYYY_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtYYYY_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtYYYY_0.MaxLength = 0
        Me.txtYYYY_0.Locked = True
        Me.txtYYYY_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtYYYY_0.AutoSelect = True
        Me.txtYYYY_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtYYYY_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtYYYY_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtMM_0
        '
        Me.txtMM_0.Location = New System.Drawing.Point(206, 64)
        Me.txtMM_0.Size = New System.Drawing.Size(45, 25)
        Me.txtMM_0.Name = "txtMM_0"
        Me.txtMM_0.TabIndex = 13
        Me.txtMM_0.TabStop = False
        Me.txtMM_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtMM_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtMM_0.MaxLength = 0
        Me.txtMM_0.Locked = True
        Me.txtMM_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtMM_0.AutoSelect = True
        Me.txtMM_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtMM_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtMM_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtDD_0
        '
        Me.txtDD_0.Location = New System.Drawing.Point(254, 64)
        Me.txtDD_0.Size = New System.Drawing.Size(45, 25)
        Me.txtDD_0.Name = "txtDD_0"
        Me.txtDD_0.TabIndex = 14
        Me.txtDD_0.TabStop = False
        Me.txtDD_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtDD_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtDD_0.MaxLength = 0
        Me.txtDD_0.Locked = True
        Me.txtDD_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtDD_0.AutoSelect = True
        Me.txtDD_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtDD_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtDD_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtHH_0
        '
        Me.txtHH_0.Location = New System.Drawing.Point(354, 64)
        Me.txtHH_0.Size = New System.Drawing.Size(45, 25)
        Me.txtHH_0.Name = "txtHH_0"
        Me.txtHH_0.TabIndex = 16
        Me.txtHH_0.TabStop = False
        Me.txtHH_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtHH_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtHH_0.MaxLength = 0
        Me.txtHH_0.Locked = True
        Me.txtHH_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtHH_0.AutoSelect = True
        Me.txtHH_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtHH_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtHH_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtNN_0
        '
        Me.txtNN_0.Location = New System.Drawing.Point(402, 64)
        Me.txtNN_0.Size = New System.Drawing.Size(45, 25)
        Me.txtNN_0.Name = "txtNN_0"
        Me.txtNN_0.TabIndex = 17
        Me.txtNN_0.TabStop = False
        Me.txtNN_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtNN_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtNN_0.MaxLength = 0
        Me.txtNN_0.Locked = True
        Me.txtNN_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtNN_0.AutoSelect = True
        Me.txtNN_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtNN_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtNN_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtSS_0
        '
        Me.txtSS_0.Location = New System.Drawing.Point(450, 64)
        Me.txtSS_0.Size = New System.Drawing.Size(45, 25)
        Me.txtSS_0.Name = "txtSS_0"
        Me.txtSS_0.TabIndex = 18
        Me.txtSS_0.TabStop = False
        Me.txtSS_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtSS_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtSS_0.MaxLength = 0
        Me.txtSS_0.Locked = True
        Me.txtSS_0.Alignment = Aqua.AlignmentConstants.Center
        Me.txtSS_0.AutoSelect = True
        Me.txtSS_0.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtSS_0.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtSS_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtYYYY_1
        '
        Me.txtYYYY_1.Location = New System.Drawing.Point(138, 94)
        Me.txtYYYY_1.Size = New System.Drawing.Size(65, 25)
        Me.txtYYYY_1.Name = "txtYYYY_1"
        Me.txtYYYY_1.TabIndex = 19
        Me.txtYYYY_1.TabStop = False
        Me.txtYYYY_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtYYYY_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtYYYY_1.MaxLength = 0
        Me.txtYYYY_1.Locked = True
        Me.txtYYYY_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtYYYY_1.AutoSelect = True
        Me.txtYYYY_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtYYYY_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtYYYY_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtMM_1
        '
        Me.txtMM_1.Location = New System.Drawing.Point(206, 94)
        Me.txtMM_1.Size = New System.Drawing.Size(45, 25)
        Me.txtMM_1.Name = "txtMM_1"
        Me.txtMM_1.TabIndex = 21
        Me.txtMM_1.TabStop = False
        Me.txtMM_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtMM_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtMM_1.MaxLength = 0
        Me.txtMM_1.Locked = True
        Me.txtMM_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtMM_1.AutoSelect = True
        Me.txtMM_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtMM_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtMM_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtDD_1
        '
        Me.txtDD_1.Location = New System.Drawing.Point(254, 94)
        Me.txtDD_1.Size = New System.Drawing.Size(45, 25)
        Me.txtDD_1.Name = "txtDD_1"
        Me.txtDD_1.TabIndex = 22
        Me.txtDD_1.TabStop = False
        Me.txtDD_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtDD_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtDD_1.MaxLength = 0
        Me.txtDD_1.Locked = True
        Me.txtDD_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtDD_1.AutoSelect = True
        Me.txtDD_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtDD_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtDD_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtHH_1
        '
        Me.txtHH_1.Location = New System.Drawing.Point(354, 94)
        Me.txtHH_1.Size = New System.Drawing.Size(45, 25)
        Me.txtHH_1.Name = "txtHH_1"
        Me.txtHH_1.TabIndex = 24
        Me.txtHH_1.TabStop = False
        Me.txtHH_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtHH_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtHH_1.MaxLength = 0
        Me.txtHH_1.Locked = True
        Me.txtHH_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtHH_1.AutoSelect = True
        Me.txtHH_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtHH_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtHH_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtNN_1
        '
        Me.txtNN_1.Location = New System.Drawing.Point(402, 94)
        Me.txtNN_1.Size = New System.Drawing.Size(45, 25)
        Me.txtNN_1.Name = "txtNN_1"
        Me.txtNN_1.TabIndex = 25
        Me.txtNN_1.TabStop = False
        Me.txtNN_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtNN_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtNN_1.MaxLength = 0
        Me.txtNN_1.Locked = True
        Me.txtNN_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtNN_1.AutoSelect = True
        Me.txtNN_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtNN_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtNN_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtSS_1
        '
        Me.txtSS_1.Location = New System.Drawing.Point(450, 94)
        Me.txtSS_1.Size = New System.Drawing.Size(45, 25)
        Me.txtSS_1.Name = "txtSS_1"
        Me.txtSS_1.TabIndex = 26
        Me.txtSS_1.TabStop = False
        Me.txtSS_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.txtSS_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtSS_1.MaxLength = 0
        Me.txtSS_1.Locked = True
        Me.txtSS_1.Alignment = Aqua.AlignmentConstants.Center
        Me.txtSS_1.AutoSelect = True
        Me.txtSS_1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtSS_1.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtSS_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtYYYY_2
        '
        Me.txtYYYY_2.Location = New System.Drawing.Point(138, 124)
        Me.txtYYYY_2.Size = New System.Drawing.Size(65, 25)
        Me.txtYYYY_2.Name = "txtYYYY_2"
        Me.txtYYYY_2.TabIndex = 0
        Me.txtYYYY_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtYYYY_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtYYYY_2.MaxLength = 0
        Me.txtYYYY_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtYYYY_2.AutoSelect = True
        Me.txtYYYY_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtYYYY_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtYYYY_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtMM_2
        '
        Me.txtMM_2.Location = New System.Drawing.Point(206, 124)
        Me.txtMM_2.Size = New System.Drawing.Size(45, 25)
        Me.txtMM_2.Name = "txtMM_2"
        Me.txtMM_2.TabIndex = 1
        Me.txtMM_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtMM_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtMM_2.MaxLength = 0
        Me.txtMM_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtMM_2.AutoSelect = True
        Me.txtMM_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtMM_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtMM_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtDD_2
        '
        Me.txtDD_2.Location = New System.Drawing.Point(254, 124)
        Me.txtDD_2.Size = New System.Drawing.Size(45, 25)
        Me.txtDD_2.Name = "txtDD_2"
        Me.txtDD_2.TabIndex = 2
        Me.txtDD_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtDD_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtDD_2.MaxLength = 0
        Me.txtDD_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtDD_2.AutoSelect = True
        Me.txtDD_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtDD_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtDD_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtHH_2
        '
        Me.txtHH_2.Location = New System.Drawing.Point(354, 124)
        Me.txtHH_2.Size = New System.Drawing.Size(45, 25)
        Me.txtHH_2.Name = "txtHH_2"
        Me.txtHH_2.TabIndex = 3
        Me.txtHH_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtHH_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtHH_2.MaxLength = 0
        Me.txtHH_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtHH_2.AutoSelect = True
        Me.txtHH_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtHH_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtHH_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtNN_2
        '
        Me.txtNN_2.Location = New System.Drawing.Point(402, 124)
        Me.txtNN_2.Size = New System.Drawing.Size(45, 25)
        Me.txtNN_2.Name = "txtNN_2"
        Me.txtNN_2.TabIndex = 4
        Me.txtNN_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtNN_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtNN_2.MaxLength = 0
        Me.txtNN_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtNN_2.AutoSelect = True
        Me.txtNN_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtNN_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtNN_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtSS_2
        '
        Me.txtSS_2.Location = New System.Drawing.Point(450, 124)
        Me.txtSS_2.Size = New System.Drawing.Size(45, 25)
        Me.txtSS_2.Name = "txtSS_2"
        Me.txtSS_2.TabIndex = 5
        Me.txtSS_2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtSS_2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtSS_2.MaxLength = 0
        Me.txtSS_2.Alignment = Aqua.AlignmentConstants.Center
        Me.txtSS_2.AutoSelect = True
        Me.txtSS_2.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtSS_2.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtSS_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butSave
        '
        Me.butSave.Location = New System.Drawing.Point(44, 240)
        Me.butSave.Size = New System.Drawing.Size(93, 27)
        Me.butSave.Name = "butSave"
        Me.butSave.TabIndex = 6
        Me.butSave.Text = "存檔"
        Me.butSave.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'cmdSaveToFolder
        '
        Me.cmdSaveToFolder.Location = New System.Drawing.Point(150, 240)
        Me.cmdSaveToFolder.Size = New System.Drawing.Size(219, 27)
        Me.cmdSaveToFolder.Name = "cmdSaveToFolder"
        Me.cmdSaveToFolder.TabIndex = 7
        Me.cmdSaveToFolder.Text = "同步所有的相片"
        Me.cmdSaveToFolder.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'chkSaveMode_0
        '
        Me.chkSaveMode_0.Location = New System.Drawing.Point(157, 168)
        Me.chkSaveMode_0.Name = "chkSaveMode_0"
        Me.chkSaveMode_0.TabIndex = 29
        Me.chkSaveMode_0.TabStop = False
        Me.chkSaveMode_0.Checked = True
        Me.chkSaveMode_0.TextValue = "修改檔案的建立日期"
        Me.chkSaveMode_0.TextGap = 8
        Me.chkSaveMode_0.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'chkSaveMode_1
        '
        Me.chkSaveMode_1.Location = New System.Drawing.Point(157, 194)
        Me.chkSaveMode_1.Name = "chkSaveMode_1"
        Me.chkSaveMode_1.TabIndex = 31
        Me.chkSaveMode_1.TabStop = False
        Me.chkSaveMode_1.Checked = True
        Me.chkSaveMode_1.TextValue = "修改 EXIF 資訊"
        Me.chkSaveMode_1.TextGap = 8
        Me.chkSaveMode_1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        '
        'cmdCancel
        '
        Me.cmdCancel.Location = New System.Drawing.Point(382, 240)
        Me.cmdCancel.Size = New System.Drawing.Size(93, 27)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.TabIndex = 8
        Me.cmdCancel.Text = "關閉"
        Me.cmdCancel.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmDateTime
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(517, 293)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Text = "照片時間調整"
        Me.ShowInTaskbar = False
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "照片時間調整"
        Me.Font = New System.Drawing.Font("新細明體", 9!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmDateTime"
        Me.Controls.Add(Me.txtYYYY_0)
        Me.Controls.Add(Me.txtMM_0)
        Me.Controls.Add(Me.txtDD_0)
        Me.Controls.Add(Me.txtHH_0)
        Me.Controls.Add(Me.txtNN_0)
        Me.Controls.Add(Me.txtSS_0)
        Me.Controls.Add(Me.txtYYYY_1)
        Me.Controls.Add(Me.txtMM_1)
        Me.Controls.Add(Me.txtDD_1)
        Me.Controls.Add(Me.txtHH_1)
        Me.Controls.Add(Me.txtNN_1)
        Me.Controls.Add(Me.txtSS_1)
        Me.Controls.Add(Me.txtYYYY_2)
        Me.Controls.Add(Me.txtMM_2)
        Me.Controls.Add(Me.txtDD_2)
        Me.Controls.Add(Me.txtHH_2)
        Me.Controls.Add(Me.txtNN_2)
        Me.Controls.Add(Me.txtSS_2)
        Me.Controls.Add(Me.butSave)
        Me.Controls.Add(Me.cmdSaveToFolder)
        Me.Controls.Add(Me.chkSaveMode_0)
        Me.Controls.Add(Me.chkSaveMode_1)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.lblFileName)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblFileName As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtYYYY_0 As Aqua.TextBox
    Friend WithEvents txtMM_0 As Aqua.TextBox
    Friend WithEvents txtDD_0 As Aqua.TextBox
    Friend WithEvents txtHH_0 As Aqua.TextBox
    Friend WithEvents txtNN_0 As Aqua.TextBox
    Friend WithEvents txtSS_0 As Aqua.TextBox
    Friend WithEvents txtYYYY_1 As Aqua.TextBox
    Friend WithEvents txtMM_1 As Aqua.TextBox
    Friend WithEvents txtDD_1 As Aqua.TextBox
    Friend WithEvents txtHH_1 As Aqua.TextBox
    Friend WithEvents txtNN_1 As Aqua.TextBox
    Friend WithEvents txtSS_1 As Aqua.TextBox
    Friend WithEvents txtYYYY_2 As Aqua.TextBox
    Friend WithEvents txtMM_2 As Aqua.TextBox
    Friend WithEvents txtDD_2 As Aqua.TextBox
    Friend WithEvents txtHH_2 As Aqua.TextBox
    Friend WithEvents txtNN_2 As Aqua.TextBox
    Friend WithEvents txtSS_2 As Aqua.TextBox
    Friend WithEvents butSave As Aqua.FlashButton
    Friend WithEvents cmdSaveToFolder As Aqua.FlashButton
    Friend WithEvents chkSaveMode_0 As Aqua.CheckBox
    Friend WithEvents chkSaveMode_1 As Aqua.CheckBox
    Friend WithEvents cmdCancel As Aqua.FlashButton
End Class
