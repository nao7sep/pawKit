# pawKit Development Guidelines

*Choose the guidelines you need for your current task. Don't load everything - be surgical.*

## Core Guidelines

### [COMMUNICATION_PROTOCOL.md](COMMUNICATION_PROTOCOL.md)
**When to use:** Every session. Enforces Ore-san's brutal honesty in English.
**Contains:** Communication rules, forbidden phrases, tone guidelines
**Load this if:** You want direct, no-bullshit AI responses

### [FOUNDATIONS.md](FOUNDATIONS.md)
**When to use:** Every session. Contains immutable design principles.
**Contains:** SOLID principles, DI rules, architectural patterns
**Load this if:** You're writing any code

### [IMPLEMENTATION.md](IMPLEMENTATION.md)
**When to use:** Building new features or components
**Contains:** Library choices, async patterns, type selection, coding rules
**Load this if:** You're creating something new

### [REFACTORING.md](REFACTORING.md)
**When to use:** Improving existing code quality
**Contains:** Code smell detection, violation checks, quality standards
**Load this if:** You're fixing or improving existing code

### [CONVENTIONS.md](CONVENTIONS.md)
**When to use:** When consistency matters for the current task
**Contains:** Naming, organization, documentation, file structure rules
**Load this if:** You're organizing code or ensuring consistency

## Session Commands

### [SESSION_COMMANDS.md](SESSION_COMMANDS.md)
**When to use:** For version management, logging, and session control
**Contains:** Direct commands for work logs, quality audits, architecture reviews
**Load this if:** You need to manage project workflow or recover from confusion

## Workflow Patterns

**New Feature Development:**
1. Load `COMMUNICATION_PROTOCOL.md` + `FOUNDATIONS.md` + `IMPLEMENTATION.md`
2. Build the feature
3. Load `REFACTORING.md` for quality check
4. Load `CONVENTIONS.md` if organizing multiple files

**Code Quality Improvement:**
1. Load `COMMUNICATION_PROTOCOL.md` + `FOUNDATIONS.md` + `REFACTORING.md`
2. Run quality checks
3. Load `CONVENTIONS.md` for consistency fixes

**Large Refactoring:**
1. Load `COMMUNICATION_PROTOCOL.md` + all technical files
2. Apply systematically: Foundations → Implementation → Refactoring → Conventions

**Brutal AI Sessions:**
1. Always load `COMMUNICATION_PROTOCOL.md` first
2. Add relevant technical files based on task
3. Expect intellectual brutality and sarcastic feedback

**Session Recovery:**
1. Load `SESSION_COMMANDS.md`
2. Use `[Emergency Session Recovery]` command
3. Load relevant technical files based on AI recommendations

## Anti-Pattern Alert

**Don't load everything at once** unless you're doing comprehensive work. Each file is designed for specific contexts. Loading irrelevant guidelines wastes tokens and dilutes focus.

**Don't skip FOUNDATIONS.md** - it contains the non-negotiable principles that govern all other decisions.