"""Core Unity MCP Tools - Basic operations"""

CORE_TOOLS = [
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
        "name": "unity_get_scene_info",
        "description": "Get information about the current Unity scene",
        "inputSchema": {
            "type": "object",
            "properties": {},
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
                    "default": True
                }
            },
            "required": []
        }
    }
]

