using Microsoft.Extensions.Caching.Memory;
using Octokit;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.GitHub;

/// <summary>
/// GitHubリポジトリからのファイル取得サービス
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly IGitHubClient _client;
    private readonly IGitHubSettings _gitHubSettings;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gitHubSettings"> GitHub設定 </param>
    /// <param name="cache"> メモリキャッシュ </param>
    /// <param name="gitHubClient"> GitHubクライアント </param>
    /// <param name="gitOperationService"> Git操作サービス（任意） </param>
    /// <remarks>
    /// <para>
    /// Git操作サービスがある場合、ローカルのGit操作を優先する。
    /// </para>
    /// </remarks>
    public GitHubService(IGitHubSettings gitHubSettings, IMemoryCache cache, IGitHubClient gitHubClient)
    {
        _gitHubSettings = gitHubSettings;
        _cache = cache;
        _cacheExpiration = TimeSpan.FromMinutes(_gitHubSettings.CacheExpirationMinutes);

        _client = gitHubClient;

        if (_gitHubSettings.AuthenticationMode == "PersonalAccessToken"
            && !string.IsNullOrEmpty(_gitHubSettings.GlobalPersonalAccessToken))
        {
            _client.Connection.Credentials = new Credentials(_gitHubSettings.GlobalPersonalAccessToken);
        }
    }

    /// <summary>
    /// リポジトリキー一覧を取得
    /// </summary>
    public IEnumerable<string> GetRepositoryKeys()
    {
        return _gitHubSettings.GitHubRepositories.Keys;
    }

    /// <summary>
    /// リポジトリが存在するかどうか
    /// </summary>
    /// <param name="repositoryKey">リポジトリ名称</param>
    /// <returns>存在する場合はtrue、それ以外はfalse</returns>
    public bool RepositoryExists(string repositoryKey)
    {
        return _gitHubSettings.GitHubRepositories.ContainsKey(repositoryKey);
    }

    /// <summary>
    /// リポジトリ情報を取得
    /// </summary>
    /// <param name="repositoryKey">リポジトリ名称 </param>
    /// <returns> リポジトリ情報、存在しない場合はnull </returns>
    public IGitHubRepositorySummary? GetRepositoryInfo(string repositoryKey)
    {
        if (!_gitHubSettings.GitHubRepositories.TryGetValue(repositoryKey, out var config))
            return null;

        return new GitHubRepositorySummary
        {
            Key = repositoryKey,
            Owner = config.GitHubSource?.Owner ?? string.Empty,
            Name = config.GitHubSource?.Name ?? string.Empty,
            Branch = config.GitHubSource?.Branch ?? string.Empty,
            PriorityDataSource = config.PriorityDataSource,
            LocalPath = config.LocalPath ?? string.Empty,
            HasLocalPath = !string.IsNullOrEmpty(config.LocalPath)
        };
    }

    /// <summary>
    /// リポジトリからファイル内容を取得（キャッシュ付き）
    /// </summary>
    /// <param name="repositoryKey">設定ファイル内のリポジトリキー（例: "AteliersAiAssistants"）</param>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>ファイル内容</returns>
    public async Task<string> GetFileContentAsync(string repositoryKey, string filePath)
    {
        // リポジトリ設定を取得
        if (!_gitHubSettings.GitHubRepositories.TryGetValue(repositoryKey, out var repoSettings))
        {
            throw new ArgumentException($"Repository '{repositoryKey}' not found in configuration.");
        }

        if (repoSettings.PriorityDataSource == "Local" && !string.IsNullOrEmpty(repoSettings.LocalPath))
        {
            try
            {
                var fullPath = Path.Combine(repoSettings.LocalPath, filePath);

                if (File.Exists(fullPath))
                {
                    return await File.ReadAllTextAsync(fullPath);
                }
                else if (repoSettings.GitHubSource != null)
                {
                    // ローカルにファイルが存在しない場合はGitHubから取得にフォールバック
                    return await GetGitHubFileAsync(
                        repoSettings.GitHubSource.Owner,
                        repoSettings.GitHubSource.Name,
                        filePath,
                        repoSettings.GitHubSource.Branch
                    );
                }
                else
                {
                    throw new FileNotFoundException($"File not found: {filePath} in local path '{repoSettings.LocalPath}'");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error accessing file: {ex.Message}", ex);
            }
        }

        // Local優先でない場合、またはローカルパスが設定されていない場合はGitHubから取得
        if (repoSettings.GitHubSource != null)
        {
            // GitHubから取得（キャッシュ付き）
            return await GetGitHubFileAsync(
                repoSettings.GitHubSource.Owner,
                repoSettings.GitHubSource.Name,
                filePath,
                repoSettings.GitHubSource.Branch
            );
        }

        throw new InvalidOperationException($"Invalid repository configuration for '{repositoryKey}'.");
    }

    /// <summary>
    /// リポジトリ内のファイル一覧を取得（キャッシュ付き）
    /// </summary>
    /// <param name="repositoryKey">設定ファイル内のリポジトリキー</param>
    /// <param name="directory">ディレクトリパス（省略時はルート）</param>
    /// <param name="extension">拡張子フィルター（例: ".md"、省略時は全て）</param>
    /// <returns>ファイルパスのリスト</returns>
    public async Task<List<string>> ListFilesAsync(
        string repositoryKey,
        string directory = "",
        string? extension = null)
    {
        if (!_gitHubSettings.GitHubRepositories.TryGetValue(repositoryKey, out var repoSettings))
        {
            throw new ArgumentException($"Repository '{repositoryKey}' not found in configuration.");
        }

        if (repoSettings.PriorityDataSource == "Local" && !string.IsNullOrEmpty(repoSettings.LocalPath))
        {
            try
            {
                // ローカルファイル一覧を取得
                return await ListLocalFilesAsync(repoSettings.LocalPath, directory, extension);
            }
            catch (Exception)
            {
                try
                {
                    // ローカルにアクセスできない場合はGitHubから取得にフォールバック
                    return await ListGitHubFilesAsync(
                        repoSettings.GitHubSource!.Owner,
                        repoSettings.GitHubSource.Name,
                        directory,
                        repoSettings.GitHubSource.Branch,
                        extension
                    );
                }
                catch (Exception ex)
                {
                    // 両方失敗した場合は例外をスロー
                    throw new Exception($"Error accessing files: {ex.Message}", ex);
                }
            }
        }

        // Local優先でない場合、またはローカルパスが設定されていない場合はGitHubから取得
        if (repoSettings.GitHubSource != null)
        {
            return await ListGitHubFilesAsync(
                repoSettings.GitHubSource.Owner,
                repoSettings.GitHubSource.Name,
                directory,
                repoSettings.GitHubSource.Branch,
                extension
            );
        }

        throw new InvalidOperationException($"Invalid repository configuration for '{repositoryKey}'.");
    }

    /// <summary>
    /// GitHubからファイル内容を取得（キャッシュ付き）
    /// </summary>
    private async Task<string> GetGitHubFileAsync(
        string owner,
        string repo,
        string path,
        string branch)
    {
        var cacheKey = $"github:{owner}/{repo}:{branch}:{path}";

        // キャッシュから取得を試みる
        if (_cache.TryGetValue(cacheKey, out string? cachedContent) && cachedContent != null)
        {
            return cachedContent;
        }

        // GitHubから取得
        try
        {
            var contents = await _client.Repository.Content.GetAllContentsByRef(
                owner,
                repo,
                path,
                branch
            );

            if (contents.Count == 0)
            {
                throw new FileNotFoundException($"File not found: {path}");
            }

            var content = contents[0].Content;

            // キャッシュに保存
            _cache.Set(cacheKey, content, _cacheExpiration);

            return content;
        }
        catch (NotFoundException)
        {
            throw new FileNotFoundException($"File not found: {path} in {owner}/{repo}");
        }
    }

    /// <summary>
    /// GitHub上のファイル一覧を取得（キャッシュ付き）
    /// </summary>
    private async Task<List<string>> ListGitHubFilesAsync(
        string owner,
        string repo,
        string directory,
        string branch,
        string? extension)
    {
        var cacheKey = $"github:list:{owner}/{repo}:{branch}:{directory}:{extension}";

        // キャッシュから取得を試みる
        if (_cache.TryGetValue(cacheKey, out List<string>? cachedList) && cachedList != null)
        {
            return cachedList;
        }

        // GitHubから取得
        var allFiles = new List<string>();

        // 空文字列をnullに正規化（ルートディレクトリの場合）
        var normalizedDirectory = string.IsNullOrEmpty(directory) ? null : directory;

        await CollectFilesRecursivelyAsync(owner, repo, normalizedDirectory, branch, allFiles, extension);

        // キャッシュに保存
        _cache.Set(cacheKey, allFiles, _cacheExpiration);

        return allFiles;
    }

    /// <summary>
    /// GitHub上のファイルを再帰的に収集
    /// </summary>
    private async Task CollectFilesRecursivelyAsync(
        string owner,
        string repo,
        string? path,
        string branch,
        List<string> files,
        string? extension)
    {
        try
        {
            IReadOnlyList<RepositoryContent> contents;

            // ルートディレクトリの場合はパスを省略
            if (string.IsNullOrEmpty(path))
            {
                contents = await _client.Repository.Content.GetAllContentsByRef(
                    owner,
                    repo,
                    branch
                );
            }
            else
            {
                contents = await _client.Repository.Content.GetAllContentsByRef(
                    owner,
                    repo,
                    path,
                    branch
                );
            }

            foreach (var item in contents)
            {
                if (item.Type == ContentType.File)
                {
                    // 拡張子フィルタ
                    if (extension == null || item.Path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(item.Path);
                    }
                }
                else if (item.Type == ContentType.Dir)
                {
                    // ディレクトリの場合は再帰的に探索
                    await CollectFilesRecursivelyAsync(owner, repo, item.Path, branch, files, extension);
                }
            }
        }
        catch (NotFoundException)
        {
            // ディレクトリが存在しない場合は無視
        }
    }

    /// <summary>
    /// ローカルファイル一覧を取得
    /// </summary>
    private async Task<List<string>> ListLocalFilesAsync(
        string basePath,
        string directory,
        string? extensionString)
    {
        var searchPath = string.IsNullOrEmpty(directory)
            ? basePath
            : Path.Combine(basePath, directory);

        if (!Directory.Exists(searchPath))
        {
            return new List<string>();
        }

        var searchPattern = extensionString != null ? $"*{extensionString}" : "*";
        var files = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories)
            .Where(f => !IsExcludedPath(f, basePath, _gitHubSettings.ExcludedDirectories))
            .Select(f => Path.GetRelativePath(basePath, f).Replace("\\", "/"))
            .ToList();

        return await Task.FromResult(files);
    }

    /// <summary>
    /// 除外対象のパスかどうかを判定
    /// </summary>
    private static bool IsExcludedPath(string filePath, string basePath, IEnumerable<string>? excludedDirectories = null)
    {
        // 除外ディレクトリが空の場合は除外しない
        if (excludedDirectories != null && !excludedDirectories.Any())
        {
            return false;
        }

        // 除外ディレクトリ一覧を設定（nullの場合はデフォルト値を使用）
        var excludedDir = excludedDirectories ?? new List<string>()
        {
          "bin",
          "obj",
          "node_modules",
          ".git"
        };
        var relativePath = Path.GetRelativePath(basePath, filePath);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // パスのいずれかの部分が除外ディレクトリに該当するか
        return pathParts.Any(part => excludedDir.Contains(part, StringComparer.OrdinalIgnoreCase));
    }
}