using Microsoft.Extensions.Caching.Memory;
using Octokit;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.GitHub;

/// <summary>
/// GitHubリポジトリからのファイル取得サービス
/// </summary>
public class GitHubService : McpServiceBase, IGitHubService
{
    private readonly IGitHubClient _client;
    private readonly IGitHubSettings _gitHubSettings;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration;
    private const string LogPrefix = $"{nameof(GitHubService)}:";

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
    public GitHubService(IMcpLogger mcpLogger, IGitHubSettings gitHubSettings, IMemoryCache cache, IGitHubClient gitHubClient)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");

        _gitHubSettings = gitHubSettings;
        _cache = cache;
        _cacheExpiration = TimeSpan.FromMinutes(_gitHubSettings.CacheExpirationMinutes);

        _client = gitHubClient;

        if (_gitHubSettings.AuthenticationMode == "PersonalAccessToken"
            && !string.IsNullOrEmpty(_gitHubSettings.GlobalPersonalAccessToken))
        {
            _client.Connection.Credentials = new Credentials(_gitHubSettings.GlobalPersonalAccessToken);
        }

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// リポジトリキー一覧を取得
    /// </summary>
    public IEnumerable<string> GetRepositoryKeys()
    {
        McpLogger?.Debug($"{LogPrefix} GetRepositoryKeys 開始");
        var keys = _gitHubSettings.GitHubRepositories.Keys;
        McpLogger?.Debug($"{LogPrefix} GetRepositoryKeys 完了: {keys.Count()}件");
        return keys;
    }

    /// <summary>
    /// リポジトリが存在するかどうか
    /// </summary>
    /// <param name="repositoryKey">リポジトリ名称</param>
    /// <returns>存在する場合はtrue、それ以外はfalse</returns>
    public bool RepositoryExists(string repositoryKey)
    {
        McpLogger?.Debug($"{LogPrefix} RepositoryExists 開始: repositoryKey={repositoryKey}");
        var exists = _gitHubSettings.GitHubRepositories.ContainsKey(repositoryKey);
        McpLogger?.Debug($"{LogPrefix} RepositoryExists 完了: repositoryKey={repositoryKey}, exists={exists}");
        return exists;
    }

    /// <summary>
    /// リポジトリサマリを取得
    /// </summary>
    /// <param name="repositoryKey">リポジトリ名称 </param>
    /// <returns> リポジトリ情報、存在しない場合はnull </returns>
    public IGitHubRepositorySummary? GetRepositorySummary(string repositoryKey)
    {
        McpLogger?.Debug($"{LogPrefix} GetRepositorySummary 開始: repositoryKey={repositoryKey}");
        
        if (!_gitHubSettings.GitHubRepositories.TryGetValue(repositoryKey, out var config))
        {
            McpLogger?.Warn($"{LogPrefix} GetRepositorySummary: リポジトリが見つかりません: repositoryKey={repositoryKey}");
            return null;
        }

        var summary = new GitHubRepositorySummary
        {
            Key = repositoryKey,
            Owner = config.GitHubSource?.Owner ?? string.Empty,
            Name = config.GitHubSource?.Name ?? string.Empty,
            Branch = config.GitHubSource?.Branch ?? string.Empty,
            PriorityDataSource = config.PriorityDataSource,
            LocalPath = config.LocalPath ?? string.Empty,
            HasLocalPath = !string.IsNullOrEmpty(config.LocalPath)
        };

        McpLogger?.Debug($"{LogPrefix} GetRepositorySummary 完了: repositoryKey={repositoryKey}, Owner={summary.Owner}, Name={summary.Name}");
        return summary;
    }

