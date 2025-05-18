# Configuration and Settings Management Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for the **Configuration and Settings Management** system in `pawKitLib`. The goal is to provide a minimal, flexible, and dependency-free configuration mechanism suitable for both small and large .NET applications, with optional integration points for ASP.NET Core and external configuration frameworks.

## 2. Design Philosophy

- **Minimalism**: Only essential features are included; no unnecessary complexity or dependencies.
- **Explicit Enablement**: Configuration must be explicitly loaded or enabled; no ambient or silent defaults.
- **Type Safety and Usability**: Type conversion helpers and sugar methods are provided for ergonomic access.
- **Cross-Platform**: Works identically on Windows, Linux, and macOS.
- **No Dependency Bloat**: Does not depend on `Microsoft.Extensions.Configuration` except optionally in ASP.NET Core.
- **Extensibility**: Optional adapters can bridge to ASP.NET Core’s `IConfiguration` or other frameworks via a separate library.

## 3. Core Features

### 3.1. Configuration Store

- **JSON-Backed**: The primary configuration source is a JSON file (e.g., `settings.json`).
- **Key-Value(s) Model**: Each key maps to a value that may be `null`, a single value, or multiple values (array).
- **Path-Based Keys**: Supports hierarchical keys using path notation (e.g., `"appSettings/defaults/fontFamily"`).
- **No Default Source**: The application must explicitly load or enable the configuration store.

### 3.2. Value Access and Type Conversion

- **Flexible Value Wrapper**: All values are wrapped in a `ConfigValue` (or similar) class that provides:
  - `AsStringOrDefault`, `AsStringsOrDefault`
  - `AsIntOrNull`, `AsIntOrDefault`
  - `AsDoubleOrNull`, `AsDoubleOrDefault`
  - `AsBoolOrNull`, `AsBoolOrDefault`
  - `AsModel<T>()` for deserializing to a model class
- **Null, Single, or Multiple**: Handles all cases gracefully; plural methods always return arrays.
- **Centralized Type Conversion**: All conversions are handled in a single utility class, with optional extension methods for advanced usage (e.g., `.AsIntOrNull()`).

### 3.3. Configuration API

- **Direct Access**: `ConfigStore.Get(path)` returns a `ConfigValue`.
- **No Model Class Required**: For simple use cases, no need to define a model class for every config section.
- **Chained/Fallback Stores**: Support for chaining multiple config sources (e.g., user config, default config).

### 3.4. Interface Abstraction (Optional)

- **IAppSettings**: An interface abstraction for DI scenarios, allowing for:
  - Swappable config sources (JSON, environment, in-memory, etc.)
  - Testability and mocking

### 3.5. Integration with ASP.NET Core

- **Optional Adapter**: In ASP.NET Core, the library can provide an adapter to read from `IConfiguration` if needed.
- **No Core Dependency**: The main config system does not depend on ASP.NET Core or Microsoft.Extensions.Configuration.

### 3.6. Explicit Enablement and Fail-Fast

- **Opt-In Activation**: The config system must be explicitly enabled or loaded; otherwise, all accessors throw an exception.
- **No Silent Fallbacks**: If configuration is not enabled, methods throw or no-op (configurable).

## 4. Advanced/Optional Features

### 4.1. Dynamic Injection

- The config system can be injected via DI in both console and web apps.
- In ASP.NET Core, both the built-in `IConfiguration` and the custom config system can coexist without conflict.

### 4.2. Chained Config Stores

- Support for chaining multiple config sources, with fallback logic (e.g., user config → default config).

### 4.3. Write-Back Support

- Optionally, the config system can write changes back to the JSON file.

### 4.4. Flexible Value Types

- Support for values that may be `null`, a primitive, an array, or a model object.
- Discriminated union pattern for dynamic values (see also Dynamic Data and Flexible Value Wrappers spec).

## 5. Implementation Notes

- **No global state**: All configuration is explicit and must be set before use.
- **No dependency on appsettings.json**: JSON config is optional and must be loaded by the application.
- **No ambient config**: Must be enabled explicitly.
- **No silent fallbacks**: If configuration is not enabled, methods throw or no-op (configurable).
- **One class/enum/struct per file**: File name matches type name.

## 6. Example Usage

```csharp
// Load config from a JSON file
var config = new ConfigStore("settings.json");

// Get a string value (with fallback)
var font = config.Get("appSettings/defaults/fontFamily").AsStringsOrDefault("Arial").First();

// Get a double value (with fallback)
var size = config.Get("appSettings/defaults/fontSize").AsDoubleOrDefault(12);

// Get a model object
var metadata = config.Get("user/profile").AsModel<UserProfile>();

// In ASP.NET Core, inject IAppSettings via DI
public class FontService
{
    private readonly IAppSettings _settings;
    public FontService(IAppSettings settings) { _settings = settings; }
    public string GetFont() =>
        _settings.Get("appSettings/defaults/fontFamily").AsStringsOrDefault("Segoe UI").First();
}
```

## 7. Extensibility and Future-Proofing

- The config system is designed to be extended with new sources, formats, and integration points without breaking existing code.
- All configuration is explicit and testable.
- The config system can be replaced or wrapped for advanced scenarios (e.g., environment-based config, secrets management).

## 8. File and Namespace Structure

- Namespace: `pawKitLib.Config`
- One class/enum/struct per file; file name matches type name.
- All config-related types are in the `Config` subfolder and namespace.

## 9. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of Configuration and Settings Management Specification for pawKitLib**
