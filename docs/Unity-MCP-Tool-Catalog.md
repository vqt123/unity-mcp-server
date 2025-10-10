# Unity MCP Tool Catalog

Complete reference of all tools in the Unity MCP Server.

## Scene Management

### `unity_list_scenes`
List all scenes in the project.

**Parameters**: None

**Returns**:
```json
{
  "scenes": [
    {"name": "MainMenu", "path": "Assets/Scenes/MainMenu.unity", "buildIndex": 0},
    {"name": "Level1", "path": "Assets/Scenes/Level1.unity", "buildIndex": 1}
  ]
}
```

### `unity_load_scene`
Load a scene by name or path.

**Parameters**:
- `name` (string, required): Scene name or path
- `mode` (string, optional): "Single" or "Additive" (default: "Single")

**Returns**:
```json
{
  "success": true,
  "sceneName": "Level1",
  "gameObjectCount": 42
}
```

### `unity_save_scene`
Save the current scene.

**Parameters**:
- `path` (string, optional): Save location (if creating new scene)

**Returns**:
```json
{
  "success": true,
  "path": "Assets/Scenes/Level1.unity"
}
```

### `unity_create_scene`
Create a new scene.

**Parameters**:
- `name` (string, required): Scene name
- `path` (string, optional): Location (default: "Assets/Scenes/")
- `setup` (string, optional): "EmptyScene" or "DefaultGameObjects"

**Returns**:
```json
{
  "success": true,
  "path": "Assets/Scenes/NewScene.unity"
}
```

### `unity_get_hierarchy`
Get the complete scene hierarchy.

**Parameters**:
- `includeInactive` (boolean, optional): Include inactive GameObjects (default: false)
- `maxDepth` (number, optional): Maximum hierarchy depth (default: unlimited)

**Returns**:
```json
{
  "rootObjects": [
    {
      "name": "Main Camera",
      "id": 12345,
      "position": [0, 1, -10],
      "children": []
    },
    {
      "name": "Player",
      "id": 12346,
      "position": [0, 0, 0],
      "children": [
        {"name": "Model", "id": 12347}
      ]
    }
  ]
}
```

---

## GameObject Operations

### `unity_create_gameobject`
Create a new GameObject.

**Parameters**:
- `name` (string, required): GameObject name
- `position` (array, optional): Position [x, y, z]
- `rotation` (array, optional): Euler angles [x, y, z]
- `scale` (array, optional): Scale [x, y, z]
- `parent` (string, optional): Parent GameObject path
- `components` (array, optional): Component names to add
- `tag` (string, optional): Tag name
- `layer` (string, optional): Layer name

**Returns**:
```json
{
  "success": true,
  "gameObjectId": 12345,
  "name": "Player",
  "path": "Player"
}
```

### `unity_delete_gameobject`
Delete a GameObject.

**Parameters**:
- `target` (string, required): GameObject name or path
- `confirm` (boolean, required): Must be true for safety

**Returns**:
```json
{
  "success": true,
  "deletedPath": "Environment/Tree"
}
```

### `unity_find_gameobject`
Find GameObjects by criteria.

**Parameters**:
- `name` (string, optional): Name to search for
- `tag` (string, optional): Tag to filter by
- `layer` (string, optional): Layer to filter by
- `includeInactive` (boolean, optional): Include inactive objects
- `findAll` (boolean, optional): Return all matches (default: false)

**Returns**:
```json
{
  "count": 3,
  "results": [
    {
      "id": 12345,
      "name": "Player",
      "path": "Player",
      "position": [0, 0, 0],
      "active": true,
      "tag": "Player",
      "layer": "Default"
    }
  ]
}
```

### `unity_modify_gameobject`
Modify GameObject properties.

**Parameters**:
- `target` (string, required): GameObject path or ID
- `name` (string, optional): New name
- `position` (array, optional): New position [x, y, z]
- `rotation` (array, optional): New rotation [x, y, z]
- `scale` (array, optional): New scale [x, y, z]
- `parent` (string, optional): New parent path
- `active` (boolean, optional): Set active state
- `tag` (string, optional): Set tag
- `layer` (string, optional): Set layer

**Returns**:
```json
{
  "success": true,
  "gameObjectId": 12345,
  "modifiedProperties": ["position", "rotation"]
}
```

### `unity_duplicate_gameobject`
Duplicate a GameObject.

