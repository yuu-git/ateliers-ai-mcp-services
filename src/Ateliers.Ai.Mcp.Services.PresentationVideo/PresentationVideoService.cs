using Ateliers.Ai.Mcp.Services.GenericModels;
using NAudio.Wave;

namespace Ateliers.Ai.Mcp.Services.PresentationVideo;

public sealed class PresentationVideoService : McpServiceBase, IPresentationVideoGenerator
{
    private readonly IPresentationVideoOptions _options;
    private readonly IGenerateVoiceService _voiceService;
    private readonly IGenerateSlideService _slideService;
    private readonly IMediaComposerService _mediaComposerService;
    private const string LogPrefix = $"{nameof(PresentationVideoService)}:";

    public PresentationVideoService(
        IMcpLogger mcpLogger,
        IPresentationVideoOptions options,
        IGenerateVoiceService voiceService,
        IGenerateSlideService slideService,
        IMediaComposerService mediaComposerService)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");

        if (options == null)
        {
            var ex = new ArgumentNullException(nameof(options));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗: optionsがnull", ex);
            throw ex;
        }

        if (voiceService == null)
        {
            var ex = new ArgumentNullException(nameof(voiceService));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗: voiceServiceがnull", ex);
            throw ex;
        }

        if (slideService == null)
        {
            var ex = new ArgumentNullException(nameof(slideService));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗: slideServiceがnull", ex);
            throw ex;
        }

        if (mediaComposerService == null)
        {
            var ex = new ArgumentNullException(nameof(mediaComposerService));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗: mediaComposerServiceがnull", ex);
            throw ex;
        }

        _options = options;
        _slideService = slideService;
        _voiceService = voiceService;
        _mediaComposerService = mediaComposerService;

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// コンテンツ生成ガイドを取得します。
    /// </summary>
    /// <returns> 未実装（将来：指定する音声やスライドなどのマークダウン形式のガイド） </returns>
    public string GetContentGenerationGuide()
    {
        // ToDo: interface IMcpContentGenerationGuideProvider のガイド実装
        return
            "未実装：PresentationVideoService では、現在コンテンツ生成ガイドは提供されていません。" +
            "将来的には、指定する音声やスライドなどのマークダウン形式のガイドが提供される予定です。";
    }

    public async Task<PresentationVideoResult> GenerateAsync(
        PresentationVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Info($"{LogPrefix} GenerateAsync 開始: sourceMarkdown={request.SourceMarkdown.Length}文字, narrationCount={request.NarrationTexts.Count}件");

        // === 1. Markdown → Slide Markdown ===
        McpLogger?.Info($"{LogPrefix} GenerateAsync: ステップ1/4 - スライドMarkdown生成中...");
        var slideMarkdown = _slideService.GenerateSlideMarkdown(request.SourceMarkdown);
        McpLogger?.Debug($"{LogPrefix} GenerateAsync: スライドMarkdown生成完了: サイズ={slideMarkdown.Length}文字");

        // === 2. Slide Markdown → PNG ===
        McpLogger?.Info($"{LogPrefix} GenerateAsync: ステップ2/4 - スライド画像レンダリング中...");
        var slideImages = await _slideService.RenderToPngAsync(
            slideMarkdown,
            cancellationToken);
        McpLogger?.Info($"{LogPrefix} GenerateAsync: スライド画像レンダリング完了: {slideImages.Count}件");

        if (slideImages.Count != request.NarrationTexts.Count)
        {
            var ex = new InvalidOperationException(
                $"Slide count ({slideImages.Count}) and narration count ({request.NarrationTexts.Count}) must match.");
            McpLogger?.Critical($"{LogPrefix} GenerateAsync: スライド数とナレーション数が一致しません: slides={slideImages.Count}, narrations={request.NarrationTexts.Count}", ex);
            throw ex;
        }

        // === 3. Voice generation ===
        McpLogger?.Info($"{LogPrefix} GenerateAsync: ステップ3/4 - 音声合成中...");
        var voiceRequests = new List<IGenerateVoiceRequest>();

        for (var i = 0; i < request.NarrationTexts.Count; i++)
        {
            var narrationText = request.NarrationTexts[i];
            voiceRequests.Add(
                new GenerateVoiceRequest
                {
                    Text = narrationText,
                    OutputWavFileName = $"voice.{i + 1:D3}.wav"
                });
        }

        McpLogger?.Debug($"{LogPrefix} GenerateAsync: 音声リクエスト作成完了: {voiceRequests.Count}件");

        var voiceFiles = await _voiceService.GenerateVoiceFilesAsync(
            voiceRequests,
            cancellationToken: cancellationToken);

        McpLogger?.Info($"{LogPrefix} GenerateAsync: 音声合成完了: {voiceFiles.Count}件");

        // === 4. Media composition ===
        McpLogger?.Info($"{LogPrefix} GenerateAsync: ステップ4/4 - 動画合成中...");
        var outputFileName = request.OutputFileName ?? "output.mp4";
        McpLogger?.Debug($"{LogPrefix} GenerateAsync: 出力ファイル名={outputFileName}");

        var slideAudioPairs = new List<SlideAudioPair>();

        for (var i = 0; i < voiceFiles.Count; i++)
        { 
            var voiceFile = voiceFiles[i];
            var slideImage = slideImages[i];
            var duration = GetWavDurationSeconds(voiceFile) + 0.1d;
            
            McpLogger?.Debug($"{LogPrefix} GenerateAsync: スライド{i + 1}: duration={duration:F2}秒");
            slideAudioPairs.Add(new SlideAudioPair(slideImage, voiceFile, duration));
        }

        McpLogger?.Debug($"{LogPrefix} GenerateAsync: スライド・音声ペア作成完了: {slideAudioPairs.Count}件");

        var composerRequest = new GenerateVideoRequest(slideAudioPairs);

        var videoPath = await _mediaComposerService.ComposeAsync(composerRequest, outputFileName);

        McpLogger?.Info($"{LogPrefix} GenerateAsync 完了: videoPath={videoPath}");

        return new PresentationVideoResult
        {
            VideoPath = videoPath,
            SlideImages = slideImages,
            VoiceFiles = voiceFiles
        };
    }

    public static double GetWavDurationSeconds(string wavPath)
    {
        using var reader = new WaveFileReader(wavPath);
        return reader.TotalTime.TotalSeconds;
    }
}

