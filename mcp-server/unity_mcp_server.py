#!/usr/bin/env python3.11
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
    },
    {
        "name": "unity_force_compile",
        "description": "Force Unity to compile all scripts. Useful before performing operations that require up-to-date code.",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    {
        "name": "unity_is_compiling",
        "description": "Check if Unity is currently compiling scripts. Returns true if compiling, false if idle.",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    {
        "name": "unity_wait_for_compile",
        "description": "Wait for Unity to finish compiling. Blocks until compilation is complete or timeout is reached.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "maxWaitSeconds": {
                    "type": "number",
                    "description": "Maximum seconds to wait for compilation",
                    "default": 30
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_get_logs",
        "description": "Get recent Unity console logs (errors, warnings, and messages). Essential for debugging issues.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "count": {
                    "type": "number",
                    "description": "Number of recent log entries to retrieve",
                    "default": 50
                },
                "includeStackTrace": {
                    "type": "boolean",
                    "description": "Include file and line number information",
                    "default": false
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_capture_screenshot",
        "description": "Capture a screenshot of the Unity Editor view. Returns base64-encoded PNG image that can be analyzed by AI vision. ESSENTIAL for visual verification of created content.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "viewType": {
                    "type": "string",
                    "enum": ["game", "scene"],
                    "description": "Which view to capture: 'game' (Game View/Camera) or 'scene' (Scene View)",
                    "default": "game"
                },
                "width": {
                    "type": "number",
                    "description": "Screenshot width in pixels",
                    "default": 1920
                },
                "height": {
                    "type": "number",
                    "description": "Screenshot height in pixels",
                    "default": 1080
                },
                "returnBase64": {
                    "type": "boolean",
                    "description": "Include base64-encoded image data in response",
                    "default": true
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

