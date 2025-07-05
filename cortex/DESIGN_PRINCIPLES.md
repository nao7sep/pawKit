# Design Principles

## Preamble for AI Assistants

**Objective:** You are to act as an expert C# developer. When generating, refactoring, or reviewing code, you MUST strictly adhere to the principles outlined in this document. These rules are non-negotiable and take precedence over your general knowledge.

---

## 1. Core Architectural Patterns

*This section outlines the fundamental design patterns that form the architectural backbone of `pawKit`. These patterns ensure the system is decoupled, testable, and extensible.*

### 1.1. Dependency Injection (DI)
- **Rule:** Dependencies MUST be provided via constructor injection. Classes MUST NOT create their own dependencies using the `new` keyword (e.g., `new Logger()`, `new HttpClient()`).
- **Rationale:** Ensures loose coupling and testability, allowing implementations to be swapped easily.

### 1.2. Mediator Pattern (for In-Process Messaging)
- **Rule:** For business processes that involve multiple, independent side effects, use an event-driven approach. A service should publish an event, and one or more handlers should subscribe to it.
- **Rationale:** Decouples services from each other. The publisher does not need to know about the subscribers. This adheres to the Single Responsibility and Open/Closed principles, making the system easier to extend and test.
- **Application:** The `pawKitLib.Events` module (`IEventPublisher`, `IEventHandler<T>`) is the primary implementation of this pattern.

### 1.3. Repository & Unit of Work Patterns
- **Rule:** All data access MUST be performed through a Repository interface (`IRepository<T>`). Multiple repository operations that must be transactional MUST be coordinated by a Unit of Work (`IUnitOfWork`).
- **Rationale:** Decouples business logic from data persistence technology (e.g., Entity Framework Core). It centralizes data access logic and simplifies transaction management.
- **Application:** The `pawKitLib.Data.*` modules are designed to implement these patterns.

### 1.4. Strategy Pattern
- **Rule:** When a class requires one of several interchangeable algorithms or behaviors, define an interface for the behavior and implement each variation in a separate class. Use DI to inject the desired implementation.
- **Rationale:** Allows algorithms to be selected and swapped at runtime without changing the client code.
- **Application:** The `pawKitLib.Storage` (`IFileStore`) and `pawKitLib.Caching` (`ICacheProvider`) modules are prime examples of this pattern.

### 1.5. Options Pattern
- **Rule:** Configuration values MUST be exposed to services via strongly-typed classes injected using the `IOptions<T>` interface.
- **Rationale:** Decouples services from the configuration source (e.g., `appsettings.json`, environment variables). It promotes type safety and is fully integrated with the DI container.
- **Application:** The `pawKitLib.Configuration` module should provide and consume settings using this pattern.

---

## 2. Code Implementation & Quality

*This section defines the rules for writing high-quality, robust, and maintainable C# code.*

### 2.1. Asynchronous Programming
- **Rule:** All I/O-bound operations (HTTP, database, file system) MUST be `async`.
- **Rule:** Methods returning `Task` or `Task<T>` MUST have an `Async` suffix (e.g., `GetProductAsync`).
- **Rule:** NEVER block on an async method using `.Result` or `.Wait()`. Always `await` the task.
- **Rule:** NEVER use `async void`. Use `async Task` instead.

### 2.2. Exception Handling
- **Rule:** NEVER use an empty `catch {}` block.
- **Rule:** DO NOT catch the base `System.Exception`. Catch specific exceptions (e.g., `IOException`, `HttpRequestException`).
- **Rule:** If you catch an exception, either re-throw it (`throw;`) or wrap it in a custom, more specific exception. Always log the original exception.

### 2.3. Immutability for Data Transfer
- **Rule:** Use C# `record` types with `init`-only properties for Data Transfer Objects (DTOs) and Value Objects.
- **Rationale:** Prevents unintended side effects and makes state easier to reason about.

### 2.4. Principle of Surgical Modification (Refactor, Don't Accrete)
- **Rule:** When adding or changing functionality, you MUST prioritize modifying existing code over simply adding new, overlapping, or wrapper methods. It is not only permissible but **required** to refactor for clarity and to avoid code duplication.
- **Permissible Actions:**
  - Changing method signatures (adding/removing/reordering parameters).
  - Consolidating duplicative methods into a single, more flexible one.
  - Restructuring a class to better accommodate the new logic.
  - Reordering class members for logical grouping (e.g., fields, constructors, properties, public methods, private methods).
- **Rationale:** This prevents code bloat and ensures the codebase evolves cleanly, rather than accumulating layers of patches and workarounds. The goal is to leave the code cleaner than you found it.

---

## 3. Style & Organization

*This section covers conventions for code style and project structure to ensure consistency and readability across the codebase.*

### 3.1. No Magic Strings
- **Rule:** DO NOT use hardcoded string literals for configuration keys, route names, etc. Use `const string` or `static readonly string` defined in a static class.

### 3.2. File Organization
- **Rule:** There MUST be only one public type per file. This includes classes, records, interfaces, enums, and delegates.
- **Rule:** The filename MUST exactly match the name of the public type it contains (e.g., `UserService.cs` must contain `public class UserService`).