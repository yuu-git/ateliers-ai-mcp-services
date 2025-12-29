# Ateliers.Ai.Mcp.Services.GitHub

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.GitHub.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.GitHub/)

Octokit を使用した GitHub API 連携サービスです。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.GitHub
```

## 機能

- リポジトリからのファイル内容取得
- リポジトリ内のファイル一覧取得（拡張子フィルター対応）
- ローカルファイルシステムとの統合（優先データソース選択）
- メモリキャッシュによるパフォーマンス最適化
- 除外ディレクトリのサポート（bin、obj、node_modules など）
- Personal Access Token による認証

## 前提条件

GitHub API を使用するには、Personal Access Token が必要です。

### Personal Access Token の取得

1. GitHub にログイン
2. Settings > Developer settings > Personal access tokens > Tokens (classic)
3. "Generate new token" をクリック
4. 必要なスコープを選択（`repo` スコープが必要）
5. トークンをコピーして安全に保存

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.GitHub;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Microsoft.Extensions.Caching.Memory;
using Octokit;

// GitHub 設定の作成
var gitHubSettings = new GenericGitHubSettings
{
    AuthenticationMode = "PersonalAccessToken",
    GlobalPersonalAccessToken = "your-github-token",
    CacheExpirationMinutes = 10,
    ExcludedDirectories = new[] { "bin", "obj", "node_modules", ".git" },
    GitHubRepositories = new Dictionary<string, IGitHubRepositoryConfig>
    {
        ["my-repo"] = new GitHubRepositoryConfig
        {
            GitHubSource = new GitHubSourceConfig
            {
                Owner = "owner-name",
                Name = "repo-name",
                Branch = "main"
            },
            PriorityDataSource = "GitHub"
        }
    }
};

// GitHub クライアントの作成
var githubClient = new GitHubClient(new ProductHeaderValue("MyApp"));

// メモリキャッシュの作成
var cache = new MemoryCache(new MemoryCacheOptions());

// サービスの初期化
var gitHubService = new GitHubService(gitHubSettings, cache, githubClient);

// ファイル内容の取得
var content = await gitHubService.GetFileContentAsync("my-repo", "README.md");
Console.WriteLine(content);
```

### ファイル一覧の取得

```csharp
// すべてのファイルを取得
var allFiles = await gitHubService.ListFilesAsync("my-repo");

// 特定のディレクトリ内のファイルを取得
var docsFiles = await gitHubService.ListFilesAsync("my-repo", "docs");

// 拡張子でフィルター（Markdown ファイルのみ）
var markdownFiles = await gitHubService.ListFilesAsync("my-repo", "", ".md");

foreach (var file in markdownFiles)
{
    Console.WriteLine(file);
}
```

### ローカルとリモートの統合

```csharp
var gitHubSettings = new GenericGitHubSettings
{
    // ...その他の設定
    GitHubRepositories = new Dictionary<string, IGitHubRepositoryConfig>
    {
        ["my-repo"] = new GitHubRepositoryConfig
        {
            LocalPath = @"C:\Projects\MyRepository",
            GitHubSource = new GitHubSourceConfig
            {
                Owner = "owner-name",
                Name = "repo-name",
                Branch = "main"
            },
            PriorityDataSource = "Local" // ローカル優先
        }
    }
};

// ローカルに存在する場合はローカルから、なければ GitHub から取得
var content = await gitHubService.GetFileContentAsync("my-repo", "README.md");
```

### リポジトリサマリの取得

```csharp
var summary = gitHubService.GetRepositorySummary("my-repo");

if (summary != null)
{
    Console.WriteLine($"Key: {summary.Key}");
    Console.WriteLine($"Owner: {summary.Owner}");
    Console.WriteLine($"Name: {summary.Name}");
    Console.WriteLine($"Branch: {summary.Branch}");
    Console.WriteLine($"Priority: {summary.PriorityDataSource}");
    Console.WriteLine($"Has Local Path: {summary.HasLocalPath}");
}
```

## 設定

### `IGitHubSettings`

```csharp
public interface IGitHubSettings
{
    string AuthenticationMode { get; }
    string? GlobalPersonalAccessToken { get; }
    int CacheExpirationMinutes { get; }
    IEnumerable<string>? ExcludedDirectories { get; }
    IReadOnlyDictionary<string, IGitHubRepositoryConfig> GitHubRepositories { get; }
}
```

### `IGitHubRepositoryConfig`

```csharp
public interface IGitHubRepositoryConfig
{
    IGitHubSourceConfig? GitHubSource { get; }
    string PriorityDataSource { get; } // "GitHub" または "Local"
    string? LocalPath { get; }
}
```

### `IGitHubSourceConfig`

```csharp
public interface IGitHubSourceConfig
{
    string Owner { get; }  // GitHub のオーナー名
    string Name { get; }   // リポジトリ名
    string Branch { get; } // ブランチ名
}
```

## キャッシュ機能

このサービスは `IMemoryCache` を使用してファイル内容とファイル一覧をキャッシュします：

- **キャッシュキー**: `github:{owner}/{repo}:{branch}:{path}`
- **有効期限**: `CacheExpirationMinutes` で設定（デフォルト: 10分）
- **利点**: GitHub API のレート制限を回避し、パフォーマンスを向上

## 除外ディレクトリ

ローカルファイルシステムを使用する場合、以下のディレクトリはデフォルトで除外されます：

- `bin`
- `obj`
- `node_modules`
- `.git`

カスタム除外ディレクトリを設定することも可能です：

```csharp
var gitHubSettings = new GenericGitHubSettings
{
    ExcludedDirectories = new[] { "bin", "obj", "dist", "build" }
};
```

## 優先データソース

`PriorityDataSource` プロパティで、ローカルファイルシステムと GitHub のどちらを優先するかを設定できます：

- **`"Local"`**: ローカルファイルを優先。存在しない場合は GitHub にフォールバック
- **`"GitHub"`**: 常に GitHub から取得

## エラーハンドリング

```csharp
try
{
    var content = await gitHubService.GetFileContentAsync("my-repo", "non-existent.md");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid repository: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## レート制限

GitHub API にはレート制限があります：

- **認証なし**: 60 requests/hour
- **Personal Access Token**: 5,000 requests/hour

キャッシュ機能を使用することで、レート制限を効果的に管理できます。

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス
- [Octokit](https://www.nuget.org/packages/Octokit/) - GitHub API クライアント
- [Microsoft.Extensions.Caching.Memory](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Memory/) - メモリキャッシュ

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **GitHub**（このパッケージ）- GitHub API 連携サービス

## セキュリティ

- Personal Access Token は環境変数や設定ファイルで管理し、コードにハードコーディングしないでください
- 必要最小限のスコープのみを持つトークンを使用してください
- トークンは定期的にローテーションしてください

## トラブルシューティング

### "Repository not found in configuration" エラー

設定で指定したリポジトリキーが `GitHubRepositories` に存在するか確認してください。

### "File not found" エラー

- ファイルパスが正しいか確認してください
- ブランチ名が正しいか確認してください
- リポジトリにファイルが存在するか確認してください

### レート制限エラー

- Personal Access Token を使用しているか確認してください
- キャッシュが有効になっているか確認してください
- `CacheExpirationMinutes` を適切に設定してください

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
