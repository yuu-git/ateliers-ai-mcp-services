namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed record PresentationVideoRequest
{
    // Slide
    public required string SourceMarkdown { get; init; }

    // Voice
    public required IReadOnlyList<string> NarrationTexts { get; init; }
    public int? VoiceStyleId { get; init; }

    // Optional
    public string? BackgroundMusicPath { get; init; }

    // Output
    public string? OutputFileName { get; init; }
}