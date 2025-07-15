# Diagnostics, Performance & Pitfalls

> **Document 8 of 8 – Monitoring, profiling, and hard‑won lessons for production‑grade DI systems**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Observability Basics](#2-observability-basics)
   1. Structured logging categories
   2. Metrics & counters
   3. Distributed tracing hooks
3. [Measuring DI Resolve Performance](#3-measuring-di-resolve-performance)
   1. `ActivatorUtilities` vs compiled expressions
   2. `Microsoft.Extensions.DependencyInjection` EventSource
   3. BenchmarkDotNet patterns
4. [Runtime Diagnostics Toolkit](#4-runtime-diagnostics-toolkit)
   1. `dotnet-counters`
   2. `dotnet-trace`
   3. `dotnet-gcdump`
   4. Visual Studio Diagnostic Tools
5. [Memory & Allocation Analysis](#5-memory--allocation-analysis)
   1. Identifying transient storms
   2. Pinpointing large singletons
   3. GC generations & LOH warnings
6. [Threading & Concurrency](#6-threading--concurrency)
   1. Async‑over‑sync pitfalls
   2. SynchronizationContext & deadlocks
   3. Parallel scopes: data races in singletons
7. [Common Production Pitfalls](#7-common-production-pitfalls)
   1. Captive dependencies missed in dev
   2. Socket exhaustion via mis‑used HttpClient
   3. Unbounded background queues in singletons
   4. Reflection‑heavy assembly scanning on cold start
8. [Performance Hardening Checklist](#8-performance-hardening-checklist)
9. [Sample Diagnostic Recipes](#9-sample-diagnostic-recipes)
   1. Capture a 30‑second trace on high CPU
   2. Compare memory snapshots before/after load test
   3. Inject correlation IDs into every log entry
10. [Cheatsheet](#10-cheatsheet)
11. [Further Reading & Tools](#11-further-reading--tools)

---

## 1. Overview

Even perfectly‑architected services can fail in production without **observability** and **performance vigilance**. This document collects practical techniques for measuring container resolve cost, spotting memory leaks, and surfacing hidden pitfalls before they become customer outages.

---

## 2. Observability Basics

### 2.1 Structured Logging Categories

| Category                                   | Example                                   | Purpose                       |
| ------------------------------------------ | ----------------------------------------- | ----------------------------- |
| `Microsoft.Extensions.DependencyInjection` | `Information: Registered service IRepo`   | Container startup diagnostics |
| `System.Net.Http.*`                        | `Debug: HttpClient.GitHub.LogicalHandler` | Outbound HTTP requests        |
| `MyApp.*`                                  | Custom categories (e.g., `MyApp.Worker`)  | Business domain telemetry     |

*Use **`ILogger<T>`** scopes to attach **`TraceId`**, **`UserId`**, etc. to every log.*

### 2.2 Metrics & Counters

- Prometheus / OpenTelemetry exporting via `Meter` (`System.Diagnostics.Metrics`).
- Key DI metrics: **service resolve count**, **singleton count**, **scoped allocations**.

### 2.3 Distributed Tracing Hooks

- Create an `ActivitySource` named `"MyApp.DI"`.
- Enrich spans when expensive object graphs (>N ms) are resolved.

---

## 3. Measuring DI Resolve Performance

### 3.1 ActivatorUtilities vs Compiled Expressions

MS.DI uses **compiled expression trees** for constructors (fast). `ActivatorUtilities` falls back to reflection—use only for rare on‑the‑fly instantiation in tests.

### 3.2 EventSource Counters

Enable with `dotnet-counters`:

```bash
dotnet-counters monitor "Microsoft.Extensions.DependencyInjection"
```

Counters:

| Name                            | Meaning               |
| ------------------------------- | --------------------- |
| `ServiceResolutionCallDuration` | Avg resolve time (µs) |
| `ServicesResolvedPerSec`        | Throughput            |

### 3.3 BenchmarkDotNet Pattern

```csharp
[MemoryDiagnoser]
public class DiBench
{
    private readonly IServiceProvider _sp;
    public DiBench() => _sp = new ServiceCollection()
        .AddTransient<Foo>()
        .BuildServiceProvider();

    [Benchmark]
    public Foo Resolve() => _sp.GetRequiredService<Foo>();
}
```

---

## 4. Runtime Diagnostics Toolkit

| Tool                | Use‑case                               |
| ------------------- | -------------------------------------- |
| `dotnet-counters`   | Live perf counters (GC, sockets, DI)   |
| `dotnet-trace`      | Collect ETW events → SpeedScope viewer |
| `dotnet-gcdump`     | Heap dumps without debugger            |
| VS Diagnostic Tools | Timeline & memory profiling            |
| PerfView            | Deep CPU sampling, allocations         |

**Recipe**: capture 60‑sec trace on prod box with high CPU:

```bash
dotnet-trace collect -p <PID> --duration 60 -o cpu.nettrace
```

Open in SpeedScope, filter for `Microsoft.Extensions.DependencyInjection` methods.

---

## 5. Memory & Allocation Analysis

### 5.1 Detecting Transient Storms

If `ServicesResolvedPerSec` spikes + Gen0 allocations rise, suspect over‑resolved transients. Solution: cache heavy helpers or raise to scoped/singleton.

### 5.2 Large Singletons

`dotnet-dump heapstat` → look for megabyte‑sized singletons (big regexes, data tables). Consider lazy loading or memory‑mapped files.

### 5.3 GC Generations & LOH Warnings

Objects >85 KB go to Large Object Heap. Repeated allocs → fragmentation. Pool byte[] buffers with `ArrayPool`.

---

## 6. Threading & Concurrency

- **Async‑over‑sync**: blocking `Result`/`Wait()` inside DI constructors → deadlocks.
- **Parallel scopes**: avoid mutable state in singletons; use `ConcurrentDictionary`.
- **ConfigureAwait(false)** in library code to avoid context capture.

---

## 7. Common Production Pitfalls

1. **Captive Scoped in Singleton** – stale DbContext ↔ wrong data.
2. **Misconfigured HttpClient** – `new HttpClient()` in loops, socket exhaustion.
3. **Background Channel with no backpressure** – memory blow‑up.
4. **Assembly scanning each request** – keep scans to startup.
5. **Silent failures in constructor** – exceptions swallowed by factories; log and rethrow.

---

## 8. Performance Hardening Checklist

-

---

## 9. Sample Diagnostic Recipes

### 9.1 High CPU Trace

```bash
dotnet-trace collect -p $(pidof MyApp) -o hot.nettrace --profile cpu-sampling --duration 30
```

### 9.2 Memory Snapshot Comparison

```bash
dotnet-gcdump collect -p <PID> -o before.gcdump
# run load test
dotnet-gcdump collect -p <PID> -o after.gcdump
```

Analyze with VS: **Debug → Performance Profiler → Open dump diff**.

### 9.3 Correlation ID in Logs

```csharp
builder.Services.AddHttpContextAccessor();
app.Use(async (ctx, next) =>
{
    using (LogContext.PushProperty("TraceId", ctx.TraceIdentifier))
        await next();
});
```

---

## 10. Cheatsheet

```csharp
// Enable live DI EventCounters
DOTNET_DiagnosticPorts=\tmp\diag dotnet run &

// Quick GC/CPU view
$ dotnet-counters monitor -p <PID>

// Ensure container graph valid in Program.cs (dev)
builder.Services.BuildServiceProvider(true, true);
```

---

## 11. Further Reading & Tools

- Docs: *dotnet-counters, dotnet-trace, dotnet-gcdump* guides
- Nick Craver – *Stack Overflow: The Architecture* (high‑perf DI tips)
- PerfView tutorial by Vance Morrison
- OpenTelemetry .NET – tracing & metrics instrumentation

---

© 2025 DI Playbook – License: MIT

