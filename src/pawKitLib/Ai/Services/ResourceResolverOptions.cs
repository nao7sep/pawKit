namespace pawKitLib.Ai.Services;

/// <summary>
/// Provides configuration options for the <see cref="ResourceResolver"/>.
/// </summary>
public sealed record ResourceResolverOptions
{
    /// <summary>The absolute base path from which local files are allowed to be resolved.</summary>
    public required string AllowedBasePath { get; init; }
}