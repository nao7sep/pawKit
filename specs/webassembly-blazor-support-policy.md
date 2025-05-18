# WebAssembly/Blazor Support Policy Specification for pawKitLib

## 1. Overview

This document defines the support policy and design considerations for **WebAssembly (Wasm) and Blazor** in `pawKitLib`. The goal is to ensure that the library is cross-platform, with clear guidance on which modules are Wasm-safe and how to handle features not supported in browser-based environments.

---

## 2. Design Philosophy

- **Cross-Platform by Default**: The core library targets .NET 6+ and is designed to work on Windows, Linux, macOS, and Blazor Server/Hybrid.
- **Explicit Wasm Policy**: Features that are not compatible with Blazor WebAssembly (browser) are clearly documented and modularized.
- **Minimalism**: Only include features in the core that are broadly supported across all platforms.
- **Extensibility**: Server-only or platform-specific features are implemented as optional modules or adapters.

---

## 3. Support Matrix

- **Blazor Server/Hybrid**: Fully supported; all core features are available.
- **Blazor WebAssembly (browser)**:
  - **Supported**: Most core features (logging, config, validation, etc.)
  - **Not Supported**: EF Core + SQLite, file system access, and other server-only features
  - **Guidance**: Mark modules as Wasm-safe or server-only in their respective specs

---

## 4. Implementation Guidance

- **Modularization**: Features not supported in Wasm (e.g., SQLite/EF Core) must be modular and not referenced by the core library.
- **Stubbing**: For unsupported features, provide stubs or no-op implementations where appropriate.
- **Documentation**: Each module spec should indicate Wasm compatibility.

---

## 5. Rationale and Best Practices

- **Why explicit policy?**
  - Prevents runtime errors in unsupported environments
  - Makes it easy for consumers to understand platform compatibility
- **Why modularization?**
  - Keeps the core library portable and lightweight
  - Enables advanced features for server scenarios without impacting browser apps

---

**End of WebAssembly/Blazor Support Policy Specification for pawKitLib**
