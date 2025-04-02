<!-- 2025-03-30T14:52:54Z -->

# pawKit Project Structure & Naming Conventions

This document outlines the naming conventions and directory structure for the **pawKit** project. The goal is to create a modular, scalable, and developer-friendly solution that serves as a foundation for business-oriented applications.

---

## Overview

**pawKit** is the core library of our ecosystem. It includes common interfaces, utilities, and contracts that are shared across multiple child projects (extensions). These child projects provide specific implementations or additional functionality, such as logging, mailing, or data access.

---

## Naming Conventions

### Core Library
- **Name:** `pawKit.Core`
- **Purpose:** Contains common interfaces, contracts, utilities, and shared business logic.
- **Example Contents:**
  - `IMailService` interface
  - Common helper methods
  - Base models and abstractions

### Child Projects (Extensions)
Each child project is named with the **`pawKit.`** prefix followed by the specific functionality. This creates a clear and modular ecosystem.

| Module           | Suggested Project Name      | Description                              |
|------------------|-----------------------------|------------------------------------------|
| Logging          | `pawKit.Logging`            | Contains wrappers, extension methods, and integrations (e.g., Serilog) for logging. |
| Mail             | `pawKit.MailKit`            | Implements mailing functionality using MailKit/MimeKit. |
| Data Access      | `pawKit.Sqlite`             | Provides lightweight data access (e.g., wrapping sqlite-net or EF Core for SQLite). |
| Sample Apps      | `pawKit.Samples.Console`    | A console application demonstrating usage of pawKit libraries. |
|                  | `pawKit.Samples.Blazor`     | A Blazor Server (or WebAssembly) demo showcasing integration in web apps. |

### General Guidelines
- **Use dots (`.`) to separate the core name from the module.** This is consistent with .NET naming conventions and clearly indicates the relationship.
- **Keep names short, memorable, and brandable.** For instance, `pawKit.Logging` is immediately understood as the logging module for pawKit.
- **Extensions should reflect their functionality.** If more modules are added in the future, the naming should remain consistent (e.g., `pawKit.MailKit`, `pawKit.Auth`, etc.).

---

## Directory Structure

Below is a recommended directory layout for the entire solution:

```plaintext
pawKit/
├── pawKit.sln                  // Solution file at the repository root
├── docs/                       // Documentation (Markdown files, guides, etc.)
│   ├── overview.md
│   ├── usage.md
│   └── architecture.md
├── src/                        // Source code for all projects
│   ├── pawKit.Core/            // Core library (interfaces, base classes, etc.)
│   ├── pawKit.Logging/         // Logging module (Serilog integration and extensions)
│   ├── pawKit.MailKit/         // Mail module (MailKit/MimeKit implementation)
│   ├── pawKit.Sqlite/          // Data access module for SQLite
│   └── pawKit.Samples.Console/ // Sample console application demonstrating usage
│       └── Program.cs
│   └── pawKit.Samples.Blazor/  // Sample Blazor application (if applicable)
├── tests/                      // Unit and integration tests
│   ├── pawKit.Core.Tests/
│   ├── pawKit.Logging.Tests/
│   └── ...
├── .gitignore
├── README.md
└── LICENSE
```

### Key Points:
- **Solution File (`pawKit.sln`):** Located in the repository root for easy discovery and integration with IDEs and build systems.
- **`src/` Folder:** Contains all the source projects, each in its own folder, following the naming conventions.
- **`docs/` Folder:** Stores all Markdown documentation files, such as usage guides, architecture documents, and contribution guidelines.
- **`tests/` Folder:** Contains test projects for unit and integration testing.

---

## Summary

- **Core Library:** `pawKit.Core` provides the base infrastructure.
- **Child Projects:** Use a dot-separated naming convention (e.g., `pawKit.Logging`, `pawKit.MailKit`) to clearly indicate modules/extensions.
- **Directory Structure:** Organize your repository with the solution file at the root, separate folders for source code (`src/`), documentation (`docs/`), and tests (`tests/`).

This structure and naming convention will help maintain consistency, clarity, and scalability as the pawKit ecosystem grows.
