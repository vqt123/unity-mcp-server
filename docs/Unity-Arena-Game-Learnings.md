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

---

## Physics and Collision Issues

### Bullets Hitting the Hero Who Fired Them

**Problem:** Bullets immediately collide with the hero, dealing damage or destroying themselves.

**Root Cause:** Bullets spawn at hero's position with a Collider, causing instant collision.

**Solution:** Use `Physics.IgnoreCollision` in bullet initialization:

```csharp
public void Initialize(Vector3 dir, float spd, float dmg, string tag, GameObject shooter)
{
    // ... other initialization ...
    
    // Ignore collision with shooter
    if (shooter != null)
    {
        Collider shooterCollider = shooter.GetComponent<Collider>();
        Collider bulletCollider = GetComponent<Collider>();
        if (shooterCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider);
        }
    }
}
```

**Key Points:**
- Pass shooter GameObject reference to bullet during instantiation
- Call `Physics.IgnoreCollision` immediately after bullet creation
- Works for both trigger and non-trigger colliders

---

### Rigidbody Required for Non-Trigger Collisions

**Problem:** Bullets with `isTrigger = false` pass through enemies without detecting collisions.

**Root Cause:** At least ONE of the colliding objects needs a Rigidbody for physics collision detection.

**Solution:** Add Rigidbody to bullet dynamically:

```csharp
// Add Rigidbody for physics-based movement and collision
rb = gameObject.AddComponent<Rigidbody>();
rb.useGravity = false;
rb.isKinematic = false;
rb.linearVelocity = direction * speed; // Unity 6+
```

**Why Dynamic Addition:**
- Rigidbody on prefab persists in scene
- Dynamic addition ensures clean prefabs
- Allows velocity-based movement

**Collision Matrix:**
| Bullet | Enemy | Collides? |
|--------|-------|-----------|
| Trigger only | Trigger only | ✓ (OnTriggerEnter) |
| Rigidbody + Collider | Trigger | ✓ (OnTriggerEnter) |
| Rigidbody + Collider | Rigidbody + Collider | ✓ (OnCollisionEnter) |
| Collider only | Collider only | ✗ (No detection) |

---

### Unity 6 API Breaking Changes

**Problem:** Unity crashes with "Some of this projects source files refer to API that has changed" popup when bullets are created.

**Root Causes:**

1. **`Rigidbody.velocity` deprecated** → Use `Rigidbody.linearVelocity`
2. **`CollisionDetectionMode` deprecated** → Use default mode

**Fixes:**

```csharp
// OLD (Unity 5.x - Unity 2022)
rb.velocity = direction * speed;
rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

// NEW (Unity 6+)
rb.linearVelocity = direction * speed;
// Don't set collisionDetectionMode - let Unity use default
```

**Critical Warning:** Setting deprecated `CollisionDetectionMode` causes Unity Editor crash loop! Remove all references to:
- `CollisionDetectionMode.Continuous`
- `CollisionDetectionMode.ContinuousDynamic`
- `CollisionDetectionMode.ContinuousSpeculative`

**User will see:**
```
Some of this projects source files refer to API that has changed.
These can be automatically updated.
Assets/Scripts/Bullet.cs
```

**Solution:** Remove ALL `collisionDetectionMode` assignments from code.

---

## Critical MCP Tool Bugs

### unity_set_component_property Not Assigning Prefab References

**Problem:** Tool reports success but prefab references remain NULL in Inspector.

**Symptoms:**
```
[Hero] Started - bulletPrefab: NULL
[EnemySpawner] enemyPrefab: NULL
```

Even though tool returns:
```json
{
  "success": true,
  "gameObject": "Hero",
  "component": "Hero",
  "property": "bulletPrefab"
}
```

**Root Cause:** The tool only handled primitive types (string, int, float, bool) but NOT object references. When receiving:

```json
{
  "value": {
    "type": "reference",
    "path": "Assets/Prefabs/Bullet.prefab"
  }
}
```

The code silently did nothing because `valueToken.Type == JTokenType.Object` was never handled.

**Fix:**

```csharp
if (valueToken.Type == JTokenType.Object)
{
    JObject valueObj = valueToken as JObject;
    string refType = valueObj["type"]?.ToString();
    string refPath = valueObj["path"]?.ToString();
    
    if (refType == "reference" && !string.IsNullOrEmpty(refPath))
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(refPath);
        if (asset != null)
        {
            prop.objectReferenceValue = asset;
            Debug.Log($"[MCP] Loaded asset from '{refPath}': {asset.name}");
        }
        else
        {
            Debug.LogWarning($"[MCP] Could not load asset from path: {refPath}");
        }
    }
}
```

