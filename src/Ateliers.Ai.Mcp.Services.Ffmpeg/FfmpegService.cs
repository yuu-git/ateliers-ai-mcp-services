using Ateliers.Ai.Mcp.Services.GenericModels;
using System.Diagnostics;
using System.Text;

namespace Ateliers.Ai.Mcp.Services.Ffmpeg;

/// <summary>
/// FFmpegを使用したメディア合成サービス
/// </summary>
/// <remarks>
/// このサービスは、FFmpegコマンドラインツールを利用して、画像と音声を組み合わせた動画を生成します。
/// </remarks>
public sealed class FfmpegService : McpServiceBase, IMediaComposerService
{
    private readonly IFfmpegServiceOptions _options;
    private const string LogPrefix = $"{nameof(FfmpegService)}:";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="mcpLogger"> 記録用のロガーを指定します。 </param>
    /// <param name="options"> サービスの設定オプションを指定します。 </param>
    public FfmpegService(IMcpLogger mcpLogger, IFfmpegServiceOptions options)
        : base(mcpLogger)
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

    /// <inheritdoc/>
    public async Task<string> ComposeAsync(
        GenerateVideoRequest request,
        string outputFileName,
        CancellationToken cancellationToken = default)
    {
        McpLogger?.Info($"{LogPrefix} ComposeAsync 開始: outputFileName={outputFileName}, slides={request.Slides.Count}件");

        if (request.Slides.Count == 0)
        {
            var ex = new ArgumentException("スライド音声のペアは1つ以上指定する必要があります。", nameof(request.Slides));
            McpLogger?.Critical($"{LogPrefix} ComposeAsync: スライド音声ペア無し", ex);
            throw ex;
        }

        McpLogger?.Debug($"{LogPrefix} ComposeAsync: 作業ディレクトリ作成中...");
        var outputDir = _options.CreateWorkDirectory(_options.MediaOutputDirectoryName, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));
        McpLogger?.Debug($"{LogPrefix} ComposeAsync: outputDir={outputDir}");

        var localSlides = CopyAssetsToWorkingDir(request.Slides, outputDir);
        var imagesList = Path.Combine(outputDir, "images.txt");
        var audioList = Path.Combine(outputDir, "audio.txt");
        var outputPath = Path.Combine(outputDir, outputFileName);

        McpLogger?.Debug($"{LogPrefix} ComposeAsync: リストファイル作成中...");
        CreateImagesList(localSlides, imagesList);
        CreateAudioList(localSlides, audioList);

        await RunFfmpegAsync(imagesList, audioList, outputPath, cancellationToken);

        McpLogger?.Info($"{LogPrefix} ComposeAsync 完了: outputPath={outputPath}");
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

        McpLogger?.Info($"{LogPrefix} RunFfmpegAsync: FFmpeg実行開始");
        McpLogger?.Debug($"{LogPrefix} RunFfmpegAsync: パラメータ={args}");

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
            var ex = new InvalidOperationException($"FFmpeg の実行に失敗しました。ExitCode={process.ExitCode} エラー内容={error}");
            McpLogger?.Critical($"{LogPrefix} RunFfmpegAsync: FFmpeg実行失敗: ExitCode={process.ExitCode}", ex);
            throw ex;
        }

        McpLogger?.Info($"{LogPrefix} RunFfmpegAsync: FFmpeg実行完了");
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