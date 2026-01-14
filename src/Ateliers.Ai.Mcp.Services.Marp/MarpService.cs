using Ateliers.Ai.Mcp.Services.GenericModels;
using System.Diagnostics;
using System.Text;

namespace Ateliers.Ai.Mcp.Services.Marp;

public sealed class MarpService : McpContentGenerationServiceBase, IGenerateSlideService
{
    protected override string LogPrefix { get; init; } = $"{nameof(MarpService)}:";

    private readonly IMarpServiceOptions _options;

    public MarpService(IMcpLogger mcpLogger, IMarpServiceOptions options)
        : base(mcpLogger, options.MarpKnowledgeOptions)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");

        if (options == null)
        {
            var ex = new ArgumentNullException(nameof(options));
            McpLogger?.Critical($"{LogPrefix} 初期化失敗", ex);
            throw ex;
        }

        _options = options;

        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// コンテンツ生成ガイドを取得します。
    /// </summary>
    /// <returns> 未実装（将来：Marp マークダウン形式のガイド） </returns>
    public string GetContentGenerationGuide()
    {
        // ToDo: interface IMcpContentGenerationGuideProvider のガイド実装
        return
            "未実装：MarpService では、現在コンテンツ生成ガイドは提供されていません。" +
            "将来的にスライド作成に適した Marp マークダウン形式のガイドが提供される予定です。";
    }

    /// <summary>
    /// ナレッジコンテンツを取得します。
    /// </summary>
    /// <returns> ナレッジコンテンツの列挙 </returns>
    public override IEnumerable<string> GetServiceKnowledgeContents()
    {
        var contents = base.GetServiceKnowledgeContents();
        if (contents == null || !contents.Any())
        {
            McpLogger?.Warn($"{LogPrefix} Marp サービスは現在ナレッジコンテンツが存在しません。");
            return new List<string>()
            {
                "# MARP MCP ナレッジ：" + Environment.NewLine + Environment.NewLine +
                "現在、MARP サービスにはナレッジコンテンツが設定されていません。",
            };
        }
        return contents;
    }

    public string GenerateSlideMarkdown(string sourceMarkdown)
    {
        McpLogger?.Info($"{LogPrefix} GenerateSlideMarkdown 開始: サイズ={sourceMarkdown.Length}文字");

        var (_, bodyLines) = SplitFrontmatter(sourceMarkdown);

        // 入力Markdown中の水平線はすべて無視する
        var lines = bodyLines
            .Select(l => l.TrimEnd())
            .Where(l => l.Trim() != "---")
            .ToList();

        McpLogger?.Debug($"{LogPrefix} GenerateSlideMarkdown: 水平線除去後の行数={lines.Count}");

        var slides = new List<List<string>>();
        List<string>? currentSlide = null;

        foreach (var line in lines)
        {
            // スライド区切り見出しであれば新しいスライドを開始
            if (_options.SeparatorHeadingPrefixList.Select(head => line.TrimStart().StartsWith(head + " ")).Any(isMatch => isMatch))
            {
                currentSlide = new List<string>();
                slides.Add(currentSlide);
            }

            // 現在のスライドがなければ新しいスライドを開始
            currentSlide ??= new List<string>();
            currentSlide.Add(line);
        }

        McpLogger?.Debug($"{LogPrefix} GenerateSlideMarkdown: スライド数={slides.Count}");

        if (slides.Count < 2)
        {
            var ex = new InvalidOperationException(
                "At least 2 slides are required for presentation.");
            McpLogger?.Critical($"{LogPrefix} GenerateSlideMarkdown: スライド数不足: slides.Count={slides.Count}", ex);
            throw ex;
        }

        var sb = new StringBuilder();

        // Frontmatter（必ず1回だけ）
        sb.AppendLine("---");
        sb.AppendLine("marp: true");
        sb.AppendLine("theme: default");
        sb.AppendLine("paginate: true");
        sb.AppendLine("---");
        sb.AppendLine();

        for (int i = 0; i < slides.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            foreach (var line in slides[i])
            {
                sb.AppendLine(line);
            }
        }

        var result = sb.ToString();
        McpLogger?.Info($"{LogPrefix} GenerateSlideMarkdown 完了: スライド数={slides.Count}, サイズ={result.Length}文字");

        return result;
    }


