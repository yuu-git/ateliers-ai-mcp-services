using Ateliers.Voice.Engines.VoicePeakTools;
using Ateliers.Voice.Engines.VoicePeakTools.Narrators;

namespace Ateliers.Ai.Mcp.Services.VoicePeak;

/// <summary>
/// VoicePeak ナレーター情報を MCP 向けに整形するフォーマッター
/// </summary>
public sealed class VoicePeakNarratorInfoFormatter
{
    private const string LogPrefix = $"{nameof(VoicePeakNarratorInfoFormatter)}:";

    /// <summary>
    /// 全ナレーター情報を含む完全なマークダウンを生成します
    /// </summary>
    /// <param name="mcpLogger"> ログを記録する場合は MCP ロガーを指定します（任意）</param>
    /// <returns> 基本情報と全ナレーター詳細を含むマークダウンを返します </returns>
    public static string ToFullInfoMarkdownAllNarrators(IMcpLogger? mcpLogger = null)
    {
        mcpLogger?.Info($"{LogPrefix} ToFullInfoMarkdown 全ナレーター情報 開始");
        var narrators = VoicePeakNarraterFactory.CreateAllNarrators();
        mcpLogger?.Info($"{LogPrefix} ナレーター数: {narrators.Count()}, 対象ナレーター: {string.Join(", ", narrators.Select(n => n.VoicePeakSystemName))}");
        return ToFullInfoMarkdown(narrators, mcpLogger);
    }

    /// <summary>
    /// 指定ナレーターリストの完全なマークダウンを生成します
    /// </summary>
    /// <param name="narratorNames"> ナレーター名のリストを指定します </param>
    /// <param name="mcpLogger"> ログを記録する場合は MCP ロガーを指定します（任意）</param>
    /// <returns> 基本情報と指定ナレーター詳細を含むマークダウンを返します </returns>
    public static string ToFullInfoMarkdown(IEnumerable<string> narratorNames, IMcpLogger? mcpLogger = null)
    {
        mcpLogger?.Info($"{LogPrefix} ToFullInfoMarkdown 指定ナレーターリスト 開始: {string.Join(", ", narratorNames)}");
        var narrators = narratorNames
            .Select(name => VoicePeakNarraterFactory.CreateNarratorByName(name))
            .ToList();
        return ToFullInfoMarkdown(narrators, mcpLogger);
    }

    /// <summary>
    /// 指定ナレーターの完全なマークダウンを生成します
    /// </summary>
    /// <param name="narratorName"> ナレーター名を指定します </param>
    /// <param name="mcpLogger"> ログを記録する場合は MCP ロガーを指定します（任意）</param>
    /// <returns> 基本情報と指定ナレーター詳細を含むマークダウンを返します </returns>
    public static string ToFullInfoMarkdown(string narratorName, IMcpLogger? mcpLogger = null)
    {
        mcpLogger?.Info($"{LogPrefix} ToFullInfoMarkdown 指定ナレーター 開始: {narratorName}");
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName(narratorName);
        return ToFullInfoMarkdown(new[] { narrator }, mcpLogger);
    }

    /// <summary>
    /// 指定ナレーターの完全なマークダウンを生成します
    /// </summary>
    /// <param name="narrator"> ナレーターを指定します </param>
    /// <param name="mcpLogger"> ログを記録する場合は MCP ロガーを指定します（任意）</param>
    /// <returns> 基本情報と指定ナレーター詳細を含むマークダウンを返します </returns>
    public static string ToFullInfoMarkdown(IVoicePeakNarrator narrator, IMcpLogger? mcpLogger = null)
    {
        mcpLogger?.Info($"{LogPrefix} ToFullInfoMarkdown 指定ナレーター 開始: {narrator.VoicePeakSystemName}");
        return ToFullInfoMarkdown(new[] { narrator }, mcpLogger);
    }

