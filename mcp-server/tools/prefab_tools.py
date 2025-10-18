"""Prefab Management Tools"""

PREFAB_TOOLS = [
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
    }
]



