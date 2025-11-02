using UnityEditor;
using System.Reflection;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Auto-executes config setup - can be triggered programmatically
    /// </summary>
    [InitializeOnLoad]
    public static class AutoSetupConfigs
    {
        // Auto-run disabled by default - uncomment to enable
        // static AutoSetupConfigs()
        // {
        //     EditorApplication.delayCall += () => {
        //         if (!EditorApplication.isPlaying)
        //         {
        //             SetupAllConfigs.Execute();
        //         }
        //     };
        // }
    }
}

