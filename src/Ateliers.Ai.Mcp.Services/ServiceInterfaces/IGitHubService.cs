namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// GitHubサービスのインターフェース
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// リポジトリが存在するかどうか
    /// </summary>
    bool RepositoryExists(string repositoryKey);

    /// <summary>
    /// リポジトリ一覧を取得
    /// </summary>
    IEnumerable<string> GetRepositoryKeys();

    /// <summary>
    /// リポジトリ情報を取得
    /// </summary>
    IGitHubRepositorySummary? GetRepositoryInfo(string repositoryKey);

    /// <summary>
    /// リポジトリからファイル内容を取得（キャッシュ付き）
    /// </summary>
    Task<string> GetFileContentAsync(string repositoryKey, string filePath);

    /// <summary>
    /// リポジトリ内のファイル一覧を取得（キャッシュ付き）
    /// </summary>
    Task<List<string>> ListFilesAsync(
        string repositoryKey,
        string directory = "",
        string? extension = null);
}
