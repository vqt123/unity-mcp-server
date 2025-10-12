# Unity Arena Game - Development Learnings

## Critical Unity UI Radial Fill Issues

### Problem: Radial Fill Not Animating

**Symptoms:**
- `Image.fillAmount` updates correctly in code (verified by logs)
- Visual sprite doesn't animate at all
- fillMethod and fillOrigin set correctly

**Root Cause:**
Image component `type` property MUST be set to `Filled` for fillAmount to work. If type is set to `Sliced`, Unity completely ignores fillAmount updates.

**Why This Happens:**
- 9-sliceable sprites (like GUI sprites) automatically set Image.type to "Sliced" when assigned
- "Sliced" type is for stretching sprites without distortion
- "Filled" type is for fill animations (radial, horizontal, vertical)
- These are mutually exclusive modes

**Solution:**
Always set Image.type to `Filled` AFTER assigning a sprite for radial/fill animations:

```csharp
image.sprite = mySprite;
image.type = UnityEngine.UI.Image.Type.Filled;  // MUST come after sprite assignment
image.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
image.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
```

**Tool Implementation:**
Created `unity_set_image_fill` tool that forces Image.type to Filled and configures fill method/origin.

---

## Sprite Loading from Sprite Sheets

### Problem: Always Loading First Sprite (GUI_0) Instead of Requested Sprite (GUI_12)

**Current Status:** ✅ RESOLVED

**Root Cause:**
`AssetDatabase.LoadAssetAtPath<Sprite>(spritePath)` automatically returns the FIRST sprite in a sprite sheet when called on a sheet file. This caused the code to never reach the sub-asset loading logic that searches for a specific sprite name.

**Original Broken Logic:**
```csharp
// This loads the first sprite from the sheet automatically!
Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
if (sprite == null)  // Never null, so sub-asset code never runs
{
    // This code is never reached
    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
    // ... search for spriteName ...
}
```

**Fixed Logic:**
```csharp
Sprite sprite = null;

// If specific sprite name requested, load from sheet FIRST
if (!string.IsNullOrEmpty(spriteName))
{
    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
    foreach (UnityEngine.Object asset in sprites)
    {
        if (asset is Sprite s && s.name == spriteName)
        {
            sprite = s;
            break;
        }
    }
}

// Only try direct load if no specific name or not found
if (sprite == null)
{
    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
}
```

**Key Insight:**
When loading a named sprite from a sheet, ALWAYS use `LoadAllAssetsAtPath` and iterate to find the matching sprite. Direct `LoadAssetAtPath<Sprite>` only works for single sprite files or to get the default (first) sprite from a sheet.

**MCP Tool Usage:**
```json
{
  "tool": "unity_ui_set_sprite",
  "args": {
    "objectPath": "Canvas/Image",
    "spritePath": "Assets/2D Casual UI/Sprite/GUI.png",
    "spriteName": "GUI_12"
  }
}
```

**Verification:**
Debug logs will show the iteration through sprites until the target is found:
```
[MCP] Loading sprite 'GUI_12' from sheet with 63 assets
[MCP] Found sprite: GUI_0
[MCP] Found sprite: GUI_1
...
[MCP] Found sprite: GUI_12
[MCP] ✓ Matched sprite: GUI_12
```

---

## MCP Tool Development Patterns

### Threading Model for Unity Editor Operations

**Critical Rule:** All Unity API calls MUST happen on main thread.

**Pattern:**
1. HTTP request received on background thread
2. Parse JSON on background thread
3. Enqueue action to `ConcurrentQueue<Action>`
4. Process queue in `EditorApplication.update` (main thread)
5. Execute Unity APIs safely
6. Send HTTP response

### Component Property Setting

**SerializedProperty Names:**
Unity's serialized property names often differ from public property names:
- `isTrigger` → `m_IsTrigger`
- `enabled` → `m_Enabled`

**Finding Property Names:**
Use Unity's SerializedObject inspector or scene file to find exact names.

### Automatic Tag Creation

**Problem:** Setting a tag that doesn't exist throws an error.

**Solution:** Check if tag exists, create it if missing:

```csharp
SerializedObject tagManager = new SerializedObject(
    AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
);
SerializedProperty tagsProp = tagManager.FindProperty("tags");
tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
newTag.stringValue = tag;
tagManager.ApplyModifiedProperties();
```

---

## Arena Game Implementation Notes

### Game Balance

