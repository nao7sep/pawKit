# Exception Management and Serialization Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Exception Management and Serialization** in `pawKitLib`. The goal is to provide a robust, structured, and ergonomic set of helpers for capturing, filtering, serializing, and persisting exceptions, with a focus on testability, extensibility, and cross-platform compatibility. All details are based on the design conversation log and the `topics-and-details.md` topic list. Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and future-proofing.

---

## 2. Design Philosophy

- **Structured Data**: Exceptions are treated as first-class, structured data, not just log strings.
- **Filtering and Classification**: Not all exceptions are equal; the system supports filtering and classification by severity and context.
- **Thread-Safety**: All exception storage and management is thread-safe.
- **Recursive Serialization**: Exception hierarchies (including `AggregateException`) are serialized recursively, preserving all relevant structure.
- **Extensibility**: New exception types, severity levels, and storage backends can be added without breaking existing code.
- **Minimal Dependencies**: Only .NET BCL and System.Text.Json are used by default; optional support for SQLite/EF Core is pluggable.
- **No Global State**: All exception management is stateless or uses explicit, thread-safe collections.

---

## 3. Core Features

### 3.1 Exception Classification

- **Severity Levels**:
  - `Critical`: Must be logged, serialized, and possibly uploaded or alerted.
  - `Warning`: Recoverable or user-facing errors; may be logged or stored for diagnostics.
  - `Ignored`: Expected, common, or spammy exceptions; not persisted.
- **Context Tagging**: Each exception can be tagged with a context string (e.g., "Startup", "UserInput").

### 3.2 Thread-Safe Storage

- Use a concurrent collection (e.g., `ConcurrentQueue<LoggedException>`) for storing exceptions.
- Optional deduplication using a hash of exception type, message, and stack trace.
- All operations are safe for concurrent use in multi-threaded environments.

### 3.3 Structured, Recursive Serialization

- **SerializableException**: A model class that captures:
  - Type (full name)
  - Message
  - StackTrace
  - Source
  - Data (as a dictionary)
  - Inner exception (recursive)
  - For `AggregateException`: a collection of inner exceptions
- **Depth Limiting**: To avoid runaway recursion, a maximum depth (default: 5) is enforced.
- **JSON Output**: All exceptions can be serialized to indented JSON for persistence or upload.

### 3.4 Filtering and Deduplication

- Only exceptions above a configurable severity are stored/serialized.
- Deduplication: Optionally skip storing duplicate exceptions (by hash).
- Rate-limiting: Optionally log only once per time window for repeated exceptions (future extension).

### 3.5 Persistence and Export

- **JSON File Output**: All stored exceptions can be exported to a JSON file.
- **Pluggable Storage**: Optional support for SQLite/EF Core for structured, queryable exception logs.
- **No mandatory dependency**: SQLite/EF Core support is opt-in and modular.

### 3.6 Global Exception Hooks (Optional)

- Support for registering global unhandled exception handlers:
  - `AppDomain.CurrentDomain.UnhandledException`
  - `TaskScheduler.UnobservedTaskException`
- These handlers should report exceptions as `Critical` with appropriate context.

### 3.7 Pluggable Storage Backends: SQLite/EF Core

- (Optional) Support for storing exceptions in a SQLite database or via EF Core context.
- **Opt-in:** No dependency unless enabled by the application.
- **Schema:** Default schema for exceptions, auto-migration support if using EF Core.
- **Usage:** `ExceptionStore.UseSqlite(path)` or similar API to enable.
- **No impact on binary size for apps that do not use this feature.**
- **Not supported in Blazor WebAssembly (browser); Blazor Server/Hybrid is supported.**

---

## 4. API Patterns

### 4.1 Exception Severity Enum

```csharp
public enum ExceptionSeverity
{
    Critical,
    Warning,
    Ignored
}
```

### 4.2 SerializableException Model

```csharp
public class SerializableException
{
    public string Type { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string Source { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public SerializableException Inner { get; set; } // For standard exceptions
    public List<SerializableException> InnerExceptions { get; set; } // For AggregateException

    public static SerializableException FromException(Exception ex, int depth = 5)
    {
        if (ex == null || depth <= 0) return null;
        var result = new SerializableException
        {
            Type = ex.GetType().FullName,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Source = ex.Source,
            Data = ex.Data?.Cast<DictionaryEntry>()
                        .ToDictionary(e => e.Key.ToString(), e => e.Value)
        };
        if (ex is AggregateException agg)
        {
            result.InnerExceptions = agg.InnerExceptions
                .Select(inner => FromException(inner, depth - 1))
                .ToList();
        }
        else
        {
            result.Inner = FromException(ex.InnerException, depth - 1);
        }
        return result;
    }
}
```

### 4.3 LoggedException Model

```csharp
public class LoggedException
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ExceptionSeverity Severity { get; set; }
    public string Context { get; set; }
    public SerializableException Exception { get; set; }
}
```

### 4.4 ExceptionManager Static Class

```csharp
public static class ExceptionManager
{
    private static readonly ConcurrentQueue<LoggedException> _exceptions = new();
    private static readonly HashSet<string> _seenHashes = new(); // for deduplication
    private static readonly object _lock = new();

    public static void Report(Exception ex, ExceptionSeverity severity = ExceptionSeverity.Warning, string context = null)
    {
        if (severity == ExceptionSeverity.Ignored || ex == null) return;
        var logged = new LoggedException
        {
            Severity = severity,
            Context = context,
            Exception = SerializableException.FromException(ex)
        };
        var hash = $"{logged.Exception.Type}:{logged.Exception.Message}:{logged.Exception.StackTrace?.GetHashCode()}";
        lock (_lock)
        {
            if (!_seenHashes.Add(hash)) return; // skip duplicates
        }
        _exceptions.Enqueue(logged);
    }

    public static List<LoggedException> GetAll() => _exceptions.ToList();

    public static void SaveToJsonFile(string path)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_exceptions.ToList(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }
}
```

---

## 5. Example Usage

```csharp
try
{
    LoadConfiguration();
}
catch (Exception ex)
{
    ExceptionManager.Report(ex, ExceptionSeverity.Critical, "During startup");
    Environment.Exit(1);
}

try
{
    user.Age = int.Parse(input);
}
catch (Exception ex)
{
    ExceptionManager.Report(ex, ExceptionSeverity.Warning, "Parsing age input");
}

// Export all captured exceptions to a file
ExceptionManager.SaveToJsonFile("exceptions.json");
```

---

## 6. Rationale and Best Practices

- **Why structured serialization?**
  - Enables post-mortem analysis, telemetry, and deduplication.
  - Preserves full causal chains (including AggregateException).
- **Why severity filtering?**
  - Avoids log spam and focuses attention on actionable issues.
- **Why deduplication?**
  - Prevents flooding logs/storage with repeated, identical exceptions.
- **Why thread-safe storage?**
  - Ensures reliability in multi-threaded and async environments.
- **Why JSON output?**
  - Human-readable, machine-processable, and easy to upload or analyze.
- **Why pluggable storage?**
  - Allows for future migration to SQLite, EF Core, or remote telemetry without breaking the API.

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Exceptions`
- One class/enum/struct per file; file name matches type name
- All exception management types are in the `Exceptions` subfolder and namespace
- Optional: SQLite/EF Core support in a separate sub-namespace/module

---

## 8. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability

---

**End of Exception Management and Serialization Specification for pawKitLib**
