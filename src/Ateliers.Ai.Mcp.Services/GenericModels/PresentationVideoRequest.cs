namespace Ateliers.Ai.Mcp.Services.GenericModels;

public record PresentationVideoRequest(
    IReadOnlyList<SlideAudioPair> Slides,
    string? BackgroundMusicPath = null
);
