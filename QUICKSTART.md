# Unity MCP Server - Quick Start Guide

## 🎯 Two Scenarios

### Scenario A: You're the Developer (Publishing)
### Scenario B: You're a User (Installing)

---

## Scenario A: Developer (You Right Now)

### What You've Built ✅
- Unity package that runs HTTP server
- Python MCP server with 7 tools
- Working MVP that creates objects in Unity via AI

### What to Do Next 📋

#### 1. Push to GitHub
```bash
git push origin main
```

#### 2. Create a Release
1. Go to: https://github.com/vqt123/unity-mcp-server/releases
2. Click "Create a new release"
3. Tag: `v0.1.0`
4. Title: "Unity MCP Server MVP"
5. Publish

#### 3. Add Documentation to Root
Create `README.md` in project root with:
- What it does
- Installation instructions
- Quick start example
- Demo GIF/video

See: [`docs/Unity-MCP-Deployment-Guide.md`](docs/Unity-MCP-Deployment-Guide.md) for templates

#### 4. Share It 📢
- Unity Forums
- Reddit (r/Unity3D, r/gamedev)
- Twitter (#unity3d #AI)
- Discord servers
- Record demo video

---

## Scenario B: User (Someone Testing Your Package)

### Installation Steps

#### 1. Prerequisites
- Unity 2021.3+
- Python 3.10+
- Claude Desktop

#### 2. Install Python Dependencies
```bash
brew install python@3.11
python3.11 -m pip install mcp httpx
```

#### 3. Clone Repository
```bash
git clone https://github.com/vqt123/unity-mcp-server.git
cd unity-mcp-server
```

#### 4. Install Unity Package
In Unity Package Manager, add:
```
https://github.com/vqt123/unity-mcp-server.git?path=/Packages/com.vtrinh.unitymcp
```

#### 5. Configure Claude Desktop
Edit `~/Library/Application Support/Claude/claude_desktop_config.json`:
```json
{
  "mcpServers": {
    "unity": {
      "command": "python3.11",
      "args": ["/full/path/to/unity-mcp-server/mcp-server/unity_mcp_server.py"]
    }
  }
}
```

#### 6. Test
Open Unity, restart Claude Desktop, then in Claude:
```
"Create a cube in Unity at position (5, 2, 3)"
```

Full guide: [`docs/Unity-MCP-User-Guide.md`](docs/Unity-MCP-User-Guide.md)

---

## What Works Right Now ✅

### 7 Tools Available:
1. `unity_ping` - Health check
2. `unity_get_scene_info` - Scene information
3. `unity_create_cube` - Create cubes at any position
4. `unity_force_compile` - Force script compilation
5. `unity_is_compiling` - Check compilation status
6. `unity_wait_for_compile` - Wait for compilation
7. `unity_get_logs` - Read Unity console logs

### Example Uses:
- "Create 10 cubes in a row"
- "What's in my Unity scene?"
- "Check Unity console for errors"
- "Force Unity to recompile"

---

## Current Status 🚀

**Version**: 0.1.0 MVP  
**Status**: ✅ Working!  
**Response Time**: < 1 second  
**Tested On**: Unity 6000.2.6f2, Python 3.11, macOS

---

## Files You Created

### Unity Package
```
Packages/com.vtrinh.unitymcp/
  ├── package.json
  └── Editor/UnityMCP/
      ├── MCPServer.cs         (HTTP server)
      ├── MCPTools.cs          (Tool implementations)
      └── MCPServerWindow.cs   (UI window)
```

### Python MCP Server
```
mcp-server/
  ├── unity_mcp_server.py  (MCP server)
  ├── requirements.txt     (Dependencies)
  └── README.md           (Instructions)
```

### Documentation
```
docs/
  ├── Unity-MCP-MVP-Guide.md            (Implementation guide)
  ├── Unity-MCP-Deployment-Guide.md     (For developers)
  ├── Unity-MCP-User-Guide.md          (For end users)
  ├── Unity-MCP-Server-Design.md       (Full architecture)
  ├── Unity-MCP-UI-System-Design.md    (Future: UI tools)
  └── Unity-MCP-UI-Testing-Validation.md (Future: testing)
```

---

## Next Steps

### For Developer:
1. Create root README.md
2. Add LICENSE file
3. Record demo video
4. Create GitHub release
5. Share with community

### For Users:
1. Follow [User Guide](docs/Unity-MCP-User-Guide.md)
2. Install and test
3. Report issues
4. Share feedback

---

## Support

- **GitHub Issues**: https://github.com/vqt123/unity-mcp-server/issues
- **Documentation**: See `docs/` folder
- **Email**: vqt123@gmail.com

---

**That's it! You have a working AI-controlled Unity system! 🎉**

