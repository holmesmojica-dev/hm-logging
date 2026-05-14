using System.Collections.Immutable;
using FluentAssertions;
using Hm.Logging.Abstractions;
using Hm.Logging.Enums;
using Hm.Logging.Extensions;
using Hm.Logging.Models;
using Moq;
using Xunit;

namespace Hm.Logging.Tests.Extensions;

public sealed class LogEntryValidationExtensionsTests
{
    [Fact]
    public void EnsureValid_ShouldSucceed_ForValidLogEntry()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Level = LogLevel.Information
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Test message");
        result.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenLogEntryIsNull()
    {
        // Arrange
        LogEntry? logEntry = null;

        Mock<ITraceContext> traceContextMock = new();

        // Act
        Action action = () => logEntry!.EnsureValid(traceContextMock.Object);

        // Assert
        action.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenTraceContextIsNull()
    {
        // Arrange
        var logEntry = LogEntry.Create("Test message");

        // Act
        Action action = () => logEntry.EnsureValid(null!);

        // Assert
        action.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenMessageIsNull()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = null!
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        Action action = () => logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenMessageIsWhitespace()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        Action action = () => logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureValid_ShouldTrimMessage()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "   Test message   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Message.Should().Be("Test message");
    }

    [Fact]
    public void EnsureValid_ShouldAssignUtcTimestamp_WhenTimestampIsDefault()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Timestamp = default
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void EnsureValid_ShouldPreserveTimestamp_WhenTimestampIsAlreadyDefined()
    {
        // Arrange
        DateTime timestamp = new(2025, 01, 01, 10, 00, 00, DateTimeKind.Utc);

        LogEntry logEntry = new()
        {
            Message = "Test message",
            Timestamp = timestamp
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void EnsureValid_ShouldTrimSource()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Source = "   MySource   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Source.Should().Be("MySource");
    }

    [Fact]
    public void EnsureValid_ShouldConvertEmptySourceToNull()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Source = "   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Source.Should().BeNull();
    }

    [Fact]
    public void EnsureValid_ShouldPreserveExistingTraceId()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            TraceId = "existing-trace-id"
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.TraceId.Should().Be("existing-trace-id");
    }

    [Fact]
    public void EnsureValid_ShouldUseTraceContextTraceId_WhenLogEntryTraceIdIsNull()
    {
        // Arrange
        Mock<ITraceContext> traceContextMock = new();

        traceContextMock
            .Setup(x => x.GetTraceId())
            .Returns("context-trace-id");

        LogEntry logEntry = new()
        {
            Message = "Test message"
        };

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.TraceId.Should().Be("context-trace-id");
    }

    [Fact]
    public void EnsureValid_ShouldGenerateTraceId_WhenNoTraceIdExists()
    {
        // Arrange
        Mock<ITraceContext> traceContextMock = new();

        traceContextMock
            .Setup(x => x.GetTraceId())
            .Returns((string?)null);

        LogEntry logEntry = new()
        {
            Message = "Test message"
        };

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.TraceId.Should().NotBeNullOrWhiteSpace();

        bool isValidGuid = Guid.TryParse(result.TraceId, out _);

        isValidGuid.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_ShouldTrimCorrelationId()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            CorrelationId = "   correlation-id   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.CorrelationId.Should().Be("correlation-id");
    }

    [Fact]
    public void EnsureValid_ShouldConvertWhitespaceCorrelationIdToNull()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            CorrelationId = "   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenLogLevelIsInvalid()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Level = (LogLevel)999
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        Action action = () => logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        action.Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EnsureValid_ShouldTrimException()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Exception = "   Exception message   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Exception.Should().Be("Exception message");
    }

    [Fact]
    public void EnsureValid_ShouldConvertWhitespaceExceptionToNull()
    {
        // Arrange
        LogEntry logEntry = new()
        {
            Message = "Test message",
            Exception = "   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void EnsureValid_ShouldNormalizeMetadata()
    {
        // Arrange
        ImmutableDictionary<string, object> metadata =
            ImmutableDictionary<string, object>.Empty
                .Add("  UserId  ", "  12345  ")
                .Add("Environment", "  Production  ")
                .Add("LogLevel", LogLevel.Warning);

        LogEntry logEntry = new()
        {
            Message = "Test message",
            Metadata = metadata
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Metadata.Should().NotBeNull();

        result.Metadata["UserId"].Should().Be("12345");
        result.Metadata["Environment"].Should().Be("Production");
        result.Metadata["LogLevel"].Should().Be("Warning");
    }

    [Fact]
    public void EnsureValid_ShouldRemoveInvalidMetadataEntries()
    {
        // Arrange
        ImmutableDictionary<string, object> metadata =
            ImmutableDictionary<string, object>.Empty
                .Add("   ", "InvalidKey")
                .Add("ValidKey", "ValidValue");

        LogEntry logEntry = new()
        {
            Message = "Test message",
            Metadata = metadata
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry result = logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().ContainKey("ValidKey");
        result.Metadata.Should().HaveCount(1);
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenMetadataContainsReservedKey()
    {
        // Arrange
        ImmutableDictionary<string, object> metadata =
            ImmutableDictionary<string, object>.Empty
                .Add("Message", "Reserved");

        LogEntry logEntry = new()
        {
            Message = "Test message",
            Metadata = metadata
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        Action action = () => logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureValid_ShouldThrow_WhenMetadataContainsUnsupportedType()
    {
        // Arrange
        ImmutableDictionary<string, object> metadata =
            ImmutableDictionary<string, object>.Empty
                .Add("ComplexObject", new object());

        LogEntry logEntry = new()
        {
            Message = "Test message",
            Metadata = metadata
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        Action action = () => logEntry.EnsureValid(traceContextMock.Object);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureValid_ShouldReturnNewImmutableInstance()
    {
        // Arrange
        LogEntry originalEntry = new()
        {
            Message = "   Original message   "
        };

        Mock<ITraceContext> traceContextMock = new();

        // Act
        LogEntry normalizedEntry = originalEntry.EnsureValid(traceContextMock.Object);

        // Assert
        normalizedEntry.Should().NotBeSameAs(originalEntry);

        originalEntry.Message.Should().Be("   Original message   ");
        normalizedEntry.Message.Should().Be("Original message");
    }
}
