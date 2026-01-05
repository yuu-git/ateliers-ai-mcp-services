namespace Ateliers.Ai.Mcp.Services.LocalFs;

/// <summary>
/// ローカルファイルシステム操作サービス
/// </summary>
public class LocalFileService : McpServiceBase, ILocalFileService
{
    private readonly ILocalFileSystemSettings _localFileSystemSettings;
    private const string LogPrefix = $"{nameof(LocalFileService)}:";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="localFileSystemSettings">ローカルファイルシステム設定</param>
    public LocalFileService(IMcpLogger mcpLogger, ILocalFileSystemSettings localFileSystemSettings)
        : base(mcpLogger)
    {
        McpLogger?.Info($"{LogPrefix} 初期化処理開始");
        _localFileSystemSettings = localFileSystemSettings;
        McpLogger?.Info($"{LogPrefix} 初期化完了");
    }

    /// <summary>
    /// ファイルを読み取る
    /// </summary>
    public async Task<string> ReadFileAsync(string basePath, string filePath)
    {
        McpLogger?.Info($"{LogPrefix} ReadFileAsync 開始: basePath={basePath}, filePath={filePath}");

        var fullPath = Path.Combine(basePath, filePath);
        McpLogger?.Debug($"{LogPrefix} ReadFileAsync: fullPath={fullPath}");

        if (!File.Exists(fullPath))
        {
            var ex = new FileNotFoundException($"File not found: {filePath}");
            McpLogger?.Critical($"{LogPrefix} ReadFileAsync: ファイルが見つかりません: fullPath={fullPath}", ex);
            throw ex;
        }

        var content = await File.ReadAllTextAsync(fullPath);
        McpLogger?.Info($"{LogPrefix} ReadFileAsync 完了: サイズ={content.Length}文字");

        return content;
    }

    /// <summary>
    /// ファイル一覧を取得
    /// </summary>
    public async Task<List<string>> ListFilesAsync(
        string basePath,
        string directory = "",
        string? extensionString = null)
    {
        McpLogger?.Info($"{LogPrefix} ListFilesAsync 開始: basePath={basePath}, directory={directory}, extension={extensionString}");

        var searchPath = string.IsNullOrEmpty(directory)
            ? basePath
            : Path.Combine(basePath, directory);

        McpLogger?.Debug($"{LogPrefix} ListFilesAsync: searchPath={searchPath}");

        if (!Directory.Exists(searchPath))
        {
            McpLogger?.Warn($"{LogPrefix} ListFilesAsync: ディレクトリが存在しません: searchPath={searchPath}");
            return new List<string>();
        }

        var searchPattern = extensionString != null ? $"*{extensionString}" : "*";
        McpLogger?.Debug($"{LogPrefix} ListFilesAsync: searchPattern={searchPattern}");

        var files = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories)
            .Where(f => !IsExcludedPath(f, basePath, _localFileSystemSettings.ExcludedDirectories))
            .Select(f => Path.GetRelativePath(basePath, f).Replace("\\", "/"))
            .ToList();

        McpLogger?.Info($"{LogPrefix} ListFilesAsync 完了: {files.Count}件取得");

