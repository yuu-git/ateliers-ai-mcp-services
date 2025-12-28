using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services;

public interface IMediaComposerService
{
    Task<string> ComposeAsync(
        GenerateVideoRequest request,
        string outputFileName,
        CancellationToken cancellationToken = default);
}

