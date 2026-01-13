namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// MCP向けのコンテンツ生成ガイドプロバイダー
/// </summary>
public interface IMcpContentGenerationGuideProvider
{
    /// <summary>
    /// MCP向けのコンテンツ生成ガイドを取得します。
    /// </summary>
    /// <remarks>
    /// これは、各コンテンツ生成サービスにおける特有の生成ガイドを提供するためのメソッドです。<br/>
    /// 例えば、音声生成の場合、利用可能なナレーターの一覧など。動画合成の場合、パラメーター指定の方法などです。<br/>
    /// ツール層で単体あるいは複数のサービスを組み合わせて利用する際に、ユーザーやLLMに対して適切なガイドを提供するために使用されます。
    /// </remarks>
    /// <returns> コンテンツ生成ガイド </returns>
    string GetContentGenerationGuide();
}
