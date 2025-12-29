# Ateliers.Ai.Mcp.Services.Git

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.Git.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Git/)

LibGit2Sharp を使用した Git リポジトリ操作サービスです。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.Git
```

## 機能

- リポジトリ情報の取得（ブランチ、コミット履歴、ステータス、リモート）
- Pull（リモートの変更をローカルに取り込む）
- Commit（単一ファイルまたは全変更）
- Push（コミット済み変更をリモートにプッシュ）
- Tag 作成（軽量タグ・注釈付きタグ）
- Tag のプッシュ
- マルチプラットフォーム認証情報サポート（GitHub、GitLab、Azure DevOps、Bitbucket）

## 前提条件

このサービスは LibGit2Sharp を使用しています。追加のインストールは不要です。

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.Git;
using Ateliers.Ai.Mcp.Services.GenericModels;

// サービス設定の作成
var gitSettings = new GenericGitSettings
{
    GitRepositories = new Dictionary<string, IGitRepositoryConfig>
    {
        ["my-repo"] = new GenericRepositoryConfig
        {
            LocalPath = @"C:\Projects\MyRepository",
            Token = "your-github-token",
            Email = "your-email@example.com",
            Username = "Your Name"
        }
    }
};

// サービスの初期化
var gitService = new GitService(gitSettings);

// リポジトリ情報の取得
var repoInfo = gitService.GetRepositoryInfo("my-repo");
Console.WriteLine($"Repository: {repoInfo.RepositoryName}");
Console.WriteLine($"Current Branch: {repoInfo.CurrentBranch}");
Console.WriteLine($"Is Clean: {repoInfo.IsClean}");
```

### Pull（リモートの変更を取り込む）

```csharp
var repositoryKey = "my-repo";
var repoPath = @"C:\Projects\MyRepository";

var pullResult = await gitService.PullAsync(repositoryKey, repoPath);

if (pullResult.Success)
{
    Console.WriteLine("Pull succeeded");
}
else if (pullResult.HasConflict)
{
    Console.WriteLine("Merge conflict detected!");
    Console.WriteLine(pullResult.Message);
}
else
{
    Console.WriteLine($"Pull failed: {pullResult.Message}");
}
```

### Commit と Push

#### 単一ファイルのコミットとプッシュ

```csharp
var repositoryKey = "my-repo";
var repoPath = @"C:\Projects\MyRepository";
var filePath = "README.md";

// Commit のみ
var commitResult = await gitService.CommitAsync(
    repositoryKey, 
    repoPath, 
    filePath, 
    "Update README.md"
);

if (commitResult.Success)
{
    Console.WriteLine($"Committed: {commitResult.CommitHash}");
    
    // Push
    var pushResult = await gitService.PushAsync(repositoryKey, repoPath);
    Console.WriteLine(pushResult.Message);
}
```

#### 一括コミットとプッシュ

```csharp
// すべての変更をコミット
var commitResult = await gitService.CommitAllAsync(
    repositoryKey, 
    repoPath, 
    "Update multiple files"
);

if (commitResult.Success && commitResult.CommitHash != null)
{
    // Push
    var pushResult = await gitService.PushAsync(repositoryKey, repoPath);
    Console.WriteLine(pushResult.Message);
}
```

#### ワンステップでコミットとプッシュ

```csharp
// 単一ファイル
var result = await gitService.CommitAndPushAsync(
    repositoryKey, 
    repoPath, 
    "README.md", 
    "Update README"
);

// または全変更
var result = await gitService.CommitAllAndPushAsync(
    repositoryKey, 
    repoPath, 
    "Update all files"
);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Message: {result.Message}");
Console.WriteLine($"Commit Hash: {result.CommitHash}");
```

### Tag の作成とプッシュ

