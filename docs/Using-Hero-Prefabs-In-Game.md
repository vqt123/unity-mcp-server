# Using Hero Prefabs In Game

**Last Updated**: January 2025  
**Current System**: GlobalGameSettings for centralized hero configuration

---

## Quick Start

The current system uses **GlobalGameSettings** to configure default hero models and animations for all heroes. This is simpler than the old per-hero configuration system.

### Step 1: Configure GlobalGameSettings

1. **Open GlobalGameSettings**:
   - Navigate to `Assets/Resources/GlobalGameSettings.asset`
   - Or create new: Right-click → `ArenaGame/Global Game Settings`

2. **Assign Default Hero Model**:
   - Drag an FBX model from `Assets/Characters/FBX/` to the **Default Hero Model** field
   - Example: `BaseCharacter.fbx` or `BlueSoldier_Female.fbx`

3. **Assign Hero Animations**:
   - Expand the FBX model in Project window (click arrow)
   - Find animation clips (e.g., `CharacterArmature|Idle`, `CharacterArmature|Walk`, `CharacterArmature|Shoot_OneHanded`)
   - Drag animations to:
     - **Hero Idle Animation**
     - **Hero Walk Animation**
     - **Hero Fire Animation**

4. **Create Base Animator Controller** (if needed):
   - Click **"Create Hero Base Animator Controller"** button in Inspector
   - This creates `Assets/Resources/HeroBaseController.controller` with Idle, Walk, and Fire states

5. **Configure Animation Loop Settings** (optional):
   - Click **"Configure Animation Loop Settings"** button
   - This sets Idle/Walk to loop, Fire to not loop

### Step 2: Test in Game

1. Open your game scene (`Assets/Scenes/ArenaGame.unity` or similar)
2. Make sure `GameBootstrapper` is in the scene
3. Enter Play Mode
4. Heroes should spawn with animations from GlobalGameSettings!

---

## How It Works

### Runtime Animation Setup

When heroes spawn, `EntityVisualizer`:
1. Loads `GlobalGameSettings` from Resources
2. Instantiates the `defaultHeroModel` GameObject
3. Creates an `AnimatorOverrideController` from `HeroBaseController`
4. Assigns animations from `GlobalGameSettings` to the override controller
5. All heroes use the same model/animations (configurable globally)

### Code Reference

**Location**: `Assets/Scripts/Client/EntityVisualizer.cs`

```csharp
private void SetupHeroAnimator(GameObject heroObj, GlobalGameSettings globalSettings)
{
    // Creates AnimatorOverrideController with animations from GlobalGameSettings
    // Assigns Idle, Walk, and Fire animations
}
```

---

## Troubleshooting

### Hero doesn't appear
- Check that `GlobalGameSettings.defaultHeroModel` is assigned
- Verify the FBX model exists in `Assets/Characters/FBX/`
- Check console for errors

### Animations not playing
- Verify `GlobalGameSettings.heroIdleAnimation` is assigned
- Check that `HeroBaseController.controller` exists in `Assets/Resources/`
- If missing, click **"Create Hero Base Animator Controller"** in GlobalGameSettings Inspector
- Check console for `[animtest]` logs

### Wrong animations playing
- Verify animation clips are assigned in `GlobalGameSettings`
- Check that animation clips come from the same FBX as the model
- Ensure animations are properly imported (expand FBX to see clips)

### Animations not looping
- Click **"Configure Animation Loop Settings"** in GlobalGameSettings Inspector
- This sets loop settings on the animation clips

---

## File Locations

- **GlobalGameSettings**: `Assets/Resources/GlobalGameSettings.asset`
- **Base Animator Controller**: `Assets/Resources/HeroBaseController.controller`
- **Hero Models**: `Assets/Characters/FBX/*.fbx`
- **Hero Configs** (for combat stats): `Assets/Resources/HeroConfigs/*_Hero.asset`

---

## Advanced: Per-Hero Overrides

Currently, all heroes use the same model/animations from `GlobalGameSettings`. 

If you need per-hero customization:
1. Assign different prefabs in `HeroConfigSO.heroPrefab` (in `Assets/Resources/HeroConfigs/`)
2. Those prefabs will be used instead of `GlobalGameSettings.defaultHeroModel`
3. The prefab should have its own Animator Controller configured

**Note**: The system prioritizes:
1. `HeroConfigSO.heroPrefab` (if assigned)
2. `GlobalGameSettings.defaultHeroModel` (fallback)

---

## Related Documentation

- **Main.md**: Project overview
- **Project-Audit.md**: Current system documentation
- **GlobalGameSettings.cs**: ScriptableObject definition
