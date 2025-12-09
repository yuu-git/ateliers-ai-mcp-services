using Notion.Client;
using Ateliers.Ai.Mcp.Services;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion Reading List データベースの操作を担当するサービス
/// </summary>
public class NotionReadingListService : NotionServiceBase, INotionReadingListService
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notionSettings"> Notion設定 </param>
    public NotionReadingListService(INotionSettings notionSettings) : base(notionSettings)
    {
    }

    /// <summary>
    /// Reading List Database IDを取得
    /// </summary>
    private string GetReadingListDatabaseId() => GetDatabaseId("ReadingList");

    /// <summary>
    /// リーディングリストに追加
    /// </summary>
    public async Task<string> AddToReadingListAsync(
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
        string? author = null)
    {
        var databaseId = GetReadingListDatabaseId();

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

        // Link
        if (!string.IsNullOrWhiteSpace(link))
        {
            properties["Link"] = new UrlPropertyValue { Url = link };
        }

        // Type
        if (!string.IsNullOrWhiteSpace(type))
        {
            properties["Type"] = new SelectPropertyValue { Select = new SelectOption { Name = type } };
        }

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
        if (date.HasValue)
        {
            properties["Date"] = new DatePropertyValue
            {
                Date = new Date { Start = date.Value }
            };
        }

        // Reference (Checkbox)
        properties["Reference"] = new CheckboxPropertyValue { Checkbox = reference };

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

        // Description (Rich Text)
        if (!string.IsNullOrWhiteSpace(description))
        {
            properties["Description"] = new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = description } }
                }
            };
        }

        // Author (Text)
        if (!string.IsNullOrWhiteSpace(author))
        {
            properties["Author"] = new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = author } }
                }
            };
        }

        var request = new PagesCreateParameters
        {
            Parent = new DatabaseParentInput { DatabaseId = databaseId },
            Properties = properties
        };

        // Notes をページブロックとして追加
        if (!string.IsNullOrWhiteSpace(notes))
        {
            request.Children = new List<IBlock>
            {
                new ParagraphBlock
                {
                    Paragraph = new ParagraphBlock.Info
                    {
                        RichText = new List<RichTextBase>
                        {
                            new RichTextText { Text = new Text { Content = notes } }
                        }
                    }
                }
            };
        }

        var page = await Client.Pages.CreateAsync(request);
        return $"Reading List added successfully: {title} (ID: {page.Id})";
    }

    /// <summary>
    /// リーディングリスト一覧を取得
    /// </summary>
    public async Task<string> ListReadingListAsync(
        string? status = null,
        string? priority = null,
        int limit = 20)
    {
        var databaseId = GetReadingListDatabaseId();
        var filters = new List<Filter>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add(new SelectFilter("Status", equal: status));
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            filters.Add(new SelectFilter("Priority", equal: priority));
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
            return "No reading list items found.";
        }

        var items = response.Results.Select(page =>
        {
            var props = (page as Page)?.Properties;
            var itemTitle = props != null && props.ContainsKey("Name") && props["Name"] is TitlePropertyValue titleProp
                ? string.Join("", titleProp.Title.Select(t => t.PlainText))
                : "Untitled";

            var statusValue = props != null && props.ContainsKey("Status") && props["Status"] is SelectPropertyValue selectProp
                ? selectProp.Select?.Name ?? "未設定"
                : "未設定";

            var priorityValue = props != null && props.ContainsKey("Priority") && props["Priority"] is SelectPropertyValue priorityProp
                ? priorityProp.Select?.Name ?? "未設定"
                : "未設定";

            var typeValue = props != null && props.ContainsKey("Type") && props["Type"] is SelectPropertyValue typeProp
                ? typeProp.Select?.Name ?? "未設定"
                : "未設定";

            return $"- [{statusValue}] {itemTitle} (種類: {typeValue}, 優先度: {priorityValue}, ID: {page.Id})";
        });

        return $"Reading List ({response.Results.Count}):\n" + string.Join("\n", items);
    }

    /// <summary>
    /// リーディングリストのステータスを更新
    /// </summary>
    public async Task<string> UpdateReadingListStatusAsync(
        string readingListId,
        string status,
        DateTime? completedDate = null)
    {
        var properties = new Dictionary<string, PropertyValue>
        {
            ["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } }
        };

        // 完了日を設定（status=完了の場合）
        if (completedDate.HasValue)
        {
            properties["CompletedDate"] = new DatePropertyValue
            {
                Date = new Date { Start = completedDate.Value }
            };
        }

        var request = new PagesUpdateParameters { Properties = properties };
        var page = await Client.Pages.UpdateAsync(readingListId, request);

        return $"Reading List status updated successfully (ID: {page.Id})";
    }
}
