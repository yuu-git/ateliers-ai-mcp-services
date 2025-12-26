namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class MarpServiceOptions
{
    public string MarpExecutablePath { get; init; } = "marp";

    /// <summary>
    /// Root directory for all generated outputs.
    /// If null or empty, %TEMP% will be used.
    /// </summary>
    public string? OutputRootDirectory { get; init; }

    /// <summary>
    /// Sub directory name for Marp outputs.
    /// Default: "marp"
    /// </summary>
    public string MarpDirectoryName { get; init; } = "marp";
}