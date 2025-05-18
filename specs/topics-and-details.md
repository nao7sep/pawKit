# Topics and Detailed Coverage for `pawKitLib` Implementation

This document lists all major topics and subtopics discussed in the design conversation for `pawKitLib`. Each topic below is detailed and intended to serve as the basis for a dedicated specification document. **Every aspect of the conversation is covered—no functionality is left behind.**

---

## 1. Logging Infrastructure
- Design of a minimal, custom logging system (file, console, levels)
- Thread-safe file writing, log levels, timestamping
- Console output with level-based coloring (no manual color resets)
- Logger configuration (base directory, file name, minimum level, console output)
- Optional integration with appsettings.json
- Logger API: direct methods (Info, Error, Debug, Warning)
- Optional: bridging to ASP.NET Core's ILogger via a separate adapter library
- No default logger: explicit configuration required

## 2. Configuration and Settings Management
- Simple, dynamic key-value(s) configuration system (JSON-backed)
- Support for null, single, or multiple values per key
- Type conversion helpers (GetIntOrNull, GetDoubleOrDefault, etc.)
- Path-based keys (e.g., "appSettings/defaults/fontFamily")
- No dependency on Microsoft.Extensions.Configuration except in ASP.NET Core
- Optional: interface abstraction (IAppSettings) for DI
- Chained/fallback config stores

## 3. Type Conversion Utilities
- Centralized type conversion helpers (string <-> int, double, bool, DateTime, enums)
- Sugar methods for direct access (GetIntOrNull, GetBoolOrDefault, etc.)
- Optional: extension methods for AsIntOrNull, AsBoolOrNull, etc.
- AI-generated region/partial class for overloads (optional)

## 4. Exception Management and Serialization
- Exception filtering and classification (Critical, Warning, Ignored)
- Thread-safe storage of exceptions (concurrent collection)
- Structured, recursive serialization (SerializableException)
- Special handling for AggregateException (multiple inner exceptions)
- Deduplication and filtering of redundant exceptions
- JSON output for persisted exceptions
- Optional: global unhandled exception hook

## 5. Path and File System Safety
- Unified, cross-platform path abstraction (normalization, separator handling)
- Detection and rejection of mixed separators
- Enforcement of fully qualified paths in all file operations
- Safe wrappers for File, Directory, FileInfo, DirectoryInfo (SafeFile, SafeDirectory, etc.)
- Path policy enforcement (sandboxing, root restriction)
- PathGuards for validation

## 6. Dynamic Data and Flexible Value Wrappers
- Discriminated union pattern for dynamic values (null, string, string[])
- Wrapper classes (e.g., StringValue) with type-safe accessors
- Implicit conversions and pattern matching support
- Use in API response parsing and config

## 7. Exception Class Design and Usage
- When to use built-in .NET exceptions vs. custom exceptions
- Custom exception base class (e.g., PawKitLibException)
- Domain-specific exceptions (e.g., ConfigParseException)
- Exception hierarchy and naming conventions
- Wrapping and translating built-in exceptions

## 8. Process and Command Line Utilities
- Cross-platform command line parser (named/positional args, normalization)
- CommandLineInput model (main command, named args, positional args)
- Grammar for Windows, Linux, macOS styles
- Context-aware and dictionary-like access

## 9. String Manipulation and Formatting
- Centralized utility class for line and content-aware string manipulation
- Normalizing line endings, trimming, removing/collapsing empty lines
- Indentation normalization, whitespace collapsing
- Rule-based, finalizer-style string wrapper (e.g., MultilineInputString)
- One class/enum/struct per file

## 10. Time Abstraction and Time Zone Handling
- IClock interface with GetUtc() method (no UtcNow property)
- SystemClock, FixedClock, OffsetClock, ServerSyncedClock implementations
- TimeZoneContext for per-user/session time zone
- Conversion helpers (ToLocal, ToUtc)
- All storage and computation in UTC; conversion only at UI/presentation layer

## 11. Validation Framework
- Fluent, testable validation engine (Validator<T>, ValidationResult)
- Built-in rules (NotNullOrEmpty, Email, RegexMatch, etc.)
- Optional: custom attributes for model-based validation (not tightly coupled)

## 12. ID Generation
- IIdGenerator interface
- Guid, ULID, KSUID, Base58, Base62, ShortId (ulong <-> string) support
- URL-safe Base64 (YouTube-style) encoding/decoding
- Centralized, testable, and future-proof ID generation

## 13. Hashing Utilities
- IHashGenerator interface with HashPurpose enum
- Four categories: FastNonSecure, SecureCompare, FingerprintPublic, Cryptographic
- Pre-configured default algorithms for each purpose
- Hashing for strings, files, streams
- Rationale for each category and algorithm choice

## 14. Localization
- ILocalizer interface, JSON-based implementation (JsonLocalizer)
- FallbackLocalizer for chaining
- Dynamic language switching (per-request, per-session, Blazor Server support)
- Integration with ASP.NET Core, desktop, and Blazor
- Razor usage via @inject

## 15. Namespace and Project Structure
- Root namespace: pawKitLib
- One class/enum/struct per file, file name = type name
- Sub-namespaces: App, Config, Time, Validation, Ids, Hashing, Files, Text, Cli, Logging, Localization
- Folder structure matches namespaces
- Single project to start (pawKitLib.csproj), ready for future modularization

---

**Each topic above is intended to be the basis for a dedicated specification document.**

- All topics are covered in detail, with no gaps or omissions from the conversation log.
- When generating code, use the conversation log and the relevant topic specification together to ensure complete coverage.
