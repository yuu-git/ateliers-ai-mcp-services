namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// GitHubリポジトリ情報（デフォルト実装）
/// </summary>
public class GitHubRepositorySummary : GitRepositorySummary, IGitHubRepositorySummary
{
    /// <inheritdoc/>
    public string Key { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Owner { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string PriorityDataSource { get; init; } = string.Empty;
}
