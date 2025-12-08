namespace Ateliers.Ai.Mcp.Services.GenericModels;

public class GenericRepositoryConfig : IGitHubRepositoryConfig, IGitRepositoryConfig
{
    /// <summary>
    /// 優先データソース（GitHub or Local）
    /// </summary>
    /// <remarks> Default: Local </remarks>
    public string PriorityDataSource { get; set; } = "Local";

    /// <summary>
    /// GitHub設定
    /// </summary>
    public IGitHubSourceConfig? GitHubSource { get; set; }

    /// <summary>
    /// ローカルパス（設定時はローカル優先）
    /// </summary>
    public string? LocalPath { get; set; }

    /// <summary>
    /// リポジトリ固有のGitHub Token（グローバル設定よりも優先）
    /// </summary>
    public string? PersonalAccessToken { get; set; }

    /// <summary>
    /// リポジトリ固有のGit Email（グローバル設定よりも優先）
    /// </summary>
    public string? GitEmail { get; set; }

    /// <summary>
    /// リポジトリ固有のGit Username（グローバル設定よりも優先）
    /// </summary>
    public string? GitUsername { get; set; }

    /// <summary>
    /// 書き込み前に自動プル
    /// </summary>
    public bool AutoPull { get; set; } = false;

    /// <summary>
    /// 書き込み後に自動プッシュ
    /// </summary>
    public bool AutoPush { get; set; } = false;

}
