using UnityEngine;
using UnityEditor;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Editor window for debug commands
    /// </summary>
    public class DebugCommandWindow : EditorWindow
    {
        private string commandInput = "";
        private Vector2 scrollPosition;
        private string logOutput = "";
        
        [MenuItem("Arena Game/Debug Commands")]
        public static void ShowWindow()
        {
            GetWindow<DebugCommandWindow>("Debug Commands");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Debug Command Window", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Command input
            GUILayout.Label("Enter Command:", EditorStyles.label);
            commandInput = EditorGUILayout.TextField(commandInput);
            
            EditorGUILayout.Space();
            
            // Execute button
            if (GUILayout.Button("Execute", GUILayout.Height(30)))
            {
                ExecuteCommand(commandInput);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Quick command buttons
            GUILayout.Label("Quick Commands:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Reset Player Blob", GUILayout.Height(25)))
            {
                ExecuteCommand("reset");
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Log output
            GUILayout.Label("Output:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            EditorGUILayout.TextArea(logOutput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            
            // Clear log button
            if (GUILayout.Button("Clear Log"))
            {
                logOutput = "";
            }
        }
        
        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                Log("No command entered.");
                return;
            }
            
            command = command.Trim().ToLower();
            
            Log($"Executing command: {command}");
            
            switch (command)
            {
                case "reset":
                    ResetPlayerBlob();
                    break;
                    
                default:
                    Log($"Unknown command: {command}");
                    Log("Available commands:");
                    Log("  - reset: Reset player blob to default state");
                    break;
            }
            
            // Clear input after execution
            commandInput = "";
        }
        
        private void ResetPlayerBlob()
        {
            Log("Resetting player blob...");
            
            if (Application.isPlaying)
            {
                // In play mode, reset via PlayerDataManager if it exists
                if (PlayerDataManager.Instance != null)
                {
                    PlayerDataManager.Instance.ResetData();
                    Log("✓ Player blob reset successfully (in play mode)");
                    
                    // Show reset details
                    var blob = PlayerDataManager.Instance.PlayerBlob;
                    if (blob != null)
                    {
                        Log($"  Energy: {blob.currentEnergy}/{blob.maxEnergy}");
                        Log($"  Gold: {blob.totalGold}");
                        Log($"  Heroes: {string.Join(", ", blob.heroInventory.unlockedHeroes)}");
                    }
                }
                else
                {
                    Log("✗ PlayerDataManager.Instance is null. Make sure the game is running.");
                }
            }
            else
            {
                // In edit mode, directly manipulate the JSON file
                try
                {
                    string filePath = System.IO.Path.Combine(
                        Application.persistentDataPath,
                        "PlayerData",
                        "player_blob.json"
                    );
                    
                    var defaultBlob = PlayerBlob.CreateDefault();
                    string json = JsonUtility.ToJson(defaultBlob, true);
                    
                    // Create directory if it doesn't exist
                    string directory = System.IO.Path.GetDirectoryName(filePath);
                    if (!System.IO.Directory.Exists(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                    }
                    
                    System.IO.File.WriteAllText(filePath, json);
                    
                    Log($"✓ Player blob reset successfully (edit mode)");
                    Log($"  File: {filePath}");
                    Log($"  Energy: {defaultBlob.currentEnergy}/{defaultBlob.maxEnergy}");
                    Log($"  Gold: {defaultBlob.totalGold}");
                    Log($"  Heroes: {string.Join(", ", defaultBlob.heroInventory.unlockedHeroes)}");
                    
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    Log($"✗ Error resetting player blob: {e.Message}");
                    Log($"  Stack trace: {e.StackTrace}");
                }
            }
        }
        
        private void Log(string message)
        {
            logOutput += $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";
            
            // Auto-scroll to bottom
            scrollPosition.y = float.MaxValue;
            
            // Also log to Unity console
            Debug.Log($"[DebugCommandWindow] {message}");
        }
    }
}

