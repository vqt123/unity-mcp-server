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
        "name": "unity_create_primitive",
        "description": "Create a primitive GameObject (Sphere, Capsule, Cylinder, Plane, Quad, Cube) in the Unity scene",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name for the primitive",
                    "default": "Primitive"
                },
                "primitiveType": {
                    "type": "string",
                    "description": "Type of primitive: Sphere, Capsule, Cylinder, Plane, Quad, or Cube",
                    "default": "Sphere"
                },
                "position": {
                    "type": "array",
                    "description": "Position as [x, y, z]",
                    "items": {"type": "number"},
                    "default": [0, 0, 0]
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
                    "default": False
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_test_log",
        "description": "Test tool that logs a message to Unity console. Useful for verifying compilation and MCP connection.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "message": {
                    "type": "string",
                    "description": "Custom message to log (optional)",
                    "default": "Test log from MCP"
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_restart_server",
        "description": "Manually restart the MCP server. Useful after compilation when server doesn't auto-restart.",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    {
        "name": "unity_add_particle_trail",
        "description": "Add a particle trail effect to a GameObject (perfect for bullets, projectiles, etc). Creates fast-moving visual effect.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject to add particle trail to"
                },
                "color": {
                    "type": "string",
                    "description": "Particle color: red, green, blue, yellow, orange, cyan, white",
                    "default": "yellow"
                },
                "emissionRate": {
                    "type": "number",
                    "description": "Particles per second (20-100, higher = denser trail)",
                    "default": 50
                },
                "startSize": {
                    "type": "number",
                    "description": "Particle size (0.01-0.2, smaller = tighter trail)",
                    "default": 0.05
                },
                "startLifetime": {
                    "type": "number",
                    "description": "How long particles last in seconds (0.1-1.0)",
                    "default": 0.3
                }
            },
            "required": ["name"]
        }
    },
    {
        "name": "unity_save_prefab",
        "description": "Save a GameObject as a prefab asset file. This creates a reusable template that can be instantiated multiple times.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "gameObjectName": {
                    "type": "string",
                    "description": "Name of the GameObject in the scene to save as prefab"
                },
                "prefabPath": {
                    "type": "string",
                    "description": "Path where to save the prefab (e.g., 'Assets/Prefabs/Enemy.prefab' or 'Prefabs/Enemy')"
                }
            },
            "required": ["gameObjectName", "prefabPath"]
        }
    },
    {
        "name": "unity_add_script_component",
        "description": "Add a C# script component to a GameObject by script name. Useful for attaching custom behaviors to objects before saving as prefabs.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "gameObjectName": {
                    "type": "string",
                    "description": "Name of the GameObject to add the component to"
                },
                "scriptName": {
                    "type": "string",
                    "description": "Name of the script class (e.g., 'Bullet', 'Enemy', 'Hero')"
                }
            },
            "required": ["gameObjectName", "scriptName"]
        }
    },
    {
        "name": "unity_update_prefab",
        "description": "Update an existing prefab file IN-PLACE without deletion. More sustainable than delete-and-recreate. Supports adding/removing components and setting properties. This is the preferred way to modify existing prefabs.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "prefabPath": {
                    "type": "string",
                    "description": "Path to the existing prefab (e.g., 'Assets/Prefabs/Enemy.prefab')"
                },
                "action": {
                    "type": "string",
                    "enum": ["add_component", "remove_component", "set_property"],
                    "description": "What modification to perform on the prefab"
                },
                "componentType": {
                    "type": "string",
                    "description": "Component type for add/remove/set actions (e.g., 'Rigidbody', 'Enemy', 'UnityEngine.ParticleSystem')"
                },
                "propertyName": {
                    "type": "string",
                    "description": "Property name for 'set_property' action (e.g., 'bulletPrefab', 'bloodEffect')"
                },
                "value": {
                    "description": "Value to set for 'set_property' action. Can be primitive or {\"type\": \"reference\", \"path\": \"Assets/...\"} for asset references"
                }
            },
            "required": ["prefabPath", "action"]
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
                    "default": True
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
                    "default": False
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
    },
    {
        "name": "unity_ui_set_sprite",
        "description": "Set a sprite on a UI Image component. Used to apply sprites from sprite sheets to UI elements.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "objectPath": {
                    "type": "string",
                    "description": "Full path to the GameObject (e.g., 'MenuCanvas/BackgroundPanel')"
                },
                "spritePath": {
                    "type": "string",
                    "description": "Asset path to the sprite (e.g., 'Assets/2D Casual UI/Sprite/GUI.png')"
                }
            },
            "required": ["objectPath", "spritePath"]
        }
    },
    # Scene Management
    {
        "name": "unity_create_scene",
        "description": "Create a new Unity scene. Essential for creating game/menu scenes.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Scene name (without .unity extension)"
                },
                "path": {
                    "type": "string",
                    "description": "Save path for the scene",
                    "default": "Assets/Scenes/"
                },
                "setup": {
                    "type": "string",
                    "enum": ["Empty", "Default"],
                    "description": "Scene setup: 'Empty' or 'Default' (with camera and light)",
                    "default": "Default"
                }
            },
            "required": ["name"]
        }
    },
    {
        "name": "unity_save_scene",
        "description": "Save the current Unity scene. Always save after making changes.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "path": {
                    "type": "string",
                    "description": "Optional: Save path (if different from current path)"
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_load_scene",
        "description": "Load a Unity scene by name. Used for switching between scenes in the editor.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "sceneName": {
                    "type": "string",
                    "description": "Name of the scene to load (without .unity extension)"
                }
            },
            "required": ["sceneName"]
        }
    },
    {
        "name": "unity_add_scene_to_build",
        "description": "Add a scene to the Build Settings so it can be loaded at runtime. ESSENTIAL for scene switching to work in Play mode.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "scenePath": {
                    "type": "string",
                    "description": "Full path to the scene file (e.g., 'Assets/Scenes/MainMenu.unity')"
                }
            },
            "required": ["scenePath"]
        }
    },
    # GameObject Operations
    {
        "name": "unity_find_gameobject",
        "description": "Find a GameObject by name and return its information (position, path, active state).",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject to find"
                }
            },
            "required": ["name"]
        }
    },
    {
        "name": "unity_set_position",
        "description": "Set the position of a GameObject in world or local space.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "position": {
                    "type": "array",
                    "description": "Position as [x, y, z]",
                    "items": {
                        "type": "number"
                    },
                    "minItems": 3,
                    "maxItems": 3
                }
            },
            "required": ["name", "position"]
        }
    },
    {
        "name": "unity_set_anchors",
        "description": "Set UI anchors and anchored position for responsive layouts. Supports presets or custom anchor points. Essential for multi-resolution support.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the UI GameObject (must have RectTransform)"
                },
                "preset": {
                    "type": "string",
                    "description": "Anchor preset: 'top-left', 'top-center', 'top-right', 'middle-left', 'center', 'middle-right', 'bottom-left', 'bottom-center', 'bottom-right', 'stretch-horizontal', 'stretch-vertical', 'stretch-all'"
                },
                "anchorMin": {
                    "type": "array",
                    "description": "Custom anchor min [x, y] (0-1 range)",
                    "items": {"type": "number"},
                    "minItems": 2,
                    "maxItems": 2
                },
                "anchorMax": {
                    "type": "array",
                    "description": "Custom anchor max [x, y] (0-1 range)",
                    "items": {"type": "number"},
                    "minItems": 2,
                    "maxItems": 2
                },
                "anchoredPosition": {
                    "type": "array",
                    "description": "Position relative to anchors [x, y] in pixels",
                    "items": {"type": "number"},
                    "minItems": 2,
                    "maxItems": 2
                }
            },
            "required": ["name"]
        }
    },
    {
        "name": "unity_set_camera_background",
        "description": "Set camera clear flags and background color. Use to hide 3D scenes behind UI or set solid color backgrounds.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "cameraName": {
                    "type": "string",
                    "description": "Name of the camera GameObject",
                    "default": "Main Camera"
                },
                "clearFlags": {
                    "type": "string",
                    "description": "Clear flags mode: 'skybox', 'solidcolor', 'depth', or 'nothing'"
                },
                "backgroundColor": {
                    "type": "array",
                    "description": "Background color as [r, g, b] or [r, g, b, a] (0-1 range)",
                    "items": {"type": "number"},
                    "minItems": 3,
                    "maxItems": 4
                }
            },
            "required": []
        }
    },
    {
        "name": "unity_set_ui_size",
        "description": "Set the size (width and height) of a UI element's RectTransform.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Full path to the UI GameObject (e.g., 'Canvas/Button')"
                },
                "size": {
                    "type": "array",
                    "description": "Size as [width, height] in pixels",
                    "items": {"type": "number"},
                    "minItems": 2,
                    "maxItems": 2
                }
            },
            "required": ["name", "size"]
        }
    },
    # Script Management
    {
        "name": "unity_create_script",
        "description": "Create a new C# script file in Unity project. IMPORTANT: After creating scripts, use unity_force_compile and unity_wait_for_compile before using them.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Script name (without .cs extension)"
                },
                "content": {
                    "type": "string",
                    "description": "Full C# script content"
                },
                "path": {
                    "type": "string",
                    "description": "Save path for the script",
                    "default": "Assets/Scripts/"
                }
            },
            "required": ["name", "content"]
        }
    },
    {
        "name": "unity_add_component",
        "description": "Add a component to a GameObject. The component type must be compiled and available first.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "gameObjectName": {
                    "type": "string",
                    "description": "Name of the GameObject to add component to"
                },
                "componentType": {
                    "type": "string",
                    "description": "Type name of the component (e.g., 'Rigidbody', 'CubeController')"
                }
            },
            "required": ["gameObjectName", "componentType"]
        }
    },
    # Button Events
    {
        "name": "unity_set_button_onclick",
        "description": "Set button click handler to load scene or quit game. Requires a SceneLoader GameObject with SceneLoader script in the scene.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "buttonName": {
                    "type": "string",
                    "description": "Name of the Button GameObject"
                },
                "action": {
                    "type": "string",
                    "enum": ["LoadScene", "Quit"],
                    "description": "Action type: 'LoadScene' or 'Quit'"
                },
                "parameter": {
                    "type": "string",
                    "description": "Parameter for the action (e.g., scene name for LoadScene)"
                }
            },
            "required": ["buttonName", "action"]
        }
    },
    {
        "name": "unity_set_component_property",
        "description": "Set a property on a component attached to a GameObject. Supports primitives, strings, and asset references (use {\"type\": \"reference\", \"path\": \"Assets/...\"} for prefabs/assets).",
        "inputSchema": {
            "type": "object",
            "properties": {
                "gameObjectName": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "componentType": {
                    "type": "string",
                    "description": "Component type name (e.g., 'EntityVisualizer', 'ArenaGame.Client.EntityVisualizer')"
                },
                "propertyName": {
                    "type": "string",
                    "description": "Name of the property/field to set (e.g., 'heroPrefab', 'speed')"
                },
                "value": {
                    "description": "Value to set. For asset references, use {\"type\": \"reference\", \"path\": \"Assets/Prefabs/Hero.prefab\"}. For primitives, use the value directly."
                }
            },
            "required": ["gameObjectName", "componentType", "propertyName", "value"]
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

