# Ateliers AI Model Context Protocol (MCP) Services

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

Service layer implementations for [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) in C#.

## Packages

```bash
# Base services and interfaces
dotnet add package Ateliers.Ai.Mcp.Services

# Notion API integration
dotnet add package Ateliers.Ai.Mcp.Services.Notion

# GitHub API integration
dotnet add package Ateliers.Ai.Mcp.Services.GitHub

# Local file system operations
dotnet add package Ateliers.Ai.Mcp.Services.LocalFs

# Git operations (LibGit2Sharp)
dotnet add package Ateliers.Ai.Mcp.Services.Git

# Voice synthesis (VOICEVOX)
dotnet add package Ateliers.Ai.Mcp.Services.Voicevox

# Marp presentation generation
dotnet add package Ateliers.Ai.Mcp.Services.Marp
```

## Features

- **Services** - Base interfaces and configuration models
- **Notion** - Tasks, Ideas, and Reading List management via Notion API
- **GitHub** - Repository file operations via GitHub API
- **LocalFs** - Local file system operations with directory exclusion
- **Git** - Git operations (pull, push, commit, tag) with multi-platform credential support
- **Voicevox** - Local voice synthesis using the VOICEVOX engine
- **Marp** - Presentation generation using Marp CLI
*(designed for MCP-based automation and content generation)*

## Voicevox Service Notes (Windows)

The Voicevox service uses VOICEVOX native libraries installed via the official
VOICEVOX installer.

No PATH modification is required

Native libraries are resolved at runtime using SetDllDirectory

Only local environments with VOICEVOX installed can execute voice synthesis

Typical installation path:
```
C:\Program Files\VOICEVOX\vv-engine
```

The OpenJTalk dictionary path is automatically detected under:
```
engine_internal\pyopenjtalk\open_jtalk_dic_utf_8-*
```

To reduce initialization time, loaded voice models (*.vvm) can be limited via
service options.
If not specified, all available models will be loaded.

## Marp Service Notes
The Marp service requires Marp CLI to be installed and accessible in the system PATH.
Install Marp CLI via npm:
```
npm install -g @marp-team/marp-cli
```

and ensure it is available in your PATH environment variable.
MarpServiceOptions allows specifying additional CLI arguments for customization:
```
var options = new MarpServiceOptions
{
	MarpExecutablePath = {your_marp_cli_path}
};
```

## Dependencies

All packages depend on:
- [Ateliers.Ai.Mcp.Core](https://www.nuget.org/packages/Ateliers.Ai.Mcp.Core/) - Core library

## Ateliers AI MCP Ecosystem

- **Core** - Base interfaces and utilities
- **Services** (this package) - Service layer implementations
- **Tools** - MCP tool implementations
- **Servers** - MCP server implementations

## Documentation

Visit **[ateliers.dev](https://ateliers.dev)** for full documentation, usage examples, and guides.

## Status

⚠️ **Development version (v0.x.x)** - API may change. Stable v1.0.0 coming soon.

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

**[ateliers.dev](https://ateliers.dev)** | **[GitHub](https://github.com/yuu-git/ateliers-ai-mcp-services)** | **[NuGet](https://www.nuget.org/profiles/ateliers)**
