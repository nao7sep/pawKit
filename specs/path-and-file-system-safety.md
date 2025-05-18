# Path and File System Safety Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Path and File System Safety** in `pawKitLib`. The goal is to provide a robust, cross-platform, and secure set of helpers and abstractions for path handling, file/directory operations, and related policies. The design is based on the full design conversation log and the topic list, with all details preserved and ambiguities resolved in favor of explicitness, safety, and future-proofing.

---

## 2. Design Philosophy

- **Unified Abstraction**: All path and file system operations are mediated through a unified, cross-platform abstraction layer.
- **Security by Default**: All file and directory operations enforce strict path validation and reject unsafe or ambiguous inputs.
- **No Silent Defaults**: Features like base directory, path policies, and file IO must be explicitly enabled/configured; fail fast if not.
- **Cross-Platform Consistency**: Path normalization, separator handling, and root/relative checks behave identically on all platforms.
- **Extensibility**: New path policies, storage backends, and wrappers can be added without breaking existing code.
- **Minimal Dependencies**: Only .NET BCL is used by default; optional support for SQLite/EF Core is pluggable.
- **Testability**: All abstractions are testable and support injection/mocking for unit tests.

---

## 3. Core Features

### 3.1 Path Normalization and Validation

- **CrossPlatformPath**: A model class that encapsulates a path string and exposes:
  - Normalized path (unified separators, collapsed redundant segments)
  - Detection of mixed separators (e.g., both `/` and `\`)
  - Platform style (Windows vs POSIX)
  - Absolute/relative status (with explicit logic, not just `Path.IsPathRooted`)
  - Segments (split by normalized separator)
  - Methods: `IsRelative()`, `IsAbsolute()`, `Normalize()`, `Collapse()`, `SanitizeForLocalUse()`, `ToPlatformPath()`
  - Rejects or flags mixed-separator paths as unsafe
- **No Trust in .NET Path Methods Alone**: Do not rely solely on `Path.IsPathRooted`, `Path.Combine`, or `Path.GetRelativePath` for security or correctness.
- **Explicit Path Policy**: All file/directory operations must check and enforce path policy (e.g., fully qualified, sandboxed, no traversal).

### 3.2 Enforcement of Fully Qualified Paths

- **PathGuards**: Static helper class to enforce that all file/directory operations receive fully qualified paths.
  - Throws if a path is not fully qualified (using `Path.IsPathFullyQualified` and additional normalization checks)
  - Optionally logs or warns if a relative path is passed
- **No Implicit Current Directory Usage**: All IO methods must reject or explicitly normalize relative paths; never silently combine with `Environment.CurrentDirectory`.
- **Fail Fast**: If a method receives an unqualified or unsafe path, it must throw an exception immediately.

### 3.3 Safe Wrappers for File and Directory Operations

- **SafeFile**, **SafeDirectory**, **SafeFileInfo**, **SafeDirectoryInfo**: Static and instance wrappers for all file/directory operations.
  - All methods enforce path validation and normalization
  - Optionally support path policy injection (e.g., sandbox root, allowed extensions)
  - Provide clear, intention-revealing APIs (e.g., `EnsureExists`, `CopySafelyTo`, `WriteAllTextSafe`)
  - Support logging/tracing hooks for all operations
- **No Direct Use of System.IO in Application Code**: All file/directory access in consumer code should go through these wrappers.

### 3.4 Path Policy Enforcement

- **PathPolicy**: Configurable policy object that defines:
  - Whether only fully qualified paths are allowed
  - Allowed base/sandbox root directory (all paths must be children)
  - Allowed/disallowed file extensions
  - Whether mixed separators are rejected
  - Optional: maximum path length, symlink/junction resolution policy
- **Policy Injection**: All SafeFile/SafeDirectory methods accept or use a configured PathPolicy; default policy is strict.

### 3.5 Sandboxing and Root Restriction

- **Sandbox Enforcement**: All file/directory operations can be restricted to a configured root directory (sandbox).
  - All paths are checked to ensure they are children of the sandbox root (after normalization and collapse)
  - Attempts to escape the sandbox (e.g., via `..` or symlinks) are detected and rejected

### 3.6 Mixed Separator and Traversal Attack Prevention

- **Mixed Separator Detection**: Any path containing both `/` and `\` is flagged as unsafe and rejected by default.
- **Traversal Prevention**: All paths are normalized and collapsed before use; attempts to use `..` to escape allowed roots are detected and rejected.
- **No Manual String Checks**: All validation is done via normalization and segment analysis, not by searching for `..` in raw strings.

### 3.7 Logging and Diagnostics

- **Original vs Normalized Path Logging**: All operations log both the original and normalized path for traceability.
- **Explicit Error Messages**: All exceptions thrown by path/file wrappers include detailed diagnostics (reason, offending path, policy in effect).

### 3.8 Testability and Extensibility

- **All Wrappers are Testable**: Support for dependency injection and mocking of file system operations.
- **Extensible Policy and Backend**: New policies, storage backends, or wrappers can be added without breaking the API.

---

## 4. API Patterns

### 4.1 CrossPlatformPath Model

```csharp
public class CrossPlatformPath
{
    public string Raw { get; }
    public string Normalized { get; }
    public bool IsWindowsStyle { get; }
    public bool IsAbsolute { get; }
    public bool HasMixedSeparators { get; }
    public string[] Segments { get; }
    // ...
    public CrossPlatformPath(string input) { /* ... */ }
    public CrossPlatformPath Normalize();
    public bool IsChildOf(string basePath);
    public override string ToString();
}
```

### 4.2 PathGuards

```csharp
public static class PathGuards
{
    public static void EnsureFullyQualified(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException($"Expected fully qualified path, got: {path}");
    }
}
```

### 4.3 SafeFile Example

```csharp
public static class SafeFile
{
    public static void WriteAllText(string path, string content, PathPolicy policy = null)
    {
        PathGuards.EnsureFullyQualified(path);
        // ... check policy, normalize, log, etc.
        File.WriteAllText(path, content);
    }
    // ... other safe wrappers
}
```

### 4.4 PathPolicy Example

```csharp
public class PathPolicy
{
    public bool RequireFullyQualified { get; set; } = true;
    public string SandboxRoot { get; set; } = null;
    public bool RejectMixedSeparators { get; set; } = true;
    public string[] AllowedExtensions { get; set; } = null;
    // ...
}
```

---

## 5. Example Usage

```csharp
// Enforce fully qualified paths and sandboxing
var policy = new PathPolicy { SandboxRoot = "/app/data", RequireFullyQualified = true };

SafeFile.WriteAllText("/app/data/output.txt", "Hello", policy); // OK
SafeFile.WriteAllText("../etc/passwd", "Oops", policy); // Throws

var path = new CrossPlatformPath(userInput);
if (path.HasMixedSeparators)
    throw new SecurityException("Path contains mixed separators");
if (!path.IsChildOf(policy.SandboxRoot))
    throw new SecurityException("Path escapes sandbox");
```

---

## 6. Rationale and Best Practices

- **Why a unified abstraction?**
  - Prevents platform-specific bugs and security issues
  - Makes code portable and testable
- **Why strict path enforcement?**
  - Prevents directory traversal, symlink, and mixed-separator attacks
- **Why fail fast?**
  - Avoids silent misbehavior and hard-to-debug bugs
- **Why wrappers for IO?**
  - Centralizes policy enforcement and logging
- **Why explicit policy injection?**
  - Allows for flexible, testable, and future-proof configuration

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Files`
- One class/enum/struct per file; file name matches type name
- All file/path management types are in the `Files` subfolder and namespace
- Optional: policy and guard types in a `Files.Policies` sub-namespace

---

## 8. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, security, and testability

---

**End of Path and File System Safety Specification for pawKitLib**
