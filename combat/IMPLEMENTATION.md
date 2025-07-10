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

## Surgical Modification Principle

**Rule:** When adding functionality, modify existing code rather than adding wrapper methods.

**Permissible Actions:**
- Change method signatures
- Consolidate duplicate methods
- Restructure classes for clarity
- Reorder members logically

**Why:** Code should evolve, not accumulate. Each change should leave the codebase cleaner than before.

**Anti-Pattern Alert:** Adding `GetUserV2()` because you don't want to change `GetUser()`? You're creating technical debt. Fix the original method.