using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Moq;

namespace Ateliers.Ai.Mcp.Services.PresentationVideo.UnitTests;

public class PresentationVideoServiceTest
{
    static PresentationVideoServiceTest()
    {
        var path = @"C:\Program Files\VOICEVOX\vv-engine";
        if (Directory.Exists(path))
        {
            NativeLibraryPath.Use(path);
        }
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ナレッジコンテンツが存在しない場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNoKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // すべてのサービスが空のナレッジを返すように設定
        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg",
            PresentationVideoKnowledgeOptions = new List<PresentationVideoGenerationKnowledgeOptions>()
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);


        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Equal(13, contents.Count); // 4サービス × メッセージ + 3セパレータ × 3行
        Assert.Contains("現在、プレゼンテーション動画生成サービスにはナレッジコンテンツが設定されていません", contents[0]);
        Assert.Contains("現在、音声生成サービスにはナレッジコンテンツが設定されていません", contents[4]);
        Assert.Contains("現在、スライド生成サービスにはナレッジコンテンツが設定されていません", contents[8]);
        Assert.Contains("現在、メディア合成サービスにはナレッジコンテンツが設定されていません", contents[12]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: nullのナレッジオプションの場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNullKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // すべてのサービスが空のナレッジを返すように設定
        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg"
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);


        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Equal(13, contents.Count);
        Assert.Contains("現在、プレゼンテーション動画生成サービスにはナレッジコンテンツが設定されていません", contents[0]);
        Assert.Contains("現在、音声生成サービスにはナレッジコンテンツが設定されていません", contents[4]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ベースクラスがナレッジを返す場合にそれを使用すること")]
    public void GetServiceKnowledgeContents_WithKnowledgeFromBase_ReturnsKnowledge()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // 他のサービスは空のナレッジを返すように設定
        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "# PresentationVideoテストナレッジ\n\nこれはプレゼンテーション動画用のテストナレッジコンテンツです。");

