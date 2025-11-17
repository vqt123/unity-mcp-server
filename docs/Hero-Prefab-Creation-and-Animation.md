# Hero Prefab Creation and Animation

**Documentation for**: Unity 2D Arena Survivor Game  
**Last Updated**: January 2025  
**Purpose**: Explains how hero prefabs are created, configured, and animated in the game

---

## Overview

Hero prefabs in this project are created from 3D FBX character models, configured as Humanoid rigs, and set up with Animator Controllers for idle animations. The system uses a two-part workflow:

1. **Editor Setup**: Automated tools configure FBX models and create prefabs
2. **Runtime Instantiation**: Event-driven system spawns heroes from prefabs

---

## Table of Contents

1. [Hero Prefab Creation Workflow](#hero-prefab-creation-workflow)
2. [Animation System](#animation-system)
3. [Runtime Instantiation](#runtime-instantiation)
4. [Configuration System](#configuration-system)
5. [Editor Tools](#editor-tools)
6. [Troubleshooting](#troubleshooting)

---

## Hero Prefab Creation Workflow

### Step 1: FBX Character Models

Hero characters are stored as FBX files in `Assets/Characters/FBX/`:

- **Archer** → `Elf.fbx`
- **IceArcher** → `Elf.fbx` (reuses Archer model)
- **Mage** → `Wizard.fbx`
- **Warrior** → `Knight_Male.fbx`
- **DefaultHero** → `Casual_Male.fbx`
- **FastHero** → `Ninja_Male.fbx`
- **TankHero** → `Knight_Golden_Male.fbx`

### Step 2: Configure FBX as Humanoid

FBX files must be configured as **Humanoid** rigs for animations to work:

**Location**: `Assets/Editor/HeroCharacterSetup.cs`

**Menu**: `Tools/Setup/Configure Hero Characters`

This tool:
1. Sets `Animation Type` to `Human` for all hero FBX files
2. Configures `Avatar Setup` to `CreateFromThisModel`
3. Enables mesh optimization
4. Reimports FBX files with new settings

**Key Settings**:
```csharp
importer.animationType = ModelImporterAnimationType.Human;
importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
importer.optimizeMesh = true;
```

### Step 3: Create Prefabs

The same tool creates prefabs from FBX models:

**Output Location**: `Assets/Resources/HeroConfigs/`

**Prefab Names**:
- `Hero_Archer.prefab`
- `Hero_IceArcher.prefab`
- `Hero_Mage.prefab`
- `Hero_Warrior.prefab`
- `Hero_DefaultHero.prefab`
- `Hero_FastHero.prefab`
- `Hero_TankHero.prefab`

**Process**:
1. Instantiates FBX model in a temporary scene context
2. Adds `Animator` component (required for animation)
3. Resets transform to origin
4. Saves as prefab asset

**Code Reference**:
```122:147:Assets/Editor/HeroCharacterSetup.cs
                    if (instance != null)
                    {
                        // Reset transform
                        instance.transform.localPosition = Vector3.zero;
                        instance.transform.localRotation = Quaternion.identity;
                        instance.transform.localScale = Vector3.one;
                        instance.name = mapping.prefabName;
                        
                        // Unparent from temp object
                        instance.transform.SetParent(null);
                        
                        // Ensure Animator component exists
                        if (instance.GetComponent<Animator>() == null)
                        {
                            instance.AddComponent<Animator>();
                        }
                        
                        // Save as prefab
                        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
```

### Step 4: Assign Prefabs to Hero Configs

Each hero type has a `HeroConfigSO` ScriptableObject that references the prefab:

**Location**: `Assets/Resources/HeroConfigs/*_Hero.asset`

**Config Structure**:
```csharp
[CreateAssetMenu(fileName = "NewHeroConfig", menuName = "ArenaGame/Hero Config")]
public class HeroConfigSO : ScriptableObject
{
    public string heroTypeName;        // e.g., "Archer"
    public GameObject heroPrefab;      // Reference to Hero_Archer.prefab
    // ... combat stats, weapon properties, etc.
}
```

The setup tool automatically assigns prefabs to their corresponding configs.

---

## Animation System

### Animation Library

All hero animations come from a shared animation library:

**File**: `Assets/Characters/AnimationLibrary_Unity_Standard.fbx`

This FBX contains common animation clips (Idle, Walk, Run, Attack, etc.) that can be used by all Humanoid characters.

### Animator Controllers

Each hero type has its own Animator Controller:

**Location**: `Assets/Characters/Animators/`

**Controllers**:
- `Hero_Archer_Controller.controller`
- `Hero_IceArcher_Controller.controller`
- `Hero_Mage_Controller.controller`
- `Hero_Warrior_Controller.controller`
- `Hero_DefaultHero_Controller.controller`
- `Hero_FastHero_Controller.controller`
- `Hero_TankHero_Controller.controller`

### Setting Up Idle Animations

**Location**: `Assets/Editor/HeroAnimationSetup.cs`

**Menu**: `Tools/Setup/Setup Hero Idle Animations`

This tool:
1. Finds the "Idle" animation clip from the animation library
2. Creates or loads Animator Controllers for each hero type
3. Adds an "Idle" state to each controller
4. Assigns the idle animation clip to the state
5. Sets the state as the default (entry state)
6. Configures the animation to loop
7. Assigns the controller to the prefab's Animator component

**Key Code**:
```233:268:Assets/Editor/HeroAnimationSetup.cs
        private static void AddIdleAnimationToController(AnimatorController controller, AnimationClip idleClip)
        {
            // Get the default state (usually named "Base Layer" or "Default")
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            
            // Find or create an "Idle" state
            AnimatorState idleState = null;
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.name == "Idle")
                {
                    idleState = childState.state;
                    break;
                }
            }
            
            if (idleState == null)
            {
                // Create idle state - position is set via the return value
                idleState = stateMachine.AddState("Idle", new Vector3(0, 0, 0));
            }
            
            // Assign the idle animation clip
            if (idleClip != null)
            {
                idleState.motion = idleClip;
                
                // Make it loop
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(idleClip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(idleClip, settings);
            }
            
            // Set as default state
            stateMachine.defaultState = idleState;
        }
```

### Avatar Configuration

Humanoid animations require an **Avatar** to map animation bones to the character model. The avatar is automatically generated when the FBX is imported as Humanoid.

**Important**: The prefab's Animator component must have:
- ✅ Animator Controller assigned
- ✅ Avatar assigned (from the source FBX)
- ✅ Component enabled

The setup tool automatically finds and assigns the avatar:
```122:162:Assets/Editor/HeroAnimationSetup.cs
        private static Avatar GetAvatarFromPrefab(GameObject prefab)
        {
            // Try to get avatar from Animator component
            Animator animator = prefab.GetComponent<Animator>();
            if (animator != null && animator.avatar != null)
            {
                return animator.avatar;
            }
            
            // Try to get avatar from the model's import settings
            // Get the source FBX path from the prefab
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (prefabPath != null)
            {
                // For prefabs created from FBX, we need to find the source FBX
                // This is a bit tricky - we'll search for it
                string[] assetPaths = AssetDatabase.GetDependencies(prefabPath);
                foreach (string depPath in assetPaths)
                {
                    if (depPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    {
                        ModelImporter importer = AssetImporter.GetAtPath(depPath) as ModelImporter;
                        if (importer != null && importer.animationType == ModelImporterAnimationType.Human)
                        {
                            // The avatar should be generated automatically
                            // Try to load it from the FBX
                            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(depPath);
                            foreach (Object asset in assets)
                            {
                                if (asset is Avatar avatar)
                                {
                                    return avatar;
                                }
                            }
                        }
                    }
                }
            }
            
            return null;
        }
```

---

## Runtime Instantiation

### Event-Driven Spawning

Heroes are spawned at runtime through an event-driven system:

1. **Simulation** creates a hero entity and emits `HeroSpawnedEvent`
2. **EntityVisualizer** listens for the event and instantiates the prefab
3. **Components** are added to link the GameObject to the entity

### Spawn Flow

**Simulation Side** (`Assets/Shared/Systems/SpawnSystem.cs`):
```13:74:Assets/Shared/Systems/SpawnSystem.cs
        public static EntityId SpawnHero(SimulationWorld world, HeroConfig data, FixV2 position, HeroLevelBonuses? bonuses = null)
        {
            // Apply level bonuses if provided
            HeroLevelBonuses b = bonuses ?? default(HeroLevelBonuses);
            // Always apply stat bonuses if bonuses struct is provided (even if Level is 0)
            // Level 0 is valid for arena spawn, stat bonuses come from persistent hero level
            bool hasBonuses = bonuses.HasValue;
            Fix64 finalMaxHealth = data.MaxHealth + (hasBonuses ? b.HealthBonus : Fix64.Zero);
            Fix64 finalDamage = data.Damage + (hasBonuses ? b.DamageBonus : Fix64.Zero);
            Fix64 finalMoveSpeed = data.MoveSpeed + (hasBonuses ? b.MoveSpeedBonus : Fix64.Zero);
            Fix64 finalAttackSpeed = data.AttackSpeed + (hasBonuses ? b.AttackSpeedBonus : Fix64.Zero);
            int heroLevel = hasBonuses ? b.Level : 0; // Use level from bonuses (always 0 for arena)
            
            // Calculate shot cooldown ticks from attacks per second (using final attack speed)
            Fix64 ticksPerAttack = SimulationConfig.TICKS_PER_SECOND / finalAttackSpeed;
            int cooldownTicks = (int)ticksPerAttack.ToLong();
            
            Hero hero = new Hero
            {
                HeroType = data.HeroType,
                Position = position,
                Velocity = FixV2.Zero,
                Rotation = Fix64.Zero,
                Health = finalMaxHealth,
                MaxHealth = finalMaxHealth,
                MoveSpeed = finalMoveSpeed,
                Damage = finalDamage,
                AttackSpeed = finalAttackSpeed,
                LastShotTick = -cooldownTicks, // Can shoot immediately
                ShotCooldownTicks = cooldownTicks,
                WeaponType = data.WeaponType,
                WeaponTier = data.WeaponTier,
                Stars = data.Stars,
                Level = heroLevel,
                CurrentXP = 0,
                XPToNextLevel = 100,
                IsAlive = true
            };
            
            EntityId id = world.CreateHero(hero);
            System.Diagnostics.Debug.WriteLine($"[SpawnSystem] Hero created with EntityId: {id.Value}, Level: {heroLevel}, Tick: {world.CurrentTick}");
            
            // Generate spawn event
            var spawnEvent = new Events.HeroSpawnedEvent
            {
                Tick = world.CurrentTick,
                HeroId = id,
                HeroType = data.HeroType,
                Position = position,
                MaxHealth = finalMaxHealth,
                MoveSpeed = finalMoveSpeed,
                Damage = finalDamage,
                AttackSpeed = finalAttackSpeed,
                WeaponType = data.WeaponType,
                WeaponTier = data.WeaponTier
            };
            
            world.AddEvent(spawnEvent);
            System.Diagnostics.Debug.WriteLine($"[SpawnSystem] HeroSpawnedEvent added to world event buffer - HeroId: {id.Value}, HeroType: {data.HeroType}");
            
            return id;
        }
```

**Client Side** (`Assets/Scripts/Client/EntityVisualizer.cs`):
```138:181:Assets/Scripts/Client/EntityVisualizer.cs
        private void CreateHeroView(HeroSpawnedEvent evt)
        {
            // Try to get hero prefab from HeroConfigDatabase
            GameObject heroPrefabToUse = GetHeroPrefabForType(evt.HeroType);
            
            // Fallback to default hero prefab if config not found
            if (heroPrefabToUse == null)
            {
                heroPrefabToUse = heroPrefab;
            }
            
            // Final fallback: try to load from Resources
            if (heroPrefabToUse == null)
            {
                heroPrefabToUse = Resources.Load<GameObject>("Hero");
            }
            
            if (heroPrefabToUse == null)
            {
                Debug.LogError($"[EntityVisualizer] Hero prefab is null for type '{evt.HeroType}'!");
                return;
            }
            
            Vector3 initialPos = ToVector3(evt.Position);
            GameObject obj = Instantiate(heroPrefabToUse, initialPos, Quaternion.identity);
            obj.name = $"Hero_{evt.HeroId.Value}_{evt.HeroType}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.HeroId;
            view.IsHero = true;
            
            // Initialize position buffer - both previous and current start at same position
            int currentTick = GameSimulation.Instance != null ? GameSimulation.Instance.Simulation.World.CurrentTick : 0;
            entityPositionBuffers[evt.HeroId] = new PositionBuffer
            {
                previousTickPos = initialPos,
                currentTickPos = initialPos,
                previousTick = currentTick,
                currentTick = currentTick
            };
            
            entityViews[evt.HeroId] = obj;
        }
```

### Prefab Resolution

The system uses a three-tier fallback to find hero prefabs:

1. **HeroConfigDatabase**: Looks up prefab from `HeroConfigSO` asset
2. **EntityVisualizer field**: Uses serialized prefab reference
3. **Resources folder**: Loads `"Hero"` prefab from Resources

**Code**:
```183:196:Assets/Scripts/Client/EntityVisualizer.cs
        private GameObject GetHeroPrefabForType(string heroType)
        {
            // Try to get from HeroConfigDatabase
            if (HeroConfigDatabase.Instance != null)
            {
                HeroConfigSO config = HeroConfigDatabase.Instance.GetHeroConfig(heroType);
                if (config != null && config.heroPrefab != null)
                {
                    return config.heroPrefab;
                }
            }
            
            return null;
        }
```

### Runtime Components

When a hero prefab is instantiated, these components are added:

1. **EntityView**: Links GameObject to simulation entity ID
2. **EntityRotation** (if present on prefab): Rotates character to face movement direction

**EntityView**:
```166:168:Assets/Scripts/Client/EntityVisualizer.cs
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.HeroId;
            view.IsHero = true;
```

**EntityRotation** (from prefab):
```8:48:Assets/Scripts/Client/EntityRotation.cs
    public class EntityRotation : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private bool faceMovementDirection = true;
        
        private EntityView entityView;
        private Vector3 lastPosition;
        private Vector3 targetDirection;
        
        void Start()
        {
            entityView = GetComponent<EntityView>();
            lastPosition = transform.position;
        }
        
        void Update()
        {
            if (entityView == null) return;
            
            if (!faceMovementDirection) return;
            
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - lastPosition;
            
            if (movement.sqrMagnitude > 0.001f)
            {
                targetDirection = movement.normalized;
            }
            
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            
            lastPosition = currentPosition;
        }
    }
```

---

## Configuration System

### HeroConfigSO ScriptableObject

Each hero type has a `HeroConfigSO` asset that defines:

- **Hero Info**: Type name, display name, description
- **Combat Stats**: Health, damage, move speed, attack speed
- **Weapon Properties**: Weapon type, projectile speed, AOE radius, etc.
- **Visual**: Hero color, prefab reference, projectile prefab, FX prefab

**Location**: `Assets/Scripts/Client/HeroConfig.cs`

**Example Config Path**: `Assets/Resources/HeroConfigs/Archer_Hero.asset`

### HeroConfigDatabase

At runtime, all hero configs are loaded from Resources:

**Location**: `Assets/Scripts/Client/HeroConfigDatabase.cs`

**Load Process**:
```33:61:Assets/Scripts/Client/HeroConfigDatabase.cs
        private void LoadHeroConfigs()
        {
            heroConfigs.Clear();
            
            // Load all HeroConfigSO assets from Resources
            HeroConfigSO[] configs = Resources.LoadAll<HeroConfigSO>("HeroConfigs");
            
            foreach (HeroConfigSO config in configs)
            {
                if (config != null && !string.IsNullOrEmpty(config.heroTypeName))
                {
                    // Use hero type name as key (case-insensitive)
                    string key = config.heroTypeName.ToLower();
                    heroConfigs[key] = config;
                    
                    // Validate config
                    if (!config.IsValid())
                    {
                        Debug.LogWarning($"[HeroConfigDatabase] Hero config '{config.heroTypeName}' is missing weapon type!");
                    }
                    else
                    {
                        Debug.Log($"[HeroConfigDatabase] Loaded hero config: {config.heroTypeName} with weapon: {config.GetWeaponType()}");
                    }
                }
            }
            
            Debug.Log($"[HeroConfigDatabase] Loaded {heroConfigs.Count} hero configs");
        }
```

---

## Editor Tools

### Complete Setup Workflow

To set up a new hero type from scratch:

1. **Configure FBX**: `Tools/Setup/Configure Hero Characters`
   - Sets FBX as Humanoid
   - Creates prefab from FBX
   - Assigns prefab to HeroConfigSO

2. **Setup Animations**: `Tools/Setup/Setup Hero Idle Animations`
   - Creates Animator Controller
   - Adds idle animation
   - Assigns controller to prefab

### Troubleshooting Tool

**Location**: `Assets/Editor/HeroAnimationTroubleshooter.cs`

**Menu**: `Tools/Debug/Troubleshoot Hero Animations`

This tool checks:
- ✅ Animation library configuration
- ✅ Hero prefab setup (Animator, Controller, Avatar)
- ✅ Animator Controller states
- ✅ Animation clip assignments

**Menu**: `Tools/Debug/Fix Hero Animation Setup`
- Re-runs the animation setup automatically

### Animation Testing Tool

**Location**: `Assets/Editor/HeroAnimationTester.cs`

**Menu**: `Tools/Test/Hero Animation Tester`

This editor window tool allows you to:
- ✅ Select any hero prefab from the project
- ✅ View all available animations from the animation library
- ✅ Play, pause, stop, and scrub through animations
- ✅ Test animations in an isolated test scene
- ✅ See animation details (length, frame rate, loop settings)

**Usage**:
1. Open `Tools/Test/Hero Animation Tester`
2. Select a hero prefab from the dropdown
3. Click "Load Prefab" to instantiate it in a test scene
4. Browse the list of available animations
5. Click "Play" on any animation or use the playback controls
6. Use the time slider to scrub through animations

**Features**:
- Automatically creates a test scene for preview
- Lists all animations from `AnimationLibrary_Unity_Standard.fbx`
- Creates a temporary Animator Controller with all animations as states
- Auto-assigns avatar if missing
- Real-time playback with play/pause/stop controls
- Time scrubbing for precise frame inspection

---

## Troubleshooting

### Common Issues

#### 1. Animations Not Playing

**Symptoms**: Hero appears but doesn't animate

**Checks**:
- ✅ Animator component exists and is enabled
- ✅ Animator Controller is assigned
- ✅ Avatar is assigned and is valid
- ✅ Animation library is configured as Humanoid

**Fix**: Run `Tools/Debug/Troubleshoot Hero Animations`

#### 2. Avatar Not Found

**Symptoms**: Console error "Avatar is invalid"

**Fix**:
1. Ensure FBX is configured as Humanoid (`Tools/Setup/Configure All FBX as Humanoid`)
2. Reimport the FBX file
3. Manually assign avatar in prefab inspector

#### 3. Prefab Not Found at Runtime

**Symptoms**: "Hero prefab is null" error

**Checks**:
1. Prefab exists in `Assets/Resources/HeroConfigs/`
2. HeroConfigSO has `heroPrefab` field assigned
3. HeroConfigDatabase loaded the config

**Fix**: Run `Tools/Setup/Configure Hero Characters` to re-assign prefabs

#### 4. Animation Library Missing

**Symptoms**: "No idle animation found"

**Fix**:
1. Verify `Assets/Characters/AnimationLibrary_Unity_Standard.fbx` exists
2. Check FBX import settings (Animation Type = Human, Import Animation = true)
3. Reimport the FBX file

#### 5. Testing Animations

**Want to preview all animations on a hero prefab?**

**Use**: `Tools/Test/Hero Animation Tester`

This tool:
- Loads hero prefabs automatically
- Shows all available animations from the library
- Allows real-time playback and testing
- Creates isolated test scenes (no need to enter Play mode)

---

## File Structure Summary

```
Assets/
├── Characters/
│   ├── FBX/                    # Source character models
│   │   ├── Elf.fbx
│   │   ├── Wizard.fbx
│   │   └── ...
│   ├── Animators/              # Animator Controllers
│   │   ├── Hero_Archer_Controller.controller
│   │   └── ...
│   └── AnimationLibrary_Unity_Standard.fbx  # Shared animations
│
├── Resources/
│   └── HeroConfigs/            # Prefabs and Configs
│       ├── Hero_Archer.prefab  # Hero prefabs
│       ├── Archer_Hero.asset   # HeroConfigSO assets
│       └── ...
│
└── Editor/
    ├── HeroCharacterSetup.cs        # FBX → Prefab tool
    ├── HeroAnimationSetup.cs         # Animation setup tool
    ├── HeroAnimationTroubleshooter.cs # Debug tool
    └── HeroAnimationTester.cs        # Animation testing tool
```

---

## Key Components Reference

### Prefab Components
- **Animator**: Required for animation playback
- **Transform**: Position, rotation, scale
- **EntityRotation** (optional): Auto-rotates to face movement

### Runtime Components (Added)
- **EntityView**: Links GameObject to EntityId

### ScriptableObjects
- **HeroConfigSO**: Hero configuration data

### Systems
- **SpawnSystem**: Creates hero entities in simulation
- **EntityVisualizer**: Instantiates prefabs from events
- **HeroConfigDatabase**: Loads and provides hero configs

---

## Animation State Machine

Currently, heroes use a simple animation setup:

- **Default State**: Idle (looping)
- **Animation Source**: `AnimationLibrary_Unity_Standard.fbx`

**Future Enhancement**: Add walk/run/attack animations with state transitions based on:
- Movement speed (idle vs walk vs run)
- Combat state (attack animations)
- Health state (death animations)

---

## Related Documentation

- **Main.md**: Project overview and architecture
- **Client-Server Architecture**: Simulation vs visualization separation
- **EntityVisualizer.cs**: Runtime prefab instantiation
- **SpawnSystem.cs**: Simulation-side hero creation

---

**End of Documentation**

