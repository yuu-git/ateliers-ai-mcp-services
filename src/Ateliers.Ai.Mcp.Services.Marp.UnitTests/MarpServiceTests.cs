using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.Marp.UnitTests;

public class MarpServiceTests
{
    [Fact]
    public void GenerateSlideMarkdown_CreatesThreeSlides()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
        };

        var source =
            """
            # Title
            Intro text

            # Slide 2
            Content

            # Slide 3
            More content
            """;

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var result = service.GenerateSlideMarkdown(source);

        var separatorCount = result
            .Split('\n')
            .Count(line => line.Trim() == "---");

        // frontmatter(2行) + slide separators(2) = 4
        // slides = separatorCount - 2
        Assert.Equal(2, separatorCount - 2);
    }

    [Fact]
    public void GenerateSlideMarkdown_DefaultHeadingPrefixesAreTreatedAsSlideSeparators()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
        };
        var source =
            """
            # Title
            Intro

            ## Heading2
            Text

            ### Heading3
            More text
            """;
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var deck = service.GenerateSlideMarkdown(source);
        var separatorCount = deck
            .Split('\n')
            .Count(line => line.Trim() == "---");
        // frontmatter(2行) + slide separators(2) = 4
        // slides = separatorCount - 2
        Assert.Equal(1, separatorCount - 2);
    }

    [Fact]
    public void GenerateSlideMarkdown_CustomHeadingPrefixesAreTreatedAsSlideSeparators()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
            SeparatorHeadingPrefixList = new List<string> { "#", "##", "###" }
        };
        var source =
            """
            # Title
            Intro

            ## Heading2
            Text

            ### Heading3
            More text 1
            
            #### Heading4
            More text 2

            ##### Heading5
            More text 3
            """;
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var deck = service.GenerateSlideMarkdown(source);
        var separatorCount = deck
            .Split('\n')
            .Count(line => line.Trim() == "---");
        // frontmatter(2行) + slide separators(4) = 6
        // slides = separatorCount - 2
        Assert.Equal(2, separatorCount - 2);
    }

    [Fact]
    public void GenerateSlideMarkdown_HeadingPrefixesAreTreatedAsSlideSeparators()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
            SeparatorHeadingPrefixList = new List<string> { "##", "###" }
        };
        var source =
            """
            # Title
            Intro

            ## Heading2
            Text

            ### Heading3
            More text
            """;
        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var deck = service.GenerateSlideMarkdown(source);
        var separatorCount = deck
            .Split('\n')
            .Count(line => line.Trim() == "---");
        // frontmatter(2行) + slide separators(2) = 3
        // slides = separatorCount - 1
        Assert.Equal(2, separatorCount - 1);
    }

    [Fact]
    public void GenerateSlideMarkdown_HorizontalRuleBeforeHeadingIsTreatedAsSlideSeparator()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe"
        };

        var source = 
            """
            # Title
            Intro

            ---
            ## Heading2
            Text
            """;

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var deck = service.GenerateSlideMarkdown(source);

        Assert.DoesNotContain("\n---\n---\n", deck); 
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RenderToPngAsync_CreatesPngFile()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var sourceMarkdown =
            """
            # Title
            Intro text

            # Slide 2
            Content

            # Slide 3
            More content
            """;

        var slideMarkdown = service.GenerateSlideMarkdown(sourceMarkdown);
        var pngFiles = await service.RenderToPngAsync(slideMarkdown);

        Assert.Equal(3, pngFiles.Count);
        Assert.True(pngFiles.All(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)));

        foreach (var file in pngFiles)
        {
            Assert.True(File.Exists(file));
            // File.Delete(file);
        }
    }

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