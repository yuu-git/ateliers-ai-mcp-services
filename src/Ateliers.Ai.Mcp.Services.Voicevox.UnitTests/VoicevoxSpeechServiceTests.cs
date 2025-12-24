using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.Ai.Mcp.Services.Voicevox;
using Xunit;

namespace Ateliers.Ai.Mcp.Services.Voicevox.UnitTests;

public sealed class VoicevoxSpeechServiceTests
{
    // ★ 環境に合わせて書き換えてください
    private const string ResourcePath =
        @"C:\Program Files\VOICEVOX\vv-engine\voicevox_core.dll";

    [Fact]
    public async Task SynthesizeToFileAsync_WavFileIsGenerated()
    {
        // Arrange
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = ResourcePath,
            DefaultStyleId = 0
        };

        using var service = new VoicevoxSpeechService(options);

        var outputDir = Path.Combine(
            Path.GetTempPath(),
            "voicevox_test");

        var outputPath = Path.Combine(
            outputDir,
            "test.wav");

        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Act
        var resultPath = await service.SynthesizeToFileAsync(
            text: "これはテスト音声です。",
            outputWavPath: outputPath);

        // Assert
        Assert.Equal(outputPath, resultPath);
        Assert.True(File.Exists(outputPath));

        var fileInfo = new FileInfo(outputPath);
        Assert.True(fileInfo.Length > 0);

        // Optional: wav ヘッダ確認（超軽量チェック）
        using var fs = File.OpenRead(outputPath);
        var header = new byte[4];
        await fs.ReadAsync(header, 0, 4);

        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header));
    }
}
