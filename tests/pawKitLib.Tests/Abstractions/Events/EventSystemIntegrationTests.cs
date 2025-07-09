using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace pawKitLib.Tests.Abstractions.Events;

/// <summary>
/// Integration tests demonstrating the usage of the event system abstractions in pawKit.
/// </summary>
/// <remarks>
/// This test verifies that:
/// - An event implementing <see cref="IEvent"/> can be published using an <see cref="IEventPublisher"/>.
/// - A handler implementing <see cref="IEventHandler{TEvent}"/> can be registered and invoked.
/// - The event is delivered to the handler and handled as expected.
/// The test setup is fully isolated and uses only in-memory implementations, following the pawKit design principles for testability and replacement.
/// </remarks>
public class EventSystemIntegrationTests
{
    /// <summary>
    /// Verifies that a published event is handled by a registered handler.
    /// </summary>
    [Fact]
    public async Task TestEvent_IsHandledByRegisteredHandler()
    {
        // Arrange: Set up the in-memory publisher and handler, and register the handler.
        var publisher = new InMemoryEventPublisher();
        var handler = new TestEventHandler();
        publisher.RegisterHandler(handler);
        var testEvent = new TestEvent("Hello, world!");

        // Act: Publish the event asynchronously.
        await publisher.PublishAsync(testEvent, CancellationToken.None);

        // Assert: The handler should have been called and received the correct event.
        Assert.True(handler.WasCalled);
        Assert.Equal(testEvent, handler.HandledEvent);
    }
}
