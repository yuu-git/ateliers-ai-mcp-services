using Ateliers.Ai.Mcp.Services.GenericModels;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Text.RegularExpressions;
using VoicevoxCoreSharp.Core;
using VoicevoxCoreSharp.Core.Enum;
using VoicevoxCoreSharp.Core.Struct;

namespace Ateliers.Ai.Mcp.Services.Voicevox;

public sealed class VoicevoxSpeechService :
    IVoicevoxSpeechService, IDisposable
{
    private readonly Synthesizer _synthesizer;
    private readonly uint _defaultStyleId;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public VoicevoxSpeechService(VoicevoxServiceOptions options)
    {
        _defaultStyleId = options.DefaultStyleId;

        var openJTalkDictPath =
            Path.Combine(options.ResourcePath, "open_jtalk_dic_utf_8-1.11");

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
        var matcher = new Matcher();
        matcher.AddInclude("*.vvm");

        foreach (var path in matcher.GetResultsInFullPath(
                     Path.Combine(options.ResourcePath, "model")))
        {
            result = VoiceModelFile.Open(path, out var voiceModel);
            EnsureOk(result);

            result = _synthesizer.LoadVoiceModel(voiceModel);
            EnsureOk(result);

            voiceModel.Dispose();
        }

        openJtalk.Dispose();
    }

    public async Task<byte[]> SynthesizeAsync(
        string text,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var sid = styleId ?? _defaultStyleId;

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

    public async Task<string> SynthesizeToFileAsync(
        string text,
        string outputWavPath,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        var wav = await SynthesizeAsync(text, styleId, cancellationToken);

        Directory.CreateDirectory(
            Path.GetDirectoryName(outputWavPath)!);

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

    public void Dispose()
    {
        _synthesizer.Dispose();
        _gate.Dispose();
    }
}