**Hero Stats:**
- Health: 100 HP
- Damage: 10
- Shoot Cooldown: 1 second
- Auto-targets closest enemy

**Enemy Stats:**
- Health: 30 HP
- Damage: 5
- Move Speed: 3 units/sec
- Attack Range: 1.5 units
- Attack Cooldown: 1 second

**Spawning:**
- Interval: 2 seconds
- Max Enemies: 20
- Spawn Location: Random position on circle (radius 10)

### Critical GameObject Requirements

**Tags:**
- Hero MUST have "Player" tag (for Enemy AI to find)
- Enemies MUST have "Enemy" tag (for Bullet collision)

**Colliders:**
- Bullet and Enemy colliders MUST be triggers (`isTrigger = true`)
- Required for OnTriggerEnter to fire

**Prefab References:**
- Hero.bulletPrefab → BulletPrefab GameObject
- EnemySpawner.enemyPrefab → EnemyPrefab GameObject
- GameManager.scoreText → Canvas/Text TextMeshProUGUI
- GameManager.cooldownRadial → Canvas/Image (with Filled type)

### Scene Setup

**Camera:**
- Position: (0, 20, 0)
- Rotation: (90, 0, 0) - looking straight down
- Clear Flags: Skybox or Solid Color

**Arena:**
- Floor: Scaled cube (20, 0.1, 20)
- Hero at center (0, 0.5, 0)

---

## UI Best Practices

### Responsive Layout

**Use Anchors for Multi-Resolution Support:**
- Top-center for score text
- Bottom-center for cooldown indicator
- Never use absolute positioning for production UI

**Anchor Presets:**
- `top-center`: Title/score text
- `bottom-center`: Action buttons, status indicators
- `center`: Centered content
- `stretch-all`: Full-screen backgrounds

### Sprite Usage

**Standard Placeholder:** GUI_12 from 2D Casual UI package
- 9-sliceable for buttons/panels
- Path: `Assets/2D Casual UI/Sprite/GUI.png`
- Use as sub-asset from sprite sheet

**Setting Sprites via MCP:**
```json
{
  "tool": "unity_ui_set_sprite",
  "args": {
    "objectPath": "Canvas/Button",
    "spritePath": "Assets/2D Casual UI/Sprite/GUI.png",
    "spriteName": "GUI_12"
  }
}
```

---

## Testing Workflow

### Compilation & Testing Loop

1. Edit script → Save
2. Call `unity_force_compile`
3. Wait for compilation (check spinner in Unity)
4. Call `unity_wait_for_compile` (or wait 5s)
5. Test changes

**Server Restart:**
- Auto-restarts after compilation (background safe)
- Manual restart: `unity_restart_server` tool

### Debug Logging

**Pattern:**
```csharp
Debug.Log($"[ComponentName] Message with {variable}");
```

**View Logs via MCP:**
```json
{"tool": "unity_get_logs", "args": {"count": 50}}
```

### Screenshot Verification

```json
{"tool": "unity_capture_screenshot", "args": {"viewType": "game"}}
```

Screenshots saved to: `/Screenshots/screenshot_YYYY-MM-DD_HH-MM-SS.png`

---

## Common Pitfalls

1. **Image.type resets to Sliced when sprite assigned**
   - Always set type AFTER assigning sprite
   
2. **Tags must exist before assignment**
   - Create tag first or use auto-creation logic

3. **Colliders need isTrigger for bullets**
   - Use `m_IsTrigger` property name in SerializedObject

4. **References don't persist without EditorUtility.SetDirty()**
   - Always mark objects dirty after changes

5. **Scene changes need explicit save**
   - Call `unity_save_scene` after modifications

6. **Server must restart after code changes**
   - Happens automatically after compilation
   - Works in background (no Unity focus required)

7. **unity_set_component_property doesn't change script default values** ⚠️
   - `unity_set_component_property` only sets Inspector values
   - Script default values (e.g., `public float bulletSpeed = 10f;`) override Inspector
   - **Solution:** Edit the script file directly to change default values
   - Example: Change `bulletSpeed = 10f` to `bulletSpeed = 400f` in Hero.cs

---

## Tools Created for Arena Game

### GameObject Creation Tools
- `unity_create_cube` - Create cube primitive (legacy, use create_primitive)
- `unity_create_primitive` - Create any primitive type (Sphere, Capsule, Cylinder, Plane, Quad, Cube)

### Visual Effects Tools
- `unity_add_particle_trail` - Add particle trail effect to GameObject (perfect for projectiles) ⭐ NEW

