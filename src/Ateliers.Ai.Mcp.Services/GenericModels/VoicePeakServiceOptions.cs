namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// VOICEPEAK MCP サービスのオプション設定実装
/// </summary>
public sealed class VoicePeakServiceOptions : IVoicePeakServiceOptions
{
    /// <summary>
    /// VOICEPEAK 実行ファイルのパス
    /// </summary>
    public required string VoicePeakExecutablePath { get; init; } = @"C:\Program Files\VOICEPEAK\VoicePeak.exe";

    /// <summary>
    /// デフォルトのナレーター名
    /// </summary>
    public string? DefaultNarrator { get; init; } = "Frimomen";

    /// <summary>
    /// 出力ディレクトリのルートパス
    /// </summary>
    public string? OutputRootDirectory { get; init; } = "output";

    /// <summary>
    /// VOICEPEAK 出力用のディレクトリ名（デフォルト: voicepeak）
    /// </summary>
    public string VoicePeakOutputDirectoryName { get; init; } = "voicepeak";
}
