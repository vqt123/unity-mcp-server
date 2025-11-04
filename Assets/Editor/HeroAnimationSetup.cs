using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Tool to set up idle animations for hero characters
    /// Creates animator controllers and assigns idle animations from FBX files
    /// </summary>
    public static class HeroAnimationSetup
    {
        [MenuItem("Tools/Setup/Setup Hero Idle Animations")]
        public static void SetupHeroIdleAnimations()
        {
            Debug.Log("[HeroAnimationSetup] Setting up idle animations for heroes...");
            
            // Mapping of hero types (we'll use the animation library for all)
            var heroTypes = new[]
            {
                "Archer",
                "IceArcher",
                "Mage",
                "Warrior",
                "DefaultHero",
                "FastHero",
                "TankHero"
            };
            
            // Animation library path
            string animationLibraryPath = "Assets/Characters/AnimationLibrary_Unity_Standard.fbx";
            string animatorDir = "Assets/Characters/Animators";
            string prefabDir = "Assets/Resources/HeroConfigs";
            
            // Create animator directory if needed
            if (!AssetDatabase.IsValidFolder(animatorDir))
            {
                AssetDatabase.CreateFolder("Assets/Characters", "Animators");
            }
            
            // Find idle animation from animation library
            AnimationClip idleClip = FindIdleAnimation(animationLibraryPath);
            
            if (idleClip == null)
            {
                Debug.LogError($"[HeroAnimationSetup] No idle animation found in animation library: {animationLibraryPath}");
                Debug.LogError("[HeroAnimationSetup] Please check the animation library file or run Tools/Debug/Inspect FBX Animations");
                return;
            }
            
            Debug.Log($"[HeroAnimationSetup] Found idle animation: {idleClip.name} (Length: {idleClip.length}s)");
            
            int setupCount = 0;
            
            foreach (string heroType in heroTypes)
            {
                string prefabPath = $"{prefabDir}/Hero_{heroType}.prefab";
                
                // Load the prefab
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogWarning($"[HeroAnimationSetup] Prefab not found: {prefabPath}");
                    continue;
                }
                
                // Check if prefab already has an Animator
                Animator animator = prefab.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = prefab.AddComponent<Animator>();
                    Debug.Log($"[HeroAnimationSetup] Added Animator component to {heroType}");
                }
                
                // Create or load animator controller
                string controllerPath = $"{animatorDir}/Hero_{heroType}_Controller.controller";
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                
                if (controller == null)
                {
                    controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                    Debug.Log($"[HeroAnimationSetup] Created animator controller: Hero_{heroType}_Controller.controller");
                }
                
                // Add idle animation to controller
                AddIdleAnimationToController(controller, idleClip);
                Debug.Log($"[HeroAnimationSetup] Added idle animation '{idleClip.name}' to {heroType} controller");
                
                // Assign controller to prefab's animator
                animator.runtimeAnimatorController = controller;
                
                // Make sure the avatar is set (required for humanoid animations)
                if (animator.avatar == null)
                {
                    // Try to get avatar from the prefab's model
                    Avatar avatar = GetAvatarFromPrefab(prefab);
                    if (avatar != null)
                    {
                        animator.avatar = avatar;
                        Debug.Log($"[HeroAnimationSetup] Set avatar for {heroType}");
                    }
                    else
                    {
                        Debug.LogWarning($"[HeroAnimationSetup] No avatar found for {heroType} - animations may not work!");
                    }
                }
                
                // Mark prefab as dirty
                EditorUtility.SetDirty(prefab);
                
                setupCount++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[HeroAnimationSetup] ✓ Set up idle animations for {setupCount} heroes!");
        }
        
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
        
        private static AnimationClip FindIdleAnimation(string fbxPath)
        {
            // Load all animation clips from the FBX
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            
            AnimationClip idleClip = null;
            
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip)
                {
                    string clipName = clip.name.ToLower();
                    
                    // Remove common prefixes like "CharacterArmature|" or "Armature|"
                    string normalizedName = clipName;
                    if (clipName.Contains("|"))
                    {
                        normalizedName = clipName.Split('|')[1]; // Get part after |
                    }
                    
                    // Look for idle animation (common names: "idle", "Idle", "IDLE", "Idle_01", etc.)
                    if (normalizedName.Contains("idle"))
                    {
                        // Prefer exact "idle" match
                        if (normalizedName == "idle" || normalizedName == "idle_01" || normalizedName == "idle_1")
                        {
                            idleClip = clip;
                            Debug.Log($"[HeroAnimationSetup] Found perfect idle match: {clip.name}");
                            break;
                        }
                        else if (idleClip == null)
                        {
                            // Keep first idle clip found as fallback
                            idleClip = clip;
                            Debug.Log($"[HeroAnimationSetup] Found idle animation: {clip.name}");
                        }
                    }
                }
            }
            
            // If still no idle found, try exact name match with prefix
            if (idleClip == null)
            {
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        // Try exact match with common prefixes
                        string clipName = clip.name;
                        if (clipName == "CharacterArmature|Idle" || 
                            clipName == "Armature|Idle" ||
                            clipName == "Idle")
                        {
                            idleClip = clip;
                            Debug.Log($"[HeroAnimationSetup] Found idle by exact name: {clip.name}");
                            break;
                        }
                    }
                }
            }
            
            if (idleClip == null)
            {
                Debug.LogWarning($"[HeroAnimationSetup] No idle animation found in {fbxPath}");
            }
            
            return idleClip;
        }
        
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
    }
}

