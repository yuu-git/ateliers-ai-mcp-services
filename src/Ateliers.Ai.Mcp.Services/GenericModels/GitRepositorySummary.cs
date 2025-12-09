namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// Gitリポジトリ情報（デフォルト実装）
/// </summary>
public class GitRepositorySummary : IGitRepositorySummary
{

    /// <inheritdoc/>
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Branch { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string LocalPath { get; init; } = string.Empty;

    /// <inheritdoc/>
    public bool HasLocalPath { get; init; }
}