**Parameters**:
- `target` (string, required): Source GameObject path
- `name` (string, optional): Name for duplicate
- `position` (array, optional): Position for duplicate

**Returns**:
```json
{
  "success": true,
  "originalId": 12345,
  "duplicateId": 12350,
  "duplicatePath": "Player (1)"
}
```

---

## Component Operations

### `unity_add_component`
Add a component to a GameObject.

**Parameters**:
- `target` (string, required): GameObject path
- `componentType` (string, required): Component type name (e.g., "Rigidbody")
- `properties` (object, optional): Initial property values

**Returns**:
```json
{
  "success": true,
  "gameObjectPath": "Player",
  "componentType": "Rigidbody",
  "componentId": 54321
}
```

### `unity_remove_component`
Remove a component from a GameObject.

**Parameters**:
- `target` (string, required): GameObject path
- `componentType` (string, required): Component type to remove
- `confirm` (boolean, required): Must be true

**Returns**:
```json
{
  "success": true,
  "removedComponent": "BoxCollider"
}
```

### `unity_get_component`
Get component data from a GameObject.

**Parameters**:
- `target` (string, required): GameObject path
- `componentType` (string, optional): Specific component type (omit for all)

**Returns**:
```json
{
  "components": [
    {
      "type": "Transform",
      "properties": {
        "position": [0, 0, 0],
        "rotation": [0, 0, 0],
        "scale": [1, 1, 1]
      }
    },
    {
      "type": "Rigidbody",
      "properties": {
        "mass": 1.0,
        "useGravity": true,
        "isKinematic": false
      }
    }
  ]
}
```

### `unity_set_component_property`
Set a component property value.

**Parameters**:
- `target` (string, required): GameObject path
- `componentType` (string, required): Component type
- `propertyPath` (string, required): Property name (use dot notation for nested)
- `value` (any, required): New value

**Returns**:
```json
{
  "success": true,
  "componentType": "Rigidbody",
  "property": "mass",
  "oldValue": 1.0,
  "newValue": 5.0
}
```

### `unity_get_all_components`
List all components on a GameObject.

**Parameters**:
- `target` (string, required): GameObject path
- `includeInherited` (boolean, optional): Include inherited components

**Returns**:
```json
{
  "components": ["Transform", "MeshRenderer", "BoxCollider", "Rigidbody", "PlayerController"]
}
```

---

## Script Management

### `unity_list_scripts`
List all C# scripts in the project.

**Parameters**:
- `path` (string, optional): Filter by folder path
- `pattern` (string, optional): Name pattern to match

**Returns**:
```json
{
  "scripts": [
    {"name": "PlayerController", "path": "Assets/Scripts/PlayerController.cs", "size": 2048},
    {"name": "EnemyAI", "path": "Assets/Scripts/AI/EnemyAI.cs", "size": 3072}
  ],
  "count": 2
}
```

### `unity_read_script`
Read the contents of a script file.

**Parameters**:
- `path` (string, required): Script file path

**Returns**:
```json
{
  "path": "Assets/Scripts/PlayerController.cs",
  "content": "using UnityEngine;\n\npublic class PlayerController : MonoBehaviour\n{...}",
  "lineCount": 45,
  "size": 2048
}
```

### `unity_create_script`
Create a new C# script file.

**Parameters**:
- `name` (string, required): Script name (without .cs)
- `path` (string, optional): Location (default: "Assets/Scripts/")
- `template` (string, optional): "MonoBehaviour", "ScriptableObject", "Editor", "Empty"
- `namespace` (string, optional): Namespace to wrap script in

**Returns**:
```json
{
  "success": true,
  "path": "Assets/Scripts/NewScript.cs",
  "content": "..."
}
```

### `unity_modify_script`
Modify an existing script file.

**Parameters**:
- `path` (string, required): Script file path
- `content` (string, required): New content
- `validate` (boolean, optional): Validate before saving (default: true)

**Returns**:
```json
{
  "success": true,
  "path": "Assets/Scripts/PlayerController.cs",
  "compilationErrors": []
}
```

### `unity_delete_script`
Delete a script file.

**Parameters**:
- `path` (string, required): Script file path
- `confirm` (boolean, required): Must be true

**Returns**:
```json
{
  "success": true,
  "deletedPath": "Assets/Scripts/OldScript.cs"
}
```

### `unity_validate_script`
Check script for errors without saving.

