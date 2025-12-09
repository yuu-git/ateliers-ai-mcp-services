namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHubリポジトリ情報インターフェース
/// </summary>
public interface IGitHubRepositoryInfo : IGitRepositoryInfo
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

public interface IGitRepositoryInfo
{
    /// <summary>
    /// リポジトリ名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// ブランチ名
    /// </summary>
    string Branch { get; }

    /// <summary>
    /// ローカルパス
    /// </summary>
    string LocalPath { get; }

    /// <summary>
    /// ローカルパスが設定されているかどうか
    /// </summary>
    bool HasLocalPath { get; }
}