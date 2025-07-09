﻿namespace pawKitLib.Abstractions;

/// <summary>
/// Defines a contract for generating unique identifiers.
/// </summary>
/// <remarks>
/// The primary purpose of this abstraction is to facilitate testing. In a unit test,
/// the default implementation can be replaced with a mock or fake generator that returns a
/// predictable, deterministic GUID. This allows services that create new entities with unique
/// IDs to be tested reliably, as the expected ID can be known in advance.
///
/// It also provides flexibility to swap out the GUID generation strategy in production
/// (e.g., from standard GUIDs to a sequential or time-based variant) without altering
/// consumer code.
/// </remarks>
public interface IUniqueIdGenerator
{
    /// <summary>
    /// Creates a new unique identifier.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    Guid NewGuid();
}