using FluentAssertions;
using Hm.Logging.Enums;
using Hm.Logging.Models;
using Xunit;

namespace Hm.Logging.Tests.Models;

public sealed class LogEntryTests
{
    [Fact]
    public void Create_ShouldInitializeLogEntry()
    {
        // Arrange
        string message = "Test log";

        // Act
        var entry = LogEntry.Create(message);

        // Assert
        entry.Message.Should().Be(message);
        entry.Level.Should().Be(LogLevel.Information);
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entry.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_ShouldTrimMessage()
    {
        // Arrange
        string message = "   Test log   ";

        // Act
        var entry = LogEntry.Create(message);

        // Assert
        entry.Message.Should().Be("Test log");
    }

    [Fact]
    public void Create_ShouldAssignUtcTimestamp()
    {
        // Arrange
        string message = "Test log";

        // Act
        var entry = LogEntry.Create(message);

        // Assert
        entry.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_ShouldThrow_WhenMessageIsNull()
    {
        // Arrange
        string? message = null;

        // Act
        Action action = () => LogEntry.Create(message!);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenMessageIsWhitespace()
    {
        // Arrange
        string message = "   ";

        // Act
        Action action = () => LogEntry.Create(message);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Info_ShouldAssignInformationLevel()
    {
        // Arrange
        string message = "Information message";

        // Act
        var entry = LogEntry.Info(message);

        // Assert
        entry.Level.Should().Be(LogLevel.Information);
        entry.Message.Should().Be(message);
    }

    [Fact]
    public void Warning_ShouldAssignWarningLevel()
    {
        // Arrange
        string message = "Warning message";

        // Act
        var entry = LogEntry.Warning(message);

        // Assert
        entry.Level.Should().Be(LogLevel.Warning);
        entry.Message.Should().Be(message);
    }

    [Fact]
    public void Error_ShouldAssignErrorLevel()
    {
        // Arrange
        string message = "Error message";
        Exception exception = new InvalidOperationException("Invalid operation");

        // Act
        var entry = LogEntry.Error(message, exception);

        // Assert
        entry.Level.Should().Be(LogLevel.Error);
        entry.Message.Should().Be(message);
    }

    [Fact]
    public void Error_ShouldSerializeException()
    {
        // Arrange
        string message = "Error message";
        Exception exception = new InvalidOperationException("Invalid operation");

        // Act
        var entry = LogEntry.Error(message, exception);

        // Assert
        entry.Exception.Should().NotBeNullOrWhiteSpace();
        entry.Exception.Should().Contain(nameof(InvalidOperationException));
        entry.Exception.Should().Contain("Invalid operation");
    }

    [Fact]
    public void Error_ShouldThrow_WhenExceptionIsNull()
    {
        // Arrange
        string message = "Error message";

        // Act
        Action action = () => LogEntry.Error(message, null!);

        // Assert
        action.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void HelperMethods_ShouldReturnImmutableInstances()
    {
        // Arrange
        var originalEntry = LogEntry.Info("Original message");

        // Act
        LogEntry modifiedEntry = originalEntry with
        {
            Message = "Modified message"
        };

        // Assert
        originalEntry.Message.Should().Be("Original message");
        modifiedEntry.Message.Should().Be("Modified message");

        originalEntry.Should().NotBeSameAs(modifiedEntry);
    }
}
