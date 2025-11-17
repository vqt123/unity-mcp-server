# Using Hero Prefabs In Game

## Quick Steps

1. **Create Animation Configs** (if not done)
   - Open `Tools/Setup/Create Hero Prefabs from FBX`
   - Click "Create Animation Configs for All Characters"
   - Assign Idle, Walk, and Fire animations in each config

2. **Create Hero Prefabs**
   - Click "Scan and Create Hero Prefabs"
   - Prefabs are saved to `Assets/Prefabs/` (e.g., `Hero_BlueSoldier_Male.prefab`)

3. **Assign Prefab to HeroConfigSO**
   - Open the HeroConfigSO asset (e.g., `Assets/Resources/HeroConfigs/BlueSoldier_Hero.asset`)
   - Drag the prefab from `Assets/Prefabs/` to the `heroPrefab` field
   - Make sure `heroTypeName` matches (e.g., "BlueSoldier")

4. **Test in Game**
   - Open your game scene (e.g., `ArenaGame.unity`)
   - Enter Play Mode
   - Hero should spawn with animations!

## Detailed Instructions

### Step 1: Create Hero Prefabs

After assigning animations in your configs, run the tool:
- `Tools/Setup/Create Hero Prefabs from FBX`
- Click "Scan and Create Hero Prefabs"
- Check console for success messages

**Output**: Prefabs in `Assets/Prefabs/` like:
- `Hero_BlueSoldier_Male.prefab`
- `Hero_Elf.prefab`
- etc.

### Step 2: Find or Create HeroConfigSO

Your hero configs are in `Assets/Resources/HeroConfigs/`:
- If you have `BlueSoldier_Hero.asset`, open it
- If not, create one: Right-click → `ArenaGame/Hero Config`
- Name it `BlueSoldier_Hero.asset`

### Step 3: Assign Prefab to Config

1. Open the HeroConfigSO asset in Inspector
2. Set `heroTypeName` to match your character (e.g., "BlueSoldier")
3. Drag `Hero_BlueSoldier_Male.prefab` from `Assets/Prefabs/` to the `heroPrefab` field
4. Save the asset

### Step 4: Verify Hero Type Name

The `heroTypeName` in HeroConfigSO must match:
- The name used when spawning heroes in code
- The name in your game's hero selection system

**Example**:
- If your code spawns `"BlueSoldier"`, the config's `heroTypeName` must be `"BlueSoldier"`
- The prefab name doesn't matter, only the `heroTypeName` field

### Step 5: Test in Game

1. Open your game scene (`Assets/Scenes/ArenaGame.unity` or similar)
2. Make sure `GameBootstrapper` is in the scene
3. Enter Play Mode
4. Your hero should spawn with animations!

## Troubleshooting

### Hero doesn't appear
- Check that `heroTypeName` in HeroConfigSO matches the spawn code
- Verify prefab is assigned in HeroConfigSO
- Check console for errors

### Animations not playing
- Verify Animator component exists on prefab
- Check that Animator Controller is assigned
- Verify avatar is assigned
- Check that animation clips are assigned in the controller

### Wrong hero appears
- Check `heroTypeName` matches what your code expects
- Verify the correct prefab is assigned

## File Locations

- **Prefabs**: `Assets/Prefabs/Hero_*.prefab`
- **Controllers**: `Assets/Prefabs/Hero_*_Controller.controller`
- **Configs**: `Assets/Resources/HeroConfigs/*_Hero.asset`
- **Animation Configs**: `Assets/Resources/HeroAnimationConfigs/*_Animations.asset`