**Parameters**:
- `content` (string, required): Script content to validate

**Returns**:
```json
{
  "valid": false,
  "errors": [
    {
      "line": 15,
      "column": 8,
      "message": "Type 'GameObject' could not be found",
      "severity": "error"
    }
  ]
}
```

---

## Asset Operations

### `unity_list_assets`
List assets by type or path.

**Parameters**:
- `type` (string, optional): Asset type ("Material", "Texture", "Prefab", etc.)
- `path` (string, optional): Filter by folder
- `search` (string, optional): Search term

**Returns**:
```json
{
  "assets": [
    {"name": "PlayerMaterial", "path": "Assets/Materials/PlayerMaterial.mat", "type": "Material"},
    {"name": "EnemyTexture", "path": "Assets/Textures/Enemy.png", "type": "Texture2D"}
  ],
  "count": 2
}
```

### `unity_create_material`
Create a new material.

**Parameters**:
- `name` (string, required): Material name
- `path` (string, optional): Save location (default: "Assets/Materials/")
- `shader` (string, optional): Shader name (default: "Standard")
- `properties` (object, optional): Initial properties

**Returns**:
```json
{
  "success": true,
  "path": "Assets/Materials/NewMaterial.mat",
  "materialId": 67890
}
```

### `unity_create_prefab`
Create a prefab from a GameObject.

**Parameters**:
- `source` (string, required): GameObject path
- `name` (string, optional): Prefab name (default: GameObject name)
- `path` (string, optional): Save location (default: "Assets/Prefabs/")

**Returns**:
```json
{
  "success": true,
  "prefabPath": "Assets/Prefabs/Player.prefab",
  "originalGameObject": "Player"
}
```

### `unity_instantiate_prefab`
Instantiate a prefab in the scene.

**Parameters**:
- `prefabPath` (string, required): Path to prefab asset
- `position` (array, optional): Spawn position [x, y, z]
- `rotation` (array, optional): Spawn rotation [x, y, z]
- `parent` (string, optional): Parent GameObject

**Returns**:
```json
{
  "success": true,
  "gameObjectId": 12355,
  "gameObjectPath": "Player (1)"
}
```

### `unity_import_asset`
Import an external asset file.

**Parameters**:
- `sourcePath` (string, required): Path to external file
- `destinationPath` (string, required): Target path in Assets/
- `importOptions` (object, optional): Import settings

**Returns**:
```json
{
  "success": true,
  "assetPath": "Assets/Models/Character.fbx",
  "assetType": "Model"
}
```

### `unity_modify_asset`
Modify asset properties.

**Parameters**:
- `path` (string, required): Asset path
- `properties` (object, required): Properties to change

**Returns**:
```json
{
  "success": true,
  "modifiedProperties": ["color", "metallic"]
}
```

### `unity_create_folder`
Create a new folder in Assets.

**Parameters**:
- `path` (string, required): Folder path (e.g., "Assets/NewFolder")

**Returns**:
```json
{
  "success": true,
  "path": "Assets/NewFolder"
}
```

---

## Editor Control

### `unity_play_mode`
Control play mode state.

**Parameters**:
- `action` (string, required): "play", "pause", or "stop"

**Returns**:
```json
{
  "success": true,
  "previousState": "stopped",
  "currentState": "playing"
}
```

### `unity_get_editor_state`
Get current editor state.

**Parameters**: None

**Returns**:
```json
{
  "playMode": "stopped",
  "isPaused": false,
  "isCompiling": false,
  "currentScene": "Assets/Scenes/Level1.unity",
  "selectedGameObject": "Player",
  "unityVersion": "2022.3.10f1"
}
```

### `unity_execute_menu`
Execute a Unity menu command.

**Parameters**:
- `menuPath` (string, required): Menu path (e.g., "GameObject/3D Object/Cube")

**Returns**:
```json
{
  "success": true,
  "menuPath": "GameObject/3D Object/Cube",
  "executed": true
}
```

### `unity_get_console`
Get console messages.

**Parameters**:
- `count` (number, optional): Number of messages (default: 50)
- `types` (array, optional): ["log", "warning", "error"] (default: all)
- `since` (string, optional): ISO timestamp to get messages after

**Returns**:
```json
{
  "messages": [
    {
      "type": "error",
      "message": "NullReferenceException: Object reference not set",
      "stackTrace": "at PlayerController.Update() ...",
      "timestamp": "2025-10-10T12:34:56Z"
    }
  ],
  "count": 1
}
```

