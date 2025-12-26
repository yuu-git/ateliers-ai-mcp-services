using System.Runtime.InteropServices;

namespace Ateliers.Ai.Mcp.Services;

public static class NativeLibraryPath
{
    [DllImport("kernel32", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    public static void Use(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException(
                $"Native library directory not found: {path}");
        }

        if (!SetDllDirectory(path))
        {
            throw new InvalidOperationException(
                $"Failed to set DLL directory: {path}");
        }
    }
}

