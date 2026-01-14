using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services;

public interface IPresentationVideoGenerator : IMcpContentGenerationGuideProvider, IMcpContentGenerationKnowledgeProvider
{
    Task<PresentationVideoResult> GenerateAsync(
        PresentationVideoRequest request,
        CancellationToken cancellationToken = default);
}