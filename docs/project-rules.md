# Project Rules

This document defines standards for file naming, writing, and code generation to ensure clarity, consistency, and maintainability across the project.

---

## 1. File Naming

- Apply these conventions to all files and directories in the repository.
- Use `lowercase-with-hyphens` for most files and folders (e.g., `api-reference.md`, `user-guide.txt`, `build-scripts/`).
- Use `ALL_CAPS_WITH_UNDERSCORES` only for universally recognized files (e.g., `README.md`, `LICENSE`).
- Do not use spaces, camelCase, PascalCase, or underscores in regular names.
- Avoid version numbers, dates, or author names in file or directory names unless required by external standards.
- Choose names that are descriptive but concise (e.g., prefer `error-handling.md` over `how-to-handle-errors-in-the-library.md`).

---

## 2. Writing and Documentation

- Use structured, bulleted, or numbered lists when they improve clarity or readability; otherwise, use paragraphs or other suitable formats.
- Write in clear, natural language with a consistent and professional tone.
- Favor short, direct sentences and avoid unnecessary jargon.
- Connect sentences using semicolons (;), colons (:), or parentheses when appropriate; do not use hyphens, as they may cause confusion with hyphenated terms (e.g., read-only, white-space).

---

## 3. Code Generation and Review

### 3.1 Production-Ready Output
- Write code and comments as if they are going directly into a production release.
- Avoid placeholders, temporary hacks, or process explanations.

### 3.2 Commenting
- Comments must clarify intent, edge cases, or usage for future maintainers.
- Do not include remarks about code generation process or tool.
- Example of a good comment:
  `// Ensures the cache is refreshed only if the data is stale.`
- Example of a bad comment:
  `// Added by AI to fix bug in previous version.`

### 3.3 Placement and Structure
- Integrate new code elements (functions, properties, classes, etc.) into their most logical and conventional place within the file or project.
- Do not simply append new code to the end unless that is the standard for the context.

### 3.4 Refactoring and Clarity
- Reorganize, split, or merge code as needed to improve clarity or maintainability.
- Favor clear, maintainable structure over minimal edits.

### 3.5 Ready to Commit
- Ensure all output is ready to commit; no further formatting, renaming, or moving should be necessary.
- The result should match what a careful, experienced developer would submit for review.
