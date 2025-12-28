using System;
using System.Collections.Generic;
using System.Text;

namespace Ateliers.Ai.Mcp.Services.GenericModels;

public class OutputDirectoryProvider : IOutputDirectoryProvider
{
    public string? OutputRootDirectory { get; init; }

    /// <summary>
    /// Root directory for all generated outputs.
    /// If null or empty, %TEMP% will be used.
    /// </summary>
    public virtual string CreateWorkDirectory(string appName, string subDirectory = "")
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
