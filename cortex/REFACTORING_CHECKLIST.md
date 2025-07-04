﻿﻿# Refactoring & Quality Checklist

*This file contains a list of specific, reusable prompts to run against new or modified code to ensure it meets our quality bar. This checklist should be consulted after a feature is considered "functionally complete."*

## General Checks

- [ ] **Prompt:** "Review the following code for any violations of the Dependency Injection principle as defined in our design principles. Specifically, look for any use of the `new` keyword to instantiate services or repositories."
- [ ] **Prompt:** "Review this code for adherence to our async/await guidelines. Check for `async void`, blocking calls like `.Result` or `.Wait()`, and ensure public async methods accept a `CancellationToken`."
- [ ] **Prompt:** "Scan this code for any 'magic strings'. Replace them with constants defined in an appropriate static class."
- [ ] **Prompt:** "Apply the 'Principle of Surgical Modification'. Review this code and identify if any new functionality was added by accretion instead of refactoring. Suggest how to refactor it cleanly."

## Code Structure & Organization

- [ ] **Prompt:** "Review the internal organization of this class. Ensure members are ordered logically: fields, constructors, properties, public methods, then private methods."
- [ ] **Prompt:** "Scan this class for any generic, reusable logic that is not specific to its primary feature. Suggest how to extract it into a separate utility class."
- [ ] **Prompt:** "Analyze this class for violations of the Single Responsibility Principle. Does it have more than one reason to change? If so, suggest how to split it into smaller, more focused classes."
- [ ] **Prompt:** "Verify that each file contains only one public type (class, record, enum, etc.). If multiple public types exist in one file, suggest splitting them into separate files."
- [ ] **Prompt:** "Verify that the public type name within each file exactly matches the filename (e.g., `MyClass.cs` must contain `public class MyClass`). List any mismatches."
- [ ] **Prompt:** "Analyze the namespace and folder location for this class. Does it accurately reflect its purpose? Suggest a better location and namespace if one exists."

## Documentation & Naming

- [ ] **Prompt:** "Ensure all public and internal types and members in this code have complete XML documentation (`///`)."
- [ ] **Prompt:** "Review all comments in this code. Are they accurate and up-to-date with the current code logic? Pay special attention to comments that reference identifiers that may have been renamed."

## Logging-Specific Checks

- [ ] **Prompt:** "Review the logging statements in this code. Ensure they use structured logging with appropriate log levels and that no sensitive data is being logged."

## Security Checks

- [ ] **Prompt:** "Scan this code for potential Insecure Direct Object Reference (IDOR) vulnerabilities. Ensure that any method retrieving data by an ID also validates that the current user is authorized to access that specific resource."