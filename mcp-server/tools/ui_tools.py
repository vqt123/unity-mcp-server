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
                },
                "referenceResolution": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Canvas Scaler reference resolution [width, height] (overrides preset)",
                    "default": [1920, 1080]
                },
                "matchValue": {
                    "type": "number",
                    "description": "Canvas Scaler match value: 0 = width, 1 = height, 0.5 = balance (overrides preset)",
                    "default": 0.5
                }
            },
            "required": []
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
    # Layout Groups (Unified)
    {
        "name": "unity_ui_create_layout",
        "description": "Create a Layout Group (Vertical, Horizontal, or Grid) that automatically arranges children. Consolidates the three layout tools into one unified tool.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Layout group name",
                    "default": "LayoutGroup"
                },
                "parent": {
                    "type": "string",
                    "description": "Parent object name",
                    "default": "Canvas"
                },
                "layoutType": {
                    "type": "string",
                    "enum": ["vertical", "horizontal", "grid"],
                    "description": "Type of layout: 'vertical', 'horizontal', or 'grid'",
                    "default": "vertical"
                },
                "spacing": {
                    "type": "number",
                    "description": "Space between children (for vertical/horizontal layouts)",
                    "default": 10
                },
                "cellSize": {
                    "type": "array",
                    "items": {"type": "number"},
                    "description": "Cell size [width, height] for grid layouts",
                    "default": [100, 100]
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
    },
    {
        "name": "unity_set_image_fill",
        "description": "Set the fill amount of a UI Image component (0-1). Used for progress bars, radial fills, etc.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Name or path to the UI Image GameObject (e.g., 'Canvas/ProgressBar')"
                },
                "fillAmount": {
                    "type": "number",
                    "description": "Fill amount from 0 to 1 (0 = empty, 1 = full)",
                    "minimum": 0,
                    "maximum": 1
                }
            },
            "required": ["name", "fillAmount"]
        }
    }
]



