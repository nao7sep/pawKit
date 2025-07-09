using pawKitLib.Abstractions.Events;

namespace pawKitLib.Tests.Abstractions.Events;

/// <summary>
/// Represents a sample event for testing the event system.
/// </summary>
/// <remarks>
/// This immutable record implements <see cref="IEvent"/> and is used to demonstrate event publishing and handling.
/// </remarks>
public record TestEvent(string Message) : IEvent;
