namespace Ateliers.Ai.Mcp.Services.GenericModels;

public class GenericGitSettings : IGitHubSettings, IGitSettings, ILocalFileSystemSettings
{
    /// <summary>
    /// 認証モード（Anonymous or PAT）
    /// </summary>
    /// <remarks> Default: "Anonymous" </remarks>
    public virtual string AuthenticationMode { get; set; } = "Anonymous";

    /// <summary>
    /// Personal Access Token（グローバル設定）
    /// </summary>
    public virtual string? GlobalPersonalAccessToken { get; set; }

    /// <summary>
    /// Git Email（グローバル設定）
    /// </summary>
    public virtual string? GlobalGitEmail { get; set; }

    /// <summary>
    /// Git Username（グローバル設定）
    /// </summary>
    public virtual string? GlobalGitUsername { get; set; }

    /// <summary>
    /// キャッシュ有効期限（分）
    /// </summary>
    /// <remarks> Default: 5分 </remarks>
    public virtual int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// GitHub用リポジトリ設定
    /// </summary>
    /// <remarks> Key: リポジトリ識別子, Value: リポジトリ設定 </remarks>
    public IDictionary<string, IGitHubRepositoryConfig> GitHubRepositories =>
        Repositories.ToDictionary(kvp => kvp.Key, kvp => (IGitHubRepositoryConfig)kvp.Value);

    /// <summary>
    /// Git用リポジトリ設定
    /// </summary>
    /// <remarks> Key: リポジトリ識別子, Value: リポジトリ設定 </remarks>
    public IDictionary<string, IGitRepositoryConfig> GitRepositories =>
        Repositories.ToDictionary(kvp => kvp.Key, kvp => (IGitRepositoryConfig)kvp.Value);

    /// <summary>
    /// リポジトリ設定
    /// </summary>
    /// <remarks> Key: リポジトリ識別子, Value: リポジトリ設定 </remarks>
    public virtual Dictionary<string, GenericRepositoryConfig> Repositories { get; set; } = new();

    /// <summary>
    /// ファイル一覧取得時の除外ディレクトリ
    /// </summary>
    /// <remarks> 初期除外ディレクトリ: bin, obj, node_modules, .git, .vs, .vscode, packages, TestResults, .idea </remarks>
    public virtual IEnumerable<string> ExcludedDirectories { get; set; } = new List<string>()
    {
        "bin",
        "obj",
        "node_modules",
        ".git",
        ".vs",
        ".vscode",
        "packages",
        "TestResults",
        ".idea"
    };

    /// <summary>
    /// Git Identity（Email, Username）を解決（リポジトリ固有 → グローバル → null）
    /// </summary>
    public virtual (string? email, string? username) ResolveGitIdentity(string repositoryKey)
    {
        if (!GitHubRepositories.TryGetValue(repositoryKey, out var repositoryConfig))
            return (null, null);

        var email = repositoryConfig.GitEmail ?? GlobalGitEmail;
        var username = repositoryConfig.GitUsername ?? GlobalGitUsername;

        return (email, username);
    }

    /// <summary>
    /// リポジトリのGitHub Tokenを解決（リポジトリ固有 → グローバル → null）
    /// </summary>
    public virtual string? ResolveToken(string repositoryKey)
    {
        if (!GitHubRepositories.TryGetValue(repositoryKey, out var repositoryConfig))
            return null;

        // 優先順位1: リポジトリ固有Token
        if (!string.IsNullOrEmpty(repositoryConfig.PersonalAccessToken))
            return repositoryConfig.PersonalAccessToken;

        // 優先順位2: グローバルToken
        if (!string.IsNullOrEmpty(GlobalPersonalAccessToken))
            return GlobalPersonalAccessToken;

        return null;
    }
}
