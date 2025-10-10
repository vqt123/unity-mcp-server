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
                "Core Tools:\n" +
                "- unity_ping\n" +
                "- unity_get_scene_info\n" +
                "- unity_create_cube\n" +
                "- unity_force_compile\n" +
                "- unity_is_compiling\n" +
                "- unity_wait_for_compile\n" +
                "- unity_get_logs\n" +
                "- unity_capture_screenshot\n\n" +
                "Scene Management 🎬 NEW:\n" +
                "- unity_create_scene\n" +
                "- unity_save_scene\n" +
                "- unity_load_scene\n\n" +
                "GameObject Operations 🎯 NEW:\n" +
                "- unity_find_gameobject\n" +
                "- unity_set_position\n" +
                "- unity_delete_gameobject\n\n" +
                "Script Management 📝 NEW:\n" +
                "- unity_create_script\n" +
                "- unity_add_component\n\n" +
                "Button Events 🔘 NEW:\n" +
                "- unity_set_button_onclick\n\n" +
                "UI Tools - Canvas:\n" +
                "- unity_ui_create_canvas\n" +
                "- unity_ui_setup_canvas_scaler\n" +
                "- unity_ui_create_event_system\n\n" +
                "UI Tools - Elements:\n" +
                "- unity_ui_create_button\n" +
                "- unity_ui_create_text\n" +
                "- unity_ui_create_image\n" +
                "- unity_ui_create_panel\n\n" +
                "UI Tools - Layout:\n" +
                "- unity_ui_create_vertical_layout\n" +
                "- unity_ui_create_horizontal_layout\n" +
                "- unity_ui_create_grid_layout",
                MessageType.Info
            );
        }
    }
}

