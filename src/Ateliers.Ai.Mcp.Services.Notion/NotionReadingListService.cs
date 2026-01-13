using Notion.Client;
using Ateliers.Ai.Mcp.Services;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion Reading List データベースの操作を担当するサービス
/// </summary>
public class NotionReadingListService : NotionServiceBase, INotionReadingListService
{
    private const string ServiceLogPrefix = $"{nameof(NotionReadingListService)}:";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notionSettings"> Notion設定 </param>
    public NotionReadingListService(IMcpLogger mcpLogger, INotionSettings notionSettings) : base(mcpLogger, notionSettings)
    {
        McpLogger?.Info($"{ServiceLogPrefix} 初期化完了");
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
        McpLogger?.Info($"{ServiceLogPrefix} AddToReadingListAsync 開始: title={title}, type={type}, status={status}");

        var databaseId = GetReadingListDatabaseId();
        McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: databaseId={databaseId}");

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
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: リンクを設定");
        }

        // Type
        if (!string.IsNullOrWhiteSpace(type))
        {
            properties["Type"] = new SelectPropertyValue { Select = new SelectOption { Name = type } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 種類を設定: {type}");
        }

        // Status
        if (!string.IsNullOrWhiteSpace(status))
        {
            properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: ステータスを設定: {status}");
        }

        // Priority
        if (!string.IsNullOrWhiteSpace(priority))
        {
            properties["Priority"] = new SelectPropertyValue { Select = new SelectOption { Name = priority } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 優先度を設定: {priority}");
        }

        // Date
        if (date.HasValue)
        {
            properties["Date"] = new DatePropertyValue
            {
                Date = new Date { Start = date.Value }
            };
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 日付を設定: {date.Value:yyyy-MM-dd}");
        }

        // Reference (Checkbox)
        properties["Reference"] = new CheckboxPropertyValue { Checkbox = reference };
        McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 参照フラグを設定: {reference}");

        // Tags
        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: タグを設定: {string.Join(", ", tags)}");
        }

        // Registrant
        if (!string.IsNullOrWhiteSpace(registrant))
        {
            properties["Registrant"] = new SelectPropertyValue { Select = new SelectOption { Name = registrant } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 登録者を設定: {registrant}");
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
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 説明を設定: サイズ={description.Length}文字");
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
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: 著者を設定: {author}");
        }

        var request = new PagesCreateParameters
        {
            Parent = new DatabaseParentRequest { DatabaseId = databaseId },
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
            McpLogger?.Debug($"{ServiceLogPrefix} AddToReadingListAsync: ノートを設定: サイズ={notes.Length}文字");
        }

        McpLogger?.Info($"{ServiceLogPrefix} AddToReadingListAsync: Notion API 呼び出し中...");
        var page = await Client.Pages.CreateAsync(request);

        var result = $"Reading List added successfully: {title} (ID: {page.Id})";
        McpLogger?.Info($"{ServiceLogPrefix} AddToReadingListAsync 完了: pageId={page.Id}");
        return result;
    }

    /// <summary>
    /// リーディングリスト一覧を取得
    /// </summary>
    public async Task<string> ListReadingListAsync(
        string? status = null,
        string? priority = null,
        int limit = 20)
    {
        McpLogger?.Info($"{ServiceLogPrefix} ListReadingListAsync 開始: status={status}, priority={priority}, limit={limit}");

        var databaseId = GetReadingListDatabaseId();

        McpLogger?.Info($"{ServiceLogPrefix} ListReadingListAsync: Notion API 呼び出し中...");
        // Search APIを使用（SearchRequestを使用）
        var searchRequest = new SearchRequest
        {
            Filter = new SearchFilter { Value = SearchObjectType.Page }
        };
        var response = await Client.Search.SearchAsync(searchRequest);

        // データベースに属するページのみをフィルタリング
        var databasePages = response.Results
            .Where(r => r is Page page && 
                   page.Parent is DatabaseParent dbParent && 
                   dbParent.DatabaseId == databaseId)
            .Cast<Page>()
            .ToList();

        // Status フィルタを手動で適用
        if (!string.IsNullOrWhiteSpace(status))
        {
            databasePages = databasePages.Where(p =>
            {
                if (p.Properties.TryGetValue("Status", out var propValue))
                {
                    if (propValue is SelectPropertyValue selectProp)
                    {
                        return selectProp.Select?.Name == status;
                    }
                }
                return false;
            }).ToList();
            McpLogger?.Debug($"{ServiceLogPrefix} ListReadingListAsync: ステータスフィルタを適用: {status}");
        }

        // Priority フィルタを手動で適用
        if (!string.IsNullOrWhiteSpace(priority))
        {
            databasePages = databasePages.Where(p =>
            {
                if (p.Properties.TryGetValue("Priority", out var propValue))
                {
                    if (propValue is SelectPropertyValue selectProp)
                    {
                        return selectProp.Select?.Name == priority;
                    }
                }
                return false;
            }).ToList();
            McpLogger?.Debug($"{ServiceLogPrefix} ListReadingListAsync: 優先度フィルタを適用: {priority}");
        }

        // Limit適用
        databasePages = databasePages.Take(limit).ToList();

        McpLogger?.Info($"{ServiceLogPrefix} ListReadingListAsync 完了: {databasePages.Count}件取得");

        if (databasePages.Count == 0)
        {
            return "No reading list items found.";
        }

        var items = databasePages.Select(page =>
        {
            var props = page.Properties;
            
            string itemTitle = "Untitled";
            if (props != null && props.TryGetValue("Name", out var nameValue))
            {
                if (nameValue is TitlePropertyValue titlePropertyValue)
                {
                    itemTitle = string.Join("", titlePropertyValue.Title.Select(t => t.PlainText));
                }
            }

            string statusValue = "未設定";
            if (props != null && props.TryGetValue("Status", out var statusVal))
            {
                if (statusVal is SelectPropertyValue statusPropertyValue)
                {
                    statusValue = statusPropertyValue.Select?.Name ?? "未設定";
                }
            }

            string priorityValue = "未設定";
            if (props != null && props.TryGetValue("Priority", out var priorityVal))
            {
                if (priorityVal is SelectPropertyValue priorityPropertyValue)
                {
                    priorityValue = priorityPropertyValue.Select?.Name ?? "未設定";
                }
            }

            string typeValue = "未設定";
            if (props != null && props.TryGetValue("Type", out var typeVal))
            {
                if (typeVal is SelectPropertyValue typePropertyValue)
                {
                    typeValue = typePropertyValue.Select?.Name ?? "未設定";
                }
            }

            return $"- [{statusValue}] {itemTitle} (種類: {typeValue}, 優先度: {priorityValue}, ID: {page.Id})";
        });

        return $"Reading List ({databasePages.Count}):\n" + string.Join("\n", items);
    }

    /// <summary>
    /// リーディングリストのステータスを更新
    /// </summary>
    public async Task<string> UpdateReadingListStatusAsync(
        string readingListId,
        string status,
        DateTime? completedDate = null)
    {
        McpLogger?.Info($"{ServiceLogPrefix} UpdateReadingListStatusAsync 開始: readingListId={readingListId}, status={status}");

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
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateReadingListStatusAsync: 完了日を設定: {completedDate.Value:yyyy-MM-dd}");
        }

        McpLogger?.Info($"{ServiceLogPrefix} UpdateReadingListStatusAsync: Notion API 呼び出し中...");
        var request = new PagesUpdateParameters { Properties = properties };
        var page = await Client.Pages.UpdateAsync(readingListId, request);

        var result = $"Reading List status updated successfully (ID: {page.Id})";
        McpLogger?.Info($"{ServiceLogPrefix} UpdateReadingListStatusAsync 完了: pageId={page.Id}");
        return result;
    }
}
