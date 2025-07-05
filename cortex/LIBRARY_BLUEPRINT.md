# pawKit Library Blueprint

This document outlines the technical specifications for the core `pawKitLib` library. It details the planned modules, their corresponding namespaces, and their key public types.

---

### 1. Foundational

This module contains the core building blocks of the library. It has no dependencies on other `pawKitLib` modules.

*   **`pawKitLib.Abstractions`**
    *   `IClock` (interface): Provides an abstraction for getting the current time. This is crucial for testing, as it allows you to control time in your unit tests instead of relying on the non-deterministic `DateTime.UtcNow`.
    *   `IValidator<T>` (interface): Defines a contract for a class that can validate an object of type `T`. This allows for a standardized validation mechanism across the application.
    *   `IUnitOfWork` (interface): Defines a contract for managing atomic database operations. It will typically contain methods like `CommitAsync()` and `RollbackAsync()` to ensure that a series of changes either all succeed or all fail together.
    *   `IRepository<T>` (interface): A generic interface for a data repository, defining standard CRUD (Create, Read, Update, Delete) operations for a given entity `T`. This decouples business logic from the data access technology.

*   **`pawKitLib.Exceptions`**
    *   `ValidationException` (class): A custom exception thrown when an object fails validation. It would typically contain a collection of validation errors.
    *   `ResourceNotFoundException` (class): A custom exception thrown when a specific entity (e.g., a user or product with a given ID) cannot be found in the data store.
    *   `ConfigurationException` (class): A custom exception thrown when a required configuration value is missing or invalid, preventing the application from starting or functioning correctly.

### 2. Data & Persistence

This module contains the concrete implementations for data access and security primitives.

*   **`pawKitLib.Data.Sqlite`**
    *   `PawKitDbContext` (class): The Entity Framework Core `DbContext` for the application. It defines the `DbSet<T>` properties for your entities and configures the database connection (in this case, to SQLite).
    *   `SqliteUnitOfWork` (class): The concrete implementation of `IUnitOfWork` for SQLite, which will manage the `PawKitDbContext` transaction.
    *   `SqliteRepository<T>` (class): The concrete implementation of `IRepository<T>`, using the `PawKitDbContext` to perform database operations against a SQLite database.

*   **`pawKitLib.Security`**
    *   `IPasswordHasher` (interface): An abstraction for hashing and verifying passwords, ensuring the specific hashing algorithm can be changed later without affecting business logic.
    *   `BcryptPasswordHasher` (class): A concrete implementation of `IPasswordHasher` using a strong, industry-standard algorithm like BCrypt.
    *   `IUniqueIdGenerator` (interface): An abstraction for generating unique identifiers.
    *   `GuidIdGenerator` (class): A simple, default implementation of `IUniqueIdGenerator` that returns a new `Guid`.

### 3. Utilities

This module contains static helper classes and extensions for common, reusable tasks.

*   **`pawKitLib.Utils.Core`**
    *   `StringExtensions` (static class): Provides extension methods for the `string` class (e.g., `IsNullOrEmpty`, `Truncate`, `ToTitleCase`).
    *   `PathUtils` (static class): Contains helper methods for safely combining and manipulating file system paths, abstracting away OS-specific differences.
    *   `TypeConverter` (static class): Provides safe methods for converting between different data types, handling potential conversion errors gracefully.

*   **`pawKitLib.Utils.Json`**
    *   `JsonSerializerDefaults` (static class): A central place to hold the default `System.Text.Json.JsonSerializerOptions` used throughout the application, ensuring consistent serialization behavior.

*   **`pawKitLib.Utils.Web`**
    *   `UrlBuilder` (class): A fluent builder class to safely construct URLs with query parameters, handling encoding automatically.

### 4. Application Services

This module contains implementations for application-level concerns like configuration, logging, and communication.

*   **`pawKitLib.Configuration`**
    *   `IConfigManager` (interface): An interface for retrieving strongly-typed application configuration objects (admin-defined).
    *   `ISettingsProvider` (interface): An interface for retrieving and saving user-specific settings, which might be stored in a database or a user-specific file.

*   **`pawKitLib.Logging`**
    *   `ILogger` (interface): The core logging abstraction that services will depend on. It will have methods like `Info`, `Warn`, `Error`.
    *   `ILogSink` (interface): An interface representing a destination for log messages (e.g., the console, a file, a remote service).
    *   `ConsoleSink` (class): An implementation of `ILogSink` that writes formatted log messages to the console.
    *   `FileSink` (class): An implementation of `ILogSink` that writes formatted log messages to a file, handling log rotation.

*   **`pawKitLib.Localization`**
    *   `IStringLocalizer` (interface): An abstraction for retrieving translated strings based on a key.
    *   `JsonLocalizer` (class): An implementation of `IStringLocalizer` that loads translations from JSON files (e.g., `en-US.json`, `es-ES.json`).

*   **`pawKitLib.Communication`**
    *   `IEmailSender` (interface): An abstraction for sending emails.
    *   `ISmsClient` (interface): An abstraction for sending SMS messages via a third-party gateway.

### 5. Console Specific

*   **`pawKitLib.Console`**
    *   `CommandLineParser` (class): A utility to parse `string[] args` from a console application into a structured object.
    *   `ArgumentBuilder` (class): A fluent API to define the expected arguments, options, and commands for a console application.

### 6. Cross-Cutting Services

This module contains implementations of key design patterns that are used across the entire application.

*   **`pawKitLib.Caching`**
    *   `ICacheProvider` (interface): An abstraction for a key-value cache.
    *   `InMemoryCacheProvider` (class): A simple, dictionary-based implementation of `ICacheProvider` for single-instance applications.

*   **`pawKitLib.Storage`**
    *   `IFileStore` (interface): An abstraction for storing and retrieving files (blobs), decoupling the application from the physical storage medium.
    *   `LocalFileStore` (class): An implementation of `IFileStore` that saves files to the local disk.

*   **`pawKitLib.Events`**
    *   `IEvent` (interface): A marker interface for event records. This helps with type constraints.
    *   `OrderPlacedEvent` (record): An example of a specific event record, containing data relevant to the event (e.g., `OrderId`).
    *   `IEventPublisher` (interface): Defines the contract for publishing events to any interested subscribers.
    *   `IEventHandler<TEvent>` (interface): A generic interface that any class wishing to handle an event of type `TEvent` must implement.
    *   `InMemoryEventPublisher` (class): An implementation of `IEventPublisher` that dispatches events to handlers within the same process.

*   **`pawKitLib.Security.Authentication`**
    *   `IAuthenticationService` (interface): An interface for services that handle user login, logout, and identity validation.
    *   `IAuthorizationHandler` (interface): An interface for services that check if an authenticated user has the required permissions to perform a specific action.