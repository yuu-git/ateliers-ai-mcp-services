namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// MCPサービスの基底クラス
/// </summary>
public abstract class McpServiceBase
{
    /// <summary>
    /// ログ接頭辞
    /// </summary>
    protected virtual string LogPrefix { get; init; } = $"{nameof(McpServiceBase)}: ";

    /// <summary>
    /// MCPロガー
    /// </summary>
    protected IMcpLogger? McpLogger { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public McpServiceBase()
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="mcpLogger"> MCPロガー </param>
    public McpServiceBase(IMcpLogger mcpLogger)
        : this()
    {
        McpLogger = mcpLogger;
    }
}
