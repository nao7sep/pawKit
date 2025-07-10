# Cortex Workflow Prompts

*This file contains the two essential prompt templates for managing work logs in AI-assisted development.*

## Design Rationale

**Why are logging instructions embedded in each work log file?**

Logging instructions are embedded in each work log file (rather than centralized in a separate file) because logging rules may evolve over time. If instructions were in one shared file and later updated, the link between the rules and existing log entries would be lost. By embedding instructions in each versioned work log, we maintain a permanent record of which formatting rules were used for each set of entries, ensuring historical consistency and traceability.

## [Start New Version]

**Usage:** When starting a new version of the project.

**Prompt Template:**
```
Start a new version. Current version is [X.Y.Z]. Increment [Major|Minor|Patch].
```

**Instructions for AI:**
1. **Calculate New Version:** Determine the new semantic version number based on the increment type
2. **Update Project Files:** Replace version strings in `.csproj`, `README.md`, and other general files (avoid version-stamped historical documents)
3. **Create New Work Log:**
   - Create `WORK_LOG_v<new_version>.md`
   - Copy the "AI Instructions for Adding New Log Entries" section from the previous work log
   - Start with an empty "Development Log Entries" section

## [Add Log Entry]

**Usage:** At the end of any development session to summarize the work completed.

**Prompt Template:**
```
Summarize this entire conversation and add a new log entry to the work log.
```

**Instructions for AI:**
1. **Read the Work Log Instructions:** Follow the "AI Instructions for Adding New Log Entries" section in the current work log file
2. **Synthesize the Session:** Review the entire conversation and create a summary of the final state (ignore intermediate steps or reversed decisions)
3. **Add Entry:** Create a new entry at the top of the "Development Log Entries" section following the embedded format guidelines

**Note:** The work log contains comprehensive instructions for entry format, content guidelines, and placement rules. Always refer to those instructions rather than duplicating them here.