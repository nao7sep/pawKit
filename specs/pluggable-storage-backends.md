# Pluggable Storage Backends (SQLite/EF Core) Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **pluggable storage backends** in `pawKitLib`, focusing on optional support for SQLite and Entity Framework Core (EF Core) as storage mechanisms for logs, exceptions, and other structured data. The goal is to provide modular, opt-in storage features without introducing unnecessary dependencies or bloat to the core library.

---

## 2. Design Philosophy

- **Opt-in Modularity**: Storage backends (e.g., SQLite, EF Core) are not referenced by the core library. They are enabled only when explicitly required by the consumer application.
- **No Dependency Bloat**: Applications that do not use these features incur no additional binary size or dependencies.
- **Extensibility**: New storage backends can be added as separate modules or adapter projects.
- **Testability**: Storage features are independently testable and do not affect core functionality.

---

## 3. Structure and Usage

- **Optional Subfolders/Namespaces**: Storage-related code lives in subfolders such as `Logging/Sqlite/`, `Exceptions/Sqlite/`, etc.
- **Default Schema**: Provide a default schema for logs and exceptions, with support for auto-migration.
- **Usage Pattern**:
  - Enable via methods such as `LogSink.UseSqlite(path)` or `ExceptionStore.UseSqlite(path)`
  - Configuration and DI can toggle storage backends on or off
- **No Core Dependency**: The core library does not reference SQLite or EF Core directly; adapters or extension methods are provided in separate projects or folders.

---

## 4. Example

```csharp
// Enable SQLite-backed log storage
LogSink.UseSqlite("logs.db");

// Enable SQLite-backed exception storage
ExceptionStore.UseSqlite("exceptions.db");
```

---

## 5. Rationale and Best Practices

- **Why modular?**
  - Keeps the core library lightweight and portable
  - Allows consumers to choose storage backends as needed
- **Why default schema and auto-migration?**
  - Simplifies setup for common use cases
  - Supports extensibility for advanced scenarios

---

**End of Pluggable Storage Backends (SQLite/EF Core) Specification for pawKitLib**
