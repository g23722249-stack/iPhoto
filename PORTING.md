# iPhoto 移植到 .NET 8

把 VB6 的 `D:\專案\電子相簿\iPhoto\iPhoto.vbp` 移植成 .NET 8（VB.NET、WinForms），控制項改用 `C:\專案\RunTime\Aqua.Net`。
目前進度：
- 專案結構、參考、資源、進入點都已就緒，可以建置。
- **37 個表單的畫面（控制項、位置、屬性、圖片）已由轉換工具自動產生**，可以用 VS 設計工具開啟，見「表單轉換」一節。
- 表單的程式碼（事件處理）、類別、模組還沒移植，每個檔案開頭都有 `TODO(port)` 並註明對應的 VB6 原始檔。

## 方案結構

```
D:\專案\電子相簿\Net\
├─ 電子相簿.Net.sln
├─ iPhoto.Net\       iPhoto 主程式（WinExe、net8.0-windows、x86）← 電子相簿\iPhoto\{Form,Module}
│    Program.vb        Sub Main（原 LibMain.bas）
│    Forms\  Modules\  Resources\Forms\<表單>\   iPhoto.Ini
├─ PhotoLib.Net\     iPhoto / iExport / iPrint 共用 ← 電子相簿\Lib\{Class,Module,Form}
│    Classes\ Modules\ Forms\  Resources\Forms\<表單>\
│    Common\         ← D:\專案\Lib（所有 TechnoSoft 程式共用：frmLogo、frmLoading、LibString…）
│    Modules\Globals.vb    g_lpConfig、g_lpFileSystem… 全域物件（原宣告在 iPhoto 的 LibMain.bas）
├─ Techno.Net\       TechnoSoft 執行階段 DLL 的 .NET 版（Carbon / CoreMedia / Darwin / Quartz）
└─ （外部）C:\專案\RunTime\Aqua.Net
```

## 建置與執行

- **Visual Studio 2022**：開啟 `電子相簿.Net.sln`，把 iPhoto.Net 設為啟動專案。
- **命令列**：建置主程式專案即可，它會連帶建置其他專案。
  ```
  dotnet build D:\專案\電子相簿\Net\iPhoto.Net\iPhoto.Net.vbproj
  ```
  不要用 `dotnet build` 建置整個 .sln：Aqua.Net 同時產出 net35，那部分需要 Visual Studio 的 MSBuild（`dotnet` 無法執行 ResGen）。
- 執行檔：`iPhoto.Net\bin\Debug\net8.0-windows\iPhoto.exe`。目前會顯示啟動畫面，然後在主螢幕開一個空白的主視窗。

## 已定案的決策

| 項目 | 決定 | 原因 |
|---|---|---|
| 平台 | iPhoto.Net 建置為 **x86** | iPhoto.mdb 用 `Microsoft.Jet.OLEDB.4.0`，Jet 只有 32 位元版；這樣不必另外安裝 ACE 驅動程式。資料庫存取用 `System.Data.OleDb` |
| 主視窗 | 一律用 `frmMain_1280x1024`，**不移植 frmMain_1440X900_NEW** | 需求決定。VB6 在寬螢幕時會改選 1440 版，這段已拿掉 |
| DPI | `HighDpiMode.DpiUnawareGdiScaled` | 跟 VB6 一樣不處理 DPI；Aqua 的外觀圖是像素圖 |
| Option Strict | iPhoto.Net、PhotoLib.Net 為 **Off**，Techno.Net 為 On | 移植過來的程式大量使用晚期繫結（As Object、Variant）；整理好的檔案再逐一改成 `Option Strict On` |
| 全域物件 | 搬到 PhotoLib.Net 的 `Globals.vb` | 共用的 Lib 程式碼要用到它們，而類別庫無法反過來參考主程式 |
| TechnoSoft 名稱 | 維持 `Carbon.FileSystem`、`Darwin.Wait(...)`、`CoreMedia.Monitors`、`Quartz.*` | Techno.Net 的 RootNamespace 留空，模組成員會提升到命名空間，所以移植後的程式碼照原樣呼叫即可 |
| `fs*` 常數 | 專案層級 `Imports Carbon.AnalyseFileConstants` | VB6 直接寫 `fsBaseName`，不加前綴 |
| Aqua 命名空間 | **不要**在專案層級 Imports `Aqua`，程式碼一律寫 `Aqua.TextBox` | Aqua 有 Label、Panel、TextBox 等同名類別，一起 Imports 會跟 WinForms 的類別模稜兩可 |

## 預設表單執行個體（`frmMsgBox.Show` 這種寫法）

VB 的預設執行個體只在**定義該表單的組件內**有效。iPhoto 自己的表單（`frmSetup.Show()`）可以直接用；搬到 PhotoLib 的 13 個表單則透過橋接：

- `PhotoLib.Net\Common\DefaultInstances.vb`：公開 PhotoLib 自己的 `My.Forms` 執行個體（`Of_frmMsgBox`…）。
- `iPhoto.Net\Modules\LibForms.vb`：用同名屬性 `frmMsgBox` 等轉送過去。

所以 iPhoto 的程式碼可以照 VB6 寫 `frmMsgBox.ShowCriticalMessage(...)`，而且跟 PhotoLib 內部拿到的是**同一個**執行個體；`New frmMsgBox`、`As frmMsgBox` 仍然指 PhotoLib 的型別。已用建置測試確認。PhotoLib 新增表單時，兩個檔案都要加一行。

## 圖片資源（原本存在 .frx 的圖片）

