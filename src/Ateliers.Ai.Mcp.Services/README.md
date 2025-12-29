# Ateliers.Ai.Mcp.Services

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)

Ateliers AI MCP サービス層の基本インターフェースとモデル定義を提供するライブラリです。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services
```

## 概要

このパッケージは、Ateliers AI MCP エコシステムのサービス層における共通のインターフェースとモデルを定義します。具体的なサービス実装は含まれておらず、インターフェース定義とデータモデルのみを提供します。

## 提供される主要なインターフェース

### スライド生成

```csharp
public interface IGenerateSlideService
{
    string GenerateSlideMarkdown(string sourceMarkdown);
    Task<IReadOnlyList<string>> RenderToPngAsync(
        string slideMarkdown,
        CancellationToken cancellationToken = default);
}
```

**実装パッケージ**: [Ateliers.Ai.Mcp.Services.Marp](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Marp/)

### 音声合成

```csharp
public interface IGenerateVoiceService
{
    Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        uint? styleId = null,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
        IEnumerable<IGenerateVoiceRequest> requests,
        uint? styleId = null,
        CancellationToken cancellationToken = default);
}
```

**実装パッケージ**: [Ateliers.Ai.Mcp.Services.Voicevox](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Voicevox/)

### メディア合成

```csharp
public interface IMediaComposerService
{
    Task<string> ComposeAsync(
        GenerateVideoRequest request,
        string outputFileName,
        CancellationToken cancellationToken = default);
}
```

**実装パッケージ**: [Ateliers.Ai.Mcp.Services.Ffmpeg](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Ffmpeg/)

### プレゼンテーション動画生成

```csharp
public interface IPresentationVideoGenerator
{
    Task<PresentationVideoResult> GenerateAsync(
        PresentationVideoRequest request,
        CancellationToken cancellationToken = default);
}
```

**実装パッケージ**: [Ateliers.Ai.Mcp.Services.PresentationVideo](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.PresentationVideo/)

### その他のサービス

- **`ILocalFileService`** - ローカルファイルシステム操作
- **`IGitService`** - Git リポジトリ操作
- **`IGitHubService`** - GitHub API 連携
- **`INotionServices`** - Notion API 連携

## 提供されるモデルとオプション

### 共通オプション

```csharp
public interface IOutputDirectoryProvider
{
    string? OutputRootDirectory { get; }
    string CreateWorkDirectory(string appName, string subDirectory = "");
}
```

### サービスオプション

- **`IMarpServiceOptions`** - Marp サービス設定
- **`IVoicevoxServiceOptions`** - Voicevox サービス設定
- **`IFfmpegServiceOptions`** - FFmpeg サービス設定
- **`IPresentationVideoOptions`** - プレゼンテーション動画サービス設定
- **`IGitSettings`** - Git 設定
- **`IGitHubSettings`** - GitHub 設定
- **`INotionSettings`** - Notion 設定
- **`ILocalFileSystemSettings`** - ローカルファイルシステム設定

### データモデル

- **`GenerateVoiceRequest`** - 音声生成リクエスト
- **`GenerateVideoRequest`** - 動画生成リクエスト
- **`PresentationVideoRequest`** - プレゼンテーション動画生成リクエスト
- **`PresentationVideoResult`** - プレゼンテーション動画生成結果
- **`SlideAudioPair`** - スライドと音声のペア
- **`GitResults`** - Git 操作結果
- **`GitHubDto`** - GitHub データ転送オブジェクト

## 使用方法

このパッケージは単独では使用できません。具体的なサービス実装パッケージと組み合わせて使用します。

### 例：プレゼンテーション動画生成

```csharp
using Ateliers.Ai.Mcp.Services;
using Ateliers.Ai.Mcp.Services.GenericModels;

// インターフェースを使用して疎結合な設計
IGenerateSlideService slideService = new MarpService(marpOptions);
IGenerateVoiceService voiceService = new VoicevoxService(voicevoxOptions);
IMediaComposerService mediaService = new FfmpegService(ffmpegOptions);

// 統合サービスの使用
IPresentationVideoGenerator generator = new PresentationVideoService(
    options,
    voiceService,
    slideService,
    mediaService
);

var request = new PresentationVideoRequest
{
    SourceMarkdown = "# タイトル\n\n内容",
    NarrationTexts = new[] { "ナレーション" },
    OutputFileName = "output.mp4"
};

var result = await generator.GenerateAsync(request);
```

## 実装パッケージ

このインターフェースを実装したパッケージ：

- **[Marp](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Marp/)** - スライド生成
- **[Voicevox](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Voicevox/)** - 音声合成
- **[Ffmpeg](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Ffmpeg/)** - メディア処理
- **[PresentationVideo](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.PresentationVideo/)** - プレゼンテーション動画生成
- **[Git](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Git/)** - Git 操作
- **[GitHub](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.GitHub/)** - GitHub API 連携
- **[Notion](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Notion/)** - Notion API 連携
- **[LocalFs](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.LocalFs/)** - ローカルファイルシステム操作

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **Services**（このパッケージ）- サービス層の基本インターフェースとモデル
- **実装パッケージ** - 各サービスの具体的な実装
- **Tools** - 複数の MCP サービスを組み合わせた MCP タスク単位の実装

## アーキテクチャ

このパッケージはインターフェース分離の原則（ISP）に基づいて設計されています：

- **疎結合**: インターフェースのみに依存することで実装の差し替えが容易
- **拡張性**: 新しいサービス実装を簡単に追加可能
- **テスタビリティ**: インターフェースベースのモック作成が容易

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
