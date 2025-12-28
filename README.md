# Ateliers AI Model Context Protocol (MCP) Services

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

C# による [Model Context Protocol (MCP)] のサービス層実装です。

## パッケージ

```bash
# 基本サービスとインターフェース
dotnet add package Ateliers.Ai.Mcp.Services

# Notion API 連携
dotnet add package Ateliers.Ai.Mcp.Services.Notion

# GitHub API 連携
dotnet add package Ateliers.Ai.Mcp.Services.GitHub

# ローカルファイルシステム操作
dotnet add package Ateliers.Ai.Mcp.Services.LocalFs

# Git 操作 (LibGit2Sharp)
dotnet add package Ateliers.Ai.Mcp.Services.Git

# 音声合成 (VOICEVOX)
dotnet add package Ateliers.Ai.Mcp.Services.Voicevox

# Marp プレゼンテーション生成
dotnet add package Ateliers.Ai.Mcp.Services.Marp

# メディア処理用 FFmpeg サポート
dotnet add package Ateliers.Ai.Mcp.Services.Ffmpeg

# プレゼンテーション動画生成
dotnet add package Ateliers.Ai.Mcp.Tools.PresentationVideo
```

## 機能

- **Services** - 基本インターフェースと設定モデル
- **Notion** - Notion API を使用したタスク、アイデア、読書リストの管理
- **GitHub** - GitHub API を使用したリポジトリファイル操作
- **LocalFs** - ディレクトリ除外機能付きローカルファイルシステム操作
- **Git** - マルチプラットフォーム認証情報サポート付き Git 操作（pull、push、commit、tag）
- **Voicevox** - VOICEVOX エンジンを使用したローカル音声合成
- **Marp** - Marp CLI を使用したプレゼンテーション生成
- **Ffmpeg** - FFmpeg を使用したメディア処理
- **PresentationVideo** - スライドと音声合成からプレゼンテーション動画を生成

## Voicevox サービスの注意事項（Windows）

Voicevox サービスは、公式 VOICEVOX インストーラーでインストールされたネイティブライブラリを使用します。
PATH の変更は不要です。

voicevox_core は色々と制御をする必要があるため、今のところ使用していません。
将来的には VOICEVOX そのものでも、voicevox_core でも動作するようにしたいと考えています。

VOICEVOX フォルダ下の vv-engine フォルダパスをサービスオプションで指定して下さい：
標準的なインストールパス：
```
C:\Program Files\VOICEVOX\vv-engine
```

OpenJTalk 辞書パスは以下の場所で自動検出されます：
```
engine_internal\pyopenjtalk\open_jtalk_dic_utf_8-*
```

初期化時間を短縮するため、読み込む音声モデル（*.vvm）をサービスオプションで制限できます。
指定しない場合、すべての利用可能なモデルが読み込まれます。

## Marp サービスの注意事項
Marp サービスには、Marp CLI のインストールが必要です。

Options クラスで CLI インスト―スパス引数を指定できます：
```
var options = new MarpServiceOptions
{
	MarpExecutablePath = {your_marp_cli_path}
};
```

## FFmpeg サービスの注意事項
FFmpeg サービスには、FFmpeg のインストールが必要です。

Options クラスで FFmpeg インスト―スパス引数を指定できます：
```
var options = new FfmpegServiceOptions
{
	FfmpegExecutablePath = {your_ffmpeg_path}
};
```

## 依存関係

すべてのパッケージは以下に依存しています：
- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ

## Ateliers AI MCP エコシステム

- **Core** - MCPエコシステム全ての基本インターフェースとユーティリティ
- **Services**（このパッケージ）- サービス層実装
- **Tools** - 複数の MCP サービスを組み合わせた MCP タスク単位の実装
- **processes** - 各アプリケーション向けの MCP ツール実行ファイル（.exe）

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - API は変更される可能性があります。安定版 v1.0.0 は近日公開予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