- 用 `C:\專案\RunTime\Tools\Export-FrxImages.ps1 -Vbp ...\iPhoto.vbp -TransparentColor FF00FF` 抽出來，已經依表單放進各專案的 `Resources\Forms\<表單 VB_Name>\` 並嵌入組件（iPhoto 330 張、PhotoLib 99 張）。有洋紅色背景的圖用已轉透明的 PNG 版本。
- 檔名規則是 `控制項[_Index]_屬性`。在程式碼裡讀取：
  ```vb
  ' VB6:  imgButton(1).MaskImage = "frmMain.frx":235E76
  imgButton1.MaskImage = FormImages.Load(Me, "imgButton_1_MaskImage")
  Me.Icon = FormImages.LoadIcon(Me, "frmMain_1280x1024_Icon")
  ```
  `Load` 會依序找 png、bmp、ico、jpg、gif；找不到會直接丟例外，名稱打錯馬上就會發現。
- 圖片和位址的完整對照在 `D:\專案\電子相簿\iPhoto\frx_export\manifest.csv`。

## 表單轉換（.frm → .Designer.vb）

表單畫面由 `C:\專案\RunTime\Tools\FrmConverter` 從 VB6 的 .frm 自動產生。每個表單會有：

| 檔案 | 內容 | 能不能手改 |
|---|---|---|
| `<表單>.vb` | 手寫的程式碼（事件處理、移植後的邏輯） | ✅ 轉換工具**不會動**這個檔 |
| `<表單>.Designer.vb` | 控制項、位置、屬性，可用 VS 設計工具開啟 | 可以，但重新轉換會被覆蓋 |
| `<表單>.resx` | 設計工具用到的圖片（直接從 .frx 讀出） | 同上 |
| `<表單>.Vb6.vb` | `Sub New`、設計工具無法表達的內容（Buttons/ToolBar 項目、Grid 欄位標題、AquaMenu 選單、ImageList 圖片、ListView 欄位）、**VB6 控制項陣列**、TODO 清單 | ❌ 每次轉換都重新產生 |

重新轉換（例如修了轉換規則之後）：
```
dotnet build C:\專案\RunTime\Tools\FrmConverter\FrmConverter.vbproj
dotnet C:\專案\RunTime\Tools\FrmConverter\bin\Debug\net8.0-windows\FrmConverter.dll D:\專案\電子相簿\iPhoto\iPhoto.vbp D:\專案\電子相簿\Net --skip frmMain_1440X900_NEW
```
加上 `--only frmSetup` 可以只轉換單一表單。工具依 `<表單>.vb` 所在位置決定輸出資料夾，並把總表寫到 `ConversionReport.md`。

**轉換規則重點：**
- **座標**：.frm 裡的位置一律是 twips（÷15 換成像素），跟容器的 ScaleMode 無關（實測確認）。
- **視窗大小**：有 AquaForm/iForm 的表單，視窗大小以該區塊的 Width/Height 為準；VB6 的 AquaForm 會把宿主視窗調成自己的大小。
- **Z 順序**：跟 VB6 相同。
- **控制項陣列**：`txtSound(0..4)` 會產生 `txtSound_0`…`txtSound_4` 五個控制項，再加一個唯讀陣列屬性 `txtSound`，所以 `txtSound(i).Text` 這種寫法照樣能用。事件處理要改成 `Handles txtSound_0.Click, txtSound_1.Click`，再用 `Array.IndexOf(txtSound, sender)` 取得 Index。
- **名稱衝突**：控制項名稱跟表單成員或 VB 關鍵字衝突時會加底線，並列在 TODO。
- **PageHead + PageSheet**：合併成一個 `Aqua.TabControl`，各頁的標題從 PageHead 的 TextN 帶過去。
- **VB.Line**：轉成 1 像素高（或寬）的 Label。**VB.Shape**：轉成有框線的 Label。
- **MediaList / MediaItem**：自動設 `ShowCheckBox = False`；VB6 沒設 DragItem/DropItem 的清單會明確設成 False。
- **無法轉換的控制項**：vbAccelerator 系列、SkinButton 會放一個佔位用的 Panel（目前只有 frmPhotoInfoBatch_2 有 15 個）。

**驗證**：37 個表單都能建置、建立，也能畫出畫面，並逐一檢查過主要表單（frmMain、frmSetup、frmPrint、frmPhotoInfo…）的截圖。注意 `DrawToBitmap` 截圖不會套用視窗區域（Region），所以 ImageButton / PicturePanel 用洋紅色遮罩挖空的部分，在截圖裡會看到洋紅色；實際執行時不會。

轉換工具無法處理的 56 項列在 `ConversionReport.md`，大多是要在程式碼裡處理的項目：AutoComplete 要提供候選清單、MaskEdit 的 Format、CommonDialog、橢圓形的 Shape，以及 frmPhotoInfoBatch_2 的 vbAccelerator 控制項。

## 控制項對照（VB6 → .NET）

### Aqua（Aqua.ocx → Aqua.Net）

| VB6 | .NET | 注意事項 |
|---|---|---|
| Aqua.AquaForm / iForm | `Aqua.AquaForm` / `Aqua.iForm`（**本身就是 Form**） | VB6 是放在 VB.Form 上的控制項；.NET 版直接繼承，表單殼已經這樣產生。`Title` → `Text`、`Picture` → `Image` |
| Aqua.Button | `Aqua.FlashButton` | Aqua.Net 的 `Aqua.Button` 是另一個控制項（來自 Unimax） |
| Aqua.TextBox / MultiLineTextBox | `Aqua.TextBox`（`Multiline = True`） | `Validation(e As CancelEventArgs)`、`EnterFocus`/`ExitFocus`；OLE 拖放改成 `AllowDrop` + `DragDrop` |
| Aqua.Icon | `Aqua.IconBox` | `Picture` → `Image` |
| Aqua.ListBox | `Aqua.ItemListBox` | |
| Aqua.PictureBox | `Aqua.PicturePanel` | 容器；`SizeMode = Fill` 是九宮格並排，跟 VB6 相同 |
| Aqua.DeskTop | `Aqua.DirListBox`（`ShowSpecialFolders`） | 設計時用的圖示屬性不需要 |
| Aqua.PageHead + PageSheet | `Aqua.TabControl` + `TabPage` | 版面要重排 |
| Aqua.AquaMenu | `Aqua.AquaMenu`（Component） | 選單在 Form_Load 用 `AddItem(name, text, level, icon, selIcon)` 一行對一個 `MenuName_i`；`MenuSelected(sender, item)` |
| Aqua.ToolBar | `Aqua.ToolBar` | `Click(sender, index)`；`Text(i)`/`Icon(i)` → `GetText(i)`/`GetIcon(i)` |
| Aqua.MediaList / MediaItem | 同名，已加 VB6 相容層 | 見下方 |
| Aqua.CheckBox / RadioButton | 同名 | `Value` → `Checked`、`ValueChanged` → `CheckedChanged`、`Group` → `GroupName`；`Aqua.CheckBoxConstants` 改成 True/False |
| Aqua.Slider / UpDown / ProgressBar | 同名 | `Max`/`Min` → `Maximum`/`Minimum` |
| Aqua.ImageButton | 同名 | 五種狀態圖＋`MaskImage`＋`TransparencyKey`＋`BorderStyle` 都有；圖片用 `FormImages.Load` |
| Aqua.Calendar / Month | 同名 | `SetYearMonth`、`YearChanged`/`MonthChanged`/`DayChanged`、`SetDayColor`、`Month.SetMonthColor` |
| Aqua.FileListBox | 同名 | `File`、`Selected(i)`、`SelectedCount`；`Refresh()` 會重新讀取資料夾 |
| Aqua.Grid / Buttons / Label / Panel / TimeLine / Loading / MaskEdit / DropDownList / iTextBox | 同名 | Grid 的 `Text0`/`Width0` 等 → `SetHeaderProperty`；Buttons 的 `Text0` → `AdditionButton`；MaskEdit 不支援 `Format` |

**MediaList / MediaItem 的 VB6 相容層：**
- `AddItem(檔名, [ToolTip])`、`RemoveItem(index)`、`Limit`、`AspectRatio`、`ScrollMin/Max/Value`、`BorderSize`、`Mark*`、`Ranking`、`PopupMenu(index, AquaMenu)`。
- 事件：`BeforeSelectedChanged → ItemSelected → SelectedChanged → AfterSelectedChanged`，順序跟 VB6 相同；另有 `ItemMarkChanged`、`ItemMouseDown`、`ItemKeyDown`、`ItemCompleteDrag`、`ItemDblClick`。
- `Aqua.MediaItemRanking.無評價…五顆星` → `NoRating`、`OneStar`…`FiveStars`。
- **`DragItem`、`DropItem` 預設是 True**（VB6 預設 False），VB6 沒設這兩個屬性的清單要明確設成 False。

### 其他 VB6 元件

| VB6 | .NET |
|---|---|
| VB.Label / TextBox / ListBox / Timer / Menu | WinForms 同名控制項（`Caption` → `Text`） |
| VB.PictureBox / VB.Image | `System.Windows.Forms.PictureBox`（要當容器時用 Panel） |
| VB.Line / VB.Shape | 沒有對應控制項，改在 Paint 事件裡自己畫 |
| MSComctlLib TreeView / ListView / ImageList | WinForms 同名控制項 |
| MSComDlg.CommonDialog | `OpenFileDialog` / `SaveFileDialog` / `ColorDialog` |
| MSMask.MaskEdBox | `MaskedTextBox`（或 `Aqua.MaskEdit`） |
| MediaPlayerCtl.MediaPlayer（msdxm.ocx） | `Aqua.MediaViewerControl`（LibVLC） |
| vbAccelerator cFlatControl（`SetStyleOffice10`） | 不需要，WinForms 有 visual styles |
| vbalMultiSel / vbalListView / vbalExplorerBar / vbalImageList / TechnoComCtls1.SkinButton | 只有 frmPhotoInfoBatch_2 用到：ExplorerBar → 可捲動的 Panel（各區標題是 Label），ListView → WinForms ListView，多選下拉 → 「▼」按鈕＋打勾的 ContextMenuStrip |

### TechnoSoft 執行階段（Techno.Net）

| VB6 | 狀態 |
|---|---|
| `Carbon.FileSystem` | ✅ 完成，照 `D:\專案\RunTime\Carbon\CoClass\FileSystem.cls` 的行為移植 |
| `Carbon.IniFile` | ✅ `FileName`、`SimpleGetValue`、`SimpleSetValue`（另有 `EnumerateKeys/Sections`） |
| `CoreMedia.Monitors` / `Monitor` | ✅ 以 `Screen.AllScreens` 實作，單位都是像素 |
| `Darwin.Wait` / `ShowWindow` | ✅ |
| `Carbon.Printer`（Canvas、PaintPicture、PrintText、TextWidth/Height、SetScale） | ✅ 以毫米畫在任何 Graphics 上（印表機、預覽點陣圖、JPEG），`EndDoc` 改由 `PrintDocument` 分頁 |
| `CoreMedia.Audio`（幻燈片背景音樂） | ✅ 用 MCI（mciSendString） |
| `Quartz.*`（SavePicture、GDI、Thumbnail、ImageInfo、Image、FixSize、Exif、Region，共 47 處） | 部分完成：`Quartz.ImageFilter`（亮度/對比、黑白、泛黃、柔焦、清晰、紅眼、旋轉、裁切，演算法照 cImageProcessDIB / LibHLSRGB / Color.cls 移植）、`Quartz.FixSize`、`Quartz.Thumbnail`（相片用 GDI+、影片用 Shell 縮圖）、`Quartz.ImageInfo`、`LoadPicture`/`SavePicture` 已完成；GDI/Region 由 .NET 繪圖取代 |

## 移植進度

| 部分 | 狀態 |
|---|---|
| 核心類別：Config、Storage、Albums、Class、Book、Photo、Dock、Import、Database | ✅ 已移植；以實際相片庫（唯讀）和資料庫複本測試，23 項全部通過 |
| `LibAlbum`、`LibMain`、`LibDataBase`、`Program`（Sub Main） | ✅ |
| `LibUserInterface` | ✅（`AddSubjectToListView` 已移到 PhotoLib 的 frmSubject；MediaListPlus 版不移植） |
| `frmMain_1280x1024` 的程式碼 | ✅ 已實測：相簿樹、開啟相簿、縮圖與評價、日曆查詢、關鍵字搜尋 |
| `frmMsgBox`、`frmQueryMsgBox`、`frmSaveChangedPhoto`、`frmDefaultPhotoInfo` | ✅ |
| `frmImport`（輸入相片） | ✅ 已在測試相片庫實測：建立相簿資料夾、Note.Ini、olyalbum.inf、每張相片的 .Exif、複製檔案、寫入資料庫，回主畫面自動開啟新相簿 |
| `frmSetup`（基本設置）、`frmBrowserFolder`、`frmBrowserFile` | ✅ 已實測：四個分頁、選擇資料夾（不存在時詢問是否建立）、儲存到 iPhoto.Ini |
| `frmSearch`、`frmSearchResult`、`frmShowPhoto`、`frmDock` | ✅ 已實測：搜尋條件檢查、依欄位搜尋資料庫（12,991 筆中找到 41 筆）、評價篩選、結果表格、單張相片資訊、加入 Dock、重新搜尋；Dock 縮圖、另存 .dck、清空 |
| `frmViewerLarge`、`frmViewerSmall`（全圖瀏覽）＋ `IPhotoViewer` | ✅ 已實測：顯示相片、八個編輯工具、亮度/對比、復原、影片播放/暫停/長度/位置、上一張/下一張、從主畫面「全圖瀏覽」開啟與 Esc 關閉。單螢幕時隱藏、由「全圖瀏覽」以對話框開啟；雙螢幕時顯示在另一個螢幕並跟著選取的相片 |
| `frmKeyWords`、`frmAddition`、`frmBuildBook`、`frmDateTime`、`frmPhotoInfo` | ✅ 已在測試相片庫實測：關鍵字（分類清單在設計工具）、新增相片庫資料夾、建立攝影集 .Alm、修改檔案時間與 Exif 日期、單張相片資訊 |
| `frmPhotoInfoBatch_1`、`frmPhotoInfoBatch_2`（批次修改資訊） | ✅ 已實測：勾選相簿（勾根節點會勾整組）、縮圖清單、單張欄位離開即存、人物/地點打勾選單、整批存檔 |
| `frmDropFileList`、`frmExport` | ✅ 已實測：燒錄清單（依日期排序、可拖出檔案）、匯出（依時間排序、縮小到指定尺寸、改名 Img1…） |
| `frmPrint`、`frmPaperSetup`、`frmPrintPages`、`frmPhotoIndex` | ✅ 已實測：列印預覽（自動/手動拖放）、版面設定、指定頁次、存成 JPEG；索引圖預覽與存檔。實際送印表機的部分沒有測（不想真的印出來），程式走的是同一個繪圖路徑 |
| `frmCover`、`frmSubject`、`frmSlideShow`、`frmResAlbum`、`frmInputString`、`frmLoading`、`frmLogo` | ✅ 已實測 frmCover（框選面孔、名片存檔）、frmSubject、frmSlideShow（轉場、Esc 中斷） |
| `frmPaint`、`frmToolTipText` | ✅ VB6 本來就沒有程式碼，也沒有地方開啟 |
| `LibApi`、`LibString`、`KeyWord`、`MatrixPrint` | ✅；`LibDraw`、`LibToolTipText`、`cTooltip` 已被 .NET 繪圖 / WinForms ToolTip 取代，保留空檔與說明 |
| 新增 `LibPrint`（iPhoto） | frmPrint 與 frmPhotoIndex 共用：印表機清單、紙張可列印範圍（mm）、紙張陰影、頁次選擇、一次送出多頁、存成 JPEG |

所有表單都已移植，`PortStatus.NotPorted` 已不再被呼叫。測試：scratchpad 的 UiTest5 在相片庫複本上逐一操作這些表單（視窗放在螢幕外），全部通過。

### 畫面設計都在設計工具裡

- 版面、位置、大小、z-order、標題文字、預設值、圖片、ToolTip 都寫在 `.Designer.vb` / `.resx`，可以直接在 VS 設計工具調整。程式碼只填「會隨資料變動」的內容（今天日期、設定值、相片清單……）。
- VB6 在 Form_Load 裡用程式擺放的東西已經搬進設計工具，例如：
  - frmMain：butMode 的寬度、pnlImport（疊在工具列上、預設隱藏）、pnlDateMode 的位置、年份箭頭和年份標籤的位置、箭頭的 ToolTip、frmClass / tvList 的高度、備註欄的 Embed、picFocus.Enabled。
  - frmImport：預設主題圖示 icoPeople、預設標題「新相簿」。
  - frmViewerLarge / Small：VB6 SetToolBarPosition 依螢幕大小擺放的各個工具列，改成設計工具裡的固定位置加上 Anchor（設計尺寸分別是 1280×1024、1024×768），最大化時會自動跟著螢幕大小調整。
  - 工具列按鈕（左轉90° … 清晰）和 butMode 的「整理 / 攝影集」改成設計工具的 Items。為此 Aqua.Net 的 ToolBarItem / ButtonItem 加上了 TypeConverter，設計工具才能把它們寫進 InitializeComponent。
  - 清單類的內容也都在設計工具裡編輯（Aqua.Net 為此加上 TypeConverter，設計工具才寫得進 InitializeComponent）：
    - frmMain 的右鍵選單：`AquaMenu.Items`，平面清單，用 Level 決定層級，跟 VB6 的 MenuName_i／MenuLevel_i 一樣。
    - frmSearch 的「評價／媒體」下拉清單：`DropDownList.Items`，Name 就是 VB6 的 Value。原本只用來放圖示的 12 個隱藏 PictureBox 已移除。
    - frmSearchResult 的表格欄位：`Grid.Columns`。
  - frmDock 的位置 (10, 10)，以及縮圖一排一張的排列方式。
  - frmKeyWords 的 12 個關鍵字分類、frmExport 的 5 種尺寸與預設名稱「Image*」、frmPrint 的「預設／樣式」下拉清單：`DropDownList.Items`。
  - frmDropFileList 的兩個欄位（日期、檔名）：ListView.Columns（轉換工具原本漏掉了）。
  - frmPrint / frmPhotoIndex 用不到的 picPicture_0、picCanvas_0、picSave 在設計工具裡設為隱藏；預覽直接畫在 picPaper 上。
  - frmAddition 預設選「新增相片庫」、frmPrint 預設「自動」、頁碼 1；frmSlideShow 的 KeyPreview / 黑底、frmLogo 的透明色與 Timer。
- **FrmConverter.keep**（放在 `D:\專案\電子相簿\Net\`）：列在裡面的表單，`Tools\FrmConverter` 重跑時不會再覆蓋它的 .Designer.vb / .resx / .Vb6.vb。移植完一個表單就把它加進去，之後就只在設計工具裡改。
- 對話框一律用 `Using f As New frmX : f.ShowDialog(Me)`（或 `f.ShowXxx(...)`）開啟，每次都從設計工具的預設值開始，跟 VB6 每次 Unload 後重新載入的行為一樣。

移植慣例：
- VB6 每次 `Unload Me` 後，下次顯示會重跑 Form_Load。.NET 的預設執行個體 `ShowDialog` + `Close` 只是隱藏，`Load` 事件只會觸發一次，所以 Form_Load 的內容放在 `OnVisibleChanged`（Visible = True 時）。
- `Screen.MousePointer = vbHourglass` → `Application.UseWaitCursor` 或表單的 `Cursor`。
- 表單停用（`Enabled = False`）時，子控制項的 `Enabled` 也會讀到 False；事件處理裡要判斷狀態請用自己的旗標，不要讀子控制項的 Enabled。
- 轉換工具產生的 Designer 常有兩行 `Me.Text =`（先 "Form1" 再真正的標題），第一行已全部刪掉。
- `SaveSetting` / `GetSetting` 仍用 VB6 的程式名稱 `iPhoto`（HKCU\Software\VB and VBA Program Settings\iPhoto），跟 VB6 版共用「最近開啟的相簿」。
- 表單裡的 `BorderStyle` 會解析成表單自己的屬性，列舉要寫 `System.Windows.Forms.BorderStyle.None`。

## 檔案對照

每個 VB6 檔案都已經有對應的 .NET 檔案；已移植的項目請見上面的「移植進度」。

| 種類 | 名稱 | VB6 原始檔（D:\專案\ 之下） | .NET 位置 |
|---|---|---|---|
| Form | `frmAddition` | 電子相簿\iPhoto\Form\frmAddition.frm | iPhoto.Net\Forms\ |
| Form | `frmBuildBook` | 電子相簿\iPhoto\Form\frmBuildBook.frm | iPhoto.Net\Forms\ |
| Form | `frmCover` | 電子相簿\iPhoto\Form\frmCover.frm | iPhoto.Net\Forms\ |
| Form | `frmDateTime` | 電子相簿\iPhoto\Form\frmDateTime.frm | iPhoto.Net\Forms\ |
| Form | `frmDefaultPhotoInfo` | 電子相簿\iPhoto\Form\frmDefaultPhotoInfo.frm | iPhoto.Net\Forms\ |
| Form | `frmDock` | 電子相簿\iPhoto\Form\frmDock.frm | iPhoto.Net\Forms\ |
| Form | `frmDropFileList` | 電子相簿\iPhoto\Form\frmDropFileList.frm | iPhoto.Net\Forms\ |
| Form | `frmExport` | 電子相簿\iPhoto\Form\frmExport.frm | iPhoto.Net\Forms\ |
| Form | `frmImport` | 電子相簿\iPhoto\Form\frmImport.frm | iPhoto.Net\Forms\ |
| Form | `frmKeyWords` | 電子相簿\iPhoto\Form\frmKeyWords.frm | iPhoto.Net\Forms\ |
| Form | `frmMain_1280x1024` | 電子相簿\iPhoto\Form\frmMain.frm | iPhoto.Net\Forms\ |
| Form | `frmPaint` | 電子相簿\iPhoto\Form\frmPaint.frm | iPhoto.Net\Forms\ |
| Form | `frmPaperSetup` | 電子相簿\iPhoto\Form\frmPaperSetup.frm | iPhoto.Net\Forms\ |
| Form | `frmPhotoIndex` | 電子相簿\iPhoto\Form\frmPhotoIndex.frm | iPhoto.Net\Forms\ |
| Form | `frmPhotoInfo` | 電子相簿\iPhoto\Form\frmPhotoInfo.frm | iPhoto.Net\Forms\ |
| Form | `frmPhotoInfoBatch_1` | 電子相簿\iPhoto\Form\frmPhotoInfoBatch_1.frm | iPhoto.Net\Forms\ |
| Form | `frmPhotoInfoBatch_2` | 電子相簿\iPhoto\Form\frmPhotoInfoBatch_2.frm | iPhoto.Net\Forms\ |
| Form | `frmPrint` | 電子相簿\iPhoto\Form\frmPrint.frm | iPhoto.Net\Forms\ |
| Form | `frmPrintPages` | 電子相簿\iPhoto\Form\frmPrintPages.frm | iPhoto.Net\Forms\ |
| Form | `frmSearch` | 電子相簿\iPhoto\Form\frmSearch.frm | iPhoto.Net\Forms\ |
| Form | `frmSearchResult` | 電子相簿\iPhoto\Form\frmSearchResult.frm | iPhoto.Net\Forms\ |
| Form | `frmSetup` | 電子相簿\iPhoto\Form\frmSetup.frm | iPhoto.Net\Forms\ |
| Form | `frmShowPhoto` | 電子相簿\iPhoto\Form\frmShowPhoto.frm | iPhoto.Net\Forms\ |
| Form | `frmToolTipText` | 電子相簿\iPhoto\Form\frmToolTipText.frm | iPhoto.Net\Forms\ |
| Module | `LibDataBase` | 電子相簿\iPhoto\Module\LibDataBase.bas | iPhoto.Net\Modules\ |
| Module | `LibMain` | 電子相簿\iPhoto\Module\LibMain.bas | iPhoto.Net\Modules\（Sub Main → Program.vb，全域物件 → PhotoLib Globals.vb） |
| Module | `LibUserInterface` | 電子相簿\iPhoto\Module\LibUserInterface.bas | iPhoto.Net\Modules\ |
| Class | `Albums` | 電子相簿\Lib\Class\Albums.cls | PhotoLib.Net\Classes\ |
| Class | `Book` | 電子相簿\Lib\Class\Book.cls | PhotoLib.Net\Classes\ |
| Class | `Class` | 電子相簿\Lib\Class\Class.cls | PhotoLib.Net\Classes\（VB 關鍵字，類別名寫成 `[Class]`） |
| Class | `Config` | 電子相簿\Lib\Class\Config.cls | PhotoLib.Net\Classes\ |
| Class | `Database` | 電子相簿\Lib\Class\Database.cls | PhotoLib.Net\Classes\ |
| Class | `Dock` | 電子相簿\Lib\Class\Dock.cls | PhotoLib.Net\Classes\ |
| Class | `Import` | 電子相簿\Lib\Class\Import.cls | PhotoLib.Net\Classes\ |
| Class | `KeyWord` | 電子相簿\Lib\Class\KeyWord.cls | PhotoLib.Net\Classes\ |
| Class | `MatrixPrint` | 電子相簿\Lib\Class\MatrixPrint.cls | PhotoLib.Net\Classes\ |
| Class | `Photo` | 電子相簿\Lib\Class\Photo.cls | PhotoLib.Net\Classes\ |
| Class | `Storage` | 電子相簿\Lib\Class\Storage.cls | PhotoLib.Net\Classes\ |
| Form | `frmBrowserFile` | 電子相簿\Lib\Form\frmBrowserFile.frm | PhotoLib.Net\Forms\ |
| Form | `frmBrowserFolder` | 電子相簿\Lib\Form\frmBrowserFolder.frm | PhotoLib.Net\Forms\ |
| Form | `frmInputString` | 電子相簿\Lib\Form\frmInputString.frm | PhotoLib.Net\Forms\ |
| Form | `frmMsgBox` | 電子相簿\Lib\Form\frmMsgBox.frm | PhotoLib.Net\Forms\ |
| Form | `frmQueryMsgBox` | 電子相簿\Lib\Form\frmQueryMsgBox.frm | PhotoLib.Net\Forms\ |
| Form | `frmResAlbum` | 電子相簿\Lib\Form\frmResAlbum.frm | PhotoLib.Net\Forms\ |
| Form | `frmSaveChangedPhoto` | 電子相簿\Lib\Form\frmSaveChangedPhoto.frm | PhotoLib.Net\Forms\ |
| Form | `frmSlideShow` | 電子相簿\Lib\Form\frmSlideShow.frm | PhotoLib.Net\Forms\ |
| Form | `frmSubject` | 電子相簿\Lib\Form\frmSubject.frm | PhotoLib.Net\Forms\ |
| Form | `frmViewerLarge` | 電子相簿\Lib\Form\frmViewerLarge.frm | PhotoLib.Net\Forms\ |
| Form | `frmViewerSmall` | 電子相簿\Lib\Form\frmViewerSmall.frm | PhotoLib.Net\Forms\ |
| Module | `LibAlbum` | 電子相簿\Lib\Module\LibAlbum.bas | PhotoLib.Net\Modules\ |
| Module | `LibApi` | 電子相簿\Lib\Module\LibApi.bas | PhotoLib.Net\Modules\ |
| Module | `LibDraw` | 電子相簿\Lib\Module\LibDraw.bas | PhotoLib.Net\Modules\ |
| Module | `LibStruct` | 電子相簿\Lib\Module\LibStruct.bas | PhotoLib.Net\Modules\（原本就是空的） |
| Class | `cTooltip` | Lib\Class\cToolTip.cls | PhotoLib.Net\Common\Classes\（可改用 WinForms ToolTip） |
| Form | `frmLoading` | Lib\Form\frmLoading.frm | PhotoLib.Net\Common\Forms\ |
| Form | `frmLogo` | Lib\Form\frmLogo.frm | PhotoLib.Net\Common\Forms\ |
| Module | `LibString` | Lib\Module\LibString.bas | PhotoLib.Net\Common\Modules\ |
| Module | `LibToolTipText` | Lib\Module\LibToolTipText.bas | PhotoLib.Net\Common\Modules\ |
| Module | `LibManifest` | Lib\Module\LibManifest.Bas | 不移植（由 `Application.EnableVisualStyles` 取代） |
| Form | `frmMain_1440X900_NEW` | 電子相簿\iPhoto\Form\frmMain1440X900_NEW.frm | 不移植 |

## 移植時發現的問題（VB6 原本就有）

已修正（✅）或保留原樣的地方：

- ✅ **frmMain `mlList_ItemKeyDown`（按 Delete 刪相片）**：VB6 先執行 `RemoveItem(Index)`，才讀 `Item(Index).FileName`，讀到的是下一張，所以攝影集會刪錯張。已改成先讀檔名。另外相簿（Class）沒有 `Save`，VB6 在這裡會出現 438 錯誤；現在只有攝影集（Book）會存檔，相簿的相片只從清單移除，跟 VB6 實際的效果相同。
- ✅ **frmMain `tvList_Click`**：在日曆模式下點選相簿樹，VB6 呼叫的是 `imgButton_Click(1)`（批次修改資訊），會跳出批次對話框。已改成 3（標準模式）。
- ✅ **frmMain `txtTitle_ExitFocus`**：VB6 寫成 `If KeyIndex >= 0 Then GoTo ExitProcess`，所以修改的相簿標題永遠不會更新到樹狀節點。已修正條件。
- ✅ **frmMain `mlList_ItemMarkChanged`**：VB6 用「目前選取的相片」的日期加入 Dock，而不是被標記的那張。已修正。
- ✅ **搜尋框**：使用者輸入的文字直接串進 SQL，輸入 `'` 就會出錯。現在會跳脫單引號。
- ✅ **frmMain 的 `m_frmViewer`**：VB6 表單自己的 CreateMultiMonters 整段被註解掉，所以這個變數永遠是 Nothing（LibMain 另外建立了一個 viewer，但表單沒有用到），「全圖瀏覽」會出現執行階段錯誤 91。現在所有 viewer 的呼叫都先檢查是否為 Nothing。
- ✅ **Aqua.Net MediaViewerControl**：原本只能「設定 FileName → 點一下播放」，沒辦法從程式控制。已補上 Play / Pause / Stop、PlayState、Duration、CurrentPosition（可設定，用來拖曳時間軸）、Volume、AutoRewind、VideoSize，以及 PlayStateChanged / MediaOpened 事件。net8（LibVLC）和 net35（WMP）兩個版本都支援，也支援 VB6 的影片副檔名（.dat、.rm、.m2p、.divx、.3gp、.m2ts…）。
- ✅ **Aqua.Net FileListBox 的 Pattern**：VB6 的格式是只寫副檔名（`Jpg;Bmp`），Aqua.Net 卻直接把 `Jpg` 當成檔名樣式，所以清單永遠是空的。已改成自動補上 `*.`。
- ✅ **frmSetup**：「附加檔案」的瀏覽按鈕按下時，閃的是同一個索引的「應用程式」按鈕；「雙螢幕切換」有顯示，但從來沒有存檔。兩者都已修正。
- ✅ **frmBrowserFolder / frmBrowserFile**：上一次的選擇會一直留著，所以按「取消」會回傳上次選的路徑；「是否建立資料夾」選「否」、或檔案不存在時，也還是會回傳那個路徑。現在只有按「確定」且成功時才回傳。
- ✅ **全圖瀏覽（viewer）**：VB6 frmMain 自己的 `m_frmViewer` 從來沒有建立（建立的程式碼被註解掉，LibMain 另外建了一個卻沒人用），所以「全圖瀏覽」會出現錯誤 91，雙螢幕的第二螢幕也不會跟著換相片。現在由 frmMain.CreateMultiMonters 建立並持有 viewer。
- ✅ **frmViewerLarge**：
  - 裁切、紅眼只有在由左上往右下框選時才正確；紅眼用的是螢幕座標而不是相片座標。
  - 縮放比例計算時拿高度去比畫布寬度。
  - ShowVideo 沒有記錄是否為第一張/最後一張，上一張/下一張按鈕狀態會錯。
  - 顯示檔案資訊時用的是上一次的媒體類型。
  - 按 Shift 畫正方形選取框，放開滑鼠時會變回一般矩形。
  - 以上都已修正。
