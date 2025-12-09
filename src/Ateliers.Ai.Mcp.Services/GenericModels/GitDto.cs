namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// Gitリポジトリ情報DTO
/// </summary>
public class GitRepositoryInfoDto
{
    /// <summary>
    /// ローカルリポジトリの名称（フォルダ名）
    /// </summary>
    /// <remarks>GitHub オーナー名とは無関係。MCP返却用にシンプル化している。</remarks>
    public string RepositoryName { get; init; } = string.Empty;

    /// <summary>
    /// 現在のブランチ名
    /// </summary>
    public string CurrentBranch { get; init; } = string.Empty;

    /// <summary>
    /// リポジトリがクリーンかどうか
    /// </summary>
    public bool IsClean { get; set; }

    /// <summary>
    /// ブランチ情報一覧
    /// </summary>
    public List<BranchInfoDto> Branches { get; set; } = new();

    /// <summary>
    /// 最近のコミット情報一覧
    /// </summary>
    public List<CommitInfoDto> RecentCommits { get; set; } = new();

    /// <summary>
    /// ファイルステータス情報一覧
    /// </summary>
    public List<FileStatusInfoDto> Status { get; set; } = new();

    /// <summary>
    /// リモート情報一覧
    /// </summary>
    public List<RemoteInfoDto> Remotes { get; set; } = new();

    /// <summary>
    /// タグ一覧
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// ブランチ情報DTO
/// </summary>
/// <remarks>
/// コミッター名・メールアドレスは返さない（機密情報になりえるため）
/// </remarks>
public class BranchInfoDto
{
    /// <summary>
    /// ブランチ名
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 最新のコミットSHA
    /// </summary>
    public string LatestCommitSha { get; init; } = string.Empty;

    /// <summary>
    /// 最新のコミット日付
    /// </summary>
    public DateTime LatestCommitDate { get; set; }
}

/// <summary>
/// コミット情報DTO
/// </summary>
/// <remarks>
/// メッセージ全文は返すが diff は返さない。全コミットの diff は数十MB 以上になる可能性があるため。
/// </remarks>
public class CommitInfoDto
{
    /// <summary>
    /// コミットSHA
    /// </summary>
    public string Sha { get; init; } = string.Empty;

    /// <summary>
    /// コミットメッセージ
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// コミット日付
    /// </summary>
    public DateTime Date { get; set; }
}

/// <summary>
/// ファイルステータス情報DTO
/// </summary>
public class FileStatusInfoDto
{
    /// <summary>
    /// ファイルパス
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// ファイルの状態
    /// </summary>
    /// <remarks>Modified, Untracked, Added, Deleted など</remarks>
    public string State { get; init; } = string.Empty;
}

/// <summary>
/// リモート情報DTO
/// </summary>
public class RemoteInfoDto
{
    /// <summary>
    /// リモート名
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// リモートURL
    /// </summary>
    /// <remarks>
    /// ＜＜注意！＞＞：必要ならマスキング推奨 (GitHub の URL はプライベート情報になりえる)
    /// <para>
    /// 【例】
    ///   https://github.com/USERNAME/REPO → github.com/...（ドメインのみ）
    /// </para>
    ///
    /// <para>
    /// 【実例】
    ///   'https://github.com/yuu-git/ateliers-ai-mcp-service'
    ///      → 'github.com/...'
    /// </para>
    /// </remarks>
    public string Url { get; init; } = string.Empty;

}
