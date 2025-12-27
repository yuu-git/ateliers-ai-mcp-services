namespace Ateliers.Ai.Mcp.Services.Ffmpeg.UnitTests;

public class FfmpegServiceInfoTests
{
    [Fact]
    public void Version_ShouldNotBeNull()
    {
        // Arrange & Act
        var version = new FfmpegServiceInfo().AssemblyVersion;

        // Assert
        Assert.NotNull(version);
    }

    [Fact]
    public void Name_ShouldMatchExpected()
    {
        // Arrange
        var expectedName = "Ateliers.Ai.Mcp.Services.Ffmpeg";

        // Act
        var actualName = new FfmpegServiceInfo().AssemblyName;

        // Assert
        Assert.Equal(expectedName, actualName);
    }

    [Fact]
    public void Description_ShouldNotBeNull()
    {
        // Arrange & Act
        var description = new FfmpegServiceInfo().Description;

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
        var actualCompany = new FfmpegServiceInfo().Company;

        // Assert
        Assert.Equal(expectedCompany, actualCompany);
    }

    [Fact]
    public void Product_ShouldMatchExpected()
    {
        // Arrange
        var expectedProduct = "Ateliers AI MCP";

        // Act
        var actualProduct = new FfmpegServiceInfo().Product;

        // Assert
        Assert.Equal(expectedProduct, actualProduct);
    }

    [Fact]
    public void RepositoryUrl_ShouldMatchExpected()
    {
        // Arrange
        var expectedUrl = new Uri("https://github.com/yuu-git/ateliers-ai-mcp-services");

        // Act
        var repositoryUrl = new FfmpegServiceInfo().RepositoryUrl;

        // Assert
        Assert.Equal(expectedUrl, repositoryUrl);
    }
}