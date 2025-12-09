namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// Git Pull操作の結果（デフォルト実装）
/// </summary>
public class GitPullResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool HasConflict { get; init; }
}

/// <summary>
/// Git Push操作の結果（デフォルト実装）
/// </summary>
public class GitPushResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Git Tag操作の結果（デフォルト実装）
/// </summary>
public class GitTagResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? TagName { get; init; }
}

/// <summary>
/// Git Commit操作の結果（デフォルト実装）
/// </summary>
public class GitCommitResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? CommitHash { get; init; }
}

/// <summary>
/// Git CommitAndPush操作の結果（デフォルト実装）
/// </summary>
public class GitCommitAndPushResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? CommitHash { get; init; }
}