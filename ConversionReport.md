# 表單轉換報告

由 `Tools\FrmConverter` 從 `D:\專案\電子相簿\iPhoto\iPhoto.vbp` 產生（2026-09-25 22:41）。每個表單的 TODO 也寫在它的 `.Vb6.vb` 開頭。

| 表單 | 基底 | 控制項 | 佔位 | 待辦 | 位置 |
|---|---|---:|---:|---:|---|
| `frmMain_1280x1024` | Aqua.iForm | 85 | 0 | 4 | iPhoto.Net\Forms |
| `frmResAlbum` | System.Windows.Forms.Form | 7 | 0 | 0 | PhotoLib.Net\Forms |
| `frmSetup` | Aqua.AquaForm | 84 | 0 | 4 | iPhoto.Net\Forms |
| `frmExport` | Aqua.AquaForm | 13 | 0 | 0 | iPhoto.Net\Forms |
| `frmMsgBox` | Aqua.iForm | 8 | 0 | 0 | PhotoLib.Net\Forms |
| `frmQueryMsgBox` | Aqua.iForm | 5 | 0 | 0 | PhotoLib.Net\Forms |
| `frmBrowserFolder` | Aqua.AquaForm | 4 | 0 | 0 | PhotoLib.Net\Forms |
| `frmBrowserFile` | Aqua.AquaForm | 8 | 0 | 0 | PhotoLib.Net\Forms |
| `frmSlideShow` | System.Windows.Forms.Form | 2 | 0 | 0 | PhotoLib.Net\Forms |
| `frmImport` | Aqua.AquaForm | 33 | 0 | 0 | iPhoto.Net\Forms |
| `frmPaint` | System.Windows.Forms.Form | 2 | 0 | 0 | iPhoto.Net\Forms |
| `frmSearch` | Aqua.AquaForm | 45 | 0 | 9 | iPhoto.Net\Forms |
| `frmSearchResult` | Aqua.AquaForm | 6 | 0 | 0 | iPhoto.Net\Forms |
| `frmPhotoInfo` | Aqua.AquaForm | 29 | 0 | 9 | iPhoto.Net\Forms |
| `frmSubject` | Aqua.AquaForm | 2 | 0 | 0 | PhotoLib.Net\Forms |
| `frmViewerSmall` | System.Windows.Forms.Form | 31 | 0 | 1 | PhotoLib.Net\Forms |
| `frmShowPhoto` | Aqua.AquaForm | 23 | 0 | 0 | iPhoto.Net\Forms |
| `frmBuildBook` | Aqua.AquaForm | 7 | 0 | 0 | iPhoto.Net\Forms |
| `frmAddition` | Aqua.AquaForm | 10 | 0 | 0 | iPhoto.Net\Forms |
| `frmDock` | Aqua.AquaForm | 5 | 0 | 0 | iPhoto.Net\Forms |
| `frmPhotoIndex` | Aqua.AquaForm | 23 | 0 | 1 | iPhoto.Net\Forms |
| `frmPrint` | Aqua.AquaForm | 37 | 0 | 1 | iPhoto.Net\Forms |
| `frmPaperSetup` | Aqua.AquaForm | 18 | 0 | 0 | iPhoto.Net\Forms |
| `frmPrintPages` | Aqua.AquaForm | 10 | 0 | 0 | iPhoto.Net\Forms |
| `frmKeyWords` | Aqua.AquaForm | 6 | 0 | 0 | iPhoto.Net\Forms |
| `frmSaveChangedPhoto` | Aqua.iForm | 11 | 0 | 0 | PhotoLib.Net\Forms |
| `frmDefaultPhotoInfo` | Aqua.AquaForm | 17 | 0 | 5 | iPhoto.Net\Forms |
| `frmDropFileList` | Aqua.AquaForm | 5 | 0 | 0 | iPhoto.Net\Forms |
| `frmDateTime` | Aqua.AquaForm | 32 | 0 | 0 | iPhoto.Net\Forms |
| `frmLogo` | System.Windows.Forms.Form | 2 | 0 | 2 | PhotoLib.Net\Common\Forms |
| `frmPhotoInfoBatch_1` | Aqua.AquaForm | 6 | 0 | 0 | iPhoto.Net\Forms |
| `frmPhotoInfoBatch_2` | Aqua.AquaForm | 39 | 15 | 17 | iPhoto.Net\Forms |
| `frmToolTipText` | System.Windows.Forms.Form | 1 | 0 | 0 | iPhoto.Net\Forms |
| `frmLoading` | Aqua.iForm | 2 | 0 | 0 | PhotoLib.Net\Common\Forms |
| `frmViewerLarge` | System.Windows.Forms.Form | 47 | 0 | 2 | PhotoLib.Net\Forms |
| `frmInputString` | Aqua.iForm | 3 | 0 | 0 | PhotoLib.Net\Forms |
| `frmCover` | Aqua.iForm | 14 | 0 | 1 | iPhoto.Net\Forms |

