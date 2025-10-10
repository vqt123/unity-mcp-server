# Unity MCP Server - UI Testing & Validation System

## Closing the Visual Feedback Loop

Complete system for AI agents to test, validate, and verify that generated UI looks good and functions correctly.

---

## Table of Contents

1. [Overview](#overview)
2. [Visual Testing Tools](#visual-testing-tools)
3. [Functional Testing](#functional-testing)
4. [AI Vision Integration](#ai-vision-integration)
5. [Automated Quality Checks](#automated-quality-checks)
6. [Responsive Testing](#responsive-testing)
7. [Accessibility Validation](#accessibility-validation)
8. [Performance Testing](#performance-testing)
9. [Feedback Loop System](#feedback-loop-system)
10. [Implementation Details](#implementation-details)

---

## Overview

### The Problem

When AI creates UI, it needs to **see and verify** the results:

```
AI Creates UI → ??? → Hope it looks good?
```

### The Solution

**Visual Testing & Validation Loop**:

```
AI Creates UI 
  ↓
Capture Screenshot
  ↓
Analyze with AI Vision
  ↓
Run Quality Checks
  ↓
Test Functionality
  ↓
Generate Report
  ↓
AI Reviews & Iterates
```

### Goals

1. **Visual Verification**: AI can "see" what it created
2. **Quality Assessment**: Automated scoring of UI quality
3. **Functional Validation**: Ensure interactivity works
4. **Iterative Improvement**: AI fixes issues automatically
5. **Confidence**: Know the UI meets standards

---

## Visual Testing Tools

### Tool Category: Screenshot & Capture (10 tools)

#### `unity_ui_capture_screenshot`
Capture high-quality screenshot of UI.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "resolution": [1920, 1080],
  "format": "PNG",
  "includeGameView": true,
  "transparentBackground": false,
  "antialiasing": 4,
  "captureArea": "canvas",  // canvas, element, fullscreen, custom
  "outputPath": "temp/screenshots/main_menu.png"
}
```

**Returns**:
```json
{
  "success": true,
  "imagePath": "temp/screenshots/main_menu.png",
  "imageData": "base64_encoded_png...",
  "width": 1920,
  "height": 1080,
  "fileSize": 245678,
  "timestamp": "2025-10-10T12:34:56Z"
}
```

**Use Case**:
```
AI: Create button → Capture screenshot → Verify it looks right
```

---

#### `unity_ui_capture_element`
Capture screenshot of specific UI element.

**Parameters**:
```json
{
  "target": "PlayButton",
  "padding": 20,  // Extra space around element
  "includeChildren": true,
  "resolution": "actual",  // or specific [width, height]
  "format": "PNG"
}
```

**Returns**: Cropped image of just that element

---

#### `unity_ui_capture_states`
Capture all states of an interactive element.

**Parameters**:
```json
{
  "target": "PlayButton",
  "states": ["normal", "hover", "pressed", "disabled"],
  "arrangement": "horizontal",  // or vertical, grid
  "spacing": 20
}
```

**Returns**:
```json
{
  "success": true,
  "compositePath": "temp/screenshots/button_states.png",
  "individualImages": {
    "normal": "temp/screenshots/button_normal.png",
    "hover": "temp/screenshots/button_hover.png",
    "pressed": "temp/screenshots/button_pressed.png",
    "disabled": "temp/screenshots/button_disabled.png"
  }
}
```

---

#### `unity_ui_capture_responsive`
Capture UI at multiple resolutions.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "resolutions": [
    {"name": "mobile_portrait", "width": 1080, "height": 1920},
    {"name": "mobile_landscape", "width": 1920, "height": 1080},
    {"name": "tablet", "width": 1536, "height": 2048},
    {"name": "desktop", "width": 1920, "height": 1080},
    {"name": "ultrawide", "width": 3440, "height": 1440}
  ],
  "arrangement": "grid"
}
```

**Returns**: Composite image showing all resolutions

---

#### `unity_ui_capture_animation`
Capture animated sequence as frames or video.

**Parameters**:
```json
{
  "target": "PlayButton",
  "animation": "hover_effect",
  "format": "gif",  // gif, mp4, frames
  "duration": 2.0,
  "frameRate": 30,
  "loop": true
}
```

**Returns**:
```json
{
  "success": true,
  "animationPath": "temp/animations/button_hover.gif",
  "frames": 60,
  "duration": 2.0
}
```

---

#### `unity_ui_record_interaction`
Record user interaction sequence.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "interactions": [
    {"action": "click", "target": "PlayButton"},
    {"action": "wait", "duration": 0.5},
    {"action": "click", "target": "SettingsButton"},
    {"action": "wait", "duration": 0.3}
  ],
  "recordFormat": "video",
  "includeMouseCursor": true,
  "highlightClicks": true
}
```

---

#### `unity_ui_compare_screenshots`
Compare two screenshots for differences.

**Parameters**:
```json
{
  "imageA": "temp/screenshots/before.png",
  "imageB": "temp/screenshots/after.png",
  "sensitivity": 0.1,
  "highlightDifferences": true,
  "generateDiffMap": true
}
```

**Returns**:
```json
{
  "success": true,
  "identical": false,
  "differencePercentage": 2.3,
  "diffMapPath": "temp/comparisons/diff_map.png",
  "regions": [
    {
      "x": 100,
      "y": 200,
      "width": 50,
      "height": 30,
      "differenceScore": 0.45
    }
  ]
}
```

---

#### `unity_ui_create_contact_sheet`
Create contact sheet of multiple UI screens.

**Parameters**:
```json
{
  "screens": [
    "MainMenu",
    "Settings",
    "LevelSelect",
    "Pause"
  ],
  "columns": 2,
  "spacing": 20,
  "labels": true,
  "resolution": [800, 600]
}
```

**Returns**: Single image with all screens laid out

---

### Tool Category: Visual Analysis (8 tools)

#### `unity_ui_analyze_screenshot`
AI-powered visual analysis of screenshot.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "analysisTypes": [
    "composition",
    "color_harmony",
    "visual_hierarchy",
    "alignment",
    "spacing",
    "readability",
    "style_consistency"
  ],
  "compareToReference": "references/good_menu.png"
}
```

**Returns**:
```json
{
  "success": true,
  "overallScore": 85,
  "analysis": {
    "composition": {
      "score": 90,
      "feedback": "Well-balanced layout with clear focal point",
      "issues": []
    },
    "colorHarmony": {
      "score": 88,
      "feedback": "Good color palette, complementary colors used well",
      "dominantColors": ["#2196F3", "#FFFFFF", "#1E1E1E"],
      "issues": ["Slight contrast issue in subtitle text"]
    },
    "visualHierarchy": {
      "score": 92,
      "feedback": "Clear hierarchy - title dominates, buttons are secondary",
      "issues": []
    },
    "alignment": {
      "score": 85,
      "feedback": "Most elements aligned, minor issues detected",
      "issues": [
        "Button at (520, 300) is 2px off-center"
      ]
    },
    "spacing": {
      "score": 80,
      "feedback": "Generally consistent, some irregularities",
      "issues": [
        "Inconsistent button spacing: 20px vs 18px"
      ]
    },
    "readability": {
      "score": 75,
      "feedback": "Text is readable but could be improved",
      "issues": [
        "Subtitle text contrast ratio 3.8:1 (min 4.5:1)"
      ]
    }
  },
  "suggestions": [
    "Increase subtitle text contrast",
    "Center the 'Settings' button precisely",
    "Make button spacing uniform at 20px"
  ]
}
```

---

#### `unity_ui_detect_elements`
Use computer vision to detect UI elements.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "detectTypes": ["buttons", "text", "images", "panels"]
}
```

**Returns**:
```json
{
  "success": true,
  "elements": [
    {
      "type": "button",
      "bounds": {"x": 500, "y": 300, "width": 200, "height": 60},
      "text": "PLAY",
      "confidence": 0.95
    },
    {
      "type": "text",
      "bounds": {"x": 450, "y": 100, "width": 300, "height": 80},
      "text": "EPIC ADVENTURE",
      "fontSize": 72,
      "confidence": 0.98
    }
  ]
}
```

---

#### `unity_ui_measure_spacing`
Measure spacing between UI elements.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "elements": ["PlayButton", "SettingsButton", "QuitButton"]
}
```

**Returns**:
```json
{
  "success": true,
  "measurements": [
    {
      "from": "PlayButton",
      "to": "SettingsButton",
      "distance": 20,
      "axis": "vertical"
    },
    {
      "from": "SettingsButton",
      "to": "QuitButton",
      "distance": 18,
      "axis": "vertical",
      "warning": "Inconsistent spacing (expected 20px)"
    }
  ],
  "gridAlignment": {
    "conformsTo8pxGrid": false,
    "deviations": [
      {"element": "QuitButton", "offset": 2}
    ]
  }
}
```

---

#### `unity_ui_check_alignment`
Check if elements are properly aligned.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "checkTypes": ["horizontal", "vertical", "center"],
  "tolerance": 2  // pixels
}
```

**Returns**:
```json
{
  "success": true,
  "alignmentScore": 85,
  "issues": [
    {
      "severity": "warning",
      "element": "SettingsButton",
      "issue": "2px off horizontal center",
      "expected": {"x": 960},
      "actual": {"x": 962}
    }
  ]
}
```

---

#### `unity_ui_analyze_color_contrast`
Check color contrast ratios for accessibility.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "standard": "WCAG_AA",  // WCAG_AA or WCAG_AAA
  "checkText": true
}
```

**Returns**:
```json
{
  "success": true,
  "overallCompliance": false,
  "checks": [
    {
      "element": "PlayButton",
      "textColor": "#FFFFFF",
      "backgroundColor": "#2196F3",
      "contrastRatio": 4.8,
      "passes": true,
      "standard": "WCAG_AA",
      "minimumRatio": 4.5
    },
    {
      "element": "SubtitleText",
      "textColor": "#B0B0B0",
      "backgroundColor": "#1E1E1E",
      "contrastRatio": 3.8,
      "passes": false,
      "standard": "WCAG_AA",
      "minimumRatio": 4.5,
      "suggestion": "Increase text lightness to #C5C5C5"
    }
  ]
}
```

---

#### `unity_ui_detect_visual_bugs`
Use AI vision to detect common UI bugs.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "checkFor": [
    "text_overflow",
    "image_distortion",
    "overlapping_elements",
    "cut_off_content",
    "missing_images",
    "broken_layouts"
  ]
}
```

**Returns**:
```json
{
  "success": true,
  "bugsFound": 2,
  "bugs": [
    {
      "type": "text_overflow",
      "severity": "high",
      "element": "DescriptionText",
      "bounds": {"x": 100, "y": 400, "width": 200, "height": 50},
      "description": "Text extends beyond container bounds",
      "suggestion": "Enable word wrapping or increase container height"
    },
    {
      "type": "overlapping_elements",
      "severity": "medium",
      "elements": ["Button1", "Button2"],
      "description": "Elements overlap by 5px",
      "suggestion": "Increase spacing between buttons"
    }
  ]
}
```

---

#### `unity_ui_similarity_check`
Check if UI matches a reference design.

**Parameters**:
```json
{
  "actualImage": "temp/screenshots/main_menu.png",
  "referenceImage": "designs/main_menu_mockup.png",
  "tolerance": 5,  // percentage
  "ignoreColors": false,
  "checkLayout": true,
  "checkSizes": true
}
```

**Returns**:
```json
{
  "success": true,
  "similarityScore": 88,
  "differences": [
    {
      "type": "position",
      "element": "PlayButton",
      "expected": {"x": 960, "y": 540},
      "actual": {"x": 960, "y": 548},
      "deviation": 8
    },
    {
      "type": "size",
      "element": "TitleText",
      "expected": {"width": 400, "height": 80},
      "actual": {"width": 420, "height": 85},
      "deviation": 6.5
    }
  ]
}
```

---

#### `unity_ui_generate_heatmap`
Generate attention heatmap for UI.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "algorithm": "visual_saliency",
  "overlayOnOriginal": true
}
```

**Returns**: Heatmap showing where users will naturally look

---

## Functional Testing

### Tool Category: Interaction Testing (12 tools)

#### `unity_ui_test_button_click`
Test if button responds to clicks.

**Parameters**:
```json
{
  "target": "PlayButton",
  "expectAction": "LoadScene",
  "expectParameters": {"sceneName": "Level1"},
  "timeout": 2.0
}
```

**Returns**:
```json
{
  "success": true,
  "clickSuccessful": true,
  "actionTriggered": true,
  "actionName": "LoadScene",
  "actionParameters": {"sceneName": "Level1"},
  "responseTime": 0.15,
  "visualFeedback": true,
  "audioFeedback": true
}
```

---

#### `unity_ui_test_hover_effects`
Test hover state transitions.

**Parameters**:
```json
{
  "target": "PlayButton",
  "captureStates": true,
  "checkTransitionSpeed": true
}
```

**Returns**:
```json
{
  "success": true,
  "hoverDetected": true,
  "transitionTime": 0.2,
  "visualChanges": {
    "scale": {"from": 1.0, "to": 1.05},
    "color": {"from": "#2196F3", "to": "#42A5F5"}
  },
  "cursorChange": "pointer",
  "audioPlayed": "UI_Hover.wav"
}
```

---

#### `unity_ui_test_input_field`
Test input field functionality.

**Parameters**:
```json
{
  "target": "PlayerNameInput",
  "testInputs": [
    "ValidName123",
    "Invalid@#$",
    "TooLongNameThatExceedsLimit"
  ],
  "checkValidation": true,
  "checkCharacterLimit": true
}
```

**Returns**:
```json
{
  "success": true,
  "tests": [
    {
      "input": "ValidName123",
      "accepted": true,
      "outputValue": "ValidName123",
      "validationPassed": true
    },
    {
      "input": "Invalid@#$",
      "accepted": false,
      "validationPassed": false,
      "errorMessage": "Only alphanumeric characters allowed"
    },
    {
      "input": "TooLongNameThatExceedsLimit",
      "accepted": false,
      "truncated": true,
      "outputValue": "TooLongNameThatExce",
      "characterLimit": 20
    }
  ]
}
```

---

#### `unity_ui_test_slider`
Test slider control.

**Parameters**:
```json
{
  "target": "VolumeSlider",
  "testValues": [0, 25, 50, 75, 100],
  "checkCallback": true,
  "checkVisualUpdate": true
}
```

---

#### `unity_ui_test_toggle`
Test toggle/checkbox.

**Parameters**:
```json
{
  "target": "MusicToggle",
  "testSequence": ["on", "off", "on"],
  "checkCallback": true
}
```

---

#### `unity_ui_test_dropdown`
Test dropdown menu.

**Parameters**:
```json
{
  "target": "QualityDropdown",
  "testOptions": [0, 1, 2],
  "checkDisplay": true,
  "checkCallback": true
}
```

---

#### `unity_ui_test_scroll_view`
Test scrolling functionality.

**Parameters**:
```json
{
  "target": "InventoryScroll",
  "testScrolling": true,
  "scrollDistance": 500,
  "checkInertia": true,
  "checkBounds": true
}
```

---

#### `unity_ui_test_drag_drop`
Test drag and drop.

**Parameters**:
```json
{
  "source": "InventoryItem1",
  "target": "EquipmentSlot",
  "expectSuccess": true,
  "captureSequence": true
}
```

---

#### `unity_ui_test_navigation`
Test keyboard/gamepad navigation.

**Parameters**:
```json
{
  "startElement": "PlayButton",
  "navigationSequence": ["down", "down", "up", "select"],
  "expectedPath": ["PlayButton", "SettingsButton", "QuitButton", "SettingsButton"],
  "checkVisualFeedback": true
}
```

---

#### `unity_ui_test_animation`
Test UI animations.

**Parameters**:
```json
{
  "target": "SettingsPanel",
  "animation": "show",
  "checkTiming": true,
  "checkEasing": true,
  "captureAnimation": true
}
```

**Returns**:
```json
{
  "success": true,
  "animationPlayed": true,
  "expectedDuration": 0.3,
  "actualDuration": 0.295,
  "easingFunction": "easeOutQuart",
  "animationPath": "temp/animations/panel_show.gif"
}
```

---

#### `unity_ui_stress_test_clicks`
Rapid click testing.

**Parameters**:
```json
{
  "target": "PlayButton",
  "clickCount": 100,
  "clicksPerSecond": 50,
  "checkForErrors": true
}
```

**Returns**:
```json
{
  "success": true,
  "totalClicks": 100,
  "successfulClicks": 98,
  "failedClicks": 2,
  "averageResponseTime": 0.12,
  "errorsDetected": 0,
  "memoryLeaks": false
}
```

---

#### `unity_ui_test_modal_flow`
Test modal dialog interactions.

**Parameters**:
```json
{
  "openModal": "ConfirmDialog",
  "testButtons": ["Cancel", "Confirm"],
  "checkBackdrop": true,
  "checkEscapeClose": true
}
```

---

## AI Vision Integration

### Using Claude/GPT Vision APIs

#### `unity_ui_ask_vision_ai`
Send screenshot to AI vision model for assessment.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png",
  "prompt": "Rate this game menu UI from 1-10 and explain what works and what doesn't",
  "model": "claude-3-opus",  // or gpt-4-vision
  "includeElements": true,
  "contextDescription": "This is a main menu for an action RPG game targeting young adults"
}
```

**Returns**:
```json
{
  "success": true,
  "rating": 8,
  "analysis": {
    "strengths": [
      "Clean, modern design with good use of space",
      "Clear visual hierarchy - title dominates, buttons are secondary",
      "Color scheme is cohesive and appropriate for genre",
      "Button states are clear and distinct"
    ],
    "weaknesses": [
      "Subtitle text is slightly hard to read due to low contrast",
      "Button spacing is inconsistent (20px vs 18px)",
      "Background could use more visual interest",
      "No visual indication of which button is currently selected"
    ],
    "suggestions": [
      "Increase subtitle text contrast to meet WCAG AA standards",
      "Make button spacing uniform",
      "Add subtle particle effects or animated background",
      "Add selection indicator for keyboard/gamepad navigation"
    ]
  },
  "professionalComparison": "Comparable to mid-tier indie games, slightly below AAA quality",
  "targetAudienceMatch": 85
}
```

---

#### `unity_ui_compare_with_reference`
Compare created UI to reference design using AI.

**Parameters**:
```json
{
  "actualImage": "temp/screenshots/main_menu.png",
  "referenceImage": "designs/menu_mockup.png",
  "prompt": "Compare these two designs. Did we successfully implement the mockup?",
  "checkFor": ["layout", "colors", "typography", "spacing", "overall_feel"]
}
```

**Returns**:
```json
{
  "success": true,
  "matchScore": 88,
  "analysis": {
    "layout": {
      "match": 95,
      "notes": "Layout is nearly identical, button positions match well"
    },
    "colors": {
      "match": 90,
      "notes": "Colors are very close, primary blue is slightly darker than mockup"
    },
    "typography": {
      "match": 85,
      "notes": "Font sizes match, but font weight seems lighter than intended"
    },
    "spacing": {
      "match": 80,
      "notes": "Minor spacing inconsistencies between buttons"
    },
    "overall_feel": {
      "match": 88,
      "notes": "Captures the modern, clean aesthetic well. Needs more polish on details."
    }
  },
  "missingElements": [],
  "additionalElements": ["Particle effect in background"],
  "recommendation": "Very good implementation, minor tweaks needed"
}
```

---

#### `unity_ui_style_classification`
Classify the style/aesthetic of UI.

**Parameters**:
```json
{
  "imagePath": "temp/screenshots/main_menu.png"
}
```

**Returns**:
```json
{
  "success": true,
  "styles": [
    {"style": "Modern", "confidence": 0.92},
    {"style": "Minimalist", "confidence": 0.85},
    {"style": "Material Design", "confidence": 0.78},
    {"style": "Flat Design", "confidence": 0.72}
  ],
  "colorPalette": "Dark with blue accent",
  "era": "Contemporary (2020s)",
  "targetPlatform": "Mobile/PC",
  "genre": "Action/RPG",
  "professionalLevel": "Mid to High"
}
```

---

## Automated Quality Checks

### Tool Category: Quality Metrics (10 tools)

#### `unity_ui_quality_score`
Comprehensive quality scoring system.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "metrics": [
    "visual_appeal",
    "usability",
    "accessibility",
    "performance",
    "consistency",
    "responsiveness"
  ],
  "captureScreenshot": true,
  "runFullAnalysis": true
}
```

**Returns**:
```json
{
  "success": true,
  "overallScore": 82,
  "grade": "B",
  "breakdown": {
    "visualAppeal": {
      "score": 85,
      "factors": {
        "colorHarmony": 88,
        "typography": 82,
        "spacing": 80,
        "alignment": 85,
        "visualHierarchy": 90
      }
    },
    "usability": {
      "score": 88,
      "factors": {
        "clarity": 90,
        "affordance": 85,
        "feedback": 90,
        "consistency": 86
      }
    },
    "accessibility": {
      "score": 75,
      "factors": {
        "colorContrast": 72,
        "textSize": 85,
        "touchTargets": 90,
        "keyboardNav": 60
      },
      "issues": [
        "Subtitle text contrast below WCAG AA",
        "No keyboard navigation setup"
      ]
    },
    "performance": {
      "score": 90,
      "factors": {
        "drawCalls": 95,
        "overdraw": 88,
        "batchingEfficiency": 90
      }
    },
    "consistency": {
      "score": 78,
      "issues": [
        "Inconsistent button spacing",
        "Font weights vary unnecessarily"
      ]
    },
    "responsiveness": {
      "score": 70,
      "testedResolutions": 5,
      "issuesFound": 2,
      "issues": [
        "Layout breaks at 1024x768",
        "Text truncates on mobile portrait"
      ]
    }
  },
  "topIssues": [
    {
      "severity": "high",
      "category": "accessibility",
      "issue": "Subtitle text contrast 3.8:1 (need 4.5:1)",
      "fix": "Change text color from #B0B0B0 to #C5C5C5"
    },
    {
      "severity": "medium",
      "category": "consistency",
      "issue": "Button spacing inconsistent (18px, 20px)",
      "fix": "Set uniform spacing to 20px"
    },
    {
      "severity": "medium",
      "category": "responsiveness",
      "issue": "Layout breaks at tablet resolution",
      "fix": "Add responsive breakpoint for 1024x768"
    }
  ],
  "recommendedActions": [
    "Fix color contrast issue",
    "Standardize button spacing",
    "Add keyboard navigation",
    "Test and fix tablet layout"
  ],
  "estimatedFixTime": "30 minutes"
}
```

---

#### `unity_ui_benchmark_against_examples`
Compare against high-quality reference UIs.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "compareAgainst": [
    "examples/aaa_game_menu.png",
    "examples/top_mobile_ui.png",
    "examples/award_winning_ui.png"
  ],
  "metrics": ["layout", "polish", "visual_design"]
}
```

**Returns**:
```json
{
  "success": true,
  "overallMatch": 75,
  "comparisons": [
    {
      "reference": "examples/aaa_game_menu.png",
      "matchScore": 78,
      "analysis": "Good foundation but lacks polish of AAA title",
      "gaps": [
        "Missing subtle animations",
        "Less sophisticated lighting/shadows",
        "Simpler visual effects"
      ]
    }
  ],
  "rating": "Mid-tier quality, professional but not exceptional"
}
```

---

#### `unity_ui_consistency_check`
Check consistency across multiple screens.

**Parameters**:
```json
{
  "screens": [
    "MainMenuCanvas",
    "SettingsCanvas",
    "PauseCanvas",
    "GameOverCanvas"
  ],
  "checkFor": [
    "color_usage",
    "typography",
    "button_styles",
    "spacing",
    "animations"
  ]
}
```

**Returns**:
```json
{
  "success": true,
  "consistencyScore": 82,
  "issues": [
    {
      "type": "color_inconsistency",
      "screens": ["MainMenuCanvas", "SettingsCanvas"],
      "issue": "Primary button color differs (#2196F3 vs #1976D2)"
    },
    {
      "type": "spacing_inconsistency",
      "screens": ["All"],
      "issue": "Button spacing varies: 15px, 18px, 20px"
    }
  ],
  "recommendations": [
    "Use theme system to ensure color consistency",
    "Standardize spacing values"
  ]
}
```

---

#### `unity_ui_performance_profile`
Profile UI performance.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "testDuration": 10,
  "simulateInteractions": true
}
```

**Returns**:
```json
{
  "success": true,
  "averageFPS": 58,
  "minFPS": 52,
  "maxFPS": 60,
  "drawCalls": 15,
  "batches": 8,
  "triangles": 2400,
  "overdrawAreas": [
    {
      "location": "Background",
      "severity": "low",
      "pixelsOverdrawn": 150000
    }
  ],
  "recommendations": [
    "Reduce background layers to decrease overdraw",
    "Combine button sprites into atlas"
  ],
  "grade": "A"
}
```

---

## Responsive Testing

### Tool Category: Multi-Resolution Testing (6 tools)

#### `unity_ui_test_all_resolutions`
Test UI across all target resolutions.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "resolutions": [
    {"name": "iPhone_SE", "width": 750, "height": 1334},
    {"name": "iPhone_14_Pro", "width": 1179, "height": 2556},
    {"name": "iPad_Pro", "width": 2048, "height": 2732},
    {"name": "Desktop_HD", "width": 1920, "height": 1080},
    {"name": "Desktop_4K", "width": 3840, "height": 2160}
  ],
  "checkFor": [
    "layout_integrity",
    "text_readability",
    "button_sizes",
    "element_visibility",
    "overflow_issues"
  ],
  "captureScreenshots": true
}
```

**Returns**:
```json
{
  "success": true,
  "testedResolutions": 5,
  "passedTests": 3,
  "failedTests": 2,
  "results": [
    {
      "resolution": "iPhone_SE",
      "passed": false,
      "issues": [
        {
          "severity": "high",
          "type": "text_truncation",
          "element": "SubtitleText",
          "description": "Text cut off at narrow width"
        },
        {
          "severity": "medium",
          "type": "button_too_small",
          "element": "CloseButton",
          "size": "38x38",
          "minimumSize": "44x44"
        }
      ],
      "screenshot": "temp/responsive/iphone_se.png"
    },
    {
      "resolution": "Desktop_HD",
      "passed": true,
      "issues": [],
      "screenshot": "temp/responsive/desktop_hd.png"
    }
  ],
  "recommendations": [
    "Add responsive text sizing for small screens",
    "Increase minimum button size to 44x44",
    "Consider portrait-specific layout for mobile"
  ]
}
```

---

#### `unity_ui_test_orientation_changes`
Test landscape/portrait switching.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "testTransitions": true,
  "captureTransition": true
}
```

---

#### `unity_ui_test_safe_areas`
Test handling of notches and safe areas.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "devices": ["iPhone_X", "iPhone_14_Pro", "Pixel_6"],
  "checkElementsInSafeArea": true
}
```

---

## Feedback Loop System

### Iterative Improvement Process

#### Complete Testing & Fix Loop

```javascript
// Pseudocode for AI agent
async function createAndValidateUI(requirements) {
  let attempts = 0;
  let maxAttempts = 5;
  
  while (attempts < maxAttempts) {
    // 1. Create UI
    await createUI(requirements);
    
    // 2. Capture screenshot
    const screenshot = await unity_ui_capture_screenshot({
      target: "MainMenuCanvas"
    });
    
    // 3. Run quality checks
    const qualityScore = await unity_ui_quality_score({
      target: "MainMenuCanvas",
      runFullAnalysis: true
    });
    
    // 4. AI vision analysis
    const visionAnalysis = await unity_ui_ask_vision_ai({
      imagePath: screenshot.imagePath,
      prompt: "Rate this UI and identify issues"
    });
    
    // 5. Check if meets standards
    if (qualityScore.overallScore >= 85 && 
        visionAnalysis.rating >= 8) {
      return {
        success: true,
        screenshot: screenshot,
        score: qualityScore,
        analysis: visionAnalysis
      };
    }
    
    // 6. Identify top issues
    const issues = [
      ...qualityScore.topIssues,
      ...visionAnalysis.analysis.weaknesses
    ];
    
    // 7. Fix issues
    for (const issue of issues) {
      await fixIssue(issue);
    }
    
    attempts++;
  }
  
  return {
    success: false,
    message: "Could not meet quality standards after 5 attempts"
  };
}
```

---

#### `unity_ui_auto_fix_issues`
Automatically fix detected issues.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "issues": [
    {
      "type": "contrast",
      "element": "SubtitleText",
      "fix": "increase_contrast"
    },
    {
      "type": "spacing",
      "elements": ["PlayButton", "SettingsButton"],
      "fix": "standardize_spacing",
      "value": 20
    },
    {
      "type": "alignment",
      "element": "SettingsButton",
      "fix": "center_align"
    }
  ],
  "verifyFixes": true,
  "captureAfter": true
}
```

**Returns**:
```json
{
  "success": true,
  "fixesApplied": 3,
  "results": [
    {
      "issue": "contrast",
      "fixed": true,
      "before": "#B0B0B0",
      "after": "#C5C5C5",
      "contrastRatioBefore": 3.8,
      "contrastRatioAfter": 4.6
    }
  ],
  "newQualityScore": 88,
  "improvement": 6,
  "screenshotAfter": "temp/screenshots/after_fixes.png"
}
```

---

#### `unity_ui_run_test_suite`
Run complete test suite.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "tests": [
    "visual_quality",
    "functional",
    "responsive",
    "accessibility",
    "performance"
  ],
  "generateReport": true,
  "autoFixIssues": false
}
```

**Returns**:
```json
{
  "success": true,
  "overallScore": 82,
  "testResults": {
    "visual_quality": {
      "passed": true,
      "score": 85,
      "time": 2.3
    },
    "functional": {
      "passed": true,
      "score": 90,
      "time": 5.1
    },
    "responsive": {
      "passed": false,
      "score": 70,
      "issuesFound": 2,
      "time": 8.5
    },
    "accessibility": {
      "passed": false,
      "score": 75,
      "issuesFound": 3,
      "time": 3.2
    },
    "performance": {
      "passed": true,
      "score": 90,
      "time": 10.0
    }
  },
  "totalTime": 29.1,
  "reportPath": "temp/reports/ui_test_report.html",
  "recommendNextSteps": [
    "Fix responsive layout for mobile",
    "Address accessibility contrast issues",
    "Add keyboard navigation"
  ]
}
```

---

## Implementation Details

### Unity C# Screenshot System

```csharp
// Assets/Editor/MCPServer/Commands/UITestingCommands.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEditor;

