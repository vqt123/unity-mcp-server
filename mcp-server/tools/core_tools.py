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
        "description": "Check if Unity compilation has finished. Returns immediately (non-blocking). If compilation is in progress, use unity_is_compiling to poll until complete. This prevents Unity Editor lock-ups.",
        "inputSchema": {
            "type": "object",
            "properties": {},
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
    }
]


