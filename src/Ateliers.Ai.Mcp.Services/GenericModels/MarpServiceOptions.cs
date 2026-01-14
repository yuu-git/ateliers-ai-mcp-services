namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// Marpサービスのオプション設定
/// </summary>
public sealed class MarpServiceOptions : OutputDirectoryProvider, IMarpServiceOptions
{
    /// <summary>
    /// Marp実行可能ファイルのパス
    /// </summary>
    public string MarpExecutablePath { get; init; } = "marp";

    /// <summary>
    /// Marp出力ディレクトリ名
    /// </summary>
    public string MarpOutputDirectoryName { get; init; } = "marp";

    /// <summary>
    /// スライド区切り見出しのリスト
    /// default: "#", "##"
    /// </summary>
    public IReadOnlyList<string> SeparatorHeadingPrefixList { get; init; } = new List<string> { "#", "##" };

    public IList<MarpGenerationKnowledgeOptions> MarpKnowledgeOptions { get; init; } = new List<MarpGenerationKnowledgeOptions>();
}