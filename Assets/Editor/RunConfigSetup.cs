using UnityEditor;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Helper script to execute the config setup
    /// Can be called programmatically
    /// </summary>
    [InitializeOnLoad]
    public static class RunConfigSetup
    {
        // Auto-run once when Unity loads (commented out to avoid running every time)
        // static RunConfigSetup()
        // {
        //     EditorApplication.delayCall += () => {
        //         SetupAllConfigs.Execute();
        //     };
        // }
        
        /// <summary>
        /// Public method that can be called to run setup
        /// </summary>
        [MenuItem("Tools/Run Config Setup Now")]
        public static void RunNow()
        {
            SetupAllConfigs.Execute();
        }
    }
}

