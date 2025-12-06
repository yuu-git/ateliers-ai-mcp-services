namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Git 共通設定
/// </summary>
public interface IGitSettings
{
    /// <summary>
    /// Git Repositories 設定
    /// </summary>
    /// <remarks>
    /// Key: リポジトリ識別子, Value: リポジトリ設定
    /// </remarks>
    public IDictionary<string, IGitRepositoryConfig> GitRepositories { get; }

    /// <summary>
    /// Git Identity を解決
    /// </summary>
    public (string? email, string? username) ResolveGitIdentity(string repositoryKey);

    /// <summary>
    /// リポジトリの Token 解決
    /// </summary>
    public string? ResolveToken(string repositoryKey);
}