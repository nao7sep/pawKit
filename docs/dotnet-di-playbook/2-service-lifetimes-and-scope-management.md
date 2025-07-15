# Service Lifetimes & Scope Management

> **Document 2 of 8 – Deep dive into how instances live and die in Microsoft.Extensions.DependencyInjection**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Lifetime Definitions](#2-lifetime-definitions)
   1. Singleton
   2. Scoped
   3. Transient
3. [Lifecycle Flowcharts](#3-lifecycle-flowcharts)
4. [Lazy vs Eager Singletons](#4-lazy-vs-eager-singletons)
5. [Captive Dependency Problem](#5-captive-dependency-problem)
6. [Creating & Disposing Scopes](#6-creating--disposing-scopes)
   1. ASP.NET Core automatic scopes
   2. Console / Worker manual scopes
   3. BackgroundService & HostedService patterns
7. [Async Scopes & IAsyncDisposable](#7-async-scopes--iasyncdisposable)
8. [Lifetimes & Validation Flags](#8-lifetimes--validation-flags)
9. [Real‑World Patterns](#9-real-world-patterns)
   1. Unit‑of‑Work in a message processor
   2. DbContext lifetime management in EF Core
   3. HttpClient reuse strategies
10. [Cheatsheet & Decision Tree](#10-cheatsheet--decision-tree)
11. [Pitfalls & Diagnostics](#11-pitfalls--diagnostics)
12. [Further Reading](#12-further-reading)

---

## 1. Overview

Service lifetimes define **how long an instance exists** and **who disposes it**. Understanding them prevents memory leaks, race conditions, and hidden performance issues.

## 2. Lifetime Definitions

### 2.1 Singleton

| Characteristic            | Details                                                 |
| ------------------------- | ------------------------------------------------------- |
| Scope                     | Entire process                                          |
| Creation                  | First time requested (lazy)                             |
| Disposal                  | When root `IServiceProvider` is disposed / app shutdown |
| Thread‑safety requirement | **Yes** – may be accessed concurrently                  |
| Typical use               | Caches, config providers, stateless HttpClient wrappers |

### 2.2 Scoped

| Characteristic            | Details                                                              |
| ------------------------- | -------------------------------------------------------------------- |
| Scope                     | **One logical operation** (HTTP request, CLI job, SignalR hub, etc.) |
| Creation                  | First request *within* the scope                                     |
| Disposal                  | When the **scope** is disposed                                       |
| Thread‑safety requirement | Not necessarily (usually used per‑request, single thread context)    |
| Typical use               | EF Core `DbContext`, business UoW objects, per‑request services      |

### 2.3 Transient

| Characteristic            | Details                                              |
| ------------------------- | ---------------------------------------------------- |
| Scope                     | None – new instance **every** resolution             |
| Disposal                  | Immediate (after object graph garbage‑collects)      |
| Thread‑safety requirement | N/A (short‑lived)                                    |
| Typical use               | Pure helper classes, formatters, lightweight mappers |

---

## 3. Lifecycle Flowcharts

**Singleton**

```
┌───────────────┐          first resolve          ┌───────────────────┐
│ Descriptor(S) │ ───────────────▶ │ new Instance(S) │
└───────────────┘                              └─────────┬─────────┘
                                      subsequent resolves│same ref
                                                         ▼
                                               Single shared ref
```

**Scoped** (within two different scopes)

```
Scope A --> first resolve --> Instance A  ──┐  (reused in A)
Scope B --> first resolve --> Instance B  ──┴  (reused in B)
```

**Transient**

```
Every resolve ➜ new instance (garbage‑collect when out of graph)
```

---

## 4. Lazy vs Eager Singletons

- **Lazy (default)** – Created when first requested.
- **Eager** – Force construction at startup to warm caches / fail fast:
  1. Manual resolve in `Program.cs`:
     ```csharp
     _ = host.Services.GetRequiredService<MyWarmCache>();
     ```
  2. Depend on the singleton from an `IHostedService` – DI will build it during host startup.

---

## 5. Captive Dependency Problem

> **Definition:** A longer‑lived service (e.g., Singleton) **captures** a shorter‑lived dependency (e.g., Scoped), keeping it alive too long → stale state / memory leaks.

**Example** (❌ Bad):

```csharp
services.AddSingleton<OrderProcessor>();      // depends on DbContext (scoped)
services.AddScoped<DbContext>();
```

**Detection & Fixes**

- Enable `validateScopes:true` in `BuildServiceProvider` (dev only).
- Refactor: make outer service scoped, or change dependency to factory pattern.

---

## 6. Creating & Disposing Scopes

### 6.1 ASP.NET Core (Automatic)

- The framework creates a scope per HTTP request.
- Middleware & controllers run inside it.

### 6.2 Console / Worker Apps (Manual)

```csharp
using var scope = host.Services.CreateScope();
var handler = scope.ServiceProvider.GetRequiredService<IJobHandler>();
await handler.ProcessAsync(job);
```

### 6.3 BackgroundService Pattern

- `BackgroundService.ExecuteAsync` receives host root provider.
- Create child scopes **per unit of work** to avoid holding scoped services forever.

---

## 7. Async Scopes & IAsyncDisposable

- .NET 6+ supports `` objects.
- Container disposes async services correctly at scope end.
- For manual scopes:
  ```csharp
  await using var scope = provider.CreateAsyncScope();
  ```

---

## 8. Lifetimes & Validation Flags

```csharp
var sp = services.BuildServiceProvider(
    validateOnBuild: true,   // Verify graphs immediately
    validateScopes: true);   // Error on captive deps
```

*Not recommended in prod; use in CI or dev.*

---

## 9. Real‑World Patterns

### 9.1 Message Processor Unit‑of‑Work

```csharp
public async Task HandleAsync(Message msg)
{
    await using var scope = _root.CreateAsyncScope();
    var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
    await handler.HandleAsync(msg);
}
```

### 9.2 EF Core DbContext Lifetime

- Register `AddDbContext<T>()` ⇒ **Scoped** by default.
- Same context reused in a request → single transaction.

### 9.3 HttpClient Reuse

- `AddHttpClient` registers typed **transient** clients backed by a pooled message handler.
- Injected into scoped/singleton services safely.

---

## 10. Cheatsheet & Decision Tree

| Question                                 | Choose                    |
| ---------------------------------------- | ------------------------- |
| Need one instance app‑wide?              | Singleton                 |
| Need shared state per web request / job? | Scoped                    |
| Lightweight, stateless helper?           | Transient                 |
| Singleton depends on scoped?             | 🔺 Refactor – captive dep |
| Console loop processing messages?        | Create scope per message  |

---

## 11. Pitfalls & Diagnostics

1. **Double disposal** – Manual `Dispose` on injected service. Don’t.
2. **Forgotten scope** – Holding a scope field forever in a singleton.
3. **Transient storm** – Heavy object (e.g., Regex) registered transient.
4. **Async locals** – Capturing scoped service outside its context in async workflows.
5. **Memory leaks** – Singletons with big caches never cleared.

**Diagnostics Tools**

- `dotnet-counters`, `dotMemory`, Visual Studio Diagnostic Tools.
- Health checks: register `IHealthCheck` per scoped resource to ensure disposal.

---

## 12. Further Reading

- Docs: *Service lifetimes in ASP.NET Core* (learn.microsoft.com)
- Andrew Lock – *Scoped services inside singletons explained*
- EF Core docs – *DbContext lifetime & thread safety*

---

© 2025 DI Playbook – License: MIT

