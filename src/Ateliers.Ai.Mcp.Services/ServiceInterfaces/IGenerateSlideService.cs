namespace Ateliers.Ai.Mcp.Services;

public interface IGenerateSlideService : IMcpContentGenerationGuideProvider, IMcpContentGenerationKnowledgeProvider
{
    string GenerateSlideMarkdown(string sourceMarkdown);

    Task<IReadOnlyList<string>> RenderToPngAsync(
        string slideMarkdown,
        CancellationToken cancellationToken = default);
}
