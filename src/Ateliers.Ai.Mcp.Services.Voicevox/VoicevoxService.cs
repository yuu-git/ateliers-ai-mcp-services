using Ateliers.Ai.Mcp.Services.GenericModels;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Text.RegularExpressions;
using VoicevoxCoreSharp.Core;
using VoicevoxCoreSharp.Core.Enum;
using VoicevoxCoreSharp.Core.Struct;

namespace Ateliers.Ai.Mcp.Services.Voicevox;

public sealed class VoicevoxService : McpServiceBase, IGenerateVoiceService, IDisposable
{
    private readonly IVoicevoxServiceOptions _options;
    private readonly Synthesizer _synthesizer;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private const string LogPrefix = $"{nameof(VoicevoxService)}:";

    public VoicevoxService(IMcpLogger mcpLogger, IVoicevoxServiceOptions options)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");

        if (options == null)
        {
            var ex = new ArgumentNullException(nameof(options));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗", ex);
            throw ex;
        }

        _options = options;

        McpLogger?.Debug($"{LogPrefix} OpenJTalk 辞書パス解決中...");
        var openJTalkDictPath = ResolveOpenJTalkDictPath(options.ResourcePath);
        McpLogger?.Debug($"{LogPrefix} OpenJTalk 辞書パス: {openJTalkDictPath}");

        // OpenJTalk
        McpLogger?.Debug($"{LogPrefix} OpenJTalk 初期化中...");
        var result = OpenJtalk.New(openJTalkDictPath, out var openJtalk);
        EnsureOk(result, "OpenJTalk初期化失敗");

        // onnxruntime
        McpLogger?.Debug($"{LogPrefix} ONNX Runtime 初期化中...");
        result = Onnxruntime.LoadOnce(
            LoadOnnxruntimeOptions.Default(),
            out var onnxruntime);
        EnsureOk(result, "ONNX Runtime初期化失敗");

        // Synthesizer
        McpLogger?.Debug($"{LogPrefix} Synthesizer 初期化中...");
        result = Synthesizer.New(
            onnxruntime,
            openJtalk,
            InitializeOptions.Default(),
            out _synthesizer);
        EnsureOk(result, "Synthesizer初期化失敗");

        // Voice models
        var modelDir = Path.Combine(options.ResourcePath, "model");
        McpLogger?.Debug($"{LogPrefix} 音声モデルディレクトリ: {modelDir}");

        var matcher = new Matcher();
        matcher.AddIncludePatterns(new[] { "*.vvm" });

        var allModelPaths = matcher
            .GetResultsInFullPath(modelDir)
            .ToList();

        McpLogger?.Debug($"{LogPrefix} 検出された音声モデル数: {allModelPaths.Count}件");

        IEnumerable<string> modelPathsToLoad;

        if (options.VoiceModelNames is null || options.VoiceModelNames.Count == 0)
        {
            // 全読み込み（従来どおり）
            modelPathsToLoad = allModelPaths;
            McpLogger?.Info($"{LogPrefix} すべての音声モデルを読み込みます: {allModelPaths.Count}件");
        }
        else
        {
            // 指定モデルのみ
            var normalizedNames = options.VoiceModelNames
                .Select(n => Path.GetFileNameWithoutExtension(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            modelPathsToLoad = allModelPaths
                .Where(path =>
                    normalizedNames.Contains(
                        Path.GetFileNameWithoutExtension(path)));

            McpLogger?.Info($"{LogPrefix} 指定された音声モデルを読み込みます: {string.Join(", ", options.VoiceModelNames)}");
        }

        if (!modelPathsToLoad.Any())
        {
            var ex = new InvalidOperationException(
                "No matching voice models (*.vvm) were found. " +
                "Please check VoiceModelNames in VoicevoxServiceOptions.");
            McpLogger?.Critical($"{LogPrefix} 初期化失敗: 音声モデルが見つかりません", ex);
            throw ex;
        }

        McpLogger?.Info($"{LogPrefix} 音声モデルを読み込み中: {modelPathsToLoad.Count()}件");
        var loadedCount = 0;
        foreach (var path in modelPathsToLoad)
        {
            McpLogger?.Debug($"{LogPrefix} 音声モデル読み込み中: {Path.GetFileName(path)}");
            result = VoiceModelFile.Open(path, out var voiceModel);
            EnsureOk(result, $"音声モデル読み込み失敗: {Path.GetFileName(path)}");

            result = _synthesizer.LoadVoiceModel(voiceModel);
            EnsureOk(result, $"音声モデルロード失敗: {Path.GetFileName(path)}");

            voiceModel.Dispose();
            loadedCount++;
        }

        McpLogger?.Info($"{LogPrefix} 音声モデル読み込み完了: {loadedCount}件");

        openJtalk.Dispose();

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    public async Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Info($"{LogPrefix} GenerateVoiceFileAsync 開始: text={request.Text.Length}文字, outputWavFileName={request.OutputWavFileName}");

        McpLogger?.Debug($"{LogPrefix} GenerateVoiceFileAsync: 作業ディレクトリ作成中...");
        var outputDir = _options.CreateWorkDirectory(_options.VoicevoxOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));
        McpLogger?.Debug($"{LogPrefix} GenerateVoiceFileAsync: outputDir={outputDir}");

        var outputWavPath = await SynthesizeToFileAsync(
            request.Text,
            outputDir,
            request.OutputWavFileName,
            styleId,
            cancellationToken);

        McpLogger?.Info($"{LogPrefix} GenerateVoiceFileAsync 完了: outputWavPath={outputWavPath}");
        return outputWavPath;
    }