    public async Task<IReadOnlyList<string>> RenderToPngAsync(
        string slideMarkdown,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Info($"{LogPrefix} RenderToPngAsync 開始: サイズ={slideMarkdown.Length}文字");

        McpLogger?.Debug($"{LogPrefix} RenderToPngAsync: Marp CLI 存在確認中...");
        if (!File.Exists(_options.MarpExecutablePath) &&
            !IsCommandAvailable(_options.MarpExecutablePath))
        {
            var ex = new InvalidOperationException(
                $"Marp CLI not found: {_options.MarpExecutablePath}");
            McpLogger?.Critical($"{LogPrefix} RenderToPngAsync: Marp CLI が見つかりません: path={_options.MarpExecutablePath}", ex);
            throw ex;
        }

        McpLogger?.Debug($"{LogPrefix} RenderToPngAsync: 作業ディレクトリ作成中...");
        var outputDir = _options.CreateWorkDirectory(_options.MarpOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));
        McpLogger?.Debug($"{LogPrefix} RenderToPngAsync: outputDir={outputDir}");

        var inputPath = Path.Combine(outputDir, "deck.md");
        McpLogger?.Debug($"{LogPrefix} RenderToPngAsync: Markdownファイル書き込み中: inputPath={inputPath}");
        await File.WriteAllTextAsync(inputPath, slideMarkdown, cancellationToken);

        var outputPrefix = Path.Combine(outputDir, "slide.png");

        var args = $"\"{inputPath}\" --images png --output \"{outputPrefix}\"";
        McpLogger?.Info($"{LogPrefix} RenderToPngAsync: Marp CLI 実行開始");
        McpLogger?.Debug($"{LogPrefix} RenderToPngAsync: パラメータ={args}");

        var psi = new ProcessStartInfo
        {
            FileName = _options.MarpExecutablePath,
            Arguments = args,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            var ex = new InvalidOperationException($"Marp failed: {error}");
            McpLogger?.Critical($"{LogPrefix} RenderToPngAsync: Marp実行失敗: ExitCode={process.ExitCode}", ex);
            throw ex;
        }

        McpLogger?.Info($"{LogPrefix} RenderToPngAsync: Marp実行完了");

        McpLogger?.Debug($"{LogPrefix} RenderToPngAsync: PNG ファイル一覧取得中...");
        var pngFiles = Directory
            .EnumerateFiles(outputDir, "*.png")
            .OrderBy(p => p)
            .ToList();

        McpLogger?.Info($"{LogPrefix} RenderToPngAsync 完了: {pngFiles.Count}件のPNGファイルを生成");

        return pngFiles;
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit(3000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static (string? frontmatter, List<string> bodyLines) SplitFrontmatter(string markdown)
    {
        var lines = markdown.Split('\n').Select(l => l.TrimEnd('\r')).ToList();

        if (lines.Count >= 3 && lines[0].Trim() == "---")
        {
            // find the closing '---'
            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    var fm = string.Join("\n", lines.Take(i + 1)) + "\n";
                    var body = lines.Skip(i + 1).ToList();
                    return (fm, body);
                }
            }
        }

        return (null, lines);
    }

    private static List<string> NormalizeHorizontalRules(List<string> bodyLines)
    {
        // Replace HR line '---' to '***' to avoid accidental slide separators.
        for (int i = 0; i < bodyLines.Count; i++)
        {
            if (bodyLines[i].Trim() == "---")
            {
                bodyLines[i] = "***";
            }
        }

        return bodyLines;
    }
}
