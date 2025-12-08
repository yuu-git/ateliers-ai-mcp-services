using LibGit2Sharp;
using Ateliers.Ai.Mcp.Services;
using Ateliers.Ai.Mcp.Services.Models;

namespace Ateliers.Ai.Mcp.Services.Git;

/// <summary>
/// Git操作サービス（LibGit2Sharp使用）
/// </summary>
public class GitOperationService : IGitOperationService
{
    private readonly IGitSettings _gitSettings;

    public GitOperationService(IGitSettings gitSettings)
    {
        _gitSettings = gitSettings;
    }

    #region 基本Git操作

    /// <summary>
    /// Pull実行（リモートの変更をローカルに取り込む）
    /// </summary>
    public async Task<IGitPullResult> PullAsync(string repositoryKey, string repoPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Tokenチェック
                var token = _gitSettings.ResolveToken(repositoryKey);
                if (token == null)
                {
                    return new GitPullResult
                    {
                        Success = false,
                        Message = "Git token not configured - skipping pull"
                    };
                }

                // Git Identityチェック
                var (email, username) = _gitSettings.ResolveGitIdentity(repositoryKey);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
                {
                    return new GitPullResult
                    {
                        Success = false,
                        Message = "Git email or username not configured."
                    };
                }

                // リポジトリチェック
                if (!Repository.IsValid(repoPath))
                {
                    return new GitPullResult
                    {
                        Success = false,
                        Message = $"Not a valid git repository: {repoPath}"
                    };
                }

                using var repo = new Repository(repoPath);
                var remoteUrl = repo.Network.Remotes["origin"]?.Url;

                // Pull実行
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

                // マージステータス確認
                if (result.Status == MergeStatus.Conflicts)
                {
                    return new GitPullResult
                    {
                        Success = false,
                        HasConflict = true,
                        Message = "Merge conflict detected. Please resolve manually:\n" +
                                  "1. Navigate to repository\n" +
                                  "2. Run: git status\n" +
                                  "3. Resolve conflicts\n" +
                                  "4. Run: git add . && git commit"
                    };
                }

                return new GitPullResult
                {
                    Success = true,
                    Message = $"Pull completed: {result.Status}"
                };
            }
            catch (Exception ex)
            {
                return new GitPullResult
                {
                    Success = false,
                    Message = $"Pull failed: {ex.Message}"
                };
            }
        });
    }

    /// <summary>
    /// Commit実行（単一ファイル）
    /// </summary>
    public async Task<IGitCommitResult> CommitAsync(
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
    public async Task<IGitCommitResult> CommitAllAsync(
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
    public async Task<IGitPushResult> PushAsync(string repositoryKey, string repoPath)
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
    public async Task<IGitTagResult> CreateTagAsync(
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
    public async Task<IGitPushResult> PushTagAsync(
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
    public async Task<IGitCommitAndPushResult> CommitAndPushAsync(
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
    public async Task<IGitCommitAndPushResult> CommitAllAndPushAsync(
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
    public async Task<IGitTagResult> CreateAndPushTagAsync(
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
}