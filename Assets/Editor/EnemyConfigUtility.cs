using UnityEngine;
using UnityEditor;
using System.IO;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    public static class EnemyConfigUtility
    {
        [MenuItem("Tools/Create/Update All Enemy Configs")]
        public static void CreateOrUpdateAllEnemyConfigs()
        {
            Debug.Log("[EnemyConfigUtility] Creating/updating enemy configs...");
            
            // Create Resources/EnemyConfigs directory if it doesn't exist
            string configDir = "Assets/Resources/EnemyConfigs";
            if (!AssetDatabase.IsValidFolder(configDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "EnemyConfigs");
            }
            
            // Enemy type definitions with prefab paths
            var enemyDefs = new[]
            {
                new { 
                    name = "BasicGrunt", 
                    displayName = "Basic Grunt", 
                    desc = "A basic enemy grunt",
                    health = 30f, damage = 5f, moveSpeed = 2f, attackRange = 1.0f, attackSpeed = 0.8f,
                    isBoss = false, isMiniBoss = false,
                    prefabPath = "Assets/Monsters Ultimate Pack 01 Cute Series/Bat Vampire Lord Evolution Pack Cute series/Bat Cute Series/Prefabs/Bat.prefab",
                    color = new Color(0.8f, 0.2f, 0.2f, 1f) // Red
                },
                new { 
                    name = "FastRunner", 
                    displayName = "Fast Runner", 
                    desc = "A fast-moving enemy",
                    health = 30f, damage = 3f, moveSpeed = 4f, attackRange = 0.8f, attackSpeed = 1.2f,
                    isBoss = false, isMiniBoss = false,
                    prefabPath = "Assets/Monsters Ultimate Pack 01 Cute Series/Bee Bumble Sting Evolution Pack Cute Series/Bee Cute Series/Prefabs/Bee.prefab",
                    color = new Color(1f, 0.8f, 0.2f, 1f) // Yellow
                },
                new { 
                    name = "Tank", 
                    displayName = "Tank", 
                    desc = "A heavily armored enemy",
                    health = 200f, damage = 15f, moveSpeed = 1f, attackRange = 1.2f, attackSpeed = 0.5f,
                    isBoss = false, isMiniBoss = false,
                    prefabPath = "Assets/Monsters Ultimate Pack 01 Cute Series/Mushroom Fungi Toadstool Evolution Pack Cute series/Mushroom Cute Series/Prefabs/Mushroom.prefab",
                    color = new Color(0.6f, 0.3f, 0.8f, 1f) // Purple
                },
                new { 
                    name = "MiniBoss", 
                    displayName = "Mini Boss", 
                    desc = "A powerful mini-boss enemy",
                    health = 500f, damage = 25f, moveSpeed = 2f, attackRange = 1.5f, attackSpeed = 0.7f,
                    isBoss = false, isMiniBoss = true,
                    prefabPath = "Assets/Monsters Ultimate Pack 01 Cute Series/Bat Vampire Lord Evolution Pack Cute series/Bat Lord Cute Series/Prefabs/Bat Lord.prefab",
                    color = new Color(0.8f, 0.4f, 0.2f, 1f) // Orange
                },
                new { 
                    name = "Boss", 
                    displayName = "Boss", 
                    desc = "The ultimate boss enemy",
                    health = 2000f, damage = 50f, moveSpeed = 1f, attackRange = 2.0f, attackSpeed = 0.4f,
                    isBoss = true, isMiniBoss = false,
                    prefabPath = "Assets/Monsters Ultimate Pack 01 Cute Series/Bat Vampire Lord Evolution Pack Cute series/Vampire Bat Cute Series/Prefabs/Vampire Bat.prefab",
                    color = new Color(0.5f, 0f, 0.5f, 1f) // Dark purple/magenta
                },
            };
            
            foreach (var enemyDef in enemyDefs)
            {
                string assetPath = $"{configDir}/{enemyDef.name}.asset";
                EnemyConfigSO config = AssetDatabase.LoadAssetAtPath<EnemyConfigSO>(assetPath);
                
                bool isNew = config == null;
                if (isNew)
                {
                    config = ScriptableObject.CreateInstance<EnemyConfigSO>();
                    Debug.Log($"[EnemyConfigUtility] Creating new config: {enemyDef.name}");
                }
                else
                {
                    Debug.Log($"[EnemyConfigUtility] Updating existing config: {enemyDef.name}");
                }
                
                // Set properties
                config.enemyTypeName = enemyDef.name;
                config.displayName = enemyDef.displayName;
                config.description = enemyDef.desc;
                config.maxHealth = enemyDef.health;
                config.damage = enemyDef.damage;
                config.moveSpeed = enemyDef.moveSpeed;
                config.attackRange = enemyDef.attackRange;
                config.attackSpeed = enemyDef.attackSpeed;
                config.isBoss = enemyDef.isBoss;
                config.isMiniBoss = enemyDef.isMiniBoss;
                config.enemyColor = enemyDef.color;
                
                // Load and assign prefab
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyDef.prefabPath);
                if (prefab != null)
                {
                    config.enemyPrefab = prefab;
                    Debug.Log($"[EnemyConfigUtility]   Assigned prefab: {prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"[EnemyConfigUtility]   Prefab not found at: {enemyDef.prefabPath}");
                }
                
                // Save asset
                if (isNew)
                {
                    AssetDatabase.CreateAsset(config, assetPath);
                }
                else
                {
                    EditorUtility.SetDirty(config);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[EnemyConfigUtility] ✓ Created/updated {enemyDefs.Length} enemy configs!");
        }
    }
}

