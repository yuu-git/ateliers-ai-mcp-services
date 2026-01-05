namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// IGenerateVoiceRequest の拡張メソッド
/// </summary>
public static class GenerateVoiceRequestExtensions
{
    /// <summary>
    /// 型安全に Options を取得します
    /// </summary>
    /// <typeparam name="TOptions">取得するオプションの型</typeparam>
    /// <param name="request">リクエスト</param>
    /// <returns>指定された型のオプション、またはキャスト失敗時は null</returns>
    public static TOptions? GetOptions<TOptions>(this IGenerateVoiceRequest request)
        where TOptions : class, IVoiceGenerationOptions
    {
        return request.Options as TOptions;
    }

    /// <summary>
    /// Options を取得し、null の場合はデフォルト値を返します
    /// </summary>
    /// <typeparam name="TOptions">取得するオプションの型</typeparam>
    /// <param name="request">リクエスト</param>
    /// <param name="defaultValue">デフォルト値</param>
    /// <returns>指定された型のオプション、または null の場合はデフォルト値</returns>
    public static TOptions GetOptionsOrDefault<TOptions>(
        this IGenerateVoiceRequest request,
        TOptions defaultValue)
        where TOptions : class, IVoiceGenerationOptions
    {
        return request.Options as TOptions ?? defaultValue;
    }

    /// <summary>
    /// Options を取得し、null の場合は新しいインスタンスを作成します
    /// </summary>
    /// <typeparam name="TOptions">取得するオプションの型</typeparam>
    /// <param name="request">リクエスト</param>
    /// <returns>指定された型のオプション、または null の場合は新しいインスタンス</returns>
    public static TOptions GetOptionsOrNew<TOptions>(this IGenerateVoiceRequest request)
        where TOptions : class, IVoiceGenerationOptions, new()
    {
        return request.Options as TOptions ?? new TOptions();
    }
}
