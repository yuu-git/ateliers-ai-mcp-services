namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHub リポジトリ個別設定
/// </summary>
public interface IGitHubRepositoryConfig : IGitRepositoryConfig
{
    /// <summary>
    /// 優先データソース
    /// </summary>
    /// <remarks>
    /// Local 指定時はローカルパスから優先取得、それ以外はGitHubからネットワーク経由で取得
    /// </remarks>
    public string PriorityDataSource { get; }

    /// <summary>
    /// GitHub設定
    /// </summary>
    public IGitHubSourceConfig? GitHubSource { get; }

    /// <summary>
    /// リポジトリ固有のGitHub Token（グローバル設定よりも優先）
    /// </summary>
    public string? PersonalAccessToken { get; }
}

