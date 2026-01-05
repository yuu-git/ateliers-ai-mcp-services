using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.Ai.Mcp.Services.Voicevox;
using Xunit;

namespace Ateliers.Ai.Mcp.Services.Voicevox.UnitTests;

public sealed class VoicevoxServiceTests
{
    static VoicevoxServiceTests()
    {
        var path = @"C:\Program Files\VOICEVOX\vv-engine";
        if (Directory.Exists(path))
        {
            NativeLibraryPath.Use(path);
        }
    }

    // ★ 環境に合わせて書き換えてください
    private const string ResourcePath =
        @"C:\Program Files\VOICEVOX\vv-engine";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WavFileIsGenerated()
    {
        // Arrange
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };
        var loggger = new InMemoryMcpLogger(new McpLoggerOptions());

        using var service = new VoicevoxService(loggger, options);
        var request = new GenerateVoiceRequest
        {
            Text = "これはテスト音声です。",
            OutputWavFileName = "test_output.wav",
            Options = new VoicevoxGenerationOptions
            {
                StyleId = 1
            }
        };

        // Act
        var resultPath = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(resultPath));

        var fileInfo = new FileInfo(resultPath);
        Assert.True(fileInfo.Length > 0);

        // Optional: wav ヘッダ確認（超軽量チェック）
        using var fs = File.OpenRead(resultPath);
        var header = new byte[4];
        await fs.ReadAsync(header, 0, 4);

        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header));

        // テキストファイルが生成されていることを確認（デフォルト: TextOnly）
        var textFilePath = Path.Combine(Path.GetDirectoryName(resultPath)!, "test_output.txt");
        Assert.True(File.Exists(textFilePath));

        var savedText = await File.ReadAllTextAsync(textFilePath);
        Assert.Equal("これはテスト音声です。", savedText);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithMetadata_JsonFileIsGenerated()
    {
        // Arrange
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };
        var loggger = new InMemoryMcpLogger(new McpLoggerOptions());

        using var service = new VoicevoxService(loggger, options);
        var request = new GenerateVoiceRequest
        {
            Text = "メタデータ付きテスト音声です。",
            OutputWavFileName = "test_metadata.wav",
            Options = new VoicevoxGenerationOptions
            {
                StyleId = 2,
                SpeedScale = 1.2f,
                TextFileSaveMode = TextFileSaveMode.WithMetadata
            }
        };

        // Act
        var resultPath = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(resultPath));

        // JSON ファイルが生成されていることを確認
        var jsonFilePath = Path.Combine(Path.GetDirectoryName(resultPath)!, "test_metadata.json");
        Assert.True(File.Exists(jsonFilePath));

        var json = await File.ReadAllTextAsync(jsonFilePath);
        Assert.Contains("メタデータ付きテスト音声です。", json);
        Assert.Contains("\"service\": \"Voicevox\"", json);
        Assert.Contains("\"styleId\": 2", json);
        Assert.Contains("\"speedScale\": 1.2", json);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithNone_NoTextFileIsGenerated()
    {
        // Arrange
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };
        var loggger = new InMemoryMcpLogger(new McpLoggerOptions());

        using var service = new VoicevoxService(loggger, options);
        var request = new GenerateVoiceRequest
        {
            Text = "テキストファイルなしのテスト音声です。",
            OutputWavFileName = "test_no_text.wav",
            Options = new VoicevoxGenerationOptions
            {
                StyleId = 1,
                TextFileSaveMode = TextFileSaveMode.None
            }
        };

        // Act
        var resultPath = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(resultPath));

        // テキストファイルが生成されていないことを確認
        var textFilePath = Path.Combine(Path.GetDirectoryName(resultPath)!, "test_no_text.txt");
        Assert.False(File.Exists(textFilePath));

        var jsonFilePath = Path.Combine(Path.GetDirectoryName(resultPath)!, "test_no_text.json");
        Assert.False(File.Exists(jsonFilePath));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFilesAsync_MultipleWavFilesAreGenerated()
    {
        // Arrange
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };
        var loggger = new InMemoryMcpLogger(new McpLoggerOptions());

        using var service = new VoicevoxService(loggger, options);
        var requests = new[]
        {
            new GenerateVoiceRequest
            {
                Text = "最初の音声です。",
                OutputWavFileName = "multi_01.wav",
                Options = new VoicevoxGenerationOptions
                {
                    StyleId = 1
                }
            },
            new GenerateVoiceRequest
            {
                Text = "2番目の音声です。",
                OutputWavFileName = "multi_02.wav",
                Options = new VoicevoxGenerationOptions
                {
                    StyleId = 1
                }
            },
            new GenerateVoiceRequest
            {
                Text = "3番目の音声です。",
                OutputWavFileName = "multi_03.wav",
                Options = new VoicevoxGenerationOptions
                {
                    StyleId = 1
                }
            }
        };

        // Act
        var resultPaths = await service.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, resultPaths.Count);

        // すべての WAV ファイルが生成されていることを確認
        foreach (var resultPath in resultPaths)
        {
            Assert.True(File.Exists(resultPath));
            
            var fileInfo = new FileInfo(resultPath);
            Assert.True(fileInfo.Length > 0);
        }

        // すべてのテキストファイルが生成されていることを確認（デフォルト: TextOnly）
        var expectedTexts = new[]
        {
            ("multi_01.txt", "最初の音声です。"),
            ("multi_02.txt", "2番目の音声です。"),
            ("multi_03.txt", "3番目の音声です。")
        };

        var outputDir = Path.GetDirectoryName(resultPaths[0])!;
        foreach (var (fileName, expectedText) in expectedTexts)
        {
            var textFilePath = Path.Combine(outputDir, fileName);
            Assert.True(File.Exists(textFilePath));

            var savedText = await File.ReadAllTextAsync(textFilePath);
            Assert.Equal(expectedText, savedText);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFilesAsync_WithDifferentModes_FilesAreGeneratedCorrectly()
    {
        // Arrange
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };
        var loggger = new InMemoryMcpLogger(new McpLoggerOptions());

        using var service = new VoicevoxService(loggger, options);
        var requests = new[]
        {
            new GenerateVoiceRequest
            {
                Text = "テキストのみ保存。",
                OutputWavFileName = "mode_text.wav",
                Options = new VoicevoxGenerationOptions
                {
                    StyleId = 1,
                    TextFileSaveMode = TextFileSaveMode.TextOnly
                }
            },
            new GenerateVoiceRequest
            {
                Text = "メタデータ付き保存。",
                OutputWavFileName = "mode_metadata.wav",
                Options = new VoicevoxGenerationOptions
                {
                    StyleId = 2,
                    SpeedScale = 1.1f,
                    TextFileSaveMode = TextFileSaveMode.WithMetadata
                }
            },
            new GenerateVoiceRequest
            {
                Text = "保存なし。",
                OutputWavFileName = "mode_none.wav",
                Options = new VoicevoxGenerationOptions
                {
                    StyleId = 1,
                    TextFileSaveMode = TextFileSaveMode.None
                }
            }
        };

        // Act
        var resultPaths = await service.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, resultPaths.Count);

        var outputDir = Path.GetDirectoryName(resultPaths[0])!;

        // TextOnly: .txt ファイルのみ存在
        var textOnlyTxtPath = Path.Combine(outputDir, "mode_text.txt");
        Assert.True(File.Exists(textOnlyTxtPath));
        var textOnlyJsonPath = Path.Combine(outputDir, "mode_text.json");
        Assert.False(File.Exists(textOnlyJsonPath));

        // WithMetadata: .json ファイルのみ存在
        var metadataJsonPath = Path.Combine(outputDir, "mode_metadata.json");
        Assert.True(File.Exists(metadataJsonPath));
        var metadataTxtPath = Path.Combine(outputDir, "mode_metadata.txt");
        Assert.False(File.Exists(metadataTxtPath));

        var json = await File.ReadAllTextAsync(metadataJsonPath);
        Assert.Contains("メタデータ付き保存。", json);
        Assert.Contains("\"styleId\": 2", json);
        Assert.Contains("\"speedScale\": 1.1", json);

        // None: テキストファイルなし
        var noneTxtPath = Path.Combine(outputDir, "mode_none.txt");
        Assert.False(File.Exists(noneTxtPath));
        var noneJsonPath = Path.Combine(outputDir, "mode_none.json");
        Assert.False(File.Exists(noneJsonPath));
    }
}
