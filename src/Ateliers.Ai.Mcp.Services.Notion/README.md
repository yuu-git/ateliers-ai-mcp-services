# Ateliers.Ai.Mcp.Services.Notion

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Ateliers.Ai.Mcp.Services.Notion.svg)](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services.Notion/)

Notion.Net を使用した Notion API 連携サービスです。タスク管理、アイデア管理、リーディングリスト管理を提供します。

## インストール

```bash
dotnet add package Ateliers.Ai.Mcp.Services.Notion
```

## 機能

### タスク管理（Tasks）
- タスクの追加（タイトル、説明、ステータス、優先度、期限、場所、タグ）
- タスクの更新
- タスク一覧の取得（フィルター機能付き）
- タスクの完了

### アイデア管理（Ideas）
- アイデアの追加（タイトル、内容、タグ、リンク）
- アイデアの検索（キーワード、タグフィルター）
- アイデアの更新

### リーディングリスト管理（Reading List）
- リーディングリストへの追加（タイトル、リンク、種類、ステータス、優先度、参考フラグ、タグ）
- リーディングリスト一覧の取得
- ステータスの更新

## 前提条件

Notion API を使用するには、以下が必要です：

### 1. Notion Integration の作成

1. [Notion Integrations](https://www.notion.so/my-integrations) にアクセス
2. "New integration" をクリック
3. 名前を入力し、ワークスペースを選択
4. "Submit" をクリック
5. "Internal Integration Token" をコピー（これが API Token です）

### 2. データベースへのアクセス権限付与

1. Notion で使用するデータベースページを開く
2. 右上の "..." メニューをクリック
3. "Add connections" を選択
4. 作成した Integration を選択

### 3. データベース ID の取得

データベースページの URL から取得：
```
https://www.notion.so/workspace/{database_id}?v=...
```

## 使用方法

### 基本的な使用例

```csharp
using Ateliers.Ai.Mcp.Services.Notion;
using Ateliers.Ai.Mcp.Services.GenericModels;

// Notion 設定の作成
var notionSettings = new GenericNotionSettings
{
    ApiToken = "your-notion-api-token",
    Databases = new Dictionary<string, string>
    {
        ["Tasks"] = "your-tasks-database-id",
        ["Ideas"] = "your-ideas-database-id",
        ["ReadingList"] = "your-reading-list-database-id"
    }
};
```

### タスク管理

```csharp
// タスクサービスの初期化
var tasksService = new NotionTasksService(notionSettings);

// タスクの追加
var result = await tasksService.AddTaskAsync(
    title: "プロジェクトの企画書作成",
    description: "新規プロジェクトの企画書を作成する",
    status: "未着手",
    priority: "高",
    dueDate: DateTime.Now.AddDays(7),
    location: "オフィス",
    tags: new[] { "企画", "重要" },
    registrant: "山田太郎"
);

Console.WriteLine(result);

// タスク一覧の取得
var taskList = await tasksService.ListTasksAsync(
    status: "未着手",
    priority: "高",
    limit: 10
);

Console.WriteLine(taskList);

// タスクの完了
var completeResult = await tasksService.CompleteTaskAsync("task-id");
Console.WriteLine(completeResult);

// タスクの更新
var updateResult = await tasksService.UpdateTaskAsync(
    taskId: "task-id",
    status: "進行中",
    priority: "中"
);
```

### アイデア管理

```csharp
// アイデアサービスの初期化
var ideasService = new NotionIdeasService(notionSettings);

// アイデアの追加
var result = await ideasService.AddIdeaAsync(
    title: "新しいサービスのアイデア",
    content: "AIを活用した自動化ツールの開発",
    tags: new[] { "AI", "自動化", "ツール" },
    link: "https://example.com/reference",
    registrant: "山田太郎"
);

Console.WriteLine(result);

// アイデアの検索
var ideas = await ideasService.SearchIdeasAsync(
    keyword: "AI",
    tags: new[] { "自動化" },
    limit: 10
);

Console.WriteLine(ideas);

// アイデアの更新
var updateResult = await ideasService.UpdateIdeaAsync(
    ideaId: "idea-id",
    status: "検討中",
    tags: new[] { "AI", "自動化", "優先" }
);
```

### リーディングリスト管理

```csharp
// リーディングリストサービスの初期化
var readingListService = new NotionReadingListService(notionSettings);

// リーディングリストへの追加
var result = await readingListService.AddToReadingListAsync(
    title: ".NET 10 の新機能",
    link: "https://docs.microsoft.com/dotnet/",
    type: "技術記事",
    status: "未読",
    priority: "高",
    date: DateTime.Now,
    reference: true,
    tags: new[] { ".NET", "技術" },
    registrant: "山田太郎",
    description: ".NET 10 の主要な新機能について",
    author: "Microsoft"
);

Console.WriteLine(result);

// リーディングリスト一覧の取得
var readingList = await readingListService.ListReadingListAsync(
    status: "未読",
    priority: "高",
    limit: 20
);

Console.WriteLine(readingList);

// ステータスの更新
var updateResult = await readingListService.UpdateReadingListStatusAsync(
    readingListId: "reading-list-id",
    status: "完了",
    completedDate: DateTime.Now
);
```

## 設定

### `INotionSettings`

```csharp
public interface INotionSettings
{
    string ApiToken { get; }
    IReadOnlyDictionary<string, string> Databases { get; }
}
```

### データベース構造

各サービスは以下のデータベースプロパティを想定しています：

#### Tasks データベース
- **Name** (Title) - タスク名
- **Status** (Select) - ステータス（未着手、進行中、完了 など）
- **Priority** (Select) - 優先度（低、中、高）
- **Date** (Date) - 期限
- **Location** (Text) - 場所
- **Tags** (Multi-select) - タグ
- **Registrant** (Select) - 登録者

#### Ideas データベース
- **Name** (Title) - アイデア名
- **Status** (Select) - ステータス（アイデア、検討中、実行中、完了）
- **Tags** (Multi-select) - タグ
- **Link** (URL) - 参考リンク
- **Registrant** (Select) - 登録者

#### Reading List データベース
- **Name** (Title) - タイトル
- **Link** (URL) - リンク
- **Type** (Select) - 種類（記事、書籍、動画 など）
- **Status** (Select) - ステータス（未読、読書中、完了）
- **Priority** (Select) - 優先度（低、中、高）
- **Date** (Date) - 日付
- **Reference** (Checkbox) - 参考資料フラグ
- **Tags** (Multi-select) - タグ
- **Registrant** (Select) - 登録者
- **Description** (Text) - 説明
- **Author** (Text) - 著者
- **CompletedDate** (Date) - 完了日

## エラーハンドリング

```csharp
try
{
    var result = await tasksService.AddTaskAsync("タスク名");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## セキュリティ

- **API Token の管理**: トークンは環境変数や設定ファイルで管理し、コードにハードコーディングしないでください
- **アクセス権限**: Integration には必要最小限の権限のみを付与してください
- **データベース ID**: データベース ID は公開情報ではありませんが、適切に管理してください

## ベストプラクティス

### 設定の外部化

```csharp
// appsettings.json
{
  "Notion": {
    "ApiToken": "your-token",
    "Databases": {
      "Tasks": "database-id-1",
      "Ideas": "database-id-2",
      "ReadingList": "database-id-3"
    }
  }
}

// 使用時
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var notionSettings = new GenericNotionSettings
{
    ApiToken = configuration["Notion:ApiToken"],
    Databases = configuration.GetSection("Notion:Databases")
        .GetChildren()
        .ToDictionary(x => x.Key, x => x.Value)
};
```

### DI コンテナでの登録

```csharp
services.AddSingleton<INotionSettings>(notionSettings);
services.AddScoped<INotionTasksService, NotionTasksService>();
services.AddScoped<INotionIdeasService, NotionIdeasService>();
services.AddScoped<INotionReadingListService, NotionReadingListService>();
```

## 依存関係

- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - コアライブラリ
- [Ateliers.Ai.Mcp.Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/) - 基本サービス
- [Notion.Net](https://www.nuget.org/packages/Notion.Net/) - Notion API クライアント

## Ateliers AI MCP エコシステム

このパッケージは Ateliers AI MCP エコシステムの一部です：

- **[Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/)** - MCP エコシステム全ての基本インターフェースとユーティリティ
- **[Services](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Services/)** - サービス層実装の基本インターフェース
- **Notion**（このパッケージ）- Notion API 連携サービス

## トラブルシューティング

### "Notion API Token is not configured" エラー

設定で `ApiToken` が正しく設定されているか確認してください。

### "Database ID for 'XXX' is not configured" エラー

使用するサービスに対応するデータベース ID が設定されているか確認してください。

### データベースにアクセスできない

- Integration がデータベースに接続されているか確認してください
- API Token が正しいか確認してください
- データベース ID が正しいか確認してください

## ドキュメント

完全なドキュメント、使用例、ガイドについては **[ateliers.dev](https://ateliers.dev)** をご覧ください。

## ステータス

⚠️ **開発版（v0.x.x）** - 破壊的な変更がされる可能性があります。安定版 v1.0.0 は以降は、極端な破壊的変更はしない予定です。

## ライセンス

MIT ライセンス - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
