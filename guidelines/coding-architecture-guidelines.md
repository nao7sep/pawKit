# pawKit Coding and Architecture Guidelines

This document contains standards and guidelines for coding, design, and architecture in the pawKit repository. All contributors should follow these practices to ensure code quality, maintainability, and clarity, while remaining flexible and pragmatic in the face of changing requirements.

## Architecture
- You **must** use constructor injection for all services and dependencies.
- You **should** avoid service locator patterns and static access to services.
- You **should** favor architectures that are extensible and adaptable, not just theoretically perfect.

## Design
- You **should** ensure each class, method, or component has a single, clear responsibility (Single Responsibility Principle).
- You **should** separate different concerns into distinct classes, methods, or layers (Separation of Concerns).
- You **should** design code to be open for extension but closed for modification (Open/Closed Principle).
- You **should** balance best practices with practical constraints—sometimes clarity and maintainability outweigh strict adherence to patterns or consistency.

## Data Handling
- You **should** use records for immutable data models when appropriate.
- You **should** prefer strongly-typed implementations for core parameters, but allow for extensibility (e.g., dictionaries for extra or evolving API fields) when requirements are unclear or APIs are volatile.
- You **should** avoid unnecessary proliferation of DTOs—favor modular, maintainable models.
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
- You **should** avoid over-engineering and recognize when "good enough" is the most effective solution.

## Code Style
- You **should** follow .NET coding conventions for naming, spacing, and formatting.
- You **should** write clear, beginner-friendly comments that explain the purpose and mechanism of code, focusing on practical intent and real-world usage.
- You **should** document public APIs and significant architectural decisions, especially when deviating from textbook standards for pragmatic reasons.

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

## Pragmatism & Adaptability

* Prioritize solutions that are maintainable, adaptable, and effective in real-world scenarios—even if they deviate from strict consistency or theoretical ideals.
* Be open to refactoring guidelines as new lessons emerge from experience. Continuous improvement is more valuable than static perfection.
