using Ateliers.Ai.Mcp.Services.GenericModels;
using NAudio.Wave;

namespace Ateliers.Ai.Mcp.Services.PresentationVideo;

public sealed class PresentationVideoService : McpServiceBase, IPresentationVideoGenerator
{
    private readonly IPresentationVideoOptions _options;
    private readonly IGenerateVoiceService _voiceService;
    private readonly IGenerateSlideService _slideService;
    private readonly IMediaComposerService _mediaComposerService;

    public PresentationVideoService(
        IMcpLogger mcpLogger,
        IPresentationVideoOptions options,
        IGenerateVoiceService voiceService,
        IGenerateSlideService slideService,
        IMediaComposerService mediaComposerService)
        : base(mcpLogger)
    {
        _options = options;
        _slideService = slideService;
        _voiceService = voiceService;
        _mediaComposerService = mediaComposerService;
    }

    public async Task<PresentationVideoResult> GenerateAsync(
        PresentationVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        // === 1. Markdown → Slide Markdown ===
        var slideMarkdown = _slideService.GenerateSlideMarkdown(request.SourceMarkdown);

        // === 2. Slide Markdown → PNG ===
        var slideImages = await _slideService.RenderToPngAsync(
            slideMarkdown,
            cancellationToken);

        if (slideImages.Count != request.NarrationTexts.Count)
        {
            throw new InvalidOperationException(
                "Slide count and narration count must match.");
        }

        // === 3. Voice generation ===
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

        var voiceFiles = await _voiceService.GenerateVoiceFilesAsync(
            voiceRequests,
            cancellationToken: cancellationToken);

        // === 4. Media composition ===
        var outputFileName = request.OutputFileName ?? "output.mp4";

        var slideAudioPairs = new List<SlideAudioPair>();

        for (var i = 0; i < voiceFiles.Count; i++)
        { 
            var voiceFile = voiceFiles[i];
            var slideImage = slideImages[i];
            var duration = GetWavDurationSeconds(voiceFile) + 0.1d;
            slideAudioPairs.Add(new SlideAudioPair(slideImage, voiceFile, duration));
        }
        var composerRequest = new GenerateVideoRequest(slideAudioPairs);

        var videoPath = await _mediaComposerService.ComposeAsync(composerRequest, outputFileName);

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

