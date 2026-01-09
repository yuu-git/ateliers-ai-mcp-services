namespace Ateliers.Ai.Mcp.Services.VoicePeak;

/// <summary>
/// VOICEPEAK MCP サービスのオプション設定
/// </summary>
public interface IVoicePeakServiceOptions
{
    /// <summary>
    /// VOICEPEAK 実行ファイルのパス
    /// </summary>
    string VoicePeakExecutablePath { get; }

    /// <summary>
    /// デフォルトのナレーター名
    /// </summary>
    string? DefaultNarrator { get; }

    /// <summary>
    /// 出力ディレクトリのルートパス
    /// </summary>
    string? OutputRootDirectory { get; }

    /// <summary>
    /// VOICEPEAK 出力用のディレクトリ名
    /// </summary>
    string VoicePeakOutputDirectoryName { get; }
}

/// <summary>
/// VOICEPEAK MCP サービスのオプション設定実装
/// </summary>
public sealed class VoicePeakServiceOptions : IVoicePeakServiceOptions
{
    /// <summary>
    /// VOICEPEAK 実行ファイルのパス
    /// </summary>
    public required string VoicePeakExecutablePath { get; init; }

    /// <summary>
    /// デフォルトのナレーター名
    /// </summary>
    public string? DefaultNarrator { get; init; }

    /// <summary>
    /// 出力ディレクトリのルートパス
    /// </summary>
    public string? OutputRootDirectory { get; init; }

    /// <summary>
    /// VOICEPEAK 出力用のディレクトリ名（デフォルト: voicepeak）
    /// </summary>
    public string VoicePeakOutputDirectoryName { get; init; } = "voicepeak";
}
