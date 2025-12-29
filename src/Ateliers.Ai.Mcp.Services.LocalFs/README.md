# Ateliers.Ai.Mcp.Services.LocalFs

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.LocalFs.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.LocalFs/)

ローカルファイルシステム操作サービスです。除外ディレクトリ機能付き。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.LocalFs
```

## 機能

- ファイルの読み取り
- ファイル一覧の取得（拡張子フィルター対応）
- ファイルの作成
- ファイルの更新（バックアップ機能付き）
- ファイルの削除（バックアップ機能付き）
- ファイルのリネーム
- ファイルのコピー
- ファイルのバックアップ作成
- 除外ディレクトリのサポート（bin、obj、node_modules など）

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.LocalFs;
using Ateliers.Ai.Mcp.Services.GenericModels;

// サービス設定の作成
var settings = new GenericLocalFileSystemSettings
{
    ExcludedDirectories = new[] { "bin", "obj", "node_modules", ".git" }
};

// サービスの初期化
var localFileService = new LocalFileService(settings);

// ベースパスの設定
var basePath = @"C:\Projects\MyProject";

// ファイルの読み取り
var content = await localFileService.ReadFileAsync(basePath, "README.md");
Console.WriteLine(content);
```

### ファイル一覧の取得

```csharp
var basePath = @"C:\Projects\MyProject";

// すべてのファイルを取得
var allFiles = await localFileService.ListFilesAsync(basePath);

// 特定のディレクトリ内のファイルを取得
var srcFiles = await localFileService.ListFilesAsync(basePath, "src");

// 拡張子でフィルター（C# ファイルのみ）
var csFiles = await localFileService.ListFilesAsync(basePath, "", ".cs");

foreach (var file in csFiles)
{
    Console.WriteLine(file);
}
```

### ファイルの作成

```csharp
var basePath = @"C:\Projects\MyProject";

// 新規ファイルを作成
await localFileService.CreateFileAsync(
    basePath, 
    "docs/guide.md", 
    "# Guide\n\nThis is a guide."
);

// ディレクトリが存在しない場合は自動的に作成されます
```

### ファイルの更新

```csharp
var basePath = @"C:\Projects\MyProject";

// バックアップを作成して更新
await localFileService.UpdateFileAsync(
    basePath, 
    "README.md", 
    "# Updated README\n\nNew content.",
    createBackup: true // README.md.backup が作成される
);

// バックアップなしで更新
await localFileService.UpdateFileAsync(
    basePath, 
    "README.md", 
    "# Updated README\n\nNew content.",
    createBackup: false
);
```

### ファイルの削除

```csharp
var basePath = @"C:\Projects\MyProject";

// バックアップを作成して削除
localFileService.DeleteFile(
    basePath, 
    "temp.txt", 
    createBackup: true // temp.txt.backup が作成される
);

// バックアップなしで削除
localFileService.DeleteFile(
    basePath, 
    "temp.txt", 
    createBackup: false
);
```

### ファイルのリネーム

```csharp
var basePath = @"C:\Projects\MyProject";

// ファイルをリネーム（移動も可能）
localFileService.RenameFile(
    basePath, 
    "old-name.txt", 
    "new-name.txt"
);

// ディレクトリを跨いだ移動
localFileService.RenameFile(
    basePath, 
    "docs/old.md", 
    "archive/old.md"
);
```

### ファイルのコピー

```csharp
var basePath = @"C:\Projects\MyProject";

// ファイルをコピー
localFileService.CopyFile(
    basePath, 
    "template.txt", 
    "new-file.txt"
);

// 上書きコピー
localFileService.CopyFile(
    basePath, 
    "source.txt", 
    "destination.txt", 
    overwrite: true
);
```

### ファイルのバックアップ

```csharp
var basePath = @"C:\Projects\MyProject";

// デフォルトのバックアップ（.backup サフィックス）
localFileService.BackupFile(basePath, "important.txt");
// → important.txt.backup

// カスタムサフィックスでバックアップ
localFileService.BackupFile(basePath, "important.txt", "20240101");
// → important.txt.20240101
```

## 設定

### `ILocalFileSystemSettings`

```csharp
public interface ILocalFileSystemSettings
{
    IEnumerable<string>? ExcludedDirectories { get; }
}
```

### 除外ディレクトリ

ファイル一覧取得時に、以下のディレクトリはデフォルトで除外されます：

- `bin`
- `obj`
- `node_modules`
- `.git`

カスタム除外ディレクトリを設定することも可能です：

```csharp
var settings = new GenericLocalFileSystemSettings
{
    ExcludedDirectories = new[] 
    { 
        "bin", 
        "obj", 
        "dist", 
        "build", 
        ".vs",
        "packages"
    }
};
```

除外機能を無効にする場合は、空のコレクションを設定します：

```csharp
var settings = new GenericLocalFileSystemSettings
{
    ExcludedDirectories = Array.Empty<string>()
};
```

## エラーハンドリング

```csharp
try
{
    var content = await localFileService.ReadFileAsync(basePath, "non-existent.txt");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid operation: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## バックアップ機能

このサービスは、ファイルの更新や削除時に自動的にバックアップを作成できます：

### バックアップの命名規則

- **デフォルト**: `{ファイル名}.backup`
- **カスタム**: `{ファイル名}.{カスタムサフィックス}`

### バックアップの注意点

- バックアップファイル（`.backup` で終わるファイル）を削除する際は、さらにバックアップは作成されません
- 上書きコピー時は、既存のバックアップファイルが上書きされます

## セキュリティ

- **パストラバーサル対策**: すべての操作はベースパス内に制限されます
- **存在確認**: ファイル操作前に存在確認を行います
- **ディレクトリ自動作成**: 必要に応じてディレクトリを自動作成します

## ベストプラクティス

### ベースパスの管理

```csharp
// 設定クラスでベースパスを管理
public class ProjectPaths
{
    public const string ProjectRoot = @"C:\Projects\MyProject";
    public const string DocsPath = "docs";
    public const string SrcPath = "src";
}

// 使用時
var files = await localFileService.ListFilesAsync(
    ProjectPaths.ProjectRoot, 
    ProjectPaths.SrcPath, 
    ".cs"
);
```

### バックアップの管理

```csharp
// タイムスタンプ付きバックアップ
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
localFileService.BackupFile(basePath, "config.json", timestamp);

// 定期的なバックアップのクリーンアップ
var backupFiles = await localFileService.ListFilesAsync(basePath, "", ".backup");
foreach (var backup in backupFiles.Where(f => IsOldBackup(f)))
{
    localFileService.DeleteFile(basePath, backup, createBackup: false);
}
```

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **LocalFs**（このパッケージ）- ローカルファイルシステム操作サービス

## トラブルシューティング

### "File not found" エラー

- ファイルパスが正しいか確認してください
- ベースパスとファイルパスの結合が正しいか確認してください

### "File already exists" エラー

- `CreateFileAsync` で既存ファイルを作成しようとしていないか確認してください
- `UpdateFileAsync` を使用して既存ファイルを更新してください

### 除外されたファイルが表示されない

- `ExcludedDirectories` の設定を確認してください
- 除外機能を無効にするには、空のコレクションを設定してください

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
