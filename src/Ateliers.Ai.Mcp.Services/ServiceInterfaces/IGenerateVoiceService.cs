namespace Ateliers.Ai.Mcp.Services;

public interface IGenerateVoiceService : IMcpContentGenerationGuideProvider
{
    Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
        IEnumerable<IGenerateVoiceRequest> requests,
        CancellationToken cancellationToken = default);
}