    /// <summary>
    /// 使い方の基本情報とナレーター詳細リストを含む完全なマークダウンを生成します
    /// </summary>
    /// <param name="narrators">ナレーターのリスト</param>
    /// <returns>基本情報とナレーター詳細を含むマークダウン</returns>
    public static string ToFullInfoMarkdown(IEnumerable<IVoicePeakNarrator> narrators, IMcpLogger? mcpLogger = null)
    {
        mcpLogger?.Info($"{LogPrefix} ToFullInfoMarkdown 開始");

        var lines = new List<string>();
        
        // 使い方の基本情報
        lines.AddRange(GetBasicUsageInfoLines());
        
        // ナレーター情報リスト
        lines.Add("");
        lines.Add("---");
        lines.Add("");
        lines.Add("# ナレーター情報リスト");
        lines.Add("");
        
        var narratorsList = narrators.ToList();
        mcpLogger?.Info($"{LogPrefix} ナレーター数: {narratorsList.Count}");

        for (int i = 0; i < narratorsList.Count; i++)
        {
            if (i > 0)
            {
                lines.Add("");
                lines.Add("---");
                lines.Add("");
            }
            
            lines.AddRange(ToNarratorDetailLines(narratorsList[i]));
        }

        mcpLogger?.Info($"{LogPrefix} ToFullInfoMarkdown 完了");
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// 使い方の基本情報を生成します
    /// </summary>
    /// <returns>基本情報のマークダウン行</returns>
    private static IList<string> GetBasicUsageInfoLines()
    {
        var lines = new List<string>
        {
            "# VoicePeak 使い方の基本情報",
            "",
            "## 概要",
            "",
            "VoicePeakは高品質な音声合成エンジンです。",
            "このサービスを通じて、複数のナレーターを使用した音声生成が可能です。",
            "",
            "## 基本パラメーター",
            "",
            "| パラメーター名 | 説明 | 値の範囲 | デフォルト値 |",
            "| --- | --- | --- | --- |",
            "| `speed` | 読み上げ速度 | 50 - 200 (50=0.5倍速, 100=通常, 200=2.0倍速) | 100 |",
            "| `pitch` | 音高調整 | -300 - 300 | 0 |",
            "",
            "## パラメーター指定方法",
            "",
            "音声生成時には以下を指定します:",
            "",
            "- **ナレーター名**: VoicePeak システム名を使用",
            "- **感情パラメーター**: 各ナレーターが対応する感情パラメーターを指定（例: `happy=50,sad=0`）",
            "- **speed**: 読み上げ速度 (50-200, デフォルト: 100)",
            "- **pitch**: 音高調整 (-300-300, デフォルト: 0)",
            "",
            "## 期待するパラメータ",
            "",
            "以下のパラメーターを使用して音声生成リクエストを行います:",
            "",
            $"- 引数：テキスト ({nameof(IGenerateVoiceRequest.Text)})",
            "   - 音声合成するテキスト文字列",
            $"- 引数：出力ファイルパス ({nameof(IGenerateVoiceRequest.OutputWavFileName)})",
            "   - 生成された音声ファイルの保存先パス",
            $"- 引数：生成オプション ({nameof(IGenerateVoiceRequest.Options)})",
            $"   - 実際の中身は {nameof(VoicePeakMcpGenerationOptions)} 型を想定",
            "   - ナレーター名、感情パラメーター、速度、音高調整などを含む生成オプション",
            "",
            "## 生成オプションの例",
            "",
            $"以下は {nameof(IGenerateVoiceRequest.Options)} の例です:",
            "```",
            "// 最小オプション例：ナレーターのみ",
            "-n Frimomen",
            "",
            "// ナレーターと感情パラメーター指定例",
            "-n Frimomen -e \"happy=50,sad=0\"",
            "",
            "// ナレーター、感情パラメーター、速度、音高調整指定例",
            "-n Frimomen -e \"happy=50,sad=0\" --speed 120 --pitch 20",
            "```",
            "",
            "指定のないパラメータはデフォルト値が使用されます。",
            "",
            "## 実際のリクエスト例",
            "",
            "以下は実際の音声生成リクエスト例です:",
            "",
            "```",
            "Text: \"こんにちは、VoicePeakの世界へようこそ！\"",
            "OutputFileName: \"output.wav\"",
            "Options: -n Frimomen -e \"happy=50,sad=0\" --speed 120 --pitch 20",
            "```",
            "",
            "複数の感情を組み合わせた例:",
            "",
            "```",
            "Text: \"今日はとても嬉しい気持ちです！\"",
            "OutputFileName: \"happy_voice.wav\"",
            "Options: -n Frimomen -e \"happy=80,ochoushimono=20\" --speed 105",
            "```",
            "",
            "別のナレーターを使用した例:",
            "",
            "```",
            "Text: \"テンション高めで読み上げます！\"",
            "OutputFileName: \"energetic_voice.wav\"",
            "Options: -n 夏色花梨 -e \"hightension=90\" --speed 110 --pitch 15",
            "```",
            "```",
            "",
            "この例では、Frimomen ナレーターを使用し、感情パラメーター、速度、音高調整を指定しています。",
            "引数の名称は本サービスではなくツールによって決定されるため、確認して設定して下さい。",
            ""
        };
        
        return lines;
    }

    /// <summary>
    /// ナレーター詳細情報のマークダウン行を生成します
    /// </summary>
    /// <param name="narrator">ナレーター</param>
    /// <returns>ナレーター詳細のマークダウン行</returns>
    private static IList<string> ToNarratorDetailLines(IVoicePeakNarrator narrator)
    {
        var lines = new List<string>();

        // ナレーター基本情報（コアドメインから取得）
        lines.AddRange(narrator.GetInfomationMarkdownLines());

        // 使用例
        lines.Add("");
        lines.Add("## 使用例");
        lines.Add("");
        lines.Add("### VoicePeak CLI コマンド形式");
        lines.Add("");
        lines.Add("```bash");
        lines.Add($"voicepeak.exe -s \"こんにちは\" -n \"{narrator.VoicePeakSystemName}\" -e \"{narrator.GetEmotionString()}\" --speed 100 --pitch 0 -o output.wav");
        lines.Add("```");

        return lines;
    }
}
