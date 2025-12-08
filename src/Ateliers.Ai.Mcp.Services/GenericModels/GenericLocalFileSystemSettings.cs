namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// 汎用的ローカルファイルシステム設定
/// </summary>
public class GenericLocalFileSystemSettings : ILocalFileSystemSettings
{
    /// <summary>
    /// ファイル一覧取得時の除外ディレクトリ
    /// </summary>
    /// <remarks> 初期除外ディレクトリ: bin, obj, node_modules, .git, .vs, .vscode, packages, TestResults, .idea </remarks>
    public virtual IEnumerable<string> ExcludedDirectories { get; set; } = new List<string>
    {
        "bin",
        "obj",
        "node_modules",
        ".git",
        ".vs",
        ".vscode",
        "packages",
        "TestResults",
        ".idea"
    };
}
