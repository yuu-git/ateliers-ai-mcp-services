using Ateliers.Ai.Mcp.Services.GenericModels;
using System.Diagnostics;
using System.Text;

namespace Ateliers.Ai.Mcp.Services.Marp;

public sealed class MarpService : McpServiceBase, IGenerateSlideService
{
    private readonly IMarpServiceOptions _options;

    public MarpService(IMcpLogger mcpLogger, IMarpServiceOptions options)
        : base(mcpLogger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string GenerateSlideMarkdown(string sourceMarkdown)
    {
        var (_, bodyLines) = SplitFrontmatter(sourceMarkdown);

        // 入力Markdown中の水平線はすべて無視する
        var lines = bodyLines
            .Select(l => l.TrimEnd())
            .Where(l => l.Trim() != "---")
            .ToList();

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

        if (slides.Count < 2)
        {
            throw new InvalidOperationException(
                "At least 2 slides are required for presentation.");
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

        return sb.ToString();
    }


    public async Task<IReadOnlyList<string>> RenderToPngAsync(
        string slideMarkdown,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_options.MarpExecutablePath) &&
            !IsCommandAvailable(_options.MarpExecutablePath))
        {
            throw new InvalidOperationException(
                $"Marp CLI not found: {_options.MarpExecutablePath}");
        }

        var outputDir = _options.CreateWorkDirectory(_options.MarpOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));

        var inputPath = Path.Combine(outputDir, "deck.md");
        await File.WriteAllTextAsync(inputPath, slideMarkdown, cancellationToken);

        var outputPrefix = Path.Combine(outputDir, "slide.png");

        var psi = new ProcessStartInfo
        {
            FileName = _options.MarpExecutablePath,
            Arguments =
                $"\"{inputPath}\" --images png --output \"{outputPrefix}\"",
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
            throw new InvalidOperationException($"Marp failed: {error}");
        }

        return Directory
            .EnumerateFiles(outputDir, "*.png")
            .OrderBy(p => p)
            .ToList();
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