namespace UnityMCP.Commands.UITesting
{
    public class CaptureScreenshotCommand : ICommand
    {
        public object Execute(JObject args)
        {
            string target = args["target"]?.ToString();
            int width = args["resolution"]?[0]?.ToObject<int>() ?? 1920;
            int height = args["resolution"]?[1]?.ToObject<int>() ?? 1080;
            string format = args["format"]?.ToString() ?? "PNG";
            string outputPath = args["outputPath"]?.ToString() ?? 
                $"temp/screenshots/{target}_{System.DateTime.Now.Ticks}.png";
            
            // Find canvas
            Canvas canvas = GameObject.Find(target)?.GetComponent<Canvas>();
            if (canvas == null)
            {
                throw new System.Exception($"Canvas '{target}' not found");
            }
            
            // Create render texture
            RenderTexture rt = new RenderTexture(width, height, 24);
            rt.antiAliasing = args["antialiasing"]?.ToObject<int>() ?? 4;
            
            // Find or create camera
            Camera camera = FindOrCreateUICamera(canvas);
            RenderTexture previousRT = camera.targetTexture;
            camera.targetTexture = rt;
            
            // Render
            camera.Render();
            
            // Read pixels
            RenderTexture.active = rt;
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            
            // Restore
            camera.targetTexture = previousRT;
            RenderTexture.active = null;
            
