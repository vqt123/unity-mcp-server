# Unity Arena Game - Main Documentation

**Project Type**: Unity 2D Arena Survivor Game with MCP Server Integration  
**Unity Version**: 6000.2.6f2  
**Architecture**: Client-Server with Deterministic Simulation  
**Last Updated**: October 16, 2025

---

## 📋 Table of Contents

### 1. [Architecture Overview](#architecture-overview)
### 2. [MCP Server System](#mcp-server-system)
### 3. [Game Systems](#game-systems)
### 4. [Project Structure](#project-structure)
### 5. [Configuration](#configuration)
### 6. [Scenes](#scenes)
### 7. [Quick Reference](#quick-reference)

---

## Architecture Overview

### Client-Server Model
- **Shared Assembly** (`Assets/Shared/`) - Deterministic simulation, no Unity dependencies
- **Client Assembly** (`Assets/Scripts/Client/`) - Unity visualization layer
- **Separation**: Clean boundary between simulation and rendering

### Key Principles
1. **Deterministic Simulation** - Fixed-point math (16.16 format)
2. **Event-Driven** - All state changes emit events
3. **Command-Driven** - Player inputs are commands
4. **Auto-Gameplay** - Survivor-style mechanics

**Details**: `docs/archive/Client-Server-Architecture-Research.md`

---

## MCP Server System

### Purpose
Unity Editor control via Model Context Protocol for AI agent automation.

### Known Issues & Workarounds

#### Adding Namespaced Components
**Issue**: `unity_add_script_component` fails with namespaced scripts  
**Workaround**: Use `unity_add_component` with fully qualified name:
```
✅ unity_add_component(gameObjectName="Obj", componentType="Namespace.ClassName")
❌ unity_add_script_component(gameObjectName="Obj", scriptName="ClassName")
```

#### Button onClick Events  
**Issue**: `unity_set_button_onclick` cannot find existing SceneLoader script  
**Status**: Tool bug - needs fix in mcp-server  
**Temporary**: Wire buttons in Start() method of controller script

### Structure
```
mcp-server/
├── unity_mcp_server.py          # Main server (89 lines)
└── tools/                        # Organized tool modules
    ├── __init__.py
    ├── core_tools.py             # Ping, compile, logs
    ├── scene_tools.py            # Scene management
    ├── gameobject_tools.py       # GameObject operations
    ├── prefab_tools.py           # Prefab management
    ├── script_tools.py           # Script & components
    └── ui_tools.py               # UI creation
```

### Unity Package
```
Packages/com.vtrinh.unitymcp/
└── Editor/UnityMCP/
    ├── MCPServer.cs              # HTTP server
    ├── MCPTools.cs               # Tool implementations (3814 lines)
    └── MCPServerWindow.cs        # Editor UI
```

**Port**: 8765  
**Protocol**: HTTP → Unity Editor

**Details**: `.cursor/rules/unity-mcp-core.mdc`

---

## Game Systems

### Core Simulation (`Assets/Shared/`)

#### Math System
- `Math/Fix64.cs` - 16.16 fixed-point numbers
- `Math/FixV2.cs` - 2D vectors
- `Math/FixV3.cs` - 3D vectors

#### Entities
- `Entities/Hero.cs` - Player character
- `Entities/Enemy.cs` - AI enemies  
- `Entities/Projectile.cs` - Bullets
- `Entities/EntityId.cs` - Unique identifiers

#### Systems (30 ticks/second)
- `Systems/MovementSystem.cs` - Position updates, arena bounds
- `Systems/CombatSystem.cs` - Auto-shooting, collision, damage
- `Systems/AISystem.cs` - Enemy targeting & pathfinding
- `Systems/SpawnSystem.cs` - Entity spawning

#### Core
- `Core/Simulation.cs` - Main orchestrator
- `Core/SimulationWorld.cs` - Entity storage
- `Core/SimulationConfig.cs` - Constants (30 tps, arena radius)
- `Core/CommandProcessor.cs` - Player command handling