**Also Added:**
- `EditorUtility.SetDirty(component)` - Ensures changes persist
- `EditorSceneManager.MarkSceneDirty(scene)` - Marks scene as modified

**Lesson:** Always check debug logs show actual asset names, not just success messages!

---

## Prefab Creation Workflow

### Problem: No Tool to Create Prefabs Programmatically

**Context:** 
- Bullets and enemies need to be spawned multiple times
- `Instantiate()` requires prefab assets, not scene GameObjects
- Scene GameObjects can't be instantiated

**Solution:** Created `unity_save_prefab` tool.

**Implementation:**

```csharp
private static JObject SaveAsPrefab(JObject args)
{
    string gameObjectName = args["gameObjectName"]?.ToString();
    string prefabPath = args["prefabPath"]?.ToString();
    
    GameObject obj = GameObject.Find(gameObjectName);
    
    // Ensure path starts with Assets/
    if (!prefabPath.StartsWith("Assets/"))
        prefabPath = "Assets/" + prefabPath;
    
    // Ensure .prefab extension
    if (!prefabPath.EndsWith(".prefab"))
        prefabPath += ".prefab";
    
    // Create folder structure
    string directory = Path.GetDirectoryName(prefabPath);
    // ... create folders if needed ...
    
    // Save as prefab
    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
    AssetDatabase.Refresh();
    
    return success;
}
```

**Usage Pattern:**

1. Create template GameObject in scene
2. Configure all components (scripts, colliders, particles)
3. Save as prefab
4. Delete template from scene
5. Assign prefab reference to spawner

**Example:**

```bash
# 1. Create and configure bullet
curl -X POST http://localhost:8765 -d '{"tool": "unity_create_primitive", "args": {"name": "BulletTemplate", "primitiveType": "Sphere", "position": [-100, 0, 0]}}'
curl -X POST http://localhost:8765 -d '{"tool": "unity_set_scale", "args": {"name": "BulletTemplate", "scale": [0.3, 0.3, 0.3]}}'
curl -X POST http://localhost:8765 -d '{"tool": "unity_add_component", "args": {"gameObjectName": "BulletTemplate", "componentType": "Bullet"}}'
curl -X POST http://localhost:8765 -d '{"tool": "unity_add_particle_trail", "args": {"name": "BulletTemplate"}}'

# 2. Save as prefab
curl -X POST http://localhost:8765 -d '{"tool": "unity_save_prefab", "args": {"gameObjectName": "BulletTemplate", "prefabPath": "Prefabs/Bullet.prefab"}}'

# 3. Clean up template
curl -X POST http://localhost:8765 -d '{"tool": "unity_delete_gameobject", "args": {"name": "BulletTemplate"}}'

# 4. Assign to Hero
curl -X POST http://localhost:8765 -d '{"tool": "unity_set_component_property", "args": {"gameObjectName": "Hero", "componentType": "Hero", "propertyName": "bulletPrefab", "value": {"type": "reference", "path": "Assets/Prefabs/Bullet.prefab"}}}'
```

---

### Enemy Spawning Stops After Initial Spawns

**Problem:** Enemies spawn for a few seconds, then stop spawning entirely.

**Symptoms:**
```
[EnemySpawner] Found 1 enemies (max: 20), enemyPrefab: EnemyPrefab
[EnemySpawner] Max enemies reached, not spawning
```

**Root Cause:** The spawner counts all GameObjects with "Enemy" tag, including:
- Template GameObjects in the scene (e.g., "EnemyPrefab")
- These templates never get destroyed
- Spawner thinks max enemies reached

**Solution:**
1. Delete template GameObjects from scene after creating prefabs
2. OR disable template GameObjects (`SetActive(false)`)
3. Only spawn from prefab assets, not scene references

**Correct Pattern:**
```csharp
GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
// This should only count spawned enemies, not templates!

if (enemies.Length >= maxEnemies) return;

GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
// newEnemy gets "Enemy" tag from prefab
```

**Key Insight:** Any GameObject with "Enemy" tag in the scene counts against spawn limit. Clean up templates!

---

## Tools Created for Arena Game (Updated)

### Prefab Management
- `unity_save_prefab` - Save GameObject as prefab asset file ⭐ NEW

### GameObject Creation Tools
- `unity_create_cube` - Create cube primitive (legacy, use create_primitive)
- `unity_create_primitive` - Create any primitive type (Sphere, Capsule, Cylinder, Plane, Quad, Cube)

### Visual Effects Tools
- `unity_add_particle_trail` - Add particle trail effect to GameObject (perfect for projectiles)

### Transform Tools
- `unity_set_position` - Set GameObject position
- `unity_set_rotation` - Set GameObject rotation (euler angles)
- `unity_set_scale` - Set GameObject scale
- `unity_set_tag` - Set/create GameObject tag

