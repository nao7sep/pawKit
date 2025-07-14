namespace pawKitLib.Models;

public abstract class BaseDto
{
    // See 'DTO Design Guidelines' for philosophy and conventions.
    // BaseDto is intentionally minimal and untyped, serving as an extensible foundation for concrete, strongly-typed DTOs.
    // ExtraProperties is non-nullable and initialized to an empty dictionary, following best practices for collections:
    //   - Always available, never null, no need for 'required' or 'init'.
    //   - Use for optional, experimental, or unknown fields only; prefer explicit properties in derived DTOs for core parameters.
    //   - Never store sensitive data here; treat as unvalidated, dynamic content.
    //   - Document conventions for keys and value types if used widely.
    //   - Ensure your serializer can handle object? values; use custom converters if needed.
    //   - For thread safety, use a concurrent dictionary or locking only if required.
    public Dictionary<string, object?> ExtraProperties { get; set; } = [];
}
