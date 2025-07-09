using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Abstractions;

namespace pawKitLib.Tests.Abstractions.UnitOfWork;

/// <summary>
/// A simple in-memory implementation of <see cref="IUnitOfWork"/> for demonstration and testing.
/// </summary>
/// <remarks>
/// This implementation tracks commit, rollback, and disposal operations using boolean flags.
/// It is designed for use in tests to verify that the unit of work pattern is respected by consuming code.
/// No external resources are used, ensuring test isolation and repeatability.
/// </remarks>
public class InMemoryUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Gets a value indicating whether <see cref="CommitAsync"/> was called.
    /// </summary>
    public bool Committed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="RollbackAsync"/> was called.
    /// </summary>
    public bool RolledBack { get; private set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="DisposeAsync"/> was called.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Committed = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        RolledBack = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}
