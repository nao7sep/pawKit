# Namespace and Project Structure Specification for pawKitLib

## 1. Overview

This document defines the namespace, folder, and project structure for `pawKitLib`, based on the design conversation and topic breakdown. The goal is to ensure clarity, maintainability, extensibility, and minimalism, while supporting both small and large-scale .NET projects. All recommendations are justified with explicit rationale, and any ambiguities are resolved in favor of explicitness and future-proofing.

---

## 2. Root Namespace and Project

- **Root namespace:** `pawKitLib`
- **Project file:** `pawKitLib.csproj` (single project to start)
- **Target:** .NET 6+ (or latest LTS), cross-platform
- **No dependency bloat:** Core library does not reference ASP.NET Core, Microsoft.Extensions.*, or third-party packages unless strictly necessary
- **Ready for modularization:** Structure supports future splitting into sub-projects or NuGet packages

---

## 3. Folder and Subnamespace Structure

- **One class/enum/struct per file**; file name matches type name
- **Folder structure mirrors namespaces**
- **Subfolders and subnamespaces:**
  - `App` — Application-level helpers and abstractions
  - `Config` — Configuration and settings management
  - `Time` — Time abstraction and time zone handling
  - `Validation` — Validation framework
  - `Ids` — ID generation
  - `Hashing` — Hashing utilities
  - `Files` — Path and file system safety
  - `Text` — String manipulation and formatting
  - `Cli` — Process and command line utilities
  - `Logging` — Logging infrastructure
  - `Localization` — Localization system

**Example:**
```
src/pawKitLib/
  App/
    AppPaths.cs
  Config/
    IAppSettings.cs
    JsonAppSettings.cs
  Time/
    IClock.cs
    SystemClock.cs
  Validation/
    Validator.cs
  Ids/
    IIdGenerator.cs
  Hashing/
    IHashGenerator.cs
  Files/
    SafeFile.cs
    PathGuards.cs
  Text/
    StringLines.cs
    MultilineInputString.cs
  Cli/
    CommandLineInput.cs
  Logging/
    Logger.cs
    ConsoleLog.cs
  Localization/
    ILocalizer.cs
    JsonLocalizer.cs
    FallbackLocalizer.cs
pawKitLib.csproj
```

---

## 4. Naming Conventions

- **Namespaces:** `pawKitLib.[SubNamespace]` (e.g., `pawKitLib.Logging`)
- **Files:** Each type in its own file, named after the type
- **No nested types** unless for private/internal helpers
- **Public types** are always in the matching namespace and file

---

## 5. Modularity and Extensibility

- **Single project to start:** All code in `pawKitLib.csproj` for simplicity
- **Future modularization:**
  - Subfolders can be split into separate projects/NuGet packages as needed (e.g., `pawKitLib.Logging`, `pawKitLib.Config`)
  - No cross-dependencies except where justified (e.g., `Logging` may depend on `Files` for file output)
- **Optional adapters:**
  - ASP.NET Core, Microsoft.Extensions.*, and other integrations live in separate adapter libraries (not in the core)
  - Example: `pawKitLib.Logging.Microsoft` bridges to `ILogger`

---

## 6. Rationale and Best Practices

- **Minimalism:** Only essential features and dependencies are included
- **Explicitness:** All structure and dependencies are visible and intentional
- **Extensibility:** New modules, sources, or adapters can be added without breaking changes
- **Testability:** Each module is independently testable; no hidden dependencies
- **Cross-platform:** All code works on Windows, Linux, macOS, and in Blazor Server
- **No dependency bloat:** Core library is lightweight and portable
- **Fail-fast:** Any environment-dependent features (e.g., base directory, config source) must be explicitly enabled or injected

---

## 7. Usage Guidelines

- **Always use the correct namespace and folder for each type**
- **Never mix unrelated types in the same file**
- **Register adapters and integrations in separate projects**
- **Document rationale for any deviations from this structure**

---

## 8. Example: Adding a New Module

To add a new feature (e.g., a caching system):
1. Create a new subfolder `Caching/` under `src/pawKitLib/`
2. Add interfaces and implementations (e.g., `ICacheProvider.cs`, `MemoryCacheProvider.cs`)
3. Use namespace `pawKitLib.Caching`
4. If integration with external systems is needed, create a separate adapter project (e.g., `pawKitLib.Caching.Redis`)

---

## 9. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability

---

**End of Namespace and Project Structure Specification for pawKitLib**
