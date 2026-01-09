using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.VoicePeak.IntegrationTests;

/// <summary>
/// VoicePeakService の統合テスト
/// 実際のファイル生成を行うため、環境依存のテスト
/// GitHub Actions では実行されない
/// </summary>
public sealed class VoicePeakServiceIntegrationTests
{
    // ★ 環境に合わせて書き換えてください
    private const string VoicePeakExecutablePath =
        @"C:\Program Files\VOICEPEAK\voicepeak.exe";

    private const string DefaultNarrator = "夏色花梨";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WavFileIsGenerated()
    {
        // Arrange
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath,
            DefaultNarrator = DefaultNarrator,
            VoicePeakOutputDirectoryName = "voicepeak"
        };
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());

        var service = new VoicePeakService(logger, options);
        var request = new GenerateVoiceRequest
        {
            Text = "これはテスト音声です。",
            OutputWavFileName = "test_output.wav",
            Options = new VoicePeakMcpGenerationOptions
            {
                Narrator = DefaultNarrator,
                Speed = 100,
            }
        };

        // Act
        var resultPath = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(resultPath), $"ファイルが存在しません: {resultPath}");

        var fileInfo = new FileInfo(resultPath);
        Assert.True(fileInfo.Length > 0, "ファイルサイズが0です");

        // WAVヘッダー確認
        using var fs = File.OpenRead(resultPath);
        var header = new byte[4];
        await fs.ReadExactlyAsync(header, 0, 4);

        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithDifferentNarrator_GeneratesCorrectly()
    {
        // Arrange
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath,
            DefaultNarrator = DefaultNarrator,
            VoicePeakOutputDirectoryName = "voicepeak"
        };
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());

        var service = new VoicePeakService(logger, options);
        var request = new GenerateVoiceRequest
        {
            Text = "ナレーターを指定したテストです。",
            OutputWavFileName = "test_narrator.wav",
            Options = new VoicePeakMcpGenerationOptions
            {
                Narrator = DefaultNarrator,
                Speed = 120
            }
        };

        // Act
        var resultPath = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(resultPath));

        var fileInfo = new FileInfo(resultPath);
        Assert.True(fileInfo.Length > 0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFilesAsync_MultipleWavFilesAreGenerated()
    {
        // Arrange
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath,
            DefaultNarrator = DefaultNarrator,
            VoicePeakOutputDirectoryName = "voicepeak"
        };
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());

        var service = new VoicePeakService(logger, options);
        var requests = new[]
        {
            new GenerateVoiceRequest
            {
                Text = "最初の音声です。",
                OutputWavFileName = "multi_01.wav",
                Options = new VoicePeakMcpGenerationOptions { Narrator = DefaultNarrator }
            },
            new GenerateVoiceRequest
            {
                Text = "2番目の音声です。",
                OutputWavFileName = "multi_02.wav",
                Options = new VoicePeakMcpGenerationOptions { Narrator = DefaultNarrator }
            },
            new GenerateVoiceRequest
            {
                Text = "3番目の音声です。",
                OutputWavFileName = "multi_03.wav",
                Options = new VoicePeakMcpGenerationOptions { Narrator = DefaultNarrator }
            }
        };

        // Act
        var resultPaths = await service.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, resultPaths.Count);

        // すべてのWAVファイルが生成されていることを確認
        foreach (var resultPath in resultPaths)
        {
            Assert.True(File.Exists(resultPath), $"ファイルが存在しません: {resultPath}");
            
            var fileInfo = new FileInfo(resultPath);
            Assert.True(fileInfo.Length > 0, $"ファイルサイズが0です: {resultPath}");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithDifferentSpeeds_GeneratesCorrectly()
    {
        // Arrange
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath,
            DefaultNarrator = DefaultNarrator,
            VoicePeakOutputDirectoryName = "voicepeak"
        };
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());

        var service = new VoicePeakService(logger, options);
        var requests = new[]
        {
            new GenerateVoiceRequest
            {
                Text = "ゆっくり話します。",
                OutputWavFileName = "speed_slow.wav",
                Options = new VoicePeakMcpGenerationOptions 
                { 
                    Narrator = DefaultNarrator,
                    Speed = 80, 
                }
            },
            new GenerateVoiceRequest
            {
                Text = "通常速度で話します。",
                OutputWavFileName = "speed_normal.wav",
                Options = new VoicePeakMcpGenerationOptions 
                { 
                    Narrator = DefaultNarrator,
                    Speed = 100,
                }
            },
            new GenerateVoiceRequest
            {
                Text = "速く話します。",
                OutputWavFileName = "speed_fast.wav",
                Options = new VoicePeakMcpGenerationOptions 
                { 
                    Narrator = DefaultNarrator,
                    Speed = 150,
                }
            }
        };

        // Act
        var resultPaths = await service.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, resultPaths.Count);

        foreach (var resultPath in resultPaths)
        {
            Assert.True(File.Exists(resultPath));
            
            var fileInfo = new FileInfo(resultPath);
            Assert.True(fileInfo.Length > 0);
        }
    }
}