#### Events & Commands
- `Events/SimulationEvents.cs` - Spawn, damage, kill events
- `Commands/SimulationCommands.cs` - Upgrade, weapon commands

### Client Visualization (`Assets/Scripts/Client/`)

#### Core Managers
- `GameSimulation.cs` - Runs simulation loop (30 tps)
- `EventBus.cs` - Pub/sub event system
- `GameBootstrapper.cs` - Auto-setup all managers

#### Visual Systems
- `EntityVisualizer.cs` - Creates GameObjects from events
- `EntityView.cs` - Links GameObject ↔ EntityId
- `EntityRotation.cs` - Face movement direction
- `ProjectileTrail.cs` - Visual projectile trails

#### UI Controllers
- `HealthBarController.cs` - Reactive health bars
- `DamageNumberSpawner.cs` - Floating damage text
- `CombatEffectsManager.cs` - VFX & audio
- `CooldownUIController.cs` - Ability cooldown display
- `UpgradeUIManager.cs` - Level-up upgrade choices
- `HeroSelectionManager.cs` - Hero selection UI

#### Game Flow
- `GameInitializer.cs` - Initial hero spawning
- `WaveManager.cs` - Enemy wave spawning
- `CameraController.cs` - Follow heroes
- `SimulationDebugger.cs` - Debug UI

#### Utilities
- `ConversionUtility.cs` - Type conversions
- `UnityLogger.cs` - Logging bridge

**Details**: `docs/archive/PROGRESS-SUMMARY.md`

---

## Project Structure

```
mcptest/
├── Main.md                       # ← YOU ARE HERE
├── Assets/
│   ├── Prefabs/                  # Reusable prefabs
│   │   ├── Hero.prefab
│   │   ├── Enemy.prefab
│   │   └── Projectile.prefab
│   ├── Resources/                # Runtime-loaded assets
│   │   ├── BloodEffect.prefab
│   │   ├── HeroTypes.json
│   │   ├── WeaponTypes.json
│   │   └── Levels.json
│   ├── Scenes/                   # Unity scenes
│   │   ├── MainMenu.unity
│   │   ├── GameScene.unity
│   │   └── ArenaGame.unity
│   ├── Scripts/
│   │   ├── Client/               # Unity visualization (Client.asmdef)
│   │   ├── ConfigManager.cs      # JSON config loader
│   │   └── SceneLoader.cs        # Scene transitions
│   └── Shared/                   # Deterministic simulation (Shared.asmdef)
│       ├── Math/                 # Fixed-point math
│       ├── Entities/             # Game entities
│       ├── Systems/              # Game systems
│       ├── Events/               # Event definitions
│       ├── Commands/             # Command definitions
│       ├── Data/                 # Configuration data
│       └── Core/                 # Simulation core
├── mcp-server/                   # Python MCP server
│   ├── unity_mcp_server.py       # Main (89 lines)
│   ├── requirements.txt
│   └── tools/                    # Modular tools
├── Packages/
│   └── com.vtrinh.unitymcp/      # Unity MCP package
├── docs/
│   └── archive/                  # Documentation
├── .cursor/
│   └── rules/                    # AI assistant rules
│       ├── main-index.mdc        # This file's rule
│       ├── unity-mcp-core.mdc
│       └── unity-ui-best-practices.mdc
└── ProjectSettings/              # Unity project settings
```

---

## Configuration

### Game Config Files (`Assets/Resources/`)

#### HeroTypes.json
Defines hero types with stats:
- Health, damage, shoot cooldown
- Bullet speed, starting weapon
- Color

#### WeaponTypes.json
Weapon progression tiers:
- Damage, cooldown, bullet speed
- Projectile count, AOE radius
- Piercing, descriptions

#### Levels.json
Wave-based level design:
- Wave configurations
- Enemy types and counts
- Spawn intervals, durations
- Boss waves

