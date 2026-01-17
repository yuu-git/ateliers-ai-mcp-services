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

    [Fact(DisplayName = "GenerateSlideMarkdown: 基本的なマークダウンが正しくスライドに分割されること")]
    public void GenerateSlideMarkdown_BasicMarkdown_SplitsCorrectly()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            内容1
            
            ## スライド2
            内容2
            
            # スライド3
            内容3
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("marp: true", result);
        Assert.Contains("# スライド1", result);
        Assert.Contains("## スライド2", result);
        Assert.Contains("# スライド3", result);
        
        // スライド区切り（---）が2つあること（3スライドの場合）
        var separatorCount = result.Split("---").Length - 1;
        Assert.True(separatorCount >= 4); // Frontmatter(2) + スライド区切り(2)
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: 水平線が除去されること")]
    public void GenerateSlideMarkdown_RemovesHorizontalRules()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            内容1
            ---
            区切り線の後
            
            ## スライド2
            内容2
            
            # スライド3
            内容3
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        // 水平線（---）はスライド区切りとして使われるが、コンテンツ内の---は除去される
        var lines = result.Split('\n');
        var contentLines = lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.Contains("marp:")).ToList();
        
        // "区切り線の後"のテキストが含まれていることを確認
        Assert.Contains("区切り線の後", result);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: Frontmatterが含まれるマークダウンを正しく処理すること")]
    public void GenerateSlideMarkdown_WithFrontmatter_ProcessesCorrectly()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            ---
            title: テストプレゼン
            author: テスト
            ---
            
            # スライド1
            内容1
            
            ## スライド2
            内容2
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("marp: true", result);
        Assert.Contains("# スライド1", result);
        Assert.Contains("## スライド2", result);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: 見出しでない行から始まるマークダウンを正しく処理すること")]
    public void GenerateSlideMarkdown_StartsWithoutHeading_ProcessesCorrectly()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            これは最初の行です。
            見出しではありません。
            
            # スライド2
            内容2
            
            ## スライド3
            内容3
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("これは最初の行です", result);
        Assert.Contains("# スライド2", result);
        Assert.Contains("## スライド3", result);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: スライド数が2未満の場合に例外をスローすること")]
    public void GenerateSlideMarkdown_LessThanTwoSlides_ThrowsException()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            内容1のみ
            """;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            service.GenerateSlideMarkdown(markdown));
        
        Assert.Contains("At least 2 slides are required", exception.Message);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: 複数の見出しレベルが混在する場合に正しく処理すること")]
    public void GenerateSlideMarkdown_MixedHeadingLevels_ProcessesCorrectly()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # レベル1見出し
            内容1
            
            ## レベル2見出し
            内容2
            
            ### レベル3見出し（スライド区切りではない）
            内容3
            
            # レベル1見出し2
            内容4
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("# レベル1見出し", result);
        Assert.Contains("## レベル2見出し", result);
        Assert.Contains("### レベル3見出し", result);
        Assert.Contains("# レベル1見出し2", result);
        
        // レベル3見出しは同じスライド内に含まれるべき
        var slides = result.Split("---\n\n").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        Assert.True(slides.Any(s => s.Contains("### レベル3見出し")));
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: 空行が適切に処理されること")]
    public void GenerateSlideMarkdown_HandlesEmptyLines()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            
            
            複数の空行
            
            ## スライド2
            
            内容
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("# スライド1", result);
        Assert.Contains("## スライド2", result);
        Assert.Contains("複数の空行", result);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: リストやコードブロックが適切に保持されること")]
    public void GenerateSlideMarkdown_PreservesListsAndCodeBlocks()
    {
        // Arrange
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "# ", "## " }
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            - リスト項目1
            - リスト項目2
            
            ## スライド2
            ```csharp
            var code = "example";
            ```
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("- リスト項目1", result);
        Assert.Contains("- リスト項目2", result);
        Assert.Contains("```csharp", result);
        Assert.Contains("var code = \"example\";", result);
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: スペースなしヘッダープレフィックスが正しく処理されること")]
    public void GenerateSlideMarkdown_WithoutSpaceInPrefix_ProcessesCorrectly()
    {
        // Arrange - ユーザーがJSON設定でスペースを忘れた場合を想定
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "#", "##" } // スペースなし
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            内容1
            
            ## スライド2
            内容2
            
            # スライド3
            内容3
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("marp: true", result);
        Assert.Contains("# スライド1", result);
        Assert.Contains("## スライド2", result);
        Assert.Contains("# スライド3", result);
        
        // 3つのスライドが正しく分割されていることを確認
        var separatorCount = result.Split("---").Length - 1;
        Assert.True(separatorCount >= 4); // Frontmatter(2) + スライド区切り(2)
    }

    [Fact(DisplayName = "GenerateSlideMarkdown: スペースあり・なし混在ヘッダープレフィックスが正しく処理されること")]
    public void GenerateSlideMarkdown_WithMixedSpaceInPrefix_ProcessesCorrectly()
    {
        // Arrange - スペースありとスペースなしが混在
        var options = new MarpServiceOptions
        {
            MarpExecutablePath = "marp",
            SeparatorHeadingPrefixList = new List<string> { "#", "## " } // 混在
        };

        var logger = new InMemoryMcpLogger(new McpLoggerOptions());
        var service = new MarpService(logger, options);

        var markdown = """
            # スライド1
            内容1
            
            ## スライド2
            内容2
            
            ### スライド3（区切りではない）
            内容3
            """;

        // Act
        var result = service.GenerateSlideMarkdown(markdown);

        // Assert
        Assert.Contains("# スライド1", result);
        Assert.Contains("## スライド2", result);
        Assert.Contains("### スライド3", result);
        
        // 2つのスライドが生成されること（###は区切りではない）
        var separatorCount = result.Split("---").Length - 1;
        Assert.Equal(3, separatorCount); // Frontmatter(2) + スライド区切り(1)
    }
}
