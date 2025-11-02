using UnityEditor;
using UnityEngine;

namespace ArenaGame.Editor
{
    /// <summary>
    /// One-time hero config setup - runs automatically once
    /// </summary>
    [InitializeOnLoad]
    public static class OneTimeHeroConfigSetup
    {
        private const string SETUP_COMPLETE_KEY = "HeroConfigsSetupComplete";
        
        static OneTimeHeroConfigSetup()
        {
            // Only run if not already completed
            if (EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
            {
                return;
            }
            
            // Wait a moment for Unity to fully initialize
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.Log("[OneTimeHeroConfigSetup] Running automatic hero config setup...");
                    
                    try
                    {
                        SetupAllConfigs.ExecuteInternal(showDialog: false);
                        EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
                        Debug.Log("[OneTimeHeroConfigSetup] Hero configs setup complete!");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[OneTimeHeroConfigSetup] Error: {e.Message}");
                    }
                }
            };
        }
        
        [MenuItem("Tools/Reset Hero Config Setup (Run Again)")]
        public static void ResetSetup()
        {
            EditorPrefs.DeleteKey(SETUP_COMPLETE_KEY);
            Debug.Log("[OneTimeHeroConfigSetup] Reset - will run setup on next compile");
        }
    }
}

