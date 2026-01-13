using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.DependencyInjection;
using Ateliers.Logging.DependencyInjection;
using Ateliers.Voice.Engines.VoicevoxTools;
using Microsoft.Extensions.DependencyInjection;

namespace Ateliers.Ai.Mcp.Services.Voicevox;

/// <summary>
/// VOICEVOX MCP サービス（Ateliers.Voice.Engines.VoicevoxTools のラッパー）
/// </summary>
public sealed class VoicevoxService : McpContentGenerationServiceBase, IGenerateVoiceService, IDisposable
{
    protected override string LogPrefix { get; init; } = $"{nameof(VoicevoxService)}:";

    private readonly IVoicevoxServiceOptions _options;
    private readonly IVoicevoxVoiceGenerator _generator;
    private readonly ServiceProvider? _serviceProvider;

    public VoicevoxService(IMcpLogger mcpLogger, IVoicevoxServiceOptions options)
        : base(mcpLogger, options.VoicevoxKnowledgeOptions)
    {
        McpLogger?.Info($"{LogPrefix} 初期化を開始");

        if (options == null)
        {
            var ex = new ArgumentNullException(nameof(options));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗", ex);
            throw ex;
        }

        _options = options;

        // Ateliers.Core の ILogger と IExecutionContext をセットアップ
        var services = new ServiceCollection();
        services.AddAteliersExecutionContext();
        services.AddAteliersLogging(logging =>
        {
            logging
                .SetCategory("Voicevox")
                .SetMinimumLevel(Ateliers.Logging.LogLevel.Information)
                .AddConsole();  // 簡略化のためコンソール出力のみ
        });

        _serviceProvider = services.BuildServiceProvider();
        var logger = _serviceProvider.GetRequiredService<Ateliers.ILogger>();
        var context = _serviceProvider.GetRequiredService<Ateliers.IExecutionContext>();

        // VoicevoxVoiceGenerator を初期化
        var outputDirectory = options.OutputRootDirectory 
            ?? Path.Combine(AppContext.BaseDirectory, "output");
        
        var voicevoxOptions = new VoicevoxOptions
        {
            ResourcePath = options.ResourcePath,
            DefaultStyleId = options.DefaultStyleId,
            VoiceModelNames = options.VoiceModelNames,
            OutputBaseDirectory = Path.Combine(outputDirectory, options.VoicevoxOutputDirectoryName)
        };

        _generator = new VoicevoxVoiceGenerator(voicevoxOptions, logger, context);

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// テスト用コンストラクタ（DI対応）
    /// </summary>
    internal VoicevoxService(IMcpLogger mcpLogger, IVoicevoxServiceOptions options, IVoicevoxVoiceGenerator generator)
        : base(mcpLogger, options.VoicevoxKnowledgeOptions)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
    }

    /// <summary>
    /// コンテンツ生成ガイドを取得します。
    /// </summary>
    /// <returns> 未実装（将来：Voicevox マークダウン形式のガイド） </returns>
    public string GetContentGenerationGuide()
    {
        // ToDo: interface IMcpContentGenerationGuideProvider のガイド実装
        return
            "未実装：VoicevoxService では、現在コンテンツ生成ガイドは提供されていません。" +
            "将来的にナレーターの特徴や使用方法を説明するガイドが追加される予定です。";
    }

    /// <summary>
    /// ナレッジコンテンツを取得します。
    /// </summary>
    /// <returns> ナレッジコンテンツの列挙 </returns>
    public override IEnumerable<string> GetServiceKnowledgeContents()
    {
        var contents = base.GetServiceKnowledgeContents();
        if (contents == null || !contents.Any())
        {
            McpLogger?.Warn($"{LogPrefix} Voicevox サービスは現在ナレッジコンテンツが存在しません。");
            return new List<string>()
            {
                "# VOICEVOX MCP ナレッジ：" + Environment.NewLine + Environment.NewLine +
                "現在、VOICEVOX サービスにはナレッジコンテンツが設定されていません。",
            };
        }
        return contents;
    }

    public async Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Info($"{LogPrefix} GenerateVoiceFileAsync 開始: text={request.Text.Length}文字, outputWavFileName={request.OutputWavFileName}");

        var voicevoxRequest = new VoicevoxGenerateRequest
        {
            Text = request.Text,
            OutputWavFileName = request.OutputWavFileName,
            Options = ConvertOptions(request.Options)
        };

        var result = await _generator.GenerateVoiceFileAsync(voicevoxRequest, cancellationToken);

        McpLogger?.Info($"{LogPrefix} GenerateVoiceFileAsync 完了: outputWavPath={result.OutputWavPath}");
        return result.OutputWavPath;
    }

    public async Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
        IEnumerable<IGenerateVoiceRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var requestList = requests.ToList();
        McpLogger?.Info($"{LogPrefix} GenerateVoiceFilesAsync 開始: リクエスト数={requestList.Count}個");

        var voicevoxRequests = requestList
            .Select(r => new VoicevoxGenerateRequest
            {
                Text = r.Text,
                OutputWavFileName = r.OutputWavFileName,
                Options = ConvertOptions(r.Options)
            })
            .ToList();

        var results = await _generator.GenerateVoiceFilesAsync(voicevoxRequests, cancellationToken);

        var outputPaths = results.Select(r => r.OutputWavPath).ToList();

        McpLogger?.Info($"{LogPrefix} GenerateVoiceFilesAsync 完了: {outputPaths.Count}個の音声ファイルを生成");
        return outputPaths;
    }

    private Voice.Engines.VoicevoxTools.VoicevoxGenerateOptions? ConvertOptions(IVoiceGenerationOptions? options)
    {
        if (options is not VoicevoxMcpGenerationOptions mcpOptions)
        {
            return null;
        }

        return new Voice.Engines.VoicevoxTools.VoicevoxGenerateOptions
        {
            StyleId = mcpOptions.StyleId,
            SpeedScale = mcpOptions.SpeedScale,
            PitchScale = mcpOptions.PitchScale,
            IntonationScale = mcpOptions.IntonationScale,
            VolumeScale = mcpOptions.VolumeScale,
            PrePhonemeLength = mcpOptions.PrePhonemeLength,
            PostPhonemeLength = mcpOptions.PostPhonemeLength,
            TextFileSaveMode = (Voice.Engines.TextFileSaveMode)mcpOptions.TextFileSaveMode
        };
    }

    public void Dispose()
    {
        McpLogger?.Debug($"{LogPrefix} Dispose: リソース破棄開始");
        _generator.Dispose();
        _serviceProvider?.Dispose();
        McpLogger?.Debug($"{LogPrefix} Dispose: リソース破棄完了");
    }
}
