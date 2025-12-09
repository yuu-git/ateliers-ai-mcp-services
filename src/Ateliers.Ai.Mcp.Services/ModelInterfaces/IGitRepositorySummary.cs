using System;
using System.Collections.Generic;
using System.Text;

namespace Ateliers.Ai.Mcp.Services;

/// <summary>
/// Gitリポジトリ情報インターフェース
/// </summary>
public interface IGitRepositorySummary
{
    /// <summary>
    /// リポジトリ名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// ブランチ名
    /// </summary>
    string Branch { get; }

    /// <summary>
    /// ローカルパス
    /// </summary>
    string LocalPath { get; }

    /// <summary>
    /// ローカルパスが設定されているかどうか
    /// </summary>
    bool HasLocalPath { get; }
}
