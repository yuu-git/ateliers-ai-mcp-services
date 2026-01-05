using Notion.Client;
using Ateliers.Ai.Mcp.Services;

namespace Ateliers.Ai.Mcp.Services.Notion;

/// <summary>
/// Notion Ideas データベースの操作を担当するサービス
/// </summary>
public class NotionIdeasService : NotionServiceBase, INotionIdeasService
{
    private const string ServiceLogPrefix = $"{nameof(NotionIdeasService)}:";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notionSettings"> Notion設定 </param>
    public NotionIdeasService(IMcpLogger mcpLogger, INotionSettings notionSettings) : base(mcpLogger, notionSettings)
    {
        McpLogger?.Info($"{ServiceLogPrefix} 初期化完了");
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
        McpLogger?.Info($"{ServiceLogPrefix} AddIdeaAsync 開始: title={title}, tags={tags?.Length ?? 0}件");

        var databaseId = GetIdeasDatabaseId();
        McpLogger?.Debug($"{ServiceLogPrefix} AddIdeaAsync: databaseId={databaseId}");

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
            McpLogger?.Debug($"{ServiceLogPrefix} AddIdeaAsync: タグを設定: {string.Join(", ", tags)}");
        }

        // Status (デフォルト: アイデア)
        properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = "アイデア" } };

        // Link
        if (!string.IsNullOrWhiteSpace(link))
        {
            properties["Link"] = new UrlPropertyValue { Url = link };
            McpLogger?.Debug($"{ServiceLogPrefix} AddIdeaAsync: リンクを設定");
        }

        // Registrant
        if (!string.IsNullOrWhiteSpace(registrant))
        {
            properties["Registrant"] = new SelectPropertyValue { Select = new SelectOption { Name = registrant } };
            McpLogger?.Debug($"{ServiceLogPrefix} AddIdeaAsync: 登録者を設定: {registrant}");
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
            McpLogger?.Debug($"{ServiceLogPrefix} AddIdeaAsync: コンテンツを設定: サイズ={content.Length}文字");
        }

        McpLogger?.Info($"{ServiceLogPrefix} AddIdeaAsync: Notion API 呼び出し中...");
        var page = await Client.Pages.CreateAsync(request);

        var result = $"Idea created successfully: {title} (ID: {page.Id})";
        McpLogger?.Info($"{ServiceLogPrefix} AddIdeaAsync 完了: pageId={page.Id}");
        return result;
    }

    /// <summary>
    /// アイデアを検索
    /// </summary>
    public async Task<string> SearchIdeasAsync(
        string? keyword = null,
        string[]? tags = null,
        int limit = 10)
    {
        McpLogger?.Info($"{ServiceLogPrefix} SearchIdeasAsync 開始: keyword={keyword}, tags={tags?.Length ?? 0}件, limit={limit}");

        var databaseId = GetIdeasDatabaseId();
        var filters = new List<Filter>();

        // キーワード検索（タイトル）
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(new TitleFilter("Name", contains: keyword));
            McpLogger?.Debug($"{ServiceLogPrefix} SearchIdeasAsync: キーワードフィルタを追加: {keyword}");
        }

        // タグフィルタ
        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                filters.Add(new MultiSelectFilter("Tags", contains: tag));
            }
            McpLogger?.Debug($"{ServiceLogPrefix} SearchIdeasAsync: タグフィルタを追加: {string.Join(", ", tags)}");
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
            McpLogger?.Debug($"{ServiceLogPrefix} SearchIdeasAsync: フィルタ数={filters.Count}");
        }

        McpLogger?.Info($"{ServiceLogPrefix} SearchIdeasAsync: Notion API 呼び出し中...");
        var response = await Client.Databases.QueryAsync(databaseId, queryParams);

        McpLogger?.Info($"{ServiceLogPrefix} SearchIdeasAsync 完了: {response.Results.Count}件取得");

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
        McpLogger?.Info($"{ServiceLogPrefix} UpdateIdeaAsync 開始: ideaId={ideaId}");

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
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateIdeaAsync: タイトルを更新: {title}");
        }

        if (tags != null && tags.Length > 0)
        {
            properties["Tags"] = new MultiSelectPropertyValue
            {
                MultiSelect = tags.Select(tag => new SelectOption { Name = tag }).ToList()
            };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateIdeaAsync: タグを更新: {string.Join(", ", tags)}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            properties["Status"] = new SelectPropertyValue { Select = new SelectOption { Name = status } };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateIdeaAsync: ステータスを更新: {status}");
        }

        if (!string.IsNullOrWhiteSpace(link))
        {
            properties["Link"] = new UrlPropertyValue { Url = link };
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateIdeaAsync: リンクを更新");
        }

        McpLogger?.Info($"{ServiceLogPrefix} UpdateIdeaAsync: Notion API 呼び出し中...");
        var request = new PagesUpdateParameters { Properties = properties };
        var page = await Client.Pages.UpdateAsync(ideaId, request);

        // Content 更新時はページブロックを追加
        if (!string.IsNullOrWhiteSpace(content))
        {
            McpLogger?.Debug($"{ServiceLogPrefix} UpdateIdeaAsync: コンテンツを追加: サイズ={content.Length}文字");
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

        var result = $"Idea updated successfully (ID: {page.Id})";
        McpLogger?.Info($"{ServiceLogPrefix} UpdateIdeaAsync 完了: pageId={page.Id}");
        return result;
    }
}
