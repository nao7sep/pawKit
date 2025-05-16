# Documentation Management

This document describes how documentation is managed in this repository.

## Principles
- **Project-Specific:** All documentation relevant to this repository is kept within the repository itself, under the `docs/` directory.
- **Versioned with Code:** Documentation is versioned alongside the code, ensuring that docs and code changes stay in sync.
- **Discoverability:** The `docs/` directory is at the root of the repository for easy access by all contributors.

## Structure
- `docs/` contains all markdown files related to development, architecture, usage, and design decisions for this repository.
- Each document should focus on a single topic (e.g., `namespace-structure.md`, `settings-system.md`, `conventions.md`).
- Reference important documents from the `README.md` as needed.

## When to Add or Update Docs
- When introducing a new feature or architectural pattern.
- When making a design decision that may not be obvious to future contributors.
- When updating or deprecating existing features.

## Not for General/Shared Docs
- Documentation that is only relevant to this repository should be kept here.
- If you have documentation that is shared across multiple unrelated projects, consider a separate documentation repository or site.

## Best Practices
- Use clear, descriptive filenames and headings.
- Keep documents concise and focused.
- Update documentation as part of pull requests that change related code.

---
This approach ensures that all contributors have access to up-to-date, relevant documentation for this project.
