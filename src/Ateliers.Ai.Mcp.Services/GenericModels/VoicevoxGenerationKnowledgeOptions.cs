namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// VOICEVOX 用のコンテンツ生成ナレッジオプション
/// </summary>
public class VoicevoxGenerationKnowledgeOptions : IMcpContentGenerationKnowledgeOptions
{
    /// <summary>
    /// ナレッジの種類（例: LocalFile, Notion, Database など）
    /// </summary>
    /// <remarks>
    /// 現在はローカルファイルのみをサポートしています。
    /// </remarks>
    public string KnowledgeType { get; init; } = "LocalFile";

    /// <summary>
    /// ナレッジのソース（例: ファイルパス、NotionページID、データベース接続文字列など）
    /// </summary>
    /// <remarks>
    /// 現在はローカルファイルのみをサポートしており、デフォルトでは "knowledge/voicevox_knowledge.md" を使用します。
    /// </remarks>
    public string KnowledgeSource { get; init; } = "knowledge/voicevox_knowledge.md";

    /// <summary>
    /// ドキュメントの種類（例: Markdown, PlainText など）
    /// </summary>
    /// <remarks>
    /// 現在は Markdown のみをサポートしています。<br/>
    /// 将来的には Markdown 以外の形式もサポートする可能性があります。(例: PlainText, HTML, JSON など)
    /// </remarks>
    public string DocumentType { get; init; } = "Markdown";

    /// <summary>
    /// ナレッジの説明
    /// </summary>
    public string Discription { get; init; } = "VOICEVOX用の音声生成ナレッジ";

    /// <summary>
    /// ナレッジヘッダーを生成するかどうか
    /// </summary>
    public bool GenerateHeader { get; init; } = false;
}
