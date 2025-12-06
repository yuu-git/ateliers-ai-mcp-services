namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// リポジトリ設定
/// </summary>
public interface IGitRepositoryConfig
{
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

