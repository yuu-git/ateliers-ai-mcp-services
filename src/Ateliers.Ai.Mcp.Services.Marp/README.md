# Ateliers.Ai.Mcp.Services.Marp

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.Marp.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Marp/)

[Marp CLI](https://github.com/marp-team/marp-cli) を使用したプレゼンテーションスライド生成サービスです。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.Marp
```

## 機能

- Markdown からプレゼンテーションスライド用 Markdown への変換
- Marp CLI を使用したスライドの PNG 画像レンダリング
- 自動スライド分割（見出しベース）
- Frontmatter の自動生成（marp、theme、paginate）

## 前提条件

このサービスを使用するには、[Marp CLI](https://github.com/marp-team/marp-cli) のインストールが必要です。

### Marp CLI のインストール

```bash
# npm を使用する場合
npm install -g @marp-team/marp-cli

# または Homebrew (macOS) を使用する場合
brew install marp-cli
```

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.Marp;

// サービスオプションの設定
var options = new MarpServiceOptions
{
    MarpExecutablePath = "marp", // または完全なパス
    WorkDirectory = @"C:\temp\marp",
    MarpOutputDirectoryName = "output"
};

// サービスの初期化
var service = new MarpService(options);

// Markdown からスライド用 Markdown を生成
var sourceMarkdown = @"
# タイトルスライド

プレゼンテーションの概要

# セクション1

内容1

# セクション2

内容2
";

var slideMarkdown = service.GenerateSlideMarkdown(sourceMarkdown);

// PNG 画像としてレンダリング
var pngFiles = await service.RenderToPngAsync(slideMarkdown);

foreach (var file in pngFiles)
{
    Console.WriteLine($"生成されたスライド: {file}");
}
```

### カスタム Marp CLI パスの指定

```csharp
var options = new MarpServiceOptions
{
    MarpExecutablePath = @"C:\Program Files\nodejs\marp.cmd",
    WorkDirectory = @"C:\Projects\presentations",
    MarpOutputDirectoryName = "slides"
};
```

## サービスオプション

### `IMarpServiceOptions`

| プロパティ | 型 | 説明 | デフォルト |
|----------|-----|------|----------|
| `MarpExecutablePath` | `string` | Marp CLI の実行可能ファイルパス | `"marp"` |
| `WorkDirectory` | `string` | 作業ディレクトリのベースパス | 必須 |
| `MarpOutputDirectoryName` | `string` | 出力ディレクトリ名 | `"output"` |

## 仕様

### スライド分割ルール

- Markdown の各見出し（`#` で始まる行）が新しいスライドの開始となります
- 水平線（`---`）は無視されます（Marp のスライド区切りと競合するため）
- 最低2枚のスライドが必要です

### 自動生成される Frontmatter

```yaml
---
marp: true
theme: default
paginate: true
---
```

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **Marp**（このパッケージ）- Marp プレゼンテーション生成サービス
- **Tools** - 複数の MCP サービスを組み合わせた MCP タスク単位の実装

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
