# Unity MCP Server - Deployment Guide

## For Developers: How to Share Your Package

### Option 1: GitHub Releases (Recommended for Public Use)

#### Step 1: Push to GitHub

```bash
cd /Users/vinhtrinh/code/mcptest
git push origin main
```

#### Step 2: Create a Release

1. Go to: https://github.com/vqt123/unity-mcp-server/releases
2. Click **"Create a new release"**
3. Tag version: `v0.1.0`
4. Title: `Unity MCP Server MVP - v0.1.0`
5. Description:
   ```markdown
   # Unity MCP Server - MVP Release
   
   Control Unity Editor through AI (Claude Desktop) via Model Context Protocol.
   
   ## Features
   - ✅ HTTP server in Unity Editor
   - ✅ 7 working tools (ping, scene info, create objects, compilation control, logs)
   - ✅ Fast responses (< 1 second)
   - ✅ Queue-based main thread execution
   - ✅ Self-diagnostic capability
   
   ## Installation
   See [Installation Guide](docs/Unity-MCP-User-Guide.md)
   
   ## Tools Available
   - `unity_ping` - Health check
   - `unity_get_scene_info` - Scene information
   - `unity_create_cube` - Create GameObjects
   - `unity_force_compile` - Force script compilation
   - `unity_is_compiling` - Check compilation status
   - `unity_wait_for_compile` - Wait for compilation to finish
   - `unity_get_logs` - Read Unity console logs
   ```
6. Click **"Publish release"**

#### Step 3: Share Installation Instructions

Users can now install via:
```
https://github.com/vqt123/unity-mcp-server.git
```

---

### Option 2: Unity Package Manager Registry (For Organizations)

If you want to publish to npm/scoped registry:

1. **Add repository field to package.json**:
```json
{
  "name": "com.vtrinh.unitymcp",
  "repository": {
    "type": "git",
    "url": "https://github.com/vqt123/unity-mcp-server.git"
  }
}
```

2. **Publish to OpenUPM** (optional):
   - Submit to https://openupm.com/
   - Follow their submission process
   - Users can install via OpenUPM CLI

---

### Option 3: Asset Store (For Wider Distribution)

1. Create Publisher account on Unity Asset Store
2. Package your plugin
3. Submit for review
4. Monetize or distribute free

---

## Python MCP Server Distribution

### Option 1: PyPI (Recommended)

Create a proper Python package structure:

```
unity-mcp-server/
  setup.py
  README.md
  unity_mcp_server/
    __init__.py
    server.py  (your current unity_mcp_server.py)
```

**setup.py**:
```python
from setuptools import setup, find_packages

setup(
    name="unity-mcp-server",
    version="0.1.0",
    packages=find_packages(),
    install_requires=[
        "mcp>=0.9.0",
        "httpx>=0.24.0",
    ],
    python_requires=">=3.10",
    author="Vinh Trinh",
    author_email="vqt123@gmail.com",
    description="Model Context Protocol server for Unity Editor",
    long_description=open("README.md").read(),
    long_description_content_type="text/markdown",
    url="https://github.com/vqt123/unity-mcp-server",
    entry_points={
        "console_scripts": [
            "unity-mcp-server=unity_mcp_server.server:main",
        ],
    },
)
```

**Publish**:
```bash
python -m pip install build twine
python -m build
twine upload dist/*
```

**Users install via**:
```bash
pip install unity-mcp-server
```

### Option 2: Direct from GitHub (Current, Simple)

Users can install directly:
```bash
pip install git+https://github.com/vqt123/unity-mcp-server.git#subdirectory=mcp-server
```

---

## What You Should Do Now (Checklist)

### As Developer:

- [x] ✅ Code is working
- [x] ✅ Code is committed to git
- [ ] 📝 Create comprehensive README.md in root
- [ ] 📝 Add LICENSE file (MIT recommended)
- [ ] 📝 Create CHANGELOG.md
- [ ] 🎬 Record demo video showing it working
- [ ] 📸 Add screenshots to README
- [ ] 🚀 Push to GitHub
- [ ] 🏷️ Create v0.1.0 release
- [ ] 📢 Share on:
  - Unity Forums
  - Reddit r/Unity3D
  - Twitter
  - Discord servers

