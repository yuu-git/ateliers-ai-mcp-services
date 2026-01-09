using Ateliers.Ai.Mcp.Services.VoicePeak;
using Ateliers.Voice.Engines.VoicePeakTools;
using Ateliers.Voice.Engines.VoicePeakTools.Narrators;
using Moq;
using Xunit;

namespace Ateliers.Ai.Mcp.Services.VoicePeak.UnitTests;

/// <summary>
/// VoicePeakNarratorInfoFormatter の単体テスト
/// </summary>
public sealed class VoicePeakNarratorInfoFormatterTests
{
    [Fact(DisplayName = "全ナレーターのMarkdown生成が正常に動作すること")]
    public void ToFullInfoMarkdownAllNarrators_ReturnsMarkdownWithAllNarrators()
    {
        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdownAllNarrators();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("# VoicePeak 使い方の基本情報", result);
        Assert.Contains("# ナレーター情報リスト", result);
        Assert.Contains("Frimomen", result);
        Assert.Contains("夏色花梨", result);
        Assert.Contains("ポロンちゃん", result);
    }

    [Fact(DisplayName = "全ナレーター生成時にロガーが情報を記録すること")]
    public void ToFullInfoMarkdownAllNarrators_WithLogger_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdownAllNarrators(mockLogger.Object);

