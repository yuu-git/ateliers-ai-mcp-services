namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// プレゼンテーション動画生成サービスの汎用オプション設定
/// </summary>
/// <remarks>
/// 汎用設定の組み合わせ： Marp + Voicevox + Ffmpeg
/// </remarks>
public class PresentationVideoServiceOptions : OutputDirectoryProvider, IPresentationVideoOptions, IVoicevoxServiceOptions, IMarpServiceOptions, IFfmpegServiceOptions
{
    // IPresentationVideoOptions
    public IList<PresentationVideoGenerationKnowledgeOptions> PresentationVideoKnowledgeOptions { get; init; } = new List<PresentationVideoGenerationKnowledgeOptions>();

    // IVoicevoxServiceOptions
    public required string ResourcePath { get; init; } = "voicevox";

    public uint DefaultStyleId { get; init; } = 0;

    public IReadOnlyCollection<string>? VoiceModelNames { get; init; }

    public string VoicevoxOutputDirectoryName { get; init; } = "voice";

    public IList<VoicevoxGenerationKnowledgeOptions> VoicevoxKnowledgeOptions { get; init; } = new List<VoicevoxGenerationKnowledgeOptions>();

    // IMarpServiceOptions
    public string MarpExecutablePath { get; init; } = "marp";

    public string MarpOutputDirectoryName { get; init; } = "slide";

    public IReadOnlyList<string> SeparatorHeadingPrefixList { get; init; } = new List<string> { "# ", "## " };

    public IList<MarpGenerationKnowledgeOptions> MarpKnowledgeOptions { get; init; } = new List<MarpGenerationKnowledgeOptions>();

    // IFfmpegServiceOptions
    public string FfmpegExecutablePath { get; init; } = "ffmpeg";

    public string MediaOutputDirectoryName { get; init; } = "media";

    public IList<FfmpegGenerationKnowledgeOptions> FfmpegKnowledgeOptions { get; init; } = new List<FfmpegGenerationKnowledgeOptions>();
}
