using UnityEngine;
using UnityEditor;
using System.Linq;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Editor tool to create variants of ProjectileFX objects
    /// </summary>
    public class ProjectileFXCreator : EditorWindow
    {
        private GameObject sourceProjectileFX;
        private string variantName = "";
        private Color particleColor = Color.white;
        private float rotationSpeed = 2000f;
        private bool useMode2 = false;
        
        [MenuItem("Tools/ProjectileFX Creator")]
        public static void ShowWindow()
        {
            GetWindow<ProjectileFXCreator>("ProjectileFX Creator");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Create ProjectileFX Variant", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Source ProjectileFX selection
            sourceProjectileFX = (GameObject)EditorGUILayout.ObjectField(
                "Source ProjectileFX", 
                sourceProjectileFX, 
                typeof(GameObject), 
                true
            );
            
            // Auto-find ProjectileFX in scene if not set
            if (sourceProjectileFX == null)
            {
                if (GUILayout.Button("Find ProjectileFX in Scene"))
                {
                    sourceProjectileFX = GameObject.Find("ProjectileFX");
                    if (sourceProjectileFX == null)
                    {
                        EditorUtility.DisplayDialog("Not Found", 
                            "ProjectileFX not found in current scene. Please select it manually.", 
                            "OK");
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            // Variant name
            variantName = EditorGUILayout.TextField("Variant Name", variantName);
            
            EditorGUILayout.Space();
            
            // Particle settings
            GUILayout.Label("Particle Settings", EditorStyles.boldLabel);
            particleColor = EditorGUILayout.ColorField("Particle Color", particleColor);
            
            EditorGUILayout.Space();
            
            // ProjectileFXController settings
            GUILayout.Label("FX Controller Settings", EditorStyles.boldLabel);
            useMode2 = EditorGUILayout.Toggle("Use Mode 2 (Rotating)", useMode2);
            if (useMode2)
            {
                rotationSpeed = EditorGUILayout.FloatField("Rotation Speed (deg/s)", rotationSpeed);
            }
            
            EditorGUILayout.Space();
            
            // Create button
            EditorGUI.BeginDisabledGroup(sourceProjectileFX == null || string.IsNullOrEmpty(variantName));
            if (GUILayout.Button("Create Variant", GUILayout.Height(30)))
            {
                CreateVariant();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // Help text
            EditorGUILayout.HelpBox(
                "This tool creates a new ProjectileFX variant based on the source. " +
                "The variant will be added to the scene with the specified name and settings.",
                MessageType.Info
            );
        }
        
        void CreateVariant()
        {
            if (sourceProjectileFX == null)
            {
                EditorUtility.DisplayDialog("Error", "Source ProjectileFX is required!", "OK");
                return;
            }
            
            if (string.IsNullOrEmpty(variantName))
            {
                EditorUtility.DisplayDialog("Error", "Variant name is required!", "OK");
                return;
            }
            
            // Clone the source
            GameObject variant = Instantiate(sourceProjectileFX);
            variant.name = variantName;
            
            // Update ProjectileFXController if it exists
            var fxController = variant.GetComponent<ArenaGame.Client.ProjectileFXController>();
            if (fxController != null)
            {
                fxController.mode = useMode2 
                    ? ArenaGame.Client.ProjectileFXController.FXMode.Mode2 
                    : ArenaGame.Client.ProjectileFXController.FXMode.Mode1;
                fxController.rotationSpeed = rotationSpeed;
            }
            else
            {
                // Add ProjectileFXController if it doesn't exist
                fxController = variant.AddComponent<ArenaGame.Client.ProjectileFXController>();
                fxController.mode = useMode2 
                    ? ArenaGame.Client.ProjectileFXController.FXMode.Mode2 
                    : ArenaGame.Client.ProjectileFXController.FXMode.Mode1;
                fxController.rotationSpeed = rotationSpeed;
            }
            
            // Update particle system colors
            ParticleSystem[] particleSystems = variant.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                var main = ps.main;
                main.startColor = particleColor;
            }
            
            // Position near source but slightly offset
            variant.transform.position = sourceProjectileFX.transform.position + Vector3.right * 5f;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(variant, "Create ProjectileFX Variant");
            
            // Select the new variant
            Selection.activeGameObject = variant;
            
            Debug.Log($"[ProjectileFXCreator] Created variant '{variantName}' from '{sourceProjectileFX.name}'");
            
            EditorUtility.DisplayDialog("Success", 
                $"Created ProjectileFX variant '{variantName}'!\n\n" +
                $"It has been added to the scene and selected.",
                "OK");
        }
    }
}

