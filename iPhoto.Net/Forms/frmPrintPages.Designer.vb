<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPrintPages
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPrintPages))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.rdPages_0 = New Aqua.RadioButton()
        Me.rdPages_1 = New Aqua.RadioButton()
        Me.rdPages_2 = New Aqua.RadioButton()
        Me.txtPages = New Aqua.TextBox()
        Me.butExit = New Aqua.FlashButton()
        Me.butOk = New Aqua.FlashButton()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(50, 132)
        Me.Label1.Size = New System.Drawing.Size(298, 45)
        Me.Label1.Name = "Label1"
        Me.Label1.TabIndex = 9
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Text = "指定頁碼/文件範圍，並以逗點分隔（例如：1,3,5）"
        Me.Label1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Label1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'rdPages_0
        '
        Me.rdPages_0.Location = New System.Drawing.Point(26, 40)
        Me.rdPages_0.Name = "rdPages_0"
        Me.rdPages_0.TabIndex = 3
        Me.rdPages_0.TabStop = False
        Me.rdPages_0.Checked = True
        Me.rdPages_0.TextValue = "全部"
        Me.rdPages_0.TextGap = 8
        Me.rdPages_0.ForeColor = System.Drawing.SystemColors.WindowText
        Me.rdPages_0.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'rdPages_1
        '
        Me.rdPages_1.Location = New System.Drawing.Point(26, 72)
        Me.rdPages_1.Name = "rdPages_1"
        Me.rdPages_1.TabIndex = 4
        Me.rdPages_1.TabStop = False
        Me.rdPages_1.TextValue = "本頁"
        Me.rdPages_1.TextGap = 8
        Me.rdPages_1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.rdPages_1.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'rdPages_2
        '
        Me.rdPages_2.Location = New System.Drawing.Point(26, 104)
        Me.rdPages_2.Name = "rdPages_2"
        Me.rdPages_2.TabIndex = 5
        Me.rdPages_2.TabStop = False
        Me.rdPages_2.TextValue = "頁數"
        Me.rdPages_2.TextGap = 8
        Me.rdPages_2.ForeColor = System.Drawing.SystemColors.WindowText
        Me.rdPages_2.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtPages
        '
        Me.txtPages.Location = New System.Drawing.Point(100, 102)
        Me.txtPages.Size = New System.Drawing.Size(239, 26)
        Me.txtPages.Name = "txtPages"
        Me.txtPages.TabIndex = 10
        Me.txtPages.TabStop = False
        Me.txtPages.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtPages.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtPages.MaxLength = 0
        Me.txtPages.AutoSelect = True
        Me.txtPages.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtPages.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtPages.Font = New System.Drawing.Font("Times New Roman", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(84, 194)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 2
        Me.butExit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(202, 194)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 0
        Me.butOk.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmPrintPages
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(389, 241)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "指定列印頁次"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmPrintPages"
        Me.Controls.Add(Me.rdPages_0)
        Me.Controls.Add(Me.rdPages_1)
        Me.Controls.Add(Me.rdPages_2)
        Me.Controls.Add(Me.txtPages)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.Label1)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents rdPages_0 As Aqua.RadioButton
    Friend WithEvents rdPages_1 As Aqua.RadioButton
    Friend WithEvents rdPages_2 As Aqua.RadioButton
    Friend WithEvents txtPages As Aqua.TextBox
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butOk As Aqua.FlashButton
End Class
