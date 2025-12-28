namespace Ateliers.Ai.Mcp.Services;

public interface IFfmpegServiceOptions : IOutputDirectoryProvider
{
    string FfmpegExecutablePath { get; }

    string MediaOutputDirectoryName { get; }
}
