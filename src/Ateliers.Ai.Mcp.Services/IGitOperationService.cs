using Ateliers.Ai.Mcp.Services.Models;

namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Git操作サービスのインターフェース
/// </summary>
public interface IGitOperationService
{
    #region 基本Git操作

    /// <summary>
    /// Pull実行（リモートの変更をローカルに取り込む）
    /// </summary>
    Task<IGitPullResult> PullAsync(string repositoryKey, string repoPath);

    /// <summary>
    /// Commit実行（単一ファイル）
    /// </summary>
    Task<IGitCommitResult> CommitAsync(
        string repositoryKey,
        string repoPath,
        string filePath,
        string? customMessage = null);

    /// <summary>
    /// Commit実行（全変更を一括コミット）
    /// </summary>
    Task<IGitCommitResult> CommitAllAsync(
        string repositoryKey,
        string repoPath,
        string? customMessage = null);

    /// <summary>
    /// Push実行（コミット済み変更をリモートにプッシュ）
    /// </summary>
    Task<IGitPushResult> PushAsync(string repositoryKey, string repoPath);

    /// <summary>
    /// Tag作成（軽量タグまたは注釈付きタグ）
    /// </summary>
    Task<IGitTagResult> CreateTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName,
        string? message = null);

    /// <summary>
    /// Tag をリモートにプッシュ
    /// </summary>
    Task<IGitPushResult> PushTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName);

    #endregion

    #region 便利メソッド

    /// <summary>
    /// CommitAndPush実行（単一ファイル）
    /// </summary>
    Task<IGitCommitAndPushResult> CommitAndPushAsync(
        string repositoryKey,
        string repoPath,
        string filePath,
        string? customMessage = null);

    /// <summary>
    /// CommitAndPush実行（全変更を一括）
    /// </summary>
    Task<IGitCommitAndPushResult> CommitAllAndPushAsync(
        string repositoryKey,
        string repoPath,
        string? customMessage = null);

    /// <summary>
    /// CreateAndPushTag実行（タグ作成→プッシュを一括実行）
    /// </summary>
    Task<IGitTagResult> CreateAndPushTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName,
        string? message = null);

    #endregion
}
