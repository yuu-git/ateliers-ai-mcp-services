#######################################
# ateliers-ai-mcp-projectbase 更新スクリプト (PowerShell版)
# 
# 使用方法:
#   .\scripts\update-project-knowledge.ps1
#######################################

param(
    [switch]$Help
)

if ($Help) {
    Write-Host @"
ateliers-ai-mcp-projectbase 更新スクリプト

使用方法:
  .\scripts\update-project-knowledge.ps1

このスクリプトは以下を実行します:
  - サブモジュールの最新版を取得
  - 変更内容の表示
"@
    exit 0
}

# 設定
$SUBMODULE_PATH = ".submodules/ateliers-ai-mcp-projectbase"
$BRANCH = "master"

# エラー発生時に停止
$ErrorActionPreference = "Stop"

Write-Host "🔄 Project Knowledge を更新中..." -ForegroundColor Blue
Write-Host ""

# サブモジュールの存在確認
if (-not (Test-Path $SUBMODULE_PATH)) {
    Write-Host "⚠️  エラー: サブモジュールが見つかりません" -ForegroundColor Yellow
    Write-Host "   先にセットアップを実行してください:"
    Write-Host "   irm https://raw.githubusercontent.com/yuu-git/ateliers-ai-mcp-projectbase/master/scripts/init-for-project.ps1 | iex"
    exit 1
}

# サブモジュールディレクトリに移動
Push-Location $SUBMODULE_PATH

try {
    # 現在のブランチを確認
    $currentBranch = git rev-parse --abbrev-ref HEAD
    Write-Host "現在のブランチ: $currentBranch"

    # masterブランチに切り替え（必要な場合）
    if ($currentBranch -ne $BRANCH) {
        Write-Host "🌿 $BRANCH ブランチに切り替え中..." -ForegroundColor Blue
        git checkout $BRANCH
    }

    # 更新前のコミットハッシュを取得
    $oldCommit = git rev-parse --short HEAD

    # 最新版を取得
    Write-Host "📥 最新版をダウンロード中..." -ForegroundColor Blue
    git pull origin $BRANCH

    # 更新後のコミットハッシュを取得
    $newCommit = git rev-parse --short HEAD

    Write-Host ""
    if ($oldCommit -ne $newCommit) {
        Write-Host "✅ 更新完了！" -ForegroundColor Green
        Write-Host ""
        Write-Host "変更内容:"
        Write-Host "  $oldCommit → $newCommit"
        Write-Host ""
        Write-Host "詳細を確認:"
        Write-Host "  cd $SUBMODULE_PATH"
        Write-Host "  git log $oldCommit..$newCommit --oneline"
    } else {
        Write-Host "✅ 既に最新版です" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "参照ファイル:"
    Write-Host "  - $SUBMODULE_PATH/llms.txt"
    Write-Host "  - $SUBMODULE_PATH/architecture/**/*.md"
    Write-Host "  - $SUBMODULE_PATH/design-principles/**/*.md"
    Write-Host ""

} finally {
    # 元のディレクトリに戻る
    Pop-Location
}
