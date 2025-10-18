"""UI Creation and Management Tools"""

UI_TOOLS = [
    # Canvas Management
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
    # UI Elements
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
    # Layout Groups
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
    # UI Properties
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
    }
]



