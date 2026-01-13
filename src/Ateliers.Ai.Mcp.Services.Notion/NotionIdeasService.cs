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
            Parent = new DatabaseParentRequest { DatabaseId = databaseId },
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

        McpLogger?.Info($"{ServiceLogPrefix} SearchIdeasAsync: Notion API 呼び出し中...");
        // Search APIを使用（SearchRequestを使用）
        var searchRequest = new SearchRequest
        {
            Query = keyword,
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

        // タグフィルタを手動で適用
        if (tags != null && tags.Length > 0)
        {
            databasePages = databasePages.Where(page =>
            {
                if (page.Properties.TryGetValue("Tags", out var tagsValue))
                {
                    if (tagsValue is MultiSelectPropertyValue multiSelectProp)
                    {
                        var pageTags = multiSelectProp.MultiSelect.Select(t => t.Name).ToList();
                        return tags.Any(tag => pageTags.Contains(tag));
                    }
                }
                return false;
            }).ToList();
        }

        McpLogger?.Info($"{ServiceLogPrefix} SearchIdeasAsync 完了: {databasePages.Count}件取得");

        if (databasePages.Count == 0)
        {
            return "No ideas found.";
        }

        var ideas = databasePages.Select(page =>
        {
            var props = page.Properties;
            
            string ideaTitle = "Untitled";
            if (props != null && props.TryGetValue("Name", out var nameValue))
            {
                if (nameValue is TitlePropertyValue titlePropertyValue)
                {
                    ideaTitle = string.Join("", titlePropertyValue.Title.Select(t => t.PlainText));
                }
            }

            string tagList = "未設定";
            if (props != null && props.TryGetValue("Tags", out var tagsValue))
            {
                if (tagsValue is MultiSelectPropertyValue tagPropertyValue)
                {
                    tagList = string.Join(", ", tagPropertyValue.MultiSelect.Select(t => t.Name));
                }
            }

            return $"- {ideaTitle} (タグ: {tagList}, ID: {page.Id})";
        });

        return $"Ideas ({databasePages.Count}):\n" + string.Join("\n", ideas);
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
