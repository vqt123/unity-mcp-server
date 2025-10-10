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
                    
                    case "unity_capture_screenshot":
                        return CaptureScreenshot(args);
                    
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
        
        // Tool 8: Capture Screenshot
        private static JObject CaptureScreenshot(JObject args)
        {
            string viewType = args["viewType"]?.ToString() ?? "game"; // game, scene, camera
            int width = args["width"]?.ToObject<int>() ?? 1920;
            int height = args["height"]?.ToObject<int>() ?? 1080;
            bool returnBase64 = args["returnBase64"]?.ToObject<bool>() ?? true;
            string outputPath = args["outputPath"]?.ToString();
            
            // Create output path if not specified
            if (string.IsNullOrEmpty(outputPath))
            {
                string tempDir = System.IO.Path.Combine(Application.dataPath, "..", "Temp", "Screenshots");
                System.IO.Directory.CreateDirectory(tempDir);
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                outputPath = System.IO.Path.Combine(tempDir, $"screenshot_{timestamp}.png");
            }
            
            byte[] imageBytes = null;
            
            try
            {
                if (viewType == "game")
                {
                    imageBytes = CaptureGameView(width, height);
                }
                else if (viewType == "scene")
                {
                    imageBytes = CaptureSceneView(width, height);
                }
                else
                {
                    throw new System.Exception($"Unsupported view type: {viewType}");
                }
                
                // Save to file
                string fullPath = System.IO.Path.GetFullPath(outputPath);
                System.IO.File.WriteAllBytes(fullPath, imageBytes);
                
                var result = new JObject
                {
                    ["success"] = true,
                    ["path"] = fullPath,
                    ["width"] = width,
                    ["height"] = height,
                    ["fileSize"] = imageBytes.Length,
                    ["viewType"] = viewType
                };
                
                // Add base64 if requested
                if (returnBase64)
                {
                    result["base64"] = System.Convert.ToBase64String(imageBytes);
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = $"Screenshot failed: {e.Message}"
                };
            }
        }
        
        private static byte[] CaptureGameView(int width, int height)
        {
            // Find Game View
            var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
            
            if (gameView == null)
            {
                throw new System.Exception("Game View not found. Please open a Game View window.");
            }
            
            // Get the camera
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }
            
            if (camera == null)
            {
                throw new System.Exception("No camera found in scene. Add a camera to capture Game View.");
            }
            
            return CaptureCamera(camera, width, height);
        }
        
        private static byte[] CaptureSceneView(int width, int height)
        {
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            
            if (sceneView == null)
            {
                throw new System.Exception("No active Scene View found. Please open a Scene View window.");
            }
            
            return CaptureCamera(sceneView.camera, width, height);
        }
        
        private static byte[] CaptureCamera(Camera camera, int width, int height)
        {
            // Create render texture
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture previousRT = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            
            try
            {
                // Render camera to texture
                camera.targetTexture = rt;
                camera.Render();
                
                // Read pixels
                RenderTexture.active = rt;
                Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
                screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenshot.Apply();
                
                // Encode to PNG
                byte[] bytes = screenshot.EncodeToPNG();
                
                // Cleanup
                UnityEngine.Object.DestroyImmediate(screenshot);
                rt.Release();
                
                return bytes;
            }
            finally
            {
                // Restore
                camera.targetTexture = previousRT;
                RenderTexture.active = previousActive;
            }
        }
    }
}