```csharp
// 軽量タグの作成
var tagResult = await gitService.CreateTagAsync(
    repositoryKey, 
    repoPath, 
    "v1.0.0"
);

// 注釈付きタグの作成
var tagResult = await gitService.CreateTagAsync(
    repositoryKey, 
    repoPath, 
    "v1.0.0", 
    "Release version 1.0.0"
);

if (tagResult.Success)
{
    // Tag をプッシュ
    var pushResult = await gitService.PushTagAsync(
        repositoryKey, 
        repoPath, 
        "v1.0.0"
    );
    Console.WriteLine(pushResult.Message);
}

// または一括実行
var result = await gitService.CreateAndPushTagAsync(
    repositoryKey, 
    repoPath, 
    "v1.0.0", 
    "Release version 1.0.0"
);
```

### リポジトリサマリの取得

```csharp
var summary = gitService.GetRepositorySummary("my-repo");

if (summary != null)
{
    Console.WriteLine($"Name: {summary.Name}");
    Console.WriteLine($"Branch: {summary.Branch}");
    Console.WriteLine($"Local Path: {summary.LocalPath}");
}
```

## 設定

### `IGitSettings`

```csharp
public interface IGitSettings
{
    IReadOnlyDictionary<string, IGitRepositoryConfig> GitRepositories { get; }
    string? ResolveToken(string repositoryKey);
    (string? email, string? username) ResolveGitIdentity(string repositoryKey);
}
```

### `IGitRepositoryConfig`

```csharp
public interface IGitRepositoryConfig
{
    string LocalPath { get; }
    string? Token { get; }
    string? Email { get; }
    string? Username { get; }
}
```

## 認証情報のサポート

このサービスは以下のプラットフォームの認証方式をサポートしています：

| プラットフォーム | 認証方式 |
|---------------|---------|
| **GitHub** | Personal Access Token（Username としてトークンを使用）|
| **GitLab** | Personal Access Token（Username: "oauth2"）|
| **Azure DevOps** | Personal Access Token（Password としてトークンを使用）|
| **Bitbucket** | App Password（Username: "x-token-auth"）|
| **その他** | Generic（Username: "git"）|

## データモデル

### 結果オブジェクト

- **`GitPullResult`** - Pull 操作の結果
- **`GitCommitResult`** - Commit 操作の結果（CommitHash を含む）
- **`GitPushResult`** - Push 操作の結果
- **`GitTagResult`** - Tag 作成の結果
- **`GitCommitAndPushResult`** - Commit と Push の統合結果

### リポジトリ情報

- **`GitRepositoryInfoDto`** - ブランチ、コミット履歴、ステータス、リモート、タグの詳細情報
- **`GitRepositorySummary`** - 基本的なリポジトリ情報（名前、ブランチ、パス）

## エラーハンドリング

すべての操作は結果オブジェクトを返し、`Success` プロパティで成功/失敗を判定できます：

```csharp
var result = await gitService.PullAsync(repositoryKey, repoPath);

if (!result.Success)
{
    Console.WriteLine($"Operation failed: {result.Message}");
    
    if (result.HasConflict)
    {
        Console.WriteLine("Manual conflict resolution required");
    }
}
```

## セキュリティ

- **リモート URL のマスキング**: `GetRepositoryInfo` メソッドは、デフォルトでリモート URL をマスキングします
- **トークンの安全な保存**: トークンは設定オブジェクトに保存し、コード内にハードコーディングしないでください
- **認証情報の自動選択**: プラットフォームに応じて適切な認証方式を自動的に選択します

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス
- [LibGit2Sharp](https://www.nuget.org/packages/LibGit2Sharp/) - Git 操作ライブラリ

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **Git**（このパッケージ）- Git リポジトリ操作サービス

## トラブルシューティング

### "Git token not configured" エラー

設定で `Token` プロパティが正しく設定されているか確認してください。

### "Git email or username not configured" エラー

Commit や注釈付きタグの作成には `Email` と `Username` の設定が必要です。

### "Merge conflict detected" エラー

手動でコンフリクトを解決する必要があります。結果メッセージに解決手順が含まれています。

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