    public async Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
    IEnumerable<IGenerateVoiceRequest> requests,
    uint? styleId = null,
    CancellationToken cancellationToken = default)
    {
        var requestList = requests.ToList();
        McpLogger?.Info($"{LogPrefix} GenerateVoiceFilesAsync 開始: リクエスト数={requestList.Count}件");

        McpLogger?.Debug($"{LogPrefix} GenerateVoiceFilesAsync: 作業ディレクトリ作成中...");
        var outputDir = _options.CreateWorkDirectory(_options.VoicevoxOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));
        McpLogger?.Debug($"{LogPrefix} GenerateVoiceFilesAsync: outputDir={outputDir}");

        var outputPaths = new List<string>();

        var index = 0;
        foreach (var request in requestList)
        {
            index++;
            McpLogger?.Debug($"{LogPrefix} GenerateVoiceFilesAsync: 音声合成中 ({index}/{requestList.Count}): {request.OutputWavFileName}");

            var outputWavPath = await SynthesizeToFileAsync(
                request.Text,
                outputDir,
                request.OutputWavFileName,
                styleId,
                cancellationToken);
            outputPaths.Add(outputWavPath);
        }

        McpLogger?.Info($"{LogPrefix} GenerateVoiceFilesAsync 完了: {outputPaths.Count}件の音声ファイルを生成");
        return outputPaths;
    }

    private async Task<byte[]> SynthesizeAsync(
        string text,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Debug($"{LogPrefix} SynthesizeAsync: 音声合成開始: text={text.Length}文字, styleId={styleId}");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var sid = styleId ?? _options.DefaultStyleId;
            McpLogger?.Debug($"{LogPrefix} SynthesizeAsync: 使用するスタイルID={sid}");

            var result = _synthesizer.Tts(
                text,
                sid,
                TtsOptions.Default(),
                out _,
                out var wav);

            EnsureOk(result, "音声合成失敗");

            McpLogger?.Debug($"{LogPrefix} SynthesizeAsync: 音声合成完了: サイズ={wav!.Length}バイト");
            return wav!;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<string> SynthesizeToFileAsync(
        string text,
        string outputDir,
        string outputWavFileName,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Debug($"{LogPrefix} SynthesizeToFileAsync: 開始: text={text.Length}文字, outputWavFileName={outputWavFileName}");

        if (string.IsNullOrWhiteSpace(outputWavFileName))
        {
            var ex = new ArgumentException(
                "OutputWavFileName must be specified",
                nameof(outputWavFileName));
            McpLogger?.Critical($"{LogPrefix} SynthesizeToFileAsync: 出力ファイル名が指定されていません", ex);
            throw ex;
        }

        var wav = await SynthesizeAsync(text, styleId, cancellationToken);

        var outputWavPath = Path.Combine(outputDir, outputWavFileName);
        McpLogger?.Debug($"{LogPrefix} SynthesizeToFileAsync: ファイル書き込み中: {outputWavPath}");

        await File.WriteAllBytesAsync(outputWavPath, wav, cancellationToken);

        McpLogger?.Debug($"{LogPrefix} SynthesizeToFileAsync: 完了: {outputWavPath}");
        return outputWavPath;
    }

    private void EnsureOk(ResultCode result, string errorContext = "処理失敗")
    {
        if (result != ResultCode.RESULT_OK)
        {
            var message = result.ToMessage();
            var ex = new InvalidOperationException($"{errorContext}: {message}");
            McpLogger?.Critical($"{LogPrefix} {errorContext}: resultCode={result}, message={message}", ex);
            throw ex;
        }
    }

    private string ResolveOpenJTalkDictPath(string resourcePath)
    {
        McpLogger?.Debug($"{LogPrefix} ResolveOpenJTalkDictPath: resourcePath={resourcePath}");

        var baseDir = Path.Combine(
            resourcePath,
            "engine_internal",
            "pyopenjtalk"
        );

        McpLogger?.Debug($"{LogPrefix} ResolveOpenJTalkDictPath: baseDir={baseDir}");

        if (!Directory.Exists(baseDir))
        {
            var ex = new DirectoryNotFoundException(
                $"pyopenjtalk directory not found: {baseDir}");
            McpLogger?.Critical($"{LogPrefix} ResolveOpenJTalkDictPath: pyopenjtalk ディレクトリが見つかりません: {baseDir}", ex);
            throw ex;
        }

        var dictDirs = Directory.EnumerateDirectories(
            baseDir,
            "open_jtalk_dic_utf_8-*",
            SearchOption.TopDirectoryOnly
        ).ToList();

        McpLogger?.Debug($"{LogPrefix} ResolveOpenJTalkDictPath: 検出された辞書ディレクトリ数={dictDirs.Count}");

        if (dictDirs.Count == 0)
        {
            var ex = new DirectoryNotFoundException(
                $"open_jtalk dictionary not found under: {baseDir}");
            McpLogger?.Critical($"{LogPrefix} ResolveOpenJTalkDictPath: open_jtalk 辞書が見つかりません: {baseDir}", ex);
            throw ex;
        }

        if (dictDirs.Count > 1)
        {
            McpLogger?.Warn($"{LogPrefix} ResolveOpenJTalkDictPath: 複数の辞書ディレクトリが見つかりました。最初のものを使用します。");
        }

        var selectedPath = dictDirs[0];
        McpLogger?.Debug($"{LogPrefix} ResolveOpenJTalkDictPath: 選択された辞書パス={selectedPath}");
        return selectedPath;
    }

    public void Dispose()
    {
        McpLogger?.Debug($"{LogPrefix} Dispose: リソース解放中...");
        _synthesizer.Dispose();
        _gate.Dispose();
        McpLogger?.Debug($"{LogPrefix} Dispose: リソース解放完了");
    }
}