共 37 個表單，56 項待辦。

## 待辦明細

### frmMain_1280x1024

- Aqua.Month Month1: SelIndex = -1 -- Month has no SelIndex
- Aqua.Calendar Calendar1: WeekFont = New System.Drawing.Font("新細明體", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte)) -- Calendar has no WeekFont
- Aqua.Calendar Calendar1: DayFont = New System.Drawing.Font("Times New Roman", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte)) -- Calendar has no DayFont
- Aqua.iTextBox iTextBox1: AutoComplete = 1 -- ITextBox has no AutoComplete

### frmSetup

- Aqua.PageSheet PageSheet1(3): became TabPage 3 of PageHead1 -- check the page layout (VB6 drew tab strip and sheet separately)
- Aqua.PageSheet PageSheet1(1): became TabPage 1 of PageHead1 -- check the page layout (VB6 drew tab strip and sheet separately)
- Aqua.PageSheet PageSheet1(0): became TabPage 0 of PageHead1 -- check the page layout (VB6 drew tab strip and sheet separately)
- Aqua.PageSheet PageSheet1(2): became TabPage 2 of PageHead1 -- check the page layout (VB6 drew tab strip and sheet separately)

### frmSearch

- Aqua.TextBox txtRemark: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtKeyWord: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.MaskEdit meStartDate: MaxLength = 10 -- MaskEdit has no MaxLength
- Aqua.MaskEdit meStartDate: Format = "yyyy/mm/dd" -- MaskEdit has no Format
- Aqua.MaskEdit meEndDate: MaxLength = 10 -- MaskEdit has no MaxLength
- Aqua.MaskEdit meEndDate: Format = "yyyy/mm/dd" -- MaskEdit has no Format
- Aqua.TextBox txtTitle: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtCharacter: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtSpot: AutoComplete = 1 -- TextBox has no AutoComplete

### frmPhotoInfo

- Aqua.TextBox txtCharacter: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtTitle: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtKeyWord: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtRemark: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtSpot: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.MaskEdit meDate: MaxLength = 10 -- MaskEdit has no MaxLength
- Aqua.MaskEdit meDate: Format = "0000/00/00" -- MaskEdit has no Format
- Aqua.MaskEdit meTime: MaxLength = 8 -- MaskEdit has no MaxLength
- Aqua.MaskEdit meTime: Format = "00:00:00" -- MaskEdit has no Format

### frmViewerSmall

- MediaPlayerCtl.MediaPlayer mpViewerVideo: Windows Media Player -> Aqua.MediaViewerControl (set FileName in code)

### frmPhotoIndex

- MSComDlg.CommonDialog CommonDialog1: MSComDlg.CommonDialog has no control -- use OpenFileDialog / SaveFileDialog / PrintDialog / ColorDialog in code

### frmPrint

- MSComDlg.CommonDialog CommonDialog1: MSComDlg.CommonDialog has no control -- use OpenFileDialog / SaveFileDialog / PrintDialog / ColorDialog in code

### frmDefaultPhotoInfo

- Aqua.TextBox txtSpot: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtRemark: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtKeyWord: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtTitle: AutoComplete = 1 -- TextBox has no AutoComplete
- Aqua.TextBox txtCharacter: AutoComplete = 1 -- TextBox has no AutoComplete

### frmLogo

- VB.Shape Shape1: Shape=3 (oval/rounded) drawn as a rectangle Label -- paint it if the shape matters
- VB.Shape Shape1: BorderColor=&H00E0E0E0& -- a Label border is always the system colour

### frmPhotoInfoBatch_2

