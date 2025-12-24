namespace Ateliers.Ai.Mcp.Services;

public interface IVoicevoxSpeechService
{
    Task<byte[]> SynthesizeAsync(
        string text,
        uint? styleId = null,
        CancellationToken cancellationToken = default);

    Task<string> SynthesizeToFileAsync(
        string text,
        string outputWavPath,
        uint? styleId = null,
        CancellationToken cancellationToken = default);
}

