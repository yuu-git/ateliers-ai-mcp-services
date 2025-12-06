namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// GitHubソース設定
/// </summary>
public class GitHubSourceConfig : IGitHubSourceConfig
{
    /// <summary>リポジトリOwner</summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>リポジトリ名</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ブランチ名</summary>
    public string Branch { get; set; } = "master";
}
