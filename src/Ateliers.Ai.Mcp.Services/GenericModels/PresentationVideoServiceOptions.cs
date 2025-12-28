namespace Ateliers.Ai.Mcp.Services.GenericModels;

public class PresentationVideoServiceOptions : OutputDirectoryProvider, IPresentationVideoOptions, IVoicevoxServiceOptions, IMarpServiceOptions, IFfmpegServiceOptions
{
    // IVoicevoxServiceOptions
    public required string ResourcePath { get; init; } = "voicevox";

    public uint DefaultStyleId { get; init; } = 0;

    public IReadOnlyCollection<string>? VoiceModelNames { get; init; }

    public string VoicevoxOutputDirectoryName { get; init; } = "voice";

    // IMarpServiceOptions
    public string MarpExecutablePath { get; init; } = "marp";

    public string MarpOutputDirectoryName { get; init; } = "slide";

    // IFfmpegServiceOptions
    public string FfmpegExecutablePath { get; init; } = "ffmpeg";

    public string MediaOutputDirectoryName { get; init; } = "media";
}
