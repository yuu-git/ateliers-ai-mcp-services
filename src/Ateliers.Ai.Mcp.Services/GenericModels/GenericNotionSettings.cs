namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// 汎用的 Notion 設定
/// </summary>
public class GenericNotionSettings : INotionSettings
{
    /// <summary>
    /// ノーション接続の API トークン
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// ノーション ワークスペース ID
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// ノーション データベース一覧 
    /// </summary>
    /// <remarks> Key: データベース名, Value: データベース ID </remarks>
    public IDictionary<string, string> Databases { get; set; } = new Dictionary<string, string>();
}
