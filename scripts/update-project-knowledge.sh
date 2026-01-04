#!/bin/bash

#######################################
# ateliers-ai-mcp-projectbase 更新スクリプト
# 
# 使用方法:
#   ./scripts/update-project-knowledge.sh
#######################################

set -e

# 設定
SUBMODULE_PATH=".submodules/ateliers-ai-mcp-projectbase"
BRANCH="master"

# カラー出力
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}🔄 Project Knowledge を更新中...${NC}"
echo ""

# サブモジュールの存在確認
if [ ! -d "$SUBMODULE_PATH" ]; then
    echo -e "${YELLOW}⚠️  エラー: サブモジュールが見つかりません${NC}"
    echo "   先にセットアップを実行してください:"
    echo "   curl -fsSL https://raw.githubusercontent.com/yuu-git/ateliers-ai-mcp-projectbase/master/scripts/init-for-project.sh | bash"
    exit 1
fi

# サブモジュールディレクトリに移動
cd "$SUBMODULE_PATH" || exit 1

# 現在のブランチを確認
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo "現在のブランチ: $CURRENT_BRANCH"

# masterブランチに切り替え（必要な場合）
if [ "$CURRENT_BRANCH" != "$BRANCH" ]; then
    echo -e "${BLUE}🌿 $BRANCH ブランチに切り替え中...${NC}"
    git checkout "$BRANCH"
fi

# 更新前のコミットハッシュを取得
OLD_COMMIT=$(git rev-parse --short HEAD)

# 最新版を取得
echo -e "${BLUE}📥 最新版をダウンロード中...${NC}"
git pull origin "$BRANCH"

# 更新後のコミットハッシュを取得
NEW_COMMIT=$(git rev-parse --short HEAD)

# 元のディレクトリに戻る
cd ../..

echo ""
if [ "$OLD_COMMIT" != "$NEW_COMMIT" ]; then
    echo -e "${GREEN}✅ 更新完了！${NC}"
    echo ""
    echo "変更内容:"
    echo "  $OLD_COMMIT → $NEW_COMMIT"
    echo ""
    echo "詳細を確認:"
    echo "  cd $SUBMODULE_PATH && git log $OLD_COMMIT..$NEW_COMMIT --oneline"
else
    echo -e "${GREEN}✅ 既に最新版です${NC}"
fi

echo ""
echo "参照ファイル:"
echo "  - $SUBMODULE_PATH/llms.txt"
echo "  - $SUBMODULE_PATH/architecture/**/*.md"
echo "  - $SUBMODULE_PATH/design-principles/**/*.md"
echo ""
