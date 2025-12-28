namespace Ateliers.Ai.Mcp.Services;

public interface IOutputDirectoryProvider
{
    string? OutputRootDirectory { get; }

    string CreateWorkDirectory(string appName, string subDirectory = "");
}
