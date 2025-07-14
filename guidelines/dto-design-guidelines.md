# DTO Design Guidelines

## Purpose and Philosophy
DTOs (Data Transfer Objects) are meant to provide clarity, safety, and maintainability when moving data between layers or systems. Prioritize practical, real-world solutions and avoid over-engineering. Flexibility and adaptability are key—let DTOs evolve as requirements change.

## Property Design: Mutable vs. Immutable
Use mutable (`get; set;`) properties for most DTO fields, especially when requirements or API behaviors are uncertain. Only enforce immutability (`required init`) for properties that are truly essential and guaranteed to never change (such as IDs or creation timestamps). In practice, it is often unclear which fields will remain unchanged, so avoid spending excessive time deliberating over immutability for pure data transfer objects.

## Handling Optional and Nested Data
Use nullable types (`?`) for properties that may be missing or undocumented in API responses. Always check for null when extracting values from nested DTOs, and document any assumptions or known API behaviors. This approach ensures robustness when dealing with unpredictable or evolving APIs.

## String Properties
For string properties that should never be null, use non-nullable strings and initialize them with `string.Empty`. Do not use `null` or `default` for such properties. This guarantees safety and eliminates the need for null checks throughout the codebase.

## Collections
Initialize collections to empty instances to avoid null checks. Avoid `required` for collections unless explicit assignment is necessary. A default empty value is almost always sufficient and avoids boilerplate.

## Extensibility
Use an extensible property (e.g., `ExtraProperties`) for rarely-used, experimental, or evolving fields. Prefer explicit properties for core, well-documented parameters. This keeps DTOs lean and maintainable while supporting future changes.

## Serialization and Deserialization
Ensure DTOs are compatible with your serializer (e.g., System.Text.Json, Newtonsoft.Json). Use custom converters for complex or polymorphic types if needed. Test serialization and deserialization paths to avoid surprises.

## Security and Data Integrity
Never store sensitive data in extensible or unvalidated properties. Validate and sanitize input where appropriate. Be mindful of data exposure and integrity when designing DTOs.

## Documentation and Conventions
Document DTO design decisions, especially for nullable, required, and extensible properties. Establish conventions for naming, nullability, and initialization. Clear documentation helps maintain consistency and understanding across the codebase.

## Best Practices and Summary
Balance safety, clarity, and productivity. Adapt DTO design to fit the needs of your system and APIs. Refactor as requirements evolve; avoid rigid, one-size-fits-all rules. Let practical experience guide your decisions, and keep DTOs as simple and effective as possible.
