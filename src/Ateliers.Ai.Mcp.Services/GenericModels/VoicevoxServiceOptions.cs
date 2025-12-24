namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class VoicevoxServiceOptions
{
    public required string ResourcePath { get; init; }
    public uint DefaultStyleId { get; init; } = 0;
}

