using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.Ai.Mcp.Services.VoicePeak;
using Ateliers.Voice.Engines.VoicePeakTools;
using Moq;
using Xunit;

namespace Ateliers.Ai.Mcp.Services.VoicePeak.UnitTests;

/// <summary>
/// VoicePeakService の単体テスト（モックベース）
/// 実際のファイル生成は行わず、ロジックのみをテスト
/// </summary>
public sealed class VoicePeakServiceTests
{
    [Fact(DisplayName = "音声ファイル生成時に正しいパラメーターでジェネレーターが呼び出されることを確認")]
    public async Task GenerateVoiceFileAsync_CallsGeneratorWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            DefaultNarrator = "夏色花梨",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        var expectedOutputPath = "/output/test.wav";
        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult 
            { 
                OutputWavPath = expectedOutputPath,
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "テスト音声です。",
            OutputWavFileName = "test.wav",
            Options = new VoicePeakMcpGenerationOptions
            {
                Narrator = "夏色花梨",
                Speed = 120
            }
        };

        // Act
        var result = await service.GenerateVoiceFileAsync(request);

        // Assert
        Assert.Equal(expectedOutputPath, result);
        
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicePeakGenerateRequest>(r =>
                    r.Text == "テスト音声です。" &&
                    r.Narrator.VoicePeakSystemName == "夏色花梨" &&
                    r.Speed == 120),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "オプションがnullの場合にデフォルトのナレーターが使用されることを確認")]
    public async Task GenerateVoiceFileAsync_WithNullOptions_UsesDefaultNarrator()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            DefaultNarrator = "Frimomen",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

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
                It.Is<VoicePeakGenerateRequest>(r => 
                    r.Narrator.VoicePeakSystemName == "Frimomen" &&
                    r.Speed == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "複数の音声ファイル生成時にジェネレーターが複数回呼び出されることを確認")]
    public async Task GenerateVoiceFilesAsync_CallsGeneratorMultipleTimes()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            DefaultNarrator = "夏色花梨",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        var callCount = 0;
        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new VoicePeakGenerateResult
            {
                OutputWavPath = $"/output/test{++callCount}.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        var requests = new[]
        {
            new GenerateVoiceRequest
            {
                Text = "最初の音声です。",
                OutputWavFileName = "test1.wav",
                Options = new VoicePeakMcpGenerationOptions { Narrator = "夏色花梨" }
            },
            new GenerateVoiceRequest
            {
                Text = "2番目の音声です。",
                OutputWavFileName = "test2.wav",
                Options = new VoicePeakMcpGenerationOptions { Narrator = "夏色花梨" }
            },
            new GenerateVoiceRequest
            {
                Text = "3番目の音声です。",
                OutputWavFileName = "test3.wav",
                Options = new VoicePeakMcpGenerationOptions { Narrator = "夏色花梨" }
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
            g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact(DisplayName = "速度パラメーターが正しく変換されることを確認")]
    public async Task GenerateVoiceFileAsync_SpeedConversion_ConvertsCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "速度変換テストです。",
            OutputWavFileName = "speed_test.wav",
            Options = new VoicePeakMcpGenerationOptions
            {
                Speed = 150
            }
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicePeakGenerateRequest>(r => r.Speed == 150),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "ピッチパラメーターが正しく変換されることを確認")]
    public async Task GenerateVoiceFileAsync_PitchConversion_ConvertsCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "ピッチ変換テストです。",
            OutputWavFileName = "pitch_test.wav",
            Options = new VoicePeakMcpGenerationOptions
            {
                Pitch = -150
            }
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicePeakGenerateRequest>(r => r.Pitch == -150), // -1.5f * 100
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "感情パラメーターが正しく設定されることを確認")]
    public async Task GenerateVoiceFileAsync_WithEmotionParameters_SetsEmotionCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        var request = new GenerateVoiceRequest
        {
            Text = "感情パラメーターテストです。",
            OutputWavFileName = "emotion_test.wav",
            Options = new VoicePeakMcpGenerationOptions
            {
                Narrator = "夏色花梨",
                Emotion = "hightension=80,buchigire=20"
            }
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicePeakGenerateRequest>(r =>
                    r.Narrator.VoicePeakSystemName == "夏色花梨" &&
                    r.Narrator.GetEmotionString().Contains("hightension=80") &&
                    r.Narrator.GetEmotionString().Contains("buchigire=20")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "パラメーター文字列が正しく解析され適用されることを確認")]
    public async Task GenerateVoiceFileAsync_WithParameterString_ParsesAndAppliesParameters()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        // パラメーター文字列から VoicePeakMcpGenerationOptions を生成
        var generationOptions = VoicePeakMcpGenerationOptions.FromParameterString(
            "-n 夏色花梨 -e hightension=80,buchigire=20 --speed 120 --pitch 50");

        var request = new GenerateVoiceRequest
        {
            Text = "パラメーター文字列テストです。",
            OutputWavFileName = "param_test.wav",
            Options = generationOptions
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicePeakGenerateRequest>(r =>
                    r.Text == "パラメーター文字列テストです。" &&
                    r.Narrator.VoicePeakSystemName == "夏色花梨" &&
                    r.Narrator.GetEmotionString().Contains("hightension=80") &&
                    r.Narrator.GetEmotionString().Contains("buchigire=20") &&
                    r.Speed == 120 &&
                    r.Pitch == 50),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "ナレーターインスタンスが提供された場合に使用されることを確認")]
    public async Task GenerateVoiceFileAsync_WithNarratorInstance_UsesProvidedInstance()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        mockGenerator
            .Setup(g => g.GenerateVoiceFileAsync(
                It.IsAny<VoicePeakGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoicePeakGenerateResult
            {
                OutputWavPath = "/output/test.wav",
                Elapsed = TimeSpan.FromSeconds(1)
            });

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        // FromParameterString を使って NarratorInstance が設定された Options を作成
        var generationOptions = VoicePeakMcpGenerationOptions.FromParameterString(
            "-n 夏色花梨 -e hightension=100");

        var request = new GenerateVoiceRequest
        {
            Text = "ナレーターインスタンステストです。",
            OutputWavFileName = "narrator_instance_test.wav",
            Options = generationOptions
        };

        // Act
        await service.GenerateVoiceFileAsync(request);

        // Assert
        mockGenerator.Verify(
            g => g.GenerateVoiceFileAsync(
                It.Is<VoicePeakGenerateRequest>(r =>
                    r.Text == "ナレーターインスタンステストです。" &&
                    r.Narrator.VoicePeakSystemName == "夏色花梨" &&
                    r.Narrator.GetEmotionString().Contains("hightension=100")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ナレッジコンテンツが存在しない場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNoKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak",
            VoicePeakKnowledgeOptions = new List<VoicePeakGenerationKnowledgeOptions>()
        };

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("VOICEPEAK MCP ナレッジ", contents[0]);
        Assert.Contains("現在、VOICEPEAK サービスにはナレッジコンテンツが設定されていません", contents[0]);
        
        mockLogger.Verify(
            l => l.Warn(It.Is<string>(s => s.Contains("現在ナレッジコンテンツが存在しません"))),
            Times.Once);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: nullのナレッジオプションの場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNullKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak"
        };

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("VOICEPEAK MCP ナレッジ", contents[0]);
        Assert.Contains("現在、VOICEPEAK サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ベースクラスがナレッジを返す場合にそれを使用すること")]
    public void GetServiceKnowledgeContents_WithKnowledgeFromBase_ReturnsKnowledge()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "# テストナレッジ\n\nこれはテスト用のナレッジコンテンツです。");

        try
        {
            var options = new VoicePeakServiceOptions
            {
                VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
                VoicePeakOutputDirectoryName = "voicepeak",
                VoicePeakKnowledgeOptions = new List<VoicePeakGenerationKnowledgeOptions>
                {
                    new VoicePeakGenerationKnowledgeOptions
                    {
                        KnowledgeType = "LocalFile",
                        KnowledgeSource = tempFile,
                        DocumentType = "Markdown",
                        GenerateHeader = false
                    }
                }
            };

            var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

            // Act
            var contents = service.GetServiceKnowledgeContents().ToList();

            // Assert
            Assert.Single(contents);
            Assert.Contains("テストナレッジ", contents[0]);
            Assert.Contains("これはテスト用のナレッジコンテンツです", contents[0]);
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
        var mockGenerator = new Mock<IVoicePeakVoiceGenerator>();
        
        var options = new VoicePeakServiceOptions
        {
            VoicePeakExecutablePath = @"C:\dummy\voicepeak.exe",
            VoicePeakOutputDirectoryName = "voicepeak",
            VoicePeakKnowledgeOptions = new List<VoicePeakGenerationKnowledgeOptions>()
        };

        var service = new VoicePeakService(mockLogger.Object, options, mockGenerator.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("VOICEPEAK MCP ナレッジ", contents[0]);
        Assert.Contains("現在、VOICEPEAK サービスにはナレッジコンテンツが設定されていません", contents[0]);
        
        mockLogger.Verify(
            l => l.Warn(It.Is<string>(s => s.Contains("現在ナレッジコンテンツが存在しません"))),
            Times.Once);
    }
}
