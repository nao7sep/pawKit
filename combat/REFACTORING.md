# Refactoring Guidelines

*Quality checks for existing code. These are not suggestions - they are requirements for professional software.*

## Dependency Injection Violations

**Check:** Scan for direct instantiation of external dependencies.

**Red Flags:**
- `new HttpClient()` - Network dependency not injected
- `new Logger()` - Logging dependency not injected
- `File.ReadAllText()` - File system dependency not abstracted
- `new SqlConnection()` - Database dependency not injected

**Fix:** Constructor inject the dependency or its abstraction.

**Anti-Pattern Alert:** "But it's just a small utility method..." No exceptions. Dependencies are dependencies.

## Async/Await Violations

**Check:** Review async patterns for correctness.

**Red Flags:**
- `async void` methods (except event handlers)
- `.Result` or `.Wait()` calls on async methods
- Missing `Async` suffix on async methods
- Missing `CancellationToken` parameters on public async methods

**Fix:**
- `async void` → `async Task`
- `.Result` → `await`
- Add `Async` suffix
- Add `CancellationToken` parameter

**Why:** These aren't style issues - they're correctness bugs waiting to happen.

## Magic String Detection

**Check:** Scan for hardcoded string literals used as keys or identifiers.

**Red Flags:**
- Configuration keys: `Configuration["ConnectionString"]`
- Route names: `[Route("/api/users")]`
- Cache keys: `cache.Get("user_123")`
- Event names: `eventBus.Publish("UserCreated", data)`

**Fix:** Extract to `const string` or `static readonly string` in appropriate static class.

**Anti-Pattern Alert:** "It's just one string..." Today it's one. Tomorrow it's scattered across 20 files with typos.

## Single Responsibility Violations

**Check:** Analyze class responsibilities.

**Red Flags:**
- Class name contains "And", "Or", "Manager", "Helper", "Utility"
- Class has methods that operate on unrelated data
- Class changes for multiple unrelated reasons
- Class has more than 7-10 public methods

**Fix:** Split into focused, single-purpose classes.

**Why:** Classes with multiple responsibilities are harder to test, understand, and modify.

## Code Organization Issues

**Check:** Review class member organization.

**Required Order:**
1. Private/Protected Fields (static then instance)
2. Constructors
3. Public/Internal Properties
4. Public/Internal Methods
5. Private/Protected Methods

**Red Flags:**
- Methods scattered between properties
- Private methods above public methods
- Fields declared after methods

**Fix:** Reorder members according to standard.

## File Organization Violations

**Check:** Verify file structure compliance.

**Red Flags:**
- Multiple public types in one file
- Filename doesn't match public type name
- Public type in wrong namespace for its folder location

**Fix:**
- Split multiple types into separate files
- Rename file to match type name exactly
- Move file to correct folder or fix namespace

**Anti-Pattern Alert:** "It's just a small helper class..." Size doesn't matter. Organization does.

## Documentation Gaps

**Check:** Verify XML documentation completeness.

**Red Flags:**
- Public types without `///` documentation
- Public methods without parameter descriptions
- Complex algorithms without explanation comments
- Outdated comments referencing renamed identifiers

**Fix:** Add complete XML documentation for all public APIs.

## Security Vulnerabilities

**Check:** Scan for common security issues.

**Red Flags:**
- Methods retrieving data by ID without authorization checks (IDOR)
- Sensitive data in log statements
- Unvalidated user input used in queries
- Hardcoded secrets or connection strings

**Fix:** Add authorization checks, sanitize logging, validate inputs, externalize secrets.

**Why:** Security isn't optional. These checks prevent real vulnerabilities.

## Performance Anti-Patterns

**Check:** Identify performance issues.

**Red Flags:**
- N+1 query patterns in loops
- Synchronous I/O in async contexts
- Large objects kept in memory unnecessarily
- Missing caching for expensive operations

**Fix:** Optimize queries, use async properly, dispose resources, add appropriate caching.

## Refactoring Workflow

1. **Run all checks** - Don't cherry-pick easy fixes
2. **Fix violations systematically** - Start with correctness issues (async, DI)
3. **Verify tests still pass** - Refactoring shouldn't change behavior
4. **Update documentation** - Keep comments current with code changes

**Anti-Pattern Alert:** "I'll fix the important stuff later..." Later never comes. Fix it now or accept that your code is unprofessional.