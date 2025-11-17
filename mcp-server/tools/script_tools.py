"""Script and Component Management Tools"""

SCRIPT_TOOLS = [
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
    },
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
        "name": "unity_remove_component",
        "description": "Remove a component from a GameObject. Useful for cleanup and modification.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "gameObjectName": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "componentType": {
                    "type": "string",
                    "description": "Type name of the component to remove (e.g., 'Rigidbody', 'BoxCollider', 'EnemyScript')"
                }
            },
            "required": ["gameObjectName", "componentType"]
        }
    },
    {
        "name": "unity_set_asset_property",
        "description": "Set a property on a ScriptableObject asset or any asset file. Supports primitives, strings, and asset references (use {\"type\": \"reference\", \"path\": \"Assets/...\"} for prefabs/assets).",
        "inputSchema": {
            "type": "object",
            "properties": {
                "assetPath": {
                    "type": "string",
                    "description": "Path to the asset file (e.g., 'Assets/Resources/HeroConfigs/Archer_Hero.asset')"
                },
                "propertyName": {
                    "type": "string",
                    "description": "Name of the property/field to set (e.g., 'heroPrefab', 'maxHealth')"
                },
                "value": {
                    "description": "Value to set. For asset references, use {\"type\": \"reference\", \"path\": \"Assets/Characters/FBX/Elf.fbx\"}. For primitives, use the value directly."
                }
            },
            "required": ["assetPath", "propertyName", "value"]
        }
    }
]



