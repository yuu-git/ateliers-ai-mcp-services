namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed class VoicevoxServiceOptions
{
    public required string ResourcePath { get; init; }

    public uint DefaultStyleId { get; init; } = 0;

    /// <summary>
    /// 読み込む voice model (*.vvm) のファイル名。
    /// null または空の場合は全モデルを読み込む。
    /// 拡張子あり / なし両対応。
    /// </summary>
    /// <example>
    /// // 全読み込み
    /// VoiceModelNames = null;
    /// // 1つ
    /// VoiceModelNames = new[] { "0.vvm" };
    /// // 複数
    /// VoiceModelNames = new[] { "0.vmm", "1.vvm" };
    /// </example>
    public IReadOnlyCollection<string>? VoiceModelNames { get; init; }
}