            // Save to file
            byte[] bytes = screenshot.EncodeToPNG();
            string fullPath = Path.Combine(Application.dataPath, "..", outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, bytes);
            
            // Clean up
            Object.DestroyImmediate(screenshot);
            rt.Release();
            
            // Encode to base64 for direct transmission
            string base64 = System.Convert.ToBase64String(bytes);
            
            return new
            {
                success = true,
                imagePath = outputPath,
                imageData = base64,
                width = width,
                height = height,
                fileSize = bytes.Length,
                timestamp = System.DateTime.UtcNow.ToString("o")
            };
        }
        
        private Camera FindOrCreateUICamera(Canvas canvas)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                return canvas.worldCamera;
            }
            
            // Create temporary camera for rendering
            GameObject cameraObj = new GameObject("TempUICamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.orthographic = true;
            camera.orthographicSize = Screen.height / 2f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.cullingMask = 1 << canvas.gameObject.layer;
            
            return camera;
        }
    }
    
    public class AnalyzeScreenshotCommand : ICommand
    {
        public object Execute(JObject args)
        {
            string imagePath = args["imagePath"]?.ToString();
            var analysisTypes = args["analysisTypes"]?.ToObject<string[]>() ?? 
                new string[] { "composition", "color_harmony", "alignment" };
            
            // Load image
            string fullPath = Path.Combine(Application.dataPath, "..", imagePath);
            if (!File.Exists(fullPath))
            {
                throw new System.Exception($"Image not found: {imagePath}");
            }
            
            byte[] imageData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            
            // Run analyses
            var results = new Dictionary<string, object>();
            int overallScore = 0;
            
            foreach (var analysisType in analysisTypes)
            {
                switch (analysisType)
                {
                    case "composition":
                        results["composition"] = AnalyzeComposition(texture);
                        overallScore += (int)((Dictionary<string, object>)results["composition"])["score"];
                        break;
                    case "color_harmony":
                        results["colorHarmony"] = AnalyzeColorHarmony(texture);
                        overallScore += (int)((Dictionary<string, object>)results["colorHarmony"])["score"];
                        break;
                    case "alignment":
                        results["alignment"] = AnalyzeAlignment(texture);
                        overallScore += (int)((Dictionary<string, object>)results["alignment"])["score"];
                        break;
                    case "spacing":
                        results["spacing"] = AnalyzeSpacing(texture);
                        overallScore += (int)((Dictionary<string, object>)results["spacing"])["score"];
                        break;
                }
            }
            
            overallScore /= analysisTypes.Length;
            
            // Clean up
            Object.DestroyImmediate(texture);
            
            return new
            {
                success = true,
                overallScore = overallScore,
                analysis = results,
                suggestions = GenerateSuggestions(results)
            };
        }
        
        private Dictionary<string, object> AnalyzeComposition(Texture2D texture)
        {
            // Analyze visual weight distribution
            var heatmap = GenerateVisualWeightHeatmap(texture);
            bool isBalanced = CheckBalance(heatmap);
            bool hasFocalPoint = DetectFocalPoint(heatmap);
            
            int score = 90;
            var issues = new List<string>();
            
            if (!isBalanced)
            {
                score -= 10;
                issues.Add("Visual weight is unbalanced");
            }
            
            if (!hasFocalPoint)
            {
                score -= 15;
                issues.Add("No clear focal point detected");
            }
            
            return new Dictionary<string, object>
            {
                ["score"] = score,
                ["feedback"] = score > 80 ? 
                    "Well-balanced composition" : 
                    "Composition needs improvement",
                ["issues"] = issues
            };
        }
        
        private Dictionary<string, object> AnalyzeColorHarmony(Texture2D texture)
        {
            // Extract dominant colors
            var dominantColors = ExtractDominantColors(texture, 5);
            
            // Check harmony
            float harmonyScore = CalculateColorHarmony(dominantColors);
            
            // Check contrast
            var contrastIssues = new List<string>();
            CheckAllTextContrast(texture, contrastIssues);
            
            int score = (int)(harmonyScore * 100);
            score -= contrastIssues.Count * 5;
            
            return new Dictionary<string, object>
            {
                ["score"] = Mathf.Max(0, score),
                ["feedback"] = score > 80 ? 
                    "Good color harmony" : 
                    "Color harmony needs work",
                ["dominantColors"] = dominantColors,
                ["issues"] = contrastIssues
            };
        }
        
        private Dictionary<string, object> AnalyzeAlignment(Texture2D texture)
        {
            // Detect UI elements
            var elements = DetectUIElements(texture);
            
            // Check alignment
            var alignmentIssues = new List<object>();
            int misalignedCount = 0;
            
            // Check horizontal alignment
            for (int i = 0; i < elements.Count - 1; i++)
            {
                for (int j = i + 1; j < elements.Count; j++)
                {
                    if (AreAlmostAligned(elements[i], elements[j], out float offset))
                    {
                        if (offset > 2) // More than 2px off
                        {
                            misalignedCount++;
                            alignmentIssues.Add(new
                            {
                                element1 = i,
                                element2 = j,
                                offset = offset
                            });
                        }
                    }
                }
            }
            
            int score = 100 - (misalignedCount * 5);
            
            return new Dictionary<string, object>
            {
                ["score"] = Mathf.Max(60, score),
                ["feedback"] = score > 85 ? 
                    "Elements are well-aligned" : 
                    "Some alignment issues detected",
                ["issues"] = alignmentIssues
            };
        }
        
        // Helper methods (simplified)
        private float[,] GenerateVisualWeightHeatmap(Texture2D texture)
        {
            int width = texture.width / 10;
            int height = texture.height / 10;
            float[,] heatmap = new float[width, height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float weight = 0;
                    for (int py = 0; py < 10; py++)
                    {
                        for (int px = 0; px < 10; px++)
                        {
                            Color pixel = texture.GetPixel(x * 10 + px, y * 10 + py);
                            weight += pixel.grayscale;
                        }
                    }
                    heatmap[x, y] = weight / 100f;
                }
            }
            
            return heatmap;
        }
        
        private List<Color> ExtractDominantColors(Texture2D texture, int count)
        {
            // K-means clustering for dominant colors
            var colors = new List<Color>();
            int sampleSize = texture.width * texture.height;
            int step = Mathf.Max(1, sampleSize / 1000);
            
            for (int i = 0; i < sampleSize; i += step)
            {
                int x = i % texture.width;
                int y = i / texture.width;
                colors.Add(texture.GetPixel(x, y));
            }
            
            // Simplified: just return most common colors
            return colors.Take(count).ToList();
        }
        
        private List<Rect> DetectUIElements(Texture2D texture)
        {
            // Edge detection and connected components
            // Simplified version
            return new List<Rect>();
        }
        
        private bool AreAlmostAligned(Rect a, Rect b, out float offset)
        {
            float centerA = a.x + a.width / 2;
            float centerB = b.x + b.width / 2;
            offset = Mathf.Abs(centerA - centerB);
            return offset < 20; // Within 20px
        }
    }
}
```

---

### AI Vision Integration (Python)

```python
# mcp_server/ui_testing.py
import base64
import anthropic
from typing import Dict, List

