using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ateliers.Voice.Engines.VoicePeakTools;

namespace Ateliers.Ai.Mcp.Services.VoicePeak;

/// <summary>
/// VOICEPEAK MCP 音声生成のオプション設定
/// </summary>
public class VoicePeakMcpGenerationOptions : IVoiceGenerationOptions
{
    /// <summary>
    /// ナレーター名
    /// </summary>
    [JsonPropertyName("narrator")]
    public string? Narrator { get; init; }

    /// <summary>
    /// 話速（デフォルト: 100）
    /// </summary>
    [JsonPropertyName("speed")]
    public int Speed { get; init; } = 100;

    /// <summary>
    /// ピッチ（デフォルト: 0）
    /// </summary>
    [JsonPropertyName("pitch")]
    public int Pitch { get; init; } = 0;

    /// <summary>
    /// 感情パラメーター文字列（例: "happy=100,sad=50"）
    /// </summary>
    [JsonPropertyName("emotion")]
    public string? Emotion { get; init; }

    /// <summary>
    /// テキストファイルの保存モード（デフォルト: TextOnly）
    /// </summary>
    [JsonPropertyName("textFileSaveMode")]
    public TextFileSaveMode TextFileSaveMode { get; init; } = TextFileSaveMode.TextOnly;

    /// <summary>
    /// ナレーターインスタンス（FromParameterString で生成された場合のみ設定される）
    /// </summary>
    [JsonIgnore]
    public IVoicePeakNarrator? NarratorInstance { get; init; }

    /// <summary>
    /// パラメーター文字列からインスタンスを生成します
    /// </summary>
    /// <param name="parametersString">パラメーター文字列（例: "-n 夏色花梨 -e hightension=80,buchigire=20 --speed 120 --pitch 50"）</param>
    /// <param name="logger">ロガー（オプション）</param>
    /// <returns>パース済みオプション</returns>
    public static VoicePeakMcpGenerationOptions FromParameterString(
        string? parametersString,
        IMcpLogger? logger = null)
    {
        logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: パース開始");

        if (string.IsNullOrWhiteSpace(parametersString))
        {
            logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: パラメーター文字列が空、デフォルトインスタンスを返却");
            return new VoicePeakMcpGenerationOptions();
        }

        logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: パラメーター文字列='{parametersString}'");

        string? narrator = null;
        int? speed = null;
        int? pitch = null;
        string? emotion = null;

        // ナレーター名をパース (-n または --narrator)
        var narratorMatch = Regex.Match(parametersString, @"(?:-n|--narrator)\s+([^\s-]+)", RegexOptions.IgnoreCase);
        if (narratorMatch.Success)
        {
            narrator = narratorMatch.Groups[1].Value.Trim();
            logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: ナレーター名をパース narrator='{narrator}'");
        }

        // 感情パラメーター文字列をパース (-e または --emotion)
        var emotionMatch = Regex.Match(parametersString, @"(?:-e|--emotion)\s+([^\s-]+(?:,[^\s-]+)*)", RegexOptions.IgnoreCase);
        if (emotionMatch.Success)
        {
            emotion = emotionMatch.Groups[1].Value.Trim();
            logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: 感情パラメーターをパース emotion='{emotion}'");
        }

        // 速度パラメーターをパース (--speed)
        var speedMatch = Regex.Match(parametersString, @"--speed\s+(-?\d+)", RegexOptions.IgnoreCase);
        if (speedMatch.Success && int.TryParse(speedMatch.Groups[1].Value, out var speedValue))
        {
            // VoicePeak の速度は 50-200 の範囲なので、0.5-2.0 に正規化
            speed = speedValue;
            logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: 速度をパース speed={speed} (元の値={speedValue})");
        }

        // 音高パラメーターをパース (--pitch)
        var pitchMatch = Regex.Match(parametersString, @"--pitch\s+(-?\d+)", RegexOptions.IgnoreCase);
        if (pitchMatch.Success && int.TryParse(pitchMatch.Groups[1].Value, out var pitchValue))
        {
            // VoicePeak のピッチは -300 ~ 300 の範囲
            pitch = pitchValue;
            logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: ピッチをパース pitch={pitch} (元の値={pitchValue})");
        }

        // ナレーターインスタンスを生成
        IVoicePeakNarrator? narratorInstance = null;
        if (!string.IsNullOrWhiteSpace(narrator))
        {
            logger?.Info($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: ナレーターインスタンスを生成 narrator='{narrator}'");
            narratorInstance = VoicePeakNarraterFactory.CreateNarratorByName(narrator);

            // 感情パラメーターを設定
            if (!string.IsNullOrWhiteSpace(emotion))
            {
                logger?.Debug($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: 感情パラメーターを設定 emotion='{emotion}'");
                narratorInstance.SetEmotionParameter(emotion);
            }
        }

        logger?.Info($"{nameof(VoicePeakMcpGenerationOptions)}.{nameof(FromParameterString)}: パース完了 narrator='{narrator}', speed={speed}, pitch={pitch}");

        return new VoicePeakMcpGenerationOptions
        {
            Narrator = narrator,
            Speed = speed ?? 100,
            Pitch = pitch ?? 0,
            Emotion = emotion,
            NarratorInstance = narratorInstance
        };
    }

    /// <summary>
    /// パラメーター文字列の検証結果を取得します
    /// </summary>
    /// <param name="parametersString">パラメーター文字列</param>
    /// <returns>検証メッセージのリスト（エラーがない場合は空）</returns>
    public static IEnumerable<string> Validate(string? parametersString)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(parametersString))
        {
            // パラメーター文字列の形式チェック（警告レベル）
            var hasValidParam = Regex.IsMatch(parametersString, @"(-n|--narrator|-e|--emotion|--speed|--pitch)", RegexOptions.IgnoreCase);
            if (!hasValidParam)
            {
                errors.Add("パラメーター文字列にサポートされているオプションが含まれていません。デフォルト値が使用されます。");
            }
        }

        return errors;
    }
}
