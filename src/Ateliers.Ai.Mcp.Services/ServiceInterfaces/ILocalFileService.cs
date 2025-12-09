namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// ローカルファイルシステム操作サービスのインターフェース
/// </summary>
public interface ILocalFileService
{
    /// <summary>
    /// ファイルを読み取る
    /// </summary>
    Task<string> ReadFileAsync(string basePath, string filePath);

    /// <summary>
    /// ファイル一覧を取得
    /// </summary>
    Task<List<string>> ListFilesAsync(
        string basePath,
        string directory = "",
        string? extensionString = null);

    /// <summary>
    /// 新規ファイルを作成
    /// </summary>
    Task CreateFileAsync(string basePath, string filePath, string content);

    /// <summary>
    /// 既存ファイルを更新
    /// </summary>
    Task UpdateFileAsync(string basePath, string filePath, string content, bool createBackup = true);

    /// <summary>
    /// ファイルを削除
    /// </summary>
    void DeleteFile(string basePath, string filePath, bool createBackup = true);

    /// <summary>
    /// ファイルをリネーム
    /// </summary>
    void RenameFile(string basePath, string oldFilePath, string newFilePath);

    /// <summary>
    /// ファイルをコピー
    /// </summary>
    void CopyFile(string basePath, string sourceFilePath, string destFilePath, bool overwrite = false);

    /// <summary>
    /// ファイルのバックアップを作成
    /// </summary>
    void BackupFile(string basePath, string filePath, string? backupSuffix = null);
}
