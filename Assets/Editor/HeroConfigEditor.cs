using UnityEngine;
using UnityEditor;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Custom editor for HeroConfigSO to improve Inspector experience
    /// </summary>
    [CustomEditor(typeof(HeroConfigSO))]
    public class HeroConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            HeroConfigSO config = (HeroConfigSO)target;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // Validation
            if (string.IsNullOrEmpty(config.weaponType))
            {
                EditorGUILayout.HelpBox("⚠️ Weapon Type is not set! Hero will not function properly.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ Weapon: {config.weaponType}" + 
                    (config.piercing ? " (Piercing)" : ""), MessageType.Info);
            }
            
            // Show stats summary
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stats Summary", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            float dps = config.attackSpeed > 0 ? config.damage * config.attackSpeed : 0f;
            EditorGUILayout.FloatField("Estimated DPS", dps);
            EditorGUILayout.FloatField("Health", config.maxHealth);
            EditorGUILayout.FloatField("Move Speed", config.moveSpeed);
            EditorGUILayout.FloatField("Attack Speed", config.attackSpeed);
            EditorGUILayout.FloatField("Projectile Speed", config.projectileSpeed);
            EditorGUILayout.IntField("Projectile Count", config.projectileCount);
            EditorGUI.EndDisabledGroup();
        }
    }
}

