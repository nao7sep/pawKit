# Conventions

*Style and organizational rules for consistent, maintainable code. Consistency isn't optional.*

## File Organization - Non-Negotiable Rules

**Rule:** One public type per file. No exceptions.
**Rule:** Filename MUST exactly match the public type name.

**Wrong:**
- `Services.cs` containing `UserService`, `ProductService`, `OrderService`
- `UserService.cs` containing `public class UserManager`

**Right:**
- `UserService.cs` containing `public class UserService`
- `ProductService.cs` containing `public class ProductService`

**Why:** Multiple types per file creates navigation hell. Mismatched names create confusion.

**Anti-Pattern Alert:** "But they're related..." Related doesn't mean they belong in the same file. Organize by responsibility, not convenience.

## Class Member Organization

**Required Order:**
1. Private/Protected Fields (static first, then instance)
2. Constructors
3. Public/Internal Properties
4. Public/Internal Methods
5. Private/Protected Methods

**Wrong:**
```csharp
public class UserService
{
    public async Task<User> GetUserAsync(int id) // Method before constructor

    private readonly ILogger _logger; // Field after method

    public UserService(ILogger logger) // Constructor in wrong place
    {
        _logger = logger;
    }

    public string Name { get; set; } // Property after method
}
```

**Right:**
```csharp
public class UserService
{
    private readonly ILogger _logger; // Fields first

    public UserService(ILogger logger) // Constructor second
    {
        _logger = logger;
    }

    public string Name { get; set; } // Properties third

    public async Task<User> GetUserAsync(int id) // Public methods fourth
    {
        return await GetUserFromDatabaseAsync(id);
    }

    private async Task<User> GetUserFromDatabaseAsync(int id) // Private methods last
    {
        // Implementation
    }
}
```

**Why:** Consistent organization makes code scannable. Readers know where to find what they need.

## Naming Conventions

**Classes:** PascalCase, descriptive nouns
- `UserService`, `ProductRepository`, `OrderValidator`
- NOT: `UserMgr`, `ProdRepo`, `OrderVal`

**Methods:** PascalCase, verb phrases
- `GetUserAsync`, `ValidateOrder`, `CalculateTotal`
- NOT: `GetUser` (for async methods), `DoValidation`, `CalcTotal`

**Properties:** PascalCase, nouns
- `FirstName`, `CreatedAt`, `IsActive`
- NOT: `firstName`, `created_at`, `isActive`

**Fields:** camelCase with underscore prefix
- `_logger`, `_httpClient`, `_connectionString`
- NOT: `logger`, `m_logger`, `Logger`

**Constants:** PascalCase or UPPER_CASE
- `MaxRetryCount` or `MAX_RETRY_COUNT`
- NOT: `maxRetryCount`, `max_retry_count`

**Anti-Pattern Alert:** "Naming doesn't matter, the code works..." Wrong. Code is read 10x more than it's written. Bad names waste everyone's time.

## Temporal Data Conventions

**Rule:** All `DateTime` or `DateTimeOffset` properties and variables MUST be in Coordinated Universal Time (UTC).
**Rule:** Property and variable names for temporal data MUST end with a `Utc` suffix.

**Wrong:**
- `public DateTimeOffset CreatedAt { get; init; }`

**Right:**
- `public DateTimeOffset CreatedAtUtc { get; init; }`

**Why:** Timezone ambiguity is a primary source of bugs. Enforcing UTC and explicit naming eliminates this entire class of problems. Local time has no place on the server.

## Documentation Requirements

**Rule:** All public and internal types MUST have XML documentation.
**Rule:** All public methods MUST document parameters and return values.
**Rule:** Complex algorithms MUST have explanation comments.

**Wrong:**
```csharp
public class UserService
{
    public async Task<User> GetUserAsync(int id)
    {
        // Some complex logic here
        return user;
    }
}
```

**Right:**
```csharp
/// <summary>
/// Provides user management operations with caching and validation.
/// </summary>
public class UserService
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique user identifier.</param>
    /// <returns>The user if found, null otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when id is less than 1.</exception>
    public async Task<User?> GetUserAsync(int id)
    {
        // Check cache first to avoid database hit
        // Fall back to database if not cached
        // Update cache with result for future requests
        return user;
    }
}
```

## Namespace Organization

**Rule:** Namespace MUST reflect folder structure.
**Rule:** Use company/project prefix to avoid conflicts.

**Structure:**
```
pawKitLib/
├── Abstractions/     → pawKitLib.Abstractions
├── Services/         → pawKitLib.Services
├── Data/            → pawKitLib.Data
└── Utils/           → pawKitLib.Utils
    └── Extensions/   → pawKitLib.Utils.Extensions
```

**Anti-Pattern Alert:** Namespace doesn't match folder? You're creating navigation confusion.

## Error Messages

**Rule:** Error messages MUST be actionable and specific.
**Rule:** Include context that helps debugging.

**Wrong:**
```csharp
throw new Exception("Error occurred");
throw new ArgumentException("Invalid input");
```

**Right:**
```csharp
throw new InvalidOperationException($"User {userId} cannot be deleted because they have active orders");
throw new ArgumentException($"User ID must be greater than 0, but was {userId}", nameof(userId));
```

**Why:** Generic error messages waste debugging time. Specific messages with context enable quick fixes.

## Test Organization

**Rule:** Test classes MUST mirror the structure of code under test.
**Rule:** Test methods MUST follow Given_When_Then or similar naming.

**Structure:**
```
pawKitLib/Services/UserService.cs
→ pawKitLib.Tests/Services/UserServiceTests.cs
```

**Method Naming:**
```csharp
[Fact]
public async Task GetUserAsync_WhenUserExists_ReturnsUser()

[Fact]
public async Task GetUserAsync_WhenUserNotFound_ReturnsNull()

[Fact]
public async Task GetUserAsync_WhenIdIsZero_ThrowsArgumentException()
```

**Anti-Pattern Alert:** Tests scattered randomly? You're making maintenance harder. Tests in wrong namespace? You're breaking discoverability.