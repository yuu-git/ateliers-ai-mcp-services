namespace Ateliers.Ai.Mcp.Services;

public interface INotionSettings
{
    string ApiToken { get; }
    IDictionary<string, string> Databases { get; }
}
