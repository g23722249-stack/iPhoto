<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPhotoInfoBatch_2
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPhotoInfoBatch_2))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lvwMain = New System.Windows.Forms.ListView()
        Me.ilsIcons = New System.Windows.Forms.ImageList(Me.components)
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.mnuKeyWords = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.barSearch = New System.Windows.Forms.Panel()
        Me.lblBarExif = New System.Windows.Forms.Label()
        Me.lblHelp = New System.Windows.Forms.Label()
        Me.lblTip1 = New System.Windows.Forms.Label()
        Me.lblBarOne = New System.Windows.Forms.Label()
        Me.lblHelpOne = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.txtTitle = New System.Windows.Forms.TextBox()
        Me.lblCharacter = New System.Windows.Forms.Label()
        Me.txtCharacter = New System.Windows.Forms.TextBox()
        Me.cboCharacter = New System.Windows.Forms.Button()
        Me.lblDateTime = New System.Windows.Forms.Label()
        Me.txtDateTime = New System.Windows.Forms.MaskedTextBox()
        Me.lblSpot = New System.Windows.Forms.Label()
        Me.txtSpot = New System.Windows.Forms.TextBox()
        Me.cboSpot = New System.Windows.Forms.Button()
        Me.lblKeyword = New System.Windows.Forms.Label()
        Me.txtKeyword = New System.Windows.Forms.TextBox()
        Me.lblRemark = New System.Windows.Forms.Label()
        Me.txtRemark = New System.Windows.Forms.TextBox()
        Me.lblBarBatch = New System.Windows.Forms.Label()
        Me.lblHelp1 = New System.Windows.Forms.Label()
        Me.lblHelp2 = New System.Windows.Forms.Label()
        Me.lblBatchTitle = New System.Windows.Forms.Label()
        Me.txtBatchTitle = New System.Windows.Forms.TextBox()
        Me.lblBatchCharacter = New System.Windows.Forms.Label()
        Me.txtBatchCharacter = New System.Windows.Forms.TextBox()
        Me.cboBatchCharacter = New System.Windows.Forms.Button()
        Me.lblBatchSpot = New System.Windows.Forms.Label()
        Me.txtBatchSpot = New System.Windows.Forms.TextBox()
        Me.cboBatchSpot = New System.Windows.Forms.Button()
        Me.lblBatchKeyword = New System.Windows.Forms.Label()
        Me.txtBatchKeyword = New System.Windows.Forms.TextBox()
        Me.lblBatchRemark = New System.Windows.Forms.Label()
        Me.txtBatchRemark = New System.Windows.Forms.TextBox()
        Me.butBatchSave = New Aqua.ThinButton()
        Me.lblBarUtility = New System.Windows.Forms.Label()
        Me.butCheckedAll = New Aqua.ThinButton()
        Me.butUnCheckedAll = New Aqua.ThinButton()
        Me.butUnload = New Aqua.ThinButton()
        Me.picIndex = New System.Windows.Forms.Panel()
        Me.chkBuildIndex = New Aqua.CheckBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.barSearch.SuspendLayout()
        Me.picIndex.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(10, 28)
        Me.Label1.Size = New System.Drawing.Size(374, 21)
        Me.Label1.Name = "Label1"
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.AutoSize = True
        Me.Label1.Text = "步驟二：選取要設定的相片並輸入資訊"
        Me.Label1.Font = New System.Drawing.Font("華康細圓體", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lvwMain
        '
        Me.lvwMain.Location = New System.Drawing.Point(2, 56)
        Me.lvwMain.Size = New System.Drawing.Size(961, 828)
        Me.lvwMain.Name = "lvwMain"
        Me.lvwMain.TabIndex = 0
        Me.lvwMain.View = System.Windows.Forms.View.LargeIcon
        Me.lvwMain.LargeImageList = Me.ilsIcons
        Me.lvwMain.MultiSelect = True
        Me.lvwMain.HideSelection = False
        Me.lvwMain.LabelEdit = False
        Me.lvwMain.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'ilsIcons
        '
        Me.ilsIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit
        Me.ilsIcons.ImageSize = New System.Drawing.Size(120, 96)
        '
        'Timer1
        '
        Me.Timer1.Interval = 100
        '
        'mnuKeyWords
        '
        Me.mnuKeyWords.Name = "mnuKeyWords"
        Me.mnuKeyWords.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'barSearch
        '
        Me.barSearch.Location = New System.Drawing.Point(966, 56)
        Me.barSearch.Size = New System.Drawing.Size(265, 828)
        Me.barSearch.Name = "barSearch"
        Me.barSearch.AutoScroll = True
        Me.barSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.barSearch.BackColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.barSearch.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.barSearch.Controls.Add(Me.txtTitle)
        Me.barSearch.Controls.Add(Me.txtCharacter)
        Me.barSearch.Controls.Add(Me.cboCharacter)
        Me.barSearch.Controls.Add(Me.txtDateTime)
        Me.barSearch.Controls.Add(Me.txtSpot)
        Me.barSearch.Controls.Add(Me.cboSpot)
        Me.barSearch.Controls.Add(Me.txtKeyword)
        Me.barSearch.Controls.Add(Me.txtRemark)
        Me.barSearch.Controls.Add(Me.txtBatchTitle)
        Me.barSearch.Controls.Add(Me.txtBatchCharacter)
        Me.barSearch.Controls.Add(Me.cboBatchCharacter)
        Me.barSearch.Controls.Add(Me.txtBatchSpot)
        Me.barSearch.Controls.Add(Me.cboBatchSpot)
        Me.barSearch.Controls.Add(Me.txtBatchKeyword)
        Me.barSearch.Controls.Add(Me.txtBatchRemark)
        Me.barSearch.Controls.Add(Me.butBatchSave)
        Me.barSearch.Controls.Add(Me.butCheckedAll)
        Me.barSearch.Controls.Add(Me.butUnCheckedAll)
        Me.barSearch.Controls.Add(Me.butUnload)
        Me.barSearch.Controls.Add(Me.picIndex)
        Me.barSearch.Controls.Add(Me.lblBarExif)
        Me.barSearch.Controls.Add(Me.lblHelp)
        Me.barSearch.Controls.Add(Me.lblTip1)
        Me.barSearch.Controls.Add(Me.lblBarOne)
        Me.barSearch.Controls.Add(Me.lblHelpOne)
        Me.barSearch.Controls.Add(Me.lblTitle)
        Me.barSearch.Controls.Add(Me.lblCharacter)
        Me.barSearch.Controls.Add(Me.lblDateTime)
        Me.barSearch.Controls.Add(Me.lblSpot)
        Me.barSearch.Controls.Add(Me.lblKeyword)
        Me.barSearch.Controls.Add(Me.lblRemark)
        Me.barSearch.Controls.Add(Me.lblBarBatch)
        Me.barSearch.Controls.Add(Me.lblHelp1)
        Me.barSearch.Controls.Add(Me.lblHelp2)
        Me.barSearch.Controls.Add(Me.lblBatchTitle)
        Me.barSearch.Controls.Add(Me.lblBatchCharacter)
        Me.barSearch.Controls.Add(Me.lblBatchSpot)
        Me.barSearch.Controls.Add(Me.lblBatchKeyword)
        Me.barSearch.Controls.Add(Me.lblBatchRemark)
        Me.barSearch.Controls.Add(Me.lblBarUtility)
        '
        'lblBarExif
        '
        Me.lblBarExif.Location = New System.Drawing.Point(0, 0)
        Me.lblBarExif.Size = New System.Drawing.Size(245, 22)
        Me.lblBarExif.Name = "lblBarExif"
        Me.lblBarExif.Text = " 說明"
        Me.lblBarExif.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBarExif.BackColor = System.Drawing.Color.FromArgb(CType(CType(198, Byte), Integer), CType(CType(211, Byte), Integer), CType(CType(247, Byte), Integer))
        Me.lblBarExif.ForeColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(93, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.lblBarExif.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblHelp
        '
        Me.lblHelp.Location = New System.Drawing.Point(4, 24)
        Me.lblHelp.Size = New System.Drawing.Size(237, 34)
        Me.lblHelp.Name = "lblHelp"
        Me.lblHelp.Text = "關於相片的拍攝紀錄資料，可在此進行修改"
        Me.lblHelp.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'lblTip1
        '
        Me.lblTip1.Location = New System.Drawing.Point(4, 58)
        Me.lblTip1.Size = New System.Drawing.Size(237, 20)
        Me.lblTip1.Name = "lblTip1"
        Me.lblTip1.Text = "按下 Shift + 滑鼠左鍵可複選"
        Me.lblTip1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer))
        '
        'lblBarOne
        '
        Me.lblBarOne.Location = New System.Drawing.Point(0, 84)
        Me.lblBarOne.Size = New System.Drawing.Size(245, 22)
        Me.lblBarOne.Name = "lblBarOne"
        Me.lblBarOne.Text = " 目前選擇的照片"
        Me.lblBarOne.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBarOne.BackColor = System.Drawing.Color.FromArgb(CType(CType(198, Byte), Integer), CType(CType(211, Byte), Integer), CType(CType(247, Byte), Integer))
        Me.lblBarOne.ForeColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(93, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.lblBarOne.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblHelpOne
        '
        Me.lblHelpOne.Location = New System.Drawing.Point(4, 108)
        Me.lblHelpOne.Size = New System.Drawing.Size(237, 20)
        Me.lblHelpOne.Name = "lblHelpOne"
        Me.lblHelpOne.Text = "修改完後會自動存檔"
        Me.lblHelpOne.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'lblTitle
        '
        Me.lblTitle.Location = New System.Drawing.Point(4, 130)
        Me.lblTitle.Size = New System.Drawing.Size(237, 18)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Text = "主題"
        Me.lblTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtTitle
        '
        Me.txtTitle.Location = New System.Drawing.Point(4, 148)
        Me.txtTitle.Size = New System.Drawing.Size(237, 27)
        Me.txtTitle.Name = "txtTitle"
        Me.txtTitle.TabIndex = 1
        '
        'lblCharacter
        '
        Me.lblCharacter.Location = New System.Drawing.Point(4, 178)
        Me.lblCharacter.Size = New System.Drawing.Size(237, 18)
        Me.lblCharacter.Name = "lblCharacter"
        Me.lblCharacter.Text = "人物"
        Me.lblCharacter.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtCharacter
        '
        Me.txtCharacter.Location = New System.Drawing.Point(4, 196)
        Me.txtCharacter.Size = New System.Drawing.Size(211, 27)
        Me.txtCharacter.Name = "txtCharacter"
        Me.txtCharacter.TabIndex = 2
        '
        'cboCharacter
        '
        Me.cboCharacter.Location = New System.Drawing.Point(216, 196)
        Me.cboCharacter.Size = New System.Drawing.Size(25, 27)
        Me.cboCharacter.Name = "cboCharacter"
        Me.cboCharacter.Text = "▼"
        Me.cboCharacter.TabStop = False
        Me.cboCharacter.Font = New System.Drawing.Font("新細明體", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblDateTime
        '
        Me.lblDateTime.Location = New System.Drawing.Point(4, 226)
        Me.lblDateTime.Size = New System.Drawing.Size(237, 18)
        Me.lblDateTime.Name = "lblDateTime"
        Me.lblDateTime.Text = "日期/時間"
        Me.lblDateTime.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtDateTime
        '
        Me.txtDateTime.Location = New System.Drawing.Point(4, 244)
        Me.txtDateTime.Size = New System.Drawing.Size(237, 27)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.TabIndex = 3
        Me.txtDateTime.Mask = "0000/00/00-00:00:00"
        Me.txtDateTime.PromptChar = Global.Microsoft.VisualBasic.ChrW(32)
        Me.txtDateTime.Font = New System.Drawing.Font("細明體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblSpot
        '
        Me.lblSpot.Location = New System.Drawing.Point(4, 274)
        Me.lblSpot.Size = New System.Drawing.Size(237, 18)
        Me.lblSpot.Name = "lblSpot"
        Me.lblSpot.Text = "地點"
        Me.lblSpot.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtSpot
        '
        Me.txtSpot.Location = New System.Drawing.Point(4, 292)
        Me.txtSpot.Size = New System.Drawing.Size(211, 27)
        Me.txtSpot.Name = "txtSpot"
        Me.txtSpot.TabIndex = 4
        '
        'cboSpot
        '
        Me.cboSpot.Location = New System.Drawing.Point(216, 292)
        Me.cboSpot.Size = New System.Drawing.Size(25, 27)
        Me.cboSpot.Name = "cboSpot"
        Me.cboSpot.Text = "▼"
        Me.cboSpot.TabStop = False
        Me.cboSpot.Font = New System.Drawing.Font("新細明體", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblKeyword
        '
        Me.lblKeyword.Location = New System.Drawing.Point(4, 322)
        Me.lblKeyword.Size = New System.Drawing.Size(237, 18)
        Me.lblKeyword.Name = "lblKeyword"
        Me.lblKeyword.Text = "關鍵字"
        Me.lblKeyword.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtKeyword
        '
        Me.txtKeyword.Location = New System.Drawing.Point(4, 340)
        Me.txtKeyword.Size = New System.Drawing.Size(237, 27)
        Me.txtKeyword.Name = "txtKeyword"
        Me.txtKeyword.TabIndex = 5
        '
        'lblRemark
        '
        Me.lblRemark.Location = New System.Drawing.Point(4, 370)
        Me.lblRemark.Size = New System.Drawing.Size(237, 18)
        Me.lblRemark.Name = "lblRemark"
        Me.lblRemark.Text = "備註"
        Me.lblRemark.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtRemark
        '
        Me.txtRemark.Location = New System.Drawing.Point(4, 388)
        Me.txtRemark.Size = New System.Drawing.Size(237, 60)
        Me.txtRemark.Name = "txtRemark"
        Me.txtRemark.TabIndex = 6
        Me.txtRemark.Multiline = True
        Me.txtRemark.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        '
        'lblBarBatch
        '
        Me.lblBarBatch.Location = New System.Drawing.Point(0, 456)
        Me.lblBarBatch.Size = New System.Drawing.Size(245, 22)
        Me.lblBarBatch.Name = "lblBarBatch"
        Me.lblBarBatch.Text = " 所有打勾的照片"
        Me.lblBarBatch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBarBatch.BackColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(93, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.lblBarBatch.ForeColor = System.Drawing.Color.White
        Me.lblBarBatch.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblHelp1
        '
        Me.lblHelp1.Location = New System.Drawing.Point(4, 480)
        Me.lblHelp1.Size = New System.Drawing.Size(237, 34)
        Me.lblHelp1.Name = "lblHelp1"
        Me.lblHelp1.Text = "整批照片修改完成後，請執行存檔動作"
        Me.lblHelp1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'lblHelp2
        '
        Me.lblHelp2.Location = New System.Drawing.Point(4, 514)
        Me.lblHelp2.Size = New System.Drawing.Size(237, 20)
        Me.lblHelp2.Name = "lblHelp2"
        Me.lblHelp2.Text = "請按下 Ctrl 鍵執行複選"
        Me.lblHelp2.ForeColor = System.Drawing.Color.Red
        '
        'lblBatchTitle
        '
        Me.lblBatchTitle.Location = New System.Drawing.Point(4, 536)
        Me.lblBatchTitle.Size = New System.Drawing.Size(237, 18)
        Me.lblBatchTitle.Name = "lblBatchTitle"
        Me.lblBatchTitle.Text = "主題"
        Me.lblBatchTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtBatchTitle
        '
        Me.txtBatchTitle.Location = New System.Drawing.Point(4, 554)
        Me.txtBatchTitle.Size = New System.Drawing.Size(237, 27)
        Me.txtBatchTitle.Name = "txtBatchTitle"
        Me.txtBatchTitle.TabIndex = 7
        '
        'lblBatchCharacter
        '
        Me.lblBatchCharacter.Location = New System.Drawing.Point(4, 584)
        Me.lblBatchCharacter.Size = New System.Drawing.Size(237, 18)
        Me.lblBatchCharacter.Name = "lblBatchCharacter"
        Me.lblBatchCharacter.Text = "人物"
        Me.lblBatchCharacter.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtBatchCharacter
        '
        Me.txtBatchCharacter.Location = New System.Drawing.Point(4, 602)
        Me.txtBatchCharacter.Size = New System.Drawing.Size(211, 27)
        Me.txtBatchCharacter.Name = "txtBatchCharacter"
        Me.txtBatchCharacter.TabIndex = 8
        '
        'cboBatchCharacter
        '
        Me.cboBatchCharacter.Location = New System.Drawing.Point(216, 602)
        Me.cboBatchCharacter.Size = New System.Drawing.Size(25, 27)
        Me.cboBatchCharacter.Name = "cboBatchCharacter"
        Me.cboBatchCharacter.Text = "▼"
        Me.cboBatchCharacter.TabStop = False
        Me.cboBatchCharacter.Font = New System.Drawing.Font("新細明體", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblBatchSpot
        '
        Me.lblBatchSpot.Location = New System.Drawing.Point(4, 632)
        Me.lblBatchSpot.Size = New System.Drawing.Size(237, 18)
        Me.lblBatchSpot.Name = "lblBatchSpot"
        Me.lblBatchSpot.Text = "地點"
        Me.lblBatchSpot.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtBatchSpot
        '
        Me.txtBatchSpot.Location = New System.Drawing.Point(4, 650)
        Me.txtBatchSpot.Size = New System.Drawing.Size(211, 27)
        Me.txtBatchSpot.Name = "txtBatchSpot"
        Me.txtBatchSpot.TabIndex = 9
        '
        'cboBatchSpot
        '
        Me.cboBatchSpot.Location = New System.Drawing.Point(216, 650)
        Me.cboBatchSpot.Size = New System.Drawing.Size(25, 27)
        Me.cboBatchSpot.Name = "cboBatchSpot"
        Me.cboBatchSpot.Text = "▼"
        Me.cboBatchSpot.TabStop = False
        Me.cboBatchSpot.Font = New System.Drawing.Font("新細明體", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'lblBatchKeyword
        '
        Me.lblBatchKeyword.Location = New System.Drawing.Point(4, 680)
        Me.lblBatchKeyword.Size = New System.Drawing.Size(237, 18)
        Me.lblBatchKeyword.Name = "lblBatchKeyword"
        Me.lblBatchKeyword.Text = "關鍵字"
        Me.lblBatchKeyword.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtBatchKeyword
        '
        Me.txtBatchKeyword.Location = New System.Drawing.Point(4, 698)
        Me.txtBatchKeyword.Size = New System.Drawing.Size(237, 27)
        Me.txtBatchKeyword.Name = "txtBatchKeyword"
        Me.txtBatchKeyword.TabIndex = 10
        '
        'lblBatchRemark
        '
        Me.lblBatchRemark.Location = New System.Drawing.Point(4, 728)
        Me.lblBatchRemark.Size = New System.Drawing.Size(237, 18)
        Me.lblBatchRemark.Name = "lblBatchRemark"
        Me.lblBatchRemark.Text = "備註"
        Me.lblBatchRemark.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer))
        '
        'txtBatchRemark
        '
        Me.txtBatchRemark.Location = New System.Drawing.Point(4, 746)
        Me.txtBatchRemark.Size = New System.Drawing.Size(237, 60)
        Me.txtBatchRemark.Name = "txtBatchRemark"
        Me.txtBatchRemark.TabIndex = 11
        Me.txtBatchRemark.Multiline = True
        Me.txtBatchRemark.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        '
        'butBatchSave
        '
        Me.butBatchSave.Location = New System.Drawing.Point(4, 814)
        Me.butBatchSave.Size = New System.Drawing.Size(237, 27)
        Me.butBatchSave.Name = "butBatchSave"
        Me.butBatchSave.TabIndex = 12
        Me.butBatchSave.Text = "我已經修改完成了"
        Me.butBatchSave.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butBatchSave.ForeColor = System.Drawing.Color.Black
        '
        'lblBarUtility
        '
        Me.lblBarUtility.Location = New System.Drawing.Point(0, 850)
        Me.lblBarUtility.Size = New System.Drawing.Size(245, 22)
        Me.lblBarUtility.Name = "lblBarUtility"
        Me.lblBarUtility.Text = " 執行"
        Me.lblBarUtility.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBarUtility.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.lblBarUtility.ForeColor = System.Drawing.Color.White
        Me.lblBarUtility.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        '
        'butCheckedAll
        '
        Me.butCheckedAll.Location = New System.Drawing.Point(4, 878)
        Me.butCheckedAll.Size = New System.Drawing.Size(237, 27)
        Me.butCheckedAll.Name = "butCheckedAll"
        Me.butCheckedAll.TabIndex = 13
        Me.butCheckedAll.Text = "選取全部的照片"
        Me.butCheckedAll.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butCheckedAll.ForeColor = System.Drawing.Color.Black
        '
        'butUnCheckedAll
        '
        Me.butUnCheckedAll.Location = New System.Drawing.Point(4, 910)
        Me.butUnCheckedAll.Size = New System.Drawing.Size(237, 27)
        Me.butUnCheckedAll.Name = "butUnCheckedAll"
        Me.butUnCheckedAll.TabIndex = 14
        Me.butUnCheckedAll.Text = "取消選取的照片"
        Me.butUnCheckedAll.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butUnCheckedAll.ForeColor = System.Drawing.Color.Black
        '
        'butUnload
        '
        Me.butUnload.Location = New System.Drawing.Point(4, 942)
        Me.butUnload.Size = New System.Drawing.Size(237, 27)
        Me.butUnload.Name = "butUnload"
        Me.butUnload.TabIndex = 15
        Me.butUnload.Text = "結束並回到主畫面"
        Me.butUnload.Font = New System.Drawing.Font("華康細圓體", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.butUnload.ForeColor = System.Drawing.Color.Black
        '
        'picIndex
        '
        Me.picIndex.Location = New System.Drawing.Point(4, 976)
        Me.picIndex.Size = New System.Drawing.Size(237, 66)
        Me.picIndex.Name = "picIndex"
        Me.picIndex.BackColor = System.Drawing.Color.Transparent
        Me.picIndex.Controls.Add(Me.chkBuildIndex)
        Me.picIndex.Controls.Add(Me.Label3)
        Me.picIndex.Controls.Add(Me.Label4)
        '
        'chkBuildIndex
        '
        Me.chkBuildIndex.Location = New System.Drawing.Point(0, 4)
        Me.chkBuildIndex.Name = "chkBuildIndex"
        Me.chkBuildIndex.TabIndex = 16
        Me.chkBuildIndex.Checked = True
        Me.chkBuildIndex.TextValue = "回主畫面時重新編列索引檔"
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(20, 24)
        Me.Label3.Size = New System.Drawing.Size(215, 18)
        Me.Label3.Name = "Label3"
        Me.Label3.Text = "此動作需花費一點時間"
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(20, 44)
        Me.Label4.Size = New System.Drawing.Size(215, 18)
        Me.Label4.Name = "Label4"
        Me.Label4.Text = "請耐心等候"
        '
        'frmPhotoInfoBatch_2
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1234, 888)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.Image = CType(resources.GetObject("$this.Image"), System.Drawing.Image)
        Me.MaxButton = False
        Me.MinButton = False
        Me.Text = "照片資訊的修改"
        Me.Font = New System.Drawing.Font("Tahoma", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TitleFont = New System.Drawing.Font("華康細圓體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.MenuFont = New System.Drawing.Font("新細明體", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.Name = "frmPhotoInfoBatch_2"
        Me.Controls.Add(Me.lvwMain)
        Me.Controls.Add(Me.barSearch)
        Me.Controls.Add(Me.Label1)
        Me.picIndex.ResumeLayout(False)
        Me.barSearch.ResumeLayout(False)
        Me.barSearch.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lvwMain As System.Windows.Forms.ListView
    Friend WithEvents ilsIcons As System.Windows.Forms.ImageList
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents mnuKeyWords As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents barSearch As System.Windows.Forms.Panel
    Friend WithEvents lblBarExif As System.Windows.Forms.Label
    Friend WithEvents lblHelp As System.Windows.Forms.Label
    Friend WithEvents lblTip1 As System.Windows.Forms.Label
    Friend WithEvents lblBarOne As System.Windows.Forms.Label
    Friend WithEvents lblHelpOne As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents txtTitle As System.Windows.Forms.TextBox
    Friend WithEvents lblCharacter As System.Windows.Forms.Label
    Friend WithEvents txtCharacter As System.Windows.Forms.TextBox
    Friend WithEvents cboCharacter As System.Windows.Forms.Button
    Friend WithEvents lblDateTime As System.Windows.Forms.Label
    Friend WithEvents txtDateTime As System.Windows.Forms.MaskedTextBox
    Friend WithEvents lblSpot As System.Windows.Forms.Label
    Friend WithEvents txtSpot As System.Windows.Forms.TextBox
    Friend WithEvents cboSpot As System.Windows.Forms.Button
    Friend WithEvents lblKeyword As System.Windows.Forms.Label
    Friend WithEvents txtKeyword As System.Windows.Forms.TextBox
    Friend WithEvents lblRemark As System.Windows.Forms.Label
    Friend WithEvents txtRemark As System.Windows.Forms.TextBox
    Friend WithEvents lblBarBatch As System.Windows.Forms.Label
    Friend WithEvents lblHelp1 As System.Windows.Forms.Label
    Friend WithEvents lblHelp2 As System.Windows.Forms.Label
    Friend WithEvents lblBatchTitle As System.Windows.Forms.Label
    Friend WithEvents txtBatchTitle As System.Windows.Forms.TextBox
    Friend WithEvents lblBatchCharacter As System.Windows.Forms.Label
    Friend WithEvents txtBatchCharacter As System.Windows.Forms.TextBox
    Friend WithEvents cboBatchCharacter As System.Windows.Forms.Button
    Friend WithEvents lblBatchSpot As System.Windows.Forms.Label
    Friend WithEvents txtBatchSpot As System.Windows.Forms.TextBox
    Friend WithEvents cboBatchSpot As System.Windows.Forms.Button
    Friend WithEvents lblBatchKeyword As System.Windows.Forms.Label
    Friend WithEvents txtBatchKeyword As System.Windows.Forms.TextBox
    Friend WithEvents lblBatchRemark As System.Windows.Forms.Label
    Friend WithEvents txtBatchRemark As System.Windows.Forms.TextBox
    Friend WithEvents butBatchSave As Aqua.ThinButton
    Friend WithEvents lblBarUtility As System.Windows.Forms.Label
    Friend WithEvents butCheckedAll As Aqua.ThinButton
    Friend WithEvents butUnCheckedAll As Aqua.ThinButton
    Friend WithEvents butUnload As Aqua.ThinButton
    Friend WithEvents picIndex As System.Windows.Forms.Panel
    Friend WithEvents chkBuildIndex As Aqua.CheckBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
End Class
