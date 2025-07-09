namespace pawKitLib.Abstractions.Events;

/// <summary>
/// Defines a handler for a specific type of event.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Asynchronously handles the specified event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}