### Component Tools
- `unity_add_component` - Add component to GameObject
- `unity_remove_component` - Remove component from GameObject
- `unity_set_component_property` - Set component property/reference (✅ NOW WORKS with prefab references!)

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

## Common Pitfalls (Updated)

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

8. **Bullets hitting the hero who fired them** ⚠️
   - Use `Physics.IgnoreCollision` between bullet and shooter
   - Pass shooter GameObject reference to bullet initialization

9. **Collisions not detected between non-trigger objects** ⚠️
   - At least ONE object needs a Rigidbody for physics collision
   - Add Rigidbody dynamically in bullet initialization

10. **Unity 6 API deprecated causing crashes** 🚨
    - NEVER use `CollisionDetectionMode` - it causes crash loops!
    - Use `rb.linearVelocity` instead of `rb.velocity` (Unity 6+)

11. **Template GameObjects preventing spawning** ⚠️
    - Delete template GameObjects after creating prefabs
    - Templates with "Enemy" tag count against spawn limit

12. **Prefab references not saving** 🚨
    - Was a critical bug in `unity_set_component_property` (NOW FIXED!)
    - Always check debug logs show asset names, not just "success"

---

## Memory Aid

**Standard UI Sprite:** GUI_12 (9-sliceable) from 2D Casual UI package

**Unity 6 Velocity API:** `rb.linearVelocity` (NOT `rb.velocity`)

**Collision Rule:** Need Rigidbody on at least ONE object for non-trigger collision detection

**Prefab Workflow:** Create → Configure → Save → Delete Template → Assign Reference

---

## Particle System Persistence and Serialization Issues 🎯

### The Problem: Particles Never Show Up or Disappear After Changes

**Symptoms:**
- Particles configured correctly in prefabs but not visible in game
- Changes to particle settings don't take effect
- Settings revert to old cached values
- Yellow particles appear as white
- Particle trails invisible or not animating

**Root Cause: Unity's Serialization System**

Unity caches serialized values from prefabs and Inspector settings. When you:
1. Create a prefab with particle settings
2. Edit the prefab YAML manually or via tools
3. Unity loads the cached version, not your changes

This creates a **circular problem** where:
- You set particle color to yellow → Unity loads white
- You set particle size to 1.0 → Unity loads 0.08
- You set simulation space to World → Unity loads Local
- Changes never persist!

---

### The Solution: [System.NonSerialized] + Resources.Load()

**Phase 1: Make ALL Fields Non-Serialized**

```csharp
public class Hero : MonoBehaviour
{
    // OLD: Unity serializes these to prefabs/scenes
    public float bulletSpeed = 20f;
    public GameObject bulletPrefab;
    
    // NEW: Unity never caches these values
    [System.NonSerialized]
    public float bulletSpeed = 20f;
    [System.NonSerialized]
    public GameObject bulletPrefab;
}
```

**Why This Works:**
- `[System.NonSerialized]` tells Unity: "Never save this value to Inspector/prefab"
- Values ONLY come from script defaults (the source of truth)
- No more cached values overriding your changes!

**Phase 2: Load Prefabs from Resources/ Folder**

```csharp
void Start()
{
    // Load bullet prefab at runtime from Resources/ folder
    bulletPrefab = Resources.Load<GameObject>("Bullet");
    
    Debug.Log($"Loaded bulletPrefab: {(bulletPrefab != null ? bulletPrefab.name : "NULL")}");
}
```

**File Structure:**
```
Assets/
├── Resources/           # Special folder - Unity includes in builds
│   ├── Bullet.prefab
│   ├── Enemy.prefab
│   └── BloodEffect.prefab
├── Scripts/
│   ├── Hero.cs         # Loads "Bullet" from Resources
│   ├── Enemy.cs        # Loads "BloodEffect" from Resources
│   └── EnemySpawner.cs # Loads "Enemy" from Resources
```

**Key Insight:**
- Inspector assignments are serialized (cached by Unity)
- `Resources.Load()` loads fresh from disk every time
- Prefab files become the ONLY source of truth
- Script defaults + Prefab files = No more circular issues!

---

### Particle System Configuration for Visible Trails

**Critical Settings:**

1. **Simulation Space: World** (NOT Local)
   - Local: Particles move with bullet → no visible trail
   - World: Particles stay where emitted → visible trail ✓

