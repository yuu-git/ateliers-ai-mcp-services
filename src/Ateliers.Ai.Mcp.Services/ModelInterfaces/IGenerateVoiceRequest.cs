using System;
using System.Collections.Generic;
using System.Text;

namespace Ateliers.Ai.Mcp.Services;

public interface IGenerateVoiceRequest
{
    string Text { get; }
    string OutputWavFileName { get; }
}