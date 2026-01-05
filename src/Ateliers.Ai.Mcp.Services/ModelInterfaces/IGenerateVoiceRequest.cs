using System;
using System.Collections.Generic;
using System.Text;

namespace Ateliers.Ai.Mcp.Services;

public interface IGenerateVoiceRequest
{
    string Text { get; }
    string OutputWavFileName { get; }
    
    /// <summary>
    /// 音声生成のオプション設定（TTS サービス固有のパラメータ）
    /// </summary>
    IVoiceGenerationOptions? Options { get; }
}