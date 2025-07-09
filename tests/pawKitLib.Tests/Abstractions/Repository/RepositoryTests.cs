using System;
using System.Threading.Tasks;
using Xunit;

namespace pawKitLib.Tests.Abstractions.Repository;

/// <summary>
/// Integration tests for <see cref="IRepository{TEntity}"/> using an in-memory implementation.
/// </summary>
/// <remarks>
/// These tests verify that the repository abstraction supports add, retrieve, update, and remove operations as expected.
/// The in-memory implementation is fully isolated, uses dependency injection for the key selector, and follows all pawKit design principles for testability and replacement.
/// </remarks>
public class RepositoryTests
{
    /// <summary>
    /// Verifies that an entity can be added and retrieved by its ID.
    /// </summary>
    [Fact]
    public async Task Can_Add_And_Retrieve_Entity()
    {
        // Arrange: Create repository and entity.
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act: Add and retrieve the entity.
        await repository.AddAsync(entity);
        var retrieved = await repository.GetByIdAsync(entity.Id);

        // Assert: The entity should be found and match the original.
        Assert.NotNull(retrieved);
        Assert.Equal(entity.Name, retrieved!.Name);
    }

    /// <summary>
    /// Verifies that an entity can be updated and removed.
    /// </summary>
    [Fact]
    public async Task Can_Update_And_Remove_Entity()
    {
        // Arrange: Create repository and add entity.
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Initial" };
        await repository.AddAsync(entity);

        // Act: Update the entity and retrieve it.
        entity.Name = "Updated";
        repository.Update(entity);
        var updated = await repository.GetByIdAsync(entity.Id);

        // Assert: The entity should reflect the update.
        Assert.Equal("Updated", updated!.Name);

        // Act: Remove the entity.
        repository.Remove(entity);
        var afterRemove = await repository.GetByIdAsync(entity.Id);

        // Assert: The entity should no longer exist.
        Assert.Null(afterRemove);
    }
}