- ✅ **frmSearch**：
  - 用資料庫搜尋時，「媒體」篩選讀的是「評價」下拉清單的值。
  - 不用資料庫時，結束日期拿去跟開始日期比較。
  - 以上都已修正。
- ✅ **frmDock**：載入 .dck 之後，標題上的張數沒有更新。已修正。
- 調整：frmSearchResult 的「成立攝影輯」在 VB6 會先把自己藏起來、做完再重新以對話框顯示；.NET 裡把對話框藏起來就等於關掉它，所以改成直接把 frmBuildBook 開在上面。
- 調整：亮度/對比滑桿每動一格，VB6 就保存一份整張相片的復原副本，在 32 位元程式裡很快就會耗盡記憶體。現在拖曳同一個滑桿只算一次復原，而且最多保留 10 步。
- ✅ **frmPhotoIndex**：每個類別的最後一張相片都印不出來（只有一張的類別什麼都沒有）；「指定列印頁次」選什麼都會整本印出；攝影集模式讀的卻是相片庫清單；頁尾可能只剩類別標題沒有相片。都已修正。
- ✅ **frmPrint**：選的頁次各自送成一個列印工作，現在合成一個；「存檔」取消資料夾時 VB6 會存到 C:\，現在直接取消；存檔的 JPEG 改為 200 dpi（VB6 是螢幕解析度），也印上拍攝日期，跟列印結果一致。
- ✅ **frmExport**：「依時間排序」勾了反而不排序（而且匯出時根本沒用到排序後的清單）；縮小時把相片硬拉成 1280×1024 等固定尺寸，現在保持比例、不放大；「是否建立資料夾」選否還是會繼續匯出。
- ✅ **frmDropFileList**：「複製到資料夾」會不經詢問刪掉整個目標資料夾，現在資料夾裡有東西時先詢問；超過 4 個檔案時編號格式失效（Photo1、Photo10…），現在補零。
- ✅ **frmPhotoInfoBatch_2**：整批存檔後表單一直停用（最後寫成 `Enabled = False`）；人物/地點清單有重複項目。
- ✅ **frmPhotoInfoBatch_1**：直接按根節點的「+」會看到「FAKE」佔位節點；沒勾任何相簿就按「下一步」會出錯。
- ✅ **frmBuildBook**：點既有的攝影集時，名稱欄填的是含標題的顯示文字，結果另外建了一本新的；現在填檔名。
- ✅ **frmCover**：往左上拖曳時裁到的是起點右下方的區域；裁切範圍沒有限制在相片內。
- ✅ **frmSlideShow**：相片庫裡沒有任何圖片（只有影片或空類別）時會無窮迴圈。
- ✅ **frmSubject**：沒選就關閉時會回傳上一次的選擇。
- ✅ **frmPaperSetup**：數字欄位連 Backspace 都被擋掉，打錯無法刪除。
- 注意：資料庫 FaceIndex.FileName 欄位只有 100 個字元（VB6 原本的 .mdb 結構），面孔封面資料夾路徑太長時 Jet 會拒絕寫入並跳出錯誤訊息。一般的相片庫路徑不會碰到。
- 保留原樣：**相簿大小永遠顯示「0 Byte」**。VB6 的 Class.Load 把加總檔案大小的那一行註解掉了，所以 VB6 本來就這樣顯示。
- ✅ **Aqua.Net Panel 的材質樣式**（LightSinking 等，主畫面工具列用的）：整張小材質圖被直接拉伸到面板大小，長條面板的左右圓角被拉成一大塊灰色。已改成跟 VB6 一樣的九宮格並排（Quartz Fill → `Skin.DrawFill`）。
- 調整：**主畫面可調整大小**。設計工具裡 `BorderStyle = Sizable`、有最大化/最小化鈕、`MinimumSize` = 設計尺寸（1275×987）；各區塊用 Anchor 跟著視窗伸縮（相簿樹與縮圖區變大，下方資訊欄、按鈕列、工具列貼齊底邊），工具列按鈕依寬度等比例分散（`Modules\LibLayout.vb` 的 ProportionalLayout，位置仍以設計工具為準）。啟動時在主螢幕最大化，最大化範圍是工作區（不蓋住工作列）；VB6 是把視窗撐滿整個螢幕，但控制項不會跟著移動。
- 新增：**雙螢幕時主視窗與相片檢視器自動對換**：把其中一個拖到另一個所在的螢幕，另一個會自動移到原本的螢幕；原本最大化的會在新螢幕再最大化。兩個視窗的螢幕、位置、是否最大化在每次移動後與關閉時存到 `HKCU\Software\VB and VBA Program Settings\iPhoto\Window`，下次開啟時還原（螢幕已不存在時回到預設）。程式在 `Modules\LibScreens.vb`（WindowPlacement、DualScreenSwap）。
- 新增：**從主畫面開啟的視窗跟主畫面在同一個螢幕**（`LibScreens.vb` 的 ChildWindowsFollowMain，在 Program.vb 啟用）。各表單的開啟位置設定不一：CenterScreen 會跑到「目前作用中視窗」所在的螢幕，WindowsDefaultLocation / Manual 和訊息框則固定在主螢幕。所以不逐一修改表單，而是用一個 UI 執行緒的視窗掛鉤（WH_CALLWNDPROC，攔 WM_SHOWWINDOW）：任何最上層視窗（WinForms 表單，以及 MessageBox、列印／檔案對話框等原生對話框）在第一次顯示的前一刻，如果跟主畫面不在同一個螢幕，就移過去。移動時保持在螢幕上的相對位置（置中的仍然置中），而且視窗還沒出現，所以不會閃。相片檢視器、投影片本身，以及由它們開啟的視窗（沿擁有者一路往上找）不動，留在檢視器的螢幕。frmMsgBox / frmQueryMsgBox 原本固定置中在主螢幕，改成置中在自己開啟的那個螢幕。
- ✅ **LoadPicture 的「Parameter is not valid」**：大照片在 32 位元程式裡 GDI+ 配置不到記憶體時會丟這個例外。現在先讓檢視器釋放上一張再載入新的、失敗時先回收記憶體再試一次，真的讀不了（檔案損壞或不支援）就清空檢視器，不會讓程式當掉。
- ✅ **Aqua.Net AquaForm / iForm 的背景圖**：SizeMode 預設是 Appose（VB6 = 並排），.NET 版卻只把背景圖原尺寸畫一張在中間，其餘區域沒畫、背景緩衝區也沒先清除，那些像素保持透明 → 畫面露出後面的舊內容，透明的 Label 也拿到這些殘影（輸入等 24 個表單文字錯亂）。現在先填 BackColor，再用 `Skin.DrawSized` 依 VB6 的 SizeMode 畫（Appose 並排、Fill 九宮格、CenterImage 置中…）。
- ✅ **轉換後 Label 蓋住其他控制項**：VB6 的 Label/Line/Shape 是無視窗控制項，永遠在最底層；WinForms 的 Label 是真的視窗，照 VB6 順序會蓋住圖片、箭頭等。所有 Designer 裡的 Label 已移到各容器的最底層，FrmConverter 也改成這樣產生。
- ✅ **AquaForm / iForm 的 `BorderStyle` 改名為 `WindowBorderStyle`**（固定／可調整大小）：原本的名稱在表單程式碼裡會擋住 WinForms 的 `BorderStyle` 列舉，VS 設計工具存檔後產生的 `picSubject.BorderStyle = BorderStyle.Fixed3D` 就編譯失敗（「'Fixed3D' 不是 'FormBorderStyle' 的成員」）。FrmConverter 也改成輸出新名稱。
- ✅ **VS 設計工具存檔會弄丟或寫錯的值**：Aqua 的 TextBox / ITextBox / MaskEdit / EditBox 的 `Text`（例如輸入畫面的「新相簿」）原本不會寫進 Designer；FileListBox.`Path`、DirListBox.`Root` 會被寫成設計工具的暫存資料夾。Aqua.Net 已修正這三項的序列化設定。
- 注意：VS 設計工具存檔時曾把 frmImport 的「華康細圓體」換成 Microsoft Sans Serif（字型其實已安裝，.NET 家族名稱是 DFYuanLight-B5），已改回。存檔後請留意 Designer 裡的字型名稱。另外不要用設計工具開 `.Vb6.vb`（專案已設定用程式碼檢視開啟）。
- ✅ **新控制項 Aqua.PngButton，取代程式裡所有的 ImageButton**（frmMain 的 imgToolBox / imgButton、frmPhotoInfo / frmSearch / frmDefaultPhotoInfo 的 imgKeyWords、frmKeyWords 的 imgAddition、frmViewerLarge / Small 的 imbNext / imbPrior，共 42 個）。只需要一張透明背景的 PNG（`Image`）：滑鼠移入或取得焦點時顯示原圖，平時淡化成 `ExitFocusOpacity`（預設 0.6），按下時稍暗，停用時灰階並同樣淡化。另外保留 `SizeMode`（CenterImage / Zoom）、`Text`、`BorderStyle`、`MousePress`。原本的五張狀態圖加上洋紅色 MaskImage 已轉成一張 PNG，存在各表單的 .resx（`<名稱>.Image`）：工具列用 MouseHover 圖，其他按鈕用 ExitFocus 圖。要換圖就在設計工具裡直接換 `Image`。ImageButton 仍留在 Aqua 裡給 UpDown 內部使用。
  - 滑鼠經過時彈跳放大：`HoverZoom`（預設 0.2 = 20%，0 = 關閉）、`HoverZoomDirection`（預設 Up = 底部不動往上放大；另有 Down / Left / Right / Center），只在 Enabled 時有作用。控制項不會畫到自己範圍外，所以要在設計工具裡把按鈕設大一點留出放大的空間，圖平時會貼著固定的那一邊。目前只有主畫面工具列 imgToolBox 開啟（按鈕 58×58，圖 48×48 貼底）；其他 PngButton 在 Designer 裡設成 `HoverZoom = 0!`。ProportionalLayout 置中時會扣掉按鈕上方預留的空間，讓圖示本身置中；但工具列面板上方只剩 7 px，放大需要 10 px，所以圖示和文字比原本低約 3 px。
