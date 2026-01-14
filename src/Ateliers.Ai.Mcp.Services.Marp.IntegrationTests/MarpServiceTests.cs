using Ateliers.Ai.Mcp.Logging;
using Ateliers.Ai.Mcp.Services.GenericModels;

namespace Ateliers.Ai.Mcp.Services.Marp.IntegrationTests;

public class MarpServiceTests
{

    [Fact(DisplayName = "GenerateSlideMarkdown: シンプルな3スライドのMarkdownから3つのスライドが生成されること")]
    [Trait("Category", "Integration")]
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

    [Fact(DisplayName = "GenerateSlideMarkdown: デフォルトの見出し接頭辞がスライド区切りとして扱われること")]
    [Trait("Category", "Integration")]
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

    [Fact(DisplayName = "GenerateSlideMarkdown: カスタム見出しの接頭辞がスライド区切りとして扱われること")]
    [Trait("Category", "Integration")]
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

    [Fact(DisplayName = "GenerateSlideMarkdown: 見出しの接頭辞がスライド区切りとして扱われること")]
    [Trait("Category", "Integration")]
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

    [Fact(DisplayName = "GenerateSlideMarkdown: 水平線の前に見出しがある場合、それはスライド区切りとして扱われること")]
    [Trait("Category", "Integration")]
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

    [Fact(DisplayName = "RenderToPngAsync: シンプルな3スライドのMarkdownから3つのPNGファイルが生成されること")]
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
}
