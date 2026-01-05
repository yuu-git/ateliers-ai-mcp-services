namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// 音声生成時のテキストファイル保存モード
/// </summary>
public enum TextFileSaveMode
{
    /// <summary>
    /// テキストファイルを保存しない
    /// </summary>
    None = 0,

    /// <summary>
    /// テキストのみを .txt ファイルとして保存（デフォルト）
    /// </summary>
    TextOnly = 1,

    /// <summary>
    /// テキストとメタデータを JSON 形式で .json ファイルとして保存
    /// </summary>
    WithMetadata = 2
}
