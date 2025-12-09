namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHub共通設定
/// </summary>
public interface IGitHubSettings
{
    /// <summary>
    /// 認証モード（Anonymous or PAT）
    /// </summary>
    public string AuthenticationMode { get; }

    /// <summary>
    /// Personal Access Token（グローバル設定）
    /// </summary>
    public string? GlobalPersonalAccessToken { get; }

    /// <summary>
    /// Git Email（グローバル設定）
    /// </summary>
    public string? GlobalGitEmail { get; }

    /// <summary>
    /// Git Username（グローバル設定）
    /// </summary>
    public string? GlobalGitUsername { get; }

    /// <summary>
    /// キャッシュ有効期限（分）
    /// </summary>
    public int CacheExpirationMinutes { get; }

    /// <summary>
    /// リポジトリ設定
    /// </summary>
    /// <remarks>
    /// Key: リポジトリ識別子, Value: リポジトリ設定
    /// </remarks>
    public IDictionary<string, IGitHubRepositoryConfig> GitHubRepositories { get; }

    /// <summary>
    /// ファイル一覧取得時の除外ディレクトリ
    /// </summary>
    public IEnumerable<string> ExcludedDirectories { get; }

    /// <summary>
    /// GitHub Identity を解決（リポジトリ固有 → グローバル → null）
    /// </summary>
    public (string? email, string? username) ResolveGitIdentity(string repositoryKey);

    /// <summary>
    /// リポジトリのGitHub Tokenを解決（リポジトリ固有 → グローバル → null）
    /// </summary>
    public string? ResolveToken(string repositoryKey);
}
