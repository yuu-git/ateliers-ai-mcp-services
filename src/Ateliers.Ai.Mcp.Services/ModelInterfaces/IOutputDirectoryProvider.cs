namespace Ateliers.Ai.Mcp.Services;

public interface IOutputDirectoryProvider
{
    string? OutputRootDirectory { get; }

    virtual string CreateWorkDirectory(string appName, string subDirectory = "")
    {
        if (string.IsNullOrWhiteSpace(appName))
        {
            throw new ArgumentException("App name must be provided.", nameof(appName));
        }
        var rootDir = string.IsNullOrWhiteSpace(OutputRootDirectory)
            ? Path.GetTempPath()
            : OutputRootDirectory;
        var workDirectory = string.IsNullOrWhiteSpace(subDirectory)
            ? Path.Combine(rootDir, appName)
            : Path.Combine(rootDir, appName, subDirectory);
        Directory.CreateDirectory(workDirectory);
        return workDirectory;
    }
}
