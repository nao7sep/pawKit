using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Abstractions;

namespace pawKitLib.Tests.Abstractions.Repository;

/// <summary>
/// An in-memory implementation of <see cref="IRepository{TEntity}"/> for testing purposes.
/// </summary>
/// <remarks>
/// This implementation uses a thread-safe <see cref="ConcurrentDictionary{TKey, TValue}"/> to store entities in memory.
/// The key selector is injected to allow flexible key strategies for different entity types.
/// All operations are synchronous in-memory, making this suitable for fast, isolated unit and integration tests.
/// No external dependencies are required, and the implementation is safe for concurrent test execution.
/// </remarks>
public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    // Thread-safe in-memory store for entities, keyed by the provided selector.
    private readonly ConcurrentDictionary<object, TEntity> _store = new();
    // Function to extract the key from an entity instance.
    private readonly Func<TEntity, object> _keySelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="keySelector">A function to extract the key from an entity.</param>
    public InMemoryRepository(Func<TEntity, object> keySelector)
    {
        _keySelector = keySelector;
    }

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        // Attempt to retrieve the entity by its key. Returns null if not found.
        _store.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Return all entities as a read-only list.
        return Task.FromResult((IReadOnlyList<TEntity>)_store.Values.ToList());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Compile the predicate and filter the in-memory collection.
        var compiled = predicate.Compile();
        var results = _store.Values.Where(compiled).ToList();
        return Task.FromResult((IReadOnlyList<TEntity>)results);
    }

    /// <inheritdoc />
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Add or replace the entity in the store using its key.
        var key = _keySelector(entity);
        _store[key] = entity;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        // Update the entity in the store. If it does not exist, it is added.
        var key = _keySelector(entity);
        _store[key] = entity;
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        // Remove the entity from the store by its key.
        var key = _keySelector(entity);
        _store.TryRemove(key, out _);
    }
}
