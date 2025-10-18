using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public class MCPServerWindow : EditorWindow
    {
        private bool mcpConfigured = false;
        private string mcpConfigPath = "";
        private string mcpStatus = "Checking...";
        
        [MenuItem("Window/MCP Server")]
        public static void ShowWindow()
        {
            GetWindow<MCPServerWindow>("MCP Server");
        }
        
        private void OnEnable()
        {
            CheckMCPConfiguration();
        }
        
        private void CheckMCPConfiguration()
        {
            // Check for Cursor MCP configuration
            string homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            
            // The correct path for Cursor MCP config
            mcpConfigPath = Path.Combine(homeDir, ".cursor/mcp.json");
            
            if (File.Exists(mcpConfigPath))
            {
                // Check if unity-mcp is configured
                try
                {
                    string json = File.ReadAllText(mcpConfigPath);
                    JObject config = JObject.Parse(json);
                    JObject mcpServers = config["mcpServers"] as JObject;
                    
                    if (mcpServers != null && mcpServers["unity-mcp"] != null)
                    {
                        mcpConfigured = true;
                        mcpStatus = "Configured and enabled ✓";
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[MCP] Could not parse config at {mcpConfigPath}: {e.Message}");
                }
            }
            
            // If we get here, no valid configuration found
            mcpConfigured = false;
            mcpStatus = File.Exists(mcpConfigPath) ? "Not configured" : "Config file not found";
        }
        
        private void ConfigureMCP()
        {
            try
            {
                // Get the project root (where mcp-server folder should be)
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string mcpServerScript = Path.Combine(projectRoot, "mcp-server", "unity_mcp_server.py");
                
                if (!File.Exists(mcpServerScript))
                {
                    EditorUtility.DisplayDialog(
                        "MCP Server Script Not Found",
                        $"Could not find unity_mcp_server.py at:\n{mcpServerScript}\n\nPlease ensure the mcp-server folder exists in your project root.",
                        "OK"
                    );
                    return;
                }
                
                // Create config directory if it doesn't exist
                string configDir = Path.GetDirectoryName(mcpConfigPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // Load existing config or create new
                JObject config;
                if (File.Exists(mcpConfigPath))
                {
                    string existingJson = File.ReadAllText(mcpConfigPath);
                    config = JObject.Parse(existingJson);
                }
                else
                {
                    config = new JObject();
                }
                
                // Ensure mcpServers object exists
                if (config["mcpServers"] == null)
                {
                    config["mcpServers"] = new JObject();
                }
                
                JObject mcpServers = config["mcpServers"] as JObject;
                
                // Check if uv is available (better package management)
                string uvPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".local/bin/uv");
                string mcpServerDir = Path.GetDirectoryName(mcpServerScript);
                
                // Add/update unity-mcp configuration (matching Cursor's format)
                if (File.Exists(uvPath))
                {
                    // Use uv for better dependency management
                    mcpServers["unity-mcp"] = new JObject
                    {
                        ["command"] = uvPath,
                        ["args"] = new JArray { "run", "--directory", mcpServerDir, "unity_mcp_server.py" }
                    };
                    Debug.Log("[MCP] Using uv for Python dependency management");
                }
                else
                {
                    // Fallback to python3 (user will need to install packages manually)
                    mcpServers["unity-mcp"] = new JObject
                    {
                        ["command"] = "python3",
                        ["args"] = new JArray { mcpServerScript }
                    };
                    Debug.LogWarning("[MCP] uv not found, using python3. You may need to install dependencies: pip3 install mcp httpx");
                }
                
                // Save configuration with proper formatting
                File.WriteAllText(mcpConfigPath, config.ToString(Newtonsoft.Json.Formatting.Indented));
                
                Debug.Log($"[MCP] Configuration saved to: {mcpConfigPath}");
                Debug.Log($"[MCP] Added unity-mcp server with script: {mcpServerScript}");
                
                mcpConfigured = true;
                mcpStatus = "Configured and enabled ✓";
                
                EditorUtility.DisplayDialog(
                    "MCP Configuration Updated",
                    $"Unity MCP server has been configured in Cursor!\n\nConfig file: {mcpConfigPath}\n\nServer script: {mcpServerScript}\n\nPlease restart Cursor for changes to take effect.",
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MCP] Failed to configure: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Configuration Failed",
                    $"Failed to configure MCP:\n{e.Message}",
                    "OK"
                );
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Unity MCP Server - MVP", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // MCP Configuration Status
            GUILayout.Label("Cursor MCP Configuration", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status:", mcpStatus, GUILayout.Width(300));
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                CheckMCPConfiguration();
            }
            EditorGUILayout.EndHorizontal();
            
            if (!mcpConfigured)
            {
                EditorGUILayout.HelpBox(
                    "Unity MCP is not configured in Cursor. Click 'Configure MCP' to automatically set it up.",
                    MessageType.Warning
                );
                
                if (GUILayout.Button("Configure MCP in Cursor"))
                {
                    ConfigureMCP();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Unity MCP is properly configured in Cursor! ✓",
                    MessageType.Info
                );
                
                if (GUILayout.Button("Reconfigure MCP"))
                {
                    ConfigureMCP();
                }
            }
            
            if (!string.IsNullOrEmpty(mcpConfigPath))
            {
                EditorGUILayout.LabelField("Config file:", mcpConfigPath, EditorStyles.miniLabel);
            }
            
            EditorGUILayout.Space();
            
            // Server Status
            GUILayout.Label("HTTP Server", EditorStyles.boldLabel);
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
                "- unity_get_logs\n\n" +
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

