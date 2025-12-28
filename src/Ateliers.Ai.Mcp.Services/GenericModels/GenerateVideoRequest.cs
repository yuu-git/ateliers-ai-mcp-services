namespace Ateliers.Ai.Mcp.Services.GenericModels;

public record GenerateVideoRequest(
    IReadOnlyList<SlideAudioPair> Slides,
    string? BackgroundMusicPath = null
);
