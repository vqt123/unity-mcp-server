using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Math;
#if UNITY_EDITOR
using UnityEditor;
#endif
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Creates and manages visual GameObjects for simulation entities
    /// </summary>
    public class EntityVisualizer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject projectilePrefab;
        
        [Header("Projectile FX Prefabs")]
        [Tooltip("Default projectile FX - used for Bow, Sword, and other default weapons")]
        [SerializeField] private GameObject projectileFXDefault;
        [Tooltip("Fireball FX - used for Firewand weapon")]
        [SerializeField] private GameObject projectileFXFireball;
        
        private Dictionary<EntityId, GameObject> entityViews = new Dictionary<EntityId, GameObject>();
        private Dictionary<EntityId, GameObject> projectileParticleEmitters = new Dictionary<EntityId, GameObject>(); // Track particle emitters for projectiles
        private Dictionary<EntityId, Animator> heroAnimators = new Dictionary<EntityId, Animator>(); // Track animators for heroes
        private Dictionary<EntityId, float> heroLastShootTime = new Dictionary<EntityId, float>(); // Track when heroes last shot (for fire animation)
        
        // Position buffer for interpolation - we need TWO positions:
        // - previousTickPos: position from tick N-1
        // - currentTickPos: position from tick N
        // We interpolate between these two positions throughout tick N
        private struct PositionBuffer
        {
            public Vector3 previousTickPos; // Position from tick N-1
            public Vector3 currentTickPos;  // Position from tick N
            public int previousTick;        // The tick number for previousTickPos
            public int currentTick;         // The tick number for currentTickPos
        }
        
        private Dictionary<EntityId, PositionBuffer> entityPositionBuffers = new Dictionary<EntityId, PositionBuffer>();
        
        // Public setters for GameBootstrapper
        public void SetPrefabs(GameObject hero, GameObject enemy, GameObject projectile)
        {
            heroPrefab = hero;
            enemyPrefab = enemy;
            projectilePrefab = projectile;
            Debug.Log($"[EntityVisualizer] Prefabs set - Hero:{hero!=null}, Enemy:{enemy!=null}, Proj:{projectile!=null}");
        }
        
        /// <summary>
        /// Sets the ProjectileFX prefab references
        /// </summary>
        public void SetProjectileFXPrefabs(GameObject defaultFX, GameObject fireballFX)
        {
            projectileFXDefault = defaultFX;
            projectileFXFireball = fireballFX;
            Debug.Log($"[EntityVisualizer] ProjectileFX prefabs set - Default:{defaultFX!=null}, Fireball:{fireballFX!=null}");
        }
        
        /// <summary>
        /// Gets the visual position of an entity by ID (for damage numbers, etc.)
        /// Returns world position if found, otherwise Vector3.zero
        /// </summary>
        public Vector3 GetEntityPosition(EntityId id)
        {
            // Try to get from visual GameObject first (most accurate)
            if (entityViews.TryGetValue(id, out GameObject obj) && obj != null)
            {
                return obj.transform.position;
            }
            
            // Try to get from position buffer (last known position)
            if (entityPositionBuffers.TryGetValue(id, out PositionBuffer buffer))
            {
                return buffer.currentTickPos;
            }
            
            return Vector3.zero;
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<HeroSpawnedEvent>(OnEvent);
            EventBus.Subscribe<EnemySpawnedEvent>(OnEvent);
            EventBus.Subscribe<ProjectileSpawnedEvent>(OnEvent);
            EventBus.Subscribe<HeroKilledEvent>(OnEvent);
            EventBus.Subscribe<EnemyKilledEvent>(OnEvent);
            EventBus.Subscribe<ProjectileDestroyedEvent>(OnEvent);
            EventBus.Subscribe<EntityMovedEvent>(OnEvent);
            EventBus.Subscribe<HeroShootEvent>(OnEvent);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<HeroSpawnedEvent>(OnEvent);
            EventBus.Unsubscribe<EnemySpawnedEvent>(OnEvent);
            EventBus.Unsubscribe<ProjectileSpawnedEvent>(OnEvent);
            EventBus.Unsubscribe<HeroKilledEvent>(OnEvent);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEvent);
            EventBus.Unsubscribe<ProjectileDestroyedEvent>(OnEvent);
            EventBus.Unsubscribe<EntityMovedEvent>(OnEvent);
            EventBus.Unsubscribe<HeroShootEvent>(OnEvent);
        }
        
        void LateUpdate()
        {
            // Sync positions from simulation with interpolation
            if (GameSimulation.Instance != null)
            {
                SyncPositions(GameSimulation.Instance.Simulation.World);
            }
        }
        
        private void OnEvent(ISimulationEvent evt)
        {
            switch (evt)
            {
                case HeroSpawnedEvent heroSpawn:
                    CreateHeroView(heroSpawn);
                    break;
                case EnemySpawnedEvent enemySpawn:
                    CreateEnemyView(enemySpawn);
                    break;
                case ProjectileSpawnedEvent projSpawn:
                    CreateProjectileView(projSpawn);
                    break;
                case HeroKilledEvent heroKill:
                    DestroyEntityView(heroKill.HeroId);
                    break;
                case EnemyKilledEvent enemyKill:
                    DestroyEntityView(enemyKill.EnemyId);
                    break;
                case ProjectileDestroyedEvent projDestroy:
                    DestroyEntityView(projDestroy.ProjectileId);
                    break;
                case EntityMovedEvent moved:
                    UpdateHeroAnimation(moved);
                    break;
                case HeroShootEvent shoot:
                    OnHeroShoot(shoot);
                    break;
            }
        }
        
        private void CreateHeroView(HeroSpawnedEvent evt)
        {
            Debug.Log($"[animtest] ========== CreateHeroView START for {evt.HeroType} ==========");
            
            // Load global game settings
            GlobalGameSettings globalSettings = Resources.Load<GlobalGameSettings>("GlobalGameSettings");
            Debug.Log($"[animtest] GlobalGameSettings loaded: {(globalSettings != null ? "YES" : "NO")}");
            
            if (globalSettings != null)
            {
                Debug.Log($"[animtest] GlobalGameSettings.defaultHeroModel: {(globalSettings.defaultHeroModel != null ? globalSettings.defaultHeroModel.name : "NULL")}");
                Debug.Log($"[animtest] GlobalGameSettings.heroIdleAnimation: {(globalSettings.heroIdleAnimation != null ? globalSettings.heroIdleAnimation.name : "NULL")}");
                Debug.Log($"[animtest] GlobalGameSettings.heroWalkAnimation: {(globalSettings.heroWalkAnimation != null ? globalSettings.heroWalkAnimation.name : "NULL")}");
                Debug.Log($"[animtest] GlobalGameSettings.heroFireAnimation: {(globalSettings.heroFireAnimation != null ? globalSettings.heroFireAnimation.name : "NULL")}");
                Debug.Log($"[animtest] IsHeroSettingsValid: {globalSettings.IsHeroSettingsValid()}");
            }
            
            GameObject heroPrefabToUse = null;
            
            // Priority 1: Use global settings if available
            if (globalSettings != null && globalSettings.defaultHeroModel != null)
            {
                heroPrefabToUse = globalSettings.defaultHeroModel;
                Debug.Log($"[animtest] ✓ Using GlobalGameSettings.defaultHeroModel: {heroPrefabToUse.name}");
            }
            // Priority 2: Try to get hero prefab from HeroConfigDatabase
            else
            {
                heroPrefabToUse = GetHeroPrefabForType(evt.HeroType);
                if (heroPrefabToUse != null)
                {
                    Debug.Log($"[animtest] Using HeroConfigDatabase prefab: {heroPrefabToUse.name}");
                }
                else
                {
                    Debug.Log($"[animtest] No prefab found in HeroConfigDatabase for {evt.HeroType}");
                }
            }
            
            // Priority 3: Fallback to default hero prefab if config not found
            if (heroPrefabToUse == null)
            {
                heroPrefabToUse = heroPrefab;
                if (heroPrefabToUse != null)
                {
                    Debug.Log($"[animtest] Using fallback heroPrefab: {heroPrefabToUse.name}");
                }
                else
                {
                    Debug.Log($"[animtest] heroPrefab field is null");
                }
            }
            
            // Priority 4: Final fallback: try to load from Resources
            if (heroPrefabToUse == null)
            {
                heroPrefabToUse = Resources.Load<GameObject>("Hero");
                if (heroPrefabToUse != null)
                {
                    Debug.Log($"[animtest] Loaded Hero prefab from Resources");
                }
                else
                {
                    Debug.Log($"[animtest] No Hero prefab found in Resources");
                }
            }
            
            if (heroPrefabToUse == null)
            {
                Debug.LogError($"[animtest] ❌ Hero prefab is null for type '{evt.HeroType}'! Cannot create hero view.");
                return;
            }
            
            Debug.Log($"[animtest] Instantiating hero prefab: {heroPrefabToUse.name}");
            Vector3 initialPos = ToVector3(evt.Position);
            GameObject obj = Instantiate(heroPrefabToUse, initialPos, Quaternion.identity);
            obj.name = $"Hero_{evt.HeroId.Value}_{evt.HeroType}";
            Debug.Log($"[animtest] Hero GameObject created: {obj.name} at position {initialPos}");
            
            // Setup Animator with global settings
            SetupHeroAnimator(obj, globalSettings);
            
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
            
            // Store animator reference
            Animator animator = obj.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = obj.GetComponent<Animator>();
            }
            if (animator != null)
            {
                heroAnimators[evt.HeroId] = animator;
            }
        }
        
        private void SetupHeroAnimator(GameObject heroObj, GlobalGameSettings globalSettings)
        {
            Debug.Log($"[animtest] ========== SetupHeroAnimator START for {heroObj.name} ==========");
            
            Animator animator = heroObj.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = heroObj.GetComponent<Animator>();
                Debug.Log($"[animtest] Found Animator on root: {(animator != null ? "YES" : "NO")}");
            }
            else
            {
                Debug.Log($"[animtest] Found Animator in children: {animator.name}");
            }
            
            // If no animator exists, add one
            if (animator == null)
            {
                animator = heroObj.AddComponent<Animator>();
                Debug.Log($"[animtest] Added Animator component to {heroObj.name}");
            }
            
            animator.enabled = true;
            animator.updateMode = AnimatorUpdateMode.Normal;
            Debug.Log($"[animtest] Animator enabled: {animator.enabled}, updateMode: {animator.updateMode}");
            
            // Use global settings if available
            if (globalSettings != null && globalSettings.IsHeroSettingsValid())
            {
                Debug.Log($"[animtest] ✓ GlobalGameSettings is valid, setting up animator...");
                
                // Get avatar from the model
                Avatar avatar = null;
                if (globalSettings.defaultHeroModel != null)
                {
                    Debug.Log($"[animtest] Looking for avatar in model: {globalSettings.defaultHeroModel.name}");
                    
                    // Try to get avatar from the model's Animator if it has one
                    Animator modelAnimator = globalSettings.defaultHeroModel.GetComponent<Animator>();
                    if (modelAnimator != null && modelAnimator.avatar != null)
                    {
                        avatar = modelAnimator.avatar;
                        Debug.Log($"[animtest] Found avatar on model's Animator: {avatar.name}, valid: {avatar.isValid}");
                    }
                    else
                    {
                        Debug.Log($"[animtest] Model has no Animator or avatar, trying to extract from FBX asset...");
                        // Try to extract avatar from FBX asset
                        #if UNITY_EDITOR
                        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(globalSettings.defaultHeroModel);
                        Debug.Log($"[animtest] FBX asset path: {assetPath}");
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            // Try to get avatar from ModelImporter first
                            UnityEditor.ModelImporter importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.ModelImporter;
                            if (importer != null)
                            {
                                Debug.Log($"[animtest] ModelImporter found, animationType: {importer.animationType}");
                                
                                // For Generic animation type, we don't need an avatar - animations use the model's actual bone structure
                                if (importer.animationType == UnityEditor.ModelImporterAnimationType.Generic)
                                {
                                    Debug.Log($"[animtest] FBX is Generic animation type - no avatar needed, animations use model's bone structure directly");
                                    // Generic animations don't require an avatar, so we can proceed without one
                                    avatar = null; // Explicitly set to null for Generic
                                }
                                else if (importer.animationType == UnityEditor.ModelImporterAnimationType.Human)
                                {
                                    // Try to load avatar for Humanoid
                                    Debug.Log($"[animtest] FBX is Humanoid, trying to get avatar...");
                                    Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
                                    Debug.Log($"[animtest] Found {assets.Length} assets in FBX");
                                    
                                    foreach (Object asset in assets)
                                    {
                                        if (asset is Avatar av && av.isValid)
                                        {
                                            avatar = av;
                                            Debug.Log($"[animtest] ✓ Using avatar: {avatar.name}, valid: {avatar.isValid}");
                                            break;
                                        }
                                    }
                                    
                                    if (avatar == null)
                                    {
                                        Debug.LogError($"[animtest] ❌ No valid avatar found for Humanoid FBX!");
                                    }
                                }
                                else
                                {
                                    Debug.Log($"[animtest] FBX animationType is {importer.animationType} - Legacy/None, no avatar needed");
                                    avatar = null;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[animtest] ⚠️ Could not get ModelImporter for {assetPath}");
                            }
                        }
                        #endif
                    }
                }
                
                // For Generic animation type, avatar is optional (animations use model's bone structure)
                // For Humanoid, avatar is required
                if (avatar != null)
                {
                    animator.avatar = avatar;
                    Debug.Log($"[animtest] ✓ Assigned avatar to animator: {avatar.name}, valid: {avatar.isValid}");
                }
                else
                {
                    #if UNITY_EDITOR
                    UnityEditor.ModelImporter importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(globalSettings.defaultHeroModel)) as UnityEditor.ModelImporter;
                    if (importer != null && importer.animationType == UnityEditor.ModelImporterAnimationType.Generic)
                    {
                        Debug.Log($"[animtest] ✓ Generic animation type - no avatar needed, animations will use model's bone structure");
                    }
                    else
                    {
                        Debug.LogWarning($"[animtest] ⚠️ No avatar found or assigned! Animations may not work.");
                    }
                    #else
                    Debug.LogWarning($"[animtest] ⚠️ No avatar found or assigned!");
                    #endif
                }
                
                // Create runtime animator controller if we have animations
                if (globalSettings.heroIdleAnimation != null || globalSettings.heroWalkAnimation != null || globalSettings.heroFireAnimation != null)
                {
                    Debug.Log($"[animtest] Creating animator controller with animations...");
                    RuntimeAnimatorController controller = CreateHeroAnimatorController(globalSettings);
                    if (controller != null)
                    {
                        animator.runtimeAnimatorController = controller;
                        Debug.Log($"[animtest] ✓ Assigned controller to animator: {controller.name}");
                        
                        // Play idle animation and ensure it loops
                        if (globalSettings.heroIdleAnimation != null)
                        {
                            // Ensure idle animation is set to loop
                            #if UNITY_EDITOR
                            AnimationClipSettings clipSettings = UnityEditor.AnimationUtility.GetAnimationClipSettings(globalSettings.heroIdleAnimation);
                            if (!clipSettings.loopTime)
                            {
                                clipSettings.loopTime = true;
                                UnityEditor.AnimationUtility.SetAnimationClipSettings(globalSettings.heroIdleAnimation, clipSettings);
                                Debug.Log($"[animtest] Set {globalSettings.heroIdleAnimation.name} to loop");
                            }
                            #endif
                            
                            // Ensure animator is enabled and updating
                            animator.enabled = true;
                            animator.updateMode = AnimatorUpdateMode.Normal;
                            
                            // Play the animation
                            animator.Play("Idle", 0, 0f);
                            animator.Update(0f); // Force immediate update
                            
                            // Verify the state is playing - comprehensive diagnostics
                            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                            
                            // Check if we can get the actual clip from the override controller
                            AnimatorOverrideController overrideCtrl = controller as AnimatorOverrideController;
                            AnimationClip actualClip = null;
                            if (overrideCtrl != null)
                            {
                                var clips = overrideCtrl.clips;
                                foreach (var clipPair in clips)
                                {
                                    if (clipPair.originalClip != null && clipPair.originalClip.name == "Idle")
                                    {
                                        actualClip = clipPair.overrideClip;
                                        break;
                                    }
                                }
                            }
                            
                            Debug.Log($"[animtest] ========== ANIMATION DIAGNOSTICS ==========");
                            Debug.Log($"[animtest] Animator enabled: {animator.enabled}");
                            Debug.Log($"[animtest] Animator has controller: {animator.runtimeAnimatorController != null}");
                            Debug.Log($"[animtest] Animator has avatar: {animator.avatar != null}, valid: {(animator.avatar != null ? animator.avatar.isValid : false)}");
                            Debug.Log($"[animtest] Current state name: {(stateInfo.IsName("Idle") ? "Idle" : "NOT Idle")}");
                            Debug.Log($"[animtest] State normalizedTime: {stateInfo.normalizedTime}");
                            Debug.Log($"[animtest] State length: {stateInfo.length}");
                            Debug.Log($"[animtest] State speed: {stateInfo.speed}");
                            Debug.Log($"[animtest] State loop: {stateInfo.loop}");
                            Debug.Log($"[animtest] Assigned clip name: {globalSettings.heroIdleAnimation.name}");
                            Debug.Log($"[animtest] Assigned clip length: {globalSettings.heroIdleAnimation.length}");
                            Debug.Log($"[animtest] Assigned clip frameRate: {globalSettings.heroIdleAnimation.frameRate}");
                            Debug.Log($"[animtest] Actual clip from controller: {(actualClip != null ? actualClip.name : "NULL")}");
                            if (actualClip != null)
                            {
                                Debug.Log($"[animtest] Actual clip length: {actualClip.length}, loopTime: {actualClip.isLooping}");
                            }
                            Debug.Log($"[animtest] ==========================================");
                        }
                        else
                        {
                            Debug.LogWarning($"[animtest] ⚠️ No idle animation to play!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[animtest] ❌ Failed to create animator controller!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[animtest] ⚠️ No animations assigned in GlobalGameSettings!");
                }
            }
            else
            {
                Debug.LogWarning($"[animtest] ⚠️ GlobalGameSettings is null or invalid!");
                if (globalSettings == null)
                {
                    Debug.LogWarning($"[animtest] GlobalGameSettings is NULL - make sure it's in Assets/Resources/GlobalGameSettings.asset");
                }
                else
                {
                    Debug.LogWarning($"[animtest] GlobalGameSettings exists but IsHeroSettingsValid() returned false");
                }
                
                // Fallback: use existing animator setup
                if (animator.runtimeAnimatorController != null)
                {
                    Debug.Log($"[animtest] Using existing controller: {animator.runtimeAnimatorController.name}");
                    if (animator.avatar != null && animator.avatar.isValid)
                    {
                        Debug.Log($"[animtest] Avatar is valid: {animator.avatar.name}");
                        if (globalSettings != null && globalSettings.heroIdleAnimation != null)
                        {
                            animator.Play("Idle", 0, 0f);
                            Debug.Log($"[animtest] Started Idle animation");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[animtest] ⚠️ Animator has no valid avatar!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[animtest] ❌ Animator has no controller assigned and GlobalGameSettings not available!");
                }
            }
            
            Debug.Log($"[animtest] ========== SetupHeroAnimator END ==========");
        }
        
        private RuntimeAnimatorController CreateHeroAnimatorController(GlobalGameSettings settings)
        {
            Debug.Log($"[animtest] ========== CreateHeroAnimatorController START ==========");
            
            // Try to load a base controller from Resources, or use a simple one
            // For now, we'll create an AnimatorOverrideController if we have a base controller
            RuntimeAnimatorController baseController = Resources.Load<RuntimeAnimatorController>("HeroBaseController");
            Debug.Log($"[animtest] Base controller loaded: {(baseController != null ? baseController.name : "NULL")}");
            
            if (baseController == null)
            {
                // No base controller - animations won't work without one
                // The user should either:
                // 1. Create a simple Animator Controller with Idle/Walk/Fire states and save it to Resources/HeroBaseController.controller
                // 2. Or set up the controller on the FBX model directly
                Debug.LogWarning("[animtest] ❌ No base Animator Controller found. Please create Resources/HeroBaseController.controller with Idle, Walk, and Fire states, or set up the controller on the FBX model.");
                return null;
            }
            
            // Create an override controller with the animations from global settings
            AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);
            Debug.Log($"[animtest] Created AnimatorOverrideController");
            
            // Check and log loop settings for each animation
            if (settings.heroIdleAnimation != null)
            {
                #if UNITY_EDITOR
                AnimationClipSettings clipSettings = UnityEditor.AnimationUtility.GetAnimationClipSettings(settings.heroIdleAnimation);
                Debug.Log($"[animtest] Idle clip loopTime: {clipSettings.loopTime}, wrapMode: {settings.heroIdleAnimation.wrapMode}");
                if (!clipSettings.loopTime)
                {
                    clipSettings.loopTime = true;
                    UnityEditor.AnimationUtility.SetAnimationClipSettings(settings.heroIdleAnimation, clipSettings);
                    Debug.Log($"[animtest] ✓ Fixed Idle animation loop setting");
                }
                #endif
                overrideController["Idle"] = settings.heroIdleAnimation;
                Debug.Log($"[animtest] ✓ Assigned Idle animation: {settings.heroIdleAnimation.name}");
            }
            else
            {
                Debug.LogWarning($"[animtest] ⚠️ heroIdleAnimation is null!");
            }
            
            if (settings.heroWalkAnimation != null)
            {
                #if UNITY_EDITOR
                AnimationClipSettings clipSettings = UnityEditor.AnimationUtility.GetAnimationClipSettings(settings.heroWalkAnimation);
                Debug.Log($"[animtest] Walk clip loopTime: {clipSettings.loopTime}, wrapMode: {settings.heroWalkAnimation.wrapMode}");
                if (!clipSettings.loopTime)
                {
                    clipSettings.loopTime = true;
                    UnityEditor.AnimationUtility.SetAnimationClipSettings(settings.heroWalkAnimation, clipSettings);
                    Debug.Log($"[animtest] ✓ Fixed Walk animation loop setting");
                }
                #endif
                overrideController["Walk"] = settings.heroWalkAnimation;
                Debug.Log($"[animtest] ✓ Assigned Walk animation: {settings.heroWalkAnimation.name}");
            }
            else
            {
                Debug.Log($"[animtest] heroWalkAnimation is null (optional)");
            }
            
            if (settings.heroFireAnimation != null)
            {
                overrideController["Fire"] = settings.heroFireAnimation;
                Debug.Log($"[animtest] ✓ Assigned Fire animation: {settings.heroFireAnimation.name}");
            }
            else
            {
                Debug.Log($"[animtest] heroFireAnimation is null (optional)");
            }
            
            Debug.Log($"[animtest] ========== CreateHeroAnimatorController END ==========");
            return overrideController;
        }
        
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
        
        private void CreateEnemyView(EnemySpawnedEvent evt)
        {
            // Try to get enemy prefab from EnemyConfigDatabase
            GameObject enemyPrefabToUse = GetEnemyPrefabForType(evt.EnemyType);
            
            // Fallback to default enemy prefab if config not found
            if (enemyPrefabToUse == null)
            {
                enemyPrefabToUse = enemyPrefab;
            }
            
            // Final fallback: try to load from Resources
            if (enemyPrefabToUse == null)
            {
                enemyPrefabToUse = Resources.Load<GameObject>("Enemy");
            }
            
            if (enemyPrefabToUse == null)
            {
                Debug.LogError($"[EntityVisualizer] Enemy prefab is null for type '{evt.EnemyType}'!");
                return;
            }
            
            Vector3 initialPos = ToVector3(evt.Position);
            GameObject obj = Instantiate(enemyPrefabToUse, initialPos, Quaternion.identity);
            obj.name = $"Enemy_{evt.EnemyId.Value}_{evt.EnemyType}";
            
            if (evt.IsBoss) obj.transform.localScale *= 2f;
            else if (evt.IsMiniBoss) obj.transform.localScale *= 1.5f;
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.EnemyId;
            view.IsHero = false;
            
            // Initialize position buffer
            int currentTick = GameSimulation.Instance != null ? GameSimulation.Instance.Simulation.World.CurrentTick : 0;
            entityPositionBuffers[evt.EnemyId] = new PositionBuffer
            {
                previousTickPos = initialPos,
                currentTickPos = initialPos,
                previousTick = currentTick,
                currentTick = currentTick
            };
            
            entityViews[evt.EnemyId] = obj;
        }
        
        private GameObject GetEnemyPrefabForType(string enemyType)
        {
            // Try to get from EnemyConfigDatabase
            if (EnemyConfigDatabase.Instance != null)
            {
                EnemyConfigSO config = EnemyConfigDatabase.Instance.GetEnemyConfig(enemyType);
                if (config != null && config.enemyPrefab != null)
                {
                    return config.enemyPrefab;
                }
            }
            
            return null;
        }
        
        private void CreateProjectileView(ProjectileSpawnedEvent evt)
        {
            // Get weapon type from owner
            string weaponType = GetWeaponTypeFromOwner(evt.OwnerId);
            
            // Try to get projectile prefab from weapon config, fallback to default
            GameObject projectileToUse = GetProjectilePrefabForWeapon(weaponType);
            if (projectileToUse == null)
            {
                projectileToUse = projectilePrefab;
            }
            
            // Fallback: try to load from Resources if still null
            if (projectileToUse == null)
            {
                projectileToUse = Resources.Load<GameObject>("Projectile");
                if (projectileToUse != null)
                {
                    Debug.Log("[EntityVisualizer] Loaded Projectile prefab from Resources");
                }
            }
            
            if (projectileToUse == null)
            {
                Debug.LogError("[EntityVisualizer] Projectile prefab is null! Make sure to assign it in GameBootstrapper or place 'Projectile.prefab' in Assets/Resources/");
                return;
            }
            
            // Rotate projectile to face its direction
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            Quaternion rotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
            
            // Clone projectile prefab
            GameObject obj = Instantiate(projectileToUse, ToVector3(evt.Position), rotation);
            obj.name = $"Projectile_{evt.ProjectileId.Value}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.ProjectileId;
            view.IsHero = false;
            
            // Initialize position buffer
            Vector3 initialPos = ToVector3(evt.Position);
            int currentTick = GameSimulation.Instance != null ? GameSimulation.Instance.Simulation.World.CurrentTick : 0;
            entityPositionBuffers[evt.ProjectileId] = new PositionBuffer
            {
                previousTickPos = initialPos,
                currentTickPos = initialPos,
                previousTick = currentTick,
                currentTick = currentTick
            };
            
            obj.transform.position = initialPos;
            
            // Get ProjectileFX from weapon config or fallback to weapon-based lookup
            GameObject projectileFXTemplate = GetProjectileFXForWeapon(weaponType);
            
            // Clone ProjectileFX from scene and attach it to the projectile for particles
            if (projectileFXTemplate != null)
            {
                GameObject projectileFXObj = Instantiate(projectileFXTemplate, obj.transform.position, obj.transform.rotation);
                projectileFXObj.transform.SetParent(obj.transform); // Parented to projectile - moves with it
                projectileFXObj.name = $"ProjectileFX_{evt.ProjectileId.Value}";
                
                // Find and configure particle systems within ProjectileFX
                ParticleSystem[] particleSystems = projectileFXObj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particles in particleSystems)
                {
                    var main = particles.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World; // Particles stay in world space
                }
                
                // Track ProjectileFX for cleanup when projectile is destroyed
                projectileParticleEmitters[evt.ProjectileId] = projectileFXObj;
            }
            else
            {
                Debug.LogWarning($"[EntityVisualizer] ProjectileFX not found for weapon type '{weaponType}' (projectile {evt.ProjectileId.Value})!");
            }
            
            entityViews[evt.ProjectileId] = obj;
        }
        
        private string GetHeroTypeFromOwner(EntityId ownerId)
        {
            // Look up hero type from simulation world
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(ownerId, out var hero))
                {
                    return hero.HeroType;
                }
            }
            
            // Fallback: check entity views for hero type
            if (entityViews.TryGetValue(ownerId, out GameObject heroObj))
            {
                // Try to get hero type from GameObject name or component
                if (heroObj.name.StartsWith("Hero_"))
                {
                    // Extract hero type from name like "Hero_123_Mage"
                    var parts = heroObj.name.Split('_');
                    if (parts.Length >= 3)
                    {
                        return parts[2]; // Hero type is third part
                    }
                }
            }
            
            return "DefaultHero"; // Default fallback
        }
        
        private string GetWeaponTypeFromOwner(EntityId ownerId)
        {
            // Look up weapon type from simulation world
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(ownerId, out var hero))
                {
                    return hero.WeaponType;
                }
            }
            
            return "Bow"; // Default fallback
        }
        
        private GameObject GetProjectilePrefabForWeapon(string weaponType)
        {
            // Try to get from hero config (weapon properties are now in hero config)
            // Note: We need to find a hero that uses this weapon type
            if (HeroConfigDatabase.Instance != null)
            {
                var allHeroConfigs = HeroConfigDatabase.Instance.GetAllHeroConfigs();
                foreach (var heroConfig in allHeroConfigs)
                {
                    if (heroConfig != null && heroConfig.weaponType == weaponType && heroConfig.projectilePrefab != null)
                    {
                        return heroConfig.projectilePrefab;
                    }
                }
            }
            
            // Fallback to old WeaponConfigDatabase if still available
            if (WeaponConfigDatabase.Instance != null)
            {
                WeaponConfig config = WeaponConfigDatabase.Instance.GetWeaponConfig(weaponType);
                if (config != null && config.projectilePrefab != null)
                {
                    return config.projectilePrefab;
                }
            }
            
            // Fallback to default
            return null;
        }
        
        private GameObject GetProjectileFXForWeapon(string weaponType)
        {
            // Try to get from hero config (weapon properties are now in hero config)
            // Note: We need to find a hero that uses this weapon type
            if (HeroConfigDatabase.Instance != null)
            {
                var allHeroConfigs = HeroConfigDatabase.Instance.GetAllHeroConfigs();
                foreach (var heroConfig in allHeroConfigs)
                {
                    if (heroConfig != null && heroConfig.weaponType == weaponType && heroConfig.projectileFXPrefab != null)
                    {
                        return heroConfig.projectileFXPrefab;
                    }
                }
            }
            
            // Fallback to old WeaponConfigDatabase if still available
            if (WeaponConfigDatabase.Instance != null)
            {
                WeaponConfig config = WeaponConfigDatabase.Instance.GetWeaponConfig(weaponType);
                if (config != null && config.projectileFXPrefab != null)
                {
                    return config.projectileFXPrefab;
                }
            }
            
            // Fallback to old system: Map weapon types to ProjectileFX prefabs
            switch (weaponType)
            {
                case "Firewand":
                    if (projectileFXFireball != null)
                    {
                        return projectileFXFireball;
                    }
                    Debug.LogWarning($"[EntityVisualizer] ProjectileFXFireball prefab not assigned, falling back to default for weapon '{weaponType}'");
                    break;
                case "Bow":
                case "Sword":
                default:
                    // Use default FX
                    break;
            }
            
            // Fallback to default ProjectileFX
            if (projectileFXDefault != null)
            {
                return projectileFXDefault;
            }
            
            // Fallback: try to load from Resources
            GameObject fxFromResources = Resources.Load<GameObject>("ProjectileFX");
            if (fxFromResources != null)
            {
                return fxFromResources;
            }
            
            // Last resort: try to find in scene (backward compatibility)
            GameObject fx = GameObject.Find("ProjectileFX");
            if (fx != null)
            {
                Debug.LogWarning($"[EntityVisualizer] ProjectileFX prefabs not assigned and found '{fx.name}' in scene (consider assigning prefabs in weapon config or GameBootstrapper)");
            }
            else
            {
                Debug.LogError($"[EntityVisualizer] No ProjectileFX prefab assigned and none found in scene for weapon '{weaponType}'!");
            }
            
            return fx;
        }
        
        private void UpdateHeroAnimation(EntityMovedEvent evt)
        {
            // Delegate to the velocity-based update method
            UpdateHeroAnimationFromVelocity(evt.EntityId, evt.Velocity);
        }
        
        private void UpdateHeroAnimationFromVelocity(EntityId heroId, FixV2 velocity)
        {
            // Only update animation for heroes
            if (!heroAnimators.TryGetValue(heroId, out Animator animator) || animator == null)
                return;
            
            // Check if hero just shot (fire animation should play for a short time)
            bool isFiring = false;
            if (heroLastShootTime.TryGetValue(heroId, out float lastShootTime))
            {
                float timeSinceShoot = Time.time - lastShootTime;
                isFiring = timeSinceShoot < 0.5f; // Fire animation plays for 0.5 seconds
            }
            
            // Determine which animation to play
            string targetState = "Idle";
            if (isFiring)
            {
                targetState = "Fire";
            }
            
            // Only change state if not already in the target state
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            string currentStateName = GetStateNameFromStateInfo(currentState);
            
            if (currentStateName != targetState)
            {
                animator.Play(targetState, 0, 0f);
                Debug.Log($"[animtest] Hero {heroId.Value} animation: {targetState} (firing: {isFiring})");
            }
        }
        
        private string GetStateNameFromStateInfo(AnimatorStateInfo stateInfo)
        {
            // Get the state name from the full path (e.g., "Base Layer.Idle" -> "Idle")
            string fullPath = stateInfo.fullPathHash.ToString();
            
            // Try to get state name from animator
            if (stateInfo.IsName("Idle"))
                return "Idle";
            if (stateInfo.IsName("Walk"))
                return "Walk";
            if (stateInfo.IsName("Fire"))
                return "Fire";
            
            // Fallback: check normalized time to determine if we're in a looping state
            // If normalizedTime > 1, we've looped at least once
            return "Unknown";
        }
        
        private void OnHeroShoot(HeroShootEvent evt)
        {
            // Record shoot time for fire animation
            heroLastShootTime[evt.HeroId] = Time.time;
            
            // Immediately play fire animation
            if (heroAnimators.TryGetValue(evt.HeroId, out Animator animator) && animator != null)
            {
                animator.Play("Fire", 0, 0f);
                Debug.Log($"[animtest] Hero {evt.HeroId.Value} shooting - playing Fire animation");
            }
        }
        
        private void DestroyEntityView(EntityId id)
        {
            // Clean up position buffer
            entityPositionBuffers.Remove(id);
            
            // Clean up animator tracking (for heroes)
            heroAnimators.Remove(id);
            heroLastShootTime.Remove(id);
            
            GameObject obj = null;
            if (entityViews.TryGetValue(id, out obj))
            {
                // If this is a projectile, detach particle emitter before destroying projectile
                if (projectileParticleEmitters.TryGetValue(id, out GameObject emitter))
                {
                    // CRITICAL: Unparent emitter FIRST so it stays in world space and won't be destroyed with projectile
                    emitter.transform.SetParent(null);
                    
                    // Find ALL particle systems within ProjectileFX (they're in children like ParticleEmitter)
                    ParticleSystem[] particleSystems = emitter.GetComponentsInChildren<ParticleSystem>();
                    
                    if (particleSystems != null && particleSystems.Length > 0)
                    {
                        float maxLifetime = 5f; // Default fallback
                        
                        // Stop all particle systems and ensure they're in world space
                        foreach (ParticleSystem particles in particleSystems)
                    {
                        // Stop emitting new particles, but keep existing particles alive
                        particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                        
                            // Ensure particles stay in world space
                        var main = particles.main;
                        main.simulationSpace = ParticleSystemSimulationSpace.World;
                        
                            // Track maximum lifetime across all systems
                            float lifetime = main.startLifetime.constantMax;
                            if (lifetime <= 0f) lifetime = main.startLifetime.constant;
                            if (lifetime > maxLifetime) maxLifetime = lifetime;
                        }
                        
                        // Destroy emitter GameObject after all particles have faded
                        Destroy(emitter, maxLifetime + 1f);
                    }
                    else
                    {
                        // No particle systems found, destroy immediately
                        Destroy(emitter);
                    }
                    
                    // Remove from tracking
                    projectileParticleEmitters.Remove(id);
                }
                
                // Now safe to destroy projectile (particles are already unparented)
                Destroy(obj);
                entityViews.Remove(id);
            }
        }
        
        private void SyncPositions(SimulationWorld world)
        {
            // For proper interpolation, we need at least 2 positions: previous tick and current tick
            // The key insight: we render one tick behind the simulation, storing positions as they arrive
            
            int currentTick = world.CurrentTick;
            
            // Calculate interpolation factor based on how far through the current tick we are
            // This tells us where we are between the previous and current tick positions
            float interpolationFactor = 0f;
            if (GameSimulation.Instance != null)
            {
                float tickInterval = GameSimulation.Instance.TickInterval;
                float tickAccumulator = GameSimulation.Instance.TickAccumulator;
                
                // Interpolation factor: 0.0 = at previous tick, 1.0 = at current tick
                interpolationFactor = Mathf.Clamp01(tickAccumulator / tickInterval);
                
            }
            
            // Proper interpolation requires buffering positions correctly:
            // - Stored position = position from previous tick (tick N-1) 
            // - Current simulation position = position from current tick (tick N)
            // - We interpolate between stored (N-1) and current (N)
            // - After interpolation, we update stored to current for next frame
            
            // For proper interpolation, we maintain TWO positions per entity:
            // - previousTickPos: position from tick N-1
            // - currentTickPos: position from tick N
            // When a new tick arrives, previousTickPos moves to what currentTickPos was, and currentTickPos becomes the new position
            
            // Sync and interpolate heroes
            foreach (var heroId in world.HeroIds)
            {
                if (world.TryGetHero(heroId, out var hero) && entityViews.TryGetValue(heroId, out GameObject obj))
                {
                    Vector3 currentSimPos = ToVector3(hero.Position);
                    
                    if (entityPositionBuffers.TryGetValue(heroId, out PositionBuffer buffer))
                    {
                        // Update buffer when tick changes
                        if (currentTick > buffer.currentTick)
                        {
                            // New tick! Move current -> previous, update current
                            buffer.previousTickPos = buffer.currentTickPos;
                            buffer.previousTick = buffer.currentTick;
                            buffer.currentTickPos = currentSimPos;
                            buffer.currentTick = currentTick;
                        }
                        else if (currentTick == buffer.currentTick)
                        {
                            // Same tick, just update current position (in case simulation updated)
                            buffer.currentTickPos = currentSimPos;
                        }
                        
                        // Interpolate between previous and current
                        Vector3 interpolatedPos = Vector3.Lerp(buffer.previousTickPos, buffer.currentTickPos, interpolationFactor);
                        obj.transform.position = interpolatedPos;
                        
                        entityPositionBuffers[heroId] = buffer;
                        
                        // Update animation based on velocity
                        UpdateHeroAnimationFromVelocity(heroId, hero.Velocity);
                    }
                    else
                    {
                        obj.transform.position = currentSimPos;
                        entityPositionBuffers[heroId] = new PositionBuffer
                        {
                            previousTickPos = currentSimPos,
                            currentTickPos = currentSimPos,
                            previousTick = currentTick,
                            currentTick = currentTick
                        };
                    }
                }
            }
            
            // Sync and interpolate enemies
            foreach (var enemyId in world.EnemyIds)
            {
                if (world.TryGetEnemy(enemyId, out var enemy) && entityViews.TryGetValue(enemyId, out GameObject obj))
                {
                    Vector3 currentSimPos = ToVector3(enemy.Position);
                    
                    if (entityPositionBuffers.TryGetValue(enemyId, out PositionBuffer buffer))
                    {
                        if (currentTick > buffer.currentTick)
                        {
                            buffer.previousTickPos = buffer.currentTickPos;
                            buffer.previousTick = buffer.currentTick;
                            buffer.currentTickPos = currentSimPos;
                            buffer.currentTick = currentTick;
                        }
                        else if (currentTick == buffer.currentTick)
                        {
                            buffer.currentTickPos = currentSimPos;
                        }
                        
                        Vector3 interpolatedPos = Vector3.Lerp(buffer.previousTickPos, buffer.currentTickPos, interpolationFactor);
                        obj.transform.position = interpolatedPos;
                        
                        entityPositionBuffers[enemyId] = buffer;
                    }
                    else
                    {
                        obj.transform.position = currentSimPos;
                        entityPositionBuffers[enemyId] = new PositionBuffer
                        {
                            previousTickPos = currentSimPos,
                            currentTickPos = currentSimPos,
                            previousTick = currentTick,
                            currentTick = currentTick
                        };
                    }
                }
            }
            
            // Sync and interpolate projectiles
            foreach (var projId in world.ProjectileIds)
            {
                if (world.TryGetProjectile(projId, out var proj) && entityViews.TryGetValue(projId, out GameObject obj))
                {
                    Vector3 currentSimPos = ToVector3(proj.Position);
                    
                    if (entityPositionBuffers.TryGetValue(projId, out PositionBuffer buffer))
                    {
                        bool tickChanged = currentTick > buffer.currentTick;
                        
                        // Update buffer when tick changes
                        if (tickChanged)
                        {
                            // New tick! Move current -> previous, update current
                            buffer.previousTickPos = buffer.currentTickPos;
                            buffer.previousTick = buffer.currentTick;
                            buffer.currentTickPos = currentSimPos;
                            buffer.currentTick = currentTick;
                            
                        }
                        else if (currentTick == buffer.currentTick)
                        {
                            // Same tick, update current position
                            buffer.currentTickPos = currentSimPos;
                        }
                        
                        // Interpolate between previous (N-1) and current (N)
                        Vector3 interpolatedPos = Vector3.Lerp(buffer.previousTickPos, buffer.currentTickPos, interpolationFactor);
                        obj.transform.position = interpolatedPos;
                        
                        entityPositionBuffers[projId] = buffer;
                    }
                    else
                    {
                        obj.transform.position = currentSimPos;
                        entityPositionBuffers[projId] = new PositionBuffer
                        {
                            previousTickPos = currentSimPos,
                            currentTickPos = currentSimPos,
                            previousTick = currentTick,
                            currentTick = currentTick
                        };
                    }
                }
            }
        }
        
        private Vector3 ToVector3(FixV2 pos)
        {
            return new Vector3((float)pos.X.ToDouble(), 0f, (float)pos.Y.ToDouble());
        }
    }
}