- ✅ **Aqua.Panel 材質樣式（DarkSinking 等）的圓角外沒有透明**：材質圖裡紅色的圓角外圍雖然有去背，但底下是面板自己的 BackColor，看起來就是一塊淺色方角（例如主畫面工具列的四個角）。現在會先畫出父容器的背景，再疊上材質圖。
- ✅ **Aqua.Slider 沒有透明**：原本用一個近似的 Region（軌道矩形＋圓形滑塊）裁切，軌道兩端和滑塊四角的白色都露出來。現在改成先畫父容器的背景，載入圖片時再把跟圖片邊緣相連的白色去背（等同 VB6 的 vbWhite 去背）。
- ✅ **其他控制項的透明檢查**（把所有 Aqua 控制項放在洋紅／綠色棋盤格背景上逐一檢查）：
  - IconBox：`Transparency = True` 時完全不畫背景，沒有圖、或 PNG 的透明像素處會露出黑色殘影；現在改畫父容器背景。
  - RatingControl：預設背景是灰色 `Control`，星星外圍是一塊灰色方框；現在預設 `BackColor = Transparent`（放在 MediaItem 裡的外觀不變）。
  - Panel 的 `Simulation` 樣式：本來就應該透出父容器，卻畫成一塊灰色；現在改畫父容器背景。
  - Buttons、UpDown（Independence 樣式）：原本用通用的圓角矩形裁切，跟圖片的圓角對不上，角落露出白／灰色像素；現在改回 VB6 的做法，用各自的遮罩圖（btn_imgMask、imgHMask / imgVMask）三段式拉伸後，把黑色部分挖掉。DropDownList 原本就是這樣做，三者現在共用 `RegionUtil.CreateStretchedMaskRegion`。
  - ToolBar：跟 VB6 一樣預設白底；新增可以設成 `BackColor = Transparent`，原本一設就會出錯。
  - ScrollBar 軌道兩端的白色跟 VB6 一樣（VB6 也沒有去背），而且只出現在白底的清單控制項裡，所以維持原樣。UpDown 的 Alignment 樣式在 VB6 用另一組方形的圖（GetUpDownSurface），這組圖沒有移植，目前借用 Independence 的圓角圖，所以角落是淺色；程式裡沒有用到這個樣式。
  - 檢查過沒問題的：Label、TextBox、ITextBox、EditBox、MaskEdit、DropDownList、FlashButton、Slider、各種 Panel 材質樣式的圓角、ImageButton、PngButton、CheckBox、RadioButton。ProgressBar、TimeLine 的圖本身就是方形。Month、Calendar 的白底是刻意設定的 BackColor，跟 VB6 一樣。
