namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Notion Tasksサービスのインターフェース
/// </summary>
public interface INotionTasksService
{
    /// <summary>タスクを追加</summary>
    Task<string> AddTaskAsync(
        string title,
        string? description = null,
        string? status = "未着手",
        string? priority = "中",
        DateTime? dueDate = null,
        string? location = null,
        string[]? tags = null,
        string? registrant = null);

    /// <summary>タスクを更新</summary>
    Task<string> UpdateTaskAsync(
        string taskId,
        string? title = null,
        string? description = null,
        string? status = null,
        string? priority = null,
        DateTime? dueDate = null,
        string? location = null,
        string[]? tags = null);

    /// <summary>タスク一覧を取得</summary>
    Task<string> ListTasksAsync(
        string? status = null,
        string? priority = null,
        bool? dueSoon = null,
        int limit = 10);

    /// <summary>タスクを完了にする</summary>
    Task<string> CompleteTaskAsync(string taskId);
}

/// <summary>
/// Notion Ideasサービスのインターフェース
/// </summary>
public interface INotionIdeasService
{
    /// <summary>アイデアを追加</summary>
    Task<string> AddIdeaAsync(
        string title,
        string? content = null,
        string[]? tags = null,
        string? link = null,
        string? registrant = null);

    /// <summary>アイデアを検索</summary>
    Task<string> SearchIdeasAsync(
        string? keyword = null,
        string[]? tags = null,
        int limit = 10);

    /// <summary>アイデアを更新</summary>
    Task<string> UpdateIdeaAsync(
        string ideaId,
        string? title = null,
        string? content = null,
        string[]? tags = null,
        string? status = null,
        string? link = null);
}

/// <summary>
/// Notion Reading Listサービスのインターフェース
/// </summary>
public interface INotionReadingListService
{
    /// <summary>リーディングリストに追加</summary>
    Task<string> AddToReadingListAsync(
        string title,
        string? link = null,
        string? type = null,
        string? status = "未読",
        string? priority = "中",
        DateTime? date = null,
        bool reference = false,
        string[]? tags = null,
        string? registrant = null,
        string? notes = null,
        string? description = null,
        string? author = null);

    /// <summary>リーディングリスト一覧を取得</summary>
    Task<string> ListReadingListAsync(
        string? status = null,
        string? priority = null,
        int limit = 20);

    /// <summary>リーディングリストのステータスを更新</summary>
    Task<string> UpdateReadingListStatusAsync(
        string readingListId,
        string status,
        DateTime? completedDate = null);
}
