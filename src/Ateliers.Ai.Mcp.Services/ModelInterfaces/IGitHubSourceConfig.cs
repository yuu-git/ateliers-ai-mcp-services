namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHubソース設定
/// </summary>
public interface IGitHubSourceConfig
{
    /// <summary>
    /// リポジトリOwner
    /// </summary>
    public string Owner { get; }

    /// <summary>
    /// リポジトリ名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// ブランチ名
    /// </summary>
    public string Branch { get; }
}