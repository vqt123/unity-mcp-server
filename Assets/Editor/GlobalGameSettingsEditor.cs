using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using ArenaGame.Client;
using System.Collections.Generic;

namespace ArenaGame.Editor
{
    [CustomEditor(typeof(GlobalGameSettings))]
    public class GlobalGameSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GlobalGameSettings settings = (GlobalGameSettings)target;
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Global settings for all entities across all scenes.\n\n" +
                "Assign models and animations here to use as defaults.\n" +
                "This is the central place for all global game parameters.",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Validation
            bool heroValid = settings.IsHeroSettingsValid();
            bool enemyValid = settings.IsEnemySettingsValid();
            
            if (!heroValid)
            {
                EditorGUILayout.HelpBox("⚠️ Hero settings: Please assign Default Hero Model and Hero Idle Animation.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Hero settings are valid!", MessageType.Info);
            }
            
            if (!enemyValid)
            {
                EditorGUILayout.HelpBox("⚠️ Enemy settings: Please assign Default Enemy Model.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Enemy settings are valid!", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Button to create base controller for heroes
            if (GUILayout.Button("Create Hero Base Animator Controller", GUILayout.Height(30)))
            {
                CreateHeroBaseAnimatorController(settings);
            }
            
            // Button to create base controller for enemies
            if (GUILayout.Button("Create Enemy Base Animator Controller", GUILayout.Height(30)))
            {
                CreateEnemyBaseAnimatorController(settings);
            }
            
            EditorGUILayout.Space();
            
            // Button to configure animation loop settings
            if (GUILayout.Button("Configure Animation Loop Settings", GUILayout.Height(30)))
            {
                ConfigureAnimationLoopSettings(settings);
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings);
            }
        }
        
        private void CreateHeroBaseAnimatorController(GlobalGameSettings settings)
        {
            string controllerPath = "Assets/Resources/HeroBaseController.controller";
            
            // Check if controller already exists
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                if (!EditorUtility.DisplayDialog("Controller Exists", 
                    "HeroBaseController.controller already exists. Overwrite it?", 
                    "Yes", "No"))
                {
                    return;
                }
            }
            
            // Create the controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            
            // Create Idle state (default)
            AnimatorState idleState = stateMachine.AddState("Idle");
            if (settings.heroIdleAnimation != null)
            {
                idleState.motion = settings.heroIdleAnimation;
            }
            stateMachine.defaultState = idleState;
            
            // Create Walk state
            AnimatorState walkState = stateMachine.AddState("Walk");
            if (settings.heroWalkAnimation != null)
            {
                walkState.motion = settings.heroWalkAnimation;
            }
            
            // Create Fire state
            AnimatorState fireState = stateMachine.AddState("Fire");
            if (settings.heroFireAnimation != null)
            {
                fireState.motion = settings.heroFireAnimation;
            }
            
            // Set loop settings
            if (settings.heroIdleAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.heroIdleAnimation);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(settings.heroIdleAnimation, clipSettings);
            }
            
            if (settings.heroWalkAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.heroWalkAnimation);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(settings.heroWalkAnimation, clipSettings);
            }
            
            if (settings.heroFireAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.heroFireAnimation);
                clipSettings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(settings.heroFireAnimation, clipSettings);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", 
                $"Created Hero base Animator Controller at:\n{controllerPath}\n\n" +
                "The system will now use this controller with your animations!", 
                "OK");
        }
        
        private void CreateEnemyBaseAnimatorController(GlobalGameSettings settings)
        {
            string controllerPath = "Assets/Resources/EnemyBaseController.controller";
            
            // Check if controller already exists
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                if (!EditorUtility.DisplayDialog("Controller Exists", 
                    "EnemyBaseController.controller already exists. Overwrite it?", 
                    "Yes", "No"))
                {
                    return;
                }
            }
            
            // Create the controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            
            // Create Idle state (default)
            AnimatorState idleState = stateMachine.AddState("Idle");
            if (settings.enemyIdleAnimation != null)
            {
                idleState.motion = settings.enemyIdleAnimation;
            }
            stateMachine.defaultState = idleState;
            
            // Create Walk state
            AnimatorState walkState = stateMachine.AddState("Walk");
            if (settings.enemyWalkAnimation != null)
            {
                walkState.motion = settings.enemyWalkAnimation;
            }
            
            // Create Attack state
            AnimatorState attackState = stateMachine.AddState("Attack");
            if (settings.enemyAttackAnimation != null)
            {
                attackState.motion = settings.enemyAttackAnimation;
            }
            
            // Set loop settings
            if (settings.enemyIdleAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.enemyIdleAnimation);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(settings.enemyIdleAnimation, clipSettings);
            }
            
            if (settings.enemyWalkAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.enemyWalkAnimation);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(settings.enemyWalkAnimation, clipSettings);
            }
            
            if (settings.enemyAttackAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.enemyAttackAnimation);
                clipSettings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(settings.enemyAttackAnimation, clipSettings);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", 
                $"Created Enemy base Animator Controller at:\n{controllerPath}\n\n" +
                "The system will now use this controller with your animations!", 
                "OK");
        }
        
        private void ConfigureAnimationLoopSettings(GlobalGameSettings settings)
        {
            bool changed = false;
            List<string> messages = new List<string>();
            
            // Configure hero animations
            if (settings.heroIdleAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.heroIdleAnimation);
                if (!clipSettings.loopTime)
                {
                    clipSettings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(settings.heroIdleAnimation, clipSettings);
                    changed = true;
                    messages.Add($"✓ Set {settings.heroIdleAnimation.name} to loop");
                    Debug.Log($"[GlobalGameSettings] Set {settings.heroIdleAnimation.name} to loop");
                }
                else
                {
                    messages.Add($"✓ {settings.heroIdleAnimation.name} already loops");
                }
            }
            
            if (settings.heroWalkAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.heroWalkAnimation);
                if (!clipSettings.loopTime)
                {
                    clipSettings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(settings.heroWalkAnimation, clipSettings);
                    changed = true;
                    messages.Add($"✓ Set {settings.heroWalkAnimation.name} to loop");
                    Debug.Log($"[GlobalGameSettings] Set {settings.heroWalkAnimation.name} to loop");
                }
                else
                {
                    messages.Add($"✓ {settings.heroWalkAnimation.name} already loops");
                }
            }
            
            if (settings.heroFireAnimation != null)
            {
                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(settings.heroFireAnimation);
                if (clipSettings.loopTime)
                {
                    clipSettings.loopTime = false;
                    AnimationUtility.SetAnimationClipSettings(settings.heroFireAnimation, clipSettings);
                    changed = true;
                    messages.Add($"✓ Set {settings.heroFireAnimation.name} to NOT loop");
                    Debug.Log($"[GlobalGameSettings] Set {settings.heroFireAnimation.name} to NOT loop");
                }
                else
                {
                    messages.Add($"✓ {settings.heroFireAnimation.name} already set to play once");
                }
            }
            
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", 
                    "Animation loop settings configured!\n\n" +
                    string.Join("\n", messages) + "\n\n" +
                    "Please restart the game to see the changes.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", 
                    "All animation loop settings are already configured correctly:\n\n" +
                    string.Join("\n", messages), 
                    "OK");
            }
        }
    }
}

