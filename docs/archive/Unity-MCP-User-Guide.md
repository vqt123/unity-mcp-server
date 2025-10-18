# Unity MCP Server - User Installation Guide

## For Users: How to Install and Use

Complete guide for someone who wants to try your Unity MCP Server.

---

## Prerequisites

Before you start, make sure you have:

- ✅ **Unity Editor** 2021.3 or newer
- ✅ **Python 3.10+** (check with `python3 --version`)
- ✅ **Claude Desktop** (or another MCP-compatible client)
- ✅ **Homebrew** (macOS) for installing Python 3.11

---

## Installation Steps

### Step 1: Install Python 3.11 (if needed)

```bash
# Check your Python version
python3 --version

# If < 3.10, install Python 3.11
brew install python@3.11

# Verify
python3.11 --version
# Should show: Python 3.11.x
```

---

### Step 2: Install Python Dependencies

```bash
# Install MCP and httpx
python3.11 -m pip install mcp httpx
```

---

### Step 3: Clone the Repository

```bash
# Clone to your preferred location
cd ~/Documents  # or wherever you want
git clone https://github.com/vqt123/unity-mcp-server.git
cd unity-mcp-server
```

---

### Step 4: Install Unity Package

#### Option A: Via Unity Package Manager (Recommended)

1. Open Unity Editor
2. Open your project (or create a new one)
3. Go to **Window → Package Manager**
4. Click **+** button (top-left)
5. Select **"Add package from git URL..."**
6. Enter:
   ```
   https://github.com/vqt123/unity-mcp-server.git?path=/Packages/com.vtrinh.unitymcp
   ```
7. Click **"Add"**
8. Wait for installation (watch bottom-right spinner)

#### Option B: Manual Installation

1. Download the repository as ZIP
2. Extract it
3. Copy `Packages/com.vtrinh.unitymcp` folder
4. Paste into your Unity project's `Packages/` folder
5. Unity will automatically import it

---

### Step 5: Verify Unity Package Installation

1. Open Unity Console: **Window → General → Console**
2. Look for: `[MCP] ✅ Server started on http://localhost:8765`
3. Open MCP Window: **Window → MCP Server**
4. Should show "Running" status

**If you see errors:**
- Make sure Newtonsoft.JSON is installed (Package Manager should auto-install it)
- Try restarting Unity
- Check console for red error messages

---

### Step 6: Configure Claude Desktop

#### Find Full Path to Python Script

```bash
cd ~/Documents/unity-mcp-server/mcp-server
pwd
# Copy the output (e.g., /Users/yourname/Documents/unity-mcp-server/mcp-server)
```

#### Edit Claude Config

**macOS**: Edit `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Windows**: Edit `%APPDATA%\Claude\claude_desktop_config.json`

```bash
# macOS: Open in editor
open -a TextEdit ~/Library/Application\ Support/Claude/claude_desktop_config.json
```

Add this configuration (use YOUR full path from above):

```json
{
  "mcpServers": {
    "unity": {
      "command": "python3.11",
      "args": ["/Users/yourname/Documents/unity-mcp-server/mcp-server/unity_mcp_server.py"]
    }
  }
}
```

**Important**: Replace `/Users/yourname/...` with the actual path you copied!

---

### Step 7: Restart Claude Desktop

1. **Completely quit** Claude Desktop (Cmd+Q on Mac)
2. **Reopen** Claude Desktop
3. Start a new conversation

---

### Step 8: Test the Connection

In Claude Desktop, try these commands:

#### Test 1: Health Check
```
"Can you ping Unity to see if it's responding?"
```

**Expected**: Claude uses `unity_ping` and shows Unity version.

#### Test 2: Scene Information
```
"What scene is currently open in Unity?"
```

**Expected**: Claude uses `unity_get_scene_info` and tells you the scene name and objects.

#### Test 3: Create an Object
```
"Create a red cube in Unity at position (5, 2, 3)"
```

**Expected**: 
- Claude uses `unity_create_cube`
- A cube appears in your Unity scene at that position
- Check Unity Hierarchy and Scene view!

---

## Available Commands

Once installed, you can tell Claude to:

### Basic Operations
- "Create a cube in Unity"
- "What objects are in my scene?"
- "Is Unity compiling?"
- "Force Unity to recompile scripts"

### Scene Manipulation
- "Create 10 cubes in a row along the X axis"
- "Create a sphere at position (0, 5, 0)"
- "Tell me what's in my current Unity scene"

### Debugging
- "What errors are in the Unity console?"
- "Show me the last 20 Unity log messages"
- "Check if Unity is still responding"

### Compilation
- "Force Unity to compile"
- "Wait for Unity to finish compiling"
- "Is Unity compiling right now?"

---

## Troubleshooting

### Problem: Claude says "Unity tool unavailable"

**Solutions**:
1. Make sure Unity Editor is open
2. Check Unity Console for "[MCP] Server started..." message
3. Verify config path is correct (use absolute path)
4. Restart Claude Desktop completely
5. Check Claude logs:
   - macOS: `~/Library/Logs/Claude/`
   - Look for mcp-related errors

### Problem: Unity server not starting

**Check Unity Console for:**
- Any red error messages
- Missing Newtonsoft.JSON error → Install via Package Manager

**Try**:
1. **Window → MCP Server** → Click "Stop Server" → Click "Start Server"
2. Restart Unity Editor
3. Reimport the package

### Problem: "Connection refused" or timeout

**Solutions**:
1. Verify Unity is running (not just Hub)
2. Check MCP Server window shows "Running"
3. Test direct connection:
   ```bash
   curl -X POST http://localhost:8765/ \
     -H "Content-Type: application/json" \
     -d '{"tool":"unity_ping","args":{}}'
   ```
   Should return JSON with Unity version

### Problem: Slow responses

**Normal behavior**: First request might be slow (1-2 seconds). Subsequent requests should be < 1 second.

**If always slow (> 5 seconds)**:
1. Check if Unity is compiling (bottom-right spinner)
2. Close other Unity projects
3. Check system resources (CPU/RAM)

### Problem: Python module not found

```
Error: No module named 'mcp'
```

**Solution**:
```bash
# Make sure you're using python3.11
python3.11 -m pip install mcp httpx

