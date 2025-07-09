using System;
using System.Threading.Tasks;
using Xunit;

namespace pawKitLib.Tests.Abstractions.Validator;

/// <summary>
/// Integration tests for <see cref="IValidator{T}"/> using a simple implementation.
/// </summary>
/// <remarks>
/// These tests verify that the validator abstraction correctly identifies valid and invalid entities.
/// The implementation is fully isolated and follows all pawKit design principles for testability and replacement.
/// </remarks>
public class ValidatorTests
{
    /// <summary>
    /// Verifies that validation returns an error for an invalid entity.
    /// </summary>
    /// <remarks>
    /// This test ensures that the validator detects when the entity's Name property is empty and returns the expected error message.
    /// </remarks>
    [Fact]
    public async Task Returns_Error_For_Invalid_Entity()
    {
        // Arrange: Create validator and invalid entity (Name is empty, which is invalid).
        var validator = new TestEntityValidator();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = string.Empty };

        // Act: Validate the entity.
        var errors = await validator.ValidateAsync(entity);

        // Assert: The expected error should be present in the result.
        Assert.Contains("Name must not be empty.", errors);
    }

    /// <summary>
    /// Verifies that validation returns no errors for a valid entity.
    /// </summary>
    /// <remarks>
    /// This test ensures that the validator does not return any errors when the entity's Name property is set to a valid value.
    /// </remarks>
    [Fact]
    public async Task Returns_No_Error_For_Valid_Entity()
    {
        // Arrange: Create validator and valid entity (Name is non-empty).
        var validator = new TestEntityValidator();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Valid" };

        // Act: Validate the entity.
        var errors = await validator.ValidateAsync(entity);

        // Assert: There should be no errors.
        Assert.Empty(errors);
    }
}
