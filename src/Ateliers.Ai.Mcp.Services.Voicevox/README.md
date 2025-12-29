# Ateliers.Ai.Mcp.Services.Voicevox

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.Voicevox.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Voicevox/)

[VOICEVOX](https://voicevox.hiroshiba.jp/) エンジンを使用したローカル音声合成サービスです。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.Voicevox
```

## 機能

- テキストから音声（WAVファイル）の生成
- 複数のテキストから複数の音声ファイルを一括生成
- 音声モデル（.vvm）の選択的読み込み
- スタイルID指定による話者・感情の変更
- スレッドセーフな音声合成処理

## 前提条件

このサービスを使用するには、[VOICEVOX](https://voicevox.hiroshiba.jp/) のインストールが必要です。

### VOICEVOX のインストール（Windows）

1. [VOICEVOX 公式サイト](https://voicevox.hiroshiba.jp/)から Windows 版をダウンロード
2. インストーラーを実行してインストール
3. 標準インストールパス: `C:\Program Files\VOICEVOX`

### 重要な注意事項

- このサービスは公式 VOICEVOX インストーラーでインストールされた **vv-engine** を使用します
- PATH の変更は不要です
- voicevox_core は現在使用していません（将来的に対応予定）

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.Voicevox;
using Ateliers.Ai.Mcp.Services.GenericModels;

// サービスオプションの設定
var options = new VoicevoxServiceOptions
{
    ResourcePath = @"C:\Program Files\VOICEVOX\vv-engine",
    WorkDirectory = @"C:\temp\voice",
    DefaultStyleId = 1, // 話者スタイル
    VoicevoxOutputDirectoryName = "output"
};

// サービスの初期化
using var service = new VoicevoxService(options);

// 音声ファイルの生成
var request = new GenerateVoiceRequest
{
    Text = "こんにちは。これはテストです。",
    OutputWavFileName = "test.wav"
};

var outputPath = await service.GenerateVoiceFileAsync(request);
Console.WriteLine($"音声ファイルが生成されました: {outputPath}");
```

### 複数の音声ファイルを一括生成

```csharp
var requests = new[]
{
    new GenerateVoiceRequest
    {
        Text = "最初のテキストです。",
        OutputWavFileName = "voice01.wav"
    },
    new GenerateVoiceRequest
    {
        Text = "2番目のテキストです。",
        OutputWavFileName = "voice02.wav"
    },
    new GenerateVoiceRequest
    {
        Text = "最後のテキストです。",
        OutputWavFileName = "voice03.wav"
    }
};

var outputPaths = await service.GenerateVoiceFilesAsync(requests);

foreach (var path in outputPaths)
{
    Console.WriteLine($"生成: {path}");
}
```

### 特定の音声モデルのみを読み込む

初期化時間を短縮するため、必要な音声モデル（*.vvm）のみを読み込むことができます：

```csharp
var options = new VoicevoxServiceOptions
{
    ResourcePath = @"C:\Program Files\VOICEVOX\vv-engine",
    WorkDirectory = @"C:\temp\voice",
    VoiceModelNames = new[] { "0.vvm", "1.vvm" }, // 特定モデルのみ
    DefaultStyleId = 0
};
```

指定しない場合、すべての利用可能なモデルが読み込まれます。

### スタイルIDの指定

話者や感情を変更する場合、スタイルIDを指定します：

```csharp
var request = new GenerateVoiceRequest
{
    Text = "怒った口調で話します。",
    OutputWavFileName = "angry.wav"
};

// スタイルIDを指定（例：怒り）
var outputPath = await service.GenerateVoiceFileAsync(request, styleId: 6);
```

## サービスオプション

### `IVoicevoxServiceOptions`

| プロパティ | 型 | 説明 | デフォルト |
|----------|-----|------|----------|
| `ResourcePath` | `string` | VOICEVOX vv-engine のパス | 必須 |
| `WorkDirectory` | `string` | 作業ディレクトリのベースパス | 必須 |
| `DefaultStyleId` | `uint` | デフォルトの話者スタイルID | `0` |
| `VoiceModelNames` | `IReadOnlyCollection<string>?` | 読み込む音声モデルファイル名（拡張子 .vvm）の配列 | `null`（全モデル読み込み）|
| `VoicevoxOutputDirectoryName` | `string` | 出力ディレクトリ名 | `"voicevox"` |

## リソースパスについて

### 標準的なインストールパス（Windows）

```
C:\Program Files\VOICEVOX\vv-engine
```

### OpenJTalk 辞書の自動検出

OpenJTalk 辞書パスは以下の場所で自動的に検出されます：

```
{ResourcePath}\engine_internal\pyopenjtalk\open_jtalk_dic_utf_8-*
```

手動で指定する必要はありません。

## スタイルID一覧

スタイルIDは使用する音声モデルによって異なります。VOICEVOX アプリケーションで確認できます。

一般的な例：
- `0` - 四国めたん（ノーマル）
- `1` - ずんだもん（ノーマル）
- `2` - 四国めたん（あまあま）
- `3` - ずんだもん（あまあま）

詳細は VOICEVOX の公式ドキュメントを参照してください。

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス
- [VoicevoxCoreSharp.Core](https://www.nuget.org/packages/VoicevoxCoreSharp.Core/) - VOICEVOX Core の C# ラッパー

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **Voicevox**（このパッケージ）- VOICEVOX 音声合成サービス
- **Tools** - 複数の MCP サービスを組み合わせた MCP タスク単位の実装

## トラブルシューティング

### "Marp CLI not found" エラー

`ResourcePath` が正しく設定されているか確認してください。

### "No matching voice models (*.vvm) were found" エラー

`VoiceModelNames` に指定したファイル名が実際に存在するか確認してください。または `VoiceModelNames` を `null` に設定してすべてのモデルを読み込んでください。

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
