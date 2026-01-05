using LibGit2Sharp;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.Git;

/// <summary>
/// Gitサービス（LibGit2Sharp使用）
/// </summary>
public class GitService : McpServiceBase, IGitService
{
    private readonly IGitSettings _gitSettings;
    private const string LogPrefix = $"{nameof(GitService)}:";

    public GitService(IMcpLogger mcpLogger, IGitSettings gitSettings)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");

        _gitSettings = gitSettings;

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// リポジトリキー一覧を取得
    /// </summary>
    public IEnumerable<string> GetRepositoryKeys()
    {
        McpLogger?.Debug($"{LogPrefix} GetRepositoryKeys 開始");
        var keys = _gitSettings.GitRepositories.Keys;
        McpLogger?.Debug($"{LogPrefix} GetRepositoryKeys 完了: {keys.Count()}件");
        return keys;
    }

    /// <summary>
    /// リポジトリが存在するかどうか
    /// </summary>
    /// <param name="repositoryKey">リポジトリキー </param>
    /// <returns>存在する場合はtrue、それ以外はfalse</returns>
    public bool RepositoryExists(string repositoryKey)
    {
        McpLogger?.Debug($"{LogPrefix} RepositoryExists 開始: repositoryKey={repositoryKey}");
        var exists = _gitSettings.GitRepositories.ContainsKey(repositoryKey);
        McpLogger?.Debug($"{LogPrefix} RepositoryExists 完了: repositoryKey={repositoryKey}, exists={exists}");
        return exists;
    }

