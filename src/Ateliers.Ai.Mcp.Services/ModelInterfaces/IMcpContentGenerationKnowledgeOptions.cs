namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// コンテンツ生成のナレッジオプションインターフェース
/// </summary>
public interface IMcpContentGenerationKnowledgeOptions
{
    /// <summary>
    /// ナレッジの種類（例: LocalFile, Notion, Database など）
    /// </summary>
    string KnowledgeType { get; }

    /// <summary>
    /// ナレッジのソース（例: ファイルパス、NotionページID、データベース接続文字列など）
    /// </summary>
    string KnowledgeSource { get; }

    /// <summary>
    /// ドキュメントの種類（例: Markdown, PlainText など）
    /// </summary>
    string DocumentType { get; }

    /// <summary>
    /// ナレッジの説明
    /// </summary>
    string Discription { get; }

    /// <summary>
    /// ナレッジヘッダーを生成するかどうか
    /// </summary>
    bool GenerateHeader { get; }
}
