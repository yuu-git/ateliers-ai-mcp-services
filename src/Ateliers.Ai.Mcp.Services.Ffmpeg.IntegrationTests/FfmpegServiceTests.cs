using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;
using NAudio.Wave;

namespace Ateliers.Ai.Mcp.Services.Ffmpeg.IntegrationTests;

public class FfmpegServiceTests
{
    public static double GetWavDurationSeconds(string wavPath)
    {
        using var reader = new WaveFileReader(wavPath);
        return reader.TotalTime.TotalSeconds;
    }

    [Fact(DisplayName = "ComposeAsync_シンプルな2スライドで動画が生成されること")]
    [Trait("Category", "Integration")]
    public async Task ComposeAsync_TwoSlides_CreatesVideo()
    {
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new FfmpegService(
            logger,
            new FfmpegServiceOptions
            {
                FfmpegExecutablePath = "C:\\Program Files\\FFmpeg\\bin\\ffmpeg.exe"
            });

        var d1 = GetWavDurationSeconds(".\\TestDatas\\P001\\voice.001.wav");
        var d2 = GetWavDurationSeconds(".\\TestDatas\\P001\\voice.002.wav");

        var request = new GenerateVideoRequest(
            [
                new SlideAudioPair(".\\TestDatas\\P001\\slide.001.png", ".\\TestDatas\\P001\\voice.001.wav", d1+0.1d),
                new SlideAudioPair(".\\TestDatas\\P001\\slide.002.png", ".\\TestDatas\\P001\\voice.002.wav", d2+0.1d),
            ]);

        var video = await service.ComposeAsync(request, "output.mp4");

        Assert.True(File.Exists(video));
    }
}
