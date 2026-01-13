using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.Ai.Mcp.Services.Voicevox;
using Ateliers.Voice.Engines.VoicevoxTools;
using Moq;
using Xunit;

namespace Ateliers.Ai.Mcp.Services.Voicevox.UnitTests;

/// <summary>
/// VoicevoxService の単体テスト（モックベース）
/// 実際のファイル生成は行わず、ロジックのみをテスト
/// </summary>
public sealed class VoicevoxServiceTests
{
    [Fact(DisplayName = "音声ファイル生成時に正しいパラメーターでジェネレーターが呼び出されることを確認")]
    public async Task GenerateVoiceFileAsync_CallsGeneratorWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };

        var expectedOutputPath = "/output/test.wav";
        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicevoxGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicevoxGenerateResult
            {
                OutputWavPath = expectedOutputPath,
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "テスト音声です。",
            OutputWavFileName = "test.wav",
            Options = new VoicevoxMcpGenerationOptions
            {
                StyleId = 1,
                SpeedScale = 1.2f
            }
        };

        // Act
        var result = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.Equal(expectedOutputPath, result);
        
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicevoxGenerateRequest>(r =>
                    r.Text == "テスト音声です。" &&
                    r.OutputWavFileName == "test.wav" &&
                    r.Options!.StyleId == 1 &&
                    r.Options.SpeedScale == 1.2f),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "オプションがnullの場合にジェネレーターがnullオプションで呼び出されることを確認")]
    public async Task GenerateVoiceFileAsync_WithNullOptions_CallsGeneratorWithNullOptions()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicevoxGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicevoxGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "テスト音声です。",
            OutputWavFileName = "test.wav",
            Options = null
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicevoxGenerateRequest>(r => r.Options == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "複数のリクエストでジェネレーターが呼び出されることを確認")]
    public async Task GenerateVoiceFilesAsync_CallsGeneratorWithMultipleRequests()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };

        var expectedResults = new List<VoicevoxGenerateResult>
        {
            new() { OutputWavPath = "/output/test1.wav", Elapsed = TimeSpan.FromSeconds(1) },
            new() { OutputWavPath = "/output/test2.wav", Elapsed = TimeSpan.FromSeconds(1) },
            new() { OutputWavPath = "/output/test3.wav", Elapsed = TimeSpan.FromSeconds(1) }
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFilesAsync(
                It.IsAny<IEnumerable<VoicevoxGenerateRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        var requests = new[]
        {
            new GenerateVoiceRequest
            {
                Text = "最初の音声です。",
                OutputWavFileName = "test1.wav",
                Options = new VoicevoxMcpGenerationOptions { StyleId = 1 }
            },
            new GenerateVoiceRequest
            {
                Text = "2番目の音声です。",
                OutputWavFileName = "test2.wav",
                Options = new VoicevoxMcpGenerationOptions { StyleId = 2 }
            },
            new GenerateVoiceRequest
            {
                Text = "3番目の音声です。",
                OutputWavFileName = "test3.wav",
                Options = new VoicevoxMcpGenerationOptions { StyleId = 1 }
            }
        };

        // Act
        var results = await service.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("/output/test1.wav", results[0]);
        Assert.Equal("/output/test2.wav", results[1]);
        Assert.Equal("/output/test3.wav", results[2]);

        mockGenerator.Verify(
            g => g.GenerateVoiceFilesAsync(
                It.Is<IEnumerable<VoicevoxGenerateRequest>>(reqs => reqs.Count() == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "オプション変換時にすべてのプロパティがマッピングされることを確認")]
    public async Task GenerateVoiceFileAsync_OptionsConversion_AllPropertiesAreMapped()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicevoxGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicevoxGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "完全なオプションテストです。",
            OutputWavFileName = "full_options.wav",
            Options = new VoicevoxMcpGenerationOptions
            {
                StyleId = 5,
                SpeedScale = 1.5f,
                PitchScale = 0.5f,
                IntonationScale = 1.2f,
                VolumeScale = 0.8f,
                PrePhonemeLength = 0.1f,
                PostPhonemeLength = 0.2f,
                TextFileSaveMode = TextFileSaveMode.WithMetadata
            }
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicevoxGenerateRequest>(r =>
                    r.Options!.StyleId == 5 &&
                    r.Options.SpeedScale == 1.5f &&
                    r.Options.PitchScale == 0.5f &&
                    r.Options.IntonationScale == 1.2f &&
                    r.Options.VolumeScale == 0.8f &&
                    r.Options.PrePhonemeLength == 0.1f &&
                    r.Options.PostPhonemeLength == 0.2f &&
                    r.Options.TextFileSaveMode == Voice.Engines.TextFileSaveMode.WithMetadata),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Dispose時にジェネレーターのDisposeが呼び出されることを確認")]
    public void Dispose_CallsGeneratorDispose()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        service.Dispose();

        // Assert
        mockGenerator.Verify(g => g.Dispose(), Times.Once);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ナレッジコンテンツが存在しない場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNoKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox",
            VoicevoxKnowledgeOptions = new List<VoicevoxGenerationKnowledgeOptions>()
        };

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("VOICEVOX MCP ナレッジ", contents[0]);
        Assert.Contains("現在、VOICEVOX サービスにはナレッジコンテンツが設定されていません", contents[0]);
        
        mockLogger.Verify(
            l => l.Warn(It.Is<string>(s => s.Contains("現在ナレッジコンテンツが存在しません"))),
            Times.Once);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: nullのナレッジオプションの場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNullKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox"
        };

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("VOICEVOX MCP ナレッジ", contents[0]);
        Assert.Contains("現在、VOICEVOX サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ベースクラスがナレッジを返す場合にそれを使用すること")]
    public void GetServiceKnowledgeContents_WithKnowledgeFromBase_ReturnsKnowledge()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "# テストナレッジ\n\nこれはVOICEVOX用のテストナレッジコンテンツです。");

        try
        {
            var options = new VoicevoxServiceOptions
            {
                ResourcePath = "/dummy/path",
                VoiceModelNames = new[] { "0.vmm" },
                VoicevoxOutputDirectoryName = "voicevox",
                VoicevoxKnowledgeOptions = new List<VoicevoxGenerationKnowledgeOptions>
                {
                    new VoicevoxGenerationKnowledgeOptions
                    {
                        KnowledgeType = "LocalFile",
                        KnowledgeSource = tempFile,
                        DocumentType = "Markdown",
                        GenerateHeader = false
                    }
                }
            };

            var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

            // Act
            var contents = service.GetServiceKnowledgeContents().ToList();

            // Assert
            Assert.Single(contents);
            Assert.Contains("テストナレッジ", contents[0]);
            Assert.Contains("VOICEVOX用のテストナレッジコンテンツです", contents[0]);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ベースクラスが空のコレクションを返す場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithEmptyKnowledgeFromBase_ReturnsDefaultMessage()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicevoxVoiceGenerator>();
        
        var options = new VoicevoxServiceOptions
        {
            ResourcePath = "/dummy/path",
            VoiceModelNames = new[] { "0.vmm" },
            VoicevoxOutputDirectoryName = "voicevox",
            VoicevoxKnowledgeOptions = new List<VoicevoxGenerationKnowledgeOptions>()
        };

        var service = new VoicevoxService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("VOICEVOX MCP ナレッジ", contents[0]);
        Assert.Contains("現在、VOICEVOX サービスにはナレッジコンテンツが設定されていません", contents[0]);
        
        mockLogger.Verify(
            l => l.Warn(It.Is<string>(s => s.Contains("現在ナレッジコンテンツが存在しません"))),
            Times.Once);
    }
}
