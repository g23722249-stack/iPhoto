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

.EXAMPLE
  .\Publish.ps1
  .\Publish.ps1 -DryRun
  .\Publish.ps1 -Target E:\生活剪輯
#>
[CmdletBinding()]
param(
    [string]$Target,
    [switch]$DryRun
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
