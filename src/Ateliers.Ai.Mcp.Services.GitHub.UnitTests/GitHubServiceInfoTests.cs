namespace Ateliers.Ai.Mcp.Services.GitHub.UnitTests;

public class GitHubServiceInfoTests
{
    [Fact]
    public void Version_ShouldNotBeNull()
    {
        // Arrange & Act
        var version = new GitHubServiceInfo().AssemblyVersion;

        // Assert
        Assert.NotNull(version);
    }

    [Fact]
    public void Name_ShouldMatchExpected()
    {
        // Arrange
        var expectedName = "Ateliers.Ai.Mcp.Services.GitHub";

        // Act
        var actualName = new GitHubServiceInfo().AssemblyName;

        // Assert
        Assert.Equal(expectedName, actualName);
    }

    [Fact]
    public void Description_ShouldNotBeNull()
    {
        // Arrange & Act
        var description = new GitHubServiceInfo().Description;

        // Assert
        Assert.NotNull(description);
        Assert.NotEmpty(description);
    }

    [Fact]
    public void Company_ShouldMatchExpected()
    {
        // Arrange
        var expectedCompany = "ateliers.dev";

        // Act
        var actualCompany = new GitHubServiceInfo().Company;

        // Assert
        Assert.Equal(expectedCompany, actualCompany);
    }

    [Fact]
    public void Product_ShouldMatchExpected()
    {
        // Arrange
        var expectedProduct = "Ateliers AI MCP";

        // Act
        var actualProduct = new GitHubServiceInfo().Product;

        // Assert
        Assert.Equal(expectedProduct, actualProduct);
    }

    [Fact]
    public void RepositoryUrl_ShouldMatchExpected()
    {
        // Arrange
        var expectedUrl = new Uri("https://github.com/yuu-git/ateliers-ai-mcp-services");

        // Act
        var repositoryUrl = new GitHubServiceInfo().RepositoryUrl;

        // Assert
        Assert.Equal(expectedUrl, repositoryUrl);
    }
}