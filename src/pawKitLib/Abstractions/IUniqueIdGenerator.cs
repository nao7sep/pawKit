namespace pawKitLib.Abstractions;

/// <summary>
/// Defines a contract for generating unique identifiers.
/// </summary>
/// <remarks>
/// Abstracting ID generation allows for deterministic IDs in tests (e.g., sequential GUIDs)
/// and flexibility in production (e.g., using standard GUIDs or other strategies).
/// </remarks>
public interface IUniqueIdGenerator
{
    /// <summary>
    /// Creates a new unique identifier.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    Guid NewGuid();
}