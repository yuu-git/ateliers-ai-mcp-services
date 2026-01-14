namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Marp サービスのオプション設定
/// </summary>
public interface IMarpServiceOptions : IOutputDirectoryProvider
{
    /// <summary>
    /// Marp 実行ファイルのパス
    /// </summary>
    string MarpExecutablePath { get; }

    /// <summary>
    /// Marp 出力ディレクトリ名
    /// </summary>
    string MarpOutputDirectoryName { get;}

    /// <summary>
    /// スライド区切り見出しのリスト
    /// </summary>
    IReadOnlyList<string> SeparatorHeadingPrefixList { get; }

    /// <summary>
    /// Marp 用のコンテンツ生成ナレッジオプション群
    /// </summary>
    IList<MarpGenerationKnowledgeOptions> MarpKnowledgeOptions { get; }
}
