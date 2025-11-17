using UnityEngine;
using UnityEditor;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Editor script to create Level 1 configuration
    /// Right-click in Project window → Create → ArenaGame → Level 1 Config
    /// </summary>
    public class CreateLevel1Config : EditorWindow
    {
        [MenuItem("Assets/Create/ArenaGame/Level 1 Config", false, 1)]
        public static void CreateLevel1()
        {
            // Create the ScriptableObject instance
            LevelConfigSO levelConfig = ScriptableObject.CreateInstance<LevelConfigSO>();
            
            // Configure Level 1
            levelConfig.levelNumber = 1;
            levelConfig.levelName = "The Beginning";
            levelConfig.spawnRateIncreaseInterval = 5f;
            levelConfig.spawnRateIncreaseMultiplier = 0.1f; // 10% increase
            
            // Schedule 1: Basic enemies - 1 per second starting at 0s
            EnemySpawnSchedule basicSchedule = new EnemySpawnSchedule
            {
                enemyType = "BasicGrunt",
                startTime = 0f,
                initialSpawnInterval = 1f, // 1 per second
                healthMultiplier = 1f,
                isActive = true
            };
            levelConfig.spawnSchedules.Add(basicSchedule);
            
            // Schedule 2: EliteTank enemies - 1 every 5 seconds starting at 30s, 4x health
            EnemySpawnSchedule eliteSchedule = new EnemySpawnSchedule
            {
                enemyType = "EliteTank",
                startTime = 30f,
                initialSpawnInterval = 5f, // 1 every 5 seconds
                healthMultiplier = 4f, // 4x health
                isActive = true
            };
            levelConfig.spawnSchedules.Add(eliteSchedule);
            
            // Save the asset
            // Ensure LevelConfigs folder exists
            string folderPath = "Assets/Resources/LevelConfigs";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "LevelConfigs");
            }
            
            string path = $"{folderPath}/LevelConfig_1.asset";
            AssetDatabase.CreateAsset(levelConfig, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[CreateLevel1Config] Created Level 1 config at {path}");
            EditorUtility.DisplayDialog("Success", $"Level 1 config created at:\n{path}", "OK");
            
            // Select the created asset
            Selection.activeObject = levelConfig;
            EditorGUIUtility.PingObject(levelConfig);
        }
    }
}

