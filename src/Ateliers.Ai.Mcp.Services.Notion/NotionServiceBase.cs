using Notion.Client;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion APIとの通信を担当するサービス
/// </summary>
public abstract class NotionServiceBase : McpServiceBase
{
    protected readonly INotionClient Client;
    protected readonly INotionSettings NotionSettings;
    protected const string LogPrefix = $"{nameof(NotionServiceBase)}:";

    public NotionServiceBase(IMcpLogger mcpLogger, INotionSettings notionSettings)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");

        if (notionSettings == null)
        {
            var ex = new ArgumentNullException(nameof(notionSettings));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗", ex);
            throw ex;
        }

        NotionSettings = notionSettings;

        // Notion APIトークン確認
        if (string.IsNullOrWhiteSpace(notionSettings.ApiToken))
        {
            var ex = new InvalidOperationException("Notion API Token is not configured.");
            McpLogger?.Critical($"{LogPrefix} 初期化失敗: APIトークンが設定されていません", ex);
            throw ex;
        }

        McpLogger?.Debug($"{LogPrefix} Notion Client 初期化中...");
        // Notion Clientを初期化
        Client = NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = notionSettings.ApiToken
        });

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// データベースIDを取得
    /// </summary>
    public string GetDatabaseId(string databaseName)
    {
        McpLogger?.Debug($"{LogPrefix} GetDatabaseId: databaseName={databaseName}");

        var databaseId = NotionSettings.Databases[databaseName];
        if (string.IsNullOrWhiteSpace(databaseId))
        {
            var ex = new InvalidOperationException(
                $"Database ID for '{databaseName}' is not configured. Please set 'Notion:Databases:{databaseName}' in notionsettings.local.json");
            McpLogger?.Critical($"{LogPrefix} GetDatabaseId: データベースIDが設定されていません: databaseName={databaseName}", ex);
            throw ex;
        }

        McpLogger?.Debug($"{LogPrefix} GetDatabaseId: 取得成功: databaseId={databaseId}");
        return databaseId;
    }
}