- VB.Shape Shape1: BorderColor=&H00C0C0C0& -- a Label border is always the system colour
- vbalListViewLib6.vbalListViewCtl lvwMain: no .NET equivalent -- placeholder Panel (lvwMain); VB6 properties: CausesValidation=0, View=4, MultiSelect=-1, LabelEdit=0, AutoArrange=0, CustomDraw=0, HeaderButtons=0, HeaderTrackSelect=0, HideSelection=0, InfoTips=0, ItemBorderSelect=-1, TileBackgroundPicture=0
- vbalIml6.vbalImageList ilsIcons: no .NET equivalent -- placeholder Panel (ilsIcons); VB6 properties: IconSizeX=120, IconSizeY=96, ColourDepth=24
- vbalExplorerBarLib6.vbalExplorerBarCtl barSearch: no .NET equivalent -- placeholder Panel (barSearch); VB6 properties: BackColorEnd=0, BackColorStart=16777215
- TechnoComCtls1.SkinButton butUnCheckedAll: no .NET equivalent -- placeholder Panel (butUnCheckedAll); VB6 properties: SPN="MyButtonDefSkin", Text="取消選取的照片", Picture="frmPhotoInfoBatch_2.frx":3BC4
- TechnoComCtls1.SkinButton butCheckedAll: no .NET equivalent -- placeholder Panel (butCheckedAll); VB6 properties: SPN="MyButtonDefSkin", Text="選取全部的照片", Picture="frmPhotoInfoBatch_2.frx":344A
- TechnoComCtls1.SkinButton butUnload: no .NET equivalent -- placeholder Panel (butUnload); VB6 properties: SPN="MyButtonDefSkin", Text="結束並回到主畫面", Picture="frmPhotoInfoBatch_2.frx":2CD0
- vbAcceleratorMultiSelected.vbalMultiSel vbalMultiSel2: no .NET equivalent -- placeholder Panel (vbalMultiSel2); VB6 properties: CausesValidation=0, Visible=0, ClickToChecked=0
- MSMask.MaskEdBox txtDateTime: Format = "yyyy/mm/dd" -- MaskedTextBox has no Format
- vbAcceleratorMultiSelected.vbalMultiSel vbalMultiSel1: no .NET equivalent -- placeholder Panel (vbalMultiSel1); VB6 properties: CausesValidation=0, Visible=0, ClickToChecked=0
- TechnoComCtls1.SkinButton butBatchSave: no .NET equivalent -- placeholder Panel (butBatchSave); VB6 properties: SPN="MyButtonDefSkin", Text="我已經修改完成了", Picture="frmPhotoInfoBatch_2.frx":2556
- vbAcceleratorMultiSelected.vbalMultiSel cboBatchCharacter: no .NET equivalent -- placeholder Panel (cboBatchCharacter); VB6 properties: ClickToChecked=0
- vbAcceleratorMultiSelected.vbalMultiSel cboBatchSpot: no .NET equivalent -- placeholder Panel (cboBatchSpot); VB6 properties: ClickToChecked=0
- vbAcceleratorMultiSelected.vbalMultiSel cboSpot: no .NET equivalent -- placeholder Panel (cboSpot); VB6 properties: ClickToChecked=0
- vbAcceleratorMultiSelected.vbalMultiSel cboCharacter: no .NET equivalent -- placeholder Panel (cboCharacter); VB6 properties: ClickToChecked=0
- vbAcceleratorMultiSelected.vbalMultiSel vbalMultiSel3: no .NET equivalent -- placeholder Panel (vbalMultiSel3); VB6 properties: CausesValidation=0, Visible=0, ClickToChecked=0
- vbAcceleratorMultiSelected.vbalMultiSel vbalMultiSel4: no .NET equivalent -- placeholder Panel (vbalMultiSel4); VB6 properties: CausesValidation=0, Visible=0, ClickToChecked=0

### frmViewerLarge

- MediaPlayerCtl.MediaPlayer mpViewerVideo: Windows Media Player -> Aqua.MediaViewerControl (set FileName in code)
- VB.Shape shpZero: Shape=3 (oval/rounded) drawn as a rectangle Label -- paint it if the shape matters

### frmCover

- VB.Shape Shape1: BorderColor=&H00E0E0E0& -- a Label border is always the system colour

