namespace Ateliers.Ai.Mcp.Services;

public interface IFfmpegServiceOptions : IOutputDirectoryProvider
{
    string FfmpegExecutablePath { get; }

    string MediaOutputDirectoryName { get; }

    /// <summary>
    /// FFmpeg 用のコンテンツ生成ナレッジオプション群
    /// </summary>
    IList<FfmpegGenerationKnowledgeOptions> FfmpegKnowledgeOptions { get; }
}