    /// <summary>
    /// リポジトリサマリを取得
    /// </summary>
    /// <param name="repositoryKey">リポジトリキー </param>
    /// <returns> リポジトリ情報、存在しない場合はnull </returns>
    public IGitRepositorySummary? GetRepositorySummary(string repositoryKey)
    {
        McpLogger?.Info($"{LogPrefix} GetRepositorySummary 開始: repositoryKey={repositoryKey}");

        if (string.IsNullOrEmpty(repositoryKey))
        {
            var ex = new ArgumentNullException(nameof(repositoryKey), "リポジトリキーが指定されていません。");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }

        if (!_gitSettings.GitRepositories.TryGetValue(repositoryKey, out var config))
        {
            McpLogger?.Warn($"{LogPrefix} GetRepositorySummary: リポジトリが見つかりません: repositoryKey={repositoryKey}");
            return null;
        }

        var repositoryInfo = _gitSettings.GitRepositories.FirstOrDefault(repo => repo.Key == repositoryKey).Value;
        if (repositoryInfo == null)
        {
            // キーに対するリポジトリ情報が見つからない場合
            var ex = new InvalidOperationException($"リポジトリキーが見つかりません。{nameof(repositoryKey)}={repositoryKey}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }
        else if (string.IsNullOrEmpty(repositoryInfo.LocalPath))
        {
            // ローカルパスが設定されていない場合
            var ex = new InvalidOperationException($"ローカルパスが設定されていません。{nameof(repositoryKey)}={repositoryKey}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }

        McpLogger?.Debug($"{LogPrefix} GetRepositorySummary: リポジトリを開きます: localPath={repositoryInfo.LocalPath}");
        using var repo = new Repository(repositoryInfo.LocalPath);

        if (repo == null)
        {
            // リポジトリが無効な場合
            var ex = new InvalidOperationException($"有効なGitリポジトリではありません。パス: {repositoryInfo.LocalPath}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }

        var status = repo.RetrieveStatus();
        var repositorySummary = new GitRepositorySummary
        {
            Name = Path.GetFileName(repo.Info.WorkingDirectory.TrimEnd('/', '\\')),
            Branch = repo.Head.FriendlyName,
            LocalPath = repositoryInfo.LocalPath,
            HasLocalPath = !string.IsNullOrEmpty(repositoryInfo.LocalPath)
        };

        McpLogger?.Info($"{LogPrefix} GetRepositorySummary 完了: name={repositorySummary.Name}, branch={repositorySummary.Branch}");

        return repositorySummary;
    }

    #region 基本Git操作

    /// <summary>
    /// リポジトリ情報取得
    /// </summary>
    /// <param name="repositoryKey"> リポジトリキー </param>
    /// <param name="remoteUrlMasked"> リモートURLをマスクするかどうか (default: true) </param>
    /// <returns> リポジトリ情報 </returns>
    public GitRepositoryInfoDto GetRepositoryInfo(string repositoryKey, bool remoteUrlMasked = true)
    {
        McpLogger?.Info($"{LogPrefix} GetRepositoryInfo 開始: repositoryKey={repositoryKey}, remoteUrlMasked={remoteUrlMasked}");

        if (string.IsNullOrEmpty(repositoryKey))
        {
            var ex = new ArgumentNullException(nameof(repositoryKey), "リポジトリキーが指定されていません。");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }

        var repositoryInfo = _gitSettings.GitRepositories.FirstOrDefault(repo => repo.Key == repositoryKey).Value;
        if (repositoryInfo == null)
        {
            // キーに対するリポジトリ情報が見つからない場合
            var ex = new InvalidOperationException($"リポジトリキーが見つかりません。{nameof(repositoryKey)}={repositoryKey}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }
        else if (string.IsNullOrEmpty(repositoryInfo.LocalPath))
        {
            // ローカルパスが設定されていない場合
            var ex = new InvalidOperationException($"ローカルパスが設定されていません。{nameof(repositoryKey)}={repositoryKey}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }

        McpLogger?.Debug($"{LogPrefix} GetRepositoryInfo: リポジトリを開きます: localPath={repositoryInfo.LocalPath}");
        using var repo = new Repository(repositoryInfo.LocalPath);

        if (repo == null)
        {
            // リポジトリが無効な場合
            var ex = new InvalidOperationException($"有効なGitリポジトリではありません。パス: {repositoryInfo.LocalPath}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }
        
        McpLogger?.Debug($"{LogPrefix} GetRepositoryInfo: ステータス取得中...");
        var status = repo.RetrieveStatus();

        var repositoryDto = new GitRepositoryInfoDto
        {
            RepositoryName = Path.GetFileName(repo.Info.WorkingDirectory.TrimEnd('/', '\\')),
            CurrentBranch = repo.Head.FriendlyName,
            IsClean = !status.IsDirty,

            Branches = repo.Branches
                .Where(b => !b.IsRemote)
                .Select(b => new BranchInfoDto
                {
                    Name = b.FriendlyName,
                    LatestCommitSha = b.Tip?.Sha ?? string.Empty,
                    LatestCommitDate = b.Tip?.Author.When.LocalDateTime ?? DateTime.MinValue
                })
                .ToList(),

            RecentCommits = repo.Commits.Take(20)
                .Select(c => new CommitInfoDto
                {
                    Sha = c.Sha,
                    Message = c.MessageShort,
                    Date = c.Author.When.LocalDateTime
                })
                .ToList(),

            Status = status
                .Select(s => new FileStatusInfoDto
                {
                    FilePath = s.FilePath,
                    State = s.State.ToString()
                })
                .ToList(),

            Remotes = repo.Network.Remotes
                .Select(r => new RemoteInfoDto
                {
                    Name = r.Name,
                    Url = remoteUrlMasked ? MaskRemoteUrl(r.Url) : r.Url
                })
                .ToList(),

            Tags = repo.Tags.Select(t => t.FriendlyName).ToList()
        };

        McpLogger?.Info($"{LogPrefix} GetRepositoryInfo 完了: repo={repositoryDto.RepositoryName}, branch={repositoryDto.CurrentBranch}, isClean={repositoryDto.IsClean}, branches={repositoryDto.Branches.Count}, commits={repositoryDto.RecentCommits.Count}");

        return repositoryDto;
    }

    /// <summary>
    /// Pull実行（リモートの変更をローカルに取り込む）
    /// </summary>
    public async Task<GitPullResult> PullAsync(string repositoryKey, string repoPath)
    {
        McpLogger?.Info($"{LogPrefix} PullAsync 開始: repositoryKey={repositoryKey}, repoPath={repoPath}");

        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                McpLogger?.Debug($"{LogPrefix} PullAsync: Tokenチェック開始");
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    McpLogger?.Warn($"{LogPrefix} PullAsync: Gitトークンが設定されていません - Pullをスキップします");
                    return new GitPullResult
                    {
                        Success = false,
                        Message = "Gitトークンが設定されていません - Pullをスキップします"
                    };
                }

                // Git Identityチェック
                McpLogger?.Debug($"{LogPrefix} PullAsync: Git Identityチェック開始");
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    McpLogger?.Warn($"{LogPrefix} PullAsync: GitメールアドレスまたはユーザーIDが設定されていません");
                    return new GitPullResult
                    {
                        Success = false,
                        Message = "GitメールアドレスまたはユーザーIDが設定されていません。"
                    };
                }

                // リポジトリチェック
                McpLogger?.Debug($"{LogPrefix} PullAsync: リポジトリ有効性チェック: {repoPath}");
                if (!Repository.IsValid(repoPath))
                {
                    var ex = new InvalidOperationException($"有効なGitリポジトリではありません: {repoPath}");
                    McpLogger?.Error($"{LogPrefix} PullAsync: {ex.Message}", ex);
                    return new GitPullResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                using var repo = new Repository(repoPath);
                var remoteUrl = repo.Network.Remotes["origin"]?.Url;
                McpLogger?.Debug($"{LogPrefix} PullAsync: リモートURL取得完了: {MaskRemoteUrl(remoteUrl ?? string.Empty)}");

                // Pull実行
                McpLogger?.Info($"{LogPrefix} PullAsync: Pull実行中...");
                var signature = new Signature(username, email, DateTimeOffset.Now);
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, user, cred) =>
                            GitCredentialsHelper.GetCredentials(remoteUrl ?? url, token)
                    }
                };

                var result = Commands.Pull(repo, signature, options);
                McpLogger?.Info($"{LogPrefix} PullAsync: Pullコマンド実行完了: status={result.Status}");

                // マージステータス確認
                if (result.Status == MergeStatus.Conflicts)
                {
                    McpLogger?.Warn($"{LogPrefix} PullAsync: マージコンフリクトが検出されました");
                    return new GitPullResult
                    {
                        Success = false,
                        HasConflict = true,
                        Message = "マージコンフリクトが検出されました。手動で解決してください:\n" +
                                  "1. リポジトリに移動\n" +
                                  "2. 実行: git status\n" +
                                  "3. コンフリクトを解決\n" +
                                  "4. 実行: git add . && git commit"
                    };
                }

                McpLogger?.Info($"{LogPrefix} PullAsync 完了: status={result.Status}");
                return new GitPullResult
                {
                    Success = true,
                    Message = $"Pull完了: {result.Status}"
                };
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} PullAsync: Pull失敗: {ex.Message}", ex);
                return new GitPullResult
                {
                    Success = false,
                    Message = $"Pull失敗: {ex.Message}"
                };
            }
        });
    }

    /// <summary>
    /// Commit実行（単一ファイル）
    /// </summary>
    public async Task<GitCommitResult> CommitAsync(
        string repositoryKey,
        string repoPath,
        string filePath,
        string? customMessage = null)
    {
        McpLogger?.Info($"{LogPrefix} CommitAsync 開始: repositoryKey={repositoryKey}, repoPath={repoPath}, filePath={filePath}");

        return await Task.Run(() =>
        {
            try
            {
                // Git Identityチェック
                McpLogger?.Debug($"{LogPrefix} CommitAsync: Git Identityチェック開始");
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    McpLogger?.Warn($"{LogPrefix} CommitAsync: Git email or username not configured");
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = "Git email or username not configured."
                    };
                }

                // リポジトリチェック
                McpLogger?.Debug($"{LogPrefix} CommitAsync: リポジトリ有効性チェック: {repoPath}");
                if (!Repository.IsValid(repoPath))
                {
                    var ex = new InvalidOperationException($"Not a valid git repository: {repoPath}");
                    McpLogger?.Error($"{LogPrefix} CommitAsync: {ex.Message}", ex);
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                using var repo = new Repository(repoPath);

                // ファイルをステージング
                McpLogger?.Debug($"{LogPrefix} CommitAsync: ファイルをステージング: {filePath}");
                Commands.Stage(repo, filePath);

                // コミットメッセージ生成
                var message = customMessage ?? $"Update {filePath} via MCP";
                McpLogger?.Debug($"{LogPrefix} CommitAsync: コミットメッセージ: {message}");

                // コミット実行
                McpLogger?.Info($"{LogPrefix} CommitAsync: コミット実行中...");
                var signature = new Signature(username, email, DateTimeOffset.Now);
                var commit = repo.Commit(message, signature, signature);

                McpLogger?.Info($"{LogPrefix} CommitAsync 完了: commitHash={commit.Sha}, message={commit.MessageShort}");
                return new GitCommitResult
                {
                    Success = true,
                    Message = $"Committed: {commit.MessageShort}",
                    CommitHash = commit.Sha
                };
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} CommitAsync: Commit failed: {ex.Message}", ex);
                return new GitCommitResult
                {
                    Success = false,
                    Message = $"Commit failed: {ex.Message}"
                };
            }
        });
    }

    /// <summary>
    /// Commit実行（全変更を一括コミット）
    /// </summary>
    public async Task<GitCommitResult> CommitAllAsync(
        string repositoryKey,
        string repoPath,
        string? customMessage = null)
    {
        McpLogger?.Info($"{LogPrefix} CommitAllAsync 開始: repositoryKey={repositoryKey}, repoPath={repoPath}");

        return await Task.Run(() =>
        {
            try
            {
                // Git Identityチェック
                McpLogger?.Debug($"{LogPrefix} CommitAllAsync: Git Identityチェック開始");
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    McpLogger?.Warn($"{LogPrefix} CommitAllAsync: Git email or username not configured");
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = "Git email or username not configured."
                    };
                }

                // リポジトリチェック
                McpLogger?.Debug($"{LogPrefix} CommitAllAsync: リポジトリ有効性チェック: {repoPath}");
                if (!Repository.IsValid(repoPath))
                {
                    var ex = new InvalidOperationException($"Not a valid git repository: {repoPath}");
                    McpLogger?.Error($"{LogPrefix} CommitAllAsync: {ex.Message}", ex);
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                using var repo = new Repository(repoPath);

                // 変更があるかチェック
                McpLogger?.Debug($"{LogPrefix} CommitAllAsync: ステータス確認中...");
                var status = repo.RetrieveStatus();
                if (!status.IsDirty)
                {
                    McpLogger?.Info($"{LogPrefix} CommitAllAsync: 変更がありません");
                    return new GitCommitResult
                    {
                        Success = true,
                        Message = "No changes to commit",
                        CommitHash = null
                    };
                }

                // 全変更をステージング
                McpLogger?.Debug($"{LogPrefix} CommitAllAsync: 全変更をステージング中...");
                Commands.Stage(repo, "*");

                // コミットメッセージ生成
                var message = customMessage ?? "Update files via MCP";
                McpLogger?.Debug($"{LogPrefix} CommitAllAsync: コミットメッセージ: {message}");

                // コミット実行
                McpLogger?.Info($"{LogPrefix} CommitAllAsync: コミット実行中...");
                var signature = new Signature(username, email, DateTimeOffset.Now);
                var commit = repo.Commit(message, signature, signature);

                McpLogger?.Info($"{LogPrefix} CommitAllAsync 完了: commitHash={commit.Sha}, message={commit.MessageShort}");
                return new GitCommitResult
                {
                    Success = true,
                    Message = $"Committed: {commit.MessageShort}",
                    CommitHash = commit.Sha
                };
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} CommitAllAsync: Commit failed: {ex.Message}", ex);
                return new GitCommitResult
                {
                    Success = false,
                    Message = $"Commit failed: {ex.Message}"
                };
            }
        });
    }

