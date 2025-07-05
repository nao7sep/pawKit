# pawKit Library Blueprint

This document outlines the technical specifications for the core `pawKitLib` library. It is a living document that details the planned modules, their corresponding namespaces, and their key public types.

---

### 1. Foundational

This module contains the core building blocks and abstractions of the library. It has minimal dependencies on other `pawKitLib` modules.

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

*   **`pawKitLib.Data.Sqlite`** (Example Implementation)
    *   `SqliteUnitOfWork` (class): The concrete implementation of `IUnitOfWork` for SQLite.
    *   `SqliteRepository<T>` (class): The concrete implementation of `IRepository<T>` for SQLite.
    *   `SqliteKeyValueStore` (class): A persistent key-value store implementation of `ICacheProvider` using a simple two-column SQLite table.

*   **`pawKitLib.Security`**
    *   `IPasswordHasher` (interface): An abstraction for hashing and verifying passwords, ensuring the specific hashing algorithm can be changed later without affecting business logic.
    *   `BcryptPasswordHasher` (class): A concrete implementation of `IPasswordHasher` using a strong, industry-standard algorithm like BCrypt.
    *   `IUniqueIdGenerator` (interface): An abstraction for generating unique identifiers.
    *   `GuidIdGenerator` (class): A simple, default implementation of `IUniqueIdGenerator` that returns a new `Guid`.
    *   `IBlobHasher` (interface): An abstraction for hashing a stream of data (a blob) asynchronously.
    *   `Sha256BlobHasher` (class): A concrete implementation of `IBlobHasher` that reads a stream in chunks and computes a SHA256 hash.

### 3. Utilities

This module contains static helper classes and extensions for common, reusable tasks.

*   **`pawKitLib.Utils.Core`**
    *   `StringExtensions` (static class): Provides extension methods for the `string` class (e.g., `IsNullOrEmpty`, `Truncate`, `ToTitleCase`).
    *   `PathUtils` (static class): Contains helper methods for safely combining and manipulating file system paths, abstracting away OS-specific differences.
    *   `TypeConverter` (static class): Provides safe methods for converting between different data types, handling potential conversion errors gracefully.
    *   `Result<TSuccess, TError>` (record): A generic type to represent the outcome of an operation that can either succeed with a value or fail with an error, avoiding exceptions for control flow.
    *   `StateMachine<TState, TTrigger>` (class): A generic finite state machine for managing object lifecycles.

*   **`pawKitLib.Utils.Json`**
    *   `JsonSerializerDefaults` (static class): A central place to hold the default `System.Text.Json.JsonSerializerOptions` used throughout the application, ensuring consistent serialization behavior.

*   **`pawKitLib.Utils.Web`**
    *   `UrlBuilder` (class): A fluent builder class to safely construct URLs with query parameters, handling encoding automatically.

### 4. Application Services

This module contains implementations for application-level concerns like configuration, logging, and communication.

*   **`pawKitLib.Configuration`**
    *   `IConfigManager` (interface): An interface for retrieving strongly-typed application configuration objects (admin-defined).
    *   `ISettingsProvider` (interface): An interface for retrieving and saving read/write settings (user-defined).
    *   `JsonFileSettingsProvider` (class): An implementation of `ISettingsProvider` that reads and writes settings to a specified JSON file.

*   **`pawKitLib.Logging`**
    *   `ILogger` (interface): The core logging abstraction that services will depend on. It will have methods like `Info`, `Warn`, `Error`.
    *   `ILogSink` (interface): An interface representing a destination for log messages (e.g., the console, a file, a remote service).
    *   `LogEntry` (record): A POCO representing a structured log event, used to pass data to sinks.
    *   `ConsoleSink` (class): An implementation of `ILogSink` that writes formatted log messages to the console.
    *   `FileSink` (class): An implementation of `ILogSink` that writes formatted log messages to a file, handling log rotation.
    *   `JsonFileSink` (class): An implementation of `ILogSink` that serializes `LogEntry` objects to a JSON file.
    *   `SqliteLogSink` (class): An implementation of `ILogSink` that writes `LogEntry` objects to a SQLite database.

*   **`pawKitLib.Localization`**
    *   `IStringLocalizer` (interface): An abstraction for retrieving translated and formatted strings based on a key, with support for fallback cultures.
    *   `JsonLocalizer` (class): An implementation of `IStringLocalizer` that loads translations from JSON files (e.g., `en-US.json`, `es-ES.json`).

*   **`pawKitLib.Communication`**
    *   `IEmailSender` (interface): An abstraction for sending emails.
    *   `ISmsClient` (interface): An abstraction for sending SMS messages via a third-party gateway.

### 5. Console Specific

*   **`pawKitLib.Console`**
    *   `CommandLineParser` (class): A utility to parse `string[] args` from a console application into a structured object.
    *   `ArgumentBuilder` (class): A fluent API to define the expected arguments, options, and commands for a console application.

### 6. AI Services

*This module is designed with a hybrid model to handle volatile external API contracts. Core, stable data is mapped to strongly-typed properties, while unexpected or changing data is captured in a dynamic dictionary using `[JsonExtensionData]` to prevent deserialization errors.*

*   **`pawKitLib.AI`**
    *   `IChatClient` (interface): A provider-agnostic interface for interacting with a multi-modal chat AI.
    *   `ChatRequest` / `ChatMessage` (records): Strongly-typed models for *sending* data to the AI, as this is a contract we control.
    *   `ChatResponse` (record): A hybrid model for *receiving* data. It will have strongly-typed properties for stable fields (`Content`, `Error`, `Usage`) and an extension data dictionary to capture all other fields, ensuring forward compatibility.
    *   `OpenAiChatClient` (class): A concrete implementation of `IChatClient` for the OpenAI API.
    *   (Other clients like `AnthropicChatClient`, `GeminiChatClient` would also be implemented here).

### 7. Cross-Cutting Services

This module contains implementations of key design patterns that are used across the entire application.

*   **`pawKitLib.Caching`**
    *   `ICacheProvider` (interface): An abstraction for a key-value cache.
    *   `InMemoryCacheProvider` (class): A simple, dictionary-based implementation of `ICacheProvider` for single-instance applications.

*   **`pawKitLib.Storage`**
    *   `IFileStore` (interface): An abstraction for storing and retrieving files (blobs), with convenience methods like `WriteAllTextAsync`.
    *   `LocalFileStore` (class): An implementation of `IFileStore` that saves files to the local disk.

*   **`pawKitLib.Events`**
    *   `IEvent` (interface): A marker interface for event records. This helps with type constraints.
    *   `OrderPlacedEvent` (record): An example of a specific event record, containing data relevant to the event (e.g., `OrderId`).
    *   `IEventPublisher` (interface): Defines the contract for publishing events to any interested subscribers.
    *   `IEventHandler<TEvent>` (interface): A generic interface that any class wishing to handle an event of type `TEvent` must implement.
    *   `InMemoryEventPublisher` (class): An implementation of `IEventPublisher` that dispatches events to handlers within the same process.

*   **`pawKitLib.Security.Authentication`**
    *   `IAuthenticationService` (interface): An interface for services that handle user login, logout, and identity validation.
    *   `IUserStore` (interface): An abstraction for the storage and retrieval of user identity information.
    *   `DatabaseUserStore` (class): An implementation of `IUserStore` that uses an `IRepository` to interact with a database.
    *   `IAuthorizationHandler` (interface): An interface for services that check if an authenticated user has the required permissions to perform a specific action.