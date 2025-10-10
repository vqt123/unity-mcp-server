# Unity MCP Server

> Enable AI assistants to interact with Unity Editor through the Model Context Protocol

## Overview

Unity MCP Server is a bridge between AI assistants (like Claude) and Unity Editor, allowing natural language control over Unity development workflows. Create GameObjects, modify scripts, manage scenes, and debug—all through AI conversation.

### Example Interactions

```
You: "Create a simple platformer scene with a player and 3 platforms"
AI: ✅ Creates ground plane, player cube with physics, and 3 floating platforms

You: "The player isn't jumping. Check the PlayerController script"
AI: ✅ Reads script, identifies input issue, fixes code, tests in play mode

You: "Organize all materials into folders by color"
AI: ✅ Scans materials, creates color-based folders, moves assets
```

## Quick Start

### Prerequisites

- Unity 2021.3 LTS or newer
- Python 3.9+ or Node.js 18+
- Claude Desktop or compatible MCP client

### Installation

#### 1. Install Unity Package

```bash
# Via Unity Package Manager (Git URL)
https://github.com/yourcompany/unity-mcp.git

# Or manual installation
# Download and copy to Packages/com.yourcompany.mcp/
```

#### 2. Install MCP Server

```bash
# Python
pip install unity-mcp-server

# Or Node.js
npm install -g unity-mcp-server
```

#### 3. Configure Claude Desktop

Edit `~/Library/Application Support/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "unity": {
      "command": "python",
      "args": ["-m", "unity_mcp_server"]
    }
  }
}
```

#### 4. Start Unity Server

1. Open Unity Editor
2. Go to **Window → MCP Server**
3. Click **Start Server**
4. Verify: Console shows "Server started on http://localhost:8765"

### First Test

Open Claude Desktop and try:

```
"Create a red cube at position (0, 5, 0) with a Rigidbody component"
```

## Documentation

### 📚 Core Documentation

| Document | Description |
|----------|-------------|
| [**Design Document**](Unity-MCP-Server-Design.md) | Complete architectural design, use cases, and development roadmap |
| [**Implementation Guide**](Unity-MCP-Implementation-Guide.md) | Step-by-step implementation with code examples |
| [**Tool Catalog**](Unity-MCP-Tool-Catalog.md) | Complete reference of all 46 MCP tools |

### 📖 Quick Links

