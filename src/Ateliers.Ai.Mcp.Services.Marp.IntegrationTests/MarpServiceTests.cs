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

        // frontmatter(2) + slide separators(2) = 4
        // 3 slides: [# Title + Intro text], [# Slide 2 + Content], [# Slide 3 + More content]
        Assert.Equal(4, separatorCount);
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
        // frontmatter(2) + slide separators(1) = 3
        // 2 slides: [# Title + Intro], [## Heading2 + Text + ### Heading3 + More text]
        // ### is NOT in default separator list
        Assert.Equal(3, separatorCount);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: カスタム見出しの接頭辞がスライド区切りとして扱われること")]
    [Trait("Category", "Integration")]
    public void GenerateSlideMarkdown_CustomHeadingPrefixesAreTreatedAsSlideSeparators()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## ", "### " }
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
        // frontmatter(2) + slide separators(2) = 4
        // 3 slides: [# Title + Intro], [## Heading2 + Text], [### Heading3 + More text 1 + #### Heading4 + More text 2 + ##### Heading5 + More text 3]
        Assert.Equal(4, separatorCount);
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
        // frontmatter(2) + slide separators(2) = 4
        // 3 slides: [# Title + Intro], [## Heading2 + Text], [### Heading3 + More text]
        Assert.Equal(4, separatorCount);
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

    [Fact(DisplayName = "RenderToPngAsync: 複雑なマークダウンから正しくPNGファイルが生成されること")]
    [Trait("Category", "Integration")]
    public async Task RenderToPngAsync_WithComplexMarkdown_CreatesPngFiles()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        
        var sourceMarkdown = """
            # イントロダクション
            
            これは複雑なマークダウンのテストです。
            
            - リスト項目1
            - リスト項目2
            - リスト項目3
            
            ## テクニカルな内容
            
            詳細な説明がここにあります。
            
            ```csharp
            var example = "コード例";
            Console.WriteLine(example);
            ```
            
            ### さらに詳細
            
            より深い内容です。
            
            # 結論
            
            まとめです。
            """;

        var slideMarkdown = service.GenerateSlideMarkdown(sourceMarkdown);
        var pngFiles = await service.RenderToPngAsync(slideMarkdown);

        Assert.True(pngFiles.Count >= 2);
        Assert.True(pngFiles.All(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)));

        foreach (var file in pngFiles)
        {
            Assert.True(File.Exists(file));
            var fileInfo = new FileInfo(file);
            Assert.True(fileInfo.Length > 0, $"ファイルサイズが0です: {file}");
        }
    }

    [Fact(DisplayName = "RenderToPngAsync: 日本語を含むマークダウンから正しくPNGファイルが生成されること")]
    [Trait("Category", "Integration")]
    public async Task RenderToPngAsync_WithJapaneseContent_CreatesPngFiles()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        
        var sourceMarkdown = """
            # 日本語タイトル
            
            これは日本語のコンテンツです。
            ひらがな、カタカナ、漢字が含まれます。
            
            ## 第二スライド
            
            - アイテム１
            - アイテム２
            - アイテム３
            
            # まとめ
            
            これで終わりです。
            ありがとうございました。
            """;

        var slideMarkdown = service.GenerateSlideMarkdown(sourceMarkdown);
        var pngFiles = await service.RenderToPngAsync(slideMarkdown);

        Assert.Equal(3, pngFiles.Count);
        
        foreach (var file in pngFiles)
        {
            Assert.True(File.Exists(file));
            var fileInfo = new FileInfo(file);
            Assert.True(fileInfo.Length > 0);
        }
    }

    [Fact(DisplayName = "RenderToPngAsync: 単一スライド（エラーケース）で例外がスローされること")]
    [Trait("Category", "Integration")]
    public void RenderToPngAsync_WithSingleSlide_ThrowsException()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        
        var sourceMarkdown = """
            # 単一スライド
            
            これは1つのスライドのみです。
            """;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            service.GenerateSlideMarkdown(sourceMarkdown));
    }

    [Fact(DisplayName = "RenderToPngAsync: Marp CLIが見つからない場合に例外がスローされること")]
    [Trait("Category", "Integration")]
    public async Task RenderToPngAsync_WithInvalidMarpPath_ThrowsException()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\NonExistent\\marp.exe"
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        
        var sourceMarkdown = """
            # スライド1
            内容1
            
            ## スライド2
            内容2
            """;

        var slideMarkdown = service.GenerateSlideMarkdown(sourceMarkdown);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await service.RenderToPngAsync(slideMarkdown));
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: スペースなしヘッダープレフィックスでも正しくスライドが生成されること")]
    [Trait("Category", "Integration")]
    public void GenerateSlideMarkdown_WithoutSpaceInPrefix_CreatesSlides()
    {
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
            SeparatorHeadingPrefixList = new List<string> { "#", "##", "###" } // スペースなし
        };

        var source =
            """
            # Title
            Intro text

            ## Slide 2
            Content

            ### Slide 3
            More content
            """;

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);
        var result = service.GenerateSlideMarkdown(source);

        var separatorCount = result
            .Split('\n')
            .Count(line => line.Trim() == "---");

        // frontmatter(2) + slide separators(2) = 4
        // 3 slides: [# Title + Intro text], [## Slide 2 + Content], [### Slide 3 + More content]
        Assert.Equal(4, separatorCount);
    }
}