        // Assert
        Assert.NotNull(result);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => msg.Contains("ToFullInfoMarkdown 全ナレーター情報 開始"))),
            Times.Once);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => msg.Contains("ナレーター数:"))),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "指定されたナレーター名リストからMarkdownを生成すること")]
    public void ToFullInfoMarkdown_WithNarratorNames_ReturnsMarkdownForSpecifiedNarrators()
    {
        // Arrange
        var narratorNames = new[] { "Frimomen", "夏色花梨" };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narratorNames);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Frimomen", result);
        Assert.Contains("夏色花梨", result);
        Assert.DoesNotContain("ポロンちゃん", result);
    }

    [Fact(DisplayName = "単一のナレーター名からMarkdownを生成すること")]
    public void ToFullInfoMarkdown_WithSingleNarratorName_ReturnsMarkdownForSingleNarrator()
    {
        // Arrange
        var narratorName = "Frimomen";

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narratorName);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"**VoicePeak システム名**: `Frimomen`", result);
        Assert.DoesNotContain(@"**VoicePeak システム名**: `夏色花梨`", result);
        Assert.DoesNotContain(@"**VoicePeak システム名**: `ポロンちゃん`", result);
    }

    [Fact(DisplayName = "ナレーターインスタンスからMarkdownを生成すること")]
    public void ToFullInfoMarkdown_WithNarratorInstance_ReturnsMarkdownForNarrator()
    {
        // Arrange
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrator);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Frimomen", result);
    }

    [Fact(DisplayName = "基本的な使い方情報を含むこと")]
    public void ToFullInfoMarkdown_ContainsBasicUsageInfo()
    {
        // Arrange
        var narrators = new[] { VoicePeakNarraterFactory.CreateNarratorByName("Frimomen") };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("# VoicePeak 使い方の基本情報", result);
        Assert.Contains("## 概要", result);
        Assert.Contains("## 基本パラメーター", result);
        Assert.Contains("speed", result);
        Assert.Contains("pitch", result);
        Assert.Contains("50 - 200", result);
        Assert.Contains("-300 - 300", result);
    }

    [Fact(DisplayName = "パラメーター指定方法を含むこと")]
    public void ToFullInfoMarkdown_ContainsParameterSpecificationMethod()
    {
        // Arrange
        var narrators = new[] { VoicePeakNarraterFactory.CreateNarratorByName("Frimomen") };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("## パラメーター指定方法", result);
        Assert.Contains("ナレーター名", result);
        Assert.Contains("感情パラメーター", result);
    }

    [Fact(DisplayName = "期待するパラメーターの説明を含むこと")]
    public void ToFullInfoMarkdown_ContainsExpectedParameters()
    {
        // Arrange
        var narrators = new[] { VoicePeakNarraterFactory.CreateNarratorByName("Frimomen") };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("## 期待するパラメータ", result);
        Assert.Contains($"{nameof(IGenerateVoiceRequest.Text)}", result);
        Assert.Contains($"{nameof(IGenerateVoiceRequest.OutputWavFileName)}", result);
        Assert.Contains($"{nameof(IGenerateVoiceRequest.Options)}", result);
        Assert.Contains($"{nameof(VoicePeakMcpGenerationOptions)}", result);
    }

    [Fact(DisplayName = "生成オプションの例を含むこと")]
    public void ToFullInfoMarkdown_ContainsGenerationOptionsExample()
    {
        // Arrange
        var narrators = new[] { VoicePeakNarraterFactory.CreateNarratorByName("Frimomen") };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("## 生成オプションの例", result);
        Assert.Contains("-n Frimomen", result);
        Assert.Contains("-e", result);
        Assert.Contains("--speed", result);
        Assert.Contains("--pitch", result);
    }

    [Fact(DisplayName = "ナレーター情報リストを含むこと")]
    public void ToFullInfoMarkdown_ContainsNarratorInfoList()
    {
        // Arrange
        var narrators = new[] { VoicePeakNarraterFactory.CreateNarratorByName("Frimomen") };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("# ナレーター情報リスト", result);
        Assert.Contains("---", result);
    }

    [Fact(DisplayName = "ナレーター詳細情報を含むこと")]
    public void ToFullInfoMarkdown_ContainsNarratorDetails()
    {
        // Arrange
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");
        var narrators = new[] { narrator };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        // ナレーターの基本情報が含まれているか
        Assert.Contains(narrator.VoicePeakSystemName, result);
        
        // 使用例が含まれているか
        Assert.Contains("## 使用例", result);
        Assert.Contains("### VoicePeak CLI コマンド形式", result);
        Assert.Contains("voicepeak.exe", result);
    }

    [Fact(DisplayName = "複数ナレーターが区切り線で分離されること")]
    public void ToFullInfoMarkdown_WithMultipleNarrators_SeparatesWithHorizontalRule()
    {
        // Arrange
        var narrators = VoicePeakNarraterFactory.CreateAllNarrators();

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        var lines = result.Split(Environment.NewLine);
        var separatorCount = lines.Count(line => line.Trim() == "---");
        
        // 基本情報とナレーターリストの区切り + (ナレーター数 - 1) の区切り
        Assert.True(separatorCount >= 3);
    }

    [Fact(DisplayName = "各ナレーターの使用例を含むこと")]
    public void ToFullInfoMarkdown_ContainsUsageExampleForEachNarrator()
    {
        // Arrange
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");
        var narrators = new[] { narrator };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        var expectedCommand = $"voicepeak.exe -s \"こんにちは\" -n \"{narrator.VoicePeakSystemName}\" -e \"{narrator.GetEmotionString()}\"";
        Assert.Contains(expectedCommand, result);
    }

    [Fact(DisplayName = "空のナレーターリストで基本情報のみを返すこと")]
    public void ToFullInfoMarkdown_EmptyNarratorsList_ReturnsOnlyBasicInfo()
    {
        // Arrange
        var narrators = Enumerable.Empty<IVoicePeakNarrator>();

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# VoicePeak 使い方の基本情報", result);
        Assert.Contains("# ナレーター情報リスト", result);
        // ナレーター情報が含まれていないことを確認
        Assert.DoesNotContain("voicepeak.exe -s", result);
    }

    [Fact(DisplayName = "ロガーが開始と完了を記録すること")]
    public void ToFullInfoMarkdown_WithLogger_LogsStartAndCompletion()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var narrators = new[] { VoicePeakNarraterFactory.CreateNarratorByName("Frimomen") };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators, mockLogger.Object);

        // Assert
        Assert.NotNull(result);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => msg.Contains("ToFullInfoMarkdown 開始"))),
            Times.Once);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => msg.Contains("ToFullInfoMarkdown 完了"))),
            Times.Once);
    }

    [Fact(DisplayName = "ナレーターの感情パラメーターを含むこと")]
    public void ToFullInfoMarkdown_IncludesNarratorEmotionParameters()
    {
        // Arrange
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName<NatukiKarin>("夏色花梨");
        narrator.HighTension = 50;
        narrator.Buchigire = 20;
        var narrators = new[] { narrator };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("夏色花梨", result);
        var emotionString = narrator.GetEmotionString();
        Assert.Contains(emotionString, result);
    }

    [Fact(DisplayName = "使用例にspeedとpitchパラメーターを含むこと")]
    public void ToFullInfoMarkdown_ContainsSpeedAndPitchInUsageExample()
    {
        // Arrange
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");
        var narrators = new[] { narrator };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrators);

        // Assert
        Assert.Contains("--speed 100", result);
        Assert.Contains("--pitch 0", result);
    }

    [Fact(DisplayName = "ナレーター名リスト指定時にロガーがナレーター名を記録すること")]
    public void ToFullInfoMarkdown_NarratorNamesOverload_WithLogger_LogsNarratorNames()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var narratorNames = new[] { "Frimomen", "夏色花梨" };

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narratorNames, mockLogger.Object);

        // Assert
        Assert.NotNull(result);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => 
                msg.Contains("ToFullInfoMarkdown 指定ナレーターリスト 開始") && 
                msg.Contains("Frimomen") && 
                msg.Contains("夏色花梨"))),
            Times.Once);
    }

    [Fact(DisplayName = "単一ナレーター名指定時にロガーがナレーター名を記録すること")]
    public void ToFullInfoMarkdown_SingleNameOverload_WithLogger_LogsNarratorName()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var narratorName = "Frimomen";

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narratorName, mockLogger.Object);

        // Assert
        Assert.NotNull(result);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => 
                msg.Contains("ToFullInfoMarkdown 指定ナレーター 開始") && 
                msg.Contains("Frimomen"))),
            Times.Once);
    }

    [Fact(DisplayName = "ナレーターインスタンス指定時にロガーがシステム名を記録すること")]
    public void ToFullInfoMarkdown_InstanceOverload_WithLogger_LogsNarratorSystemName()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");

        // Act
        var result = VoicePeakNarratorInfoFormatter.ToFullInfoMarkdown(narrator, mockLogger.Object);

        // Assert
        Assert.NotNull(result);
        mockLogger.Verify(
            logger => logger.Info(It.Is<string>(msg => 
                msg.Contains("ToFullInfoMarkdown 指定ナレーター 開始") && 
                msg.Contains(narrator.VoicePeakSystemName))),
            Times.Once);
    }
}
