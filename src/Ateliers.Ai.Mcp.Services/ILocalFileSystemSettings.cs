namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// ローカルファイルシステム設定
/// </summary>
public interface ILocalFileSystemSettings
{
    /// <summary>
    /// ファイル一覧取得時の除外ディレクトリ
    /// </summary>
    public IEnumerable<string> ExcludedDirectories { get; }
}
