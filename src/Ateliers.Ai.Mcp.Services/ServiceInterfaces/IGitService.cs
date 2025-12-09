using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Gitサービスのインターフェース
/// </summary>
public interface IGitService
{

    /// <summary>
    /// リポジトリキー一覧を取得
    /// </summary>
    public IEnumerable<string> GetRepositoryKeys();

    /// <summary>
    /// リポジトリが存在するかどうか
    /// </summary>
    /// <param name="repositoryKey">リポジトリキー </param>
    /// <returns>存在する場合はtrue、それ以外はfalse</returns>
    public bool RepositoryExists(string repositoryKey);

    /// <summary>
    /// リポジトリサマリを取得
    /// </summary>
    /// <param name="repositoryKey">リポジトリキー </param>
    /// <returns> リポジトリ情報、存在しない場合はnull </returns>
    public IGitRepositorySummary? GetRepositorySummary(string repositoryKey);

    #region 基本Git操作

    /// <summary>
    /// Pull実行（リモートの変更をローカルに取り込む）
    /// </summary>
    Task<GitPullResult> PullAsync(string repositoryKey, string repoPath);

    /// <summary>
    /// Repository情報取得
    /// </summary>
    /// <param name="repositoryKey"> リポジトリキー </param>
    /// <param name="remoteUrlMasked"> リモートURLをマスクするかどうか </param>
    /// <returns></returns>
    public GitRepositoryInfoDto GetRepositoryInfo(string repositoryKey, bool remoteUrlMasked = true);

    /// <summary>
    /// Commit実行（単一ファイル）
    /// </summary>
    Task<GitCommitResult> CommitAsync(
        string repositoryKey,
        string repoPath,
        string filePath,
        string? customMessage = null);

    /// <summary>
    /// Commit実行（全変更を一括コミット）
    /// </summary>
    Task<GitCommitResult> CommitAllAsync(
        string repositoryKey,
        string repoPath,
        string? customMessage = null);

    /// <summary>
    /// Push実行（コミット済み変更をリモートにプッシュ）
    /// </summary>
    Task<GitPushResult> PushAsync(string repositoryKey, string repoPath);

    /// <summary>
    /// Tag作成（軽量タグまたは注釈付きタグ）
    /// </summary>
    Task<GitTagResult> CreateTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName,
        string? message = null);

    /// <summary>
    /// Tag をリモートにプッシュ
    /// </summary>
    Task<GitPushResult> PushTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName);

    #endregion

    #region 便利メソッド

    /// <summary>
    /// CommitAndPush実行（単一ファイル）
    /// </summary>
    Task<GitCommitAndPushResult> CommitAndPushAsync(
        string repositoryKey,
        string repoPath,
        string filePath,
        string? customMessage = null);

    /// <summary>
    /// CommitAndPush実行（全変更を一括）
    /// </summary>
    Task<GitCommitAndPushResult> CommitAllAndPushAsync(
        string repositoryKey,
        string repoPath,
        string? customMessage = null);

    /// <summary>
    /// CreateAndPushTag実行（タグ作成→プッシュを一括実行）
    /// </summary>
    Task<GitTagResult> CreateAndPushTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName,
        string? message = null);

    #endregion
}
