# Ateliers.Ai.Mcp.Services.Ffmpeg

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.Ffmpeg.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Ffmpeg/)

[FFmpeg](https://ffmpeg.org/) を使用したメディア処理・動画生成サービスです。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.Ffmpeg
```

## 機能

- 画像と音声ファイルからの動画生成
- 複数のスライド（画像）と音声の結合
- スライドごとの表示時間制御
- FFmpeg の concat デマルチプレクサを使用した高品質な動画合成

## 前提条件

このサービスを使用するには、[FFmpeg](https://ffmpeg.org/) のインストールが必要です。

### FFmpeg のインストール

#### Windows

```bash
# Chocolatey を使用する場合
choco install ffmpeg

# または Scoop を使用する場合
scoop install ffmpeg
```

または [FFmpeg 公式サイト](https://ffmpeg.org/download.html) からバイナリをダウンロードしてインストールします。

#### macOS

```bash
brew install ffmpeg
```

#### Linux

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install ffmpeg

# Fedora
sudo dnf install ffmpeg
```

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.Ffmpeg;
using Ateliers.Ai.Mcp.Services.GenericModels;

// サービスオプションの設定
var options = new FfmpegServiceOptions
{
    FfmpegExecutablePath = "ffmpeg", // または完全なパス
    WorkDirectory = @"C:\temp\videos",
    MediaOutputDirectoryName = "output"
};

// サービスの初期化
var service = new FfmpegService(options);

// スライドと音声のペアを準備
var slides = new List<SlideAudioPair>
{
    new SlideAudioPair(
        ImagePath: @"C:\slides\slide01.png",
        AudioPath: @"C:\audio\narration01.wav",
        DurationSeconds: 5.0
    ),
    new SlideAudioPair(
        ImagePath: @"C:\slides\slide02.png",
        AudioPath: @"C:\audio\narration02.wav",
        DurationSeconds: 8.0
    ),
    new SlideAudioPair(
        ImagePath: @"C:\slides\slide03.png",
        AudioPath: @"C:\audio\narration03.wav",
        DurationSeconds: 6.0
    )
};

// 動画リクエストの作成
var request = new GenerateVideoRequest(
    Slides: slides,
    BackgroundMusicPath: null // オプション
);

// 動画の生成
var outputPath = await service.ComposeAsync(
    request, 
    outputFileName: "presentation.mp4"
);

Console.WriteLine($"動画が生成されました: {outputPath}");
```

### カスタム FFmpeg パスの指定

```csharp
var options = new FfmpegServiceOptions
{
    FfmpegExecutablePath = @"C:\ffmpeg\bin\ffmpeg.exe",
    WorkDirectory = @"C:\Projects\videos",
    MediaOutputDirectoryName = "renders"
};
```

### プレゼンテーション動画の生成

Marp と Voicevox サービスと組み合わせて使用する例：

```csharp
// 1. Marp でスライド画像を生成
var marpService = new MarpService(marpOptions);
var slideMarkdown = marpService.GenerateSlideMarkdown(sourceMarkdown);
var pngFiles = await marpService.RenderToPngAsync(slideMarkdown);

// 2. Voicevox で音声を生成
var voicevoxService = new VoicevoxService(voicevoxOptions);
var audioFiles = await voicevoxService.GenerateVoiceFilesAsync(narrationRequests);

// 3. FFmpeg で動画を合成
var slides = pngFiles.Zip(audioFiles, (img, audio) => 
    new SlideAudioPair(img, audio, DurationSeconds: 5.0)
).ToList();

var request = new GenerateVideoRequest(slides);
var videoPath = await ffmpegService.ComposeAsync(request, "presentation.mp4");
```

## サービスオプション

### `IFfmpegServiceOptions`

| プロパティ | 型 | 説明 | デフォルト |
|----------|-----|------|----------|
| `FfmpegExecutablePath` | `string` | FFmpeg の実行可能ファイルパス | `"ffmpeg"` |
| `WorkDirectory` | `string` | 作業ディレクトリのベースパス | 必須 |
| `MediaOutputDirectoryName` | `string` | 出力ディレクトリ名 | `"media"` |

## データモデル

### `SlideAudioPair`

スライド画像と音声のペアを表します。

```csharp
public record SlideAudioPair(
    string ImagePath,      // 画像ファイルのパス
    string AudioPath,      // 音声ファイルのパス
    double DurationSeconds // スライドの表示時間（秒）
);
```

### `GenerateVideoRequest`

動画生成リクエストを表します。

```csharp
public record GenerateVideoRequest(
    IReadOnlyList<SlideAudioPair> Slides,  // スライドと音声のリスト
    string? BackgroundMusicPath = null     // 背景音楽（オプション）
);
```

## FFmpeg の処理について

このサービスは FFmpeg の concat デマルチプレクサを使用して動画を生成します：

- **画像リスト** - 各スライド画像とその表示時間を含むテキストファイル
- **音声リスト** - 各音声ファイルを含むテキストファイル
- **出力形式** - MP4（H.264、yuv420p ピクセルフォーマット）

### FFmpeg コマンドライン例

```bash
ffmpeg -y -nostdin \
  -f concat -safe 0 -i "images.txt" \
  -f concat -safe 0 -i "audio.txt" \
  -shortest \
  -fps_mode vfr \
  -pix_fmt yuv420p \
  "output.mp4"
```

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **Ffmpeg**（このパッケージ）- FFmpeg メディア処理サービス
- **[Marp](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Marp/)** - プレゼンテーションスライド生成
- **[Voicevox](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Voicevox/)** - 音声合成
- **Tools** - 複数の MCP サービスを組み合わせた MCP タスク単位の実装

## トラブルシューティング

### "FFmpeg failed" エラー

`FfmpegExecutablePath` が正しく設定されているか確認してください。コマンドラインで `ffmpeg -version` が動作するか確認してください。

### 動画が正しく生成されない

- 画像ファイルと音声ファイルが存在することを確認してください
- 少なくとも1つのスライドが必要です
- すべての画像ファイルが同じ解像度であることを推奨します

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。


## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
