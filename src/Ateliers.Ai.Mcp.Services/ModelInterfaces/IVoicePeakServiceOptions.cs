namespace Ateliers.Ai.Mcp.Services;

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
