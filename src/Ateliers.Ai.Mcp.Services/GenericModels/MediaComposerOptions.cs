namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class MediaComposerOptions
{
    public string FfmpegExecutablePath { get; init; } = "ffmpeg";
    public string? OutputRootDirectory { get; init; } = null; // null = Temp
    public string MediaDirectoryName { get; init; } = "media";
}