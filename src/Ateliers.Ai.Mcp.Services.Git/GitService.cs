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
        McpLogger?.Info($"{LogPrefix} リポジトリキーの一覧取得");

        return _gitSettings.GitRepositories.Keys;
    }

    /// <summary>
    /// リポジトリが存在するかどうか
    /// </summary>
    /// <param name="repositoryKey">リポジトリキー </param>
    /// <returns>存在する場合はtrue、それ以外はfalse</returns>
    public bool RepositoryExists(string repositoryKey)
    {
        McpLogger?.Info($"{LogPrefix} リポジトリの存在確認 {repositoryKey}");

        return _gitSettings.GitRepositories.ContainsKey(repositoryKey);
    }

    /// <summary>
    /// リポジトリサマリを取得
    /// </summary>
    /// <param name="repositoryKey">リポジトリキー </param>
    /// <returns> リポジトリ情報、存在しない場合はnull </returns>
    public IGitRepositorySummary? GetRepositorySummary(string repositoryKey)
    {
        McpLogger?.Info($"{LogPrefix} リポジトリサマリ取得開始 {repositoryKey}");

        if (string.IsNullOrEmpty(repositoryKey))
        {
            var ex = new ArgumentNullException(nameof(repositoryKey), "リポジトリキーが指定されていません。");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }

        if (!_gitSettings.GitRepositories.TryGetValue(repositoryKey, out var config))
            return null;

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

        McpLogger?.Info($"{LogPrefix} リポジトリサマリ取得完了");

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
        McpLogger?.Info($"{LogPrefix} リポジトリ情報取得開始 {repositoryKey}");

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

        using var repo = new Repository(repositoryInfo.LocalPath);

        if (repo == null)
        {
            // リポジトリが無効な場合
            var ex = new InvalidOperationException($"有効なGitリポジトリではありません。パス: {repositoryInfo.LocalPath}");
            McpLogger?.Critical($"{LogPrefix} {ex.Message}", ex);
            throw ex;
        }
        
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

        McpLogger?.Info($"{LogPrefix} リポジトリ情報取得完了");

        return repositoryDto;
    }

    /// <summary>
    /// Pull実行（リモートの変更をローカルに取り込む）
    /// </summary>
    public async Task<GitPullResult> PullAsync(string repositoryKey, string repoPath)
    {
        McpLogger?.Info($"{LogPrefix} Pull実行開始 {repositoryKey}");

        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                McpLogger?.Debug($"{LogPrefix} Tokenチェック開始");
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    McpLogger?.Warn($"{LogPrefix} Gitトークンが設定されていません");
                    return new GitPullResult
                    {
                        Success = false,
                        Message = "Gitトークンが設定されていません - Pullをスキップします"
                    };
                }

                // Git Identityチェック
                McpLogger?.Debug($"{LogPrefix} Git Identityチェック開始");
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    McpLogger?.Warn($"{LogPrefix} GitメールアドレスまたはユーザーIDが設定されていません");
                    return new GitPullResult
                    {
                        Success = false,
                        Message = "GitメールアドレスまたはユーザーIDが設定されていません。"
                    };
                }

                // リポジトリチェック
                McpLogger?.Debug($"{LogPrefix} リポジトリ有効性チェック: {repoPath}");
                if (!Repository.IsValid(repoPath))
                {
                    McpLogger?.Error($"{LogPrefix} 有効なGitリポジトリではありません: {repoPath}");
                    return new GitPullResult
                    {
                        Success = false,
                        Message = $"有効なGitリポジトリではありません: {repoPath}"
                    };
                }

                using var repo = new Repository(repoPath);
                var remoteUrl = repo.Network.Remotes["origin"]?.Url;
                McpLogger?.Debug($"{LogPrefix} リモートURL取得完了");

                // Pull実行
                McpLogger?.Info($"{LogPrefix} Pull実行中...");
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
                McpLogger?.Info($"{LogPrefix} Pullコマンド実行完了: {result.Status}");

                // マージステータス確認
                if (result.Status == MergeStatus.Conflicts)
                {
                    McpLogger?.Warn($"{LogPrefix} マージコンフリクトが検出されました");
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

                McpLogger?.Info($"{LogPrefix} Pull完了");
                return new GitPullResult
                {
                    Success = true,
                    Message = $"Pull完了: {result.Status}"
                };
            }
            catch (Exception ex)
            {
                McpLogger?.Error($"{LogPrefix} Pull失敗: {ex.Message}", ex);
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
        return await Task.Run(() =>
        {
            try
            {
                // Git Identityチェック
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = "Git email or username not configured."
                    };
                }

                // リポジトリチェック
                if (!Repository.IsValid(repoPath))
                {
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = $"Not a valid git repository: {repoPath}"
                    };
                }

                using var repo = new Repository(repoPath);

                // ファイルをステージング
                Commands.Stage(repo, filePath);

                // コミットメッセージ生成
                var message = customMessage ?? $"Update {filePath} via MCP";

                // コミット実行
                var signature = new Signature(username, email, DateTimeOffset.Now);
                var commit = repo.Commit(message, signature, signature);

                return new GitCommitResult
                {
                    Success = true,
                    Message = $"Committed: {commit.MessageShort}",
                    CommitHash = commit.Sha
                };
            }
            catch (Exception ex)
            {
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
        return await Task.Run(() =>
        {
            try
            {
                // Git Identityチェック
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = "Git email or username not configured."
                    };
                }

                // リポジトリチェック
                if (!Repository.IsValid(repoPath))
                {
                    return new GitCommitResult
                    {
                        Success = false,
                        Message = $"Not a valid git repository: {repoPath}"
                    };
                }

                using var repo = new Repository(repoPath);

                // 変更があるかチェック
                var status = repo.RetrieveStatus();
                if (!status.IsDirty)
                {
                    return new GitCommitResult
                    {
                        Success = true,
                        Message = "No changes to commit",
                        CommitHash = null
                    };
                }

                // 全変更をステージング
                Commands.Stage(repo, "*");

                // コミットメッセージ生成
                var message = customMessage ?? "Update files via MCP";

                // コミット実行
                var signature = new Signature(username, email, DateTimeOffset.Now);
                var commit = repo.Commit(message, signature, signature);

                return new GitCommitResult
                {
                    Success = true,
                    Message = $"Committed: {commit.MessageShort}",
                    CommitHash = commit.Sha
                };
            }
            catch (Exception ex)
            {
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
        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = "Git token not configured - skipping push"
                    };
                }

                // リポジトリチェック
                if (!Repository.IsValid(repoPath))
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = $"Not a valid git repository: {repoPath}"
                    };
                }

                using var repo = new Repository(repoPath);

                // リモートブランチ取得
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = "Remote 'origin' not found"
                    };
                }

                var remoteUrl = repo.Network.Remotes["origin"].Url;

                // 現在のブランチ取得
                var branch = repo.Head;

                // Push実行
                var options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) =>
                        GitCredentialsHelper.GetCredentials(remoteUrl ?? url, token)
                };

                repo.Network.Push(branch, options);

                return new GitPushResult
                {
                    Success = true,
                    Message = $"Pushed to {remote.Name}/{branch.FriendlyName}"
                };
            }
            catch (Exception ex)
            {
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
        return await Task.Run(() =>
        {
            try
            {
                // Git Identityチェック（注釈付きタグの場合のみ必要）
                if (!string.IsNullOrEmpty(message))
                {
                    var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                    {
                        return new GitTagResult
                        {
                            Success = false,
                            Message = "Git email or username not configured. (required for annotated tags)"
                        };
                    }

                    // リポジトリチェック
                    if (!Repository.IsValid(repoPath))
                    {
                        return new GitTagResult
                        {
                            Success = false,
                            Message = $"Not a valid git repository: {repoPath}"
                        };
                    }

                    using var repo = new Repository(repoPath);

                    // 注釈付きタグ
                    var signature = new Signature(username, email, DateTimeOffset.Now);
                    var tag = repo.Tags.Add(tagName, repo.Head.Tip, signature, message);

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
                    if (!Repository.IsValid(repoPath))
                    {
                        return new GitTagResult
                        {
                            Success = false,
                            Message = $"Not a valid git repository: {repoPath}"
                        };
                    }

                    using var repo = new Repository(repoPath);
                    var tag = repo.Tags.Add(tagName, repo.Head.Tip);

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
        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = "Git token not configured - skipping tag push"
                    };
                }

                // リポジトリチェック
                if (!Repository.IsValid(repoPath))
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = $"Not a valid git repository: {repoPath}"
                    };
                }

                using var repo = new Repository(repoPath);

                // リモート取得
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = "Remote 'origin' not found"
                    };
                }

                var remoteUrl = repo.Network.Remotes["origin"].Url;

                // タグ存在確認
                var tag = repo.Tags[tagName];
                if (tag == null)
                {
                    return new GitPushResult
                    {
                        Success = false,
                        Message = $"Tag '{tagName}' not found"
                    };
                }

                // Push実行
                var options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) =>
                        GitCredentialsHelper.GetCredentials(remoteUrl ?? url, token)
                };

                repo.Network.Push(remote, $"refs/tags/{tagName}", options);

                return new GitPushResult
                {
                    Success = true,
                    Message = $"Tag pushed: {tagName}"
                };
            }
            catch (Exception ex)
            {
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
        // 1. Commit
        var commitResult = await CommitAsync(repositoryKey, repoPath, filePath, customMessage);
        if (!commitResult.Success)
        {
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Commit failed: {commitResult.Message}"
            };
        }

        // 2. Push
        var pushResult = await PushAsync(repositoryKey, repoPath);
        if (!pushResult.Success)
        {
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Push failed: {pushResult.Message}",
                CommitHash = commitResult.CommitHash
            };
        }

        return new GitCommitAndPushResult
        {
            Success = true,
            Message = "Committed and pushed successfully",
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
        // 1. Commit All
        var commitResult = await CommitAllAsync(repositoryKey, repoPath, customMessage);
        if (!commitResult.Success)
        {
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Commit failed: {commitResult.Message}"
            };
        }

        // 変更がない場合はプッシュしない
        if (commitResult.CommitHash == null)
        {
            return new GitCommitAndPushResult
            {
                Success = true,
                Message = "No changes to push",
                CommitHash = null
            };
        }

        // 2. Push
        var pushResult = await PushAsync(repositoryKey, repoPath);
        if (!pushResult.Success)
        {
            return new GitCommitAndPushResult
            {
                Success = false,
                Message = $"Push failed: {pushResult.Message}",
                CommitHash = commitResult.CommitHash
            };
        }

        return new GitCommitAndPushResult
        {
            Success = true,
            Message = "Committed and pushed successfully",
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
        // 1. Tag作成
        var tagResult = await CreateTagAsync(repositoryKey, repoPath, tagName, message);
        if (!tagResult.Success)
        {
            return tagResult;
        }

        // 2. Tag プッシュ
        var pushResult = await PushTagAsync(repositoryKey, repoPath, tagName);
        if (!pushResult.Success)
        {
            return new GitTagResult
            {
                Success = false,
                Message = $"Tag created but push failed: {pushResult.Message}",
                TagName = tagName
            };
        }

        return new GitTagResult
        {
            Success = true,
            Message = $"Tag created and pushed: {tagName}",
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