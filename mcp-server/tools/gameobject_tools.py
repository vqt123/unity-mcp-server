"""GameObject Operations Tools"""

GAMEOBJECT_TOOLS = [
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
        "name": "unity_set_parent",
        "description": "Set the parent of a GameObject. If parent is null or empty, unparents the object.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the child GameObject"
                },
                "parent": {
                    "type": "string",
                    "description": "Name of the parent GameObject. Leave empty or null to unparent."
                },
                "worldPositionStays": {
                    "type": "boolean",
                    "description": "If true, the GameObject keeps its world position. If false, it keeps its local position relative to the new parent.",
                    "default": True
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
        "name": "unity_list_all_gameobjects",
        "description": "List all GameObjects in the current scene (recursive). Returns full hierarchy with position, active state, tag, and layer.",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    },
    {
        "name": "unity_set_rotation",
        "description": "Set the rotation of a GameObject in world or local space.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "rotation": {
                    "type": "array",
                    "description": "Rotation as [x, y, z] in Euler angles (degrees) or [x, y, z, w] as quaternion",
                    "items": {"type": "number"},
                    "minItems": 3,
                    "maxItems": 4
                }
            },
            "required": ["name", "rotation"]
        }
    },
    {
        "name": "unity_set_scale",
        "description": "Set the scale of a GameObject.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "scale": {
                    "type": "array",
                    "description": "Scale as [x, y, z] or single value for uniform scale",
                    "items": {"type": "number"},
                    "minItems": 1,
                    "maxItems": 3
                }
            },
            "required": ["name", "scale"]
        }
    },
    {
        "name": "unity_set_tag",
        "description": "Set the tag of a GameObject. Tags are useful for finding objects and organizing the scene hierarchy.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name of the GameObject"
                },
                "tag": {
                    "type": "string",
                    "description": "Tag name (must be a valid Unity tag)"
                }
            },
            "required": ["name", "tag"]
        }
    }
]



