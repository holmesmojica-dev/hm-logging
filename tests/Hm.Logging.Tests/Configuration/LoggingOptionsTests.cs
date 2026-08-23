using FluentAssertions;
using Hm.Logging.Configuration;
using Hm.Logging.Enums;
using Xunit;

namespace Hm.Logging.Tests.Configuration;

public sealed class LoggingOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeMaxMessageLengthTo4000()
    {
        // Act
        LoggingOptions options = new();

        // Assert
        options.MaxMessageLength.Should().Be(4000);
    }

    [Fact]
    public void Constructor_ShouldInitializeMinimumLevelToInformation()
    {
        // Act
        LoggingOptions options = new();

        // Assert
        options.MinimumLevel.Should().Be(LogLevel.Information);
    }
}
