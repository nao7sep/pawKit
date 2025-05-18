# Process and Command Line Utilities Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Process and Command Line Utilities** in `pawKitLib`. The goal is to provide robust, cross-platform, and ergonomic APIs for parsing, modeling, and interacting with command line arguments and external processes. The design is based on the detailed conversation log and the `topics-and-details.md` topic list. Any ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Cross-platform First**: All utilities must work consistently on Windows, Linux, and macOS.
- **Minimalism**: Avoid over-engineering; provide only what is needed for robust CLI and process handling.
- **Explicitness**: No hidden magic; all parsing and normalization rules are clear and documented.
- **Testability**: All parsing and modeling logic is easily unit-testable.
- **Extensibility**: The model allows for future expansion (e.g., new argument types, process features) without breaking changes.

---

## 3. Command Line Parsing

### 3.1 Goals
- Parse command line input into a structured, context-aware model.
- Support both named (e.g., `--output=foo.txt`, `-o foo.txt`) and positional arguments.
- Normalize argument styles across platforms (Windows, Linux, macOS).
- Allow dictionary-like and context-aware access to arguments.

### 3.2 Grammar and Normalization
- Recognize all common argument forms:
  - `--name=value`, `--name value`, `-n value`, `/name:value` (Windows)
  - Support for flags (e.g., `--verbose`, `-v`)
  - Support for positional arguments (e.g., `mytool input.txt output.txt`)
- Normalize all argument names to a canonical form (e.g., lower-case, no prefix)
- Handle quoted values and escaped characters correctly
- Support both single-dash and double-dash prefixes
- Optionally support argument grouping (e.g., `-abc` as `-a -b -c`)

### 3.3 Model: `CommandLineInput`
- Properties:
  - `MainCommand` (string): The main command or executable name
  - `NamedArgs` (Dictionary<string, string?>): All named arguments (normalized)
  - `Flags` (HashSet<string>): All present flags (normalized)
  - `Positionals` (List<string>): All positional arguments
- Methods:
  - `TryGet(string name, out string? value)`
  - `HasFlag(string name)`
  - `GetPositional(int index)`
  - `Count` properties for each category
- Construction:
  - From `string[] args` (as in `Main(string[] args)`)
  - From raw command line string (for testing or scripting)

### 3.4 Example
```csharp
var cli = CommandLineInput.Parse(args);
if (cli.HasFlag("help")) { ... }
var output = cli.NamedArgs.TryGetValue("output", out var val) ? val : "default.txt";
var inputFile = cli.GetPositional(0);
```

---

## 4. Process Utilities

### 4.1 Goals
- Provide a safe, cross-platform API for launching and interacting with external processes.
- Support capturing standard output, error, and exit codes.
- Allow for timeouts, cancellation, and environment variable overrides.
- Support both synchronous and asynchronous execution.

### 4.2 Model: `ProcessRunner`
- Methods:
  - `Run(string fileName, string arguments, ...)` (sync/async)
  - `RunAndCapture(string fileName, string arguments, ...)` (returns output, error, exit code)
  - Overloads for environment variables, working directory, timeout, cancellation
- Output model: `ProcessResult`
  - `ExitCode` (int)
  - `StandardOutput` (string)
  - `StandardError` (string)
  - `TimedOut` (bool)
  - `Exception` (Exception?)

### 4.3 Safety and Best Practices
- All process launches are opt-in and require explicit configuration.
- No shell injection: arguments are always passed as arrays or safely quoted.
- Timeouts and cancellation tokens are supported to avoid hanging processes.
- All exceptions are wrapped in domain-specific exceptions (e.g., `ProcessLaunchException`).

---

## 5. Usage Guidelines

- **Always parse arguments using `CommandLineInput`**: Do not access `args[]` directly.
- **Never trust raw user input**: Always normalize and validate.
- **Prefer async process execution** for long-running or user-facing commands.
- **Log all process launches and results** for auditability.
- **Wrap all process errors** in custom exceptions for clarity.

---

## 6. File and Namespace Structure

- Namespace: `PawKitLib.Cli`
- One class per file, file name matches type name
- All CLI/process classes in the `Cli` subfolder and namespace

---

## 7. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability

---

**End of Process and Command Line Utilities Specification for pawKitLib**
