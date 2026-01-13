using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// メディア合成サービスインターフェース
/// </summary>
public interface IMediaComposerService : IMcpContentGenerationGuideProvider
{
    /// <summary>
    /// ビデオ合成を非同期で実行します。
    /// </summary>
    /// <param name="request"> ビデオ生成リクエストを指定します。 </param>
    /// <param name="outputFileName"> 出力ファイル名を指定します。 </param>
    /// <param name="cancellationToken"> キャンセレーションをサポートするためのトークンを指定します。（省略可能） </param>
    /// <returns> 合成されたビデオのファイルパスを返します。 </returns>
    Task<string> ComposeAsync(
        GenerateVideoRequest request,
        string outputFileName,
        CancellationToken cancellationToken = default);
}

