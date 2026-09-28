<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmKeyWords
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmKeyWords))
        Me.cboSection = New Aqua.DropDownList()
        Me.lstKeyWord = New Aqua.ItemListBox()
        Me.butExit = New Aqua.FlashButton()
        Me.butOk = New Aqua.FlashButton()
        Me.imgAddition = New Aqua.PngButton()
        Me.txtKeyWords = New Aqua.TextBox()
        Me.vb6ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout()
        '
        'cboSection
        '
        Me.cboSection.Location = New System.Drawing.Point(52, 38)
        Me.cboSection.Size = New System.Drawing.Size(291, 23)
        Me.cboSection.Name = "cboSection"
        Me.cboSection.TabIndex = 5
        Me.cboSection.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cboSection.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.cboSection.Items.Add(New Aqua.MenuItem("家人", "家人", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("朋友", "朋友", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("親戚", "親戚", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("地點", "地點", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("景點", "景點", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("學校", "學校", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("公園", "公園", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("寵物", "寵物", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("動物", "動物", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("植物", "植物", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("工作", "工作", 1, Nothing, Nothing))
        Me.cboSection.Items.Add(New Aqua.MenuItem("其它", "其它", 1, Nothing, Nothing))
        '
        'lstKeyWord
        '
        Me.lstKeyWord.Location = New System.Drawing.Point(21, 74)
        Me.lstKeyWord.Size = New System.Drawing.Size(353, 323)
        Me.lstKeyWord.Name = "lstKeyWord"
        Me.lstKeyWord.TabIndex = 0
        Me.lstKeyWord.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lstKeyWord.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.lstKeyWord.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.lstKeyWord.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butExit
        '
        Me.butExit.Location = New System.Drawing.Point(87, 440)
        Me.butExit.Size = New System.Drawing.Size(103, 27)
        Me.butExit.Name = "butExit"
        Me.butExit.TabIndex = 2
        Me.butExit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butExit.Text = "取消"
        Me.butExit.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butOk
        '
        Me.butOk.Location = New System.Drawing.Point(205, 440)
        Me.butOk.Size = New System.Drawing.Size(103, 27)
        Me.butOk.Name = "butOk"
        Me.butOk.TabIndex = 3
        Me.butOk.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.butOk.Text = "好"
        Me.butOk.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'imgAddition
        '
        Me.imgAddition.Location = New System.Drawing.Point(344, 404)
        Me.imgAddition.Size = New System.Drawing.Size(30, 21)
        Me.imgAddition.Name = "imgAddition"
        Me.imgAddition.TabIndex = 6
        Me.imgAddition.TabStop = False
        Me.imgAddition.ForeColor = System.Drawing.SystemColors.WindowText
        Me.imgAddition.Image = CType(resources.GetObject("imgAddition.Image"), System.Drawing.Image)
        Me.imgAddition.HoverZoom = 0!
        Me.imgAddition.Font = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'txtKeyWords
        '
        Me.txtKeyWords.Location = New System.Drawing.Point(21, 402)
        Me.txtKeyWords.Size = New System.Drawing.Size(319, 25)
        Me.txtKeyWords.Name = "txtKeyWords"
        Me.txtKeyWords.TabIndex = 1
        Me.txtKeyWords.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtKeyWords.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.txtKeyWords.MaxLength = 0
        Me.txtKeyWords.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.txtKeyWords.BorderFocusColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(244, Byte), Integer))
        Me.txtKeyWords.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'frmKeyWords
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(395, 479)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ShowInTaskbar = False
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "關鍵字"
        Me.Font = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("華康細圓體", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmKeyWords"
        Me.Controls.Add(Me.cboSection)
        Me.Controls.Add(Me.lstKeyWord)
        Me.Controls.Add(Me.butExit)
        Me.Controls.Add(Me.butOk)
        Me.Controls.Add(Me.imgAddition)
        Me.Controls.Add(Me.txtKeyWords)
        Me.vb6ToolTip.SetToolTip(Me.imgAddition, "新增")
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents cboSection As Aqua.DropDownList
    Friend WithEvents lstKeyWord As Aqua.ItemListBox
    Friend WithEvents butExit As Aqua.FlashButton
    Friend WithEvents butOk As Aqua.FlashButton
    Friend WithEvents imgAddition As Aqua.PngButton
    Friend WithEvents txtKeyWords As Aqua.TextBox
    Friend WithEvents vb6ToolTip As System.Windows.Forms.ToolTip
End Class