- [Architecture Overview](#architecture)
- [Core Capabilities](#core-capabilities)
- [Security & Safety](#security--safety)
- [Development Roadmap](#development-roadmap)
- [Contributing](#contributing)

## Architecture

```
┌─────────────────────────────────┐
│      AI Assistant (Claude)       │
│                                  │
└────────────┬────────────────────┘
             │ MCP Protocol
             ↓
┌─────────────────────────────────┐
│    MCP Server (Python/Node)     │
│    - Tool Registry              │
│    - Request Router             │
└────────────┬────────────────────┘
             │ HTTP/WebSocket
             ↓
┌─────────────────────────────────┐
│   Unity Editor Plugin (C#)      │
│   - HTTP Server                 │
│   - Command Executor            │
│   - Safety Validator            │
└────────────┬────────────────────┘
             │ Unity API
             ↓
┌─────────────────────────────────┐
│         Unity Editor            │
└─────────────────────────────────┘
```

### How It Works

1. **AI Assistant** sends tool request via MCP protocol
2. **MCP Server** validates and forwards to Unity
3. **Unity Plugin** executes command on main thread
4. **Result** flows back to AI through the chain

## Core Capabilities

### 🎮 Scene Management
- Load, save, create scenes
- Get scene hierarchy
- Switch between scenes

### 🎯 GameObject Operations
- Create, delete, find, modify GameObjects
- Manage transforms and hierarchy
- Batch operations

### 🔧 Component Manipulation
- Add/remove components
- Get/set component properties
- Query component data

### 📝 Script Management
- Read, create, modify C# scripts
- Validate before compilation
- Safe script execution

### 🎨 Asset Operations
- Create materials, textures, prefabs
- Import external assets
- Organize asset folders

### ⚡ Editor Control
- Play/pause/stop play mode
- Execute menu commands
- Read console logs

### 🏗️ Build & Settings
- Configure build settings
- Modify player settings
- Trigger builds

## Tool Examples

### Create a GameObject

```python
# AI uses this tool
unity_create_gameobject(
    name="Player",
    position=[0, 0, 0],
    components=["Rigidbody", "BoxCollider", "PlayerController"],
    tag="Player"
)
```

### Modify a Script

```python
# AI reads the script
content = unity_read_script(path="Assets/Scripts/PlayerController.cs")

# AI modifies it
unity_modify_script(
    path="Assets/Scripts/PlayerController.cs",
    content=modified_content,
    validate=True
)
```

### Debug with Console

```python
# AI checks for errors
messages = unity_get_console(types=["error", "warning"])

# AI analyzes and suggests fixes
```

## Security & Safety

### 🔒 Built-in Protections

1. **Localhost Only**: Server only accepts connections from 127.0.0.1
2. **Operation Validation**: Protected paths cannot be modified/deleted
3. **Code Safety**: Scripts scanned for dangerous patterns
4. **Undo Support**: All operations support Unity's undo system
5. **Dry Run Mode**: Preview changes before applying
6. **Rate Limiting**: Prevent runaway operations
7. **Operation Logging**: Full audit trail

### Example Safety Features

```csharp
// Protected paths
Assets/Settings/
Assets/Editor Default Resources/

// Blocked code patterns
System.IO.File.Delete
System.Diagnostics.Process.Start
System.Reflection.Assembly.Load

// Required confirmations
Delete operations: confirm=true
Destructive changes: dryRun preview first
```

### User Controls

**MCP Server Window** (Window → MCP Server):
- Start/Stop server
- View operation history
- Configure settings
- Clear logs

## Use Cases

### 🚀 Rapid Prototyping

**Before**: Manually create scene, GameObjects, add components
**With MCP**: "Create a racing game scene with a track, car, and 3 checkpoints"

### 🐛 Intelligent Debugging

**Before**: Read logs, search code, manually fix
**With MCP**: "My player controller isn't working, fix it" → AI reads logs, analyzes code, applies fix

### 📦 Asset Management

**Before**: Manually organize hundreds of assets
**With MCP**: "Organize all materials by type and color"

### 🎓 Learning Assistant

**Before**: Read documentation, trial and error
**With MCP**: "Show me how to set up a character controller with animations"

### 🧪 Automated Testing

**Before**: Manual test scene setup
**With MCP**: "Create a test scene for physics collision detection"

## Development Roadmap

### ✅ Phase 1: MVP (Weeks 1-2)
- Basic HTTP server
- 5 core tools
- Error handling
- **Goal**: AI can create GameObjects

### 🔄 Phase 2: Core Tools (Weeks 3-4)
- Scene management (10 tools)
- GameObject operations (15 tools)
- Component manipulation (10 tools)
- **Goal**: Build simple scenes from scratch

### 📅 Phase 3: Advanced Features (Weeks 5-6)
- Asset operations (15 tools)
- Prefab system (8 tools)
- Editor control (5 tools)
- **Goal**: Professional workflow support

### 🚀 Phase 4: Polish (Weeks 7-8)
- Documentation
- Performance optimization
- Security audit
- **Goal**: Production ready

### Future Enhancements
- Visual Scripting integration
- Animation system support
- Shader Graph manipulation
- UI system automation
- Version control integration

## Performance

### Benchmarks

| Operation | Response Time | Notes |
|-----------|---------------|-------|
| Read operations | < 100ms | Script reading, queries |
| Write operations | < 500ms | GameObject creation, modifications |
| Script validation | < 2s | Compilation check |
| Scene operations | < 1s | Load/save scenes |

### Optimization Tips

1. Use batch operations when possible
2. Enable caching for repeated queries
3. Limit console message retrieval count
4. Use `dryRun` for preview before bulk operations

## Troubleshooting

### Server Won't Start

**Symptom**: "Failed to start server: Port already in use"
**Solution**: 
1. Check if another instance is running
2. Change port in MCP Server Window
3. Restart Unity Editor

### Connection Timeout

**Symptom**: "Request to Unity timed out"
**Solution**:
1. Ensure Unity Editor is running
2. Check MCP Server Window shows "Running"
3. Verify firewall isn't blocking localhost:8765
4. Restart MCP server

### Compilation Errors

**Symptom**: "Type 'Newtonsoft.Json.JObject' not found"
**Solution**:
1. Open Package Manager
2. Add package: `com.unity.nuget.newtonsoft-json`
3. Wait for compilation

### AI Can't Connect

**Symptom**: AI says "Unity tool unavailable"
**Solution**:
1. Restart Claude Desktop
2. Verify config file syntax
3. Check MCP server is installed: `pip list | grep unity-mcp`

## API Reference

See [Tool Catalog](Unity-MCP-Tool-Catalog.md) for complete API documentation of all 46 tools.

### Tool Categories

- **Scene**: 5 tools
- **GameObject**: 5 tools  
- **Component**: 6 tools
- **Script**: 6 tools
- **Asset**: 7 tools
- **Editor**: 7 tools
- **Build**: 5 tools
- **Project**: 2 tools
- **Utility**: 3 tools

## Contributing

We welcome contributions! Here's how to help:

### Development Setup

```bash
# Clone repository
git clone https://github.com/yourcompany/unity-mcp
cd unity-mcp

# Install development dependencies
pip install -e ".[dev]"

# Run tests
pytest tests/

# Build Unity package
./build_package.sh
```

### Contribution Areas

1. **New Tools**: Add tools for Unity features
2. **Bug Fixes**: Fix issues in tool execution
3. **Documentation**: Improve guides and examples
4. **Testing**: Add test coverage
5. **Performance**: Optimize tool execution

### Submission Process

1. Fork the repository
2. Create feature branch: `git checkout -b feature/my-tool`
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit pull request with description

## FAQ

### Q: Is it safe to let AI control my Unity project?

**A**: Yes, with built-in safety measures:
- All operations support undo
- Protected paths cannot be modified
- Dangerous code patterns are blocked
- Operations are logged
- You can review before applying

### Q: Does it work with Unity Cloud Build?

**A**: The MCP server runs locally in Unity Editor. For CI/CD, consider command-line tools instead.

### Q: Can I use it with multiple Unity projects?

**A**: Yes, start the MCP server in each Unity instance. Configure different ports in MCP Server Window.

### Q: Does it slow down Unity Editor?

**A**: Minimal impact. The HTTP server runs in a background thread. Commands execute on the main thread only when needed.

### Q: What about Unity versions < 2021.3?

**A**: May work but not officially supported. Requires .NET Standard 2.1 support.

### Q: Can I extend it with custom tools?

**A**: Yes! See Implementation Guide for creating custom commands.

## Examples

### Example 1: Quick Scene Setup

```
User: Create a simple scene with ground, player, and lighting

AI Actions:
1. unity_create_gameobject(name="Ground", components=["MeshRenderer", "MeshCollider"])
2. unity_set_component_property(target="Ground", component="MeshFilter", property="mesh", value="Plane")
3. unity_create_gameobject(name="Player", position=[0,1,0], components=["Rigidbody", "CapsuleCollider"])
4. unity_create_gameobject(name="Directional Light", components=["Light"])
5. unity_save_scene(path="Assets/Scenes/QuickTest.unity")

Result: Ready-to-test scene in seconds
```

### Example 2: Debug Assistance

```
User: My character isn't moving when I press WASD

AI Actions:
1. unity_get_console() - Check for errors
2. unity_find_gameobject(name="Player") - Locate player
3. unity_get_component(target="Player", type="PlayerController") - Read component
4. unity_read_script(path="Assets/Scripts/PlayerController.cs") - Analyze code
5. [AI identifies missing Input System package]
6. Suggests solution to user

Result: Issue identified and fix suggested
```

### Example 3: Asset Organization

```
User: Organize my materials - put metals in one folder, fabrics in another

AI Actions:
1. unity_list_assets(type="Material") - Get all materials
2. For each material:
   - unity_get_component(material, properties) - Check properties
   - Analyze shader/properties to determine type
3. unity_create_folder("Assets/Materials/Metals")
4. unity_create_folder("Assets/Materials/Fabrics")
5. Move materials to appropriate folders

Result: Clean, organized asset structure
```

## Community

- **Discord**: [Join our server](https://discord.gg/unity-mcp)
- **GitHub**: [Report issues](https://github.com/yourcompany/unity-mcp/issues)
- **Twitter**: [@UnityMCP](https://twitter.com/UnityMCP)
- **Forum**: [Unity Discussions](https://forum.unity.com/tags/mcp)

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Credits

- Built with [Model Context Protocol](https://modelcontextprotocol.io)
- Powered by [Unity Editor API](https://docs.unity3d.com/ScriptReference/)
- Inspired by the need to make game development more accessible

## Support

- 📧 Email: support@yourcompany.com
- 💬 Discord: [Unity MCP Server](https://discord.gg/unity-mcp)
- 📖 Docs: [Full Documentation](Unity-MCP-Server-Design.md)
- 🐛 Issues: [GitHub Issues](https://github.com/yourcompany/unity-mcp/issues)

---

**Version**: 1.0.0  
**Last Updated**: October 10, 2025  
**Status**: Planning Phase  
**Maintainers**: Unity MCP Team

Made with ❤️ for the Unity community

