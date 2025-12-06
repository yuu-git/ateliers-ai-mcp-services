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
```

## Features

- **Services** - Base interfaces and configuration models
- **Notion** - Tasks, Ideas, and Reading List management via Notion API
- **GitHub** - Repository file operations via GitHub API
- **LocalFs** - Local file system operations with directory exclusion
- **Git** - Git operations (pull, push, commit, tag) with multi-platform credential support

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
