# Ateliers.Ai.Mcp.Services.PresentationVideo

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.PresentationVideo.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.PresentationVideo/)

Markdown からプレゼンテーション動画を生成する統合サービスです。スライド生成、音声合成、動画合成の3つのサービスインターフェースを組み合わせて、テキストから完全な動画を自動生成します。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.PresentationVideo
```

## 機能

- Markdown からプレゼンテーション動画への自動変換
- `IGenerateSlideService` によるスライド画像の生成
- `IGenerateVoiceService` によるナレーション音声の生成
- `IMediaComposerService` による動画への統合
- 音声の長さに基づいた自動タイミング調整
- インターフェースベースの疎結合設計による実装の差し替え可能性

## アーキテクチャ

このサービスは以下の3つのインターフェースに依存しています：

- **`IGenerateSlideService`** - スライド画像生成（実装例: Marp）
- **`IGenerateVoiceService`** - 音声合成（実装例: Voicevox）
- **`IMediaComposerService`** - 動画合成（実装例: FFmpeg）

これらのインターフェースを実装した任意のサービスを使用できます。

## 使用方法

### 基本的な使用例

標準的な実装として Marp、Voicevox、FFmpeg を使用する例：

```csharp
using Ateliers.Ai.Mcp.Services.PresentationVideo;
using Ateliers.Ai.Mcp.Services.Marp;
using Ateliers.Ai.Mcp.Services.Voicevox;
using Ateliers.Ai.Mcp.Services.Ffmpeg;
using Ateliers.Ai.Mcp.Services.GenericModels;

// サービスオプションの設定
var options = new PresentationVideoServiceOptions
{
    // 共通
    WorkDirectory = @"C:\temp\presentation",
    
    // Voicevox
    ResourcePath = @"C:\Program Files\VOICEVOX\vv-engine",
    DefaultStyleId = 1,
    VoicevoxOutputDirectoryName = "voice",
    
    // Marp
    MarpExecutablePath = "marp",
    MarpOutputDirectoryName = "slide",
    
    // FFmpeg
    FfmpegExecutablePath = "ffmpeg",
    MediaOutputDirectoryName = "media"
};

// 各サービスの初期化（インターフェースを実装した任意のサービスを使用可能）
IGenerateVoiceService voiceService = new VoicevoxService(options);
IGenerateSlideService slideService = new MarpService(options);
IMediaComposerService mediaService = new FfmpegService(options);

// 統合サービスの初期化
var service = new PresentationVideoService(
    options,
    voiceService,
    slideService,
    mediaService
);

// プレゼンテーション動画の生成
var request = new PresentationVideoRequest
{
    SourceMarkdown = @"
# タイトルスライド

プレゼンテーションへようこそ

# セクション1

最初のトピックについて説明します

# セクション2

次のトピックに移ります

# まとめ

本日のまとめです
",
    NarrationTexts = new[]
    {
        "プレゼンテーションへようこそ。今日は重要なトピックについてお話しします。",
        "最初のトピックについて説明します。これは非常に重要なポイントです。",
        "次のトピックに移ります。ここでは具体的な例を見ていきましょう。",
        "本日のまとめです。ご清聴ありがとうございました。"
    },
    VoiceStyleId = 1,
    OutputFileName = "presentation.mp4"
};

var result = await service.GenerateAsync(request);

Console.WriteLine($"動画が生成されました: {result.VideoPath}");
Console.WriteLine($"スライド数: {result.SlideImages.Count}");
Console.WriteLine($"音声ファイル数: {result.VoiceFiles.Count}");
```

### カスタム実装の使用

独自のスライド生成、音声合成、動画合成サービスを使用することも可能です：

```csharp
// 独自実装のサービスを使用
IGenerateVoiceService customVoiceService = new MyCustomVoiceService();
IGenerateSlideService customSlideService = new MyCustomSlideService();
IMediaComposerService customMediaService = new MyCustomMediaService();

