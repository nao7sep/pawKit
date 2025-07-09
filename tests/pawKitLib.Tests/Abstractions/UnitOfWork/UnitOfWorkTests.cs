using System.Threading.Tasks;
using Xunit;

namespace pawKitLib.Tests.Abstractions.UnitOfWork;

/// <summary>
/// Integration tests for <see cref="IUnitOfWork"/> using an in-memory implementation.
/// </summary>
/// <remarks>
/// These tests verify that the unit of work abstraction supports commit, rollback, and disposal as expected.
/// The in-memory implementation is fully isolated and follows all pawKit design principles for testability and replacement.
/// </remarks>
public class UnitOfWorkTests
{
    /// <summary>
    /// Verifies that commit, rollback, and disposal operations are tracked correctly.
    /// </summary>
    /// <remarks>
    /// This test ensures that the in-memory unit of work correctly updates its state flags when each operation is called.
    /// It demonstrates that the abstraction can be used in a test context without side effects or external dependencies.
    /// </remarks>
    [Fact]
    public async Task Can_Commit_And_Rollback()
    {
        // Arrange: Create the in-memory unit of work.
        await using var uow = new InMemoryUnitOfWork();

        // Act: Commit, rollback, and dispose the unit of work.
        await uow.CommitAsync();
        await uow.RollbackAsync();
        await uow.DisposeAsync();

        // Assert: All operations should be reflected in the state flags.
        Assert.True(uow.Committed, "CommitAsync should set Committed to true.");
        Assert.True(uow.RolledBack, "RollbackAsync should set RolledBack to true.");
        Assert.True(uow.Disposed, "DisposeAsync should set Disposed to true.");
    }
}
