namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHub リポジトリ個別設定
/// </summary>
public interface IGitHubRepositoryConfig
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
    /// ローカルパス（設定時はローカル優先）
    /// </summary>
    public string? LocalPath { get; }

    /// <summary>
    /// リポジトリ固有のGitHub Token（グローバル設定よりも優先）
    /// </summary>
    public string? PersonalAccessToken { get; }

    /// <summary>
    /// リポジトリ固有のGit Email（グローバル設定よりも優先）
    /// </summary>
    public string? GitEmail { get; }

    /// <summary>
    /// リポジトリ固有のGit Username（グローバル設定よりも優先）
    /// </summary>
    public string? GitUsername { get; }

    /// <summary>
    /// 書き込み前に自動プル
    /// </summary>
    public bool AutoPull { get; }

    /// <summary>
    /// 書き込み後に自動プッシュ
    /// </summary>
    public bool AutoPush { get; }
}

