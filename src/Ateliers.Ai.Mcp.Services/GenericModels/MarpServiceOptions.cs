namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class MarpServiceOptions : OutputDirectoryProvider, IMarpServiceOptions
{
    public string MarpExecutablePath { get; init; } = "marp";

    /// <summary>
    /// Sub directory name for Marp outputs.
    /// Default: "marp"
    /// </summary>
    public string MarpOutputDirectoryName { get; init; } = "marp";
}