namespace Ateliers.Ai.Mcp.Services;

public interface IGenerateVoiceService
{
    Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        uint? styleId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
    IEnumerable<IGenerateVoiceRequest> requests,
    uint? styleId = null,
    CancellationToken cancellationToken = default);
}

