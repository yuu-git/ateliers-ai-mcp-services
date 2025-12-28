using Ateliers.Ai.Mcp.Services.Ffmpeg;
using Ateliers.Ai.Mcp.Services.GenericModels;
using Ateliers.Ai.Mcp.Services.Marp;
using Ateliers.Ai.Mcp.Services.Voicevox;

namespace Ateliers.Ai.Mcp.Services.PresentationVideo.UnitTests
{
    public class PresentationVideoServiceTest
    {
        static PresentationVideoServiceTest()
        {
            var path = @"C:\Program Files\VOICEVOX\vv-engine";
            if (Directory.Exists(path))
            {
                NativeLibraryPath.Use(path);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GenerateAsync_SimpleCase_Works()
        {
            var options = new PresentationVideoServiceOptions
            {
                OutputRootDirectory = Path.Combine(Path.GetTempPath(), "presentations"),

                ResourcePath = @"C:\Program Files\VOICEVOX\vv-engine",
                VoiceModelNames = ["0.vmm"],
                VoicevoxOutputDirectoryName = "voicevox",

                MarpExecutablePath = "C:\\Program Files\\Marp-CLI\\marp.exe",
                MarpOutputDirectoryName = "marp",

                FfmpegExecutablePath = "C:\\Program Files\\FFmpeg\\bin\\ffmpeg.exe",
                MediaOutputDirectoryName = "media",
            };

            var voicevoxService = new VoicevoxService(options);
            var marpService = new MarpService(options);
            var ffmpegService = new FfmpegService(options);
            var presentationVideoService = new PresentationVideoService(
                options,
                voicevoxService,
                marpService,
                ffmpegService);

            var testMarkdown =
            """
            # 5分でわかる：CI/CDパイプラインの基本
            本資料は「なぜCI/CDが必要か」と「最小の導入手順」を短時間で共有します。

            ---

            # 1. なぜCI/CDが必要？
            - 手作業デプロイはミスが増える
            - リリース頻度が上がるほど、属人化がリスクになる
            - テストとデプロイを自動化すると「品質」と「速度」が両立できる

            ---

            # 2. 最小構成（MVP）の考え方
            - まずは **Build → Test → Artifact** まで
            - 次に **Stage へ自動デプロイ**
            - 最後に **本番は手動承認（Approval）**
            ポイント：最初から全部やらない。小さく始めて育てる。

            ---

            # 3. よくある失敗と対策
            - 失敗：環境差分が多くて動かない  
              対策：設定を外出し（env / secrets / config）
            - 失敗：テストが遅くて回らない  
              対策：単体テストと統合テストを分離、キャッシュ活用

            ---

            # 4. 次の一手（今日からできること）
            - CIで「毎回同じコマンド」を固定化する
            - 成果物（zip / container）を必ず残す
            - 変更履歴（タグ/リリースノート）を自動生成する

            おわり：まずは “壊れない最小パイプライン” を作る。
            """;

            var narrationTexts = new[]
            {
                "今回はシーアイシーディーの基本を5分で説明します。まず全体像から見ていきます。",
                "次に、なぜシーアイシーディーが必要なのか。手作業の限界と自動化のメリットです。",
                "続いて最小構成の考え方です。最初から全部やらず、小さく導入します。",
                "よくある失敗パターンと、その対策を整理します。",
                "最後に次の一手です。今日からできる小さな改善を提案します。"
            };

            var request = new PresentationVideoRequest
            {
                SourceMarkdown = testMarkdown,
                NarrationTexts = narrationTexts
            };

            var result = await presentationVideoService.GenerateAsync(request);

            Assert.True(File.Exists(result.VideoPath));
        }
    }
}
