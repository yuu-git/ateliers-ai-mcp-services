using Notion.Client;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion APIとの通信を担当するサービス
/// </summary>
public abstract class NotionServiceBase
{
    protected readonly INotionClient Client;
    protected readonly INotionSettings NotionSettings;

    public NotionServiceBase(INotionSettings notionSettings)
    {
        NotionSettings = notionSettings;

        // Notion APIトークン確認
        if (string.IsNullOrWhiteSpace(notionSettings.ApiToken))
        {
            throw new InvalidOperationException("Notion API Token is not configured.");
        }

        // Notion Clientを初期化
        Client = NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = notionSettings.ApiToken
        });
    }

    /// <summary>
    /// データベースIDを取得
    /// </summary>
    public string GetDatabaseId(string databaseName)
    {
        var databaseId = NotionSettings.Databases[databaseName];
        if (string.IsNullOrWhiteSpace(databaseId))
        {
            throw new InvalidOperationException(
                $"Database ID for '{databaseName}' is not configured. Please set 'Notion:Databases:{databaseName}' in notionsettings.local.json");
        }
        return databaseId;
    }
}
