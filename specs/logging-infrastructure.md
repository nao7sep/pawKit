# Logging Infrastructure Specification for pawKitLib

## 1. Overview
This document specifies the requirements and design for the **Logging Infrastructure** of `pawKitLib`. The goal is to provide a minimal, highly configurable, and dependency-free logging system suitable for both small and large .NET applications, with optional integration points for ASP.NET Core and external logging frameworks.

## 2. Design Philosophy
- **Minimalism**: Only essential features are included; no unnecessary complexity or dependencies.
- **Explicit Configuration**: No default logger is enabled. Logging must be explicitly configured by the application.
- **Thread Safety**: All file and console operations are thread-safe.
- **Cross-Platform**: Works identically on Windows, Linux, and macOS.
- **No Dependency Bloat**: Does not depend on Serilog, Microsoft.Extensions.Logging, or any external logging library in the core.
- **Extensibility**: Optional adapters can bridge to ASP.NET Core’s `ILogger` or other frameworks via a separate library.

## 3. Core Features
### 3.1. Log Destinations
- **File Output**: Logs are written to a file in a configurable base directory and file name.
- **Console Output**: Logs can be written to the console, with level-based coloring.
- **No default output**: The application must call a configuration method to enable logging.

### 3.2. Log Levels
- Supported levels: `Debug`, `Info`, `Warning`, `Error`.
- Each log entry is tagged with its level.
- Minimum log level is configurable; lower-level logs are ignored.

### 3.3. Timestamps
- Each log entry is timestamped (configurable format, default: `yyyy-MM-dd HH:mm:ss.fff`).
- Option to use UTC or local time.

### 3.4. Thread Safety
- All file and console writes are protected by a lock to prevent interleaved output.

### 3.5. Console Coloring
- Console output uses color based on log level:
  - `Debug`: Gray
  - `Info`: White
  - `Warning`: Yellow
  - `Error`: Red
- No manual color resets required by the user; the logger handles color changes and resets.

### 3.6. Logger Configuration
- **Base Directory**: Configurable; defaults to `logs` under the app base directory.
- **File Name**: Configurable; defaults to `app.log`.
- **Minimum Level**: Configurable; defaults to `Info`.
- **Console Output**: Configurable; defaults to enabled.
- **Timestamp Format**: Configurable.
- **UTC/Local Time**: Configurable.
- **Explicit Enablement**: Logging must be enabled via a method call (e.g., `Logger.Enable(settings)`), otherwise all logging methods throw or no-op.

### 3.7. API Design
- Static `Logger` class with direct methods:
  - `Logger.Info(string message)`
  - `Logger.Error(string message, Exception? ex = null)`
  - `Logger.Debug(string message)`
  - `Logger.Warning(string message)`
  - `Logger.Log(string message, LogLevel level, Exception? ex = null)`
- All methods are thread-safe.
- No ambient static logger; must be explicitly enabled/configured.

### 3.8. Exception Logging
- Exceptions can be logged with stack trace and message.
- Optionally, exceptions can be serialized and stored separately (see Exception Management spec).

### 3.9. Configuration via JSON
- Optionally, logger settings can be loaded from a JSON file (e.g., `appsettings.json`).
- The logger can bind to a configuration section if provided, but does not require Microsoft.Extensions.Configuration.

### 3.10. Integration Points
- **ASP.NET Core**: A separate adapter library can bridge the logger to ASP.NET Core’s `ILogger` infrastructure by implementing `ILoggerProvider` and `ILogger`.
- **No core dependency**: The main logger does not reference ASP.NET Core or Microsoft.Extensions.Logging.

### 3.11. No Default Logger
- Logging is **opt-in**. If not configured, logging methods throw or silently do nothing (configurable).
- This prevents accidental file or console output in unconfigured environments.

## 4. Advanced/Optional Features
### 4.1. Rolling Log Files
- (Optional) Support for rolling log files by date or size.

### 4.2. Structured Logging
- (Optional) Support for structured log entries (e.g., key-value pairs, JSON output).

### 4.3. Multiple Sinks
- (Optional) Support for multiple simultaneous outputs (e.g., file + console + SQLite).

### 4.4. SQLite Log Sink
- (Optional) Support for logging to a SQLite database, with a pluggable sink.

### 4.5. Log Message Formatting
- (Optional) Customizable log message format (template strings).

### 4.6. Pluggable Storage Backends: SQLite/EF Core
- (Optional) Support for logging to a SQLite database or EF Core context, with a pluggable sink.
- **Opt-in:** No dependency unless enabled by the application.
- **Schema:** Default schema for logs, auto-migration support if using EF Core.
- **Usage:** `LogSink.UseSqlite(path)` or similar API to enable.
- **No impact on binary size for apps that do not use this feature.**
- **Not supported in Blazor WebAssembly (browser); Blazor Server/Hybrid is supported.**

## 5. Implementation Notes
- **No global state**: All configuration is explicit and must be set before use.
- **No dependency on appsettings.json**: JSON config is optional and must be loaded by the application.
- **No color themes**: Only level-based coloring; no Serilog-style themes.
- **No ambient logger**: Must be enabled explicitly.
- **No silent fallbacks**: If logging is not enabled, methods throw or no-op (configurable).

## 6. Example Usage
```csharp
Logger.Enable(new LoggerSettings {
    BaseDirectory = AppPaths.BaseDirectory + "/logs",
    FileName = "myapp.log",
    MinimumLevel = LogLevel.Debug,
    WriteToConsole = true,
    TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff",
    UseUtc = true
});

Logger.Info("Application started.");
Logger.Error("Failed to load config", ex);
```

## 7. Extensibility and Future-Proofing
- The logger is designed to be extended with new sinks, formats, and integration points without breaking existing code.
- All configuration is explicit and testable.
- The logger can be replaced or wrapped for advanced scenarios (e.g., structured logging, distributed tracing).

## 8. File and Namespace Structure
- Namespace: `pawKitLib.Logging`
- One class/enum/struct per file; file name matches type name.
- All logging-related types are in the `Logging` subfolder and namespace.

## 9. References
- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---
**End of Logging Infrastructure Specification for pawKitLib**