        try
        {
            var options = new PresentationVideoServiceOptions
            {
                ResourcePath = "/dummy/path",
                MarpExecutablePath = "marp",
                FfmpegExecutablePath = "ffmpeg",
                PresentationVideoKnowledgeOptions = new List<PresentationVideoGenerationKnowledgeOptions>
                {
                    new PresentationVideoGenerationKnowledgeOptions
                    {
                        KnowledgeType = "LocalFile",
                        KnowledgeSource = tempFile,
                        DocumentType = "Markdown",
                        GenerateHeader = false
                    }
                }
            };

            var service = new PresentationVideoService(
                logger,
                options,
                mockVoiceService.Object,
                mockSlideService.Object,
                mockMediaService.Object);

            // Act
            var contents = service.GetServiceKnowledgeContents().ToList();

            // Assert
            // プレゼン(1) + セパレータ(3) + 音声空(1) + セパレータ(3) + スライド空(1) + セパレータ(3) + メディア空(1) = 13
            Assert.Equal(13, contents.Count);
            Assert.Contains("PresentationVideoテストナレッジ", contents[0]);
            Assert.Contains("プレゼンテーション動画用のテストナレッジコンテンツです", contents[0]);
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
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // すべてのサービスが空のナレッジを返すように設定
        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg",
            PresentationVideoKnowledgeOptions = new List<PresentationVideoGenerationKnowledgeOptions>()
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);


        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        // 4つのサービス × (メッセージ + 空行 + セパレータ + 空行) = 16件
        // ただし最初のセパレータは無いので: 1 + 3×4 = 13件
        Assert.Equal(13, contents.Count);
        Assert.Contains("現在、プレゼンテーション動画生成サービスにはナレッジコンテンツが設定されていません", contents[0]);
        Assert.Equal("---", contents[2]); // 最初のセパレータ
        Assert.Contains("現在、音声生成サービスにはナレッジコンテンツが設定されていません", contents[4]);
        Assert.Equal("---", contents[6]); // 2番目のセパレータ
        Assert.Contains("現在、スライド生成サービスにはナレッジコンテンツが設定されていません", contents[8]);
        Assert.Equal("---", contents[10]); // 3番目のセパレータ
        Assert.Contains("現在、メディア合成サービスにはナレッジコンテンツが設定されていません", contents[12]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: 各サービスからナレッジを統合して返すこと")]
    public void GetServiceKnowledgeContents_WithAllServicesKnowledge_ReturnsIntegratedKnowledge()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // 各サービスがナレッジを返すように設定
        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents())
            .Returns(new List<string> { "# 音声生成ナレッジ", "音声に関する情報" });
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents())
            .Returns(new List<string> { "# スライド生成ナレッジ", "スライドに関する情報" });
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents())
            .Returns(new List<string> { "# メディア合成ナレッジ", "動画合成に関する情報" });

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "# プレゼンテーションナレッジ\nプレゼンテーションに関する情報");

        try
        {
            var options = new PresentationVideoServiceOptions
            {
                ResourcePath = "/dummy/path",
                MarpExecutablePath = "marp",
                FfmpegExecutablePath = "ffmpeg",
                PresentationVideoKnowledgeOptions = new List<PresentationVideoGenerationKnowledgeOptions>
                {
                    new PresentationVideoGenerationKnowledgeOptions
                    {
                        KnowledgeType = "LocalFile",
                        KnowledgeSource = tempFile,
                        DocumentType = "Markdown",
                        GenerateHeader = false
                    }
                }
            };

            var service = new PresentationVideoService(
                logger,
                options,
                mockVoiceService.Object,
                mockSlideService.Object,
                mockMediaService.Object);

            // Act
            var contents = service.GetServiceKnowledgeContents().ToList();

            // Assert
            // プレゼン(1) + セパレータ(3) + 音声(2) + セパレータ(3) + スライド(2) + セパレータ(3) + メディア(2) = 16
            Assert.Equal(16, contents.Count);
            Assert.Contains("プレゼンテーションナレッジ", contents[0]);
            Assert.Equal("---", contents[2]); // 最初のセパレータ
            Assert.Contains("音声生成ナレッジ", contents[4]);
            Assert.Equal("---", contents[7]); // 2番目のセパレータ
            Assert.Contains("スライド生成ナレッジ", contents[9]);
            Assert.Equal("---", contents[12]); // 3番目のセパレータ
            Assert.Contains("メディア合成ナレッジ", contents[14]);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: 一部のサービスのみナレッジがある場合に統合して返すこと")]
    public void GetServiceKnowledgeContents_WithPartialKnowledge_ReturnsAvailableKnowledge()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // 音声とスライドのみナレッジを返すように設定
        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents())
            .Returns(new List<string> { "# 音声生成ナレッジ", "音声に関する情報" });
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents())
            .Returns(new List<string> { "# スライド生成ナレッジ", "スライドに関する情報" });
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents())
            .Returns(new List<string>());

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg",
            PresentationVideoKnowledgeOptions = new List<PresentationVideoGenerationKnowledgeOptions>()
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        // プレゼン空メッセージ(1) + セパレータ(3) + 音声(2) + セパレータ(3) + スライド(2) + セパレータ(3) + メディア空メッセージ(1) = 15
        Assert.Equal(15, contents.Count);
        Assert.Contains("現在、プレゼンテーション動画生成サービスにはナレッジコンテンツが設定されていません", contents[0]);
        Assert.Contains("音声生成ナレッジ", contents[4]);
        Assert.Contains("スライド生成ナレッジ", contents[9]);
        Assert.Contains("現在、メディア合成サービスにはナレッジコンテンツが設定されていません", contents[14]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: 各サービスメソッドが呼び出されることを確認")]
    public void GetServiceKnowledgeContents_CallsAllServiceMethods()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        mockVoiceService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockSlideService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());
        mockMediaService.Setup(s => s.GetServiceKnowledgeContents()).Returns(new List<string>());

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg"
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);


        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        mockVoiceService.Verify(s => s.GetServiceKnowledgeContents(), Times.Once);
        mockSlideService.Verify(s => s.GetServiceKnowledgeContents(), Times.Once);
        mockMediaService.Verify(s => s.GetServiceKnowledgeContents(), Times.Once);
    }

    [Fact(DisplayName = "GetContentGenerationGuide: 各サービスからガイドを統合して返すこと")]
    public void GetContentGenerationGuide_WithAllServicesGuides_ReturnsIntegratedGuide()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // 各サービスがガイドを返すように設定
        mockVoiceService.Setup(s => s.GetContentGenerationGuide())
            .Returns("音声生成ガイド：ナレーターを選択してください。");
        mockSlideService.Setup(s => s.GetContentGenerationGuide())
            .Returns("スライド生成ガイド：Marp形式でスライドを作成します。");
        mockMediaService.Setup(s => s.GetContentGenerationGuide())
            .Returns("メディア合成ガイド：FFmpegで動画を合成します。");

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg"
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);

        // Act
        var guide = service.GetContentGenerationGuide();

        // Assert
        Assert.Contains("音声生成ガイド", guide);
        Assert.Contains("スライド生成ガイド", guide);
        Assert.Contains("メディア合成ガイド", guide);
        Assert.Contains("プレゼンテーション動画生成の流れ", guide);
        Assert.Contains("---", guide); // セパレータの存在確認
    }

    [Fact(DisplayName = "GetContentGenerationGuide: 未実装のサービスガイドの場合にメッセージを返すこと")]
    public void GetContentGenerationGuide_WithUnimplementedGuides_ReturnsDefaultMessages()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // すべてのサービスが未実装メッセージを返すように設定
        mockVoiceService.Setup(s => s.GetContentGenerationGuide())
            .Returns("未実装：音声生成サービスでは...");
        mockSlideService.Setup(s => s.GetContentGenerationGuide())
            .Returns("未実装：スライド生成サービスでは...");
        mockMediaService.Setup(s => s.GetContentGenerationGuide())
            .Returns("未実装：メディア合成サービスでは...");

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg"
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);

        // Act
        var guide = service.GetContentGenerationGuide();

        // Assert
        Assert.Contains("現在、音声生成サービスにはコンテンツ生成ガイドが実装されていません", guide);
        Assert.Contains("現在、スライド生成サービスにはコンテンツ生成ガイドが実装されていません", guide);
        Assert.Contains("現在、メディア合成サービスにはコンテンツ生成ガイドが実装されていません", guide);
        Assert.Contains("プレゼンテーション動画生成の流れ", guide);
    }

    [Fact(DisplayName = "GetContentGenerationGuide: 一部のサービスのみガイドがある場合に統合して返すこと")]
    public void GetContentGenerationGuide_WithPartialGuides_ReturnsAvailableGuides()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        // 音声とスライドのみガイドを返すように設定
        mockVoiceService.Setup(s => s.GetContentGenerationGuide())
            .Returns("音声生成ガイド：利用可能なナレーター一覧");
        mockSlideService.Setup(s => s.GetContentGenerationGuide())
            .Returns("スライド生成ガイド：Marpの使い方");
        mockMediaService.Setup(s => s.GetContentGenerationGuide())
            .Returns("未実装");

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg"
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);

        // Act
        var guide = service.GetContentGenerationGuide();

        // Assert
        Assert.Contains("音声生成ガイド", guide);
        Assert.Contains("スライド生成ガイド", guide);
        Assert.Contains("現在、メディア合成サービスにはコンテンツ生成ガイドが実装されていません", guide);
        Assert.Contains("プレゼンテーション動画生成の流れ", guide);
    }

    [Fact(DisplayName = "GetContentGenerationGuide: 各サービスメソッドが呼び出されることを確認")]
    public void GetContentGenerationGuide_CallsAllServiceMethods()
    {
        // Arrange
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var mockVoiceService = new Mock<IGenerateVoiceService>();
        var mockSlideService = new Mock<IGenerateSlideService>();
        var mockMediaService = new Mock<IMediaComposerService>();

        mockVoiceService.Setup(s => s.GetContentGenerationGuide()).Returns("");
        mockSlideService.Setup(s => s.GetContentGenerationGuide()).Returns("");
        mockMediaService.Setup(s => s.GetContentGenerationGuide()).Returns("");

        var options = new PresentationVideoServiceOptions
        {
            ResourcePath = "/dummy/path",
            MarpExecutablePath = "marp",
            FfmpegExecutablePath = "ffmpeg"
        };

        var service = new PresentationVideoService(
            logger,
            options,
            mockVoiceService.Object,
            mockSlideService.Object,
            mockMediaService.Object);

        // Act
        var guide = service.GetContentGenerationGuide();

        // Assert
        mockVoiceService.Verify(s => s.GetContentGenerationGuide(), Times.Once);
        mockSlideService.Verify(s => s.GetContentGenerationGuide(), Times.Once);
        mockMediaService.Verify(s => s.GetContentGenerationGuide(), Times.Once);
    }
}



