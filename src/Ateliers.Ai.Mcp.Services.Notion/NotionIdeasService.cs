using Notion.Client;
using Ateliers.Ai.Mcp.Services;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion Ideas データベースの操作を担当するサービス
/// </summary>
public class NotionIdeasService : NotionServiceBase, INotionIdeasService
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notionSettings"> Notion設定 </param>
    public NotionIdeasService(INotionSettings notionSettings) : base(notionSettings)
    {
    }

    /// <summary>
    /// Ideas Database IDを取得
    /// </summary>
    private string GetIdeasDatabaseId() => GetDatabaseId("Ideas");

    /// <summary>
    /// アイデアを追加
    /// </summary>
    public async Task<string> AddIdeaAsync(
        string title,
        string? content = null,
        string[]? tags = null,
        string? link = null,
        string? registrant = null)
    {
        var databaseId = GetIdeasDatabaseId();

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

        // Tags
        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
        }

        // Status (デフォルト: アイデア)
        properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = "アイデア" } };

        // Link
        if (!string.IsNullOrWhiteSpace(link))
        {
            properties["Link"] = new UrlPropertyValue { Url = link };
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

        // Content をページブロックとして追加
        if (!string.IsNullOrWhiteSpace(content))
        {
            request.Children = new List<IBlock>
            {
                new ParagraphBlock
                {
                    Paragraph = new ParagraphBlock.Info
                    {
                        RichText = new List<RichTextBase>
                        {
                            new RichTextText { Text = new Text { Content = content } }
                        }
                    }
                }
            };
        }

        var page = await Client.Pages.CreateAsync(request);
        return $"Idea created successfully: {title} (ID: {page.Id})";
    }

    /// <summary>
    /// アイデアを検索
    /// </summary>
    public async Task<string> SearchIdeasAsync(
        string? keyword = null,
        string[]? tags = null,
        int limit = 10)
    {
        var databaseId = GetIdeasDatabaseId();
        var filters = new List<Filter>();

        // キーワード検索（タイトル）
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(new TitleFilter("Name", contains: keyword));
        }

        // タグフィルタ
        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                filters.Add(new MultiSelectFilter("Tags", contains: tag));
            }
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
            return "No ideas found.";
        }

        var ideas = response.Results.Select(page =>
        {
            var props = (page as Page)?.Properties;
            var ideaTitle = props != null && props.ContainsKey("Name") && props["Name"] is TitlePropertyValue titleProp
                ? string.Join("", titleProp.Title.Select(t => t.PlainText))
                : "Untitled";

            var tagList = props != null && props.ContainsKey("Tags") && props["Tags"] is MultiSelectPropertyValue tagProp
                ? string.Join(", ", tagProp.MultiSelect.Select(t => t.Name))
                : "未設定";

            return $"- {ideaTitle} (タグ: {tagList}, ID: {page.Id})";
        });

        return $"Ideas ({response.Results.Count}):\n" + string.Join("\n", ideas);
    }

    /// <summary>
    /// アイデアを更新
    /// </summary>
    public async Task<string> UpdateIdeaAsync(
        string ideaId,
        string? title = null,
        string? content = null,
        string[]? tags = null,
        string? status = null,
        string? link = null)
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

        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } };
        }

        if (!string.IsNullOrWhiteSpace(link))
        {
            properties["Link"] = new UrlPropertyValue { Url = link };
        }

        var request = new PagesUpdateParameters { Properties = properties };
        var page = await Client.Pages.UpdateAsync(ideaId, request);

        // Content 更新時はページブロックを追加
        if (!string.IsNullOrWhiteSpace(content))
        {
            await Client.Blocks.AppendChildrenAsync(new BlockAppendChildrenRequest
            {
                BlockId = ideaId,
                Children = new List<IBlockObjectRequest>
                {
                    new ParagraphBlockRequest
                    {
                        Paragraph = new ParagraphBlockRequest.Info
                        {
                            RichText = new List<RichTextBase>
                            {
                                new RichTextText { Text = new Text { Content = content } }
                            }
                        }
                    }
                }
            });
        }

        return $"Idea updated successfully (ID: {page.Id})";
    }
}