- 新增：**同步備份**（.NET 版新功能，把原本獨立的 FolderSyncWPF 工具改用 iPhoto 的風格做進來）。
  - 主畫面工具列：新增 `imgToolBox_14`「同步備份」（NAS.PNG，圖存在 .resx），放在燒錄前面。15 個按鈕重新平均排列：同一組內間距 76 px，跨組 104 px，分隔線在兩組正中間。
  - `Forms\frmBackup`（AquaForm，跟「匯出」同尺寸，版面都在 Designer）：
    - 「備份來源」清單，每列顯示「來源 → 實際目的」，用 ＋ ✕ 新增／刪除；
    - 「備份目的」：瀏覽圖示＋路徑，另有「套用至所有來源」；
    - 日誌（只保留最後 400 行，完整內容在 sync_log.txt）；
    - 結束／開始同步按鈕（同步中「結束」變「取消」）和進度條。
    - 圖示和背景沿用「設定」、「匯出」的圖。清單與選項存在登錄檔 `...\iPhoto\Backup`。
  - 同步規則在 `Modules\LibBackup.vb`（FolderBackup），跟 FolderSyncWPF 一樣：
    - 單向；來源保留路徑、去掉磁碟代號，放到目的底下；
    - 目的沒有的檔案就複製，來源比較新的就覆蓋；
    - 來源已刪除的不刪除，移到 `Lost\yyyyMMdd\`；每次寫一份 sync_log.txt。
  - 修正 FolderSyncWPF 的問題：
    - 子資料夾裡刪除的檔案也會移到 Lost（原本只檢查最上層）；
    - 顯示的目的路徑和日誌路徑是實際位置（原本少了中間的路徑）；
    - 空的來源不會除以 0；
    - 設定不再存成跟著啟動目錄走的 sources.json。
  - 沒有做的：定時自動備份、縮小到系統列。iPhoto 不是常駐程式，這兩項不適合放進來。
  - Aqua.EditBox 新增 `ScrollToEnd()`：沒有焦點的文字框設定 SelStart 不會捲動，日誌要用這個捲到最後。
- ✅ **圓鈕／核取方塊與旁邊的 Label 合併**：VB6 的 Aqua.RadioButton / CheckBox 只畫圖，說明文字是旁邊另一個 Label，所以點文字選不到。13 個畫面、29 組（frmAddition、frmDateTime、frmExport、frmImport、frmPhotoIndex、frmPhotoInfoBatch_2、frmPrint、frmPrintPages、frmSearch、frmSetup、frmSaveChangedPhoto、frmViewerLarge、frmViewerSmall）都改成把 Label 的文字放進 `TextValue`，並把 Label 刪掉。
  - 位置：移到 Label 那一行；`TextGap` 設成讓文字維持在原來的 X。
  - Label 自己的 ForeColor／Font 一併移到圓鈕上。
  - 寬度依文字自動計算，所以拿掉固定的 Size。
  - 原本操作 Label 的程式改成操作圓鈕：frmSaveChangedPhoto 的「儲存」刪除線、frmPrintPages 的「本頁：n」、frmSearch 未選項目變灰。frmSetup「左右螢幕對調」和 frmSaveChangedPhoto 原本手動把 Label 設灰，現在停用時圓鈕會自己畫灰字，所以拿掉。frmImport、frmPrintPages「點文字就選取」的處理不再需要。
  - 只剩 Label 的 VB6 控制項陣列（lblSaveMode、lblPrintNote、lblPage、lblRange、lblExecute）已移除。
  - frmSetup 的 rbStyle、chkSwitchScreen 在 VB6 就是隱藏的（連同它們的 Label），維持隱藏。
  - Aqua.RadioButton / CheckBox：文字顏色改用 `ForeColor`（原本固定黑色），停用時仍是灰色；新增 `TextGap`（圖與文字的間距，預設 4，跟原本一樣）。
- ✅ **iForm / AquaForm 標題字不清楚**：標題和 AquaForm 的選單列文字是用 TextRenderer（GDI）畫進表單自己的後台緩衝點陣圖，GDI 畫進點陣圖時完全沒有平滑處理，Times New Roman 被畫成粗糙的鋸齒塊，非作用中視窗的灰色（VB6 的 gc_lngFormTitleDeactiveColor &H808080）就糊成一團。VB6 是用 Label 直接畫在螢幕上，由 Windows 平滑處理。現在改用 GDI+ 加 ClearType 畫（`Internal\ChromeText.vb`），標題列底圖是不透明的，所以畫進點陣圖也能平滑。選單項目的寬度（點擊範圍）仍用 TextRenderer 計算，不變。
- ✅ **frmSetup 的分頁整理**：VB6 的 PageHead1 + PageSheet1(0..3) 早已轉成一個 Aqua.TabControl，這次把 VB6 殘留整理掉。
  - 改用有意義的名稱：`tabSetup`；頁面 `pageGeneral`（一般設定）、`pageAlbums`（相簿位置）、`pageAppearance`（外觀/音效）、`pagePlugins`（外掛應用程式）。
  - 刪掉沒用到的 `PageSheet1()` 控制項陣列。
  - 刪掉各頁沒有作用的 `Image`（.resx 裡的 4 張圖）、`SizeMode`、`BorderColor`、`BorderFocusColor`。
  - **Aqua.TabPage** 原本寫 `Inherits Panel`，在 Aqua 命名空間裡被解析成 Aqua.Panel，所以屬性視窗多出一堆沒作用的屬性；現在明確繼承 WinForms 的 Panel。
  - **Aqua.TabControl 新增 `TabStripPadding`**：標籤列兩端內縮的距離（標籤在上／下時用 Left、Right，在左／右時用 Top、Bottom）。frmSetup 設成 (76, 0, 41, 0)，和 VB6 的 PageHead 位置相同。標籤列以外的標頭區改畫父容器的真實背景，原本只填父容器的背景色。
  - FrmConverter 同步修改：PageSheet 不再輸出上述屬性；PageHead 比頁面窄時，自動輸出對應的 `TabStripPadding`。
  - **設計工具裡點標籤不會切換頁面**：原本靠 AquaTabControlDesigner 的 `GetHitTest` 把標籤列設為「可操作」，讓點擊傳給控制項的 OnMouseDown。實際查出兩個問題：
    1. 設計工具即使 GetHitTest 回傳 True，也不會把點擊交給 OnMouseDown（用 DesignSurface 實測）。
    2. **.NET 8 專案用的 Visual Studio 跨處理序設計工具（DesignToolsServer）根本不會載入自訂的 ControlDesigner 類別**，只有 .NET Framework 專案（例如 Aqua.Net.Demo）的處理序內設計工具會用。所以 Demo 會切換，frmSetup 不會。
  - 現在的做法：
    - 處理序內設計工具（.NET Framework）：AquaTabControlDesigner 在按下左鍵時自己判斷點到哪個標籤並切換（`TabControl.SelectTabAt`）。
    - 所有設計工具（含 .NET 8）：TabControl 在設計模式下用一個 30 ms 的計時器偵測左鍵剛按下；如果點在它的標籤上，而且那個位置沒被其他程式的視窗蓋住，就切換。「沒被蓋住」的判斷是：滑鼠下的視窗屬於同一個處理序，或跟 TabControl 在同一個最上層視窗。在 .NET 8 設計工具裡，滑鼠下的其實是 Visual Studio（devenv）的視窗，所以不能只判斷處理序。改用「選取改變」偵測行不通：再點一次已選取的 TabControl 不會觸發選取改變。
    - 另外：在文件大綱或屬性視窗選取某一頁上的控制項，會自動切到那一頁（透過設計工具的選取服務 ISelectionService）。
    - 計時器只在設計模式下啟動，執行時不會有。
- ⚡ **MediaList 大量相片（虛擬化）**：原本每張相片都是一組完整的 WinForms 控制項（5 個視窗），加入時就全部建立。約 1990 張會用完 Windows 每個程式 10,000 個視窗的上限而出錯（「建立視窗控制代碼時發生錯誤」），1000 張加入要 12 秒、清空 2 秒。現在每張相片仍有一個 MediaItem 物件（`AddItem`、`Item(i)`、`SelectedItem`、`FileName`、`Marked`、`Ranking` 用法都不變），但只有顯示區域前後各一列的項目真的放進清單、有視窗；捲出去就釋放（播放中的影片先停止、提示框一起釋放），資料與縮圖保留。項目直接用畫面座標放在可視區，不再有超過 32767 px 高的內容面板。實測（UiTest10 `ml`）：
  - 視窗物件固定約 176 個（原本 1000 張 5,033 個），3000 張也正常（原本約 1990 張就失敗）。
  - 加入 1000 張 2.4 秒（原本 12.3 秒），3000 張 4.8 秒；清空 1000 張 0.08 秒（原本 2.2 秒）。
  - 一併修正：Aqua 的 CheckBox / RadioButton / Button（ExamControls）每建立一個就 new 一次 frmResource 表單取預設圖（每個約 3 ms），改成整個程式只載入一次共用（`ExamDefaultImages`）；MediaViewerControl 的訊息過濾器改成有視窗時才註冊（原本一個項目一個，上千個過濾器每則訊息都要經過）。
  - **加入相片時不再自動捲動**：VB6 每加一張就捲到最後（LibUserInterface.AddPhotoToMediaList、frmMain.MoveFilesToMediaList），所以每張縮圖在載入時就全部解碼；現在停在原位置，只載入看得到的。
  - **縮圖在背景產生**（`ExamControls\ThumbnailLoader.vb`）：MediaList 的項目改由 2 條背景執行緒解碼（先空白，好了再顯示）。請求後進先出，剛捲進畫面的優先；捲走或換了檔案的請求輪到時直接跳過。影片縮圖（Shell）也在背景取得，所以執行緒是 STA。單獨使用的 MediaViewerControl／MediaItem（看片視窗、匯入畫面）仍是同步載入。
  - **記憶體上限**：項目捲出畫面就釋放縮圖。ThumbnailLoader 另外保留 48 MB 的 LRU 快取（以「路徑＋大小＋檔案修改時間」為鍵，存檔修改後會重新解碼），捲回來時直接取用。另外拿掉每張縮圖多複製一份的 `_thumb`（原本是給拖曳用；現在只有影片開始播放、背景圖被釋放前才保留一份）。
  - 實測：1000 張從頭捲到尾 6.7 秒（原本 29 秒），最長一次捲動（一整個畫面）UI 忙 0.12 秒；捲完後記憶體 1000 張 98 MB（原本 300 MB）、3000 張 224 MB（原本 837 MB），不再隨捲過的張數增加。
- ⚡ **效能優化**（UiTest10 的 bench 實測）：
  - Aqua.Panel 材質樣式：原本每次重畫都重新建一張整個面板大小的點陣圖，做九宮格再去背；現在依大小／樣式快取，重畫時只貼需要更新的那一塊。工具列上 PngButton 動畫每一格從約 1.05 ms 降到 0.21 ms，整個面板重畫從約 2.7 ms 降到 1.0 ms。
  - RegionUtil.CreateRegionFromBitmap（DropDownList、Buttons、UpDown、Label、IconBox 等的外形裁切）：改用 LockBits 一次讀取像素（原本每個像素呼叫 GetPixel），並把形狀相同的連續列合併成一個矩形（原本每列每段一個矩形）。400×23 的遮罩從約 2–3 ms 降到 0.19 ms。
  - ChildWindowsFollowMain 的視窗掛鉤會收到 UI 執行緒的每一則訊息：改成先只讀訊息編號，不是 WM_SHOWWINDOW 就直接略過，不再每則都複製整個結構。
- ✅ **Aqua.Net 的 AquaForm / iForm** 的 `SizeMode = Fill` 原本畫成直接拉伸，現在已照 VB6 用九宮格並排（上面 Skin.DrawSized 的修正）。
- **Aqua.Net.Tests 無法建置**：`Label` 模稜兩可（Aqua.Label 和 WinForms Label）。
