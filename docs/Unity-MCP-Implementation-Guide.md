# Unity MCP Server - Implementation Guide

## Quick Start

This guide provides detailed implementation examples for building the Unity MCP Server.

## Table of Contents

1. [Unity Plugin Implementation](#unity-plugin-implementation)
2. [MCP Server Implementation](#mcp-server-implementation)
3. [Tool Examples](#tool-examples)
4. [Testing Strategy](#testing-strategy)
5. [Deployment](#deployment)

---

## Unity Plugin Implementation

### 1. Package Structure

Create a Unity Package structure:

```
Packages/
  com.yourcompany.mcp/
    package.json
    Editor/
      MCPServer/
        MCPServerController.cs
        CommandRouter.cs
        SafetyValidator.cs
        OperationLogger.cs
        MCPServerWindow.cs
        Commands/
          ICommand.cs
          SceneCommands.cs
          GameObjectCommands.cs
          ScriptCommands.cs
    Runtime/
      MCPRuntime.cs  # If needed for play mode operations
    Documentation~/
      manual.md
```

### 2. Package.json

```json
{
  "name": "com.yourcompany.mcp",
  "version": "1.0.0",
  "displayName": "Unity MCP Server",
  "description": "Model Context Protocol server for Unity Editor",
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
    "name": "Your Company",
    "email": "support@yourcompany.com"
  }
}
```

### 3. Core Server Controller

```csharp
// Editor/MCPServer/MCPServerController.cs
using UnityEngine;
using UnityEditor;
using System;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    [InitializeOnLoad]
    public class MCPServerController
    {
        private static HttpListener listener;
        private static Thread serverThread;
        private static bool isRunning = false;
        private static int port = 8765;
        private static CommandRouter commandRouter;

        static MCPServerController()
        {
            EditorApplication.update += Initialize;
        }

        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            
            commandRouter = new CommandRouter();
            
            // Auto-start if preference is set
            if (EditorPrefs.GetBool("MCPServer_AutoStart", true))
            {
                StartServer();
            }
        }

        public static void StartServer()
        {
            if (isRunning)
            {
                Debug.LogWarning("[MCP] Server is already running");
                return;
            }

            try
            {
                port = EditorPrefs.GetInt("MCPServer_Port", 8765);
                serverThread = new Thread(ServerLoop);
                serverThread.IsBackground = true;
                serverThread.Start();
                isRunning = true;
                
                Debug.Log($"[MCP] Server started on http://localhost:{port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Failed to start server: {e.Message}");
            }
        }

        public static void StopServer()
        {
            if (!isRunning) return;

            try
            {
                isRunning = false;
                listener?.Stop();
                listener?.Close();
                serverThread?.Abort();
                Debug.Log("[MCP] Server stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Error stopping server: {e.Message}");
            }
        }

        private static void ServerLoop()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/");
                listener.Start();

                while (isRunning)
                {
                    try
                    {
                        var context = listener.GetContext();
                        ThreadPool.QueueUserWorkItem((_) => HandleRequestAsync(context));
                    }
                    catch (HttpListenerException)
                    {
                        // Expected when stopping
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Server error: {e.Message}");
            }
        }

        private static void HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                // Read request body
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }

                // Parse request
                var request = JObject.Parse(requestBody);
                string requestId = request["id"]?.ToString() ?? Guid.NewGuid().ToString();
                string tool = request["tool"]?.ToString();
                var args = request["args"] as JObject ?? new JObject();

                // Log request
                OperationLogger.LogRequest(requestId, tool, args);

                // Execute on main thread and wait for result
                JObject response = null;
                var resetEvent = new ManualResetEvent(false);

                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        response = ExecuteCommand(requestId, tool, args);
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                };

                // Wait for main thread execution (with timeout)
                if (resetEvent.WaitOne(30000))
                {
                    SendResponse(context, response);
                }
                else
                {
                    SendError(context, requestId, "TIMEOUT", "Operation timed out after 30 seconds");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Request handling error: {e.Message}");
                SendError(context, "unknown", "INTERNAL_ERROR", e.Message);
            }
        }

        private static JObject ExecuteCommand(string requestId, string tool, JObject args)
        {
            try
            {
                // Validate safety
                if (!SafetyValidator.ValidateOperation(tool, args))
                {
                    return CreateErrorResponse(requestId, "SAFETY_ERROR", "Operation blocked by safety validator");
                }

                // Execute command
                var result = commandRouter.Execute(tool, args);
                
                // Log success
                OperationLogger.LogSuccess(requestId, tool, result);

                return CreateSuccessResponse(requestId, result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Command execution error: {e.Message}\n{e.StackTrace}");
                OperationLogger.LogError(requestId, tool, e.Message);
                return CreateErrorResponse(requestId, "EXECUTION_ERROR", e.Message);
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

        private static void SendError(HttpListenerContext context, string requestId, string code, string message)
        {
            var error = CreateErrorResponse(requestId, code, message);
            SendResponse(context, error);
        }

        private static JObject CreateSuccessResponse(string requestId, object data)
        {
            return new JObject
            {
                ["id"] = requestId,
                ["success"] = true,
                ["data"] = JToken.FromObject(data),
                ["error"] = null
            };
        }

        private static JObject CreateErrorResponse(string requestId, string code, string message)
        {
            return new JObject
            {
                ["id"] = requestId,
                ["success"] = false,
                ["data"] = null,
                ["error"] = new JObject
                {
                    ["code"] = code,
                    ["message"] = message
                }
            };
        }
    }
}
```

### 4. Command Router

```csharp
// Editor/MCPServer/CommandRouter.cs
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public class CommandRouter
    {
        private Dictionary<string, ICommand> commands;

        public CommandRouter()
        {
            commands = new Dictionary<string, ICommand>();
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            // Scene commands
            RegisterCommand("unity_load_scene", new LoadSceneCommand());
            RegisterCommand("unity_save_scene", new SaveSceneCommand());
            RegisterCommand("unity_get_hierarchy", new GetHierarchyCommand());

            // GameObject commands
            RegisterCommand("unity_create_gameobject", new CreateGameObjectCommand());
            RegisterCommand("unity_delete_gameobject", new DeleteGameObjectCommand());
            RegisterCommand("unity_find_gameobject", new FindGameObjectCommand());
            RegisterCommand("unity_modify_gameobject", new ModifyGameObjectCommand());

            // Component commands
            RegisterCommand("unity_add_component", new AddComponentCommand());
            RegisterCommand("unity_remove_component", new RemoveComponentCommand());
            RegisterCommand("unity_get_component", new GetComponentCommand());
            RegisterCommand("unity_set_component_property", new SetComponentPropertyCommand());

            // Script commands
            RegisterCommand("unity_read_script", new ReadScriptCommand());
            RegisterCommand("unity_create_script", new CreateScriptCommand());
            RegisterCommand("unity_modify_script", new ModifyScriptCommand());
            RegisterCommand("unity_list_scripts", new ListScriptsCommand());

            // Editor commands
            RegisterCommand("unity_play_mode", new PlayModeCommand());
            RegisterCommand("unity_get_console", new GetConsoleCommand());
            RegisterCommand("unity_execute_menu", new ExecuteMenuCommand());
        }

        private void RegisterCommand(string name, ICommand command)
        {
            commands[name] = command;
        }

        public object Execute(string commandName, JObject args)
        {
            if (!commands.TryGetValue(commandName, out var command))
            {
                throw new Exception($"Unknown command: {commandName}");
            }

            return command.Execute(args);
        }
    }

    public interface ICommand
    {
        object Execute(JObject args);
    }
}
```

### 5. Example Commands

```csharp
// Editor/MCPServer/Commands/GameObjectCommands.cs
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace UnityMCP.Commands
{
    public class CreateGameObjectCommand : ICommand
    {
        public object Execute(JObject args)
        {
            string name = args["name"]?.ToString() ?? "GameObject";
            var position = args["position"]?.ToObject<Vector3>() ?? Vector3.zero;
            var components = args["components"]?.ToObject<string[]>() ?? new string[0];

            // Create GameObject
            GameObject go = new GameObject(name);
            go.transform.position = position;

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");

            // Add components
            foreach (var componentName in components)
            {
                var componentType = System.Type.GetType($"UnityEngine.{componentName}, UnityEngine");
                if (componentType != null)
                {
                    Undo.AddComponent(go, componentType);
                }
            }

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(go.scene);

            return new
            {
                success = true,
                gameObjectId = go.GetInstanceID(),
                name = go.name,
                path = GetGameObjectPath(go)
            };
        }

        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }
    }

    public class FindGameObjectCommand : ICommand
    {
        public object Execute(JObject args)
        {
            string name = args["name"]?.ToString();
            bool includeInactive = args["includeInactive"]?.ToObject<bool>() ?? false;

            if (string.IsNullOrEmpty(name))
                throw new System.ArgumentException("name is required");

            GameObject[] allObjects = includeInactive 
                ? Resources.FindObjectsOfTypeAll<GameObject>()
                : GameObject.FindObjectsOfType<GameObject>();

            var results = new System.Collections.Generic.List<object>();
            
            foreach (var go in allObjects)
            {
                if (go.name.Contains(name))
                {
                    results.Add(new
                    {
                        id = go.GetInstanceID(),
                        name = go.name,
                        path = GetGameObjectPath(go),
                        position = go.transform.position,
                        active = go.activeInHierarchy,
                        tag = go.tag,
                        layer = LayerMask.LayerToName(go.layer)
                    });
                }
            }

            return new { count = results.Count, results = results };
        }

        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }
    }
}
```

### 6. Safety Validator

```csharp
// Editor/MCPServer/SafetyValidator.cs
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace UnityMCP
{
    public static class SafetyValidator
    {
        private static readonly string[] PROTECTED_PATHS = new string[]
        {
            "Assets/Settings",
            "Assets/Editor Default Resources",
            "Assets/Gizmos"
        };

        private static readonly string[] DANGEROUS_CODE_PATTERNS = new string[]
        {
            @"System\.IO\.File\.Delete",
            @"System\.IO\.Directory\.Delete",
            @"UnityEditor\.FileUtil\.DeleteFileOrDirectory",
            @"System\.Diagnostics\.Process\.Start",
            @"System\.Reflection\.Assembly\.Load"
        };

        public static bool ValidateOperation(string operation, JObject args)
        {
            // Check if operation is in destructive list
            if (IsDestructiveOperation(operation))
            {
                if (!ValidateDestructiveOperation(operation, args))
                    return false;
            }

            // Validate script modifications
            if (operation.Contains("script"))
            {
                if (!ValidateScriptOperation(operation, args))
                    return false;
            }

            // Validate file paths
            if (args.ContainsKey("path"))
            {
                string path = args["path"].ToString();
                if (IsProtectedPath(path))
                {
                    Debug.LogWarning($"[MCP Safety] Operation blocked: Protected path '{path}'");
                    return false;
                }
            }

            return true;
        }

        private static bool IsDestructiveOperation(string operation)
        {
            return operation.Contains("delete") || 
                   operation.Contains("remove") || 
                   operation.Contains("destroy");
        }

        private static bool ValidateDestructiveOperation(string operation, JObject args)
        {
            // Require confirmation for destructive operations
            bool confirmed = args["confirm"]?.ToObject<bool>() ?? false;
            
            if (!confirmed)
            {
                Debug.LogWarning($"[MCP Safety] Destructive operation '{operation}' requires confirmation");
                return false;
            }

            return true;
        }

        private static bool ValidateScriptOperation(string operation, JObject args)
        {
            if (operation.Contains("create") || operation.Contains("modify"))
            {
                string content = args["content"]?.ToString() ?? "";
                
                foreach (var pattern in DANGEROUS_CODE_PATTERNS)
                {
                    if (Regex.IsMatch(content, pattern))
                    {
                        Debug.LogWarning($"[MCP Safety] Script contains dangerous pattern: {pattern}");
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsProtectedPath(string path)
        {
            foreach (var protectedPath in PROTECTED_PATHS)
            {
                if (path.StartsWith(protectedPath))
                    return true;
            }
            return false;
        }
    }
}
```

### 7. Editor Window

```csharp
// Editor/MCPServer/MCPServerWindow.cs
using UnityEngine;
using UnityEditor;

namespace UnityMCP
{
    public class MCPServerWindow : EditorWindow
    {
        private bool autoStart;
        private int port;
        private Vector2 scrollPosition;

        [MenuItem("Window/MCP Server")]
        public static void ShowWindow()
        {
            GetWindow<MCPServerWindow>("MCP Server");
        }

        private void OnEnable()
        {
            autoStart = EditorPrefs.GetBool("MCPServer_AutoStart", true);
            port = EditorPrefs.GetInt("MCPServer_Port", 8765);
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity MCP Server", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Server status
            EditorGUILayout.LabelField("Status:", MCPServerController.IsRunning ? "Running" : "Stopped");
            EditorGUILayout.LabelField("Port:", port.ToString());
            EditorGUILayout.LabelField("URL:", $"http://localhost:{port}");
            EditorGUILayout.Space();

            // Controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(MCPServerController.IsRunning ? "Stop Server" : "Start Server"))
            {
                if (MCPServerController.IsRunning)
                    MCPServerController.StopServer();
                else
                    MCPServerController.StartServer();
            }

            if (GUILayout.Button("Restart"))
            {
                MCPServerController.StopServer();
                MCPServerController.StartServer();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Settings
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            
            bool newAutoStart = EditorGUILayout.Toggle("Auto-start on launch", autoStart);
            if (newAutoStart != autoStart)
            {
                autoStart = newAutoStart;
                EditorPrefs.SetBool("MCPServer_AutoStart", autoStart);
            }

            int newPort = EditorGUILayout.IntField("Port", port);
            if (newPort != port)
            {
                port = newPort;
                EditorPrefs.SetInt("MCPServer_Port", port);
            }

            EditorGUILayout.Space();

            // Operation history
            GUILayout.Label("Recent Operations", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            var history = OperationLogger.GetRecentOperations(20);
            foreach (var operation in history)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"[{operation.timestamp:HH:mm:ss}] {operation.tool}");
                EditorGUILayout.LabelField($"Status: {operation.status}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear History"))
            {
                OperationLogger.ClearHistory();
            }
        }
    }
}
```

---

## MCP Server Implementation

### Python Implementation

```python
# mcp_server.py
import asyncio
import httpx
from typing import Any, Dict, List
from mcp.server import Server
from mcp.types import Tool, TextContent

# Configuration
UNITY_URL = "http://localhost:8765"
TIMEOUT = 30.0

# Create MCP server
app = Server("unity-mcp")

# Tool definitions
TOOLS = [
    {
        "name": "unity_create_gameobject",
        "description": "Create a new GameObject in the Unity scene",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Position [x, y, z]",
                    "minItems": 3,
                    "maxItems": 3
                },
                "components": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "List of component names to add"
                }
            },
            "required": ["name"]
        }
    },
    {
        "name": "unity_find_gameobject",
        "description": "Find GameObjects by name in the scene",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name or partial name to search for"
                },
                "includeInactive": {
                    "type": "boolean",
                    "description": "Include inactive GameObjects",
                    "default": False
                }
            },
            "required": ["name"]
        }
    },
    {
        "name": "unity_read_script",
        "description": "Read the contents of a C# script file",
        "inputSchema": {
            "type": "object",
            "properties": {
                "path": {
                    "type": "string",
                    "description": "Path to the script file (e.g., 'Assets/Scripts/PlayerController.cs')"
                }
            },
            "required": ["path"]
        }
    },
    {
        "name": "unity_get_console",
        "description": "Get recent console messages (logs, warnings, errors)",
        "inputSchema": {
            "type": "object",
            "properties": {
                "count": {
                    "type": "number",
                    "description": "Number of recent messages to retrieve",
                    "default": 50
                },
                "types": {
                    "type": "array",
                    "items": {"enum": ["log", "warning", "error"]},
                    "description": "Types of messages to retrieve"
                }
            }
        }
    },
    {
        "name": "unity_play_mode",
        "description": "Control Unity play mode",
        "inputSchema": {
            "type": "object",
            "properties": {
                "action": {
                    "type": "string",
                    "enum": ["play", "pause", "stop"],
                    "description": "Play mode action"
                }
            },
            "required": ["action"]
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
        async with httpx.AsyncClient(timeout=TIMEOUT) as client:
            response = await client.post(
                f"{UNITY_URL}/execute",
                json={
                    "tool": name,
                    "args": arguments
                }
            )
            
            result = response.json()
            
            if result.get("success"):
                import json
                return [TextContent(
                    type="text",
                    text=json.dumps(result.get("data"), indent=2)
                )]
            else:
                error = result.get("error", {})
                return [TextContent(
                    type="text",
                    text=f"Error: {error.get('message', 'Unknown error')}"
                )]
                
    except httpx.TimeoutException:
        return [TextContent(
            type="text",
            text="Error: Request to Unity timed out"
        )]
    except Exception as e:
        return [TextContent(
            type="text",
            text=f"Error: {str(e)}"
        )]

# Health check endpoint
@app.list_resources()
async def list_resources():
    """List available resources"""
    return []

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

### Configuration File

```json
// mcp_config.json
{
  "mcpServers": {
    "unity": {
      "command": "python",
      "args": ["/path/to/mcp_server.py"],
      "env": {
        "UNITY_PORT": "8765"
      }
    }
  }
}
```

---

## Testing Strategy

### Unit Tests (Unity)

```csharp
// Tests/Editor/MCPServerTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityMCP;
using Newtonsoft.Json.Linq;

public class MCPServerTests
{
    [Test]
    public void TestCreateGameObject()
    {
        var command = new CreateGameObjectCommand();
        var args = new JObject
        {
            ["name"] = "TestObject",
            ["position"] = JArray.FromObject(new float[] { 1, 2, 3 })
        };

        var result = command.Execute(args);
        
        Assert.IsNotNull(result);
        
        var go = GameObject.Find("TestObject");
        Assert.IsNotNull(go);
        Assert.AreEqual(new Vector3(1, 2, 3), go.transform.position);
        
        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestSafetyValidator()
    {
        var args = new JObject
        {
            ["path"] = "Assets/Settings/ImportantFile.asset"
        };

        bool isValid = SafetyValidator.ValidateOperation("delete", args);
        Assert.IsFalse(isValid, "Should block deletion of protected path");
    }
}
```

### Integration Tests (Python)

```python
# test_mcp_server.py
import pytest
import httpx
import asyncio

UNITY_URL = "http://localhost:8765"

@pytest.mark.asyncio
async def test_create_gameobject():
    """Test creating a GameObject"""
    async with httpx.AsyncClient() as client:
        response = await client.post(
            f"{UNITY_URL}/execute",
            json={
                "tool": "unity_create_gameobject",
                "args": {
                    "name": "TestCube",
                    "components": ["BoxCollider", "Rigidbody"]
                }
            }
        )
        
        assert response.status_code == 200
        data = response.json()
        assert data["success"] is True
        assert "gameObjectId" in data["data"]

@pytest.mark.asyncio
async def test_find_gameobject():
    """Test finding a GameObject"""
    # First create one
    async with httpx.AsyncClient() as client:
        await client.post(
            f"{UNITY_URL}/execute",
            json={
                "tool": "unity_create_gameobject",
                "args": {"name": "FindMe"}
            }
        )
        
        # Now find it
        response = await client.post(
            f"{UNITY_URL}/execute",
            json={
                "tool": "unity_find_gameobject",
                "args": {"name": "FindMe"}
            }
        )
        
        data = response.json()
        assert data["success"] is True
        assert data["data"]["count"] > 0
```

---

## Deployment

### Unity Package Installation

1. **Via Package Manager (Git URL)**:
   ```
   https://github.com/yourcompany/unity-mcp.git
   ```

2. **Via Unity Package Manager UI**:
   - Window → Package Manager
   - + button → "Add package from git URL"
   - Enter repository URL

3. **Manual Installation**:
   - Download package
   - Copy to `Packages/` folder

### MCP Server Installation

```bash
# Install via pip
pip install unity-mcp-server

# Or clone and install
git clone https://github.com/yourcompany/unity-mcp-server
cd unity-mcp-server
pip install -e .
```

### Configuration in Claude Desktop

```json
// ~/Library/Application Support/Claude/claude_desktop_config.json
{
  "mcpServers": {
    "unity": {
      "command": "python",
      "args": ["-m", "unity_mcp_server"],
      "env": {
        "UNITY_PORT": "8765"
      }
    }
  }
}
```

### First Time Setup

1. **Start Unity Editor**
2. **Open MCP Window**: Window → MCP Server
3. **Start Server**: Click "Start Server"
4. **Verify**: Check console for "Server started" message
5. **Test**: Open Claude Desktop and try: "Create a cube in Unity"

---

## Troubleshooting

### Common Issues

#### 1. Port Already in Use
```
[MCP] Failed to start server: Port 8765 is already in use
```
**Solution**: Change port in MCP Server Window

#### 2. Connection Refused
```
Error: Request to Unity timed out
```
**Solution**: 
- Ensure Unity Editor is running
- Check MCP Server Window shows "Running"
- Verify firewall settings

#### 3. Script Compilation Errors
```
[MCP] Command execution error: Type 'Newtonsoft.Json.JObject' not found
```
**Solution**: Install Newtonsoft JSON package via Package Manager

---

## Next Steps

1. Implement remaining commands (see Design Doc Phase 2)
2. Add WebSocket support for real-time updates
3. Create comprehensive test suite
4. Write user documentation and tutorials
5. Create video demonstrations
6. Set up CI/CD pipeline
7. Release v1.0

---

**Version**: 1.0  
**Last Updated**: October 10, 2025