---

## Documentation You Should Create

### 1. Root README.md

Create `/Users/vinhtrinh/code/mcptest/README.md`:

```markdown
# Unity MCP Server

Control Unity Editor through AI using the Model Context Protocol.

![Demo](docs/images/demo.gif)

## Features

- 🤖 Control Unity through Claude Desktop (or any MCP client)
- ⚡ Fast responses (< 1 second)
- 🛡️ Safe main thread execution
- 📊 Self-diagnostic with log reading
- 🔧 7 essential tools

## Quick Start

### 1. Install Unity Package

Add via Package Manager:
\`\`\`
https://github.com/vqt123/unity-mcp-server.git?path=/Packages/com.vtrinh.unitymcp
\`\`\`

### 2. Install Python MCP Server

\`\`\`bash
pip3.11 install mcp httpx
git clone https://github.com/vqt123/unity-mcp-server.git
\`\`\`

### 3. Configure Claude Desktop

Edit \`~/Library/Application Support/Claude/claude_desktop_config.json\`:

\`\`\`json
{
  "mcpServers": {
    "unity": {
      "command": "python3.11",
      "args": ["/full/path/to/unity-mcp-server/mcp-server/unity_mcp_server.py"]
    }
  }
}
\`\`\`

### 4. Test

Open Unity, then in Claude Desktop:
"Create a cube in Unity at position (5, 2, 3)"

## Documentation

- [User Installation Guide](docs/Unity-MCP-User-Guide.md)
- [MVP Implementation Guide](docs/Unity-MCP-MVP-Guide.md)
- [Full Design Documentation](docs/Unity-MCP-Server-Design.md)

## Requirements

- Unity 2021.3+
- Python 3.10+
- Claude Desktop (or MCP-compatible client)

## License

MIT License - see [LICENSE](LICENSE)
\`\`\`

### 2. Create LICENSE File

```bash
# MIT License is most common for open source
curl https://choosealicense.com/licenses/mit/ > LICENSE
# Edit to add your name and year
```

### 3. Add .gitignore for Python

Create `/Users/vinhtrinh/code/mcptest/.gitignore`:
```
# Python
mcp-server/__pycache__/
mcp-server/*.pyc
mcp-server/.pytest_cache/
mcp-server/dist/
mcp-server/build/
mcp-server/*.egg-info/
```

---

## Marketing Your Package

### Where to Share:

1. **Unity Forums**
   - https://forum.unity.com/
   - Post in "Assets and Asset Store" section

2. **Reddit**
   - r/Unity3D
   - r/gamedev
   - r/IndieDev

3. **Twitter/X**
   - Tag #unity3d #gamedev #AI #MCP
   - Show demo video
   - Tag @unity

4. **Discord Servers**
   - Unity Developer Community
   - Game Dev League
   - Indie Game Developers

5. **YouTube**
   - Create tutorial video
   - Show real use cases

### Demo Ideas:

- "Create a game scene using only voice commands"
- "Debug Unity issues by talking to AI"
- "Generate 100 objects in seconds"
- "AI builds a platformer level"

---

## Next Steps for Future Versions

### v0.2.0 - More GameObject Tools
- Delete objects
- Move/rotate/scale
- Add more primitive types
- Component management

### v0.3.0 - UI Tools
- Canvas creation
- Button creation
- Text creation
- Basic layouts

### v0.4.0 - Screenshots & Testing
- Screenshot capture
- Visual verification
- Automated testing

### v1.0.0 - Production Ready
- 50+ tools
- UI system
- Testing suite
- Professional polish

---

## Support

- **Issues**: https://github.com/vqt123/unity-mcp-server/issues
- **Discussions**: https://github.com/vqt123/unity-mcp-server/discussions
- **Email**: vqt123@gmail.com

---

**Version**: 0.1.0  
**Last Updated**: October 10, 2025  
**Status**: MVP - Working!

