# Unity MCP Server - Quick Reference

## One-Page Overview

### What Is It?

AI-powered control of Unity Editor through natural language conversation.

### How It Works

```
You say → AI understands → MCP translates → Unity executes → Result returns
```

### Installation (3 Steps)

```bash
# 1. Install Unity Package
Add via Package Manager: https://github.com/yourcompany/unity-mcp.git

# 2. Install MCP Server
pip install unity-mcp-server

# 3. Configure Claude
Edit: ~/Library/Application Support/Claude/claude_desktop_config.json
{
  "mcpServers": {
    "unity": {
      "command": "python",
      "args": ["-m", "unity_mcp_server"]
    }
  }
}
```

### Start Unity Server

**Window → MCP Server → Start Server**

---

## Common Commands

### Scene Setup

```
"Create a new scene called TestLevel"
"Add a ground plane and player capsule"
"Put a directional light at angle 45,30,0"
```

### GameObject Operations

```
"Create a cube at position 5,2,3"
"Find all objects tagged Enemy"
"Move the Player object to 0,10,0"
"Delete all objects tagged Temporary"
```

### Component Management

```
"Add a Rigidbody to the Player"
"Set the mass of Player's Rigidbody to 5"
"Remove the BoxCollider from Enemy"
"Show me all components on the Player"
```

### Script Operations

```
"Show me the PlayerController script"
"Create a new script called EnemyAI"
"Fix the compilation errors in GameManager"
"Add input handling to PlayerController"
```

### Debugging

```
"What errors are in the console?"
"Why isn't my player moving?"
"Check the PlayerController for issues"
"Test the scene in play mode"
```

### Asset Management

```
"Create a red material called PlayerMat"
"Make a prefab from the Enemy object"
"Organize materials into folders by type"
"Import the model from /path/to/model.fbx"
```

---

## Tool Categories (46 Total)

| Category | Count | Examples |
|----------|-------|----------|
| 🎬 Scene | 5 | Load, save, create, hierarchy |
| 🎯 GameObject | 5 | Create, delete, find, modify |
| 🔧 Component | 6 | Add, remove, get, set properties |
| 📝 Script | 6 | Read, write, create, validate |
| 🎨 Asset | 7 | Materials, prefabs, import |
| ⚡ Editor | 7 | Play mode, console, menu |
| 🏗️ Build | 5 | Settings, player config, build |
| 📊 Project | 2 | Info, search |
| 🛠️ Utility | 3 | Undo, redo, ping |

---

## Architecture

```
┌──────────────────────────────────────────────────────┐
│                   Claude Desktop                      │
│              "Create a red cube"                      │
└────────────────────┬─────────────────────────────────┘
                     │ MCP Protocol (JSON-RPC)
                     ↓
┌──────────────────────────────────────────────────────┐
│               Python/Node MCP Server                  │
│  • Translates natural language → tool calls          │
│  • Routes to Unity HTTP endpoint                     │
└────────────────────┬─────────────────────────────────┘
                     │ HTTP POST (localhost:8765)
                     ↓
┌──────────────────────────────────────────────────────┐
│              Unity Editor Plugin (C#)                 │
│  • HTTP Server listening on port 8765                │
│  • Safety validation                                 │
│  • Executes on main thread                           │
│  • Returns JSON response                             │
└────────────────────┬─────────────────────────────────┘
                     │ Unity C# API
                     ↓
┌──────────────────────────────────────────────────────┐
│                 Unity Editor                          │
│  Scene → GameObjects → Components → Assets            │
└──────────────────────────────────────────────────────┘
```

---

## Request/Response Format

### Request Example
```json
{
  "id": "req-12345",
  "tool": "unity_create_gameobject",
  "args": {
    "name": "Player",
    "position": [0, 1, 0],
    "components": ["Rigidbody", "CapsuleCollider"]
  }
}
```

### Response Example
```json
{
  "id": "req-12345",
  "success": true,
  "data": {
    "gameObjectId": 54321,
    "name": "Player",
    "path": "Player"
  },
  "error": null
}
```

---

## Safety Features

### ✅ What's Protected

