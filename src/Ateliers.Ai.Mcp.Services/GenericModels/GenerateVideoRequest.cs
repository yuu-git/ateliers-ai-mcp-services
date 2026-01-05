namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// ビデオ生成リクエスト
/// </summary>
/// <param name="Slides"> スライドとオーディオのペアリスト </param>
/// <param name="BackgroundMusicPath"> BGMのパス（オプション） </param>
public record GenerateVideoRequest(
    IReadOnlyList<SlideAudioPair> Slides,
    string? BackgroundMusicPath = null
);
