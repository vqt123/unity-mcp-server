using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Tool to troubleshoot hero animation issues
    /// </summary>
    public static class HeroAnimationTroubleshooter
    {
        [MenuItem("Tools/Debug/Troubleshoot Hero Animations")]
        public static void TroubleshootAnimations()
        {
            Debug.Log("=== TROUBLESHOOTING HERO ANIMATIONS ===\n");
            
            // Check animation library
            CheckAnimationLibrary();
            
            // Check hero prefabs
            CheckHeroPrefabs();
            
            // Check animator controllers
            CheckAnimatorControllers();
            
            Debug.Log("\n=== TROUBLESHOOTING COMPLETE ===");
        }
        
        private static void CheckAnimationLibrary()
        {
            Debug.Log("\n--- ANIMATION LIBRARY CHECK ---");
            
            string animationLibraryPath = "Assets/Characters/AnimationLibrary_Unity_Standard.fbx";
            ModelImporter importer = AssetImporter.GetAtPath(animationLibraryPath) as ModelImporter;
            
            if (importer == null)
            {
                Debug.LogError($"Animation library not found: {animationLibraryPath}");
                return;
            }
            
            Debug.Log($"✓ Animation library found: {animationLibraryPath}");
            Debug.Log($"  - Animation Type: {importer.animationType}");
            Debug.Log($"  - Avatar Setup: {importer.avatarSetup}");
            Debug.Log($"  - Import Animation: {importer.importAnimation}");
            
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                Debug.LogError("  ✗ Animation Type is NOT Humanoid! It should be 'Human'.");
                Debug.Log("  → Fix: Set Animation Type to 'Human' in the FBX import settings");
            }
            else
            {
                Debug.Log("  ✓ Animation Type is Humanoid");
            }
            
            if (!importer.importAnimation)
            {
                Debug.LogError("  ✗ Import Animation is disabled!");
                Debug.Log("  → Fix: Enable 'Import Animation' in the FBX import settings");
            }
            else
            {
                Debug.Log("  ✓ Import Animation is enabled");
            }
            
            // Check for idle animation
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(animationLibraryPath);
            AnimationClip idleClip = null;
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && clip.name.Contains("Idle"))
                {
                    idleClip = clip;
                    break;
                }
            }
            
            if (idleClip == null)
            {
                Debug.LogError("  ✗ No Idle animation found!");
            }
            else
            {
                Debug.Log($"  ✓ Found Idle animation: {idleClip.name}");
                Debug.Log($"    - Length: {idleClip.length}s");
                Debug.Log($"    - Frame Rate: {idleClip.frameRate}");
                
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(idleClip);
                Debug.Log($"    - Loop Time: {settings.loopTime}");
            }
        }
        
        private static void CheckHeroPrefabs()
        {
            Debug.Log("\n--- HERO PREFABS CHECK ---");
            
            string[] heroTypes = { "Archer", "IceArcher", "Mage", "Warrior", "DefaultHero", "FastHero", "TankHero" };
            string prefabDir = "Assets/Resources/HeroConfigs";
            
            foreach (string heroType in heroTypes)
            {
                string prefabPath = $"{prefabDir}/Hero_{heroType}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (prefab == null)
                {
                    Debug.LogWarning($"  ✗ Prefab not found: {prefabPath}");
                    continue;
                }
                
                Debug.Log($"\n  {heroType}:");
                
                // Check Animator component
                Animator animator = prefab.GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogError("    ✗ No Animator component!");
                    Debug.Log("    → Fix: Add Animator component to the prefab");
                    continue;
                }
                else
                {
                    Debug.Log("    ✓ Animator component exists");
                }
                
                // Check if animator is enabled
                if (!animator.enabled)
                {
                    Debug.LogWarning("    ✗ Animator is disabled!");
                    Debug.Log("    → Fix: Enable the Animator component");
                }
                else
                {
                    Debug.Log("    ✓ Animator is enabled");
                }
                
                // Check controller
                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("    ✗ No Animator Controller assigned!");
                    Debug.Log("    → Fix: Assign an Animator Controller to the Animator component");
                }
                else
                {
                    Debug.Log($"    ✓ Controller assigned: {animator.runtimeAnimatorController.name}");
                }
                
                // Check avatar
                if (animator.avatar == null)
                {
                    Debug.LogError("    ✗ No Avatar assigned!");
                    Debug.Log("    → Fix: Avatar is required for Humanoid animations");
                    Debug.Log("    → The avatar should be generated from the character FBX file");
                    
                    // Try to find avatar
                    Avatar avatar = FindAvatarForPrefab(prefab);
                    if (avatar != null)
                    {
                        Debug.Log($"    → Found avatar: {avatar.name}");
                        Debug.Log("    → You can assign it manually in the prefab");
                    }
                    else
                    {
                        Debug.LogError("    → No avatar found! Make sure the character FBX is configured as Humanoid");
                    }
                }
                else
                {
                    Debug.Log($"    ✓ Avatar assigned: {animator.avatar.name}");
                    Debug.Log($"      - Is Valid: {animator.avatar.isValid}");
                    Debug.Log($"      - Is Human: {animator.avatar.isHuman}");
                }
            }
        }
        
        private static void CheckAnimatorControllers()
        {
            Debug.Log("\n--- ANIMATOR CONTROLLERS CHECK ---");
            
            string animatorDir = "Assets/Characters/Animators";
            string[] heroTypes = { "Archer", "IceArcher", "Mage", "Warrior", "DefaultHero", "FastHero", "TankHero" };
            
            foreach (string heroType in heroTypes)
            {
                string controllerPath = $"{animatorDir}/Hero_{heroType}_Controller.controller";
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                
                if (controller == null)
                {
                    Debug.LogWarning($"  ✗ Controller not found: {controllerPath}");
                    continue;
                }
                
                Debug.Log($"\n  {heroType} Controller:");
                
                // Check layers
                if (controller.layers.Length == 0)
                {
                    Debug.LogError("    ✗ No layers in controller!");
                    continue;
                }
                
                AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
                
                // Check states
                if (stateMachine.states.Length == 0)
                {
                    Debug.LogError("    ✗ No states in controller!");
                    Debug.Log("    → Fix: Add an Idle state with an animation clip");
                    continue;
                }
                
                Debug.Log($"    ✓ Controller has {stateMachine.states.Length} state(s)");
                
                // Check for idle state
                bool hasIdleState = false;
                AnimatorState idleState = null;
                
                foreach (ChildAnimatorState childState in stateMachine.states)
                {
                    if (childState.state.name == "Idle")
                    {
                        hasIdleState = true;
                        idleState = childState.state;
                        break;
                    }
                }
                
                if (!hasIdleState)
                {
                    Debug.LogError("    ✗ No 'Idle' state found!");
                    Debug.Log("    → Fix: Create an Idle state in the controller");
                }
                else
                {
                    Debug.Log("    ✓ Idle state exists");
                    
                    if (idleState.motion == null)
                    {
                        Debug.LogError("    ✗ Idle state has no animation clip!");
                        Debug.Log("    → Fix: Assign an animation clip to the Idle state");
                    }
                    else
                    {
                        Debug.Log($"    ✓ Idle state has animation: {idleState.motion.name}");
                    }
                    
                    // Check if it's the default state
                    if (stateMachine.defaultState == idleState)
                    {
                        Debug.Log("    ✓ Idle is the default state");
                    }
                    else
                    {
                        Debug.LogWarning("    ⚠ Idle is not the default state");
                        Debug.Log($"    → Default state is: {stateMachine.defaultState?.name ?? "null"}");
                    }
                }
            }
        }
        
        private static Avatar FindAvatarForPrefab(GameObject prefab)
        {
            // Get dependencies to find source FBX
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            string[] dependencies = AssetDatabase.GetDependencies(prefabPath);
            
            foreach (string depPath in dependencies)
            {
                if (depPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
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
            
            return null;
        }
        
        [MenuItem("Tools/Debug/Fix Hero Animation Setup")]
        public static void FixAnimationSetup()
        {
            Debug.Log("[HeroAnimationTroubleshooter] Attempting to fix animation setup...");
            
            // Re-run the setup
            HeroAnimationSetup.SetupHeroIdleAnimations();
            
            Debug.Log("[HeroAnimationTroubleshooter] Fix complete! Check the console for details.");
        }
    }
}

