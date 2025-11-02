using UnityEngine;
using UnityEditor;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Custom editor for WeaponConfig to improve Inspector experience
    /// </summary>
    [CustomEditor(typeof(WeaponConfig))]
    public class WeaponConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            WeaponConfig config = (WeaponConfig)target;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            // Button to create matching ProjectileFX prefab if missing
            if (config.projectileFXPrefab == null)
            {
                EditorGUILayout.HelpBox("No ProjectileFX prefab assigned. Use the ProjectileFX Creator tool to create variants.", MessageType.Warning);
                
                if (GUILayout.Button("Open ProjectileFX Creator"))
                {
                    ProjectileFXCreator.ShowWindow();
                }
            }
            
            // Show stats summary
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stats Summary", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("DPS (Base)", config.damage / config.shootCooldown);
            EditorGUILayout.FloatField("Projectiles/Second", 1f / config.shootCooldown);
            EditorGUI.EndDisabledGroup();
        }
    }
}

