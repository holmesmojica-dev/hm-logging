using Hm.Logging.Models;

namespace Hm.Logging.Core.Scopes;

/// <summary>
/// Represents a node within the internal logging scope propagation chain.
/// </summary>
/// <remarks>
/// Each <see cref="ScopeNode"/> contains the current <see cref="LogContext"/>
/// and a reference to its parent scope, enabling nested scope propagation
/// through an immutable linked-chain structure.
///
/// <para>
/// This internal structure allows:
/// </para>
///
/// <list type="bullet">
/// <item>
/// Parent-to-child contextual propagation.
/// </item>
/// <item>
/// Child scope override precedence.
/// </item>
/// <item>
/// Proper scope restoration when nested scopes are disposed.
/// </item>
/// <item>
/// AsyncLocal-based contextual flow across asynchronous operations.
/// </item>
/// </list>
///
/// <para>
/// Scope precedence order:
/// </para>
///
/// <code>
/// Parent Scope
///     ↓ overridden by
/// Child Scope
///     ↓ overridden by
/// LogEntry
/// </code>
/// </remarks>
internal sealed record ScopeNode(
    LogContext Context,
    ScopeNode? Parent);