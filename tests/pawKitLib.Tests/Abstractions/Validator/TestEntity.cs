using System;

namespace pawKitLib.Tests.Abstractions.Validator;

/// <summary>
/// A simple test entity used for validator tests.
/// </summary>
/// <remarks>
/// This entity is intentionally minimal, with only an ID and Name, to focus tests on validation logic.
/// </remarks>
public class TestEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// Used as the primary key in validation tests.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the entity.
    /// Used for validation scenarios.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
