namespace Ateliers.Ai.Mcp.Services;

public interface IPresentationVideoOptions : IOutputDirectoryProvider
{
    /// <summary>
    /// プレゼンテーション動画生成用のコンテンツ生成ナレッジオプション群
    /// </summary>
    IList<PresentationVideoGenerationKnowledgeOptions> PresentationVideoKnowledgeOptions { get; }
}
