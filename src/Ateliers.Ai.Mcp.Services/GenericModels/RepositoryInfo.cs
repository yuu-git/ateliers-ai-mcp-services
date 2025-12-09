namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// GitHubリポジトリ情報（デフォルト実装）
/// </summary>
public class RepositoryInfo : IGitHubRepositoryInfo
{
    /// <inheritdoc/>
    public string Key { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Owner { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Branch { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string PriorityDataSource { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string LocalPath { get; init; } = string.Empty;

    /// <inheritdoc/>
    public bool HasLocalPath { get; init; }
}
