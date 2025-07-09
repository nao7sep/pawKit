using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Abstractions.Events;

namespace pawKitLib.Tests.Abstractions.Events;

/// <summary>
/// An in-memory implementation of <see cref="IEventPublisher"/> for testing and demonstration.
/// </summary>
/// <remarks>
/// This publisher allows registration of event handlers and publishes events to all registered handlers of the matching type.
/// It is designed for isolation and replacement, following the pawKit design principles.
/// </remarks>
public class InMemoryEventPublisher : IEventPublisher
{
    // Stores handlers by event type. Thread-safe for concurrent test scenarios.
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="handler">The handler to register.</param>
    public void RegisterHandler<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var handlers = _handlers.GetOrAdd(typeof(TEvent), _ => new List<object>());
        handlers.Add(handler);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        // Find all registered handlers for this event type and invoke them asynchronously.
        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            var tasks = handlers
                .OfType<IEventHandler<TEvent>>()
                .Select(h => h.HandleAsync(@event, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}
