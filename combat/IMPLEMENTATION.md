# Implementation Guidelines

*Rules for building new functionality. These are not suggestions - they are requirements.*

## Community Standards - No Exceptions

When implementing new functionality, use these libraries. Period.

- **Dependency Injection:** Microsoft.Extensions.DependencyInjection
- **Configuration:** Microsoft.Extensions.Configuration
- **Logging:** Microsoft.Extensions.Logging
- **Validation:** FluentValidation
- **Mediator Pattern:** MediatR
- **ORM:** Entity Framework Core
- **Caching:** Microsoft.Extensions.Caching.Memory
- **JSON:** System.Text.Json (Newtonsoft.Json only for legacy compatibility)
- **Testing:** xUnit + Moq
- **HTTP Client:** Microsoft.Extensions.Http (HttpClientFactory)
    - Flurl may be used as an HTTP client abstraction, but only if all Flurl HTTP calls are configured to use an `HttpClient` instance provided by `HttpClientFactory`.
- **Authentication:** Microsoft.AspNetCore.Authentication.JwtBearer
- **Database:** SQLite for embedded scenarios

**Anti-Pattern Alert:** "But I found this cool new library..." Stop. Use the standard. Your creativity belongs in business logic, not infrastructure choices.

## Asynchronous Programming - Get It Right

**Rule:** All I/O operations MUST be async.
**Rule:** Async methods MUST have `Async` suffix.
**Rule:** Public async methods MUST accept `CancellationToken`.

**Wrong:**
```csharp
public string GetUser(int id)
{
    return httpClient.GetStringAsync($"/users/{id}").Result; // Deadlock waiting to happen
}

public async void SaveUser(User user) // async void is evil
{
    await repository.SaveAsync(user);
}
```

**Right:**
```csharp
public async Task<string> GetUserAsync(int id, CancellationToken cancellationToken = default)
{
    return await httpClient.GetStringAsync($"/users/{id}", cancellationToken);
}

public async Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
{
    await repository.SaveAsync(user, cancellationToken);
}
```

**Why:** `.Result` and `.Wait()` cause deadlocks. `async void` swallows exceptions. These aren't style preferences - they're correctness issues.

## Exception Handling - No Lazy Catches

**Rule:** NEVER use empty catch blocks.
**Rule:** Catch specific exceptions, not `System.Exception`.
**Rule:** Log before re-throwing.

**Wrong:**
```csharp
try
{
    await SomeOperation();
}
catch
{
    // Silent failure is cowardice
}

try
{
    await SomeOperation();
}
catch (Exception ex) // Too broad
{
    throw new CustomException("Something went wrong");
}
```

**Right:**
```csharp
try
{
    await SomeOperation();
}
catch (HttpRequestException ex)
{
    logger.LogError(ex, "HTTP request failed for operation {Operation}", nameof(SomeOperation));
    throw new ServiceUnavailableException("External service unavailable", ex);
}
```

## Data Transfer Objects - Immutability Rules

**Rule:** Use `record` types with `init`-only properties for DTOs.
**Rule:** Required properties use `required` keyword.
**Rule:** Optional properties are nullable.
**Rule:** Collections are never null, always initialized.

**Wrong:**
```csharp
public class UserDto
{
    public int Id { get; set; } // Mutable
    public string Name { get; set; } // Nullable when it shouldn't be
    public List<string> Roles { get; set; } // Can be null
}
```

**Right:**
```csharp
public record UserDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? MiddleName { get; init; } // Explicitly optional
    public IReadOnlyList<string> Roles { get; init; } = []; // Never null
}
```

**Why:** Immutable objects prevent side effects. Required properties prevent invalid state. Initialized collections prevent null reference exceptions.

## Type Selection by Purpose

**`record`:** Immutable data snapshots (DTOs, Events, Value Objects)
**`class`:** Services with behavior and identity
**`interface`:** Public contracts (default choice for abstraction)
**`struct`:** Small, simple value types with no identity
**`abstract class`:** Shared implementation details (use sparingly)

**Anti-Pattern Alert:** Using `class` for everything because it's familiar? You're ignoring better tools. Using `abstract class` for contracts? You're limiting extensibility.

### State vs. Behavior - The Core Distinction

The choice between `record` and `class` is not arbitrary. It is a deliberate architectural decision based on a fundamental principle: the separation of state and behavior.

**Use `record` for State:**
- **What it is:** An immutable snapshot of data at a point in time.
- **Examples:** Data Transfer Objects (DTOs), Events, Messages, Value Objects.
- **Characteristics:** Has no dependencies, no complex logic, and its identity is defined by its data. Two `UserDto` objects with the same `Id` are conceptually the same.
- **Why:** Immutability prevents side effects, makes state easy to reason about, and is inherently thread-safe. This is non-negotiable for data that flows through your system.

**Use `class` for Behavior:**
- **What it is:** A component that performs actions and manages processes over time.
- **Examples:** Services (`UserService`), Repositories (`ProductRepository`), Controllers, long-lived application components.
- **Characteristics:** Has dependencies (injected via constructor), contains methods that orchestrate logic, and has a distinct identity and lifecycle managed by the DI container. Two instances of `UserService` are not interchangeable.
- **Why:** Services are inherently stateful (in terms of their dependencies and lifecycle, not their data) and exist to cause controlled side effects. Their mutability is a feature, managed by the container.

**Anti-Pattern Alert:** Adding complex methods and dependencies to a `record`? You've created a schizophrenic object that's neither a good DTO nor a good service. Putting complex state inside a service that should be passed in via method calls? You've created a stateful service that's a nightmare to test and reason about.

## Surgical Modification Principle

**Rule:** When adding functionality, modify existing code rather than adding wrapper methods.

**Permissible Actions:**
- Change method signatures
- Consolidate duplicate methods
- Restructure classes for clarity
- Reorder members logically

**Why:** Code should evolve, not accumulate. Each change should leave the codebase cleaner than before.

**Anti-Pattern Alert:** Adding `GetUserV2()` because you don't want to change `GetUser()`? You're creating technical debt. Fix the original method.