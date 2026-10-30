<#
.SYNOPSIS
  發佈 iPhoto 到相片庫的程式資料夾（預設是 FolderProfile.pubxml 的 PublishDir，例如 D:\生活剪輯）。

.DESCRIPTION
  不直接用 Visual Studio 的資料夾設定檔發佈到相片庫，因為那樣會：
    - 把專案裡的 iPhoto.Ini（範本）蓋過使用中的那份；
    - 多帶出 libvlc\win-x86（約 100 MB；程式是 x64，只用 win-x64）。
  所以：
    1. 確認目的地的 iPhoto 沒在執行；
    2. 備份目的地的 iPhoto.Ini；
    3. dotnet publish 到暫存資料夾；
    4. robocopy 到目的地，只複製有變的檔案，不複製 iPhoto.Ini、Places\Places.txt、
       Places\Attractions.txt（景點資料可能用「更新景點資料」更新過）和 libvlc\win-x86，
       也不刪除目的地任何檔案；
    5. 確認 iPhoto.Ini 沒被改動。

.PARAMETER Target
  發佈到哪裡；省略時讀 FolderProfile.pubxml 的 PublishDir。

.PARAMETER DryRun
  只列出會複製哪些檔案，不實際複製（仍會建置）。

.PARAMETER NoPhotoEdit
  不一起放 PhotoEdit（C:\專案\PhotoEdit；預設會建置後併入，供工具列「編輯」使用）。

.EXAMPLE
  .\Publish.ps1
  .\Publish.ps1 -DryRun
  .\Publish.ps1 -Target E:\生活剪輯
#>
[CmdletBinding()]
param(
    [string]$Target,
    [switch]$DryRun,
    [switch]$NoPhotoEdit
)

$ErrorActionPreference = 'Stop'
$repo    = $PSScriptRoot
$project = Join-Path $repo 'iPhoto.Net\iPhoto.Net.vbproj'
$pubxml = Join-Path $repo 'iPhoto.Net\My Project\PublishProfiles\FolderProfile.pubxml'
$work    = Join-Path $env:TEMP 'iPhoto-publish'
$staging = Join-Path $work 'out'
$excludeFiles = @('iPhoto.Ini', 'Places.txt', 'Attractions.txt')

function Step([string]$text) { Write-Host "`n== $text" -ForegroundColor Cyan }
function Fail([string]$text) { Write-Host "`n發佈中止：$text" -ForegroundColor Red; exit 1 }