### `unity_clear_console`
Clear all console messages.

**Parameters**: None

**Returns**:
```json
{
  "success": true,
  "clearedCount": 42
}
```

### `unity_select_gameobject`
Select a GameObject in the hierarchy.

**Parameters**:
- `target` (string, required): GameObject path

**Returns**:
```json
{
  "success": true,
  "selected": "Player"
}
```

### `unity_focus_scene_view`
Focus the scene view on a GameObject.

**Parameters**:
- `target` (string, required): GameObject path

**Returns**:
```json
{
  "success": true,
  "focusedOn": "Player"
}
```

---

## Build & Settings

### `unity_get_build_settings`
Get current build settings.

**Parameters**: None

**Returns**:
```json
{
  "target": "StandaloneWindows64",
  "scenes": [
    {"path": "Assets/Scenes/MainMenu.unity", "enabled": true},
    {"path": "Assets/Scenes/Level1.unity", "enabled": true}
  ],
  "developmentBuild": false
}
```

### `unity_set_build_settings`
Modify build settings.

**Parameters**:
- `target` (string, optional): Build target platform
- `scenes` (array, optional): Scene list
- `options` (object, optional): Build options

**Returns**:
```json
{
  "success": true,
  "target": "WebGL"
}
```

### `unity_get_player_settings`
Get player settings.

**Parameters**:
- `category` (string, optional): Specific category to get

**Returns**:
```json
{
  "companyName": "MyCompany",
  "productName": "MyGame",
  "version": "1.0.0",
  "bundleIdentifier": "com.mycompany.mygame"
}
```

### `unity_set_player_settings`
Modify player settings.

**Parameters**:
- `properties` (object, required): Properties to change

**Returns**:
```json
{
  "success": true,
  "modifiedProperties": ["companyName", "productName"]
}
```

### `unity_build_project`
Trigger a build.

**Parameters**:
- `outputPath` (string, required): Build output location
- `target` (string, optional): Build target (uses current if omitted)
- `options` (object, optional): Build options

**Returns**:
```json
{
  "success": true,
  "outputPath": "/Users/user/Builds/MyGame.app",
  "buildTime": 120.5,
  "size": 52428800
}
```

---

## Project Information

### `unity_get_project_info`
Get overall project information.

**Parameters**: None

**Returns**:
```json
{
  "projectName": "MyGame",
  "unityVersion": "2022.3.10f1",
  "projectPath": "/Users/user/Projects/MyGame",
  "sceneCount": 5,
  "scriptCount": 42,
  "assetCount": 234
}
```

### `unity_search_assets`
Search for assets across the project.

**Parameters**:
- `query` (string, required): Search query
- `type` (string, optional): Filter by type
- `path` (string, optional): Filter by path

**Returns**:
```json
{
  "results": [
    {"name": "Player", "path": "Assets/Scripts/Player.cs", "type": "Script"},
    {"name": "Player", "path": "Assets/Prefabs/Player.prefab", "type": "Prefab"}
  ],
  "count": 2
}
```

---

## Utility Tools

### `unity_undo`
Undo the last operation.

**Parameters**:
- `steps` (number, optional): Number of steps to undo (default: 1)

**Returns**:
```json
{
  "success": true,
  "undoneOperations": ["Create GameObject 'Cube'"]
}
```

### `unity_redo`
Redo the last undone operation.

**Parameters**:
- `steps` (number, optional): Number of steps to redo (default: 1)

**Returns**:
```json
{
  "success": true,
  "redoneOperations": ["Create GameObject 'Cube'"]
}
```

### `unity_ping_server`
Health check for Unity server.

**Parameters**: None

**Returns**:
```json
{
  "status": "ok",
  "version": "1.0.0",
  "uptime": 3600,
  "unityVersion": "2022.3.10f1"
}
```

---

## Total Tool Count

- **Scene Management**: 5 tools
- **GameObject Operations**: 5 tools
- **Component Operations**: 6 tools
- **Script Management**: 6 tools
- **Asset Operations**: 7 tools
- **Editor Control**: 7 tools
- **Build & Settings**: 5 tools
- **Project Information**: 2 tools
- **Utility**: 3 tools

**Total: 46 core tools**

---

**Version**: 1.0  
**Last Updated**: October 10, 2025

