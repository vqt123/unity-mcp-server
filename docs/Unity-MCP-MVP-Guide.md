# Unity MCP Server - MVP Implementation Guide

## Building the Simplest Possible Working System

A step-by-step guide to create a minimal Unity package that proves the MCP pipeline works end-to-end.

---

## Table of Contents

1. [Overview](#overview)
2. [What We're Building](#what-were-building)
3. [Prerequisites](#prerequisites)
4. [Step 1: Create Unity Package](#step-1-create-unity-package)
5. [Step 2: Build HTTP Server](#step-2-build-http-server)
6. [Step 3: Implement Basic Tools](#step-3-implement-basic-tools)
7. [Step 4: Create MCP Server](#step-4-create-mcp-server)
8. [Step 5: Test End-to-End](#step-5-test-end-to-end)
9. [Troubleshooting](#troubleshooting)
10. [Next Steps](#next-steps)

---

## Overview

### Goal

Create the absolute minimum viable system that demonstrates:
1. ✅ Unity can run an HTTP server
2. ✅ MCP server can talk to Unity
3. ✅ AI can control Unity through natural language
4. ✅ The basic architecture works

### What This Proves

- The communication pipeline works
- Unity's threading model doesn't block us
- MCP protocol integration is solid
- We can build on this foundation

### Time to Complete

**1-2 hours** for someone familiar with Unity and Python

---

## What We're Building

### Minimal Feature Set

**3 Tools Only**:
1. `unity_ping` - Health check
2. `unity_get_scene_info` - Read scene data
3. `unity_create_cube` - Create a simple GameObject

### Architecture

```
Claude Desktop (AI)
  ↓
MCP Server (Python) - 50 lines
  ↓
Unity HTTP Server (C#) - 150 lines
  ↓
Unity Editor
```

### Success Criteria

```
User: "Create a cube in Unity"
AI: [Uses unity_create_cube tool]
Result: Cube appears in Unity scene ✅
```

---

## Prerequisites

### Required Software

- **Unity Editor**: 2021.3 or newer
- **Python**: 3.9+
- **pip**: Python package manager
- **Code Editor**: VS Code, Rider, or Visual Studio
- **Claude Desktop** (or MCP-compatible client)

### Required Knowledge

- Basic Unity (opening projects, using Editor)
- Basic C# (can read and modify code)
- Basic Python (can run scripts)
- Basic terminal/command line usage

### Installation Check

```bash
# Check Unity (open Unity Hub and check version)

# Check Python
python3 --version
# Should output: Python 3.9.x or higher

# Check pip
pip3 --version

# Install MCP SDK
pip3 install mcp
```

---

## Step 1: Create Unity Package

### 1.1: Create Package Structure

In your Unity project, create this folder structure:

```
Packages/
  com.yourname.unitymcp/
    package.json
    Editor/
      UnityMCP/
        MCPServer.cs
        MCPTools.cs
        MCPServerWindow.cs
    Runtime/
      (empty for now)
    Documentation~/
      README.md
```

**How to create**:

1. Open your Unity project
2. Navigate to project root in file explorer/finder
3. Go to `Packages/` folder
4. Create folder: `com.yourname.unitymcp`
5. Create subfolders as shown above

---

### 1.2: Create package.json

**File**: `Packages/com.yourname.unitymcp/package.json`

```json
{
  "name": "com.yourname.unitymcp",
  "version": "0.1.0",
  "displayName": "Unity MCP Server",
  "description": "Model Context Protocol server for Unity Editor - MVP",
  "unity": "2021.3",
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "3.2.1"
  },
  "keywords": [
    "mcp",
    "ai",
    "automation"
  ],
  "author": {
    "name": "Your Name",
    "email": "your.email@example.com"
  }
}
```

**What this does**:
- Registers package with Unity
- Declares dependency on Newtonsoft JSON (for parsing)
- Sets minimum Unity version

---

### 1.3: Add Newtonsoft JSON Dependency

1. Open Unity
2. Go to **Window → Package Manager**
3. Click **+** button → **Add package by name**
4. Enter: `com.unity.nuget.newtonsoft-json`
5. Click **Add**
6. Wait for installation

---

## Step 2: Build HTTP Server

### 2.1: Create MCPServer.cs

**File**: `Packages/com.yourname.unitymcp/Editor/UnityMCP/MCPServer.cs`

```csharp
using UnityEngine;
using UnityEditor;
using System;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    [InitializeOnLoad]
    public static class MCPServer
    {
        private static HttpListener listener;
        private static Thread serverThread;
        private static bool isRunning = false;
        private static int port = 8765;
        
        // Auto-start on Unity load
        static MCPServer()
        {
            EditorApplication.update += Initialize;
        }
        
        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            StartServer();
        }
        
        public static void StartServer()
        {
            if (isRunning)
            {
                Debug.Log("[MCP] Server already running");
                return;
            }
            
            try
            {
                serverThread = new Thread(ServerLoop);
                serverThread.IsBackground = true;
                serverThread.Start();
                isRunning = true;
                
                Debug.Log($"[MCP] ✅ Server started on http://localhost:{port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Failed to start: {e.Message}");
            }
        }
        
        public static void StopServer()
        {
            if (!isRunning) return;
            
            isRunning = false;
            listener?.Stop();
            listener?.Close();
            Debug.Log("[MCP] Server stopped");
        }
        
        private static void ServerLoop()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            
            Debug.Log("[MCP] Listening for requests...");
            
            while (isRunning)
            {
                try
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MCP] Server error: {e.Message}");
                }
            }
        }
        
        private static void HandleRequest(HttpListenerContext context)
        {
            try
            {
                // Read request
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }
                
                // Parse JSON
                var request = JObject.Parse(requestBody);
                string tool = request["tool"]?.ToString();
                var args = request["args"] as JObject ?? new JObject();
                
                Debug.Log($"[MCP] Request: {tool}");
                
                // Execute on main thread
                JObject response = null;
                var resetEvent = new ManualResetEvent(false);
                
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        response = MCPTools.Execute(tool, args);
                    }
                    catch (Exception e)
                    {
                        response = CreateError(e.Message);
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                };
                
                // Wait for main thread (30 second timeout)
                if (resetEvent.WaitOne(30000))
                {
                    SendResponse(context, response);
                }
                else
                {
                    SendResponse(context, CreateError("Timeout"));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Request error: {e.Message}");
                SendResponse(context, CreateError(e.Message));
            }
        }
        
        private static void SendResponse(HttpListenerContext context, JObject response)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            
            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
            context.Response.ContentLength64 = responseBytes.Length;
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            context.Response.OutputStream.Close();
        }
        
        private static JObject CreateError(string message)
        {
            return new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
        }
    }
}
```

**What this does**:
- ✅ Starts HTTP server on Unity load
- ✅ Listens on port 8765
- ✅ Receives JSON requests
- ✅ Executes tools on main thread (Unity requirement)
- ✅ Returns JSON responses
- ✅ Handles errors gracefully

---

### 2.2: Create MCPTools.cs

**File**: `Packages/com.yourname.unitymcp/Editor/UnityMCP/MCPTools.cs`

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace UnityMCP
{
    public static class MCPTools
    {
        public static JObject Execute(string tool, JObject args)
        {
            try
            {
                switch (tool)
                {
                    case "unity_ping":
                        return Ping();
                    
                    case "unity_get_scene_info":
                        return GetSceneInfo();
                    
                    case "unity_create_cube":
                        return CreateCube(args);
                    
                    default:
                        throw new System.Exception($"Unknown tool: {tool}");
                }
            }
            catch (System.Exception e)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = e.Message
                };
            }
        }
        
        // Tool 1: Ping (health check)
        private static JObject Ping()
        {
            return new JObject
            {
                ["success"] = true,
                ["message"] = "pong",
                ["unityVersion"] = Application.unityVersion,
                ["timestamp"] = System.DateTime.UtcNow.ToString("o")
            };
        }
        
        // Tool 2: Get Scene Info
        private static JObject GetSceneInfo()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            
            return new JObject
            {
                ["success"] = true,
                ["sceneName"] = scene.name,
                ["scenePath"] = scene.path,
                ["isLoaded"] = scene.isLoaded,
                ["rootObjectCount"] = rootObjects.Length,
                ["rootObjects"] = new JArray(
                    rootObjects.Select(go => go.name)
                )
            };
        }
        
        // Tool 3: Create Cube
        private static JObject CreateCube(JObject args)
        {
            // Parse arguments
            string name = args["name"]?.ToString() ?? "Cube";
            float x = args["x"]?.ToObject<float>() ?? 0;
            float y = args["y"]?.ToObject<float>() ?? 0;
            float z = args["z"]?.ToObject<float>() ?? 0;
            
            // Create cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = new Vector3(x, y, z);
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(cube, $"Create {name}");
            
            // Select it
            Selection.activeGameObject = cube;
            
            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            return new JObject
            {
                ["success"] = true,
                ["name"] = cube.name,
                ["position"] = new JArray(x, y, z),
                ["instanceId"] = cube.GetInstanceID()
            };
        }
    }
}
```

**What this does**:
- ✅ Implements 3 simple tools
- ✅ Ping: Tests connection
- ✅ GetSceneInfo: Reads Unity scene data
- ✅ CreateCube: Creates a GameObject
- ✅ Returns structured JSON responses

---

### 2.3: Create MCPServerWindow.cs

**File**: `Packages/com.yourname.unitymcp/Editor/UnityMCP/MCPServerWindow.cs`

```csharp
using UnityEngine;
using UnityEditor;

namespace UnityMCP
{
    public class MCPServerWindow : EditorWindow
    {
        [MenuItem("Window/MCP Server")]
        public static void ShowWindow()
        {
            GetWindow<MCPServerWindow>("MCP Server");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Unity MCP Server - MVP", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Status:", "Running");
            EditorGUILayout.LabelField("Port:", "8765");
            EditorGUILayout.LabelField("URL:", "http://localhost:8765");
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Stop Server"))
            {
                MCPServer.StopServer();
            }
            
            if (GUILayout.Button("Start Server"))
            {
                MCPServer.StartServer();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "MCP Server is running!\n\n" +
                "Tools available:\n" +
                "- unity_ping\n" +
                "- unity_get_scene_info\n" +
                "- unity_create_cube",
                MessageType.Info
            );
        }
    }
}
```

**What this does**:
- ✅ Provides UI to control server
- ✅ Shows status and available tools
- ✅ Accessible via Window menu

---

## Step 3: Test Unity Package

### 3.1: Import Package

1. **Restart Unity** (to load the new package)
2. Check Package Manager: Window → Package Manager
3. Switch to "In Project" packages
4. You should see "Unity MCP Server"

### 3.2: Test Server Window

1. Go to **Window → MCP Server**
2. Window should open showing "Running" status
3. Check Unity Console for: `[MCP] ✅ Server started on http://localhost:8765`

### 3.3: Test Direct HTTP Request

Open terminal and test with curl:

```bash
# Test ping
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_ping","args":{}}'

# Expected response:
# {"success":true,"message":"pong","unityVersion":"2022.3.10f1","timestamp":"2025-10-10T..."}

# Test get scene info
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_get_scene_info","args":{}}'

# Expected response:
# {"success":true,"sceneName":"SampleScene","rootObjectCount":2,...}

# Test create cube
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_create_cube","args":{"name":"TestCube","x":0,"y":2,"z":0}}'

# Expected response:
# {"success":true,"name":"TestCube","position":[0,2,0],...}
```

**✅ If you see a cube appear in Unity, the Unity side works!**

---

## Step 4: Create MCP Server

### 4.1: Create Python Package Structure

Create this folder structure anywhere on your computer:

```
unity-mcp/
  unity_mcp_server.py
  requirements.txt
  README.md
```

### 4.2: Create requirements.txt

**File**: `unity-mcp/requirements.txt`

```txt
mcp>=0.9.0
httpx>=0.24.0
```

### 4.3: Create unity_mcp_server.py

**File**: `unity-mcp/unity_mcp_server.py`

```python
#!/usr/bin/env python3
"""
Unity MCP Server - MVP
Provides 3 basic tools to control Unity Editor
"""

import asyncio
import httpx
import json
from typing import Any, Dict, List
from mcp.server import Server
from mcp.types import Tool, TextContent

# Configuration
UNITY_URL = "http://localhost:8765"
TIMEOUT = 30.0

# Create MCP server
app = Server("unity-mcp-mvp")

# Tool definitions
TOOLS = [
    {
        "name": "unity_ping",
        "description": "Check if Unity Editor is responding",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    {
        "name": "unity_get_scene_info",
        "description": "Get information about the current Unity scene",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    {
        "name": "unity_create_cube",
        "description": "Create a cube GameObject in the Unity scene",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name for the cube",
                    "default": "Cube"
                },
                "x": {
                    "type": "number",
                    "description": "X position",
                    "default": 0
                },
                "y": {
                    "type": "number",
                    "description": "Y position",
                    "default": 0
                },
                "z": {
                    "type": "number",
                    "description": "Z position",
                    "default": 0
                }
            },
            "required": []
        }
    }
]

@app.list_tools()
async def list_tools() -> List[Tool]:
    """List available Unity tools"""
    return [Tool(**tool) for tool in TOOLS]

@app.call_tool()
async def call_tool(name: str, arguments: Dict[str, Any]) -> List[TextContent]:
    """Execute a Unity tool"""
    try:
        # Send request to Unity
        async with httpx.AsyncClient(timeout=TIMEOUT) as client:
            response = await client.post(
                UNITY_URL,
                json={
                    "tool": name,
                    "args": arguments
                }
            )
            
            result = response.json()
            
            # Format response
            if result.get("success"):
                return [TextContent(
                    type="text",
                    text=json.dumps(result, indent=2)
                )]
            else:
                error_msg = result.get("error", "Unknown error")
                return [TextContent(
                    type="text",
                    text=f"Error: {error_msg}"
                )]
                
    except httpx.TimeoutException:
        return [TextContent(
            type="text",
            text="Error: Request to Unity timed out. Is Unity Editor running?"
        )]
    except httpx.ConnectError:
        return [TextContent(
            type="text",
            text="Error: Cannot connect to Unity. Make sure Unity Editor is running and MCP Server is started."
        )]
    except Exception as e:
        return [TextContent(
            type="text",
            text=f"Error: {str(e)}"
        )]

async def main():
    """Run the MCP server"""
    from mcp.server.stdio import stdio_server
    
    async with stdio_server() as (read_stream, write_stream):
        await app.run(
            read_stream,
            write_stream,
            app.create_initialization_options()
        )

if __name__ == "__main__":
    asyncio.run(main())
```

**What this does**:
- ✅ Implements MCP protocol
- ✅ Defines 3 tools matching Unity side
- ✅ Forwards requests to Unity HTTP server
- ✅ Handles errors gracefully
- ✅ Returns results to AI

---

### 4.4: Install Dependencies

```bash
cd unity-mcp
pip3 install -r requirements.txt
```

---

### 4.5: Test Python Server

```bash
# Test that it runs (will wait for stdin)
python3 unity_mcp_server.py

# Press Ctrl+C to stop
```

If no errors, it's working! (It won't do much without MCP client input)

---

## Step 5: Connect to Claude Desktop

### 5.1: Configure Claude Desktop

Edit Claude config file:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "unity": {
      "command": "python3",
      "args": ["/FULL/PATH/TO/unity-mcp/unity_mcp_server.py"]
    }
  }
}
```

**Important**: Replace `/FULL/PATH/TO/` with actual absolute path!

To get full path:
```bash
cd unity-mcp
pwd
# Copy this path and use it in config
```

---

### 5.2: Restart Claude Desktop

1. **Quit** Claude Desktop completely
2. **Reopen** Claude Desktop
3. Start a new conversation

---

## Step 6: Test End-to-End

### 6.1: Verify Connection

In Claude Desktop, try:

```
"Can you check if Unity is responding?"
```

**Expected**: Claude should use `unity_ping` tool and show Unity version.

---

### 6.2: Test Scene Info

```
"What scene is currently open in Unity?"
```

**Expected**: Claude should use `unity_get_scene_info` and tell you the scene name and objects.

---

### 6.3: Test Cube Creation

```
"Create a cube in Unity at position (5, 2, 3)"
```

**Expected**: 
- Claude uses `unity_create_cube` tool
- Cube appears in Unity scene at position (5, 2, 3)
- Unity Console shows: `[MCP] Request: unity_create_cube`

---

### 6.4: Test Natural Language

```
"Create 3 cubes in a row along the X axis"
```

**Expected**:
- Claude calls `unity_create_cube` three times
- Three cubes appear at x=0, x=1, x=2 (or similar)

---

## Troubleshooting

### Problem: Unity Server Not Starting

**Symptoms**: No console message about server starting

**Solutions**:
1. Check Unity Console for errors
2. Verify Newtonsoft.JSON is installed (Package Manager)
3. Check if port 8765 is already in use:
   ```bash
   lsof -i :8765  # macOS/Linux
   netstat -ano | findstr :8765  # Windows
   ```
4. Restart Unity Editor

---

### Problem: curl Test Fails

**Symptoms**: `Connection refused` or timeout

**Solutions**:
1. Make sure Unity Editor is open
2. Check MCP Server window shows "Running"
3. Try stopping and starting server via Window → MCP Server
4. Check firewall isn't blocking port 8765
5. Verify URL is exactly `http://localhost:8765`

---

### Problem: Claude Can't Connect

**Symptoms**: Claude says "Unity tool unavailable"

**Solutions**:
1. Verify config file path is correct (use `pwd`)
2. Check Python script runs manually:
   ```bash
   python3 /path/to/unity_mcp_server.py
   ```
3. Verify Python dependencies installed:
   ```bash
   pip3 list | grep mcp
   ```
4. **Restart Claude Desktop completely** (not just close window)
5. Check Claude logs:
   - macOS: `~/Library/Logs/Claude/`
   - Windows: `%APPDATA%\Claude\logs\`

---

### Problem: Tool Executes But Nothing Happens

**Symptoms**: No error but cube doesn't appear

**Solutions**:
1. Check Unity Console for errors
2. Verify scene is open (not just project view)
3. Try clicking in scene view to refresh
4. Check cube isn't created at (0,0,0) behind camera
5. Use Scene view search: Type "Cube" in hierarchy search

---

### Problem: "Timeout" Error

**Symptoms**: Request times out after 30 seconds

**Solutions**:
1. Unity might be compiling scripts - wait and retry
2. Unity might be in Play mode - exit Play mode
3. Unity might have a modal dialog open - close it
4. Try a simpler operation first (unity_ping)

---

## Success Checklist

Before moving forward, verify:

- [ ] Unity package installed and visible in Package Manager
- [ ] MCP Server window opens (Window → MCP Server)
- [ ] Console shows: `[MCP] ✅ Server started on http://localhost:8765`
- [ ] curl test works and returns JSON
- [ ] Cube appears in Unity when using curl
- [ ] Python script installed and dependencies working
- [ ] Claude Desktop config file updated with correct path
- [ ] Claude can use `unity_ping` tool
- [ ] Claude can create cubes in Unity
- [ ] Natural language commands work

**If all checkboxes are ticked, you have a working MVP! 🎉**

---

## What We've Proven

✅ **Architecture Works**: Unity ↔ MCP ↔ AI pipeline is solid  
✅ **Threading Works**: Main thread execution doesn't block  
✅ **JSON Works**: Serialization/deserialization is functional  
✅ **Tools Work**: Can implement and call tools  
✅ **AI Integration Works**: Claude can control Unity naturally  

---

## Next Steps

Now that the MVP works, you can:

### Phase 2: Add More Tools (Week 2)

Add 10-15 more tools:
- Create other primitives (sphere, cylinder, plane)
- Delete GameObjects
- Move/rotate/scale objects
- Add components
- Read/write simple properties

### Phase 3: UI Tools (Week 3-4)

Add the UI system:
- Canvas creation
- Button creation
- Text creation
- Basic layouts

### Phase 4: Screenshot System (Week 5-6)

Add visual verification:
- Screenshot capture
- Visual analysis
- AI vision integration

### Phase 5: Testing Suite (Week 7-8)

Add comprehensive testing:
- Automated quality checks
- Responsive testing
- Accessibility validation

---

## File Summary

### What You Created

**Unity Package** (3 files, ~350 lines):
```
Packages/com.yourname.unitymcp/
  package.json
  Editor/UnityMCP/
    MCPServer.cs        - HTTP server (150 lines)
    MCPTools.cs         - Tool implementations (150 lines)
    MCPServerWindow.cs  - UI window (50 lines)
```

**Python MCP Server** (1 file, ~150 lines):
```
unity-mcp/
  unity_mcp_server.py   - MCP protocol implementation
  requirements.txt      - Dependencies
```

**Total**: ~500 lines of code for a working AI-controlled Unity system!

---

## Common Questions

### Q: Can I add more tools?

**A**: Yes! Just add new cases to `MCPTools.Execute()` in Unity and add tool definitions to `TOOLS` array in Python.

### Q: Can I change the port?

**A**: Yes! Change `port` variable in both Unity (MCPServer.cs) and Python (unity_mcp_server.py).

### Q: Does this work in Unity 2020?

**A**: Maybe. Try it! The code is simple and should work on older versions. Main requirement is C# 7.3+ and .NET Standard 2.1.

### Q: Can I use this in Play mode?

**A**: Not yet. This MVP only works in Edit mode. For Play mode support, you'd need to handle runtime differently.

### Q: Is this secure?

**A**: No! This is localhost-only but has no authentication. Don't expose port 8765 to network.

### Q: Can multiple AI clients connect?

**A**: The Unity server can handle multiple clients, but MCP protocol is typically 1:1.

### Q: Does this work with GPT/other AIs?

**A**: Yes! Any MCP-compatible client should work. Currently tested with Claude Desktop.

---

## Performance Notes

### Expected Response Times

- `unity_ping`: < 10ms
- `unity_get_scene_info`: < 50ms
- `unity_create_cube`: < 100ms

### Limitations

- One request at a time (synchronous on Unity main thread)
- 30 second timeout per request
- No request queuing

### Scaling Up

For production, you'd want:
- Request queuing
- Async operations where possible
- Batch operations
- Caching
- Progress reporting

---

## Debug Tips

### Enable Verbose Logging

Add to Unity code:
```csharp
Debug.Log($"[MCP] Received: {requestBody}");
Debug.Log($"[MCP] Sending: {response}");
```

### Test Without AI

Use curl to test each tool individually before testing with Claude.

### Check JSON Format

Use [JSONLint](https://jsonlint.com/) to validate your JSON if getting parse errors.

### Monitor Unity Console

Keep Unity Console visible while testing. Filter to "MCP" to see only relevant logs.

---

## Resources

### Documentation
- [MCP Protocol Spec](https://modelcontextprotocol.io)
- [Unity C# Scripting](https://docs.unity3d.com/ScriptReference/)
- [Newtonsoft.JSON](https://www.newtonsoft.com/json/help/html/Introduction.htm)

### Example curl Commands

Save these for quick testing:

```bash
# Ping
curl -X POST http://localhost:8765/ -H "Content-Type: application/json" -d '{"tool":"unity_ping","args":{}}'

# Scene Info
curl -X POST http://localhost:8765/ -H "Content-Type: application/json" -d '{"tool":"unity_get_scene_info","args":{}}'

# Create Cube at origin
curl -X POST http://localhost:8765/ -H "Content-Type: application/json" -d '{"tool":"unity_create_cube","args":{"name":"TestCube"}}'

# Create Cube at specific position
curl -X POST http://localhost:8765/ -H "Content-Type: application/json" -d '{"tool":"unity_create_cube","args":{"name":"Cube1","x":5,"y":2,"z":3}}'
```

---

## Congratulations!

If you've made it this far and everything works, you've successfully created a working Unity MCP Server MVP!

You now have:
- ✅ Functional Unity package
- ✅ Working HTTP server
- ✅ MCP protocol integration
- ✅ AI controlling Unity
- ✅ Foundation to build on

**Time to build the rest! 🚀**

---

**Document Version**: 1.0  
**Last Updated**: October 10, 2025  
**Status**: Complete Step-by-Step Guide  
**Difficulty**: Beginner-Intermediate  
**Time Required**: 1-2 hours