# ---- 目的地 ----
if (-not $Target) {
    [xml]$pub = Get-Content -LiteralPath $pubxml -Encoding UTF8
    $Target = ($pub.Project.PropertyGroup | ForEach-Object { $_.PublishDir } | Where-Object { $_ } | Select-Object -First 1)
    if (-not $Target) { Fail "FolderProfile.pubxml 沒有 PublishDir，請用 -Target 指定" }
}
$Target = $Target.TrimEnd('\')
if (-not (Test-Path -LiteralPath $Target -PathType Container)) { Fail "找不到目的地 $Target" }
Write-Host "目的地：$Target$(if ($DryRun) { '（試跑，不複製）' })"

# ---- 1. iPhoto 不能在執行 ----
Step '檢查 iPhoto 是否在執行'
$running = Get-Process -Name iPhoto -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -and $_.Path.StartsWith($Target + '\', [StringComparison]::OrdinalIgnoreCase) }
if ($running) { Fail "目的地的 iPhoto 正在執行（PID $($running.Id -join ', ')），請先關閉" }
Write-Host '沒有在執行'

# ---- 2. 備份 iPhoto.Ini ----
$ini = Join-Path $Target 'iPhoto.Ini'
$iniBackup = $null
if (Test-Path -LiteralPath $ini) {
    Step '備份 iPhoto.Ini'
    New-Item -ItemType Directory -Force $work | Out-Null
    $iniBackup = Join-Path $work ("iPhoto.Ini.{0:yyyyMMdd-HHmmss}" -f (Get-Date))
    Copy-Item -LiteralPath $ini -Destination $iniBackup
    Write-Host "備份到 $iniBackup"
}

# ---- 3. 發佈到暫存資料夾 ----
Step 'dotnet publish 到暫存資料夾'
if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
& dotnet publish $project -c Release "-p:PublishDir=$staging\" -nologo -v q -clp:ErrorsOnly   # dotnet 不讀 My Project\PublishProfiles，設定檔只用來取目的地
if ($LASTEXITCODE -ne 0) { Fail "dotnet publish 失敗（$LASTEXITCODE）" }
if (-not (Test-Path -LiteralPath (Join-Path $staging 'iPhoto.exe'))) { Fail '暫存資料夾裡沒有 iPhoto.exe' }
Write-Host "完成：$staging"

# ---- 3b. PhotoEdit（工具列「編輯」）一起放進去 ----
# 建置（不是 publish：宮崎風 AI 重繪的 Redraw\ 只在建置時產生），不複製模型（Models\ 約 6.6 GB，
# 目的地已有，和 iPhoto 共用）；只補上 iPhoto 沒有的檔案，共用的 DLL（Aqua.Net、OpenCvSharp ...）以 iPhoto 的為準；
# ONNX Runtime 其他平台的原生檔（android、ios、linux、osx，約 150 MB）不放。
$peProject = Join-Path (Split-Path $repo -Parent) 'PhotoEdit\PhotoEdit\PhotoEdit.vbproj'
if (-not $NoPhotoEdit -and (Test-Path -LiteralPath $peProject)) {
    Step 'PhotoEdit 建置到暫存資料夾'
    $peOut = Join-Path $work 'photoedit'
    if (Test-Path -LiteralPath $peOut) { Remove-Item -LiteralPath $peOut -Recurse -Force }
    & dotnet build $peProject -c Release "-p:OutDir=$peOut\" -p:SkipModelCopy=true -nologo -v q -clp:ErrorsOnly
    if ($LASTEXITCODE -ne 0) { Fail "PhotoEdit 建置失敗（$LASTEXITCODE）；要先發佈 iPhoto 可加 -NoPhotoEdit" }
    if (-not (Test-Path -LiteralPath (Join-Path $peOut 'PhotoEdit.exe'))) { Fail 'PhotoEdit 的輸出裡沒有 PhotoEdit.exe' }
    & robocopy $peOut $staging /E /XC /XN /XO /XD (Join-Path $peOut 'Models') android ios 'linux-*' 'osx-*' 'maccatalyst-*' browser-wasm /NJH /NJS /NP /NDL /NFL | Out-Null
    if ($LASTEXITCODE -ge 8) { Fail "合併 PhotoEdit 失敗（robocopy $LASTEXITCODE）" }
    Write-Host '完成：PhotoEdit 已併入'
}

# ---- 4. 複製到目的地 ----
Step $(if ($DryRun) { '列出會複製的檔案' } else { '複製到目的地' })
$copyArgs = @($staging, $Target, '/E', '/XX', '/NJH', '/NP', '/NDL',
          '/XF') + $excludeFiles + @('/XD', (Join-Path $staging 'libvlc\win-x86'))
if ($DryRun) { $copyArgs += '/L' }
& robocopy @copyArgs
$rc = $LASTEXITCODE
if ($rc -ge 8) { Fail "robocopy 失敗（$rc）" }   # 0-7 = 成功（有沒有複製檔案）

# ---- 5. iPhoto.Ini 沒被動 ----
if ($iniBackup) {
    Step '檢查 iPhoto.Ini'
    $same = (Get-FileHash -LiteralPath $ini).Hash -eq (Get-FileHash -LiteralPath $iniBackup).Hash
    if (-not $same) { Fail "iPhoto.Ini 被改動了！原本的在 $iniBackup" }
    Write-Host '沒有改動'
}

Write-Host "`n$(if ($DryRun) { '試跑完成，沒有複製任何檔案' } else { '發佈完成' })" -ForegroundColor Green
exit 0
