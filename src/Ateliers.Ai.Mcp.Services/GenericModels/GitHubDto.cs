namespace Ateliers.Ai.Mcp.Services.GenericModels;

/// <summary>
/// GitHubリポジトリ情報DTO
/// </summary>
/// /// <remarks>
/// GitHub API 専用の拡張DTO。GitRepositoryInfoDtoのベース情報に加えて、
/// スター数・フォーク数・説明・トピック・リリースなどのメタ情報を含む。
/// </remarks>
public class GitHubRepositoryInfoDto : GitRepositoryInfoDto
{
    /// <summary>
    /// スター数
    /// </summary>
    public int Stars { get; init; }

    /// <summary>
    /// フォーク数
    /// </summary>
    public int Forks { get; init; }

    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// デフォルトブランチ名（mainやmasterなど）
    /// </summary>
    public string DefaultBranch { get; init; } = string.Empty;

    /// <summary>
    /// トピック一覧
    /// </summary>
    public List<string> Topics { get; init; } = new();

    /// <summary>
    /// リリース一覧
    /// </summary>
    public List<GitHubReleaseDto> Releases { get; init; } = new();
}

/// <summary>
/// GitHubリリースDTO
/// </summary>
public class GitHubReleaseDto
{
    /// <summary>
    /// タグ名（v1.0.0など）
    /// </summary>
    public string TagName { get; init; } = string.Empty;

    /// <summary>
    /// リリース名
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// リリースノート（本文）
    /// </summary>
    /// <remarks>
    /// 必要に応じてMarkdownを返す。全文は長い場合がある点に注意。
    /// </remarks>
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// 公開日時
    /// </summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>
    /// アセット数（詳細情報は含めない）
    /// </summary>
    public int AssetCount { get; init; }

    // Asset情報はサイズ・URLなどで情報量が非常に多く、MCP返却では過剰となるため当面は非公開。
    // 必要になった場合（ビルド成果物ダウンロードなど）にのみ実装する。
    // public List<GitHubAssetDto> Assets { get; init; } = new();
}

/* (忘れてしまうので) アセット詳細の実装例：
public class GitHubAssetDto
{
    public string Name { get; init; } = string.Empty;
    public long Size { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;
}
*/