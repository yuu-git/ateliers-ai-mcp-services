namespace Ateliers.Ai.Mcp.Services;

public interface IMarpServiceOptions : IOutputDirectoryProvider
{
    string MarpExecutablePath { get; }

    string MarpOutputDirectoryName { get;}
}
