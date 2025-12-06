using Notion.Client;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion Tasks データベースの操作を担当するサービス
/// </summary>
public class NotionTasksService : NotionServiceBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notionSettings"> Notion設定 </param>
    public NotionTasksService(INotionSettings notionSettings) : base(notionSettings)
    {
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
        var databaseId = GetTasksDatabaseId();

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
        }

        // Priority
        if (!string.IsNullOrWhiteSpace(priority))
        {
            properties["Priority"] = new SelectPropertyValue { Select = new SelectOption { Name = priority } };
        }

        // Date
        if (dueDate.HasValue)
        {
            properties["Date"] = new DatePropertyValue
            {
                Date = new Date { Start = dueDate.Value }
            };
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
        }

        // Tags
        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
        }

        // Registrant
        if (!string.IsNullOrWhiteSpace(registrant))
        {
            properties["Registrant"] = new SelectPropertyValue { Select = new SelectOption { Name = registrant } };
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
        }

        var page = await Client.Pages.CreateAsync(request);
        return $"Task created successfully: {title} (ID: {page.Id})";
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
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } };
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            properties["Priority"] = new SelectPropertyValue { Select = new SelectOption { Name = priority } };
        }

        if (dueDate.HasValue)
        {
            properties["Date"] = new DatePropertyValue
            {
                Date = new Date { Start = dueDate.Value }
            };
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
        }

        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
        }

        var request = new PagesUpdateParameters { Properties = properties };
        var page = await Client.Pages.UpdateAsync(taskId, request);

        // Description 更新時はページブロックを追加
        if (!string.IsNullOrWhiteSpace(description))
        {
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

        return $"Task updated successfully (ID: {page.Id})";
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
        var databaseId = GetTasksDatabaseId();
        var filters = new List<Filter>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add(new SelectFilter("Status", equal: status));
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            filters.Add(new SelectFilter("Priority", equal: priority));
        }

        if (dueSoon == true)
        {
            var nextWeek = DateTime.Now.AddDays(7);
            filters.Add(new DateFilter("Date", onOrBefore: nextWeek));
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
        }

        var response = await Client.Databases.QueryAsync(databaseId, queryParams);

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
        return await UpdateTaskAsync(taskId, status: "完了");
    }
}
