using LibGit2Sharp;

namespace Ateliers.Ai.Mcp.Services.Git;

public static class GitCredentialsHelper
{
    public static Credentials GetCredentials(string remoteUrl, string token)
    {
        if (string.IsNullOrEmpty(remoteUrl))
        {
            // フォールバック: GitHub形式
            return new UsernamePasswordCredentials
            {
                Username = token,
                Password = string.Empty
            };
        }

        var url = remoteUrl.ToLowerInvariant();

        return url switch
        {
            _ when url.Contains("github.com") => new UsernamePasswordCredentials
            {
                Username = token,
                Password = string.Empty
            },
            _ when url.Contains("gitlab.com") => new UsernamePasswordCredentials
            {
                Username = "oauth2",
                Password = token
            },
            _ when url.Contains("dev.azure.com") || url.Contains("visualstudio.com") => new UsernamePasswordCredentials
            {
                Username = string.Empty,
                Password = token
            },
            _ when url.Contains("bitbucket.org") => new UsernamePasswordCredentials
            {
                Username = "x-token-auth",
                Password = token
            },
            _ => new UsernamePasswordCredentials
            {
                Username = "git",
                Password = token
            }
        };
    }
}
