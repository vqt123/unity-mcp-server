# Unity MCP Server - Design Document

## Executive Summary

The Unity MCP (Model Context Protocol) Server is a bridge that enables AI assistants to interact with Unity Editor in real-time. It exposes Unity's functionality through a standardized protocol, allowing AI to read, modify, and control Unity projects programmatically.

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Core Capabilities](#core-capabilities)
4. [Implementation Strategy](#implementation-strategy)
5. [Communication Protocol](#communication-protocol)
6. [Security & Safety](#security--safety)
7. [Use Cases](#use-cases)
8. [Technical Requirements](#technical-requirements)
9. [Development Phases](#development-phases)

---

## Overview

### Purpose

Enable AI assistants to:
- Read Unity project structure and assets
- Modify C# scripts safely
- Create and manipulate GameObjects and Components
- Control Unity Editor state (play mode, scene management)
- Query project settings and configurations
- Execute Unity menu commands

### Key Benefits

- **AI-Powered Development**: Let AI handle repetitive Unity tasks
- **Rapid Prototyping**: Generate Unity scenes and scripts through natural language
- **Intelligent Debugging**: AI can analyze Unity console logs and suggest fixes
- **Learning Assistant**: Help developers learn Unity by showing and explaining
- **Automated Testing**: AI can set up test scenes and run play mode tests

---

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     AI Assistant                         │
│                  (Claude, GPT, etc.)                     │
└─────────────────────┬───────────────────────────────────┘
                      │ MCP Protocol (JSON-RPC over stdio/HTTP)
                      ↓
┌─────────────────────────────────────────────────────────┐
│               MCP Server (Python/Node.js)                │
│  - Protocol Handler                                      │
│  - Tool Registry                                         │
│  - Request Router                                        │
└─────────────────────┬───────────────────────────────────┘
                      │ HTTP/WebSocket
                      ↓
┌─────────────────────────────────────────────────────────┐
│              Unity Editor Plugin (C#)                    │
│  - HTTP Server (listening on localhost)                 │
│  - Command Executor                                      │
│  - State Manager                                         │
│  - Safety Validator                                      │
└─────────────────────────────────────────────────────────┘
                      │ Unity API
                      ↓
┌─────────────────────────────────────────────────────────┐
│                   Unity Editor                           │
│  - Scene Hierarchy                                       │
│  - Asset Database                                        │
│  - Scripts & Components                                  │
└─────────────────────────────────────────────────────────┘
```

### Components

#### 1. MCP Server (External Process)
- **Language**: Python or Node.js
- **Responsibilities**:
  - Implements MCP protocol specification
  - Manages tool definitions and schemas
  - Validates requests from AI
  - Forwards commands to Unity Editor
  - Handles errors and timeouts

#### 2. Unity Editor Plugin (C# Package)
- **Type**: Unity Editor Extension
- **Responsibilities**:
  - Runs embedded HTTP/WebSocket server
  - Executes Unity API commands on main thread
  - Serializes Unity objects to JSON
  - Maintains safety guards
  - Logs all operations

#### 3. Communication Layer
- **Protocol**: HTTP REST API + WebSocket for real-time updates
- **Format**: JSON for all data exchange
- **Port**: Configurable (default: 8765)
- **Security**: localhost-only, optional API key

---

## Core Capabilities

### 1. Project Structure Operations

#### Tools:
- `unity.list_scenes` - List all scenes in build settings
- `unity.list_assets` - List assets by type/path
- `unity.get_project_info` - Get Unity version, project name, settings
- `unity.search_assets` - Search for assets by name/type

### 2. Scene Management

#### Tools:
- `unity.load_scene` - Load a scene by name/path
- `unity.save_scene` - Save current scene
- `unity.create_scene` - Create new scene
- `unity.get_scene_hierarchy` - Get all GameObjects in current scene

### 3. GameObject Operations

#### Tools:
- `unity.create_gameobject` - Create GameObject with components
- `unity.delete_gameobject` - Delete GameObject by path/name
- `unity.modify_gameobject` - Change transform, properties
- `unity.find_gameobject` - Search for GameObject
- `unity.add_component` - Add component to GameObject
- `unity.remove_component` - Remove component
- `unity.get_component_properties` - Read component data
- `unity.set_component_properties` - Modify component data

### 4. Script Management

#### Tools:
- `unity.read_script` - Read C# script contents
- `unity.create_script` - Create new C# script
- `unity.modify_script` - Edit existing script (with validation)
- `unity.list_scripts` - List all scripts in project
- `unity.validate_script` - Check script for compilation errors

### 5. Asset Operations

#### Tools:
- `unity.create_material` - Create new material
- `unity.import_asset` - Import external asset
- `unity.create_prefab` - Create prefab from GameObject
- `unity.instantiate_prefab` - Place prefab in scene
- `unity.modify_asset` - Change asset properties

### 6. Editor Control

#### Tools:
- `unity.play_mode` - Enter/exit play mode
- `unity.pause_mode` - Pause/unpause
- `unity.get_editor_state` - Get current editor state
- `unity.execute_menu_item` - Execute Unity menu command
- `unity.get_console_messages` - Read console logs (errors, warnings)

### 7. Build & Settings

#### Tools:
- `unity.get_build_settings` - Read build settings
- `unity.set_build_settings` - Modify build settings
- `unity.get_player_settings` - Read player settings
- `unity.build_project` - Trigger build

---

## Implementation Strategy

### Phase 1: Foundation (Week 1-2)

#### Unity Plugin
```csharp
// Assets/Editor/MCPServer/MCPServerController.cs
using UnityEngine;
using UnityEditor;
using System.Net;
using System.Threading;

[InitializeOnLoad]
public class MCPServerController
{
    private static HttpListener listener;
    private static Thread serverThread;
    private static int port = 8765;
    
    static MCPServerController()
    {
        EditorApplication.update += Initialize;
    }
    
    private static void Initialize()
    {
        EditorApplication.update -= Initialize;
        StartServer();
    }
    
    private static void StartServer()
    {
        serverThread = new Thread(ServerLoop);
        serverThread.Start();
        Debug.Log($"[MCP] Server started on port {port}");
    }
    
    private static void ServerLoop()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        
        while (true)
        {
            var context = listener.GetContext();
            // Handle request on main thread
            EditorApplication.delayCall += () => HandleRequest(context);
        }
    }
}
```

#### MCP Server (Python)
```python
# mcp_server.py
from mcp.server import Server
import httpx
import json

app = Server("unity-mcp")
UNITY_URL = "http://localhost:8765"

@app.list_tools()
async def list_tools():
    return [
        {
            "name": "unity_create_gameobject",
            "description": "Create a new GameObject in Unity scene",
            "inputSchema": {
                "type": "object",
                "properties": {
                    "name": {"type": "string"},
                    "position": {"type": "array", "items": {"type": "number"}},
                    "components": {"type": "array", "items": {"type": "string"}}
                },
                "required": ["name"]
            }
        },
        # ... more tools
    ]

@app.call_tool()
async def call_tool(name: str, arguments: dict):
    async with httpx.AsyncClient() as client:
        response = await client.post(
            f"{UNITY_URL}/execute",
            json={"tool": name, "args": arguments}
        )
        return response.json()
```

### Phase 2: Core Tools (Week 3-4)

Implement essential tools:
1. Scene management (load, save, create)
2. GameObject CRUD operations
3. Script reading/writing
4. Basic asset operations

### Phase 3: Advanced Features (Week 5-6)

1. Component manipulation
2. Prefab operations
3. Material/texture creation
4. Editor control (play mode, menu items)

### Phase 4: Safety & Polish (Week 7-8)

1. Comprehensive validation
2. Undo/redo support
3. Error handling and recovery
4. Documentation and examples

---

## Communication Protocol

### Request Format

```json
{
  "id": "unique-request-id",
  "tool": "unity_create_gameobject",
  "args": {
    "name": "Player",
    "position": [0, 0, 0],
    "components": ["Rigidbody", "BoxCollider"]
  }
}
```

### Response Format

```json
{
  "id": "unique-request-id",
  "success": true,
  "data": {
    "gameObjectId": "12345",
    "path": "Player"
  },
  "error": null
}
```

### Error Response

```json
{
  "id": "unique-request-id",
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "GameObject name cannot be empty",
    "details": {}
  }
}
```

### WebSocket Events (Optional)

For real-time updates:
```json
{
  "event": "scene_changed",
  "timestamp": "2025-10-10T12:34:56Z",
  "data": {
    "sceneName": "MainScene",
    "gameObjectCount": 42
  }
}
```

---

## Security & Safety

### Safety Measures

#### 1. Localhost Only
- Server only accepts connections from 127.0.0.1
- No external network access

#### 2. Operation Validation
```csharp
public class SafetyValidator
{
    public static bool ValidateOperation(string operation, JObject args)
    {
        // Prevent deletion of critical assets
        if (operation == "delete" && IsCriticalAsset(args["path"]))
            return false;
            
        // Prevent modifications to readonly files
        if (operation == "modify_script" && IsReadOnly(args["path"]))
            return false;
            
        // Prevent dangerous code execution
        if (operation == "create_script" && ContainsDangerousCode(args["content"]))
            return false;
            
        return true;
    }
}
```

#### 3. Undo Support
- All operations support Unity's undo system
- AI can undo mistakes with `unity.undo` tool

#### 4. Dry Run Mode
- Preview changes before applying
- `dryRun: true` parameter for destructive operations

#### 5. Operation Logging
```csharp
public class OperationLogger
{
    private static List<Operation> history = new List<Operation>();
    
    public static void Log(string tool, JObject args, object result)
    {
        history.Add(new Operation
        {
            timestamp = DateTime.Now,
            tool = tool,
            args = args,
            result = result
        });
    }
}
```

#### 6. Rate Limiting
- Maximum 100 requests per minute
- Prevents runaway AI loops

### User Controls

#### Editor Window
```csharp
public class MCPServerWindow : EditorWindow
{
    [MenuItem("Window/MCP Server")]
    public static void ShowWindow()
    {
        GetWindow<MCPServerWindow>("MCP Server");
    }
    
    private void OnGUI()
    {
        // Server status
        GUILayout.Label("Server Status: Running", EditorStyles.boldLabel);
        
        // Controls
        if (GUILayout.Button("Stop Server"))
            MCPServerController.StopServer();
            
        if (GUILayout.Button("Clear History"))
            OperationLogger.Clear();
            
        // Recent operations
        GUILayout.Label("Recent Operations:", EditorStyles.boldLabel);
        // Display operation history
    }
}
```

---

## Use Cases

### 1. Rapid Scene Setup

**User**: "Create a simple platformer scene with a ground plane, player cube with Rigidbody, and 3 floating platforms"

**AI Actions**:
1. `unity.create_gameobject` - Ground (plane)
2. `unity.create_gameobject` - Player (cube + Rigidbody + BoxCollider)
3. `unity.create_gameobject` x3 - Platforms at different heights
4. `unity.modify_gameobject` - Position platforms
5. `unity.save_scene` - Save as "PlatformerLevel1"

### 2. Script Generation & Debugging

**User**: "My player controller isn't responding to input. Check the script and fix it"

**AI Actions**:
1. `unity.get_console_messages` - Read errors
2. `unity.read_script` - Read PlayerController.cs
3. Analyze code, identify input system issue
4. `unity.modify_script` - Fix input handling
5. `unity.play_mode` - Test in play mode

### 3. Asset Organization

**User**: "Organize all materials into folders by color"

**AI Actions**:
1. `unity.list_assets` - Get all materials
2. Parse material properties (color)
3. `unity.create_folder` - Create color folders
4. `unity.move_asset` x N - Move materials to appropriate folders

### 4. Batch Component Setup

**User**: "Add a LOD Group component to all GameObjects tagged 'Environment'"

**AI Actions**:
1. `unity.find_gameobject` - Find all with "Environment" tag
2. `unity.add_component` x N - Add LODGroup to each
3. `unity.set_component_properties` - Configure LOD levels

### 5. Build Configuration

**User**: "Set up build settings for WebGL with compression enabled"

**AI Actions**:
1. `unity.get_build_settings` - Read current settings
2. `unity.set_build_settings` - Switch to WebGL platform
3. `unity.set_player_settings` - Enable compression
4. `unity.build_project` - Trigger build

---

## Technical Requirements

### Unity Editor Plugin

#### Required Unity Version
- Unity 2021.3 LTS or newer
- .NET Standard 2.1

#### Dependencies
```json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "3.2.1"
  }
}
```

#### File Structure
```
Assets/
  Editor/
    MCPServer/
      MCPServerController.cs
      CommandExecutor.cs
      SafetyValidator.cs
      OperationLogger.cs
      Serializers/
        GameObjectSerializer.cs
        ComponentSerializer.cs
        AssetSerializer.cs
      Commands/
        SceneCommands.cs
        GameObjectCommands.cs
        ScriptCommands.cs
        AssetCommands.cs
      UI/
        MCPServerWindow.cs
```

### MCP Server

#### Language Options

**Option A: Python**
```python
# requirements.txt
mcp>=0.1.0
httpx>=0.24.0
uvicorn>=0.23.0
pydantic>=2.0.0
```

**Option B: Node.js**
```json
{
  "dependencies": {
    "@modelcontextprotocol/sdk": "^0.1.0",
    "axios": "^1.6.0",
    "express": "^4.18.0"
  }
}
```

#### Configuration File
```json
{
  "server": {
    "name": "unity-mcp",
    "version": "1.0.0",
    "unity_port": 8765,
    "timeout": 30000
  },
  "safety": {
    "rate_limit": 100,
    "enable_undo": true,
    "dry_run_default": false
  },
  "logging": {
    "level": "info",
    "file": "unity-mcp.log"
  }
}
```

---

## Development Phases

### Phase 1: MVP (Weeks 1-2)
**Goal**: Basic communication working

- [ ] Unity HTTP server running
- [ ] MCP server connecting to Unity
- [ ] 5 basic tools working:
  - unity.get_project_info
  - unity.list_scenes
  - unity.create_gameobject
  - unity.read_script
  - unity.get_console_messages
- [ ] Error handling
- [ ] Basic logging

**Deliverable**: Demo showing AI creating a GameObject

### Phase 2: Core Functionality (Weeks 3-4)
**Goal**: Essential Unity operations

- [ ] Scene management (10 tools)
- [ ] GameObject operations (15 tools)
- [ ] Script CRUD (8 tools)
- [ ] Component manipulation (10 tools)
- [ ] Undo support
- [ ] Safety validation

**Deliverable**: AI can build a simple scene from scratch

### Phase 3: Advanced Features (Weeks 5-6)
**Goal**: Professional workflow support

- [ ] Asset operations (15 tools)
- [ ] Prefab system (8 tools)
- [ ] Editor control (5 tools)
- [ ] Build settings (5 tools)
- [ ] WebSocket events
- [ ] Editor UI window

**Deliverable**: AI can manage complex projects

### Phase 4: Polish & Release (Weeks 7-8)
**Goal**: Production ready

- [ ] Comprehensive documentation
- [ ] Example use cases
- [ ] Performance optimization
- [ ] Security audit
- [ ] Testing suite
- [ ] Installation guides
- [ ] Video tutorials

**Deliverable**: Public release

---

## Success Metrics

### Technical Metrics
- **Response Time**: < 100ms for read operations, < 500ms for write operations
- **Reliability**: 99.9% success rate for valid requests
- **Coverage**: 80+ tools covering major Unity workflows

### User Experience Metrics
- **Task Completion**: Users can complete common tasks faster with AI
- **Learning Curve**: New Unity users productive within 1 day
- **Satisfaction**: 4.5+ star rating from users

---

## Future Enhancements

### Phase 5+: Advanced Features

1. **Visual Scripting Integration**
   - Create and modify Visual Scripting graphs
   - Generate node connections from natural language

2. **Animation System**
   - Create and modify Animator Controllers
   - Generate animation clips
   - Manage animation parameters

3. **UI System**
   - Create Canvas and UI elements
   - Set up UI layouts and anchoring
   - Generate UI event handlers

4. **Shader Graph**
   - Create and modify shader graphs
   - Connect shader nodes programmatically

5. **Version Control Integration**
   - Git operations through MCP
   - Commit tracking and history

6. **Multi-Project Support**
   - Manage multiple Unity instances
   - Cross-project asset sharing

7. **AI Training Integration**
   - Export training data from Unity
   - ML-Agents integration

---

## Conclusion

The Unity MCP Server bridges AI assistants with Unity Editor, enabling natural language control over game development workflows. By following this phased approach, we'll create a robust, safe, and powerful tool that accelerates Unity development while maintaining security and usability.

### Next Steps

1. **Week 1**: Set up Unity plugin skeleton + HTTP server
2. **Week 1**: Set up MCP server with basic tools
3. **Week 2**: Implement first 5 MVP tools
4. **Week 2**: Test end-to-end with real AI assistant
5. **Week 3+**: Continue with Phase 2 implementation

### Resources

- MCP Protocol Spec: https://modelcontextprotocol.io
- Unity Editor Scripting: https://docs.unity3d.com/ScriptReference/
- HttpListener Documentation: https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener

---

**Document Version**: 1.0  
**Last Updated**: October 10, 2025  
**Authors**: Unity MCP Team  
**Status**: Planning Phase

