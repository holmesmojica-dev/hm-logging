# Hm.Logging

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=holmesmojica-dev_hm-logging&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=holmesmojica-dev_hm-logging)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=holmesmojica-dev_hm-logging&metric=coverage)](https://sonarcloud.io/summary/new_code?id=holmesmojica-dev_hm-logging)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-purple)](https://dotnet.microsoft.com/)

Enterprise-grade structured logging library for modern .NET applications and distributed systems.

Hm.Logging provides resilient multi-provider logging orchestration, distributed tracing integration, scope propagation, metadata normalization, and immutable log processing semantics while maintaining a clean and developer-friendly API.

---

## Overview

Hm.Logging was designed for modern microservice-based architectures where observability, traceability, resilience, and maintainability are critical.

The library focuses on:

- Structured logging
- Immutable log processing
- Distributed tracing integration
- Scope propagation
- Multi-provider orchestration
- Provider isolation
- Metadata normalization
- Enterprise-grade developer experience (DX)

Hm.Logging intentionally separates:
- lightweight logging orchestration responsibilities

from:

- advanced resilience,
- retries,
- buffering,
- batching,
- and transport-specific operational concerns.

---

## Design Philosophy

The library follows a set of architectural principles:

- Clean Architecture
- Explicit contracts
- Immutable models
- Provider isolation
- Predictable orchestration behavior
- Low coupling
- High cohesion
- Developer Experience (DX) first
- Production-ready defaults

Hm.Logging avoids:
- hidden side effects,
- silent normalization inconsistencies,
- overly complex pipelines,
- unnecessary abstractions,
- and external resilience dependencies.

---

## Features

- Structured logging
- Multi-provider orchestration
- Distributed tracing integration
- AsyncLocal-based scope propagation
- Metadata normalization
- Reserved metadata protection
- Immutable log processing
- Provider isolation behavior
- Provider failure diagnostics callbacks
- CancellationToken propagation
- Validation pipeline
- XML documentation
- Roslyn analyzer support
- SonarCloud validated
- High unit test coverage

---

## Installation

### NuGet

```bash
dotnet add package Hm.Logging
```

---

## Supported Platforms

- .NET 10

---

## Quick Start

### Dependency Injection Registration

```csharp
builder.Services.AddHmLogging(options =>
{
    options.MinimumLevel = LogLevel.Information;
    options.MaxMessageLength = 4000;
});
```

---

## Basic Logging

```csharp
public sealed class UserService(ILoggerService logger)
{
    public async Task CreateUserAsync()
    {
        await logger.LogAsync(
            LogEntry.Info("User created successfully"));
    }
}
```

---

## Warning Logging

```csharp
await logger.LogAsync(
    LogEntry.Warning("Cache response time exceeded threshold."));
```

---

## Error Logging

```csharp
try
{
    await repository.SaveAsync();
}
catch (Exception ex)
{
    await logger.LogAsync(
        LogEntry.Error("Failed to persist user.", ex));
}
```

---

## Structured Metadata

```csharp
LogEntry entry = LogEntry.Info("Order processed.") with
{
    Metadata = ImmutableDictionary<string, object>.Empty
        .Add("OrderId", 1001)
        .Add("Environment", "Production")
        .Add("ElapsedMilliseconds", 245)
};

await logger.LogAsync(entry);
```

---

## Distributed Tracing

Hm.Logging integrates with distributed tracing systems through `ITraceContext`.

If a `TraceId` is not explicitly provided:
- the current trace context is used automatically,
- or a fallback trace identifier is automatically generated internally.

### Explicit TraceId

```csharp
LogEntry entry = LogEntry.Info("Processing request.") with
{
    TraceId = "trace-123"
};

await logger.LogAsync(entry);
```

Resulting log entry:

```json
{
  "Message": "Processing request.",
  "TraceId": "trace-123"
}
```

In this example:
- the explicit `TraceId` provided by the log entry is preserved,
- and no automatic trace resolution occurs.

---

### Automatic Trace Context Resolution

If the log entry does not define a `TraceId`, Hm.Logging attempts to resolve the current distributed trace context automatically.

```csharp
await logger.LogAsync(
    LogEntry.Info("Processing payment."));
```

Example resulting log entry:

```json
{
  "Message": "Processing payment.",
  "TraceId": "4f9c2d13-71b7-4bc1-9b3a-92f7b26d8f44"
}
```

In this example:
- the `TraceId` was automatically resolved from the active trace context,
- or generated internally as a fallback identifier.

This behavior ensures log traceability consistency across distributed systems and microservice boundaries.

---

## Logging Scopes

Scopes allow contextual information to automatically propagate across multiple log entries.

### Basic Scope Usage

```csharp
LogContext context = new()
{
    TraceId = "trace-123",
    CorrelationId = "correlation-456",
    Source = "OrderService",
    Metadata = ImmutableDictionary<string, object>.Empty
        .Add("Tenant", "NorthAmerica")
};

using IDisposable scope = logger.BeginScope(context);

await logger.LogAsync(
    LogEntry.Info("Order validation completed."));

await logger.LogAsync(
    LogEntry.Info("Payment authorization completed."));
```

When the log entry is processed, contextual information from the active scope is automatically merged into the log entry.

Resulting log entry:

```json
{
  "Message": "Order validation completed.",
  "TraceId": "trace-123",
  "CorrelationId": "correlation-456",
  "Source": "OrderService",
  "Metadata": {
    "Tenant": "NorthAmerica"
  }
}
```

If the log entry explicitly defines a value already present in the scope, the log entry value takes precedence.

Example:

```csharp
await logger.LogAsync(
    LogEntry.Info("Payment completed.") with
    {
        Source = "PaymentService"
    });
```

Resulting log entry:

```json
{
  "Message": "Payment completed.",
  "TraceId": "trace-123",
  "CorrelationId": "correlation-456",
  "Source": "PaymentService",
  "Metadata": {
    "Tenant": "NorthAmerica"
  }
}
```

In this example:
- `TraceId` and `CorrelationId` are inherited from the scope.
- `Source` is overridden by the log entry.

---

## Nested Scopes & Context Propagation

Hm.Logging supports nested logging scopes through `AsyncLocal` context propagation.

Scopes behave as a contextual propagation stack:
- parent scopes propagate values downward,
- child scopes inherit parent values,
- and child scopes override parent values when conflicts occur.

This allows contextual information to flow naturally across asynchronous operations and distributed execution pipelines.

### Parent Scope

```csharp
using IDisposable parentScope = logger.BeginScope(new LogContext
{
    TraceId = "trace-123",
    Source = "OrderService",
    Metadata = ImmutableDictionary<string, object>.Empty
        .Add("Region", "NorthAmerica")
});

await logger.LogAsync(
    LogEntry.Info("Order received."));
```

Resulting log entry:

```json
{
  "Message": "Order received.",
  "TraceId": "trace-123",
  "Source": "OrderService",
  "Metadata": {
    "Region": "NorthAmerica"
  }
}
```

---

### Nested Child Scope

```csharp
using IDisposable parentScope = logger.BeginScope(new LogContext
{
    TraceId = "trace-123",
    Source = "OrderService",
    Metadata = ImmutableDictionary<string, object>.Empty
        .Add("Region", "NorthAmerica")
});

using IDisposable childScope = logger.BeginScope(new LogContext
{
    Source = "PaymentService",
    Metadata = ImmutableDictionary<string, object>.Empty
        .Add("PaymentProvider", "Stripe")
});

await logger.LogAsync(
    LogEntry.Info("Payment authorized."));
```

Resulting log entry:

```json
{
  "Message": "Payment authorized.",
  "TraceId": "trace-123",
  "Source": "PaymentService",
  "Metadata": {
    "Region": "NorthAmerica",
    "PaymentProvider": "Stripe"
  }
}
```

In this example:
- `TraceId` is inherited from the parent scope.
- `Source` is overridden by the child scope.
- Metadata from both scopes is merged together.
- Child scope values take precedence when conflicts occur.

---

### LogEntry Override Precedence

Explicit `LogEntry` values always take precedence over active scope values.

```csharp
using IDisposable scope = logger.BeginScope(new LogContext
{
    Source = "OrderService"
});

await logger.LogAsync(
    LogEntry.Info("Custom source example") with
    {
        Source = "ManualOverrideService"
    });
```

Resulting log entry:

```json
{
  "Message": "Custom source example",
  "Source": "ManualOverrideService"
}
```

In this example:
- the active scope provides `OrderService`,
- but the explicit `LogEntry` value overrides the scope value.

---

### Scope Disposal Behavior

Scopes only affect log entries executed while the scope is active.

Once a scope is disposed:
- its contextual values stop propagating,
- and parent scopes (if any) become active again.

This behavior ensures predictable contextual propagation across asynchronous execution flows.

---

## Multi-Provider Orchestration

Hm.Logging supports multiple providers simultaneously.

Example providers:
- File provider
- Database provider
- Console provider

Providers are executed sequentially in registration order while preserving:
- ordering behavior,
- async semantics,
- and immutable processing.

---

## Provider Isolation

Provider failures do NOT interrupt the logging pipeline.

If a provider fails:
- the exception is captured internally,
- remaining registered providers continue executing,
- and optional diagnostics callbacks may be triggered.

This behavior prevents a single failing provider from affecting:
- application flow,
- or remaining logging infrastructure.

---

## Provider Failure Diagnostics

Hm.Logging supports optional provider diagnostics callbacks for observability scenarios.

```csharp
await logger.LogAsync(
    LogEntry.Error("Provider test"),
    providerFailureCallback: async (context, cancellationToken) =>
    {
        Console.WriteLine(
            $"Provider failed: {context.ProviderType.Name}");

        Console.WriteLine(context.Exception.Message);

        await Task.CompletedTask;
    });
```

---

## Important Callback Semantics

Exceptions thrown by diagnostics callbacks:
- are NOT intercepted by the logging pipeline,
- and propagate to the caller.

This is intentional.

Hm.Logging protects:
- provider orchestration,
- internal logging flow,
- and provider isolation.

Hm.Logging does NOT protect:
- arbitrary external consumer logic.

This preserves:
- ownership boundaries,
- debugging clarity,
- and operational traceability.

Consumers are responsible for handling exceptions originating from custom diagnostics callbacks.

---

## Immutable Processing Behavior

All log normalization and enrichment operations return new immutable instances.

Original log entries are never modified.

Example:

```csharp
ITraceContext traceContext = ...;

LogEntry original = new()
{
    Message = "   Example message   "
};

LogEntry normalized = original.EnsureValid(traceContext);

Console.WriteLine(original.Message);
// "   Example message   "

Console.WriteLine(normalized.Message);
// "Example message"
```

---

## Metadata Normalization

Metadata is automatically normalized before provider dispatch.

Normalization includes:
- key trimming,
- value trimming,
- whitespace cleanup,
- enum string conversion,
- Guid string conversion,
- unsupported metadata value rejection,
- reserved key protection.

---

## Supported Metadata Types

Supported metadata values include:

- string
- bool
- byte
- short
- int
- long
- float
- double
- decimal
- Guid
- DateTime
- DateTimeOffset
- TimeSpan
- enums

Unsupported types throw validation exceptions.

---

## Reserved Metadata Keys

The following keys are reserved by the logging pipeline:

- TraceId
- CorrelationId
- Timestamp
- Level
- Message
- Source

Reserved keys are protected using case-insensitive comparisons, meaning keys such as `TraceId`, `traceid`, and `TRACEID` are treated as equivalent.

---

## Validation Pipeline

Before dispatching to providers, log entries are validated and normalized.

Validation includes:
- message validation,
- UTC timestamp normalization,
- log level validation,
- metadata validation,
- distributed trace enrichment,
- string cleanup.

---

## CancellationToken Support

Hm.Logging fully propagates `CancellationToken` values across the logging pipeline and provider dispatch flow.

```csharp
await logger.LogAsync(
    LogEntry.Info("Cancellation example"),
    cancellationToken: cancellationToken);
```

---

## Quality & Testing

Hm.Logging includes:

- High unit test coverage
- SonarCloud quality validation
- Roslyn analyzers
- `dotnet format` integration
- Immutable behavior validation
- Provider isolation testing
- Distributed tracing validation
- Scope propagation validation

Current quality metrics:

- ~95% test coverage
- 0 security hotspots
- 0 code duplications
- Sonar Quality Gate passing

---

## Architecture Overview

The Hm.Logging ecosystem is designed as a layered observability platform composed of independent but complementary components.

### Hm.Logging

Core logging library distributed as a NuGet package.

Responsibilities:
- orchestration,
- validation,
- normalization,
- tracing integration,
- provider dispatching,
- observability.

---

### Hm.Logging.Contracts

Future gRPC contract package.

Responsibilities:
- shared contracts,
- DTOs,
- protobuf definitions.

---

### Hm.Logging.Service

Future centralized logging microservice.

Responsibilities:
- persistence,
- advanced resilience,
- retries,
- buffering,
- batching,
- provider failover,
- centralized observability.

---

## Roadmap

Planned future improvements include:

- Additional logging providers
- OpenTelemetry integration
- Centralized logging service
- Observability dashboard integrations
- Advanced observability tooling
- Distributed ingestion pipeline

---

## Best Practices

Recommended practices:

- Use scopes for contextual propagation
- Prefer structured metadata over string concatenation
- Keep providers lightweight
- Handle diagnostics callbacks carefully
- Avoid large message payloads
- Use distributed tracing consistently
- Preserve immutable log semantics

---

## Contributing

Contributions, suggestions, and architectural discussions are welcome.

The project prioritizes:
- architectural consistency,
- maintainability,
- and developer experience.

Before contributing:
- ensure tests pass,
- follow formatting rules,
- maintain architectural consistency,
- and preserve DX quality standards.

---

## License

MIT License.