### Transform Tools
- `unity_set_position` - Set GameObject position
- `unity_set_rotation` - Set GameObject rotation (euler angles)
- `unity_set_scale` - Set GameObject scale
- `unity_set_tag` - Set/create GameObject tag

### Component Tools
- `unity_add_component` - Add component to GameObject
- `unity_remove_component` - Remove component from GameObject
- `unity_set_component_property` - Set component property/reference (Inspector values only, not script defaults)

### UI Tools
- `unity_set_ui_size` - Set RectTransform size
- `unity_set_anchors` - Set UI anchors (with presets)
- `unity_set_image_fill` - Configure Image for fill animations
- `unity_ui_set_sprite` - Assign sprite to Image component

### Scene Tools
- `unity_create_scene` - Create new scene
- `unity_load_scene` - Load scene by name
- `unity_save_scene` - Save current scene
- `unity_add_scene_to_build` - Register scene in build settings

### Camera Tools
- `unity_set_camera_background` - Set camera clear flags and color

---

## Bullet Visual Effects

### Creating Better Projectiles

**Problem:** Default cube bullets are too large, slow, and visually boring.

**Solution:**
1. Create small sphere primitive (scale 0.3)
2. Increase bullet speed significantly
3. Add particle effects for visual flair

**Implementation:**
```csharp
// Created unity_create_primitive tool for flexible primitive creation
GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
sphere.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
```

**Bullet Speed Tuning:**
- Original: 10 units/sec (too slow, hard to track enemies)
- Updated v1: 25 units/sec (better, but still not snappy)
- Updated v2: 50 units/sec (fast, but needed more)
- Updated v3: 100 units/sec (very fast, but particles hard to see)
- Updated v4: 200 units/sec (insanely fast, but trail too thin)
- **Final: 400 units/sec** (LUDICROUS SPEED! Near-instant across entire arena! 🚀💨⚡🔥)
- General rule: Bullet speed should be 20-40x faster than enemy move speed for maximum arcade feel

**Particle Trail Implementation:**
Created `unity_add_particle_trail` tool for automated particle effect setup.

**Key Configuration for ULTRA-THICK Continuous Speed Trail:**
- **Simulation Space:** World (critical! Particles stay behind as bullet moves forward)
- **Start Speed:** 0 (particles don't move relative to emission point)
- **Start Lifetime:** 1.5 seconds (extra long persistent trail)
- **Emission Rate:** 500 particles/sec (INSANE density - continuous thick stream)
- **Start Size:** 1.0 (MASSIVE particles - 20x larger than original!)
- **Color:** Cyan (bright, high-contrast, impossible to miss)
- **Size Over Lifetime:** Fade from 100% → 0% (taper effect)
- **Color Over Lifetime:** Alpha from 100% → 0% (smooth fade out)
- **Max Particles:** 1000 (high limit for ultra-fast bullets)
- **Play On Awake:** true (starts immediately when bullet spawns)
- **Stop Action:** None (particles persist even after bullet destroyed)

**Why World Space is Critical:**
- `SimulationSpace.Local`: Particles move with bullet → no trail
- `SimulationSpace.World`: Particles stay where emitted → visible trail ✓

**Visual Effect Result:**
LUDICROUSLY FAST bullets (400 units/sec) leave ULTRA-THICK bright cyan particle trails that:
- Create MASSIVE, continuous trails that paint across the entire arena
- Emphasize instantaneous projectile speed (40x faster than original!)
- Particles are HUGE (1.0 size - 20x larger!) and last 1.5 seconds
- 500 particles/sec creates solid, unbroken laser-beam effect
- Continuous emission - particles never stop coming from bullet
- Cyan color provides maximum contrast against any background
- Looks like shooting thick cyan laser beams or energy weapons

**Final Bullet Configuration:**
- Speed: 400 units/sec (40x faster than original!)
- Trail Color: Bright cyan (maximum contrast)
- Emission Rate: 500 particles/sec (insane density - solid stream)
- Particle Size: 1.0 (MASSIVE - 20x larger than original)
- Trail Duration: 1.5 seconds (extra long persistence)
- Max Particles: 1000 (supports ultra-high emission rate)
- Play On Awake: true (instant emission)
- Stop Action: None (continuous trail)
- Result: Instant projectiles with thick, continuous cyan laser beam trails

---

## Memory Aid

**Standard UI Sprite:** GUI_12 (9-sliceable) from 2D Casual UI package

