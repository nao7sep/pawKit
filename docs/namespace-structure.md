# Namespace Structure

This document describes the organization and rationale for namespaces in this repository.

## Principles
- **Group by Feature/Domain:** Classes are organized by the feature or domain they belong to, not by technical type (e.g., not all helpers or all converters in one place).
- **Cohesion:** Related classes (such as all key-value store types) are grouped together for clarity and maintainability.
- **Scalability:** As the codebase grows, new sub-namespaces or folders can be introduced for sub-features or cross-cutting concerns.

## Current Structure

- `pawKitLib.KeyValueStore`
  - Contains all classes related to the general-purpose key-value store system, such as `KeyValueStore`, `StringValues`, and `StringValuesJsonConverter`.
- `pawKitLib`
  - Contains general-purpose helpers that are not specific to a single feature, such as `StringTypeConverter` and `StringDisplayHelper`.

## Example

```
pawKitLib/
    KeyValueStore/
        KeyValueStore.cs
        StringValues.cs
        StringValuesJsonConverter.cs
    StringTypeConverter.cs
    StringDisplayHelper.cs
```

## Future Guidance
- If a feature area grows large, consider introducing sub-namespaces (e.g., `pawKitLib.KeyValueStore.Conversions`).
- If a utility becomes widely used across features, keep it in the root or move to a `Common` or `Core` namespace.
- Avoid duplicating class names across namespaces unless there is a clear, non-overlapping context.

---
This structure is designed for clarity, maintainability, and ease of navigation for all contributors.
