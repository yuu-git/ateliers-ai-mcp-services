using Ateliers.Voice.Engines;
using Ateliers.Voice.Engines.VoicePeakTools;

namespace Ateliers.Ai.Mcp.Services.VoicePeak;

/// <summary>
/// VOICEPEAK MCP サービス
/// </summary>
public sealed class VoicePeakService : McpServiceBase, IGenerateVoiceService
{
    private readonly IVoicePeakServiceOptions _options;
    private readonly IVoicePeakVoiceGenerator _generator;
    private readonly string _outputBaseDirectory;
    private const string LogPrefix = $"{nameof(VoicePeakService)}:";

    public VoicePeakService(IMcpLogger mcpLogger, IVoicePeakServiceOptions options)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化を開始");

        if (options == null)
        {
            var ex = new ArgumentNullException(nameof(options));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗", ex);
            throw ex;
        }

        _options = options;

        // VoicePeakVoiceGenerator を初期化
        var voicePeakOptions = new VoicePeakOptions
        {
            VoicePeakExecutablePath = options.VoicePeakExecutablePath
        };

        _generator = new VoicePeakVoiceGenerator(voicePeakOptions);

        // 共通ヘルパーを使用して出力ディレクトリを設定
        _outputBaseDirectory = VoiceOutputDirectoryHelper.GetOrCreateBaseDirectory(
            options.OutputRootDirectory,
            options.VoicePeakOutputDirectoryName);

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// テスト用コンストラクタ（DI対応）
    /// </summary>
    internal VoicePeakService(IMcpLogger mcpLogger, IVoicePeakServiceOptions options, IVoicePeakVoiceGenerator generator)
        : base(mcpLogger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _outputBaseDirectory = VoiceOutputDirectoryHelper.GetOrCreateBaseDirectory(
            options.OutputRootDirectory,
            options.VoicePeakOutputDirectoryName);
    }

    /// <summary>
    /// コンテンツ生成ガイドを取得します。
    /// </summary>
    /// <returns> VoicePeakナレーターの情報を含むマークダウン形式のガイド </returns>
    public string GetContentGenerationGuide()
    {
        return VoicePeakNarratorInfoFormatter.ToFullInfoMarkdownAllNarrators(McpLogger);
    }

    public async Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Info($"{LogPrefix} GenerateVoiceFileAsync 開始: text={request.Text.Length}文字, outputWavFileName={request.OutputWavFileName}");

        // 共通ヘルパーを使用してタイムスタンプベースのディレクトリを作成
        var outputDir = VoiceOutputDirectoryHelper.CreateTimestampedDirectory(_outputBaseDirectory);
        var outputPath = VoiceOutputDirectoryHelper.GetOutputFilePath(outputDir, request.OutputWavFileName);

        // VoicePeakリクエストを構築
        var narrator = CreateNarrator(request.Options);
        var voicePeakRequest = new VoicePeakGenerateRequest
        {
            Text = request.Text,
            OutputPath = outputPath,
            Narrator = narrator,
            Speed = GetSpeed(request.Options),
            Pitch = GetPitch(request.Options),
            Options = CreateVoicePeakOptions(request.Options)
        };

        var result = await _generator.GenerateVoiceFileAsync(voicePeakRequest, cancellationToken);

        McpLogger?.Info($"{LogPrefix} GenerateVoiceFileAsync 完了: outputPath={result.OutputWavPath}");
        return result.OutputWavPath;
    }

    public async Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
        IEnumerable<IGenerateVoiceRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var requestList = requests.ToList();
        McpLogger?.Info($"{LogPrefix} GenerateVoiceFilesAsync 開始: リクエスト数={requestList.Count}個");

        // 共通ヘルパーを使用してタイムスタンプベースのディレクトリを作成（複数ファイル用に共通ディレクトリ）
        var outputDir = VoiceOutputDirectoryHelper.CreateTimestampedDirectory(_outputBaseDirectory);

        var results = new List<string>();

        foreach (var request in requestList)
        {
            var outputPath = VoiceOutputDirectoryHelper.GetOutputFilePath(outputDir, request.OutputWavFileName);

            var narrator = CreateNarrator(request.Options);
            var voicePeakRequest = new VoicePeakGenerateRequest
            {
                Text = request.Text,
                OutputPath = outputPath,
                Narrator = narrator,
                Speed = GetSpeed(request.Options),
                Pitch = GetPitch(request.Options),
                Options = CreateVoicePeakOptions(request.Options)
            };

            var result = await _generator.GenerateVoiceFileAsync(voicePeakRequest, cancellationToken);
            results.Add(result.OutputWavPath);
        }

        McpLogger?.Info($"{LogPrefix} GenerateVoiceFilesAsync 完了: {results.Count}個の音声ファイルを生成");
        return results;
    }

    private IVoicePeakNarrator CreateNarrator(IVoiceGenerationOptions? options)
    {
        // VoicePeakMcpGenerationOptions で NarratorInstance が設定されている場合はそれを使用
        if (options is VoicePeakMcpGenerationOptions voicePeakOptions &&
            voicePeakOptions.NarratorInstance != null)
        {
            return voicePeakOptions.NarratorInstance;
        }

        // 従来通りの処理: ナレーター名から生成
        var narratorName = GetNarrator(options);
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName(narratorName);

        // 感情パラメーターを設定（Emotion プロパティがある場合）
        if (options is VoicePeakMcpGenerationOptions voicePeakOpts &&
            !string.IsNullOrWhiteSpace(voicePeakOpts.Emotion))
        {
            narrator.SetEmotionParameter(voicePeakOpts.Emotion);
        }

        return narrator;
    }

    private string GetNarrator(IVoiceGenerationOptions? options)
    {
        if (options is VoicePeakMcpGenerationOptions voicePeakOptions && 
            !string.IsNullOrWhiteSpace(voicePeakOptions.Narrator))
        {
            return voicePeakOptions.Narrator;
        }

        return _options.DefaultNarrator ?? "Frimomen";
    }

    private int GetSpeed(IVoiceGenerationOptions? options)
    {
        if (options is VoicePeakMcpGenerationOptions voicePeakOptions)
        {
            return voicePeakOptions.Speed;
        }

        return 100; // デフォルト
    }

    private int GetPitch(IVoiceGenerationOptions? options)
    {
        if (options is VoicePeakMcpGenerationOptions voicePeakOptions)
        {
            return voicePeakOptions.Pitch;
        }

        return 0; // デフォルト
    }

    private VoicePeakGenerateOptions? CreateVoicePeakOptions(IVoiceGenerationOptions? options)
    {
        if (options is VoicePeakMcpGenerationOptions voicePeakOptions)
        {
            return new VoicePeakGenerateOptions
            {
                NarratorName = GetNarrator(options),
                Speed = GetSpeed(options),
                Pitch = GetPitch(options),
                TextFileSaveMode = (Voice.Engines.TextFileSaveMode)voicePeakOptions.TextFileSaveMode
            };
        }

        return null;
    }
}