class UIVisionTester:
    def __init__(self, api_key: str):
        self.client = anthropic.Anthropic(api_key=api_key)
    
    async def analyze_ui_screenshot(
        self,
        image_path: str,
        prompt: str = None,
        context: str = None
    ) -> Dict:
        """
        Use Claude Vision to analyze UI screenshot
        """
        # Read and encode image
        with open(image_path, 'rb') as f:
            image_data = base64.standard_b64encode(f.read()).decode('utf-8')
        
        # Default prompt
        if prompt is None:
            prompt = """
            Analyze this game UI and provide:
            1. Overall rating (1-10)
            2. Visual design quality assessment
            3. List of strengths
            4. List of weaknesses
            5. Specific suggestions for improvement
            6. Comparison to professional game UIs
            
            Be specific and actionable in your feedback.
            """
        
        if context:
            prompt = f"Context: {context}\n\n{prompt}"
        
        # Call Claude Vision API
        response = self.client.messages.create(
            model="claude-3-opus-20240229",
            max_tokens=2048,
            messages=[
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "image",
                            "source": {
                                "type": "base64",
                                "media_type": "image/png",
                                "data": image_data
                            }
                        },
                        {
                            "type": "text",
                            "text": prompt
                        }
                    ]
                }
            ]
        )
        
        # Parse response
        analysis_text = response.content[0].text
        
        # Extract structured data (simplified)
        return {
            "success": True,
            "rating": self._extract_rating(analysis_text),
            "analysis": {
                "full_text": analysis_text,
                "strengths": self._extract_section(analysis_text, "strengths"),
                "weaknesses": self._extract_section(analysis_text, "weaknesses"),
                "suggestions": self._extract_section(analysis_text, "suggestions")
            }
        }
    
    def _extract_rating(self, text: str) -> int:
        """Extract numerical rating from text"""
        import re
        match = re.search(r'rating[:\s]+(\d+)/10', text, re.IGNORECASE)
        if match:
            return int(match.group(1))
        return 0
    
    def _extract_section(self, text: str, section: str) -> List[str]:
        """Extract bulleted list from section"""
        import re
        pattern = rf'{section}[:\s]+(.*?)(?=\n\n|\Z)'
        match = re.search(pattern, text, re.IGNORECASE | re.DOTALL)
        if match:
            section_text = match.group(1)
            items = re.findall(r'[-•*]\s*(.+)', section_text)
            return items
        return []

