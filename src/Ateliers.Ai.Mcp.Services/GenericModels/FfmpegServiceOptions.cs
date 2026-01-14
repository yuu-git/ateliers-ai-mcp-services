namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class FfmpegServiceOptions : OutputDirectoryProvider, IFfmpegServiceOptions
{
    public string FfmpegExecutablePath { get; init; } = "ffmpeg";

    public string MediaOutputDirectoryName { get; init; } = "media";

    public IList<FfmpegGenerationKnowledgeOptions> FfmpegKnowledgeOptions { get; init; } = new List<FfmpegGenerationKnowledgeOptions>();
}