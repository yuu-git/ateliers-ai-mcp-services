<#
.SYNOPSIS
    Build後のNuGetパッケージをローカルフィードにコピー

.DESCRIPTION
    src配下のビルド成果物(.nupkg)をローカルNuGetフィードにコピーし、
    キャッシュをクリアして即座に参照可能にします。
    
    パスの優先順位:
    1. -LocalFeedPath パラメータ
    2. 環境変数 ATELIERS_LOCAL_NUGET_PATH
    3. デフォルト C:\LocalNuGet

.PARAMETER LocalFeedPath
    ローカルNuGetフィードのパス（省略時は環境変数またはデフォルト）

.PARAMETER ClearOldVersions
    同名パッケージの古いバージョンを削除（デフォルト: true）

.EXAMPLE
    .\copy-to-local-nuget.ps1
    
.EXAMPLE
    .\copy-to-local-nuget.ps1 -LocalFeedPath "D:\MyNuGet"
#>

param(
    [string]$LocalFeedPath = "",
    [bool]$ClearOldVersions = $true
)

# エラー時に停止
$ErrorActionPreference = "Stop"

Write-Host "=== Local NuGet Feed Copy Tool ===" -ForegroundColor Cyan
Write-Host ""

# 現在のディレクトリ表示
Write-Host "Current Directory: $(Get-Location)" -ForegroundColor Gray
Write-Host ""

# ソリューションルートチェック
if (-not (Test-Path "..\src")) {
    Write-Error "Error: src directory not found. Please run from solution root."
    exit 1
}

# パス決定（優先順位: パラメータ > 環境変数 > デフォルト）
if (-not $LocalFeedPath) {
    if ($env:ATELIERS_LOCAL_NUGET_PATH) {
        $LocalFeedPath = $env:ATELIERS_LOCAL_NUGET_PATH
        Write-Host "Using path from environment variable: $LocalFeedPath" -ForegroundColor Cyan
    } else {
        $LocalFeedPath = "C:\LocalNuGet"
        Write-Host "Using default path: $LocalFeedPath" -ForegroundColor Yellow
        Write-Host "Tip: Set ATELIERS_LOCAL_NUGET_PATH environment variable for custom path" -ForegroundColor Gray
    }
} else {
    Write-Host "Using path from parameter: $LocalFeedPath" -ForegroundColor Cyan
}
Write-Host ""

# ローカルフィード作成
if (-not (Test-Path $LocalFeedPath)) {
    Write-Host "Creating local feed directory: $LocalFeedPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $LocalFeedPath | Out-Null
}

# .nupkg ファイルを検索
Write-Host "Searching for .nupkg files in src..." -ForegroundColor White
$packages = Get-ChildItem -Path "..\src" -Include "*.nupkg", "*.snupkg" -Recurse | 
    Where-Object { $_.FullName -match "\\bin\\(Debug|Release)\\" }

if ($packages.Count -eq 0) {
    Write-Warning "No .nupkg files found. Please build your projects first."
    exit 0
}

Write-Host "Found $($packages.Count) package(s):" -ForegroundColor Green
$packages | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""

# 古いバージョン削除（オプション）
if ($ClearOldVersions) {
    Write-Host "Clearing old versions from local feed..." -ForegroundColor Yellow
    
    $packagePrefixes = $packages | ForEach-Object {
        # .nupkg と .snupkg の両方に対応
        $_.Name -replace '\.\d+\.\d+\.\d+(-\w+)?\.(s)?nupkg$', ''
    } | Select-Object -Unique
    
    foreach ($prefix in $packagePrefixes) {
        # .nupkg と .snupkg の両方を削除
        Get-ChildItem -Path $LocalFeedPath -Include "$prefix.*.nupkg", "$prefix.*.snupkg" -ErrorAction SilentlyContinue |
            Remove-Item -Force -Verbose
    }
    Write-Host ""
}

# 新しいパッケージをコピー
Write-Host "Copying packages to local feed..." -ForegroundColor Green
$packages | Copy-Item -Destination $LocalFeedPath -Force -Verbose
Write-Host ""

# NuGetキャッシュクリア
Write-Host "Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear | Out-Null
Write-Host "✓ Cache cleared" -ForegroundColor Green
Write-Host ""

# 完了メッセージ
Write-Host "=== Completed ===" -ForegroundColor Cyan
Write-Host "Local feed path: $LocalFeedPath" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Verify local feed is registered:" -ForegroundColor White
Write-Host "     nuget sources list" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. If not registered, add it once:" -ForegroundColor White
Write-Host "     nuget sources add -name 'LocalDev' -source '$LocalFeedPath'" -ForegroundColor Gray
Write-Host ""