var service = new PresentationVideoService(
    options,
    customVoiceService,
    customSlideService,
    customMediaService
);
```

## サービスオプション

### `PresentationVideoServiceOptions`

このクラスは複数のサービスオプションインターフェースを実装した便利クラスです：
- `IPresentationVideoOptions`
- `IVoicevoxServiceOptions`
- `IMarpServiceOptions`
- `IFfmpegServiceOptions`

標準実装（Marp、Voicevox、FFmpeg）を使用する場合に便利です。

| プロパティ | 型 | 説明 | デフォルト |
|----------|-----|------|----------|
| **共通設定** ||||
| `WorkDirectory` | `string` | 作業ディレクトリのベースパス | 必須 |
| **Voicevox 設定** ||||
| `ResourcePath` | `string` | VOICEVOX vv-engine のパス | 必須 |
| `DefaultStyleId` | `uint` | デフォルトの話者スタイルID | `0` |
| `VoiceModelNames` | `IReadOnlyCollection<string>?` | 読み込む音声モデルファイル名 | `null`（全モデル）|
| `VoicevoxOutputDirectoryName` | `string` | 音声出力ディレクトリ名 | `"voice"` |
| **Marp 設定** ||||
| `MarpExecutablePath` | `string` | Marp CLI の実行可能ファイルパス | `"marp"` |
| `MarpOutputDirectoryName` | `string` | スライド出力ディレクトリ名 | `"slide"` |
| **FFmpeg 設定** ||||
| `FfmpegExecutablePath` | `string` | FFmpeg の実行可能ファイルパス | `"ffmpeg"` |
| `MediaOutputDirectoryName` | `string` | 動画出力ディレクトリ名 | `"media"` |

## データモデル

### `PresentationVideoRequest`

```csharp
public sealed record PresentationVideoRequest
{
    public required string SourceMarkdown { get; init; }           // 元のMarkdown
    public required IReadOnlyList<string> NarrationTexts { get; init; } // 各スライドのナレーション
    public int? VoiceStyleId { get; init; }                        // 音声スタイルID（オプション）
    public string? BackgroundMusicPath { get; init; }              // 背景音楽（オプション）
    public string? OutputFileName { get; init; }                   // 出力ファイル名
}
```

### `PresentationVideoResult`

```csharp
public sealed record PresentationVideoResult
{
    public required string VideoPath { get; init; }                // 生成された動画のパス
    public IReadOnlyList<string> SlideImages { get; init; }       // 生成されたスライド画像のパス
    public IReadOnlyList<string> VoiceFiles { get; init; }        // 生成された音声ファイルのパス
}
```

## 処理フロー

1. **Markdown → スライドMarkdown** - `IGenerateSlideService` による変換
2. **スライドMarkdown → PNG画像** - `IGenerateSlideService` による画像生成
3. **テキスト → 音声ファイル** - `IGenerateVoiceService` による音声合成
4. **画像 + 音声 → 動画** - `IMediaComposerService` による統合

### 重要な制約

- **スライド数とナレーション数は一致する必要があります**
- スライドの区切り方は使用する `IGenerateSlideService` の実装に依存します
- 最低限必要なスライド数は使用する実装に依存します

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービスインターフェース
- [NAudio](https://www.nuget.org/packages/NAudio/) - 音声ファイルの長さ取得

### 推奨パッケージ（標準実装を使用する場合）

標準的な実装として以下のパッケージの使用を推奨：

```bash
dotnet add package Ateliers.Ai.Mcp.Services.Marp      # スライド生成
dotnet add package Ateliers.Ai.Mcp.Services.Voicevox  # 音声合成
dotnet add package Ateliers.Ai.Mcp.Services.Ffmpeg    # 動画合成
```

これらの実装については各パッケージの README を参照してください。

## 必要なインターフェース

このサービスを使用するには、以下のインターフェースを実装したサービスが必要です：

### `IGenerateSlideService`

```csharp
public interface IGenerateSlideService
{
    string GenerateSlideMarkdown(string sourceMarkdown);
    Task<IReadOnlyList<string>> RenderToPngAsync(
        string slideMarkdown,
        CancellationToken cancellationToken = default);
}
```

### `IGenerateVoiceService`

```csharp
public interface IGenerateVoiceService
{
    Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
        IEnumerable<IGenerateVoiceRequest> requests,
        uint? styleId = null,
        CancellationToken cancellationToken = default);
}
```

### `IMediaComposerService`

```csharp
public interface IMediaComposerService
{
    Task<string> ComposeAsync(
        GenerateVideoRequest request,
        string outputFileName,
        CancellationToken cancellationToken = default);
}
```

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **PresentationVideo**（このパッケージ）- プレゼンテーション動画生成統合サービス

### 標準実装パッケージ

- **[Marp](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Marp/)** - `IGenerateSlideService` の実装
- **[Voicevox](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Voicevox/)** - `IGenerateVoiceService` の実装
- **[Ffmpeg](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Ffmpeg/)** - `IMediaComposerService` の実装

## トラブルシューティング

### "Slide count and narration count must match" エラー

生成されたスライド数とナレーションテキストの数が一致しているか確認してください。スライドの分割方法は使用する `IGenerateSlideService` の実装に依存します。

### サービス初期化エラー

使用する具体的な実装（例：Marp、Voicevox、FFmpeg）が正しく設定されているか確認してください。各実装の README を参照してください。

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
