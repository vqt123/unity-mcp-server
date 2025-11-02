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
            if (config.weaponConfig == null)
            {
                EditorGUILayout.HelpBox("⚠️ Weapon Config is not assigned! Hero will not function properly.", MessageType.Error);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Find Weapon Config"))
                {
                    // Try to find matching weapon config
                    string weaponName = GetDefaultWeaponForHero(config.heroTypeName);
                    WeaponConfig weapon = FindWeaponConfig(weaponName);
                    if (weapon != null)
                    {
                        config.weaponConfig = weapon;
                        EditorUtility.SetDirty(config);
                        Debug.Log($"[HeroConfigEditor] Assigned weapon config '{weaponName}' to {config.heroTypeName}");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Not Found", 
                            $"Could not find weapon config '{weaponName}'. Please create it first or assign manually.",
                            "OK");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ Weapon: {config.weaponConfig.weaponName}", MessageType.Info);
            }
            
            // Show stats summary
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stats Summary", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            float dps = config.weaponConfig != null 
                ? (config.damage / (config.weaponConfig.shootCooldown > 0 ? config.weaponConfig.shootCooldown : config.attackSpeed > 0 ? 1f / config.attackSpeed : 1f))
                : (config.damage / (config.attackSpeed > 0 ? 1f / config.attackSpeed : 1f));
            EditorGUILayout.FloatField("Estimated DPS", dps);
            EditorGUILayout.FloatField("Health", config.maxHealth);
            EditorGUILayout.FloatField("Move Speed", config.moveSpeed);
            EditorGUI.EndDisabledGroup();
        }
        
        private WeaponConfig FindWeaponConfig(string weaponName)
        {
            WeaponConfig[] configs = Resources.LoadAll<WeaponConfig>("WeaponConfigs");
            foreach (WeaponConfig config in configs)
            {
                if (config != null && config.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return config;
                }
            }
            return null;
        }
        
        private string GetDefaultWeaponForHero(string heroType)
        {
            switch (heroType.ToLower())
            {
                case "archer":
                    return "Bow";
                case "mage":
                    return "Firewand";
                case "warrior":
                    return "Sword";
                default:
                    return "Bow"; // Default
            }
        }
    }
}

