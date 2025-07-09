using System;

namespace pawKitLib.Tests.Abstractions.Repository;

/// <summary>
/// A simple test entity used for repository and validator tests.
/// </summary>
/// <remarks>
/// This entity is intentionally minimal, with only an ID and Name, to focus tests on repository and validation logic.
/// </remarks>
public class TestEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// Used as the primary key in repository tests.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the entity.
    /// Used for validation scenarios.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
