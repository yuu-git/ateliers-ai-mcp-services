namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// MCP向けのコンテンツ生成ナレッジプロバイダー
/// </summary>
public interface IMcpContentGenerationKnowledgeProvider
{
    /// <summary>
    /// MCP向けのコンテンツ生成ナレッジを取得します。
    /// </summary>
    /// <remarks>
    /// これは、各コンテンツ生成サービスにおけるユーザー特有の生成ナレッジを提供するためのメソッドです。<br/>
    /// 例えば、音声生成の場合、初期のナレーター設定や速度など。スライドの場合、デザインテンプレートなどです。<br/>
    /// ツール層で単体あるいは複数のサービスを組み合わせて利用する際に、ユーザーやLLMに対して適切なナレッジを提供するために使用されます。<br/>
    /// ローカルファイルやNotion、データベースなど、外部からの注入できることを前提としています。
    /// </remarks>
    /// <returns> コンテンツ生成ナレッジ </returns>
    IEnumerable<string> GetServiceKnowledgeContents();
}