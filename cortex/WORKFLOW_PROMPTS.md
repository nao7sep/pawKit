# Cortex Workflow Prompts

*This file contains standardized prompt templates for common development tasks. Use these to ensure consistent interactions with AI assistants.*

---

## [Log Implementation Session]

**Objective:** Generate a work log entry after implementing a new feature or making significant changes. The entry should be a concise, factual summary of the work completed.

**Prompt Template:**
"Based on our conversation, generate a new entry for the `WORK_LOG_vX.Y.Z.md`.

*Instructions for AI:*
- If a text summary of previous work is provided, treat it as context that occurred *before* this conversation.
- Synthesize the entire session's history into a summary of the **final state**. Do not log intermediate or reversed decisions.
- Generate the following fields for the log entry:
  - **Date:** Use today's date (YYYY-MM-DD).
  - **Module:** `[Specify Module Name, e.g., pawKitLib.Logging]`
  - **Summary:** A brief, factual summary of what was implemented (e.g., "Implemented the initial `ILogger` interface and a `ConsoleLogger` class.").
  - **Key Architectural Decisions:** Note any significant patterns or choices made (e.g., "Established a provider pattern for future loggers."). If none, state "N/A".

Append the generated entry to the top of the 'Detailed Log Entries' section in the work log file."

---

## [Log Check & Refactor Session]

**Objective:** Generate a work log entry after completing a quality and refactoring audit on a module.

**Prompt Template:**
"Based on our conversation, generate a new entry for the `WORK_LOG_vX.Y.Z.md`.

*Instructions for AI:*
- Generate the following fields for the log entry:
  - **Date:** Use today's date (YYYY-MM-DD).
  - **Module:** `[Specify Module Name, e.g., pawKitLib.Logging]`
  - **Audit & Refinements:** Create a bulleted list. Each bullet point should summarize the outcome of a specific check from the `REFACTORING_CHECKLIST.md`. Describe any code changes made as a result of the check, or confirm that no issues were found.
    - Example:
      - "Verified adherence to the Single Responsibility Principle by extracting message formatting logic into a new `DefaultLogFormatter` class."
      - "Confirmed that each file contains only one public type and that filenames match their types; no changes were needed."

Append the generated entry to the top of the 'Detailed Log Entries' section in the work log file."

---

## [Audit for Checklist Debt]

**Objective:** Compare the master `REFACTORING_CHECKLIST.md` against a specific module's "Applied Checks" in the work log to find any new checks that need to be retroactively applied.

**Prompt Template:**
"Analyze the master `REFACTORING_CHECKLIST.md` and the `WORK_LOG_vX.Y.Z.md`.

For the module `[Specify Module Name]`, identify any checks that exist in the master checklist but are NOT listed under the 'Applied Checks' section of its most recent 'Log Check & Refactor Session' entry.

List the module and the specific new checks that should be retroactively applied."