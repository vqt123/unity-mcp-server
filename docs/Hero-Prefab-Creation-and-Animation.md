# Hero Prefab Creation and Animation

**Documentation for**: Unity 2D Arena Survivor Game  
**Last Updated**: January 2025  
**Current System**: GlobalGameSettings for centralized configuration

---

## Overview

The current animation system uses **GlobalGameSettings** to configure default hero models and animations for all heroes. This is simpler than the old per-hero configuration system.

**Key Components**:
- `GlobalGameSettings.cs` - Central ScriptableObject for all hero/enemy settings
- `EntityVisualizer.cs` - Sets up animators at runtime using GlobalGameSettings
- `GlobalGameSettingsEditor.cs` - Custom editor with validation and controller creation

---

## Table of Contents

1. [Current Animation System](#current-animation-system)
2. [GlobalGameSettings Configuration](#globalgamesettings-configuration)
3. [Runtime Animation Setup](#runtime-animation-setup)
4. [FBX Character Models](#fbx-character-models)
5. [Troubleshooting](#troubleshooting)

---

## Current Animation System

### How It Works

1. **Global Configuration**: All heroes use the same model/animations from `GlobalGameSettings`
2. **Runtime Setup**: `EntityVisualizer` creates `AnimatorOverrideController` at runtime
3. **Animation Source**: Animations come directly from character FBX files (e.g., `BaseCharacter.fbx`)
4. **Base Controller**: Uses `HeroBaseController.controller` as the base Animator Controller

### Key Files

- **`Assets/Resources/GlobalGameSettings.asset`** - Main settings asset
- **`Assets/Resources/HeroBaseController.controller`** - Base animator controller (created via editor)
- **`Assets/Characters/FBX/BaseCharacter.fbx`** - Default hero model with animations
- **`Assets/Editor/GlobalGameSettingsEditor.cs`** - Editor tooling

---

## GlobalGameSettings Configuration

### Step 1: Open GlobalGameSettings

1. Navigate to `Assets/Resources/GlobalGameSettings.asset`
2. Or create new: Right-click → `ArenaGame/Global Game Settings`

### Step 2: Assign Default Hero Model

1. Drag an FBX model from `Assets/Characters/FBX/` to **Default Hero Model** field
2. Example: `BaseCharacter.fbx` or `BlueSoldier_Female.fbx`

### Step 3: Assign Hero Animations

1. **Expand the FBX model** in Project window (click arrow next to FBX)
2. **Find animation clips** (e.g., `CharacterArmature|Idle`, `CharacterArmature|Walk`, `CharacterArmature|Shoot_OneHanded`)
3. **Drag animations** to:
   - **Hero Idle Animation** - Looping idle animation
   - **Hero Walk Animation** - Looping walk animation
   - **Hero Fire Animation** - Non-looping fire/attack animation

### Step 4: Create Base Animator Controller

1. Click **"Create Hero Base Animator Controller"** button in Inspector
2. This creates `Assets/Resources/HeroBaseController.controller` with:
   - **Idle** state (default)
   - **Walk** state
   - **Fire** state

### Step 5: Configure Animation Loop Settings

1. Click **"Configure Animation Loop Settings"** button
2. This automatically sets:
   - `loopTime = true` for Idle and Walk animations
   - `loopTime = false` for Fire animation

### Validation

The editor shows validation messages:
- ✅ **Hero settings are valid!** - All required fields assigned
- ⚠️ **Hero settings: Please assign...** - Missing required fields

---

## Runtime Animation Setup

### How EntityVisualizer Works

**Location**: `Assets/Scripts/Client/EntityVisualizer.cs`

When a hero spawns:

1. **Load GlobalGameSettings**:
   ```csharp
   GlobalGameSettings globalSettings = Resources.Load<GlobalGameSettings>("GlobalGameSettings");
   ```

2. **Instantiate Hero Model**:
   ```csharp
   GameObject heroObj = Instantiate(globalSettings.defaultHeroModel, position, rotation);
   ```

3. **Setup Animator**:
   ```csharp
   SetupHeroAnimator(heroObj, globalSettings);
   ```

4. **Create AnimatorOverrideController**:
   - Loads `HeroBaseController.controller` from Resources
   - Creates `AnimatorOverrideController` from base controller
   - Assigns animations from `GlobalGameSettings`:
     - `overrideController["Idle"] = settings.heroIdleAnimation;`
     - `overrideController["Walk"] = settings.heroWalkAnimation;`
     - `overrideController["Fire"] = settings.heroFireAnimation;`

5. **Play Initial Animation**:
   ```csharp
   animator.Play("Idle", 0, 0f);
   ```

### Animation State Management

The system automatically switches animations based on:
- **Movement**: Idle vs Walk (based on velocity)
- **Shooting**: Fire animation plays when hero shoots

**Code Reference**: `UpdateHeroAnimationFromVelocity()` in `EntityVisualizer.cs`

---

## FBX Character Models

### Location

Character models are stored in `Assets/Characters/FBX/`:
- `BaseCharacter.fbx` - Default hero model (currently used)
- `BlueSoldier_Female.fbx` - Alternative hero model
- `BlueSoldier_Male.fbx` - Alternative hero model
- Many other character variants...

### FBX Import Settings

For animations to work, FBX files should be configured:

1. **Select FBX** in Project window
2. **Inspector → Rig tab**:
   - **Animation Type**: Can be `Generic`, `Humanoid`, or `Legacy`
   - **Avatar Definition**: `Create From This Model` (if Humanoid)
3. **Inspector → Animation tab**:
   - **Import Animation**: ✅ Enabled
   - Animation clips should be visible when you expand the FBX

### Animation Clips in FBX

Animations are embedded in the FBX files:
- Expand the FBX in Project window to see clips
- Names like `CharacterArmature|Idle`, `CharacterArmature|Walk`, etc.
- These are the clips you assign in `GlobalGameSettings`

---

## Troubleshooting

### Heroes Not Appearing

**Symptoms**: Heroes don't spawn in game

**Checks**:
1. Verify `GlobalGameSettings.defaultHeroModel` is assigned
2. Check that FBX model exists in `Assets/Characters/FBX/`
3. Check console for errors

**Fix**: Assign a valid FBX model to `GlobalGameSettings.defaultHeroModel`

---

### Animations Not Playing

**Symptoms**: Heroes appear but don't animate

**Checks**:
1. Verify `GlobalGameSettings.heroIdleAnimation` is assigned
2. Check that `HeroBaseController.controller` exists in `Assets/Resources/`
3. Check console for `[animtest]` logs

**Fix**:
1. Click **"Create Hero Base Animator Controller"** in GlobalGameSettings Inspector
2. Assign animation clips to GlobalGameSettings
3. Check console logs for specific errors

---

### Animations Not Looping

**Symptoms**: Idle/Walk animations play once and stop

**Checks**:
1. Verify animation clips have `loopTime = true`
2. Check Animator state loop settings

**Fix**:
1. Click **"Configure Animation Loop Settings"** in GlobalGameSettings Inspector
2. Or manually set loop settings on animation clips

---

### Wrong Animations Playing

**Symptoms**: Wrong animation clips are playing

**Checks**:
1. Verify animation clips are assigned in `GlobalGameSettings`
2. Check that clips come from the same FBX as the model
3. Verify clips are properly imported

**Fix**: Reassign correct animation clips in GlobalGameSettings

---

### Avatar Errors

**Symptoms**: "Invalid Avatar" or "No avatar found" errors

**Checks**:
1. FBX import settings (Rig tab)
2. Animation Type setting (Generic/Humanoid/Legacy)

**Fix**:
- For Generic animations: Avatar not required
- For Humanoid: Set Avatar Definition to "Create From This Model"
- Reimport FBX after changing settings

---

## Advanced: Per-Hero Overrides

Currently, all heroes use the same model/animations from `GlobalGameSettings`.

### Using HeroConfigSO for Per-Hero Prefabs

If you need per-hero customization:

1. **Assign Prefab in HeroConfigSO**:
   - Open `Assets/Resources/HeroConfigs/*_Hero.asset`
   - Assign `heroPrefab` field to a custom prefab
   - That prefab will be used instead of `GlobalGameSettings.defaultHeroModel`

2. **Prefab Priority**:
   - System checks `HeroConfigSO.heroPrefab` first
   - Falls back to `GlobalGameSettings.defaultHeroModel` if not assigned

3. **Custom Prefab Setup**:
   - Prefab should have Animator component
   - Can have its own Animator Controller
   - Or will use GlobalGameSettings animations if no controller

---

## File Structure Summary

```
Assets/
├── Resources/
│   ├── GlobalGameSettings.asset          # Main settings (configure here)
│   └── HeroBaseController.controller    # Base animator controller (auto-created)
│
├── Characters/
│   └── FBX/                              # Character models with animations
│       ├── BaseCharacter.fbx            # Default hero model
│       ├── BlueSoldier_Female.fbx       # Alternative models
│       └── ...
│
├── Scripts/Client/
│   ├── GlobalGameSettings.cs            # ScriptableObject definition
│   └── EntityVisualizer.cs              # Runtime animation setup
│
└── Editor/
    └── GlobalGameSettingsEditor.cs       # Custom editor with buttons
```

---

## Key Components Reference

### ScriptableObjects
- **GlobalGameSettings**: Central configuration for all heroes/enemies

### Runtime Components
- **EntityVisualizer**: Creates GameObjects and sets up animators
- **AnimatorOverrideController**: Created at runtime with animations from GlobalGameSettings

### Editor Tools
- **GlobalGameSettingsEditor**: Custom inspector with validation and controller creation buttons

---

## Related Documentation

- **Main.md**: Project overview and architecture
- **Using-Hero-Prefabs-In-Game.md**: Quick start guide
- **Manual-Animation-Testing-Guide.md**: Manual testing steps
- **Project-Audit.md**: Current system documentation

---

**End of Documentation**
