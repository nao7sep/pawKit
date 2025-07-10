# Foundations

*Immutable design principles that govern all pawKit code. Violation of these principles is non-negotiable failure.*

## Core Philosophy

**Single Responsibility Principle (SRP):** A class should have only one reason to change. If you can't explain what your class does in one sentence, you've failed.

**Open/Closed Principle (OCP):** Software entities should be open for extension, but closed for modification. Modify existing code to add features? You're doing it wrong.

**Dependency Inversion Principle (DIP):** High-level modules should not depend on low-level modules. Both should depend on abstractions. Direct instantiation is architectural failure.

**Separation of Concerns (SoC):** The application is divided into distinct sections with minimal overlap. Mixing concerns is lazy thinking.

## Dependency Injection - Non-Negotiable

**Rule:** Dependencies MUST be provided via constructor injection. Classes MUST NOT create their own dependencies.

**Wrong:**
```csharp
public class UserService
{
    private readonly HttpClient _client = new HttpClient(); // Failure
    private readonly ILogger _logger = LoggerFactory.Create(); // Failure
}
```

**Right:**
```csharp
public class UserService
{
    public UserService(HttpClient client, ILogger<UserService> logger)
    {
        _client = client;
        _logger = logger;
    }
}
```

**Why:** Your class creating its own dependencies is architectural suicide. It kills testability, violates SRP, and creates hidden coupling.

## Abstraction Rules

**For Persistent Resources (file, database, network):** Always inject an abstraction. Never instantiate directly.
- `IFileStore`, not `File.ReadAllText()`
- `DbContext`, not `new SqlConnection()`
- `HttpClient` (injected), not `new HttpClient()`

**For .NET Primitives:** Use directly unless you have a real need for abstraction.
- `DateTime.UtcNow` - acceptable
- `Guid.NewGuid()` - acceptable
- `Random.Shared` - acceptable

**Anti-Pattern Alert:** Creating `IClock` interface just because you read about it somewhere? You're cargo-culting patterns you don't understand. Only abstract when you have actual need for swappability or testability, not because some blog post told you to.

## Mediator Pattern

**Rule:** For business processes with multiple independent side effects, use event-driven approach via MediatR.

**Why:** Publisher doesn't need to know about subscribers. This is decoupling done right, not the fake decoupling you get from repository patterns.

**Wrong:** Service calling multiple other services directly.
**Right:** Service publishes event, handlers respond independently.

## Data Access

**Rule:** Entity Framework Core is the default. Repository pattern is usually unnecessary abstraction.

**Anti-Pattern Alert:** "But what about testability?" EF Core has in-memory provider. "But what about multiple data stores?" You probably don't need that complexity. Stop solving imaginary problems and focus on real ones.

## Configuration

**Rule:** Configuration values MUST be exposed via strongly-typed classes using `IOptions<T>`.

**Wrong:** `Configuration["ConnectionString"]` scattered throughout code.
**Right:** `IOptions<DatabaseOptions>` injected with strongly-typed properties.

**Why:** Type safety, intellisense, and centralized configuration validation. String-based configuration access is amateur hour.