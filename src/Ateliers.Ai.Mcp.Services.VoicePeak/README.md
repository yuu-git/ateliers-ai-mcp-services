# Ateliers.Ai.Mcp.Services.VoicePeak

VOICEPEAK エンジンを使用したローカル音声合成サービス（MCPラッパー）

## 概要

`Ateliers.Ai.Mcp.Services.VoicePeak` は、`Ateliers.Voice.Engines.VoicePeakTools` をラップし、MCPプロトコルの `IGenerateVoiceService` インターフェースを実装したサービスです。

## 特徴

- **MCPプロトコル対応**: `IGenerateVoiceService` を実装
- **VOICEPEAK連携**: VoicePeakVoiceGenerator を内部で使用
- **複数ファイル生成**: 一度に複数の音声ファイルを生成可能
- **カスタマイズ可能**: ナレーター、話速などの設定に対応

## 使用方法

```csharp
using Ateliers.Ai.Mcp.Services.VoicePeak;
using Ateliers.Ai.Mcp.Services.GenericModels;

// サービスの初期化
var options = new VoicePeakServiceOptions
{
    VoicePeakExecutablePath = @"C:\Program Files\VOICEPEAK\voicepeak.exe",
    DefaultNarrator = "夏色花梨",
    VoicePeakOutputDirectoryName = "voicepeak"
};

var logger = new InMemoryMcpLogger(new McpLoggerOptions());
var service = new VoicePeakService(logger, options);

// 音声生成リクエスト
var request = new GenerateVoiceRequest
{
    Text = "こんにちは、これはテストです。",
    OutputWavFileName = "output.wav",
    Options = new VoicePeakMcpGenerationOptions
    {
        Narrator = "夏色花梨",
        Speed = 1.2f,
        Happy = 0.5f
    }
};

// 音声ファイル生成
var outputPath = await service.GenerateVoiceFileAsync(request);
Console.WriteLine($"Generated: {outputPath}");
```

## 依存関係

- `Ateliers.Ai.Mcp.Services` - MCPサービスの基盤
- `Ateliers.Voice.Engines.VoicePeakTools` - VOICEPEAK音声エンジン

## ライセンス

MIT License
