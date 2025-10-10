using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace UnityMCP
{
    public static class MCPTools
    {
        public static JObject Execute(string tool, JObject args)
        {
            try
            {
                switch (tool)
                {
                    case "unity_ping":
                        return Ping();
                    
                    case "unity_get_scene_info":
                        return GetSceneInfo();
                    
                    case "unity_create_cube":
                        return CreateCube(args);
                    
                    case "unity_force_compile":
                        return ForceCompile();
                    
                    case "unity_is_compiling":
                        return IsCompiling();
                    
                    case "unity_wait_for_compile":
                        return WaitForCompile(args);
                    
                    case "unity_get_logs":
                        return GetLogs(args);
                    
                    default:
                        throw new System.Exception($"Unknown tool: {tool}");
                }
            }
            catch (System.Exception e)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = e.Message
                };
            }
        }
        
        // Tool 1: Ping (health check)
        private static JObject Ping()
        {
            return new JObject
            {
                ["success"] = true,
                ["message"] = "pong",
                ["unityVersion"] = Application.unityVersion,
                ["timestamp"] = System.DateTime.UtcNow.ToString("o")
            };
        }
        
        // Tool 2: Get Scene Info
        private static JObject GetSceneInfo()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            
            return new JObject
            {
                ["success"] = true,
                ["sceneName"] = scene.name,
                ["scenePath"] = scene.path,
                ["isLoaded"] = scene.isLoaded,
                ["rootObjectCount"] = rootObjects.Length,
                ["rootObjects"] = new JArray(
                    rootObjects.Select(go => go.name)
                )
            };
        }
        
        // Tool 3: Create Cube
        private static JObject CreateCube(JObject args)
        {
            // Parse arguments
            string name = args["name"]?.ToString() ?? "Cube";
            float x = args["x"]?.ToObject<float>() ?? 0;
            float y = args["y"]?.ToObject<float>() ?? 0;
            float z = args["z"]?.ToObject<float>() ?? 0;
            
            // Create cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = new Vector3(x, y, z);
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(cube, $"Create {name}");
            
            // Select it
            Selection.activeGameObject = cube;
            
            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            return new JObject
            {
                ["success"] = true,
                ["name"] = cube.name,
                ["position"] = new JArray(x, y, z),
                ["instanceId"] = cube.GetInstanceID()
            };
        }
        
        // Tool 4: Force Compile
        private static JObject ForceCompile()
        {
            // Request script compilation
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            
            return new JObject
            {
                ["success"] = true,
                ["message"] = "Script compilation requested",
                ["isCompiling"] = EditorApplication.isCompiling
            };
        }
        
        // Tool 5: Check if Compiling
        private static JObject IsCompiling()
        {
            return new JObject
            {
                ["success"] = true,
                ["isCompiling"] = EditorApplication.isCompiling,
                ["message"] = EditorApplication.isCompiling ? "Unity is compiling" : "Unity is idle"
            };
        }
        
        // Tool 6: Wait for Compile
        private static JObject WaitForCompile(JObject args)
        {
            int maxWaitSeconds = args["maxWaitSeconds"]?.ToObject<int>() ?? 30;
            
            if (!EditorApplication.isCompiling)
            {
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = "Unity is not compiling, no need to wait",
                    ["isCompiling"] = false,
                    ["waitedSeconds"] = 0
                };
            }
            
            // Wait for compilation to finish
            var startTime = System.DateTime.Now;
            int checkCount = 0;
            
            while (EditorApplication.isCompiling)
            {
                System.Threading.Thread.Sleep(100); // Check every 100ms
                checkCount++;
                
                var elapsed = (System.DateTime.Now - startTime).TotalSeconds;
                if (elapsed >= maxWaitSeconds)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["message"] = $"Compilation did not finish within {maxWaitSeconds} seconds",
                        ["isCompiling"] = true,
                        ["waitedSeconds"] = elapsed
                    };
                }
            }
            
            var totalWait = (System.DateTime.Now - startTime).TotalSeconds;
            
            return new JObject
            {
                ["success"] = true,
                ["message"] = "Compilation finished successfully",
                ["isCompiling"] = false,
                ["waitedSeconds"] = totalWait
            };
        }
        
        // Tool 7: Get Console Logs
        private static JObject GetLogs(JObject args)
        {
            int count = args["count"]?.ToObject<int>() ?? 50;
            bool includeStackTrace = args["includeStackTrace"]?.ToObject<bool>() ?? false;
            
            // Access Unity console logs using reflection
            var logEntriesType = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var getCountMethod = logEntriesType.GetMethod("GetCount");
            var startGettingEntriesMethod = logEntriesType.GetMethod("StartGettingEntries");
            var getEntryInternalMethod = logEntriesType.GetMethod("GetEntryInternal");
            var endGettingEntriesMethod = logEntriesType.GetMethod("EndGettingEntries");
            
            var logEntryType = System.Type.GetType("UnityEditor.LogEntry,UnityEditor.dll");
            
            int totalCount = (int)getCountMethod.Invoke(null, null);
            int startIndex = System.Math.Max(0, totalCount - count);
            
            startGettingEntriesMethod.Invoke(null, null);
            
            var logs = new JArray();
            
            try
            {
                for (int i = startIndex; i < totalCount; i++)
                {
                    var logEntry = System.Activator.CreateInstance(logEntryType);
                    var args_array = new object[] { i, logEntry };
                    getEntryInternalMethod.Invoke(null, args_array);
                    logEntry = args_array[1];
                    
                    // Extract log entry details
                    var message = logEntryType.GetField("message").GetValue(logEntry) as string;
                    var file = logEntryType.GetField("file").GetValue(logEntry) as string;
                    var line = (int)logEntryType.GetField("line").GetValue(logEntry);
                    var mode = (int)logEntryType.GetField("mode").GetValue(logEntry);
                    var instanceID = (int)logEntryType.GetField("instanceID").GetValue(logEntry);
                    
                    // Determine log type
                    string logType = "Log";
                    if ((mode & (int)LogType.Error) != 0)
                        logType = "Error";
                    else if ((mode & (int)LogType.Warning) != 0)
                        logType = "Warning";
                    
                    var logObj = new JObject
                    {
                        ["index"] = i,
                        ["type"] = logType,
                        ["message"] = message
                    };
                    
                    if (includeStackTrace && !string.IsNullOrEmpty(file))
                    {
                        logObj["file"] = file;
                        logObj["line"] = line;
                    }
                    
                    logs.Add(logObj);
                }
            }
            finally
            {
                endGettingEntriesMethod.Invoke(null, null);
            }
            
            return new JObject
            {
                ["success"] = true,
                ["totalCount"] = totalCount,
                ["returnedCount"] = logs.Count,
                ["logs"] = logs
            };
        }
    }
}

