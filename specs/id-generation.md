# ID Generation Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for the **ID Generation** subsystem in `pawKitLib`. The goal is to provide a centralized, extensible, and testable set of utilities for generating, parsing, and validating unique identifiers (IDs) suitable for a wide range of application scenarios, including distributed systems, databases, URLs, and user-facing tokens. The design is based on the detailed design conversation and the `topics-and-details.md` topic list. All ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Centralization**: All ID generation logic is encapsulated in a single, well-defined subsystem.
- **Extensibility**: New ID formats and algorithms can be added without breaking changes.
- **Testability**: All ID generators are deterministic and unit-testable.
- **Minimalism**: Only essential features are included; no unnecessary complexity or dependencies.
- **Future-proofing**: Support for modern and legacy ID formats, with clear migration paths.
- **No Dependency Bloat**: No external dependencies unless absolutely necessary (e.g., for ULID/KSUID).

---

## 3. Core Interfaces and Classes

### 3.1. `IIdGenerator`
- **Purpose**: Abstraction for all ID generation strategies.
- **API**:
  - `string NewId()` — generates a new unique ID as a string.
  - `T NewId<T>()` — generates a new ID as a strongly-typed value (e.g., Guid, ulong).
  - `bool TryParse(string input, out T id)` — attempts to parse a string into the native ID type.
  - `string ToString(T id)` — converts a native ID type to its string representation.
  - `bool IsValid(string input)` — checks if a string is a valid ID of this type.
- **Usage**: All ID generation and parsing is performed via `IIdGenerator` implementations.

### 3.2. Built-in Generators
- **GuidIdGenerator**: Standard .NET GUID/UUID (128-bit, RFC 4122).
- **UlidIdGenerator**: ULID (Universally Unique Lexicographically Sortable Identifier).
- **KsuidIdGenerator**: KSUID (K-Sortable Unique Identifier).
- **ShortIdGenerator**: Short, URL-safe IDs based on Base58/Base62/Base64 encoding of `ulong` or random bytes.
- **Base58IdGenerator**: Encodes/decodes `ulong` or byte[] as Base58 (Bitcoin-style).
- **Base62IdGenerator**: Encodes/decodes `ulong` or byte[] as Base62 (alphanumeric).
- **Base64UrlIdGenerator**: URL-safe Base64 encoding/decoding (YouTube-style, RFC 4648 §5).

#### Example
```csharp
var id = UlidIdGenerator.Instance.NewId(); // "01H7Z6YQK7J8ZQ2V6F3Q4K8Y9A"
var guid = GuidIdGenerator.Instance.NewId<Guid>(); // Guid value
var isValid = Base58IdGenerator.Instance.IsValid("4ER7k2");
```

---

## 4. Supported ID Types and Encodings

### 4.1. Guid/UUID
- 128-bit, globally unique, standard .NET type.
- String format: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".
- Use for database keys, distributed systems, etc.

### 4.2. ULID
- 128-bit, lexicographically sortable, URL-safe.
- String format: 26-character Crockford Base32.
- Use for time-ordered IDs, event streams, etc.

### 4.3. KSUID
- 160-bit, k-sortable, includes timestamp.
- String format: 27-character Base62.
- Use for distributed logs, event sourcing, etc.

### 4.4. ShortId (Base58/Base62/Base64Url)
- Encodes a 64-bit unsigned integer or random bytes.
- Short, URL-safe, no ambiguous characters.
- Use for user-facing tokens, URLs, invitation codes, etc.

---

## 5. API Patterns

### 5.1. Centralized Access
- All generators are accessible via static properties or a registry (e.g., `IdGenerators.Guid`, `IdGenerators.Ulid`).
- New generators can be registered for custom formats.

### 5.2. Type Safety
- Strongly-typed overloads for `Guid`, `ulong`, etc.
- String-based APIs for interoperability.

### 5.3. Parsing and Validation
- All generators provide `TryParse` and `IsValid` methods.
- Parsing is strict: only valid, canonical forms are accepted.

### 5.4. URL Safety
- All string representations are URL-safe by default (no `+`, `/`, or padding in Base64Url).
- Base58/Base62 avoid ambiguous characters (0/O, l/I, etc.).

---

## 6. Example Usage

```csharp
// Generate a new ULID
var ulid = IdGenerators.Ulid.NewId();

// Generate a short, URL-safe ID from a ulong
var shortId = IdGenerators.Base58.NewId(123456789UL); // "BukQL"

// Parse a GUID from string
if (IdGenerators.Guid.TryParse("a1b2c3d4-e5f6-7890-abcd-ef1234567890", out Guid guid))
    Console.WriteLine(guid);

// Validate a KSUID
bool valid = IdGenerators.Ksuid.IsValid("0o5Fs0EELR0fUjHjbCnEtdUwQe3");
```

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Ids`
- One class/enum/struct per file, file name matches type name
- All ID-related classes in the `Ids` subfolder and namespace

---

## 8. Usage Guidelines

- **Always use `IIdGenerator` for ID generation and parsing.**
- **Prefer ULID or KSUID for time-ordered IDs.**
- **Use Base58/Base62/Base64Url for short, user-facing IDs.**
- **Do not use random strings or ad-hoc IDs; always use a generator.**
- **Write custom generators as needed and register them for reuse.**

---

## 9. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of ID Generation Specification for pawKitLib**
