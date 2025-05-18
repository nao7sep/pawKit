# Time Abstraction and Time Zone Handling Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Time Abstraction and Time Zone Handling** in `pawKitLib`. The goal is to provide robust, testable, and future-proof APIs for all time-related operations, including UTC/Local conversion, time zone context, and testable time providers. The design is based on the detailed conversation log and the `topics-and-details.md` topic list. Any ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **UTC as the Foundation**: All storage, computation, and business logic use UTC. Local time is only for UI/presentation.
- **Abstraction**: All time access is via interfaces (e.g., `IClock`), never direct calls to `DateTime.UtcNow` or `DateTime.Now`.
- **Testability**: Time providers can be swapped for deterministic testing (e.g., `FixedClock`).
- **Extensibility**: The model allows for new clock types and time zone strategies without breaking changes.
- **Explicitness**: All conversions and time zone usages are explicit and documented.

---

## 3. Core Interfaces and Classes

### 3.1. `IClock` Interface
- **Purpose**: Abstracts the source of current time.
- **API**:
  - `DateTime GetUtc();` // Returns the current UTC time. No `UtcNow` property to avoid confusion with static APIs.
- **Usage**: All code requiring the current time must depend on `IClock`.

### 3.2. Implementations
- **SystemClock**: Returns the real system UTC time.
- **FixedClock**: Always returns a fixed UTC time (for tests, replay, or deterministic scenarios).
- **OffsetClock**: Returns `SystemClock` time plus a fixed offset (useful for simulating time travel in tests).
- **ServerSyncedClock**: Optionally, synchronizes with a remote server or NTP source (future extension).

#### Example
```csharp
IClock clock = new SystemClock();
DateTime now = clock.GetUtc();
```

---

## 4. Time Zone Context and Conversion

### 4.1. `TimeZoneContext`
- **Purpose**: Encapsulates the current time zone for a user, session, or operation.
- **API**:
  - `TimeZoneInfo TimeZone { get; }`
  - `DateTime ToLocal(DateTime utc)` // Converts UTC to the context's local time.
  - `DateTime ToUtc(DateTime local)` // Converts local time to UTC, using the context's time zone.
- **Usage**: All UI/presentation code must use `TimeZoneContext` for conversions.
- **Per-User/Session**: In web apps, `TimeZoneContext` can be scoped per user/session/request.

#### Example
```csharp
var tzContext = new TimeZoneContext(TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo"));
DateTime local = tzContext.ToLocal(utcTime);
DateTime utc = tzContext.ToUtc(localTime);
```

---

## 5. Best Practices and Rules

- **All storage and computation is in UTC.** Never store or compare local times.
- **All time access is via `IClock`.** Never use `DateTime.UtcNow` or `DateTime.Now` directly.
- **All time zone conversions are explicit.** Never assume the server's local time zone is correct for the user.
- **Time zone context is per user/session/request.** Never use a global static time zone.
- **Testing uses `FixedClock` or `OffsetClock`.** Never use real time in tests.
- **APIs must be thread-safe and allocation-efficient.**

---

## 6. Extension Methods and Helpers

- Provide extension methods for common conversions:
  - `DateTime ToLocal(this DateTime utc, TimeZoneInfo tz)`
  - `DateTime ToUtc(this DateTime local, TimeZoneInfo tz)`
- Provide helpers for parsing and formatting ISO 8601 timestamps.
- Optionally, provide helpers for `DateTimeOffset` and `Instant` (if using NodaTime or similar).

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Time`
- One class/enum/struct per file, file name matches type name
- All time-related classes in the `Time` subfolder and namespace

---

## 8. Usage Guidelines

- **Always use `IClock` for time access.**
- **Always use `TimeZoneContext` for time zone conversion.**
- **Never store or compare local times.**
- **Use `FixedClock` or `OffsetClock` in tests.**
- **Prefer extension methods for one-off conversions.**

---

## 9. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of Time Abstraction and Time Zone Handling Specification for pawKitLib**
