namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Notion 設定
/// </summary>
public interface INotionSettings
{
    /// <summary>
    /// ノーション接続の API トークン
    /// </summary>
    public string ApiToken { get; }

    /// <summary>
    /// ノーション ワークスペース ID
    /// </summary>
    public string WorkspaceId { get; }

    /// <summary>
    /// ノーション データベース一覧 
    /// </summary>
    /// <remarks> Key: データベース名, Value: データベース ID </remarks>
    public IDictionary<string, string> Databases { get; }
}