    /// <summary>
    /// Push実行（コミット済み変更をリモートにプッシュ）
    /// </summary>
    public async Task<GitPushResult> PushAsync(string repositoryKey, string repoPath)
    {
        McpLogger?.Info($"{LogPrefix} PushAsync 開始: repositoryKey={repositoryKey}, repoPath={repoPath}");

        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                McpLogger?.Debug($"{LogPrefix} PushAsync: Tokenチェック開始");
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    McpLogger?.Warn($"{LogPrefix} PushAsync: Git token not configured - skipping push");
                    return new GitPushResult
                    {
                        Success = false,
                        Message = "Git token not configured - skipping push"
                    };
                }

                // リポジトリチェック
                McpLogger?.Debug($"{LogPrefix} PushAsync: リポジトリ有効性チェック: {repoPath}");
                if (!Repository.IsValid(repoPath))
                {
                    var ex = new InvalidOperationException($"Not a valid git repository: {repoPath}");
                    McpLogger?.Error($"{LogPrefix} PushAsync: {ex.Message}", ex);
                    return new GitPushResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                using var repo = new Repository(repoPath);

                // リモートブランチ取得
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    var ex = new InvalidOperationException("Remote 'origin' not found");
                    McpLogger?.Error($"{LogPrefix} PushAsync: {ex.Message}", ex);
                    return new GitPushResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                var remoteUrl = repo.Network.Remotes["origin"].Url;
                McpLogger?.Debug($"{LogPrefix} PushAsync: リモートURL: {MaskRemoteUrl(remoteUrl)}");

                // 現在のブランチ取得
                var branch = repo.Head;
                McpLogger?.Debug($"{LogPrefix} PushAsync: 現在のブランチ: {branch.FriendlyName}");

                // Push実行
                McpLogger?.Info($"{LogPrefix} PushAsync: Push実行中...");
                var options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) =>
                        GitCredentialsHelper.GetCredentials(remoteUrl ?? url, token)
                };

                repo.Network.Push(branch, options);

                McpLogger?.Info($"{LogPrefix} PushAsync 完了: remote={remote.Name}, branch={branch.FriendlyName}");
                return new GitPushResult
                {
                    Success = true,
                    Message = $"Pushed to {remote.Name}/{branch.FriendlyName}"
                };
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} PushAsync: Push failed: {ex.Message}", ex);
                return new GitPushResult
                {
                    Success = false,
                    Message = $"Push failed: {ex.Message}"
                };
            }
        });
    }

    /// <summary>
    /// Tag作成（軽量タグまたは注釈付きタグ）
    /// </summary>
    public async Task<GitTagResult> CreateTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName,
        string? message = null)  // null = 軽量タグ、あり = 注釈付きタグ
    {
        McpLogger?.Info($"{LogPrefix} CreateTagAsync 開始: repositoryKey={repositoryKey}, repoPath={repoPath}, tagName={tagName}, hasMessage={!string.IsNullOrEmpty(message)}");

        return await Task.Run(() =>
        {
            try
            {
                // Git Identityチェック（注釈付きタグの場合のみ必要）
                if (!string.IsNullOrEmpty(message))
                {
                    McpLogger?.Debug($"{LogPrefix} CreateTagAsync: Git Identityチェック開始（注釈付きタグ）");
                    var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                    {
                        McpLogger?.Warn($"{LogPrefix} CreateTagAsync: Git email or username not configured");
                        return new GitTagResult
                        {
                            Success = false,
                            Message = "Git email or username not configured. (required for annotated tags)"
                        };
                    }

                    // リポジトリチェック
                    McpLogger?.Debug($"{LogPrefix} CreateTagAsync: リポジトリ有効性チェック: {repoPath}");
                    if (!Repository.IsValid(repoPath))
                    {
                        var ex = new InvalidOperationException($"Not a valid git repository: {repoPath}");
                        McpLogger?.Error($"{LogPrefix} CreateTagAsync: {ex.Message}", ex);
                        return new GitTagResult
                        {
                            Success = false,
                            Message = ex.Message
                        };
                    }

                    using var repo = new Repository(repoPath);

                    // 注釈付きタグ
                    McpLogger?.Info($"{LogPrefix} CreateTagAsync: 注釈付きタグを作成中...");
                    var signature = new Signature(username, email, DateTimeOffset.Now);
                    var tag = repo.Tags.Add(tagName, repo.Head.Tip, signature, message);

                    McpLogger?.Info($"{LogPrefix} CreateTagAsync 完了: tagName={tagName}（注釈付き）");
                    return new GitTagResult
                    {
                        Success = true,
                        Message = $"Tag created: {tagName}",
                        TagName = tagName
                    };
                }
                else
                {
                    // 軽量タグ（Email/Username不要）
                    McpLogger?.Debug($"{LogPrefix} CreateTagAsync: リポジトリ有効性チェック: {repoPath}");
                    if (!Repository.IsValid(repoPath))
                    {
                        var ex = new InvalidOperationException($"Not a valid git repository: {repoPath}");
                        McpLogger?.Error($"{LogPrefix} CreateTagAsync: {ex.Message}", ex);
                        return new GitTagResult
                        {
                            Success = false,
                            Message = ex.Message
                        };
                    }

                    using var repo = new Repository(repoPath);
                    McpLogger?.Info($"{LogPrefix} CreateTagAsync: 軽量タグを作成中...");
                    var tag = repo.Tags.Add(tagName, repo.Head.Tip);

                    McpLogger?.Info($"{LogPrefix} CreateTagAsync 完了: tagName={tagName}（軽量）");
                    return new GitTagResult
                    {
                        Success = true,
                        Message = $"Tag created: {tagName}",
                        TagName = tagName
                    };
                }
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} CreateTagAsync: Tag creation failed: {ex.Message}", ex);
                return new GitTagResult
                {
                    Success = false,
                    Message = $"Tag creation failed: {ex.Message}"
                };
            }
        });
    }

    /// <summary>
    /// Tag をリモートにプッシュ
    /// </summary>
    public async Task<GitPushResult> PushTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName)
    {
        McpLogger?.Info($"{LogPrefix} PushTagAsync 開始: repositoryKey={repositoryKey}, repoPath={repoPath}, tagName={tagName}");

        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                McpLogger?.Debug($"{LogPrefix} PushTagAsync: Tokenチェック開始");
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    McpLogger?.Warn($"{LogPrefix} PushTagAsync: Git token not configured - skipping tag push");
                    return new GitPushResult
                    {
                        Success = false,
                        Message = "Git token not configured - skipping tag push"
                    };
                }

                // リポジトリチェック
                McpLogger?.Debug($"{LogPrefix} PushTagAsync: リポジトリ有効性チェック: {repoPath}");
                if (!Repository.IsValid(repoPath))
                {
                    var ex = new InvalidOperationException($"Not a valid git repository: {repoPath}");
                    McpLogger?.Error($"{LogPrefix} PushTagAsync: {ex.Message}", ex);
                    return new GitPushResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                using var repo = new Repository(repoPath);

                // リモート取得
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    var ex = new InvalidOperationException("Remote 'origin' not found");
                    McpLogger?.Error($"{LogPrefix} PushTagAsync: {ex.Message}", ex);
                    return new GitPushResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                var remoteUrl = repo.Network.Remotes["origin"].Url;
                McpLogger?.Debug($"{LogPrefix} PushTagAsync: リモートURL: {MaskRemoteUrl(remoteUrl)}");

                // タグ存在確認
                var tag = repo.Tags[tagName];
                if (tag == null)
                {
                    var ex = new InvalidOperationException($"Tag '{tagName}' not found");
                    McpLogger?.Error($"{LogPrefix} PushTagAsync: {ex.Message}", ex);
                    return new GitPushResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }

                // Push実行
                McpLogger?.Info($"{LogPrefix} PushTagAsync: タグをPush中...");
                var options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) =>
                        GitCredentialsHelper.GetCredentials(remoteUrl ?? url, token)
                };

                repo.Network.Push(remote, $"refs/tags/{tagName}", options);

                McpLogger?.Info($"{LogPrefix} PushTagAsync 完了: tagName={tagName}");
                return new GitPushResult
                {
                    Success = true,
                    Message = $"Tag pushed: {tagName}"
                };
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} PushTagAsync: Push failed: {ex.Message}", ex);
                return new GitPushResult
                {
                    Success = false,
                    Message = $"Push failed: {ex.Message}"
                };
            }
        });
    }

    #endregion

    #region 便利メソッド

    /// <summary>
    /// CommitAndPush実行（単一ファイル）
    /// </summary>
    public async Task<GitCommitAndPushResult> CommitAndPushAsync(
        string repositoryKey,
        string repoPath,
        string filePath,
        string? customMessage = null)
    {
        McpLogger?.Info($"{LogPrefix} CommitAndPushAsync 開始: repositoryKey={repositoryKey}, filePath={filePath}");

        // 1. Commit
        var commitResult = await CommitAsync(repositoryKey, repoPath, filePath, customMessage);
        if (!commitResult.Success)
        {
            McpLogger?.Error($"{LogPrefix} CommitAndPushAsync: Commit失敗: {commitResult.Message}");
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Commit失敗: {commitResult.Message}"
            };
        }

        // 2. Push
        var pushResult = await PushAsync(repositoryKey, repoPath);
        if (!pushResult.Success)
        {
            McpLogger?.Warn($"{LogPrefix} CommitAndPushAsync: Push失敗: {pushResult.Message}");
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Push失敗: {pushResult.Message}",
                CommitHash = commitResult.CommitHash
            };
        }

        McpLogger?.Info($"{LogPrefix} CommitAndPushAsync 完了: commitHash={commitResult.CommitHash}");
        return new GitCommitAndPushResult
        {
            Success = true,
            Message = "コミットとプッシュが成功しました",
            CommitHash = commitResult.CommitHash
        };
    }

    /// <summary>
    /// CommitAndPush実行（全変更を一括）
    /// </summary>
    public async Task<GitCommitAndPushResult> CommitAllAndPushAsync(
        string repositoryKey,
        string repoPath,
        string? customMessage = null)
    {
        McpLogger?.Info($"{LogPrefix} CommitAllAndPushAsync 開始: repositoryKey={repositoryKey}");

        // 1. Commit All
        var commitResult = await CommitAllAsync(repositoryKey, repoPath, customMessage);
        if (!commitResult.Success)
        {
            McpLogger?.Error($"{LogPrefix} CommitAllAndPushAsync: Commit失敗: {commitResult.Message}");
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Commit失敗: {commitResult.Message}"
            };
        }

        // 変更がない場合はプッシュしない
        if (commitResult.CommitHash == null)
        {
            McpLogger?.Info($"{LogPrefix} CommitAllAndPushAsync: 変更がないためPushをスキップ");
            return new GitCommitAndPushResult
            {
                Success = true,
                Message = "プッシュする変更はありません",
                CommitHash = null
            };
        }

        // 2. Push
        var pushResult = await PushAsync(repositoryKey, repoPath);
        if (!pushResult.Success)
        {
            McpLogger?.Warn($"{LogPrefix} CommitAllAndPushAsync: Push失敗: {pushResult.Message}");
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Push失敗: {pushResult.Message}",
                CommitHash = commitResult.CommitHash
            };
        }

        McpLogger?.Info($"{LogPrefix} CommitAllAndPushAsync 完了: commitHash={commitResult.CommitHash}");
        return new GitCommitAndPushResult
        {
            Success = true,
            Message = "コミットとプッシュが成功しました",
            CommitHash = commitResult.CommitHash
        };
    }

    /// <summary>
    /// CreateAndPushTag実行（タグ作成→プッシュを一括実行）
    /// </summary>
    public async Task<GitTagResult> CreateAndPushTagAsync(
        string repositoryKey,
        string repoPath,
        string tagName,
        string? message = null)
    {
        McpLogger?.Info($"{LogPrefix} CreateAndPushTagAsync 開始: repositoryKey={repositoryKey}, tagName={tagName}");

        // 1. Tag作成
        var tagResult = await CreateTagAsync(repositoryKey, repoPath, tagName, message);
        if (!tagResult.Success)
        {
            McpLogger?.Error($"{LogPrefix} CreateAndPushTagAsync: タグ作成失敗: {tagResult.Message}");
            return tagResult;
        }

        // 2. Tag プッシュ
        var pushResult = await PushTagAsync(repositoryKey, repoPath, tagName);
        if (!pushResult.Success)
        {
            McpLogger?.Warn($"{LogPrefix} CreateAndPushTagAsync: タグ作成後のプッシュ失敗: {pushResult.Message}");
            return new GitTagResult
            {
                Success = false,
                Message = $"タグ作成後のプッシュ失敗: {pushResult.Message}",
                TagName = tagName
            };
        }

        McpLogger?.Info($"{LogPrefix} CreateAndPushTagAsync 完了: tagName={tagName}");
        return new GitTagResult
        {
            Success = true,
            Message = $"タグが作成され、プッシュされました: {tagName}",
            TagName = tagName
        };
    }

    #endregion

    /// <summary>
    /// リモートURLのマスキング
    /// </summary>
    /// <param name="url"> リモートURL </param>
    /// <returns> マスク済みURL </returns>
    /// <remarks>
    /// GitHub/GitLab などのドメインのみを残し、ユーザー名・リポジトリ名などの秘匿情報をマスクする。
    /// ローカルパスは常に "(local path)" を返す。
    /// </remarks>
    /// <example>
    /// MaskRemoteUrl("https://github.com/yuu-git/ateliers-ai-mcp-service");
    /// => "github.com/..."
    /// 
    /// MaskRemoteUrl("git@github.com:yuu-git/repo.git");
    /// => "github.com/..."
    /// 
    /// MaskRemoteUrl("https://gitlab.com/group/repo");
    /// => "gitlab.com/..."
    /// 
    /// MaskRemoteUrl(@"C:\Repos\PrivateRepo");
    /// => "(local path)"
    /// 
    /// MaskRemoteUrl("file:///home/user/repos/test");
    /// => "(local path)"
    ///
    /// MaskRemoteUrl("something-strange");
    /// => "(masked)"
    /// </example>
    private string MaskRemoteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // ローカルパス or file://
        if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ||
            Path.IsPathRooted(url))
        {
            return "(local path)";
        }

        try
        {
            // HTTPS 形式の場合
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return $"{uri.Host}/...";
            }
        }
        catch
        {
            // 無視して次の処理へ
        }

        // SSH 形式: git@github.com:user/repo.git
        // パターン: ユーザー名@ホスト名:
        var sshPattern = @"^[\w\-]+@([\w\.\-]+):";
        var match = System.Text.RegularExpressions.Regex.Match(url, sshPattern);
        if (match.Success)
        {
            return $"{match.Groups[1].Value}/...";
        }

        // 最終fallback: ドメインだけ抽出（雑だけど安全）
        var domainMatch = System.Text.RegularExpressions.Regex.Match(url, @"([\w\-]+\.[\w\.\-]+)");
        if (domainMatch.Success)
        {
            return $"{domainMatch.Groups[1].Value}/...";
        }

        return "(masked)";
    }

}