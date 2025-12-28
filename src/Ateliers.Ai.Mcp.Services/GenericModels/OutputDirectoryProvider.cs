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
    public virtual string CreateWorkDirectory(string workDirectory)
    {
        var rootDir = string.IsNullOrWhiteSpace(OutputRootDirectory)
            ? Path.GetTempPath()
            : OutputRootDirectory;

        var fullPath = Path.Combine(rootDir, workDirectory);
        Directory.CreateDirectory(fullPath);

        return fullPath;
    }
}
