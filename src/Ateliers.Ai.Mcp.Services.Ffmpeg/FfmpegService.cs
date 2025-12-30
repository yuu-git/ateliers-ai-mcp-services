using Ateliers.Ai.Mcp.Services.GenericModels;
using System.Diagnostics;
using System.Text;

namespace Ateliers.Ai.Mcp.Services.Ffmpeg;

public sealed class FfmpegService : McpServiceBase, IMediaComposerService
{
    private readonly IFfmpegServiceOptions _options;

    public FfmpegService(IMcpLogger mcpLogger, IFfmpegServiceOptions options)
        : base(mcpLogger)
    {
        _options = options;
    }

    public async Task<string> ComposeAsync(
        GenerateVideoRequest request,
        string outputFileName,
        CancellationToken cancellationToken = default)
    {
        if (request.Slides.Count == 0)
            throw new InvalidOperationException("At least one slide is required.");

        var outputDir = _options.CreateWorkDirectory(_options.MediaOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));

        var localSlides = CopyAssetsToWorkingDir(request.Slides, outputDir);
        var imagesList = Path.Combine(outputDir, "images.txt");
        var audioList = Path.Combine(outputDir, "audio.txt");
        var outputPath = Path.Combine(outputDir, outputFileName);

        CreateImagesList(localSlides, imagesList);
        CreateAudioList(localSlides, audioList);

        await RunFfmpegAsync(imagesList, audioList, outputPath, cancellationToken);

        return outputPath;
    }

    // ------------------------
    // helpers
    // ------------------------

    private static void CreateImagesList(
        IReadOnlyList<SlideAudioPair> slides,
        string path)
    {
        using var writer = new StreamWriter(
            path,
            false,
            new UTF8Encoding(false)); // BOMなし

        for (int i = 0; i < slides.Count; i++)
        {
            writer.WriteLine($"file '{slides[i].ImagePath}'");

            writer.WriteLine($"duration {slides[i].DurationSeconds}");
        }

        // ★ 最後の file をもう一度書く（超重要）
        writer.WriteLine($"file '{slides[^1].ImagePath}'");
    }


    private static void CreateAudioList(
        IReadOnlyList<SlideAudioPair> slides,
        string path)
    {
        using var writer = new StreamWriter(
            path,
            false,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        );

        foreach (var slide in slides)
        {
            writer.WriteLine($"file '{NormalizePath(slide.AudioPath)}'");
        }
    }

    private async Task RunFfmpegAsync(
        string imagesList,
        string audioList,
        string outputPath,
        CancellationToken ct)
    {
        var args =
            $"-y " +
            $"-nostdin " + 
            $"-f concat -safe 0 -i \"{imagesList}\" " +
            $"-f concat -safe 0 -i \"{audioList}\" " +
            $"-shortest " +
            $"-fps_mode vfr " +
            $"-pix_fmt yuv420p " +
            $"\"{outputPath}\"";

        var psi = new ProcessStartInfo
        {
            FileName = _options.FfmpegExecutablePath,
            Arguments = args,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(ct);
            throw new InvalidOperationException($"FFmpeg failed: {error}");
        }
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    private static IReadOnlyList<SlideAudioPair> CopyAssetsToWorkingDir(
    IReadOnlyList<SlideAudioPair> slides,
    string workingDir)
    {
        var result = new List<SlideAudioPair>();

        foreach (var slide in slides)
        {
            var imageName = Path.GetFileName(slide.ImagePath);
            var audioName = Path.GetFileName(slide.AudioPath);

            var imageDest = Path.Combine(workingDir, imageName);
            var audioDest = Path.Combine(workingDir, audioName);

            File.Copy(slide.ImagePath, imageDest, overwrite: true);
            File.Copy(slide.AudioPath, audioDest, overwrite: true);

            result.Add(new SlideAudioPair(
                imageName,
                audioName,
                slide.DurationSeconds));
        }

        return result;
    }
}