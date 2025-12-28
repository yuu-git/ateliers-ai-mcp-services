using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services;

public interface IPresentationVideoGenerator
{
    Task<PresentationVideoResult> GenerateAsync(
        PresentationVideoRequest request,
        CancellationToken cancellationToken = default);
}