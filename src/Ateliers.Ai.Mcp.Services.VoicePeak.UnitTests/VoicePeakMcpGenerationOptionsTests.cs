using Moq;

namespace Ateliers.Ai.Mcp.Services.VoicePeak.UnitTests;

public class VoicePeakMcpGenerationOptionsTests
{
    [Fact(DisplayName = "FromParameterString: パラメーター文字列が空の場合はデフォルトインスタンスを返す")]
    public void FromParameterString_WithEmptyString_ReturnsDefaultInstance()
    {
        // Arrange & Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString("");

        // Assert
        Assert.Null(options.Narrator);
        Assert.Equal(100, options.Speed);
        Assert.Equal(0, options.Pitch);
        Assert.Null(options.Emotion);
    }

    [Fact(DisplayName = "FromParameterString: パラメーター文字列がnullの場合はデフォルトインスタンスを返す")]
    public void FromParameterString_WithNull_ReturnsDefaultInstance()
    {
        // Arrange & Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(null);

        // Assert
        Assert.Null(options.Narrator);
        Assert.Equal(100, options.Speed);
        Assert.Equal(0, options.Pitch);
        Assert.Null(options.Emotion);
    }

    [Fact(DisplayName = "FromParameterString: ナレーター名(-n)をパース")]
    public void FromParameterString_WithNarratorShortOption_ParsesNarrator()
    {
        // Arrange
        var paramsString = "-n 夏色花梨";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal("夏色花梨", options.Narrator);
        Assert.NotNull(options.NarratorInstance);
        Assert.Equal("夏色花梨", options.NarratorInstance.VoicePeakSystemName);
    }

    [Fact(DisplayName = "FromParameterString: ナレーター名(--narrator)をパース")]
    public void FromParameterString_WithNarratorLongOption_ParsesNarrator()
    {
        // Arrange
        var paramsString = "--narrator ポロンちゃん";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal("ポロンちゃん", options.Narrator);
        Assert.NotNull(options.NarratorInstance);
        Assert.Equal("ポロンちゃん", options.NarratorInstance.VoicePeakSystemName);
    }

    [Fact(DisplayName = "FromParameterString: 感情パラメーター(-e)をパース")]
    public void FromParameterString_WithEmotionShortOption_ParsesEmotion()
    {
        // Arrange
        var paramsString = "-n 夏色花梨 -e hightension=80,buchigire=20";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal("hightension=80,buchigire=20", options.Emotion);
        Assert.NotNull(options.NarratorInstance);
        Assert.Equal("夏色花梨", options.NarratorInstance.VoicePeakSystemName);
        
        // 感情パラメーターが設定されていることを確認
        var emotionString = options.NarratorInstance.GetEmotionString();
        Assert.Contains("hightension=80", emotionString);
        Assert.Contains("buchigire=20", emotionString);
    }

    [Fact(DisplayName = "FromParameterString: 感情パラメーター(--emotion)をパース")]
    public void FromParameterString_WithEmotionLongOption_ParsesEmotion()
    {
        // Arrange
        var paramsString = "-n Frimomen --emotion happy=100,sad=50";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal("happy=100,sad=50", options.Emotion);
        Assert.NotNull(options.NarratorInstance);
        
        // 感情パラメーターが設定されていることを確認
        var emotionString = options.NarratorInstance.GetEmotionString();
        Assert.Contains("happy=100", emotionString);
        Assert.Contains("sad=50", emotionString);
    }

    [Fact(DisplayName = "FromParameterString: 速度パラメーター(--speed)をパース")]
    public void FromParameterString_WithSpeed_ParsesSpeed()
    {
        // Arrange
        var paramsString = "--speed 150";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal(150, options.Speed);
        Assert.Null(options.NarratorInstance); // ナレーター名がないのでインスタンスは生成されない
    }

    [Fact(DisplayName = "FromParameterString: 音高パラメーター(--pitch)をパース")]
    public void FromParameterString_WithPitch_ParsesPitch()
    {
        // Arrange
        var paramsString = "--pitch -100";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal(-100, options.Pitch);
        Assert.Null(options.NarratorInstance); // ナレーター名がないのでインスタンスは生成されない
    }

    [Fact(DisplayName = "FromParameterString: 全パラメーターをパース")]
    public void FromParameterString_WithAllParameters_ParsesAll()
    {
        // Arrange
        var paramsString = "-n 夏色花梨 -e hightension=80,buchigire=20,nageki=0 --speed 120 --pitch 50";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal("夏色花梨", options.Narrator);
        Assert.Equal("hightension=80,buchigire=20,nageki=0", options.Emotion);
        Assert.Equal(120, options.Speed);
        Assert.Equal(50, options.Pitch);
        Assert.NotNull(options.NarratorInstance);
        Assert.Equal("夏色花梨", options.NarratorInstance.VoicePeakSystemName);
        
        // 感情パラメーターが設定されていることを確認
        var emotionString = options.NarratorInstance.GetEmotionString();
        Assert.Contains("hightension=80", emotionString);
        Assert.Contains("buchigire=20", emotionString);
        Assert.Contains("nageki=0", emotionString);
    }

    [Fact(DisplayName = "FromParameterString: 速度が最小値を下回る場合もそのまま保持（クランプは VoicePeakGenerateRequest で行う）")]
    public void FromParameterString_WithSpeedBelowMin_PreservesValue()
    {
        // Arrange
        var paramsString = "--speed 30";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal(30, options.Speed);
    }