# MCP Tool
@app.call_tool()
async def unity_ui_ask_vision_ai(
    imagePath: str,
    prompt: str = None,
    model: str = "claude-3-opus",
    contextDescription: str = None
) -> List[TextContent]:
    """
    Analyze UI screenshot using AI vision
    """
    tester = UIVisionTester(api_key=ANTHROPIC_API_KEY)
    
    result = await tester.analyze_ui_screenshot(
        image_path=imagePath,
        prompt=prompt,
        context=contextDescription
    )
    
    import json
    return [TextContent(
        type="text",
        text=json.dumps(result, indent=2)
    )]
```

---

## Test Report Generation

### Tool: `unity_ui_generate_test_report`

Creates comprehensive HTML report with screenshots.

**Parameters**:
```json
{
  "target": "MainMenuCanvas",
  "includeScreenshots": true,
  "includeMetrics": true,
  "includeRecommendations": true,
  "outputPath": "temp/reports/ui_test_report.html"
}
```

**Generated Report Includes**:
- Executive summary with overall score
- Screenshots (before/after, all states, all resolutions)
- Detailed metrics and scores
- Issue list with severity
- Comparison to standards
- Specific recommendations
- Code snippets for fixes
- Estimated fix time

---

## Success Metrics

### Quality Gates

UI must pass these thresholds:

| Metric | Minimum | Target |
|--------|---------|--------|
| Overall Quality Score | 75 | 85+ |
| Visual Appeal | 75 | 85+ |
| Accessibility Score | 70 (WCAG AA) | 85+ |
| Performance (FPS) | 30 | 60 |
| Responsive Tests Passed | 80% | 100% |
| Functional Tests Passed | 90% | 100% |
| AI Vision Rating | 7/10 | 8/10+ |

### Automated Pass/Fail

```json
{
  "qualityGates": {
    "overallScore": {"min": 75, "actual": 82, "passed": true},
    "accessibility": {"min": 70, "actual": 75, "passed": true},
    "responsive": {"min": 80, "actual": 60, "passed": false},
    "functional": {"min": 90, "actual": 95, "passed": true},
    "aiVisionRating": {"min": 7, "actual": 8, "passed": true}
  },
  "overallPassed": false,
  "blockers": ["Responsive test failed at 60%"],
  "recommendation": "Fix responsive layout issues before deployment"
}
```

---

## Conclusion

With comprehensive testing and validation tools, AI agents can:

1. **See** what they created via screenshots
2. **Analyze** quality using AI vision and automated metrics
3. **Test** functionality and responsiveness
4. **Identify** issues automatically
5. **Fix** problems iteratively
6. **Verify** fixes worked
7. **Report** results comprehensively

This closes the feedback loop, enabling AI to create **truly professional UI** with confidence.

### Implementation Priority

**Phase 1** (Week 1-2):
- Screenshot capture
- Basic quality scoring
- Visual analysis

**Phase 2** (Week 3-4):
- AI vision integration
- Functional testing
- Responsive testing

**Phase 3** (Week 5-6):
- Automated fixing
- Complete test suites
- Report generation

---

**Document Version**: 1.0  
**Last Updated**: October 10, 2025  
**Focus**: UI Testing & Validation for Unity MCP Server  
**Status**: Complete Design

