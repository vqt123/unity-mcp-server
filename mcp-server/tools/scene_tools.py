"""Scene Management Tools"""

SCENE_TOOLS = [
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
    }
]

