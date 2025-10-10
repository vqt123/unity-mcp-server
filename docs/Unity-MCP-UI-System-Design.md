# Unity MCP Server - UI System Design

## World-Class UI Creation Through AI

Complete design for MCP tools that enable AI assistants to create professional, responsive, and beautiful Unity UI.

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [UI System Overview](#ui-system-overview)
3. [Core UI Tools](#core-ui-tools)
4. [Advanced UI Features](#advanced-ui-features)
5. [UI Design Patterns](#ui-design-patterns)
6. [Responsive Design System](#responsive-design-system)
7. [Animation & Transitions](#animation--transitions)
8. [Accessibility](#accessibility)
9. [Implementation Details](#implementation-details)
10. [AI-Friendly UI Creation](#ai-friendly-ui-creation)

---

## Executive Summary

### Vision

Enable AI to create **production-quality Unity UI** through natural language, from simple buttons to complex responsive layouts with animations, theming, and accessibility features.

### User Experience

```
User: "Create a main menu with play, settings, and quit buttons, 
       with hover animations and a background image"

AI: ✅ Creates Canvas
    ✅ Sets up layout with vertical button arrangement
    ✅ Creates 3 styled buttons with TextMeshPro
    ✅ Adds hover scale animations
    ✅ Applies modern dark theme
    ✅ Sets up event handlers
    ✅ Tests responsiveness

Result: Production-ready main menu in 5 seconds
```

### Goals

1. **Speed**: Create complex UI in seconds, not hours
2. **Quality**: Professional appearance by default
3. **Responsive**: Works on all screen sizes automatically
4. **Accessible**: WCAG 2.1 AA compliance
5. **Modern**: Best practices and current design trends
6. **Flexible**: Easy customization through natural language

---

## UI System Overview

### Unity UI Systems

Unity has three main UI systems we need to support:

#### 1. Unity UI (uGUI) - Legacy Canvas System
- **Status**: Stable, widely used
- **Best for**: 2D screen-space UI, mobile games
- **Components**: Canvas, Button, Text, Image, etc.

#### 2. UI Toolkit (UIElements) - Modern System
- **Status**: Recommended for new projects
- **Best for**: Editor extensions, complex layouts
- **Components**: UXML, USS, C# scripting

#### 3. TextMeshPro (TMP)
- **Status**: Industry standard for text
- **Best for**: All text rendering
- **Features**: Better quality, effects, emoji support

### MCP UI Strategy

**Phase 1**: Focus on Unity UI (uGUI) + TextMeshPro
- Most widely adopted
- Easier for AI to understand
- Rich component ecosystem

**Phase 2**: Add UI Toolkit support
- Modern and future-proof
- Better for complex UIs
- Closer to web development

**Phase 3**: Advanced features
- Custom shaders
- Procedural UI generation
- VR/AR UI support

---

## Core UI Tools

### 60+ UI-Specific Tools Across 8 Categories

### Category 1: Canvas Management (8 tools)

#### `unity_ui_create_canvas`
Create a UI Canvas with automatic setup.

**Parameters**:
```json
{
  "name": "MainMenuCanvas",
  "renderMode": "ScreenSpaceOverlay",  // or Camera, WorldSpace
  "sortingOrder": 0,
  "pixelPerfect": false,
  "additionalComponents": ["GraphicRaycaster"]
}
```

**AI-Friendly Options**:
```json
{
  "preset": "game_menu",  // Auto-configures for game UI
  "resolution": [1920, 1080],
  "scaleMode": "scale_with_screen_size"
}
```

---

#### `unity_ui_setup_canvas_scaler`
Configure canvas scaling for responsiveness.

**Parameters**:
```json
{
  "canvas": "MainMenuCanvas",
  "uiScaleMode": "ScaleWithScreenSize",
  "referenceResolution": [1920, 1080],
  "screenMatchMode": "MatchWidthOrHeight",
  "matchValue": 0.5,  // 0=width, 1=height, 0.5=balanced
  "referencePixelsPerUnit": 100
}
```

**Presets**:
- `mobile_portrait`: 1080x1920, match height
- `mobile_landscape`: 1920x1080, match width
- `desktop`: 1920x1080, balanced
- `tablet`: 1536x2048, balanced

---

#### `unity_ui_create_event_system`
Set up input handling for UI.

**Parameters**:
```json
{
  "inputModule": "Standalone",  // or Touch, VR
  "firstSelected": "PlayButton"
}
```

---

### Category 2: UI Elements (15 tools)

#### `unity_ui_create_button`
Create a fully configured button.

**Parameters**:
```json
{
  "name": "PlayButton",
  "parent": "MainMenuCanvas",
  "position": [0, 100],  // Anchored position
  "size": [200, 60],
  "text": "Play Game",
  "textSize": 24,
  "style": "primary",  // or secondary, danger, success
  "onClick": {
    "action": "LoadScene",
    "parameters": {"sceneName": "Level1"}
  }
}
```

**AI-Enhanced Features**:
```json
{
  "autoSize": true,  // Size based on text
  "icon": "play_arrow",  // Material Design icons
  "iconPosition": "left",
  "tooltip": "Start a new game",
  "soundOnClick": "UI_Button_Click",
  "hapticFeedback": true,
  "animation": {
    "hover": "scale_up",
    "click": "press_down"
  }
}
```

**Result**:
```json
{
  "success": true,
  "buttonPath": "MainMenuCanvas/PlayButton",
  "components": ["Button", "Image", "TextMeshProUGUI"],
  "eventHandlers": ["PointerEnter", "PointerExit", "Click"]
}
```

---

#### `unity_ui_create_text`
Create a TextMeshPro text element.

**Parameters**:
```json
{
  "name": "TitleText",
  "parent": "MainMenuCanvas",
  "text": "Epic Adventure",
  "fontSize": 48,
  "font": "Roboto-Bold",
  "color": "#FFFFFF",
  "alignment": "center",
  "position": [0, 200],
  "autoSize": true,
  "enableRichText": true,
  "overflow": "ellipsis",
  "effects": {
    "outline": {
      "enabled": true,
      "color": "#000000",
      "thickness": 0.2
    },
    "shadow": {
      "enabled": true,
      "offset": [2, -2],
      "color": "#00000080"
    },
    "gradient": {
      "enabled": true,
      "topColor": "#FFFFFF",
      "bottomColor": "#CCCCCC"
    }
  }
}
```

---

#### `unity_ui_create_image`
Create an Image component.

**Parameters**:
```json
{
  "name": "Background",
  "parent": "MainMenuCanvas",
  "sprite": "UI/Backgrounds/MenuBG",
  "color": "#FFFFFF",
  "material": "UI/Default",
  "imageType": "Sliced",  // Simple, Sliced, Tiled, Filled
  "fillMethod": "Horizontal",  // For Filled type
  "fillAmount": 1.0,
  "preserveAspect": true,
  "raycastTarget": true,
  "maskable": true
}
```

**AI Features**:
```json
{
  "preset": "background",  // Automatically sizes to canvas
  "blur": 5,  // Background blur effect
  "overlay": "#00000040",  // Tint overlay
  "parallax": 0.5  // Parallax scrolling speed
}
```

---

#### `unity_ui_create_panel`
Create a container panel.

**Parameters**:
```json
{
  "name": "SettingsPanel",
  "parent": "MainMenuCanvas",
  "size": [600, 400],
  "anchors": {
    "min": [0.5, 0.5],
    "max": [0.5, 0.5],
    "pivot": [0.5, 0.5]
  },
  "background": {
    "color": "#2D2D2DDD",
    "sprite": "UI/Panel",
    "type": "Sliced"
  },
  "border": {
    "enabled": true,
    "color": "#FFFFFF40",
    "width": 2
  },
  "shadow": {
    "enabled": true,
    "distance": 10,
    "softness": 5,
    "color": "#00000080"
  }
}
```

---

#### `unity_ui_create_slider`
Create a slider control.

**Parameters**:
```json
{
  "name": "VolumeSlider",
  "parent": "SettingsPanel",
  "minValue": 0,
  "maxValue": 100,
  "value": 75,
  "wholeNumbers": true,
  "label": "Master Volume",
  "showValueText": true,
  "valueFormat": "{0}%",
  "style": "modern",
  "onValueChanged": {
    "action": "SetVolume",
    "parameters": {"channel": "master"}
  }
}
```

---

#### `unity_ui_create_toggle`
Create a toggle/checkbox.

**Parameters**:
```json
{
  "name": "MusicToggle",
  "parent": "SettingsPanel",
  "label": "Enable Music",
  "isOn": true,
  "style": "checkbox",  // or switch, radio
  "onValueChanged": {
    "action": "ToggleMusic"
  }
}
```

---

#### `unity_ui_create_dropdown`
Create a dropdown menu.

**Parameters**:
```json
{
  "name": "QualityDropdown",
  "parent": "SettingsPanel",
  "label": "Graphics Quality",
  "options": ["Low", "Medium", "High", "Ultra"],
  "selectedIndex": 2,
  "style": "modern",
  "onValueChanged": {
    "action": "SetQuality"
  }
}
```

---

#### `unity_ui_create_input_field`
Create a text input field.

**Parameters**:
```json
{
  "name": "PlayerNameInput",
  "parent": "ProfilePanel",
  "placeholder": "Enter your name",
  "characterLimit": 20,
  "contentType": "Alphanumeric",
  "validation": "^[a-zA-Z0-9]+$",
  "onValueChanged": {
    "action": "UpdatePlayerName"
  },
  "onSubmit": {
    "action": "SaveProfile"
  }
}
```

---

#### `unity_ui_create_scroll_view`
Create a scrollable area.

**Parameters**:
```json
{
  "name": "InventoryScroll",
  "parent": "InventoryPanel",
  "size": [400, 500],
  "horizontal": false,
  "vertical": true,
  "scrollSensitivity": 20,
  "inertia": true,
  "elasticity": 0.1,
  "scrollbarVisibility": "AutoHide"
}
```

---

#### `unity_ui_create_grid`
Create a grid layout.

**Parameters**:
```json
{
  "name": "ItemGrid",
  "parent": "InventoryScroll/Viewport/Content",
  "cellSize": [80, 80],
  "spacing": [10, 10],
  "constraint": "FixedColumnCount",
  "constraintCount": 4,
  "startCorner": "UpperLeft",
  "startAxis": "Horizontal",
  "childAlignment": "UpperLeft"
}
```

---

#### `unity_ui_create_progress_bar`
Create a progress/health bar.

**Parameters**:
```json
{
  "name": "HealthBar",
  "parent": "HUD",
  "position": [0, -50],
  "size": [300, 30],
  "minValue": 0,
  "maxValue": 100,
  "currentValue": 75,
  "fillDirection": "LeftToRight",
  "colors": {
    "background": "#3D3D3D",
    "fill": "#00FF00",
    "border": "#FFFFFF"
  },
  "gradient": {
    "enabled": true,
    "colorStops": [
      {"value": 0, "color": "#FF0000"},
      {"value": 0.5, "color": "#FFFF00"},
      {"value": 1, "color": "#00FF00"}
    ]
  },
  "showText": true,
  "textFormat": "{current}/{max}",
  "animation": {
    "smooth": true,
    "duration": 0.3
  }
}
```

---

#### `unity_ui_create_tooltip`
Create a tooltip system.

**Parameters**:
```json
{
  "name": "TooltipSystem",
  "style": "dark",
  "followMouse": true,
  "offset": [10, 10],
  "showDelay": 0.5,
  "hideDelay": 0.1,
  "maxWidth": 300,
  "animation": "fade"
}
```

---

### Category 3: Layout Management (12 tools)

#### `unity_ui_set_anchors`
Set RectTransform anchors precisely.

**Parameters**:
```json
{
  "target": "PlayButton",
  "preset": "center",  // or custom values
  "anchors": {
    "min": [0.5, 0.5],
    "max": [0.5, 0.5]
  },
  "pivot": [0.5, 0.5],
  "anchoredPosition": [0, 100]
}
```

**Anchor Presets**:
- `top_left`, `top_center`, `top_right`
- `middle_left`, `center`, `middle_right`
- `bottom_left`, `bottom_center`, `bottom_right`
- `stretch_horizontal`, `stretch_vertical`, `stretch_full`

---

#### `unity_ui_add_layout_group`
Add layout component for auto-arrangement.

**Parameters**:
```json
{
  "target": "ButtonPanel",
  "layoutType": "Vertical",  // Horizontal, Vertical, Grid
  "spacing": 10,
  "padding": {
    "left": 20,
    "right": 20,
    "top": 20,
    "bottom": 20
  },
  "childAlignment": "MiddleCenter",
  "childControlSize": true,
  "childForceExpand": false,
  "reverseArrangement": false
}
```

---

#### `unity_ui_add_content_size_fitter`
Make element auto-size to content.

**Parameters**:
```json
{
  "target": "TextPanel",
  "horizontalFit": "PreferredSize",
  "verticalFit": "PreferredSize"
}
```

---

#### `unity_ui_add_aspect_ratio_fitter`
Maintain aspect ratio.

**Parameters**:
```json
{
  "target": "VideoPlayer",
  "aspectMode": "FitInParent",
  "aspectRatio": 1.777  // 16:9
}
```

---

#### `unity_ui_create_safe_area`
Handle notches and safe areas (mobile).

**Parameters**:
```json
{
  "target": "MainCanvas",
  "respectSafeArea": true,
  "orientation": "portrait"
}
```

---

#### `unity_ui_arrange_elements`
Smart arrangement of multiple elements.

**Parameters**:
```json
{
  "elements": ["Button1", "Button2", "Button3"],
  "arrangement": "vertical_stack",  // horizontal_stack, grid, circular
  "spacing": 15,
  "alignment": "center",
  "animate": true
}
```

---

### Category 4: Styling & Theming (10 tools)

#### `unity_ui_create_theme`
Create a comprehensive UI theme.

**Parameters**:
```json
{
  "name": "DarkTheme",
  "colors": {
    "primary": "#2196F3",
    "secondary": "#FFC107",
    "background": "#1E1E1E",
    "surface": "#2D2D2D",
    "error": "#F44336",
    "success": "#4CAF50",
    "warning": "#FF9800",
    "text": "#FFFFFF",
    "textSecondary": "#B0B0B0",
    "disabled": "#666666",
    "border": "#404040"
  },
  "typography": {
    "fontFamily": "Roboto",
    "h1": {"size": 48, "weight": "Bold"},
    "h2": {"size": 36, "weight": "Bold"},
    "h3": {"size": 24, "weight": "Medium"},
    "body": {"size": 16, "weight": "Regular"},
    "caption": {"size": 12, "weight": "Regular"}
  },
  "spacing": {
    "xs": 4,
    "sm": 8,
    "md": 16,
    "lg": 24,
    "xl": 32
  },
  "borderRadius": {
    "none": 0,
    "sm": 4,
    "md": 8,
    "lg": 16,
    "full": 9999
  },
  "shadows": {
    "sm": {"distance": 2, "softness": 2},
    "md": {"distance": 5, "softness": 5},
    "lg": {"distance": 10, "softness": 10}
  }
}
```

---

#### `unity_ui_apply_theme`
Apply theme to element or hierarchy.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "theme": "DarkTheme",
  "recursive": true,
  "override": false
}
```

---

#### `unity_ui_style_button`
Apply comprehensive button styling.

**Parameters**:
```json
{
  "target": "PlayButton",
  "variant": "primary",  // secondary, outlined, text, icon
  "colors": {
    "normal": "#2196F3",
    "hover": "#42A5F5",
    "pressed": "#1976D2",
    "disabled": "#666666"
  },
  "textColor": "#FFFFFF",
  "borderRadius": 8,
  "elevation": 2,  // Shadow depth
  "rippleEffect": true
}
```

---

#### `unity_ui_style_panel`
Apply panel styling.

**Parameters**:
```json
{
  "target": "SettingsPanel",
  "background": "#2D2D2D",
  "borderRadius": 16,
  "border": {
    "width": 1,
    "color": "#404040"
  },
  "shadow": {
    "elevation": 8,
    "color": "#000000"
  },
  "blur": 10
}
```

---

#### `unity_ui_apply_gradient`
Apply gradient to UI element.

**Parameters**:
```json
{
  "target": "TitleText",
  "gradientType": "vertical",
  "colors": [
    {"position": 0, "color": "#FFD700"},
    {"position": 1, "color": "#FFA500"}
  ]
}
```

---

### Category 5: Animation & Transitions (8 tools)

#### `unity_ui_animate_element`
Animate UI element properties.

**Parameters**:
```json
{
  "target": "PlayButton",
  "animation": {
    "property": "scale",
    "from": [1, 1, 1],
    "to": [1.1, 1.1, 1],
    "duration": 0.2,
    "easing": "easeOutBack",
    "loop": false,
    "yoyo": true
  },
  "trigger": "hover"
}
```

**Animation Properties**:
- `position`, `rotation`, `scale`
- `color`, `alpha`
- `size`, `anchored_position`
- Custom properties

**Easing Functions**:
- Linear
- EaseIn/Out/InOut (Quad, Cubic, Quart, Quint, Sine, Expo, Circ, Back, Elastic, Bounce)

---

#### `unity_ui_create_transition`
Create state transitions for interactive elements.

**Parameters**:
```json
{
  "target": "PlayButton",
  "transitions": {
    "normal_to_hover": {
      "scale": 1.05,
      "color": "#42A5F5",
      "duration": 0.15
    },
    "hover_to_pressed": {
      "scale": 0.95,
      "color": "#1976D2",
      "duration": 0.1
    },
    "any_to_disabled": {
      "color": "#666666",
      "alpha": 0.5,
      "duration": 0.2
    }
  }
}
```

---

#### `unity_ui_animate_show`
Animate element appearing.

**Parameters**:
```json
{
  "target": "SettingsPanel",
  "animation": "fade_scale",  // fade, slide, scale, bounce
  "direction": "center",  // for slide: up, down, left, right
  "duration": 0.3,
  "easing": "easeOutQuart",
  "delay": 0
}
```

---

#### `unity_ui_animate_hide`
Animate element disappearing.

**Parameters**:
```json
{
  "target": "SettingsPanel",
  "animation": "fade_scale",
  "direction": "center",
  "duration": 0.2,
  "easing": "easeInQuart",
  "destroyAfter": false
}
```

---

#### `unity_ui_create_page_transition`
Transition between UI screens/pages.

**Parameters**:
```json
{
  "from": "MainMenu",
  "to": "LevelSelect",
  "transition": "slide_left",  // slide, fade, cross_fade, push, reveal
  "duration": 0.4,
  "easing": "easeInOutQuart"
}
```

---

#### `unity_ui_add_pulse_animation`
Add attention-grabbing pulse effect.

**Parameters**:
```json
{
  "target": "NewItemBadge",
  "pulseScale": 1.2,
  "duration": 0.8,
  "loop": true
}
```

---

#### `unity_ui_add_floating_animation`
Add floating/bobbing animation.

**Parameters**:
```json
{
  "target": "FloatingIcon",
  "amplitude": 10,
  "frequency": 1,
  "axis": "y"
}
```

---

### Category 6: Interactive Features (5 tools)

#### `unity_ui_add_click_handler`
Add click/tap handling.

**Parameters**:
```json
{
  "target": "PlayButton",
  "action": "LoadScene",
  "parameters": {"sceneName": "Level1"},
  "soundEffect": "UI_Button_Click",
  "hapticFeedback": true,
  "cooldown": 0.5  // Prevent double-clicks
}
```

---

#### `unity_ui_add_hover_handler`
Add hover effects (PC/console).

**Parameters**:
```json
{
  "target": "PlayButton",
  "onEnter": {
    "sound": "UI_Hover",
    "cursor": "pointer"
  },
  "onExit": {
    "cursor": "default"
  }
}
```

---

#### `unity_ui_add_drag_handler`
Make element draggable.

**Parameters**:
```json
{
  "target": "InventoryItem",
  "dragType": "free",  // horizontal, vertical, free
  "returnToOrigin": false,
  "onDragStart": "ItemDragStart",
  "onDrag": "ItemDrag",
  "onDragEnd": "ItemDragEnd"
}
```

---

#### `unity_ui_add_drop_zone`
Create drop target area.

**Parameters**:
```json
{
  "target": "EquipmentSlot",
  "acceptedTypes": ["Weapon", "Armor"],
  "highlightOnHover": true,
  "onDrop": "EquipItem"
}
```

---

#### `unity_ui_add_context_menu`
Add right-click context menu.

**Parameters**:
```json
{
  "target": "InventoryItem",
  "menuItems": [
    {"label": "Use", "action": "UseItem", "icon": "use"},
    {"label": "Drop", "action": "DropItem", "icon": "delete"},
    {"label": "Info", "action": "ShowInfo", "icon": "info"}
  ]
}
```

---

### Category 7: Advanced Components (7 tools)

#### `unity_ui_create_card`
Create a modern card component.

**Parameters**:
```json
{
  "name": "CharacterCard",
  "parent": "CardGrid",
  "size": [250, 350],
  "content": {
    "image": "Characters/Hero1",
    "title": "Knight",
    "subtitle": "Level 10 Warrior",
    "description": "A brave knight with high defense",
    "stats": [
      {"label": "Health", "value": 100, "max": 100},
      {"label": "Attack", "value": 75, "max": 100}
    ],
    "button": {
      "text": "Select",
      "action": "SelectCharacter"
    }
  },
  "style": {
    "theme": "DarkTheme",
    "elevation": 4,
    "borderRadius": 12,
    "hoverEffect": "lift_glow"
  }
}
```

---

#### `unity_ui_create_notification`
Create notification/toast message.

**Parameters**:
```json
{
  "message": "Item collected!",
  "type": "success",  // info, success, warning, error
  "duration": 3,
  "position": "top_right",
  "icon": "check_circle",
  "dismissible": true,
  "sound": "Notification_Success"
}
```

---

#### `unity_ui_create_modal`
Create modal dialog.

**Parameters**:
```json
{
  "name": "ConfirmDialog",
  "title": "Confirm Action",
  "message": "Are you sure you want to delete this item?",
  "icon": "warning",
  "buttons": [
    {
      "text": "Cancel",
      "variant": "secondary",
      "action": "CloseModal"
    },
    {
      "text": "Delete",
      "variant": "danger",
      "action": "DeleteItem"
    }
  ],
  "backdrop": {
    "color": "#00000080",
    "blur": 5,
    "closeOnClick": false
  }
}
```

---

#### `unity_ui_create_tabs`
Create tabbed interface.

**Parameters**:
```json
{
  "name": "SettingsTabs",
  "parent": "SettingsPanel",
  "tabs": [
    {"id": "graphics", "label": "Graphics", "icon": "monitor"},
    {"id": "audio", "label": "Audio", "icon": "volume_up"},
    {"id": "controls", "label": "Controls", "icon": "gamepad"}
  ],
  "defaultTab": "graphics",
  "style": "underlined",  // pills, underlined, enclosed
  "orientation": "horizontal"
}
```

---

#### `unity_ui_create_accordion`
Create expandable sections.

**Parameters**:
```json
{
  "name": "FAQAccordion",
  "parent": "HelpPanel",
  "sections": [
    {
      "title": "How to play?",
      "content": "Use WASD to move...",
      "expanded": true
    },
    {
      "title": "Controls",
      "content": "Jump: Space\nAttack: Mouse Left..."
    }
  ],
  "allowMultiple": false
}
```

---

#### `unity_ui_create_carousel`
Create image/content carousel.

**Parameters**:
```json
{
  "name": "LevelCarousel",
  "parent": "LevelSelectPanel",
  "items": ["Level1", "Level2", "Level3"],
  "autoPlay": false,
  "loop": true,
  "showIndicators": true,
  "showArrows": true,
  "transitionDuration": 0.5
}
```

---

#### `unity_ui_create_mini_map`
Create mini-map UI component.

**Parameters**:
```json
{
  "name": "MiniMap",
  "parent": "HUD",
  "position": "top_right",
  "size": [200, 200],
  "camera": "MinimapCamera",
  "zoomLevels": [1, 2, 4],
  "showPlayerIcon": true,
  "showEnemies": true,
  "showObjectives": true,
  "interactive": true
}
```

---

### Category 8: Responsive Design (5 tools)

#### `unity_ui_create_breakpoints`
Define responsive breakpoints.

**Parameters**:
```json
{
  "canvas": "MainCanvas",
  "breakpoints": {
    "mobile": {"width": 0, "height": 0},
    "tablet": {"width": 768, "height": 1024},
    "desktop": {"width": 1920, "height": 1080},
    "ultrawide": {"width": 2560, "height": 1080}
  }
}
```

---

#### `unity_ui_set_responsive_layout`
Make layout adapt to screen size.

**Parameters**:
```json
{
  "target": "MainMenu",
  "layouts": {
    "mobile": {
      "buttonSize": [150, 50],
      "spacing": 10,
      "orientation": "vertical"
    },
    "desktop": {
      "buttonSize": [200, 60],
      "spacing": 15,
      "orientation": "horizontal"
    }
  }
}
```

---

#### `unity_ui_create_adaptive_grid`
Create grid that adapts to screen size.

**Parameters**:
```json
{
  "target": "ItemGrid",
  "minCellSize": [80, 80],
  "maxCellSize": [120, 120],
  "preferredColumns": 4,
  "maintainAspectRatio": true
}
```

---

#### `unity_ui_hide_on_platform`
Show/hide based on platform.

**Parameters**:
```json
{
  "target": "MouseCursor",
  "hideOn": ["iOS", "Android"],
  "showOn": ["Windows", "Mac", "Linux"]
}
```

---

## AI-Friendly UI Creation

### Natural Language Patterns

#### Pattern 1: High-Level UI Description

```
User: "Create a modern settings menu with graphics, audio, and controls tabs"

AI Analysis:
1. Detects: menu creation request
2. Identifies: modern style, tabbed layout
3. Infers: need for Canvas, EventSystem, Theme
4. Plans: Multi-step UI creation

AI Actions:
unity_ui_create_canvas(name="SettingsCanvas", preset="game_menu")
unity_ui_create_theme(name="ModernTheme", style="dark")
unity_ui_create_panel(name="SettingsPanel", style="modern")
unity_ui_create_tabs(
  tabs=[
    {id: "graphics", label: "Graphics"},
    {id: "audio", label: "Audio"},
    {id: "controls", label: "Controls"}
  ]
)
unity_ui_populate_graphics_tab(...)
unity_ui_populate_audio_tab(...)
unity_ui_populate_controls_tab(...)
```

---

#### Pattern 2: Style-Based Creation

```
User: "Make it look like a mobile game from 2024 - colorful, rounded, with animations"

AI Understands:
- Modern mobile aesthetics
- Material Design 3 / iOS-style
- Vibrant colors
- Smooth animations

AI Creates:
- Theme with gradient colors
- High border radius (16-24px)
- Elevation shadows
- Micro-interactions
- Haptic feedback ready
```

---

#### Pattern 3: Functional Requirements

```
User: "I need a shop UI where players can buy items with coins"

AI Creates:
1. Shop canvas with currency display
2. Grid of purchasable items (cards)
3. Each card shows: image, name, price, buy button
4. Confirmation modal for purchases
5. Insufficient funds notification
6. Purchase success animation
```

---

### AI Design Intelligence

#### Smart Defaults

AI should apply professional defaults automatically:

1. **Spacing**: Follow 8px grid system
2. **Colors**: Accessible contrast ratios (4.5:1+)
3. **Typography**: Size hierarchy (12/16/24/36/48)
4. **Animations**: 200-400ms for most transitions
5. **Touch Targets**: Minimum 44x44px
6. **Shadows**: Elevation-based depth
7. **Borders**: Subtle, 1-2px typically
8. **Feedback**: Visual response within 100ms

#### Style Inference

```
User: "Create a button"

AI Analyzes Context:
- Existing theme? → Apply theme
- No theme? → Create modern default
- Parent style? → Inherit properties
- Platform? → Adjust for touch/mouse
- User preferences? → Remember and apply

Result: Professional button without explicit styling
```

---

### Composition Patterns

#### Pattern: Navigation Menu

```
User: "Add a nav menu with Home, Inventory, Settings, Logout"

AI Creates:
- Horizontal or vertical based on context
- Icon + text for each item
- Active state highlighting
- Hover effects (if desktop)
- Proper spacing and alignment
- Responsive behavior
```

#### Pattern: Form Layout

```
User: "Create a login form"

AI Creates:
- Panel container
- Title "Login"
- Username input field
- Password input field (masked)
- "Remember me" checkbox
- Login button (primary)
- "Forgot password?" link (secondary)
- Form validation
- Error message display
- Loading state
```

#### Pattern: Dashboard

```
User: "Create a game dashboard"

AI Creates:
- Canvas with safe area handling
- Top bar: currency, level, profile
- Center: main action button
- Grid: feature cards
- Bottom: navigation menu
- Notification area
```

---

## Responsive Design System

### Screen Size Handling

#### Breakpoint System

```csharp
public enum ScreenSize
{
    Mobile,      // < 768px
    Tablet,      // 768-1366px
    Desktop,     // 1366-2560px
    Ultrawide    // > 2560px
}

public class ResponsiveUI
{
    private ScreenSize currentSize;
    
    void OnResolutionChange()
    {
        ScreenSize newSize = DetectScreenSize();
        if (newSize != currentSize)
        {
            ApplyResponsiveLayout(newSize);
        }
    }
}
```

### Adaptive Components

#### Responsive Text

```csharp
// Auto-scale text based on screen size
[Serializable]
public class ResponsiveText
{
    public float mobileFontSize = 14;
    public float tabletFontSize = 16;
    public float desktopFontSize = 18;
    
    public float GetFontSize(ScreenSize size)
    {
        return size switch
        {
            ScreenSize.Mobile => mobileFontSize,
            ScreenSize.Tablet => tabletFontSize,
            ScreenSize.Desktop => desktopFontSize,
            _ => desktopFontSize
        };
    }
}
```

#### Responsive Layout

```csharp
// Auto-adjust layout based on aspect ratio
public class ResponsiveLayout : MonoBehaviour
{
    public LayoutConfiguration portraitLayout;
    public LayoutConfiguration landscapeLayout;
    
    void Update()
    {
        bool isPortrait = Screen.height > Screen.width;
        ApplyLayout(isPortrait ? portraitLayout : landscapeLayout);
    }
}
```

---

## Accessibility Features

### WCAG 2.1 AA Compliance

#### Tool: `unity_ui_check_accessibility`

Validates UI for accessibility:

```json
{
  "target": "MainMenu",
  "checks": [
    "color_contrast",
    "text_size",
    "touch_target_size",
    "keyboard_navigation",
    "screen_reader_support"
  ],
  "reportIssues": true,
  "autoFix": false
}
```

**Results**:
```json
{
  "score": 85,
  "issues": [
    {
      "severity": "warning",
      "element": "SecondaryButton",
      "issue": "Text contrast ratio 3.8:1, minimum 4.5:1",
      "suggestion": "Darken text or lighten background"
    }
  ]
}
```

---

### Accessible UI Features

#### 1. Color Contrast

```csharp
public static bool CheckColorContrast(Color text, Color background)
{
    float ratio = CalculateContrastRatio(text, background);
    return ratio >= 4.5f; // WCAG AA standard
}
```

#### 2. Keyboard Navigation

```json
{
  "target": "MainMenu",
  "navigation": {
    "enabled": true,
    "initialSelection": "PlayButton",
    "wrapAround": true,
    "navigationScheme": {
      "up": "SettingsButton",
      "down": "QuitButton",
      "left": null,
      "right": null
    }
  }
}
```

#### 3. Screen Reader Support

```json
{
  "target": "PlayButton",
  "accessibility": {
    "label": "Play Game Button",
    "hint": "Starts a new game",
    "traits": ["button"],
    "value": null
  }
}
```

#### 4. Touch Target Sizes

```csharp
// Ensure minimum 44x44 points for touch
public void ValidateTouchTarget(RectTransform element)
{
    Vector2 size = element.sizeDelta;
    if (size.x < 44 || size.y < 44)
    {
        Debug.LogWarning($"{element.name} is too small for touch");
    }
}
```

---

## Implementation Details

### Unity C# Implementation

#### UI Manager System

```csharp
// Assets/Editor/MCPServer/Commands/UICommands.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Newtonsoft.Json.Linq;

namespace UnityMCP.Commands.UI
{
    public class CreateButtonCommand : ICommand
    {
        public object Execute(JObject args)
        {
            // Parse parameters
            string name = args["name"]?.ToString() ?? "Button";
            string parentPath = args["parent"]?.ToString();
            Vector2 position = args["position"]?.ToObject<Vector2>() ?? Vector2.zero;
            Vector2 size = args["size"]?.ToObject<Vector2>() ?? new Vector2(200, 60);
            string text = args["text"]?.ToString() ?? "Button";
            float textSize = args["textSize"]?.ToObject<float>() ?? 24f;
            string style = args["style"]?.ToString() ?? "primary";
            
            // Find or create canvas
            Canvas canvas = FindCanvas(parentPath);
            if (canvas == null)
            {
                canvas = CreateDefaultCanvas();
            }
            
            // Create button GameObject
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(canvas.transform, false);
            
            // Add Image component
            Image image = buttonObj.AddComponent<Image>();
            image.sprite = LoadButtonSprite(style);
            image.type = Image.Type.Sliced;
            image.color = GetStyleColor(style);
            
            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            
            // Set up color transitions
            var colors = button.colors;
            colors.normalColor = GetStyleColor(style);
            colors.highlightedColor = GetStyleColor(style + "_hover");
            colors.pressedColor = GetStyleColor(style + "_pressed");
            colors.disabledColor = GetStyleColor("disabled");
            button.colors = colors;
            
            // Set up RectTransform
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = textSize;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.enableWordWrapping = false;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Add click handler if specified
            if (args["onClick"] != null)
            {
                AddClickHandler(button, args["onClick"] as JObject);
            }
            
            // Add animations if specified
            if (args["animation"] != null)
            {
                AddButtonAnimations(buttonObj, args["animation"] as JObject);
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(buttonObj, $"Create {name}");
            
            return new
            {
                success = true,
                buttonPath = GetGameObjectPath(buttonObj),
                instanceId = buttonObj.GetInstanceID(),
                components = new[] { "Image", "Button", "TextMeshProUGUI" }
            };
        }
        
        private void AddClickHandler(Button button, JObject onClick)
        {
            string action = onClick["action"]?.ToString();
            JObject parameters = onClick["parameters"] as JObject;
            
            button.onClick.AddListener(() =>
            {
                // Execute action through command system
                CommandExecutor.Execute(action, parameters);
            });
        }
        
        private void AddButtonAnimations(GameObject button, JObject animation)
        {
            ButtonAnimator animator = button.AddComponent<ButtonAnimator>();
            
            if (animation["hover"]?.ToString() == "scale_up")
            {
                animator.hoverScale = 1.05f;
                animator.hoverDuration = 0.2f;
            }
            
            if (animation["click"]?.ToString() == "press_down")
            {
                animator.pressScale = 0.95f;
                animator.pressDuration = 0.1f;
            }
        }
        
        private Color GetStyleColor(string style)
        {
            return style switch
            {
                "primary" => new Color(0.129f, 0.588f, 0.953f), // #2196F3
                "primary_hover" => new Color(0.259f, 0.647f, 0.961f), // #42A5F5
                "primary_pressed" => new Color(0.098f, 0.463f, 0.824f), // #1976D2
                "secondary" => new Color(1f, 0.761f, 0.027f), // #FFC107
                "danger" => new Color(0.957f, 0.263f, 0.212f), // #F44336
                "success" => new Color(0.298f, 0.686f, 0.314f), // #4CAF50
                "disabled" => new Color(0.4f, 0.4f, 0.4f), // #666666
                _ => Color.white
            };
        }
    }
    
    // Button animation component
    public class ButtonAnimator : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, 
        IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.05f;
        public float hoverDuration = 0.2f;
        public float pressScale = 0.95f;
        public float pressDuration = 0.1f;
        
        private Vector3 originalScale;
        private Coroutine currentAnimation;
        
        void Start()
        {
            originalScale = transform.localScale;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(
                AnimateScale(originalScale * hoverScale, hoverDuration)
            );
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(
                AnimateScale(originalScale, hoverDuration)
            );
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(
                AnimateScale(originalScale * pressScale, pressDuration)
            );
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(
                AnimateScale(originalScale * hoverScale, pressDuration)
            );
        }
        
        private IEnumerator AnimateScale(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad
                t = 1 - (1 - t) * (1 - t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }
    }
}
```

---

### Theme System Implementation

```csharp
// Assets/Editor/MCPServer/UI/ThemeManager.cs
using UnityEngine;
using System.Collections.Generic;

namespace UnityMCP.UI
{
    [System.Serializable]
    public class UITheme
    {
        public string name;
        public ColorPalette colors;
        public Typography typography;
        public Spacing spacing;
        public Shadows shadows;
        public BorderRadius borderRadius;
    }
    
    [System.Serializable]
    public class ColorPalette
    {
        public Color primary;
        public Color secondary;
        public Color background;
        public Color surface;
        public Color error;
        public Color success;
        public Color warning;
        public Color text;
        public Color textSecondary;
        public Color disabled;
        public Color border;
    }
    
    [System.Serializable]
    public class Typography
    {
        public string fontFamily;
        public TextStyle h1;
        public TextStyle h2;
        public TextStyle h3;
        public TextStyle body;
        public TextStyle caption;
    }
    
    [System.Serializable]
    public class TextStyle
    {
        public float size;
        public FontWeight weight;
    }
    
    public enum FontWeight
    {
        Regular,
        Medium,
        Bold
    }
    
    public static class ThemeManager
    {
        private static Dictionary<string, UITheme> themes = new Dictionary<string, UITheme>();
        private static UITheme currentTheme;
        
        public static void RegisterTheme(UITheme theme)
        {
            themes[theme.name] = theme;
        }
        
        public static void ApplyTheme(string themeName, GameObject root, bool recursive)
        {
            if (!themes.TryGetValue(themeName, out UITheme theme))
            {
                Debug.LogError($"Theme '{themeName}' not found");
                return;
            }
            
            currentTheme = theme;
            
            if (recursive)
            {
                ApplyThemeRecursive(root.transform, theme);
            }
            else
            {
                ApplyThemeToObject(root, theme);
            }
        }
        
        private static void ApplyThemeRecursive(Transform transform, UITheme theme)
        {
            ApplyThemeToObject(transform.gameObject, theme);
            
            foreach (Transform child in transform)
            {
                ApplyThemeRecursive(child, theme);
            }
        }
        
        private static void ApplyThemeToObject(GameObject obj, UITheme theme)
        {
            // Apply to Button
            Button button = obj.GetComponent<Button>();
            if (button != null)
            {
                ApplyThemeToButton(button, theme);
            }
            
            // Apply to Text
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                ApplyThemeToText(text, theme);
            }
            
            // Apply to Image (panels, backgrounds)
            Image image = obj.GetComponent<Image>();
            if (image != null && obj.name.Contains("Panel"))
            {
                image.color = theme.colors.surface;
            }
        }
        
        private static void ApplyThemeToButton(Button button, UITheme theme)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                var colors = button.colors;
                colors.normalColor = theme.colors.primary;
                colors.highlightedColor = AdjustColor(theme.colors.primary, 1.2f);
                colors.pressedColor = AdjustColor(theme.colors.primary, 0.8f);
                colors.disabledColor = theme.colors.disabled;
                button.colors = colors;
            }
            
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = theme.colors.text;
            }
        }
        
        private static void ApplyThemeToText(TextMeshProUGUI text, UITheme theme)
        {
            text.color = theme.colors.text;
            
            // Apply typography based on object name/tag
            if (text.name.Contains("Title") || text.name.Contains("H1"))
            {
                text.fontSize = theme.typography.h1.size;
            }
            else if (text.name.Contains("Subtitle") || text.name.Contains("H2"))
            {
                text.fontSize = theme.typography.h2.size;
            }
            else
            {
                text.fontSize = theme.typography.body.size;
            }
        }
        
        private static Color AdjustColor(Color color, float factor)
        {
            return new Color(
                Mathf.Clamp01(color.r * factor),
                Mathf.Clamp01(color.g * factor),
                Mathf.Clamp01(color.b * factor),
                color.a
            );
        }
        
        public static UITheme GetCurrentTheme()
        {
            return currentTheme;
        }
    }
}
```

---

## Example: Complete UI Creation Flow

### Scenario: Create Main Menu

```
User: "Create a main menu for my game with a cool modern design"

AI Executes:
```

```json
// Step 1: Create Canvas
{
  "tool": "unity_ui_create_canvas",
  "args": {
    "name": "MainMenuCanvas",
    "preset": "game_menu"
  }
}

// Step 2: Create Theme
{
  "tool": "unity_ui_create_theme",
  "args": {
    "name": "ModernDarkTheme",
    "style": "dark",
    "primaryColor": "#00BCD4",
    "accentColor": "#FF4081"
  }
}

// Step 3: Create Background
{
  "tool": "unity_ui_create_image",
  "args": {
    "name": "Background",
    "parent": "MainMenuCanvas",
    "preset": "full_screen_background",
    "gradient": {
      "angle": 135,
      "colors": ["#1A237E", "#4A148C"]
    },
    "blur": 0
  }
}

// Step 4: Create Title
{
  "tool": "unity_ui_create_text",
  "args": {
    "name": "GameTitle",
    "parent": "MainMenuCanvas",
    "text": "EPIC ADVENTURE",
    "fontSize": 72,
    "font": "Roboto-Black",
    "position": [0, 250],
    "effects": {
      "outline": true,
      "shadow": true,
      "gradient": {
        "colors": ["#FFFFFF", "#00BCD4"]
      }
    },
    "animation": {
      "type": "fade_scale_in",
      "duration": 0.8,
      "delay": 0
    }
  }
}

// Step 5: Create Button Container
{
  "tool": "unity_ui_create_panel",
  "args": {
    "name": "ButtonPanel",
    "parent": "MainMenuCanvas",
    "size": [300, 400],
    "position": [0, -50],
    "background": "transparent"
  }
}

// Step 6: Add Layout
{
  "tool": "unity_ui_add_layout_group",
  "args": {
    "target": "ButtonPanel",
    "layoutType": "Vertical",
    "spacing": 20,
    "padding": {"all": 20},
    "childControlSize": true
  }
}

// Step 7: Create Buttons
{
  "tool": "unity_ui_create_button",
  "args": {
    "name": "PlayButton",
    "parent": "ButtonPanel",
    "text": "PLAY",
    "icon": "play_arrow",
    "style": "primary",
    "size": [260, 70],
    "onClick": {
      "action": "LoadScene",
      "parameters": {"scene": "Level1"}
    },
    "animation": {
      "hover": "scale_glow",
      "click": "press_bounce"
    },
    "sound": "UI_Button_Click"
  }
}

{
  "tool": "unity_ui_create_button",
  "args": {
    "name": "SettingsButton",
    "parent": "ButtonPanel",
    "text": "SETTINGS",
    "icon": "settings",
    "style": "secondary",
    "size": [260, 70],
    "onClick": {
      "action": "OpenPanel",
      "parameters": {"panel": "SettingsPanel"}
    }
  }
}

{
  "tool": "unity_ui_create_button",
  "args": {
    "name": "QuitButton",
    "parent": "ButtonPanel",
    "text": "QUIT",
    "icon": "exit_to_app",
    "style": "outlined",
    "size": [260, 70],
    "onClick": {
      "action": "QuitGame"
    }
  }
}

// Step 8: Add Particle Effect
{
  "tool": "unity_ui_add_particle_effect",
  "args": {
    "parent": "MainMenuCanvas",
    "preset": "floating_particles",
    "count": 50,
    "color": "#00BCD440"
  }
}

// Step 9: Animate In
{
  "tool": "unity_ui_animate_show",
  "args": {
    "target": "ButtonPanel",
    "animation": "slide_up",
    "duration": 0.6,
    "delay": 0.3,
    "easing": "easeOutBack"
  }
}
```

**Result**: Professional main menu created in < 1 second!

---

## Performance Optimization

### Best Practices for AI-Created UI

1. **Batch Operations**: Create related elements together
2. **Canvas Optimization**: Use multiple canvases for dynamic/static content
3. **Atlas Usage**: Group sprites in atlases
4. **Overdraw Reduction**: Minimize overlapping UI elements
5. **Layout Updates**: Cache layout calculations
6. **Pooling**: Reuse UI elements for lists/grids

### Tool: `unity_ui_optimize`

```json
{
  "target": "MainMenuCanvas",
  "optimizations": [
    "separate_dynamic_static",
    "create_sprite_atlas",
    "reduce_overdraw",
    "batch_materials"
  ]
}
```

---

## Success Metrics

### Quality Metrics

- **Visual Quality**: Professional appearance by default
- **Performance**: 60 FPS on target platforms
- **Accessibility**: WCAG 2.1 AA compliance
- **Responsiveness**: Works on all target resolutions
- **Consistency**: Follows design system

### Speed Metrics

- **Simple UI**: < 1 second (button, text)
- **Medium UI**: < 3 seconds (panel with form)
- **Complex UI**: < 10 seconds (complete menu system)

### User Satisfaction

- **Learning Curve**: Create first UI in < 5 minutes
- **Iteration Speed**: 10x faster than manual creation
- **Quality**: Comparable to hand-crafted UI

---

## Future Enhancements

### Phase 5+: Advanced UI

1. **UI Toolkit Support**: Modern UI system
2. **Custom Shaders**: Advanced visual effects
3. **VR/AR UI**: Spatial interfaces
4. **Procedural Generation**: AI-designed layouts
5. **A/B Testing**: Automated UI optimization
6. **Localization**: Multi-language support
7. **Analytics**: UI interaction tracking

---

## Conclusion

With 60+ specialized UI tools, AI can create professional, responsive, accessible Unity UI through natural language. The system combines:

- **Intelligence**: Understanding design intent
- **Best Practices**: Professional defaults
- **Flexibility**: Full customization available
- **Speed**: Production-quality UI in seconds

### Next Steps

1. Implement Canvas and basic element tools (Week 1-2)
2. Add styling and theming system (Week 3-4)
3. Implement animations and interactions (Week 5-6)
4. Add responsive and accessibility features (Week 7-8)
5. Polish and optimize (Week 9-10)

---

**Document Version**: 1.0  
**Last Updated**: October 10, 2025  
**Focus**: UI System Design for Unity MCP Server  
**Status**: Comprehensive Design Complete

