namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class VoicevoxServiceOptions : OutputDirectoryProvider, IVoicevoxServiceOptions
{
    public required string ResourcePath { get; init; }

    public uint DefaultStyleId { get; init; } = 0;

    public IReadOnlyCollection<string>? VoiceModelNames { get; init; }

    public string VoicevoxOutputDirectoryName { get; init; } = "voicevox";

    public IList<VoicevoxGenerationKnowledgeOptions> VoicevoxKnowledgeOptions { get; init; } = new List<VoicevoxGenerationKnowledgeOptions>();
}

