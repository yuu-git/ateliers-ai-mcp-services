using System;
using System.Collections.Generic;
using System.Text;

namespace Ateliers.Ai.Mcp.Services.GenericModels;

public sealed record PresentationVideoResult
{
    public required string VideoPath { get; init; }

    public IReadOnlyList<string> SlideImages { get; init; } = [];
    public IReadOnlyList<string> VoiceFiles { get; init; } = [];
}