- **Assets/Settings/** - Core project settings
- **Editor folders** - Unity Editor resources
- **Readonly files** - System files

### ✅ What's Blocked

- Dangerous code patterns (File.Delete, Process.Start)
- Operations without confirmation on destructive actions
- External network access
- Execution of arbitrary code

### ✅ What's Logged

- Every operation with timestamp
- Request parameters
- Results or errors
- Available in MCP Server Window

### ✅ User Controls

- Start/Stop server anytime
- Review operation history
- Undo any operation
- Dry-run preview for destructive changes

---

## Troubleshooting Flowchart

```
Can't connect to Unity?
  ├─ Is Unity Editor running? → Start Unity
  ├─ Is MCP Server running? → Window → MCP Server → Start
  ├─ Check port (default 8765) → Change if needed
  └─ Restart Claude Desktop → Reload MCP config

Operation failed?
  ├─ Check Unity Console → Read error messages
  ├─ Check MCP Server Window → View operation log
  ├─ Protected path? → Operation blocked by safety
  └─ Script error? → Fix compilation errors first

Slow response?
  ├─ Large scene? → Reduce GameObject count
  ├─ Complex operation? → Break into smaller steps
  └─ Unity compiling? → Wait for compilation

AI doesn't understand?
  ├─ Be specific → "Create cube at 0,0,0 with Rigidbody"
  ├─ Use Unity terms → "GameObject" not "object"
  └─ One action at a time → Break complex tasks
```

---

## Keyboard Shortcuts

### Unity Editor

- `Ctrl/Cmd + Shift + M` - Open MCP Server Window (custom)
- `Ctrl/Cmd + Z` - Undo (works with MCP operations)
- `Ctrl/Cmd + Shift + Z` - Redo
- `Ctrl/Cmd + P` - Toggle Play Mode

### Quick Actions

| Action | Voice Command |
|--------|---------------|
| Play | "Enter play mode" |
| Stop | "Stop play mode" |
| Save | "Save the scene" |
| Undo | "Undo that" |
| Console | "What errors do I have?" |

---

## Performance Tips

### ⚡ Fast Operations (< 100ms)

- Read scripts
- Get GameObject properties
- Query scene hierarchy
- Read console messages

### 🔄 Medium Operations (100-500ms)

- Create GameObjects
- Modify components
- Save scenes
- Import small assets

### 🐌 Slow Operations (> 500ms)

- Script compilation
- Large scene operations
- Bulk asset imports
- Project builds

### 🎯 Optimization Strategies

1. **Batch operations** - Do multiple things at once
2. **Use specific queries** - "Find Player" not "Find all"
3. **Limit results** - Get 10 console messages, not 1000
4. **Cache results** - Store info in AI context

---

## Development Status

### ✅ Phase 1: MVP (Current)
- Basic server architecture
- 5 core tools working
- Safety validation
- Error handling

### 🔄 Phase 2: Core Tools (Next)
- 40+ tools implemented
- Full GameObject management
- Complete script operations
- Asset manipulation

### 📅 Phase 3: Advanced (Future)
- WebSocket real-time updates
- Visual Scripting support
- Animation system
- Shader Graph manipulation

### 🚀 Phase 4: Polish (Future)
- Performance optimization
- Comprehensive testing
- Video tutorials
- Public release

---

## Useful Links

| Resource | URL |
|----------|-----|
| Full Design Doc | [Unity-MCP-Server-Design.md](Unity-MCP-Server-Design.md) |
| Implementation Guide | [Unity-MCP-Implementation-Guide.md](Unity-MCP-Implementation-Guide.md) |
| Complete Tool List | [Unity-MCP-Tool-Catalog.md](Unity-MCP-Tool-Catalog.md) |
| Main README | [Unity-MCP-README.md](Unity-MCP-README.md) |
| MCP Protocol Spec | https://modelcontextprotocol.io |
| Unity Scripting API | https://docs.unity3d.com/ScriptReference/ |

---

## Quick Start Checklist

- [ ] Unity 2021.3+ installed
- [ ] Python 3.9+ or Node.js 18+ installed
- [ ] Install Unity MCP package
- [ ] Install MCP server (`pip install unity-mcp-server`)
- [ ] Configure Claude Desktop config file
- [ ] Restart Claude Desktop
- [ ] Open Unity project
- [ ] Start MCP Server (Window → MCP Server)
- [ ] Test: "Create a cube in Unity"
- [ ] Verify: Cube appears in scene

---

## Example Workflows

### Workflow 1: New Scene Setup (30 seconds)

```
1. "Create a new scene called Level1"
2. "Add a ground plane with a grid texture"
3. "Create a player capsule at 0,2,0 with Rigidbody and PlayerController"
4. "Add a directional light and skybox"
5. "Save the scene"
```

### Workflow 2: Debug Player Movement (1 minute)

```
1. "What errors are in the console?"
2. "Show me the PlayerController script"
3. "The Input.GetAxis is returning 0, fix the input handling"
4. "Enter play mode and test"
5. "If it works, save the script"
```

### Workflow 3: Asset Organization (2 minutes)

```
1. "List all materials in the project"
2. "Create folders: Materials/Metal, Materials/Wood, Materials/Stone"
3. "Analyze each material and move to appropriate folder"
4. "Create a prefab for each unique material combination"
5. "Generate a report of the organization"
```

---

## Common Patterns

### Pattern: Create with Components

```python
unity_create_gameobject(
    name="Enemy",
    position=[5, 0, 5],
    components=["Rigidbody", "BoxCollider", "EnemyAI"],
    tag="Enemy"
)
```

### Pattern: Find and Modify

```python
# Find
results = unity_find_gameobject(name="Player")
player_path = results["results"][0]["path"]

# Modify
unity_modify_gameobject(
    target=player_path,
    position=[0, 10, 0]
)
```

### Pattern: Safe Script Modification

```python
# Read
content = unity_read_script("Assets/Scripts/Player.cs")

# Validate changes
validation = unity_validate_script(modified_content)

# Write if valid
if validation["valid"]:
    unity_modify_script(
        path="Assets/Scripts/Player.cs",
        content=modified_content
    )
```

---

## Version Compatibility

| Unity Version | MCP Server | Status |
|---------------|------------|---------|
| 2023.x | 1.0+ | ✅ Fully Supported |
| 2022.3 LTS | 1.0+ | ✅ Fully Supported |
| 2021.3 LTS | 1.0+ | ✅ Fully Supported |
| 2020.3 LTS | 1.0+ | ⚠️ Partial Support |
| < 2020.3 | 1.0+ | ❌ Not Supported |

---

## Support Channels

| Issue Type | Contact |
|------------|---------|
| 🐛 Bugs | [GitHub Issues](https://github.com/yourcompany/unity-mcp/issues) |
| 💬 Questions | [Discord Server](https://discord.gg/unity-mcp) |
| 📧 Enterprise | support@yourcompany.com |
| 📖 Documentation | [Full Docs](Unity-MCP-Server-Design.md) |

---

**Print this page for your desk reference!**

Version 1.0 | Last Updated: October 10, 2025