# Verify installation
python3.11 -c "import mcp; print('OK')"
```

---

## Example Workflows

### Workflow 1: Quick Scene Setup

```
You: "I need a simple test scene"

Claude: Creates:
- Ground plane
- Player cube with collider
- Camera positioned above
- Directional light
```

### Workflow 2: Debug Helper

```
You: "Something's wrong with my game, can you check the Unity logs?"

Claude:
- Reads console logs
- Identifies errors
- Suggests fixes
```

### Workflow 3: Rapid Prototyping

```
You: "Create 20 cubes in a circle formation"

Claude:
- Calculates positions
- Creates cubes in loop
- Arranges them perfectly
```

---

## What You Can Do

### Currently Working (v0.1.0):
- ✅ Ping Unity (health check)
- ✅ Get scene information
- ✅ Create cubes at any position
- ✅ Force compilation
- ✅ Check compilation status
- ✅ Wait for compilation
- ✅ Read Unity console logs

### Coming Soon:
- 🔄 More GameObject types (sphere, cylinder, plane)
- 🔄 Delete objects
- 🔄 Move/rotate/scale objects
- 🔄 Add components
- 🔄 Modify properties
- 🔄 UI creation (buttons, text, canvases)
- 🔄 Visual testing

---

## Tips for Best Results

### 1. Be Specific
❌ "Create something"  
✅ "Create a red cube at position (5, 2, 3)"

### 2. One Task at a Time
❌ "Create 100 objects and arrange them and add physics and..."  
✅ "Create 10 cubes in a row" → then → "Add physics to them"

### 3. Use Unity Terms
❌ "Make a box"  
✅ "Create a cube GameObject"

### 4. Check Results
After AI creates something, look at Unity to verify it worked!

### 5. Iterate
If something isn't right, tell Claude and it will fix it.

---

## Uninstalling

### Remove Unity Package

1. Open Unity
2. Go to **Window → Package Manager**
3. Find "Unity MCP Server"
4. Click **Remove**

### Remove Python Server

```bash
rm -rf ~/Documents/unity-mcp-server
```

### Remove Claude Config

Edit `~/Library/Application Support/Claude/claude_desktop_config.json`:
- Remove the "unity" section from mcpServers
- Or delete the whole file to reset

---

## Getting Help

### Resources:
- **Documentation**: https://github.com/vqt123/unity-mcp-server/docs
- **Issues**: https://github.com/vqt123/unity-mcp-server/issues
- **Discussions**: https://github.com/vqt123/unity-mcp-server/discussions

### Before Asking for Help:

1. Check Unity Console for errors
2. Try restarting both Unity and Claude
3. Test direct connection with curl (see troubleshooting)
4. Check that all paths are correct (absolute paths!)

### When Reporting Issues:

Include:
- Unity version
- Python version (`python3.11 --version`)
- Operating system
- Error messages from Unity Console
- Error messages from Claude logs
- What you tried and what happened

---

## FAQ

### Q: Does this work on Windows?
**A**: The Unity package should work, but Python paths will be different. Not fully tested on Windows yet.

### Q: Can I use this with other AI assistants?
**A**: Yes! Any MCP-compatible client should work (not just Claude).

### Q: Is it safe?
**A**: Yes, it only runs locally on your machine. All operations support Unity's undo (Cmd/Ctrl+Z).

### Q: Does it slow down Unity?
**A**: No, minimal impact. The server runs in a background thread.

### Q: Can I use it in Play mode?
**A**: Not yet - currently only works in Edit mode.

### Q: Does it work with Unity 2020 or older?
**A**: Not tested. Requires Unity 2021.3+.

---

## Success Checklist

- [ ] Python 3.11 installed
- [ ] MCP and httpx installed
- [ ] Repository cloned
- [ ] Unity package installed
- [ ] Unity Console shows server started
- [ ] Claude config file updated with absolute path
- [ ] Claude Desktop restarted
- [ ] Successfully tested ping command
- [ ] Successfully created a cube
- [ ] Cube visible in Unity scene

**If all checked ✅ - You're ready to go!**

---

**Version**: 0.1.0  
**Last Updated**: October 10, 2025  
**Difficulty**: Beginner-Intermediate  
**Time to Install**: 15-20 minutes

