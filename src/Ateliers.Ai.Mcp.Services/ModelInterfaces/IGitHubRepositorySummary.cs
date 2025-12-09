namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHubリポジトリ情報インターフェース
/// </summary>
public interface IGitHubRepositorySummary : IGitRepositorySummary
{
    /// <summary>
    /// リポジトリキー
    /// </summary>
    string Key { get; }

    /// <summary>
    /// オーナー名
    /// </summary>
    string Owner { get; }

    /// <summary>
    /// 優先データソース（GitHub or Local）
    /// </summary>
    string PriorityDataSource { get; }
}