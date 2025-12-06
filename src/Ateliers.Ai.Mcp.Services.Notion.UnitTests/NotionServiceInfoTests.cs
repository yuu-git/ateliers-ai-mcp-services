namespace Ateliers.Ai.Mcp.Services.Notion.UnitTests;

public class NotionServiceInfoTests
{
    [Fact]
    public void Version_ShouldNotBeNull()
    {
        // Arrange & Act
        var version = new NotionServiceInfo().AssemblyVersion;

        // Assert
        Assert.NotNull(version);
    }

    [Fact]
    public void Name_ShouldMatchExpected()
    {
        // Arrange
        var expectedName = "Ateliers.Ai.Mcp.Services.Notion";

        // Act
        var actualName = new NotionServiceInfo().AssemblyName;

        // Assert
        Assert.Equal(expectedName, actualName);
    }

    [Fact]
    public void Description_ShouldNotBeNull()
    {
        // Arrange & Act
        var description = new NotionServiceInfo().Description;

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
        var actualCompany = new NotionServiceInfo().Company;

        // Assert
        Assert.Equal(expectedCompany, actualCompany);
    }

    [Fact]
    public void Product_ShouldMatchExpected()
    {
        // Arrange
        var expectedProduct = "Ateliers AI MCP";

        // Act
        var actualProduct = new NotionServiceInfo().Product;

        // Assert
        Assert.Equal(expectedProduct, actualProduct);
    }

    [Fact]
    public void RepositoryUrl_ShouldMatchExpected()
    {
        // Arrange
        var expectedUrl = new Uri("https://github.com/yuu-git/ateliers-ai-mcp-services");

        // Act
        var repositoryUrl = new NotionServiceInfo().RepositoryUrl;

        // Assert
        Assert.Equal(expectedUrl, repositoryUrl);
    }
}