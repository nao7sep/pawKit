# Cortex Workflow Prompts

*This file contains standardized prompt templates for common development tasks. Use these to ensure consistent interactions with AI assistants.*

## [Start New Version Session]

**Objective:** To bump the version number consistently across the entire solution and prepare a new work log for the upcoming development cycle.

**Prompt Template:** Start a new versioning session. The current version is `[Specify Current Version, e.g., 0.2]`. The segment to increment is `[Major|Minor|Patch|Default: Minor]`.

*Instructions for AI:*
1.  **Calculate New Version:** Based on the current version and the segment to increment, determine the new semantic version number (`Major.Minor.Patch`).
    -   If the provided version is incomplete (e.g., `0.2`), treat it as `0.2.0`.
    -   Incrementing `Major` or `Minor` resets subsequent segments to zero (e.g., a `Minor` bump on `0.2.1` results in `0.3.0`).
    -   If no segment is specified, default to a **Minor** increment.

2.  **Update All Files:** Search the repository and replace the old version string with the new one in key locations like `.csproj` files and general documentation (`README.md`). Do not modify historical documents where the version is part of the filename. Generate a diff for each modified file.

3.  **Create New Work Log:** Create a new file named `WORK_LOG_v<new_version>.md`. Copy the template/header from the most recent work log file, but omit the specific log entries from the previous version. The new file should be a clean slate, ready for new entries under the 'Detailed Log Entries' section.

## [Log Implementation Session]

**Objective:** Generate a work log entry after implementing a new feature or making significant changes. The entry should be a concise, factual summary of the work completed.

**Prompt Template:** Based on our conversation, generate a new entry for the `WORK_LOG_vX.Y.Z.md`.

*Instructions for AI:*
- If a text summary of previous work is provided, treat it as context that occurred *before* this conversation.
- Synthesize the entire session's history into a summary of the **final state**. Do not log intermediate or reversed decisions.
- Generate a new entry in Markdown format, preceded by a `---` separator.
- The entry must start with a level 3 heading (`###`) containing today's date (YYYY-MM-DD) and a concise title for the work, inferred from the conversation context.
  - Example: `### 2025-07-07 - Initial Logging Implementation`
- Below the heading, generate the following sections:
  - **Summary:** A bulleted list detailing what was implemented.
  - **Key Architectural Decisions:** A bulleted list of any significant patterns or choices made. If none, state "N/A".

Append the generated entry to the top of the 'Detailed Log Entries' section in the work log file.

## [Log Check & Refactor Session]

**Objective:** Generate a work log entry after completing a quality and refactoring audit on a module.

**Prompt Template:** Based on our conversation, generate a new entry for the `WORK_LOG_vX.Y.Z.md`.

*Instructions for AI:*
- Generate a new entry in Markdown format, preceded by a `---` separator.
- The entry must start with a level 3 heading (`###`) containing today's date (YYYY-MM-DD) and a concise title for the session, inferred from the conversation context.
  - Example: `### 2025-07-08 - Quality Audit and Refinement for pawKitLib.Logging`
- Below the heading, generate the following section:
  - **Audit & Refinements:** Create a bulleted list. Each bullet point should summarize the outcome of a specific check from the `REFACTORING_CHECKLIST.md`. Describe any code changes made as a result of the check, or confirm that no issues were found.
    - Example:
      - "Verified adherence to the Single Responsibility Principle by extracting message formatting logic into a new `DefaultLogFormatter` class."
      - "Confirmed that each file contains only one public type and that filenames match their types; no changes were needed."

Append the generated entry to the top of the 'Detailed Log Entries' section in the work log file.

## [Audit for Checklist Debt]

**Objective:** Compare the master `REFACTORING_CHECKLIST.md` against a specific module's "Applied Checks" in the work log to find any new checks that need to be retroactively applied.

**Prompt Template:** Analyze the master `REFACTORING_CHECKLIST.md` and the `WORK_LOG_vX.Y.Z.md`.

For the module `[Specify Module Name]`, identify any checks that exist in the master checklist but are NOT listed under the 'Applied Checks' section of its most recent 'Log Check & Refactor Session' entry.

List the module and the specific new checks that should be retroactively applied.