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

        var service = new MarpService(options);
        var result = service.GenerateSlideMarkdown(source);

        var separatorCount = result
            .Split('\n')
            .Count(line => line.Trim() == "---");

        // frontmatter(2行) + slide separators(2) = 4
        // slides = separatorCount - 2
        Assert.Equal(2, separatorCount - 2);
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

        var service = new MarpService(options);
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
        var service = new MarpService(options);
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