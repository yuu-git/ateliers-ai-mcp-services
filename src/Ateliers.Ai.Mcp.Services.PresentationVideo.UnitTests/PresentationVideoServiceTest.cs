using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.Ffmpeg;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.Ai.Mcp.Services.Marp;
using Ateliers.Ai.Mcp.Services.Voicevox;
using Moq;

namespace Ateliers.Ai.Mcp.Services.PresentationVideo.UnitTests
{
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

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GenerateAsync_SimpleCase_Works()
        {
            var options = new PresentationVideoServiceOptions
            {
                OutputRootDirectory = Path.Combine(Path.GetTempPath(), "presentations"),

                ResourcePath = @"C:\Program Files\VOICEVOX\vv-engine",
                VoiceModelNames = ["0.vmm"],
                VoicevoxOutputDirectoryName = "voicevox",

                MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
                MarpOutputDirectoryName = "marp",

                FfmpegExecutablePath = "C:\\Program Files\\FFmpeg\\bin\\ffmpeg.exe",
                MediaOutputDirectoryName = "media",
            };

            var loggger = new InMemoryMcpLogger(new McpLoggerOptions());

            var voicevoxService = new VoicevoxService(loggger, options);
            var marpService = new MarpService(loggger, options);
            var ffmpegService = new FfmpegService(loggger, options);
            var presentationVideoService = new PresentationVideoService(
                loggger,
                options,
                voicevoxService,
                marpService,
                ffmpegService);

            var testMarkdown =
            """
            # 5分でわかる：CI/CDパイプラインの基本
            本資料は「なぜCI/CDが必要か」と「最小の導入手順」を短時間で共有します。

            ---

            # 1. なぜCI/CDが必要？
            - 手作業デプロイはミスが増える
            - リリース頻度が上がるほど、属人化がリスクになる
            - テストとデプロイを自動化すると「品質」と「速度」が両立できる

            ---

            # 2. 最小構成（MVP）の考え方
            - まずは **Build → Test → Artifact** まで
            - 次に **Stage へ自動デプロイ**
            - 最後に **本番は手動承認（Approval）**
            ポイント：最初から全部やらない。小さく始めて育てる。

            ---

            # 3. よくある失敗と対策
            - 失敗：環境差分が多くて動かない  
              対策：設定を外出し（env / secrets / config）
            - 失敗：テストが遅くて回らない  
              対策：単体テストと統合テストを分離、キャッシュ活用

            ---

            # 4. 次の一手（今日からできること）
            - CIで「毎回同じコマンド」を固定化する
            - 成果物（zip / container）を必ず残す
            - 変更履歴（タグ/リリースノート）を自動生成する

            おわり：まずは “壊れない最小パイプライン” を作る。
            """;

            var narrationTexts = new[]
            {
                "今回はシーアイシーディーの基本を5分で説明します。まず全体像から見ていきます。",
                "次に、なぜシーアイシーディーが必要なのか。手作業の限界と自動化のメリットです。",
                "続いて最小構成の考え方です。最初から全部やらず、小さく導入します。",
                "よくある失敗パターンと、その対策を整理します。",
                "最後に次の一手です。今日からできる小さな改善を提案します。"
            };

            var request = new PresentationVideoRequest
            {
                SourceMarkdown = testMarkdown,
                NarrationTexts = narrationTexts
            };

            var result = await presentationVideoService.GenerateAsync(request);

            Assert.True(File.Exists(result.VideoPath));
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
    }
}


