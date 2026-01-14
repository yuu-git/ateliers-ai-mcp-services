using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;
using NAudio.Wave;

namespace Ateliers.Ai.Mcp.Services.Ffmpeg.UnitTests;

public class FfmpegServiceTests
{

    [Fact(DisplayName = "GetServiceKnowledgeContents: ナレッジコンテンツが存在しない場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNoKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var options = new FfmpegServiceOptions
        {
            FfmpegExecutablePath = "ffmpeg",
            FfmpegKnowledgeOptions = new List<FfmpegGenerationKnowledgeOptions>()
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new FfmpegService(logger, options);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("FFMPEG MCP ナレッジ", contents[0]);
        Assert.Contains("現在、FFMPEG サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: nullのナレッジオプションの場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNullKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var options = new FfmpegServiceOptions
        {
            FfmpegExecutablePath = "ffmpeg"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new FfmpegService(logger, options);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("FFMPEG MCP ナレッジ", contents[0]);
        Assert.Contains("現在、FFMPEG サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ベースクラスがナレッジを返す場合にそれを使用すること")]
    public void GetServiceKnowledgeContents_WithKnowledgeFromBase_ReturnsKnowledge()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "# FFmpegテストナレッジ\n\nこれはFFmpeg用のテストナレッジコンテンツです。");

        try
        {
            var options = new FfmpegServiceOptions
            {
                FfmpegExecutablePath = "ffmpeg",
                FfmpegKnowledgeOptions = new List<FfmpegGenerationKnowledgeOptions>
                {
                    new FfmpegGenerationKnowledgeOptions
                    {
                        KnowledgeType = "LocalFile",
                        KnowledgeSource = tempFile,
                        DocumentType = "Markdown",
                        GenerateHeader = false
                    }
                }
            };

            var logger = new InMemoryMcpLogger(new McpLoggerOptions());
            var service = new FfmpegService(logger, options);

            // Act
            var contents = service.GetServiceKnowledgeContents().ToList();

            // Assert
            Assert.Single(contents);
            Assert.Contains("FFmpegテストナレッジ", contents[0]);
            Assert.Contains("FFmpeg用のテストナレッジコンテンツです", contents[0]);
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
        var options = new FfmpegServiceOptions
        {
            FfmpegExecutablePath = "ffmpeg",
            FfmpegKnowledgeOptions = new List<FfmpegGenerationKnowledgeOptions>()
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new FfmpegService(logger, options);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("FFMPEG MCP ナレッジ", contents[0]);
        Assert.Contains("現在、FFMPEG サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }
}