    /// <summary>
    /// リポジトリからファイル内容を取得（キャッシュ付き）
    /// </summary>
    /// <param name="repositoryKey">設定ファイル内のリポジトリキー（例: "AteliersAiAssistants"）</param>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>ファイル内容</returns>
    public async Task<string> GetFileContentAsync(string repositoryKey, string filePath)
    {
        McpLogger?.Info($"{LogPrefix} GetFileContentAsync 開始: repositoryKey={repositoryKey}, filePath={filePath}");

        // リポジトリ設定を取得
        if (!_gitHubSettings.GitHubRepositories.TryGetValue(repositoryKey, out var repoSettings))
        {
            var ex = new ArgumentException($"Repository '{repositoryKey}' not found in configuration.");
            McpLogger?.Critical($"{LogPrefix} GetFileContentAsync: リポジトリが設定に見つかりません: repositoryKey={repositoryKey}", ex);
            throw ex;
        }

        if (repoSettings.PriorityDataSource == "Local" && !string.IsNullOrEmpty(repoSettings.LocalPath))
        {
            McpLogger?.Debug($"{LogPrefix} GetFileContentAsync: ローカル優先モード: localPath={repoSettings.LocalPath}");
            try
            {
                var fullPath = Path.Combine(repoSettings.LocalPath, filePath);

                if (File.Exists(fullPath))
                {
                    McpLogger?.Info($"{LogPrefix} GetFileContentAsync: ローカルファイルから取得: fullPath={fullPath}");
                    var content = await File.ReadAllTextAsync(fullPath);
                    McpLogger?.Debug($"{LogPrefix} GetFileContentAsync 完了: サイズ={content.Length}文字");
                    return content;
                }
                else if (repoSettings.GitHubSource != null)
                {
                    McpLogger?.Warn($"{LogPrefix} GetFileContentAsync: ローカルファイルが存在しないためGitHubにフォールバック: fullPath={fullPath}");
                    // ローカルにファイルが存在しない場合はGitHubから取得にフォールバック
                    var content = await GetGitHubFileAsync(
                        repoSettings.GitHubSource.Owner,
                        repoSettings.GitHubSource.Name,
                        filePath,
                        repoSettings.GitHubSource.Branch
                    );
                    McpLogger?.Info($"{LogPrefix} GetFileContentAsync 完了: GitHubフォールバックで取得成功");
                    return content;
                }
                else
                {
                    var ex = new FileNotFoundException($"File not found: {filePath} in local path '{repoSettings.LocalPath}'");
                    McpLogger?.Critical($"{LogPrefix} GetFileContentAsync: ファイルが見つかりません: fullPath={fullPath}", ex);
                    throw ex;
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var wrappedEx = new Exception($"Error accessing file: {ex.Message}", ex);
                McpLogger?.Critical($"{LogPrefix} GetFileContentAsync: ファイルアクセスエラー: {ex.Message}", wrappedEx);
                throw wrappedEx;
            }
        }

        // Local優先でない場合、またはローカルパスが設定されていない場合はGitHubから取得
        if (repoSettings.GitHubSource != null)
        {
            McpLogger?.Debug($"{LogPrefix} GetFileContentAsync: GitHubから取得: owner={repoSettings.GitHubSource.Owner}, repo={repoSettings.GitHubSource.Name}");
            // GitHubから取得（キャッシュ付き）
            var content = await GetGitHubFileAsync(
                repoSettings.GitHubSource.Owner,
                repoSettings.GitHubSource.Name,
                filePath,
                repoSettings.GitHubSource.Branch
            );
            McpLogger?.Info($"{LogPrefix} GetFileContentAsync 完了: GitHubから取得成功");
            return content;
        }

        var invalidOpEx = new InvalidOperationException($"Invalid repository configuration for '{repositoryKey}'.");
        McpLogger?.Critical($"{LogPrefix} GetFileContentAsync: 無効なリポジトリ設定: repositoryKey={repositoryKey}", invalidOpEx);
        throw invalidOpEx;
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
        McpLogger?.Info($"{LogPrefix} ListFilesAsync 開始: repositoryKey={repositoryKey}, directory={directory}, extension={extension}");

        if (!_gitHubSettings.GitHubRepositories.TryGetValue(repositoryKey, out var repoSettings))
        {
            var ex = new ArgumentException($"Repository '{repositoryKey}' not found in configuration.");
            McpLogger?.Critical($"{LogPrefix} ListFilesAsync: リポジトリが設定に見つかりません: repositoryKey={repositoryKey}", ex);
            throw ex;
        }

        if (repoSettings.PriorityDataSource == "Local" && !string.IsNullOrEmpty(repoSettings.LocalPath))
        {
            McpLogger?.Debug($"{LogPrefix} ListFilesAsync: ローカル優先モード: localPath={repoSettings.LocalPath}");
            try
            {
                // ローカルファイル一覧を取得
                var files = await ListLocalFilesAsync(repoSettings.LocalPath, directory, extension);
                McpLogger?.Info($"{LogPrefix} ListFilesAsync 完了: ローカルから{files.Count}件取得");
                return files;
            }
            catch (Exception ex)
            {
                McpLogger?.Warn($"{LogPrefix} ListFilesAsync: ローカルアクセス失敗、GitHubにフォールバック: {ex.Message}");
                try
                {
                    // ローカルにアクセスできない場合はGitHubから取得にフォールバック
                    var files = await ListGitHubFilesAsync(
                        repoSettings.GitHubSource!.Owner,
                        repoSettings.GitHubSource.Name,
                        directory,
                        repoSettings.GitHubSource.Branch,
                        extension
                    );
                    McpLogger?.Info($"{LogPrefix} ListFilesAsync 完了: GitHubフォールバックで{files.Count}件取得");
                    return files;
                }
                catch (Exception fallbackEx)
                {
                    // 両方失敗した場合は例外をスロー
                    var wrappedEx = new Exception($"Error accessing files: {fallbackEx.Message}", fallbackEx);
                    McpLogger?.Critical($"{LogPrefix} ListFilesAsync: 両方のデータソースでエラー: {fallbackEx.Message}", wrappedEx);
                    throw wrappedEx;
                }
            }
        }

        // Local優先でない場合、またはローカルパスが設定されていない場合はGitHubから取得
        if (repoSettings.GitHubSource != null)
        {
            McpLogger?.Debug($"{LogPrefix} ListFilesAsync: GitHubから取得: owner={repoSettings.GitHubSource.Owner}, repo={repoSettings.GitHubSource.Name}");
            var files = await ListGitHubFilesAsync(
                repoSettings.GitHubSource.Owner,
                repoSettings.GitHubSource.Name,
                directory,
                repoSettings.GitHubSource.Branch,
                extension
            );
            McpLogger?.Info($"{LogPrefix} ListFilesAsync 完了: GitHubから{files.Count}件取得");
            return files;
        }

        var invalidOpEx = new InvalidOperationException($"Invalid repository configuration for '{repositoryKey}'.");
        McpLogger?.Critical($"{LogPrefix} ListFilesAsync: 無効なリポジトリ設定: repositoryKey={repositoryKey}", invalidOpEx);
        throw invalidOpEx;
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
        McpLogger?.Debug($"{LogPrefix} GetGitHubFileAsync 開始: owner={owner}, repo={repo}, path={path}, branch={branch}");

        // キャッシュから取得を試みる
        if (_cache.TryGetValue(cacheKey, out string? cachedContent) && cachedContent != null)
        {
            McpLogger?.Debug($"{LogPrefix} GetGitHubFileAsync: キャッシュヒット: cacheKey={cacheKey}");
            return cachedContent;
        }

        McpLogger?.Debug($"{LogPrefix} GetGitHubFileAsync: キャッシュミス、GitHubから取得: cacheKey={cacheKey}");

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
                var ex = new FileNotFoundException($"File not found: {path}");
                McpLogger?.Critical($"{LogPrefix} GetGitHubFileAsync: ファイルが見つかりません: path={path}", ex);
                throw ex;
            }

            var content = contents[0].Content;

            // キャッシュに保存
            _cache.Set(cacheKey, content, _cacheExpiration);
            McpLogger?.Debug($"{LogPrefix} GetGitHubFileAsync 完了: サイズ={content.Length}文字, キャッシュに保存");

            return content;
        }
        catch (NotFoundException)
        {
            var ex = new FileNotFoundException($"File not found: {path} in {owner}/{repo}");
            McpLogger?.Critical($"{LogPrefix} GetGitHubFileAsync: ファイルが見つかりません: {owner}/{repo}/{path}", ex);
            throw ex;
        }
        catch (Exception ex)
        {
            McpLogger?.Critical($"{LogPrefix} GetGitHubFileAsync: GitHub APIエラー: {ex.Message}", ex);
            throw;
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
        McpLogger?.Debug($"{LogPrefix} ListGitHubFilesAsync 開始: owner={owner}, repo={repo}, directory={directory}, branch={branch}, extension={extension}");

        // キャッシュから取得を試みる
        if (_cache.TryGetValue(cacheKey, out List<string>? cachedList) && cachedList != null)
        {
            McpLogger?.Debug($"{LogPrefix} ListGitHubFilesAsync: キャッシュヒット: {cachedList.Count}件");
            return cachedList;
        }

        McpLogger?.Debug($"{LogPrefix} ListGitHubFilesAsync: キャッシュミス、GitHubから取得");

        // GitHubから取得
        var allFiles = new List<string>();

        // 空文字列をnullに正規化（ルートディレクトリの場合）
        var normalizedDirectory = string.IsNullOrEmpty(directory) ? null : directory;

        await CollectFilesRecursivelyAsync(owner, repo, normalizedDirectory, branch, allFiles, extension);

        // キャッシュに保存
        _cache.Set(cacheKey, allFiles, _cacheExpiration);
        McpLogger?.Debug($"{LogPrefix} ListGitHubFilesAsync 完了: {allFiles.Count}件取得、キャッシュに保存");

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
        McpLogger?.Debug($"{LogPrefix} CollectFilesRecursivelyAsync: path={path ?? "(root)"}");

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

            var fileCount = 0;
            var dirCount = 0;

            foreach (var item in contents)
            {
                if (item.Type == ContentType.File)
                {
                    // 拡張子フィルタ
                    if (extension == null || item.Path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(item.Path);
                        fileCount++;
                    }
                }
                else if (item.Type == ContentType.Dir)
                {
                    dirCount++;
                    // ディレクトリの場合は再帰的に探索
                    await CollectFilesRecursivelyAsync(owner, repo, item.Path, branch, files, extension);
                }
            }

            McpLogger?.Debug($"{LogPrefix} CollectFilesRecursivelyAsync 完了: path={path ?? "(root)"}, files={fileCount}, dirs={dirCount}");
        }
        catch (NotFoundException)
        {
            McpLogger?.Warn($"{LogPrefix} CollectFilesRecursivelyAsync: ディレクトリが見つかりません: path={path}");
            // ディレクトリが存在しない場合は無視
        }
        catch (Exception ex)
        {
            McpLogger?.Error($"{LogPrefix} CollectFilesRecursivelyAsync: エラー発生: path={path}, error={ex.Message}", ex);
            throw;
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
        McpLogger?.Debug($"{LogPrefix} ListLocalFilesAsync 開始: basePath={basePath}, directory={directory}, extension={extensionString}");

        var searchPath = string.IsNullOrEmpty(directory)
            ? basePath
            : Path.Combine(basePath, directory);

        if (!Directory.Exists(searchPath))
        {
            McpLogger?.Warn($"{LogPrefix} ListLocalFilesAsync: ディレクトリが存在しません: searchPath={searchPath}");
            return new List<string>();
        }

        var searchPattern = extensionString != null ? $"*{extensionString}" : "*";
        var files = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories)
            .Where(f => !IsExcludedPath(f, basePath, _gitHubSettings.ExcludedDirectories))
            .Select(f => Path.GetRelativePath(basePath, f).Replace("\\", "/"))
            .ToList();

        McpLogger?.Debug($"{LogPrefix} ListLocalFilesAsync 完了: {files.Count}件取得");
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