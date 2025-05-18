# Localization Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for the **Localization** subsystem in `pawKitLib`. The goal is to provide a robust, extensible, and testable set of APIs for localizing strings and resources, supporting dynamic language switching, fallback strategies, and seamless integration with both web and desktop applications. The design is based on the detailed design conversation and the `topics-and-details.md` topic list. All ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Minimalism**: Only essential features are included; no unnecessary complexity or dependencies.
- **Explicitness**: All localization sources and fallbacks are explicit; no silent or ambiguous defaults.
- **Extensibility**: New sources, formats, and fallback strategies can be added without breaking changes.
- **Testability**: All localizers are deterministic and unit-testable.
- **Cross-Platform**: Works identically on Windows, Linux, macOS, and in Blazor Server.
- **No Dependency Bloat**: Does not depend on ASP.NET Core or Microsoft.Extensions.Localization in the core.

---

## 3. Core Interfaces and Classes

### 3.1. `ILocalizer`
- **Purpose**: Abstraction for all localization strategies.
- **API**:
  - `string this[string key] { get; }` — gets the localized string for the given key.
  - `string Get(string key, params object[] args)` — gets the localized string and formats it with arguments.
  - `bool TryGet(string key, out string value)` — attempts to get the localized string.
  - `IReadOnlyDictionary<string, string> All { get; }` — all available localizations for the current language.
  - `string Language { get; }` — the current language code (e.g., "en", "ja").
- **Usage**: All localization is performed via `ILocalizer` implementations.

### 3.2. `JsonLocalizer`
- **Purpose**: Loads localizations from JSON files (one per language).
- **Features**:
  - Loads from a directory or embedded resource.
  - Supports language fallback (e.g., "en-US" → "en" → default).
  - Reloadable at runtime (optional).
  - Thread-safe.
- **File Format**:
  - Each language is a separate JSON file: `en.json`, `ja.json`, etc.
  - Flat key-value pairs: `{ "Hello": "Hello", "Goodbye": "Goodbye" }`

### 3.3. `FallbackLocalizer`
- **Purpose**: Chains multiple `ILocalizer` instances for fallback.
- **Features**:
  - Tries each localizer in order until a value is found.
  - Supports chaining (e.g., per-module, per-app, global fallback).

### 3.4. Dynamic Language Switching
- **Features**:
  - Language can be changed at runtime (per-request, per-session, or globally).
  - In Blazor Server, supports per-user/session language context.
  - In ASP.NET Core, can be scoped per request (via DI or middleware).
  - Thread-safe and context-aware.

---

## 4. Supported Scenarios and Use Cases

### 4.1. Web Applications (ASP.NET Core, Blazor Server)
- **Integration**:
  - Can be injected via DI (`@inject ILocalizer L` in Razor).
  - Supports per-request or per-session language selection.
  - Works with both server-side and client-side Blazor (where possible).

### 4.2. Desktop Applications
- **Integration**:
  - Localizer can be set globally or per-viewmodel.
  - Supports dynamic language switching at runtime.

### 4.3. Fallback and Chaining
- **Use Case**:
  - Module-specific localizations can fall back to app-wide or global localizations.
  - If a key is missing in the primary localizer, the fallback(s) are queried in order.

---

## 5. API Patterns

### 5.1. Centralized Access
- All localizers are accessible via static properties or a registry (e.g., `Localizers.Default`, `Localizers.For("ja")`).
- New localizers can be registered for custom sources or fallback chains.

### 5.2. Explicit Language Selection
- All APIs require the caller to specify or select the intended language; no silent fallback to system locale unless explicitly configured.
- Attempting to use a localizer for an unsupported language should throw or log a warning.

### 5.3. Output Formats
- All localizers return output as `string`.
- Formatting with arguments is explicit and uses standard .NET string formatting.

---

## 6. Example Usage

```csharp
// Load a localizer for Japanese
var localizer = Localizers.For("ja");

// Get a localized string
var hello = localizer["Hello"];

// Get a formatted localized string
var greeting = localizer.Get("WelcomeUser", userName);

// Fallback chain
var fallback = new FallbackLocalizer(moduleLocalizer, appLocalizer, globalLocalizer);
var value = fallback["SomeKey"];

// In Razor (Blazor/ASP.NET Core)
@inject ILocalizer L
<h1>@L["Title"]</h1>
```

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Localization`
- One class/enum/struct per file, file name matches type name
- All localization-related classes in the `Localization` subfolder and namespace

---

## 8. Usage Guidelines

- **Always use the correct localizer for your context (per-request, per-session, or global).**
- **Never assume a system locale unless explicitly configured.**
- **Register custom localizers for new sources or fallback requirements.**
- **Document the rationale for language and fallback choices in code and specs.**

---

## 9. Rationale and Best Practices

- **Why explicit fallback and chaining?**
  - Prevents accidental missing translations and makes intent explicit.
- **Why JSON-based localizers?**
  - Easy to edit, diff, and version; widely supported.
- **Why dynamic language switching?**
  - Supports modern UX requirements (per-user, per-session, live switching).
- **Why extensibility?**
  - Allows for future sources (e.g., database, remote service) and compliance with evolving best practices.
- **Why minimalism?**
  - Reduces attack surface and maintenance burden; only high-value features are included by default.
- **Why testability?**
  - All localizers are deterministic and have test vectors; easy to verify correctness.

---

## 10. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of Localization Specification for pawKitLib**
