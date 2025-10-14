# Cursor AI Rules for Unity MCP Server

This folder contains rules and guidelines for AI assistants working on the Unity MCP Server project.

## Rule Files

### 🎯 [quick-reference.mdc](quick-reference.mdc)
**Start here.** Fast lookup for common tasks, current status, and critical rules.

**Use when:**
- Starting work on the project
- Need quick syntax/pattern reference
- Troubleshooting common issues
- Adding a new tool (fast path)

### 🏗️ [unity-mcp-core.mdc](unity-mcp-core.mdc)
**Architecture and core principles.** Main thread execution, HTTP server design, tool patterns.

**Use when:**
- Understanding project architecture
- Making structural changes
- Threading/async questions
- Assembly reload handling
- Adding complex features

### 📸 [visual-feedback-tools.mdc](visual-feedback-tools.mdc)
**Screenshot and visual feedback implementation.** Detailed guide for capture tools.

**Use when:**
- Working on screenshot features
- Adding new capture methods
- Visual validation tools
- Render pipeline questions
- Image processing features

### 🔧 [tool-development-workflow.mdc](tool-development-workflow.mdc)
**Complete guide to adding new MCP tools.** Step-by-step with examples and checklists.

**Use when:**
- Adding any new tool
- Need detailed implementation steps
- Want code examples
- Following best practices
- Creating tool categories

### 🧪 [no-automated-testing.mdc](no-automated-testing.mdc)
**Testing philosophy: Manual only.** Why no test scripts, how to test, self-debugging.

**Use when:**
- Tempted to create test scripts (don't!)
- Wondering about validation approach
- Need testing instructions
- Writing documentation
- Building self-debugging features

### 📦 [git-and-deployment.mdc](git-and-deployment.mdc)
**Git workflow, versioning, and deployment.** Commits, releases, package distribution.

**Use when:**
- Committing code
- Creating releases
- Updating versions
- Publishing package
- Writing changelogs

### 🎨 [unity-ui-best-practices.mdc](unity-ui-best-practices.mdc)
**Unity UI development rules.** Critical: UI Images MUST have sprites! UI patterns, common issues.

**Use when:**
- Creating any UI elements
- Working with Unity Canvas/Image components
- Debugging invisible UI
- Creating progress bars, buttons, panels
- Using fill animations

## Quick Navigation

### I want to...

#### Add a new tool
1. Read: [quick-reference.mdc](quick-reference.mdc) - "Adding a New Tool (Fast)"
2. Details: [tool-development-workflow.mdc](tool-development-workflow.mdc)
3. Test: [no-automated-testing.mdc](no-automated-testing.mdc)

#### Create UI elements
1. Read: [unity-ui-best-practices.mdc](unity-ui-best-practices.mdc) - "CRITICAL: Sprites"
2. Patterns: [unity-ui-best-practices.mdc](unity-ui-best-practices.mdc) - "Common UI Patterns"
3. Debug: [unity-ui-best-practices.mdc](unity-ui-best-practices.mdc) - "Debugging Invisible UI"

#### Understand architecture
1. Read: [unity-mcp-core.mdc](unity-mcp-core.mdc)
2. Threading: [unity-mcp-core.mdc](unity-mcp-core.mdc) - "Main Thread Execution"
3. Quick ref: [quick-reference.mdc](quick-reference.mdc) - "Threading Model"

#### Work on screenshots
1. Read: [visual-feedback-tools.mdc](visual-feedback-tools.mdc)
2. Core principles: [unity-mcp-core.mdc](unity-mcp-core.mdc) - "Resource Cleanup"

#### Commit changes
1. Read: [git-and-deployment.mdc](git-and-deployment.mdc) - "Commit Message Format"
2. Quick ref: [quick-reference.mdc](quick-reference.mdc) - "Git Commit"

#### Test something
1. Read: [no-automated-testing.mdc](no-automated-testing.mdc)
2. Examples: [quick-reference.mdc](quick-reference.mdc) - "Common Tasks"

#### Troubleshoot
1. Start: [quick-reference.mdc](quick-reference.mdc) - "Troubleshooting"
2. Logs: Use `unity_get_logs` tool
3. Architecture: [unity-mcp-core.mdc](unity-mcp-core.mdc) - "Unity-Specific Gotchas"

## Rule Priority

When rules conflict (shouldn't happen, but):

1. **quick-reference.mdc** - Most up-to-date, highest authority
2. **Specific rule files** - Authoritative for their domain
3. **Code comments** - Implementation details
4. **User feedback** - Always overrides rules

## Contributing to Rules

When you learn something important working on the project:

1. Update relevant rule file
2. Update quick-reference.mdc if critical
3. Keep rules concise and actionable
4. Include examples
5. Commit with: `docs(rules): update X with Y learnings`

## Rules Overview

### Core Principles (All Files)

**Threading:**
- All Unity APIs on main thread
- Use ConcurrentQueue + EditorApplication.update
- Never use delayCall for HTTP

**Testing:**
- No automated test scripts
- Manual testing only
- User tests with curl or Claude Desktop

**Tool Development:**
- Update C# AND Python
- Return structured JSON
- Always try-catch
- Clear error messages

**Git:**
- Conventional commits
- Semantic versioning
- Push often
- Document in commit

**Documentation:**
- Code comments for WHY
- Tool descriptions for AI
- Examples in docs
- Update with code

### File Sizes

- quick-reference.mdc: ~16 KB (fast to read)
- unity-mcp-core.mdc: ~6 KB (comprehensive)
- visual-feedback-tools.mdc: ~6 KB (detailed)
- tool-development-workflow.mdc: ~10 KB (step-by-step)
- no-automated-testing.mdc: ~7 KB (philosophy)
- git-and-deployment.mdc: ~9 KB (procedures)
- unity-ui-best-practices.mdc: ~4 KB (UI patterns) ⭐ NEW

Total: ~58 KB of guidelines

## Using These Rules

### For AI Assistants (like me)

1. **Start every session** by reading quick-reference.mdc
2. **Before adding tools** check tool-development-workflow.mdc
3. **When stuck** search relevant rule file
4. **Follow patterns** shown in examples
5. **Update rules** when learning something new

### For Human Developers

1. **Read quick-reference.mdc** to understand project
2. **Follow workflows** in tool-development-workflow.mdc
3. **Reference as needed** during development
4. **Suggest updates** when rules are outdated
5. **Ignore AI-specific notes** (like "For AI Assistants")

## Maintenance

### Keep Rules Updated

- Review after major features
- Update when patterns change
- Remove obsolete information
- Add new learnings
- Keep examples current

### Rule Health Check

Rules are healthy when:
- ✅ Quick reference is accurate
- ✅ Examples compile and work
- ✅ No contradictions between files
- ✅ Version info is current
- ✅ New contributors can follow them

## Questions?

If rules are unclear:
1. Check other rule files
2. Look at actual code
3. Test to verify
4. Update rules with findings

## Summary

- **7 rule files** covering all aspects of development
- **quick-reference.mdc** - Start here every time
- **unity-ui-best-practices.mdc** - UI Images MUST have sprites!
- **No automated tests** - Manual testing only
- **Tools need C# + Python** - Always update both
- **Main thread execution** - Critical architecture rule
- **Push often** - Git is backup
- **AI is the user** - Design for AI consumption

---

**Current Project Status:** v0.1.0 - Screenshot capability just added  
**Next:** v0.2.0 - GameObject manipulation tools  
**Repository:** https://github.com/vqt123/unity-mcp-server

