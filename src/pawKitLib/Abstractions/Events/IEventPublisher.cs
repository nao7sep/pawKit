namespace pawKitLib.Abstractions.Events;

/// <summary>
/// Defines a contract for publishing events to any interested subscribers.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Asynchronously publishes an event to all registered handlers.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}