    [Fact(DisplayName = "FromParameterString: 速度が最大値を上回る場合もそのまま保持（クランプは VoicePeakGenerateRequest で行う）")]
    public void FromParameterString_WithSpeedAboveMax_PreservesValue()
    {
        // Arrange
        var paramsString = "--speed 300";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal(300, options.Speed);
    }

    [Fact(DisplayName = "FromParameterString: 音高が最小値を下回る場合もそのまま保持（クランプは VoicePeakGenerateRequest で行う）")]
    public void FromParameterString_WithPitchBelowMin_PreservesValue()
    {
        // Arrange
        var paramsString = "--pitch -500";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal(-500, options.Pitch);
    }

    [Fact(DisplayName = "FromParameterString: 音高が最大値を上回る場合もそのまま保持（クランプは VoicePeakGenerateRequest で行う）")]
    public void FromParameterString_WithPitchAboveMax_PreservesValue()
    {
        // Arrange
        var paramsString = "--pitch 500";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal(500, options.Pitch);
    }

    [Fact(DisplayName = "FromParameterString: 不正なパラメーター文字列でもデフォルトインスタンスを返す")]
    public void FromParameterString_WithInvalidParametersString_ReturnsDefaultInstance()
    {
        // Arrange
        var paramsString = "invalid parameter string";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Null(options.Narrator);
        Assert.Equal(100, options.Speed);
        Assert.Equal(0, options.Pitch);
        Assert.Null(options.Emotion);
        Assert.Null(options.NarratorInstance);
    }

    [Fact(DisplayName = "Validate: 正常なパラメーター")]
    public void Validate_WithValidParameters_ReturnsEmpty()
    {
        // Arrange
        var paramsString = "-n 夏色花梨 --speed 100";

        // Act
        var errors = VoicePeakMcpGenerationOptions.Validate(paramsString);

        // Assert
        Assert.Empty(errors);
    }

    [Fact(DisplayName = "Validate: パラメーター文字列がnull")]
    public void Validate_WithNull_ReturnsEmpty()
    {
        // Arrange & Act
        var errors = VoicePeakMcpGenerationOptions.Validate(null);

        // Assert
        Assert.Empty(errors);
    }

    [Fact(DisplayName = "Validate: パラメーター文字列が空")]
    public void Validate_WithEmpty_ReturnsEmpty()
    {
        // Arrange & Act
        var errors = VoicePeakMcpGenerationOptions.Validate("");

        // Assert
        Assert.Empty(errors);
    }

    [Fact(DisplayName = "Validate: 不正なパラメーター文字列")]
    public void Validate_WithInvalidParametersString_ReturnsWarning()
    {
        // Arrange
        var paramsString = "invalid string";

        // Act
        var errors = VoicePeakMcpGenerationOptions.Validate(paramsString);

        // Assert
        Assert.Single(errors);
        Assert.Contains("サポートされているオプション", errors.First());
    }

    [Fact(DisplayName = "FromParameterString: ロガー付きでパース（全パラメーター）")]
    public void FromParameterString_WithLogger_LogsParsingSteps()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var paramsString = "-n 夏色花梨 -e hightension=80,buchigire=20 --speed 120 --pitch 50";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString, mockLogger.Object);

        // Assert
        Assert.Equal("夏色花梨", options.Narrator);
        Assert.Equal(120, options.Speed);
        Assert.Equal(50, options.Pitch);
        Assert.NotNull(options.NarratorInstance);

        // ログが呼ばれたことを確認
        mockLogger.Verify(l => l.Debug(It.Is<string>(s => s.Contains("パース開始"))), Times.Once);
        mockLogger.Verify(l => l.Debug(It.Is<string>(s => s.Contains("ナレーター名をパース"))), Times.Once);
        mockLogger.Verify(l => l.Debug(It.Is<string>(s => s.Contains("感情パラメーターをパース"))), Times.Once);
        mockLogger.Verify(l => l.Debug(It.Is<string>(s => s.Contains("速度をパース"))), Times.Once);
        mockLogger.Verify(l => l.Debug(It.Is<string>(s => s.Contains("ピッチをパース"))), Times.Once);
        mockLogger.Verify(l => l.Info(It.Is<string>(s => s.Contains("ナレーターインスタンスを生成"))), Times.Once);
        mockLogger.Verify(l => l.Info(It.Is<string>(s => s.Contains("パース完了"))), Times.Once);
    }

    [Fact(DisplayName = "FromParameterString: ロガー付きでパース（パラメーター文字列が空）")]
    public void FromParameterString_WithLogger_EmptyString_LogsDefault()
    {
        // Arrange
        var mockLogger = new Mock<IMcpLogger>();
        var paramsString = "";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString, mockLogger.Object);

        // Assert
        Assert.Null(options.Narrator);
        Assert.Equal(100, options.Speed);
        Assert.Equal(0, options.Pitch);

        // デフォルトインスタンス返却のログが呼ばれたことを確認
        mockLogger.Verify(l => l.Debug(It.Is<string>(s => s.Contains("デフォルトインスタンスを返却"))), Times.Once);
    }

    [Fact(DisplayName = "FromParameterString: ロガーなしでパース（後方互換性）")]
    public void FromParameterString_WithoutLogger_WorksNormally()
    {
        // Arrange
        var paramsString = "-n 夏色花梨 --speed 120";

        // Act
        var options = VoicePeakMcpGenerationOptions.FromParameterString(paramsString);

        // Assert
        Assert.Equal("夏色花梨", options.Narrator);
        Assert.Equal(120, options.Speed);
        Assert.NotNull(options.NarratorInstance);
    }
}
