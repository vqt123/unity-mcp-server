using UnityEngine;
using UnityEditor;

namespace UnityMCP
{
    public class MCPServerWindow : EditorWindow
    {
        [MenuItem("Window/MCP Server")]
        public static void ShowWindow()
        {
            GetWindow<MCPServerWindow>("MCP Server");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Unity MCP Server - MVP", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Status:", "Running");
            EditorGUILayout.LabelField("Port:", "8765");
            EditorGUILayout.LabelField("URL:", "http://localhost:8765");
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Stop Server"))
            {
                MCPServer.StopServer();
            }
            
            if (GUILayout.Button("Start Server"))
            {
                MCPServer.StartServer();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "MCP Server is running!\n\n" +
                "Tools available:\n" +
                "- unity_ping\n" +
                "- unity_get_scene_info\n" +
                "- unity_create_cube\n" +
                "- unity_force_compile\n" +
                "- unity_is_compiling\n" +
                "- unity_wait_for_compile\n" +
                "- unity_get_logs\n" +
                "- unity_capture_screenshot ⭐ NEW",
                MessageType.Info
            );
        }
    }
}