2. **Color: Yellow (#FFFF00)** (NOT white)
   - White particles blend into backgrounds
   - Yellow provides high contrast

3. **Particle Renderer Mode: Stretched Billboard**
   - Enables velocity-based stretching
   - Creates motion blur effect

4. **Velocity Scale: 0.5** (NOT 0)
   - Controls trail length based on movement speed
   - 0 = no stretching, 0.5 = moderate stretch

5. **Start Size: 1.0** (NOT 0.08)
   - Particles need to be visible!
   - 1.0 is 10-12x larger than default

6. **Emission Rate: 500 particles/sec**
   - Creates continuous, thick trail
   - Lower rates create dotted lines

**Using MCP Tools:**

```bash
# Create bullet with yellow particles
curl -X POST http://localhost:8765 -d '{
  "tool": "unity_create_primitive",
  "args": {
    "primitiveType": "Sphere",
    "name": "Bullet",
    "scale": [0.35, 0.35, 0.35]
  }
}'

# Add yellow particle trail
curl -X POST http://localhost:8765 -d '{
  "tool": "unity_add_particle_trail",
  "args": {
    "name": "Bullet",
    "color": "#FFFF00",
    "startSize": 1.0,
    "emissionRate": 500
  }
}'

# Save as prefab
curl -X POST http://localhost:8765 -d '{
  "tool": "unity_save_prefab",
  "args": {
    "gameObjectName": "Bullet",
    "prefabPath": "Assets/Resources/Bullet.prefab"
  }
}'
```

---

### The Complete Fix: Step-by-Step

**1. Update All Scripts to Use [System.NonSerialized]**

```csharp
// Hero.cs, Enemy.cs, EnemySpawner.cs, etc.
[System.NonSerialized]
public float bulletSpeed = 5f;

[System.NonSerialized]
public GameObject bulletPrefab;
```

**2. Move Prefabs to Resources/ Folder**

```bash
mkdir -p Assets/Resources
mv Assets/Prefabs/*.prefab Assets/Resources/
```

**3. Load Prefabs in Start() Methods**

```csharp
void Start()
{
    // Load from Resources folder (path without "Assets/Resources/" or ".prefab")
    bulletPrefab = Resources.Load<GameObject>("Bullet");
    bloodEffect = Resources.Load<GameObject>("BloodEffect");
}
```

**4. Recreate Prefabs Using MCP Tools**

- Delete old prefabs: `rm Assets/Resources/Bullet.prefab`
- Create fresh with correct particle settings
- Save using `unity_save_prefab`
- Delete template GameObject from scene
- Compile and test

**5. Verify in Logs**

```
[Hero] Loaded bulletPrefab: Bullet
[Enemy] Loaded bloodEffect: BloodEffect
[EnemySpawner] Loaded enemyPrefab: Enemy
```

---

### Why Manual YAML Editing Doesn't Work

**The Problem:**
```bash
# Manually edit prefab YAML
sed -i '' 's/minColor: {r: 1, g: 1, b: 1}/minColor: {r: 1, g: 1, b: 0}/' Bullet.prefab
```

**What Happens:**
1. Unity has cached version in memory
2. Your YAML edit changes disk file
3. Unity may or may not reload the file
4. If Unity reloads, it might overwrite your changes with cached version
5. Circular issue: You edit → Unity overwrites → You edit again → ...

**The Solution:**
- Use MCP tools to recreate prefabs from scratch
- `[System.NonSerialized]` prevents caching
- `Resources.Load()` always loads fresh
- Prefab file is the ONLY source of truth

---

### Particle System Troubleshooting Checklist

If particles aren't showing:

- [ ] Check particle color (should be yellow `#FFFF00`, not white)
- [ ] Check simulation space (should be `World`, not `Local`)
- [ ] Check render mode (should be `Stretched Billboard`, not `Billboard`)
- [ ] Check velocity scale (should be `0.5`, not `0`)
- [ ] Check start size (should be `1.0`, not tiny like `0.08`)
- [ ] Check emission rate (should be `500`, not low like `10`)
- [ ] Check prefab is in `Resources/` folder
- [ ] Check script loads prefab with `Resources.Load()`
- [ ] Check all public fields have `[System.NonSerialized]`
- [ ] Recreate prefab using MCP tools (not manual YAML edits)
- [ ] Compile scripts after changes
- [ ] Delete any template GameObjects from scene

---

### Key Takeaways

1. **Unity's serialization system caches values** - This is the root of all evil!
2. **`[System.NonSerialized]` disables caching** - Values only come from script defaults
3. **`Resources.Load()` loads fresh every time** - No cached Inspector assignments
4. **Prefab files become source of truth** - Edit prefabs, not scene objects
5. **Use MCP tools, not manual YAML edits** - Ensures clean, consistent prefabs
6. **Always recreate prefabs when debugging** - Don't try to patch, start fresh
7. **Verify with debug logs** - Check that assets load with correct names

**The Golden Rule:**
> When particle settings don't stick, the problem is **serialization**, not your settings.
> Fix serialization first, then settings will work!

