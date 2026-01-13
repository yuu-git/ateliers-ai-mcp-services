namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// コンテンツ生成サービスの基底クラス
/// </summary>
public abstract class McpContentGenerationServiceBase : McpServiceBase
{
    /// <summary>
    /// ログ接頭辞
    /// </summary>
    protected override string LogPrefix { get; init; } = $"{nameof(McpContentGenerationServiceBase)}: ";

    private IEnumerable<IMcpContentGenerationKnowledgeOptions> _generationKnowledgeOptions;

    /// <summary>
    /// コンテンツ生成サービスの基底クラスのコンストラクタ
    /// </summary>
    /// <param name="generationKnowledgeOptions"> コンテンツ生成に使用されるナレッジオプションの列挙 </param>
    public McpContentGenerationServiceBase(IEnumerable<IMcpContentGenerationKnowledgeOptions> generationKnowledgeOptions)
        : base()
    {
        _generationKnowledgeOptions = generationKnowledgeOptions;
    }

    /// <summary>
    /// コンテンツ生成サービスの基底クラスのコンストラクタ
    /// </summary>
    /// <param name="mcpLogger"> MCP ロガーインスタンス </param>
    /// <param name="generationKnowledgeOptions"> コンテンツ生成に使用されるナレッジオプションの列挙 </param>
    public McpContentGenerationServiceBase(IMcpLogger mcpLogger, IEnumerable<IMcpContentGenerationKnowledgeOptions> generationKnowledgeOptions)
        : base(mcpLogger)
    {
        _generationKnowledgeOptions = generationKnowledgeOptions;
    }

    /// <summary>
    /// サービスで使用されるナレッジコンテンツを取得します。
    /// </summary>
    /// <returns> ナレッジコンテンツの列挙 </returns>
    public virtual IEnumerable<string> GetServiceKnowledgeContents()
    {
        McpLogger?.Info($"{LogPrefix} ナレッジコンテンツの取得を開始");

        if (_generationKnowledgeOptions == null || !_generationKnowledgeOptions.Any())
        {
            McpLogger?.Warn($"{LogPrefix} 警告: ナレッジコンテンツが存在しないため、取得をスキップ");
            yield break;
        }

        foreach (var option in _generationKnowledgeOptions)
        {
            if (option.KnowledgeType == "LocalFile")
            {
                McpLogger?.Info($"{LogPrefix} ローカルファイルからナレッジコンテンツを取得: {option.KnowledgeSource}");
                yield return GetLocalFileKnowledge(option);
            }
            else
            {
                McpLogger?.Warn($"{LogPrefix} 警告: {option.KnowledgeSource} = サポートされていないナレッジタイプのためスキップ ({option.KnowledgeType})");
            }
        }

        McpLogger?.Info($"{LogPrefix} ナレッジコンテンツの取得を完了");
    }

    /// <summary>
    /// ローカルファイルからナレッジを取得します。
    /// </summary>
    /// <param name="options"> コンテンツ生成オプション </param>
    /// <returns> 取得されたナレッジコンテンツ </returns>
    /// <exception cref="FileNotFoundException"> ファイルが見つからない場合 </exception> 
    /// <exception cref="NotSupportedException"> サポートされていないドキュメントタイプの場合 </exception>
    protected virtual string GetLocalFileKnowledge(IMcpContentGenerationKnowledgeOptions options)
    {
        McpLogger?.Debug($"{LogPrefix} ローカルファイルナレッジの取得を開始: {options.KnowledgeSource}");

        if (!File.Exists(options.KnowledgeSource))
        {
            var ex = new FileNotFoundException($"Knowledge file not found: {options.KnowledgeSource}");
            McpLogger?.Error($"{LogPrefix} エラー: {ex.Message}");
            throw ex;
        }

        var result = string.Empty;

        if (options.GenerateHeader)
        {
            result += GenerateHeaderContent(options);
        }

        switch (options.DocumentType)
        {
            case "Markdown":
                result += ParseMarkdownContent(File.ReadAllText(options.KnowledgeSource));
                break;
            default:
                var ex = new NotSupportedException($"Unsupported document type: {options.DocumentType}");
                McpLogger?.Error($"{LogPrefix} エラー: {ex.Message}");
                throw ex;
        }

        return result;
    }

    /// <summary>
    /// ヘッダーコンテンツを生成します。
    /// </summary>
    /// <param name="options"> コンテンツ生成オプション </param>
    /// <returns> 生成されたヘッダー文字列 </returns>
    protected virtual string GenerateHeaderContent(IMcpContentGenerationKnowledgeOptions options)
    {
        McpLogger?.Debug($"{LogPrefix} ヘッダーコンテンツの生成を開始: {options.KnowledgeSource}");

        switch (options.DocumentType)
        {
            case "Markdown":
                return GenerateMarkdownHeader(options);
            default:
                McpLogger?.Warn($"{LogPrefix} 警告: {options.KnowledgeSource} = サポートされていないドキュメントタイプのためヘッダー生成をスキップ ({options.DocumentType})");
                return string.Empty;
        }
    }

    /// <summary>
    /// Markdown ヘッダーを生成します。
    /// </summary>
    /// <param name="options"> コンテンツ生成オプション </param>
    /// <returns> 生成されたヘッダー文字列 </returns>
    protected virtual string GenerateMarkdownHeader(IMcpContentGenerationKnowledgeOptions options)
    {
        McpLogger?.Debug($"{LogPrefix} Markdown ヘッダーの生成を開始: {options.KnowledgeSource}");

        var header = string.Empty;
        header += "---" + Environment.NewLine;
        header += $"<!-- Source: {options.KnowledgeSource} -->" + Environment.NewLine;

        if (!string.IsNullOrWhiteSpace(options.Discription))
        {
            header += $"<!-- Discription: {options.Discription} -->" + Environment.NewLine;
        }

        header += "---" + Environment.NewLine + Environment.NewLine;

        return header;
    }


    /// <summary>
    /// Markdown コンテンツを解析してインポートします。
    /// </summary>
    /// <param name="markdownContent"> Markdown コンテンツ </param>
    /// <returns> 解析されたコンテンツ </returns>
    protected virtual string ParseMarkdownContent(string markdownContent)
    {
        McpLogger?.Debug($"{LogPrefix} Markdown コンテンツの解析を開始");

        // ここで Markdown コンテンツのインポート処理を実装します。
        // 例えば、Markdown パーサーを使用して内容を解析し、必要に応じて変換やフィルタリングを行うことができます。
        return markdownContent;
    }
}
