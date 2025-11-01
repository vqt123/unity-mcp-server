using UnityEngine;
using UnityEditor;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Utility script to create projectileFXFireball variant
    /// Run this from Tools menu after ProjectileFX exists in the scene
    /// </summary>
    public class CreateFireballFX
    {
        [MenuItem("Tools/Create Fireball FX Variant")]
        public static void CreateFireballVariant()
        {
            // Find the source ProjectileFX
            GameObject sourceFX = GameObject.Find("ProjectileFX");
            if (sourceFX == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "ProjectileFX not found in scene. Please ensure it exists first.", 
                    "OK");
                return;
            }
            
            // Check if fireball variant already exists
            GameObject existing = GameObject.Find("projectileFXFireball");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Already Exists", 
                    "projectileFXFireball already exists. Replace it?", 
                    "Yes", "No"))
                {
                    return;
                }
                Undo.DestroyObjectImmediate(existing);
            }
            
            // Clone the source
            GameObject fireballFX = Object.Instantiate(sourceFX);
            fireballFX.name = "projectileFXFireball";
            
            // Update ProjectileFXController
            var fxController = fireballFX.GetComponent<ArenaGame.Client.ProjectileFXController>();
            if (fxController == null)
            {
                fxController = fireballFX.AddComponent<ArenaGame.Client.ProjectileFXController>();
            }
            fxController.mode = ArenaGame.Client.ProjectileFXController.FXMode.Mode2;
            fxController.rotationSpeed = 2000f;
            
            // Update particle system colors to fire colors (red/orange)
            ParticleSystem[] particleSystems = fireballFX.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                var main = ps.main;
                // Fire colors: orange to red
                main.startColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
            }
            
            // Position next to source
            fireballFX.transform.position = sourceFX.transform.position + Vector3.right * 5f;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(fireballFX, "Create Fireball FX Variant");
            
            // Select the new variant
            Selection.activeGameObject = fireballFX;
            
            Debug.Log("[CreateFireballFX] Created projectileFXFireball variant successfully!");
            
            EditorUtility.DisplayDialog("Success", 
                "Created projectileFXFireball variant!\n\n" +
                "It has been added to the scene and selected.",
                "OK");
        }
    }
}

