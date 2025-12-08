namespace Ateliers.Ai.Mcp.Services.Models;

#region Result Interfaces

/// <summary>
/// Git Pull操作の結果インターフェース
/// </summary>
public interface IGitPullResult
{
    /// <summary>成功したかどうか</summary>
    bool Success { get; }
    
    /// <summary>結果メッセージ</summary>
    string Message { get; }
    
    /// <summary>コンフリクトが発生したかどうか</summary>
    bool HasConflict { get; }
}

/// <summary>
/// Git Commit操作の結果インターフェース
/// </summary>
public interface IGitCommitResult
{
    /// <summary>成功したかどうか</summary>
    bool Success { get; }
    
    /// <summary>結果メッセージ</summary>
    string Message { get; }
    
    /// <summary>コミットハッシュ</summary>
    string? CommitHash { get; }
}

/// <summary>
/// Git Push操作の結果インターフェース
/// </summary>
public interface IGitPushResult
{
    /// <summary>成功したかどうか</summary>
    bool Success { get; }
    
    /// <summary>結果メッセージ</summary>
    string Message { get; }
}

/// <summary>
/// Git CommitAndPush操作の結果インターフェース
/// </summary>
public interface IGitCommitAndPushResult
{
    /// <summary>成功したかどうか</summary>
    bool Success { get; }
    
    /// <summary>結果メッセージ</summary>
    string Message { get; }
    
    /// <summary>コミットハッシュ</summary>
    string? CommitHash { get; }
}

/// <summary>
/// Git Tag操作の結果インターフェース
/// </summary>
public interface IGitTagResult
{
    /// <summary>成功したかどうか</summary>
    bool Success { get; }
    
    /// <summary>結果メッセージ</summary>
    string Message { get; }
    
    /// <summary>タグ名</summary>
    string? TagName { get; }
}

#endregion

#region Default Implementations

/// <summary>
/// Git Pull操作の結果（デフォルト実装）
/// </summary>
public class GitPullResult : IGitPullResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool HasConflict { get; init; }
}

/// <summary>
/// Git Commit操作の結果（デフォルト実装）
/// </summary>
public class GitCommitResult : IGitCommitResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? CommitHash { get; init; }
}

/// <summary>
/// Git Push操作の結果（デフォルト実装）
/// </summary>
public class GitPushResult : IGitPushResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Git CommitAndPush操作の結果（デフォルト実装）
/// </summary>
public class GitCommitAndPushResult : IGitCommitAndPushResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? CommitHash { get; init; }
}

/// <summary>
/// Git Tag操作の結果（デフォルト実装）
/// </summary>
public class GitTagResult : IGitTagResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? TagName { get; init; }
}

#endregion
