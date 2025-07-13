# pawKit Coding and Architecture Guidelines

This document contains standards and guidelines for coding, design, and architecture in the pawKit repository. All contributors should follow these practices to ensure code quality, maintainability, and consistency.

## Architecture
- You **must** use constructor injection for all services and dependencies.
- You **should** avoid service locator patterns and static access to services.

## Design
- You **should** ensure each class, method, or component has a single, clear responsibility (Single Responsibility Principle).
- You **should** separate different concerns into distinct classes, methods, or layers (Separation of Concerns).
- You **should** design code to be open for extension but closed for modification (Open/Closed Principle).

## Data Handling
- You **should** use records for immutable data models when appropriate.
- You **should** prefer strongly-typed implementations while avoiding unnecessary proliferation of DTOs.
- You **should** use immutable collections (e.g., `ImmutableList<T>`, `ImmutableDictionary<TKey, TValue>`) instead of `IReadOnlyList<T>` or `IReadOnlyDictionary<TKey, TValue>` when practical.
- You **should** use async code and APIs when they improve effectiveness, scalability, or quality.

## File Organization
- You **must** name files and folders clearly and descriptively.
- You **should** organize related files by feature or layer (e.g., Data, Pages, Shared).
- You **should** define only one type (class, record, interface, etc.) per file, unless types are tightly related (e.g., private nested types).

## Code Organization
- You **should** order members as follows: private/protected fields (static first, then instance), constructors, public/internal properties, public/internal methods, private/protected methods.
- You **must** place new code, members, and elements in the most appropriate location within a file or type, not simply append them to the end.
- You **must** refactor or reorganize existing code when it improves quality, readability, simplicity, or efficiency.

## Code Style
- You **should** follow .NET coding conventions for naming, spacing, and formatting.
- You **should** write clear, beginner-friendly comments that explain the purpose and mechanism of code.
- You **should** document public APIs and significant architectural decisions.

## Structured Logging
- All log messages **must** use structured logging.
- You **must** use message templates with named placeholders (e.g., `{UserId}`, `{RequestId}`) for all variable data.
- You **must not** use string concatenation or interpolation in log messages.

## Exception Handling
- You **must** log exceptions with structured logging and include relevant context.
- You **must not** swallow exceptions silently.
- You **should** catch exceptions only where you can handle them meaningfully.

## Security
- You **must** always use antiforgery protection on pages that accept user input.
- You **must never** log sensitive data (passwords, secrets, etc.).
