using Notion.Client;
using Ateliers.Ai.Mcp.Services;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion Tasks データベースの操作を担当するサービス
/// </summary>
public class NotionTasksService : NotionServiceBase, INotionTasksService
{
    private const string ServiceLogPrefix = $"{nameof(NotionTasksService)}:";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notionSettings"> Notion設定 </param>
    public NotionTasksService(IMcpLogger mcpLogger, INotionSettings notionSettings) : base(mcpLogger, notionSettings)
    {
        McpLogger?.Info($"{ServiceLogPrefix} 初期化完了");
    }

    /// <summary>
    /// Tasks Database IDを取得
    /// </summary>
    private string GetTasksDatabaseId() => GetDatabaseId("Tasks");

    /// <summary>
    /// タスクを追加
    /// </summary>
    public async Task<string> AddTaskAsync(
        string title,
        string? description = null,
        string? status = "未着手",
        string? priority = "中",
        DateTime? dueDate = null,
        string? location = null,
        string[]? tags = null,
        string? registrant = null)
    {
        McpLogger?.Info($"{ServiceLogPrefix} AddTaskAsync 開始: title={title}, status={status}, priority={priority}");

        var databaseId = GetTasksDatabaseId();
        McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: databaseId={databaseId}");

        var properties = new Dictionary<string, PropertyValue>
        {
            ["Name"] = new TitlePropertyValue
            {
                Title = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = title } }
                }
            }
        };

        // Status
        if (!string.IsNullOrWhiteSpace(status))
        {
            properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: ステータスを設定: {status}");
        }

        // Priority
        if (!string.IsNullOrWhiteSpace(priority))
        {
            properties["Priority"] = new SelectPropertyValue { Select = new SelectOption { Name = priority } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: 優先度を設定: {priority}");
        }

        // Date
        if (dueDate.HasValue)
        {
            properties["Date"] = new DatePropertyValue
            {
                Date = new Date { Start = dueDate.Value }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: 期限を設定: {dueDate.Value:yyyy-MM-dd}");
        }

        // Location (Text)
        if (!string.IsNullOrWhiteSpace(location))
        {
            properties["Location"] = new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = location } }
                }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: 場所を設定: {location}");
        }

        // Tags
        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: タグを設定: {string.Join(", ", tags)}");
        }

        // Registrant
        if (!string.IsNullOrWhiteSpace(registrant))
        {
            properties["Registrant"] = new SelectPropertyValue { Select = new SelectOption { Name = registrant } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: 登録者を設定: {registrant}");
        }

        var request = new PagesCreateParameters
        {
            Parent = new DatabaseParentInput { DatabaseId = databaseId },
            Properties = properties
        };

        // Description をページブロックとして追加
        if (!string.IsNullOrWhiteSpace(description))
        {
            request.Children = new List<IBlock>
            {
                new ParagraphBlock
                {
                    Paragraph = new ParagraphBlock.Info
                    {
                        RichText = new List<RichTextBase>
                        {
                            new RichTextText { Text = new Text { Content = description } }
                        }
                    }
                }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} AddTaskAsync: 説明を設定: サイズ={description.Length}文字");
        }

        McpLogger?.Info($"{ServiceLogPrefix} AddTaskAsync: Notion API 呼び出し中...");
        var page = await Client.Pages.CreateAsync(request);

        var result = $"Task created successfully: {title} (ID: {page.Id})";
        McpLogger?.Info($"{ServiceLogPrefix} AddTaskAsync 完了: pageId={page.Id}");
        return result;
    }

    /// <summary>
    /// タスクを更新
    /// </summary>
    public async Task<string> UpdateTaskAsync(
        string taskId,
        string? title = null,
        string? description = null,
        string? status = null,
        string? priority = null,
        DateTime? dueDate = null,
        string? location = null,
        string[]? tags = null)
    {
        McpLogger?.Info($"{ServiceLogPrefix} UpdateTaskAsync 開始: taskId={taskId}");

        var properties = new Dictionary<string, PropertyValue>();

        if (!string.IsNullOrWhiteSpace(title))
        {
            properties["Name"] = new TitlePropertyValue
            {
                Title = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = title } }
                }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: タイトルを更新: {title}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: ステータスを更新: {status}");
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            properties["Priority"] = new SelectPropertyValue { Select = new SelectOption { Name = priority } };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: 優先度を更新: {priority}");
        }

        if (dueDate.HasValue)
        {
            properties["Date"] = new DatePropertyValue
            {
                Date = new Date { Start = dueDate.Value }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: 期限を更新: {dueDate.Value:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            properties["Location"] = new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = location } }
                }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: 場所を更新: {location}");
        }

        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: タグを更新: {string.Join(", ", tags)}");
        }

        McpLogger?.Info($"{ServiceLogPrefix} UpdateTaskAsync: Notion API 呼び出し中...");
        var request = new PagesUpdateParameters { Properties = properties };
        var page = await Client.Pages.UpdateAsync(taskId, request);

        // Description 更新時はページブロックを追加
        if (!string.IsNullOrWhiteSpace(description))
        {
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateTaskAsync: 説明を追加: サイズ={description.Length}文字");
            await Client.Blocks.AppendChildrenAsync(new BlockAppendChildrenRequest
            {
                BlockId = taskId,
                Children = new List<IBlockObjectRequest>
                {
                    new ParagraphBlockRequest
                    {
                        Paragraph = new ParagraphBlockRequest.Info
                        {
                            RichText = new List<RichTextBase>
                            {
                                new RichTextText { Text = new Text { Content = description } }
                            }
                        }
                    }
                }
            });
        }

        var result = $"Task updated successfully (ID: {page.Id})";
        McpLogger?.Info($"{ServiceLogPrefix} UpdateTaskAsync 完了: pageId={page.Id}");
        return result;
    }

    /// <summary>
    /// タスク一覧を取得
    /// </summary>
    public async Task<string> ListTasksAsync(
        string? status = null,
        string? priority = null,
        bool? dueSoon = null,
        int limit = 10)
    {
        McpLogger?.Info($"{ServiceLogPrefix} ListTasksAsync 開始: status={status}, priority={priority}, dueSoon={dueSoon}, limit={limit}");

        var databaseId = GetTasksDatabaseId();
        var filters = new List<Filter>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add(new SelectFilter("Status", equal: status));
            McpLogger?.Debug($"{ServiceLogPrefix} ListTasksAsync: ステータスフィルタを追加: {status}");
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            filters.Add(new SelectFilter("Priority", equal: priority));
            McpLogger?.Debug($"{ServiceLogPrefix} ListTasksAsync: 優先度フィルタを追加: {priority}");
        }

        if (dueSoon == true)
        {
            var nextWeek = DateTime.Now.AddDays(7);
            filters.Add(new DateFilter("Date", onOrBefore: nextWeek));
            McpLogger?.Debug($"{ServiceLogPrefix} ListTasksAsync: 期限フィルタを追加: {nextWeek:yyyy-MM-dd}まで");
        }

        var queryParams = new DatabasesQueryParameters
        {
            PageSize = limit
        };

        if (filters.Count > 0)
        {
            queryParams.Filter = filters.Count == 1
                ? filters[0]
                : new CompoundFilter { And = filters };
            McpLogger?.Debug($"{ServiceLogPrefix} ListTasksAsync: フィルタ数={filters.Count}");
        }

        McpLogger?.Info($"{ServiceLogPrefix} ListTasksAsync: Notion API 呼び出し中...");
        var response = await Client.Databases.QueryAsync(databaseId, queryParams);

        McpLogger?.Info($"{ServiceLogPrefix} ListTasksAsync 完了: {response.Results.Count}件取得");

        if (response.Results.Count == 0)
        {
            return "No tasks found.";
        }

        var tasks = response.Results.Select(page =>
        {
            var props = (page as Page)?.Properties;
            var title = props != null && props.ContainsKey("Name") && props["Name"] is TitlePropertyValue titleProp
                ? string.Join("", titleProp.Title.Select(t => t.PlainText))
                : "Untitled";

            var statusValue = props != null && props.ContainsKey("Status") && props["Status"] is SelectPropertyValue selectProp
                ? selectProp.Select?.Name ?? "未設定"
                : "未設定";

            var priorityValue = props != null && props.ContainsKey("Priority") && props["Priority"] is SelectPropertyValue priorityProp
                ? priorityProp.Select?.Name ?? "未設定"
                : "未設定";

            return $"- [{statusValue}] {title} (優先度: {priorityValue}, ID: {page.Id})";
        });

        return $"Tasks ({response.Results.Count}):\n" + string.Join("\n", tasks);
    }

    /// <summary>
    /// タスクを完了にする
    /// </summary>
    public async Task<string> CompleteTaskAsync(string taskId)
    {
        McpLogger?.Info($"{ServiceLogPrefix} CompleteTaskAsync: taskId={taskId}");
        return await UpdateTaskAsync(taskId, status: "完了");
    }
}