        return await Task.FromResult(files);
    }

    /// <summary>
    /// 新規ファイルを作成
    /// </summary>
    public async Task CreateFileAsync(string basePath, string filePath, string content)
    {
        McpLogger?.Info($"{LogPrefix} CreateFileAsync 開始: basePath={basePath}, filePath={filePath}");

        var fullPath = Path.Combine(basePath, filePath);
        McpLogger?.Debug($"{LogPrefix} CreateFileAsync: fullPath={fullPath}");

        if (File.Exists(fullPath))
        {
            var ex = new InvalidOperationException($"File already exists: {filePath}");
            McpLogger?.Critical($"{LogPrefix} CreateFileAsync: ファイルが既に存在します: fullPath={fullPath}", ex);
            throw ex;
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null && !Directory.Exists(directory))
        {
            McpLogger?.Debug($"{LogPrefix} CreateFileAsync: ディレクトリを作成します: directory={directory}");
            Directory.CreateDirectory(directory);
        }

        McpLogger?.Debug($"{LogPrefix} CreateFileAsync: ファイルを書き込み中... サイズ={content.Length}文字");
        await File.WriteAllTextAsync(fullPath, content);

        McpLogger?.Info($"{LogPrefix} CreateFileAsync 完了: fullPath={fullPath}");
    }

    /// <summary>
    /// 既存ファイルを更新
    /// </summary>
    public async Task UpdateFileAsync(string basePath, string filePath, string content, bool createBackup = true)
    {
        McpLogger?.Info($"{LogPrefix} UpdateFileAsync 開始: basePath={basePath}, filePath={filePath}, createBackup={createBackup}");

        var fullPath = Path.Combine(basePath, filePath);
        McpLogger?.Debug($"{LogPrefix} UpdateFileAsync: fullPath={fullPath}");

        if (!File.Exists(fullPath))
        {
            var ex = new FileNotFoundException($"File not found: {filePath}");
            McpLogger?.Critical($"{LogPrefix} UpdateFileAsync: ファイルが見つかりません: fullPath={fullPath}", ex);
            throw ex;
        }

        // バックアップ作成
        if (createBackup)
        {
            var backupPath = $"{fullPath}.backup";
            McpLogger?.Debug($"{LogPrefix} UpdateFileAsync: バックアップを作成します: backupPath={backupPath}");
            File.Copy(fullPath, backupPath, overwrite: true);
        }

        McpLogger?.Debug($"{LogPrefix} UpdateFileAsync: ファイルを更新中... サイズ={content.Length}文字");
        await File.WriteAllTextAsync(fullPath, content);

        McpLogger?.Info($"{LogPrefix} UpdateFileAsync 完了: fullPath={fullPath}");
    }

    /// <summary>
    /// ファイルを削除
    /// </summary>
    public void DeleteFile(string basePath, string filePath, bool createBackup = true)
    {
        McpLogger?.Info($"{LogPrefix} DeleteFile 開始: basePath={basePath}, filePath={filePath}, createBackup={createBackup}");

        var fullPath = Path.Combine(basePath, filePath);
        McpLogger?.Debug($"{LogPrefix} DeleteFile: fullPath={fullPath}");

        if (!File.Exists(fullPath))
        {
            var ex = new FileNotFoundException($"File not found: {filePath}");
            McpLogger?.Critical($"{LogPrefix} DeleteFile: ファイルが見つかりません: fullPath={fullPath}", ex);
            throw ex;
        }

        // .backupファイルはバックアップを作らない
        var shouldBackup = createBackup && !filePath.EndsWith(".backup");

        if (shouldBackup)
        {
            var backupPath = $"{fullPath}.backup";
            McpLogger?.Debug($"{LogPrefix} DeleteFile: バックアップを作成します: backupPath={backupPath}");
            File.Copy(fullPath, backupPath, overwrite: true);
        }

        McpLogger?.Debug($"{LogPrefix} DeleteFile: ファイルを削除中...");
        File.Delete(fullPath);

        McpLogger?.Info($"{LogPrefix} DeleteFile 完了: fullPath={fullPath}");
    }

    /// <summary>
    /// ファイルをリネーム
    /// </summary>
    public void RenameFile(string basePath, string oldFilePath, string newFilePath)
    {
        McpLogger?.Info($"{LogPrefix} RenameFile 開始: basePath={basePath}, oldFilePath={oldFilePath}, newFilePath={newFilePath}");

        var oldFullPath = Path.Combine(basePath, oldFilePath);
        var newFullPath = Path.Combine(basePath, newFilePath);
        McpLogger?.Debug($"{LogPrefix} RenameFile: oldFullPath={oldFullPath}, newFullPath={newFullPath}");

        if (!File.Exists(oldFullPath))
        {
            var ex = new FileNotFoundException($"File not found: {oldFilePath}");
            McpLogger?.Critical($"{LogPrefix} RenameFile: 元ファイルが見つかりません: oldFullPath={oldFullPath}", ex);
            throw ex;
        }

        if (File.Exists(newFullPath))
        {
            var ex = new InvalidOperationException($"Destination file already exists: {newFilePath}");
            McpLogger?.Critical($"{LogPrefix} RenameFile: 移動先ファイルが既に存在します: newFullPath={newFullPath}", ex);
            throw ex;
        }

        var newDirectory = Path.GetDirectoryName(newFullPath);
        if (newDirectory != null && !Directory.Exists(newDirectory))
        {
            McpLogger?.Debug($"{LogPrefix} RenameFile: ディレクトリを作成します: directory={newDirectory}");
            Directory.CreateDirectory(newDirectory);
        }

        McpLogger?.Debug($"{LogPrefix} RenameFile: ファイルを移動中...");
        File.Move(oldFullPath, newFullPath);

        McpLogger?.Info($"{LogPrefix} RenameFile 完了: {oldFilePath} -> {newFilePath}");
    }

    /// <summary>
    /// ファイルをコピー
    /// </summary>
    public void CopyFile(string basePath, string sourceFilePath, string destFilePath, bool overwrite = false)
    {
        McpLogger?.Info($"{LogPrefix} CopyFile 開始: basePath={basePath}, sourceFilePath={sourceFilePath}, destFilePath={destFilePath}, overwrite={overwrite}");

        var sourceFullPath = Path.Combine(basePath, sourceFilePath);
        var destFullPath = Path.Combine(basePath, destFilePath);
        McpLogger?.Debug($"{LogPrefix} CopyFile: sourceFullPath={sourceFullPath}, destFullPath={destFullPath}");

        if (!File.Exists(sourceFullPath))
        {
            var ex = new FileNotFoundException($"Source file not found: {sourceFilePath}");
            McpLogger?.Critical($"{LogPrefix} CopyFile: ソースファイルが見つかりません: sourceFullPath={sourceFullPath}", ex);
            throw ex;
        }

        if (File.Exists(destFullPath) && !overwrite)
        {
            var ex = new InvalidOperationException($"Destination file already exists: {destFilePath}");
            McpLogger?.Critical($"{LogPrefix} CopyFile: コピー先ファイルが既に存在します: destFullPath={destFullPath}", ex);
            throw ex;
        }

        var destDirectory = Path.GetDirectoryName(destFullPath);
        if (destDirectory != null && !Directory.Exists(destDirectory))
        {
            McpLogger?.Debug($"{LogPrefix} CopyFile: ディレクトリを作成します: directory={destDirectory}");
            Directory.CreateDirectory(destDirectory);
        }

        McpLogger?.Debug($"{LogPrefix} CopyFile: ファイルをコピー中...");
        File.Copy(sourceFullPath, destFullPath, overwrite);

        McpLogger?.Info($"{LogPrefix} CopyFile 完了: {sourceFilePath} -> {destFilePath}");
    }

    /// <summary>
    /// ファイルのバックアップを作成
    /// </summary>
    public void BackupFile(string basePath, string filePath, string? backupSuffix = null)
    {
        McpLogger?.Info($"{LogPrefix} BackupFile 開始: basePath={basePath}, filePath={filePath}, backupSuffix={backupSuffix}");

        var fullPath = Path.Combine(basePath, filePath);
        McpLogger?.Debug($"{LogPrefix} BackupFile: fullPath={fullPath}");

        if (!File.Exists(fullPath))
        {
            var ex = new FileNotFoundException($"File not found: {filePath}");
            McpLogger?.Critical($"{LogPrefix} BackupFile: ファイルが見つかりません: fullPath={fullPath}", ex);
            throw ex;
        }

        var backupPath = backupSuffix != null
            ? $"{fullPath}.{backupSuffix}"
            : $"{fullPath}.backup";

        McpLogger?.Debug($"{LogPrefix} BackupFile: バックアップを作成中... backupPath={backupPath}");
        File.Copy(fullPath, backupPath, overwrite: true);

        McpLogger?.Info($"{LogPrefix} BackupFile 完了: backupPath={backupPath}");
    }

    /// <summary>
    /// 除外対象のパスかどうかを判定
    /// </summary>
    private static bool IsExcludedPath(string filePath, string basePath, IEnumerable<string>? excludedDirectories = null)
    {
        // 除外ディレクトリが空の場合は除外しない
        if (excludedDirectories != null && !excludedDirectories.Any())
        {
            return false;
        }

        // 除外ディレクトリ一覧を設定（nullの場合はデフォルト値を使用）
        var excludedDir = excludedDirectories ?? new List<string>()
        {
          "bin",
          "obj",
          "node_modules",
          ".git"
        };
        var relativePath = Path.GetRelativePath(basePath, filePath);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // パスのいずれかの部分が除外ディレクトリに該当するか
        return pathParts.Any(part => excludedDir.Contains(part, StringComparer.OrdinalIgnoreCase));
    }
}