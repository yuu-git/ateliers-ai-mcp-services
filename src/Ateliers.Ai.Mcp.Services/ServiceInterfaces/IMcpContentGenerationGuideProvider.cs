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

// 将来的な拡張予定として、以下を残しておく

/*

// 問題：string GetContentGenerationGuide() は情報がフラットすぎる
//
// 今は Markdown 文字列で返す想定ですが、将来こうなります：
// - どこまでが「前提条件」？
// - どこまでが「手順」？
// - どこが「制約」？
// - どこが「例」？
// 
// → 再利用・合成・順序制御が難しくなる
// 今はまだ大丈夫だが、あと2〜3サービス増えると並び順・重複・整形地獄になる。
//
// 解決策：返却型を「構造化ガイド」にする
//
// これで何が良くなるか:
// - Tool は 並び順・統合ルールだけ持てばいい
// - サービスは 責務範囲だけ埋める
// - Markdown生成は最後の1箇所だけ
// 
// 👉 説明 = データ / 表示 = 別責務

public interface IMcpContentGenerationGuideProvider
{
    ContentGenerationGuide GetContentGenerationGuide();
}

public sealed class ContentGenerationGuide
{
    public string Title { get; init; }
    public IReadOnlyList<string> Preconditions { get; init; }
    public IReadOnlyList<string> Steps { get; init; }
    public IReadOnlyList<string> Constraints { get; init; }
    public IReadOnlyList<string> Examples { get; init; }
}

*/