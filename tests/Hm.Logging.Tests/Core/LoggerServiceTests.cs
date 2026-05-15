using System.Collections.Immutable;
using FluentAssertions;
using Hm.Logging.Abstractions;
using Hm.Logging.Configuration;
using Hm.Logging.Enums;
using Hm.Logging.Extensions;
using Hm.Logging.Models;
using Hm.Logging.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Hm.Logging.Tests.Core;

public sealed class LoggerServiceTests
{
    [Fact]
    public async Task LogAsync_ShouldExecuteSuccessfully_ForValidLogEntry()
    {
        // Arrange
        FakeLogProvider provider = new();
        ILoggerService logger = CreateLoggerService(providers: provider);
        var entry = LogEntry.Info("Test message");

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().HaveCount(1);
        provider.Entries[0].Message.Should().Be("Test message");
        provider.Entries[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task LogAsync_ShouldThrow_WhenLogEntryIsNull()
    {
        // Arrange
        FakeLogProvider provider = new();
        ILoggerService logger = CreateLoggerService(providers: provider);

        // Act
        Func<Task> action = async () =>
            await logger.LogAsync(null!);

        // Assert
        await action.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogAsync_ShouldIgnoreLogEntries_BelowMinimumLevel()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(
            options =>
            {
                options.MinimumLevel = LogLevel.Warning;
            },
            provider);

        var entry = LogEntry.Info("Ignored message");

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task LogAsync_ShouldDispatchLogEntry_ToProvider()
    {
        // Arrange
        FakeLogProvider provider = new();
        ILoggerService logger = CreateLoggerService(providers: provider);
        var entry = LogEntry.Warning("Warning message");

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();

        provider.Entries[0].Message.Should().Be("Warning message");
        provider.Entries[0].Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public async Task LogAsync_ShouldDispatchLogEntry_ToMultipleProviders()
    {
        // Arrange
        FakeLogProvider provider1 = new();
        FakeLogProvider provider2 = new();

        ILoggerService logger = CreateLoggerService(
            providers: [provider1, provider2]);

        var entry = LogEntry.Info("Multi-provider message");

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider1.Entries.Should().ContainSingle();
        provider2.Entries.Should().ContainSingle();

        provider1.Entries[0].Message.Should().Be("Multi-provider message");
        provider2.Entries[0].Message.Should().Be("Multi-provider message");
    }

    [Fact]
    public async Task LogAsync_ShouldNormalizeLogEntry_BeforeProviderExecution()
    {
        // Arrange
        FakeLogProvider provider = new();
        ILoggerService logger = CreateLoggerService(providers: provider);

        LogEntry entry = new()
        {
            Message = "   Normalized message   "
        };

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();
        provider.Entries[0].Message.Should().Be("Normalized message");
    }

    [Fact]
    public async Task LogAsync_ShouldAssignTraceId_FromTraceContext()
    {
        // Arrange
        FakeLogProvider provider = new();
        ILoggerService logger = CreateLoggerService(providers: provider);
        var entry = LogEntry.Info("Trace test");

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();
        provider.Entries[0].TraceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LogAsync_ShouldThrow_WhenMessageExceedsMaximumLength()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(
            options =>
            {
                options.MaxMessageLength = 10;
            },
            provider);

        var entry = LogEntry.Info(
            "This message is definitely too long");

        // Act
        Func<Task> action = async () =>
            await logger.LogAsync(entry);

        // Assert
        await action.Should()
            .ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LogAsync_ShouldAllowUnlimitedMessageLength_WhenMaxLengthIsZero()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(
            options =>
            {
                options.MaxMessageLength = 0;
            },
            provider);

        var entry = LogEntry.Info(new string('A', 5000));

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();
    }

    [Fact]
    public async Task LogAsync_ShouldContinueExecutingRemainingProviders_WhenProviderFails()
    {
        // Arrange
        FakeLogProvider failingProvider = new()
        {
            ThrowException = true
        };

        FakeLogProvider successfulProvider = new();

        ILoggerService logger = CreateLoggerService(
            providers: [failingProvider, successfulProvider]);

        var entry = LogEntry.Error(
            "Provider isolation test",
            new InvalidOperationException("Failure"));

        // Act
        Func<Task> action = async () =>
            await logger.LogAsync(
                entry,
                cancellationToken: CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();

        successfulProvider.Entries.Should().ContainSingle();

        successfulProvider.Entries[0]
            .Message.Should()
            .Be("Provider isolation test");
    }

    [Fact]
    public async Task LogAsync_ShouldInvokeProviderFailureCallback_WhenProviderFails()
    {
        // Arrange
        FakeLogProvider failingProvider = new()
        {
            ThrowException = true
        };

        ILoggerService logger = CreateLoggerService(
            providers: failingProvider);

        ProviderFailureContext? capturedContext = null;

        var entry = LogEntry.Error(
            "Callback test",
            new InvalidOperationException("Failure"));

        // Act
        await logger.LogAsync(
            entry,
            providerFailureCallback: (context, _) =>
            {
                capturedContext = context;
                return Task.CompletedTask;
            },
            cancellationToken: CancellationToken.None);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext.ProviderType.Should().Be(failingProvider.GetType());

        capturedContext.Exception.Should()
            .BeOfType<InvalidOperationException>();

        capturedContext.LogEntry.Message.Should()
            .Be("Callback test");
    }

    [Fact]
    public async Task LogAsync_ShouldPropagateException_WhenProviderFailureCallbackFails()
    {
        // Arrange
        FakeLogProvider failingProvider = new()
        {
            ThrowException = true
        };

        ILoggerService logger = CreateLoggerService(
            providers: failingProvider);

        var entry = LogEntry.Error("Callback propagation test");

        // Act
        Func<Task> action = async () =>
            await logger.LogAsync(
                entry,
                providerFailureCallback: (_, _) =>
                {
                    throw new InvalidOperationException("Callback failure.");
                },
                cancellationToken: CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Callback failure.");
    }

    [Fact]
    public async Task LogAsync_ShouldMergeScopeContext_WithLogEntry()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(providers: provider);

        LogContext context = new()
        {
            TraceId = "scope-trace-id",
            CorrelationId = "scope-correlation-id",
            Source = "scope-source",
            Metadata = ImmutableDictionary<string, object>.Empty
                .Add("Environment", "Production")
        };

        using IDisposable scope = logger.BeginScope(context);

        var entry = LogEntry.Info("Scoped message");

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();

        LogEntry loggedEntry = provider.Entries[0];

        loggedEntry.TraceId.Should().Be("scope-trace-id");
        loggedEntry.CorrelationId.Should().Be("scope-correlation-id");
        loggedEntry.Source.Should().Be("scope-source");

        loggedEntry.Metadata.Should().NotBeNull();

        loggedEntry.Metadata["Environment"]
            .Should()
            .Be("Production");
    }

    [Fact]
    public async Task LogAsync_ShouldPreserveLogEntryValues_OverScopeContext()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(providers: provider);

        LogContext context = new()
        {
            TraceId = "scope-trace-id",
            CorrelationId = "scope-correlation-id",
            Source = "scope-source"
        };

        using IDisposable scope = logger.BeginScope(context);

        LogEntry entry = new()
        {
            Message = "Scoped override",
            TraceId = "entry-trace-id",
            CorrelationId = "entry-correlation-id",
            Source = "entry-source"
        };

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();

        LogEntry loggedEntry = provider.Entries[0];

        loggedEntry.TraceId.Should().Be("entry-trace-id");
        loggedEntry.CorrelationId.Should().Be("entry-correlation-id");
        loggedEntry.Source.Should().Be("entry-source");
    }

    [Fact]
    public async Task LogAsync_ShouldMergeMetadata_FromScopeAndLogEntry()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(providers: provider);

        LogContext context = new()
        {
            Metadata = ImmutableDictionary<string, object>.Empty
                .Add("Environment", "Production")
                .Add("Version", "1.0")
        };

        using IDisposable scope = logger.BeginScope(context);

        LogEntry entry = new()
        {
            Message = "Metadata merge",
            Metadata = ImmutableDictionary<string, object>.Empty
                .Add("UserId", "123")
                .Add("Version", "2.0")
        };

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();

        LogEntry loggedEntry = provider.Entries[0];

        loggedEntry.Metadata.Should().NotBeNull();

        loggedEntry.Metadata["Environment"]
            .Should()
            .Be("Production");

        loggedEntry.Metadata["UserId"]
            .Should()
            .Be("123");

        // Entry metadata overrides scope metadata.
        loggedEntry.Metadata["Version"]
            .Should()
            .Be("2.0");
    }

    [Fact]
    public async Task LogAsync_ShouldExecuteSuccessfully_WhenNoProvidersAreRegistered()
    {
        // Arrange
        ILoggerService logger = CreateLoggerService();

        var entry = LogEntry.Info("No providers");

        // Act
        Func<Task> action = async () =>
            await logger.LogAsync(
                entry,
                cancellationToken: CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogAsync_ShouldPreserveImmutableLogEntryBehavior()
    {
        // Arrange
        FakeLogProvider provider = new();

        ILoggerService logger = CreateLoggerService(providers: provider);

        LogEntry originalEntry = new()
        {
            Message = "   Immutable message   "
        };

        // Act
        await logger.LogAsync(
            originalEntry,
            cancellationToken: CancellationToken.None);

        // Assert
        provider.Entries.Should().ContainSingle();

        originalEntry.Message.Should()
            .Be("   Immutable message   ");

        provider.Entries[0].Message.Should()
            .Be("Immutable message");

        provider.Entries[0]
            .Should()
            .NotBeSameAs(originalEntry);
    }

    [Fact]
    public async Task LogAsync_ShouldPropagateCancellationToken_ToProviders()
    {
        // Arrange
        CancellationToken receivedToken = default;

        Mock<ILogProvider> providerMock = new();

        providerMock
            .Setup(x => x.WriteAsync(
                It.IsAny<LogEntry>(),
                It.IsAny<CancellationToken>()))
            .Callback<LogEntry, CancellationToken>((_, token) =>
            {
                receivedToken = token;
            })
            .Returns(Task.CompletedTask);

        ILoggerService logger = CreateLoggerService(
            providers: providerMock.Object);

        var entry = LogEntry.Info("Cancellation token test");

        using CancellationTokenSource cancellationTokenSource = new();

        // Act
        await logger.LogAsync(
            entry,
            cancellationToken: cancellationTokenSource.Token);

        // Assert
        receivedToken.Should()
            .Be(cancellationTokenSource.Token);
    }

    private static ILoggerService CreateLoggerService(
        Action<LoggingOptions>? configure = null,
        params ILogProvider[] providers)
    {
        ServiceCollection services = new();

        services.AddHmLogging(options =>
            configure?.Invoke(options));

        foreach (ILogProvider provider in providers)
        {
            services.AddSingleton(provider);
        }

        ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        return serviceProvider
            .GetRequiredService<ILoggerService>();
    }
}