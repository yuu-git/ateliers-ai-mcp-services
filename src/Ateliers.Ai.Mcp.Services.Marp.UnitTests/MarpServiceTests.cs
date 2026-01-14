using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.Marp.UnitTests;

public class MarpServiceTests
{
    [Fact(DisplayName = "GetServiceKnowledgeContents: ナレッジコンテンツが存在しない場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNoKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            MarpKnowledgeOptions = new List<MarpGenerationKnowledgeOptions>()
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("MARP MCP ナレッジ", contents[0]);
        Assert.Contains("現在、MARP サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: nullのナレッジオプションの場合にデフォルトメッセージを返すこと")]
    public void GetServiceKnowledgeContents_WithNullKnowledge_ReturnsDefaultMessage()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("MARP MCP ナレッジ", contents[0]);
        Assert.Contains("現在、MARP サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }

    [Fact(DisplayName = "GetServiceKnowledgeContents: ベースクラスがナレッジを返す場合にそれを使用すること")]
    public void GetServiceKnowledgeContents_WithKnowledgeFromBase_ReturnsKnowledge()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "# Marpテストナレッジ\n\nこれはMarp用のテストナレッジコンテンツです。");

        try
        {
            var options = new MarpServiceOptions
            {
                MarpExecutablePath = "marp",
                MarpKnowledgeOptions = new List<MarpGenerationKnowledgeOptions>
                {
                    new MarpGenerationKnowledgeOptions
                    {
                        KnowledgeType = "LocalFile",
                        KnowledgeSource = tempFile,
                        DocumentType = "Markdown",
                        GenerateHeader = false
                    }
                }
            };

            var logger = new InMemoryMcpLogger(new McpLoggerOptions());
            var service = new MarpService(logger, options);

            // Act
            var contents = service.GetServiceKnowledgeContents().ToList();

            // Assert
            Assert.Single(contents);
            Assert.Contains("Marpテストナレッジ", contents[0]);
            Assert.Contains("Marp用のテストナレッジコンテンツです", contents[0]);
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
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            MarpKnowledgeOptions = new List<MarpGenerationKnowledgeOptions>()
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        // Act
        var contents = service.GetServiceKnowledgeContents().ToList();

        // Assert
        Assert.Single(contents);
        Assert.Contains("MARP MCP ナレッジ", contents[0]);
        Assert.Contains("現在、MARP サービスにはナレッジコンテンツが設定されていません", contents[0]);
    }
}