**Manager**: `Assets/Scripts/ConfigManager.cs`

---

## Scenes

### MainMenu.unity
- Main menu UI
- Scene navigation
- SceneLoader for transitions

### GameScene.unity
- Full game scene with ground tiles
- GameBootstrap auto-setup
- All managers pre-configured

### ArenaGame.unity
- Arena-style game scene
- Simpler setup
- Used for development

**Scene Management**: Tools available in MCP server (`unity_load_scene`, `unity_create_scene`)

---

## Quick Reference

### Starting Development

1. **Open Unity**: Load any scene (GameScene recommended)
2. **MCP Server**: Auto-starts in Unity Editor (Window → Unity MCP → MCP Server)
3. **Play Mode**: Heroes auto-select, waves auto-start

### Key Files to Edit

- **Game Balance**: `Assets/Resources/*.json`
- **Hero Behavior**: `Assets/Shared/Systems/CombatSystem.cs`
- **Enemy AI**: `Assets/Shared/Systems/AISystem.cs`
- **Visual Effects**: `Assets/Scripts/Client/CombatEffectsManager.cs`
- **UI**: Scenes under `Assets/Scenes/`

### Adding New Features

1. **Simulation Logic** → `Assets/Shared/Systems/`
2. **Events** → `Assets/Shared/Events/SimulationEvents.cs`
3. **Visualization** → `Assets/Scripts/Client/`
4. **UI** → Use MCP tools or Unity Editor

### MCP Server Usage

**Port**: 8765  
**Test Connection**: `unity_ping`  
**Get Logs**: `unity_get_logs`

**Full Tool List**: See `.cursor/rules/unity-mcp-core.mdc`

---

## Recent Changes

### Cleanup (Oct 16, 2025)
- ✅ Deleted 82 redundant files
- ✅ Consolidated prefabs to single location
- ✅ Refactored MCP server (1017 → 89 lines)
- ✅ Removed duplicate documentation

**Details**: `CLEANUP-COMPLETE.md`

---

## External Resources

- **Architecture Deep Dive**: `docs/archive/Client-Server-Architecture-Research.md`
- **Wave System**: `docs/archive/Level-Wave-System.md`  
- **Config System**: `docs/archive/MVP-Game-Config-System.md`
- **Unity Learnings**: `docs/archive/Unity-Arena-Game-Learnings.md`
- **MCP User Guide**: `docs/archive/Unity-MCP-User-Guide.md`

---

## Assembly Definitions

### ArenaGame.Shared.asmdef
- Path: `Assets/Shared/`
- Dependencies: None
- Flag: `noEngineReferences: true`
- Purpose: Deterministic simulation (can run on server)

### Client.asmdef
- Path: `Assets/Scripts/Client/`
- Dependencies: `ArenaGame.Shared`
- Purpose: Unity visualization layer

**No circular dependencies** - Clean separation

---

## Common Tasks

### Debug Simulation
1. Enable `SimulationDebugger` in scene
2. Check console for `[Simulation]` logs
3. Use `unity_get_logs` via MCP

### Add New Enemy Type
1. Update `Resources/Levels.json`
2. Optionally add to `Resources/HeroTypes.json` if new stats needed
3. Create prefab variant in `Assets/Prefabs/`

### Modify UI
1. Use MCP UI tools (`unity_ui_create_*`)
2. Or edit scenes directly in Unity
3. See `.cursor/rules/unity-ui-best-practices.mdc`

### Test Build
1. Ensure scenes in Build Settings (`unity_add_scene_to_build`)
2. File → Build Settings → Build
3. Test executable

---

## Development Guidelines

1. **Simulation Code** - No Unity dependencies, use fixed-point math
2. **Client Code** - Subscribe to events, don't call simulation directly
3. **Events** - Emit for all state changes
4. **Commands** - Only for player choices
5. **Testing** - Use MCP tools for automation

---

**For detailed information on any system, refer to the linked documents above.**


