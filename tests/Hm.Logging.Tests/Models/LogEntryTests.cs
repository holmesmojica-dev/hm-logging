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
        entry.Timestamp.Should().NotBe(default);
    }
}
