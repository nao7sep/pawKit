using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Abstractions.Events;

namespace pawKitLib.Tests.Abstractions.Events;

/// <summary>
/// Handles <see cref="TestEvent"/> instances for testing purposes.
/// </summary>
/// <remarks>
/// This handler records the event it receives and sets a flag when called, allowing assertions in tests.
/// </remarks>
public class TestEventHandler : IEventHandler<TestEvent>
{
    /// <summary>
    /// Gets the event instance that was handled, or null if none.
    /// </summary>
    public TestEvent? HandledEvent { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the handler was called.
    /// </summary>
    public bool WasCalled { get; private set; }

    /// <inheritdoc />
    public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
    {
        HandledEvent = @event;
        WasCalled = true;
        return Task.CompletedTask;
    }
}
