using pawKitLib.Abstractions;

namespace pawKitLib.Security;

/// <summary>
/// A default implementation of <see cref="IUniqueIdGenerator"/> that uses <see cref="Guid.NewGuid()"/>.
/// </summary>
public sealed class GuidIdGenerator : IUniqueIdGenerator
{
    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();
}