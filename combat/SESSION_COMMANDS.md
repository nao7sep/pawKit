# Session Commands

*Direct commands for managing development work. No fluff, no explanations you don't need.*

## Design Rationale

**Why embed logging instructions in each work log file?**

Because you'll forget the rules otherwise. Logging instructions are embedded in each work log file so when you inevitably change the format later, you won't break historical consistency. This isn't rocket science - it's basic version control thinking.

## [Start New Version] - Don't Screw This Up

**Usage:** When you're ready to increment the version. Make sure you actually finished something first.

**Command:**
```
Start a new version. Current version is [X.Y.Z]. Increment [Major|Minor|Patch].
```

**What the AI will do (and you better not interfere):**

1. **Calculate New Version:** Basic semantic versioning. If you don't know what Major/Minor/Patch means, go read the spec.

2. **Update Project Files:** Replace version strings in `.csproj`, `README.md`, and other files that matter. The AI won't touch historical documents because it's not stupid.

3. **Create New Work Log:**
   - Create `WORK_LOG_v<new_version>.md`
   - Copy logging instructions from the previous work log (because consistency matters)
   - Start with empty log entries (obviously)

**Don't:**
- Ask the AI to explain semantic versioning
- Manually edit version numbers after giving this command
- Create the work log file yourself

## [Add Log Entry] - Document Your Work

**Usage:** At the end of any session where you actually accomplished something worth recording.

**Command:**
```
Summarize this entire conversation and add a new log entry to the work log.
```

**What the AI will do:**

1. **Read the Work Log Instructions:** It will follow the embedded rules, not make up its own format.

2. **Synthesize the Session:** It will create a summary of what actually got done, ignoring your false starts and dead ends.

3. **Add Entry:** New entry goes at the top of the log, following the format rules.

**Don't:**
- Ask for a summary without adding it to the log
- Try to dictate the summary format (the work log has rules for a reason)
- Add entries for sessions where nothing meaningful happened

## [Emergency Session Recovery] - When You Mess Up

**Usage:** When you've lost track of what you were doing or need to reconstruct a session.

**Command:**
```
Analyze the current state of the project and tell me what needs to be done next.
```

**What the AI will do:**
- Read recent work logs to understand context
- Examine current code state
- Identify incomplete work or broken functionality
- Give you a prioritized list of next actions

**This is not:**
- A substitute for proper planning
- An excuse to work without direction
- A way to avoid reading your own work logs

## [Code Quality Audit] - Face the Truth

**Usage:** When you suspect your code is garbage and need confirmation.

**Command:**
```
Load FOUNDATIONS.md and REFACTORING.md. Audit the current codebase for violations.
```

**What the AI will do:**
- Apply all quality checks systematically
- List every violation it finds
- Prioritize fixes by impact
- Give you specific remediation steps

**Warning:** This will hurt. The AI will find problems you didn't know existed. Deal with it.

## [Architecture Review] - Before You Dig Deeper

**Usage:** Before making significant changes to ensure you're not about to make things worse.

**Command:**
```
Load FOUNDATIONS.md and IMPLEMENTATION.md. Review the proposed changes for architectural soundness.
```

**What the AI will do:**
- Challenge your assumptions
- Identify potential coupling issues
- Suggest better approaches
- Tell you if your plan is fundamentally flawed

**This is mandatory for:**
- Adding new major features
- Changing core abstractions
- Refactoring multiple components

## Anti-Patterns - Don't Do These

**Don't ask for "help"** - Be specific about what you need
**Don't ask "what do you think?"** - State your position and ask for critique
**Don't provide context dumps** - Give the AI specific files to read
**Don't ignore the AI's recommendations** - If you disagree, argue with logic, not feelings

The AI is not your cheerleader. It's your brutal, and occasionally amusing, code reviewer. Use it accordingly.