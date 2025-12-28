using Ateliers.Ai.Mcp.Services.GenericModels;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Text.RegularExpressions;
using VoicevoxCoreSharp.Core;
using VoicevoxCoreSharp.Core.Enum;
using VoicevoxCoreSharp.Core.Struct;

namespace Ateliers.Ai.Mcp.Services.Voicevox;

public sealed class VoicevoxService :
    IGenerateVoiceService, IDisposable
{
    private readonly IVoicevoxServiceOptions _options;
    private readonly Synthesizer _synthesizer;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public VoicevoxService(IVoicevoxServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        var openJTalkDictPath = ResolveOpenJTalkDictPath(options.ResourcePath);

        // OpenJTalk
        var result = OpenJtalk.New(openJTalkDictPath, out var openJtalk);
        EnsureOk(result);

        // onnxruntime
        result = Onnxruntime.LoadOnce(
            LoadOnnxruntimeOptions.Default(),
            out var onnxruntime);
        EnsureOk(result);

        // Synthesizer
        result = Synthesizer.New(
            onnxruntime,
            openJtalk,
            InitializeOptions.Default(),
            out _synthesizer);
        EnsureOk(result);

        // Voice models
        var modelDir = Path.Combine(options.ResourcePath, "model");

        var matcher = new Matcher();
        matcher.AddIncludePatterns(new[] { "*.vvm" });

        var allModelPaths = matcher
            .GetResultsInFullPath(modelDir)
            .ToList();

        IEnumerable<string> modelPathsToLoad;

        if (options.VoiceModelNames is null || options.VoiceModelNames.Count == 0)
        {
            // 全読み込み（従来どおり）
            modelPathsToLoad = allModelPaths;
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
        }

        if (!modelPathsToLoad.Any())
        {
            throw new InvalidOperationException(
                "No matching voice models (*.vvm) were found. " +
                "Please check VoiceModelNames in VoicevoxServiceOptions.");
        }

        foreach (var path in modelPathsToLoad)
        {
            result = VoiceModelFile.Open(path, out var voiceModel);
            EnsureOk(result);

            result = _synthesizer.LoadVoiceModel(voiceModel);
            EnsureOk(result);

            voiceModel.Dispose();
        }

        openJtalk.Dispose();
    }

    public async Task<string> GenerateVoiceFileAsync(
        IGenerateVoiceRequest request,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        var outputDir = _options.CreateWorkDirectory(_options.VoicevoxOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));

        var outputWavPath = await SynthesizeToFileAsync(
            request.Text,
            outputDir,
            request.OutputWavFileName,
            styleId,
            cancellationToken);

        return outputWavPath;
    }

    public async Task<IReadOnlyList<string>> GenerateVoiceFilesAsync(
    IEnumerable<IGenerateVoiceRequest> requests,
    uint? styleId = null,
    CancellationToken cancellationToken = default)
    {
        var outputDir = _options.CreateWorkDirectory(_options.VoicevoxOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));

        var outputPaths = new List<string>();

        foreach (var request in requests)
        {
            var outputWavPath = await SynthesizeToFileAsync(
                request.Text,
                outputDir,
                request.OutputWavFileName,
                styleId,
                cancellationToken);
            outputPaths.Add(outputWavPath);
        }

        return outputPaths;
    }

    private async Task<byte[]> SynthesizeAsync(
        string text,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var sid = styleId ?? _options.DefaultStyleId;

            var result = _synthesizer.Tts(
                text,
                sid,
                TtsOptions.Default(),
                out _,
                out var wav);

            EnsureOk(result);

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
        if (string.IsNullOrWhiteSpace(outputWavFileName))
        {
            throw new ArgumentException(
                "OutputWavFileName must be specified",
                nameof(outputWavFileName));
        }

        var wav = await SynthesizeAsync(text, styleId, cancellationToken);

        var outputWavPath = Path.Combine(outputDir, outputWavFileName);

        await File.WriteAllBytesAsync(outputWavPath, wav, cancellationToken);

        return outputWavPath;
    }

    private static void EnsureOk(ResultCode result)
    {
        if (result != ResultCode.RESULT_OK)
        {
            throw new InvalidOperationException(result.ToMessage());
        }
    }
    private static string ResolveOpenJTalkDictPath(string resourcePath)
    {
        var baseDir = Path.Combine(
            resourcePath,
            "engine_internal",
            "pyopenjtalk"
        );

        if (!Directory.Exists(baseDir))
        {
            throw new DirectoryNotFoundException(
                $"pyopenjtalk directory not found: {baseDir}");
        }

        var dictDirs = Directory.EnumerateDirectories(
            baseDir,
            "open_jtalk_dic_utf_8-*",
            SearchOption.TopDirectoryOnly
        ).ToList();

        if (dictDirs.Count == 0)
        {
            throw new DirectoryNotFoundException(
                $"open_jtalk dictionary not found under: {baseDir}");
        }

        if (dictDirs.Count > 1)
        {
            // 将来の保険：一応警告だけ出す
            // （基本は1つしか存在しない）
            // Console.WriteLine("Multiple open_jtalk dictionaries found. Using the first one.");
        }

        return dictDirs[0];
    }

    public void Dispose()
    {
        _synthesizer.Dispose();
        _gate.Dispose();
    }
}
