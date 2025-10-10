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
        "name": "unity_delete_gameobject",
        "description": "Delete a GameObject from the scene by name. Useful for cleanup and iteration.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject to delete"
                }
            },
            "required": ["name"]
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
    },
    # UI Tools - Canvas Management
    {
        "name": "unity_ui_create_canvas",
        "description": "Create a UI Canvas for screen-space UI. Automatically adds CanvasScaler, GraphicRaycaster, and EventSystem if needed.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the canvas",
                    "default": "Canvas"
                },
                "renderMode": {
                    "type": "string",
                    "enum": ["ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace"],
                    "description": "Canvas render mode",
                    "default": "ScreenSpaceOverlay"
                },
                "sortingOrder": {
                    "type": "number",
                    "description": "Sorting order for overlapping canvases",
                    "default": 0
                },
                "pixelPerfect": {
                    "type": "boolean",
                    "description": "Enable pixel perfect rendering",
                    "default": false
                },
                "preset": {
                    "type": "string",
                    "enum": ["desktop", "mobile_portrait", "mobile_landscape", "tablet", "game_menu"],
                    "description": "Apply a resolution preset"
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_ui_setup_canvas_scaler",
        "description": "Configure Canvas Scaler for responsive UI. Adjusts UI scale based on screen size.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "canvas": {
                    "type": "string",
                    "description": "Name of the canvas to configure",
                    "default": "Canvas"
                },
                "uiScaleMode": {
                    "type": "string",
                    "description": "Scale mode",
                    "default": "ScaleWithScreenSize"
                },
                "referenceResolution": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Reference resolution [width, height]",
                    "default": [1920, 1080]
                },
                "matchValue": {
                    "type": "number",
                    "description": "Match width (0) or height (1), or balance (0.5)",
                    "default": 0.5
                }
            },
            "required": ["canvas"]
        }
    },
    {
        "name": "unity_ui_create_event_system",
        "description": "Create an EventSystem for UI input handling. Required for buttons, sliders, etc.",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    # UI Tools - Elements
    {
        "name": "unity_ui_create_button",
        "description": "Create a UI Button with text. Includes background image, text child, and click handling setup.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Button name",
                    "default": "Button"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name (usually Canvas)",
                    "default": "Canvas"
                },
                "text": {
                    "type": "string",
                    "description": "Button text",
                    "default": "Button"
                },
                "textSize": {
                    "type": "number",
                    "description": "Font size",
                    "default": 24
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Anchored position [x, y]",
                    "default": [0, 0]
                },
                "size": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Button size [width, height]",
                    "default": [200, 60]
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_ui_create_text",
        "description": "Create a TextMeshPro text element with rich formatting options.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Text object name",
                    "default": "Text"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "text": {
                    "type": "string",
                    "description": "Text content",
                    "default": "Text"
                },
                "fontSize": {
                    "type": "number",
                    "description": "Font size",
                    "default": 24
                },
                "color": {
                    "type": "string",
                    "description": "Text color in hex (#RRGGBB or #RRGGBBAA)",
                    "default": "#FFFFFF"
                },
                "alignment": {
                    "type": "string",
                    "enum": ["left", "center", "right"],
                    "description": "Text alignment",
                    "default": "center"
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Anchored position [x, y]",
                    "default": [0, 0]
                },
                "effects": {
                    "type": "object",
                    "description": "Text effects (outline, shadow, etc.)",
                    "properties": {
                        "outline": {
                            "type": "object",
                            "properties": {
                                "enabled": {"type": "boolean"},
                                "color": {"type": "string"},
                                "thickness": {"type": "number"}
                            }
                        }
                    }
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_ui_create_image",
        "description": "Create a UI Image element (for icons, backgrounds, etc.)",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Image object name",
                    "default": "Image"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "color": {
                    "type": "string",
                    "description": "Tint color in hex",
                    "default": "#FFFFFF"
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Anchored position [x, y]",
                    "default": [0, 0]
                },
                "size": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Image size [width, height]",
                    "default": [100, 100]
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_ui_create_panel",
        "description": "Create a UI Panel (full-screen or custom size) for background or grouping.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Panel name",
                    "default": "Panel"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "color": {
                    "type": "string",
                    "description": "Background color in hex (use alpha for transparency)",
                    "default": "#000000AA"
                }
            },
            "required": []
        }
    },
    # UI Tools - Layout
    {
        "name": "unity_ui_create_vertical_layout",
        "description": "Create a Vertical Layout Group that automatically arranges children vertically.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Layout group name",
                    "default": "VerticalLayout"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "spacing": {
                    "type": "number",
                    "description": "Space between children",
                    "default": 10
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Anchored position [x, y]",
                    "default": [0, 0]
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_ui_create_horizontal_layout",
        "description": "Create a Horizontal Layout Group that automatically arranges children horizontally.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Layout group name",
                    "default": "HorizontalLayout"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "spacing": {
                    "type": "number",
                    "description": "Space between children",
                    "default": 10
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Anchored position [x, y]",
                    "default": [0, 0]
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_ui_create_grid_layout",
        "description": "Create a Grid Layout Group that automatically arranges children in a grid pattern.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Layout group name",
                    "default": "GridLayout"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "position": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Anchored position [x, y]",
                    "default": [0, 0]
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

