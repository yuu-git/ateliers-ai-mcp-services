namespace Ateliers.Ai.Mcp.Services.GenericModels;

public class GenerateVoiceRequest : IGenerateVoiceRequest
{
    public string Text { get; set; } = string.Empty;
    public string OutputWavFileName { get; set; } = string.Empty;
    public IVoiceGenerationOptions? Options { get; set; }
}
