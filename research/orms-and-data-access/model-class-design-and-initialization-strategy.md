<!-- 2025-04-04T05:01:26Z -->

# Model Class Design and Initialization Strategy

This document outlines the rationale and design principles for how model classes are defined and initialized in this project. These guidelines are applicable across different domains and projects where consistency, safety, and maintainability are priorities.

## Overview

Model classes in this codebase are designed to be simple, passive data containers. They do not contain logic for default value initialization beyond what is strictly necessary for safe operation. All meaningful default values and construction logic are delegated to external factory methods.

This approach is intended to:

- Prevent unintended behavior due to misleading default values.
- Make initialization paths explicit and intentional.
- Support multiple use cases and modes for models without hardcoding assumptions.
- Reduce the risk of bugs introduced by partially initialized or misused model instances.

## Collection Properties

### Default Initialization

All collection properties (e.g., `IList<T>`, `IEnumerable<T>`) are initialized to empty collections (`new List<T>()`) within the model class.

Example:

```csharp
public class Product
{
    public IList<string> Tags { get; set; } = new List<string>();
}
```

### Rationale

- Prevents null reference exceptions in consuming code.
- Eliminates the need for null checks before accessing the collection.
- Provides a safe and predictable baseline behavior.

### When to Use Null Instead

Only use `null` for a collection property when the `null` value conveys meaningful semantics, such as:

- The collection is intentionally uninitialized (e.g., lazy-loaded).
- The absence of the collection itself has domain-specific significance.

In such cases, the property should be declared as nullable:

```csharp
public IList<string>? Tags { get; set; }
```

## Default Property Values

### Avoid Meaningful Defaults in Model Classes

Model classes should not assign meaningful or domain-specific default values. For example, avoid the following pattern:

```csharp
public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Not recommended
    public string Name { get; set; } = "Default";  // Not recommended
}
```

### Problems with In-Model Defaults

1. **Incorrect Validity Assumptions**
   Default values can make the model appear valid when it is not fully initialized. For instance, a non-empty GUID or an empty string may bypass validation unintentionally.

2. **Multiple Modes or Contexts**
   Some models are used in multiple contexts (e.g., a "Product" in either "Fruit" or "Vegetable" mode). Each mode may require different defaults or required properties, which cannot be properly enforced by a single in-model constructor or default initializer.

3. **Obscured Initialization Paths**
   It becomes harder to determine how a model was initialized when defaults are baked into the class definition. This can make the behavior unpredictable and difficult to debug.

## Use of Factory Methods

All meaningful initialization of model instances should be done through factory methods. These methods are responsible for setting required properties and ensuring that each instance is valid and correctly configured for its intended use.

### Example

```csharp
public static class ProductFactory
{
    public static Product CreateFruit(string name, decimal sugarContent)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = ProductCategory.Fruit,
            SugarContent = sugarContent,
            Tags = new List<string>()
        };
    }

    public static Product CreateVegetable(string name, string leafType)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = ProductCategory.Vegetable,
            LeafType = leafType,
            Tags = new List<string>()
        };
    }
}
```

### Benefits

- Enforces consistent and complete initialization.
- Encapsulates logic for different construction modes.
- Improves testability and readability.
- Centralizes change when construction logic evolves.

## Design Summary

| Practice | Guideline |
|---------|-----------|
| Collection properties | Initialize to `new List<T>()` in the model class |
| Nullable collections | Use only when `null` has specific meaning |
| Default values | Avoid assigning meaningful defaults inside model classes |
| Construction | Use factory methods to create model instances |
| Validation | Ensure models are validated after creation, not during construction |

## Conclusion

This pattern prioritizes clarity, safety, and adaptability. By avoiding implicit defaults and using factory methods for initialization, we ensure that model instances are always in a known and valid state, aligned with their specific use cases.

Model classes remain simple and reliable data structures, while construction logic is explicit and maintainable.
