# Hashing Utilities Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for the **Hashing Utilities** subsystem in `pawKitLib`. The goal is to provide a robust, extensible, and testable set of APIs for hashing strings, files, and streams, supporting a range of use cases from fast non-secure hashes to cryptographic and public fingerprinting. The design is based on the detailed design conversation and the `topics-and-details.md` topic list. All ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Purpose-Driven**: All hashing is categorized by intended use (e.g., fast lookup, secure comparison, public fingerprint, cryptographic).
- **Centralization**: All hashing logic is encapsulated in a single, well-defined subsystem.
- **Extensibility**: New algorithms and purposes can be added without breaking changes.
- **Minimalism**: Only essential, high-value algorithms are included by default; no dependency bloat.
- **Testability**: All hashers are deterministic and unit-testable.
- **Explicitness**: The API makes the purpose and algorithm clear; no silent or ambiguous defaults.

---

## 3. Core Interfaces and Classes

### 3.1. `IHashGenerator`
- **Purpose**: Abstraction for all hash generation strategies.
- **API**:
  - `byte[] ComputeHash(byte[] input)` — hashes a byte array.
  - `string ComputeHash(string input)` — hashes a string (UTF-8 by default).
  - `string ComputeHash(Stream input)` — hashes a stream (reset to start if possible).
  - `string AlgorithmName { get; }` — the name of the algorithm (e.g., "SHA256").
  - `HashPurpose Purpose { get; }` — the intended use category.
- **Usage**: All hashing is performed via `IHashGenerator` implementations.

### 3.2. `HashPurpose` Enum
- **Categories**:
  - `FastNonSecure` — e.g., for hash tables, quick checksums (e.g., xxHash, CRC32).
  - `SecureCompare` — for password or token comparison (e.g., PBKDF2, bcrypt, Argon2).
  - `FingerprintPublic` — for public file or data fingerprints (e.g., SHA256, SHA3, Blake2b).
  - `Cryptographic` — for cryptographic signatures, HMAC, or security-sensitive use (e.g., HMACSHA256, SHA512).

### 3.3. Built-in Hash Generators
- **FastNonSecureHashGenerator**: e.g., xxHash, CRC32 (no security guarantees).
- **SecureCompareHashGenerator**: e.g., PBKDF2, bcrypt, Argon2 (configurable rounds/salt).
- **FingerprintHashGenerator**: e.g., SHA256, SHA3, Blake2b (public fingerprints, not for passwords).
- **CryptographicHashGenerator**: e.g., HMACSHA256, SHA512 (for signatures, MACs).

---

## 4. Supported Algorithms and Use Cases

### 4.1. FastNonSecure
- **Algorithms**: xxHash, CRC32, MurmurHash3 (if available in .NET or via optional package).
- **Use Cases**: Hash tables, quick file deduplication, non-security checksums.
- **Notes**: Not suitable for security or public fingerprints.

### 4.2. SecureCompare
- **Algorithms**: PBKDF2 (built-in), bcrypt, Argon2 (optional, via pluggable package).
- **Use Cases**: Password hashing, token comparison, authentication.
- **Notes**: Always uses salt and configurable rounds; never used for public fingerprints.

### 4.3. FingerprintPublic
- **Algorithms**: SHA256, SHA3, Blake2b (preferably via .NET BCL or optional package).
- **Use Cases**: File fingerprints, public data integrity, API keys, public IDs.
- **Notes**: Not for password storage; output is safe to publish.

### 4.4. Cryptographic
- **Algorithms**: HMACSHA256, SHA512, SHA3 (if available), Blake2b-HMAC.
- **Use Cases**: Message authentication, digital signatures, secure tokens.
- **Notes**: Requires a key for HMAC; never used for password storage.

---

## 5. API Patterns

### 5.1. Centralized Access
- All hash generators are accessible via static properties or a registry (e.g., `HashGenerators.Sha256`, `HashGenerators.FastNonSecure`).
- New generators can be registered for custom algorithms or purposes.

### 5.2. Explicit Purpose
- All APIs require the caller to specify or select the intended purpose; no silent fallback to insecure algorithms.
- Attempting to use a hash for the wrong purpose (e.g., using FastNonSecure for password hashing) should throw or log a warning.

### 5.3. Input Types
- All hashers support hashing of:
  - `string` (UTF-8, with optional encoding parameter)
  - `byte[]`
  - `Stream` (reset to start if possible; caller responsible for stream position)
  - Optional: file path overloads for convenience (with explicit opt-in)

### 5.4. Output Formats
- All hashers return output as:
  - `byte[]` (raw hash)
  - `string` (hex or Base64, algorithm-specific; hex by default for fingerprints)
- Output format is explicit and documented; no silent conversion.

---

## 6. Example Usage

```csharp
// Compute a SHA256 fingerprint of a string
var hash = HashGenerators.Sha256.ComputeHash("hello world");

// Compute a fast, non-secure hash of a file
var hash = HashGenerators.FastNonSecure.ComputeHash(File.OpenRead("data.bin"));

// Compute a password hash (PBKDF2)
var hash = HashGenerators.Pbkdf2.ComputeHash("password123");

// Register a custom hash generator
HashGenerators.Register("MyCustom", new MyCustomHashGenerator());
```

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Hashing`
- One class/enum/struct per file, file name matches type name
- All hashing-related classes in the `Hashing` subfolder and namespace

---

## 8. Usage Guidelines

- **Always use the correct hash purpose for your use case.**
- **Never use FastNonSecure hashes for security or public fingerprints.**
- **Never use fingerprint or cryptographic hashes for password storage; use SecureCompare hashes.**
- **Register custom hashers for new algorithms or special requirements.**
- **Document the rationale for algorithm choice in code and specs.**

---

## 9. Rationale and Best Practices

- **Why purpose-driven categories?**
  - Prevents accidental misuse of weak hashes for security or public fingerprints.
  - Makes intent explicit and code self-documenting.
- **Why explicit output formats?**
  - Avoids confusion between hex, Base64, and raw bytes; ensures interoperability.
- **Why extensibility?**
  - Allows for future algorithms and compliance with evolving best practices.
- **Why minimalism?**
  - Reduces attack surface and maintenance burden; only high-value algorithms are included by default.
- **Why testability?**
  - All hashers are deterministic and have test vectors; easy to verify correctness.

---

## 10. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of Hashing Utilities Specification for pawKitLib**
