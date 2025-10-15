using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Linq;
using TMPro;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

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
                    
                    case "unity_list_all_gameobjects":
                        return ListAllGameObjects();

                    case "unity_create_cube":
                        return CreateCube(args);
                    
                    case "unity_create_primitive":
                        return CreatePrimitive(args);
                    
                    case "unity_save_prefab":
                        return SaveAsPrefab(args);
                    
                    case "unity_add_script_component":
                        return AddScriptComponent(args);
                    
                    case "unity_update_prefab":
                        return UpdatePrefab(args);
                    
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
                    
                    // UI Tools - Canvas Management
                    case "unity_ui_create_canvas":
                        return UI_CreateCanvas(args);
                    
                    case "unity_ui_setup_canvas_scaler":
                        return UI_SetupCanvasScaler(args);
                    
                    case "unity_ui_create_event_system":
                        return UI_CreateEventSystem(args);
                    
                    // UI Tools - Elements
                    case "unity_ui_create_button":
                        return UI_CreateButton(args);
                    
                    case "unity_ui_create_text":
                        return UI_CreateText(args);
                    
                    case "unity_ui_create_image":
                        return UI_CreateImage(args);
                    
                    case "unity_ui_create_panel":
                        return UI_CreatePanel(args);
                    
                    // UI Tools - Layout
                    case "unity_ui_create_vertical_layout":
                        return UI_CreateVerticalLayout(args);
                    
                    case "unity_ui_create_horizontal_layout":
                        return UI_CreateHorizontalLayout(args);
                    
                    case "unity_ui_create_grid_layout":
                        return UI_CreateGridLayout(args);
                    
                    case "unity_ui_set_sprite":
                        return UI_SetSprite(args);
                    
                    // GameObject Management
                    case "unity_delete_gameobject":
                        return DeleteGameObject(args);
                    
                    case "unity_find_gameobject":
                        return FindGameObject(args);
                    
                    case "unity_set_position":
                        return SetPosition(args);
                    
                    case "unity_set_rotation":
                        return SetRotation(args);
                    
                    case "unity_set_scale":
                        return SetScale(args);
                    
                    case "unity_set_tag":
                        return SetTag(args);

                    case "unity_set_anchors":
                        return SetAnchors(args);
                    
                    case "unity_set_camera_background":
                        return SetCameraBackground(args);
                    
                    case "unity_set_ui_size":
                        return SetUISize(args);
                    
                    case "unity_set_image_fill":
                        return SetImageFill(args);
                    
                    // Scene Management
                    case "unity_create_scene":
                        return CreateScene(args);
                    
                    case "unity_save_scene":
                        return SaveScene(args);
                    
                    case "unity_load_scene":
                        return LoadScene(args);
                    
                    case "unity_add_scene_to_build":
                        return AddSceneToBuild(args);
                    
                    // Script Management
                    case "unity_create_script":
                        return CreateScript(args);
                    
                    case "unity_add_component":
                        return AddComponent(args);
                    
                    case "unity_remove_component":
                        return RemoveComponent(args);
                    
                    case "unity_set_component_property":
                        return SetComponentProperty(args);
                    
                    // Button Events
                    case "unity_set_button_onclick":
                        return SetButtonOnClick(args);
                    
                    // Test/Debug Tools
                    case "unity_test_log":
                        return TestLog(args);
                    
                    case "unity_restart_server":
                        return RestartServer();
                    
                    case "unity_add_particle_trail":
                        return AddParticleTrail(args);
                    
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
        
        private static JObject ListAllGameObjects()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            
            var allObjects = new JArray();
            
            void AddGameObjectAndChildren(GameObject go, string parentPath = "")
            {
                string path = string.IsNullOrEmpty(parentPath) ? go.name : $"{parentPath}/{go.name}";
                
                allObjects.Add(new JObject
                {
                    ["name"] = go.name,
                    ["path"] = path,
                    ["active"] = go.activeInHierarchy,
                    ["tag"] = go.tag,
                    ["layer"] = LayerMask.LayerToName(go.layer),
                    ["position"] = new JArray { go.transform.position.x, go.transform.position.y, go.transform.position.z }
                });
                
                // Recursively add children
                foreach (Transform child in go.transform)
                {
                    AddGameObjectAndChildren(child.gameObject, path);
                }
            }
            
            foreach (var rootObject in rootObjects)
            {
                AddGameObjectAndChildren(rootObject);
            }
            
            return new JObject
            {
                ["success"] = true,
                ["sceneName"] = scene.name,
                ["totalObjects"] = allObjects.Count,
                ["objects"] = allObjects
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
        
        // Create Primitive (Sphere, Capsule, Cylinder, etc.)
        private static JObject CreatePrimitive(JObject args)
        {
            string name = args["name"]?.ToString() ?? "Primitive";
            string primitiveType = args["primitiveType"]?.ToString() ?? "Cube";
            JArray posArray = args["position"] as JArray;
            
            Vector3 position = Vector3.zero;
            if (posArray != null && posArray.Count == 3)
            {
                position = new Vector3(
                    posArray[0].ToObject<float>(),
                    posArray[1].ToObject<float>(),
                    posArray[2].ToObject<float>()
                );
            }
            
            PrimitiveType type;
            switch (primitiveType.ToLower())
            {
                case "sphere":
                    type = PrimitiveType.Sphere;
                    break;
                case "capsule":
                    type = PrimitiveType.Capsule;
                    break;
                case "cylinder":
                    type = PrimitiveType.Cylinder;
                    break;
                case "plane":
                    type = PrimitiveType.Plane;
                    break;
                case "quad":
                    type = PrimitiveType.Quad;
                    break;
                case "cube":
                default:
                    type = PrimitiveType.Cube;
                    break;
            }
            
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.position = position;
            
            Undo.RegisterCreatedObjectUndo(primitive, $"Create {name}");
            Selection.activeGameObject = primitive;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            Debug.Log($"[MCP] Created {primitiveType} primitive '{name}' at {position}");
            
            return new JObject
            {
                ["success"] = true,
                ["name"] = primitive.name,
                ["primitiveType"] = primitiveType,
                ["position"] = new JArray { position.x, position.y, position.z },
                ["instanceId"] = primitive.GetInstanceID()
            };
        }
        
        // Tool 4: Force Compile
        private static JObject ForceCompile()
        {
            try
            {
                Debug.Log("[MCP] Force compile requested");
                
                // Step 1: Refresh asset database to detect any file changes
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Debug.Log("[MCP] AssetDatabase.Refresh() called");
                
                // Step 2: Request script compilation
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                Debug.Log("[MCP] CompilationPipeline.RequestScriptCompilation() called");
                
                // Give Unity a moment to start compilation
                System.Threading.Thread.Sleep(100);
                
                bool isCompiling = EditorApplication.isCompiling;
                
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = isCompiling ? "Compilation started" : "Compilation requested (may already be up to date)",
                    ["isCompiling"] = isCompiling
                };
            }
            catch (System.Exception e)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = $"Force compile failed: {e.Message}"
                };
            }
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
                string screenshotsDir = System.IO.Path.Combine(Application.dataPath, "..", "Screenshots");
                System.IO.Directory.CreateDirectory(screenshotsDir);
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                outputPath = System.IO.Path.Combine(screenshotsDir, $"screenshot_{timestamp}.png");
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
            // For Game View with UI overlays, we need to temporarily convert overlay canvases
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }
            
            if (camera == null)
            {
                throw new System.Exception("No camera found in scene. Add a camera to capture Game View.");
            }
            
            // Store canvas states and temporarily convert overlays to camera space
            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            System.Collections.Generic.List<(Canvas canvas, RenderMode originalMode, Camera originalCamera)> canvasStates = 
                new System.Collections.Generic.List<(Canvas, RenderMode, Camera)>();
            
            foreach (Canvas canvas in canvases)
            {
                canvasStates.Add((canvas, canvas.renderMode, canvas.worldCamera));
                
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Temporarily convert to camera space
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
                {
                    canvas.worldCamera = camera;
                }
            }
            
            // Create render texture
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture previousRT = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            
            try
            {
                // Render camera with UI to texture
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
                // Restore camera
                camera.targetTexture = previousRT;
                RenderTexture.active = previousActive;
                
                // Restore all canvas states
                foreach (var state in canvasStates)
                {
                    state.canvas.renderMode = state.originalMode;
                    state.canvas.worldCamera = state.originalCamera;
                }
            }
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
        
        // ==================== TEST/DEBUG TOOLS ====================
        
        private static JObject TestLog(JObject args)
        {
            string message = args["message"]?.ToString() ?? "Test log from MCP";
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            string fullMessage = $"🧪 TEST LOG [{timestamp}]: {message}";
            
            Debug.Log(fullMessage);
            Debug.LogWarning($"⚠️  TEST WARNING [{timestamp}]: This is a test warning");
            
            // RANDOM TEST MESSAGE - This proves compilation worked!
            Debug.Log($"🎲 RANDOM TEST: The magic number is 77777 and compilation timestamp is {timestamp}");
            Debug.Log($"💪 AUTO-RESTART TEST: Server auto-restart working! Vinh's Game is ready!");
            
            return new JObject
            {
                ["success"] = true,
                ["message"] = "Test log written to Unity console",
                ["timestamp"] = timestamp,
                ["loggedMessage"] = fullMessage,
                ["randomTestNumber"] = 77777,
                ["autoRestartWorking"] = true
            };
        }
        
        private static JObject RestartServer()
        {
            try
            {
                Debug.Log("[MCP] Manual server restart requested - will restart after sending response");
                
                // Schedule restart after this request completes
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("[MCP] Stopping server...");
                    MCPServer.StopServer();
                    
                    // Wait a moment then start
                    EditorApplication.delayCall += () =>
                    {
                        Debug.Log("[MCP] Starting server...");
                        MCPServer.StartServer();
                    };
                };
                
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = "MCP Server restart scheduled (will restart after response is sent)"
                };
            }
            catch (System.Exception e)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = $"Failed to schedule server restart: {e.Message}"
                };
            }
        }
        
        // ==================== SCENE MANAGEMENT ====================
        
        private static JObject CreateScene(JObject args)
        {
            string name = args["name"]?.ToString();
            string path = args["path"]?.ToString() ?? "Assets/Scenes/";
            string setup = args["setup"]?.ToString() ?? "Default";
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Scene name is required"
                };
            }
            
            try
            {
                // Ensure directory exists
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                
                // Create new scene
                UnityEngine.SceneManagement.Scene newScene;
                if (setup == "Empty")
                {
                    newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }
                else
                {
                    newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                }
                
                // Save the scene
                string fullPath = System.IO.Path.Combine(path, name + ".unity");
                bool saved = EditorSceneManager.SaveScene(newScene, fullPath);
                
                if (!saved)
                {
                    throw new System.Exception("Failed to save scene");
                }
                
                Debug.Log($"[MCP] Created scene: {fullPath}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["sceneName"] = name,
                    ["scenePath"] = fullPath
                };
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
        
        private static JObject SaveScene(JObject args)
        {
            string path = args["path"]?.ToString();
            
            try
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                
                bool saved;
                if (!string.IsNullOrEmpty(path))
                {
                    saved = EditorSceneManager.SaveScene(activeScene, path);
                }
                else
                {
                    saved = EditorSceneManager.SaveScene(activeScene);
                }
                
                if (!saved)
                {
                    throw new System.Exception("Failed to save scene");
                }
                
                Debug.Log($"[MCP] Saved scene: {activeScene.path}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["scenePath"] = activeScene.path,
                    ["sceneName"] = activeScene.name
                };
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
        
        private static JObject LoadScene(JObject args)
        {
            string sceneName = args["sceneName"]?.ToString();
            
            if (string.IsNullOrEmpty(sceneName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Scene name is required"
                };
            }
            
            try
            {
                // Find scene path
                string[] sceneGUIDs = AssetDatabase.FindAssets($"{sceneName} t:Scene");
                
                if (sceneGUIDs.Length == 0)
                {
                    throw new System.Exception($"Scene '{sceneName}' not found");
                }
                
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUIDs[0]);
                
                // Load the scene
                var loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                
                Debug.Log($"[MCP] Loaded scene: {scenePath}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["sceneName"] = loadedScene.name,
                    ["scenePath"] = loadedScene.path,
                    ["rootObjectCount"] = loadedScene.rootCount
                };
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
        
        private static JObject AddSceneToBuild(JObject args)
        {
            string scenePath = args["scenePath"]?.ToString();
            
            if (string.IsNullOrEmpty(scenePath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Scene path is required"
                };
            }
            
            try
            {
                // Get existing scenes in build settings
                var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                
                // Check if scene is already in build settings
                bool alreadyExists = false;
                foreach (var scene in scenes)
                {
                    if (scene.path == scenePath)
                    {
                        alreadyExists = true;
                        break;
                    }
                }
                
                if (alreadyExists)
                {
                    Debug.Log($"[MCP] Scene '{scenePath}' is already in build settings");
                    return new JObject
                    {
                        ["success"] = true,
                        ["message"] = $"Scene '{scenePath}' is already in build settings",
                        ["buildIndex"] = System.Array.FindIndex(scenes.ToArray(), s => s.path == scenePath)
                    };
                }
                
                // Add scene to build settings
                var newScene = new EditorBuildSettingsScene(scenePath, true);
                scenes.Add(newScene);
                EditorBuildSettings.scenes = scenes.ToArray();
                
                int buildIndex = scenes.Count - 1;
                
                Debug.Log($"[MCP] Added scene '{scenePath}' to build settings at index {buildIndex}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["scenePath"] = scenePath,
                    ["buildIndex"] = buildIndex,
                    ["totalScenes"] = scenes.Count
                };
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
        
        // ==================== GAMEOBJECT MANAGEMENT ====================
        
        private static JObject DeleteGameObject(JObject args)
        {
            string name = args["name"]?.ToString();
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                string path = GetGameObjectPath(obj);
                Undo.DestroyObjectImmediate(obj);
                
                Debug.Log($"[MCP] Deleted GameObject: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = $"Deleted GameObject: {path}"
                };
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
        
        private static JObject FindGameObject(JObject args)
        {
            string name = args["name"]?.ToString();
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                string path = GetGameObjectPath(obj);
                Vector3 pos = obj.transform.position;
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = obj.name,
                    ["path"] = path,
                    ["position"] = new JArray { pos.x, pos.y, pos.z },
                    ["active"] = obj.activeSelf
                };
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
        
        private static JObject SetPosition(JObject args)
        {
            string name = args["name"]?.ToString();
            JArray positionArray = args["position"] as JArray;
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (positionArray == null || positionArray.Count != 3)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Position must be an array of 3 numbers [x, y, z]"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                Vector3 newPos = new Vector3(
                    positionArray[0].ToObject<float>(),
                    positionArray[1].ToObject<float>(),
                    positionArray[2].ToObject<float>()
                );
                
                Undo.RecordObject(obj.transform, "Set Position");
                obj.transform.position = newPos;
                
                Debug.Log($"[MCP] Set position of '{name}' to {newPos}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["newPosition"] = new JArray { newPos.x, newPos.y, newPos.z }
                };
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
        
        private static JObject SetRotation(JObject args)
        {
            string name = args["name"]?.ToString();
            JArray rotationArray = args["rotation"] as JArray;
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (rotationArray == null || rotationArray.Count != 3)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Rotation must be an array of 3 numbers [x, y, z] in degrees"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                Vector3 newRot = new Vector3(
                    rotationArray[0].ToObject<float>(),
                    rotationArray[1].ToObject<float>(),
                    rotationArray[2].ToObject<float>()
                );
                
                Undo.RecordObject(obj.transform, "Set Rotation");
                obj.transform.eulerAngles = newRot;
                
                Debug.Log($"[MCP] Set rotation of '{name}' to {newRot}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["newRotation"] = new JArray { newRot.x, newRot.y, newRot.z }
                };
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
        
        private static JObject SetScale(JObject args)
        {
            string name = args["name"]?.ToString();
            JArray scaleArray = args["scale"] as JArray;
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (scaleArray == null || scaleArray.Count != 3)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Scale must be an array of 3 numbers [x, y, z]"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                Vector3 newScale = new Vector3(
                    scaleArray[0].ToObject<float>(),
                    scaleArray[1].ToObject<float>(),
                    scaleArray[2].ToObject<float>()
                );
                
                Undo.RecordObject(obj.transform, "Set Scale");
                obj.transform.localScale = newScale;
                
                Debug.Log($"[MCP] Set scale of '{name}' to {newScale}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["newScale"] = new JArray { newScale.x, newScale.y, newScale.z }
                };
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
        
        private static JObject SetTag(JObject args)
        {
            string name = args["name"]?.ToString();
            string tag = args["tag"]?.ToString();
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (string.IsNullOrEmpty(tag))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Tag is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                // Check if tag exists, if not try to add it
                bool tagExists = false;
                foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
                {
                    if (t == tag)
                    {
                        tagExists = true;
                        break;
                    }
                }
                
                if (!tagExists)
                {
                    // Add tag using SerializedObject
                    SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                    SerializedProperty tagsProp = tagManager.FindProperty("tags");
                    
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                    newTag.stringValue = tag;
                    tagManager.ApplyModifiedProperties();
                    
                    Debug.Log($"[MCP] Created new tag '{tag}'");
                }
                
                Undo.RecordObject(obj, "Set Tag");
                obj.tag = tag;
                
                Debug.Log($"[MCP] Set tag of '{name}' to '{tag}'");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["tag"] = tag
                };
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
        
        private static JObject SetAnchors(JObject args)
        {
            string name = args["name"]?.ToString();
            string preset = args["preset"]?.ToString();
            JArray anchorMinArray = args["anchorMin"] as JArray;
            JArray anchorMaxArray = args["anchorMax"] as JArray;
            JArray anchoredPosArray = args["anchoredPosition"] as JArray;
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' does not have a RectTransform (not a UI element)"
                    };
                }
                
                Undo.RecordObject(rectTransform, "Set Anchors");
                
                // Apply preset if provided
                if (!string.IsNullOrEmpty(preset))
                {
                    switch (preset.ToLower())
                    {
                        case "top-left":
                            rectTransform.anchorMin = new Vector2(0, 1);
                            rectTransform.anchorMax = new Vector2(0, 1);
                            break;
                        case "top-center":
                            rectTransform.anchorMin = new Vector2(0.5f, 1);
                            rectTransform.anchorMax = new Vector2(0.5f, 1);
                            break;
                        case "top-right":
                            rectTransform.anchorMin = new Vector2(1, 1);
                            rectTransform.anchorMax = new Vector2(1, 1);
                            break;
                        case "middle-left":
                            rectTransform.anchorMin = new Vector2(0, 0.5f);
                            rectTransform.anchorMax = new Vector2(0, 0.5f);
                            break;
                        case "center":
                            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                            break;
                        case "middle-right":
                            rectTransform.anchorMin = new Vector2(1, 0.5f);
                            rectTransform.anchorMax = new Vector2(1, 0.5f);
                            break;
                        case "bottom-left":
                            rectTransform.anchorMin = new Vector2(0, 0);
                            rectTransform.anchorMax = new Vector2(0, 0);
                            break;
                        case "bottom-center":
                            rectTransform.anchorMin = new Vector2(0.5f, 0);
                            rectTransform.anchorMax = new Vector2(0.5f, 0);
                            break;
                        case "bottom-right":
                            rectTransform.anchorMin = new Vector2(1, 0);
                            rectTransform.anchorMax = new Vector2(1, 0);
                            break;
                        case "stretch-horizontal":
                            rectTransform.anchorMin = new Vector2(0, 0.5f);
                            rectTransform.anchorMax = new Vector2(1, 0.5f);
                            break;
                        case "stretch-vertical":
                            rectTransform.anchorMin = new Vector2(0.5f, 0);
                            rectTransform.anchorMax = new Vector2(0.5f, 1);
                            break;
                        case "stretch-all":
                            rectTransform.anchorMin = new Vector2(0, 0);
                            rectTransform.anchorMax = new Vector2(1, 1);
                            break;
                        default:
                            return new JObject
                            {
                                ["success"] = false,
                                ["error"] = $"Unknown preset '{preset}'. Valid presets: top-left, top-center, top-right, middle-left, center, middle-right, bottom-left, bottom-center, bottom-right, stretch-horizontal, stretch-vertical, stretch-all"
                            };
                    }
                }
                
                // Apply custom anchors if provided
                if (anchorMinArray != null && anchorMinArray.Count == 2)
                {
                    rectTransform.anchorMin = new Vector2(
                        anchorMinArray[0].ToObject<float>(),
                        anchorMinArray[1].ToObject<float>()
                    );
                }
                
                if (anchorMaxArray != null && anchorMaxArray.Count == 2)
                {
                    rectTransform.anchorMax = new Vector2(
                        anchorMaxArray[0].ToObject<float>(),
                        anchorMaxArray[1].ToObject<float>()
                    );
                }
                
                // Apply anchored position if provided
                if (anchoredPosArray != null && anchoredPosArray.Count == 2)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        anchoredPosArray[0].ToObject<float>(),
                        anchoredPosArray[1].ToObject<float>()
                    );
                }
                
                EditorUtility.SetDirty(obj);
                
                Debug.Log($"[MCP] Set anchors for '{name}' - Min: {rectTransform.anchorMin}, Max: {rectTransform.anchorMax}, Pos: {rectTransform.anchoredPosition}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["anchorMin"] = new JArray { rectTransform.anchorMin.x, rectTransform.anchorMin.y },
                    ["anchorMax"] = new JArray { rectTransform.anchorMax.x, rectTransform.anchorMax.y },
                    ["anchoredPosition"] = new JArray { rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y }
                };
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
        
        private static JObject SetCameraBackground(JObject args)
        {
            string cameraName = args["cameraName"]?.ToString() ?? "Main Camera";
            string clearFlags = args["clearFlags"]?.ToString();
            JArray colorArray = args["backgroundColor"] as JArray;
            
            try
            {
                Camera camera = null;
                
                // Try to find the camera
                GameObject cameraObj = GameObject.Find(cameraName);
                if (cameraObj != null)
                {
                    camera = cameraObj.GetComponent<Camera>();
                }
                
                // If not found by name, try Camera.main
                if (camera == null && cameraName == "Main Camera")
                {
                    camera = Camera.main;
                }
                
                if (camera == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Camera '{cameraName}' not found"
                    };
                }
                
                Undo.RecordObject(camera, "Set Camera Background");
                
                // Set clear flags
                if (!string.IsNullOrEmpty(clearFlags))
                {
                    switch (clearFlags.ToLower())
                    {
                        case "skybox":
                            camera.clearFlags = CameraClearFlags.Skybox;
                            break;
                        case "solidcolor":
                        case "solid":
                            camera.clearFlags = CameraClearFlags.SolidColor;
                            break;
                        case "depth":
                            camera.clearFlags = CameraClearFlags.Depth;
                            break;
                        case "nothing":
                            camera.clearFlags = CameraClearFlags.Nothing;
                            break;
                        default:
                            return new JObject
                            {
                                ["success"] = false,
                                ["error"] = $"Unknown clearFlags '{clearFlags}'. Valid options: skybox, solidcolor, depth, nothing"
                            };
                    }
                }
                
                // Set background color
                if (colorArray != null && colorArray.Count >= 3)
                {
                    float r = colorArray[0].ToObject<float>();
                    float g = colorArray[1].ToObject<float>();
                    float b = colorArray[2].ToObject<float>();
                    float a = colorArray.Count >= 4 ? colorArray[3].ToObject<float>() : 1f;
                    
                    camera.backgroundColor = new Color(r, g, b, a);
                }
                
                EditorUtility.SetDirty(camera);
                
                Debug.Log($"[MCP] Set camera '{cameraName}' - ClearFlags: {camera.clearFlags}, BackgroundColor: {camera.backgroundColor}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["cameraName"] = cameraName,
                    ["clearFlags"] = camera.clearFlags.ToString(),
                    ["backgroundColor"] = new JArray { camera.backgroundColor.r, camera.backgroundColor.g, camera.backgroundColor.b, camera.backgroundColor.a }
                };
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
        
        private static JObject SetUISize(JObject args)
        {
            string name = args["name"]?.ToString();
            JArray sizeArray = args["size"] as JArray;
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (sizeArray == null || sizeArray.Count != 2)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Size must be an array of [width, height]"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' does not have a RectTransform (not a UI element)"
                    };
                }
                
                Undo.RecordObject(rectTransform, "Set UI Size");
                
                float width = sizeArray[0].ToObject<float>();
                float height = sizeArray[1].ToObject<float>();
                
                rectTransform.sizeDelta = new Vector2(width, height);
                
                EditorUtility.SetDirty(obj);
                
                Debug.Log($"[MCP] Set size for '{name}' to {width}x{height}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["size"] = new JArray { rectTransform.sizeDelta.x, rectTransform.sizeDelta.y }
                };
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
        
        private static JObject SetImageFill(JObject args)
        {
            string name = args["name"]?.ToString();
            string fillMethod = args["fillMethod"]?.ToString();
            string fillOrigin = args["fillOrigin"]?.ToString();
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image>();
                if (image == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' does not have an Image component"
                    };
                }
                
                Undo.RecordObject(image, "Set Image Fill");
                
                // Set to Filled type
                image.type = UnityEngine.UI.Image.Type.Filled;
                
                // Set fill method
                if (!string.IsNullOrEmpty(fillMethod))
                {
                    switch (fillMethod.ToLower())
                    {
                        case "horizontal":
                            image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
                            break;
                        case "vertical":
                            image.fillMethod = UnityEngine.UI.Image.FillMethod.Vertical;
                            break;
                        case "radial90":
                            image.fillMethod = UnityEngine.UI.Image.FillMethod.Radial90;
                            break;
                        case "radial180":
                            image.fillMethod = UnityEngine.UI.Image.FillMethod.Radial180;
                            break;
                        case "radial360":
                            image.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
                            break;
                        default:
                            return new JObject
                            {
                                ["success"] = false,
                                ["error"] = $"Unknown fill method '{fillMethod}'. Valid options: horizontal, vertical, radial90, radial180, radial360"
                            };
                    }
                }
                
                // Set fill origin
                if (!string.IsNullOrEmpty(fillOrigin))
                {
                    if (image.fillMethod == UnityEngine.UI.Image.FillMethod.Radial360)
                    {
                        switch (fillOrigin.ToLower())
                        {
                            case "bottom":
                                image.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Bottom;
                                break;
                            case "right":
                                image.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Right;
                                break;
                            case "top":
                                image.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
                                break;
                            case "left":
                                image.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Left;
                                break;
                        }
                    }
                }
                
                image.fillAmount = 1.0f; // Start full
                
                EditorUtility.SetDirty(obj);
                
                Debug.Log($"[MCP] Set Image fill for '{name}' - Method: {image.fillMethod}, Origin: {image.fillOrigin}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["fillMethod"] = image.fillMethod.ToString(),
                    ["fillOrigin"] = image.fillOrigin
                };
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
        
        // ==================== SCRIPT MANAGEMENT ====================
        
        private static JObject CreateScript(JObject args)
        {
            string name = args["name"]?.ToString();
            string content = args["content"]?.ToString();
            string path = args["path"]?.ToString() ?? "Assets/Scripts/";
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Script name is required"
                };
            }
            
            if (string.IsNullOrEmpty(content))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Script content is required"
                };
            }
            
            try
            {
                // Ensure directory exists
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                
                // Ensure .cs extension
                if (!name.EndsWith(".cs"))
                {
                    name += ".cs";
                }
                
                string fullPath = System.IO.Path.Combine(path, name);
                
                // Write script file
                System.IO.File.WriteAllText(fullPath, content);
                AssetDatabase.Refresh();
                
                Debug.Log($"[MCP] Created script: {fullPath}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["scriptPath"] = fullPath,
                    ["scriptName"] = name
                };
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
        
        private static JObject AddComponent(JObject args)
        {
            string gameObjectName = args["gameObjectName"]?.ToString();
            string componentType = args["componentType"]?.ToString();
            
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (string.IsNullOrEmpty(componentType))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Component type is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(gameObjectName);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{gameObjectName}' not found"
                    };
                }
                
                // Try to add component by type name
                System.Type type = System.Type.GetType(componentType + ", Assembly-CSharp") 
                                ?? System.Type.GetType(componentType + ", UnityEngine");
                
                if (type == null)
                {
                    // Try searching all assemblies
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(componentType);
                        if (type != null) break;
                    }
                }
                
                if (type == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Component type '{componentType}' not found. Make sure the script has been compiled."
                    };
                }
                
                Component component = Undo.AddComponent(obj, type);
                
                Debug.Log($"[MCP] Added component '{componentType}' to '{gameObjectName}'");
                
                return new JObject
                {
                    ["success"] = true,
                    ["gameObject"] = gameObjectName,
                    ["component"] = componentType
                };
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
        
        private static JObject RemoveComponent(JObject args)
        {
            string gameObjectName = args["gameObjectName"]?.ToString();
            string componentType = args["componentType"]?.ToString();
            
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (string.IsNullOrEmpty(componentType))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Component type is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(gameObjectName);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{gameObjectName}' not found"
                    };
                }
                
                // Try to find component by type name
                System.Type type = System.Type.GetType(componentType + ", Assembly-CSharp") 
                                ?? System.Type.GetType(componentType + ", UnityEngine")
                                ?? System.Type.GetType("UnityEngine." + componentType + ", UnityEngine");
                
                if (type == null)
                {
                    // Try searching all assemblies
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(componentType);
                        if (type != null) break;
                    }
                }
                
                if (type == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Component type '{componentType}' not found"
                    };
                }
                
                Component component = obj.GetComponent(type);
                if (component == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Component '{componentType}' not found on GameObject '{gameObjectName}'"
                    };
                }
                
                Undo.DestroyObjectImmediate(component);
                
                Debug.Log($"[MCP] Removed component '{componentType}' from '{gameObjectName}'");
                
                return new JObject
                {
                    ["success"] = true,
                    ["gameObject"] = gameObjectName,
                    ["component"] = componentType,
                    ["message"] = $"Removed {componentType} from {gameObjectName}"
                };
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
        
        private static JObject SetComponentProperty(JObject args)
        {
            string gameObjectName = args["gameObjectName"]?.ToString();
            string componentType = args["componentType"]?.ToString();
            string propertyName = args["propertyName"]?.ToString();
            JToken valueToken = args["value"];
            
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (string.IsNullOrEmpty(componentType))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Component type is required"
                };
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Property name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(gameObjectName);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{gameObjectName}' not found"
                    };
                }
                
                // Find component
                System.Type type = System.Type.GetType(componentType + ", Assembly-CSharp") 
                                ?? System.Type.GetType(componentType + ", UnityEngine")
                                ?? System.Type.GetType("UnityEngine." + componentType + ", UnityEngine")
                                ?? System.Type.GetType("UnityEngine.UI." + componentType + ", Unity.ugui")
                                ?? System.Type.GetType("TMPro." + componentType + ", Unity.TextMeshPro");
                
                if (type == null)
                {
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(componentType);
                        if (type != null) break;
                    }
                }
                
                if (type == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Component type '{componentType}' not found"
                    };
                }
                
                Component component = obj.GetComponent(type);
                if (component == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Component '{componentType}' not found on GameObject '{gameObjectName}'"
                    };
                }
                
                // Use SerializedObject for proper Undo support
                SerializedObject so = new SerializedObject(component);
                SerializedProperty prop = so.FindProperty(propertyName);
                
                if (prop == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Property '{propertyName}' not found on component '{componentType}'"
                    };
                }
                
                // Set value based on property type
                bool wasSet = false;
                
                Debug.Log($"[MCP] Property type: {prop.propertyType}, ValueToken type: {valueToken.Type}");
                
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // Handle object references (prefabs, assets, GameObjects)
                    JObject valueObj = null;
                    
                    if (valueToken.Type == JTokenType.Object)
                    {
                        valueObj = valueToken as JObject;
                    }
                    else if (valueToken.Type == JTokenType.String)
                    {
                        // Try to parse as JSON if it looks like JSON
                        string valueStr = valueToken.ToString();
                        if (valueStr.StartsWith("{"))
                        {
                            try
                            {
                                valueObj = JObject.Parse(valueStr);
                                Debug.Log($"[MCP] Parsed JSON string into object");
                            }
                            catch
                            {
                                // Not valid JSON, treat as GameObject name
                            }
                        }
                    }
                    
                    if (valueObj != null)
                    {
                        string refType = valueObj["type"]?.ToString();
                        string refPath = valueObj["path"]?.ToString();
                        
                        if (refType == "reference" && !string.IsNullOrEmpty(refPath))
                        {
                            Debug.Log($"[MCP] Attempting to load asset from: {refPath}");
                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(refPath);
                            if (asset != null)
                            {
                                prop.objectReferenceValue = asset;
                                wasSet = true;
                                Debug.Log($"[MCP] ✓ Loaded asset: {asset.name} (type: {asset.GetType().Name})");
                            }
                            else
                            {
                                Debug.LogError($"[MCP] ✗ Could not load asset from path: {refPath}");
                                return new JObject
                                {
                                    ["success"] = false,
                                    ["error"] = $"Could not load asset from path: {refPath}"
                                };
                            }
                        }
                    }
                    else if (valueToken.Type == JTokenType.String)
                    {
                        // Try to find GameObject by name
                        string valueStr = valueToken.ToString();
                        GameObject refObj = GameObject.Find(valueStr);
                        if (refObj != null)
                        {
                            prop.objectReferenceValue = refObj;
                            wasSet = true;
                        }
                    }
                }
                else if (valueToken.Type == JTokenType.String)
                {
                    prop.stringValue = valueToken.ToString();
                    wasSet = true;
                }
                else if (valueToken.Type == JTokenType.Integer)
                {
                    prop.intValue = valueToken.ToObject<int>();
                    wasSet = true;
                }
                else if (valueToken.Type == JTokenType.Float)
                {
                    prop.floatValue = valueToken.ToObject<float>();
                    wasSet = true;
                }
                else if (valueToken.Type == JTokenType.Boolean)
                {
                    prop.boolValue = valueToken.ToObject<bool>();
                    wasSet = true;
                }
                
                if (!wasSet)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Could not set property '{propertyName}' - incompatible types"
                    };
                }
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                
                // Verify the property was actually set
                so.Update();
                SerializedProperty verifyProp = so.FindProperty(propertyName);
                if (verifyProp != null && verifyProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (verifyProp.objectReferenceValue == null)
                    {
                        Debug.LogError($"[MCP] ✗ Property '{propertyName}' is still null after setting!");
                        return new JObject
                        {
                            ["success"] = false,
                            ["error"] = $"Property '{propertyName}' is still null after setting"
                        };
                    }
                    else
                    {
                        Debug.Log($"[MCP] ✓ Verified property '{propertyName}' = {verifyProp.objectReferenceValue.name}");
                    }
                }
                
                Debug.Log($"[MCP] Set property '{propertyName}' on '{componentType}' of '{gameObjectName}'");
                
                return new JObject
                {
                    ["success"] = true,
                    ["gameObject"] = gameObjectName,
                    ["component"] = componentType,
                    ["property"] = propertyName
                };
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
        
        // ==================== BUTTON EVENT HANDLERS ====================
        
        private static JObject SetButtonOnClick(JObject args)
        {
            string buttonName = args["buttonName"]?.ToString();
            string action = args["action"]?.ToString();
            string parameter = args["parameter"]?.ToString();
            
            if (string.IsNullOrEmpty(buttonName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Button name is required"
                };
            }
            
            if (string.IsNullOrEmpty(action))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Action is required"
                };
            }
            
            try
            {
                GameObject buttonObj = GameObject.Find(buttonName);
                if (buttonObj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Button '{buttonName}' not found"
                    };
                }
                
                UnityEngine.UI.Button button = buttonObj.GetComponent<UnityEngine.UI.Button>();
                if (button == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{buttonName}' does not have a Button component"
                    };
                }
                
                // Find or create SceneLoader component
                GameObject sceneLoaderObj = GameObject.Find("SceneLoader");
                if (sceneLoaderObj == null)
                {
                    sceneLoaderObj = new GameObject("SceneLoader");
                }
                
                // Check if SceneLoader component exists
                UnityEngine.Component sceneLoader = sceneLoaderObj.GetComponent("SceneLoader");
                if (sceneLoader == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = "SceneLoader script not found. Create it first using unity_create_script."
                    };
                }
                
                // Clear all existing persistent listeners
                int listenerCount = button.onClick.GetPersistentEventCount();
                for (int i = listenerCount - 1; i >= 0; i--)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
                }
                
                // Add persistent listener based on action type
                if (action == "LoadScene")
                {
                    if (string.IsNullOrEmpty(parameter))
                    {
                        return new JObject
                        {
                            ["success"] = false,
                            ["error"] = "Scene name parameter is required for LoadScene action"
                        };
                    }
                    
                    // Add persistent listener with string parameter
                    UnityEditor.Events.UnityEventTools.AddStringPersistentListener(
                        button.onClick,
                        (UnityEngine.Events.UnityAction<string>)System.Delegate.CreateDelegate(
                            typeof(UnityEngine.Events.UnityAction<string>),
                            sceneLoader,
                            "LoadScene"
                        ),
                        parameter
                    );
                    
                    Debug.Log($"[MCP] Set button '{buttonName}' to load scene '{parameter}' (persistent)");
                }
                else if (action == "Quit")
                {
                    // Add persistent listener with no parameters
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(
                        button.onClick,
                        (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                            typeof(UnityEngine.Events.UnityAction),
                            sceneLoader,
                            "QuitGame"
                        )
                    );
                    
                    Debug.Log($"[MCP] Set button '{buttonName}' to quit game (persistent)");
                }
                else
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Unknown action: {action}. Supported: LoadScene, Quit"
                    };
                }
                
                EditorUtility.SetDirty(button);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                
                return new JObject
                {
                    ["success"] = true,
                    ["button"] = buttonName,
                    ["action"] = action,
                    ["parameter"] = parameter ?? ""
                };
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
        
        // ==================== UI TOOLS - CANVAS MANAGEMENT ====================
        
        private static JObject UI_CreateCanvas(JObject args)
        {
            string name = args["name"]?.ToString() ?? "Canvas";
            string renderMode = args["renderMode"]?.ToString() ?? "ScreenSpaceOverlay";
            int sortingOrder = args["sortingOrder"]?.ToObject<int>() ?? 0;
            bool pixelPerfect = args["pixelPerfect"]?.ToObject<bool>() ?? false;
            string preset = args["preset"]?.ToString();
            
            try
            {
                // Check if canvas already exists
                Canvas existingCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (existingCanvas != null && existingCanvas.name == name)
                {
                    Debug.Log($"[MCP] Canvas '{name}' already exists");
                    return new JObject
                    {
                        ["success"] = true,
                        ["message"] = "Canvas already exists",
                        ["canvasPath"] = GetGameObjectPath(existingCanvas.gameObject)
                    };
                }
                
                // Create canvas GameObject
                GameObject canvasObj = new GameObject(name);
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                
                // Set render mode
                switch (renderMode.ToLower())
                {
                    case "screenspaceoverlay":
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        break;
                    case "screenspacecamera":
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        break;
                    case "worldspace":
                        canvas.renderMode = RenderMode.WorldSpace;
                        break;
                }
                
                canvas.sortingOrder = sortingOrder;
                canvas.pixelPerfect = pixelPerfect;
                
                // Add CanvasScaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                // Apply preset if specified
                if (!string.IsNullOrEmpty(preset))
                {
                    ApplyCanvasPreset(scaler, preset);
                }
                
                // Add GraphicRaycaster for UI interaction
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Ensure EventSystem exists
                if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
                {
                    CreateEventSystemInternal();
                }
                
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
                
                Debug.Log($"[MCP] Created canvas: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["canvasPath"] = GetGameObjectPath(canvasObj),
                    ["renderMode"] = canvas.renderMode.ToString(),
                    ["components"] = new JArray("Canvas", "CanvasScaler", "GraphicRaycaster")
                };
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
        
        private static void ApplyCanvasPreset(CanvasScaler scaler, string preset)
        {
            switch (preset.ToLower())
            {
                case "mobile_portrait":
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 1f; // Match height
                    break;
                case "mobile_landscape":
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0f; // Match width
                    break;
                case "tablet":
                    scaler.referenceResolution = new Vector2(1536, 2048);
                    scaler.matchWidthOrHeight = 0.5f;
                    break;
                case "desktop":
                case "game_menu":
                default:
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                    break;
            }
        }
        
        private static JObject UI_SetupCanvasScaler(JObject args)
        {
            string canvasName = args["canvas"]?.ToString() ?? "Canvas";
            string scaleMode = args["uiScaleMode"]?.ToString() ?? "ScaleWithScreenSize";
            
            try
            {
                GameObject canvasObj = GameObject.Find(canvasName);
                if (canvasObj == null)
                {
                    throw new System.Exception($"Canvas '{canvasName}' not found");
                }
                
                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = canvasObj.AddComponent<CanvasScaler>();
                }
                
                // Set scale mode
                if (scaleMode == "ScaleWithScreenSize")
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    
                    if (args["referenceResolution"] != null)
                    {
                        var res = args["referenceResolution"] as JArray;
                        scaler.referenceResolution = new Vector2(
                            res[0].ToObject<float>(),
                            res[1].ToObject<float>()
                        );
                    }
                    
                    if (args["matchValue"] != null)
                    {
                        scaler.matchWidthOrHeight = args["matchValue"].ToObject<float>();
                    }
                }
                
                EditorUtility.SetDirty(canvasObj);
                
                Debug.Log($"[MCP] Configured canvas scaler for: {canvasName}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["scaleMode"] = scaler.uiScaleMode.ToString(),
                    ["referenceResolution"] = new JArray(scaler.referenceResolution.x, scaler.referenceResolution.y)
                };
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
        
        private static JObject UI_CreateEventSystem(JObject args)
        {
            try
            {
                // Check if EventSystem already exists
                EventSystem existingSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
                if (existingSystem != null)
                {
                    Debug.Log("[MCP] EventSystem already exists");
                    return new JObject
                    {
                        ["success"] = true,
                        ["message"] = "EventSystem already exists",
                        ["path"] = GetGameObjectPath(existingSystem.gameObject)
                    };
                }
                
                // Create EventSystem with appropriate input module
                GameObject eventSystemObj = CreateEventSystemInternal();
                
                return new JObject
                {
                    ["success"] = true,
                    ["path"] = GetGameObjectPath(eventSystemObj),
                    ["components"] = GetEventSystemComponents(eventSystemObj)
                };
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
        
        private static GameObject CreateEventSystemInternal()
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            EventSystem eventSystem = eventSystemObj.AddComponent<EventSystem>();
            
            // Use the appropriate input module based on which Input System is active
#if ENABLE_INPUT_SYSTEM
            // New Input System
            InputSystemUIInputModule inputModule = eventSystemObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[MCP] Created EventSystem with InputSystemUIInputModule (new Input System)");
#else
            // Legacy Input System
            StandaloneInputModule inputModule = eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[MCP] Created EventSystem with StandaloneInputModule (legacy Input)");
#endif
            
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
            
            return eventSystemObj;
        }
        
        private static JArray GetEventSystemComponents(GameObject eventSystemObj)
        {
            var components = new JArray("EventSystem");
            
#if ENABLE_INPUT_SYSTEM
            if (eventSystemObj.GetComponent<InputSystemUIInputModule>() != null)
                components.Add("InputSystemUIInputModule");
#endif
            if (eventSystemObj.GetComponent<StandaloneInputModule>() != null)
                components.Add("StandaloneInputModule");
            
            return components;
        }
        
        // ==================== UI TOOLS - ELEMENTS ====================
        
        private static JObject UI_CreateButton(JObject args)
        {
            string name = args["name"]?.ToString() ?? "Button";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            string text = args["text"]?.ToString() ?? "Button";
            int textSize = args["textSize"]?.ToObject<int>() ?? 24;
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found. Create a canvas first.");
                }
                
                // Create button GameObject
                GameObject buttonObj = new GameObject(name);
                buttonObj.transform.SetParent(parentObj.transform, false);
                
                // Add RectTransform
                RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(
                    args["size"]?[0]?.ToObject<float>() ?? 200f,
                    args["size"]?[1]?.ToObject<float>() ?? 60f
                );
                
                if (args["position"] != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        args["position"][0].ToObject<float>(),
                        args["position"][1].ToObject<float>()
                    );
                }
                
                // Add Image component for background
                Image image = buttonObj.AddComponent<Image>();
                image.color = new Color(0.2f, 0.3f, 0.8f, 1f); // Default blue
                
                // Add Button component
                Button button = buttonObj.AddComponent<Button>();
                
                // Create text child
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = textSize;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                
                Undo.RegisterCreatedObjectUndo(buttonObj, "Create Button");
                
                Debug.Log($"[MCP] Created button: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["buttonPath"] = GetGameObjectPath(buttonObj),
                    ["textPath"] = GetGameObjectPath(textObj),
                    ["components"] = new JArray("Button", "Image", "RectTransform")
                };
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
        
        private static JObject UI_CreateText(JObject args)
        {
            string name = args["name"]?.ToString() ?? "Text";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            string textContent = args["text"]?.ToString() ?? "Text";
            int fontSize = args["fontSize"]?.ToObject<int>() ?? 24;
            string alignment = args["alignment"]?.ToString() ?? "center";
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found");
                }
                
                GameObject textObj = new GameObject(name);
                textObj.transform.SetParent(parentObj.transform, false);
                
                RectTransform rectTransform = textObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400, 100);
                
                if (args["position"] != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        args["position"][0].ToObject<float>(),
                        args["position"][1].ToObject<float>()
                    );
                }
                
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = textContent;
                tmp.fontSize = fontSize;
                tmp.color = ParseColor(args["color"]?.ToString() ?? "#FFFFFF");
                
                // Set alignment
                switch (alignment.ToLower())
                {
                    case "left":
                        tmp.alignment = TextAlignmentOptions.Left;
                        break;
                    case "right":
                        tmp.alignment = TextAlignmentOptions.Right;
                        break;
                    case "center":
                    default:
                        tmp.alignment = TextAlignmentOptions.Center;
                        break;
                }
                
                // Apply effects if specified
                if (args["effects"]?["outline"]?["enabled"]?.ToObject<bool>() == true)
                {
                    tmp.outlineWidth = args["effects"]["outline"]["thickness"]?.ToObject<float>() ?? 0.2f;
                    tmp.outlineColor = ParseColor(args["effects"]["outline"]["color"]?.ToString() ?? "#000000");
                }
                
                Undo.RegisterCreatedObjectUndo(textObj, "Create Text");
                
                Debug.Log($"[MCP] Created text: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["textPath"] = GetGameObjectPath(textObj),
                    ["components"] = new JArray("TextMeshProUGUI", "RectTransform")
                };
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
        
        private static JObject UI_CreateImage(JObject args)
        {
            string name = args["name"]?.ToString() ?? "Image";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found");
                }
                
                GameObject imageObj = new GameObject(name);
                imageObj.transform.SetParent(parentObj.transform, false);
                
                RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(
                    args["size"]?[0]?.ToObject<float>() ?? 100f,
                    args["size"]?[1]?.ToObject<float>() ?? 100f
                );
                
                if (args["position"] != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        args["position"][0].ToObject<float>(),
                        args["position"][1].ToObject<float>()
                    );
                }
                
                Image image = imageObj.AddComponent<Image>();
                image.color = ParseColor(args["color"]?.ToString() ?? "#FFFFFF");
                
                Undo.RegisterCreatedObjectUndo(imageObj, "Create Image");
                
                Debug.Log($"[MCP] Created image: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["imagePath"] = GetGameObjectPath(imageObj),
                    ["components"] = new JArray("Image", "RectTransform")
                };
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
        
        private static JObject UI_CreatePanel(JObject args)
        {
            string name = args["name"]?.ToString() ?? "Panel";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found");
                }
                
                GameObject panelObj = new GameObject(name);
                panelObj.transform.SetParent(parentObj.transform, false);
                
                RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
                
                // Full canvas size by default
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                
                Image image = panelObj.AddComponent<Image>();
                image.color = ParseColor(args["color"]?.ToString() ?? "#000000AA"); // Semi-transparent black
                
                Undo.RegisterCreatedObjectUndo(panelObj, "Create Panel");
                
                Debug.Log($"[MCP] Created panel: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["panelPath"] = GetGameObjectPath(panelObj),
                    ["components"] = new JArray("Image", "RectTransform")
                };
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
        
        // ==================== UI TOOLS - LAYOUT ====================
        
        private static JObject UI_CreateVerticalLayout(JObject args)
        {
            string name = args["name"]?.ToString() ?? "VerticalLayout";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found");
                }
                
                GameObject layoutObj = new GameObject(name);
                layoutObj.transform.SetParent(parentObj.transform, false);
                
                RectTransform rectTransform = layoutObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 400);
                
                if (args["position"] != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        args["position"][0].ToObject<float>(),
                        args["position"][1].ToObject<float>()
                    );
                }
                
                VerticalLayoutGroup layout = layoutObj.AddComponent<VerticalLayoutGroup>();
                layout.spacing = args["spacing"]?.ToObject<float>() ?? 10f;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                
                Undo.RegisterCreatedObjectUndo(layoutObj, "Create Vertical Layout");
                
                Debug.Log($"[MCP] Created vertical layout: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["layoutPath"] = GetGameObjectPath(layoutObj),
                    ["components"] = new JArray("VerticalLayoutGroup", "RectTransform")
                };
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
        
        private static JObject UI_CreateHorizontalLayout(JObject args)
        {
            string name = args["name"]?.ToString() ?? "HorizontalLayout";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found");
                }
                
                GameObject layoutObj = new GameObject(name);
                layoutObj.transform.SetParent(parentObj.transform, false);
                
                RectTransform rectTransform = layoutObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400, 100);
                
                if (args["position"] != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        args["position"][0].ToObject<float>(),
                        args["position"][1].ToObject<float>()
                    );
                }
                
                HorizontalLayoutGroup layout = layoutObj.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = args["spacing"]?.ToObject<float>() ?? 10f;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = true;
                
                Undo.RegisterCreatedObjectUndo(layoutObj, "Create Horizontal Layout");
                
                Debug.Log($"[MCP] Created horizontal layout: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["layoutPath"] = GetGameObjectPath(layoutObj),
                    ["components"] = new JArray("HorizontalLayoutGroup", "RectTransform")
                };
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
        
        private static JObject UI_CreateGridLayout(JObject args)
        {
            string name = args["name"]?.ToString() ?? "GridLayout";
            string parent = args["parent"]?.ToString() ?? "Canvas";
            
            try
            {
                GameObject parentObj = GameObject.Find(parent);
                if (parentObj == null)
                {
                    throw new System.Exception($"Parent '{parent}' not found");
                }
                
                GameObject layoutObj = new GameObject(name);
                layoutObj.transform.SetParent(parentObj.transform, false);
                
                RectTransform rectTransform = layoutObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400, 400);
                
                if (args["position"] != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        args["position"][0].ToObject<float>(),
                        args["position"][1].ToObject<float>()
                    );
                }
                
                GridLayoutGroup layout = layoutObj.AddComponent<GridLayoutGroup>();
                layout.cellSize = new Vector2(100, 100);
                layout.spacing = new Vector2(10, 10);
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childAlignment = TextAnchor.UpperLeft;
                
                Undo.RegisterCreatedObjectUndo(layoutObj, "Create Grid Layout");
                
                Debug.Log($"[MCP] Created grid layout: {name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["layoutPath"] = GetGameObjectPath(layoutObj),
                    ["components"] = new JArray("GridLayoutGroup", "RectTransform")
                };
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
        
        private static JObject UI_SetSprite(JObject args)
        {
            string objectPath = args["objectPath"]?.ToString();
            string spritePath = args["spritePath"]?.ToString();
            string spriteName = args["spriteName"]?.ToString(); // Optional: specific sprite from sheet
            
            if (string.IsNullOrEmpty(objectPath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "objectPath is required"
                };
            }
            
            if (string.IsNullOrEmpty(spritePath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "spritePath is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(objectPath);
                if (obj == null)
                {
                    throw new System.Exception($"GameObject '{objectPath}' not found");
                }
                
                Image image = obj.GetComponent<Image>();
                if (image == null)
                {
                    throw new System.Exception($"GameObject '{objectPath}' does not have an Image component");
                }
                
                Sprite sprite = null;
                
                // If a specific sprite name is requested, load from sprite sheet directly
                if (!string.IsNullOrEmpty(spriteName))
                {
                    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                    
                    Debug.Log($"[MCP] Loading sprite '{spriteName}' from sheet with {sprites.Length} assets at {spritePath}");
                    
                    foreach (UnityEngine.Object asset in sprites)
                    {
                        if (asset is Sprite)
                        {
                            Sprite s = asset as Sprite;
                            Debug.Log($"[MCP] Found sprite: {s.name}");
                            
                            if (s.name == spriteName)
                            {
                                sprite = s;
                                Debug.Log($"[MCP] ✓ Matched sprite: {s.name}");
                                break;
                            }
                        }
                    }
                    
                    if (sprite == null)
                    {
                        Debug.LogWarning($"[MCP] Sprite '{spriteName}' not found in sheet, falling back to default");
                    }
                }
                
                // If no specific name or not found, try loading directly
                if (sprite == null)
                {
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                }
                
                // Try Resources as fallback
                if (sprite == null)
                {
                    string resourcePath = spritePath.Replace("Assets/Resources/", "").Replace(".png", "").Replace(".jpg", "");
                    sprite = Resources.Load<Sprite>(resourcePath);
                }
                
                // Last resort: load first sprite from sheet
                if (sprite == null)
                {
                    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                    foreach (UnityEngine.Object asset in sprites)
                    {
                        if (asset is Sprite)
                        {
                            sprite = asset as Sprite;
                            break;
                        }
                    }
                }
                
                if (sprite == null)
                {
                    throw new System.Exception($"Sprite not found at path: {spritePath}");
                }
                
                image.sprite = sprite;
                EditorUtility.SetDirty(obj);
                
                Debug.Log($"[MCP] Set sprite on {objectPath} to {sprite.name}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["objectPath"] = objectPath,
                    ["spriteName"] = sprite.name,
                    ["spritePath"] = spritePath
                };
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
        
        // Add Particle Trail Effect
        private static JObject AddParticleTrail(JObject args)
        {
            string name = args["name"]?.ToString();
            string color = args["color"]?.ToString() ?? "yellow";
            float emissionRate = args["emissionRate"]?.ToObject<float>() ?? 50f;
            float startSize = args["startSize"]?.ToObject<float>() ?? 0.05f;
            float startLifetime = args["startLifetime"]?.ToObject<float>() ?? 0.3f;
            
            if (string.IsNullOrEmpty(name))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{name}' not found"
                    };
                }
                
                // Add ParticleSystem component
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    ps = obj.AddComponent<ParticleSystem>();
                }
                
                Undo.RecordObject(ps, "Configure Particle Trail");
                
                // Main module settings
                var main = ps.main;
                main.startLifetime = startLifetime;
                main.startSpeed = 0f; // Particles stay with bullet
                main.startSize = startSize;
                main.startRotation = 0f;
                main.simulationSpace = ParticleSystemSimulationSpace.World; // Particles stay behind as bullet moves
                main.maxParticles = 1000; // Increased for high emission rates
                main.loop = true;
                main.playOnAwake = true;
                main.stopAction = ParticleSystemStopAction.None; // Keep particles alive when stopped
                
                // Set color
                Color particleColor;
                switch (color.ToLower())
                {
                    case "red":
                        particleColor = Color.red;
                        break;
                    case "green":
                        particleColor = Color.green;
                        break;
                    case "blue":
                        particleColor = Color.blue;
                        break;
                    case "white":
                        particleColor = Color.white;
                        break;
                    case "orange":
                        particleColor = new Color(1f, 0.5f, 0f);
                        break;
                    case "cyan":
                        particleColor = Color.cyan;
                        break;
                    case "yellow":
                    default:
                        particleColor = Color.yellow;
                        break;
                }
                main.startColor = particleColor;
                
                // Emission module
                var emission = ps.emission;
                emission.rateOverTime = emissionRate;
                
                // Shape module - emit from point
                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.01f; // Very small, nearly a point
                
                // Size over lifetime - fade out
                var sizeOverLifetime = ps.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                AnimationCurve sizeCurve = new AnimationCurve();
                sizeCurve.AddKey(0f, 1f);
                sizeCurve.AddKey(1f, 0f);
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
                
                // Color over lifetime - fade out
                var colorOverLifetime = ps.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(particleColor, 0f), new GradientColorKey(particleColor, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
                
                // Renderer settings
                var renderer = obj.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
                }
                
                EditorUtility.SetDirty(obj);
                
                Debug.Log($"[MCP] Added particle trail to '{name}' - Color: {color}, Emission: {emissionRate}, Size: {startSize}, Lifetime: {startLifetime}");
                
                return new JObject
                {
                    ["success"] = true,
                    ["name"] = name,
                    ["color"] = color,
                    ["emissionRate"] = emissionRate,
                    ["startSize"] = startSize,
                    ["startLifetime"] = startLifetime
                };
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
        
        // Add Script Component to GameObject
        private static JObject AddScriptComponent(JObject args)
        {
            string gameObjectName = args["gameObjectName"]?.ToString();
            string scriptName = args["scriptName"]?.ToString();
            
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (string.IsNullOrEmpty(scriptName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Script name is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(gameObjectName);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{gameObjectName}' not found"
                    };
                }
                
                // Find the script type by name
                System.Type scriptType = null;
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    scriptType = assembly.GetType(scriptName);
                    if (scriptType != null && typeof(MonoBehaviour).IsAssignableFrom(scriptType))
                    {
                        break;
                    }
                }
                
                if (scriptType == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"Script type '{scriptName}' not found or is not a MonoBehaviour"
                    };
                }
                
                // Add the component
                if (obj.GetComponent(scriptType) == null)
                {
                    obj.AddComponent(scriptType);
                    EditorUtility.SetDirty(obj);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    Debug.Log($"[MCP] Added {scriptName} component to {gameObjectName}");
                }
                else
                {
                    Debug.Log($"[MCP] {scriptName} component already exists on {gameObjectName}");
                }
                
                return new JObject
                {
                    ["success"] = true,
                    ["gameObject"] = gameObjectName,
                    ["scriptName"] = scriptName
                };
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
        
        private static JObject SaveAsPrefab(JObject args)
        {
            string gameObjectName = args["gameObjectName"]?.ToString();
            string prefabPath = args["prefabPath"]?.ToString();
            
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "GameObject name is required"
                };
            }
            
            if (string.IsNullOrEmpty(prefabPath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Prefab path is required"
                };
            }
            
            try
            {
                GameObject obj = GameObject.Find(gameObjectName);
                if (obj == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = $"GameObject '{gameObjectName}' not found"
                    };
                }
                
                // Ensure path starts with Assets/
                if (!prefabPath.StartsWith("Assets/"))
                {
                    prefabPath = "Assets/" + prefabPath;
                }
                
                // Ensure path ends with .prefab
                if (!prefabPath.EndsWith(".prefab"))
                {
                    prefabPath += ".prefab";
                }
                
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(prefabPath);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    string[] folders = directory.Split('/');
                    string currentPath = folders[0];
                    for (int i = 1; i < folders.Length; i++)
                    {
                        string newPath = currentPath + "/" + folders[i];
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, folders[i]);
                        }
                        currentPath = newPath;
                    }
                }
                
                // Save as prefab
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
                
                if (prefab != null)
                {
                    AssetDatabase.Refresh();
                    Debug.Log($"[MCP] Created prefab: {prefabPath}");
                    
                    return new JObject
                    {
                        ["success"] = true,
                        ["prefabPath"] = prefabPath,
                        ["gameObjectName"] = gameObjectName
                    };
                }
                else
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = "Failed to create prefab"
                    };
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
        
        private static JObject UpdatePrefab(JObject args)
        {
            string prefabPath = args["prefabPath"]?.ToString();
            string action = args["action"]?.ToString(); // "add_component", "remove_component", "set_property"
            
            if (string.IsNullOrEmpty(prefabPath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "Prefab path is required"
                };
            }
            
            // Ensure path starts with Assets/
            if (!prefabPath.StartsWith("Assets/"))
            {
                prefabPath = "Assets/" + prefabPath;
            }
            
            // Ensure path ends with .prefab
            if (!prefabPath.EndsWith(".prefab"))
            {
                prefabPath += ".prefab";
            }
            
            // Check if prefab exists
            if (!System.IO.File.Exists(prefabPath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = $"Prefab not found: {prefabPath}"
                };
            }
            
            try
            {
                // Load prefab contents for editing
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                
                if (prefabRoot == null)
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = "Failed to load prefab"
                    };
                }
                
                // Perform the requested action
                bool modified = false;
                string actionResult = "";
                
                switch (action)
                {
                    case "add_component":
                        string componentType = args["componentType"]?.ToString();
                        if (!string.IsNullOrEmpty(componentType))
                        {
                            System.Type type = System.Type.GetType(componentType);
                            if (type != null)
                            {
                                prefabRoot.AddComponent(type);
                                modified = true;
                                actionResult = $"Added component: {componentType}";
                            }
                            else
                            {
                                actionResult = $"Component type not found: {componentType}";
                            }
                        }
                        break;
                    
                    case "remove_component":
                        string removeComponentType = args["componentType"]?.ToString();
                        if (!string.IsNullOrEmpty(removeComponentType))
                        {
                            Component comp = prefabRoot.GetComponent(removeComponentType);
                            if (comp != null)
                            {
                                UnityEngine.Object.DestroyImmediate(comp);
                                modified = true;
                                actionResult = $"Removed component: {removeComponentType}";
                            }
                            else
                            {
                                actionResult = $"Component not found: {removeComponentType}";
                            }
                        }
                        break;
                    
                    case "set_property":
                        string propComponentType = args["componentType"]?.ToString();
                        string propertyName = args["propertyName"]?.ToString();
                        JToken valueToken = args["value"];
                        
                        if (!string.IsNullOrEmpty(propComponentType) && !string.IsNullOrEmpty(propertyName))
                        {
                            Component component = prefabRoot.GetComponent(propComponentType);
                            if (component != null)
                            {
                                SerializedObject so = new SerializedObject(component);
                                SerializedProperty prop = so.FindProperty(propertyName);
                                
                                if (prop != null)
                                {
                                    // Set value based on type (reuse logic from SetComponentProperty)
                                    if (valueToken.Type == JTokenType.Object)
                                    {
                                        JObject valueObj = valueToken as JObject;
                                        string refType = valueObj["type"]?.ToString();
                                        string refPath = valueObj["path"]?.ToString();
                                        
                                        if (refType == "reference" && !string.IsNullOrEmpty(refPath))
                                        {
                                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(refPath);
                                            if (asset != null)
                                            {
                                                prop.objectReferenceValue = asset;
                                            }
                                        }
                                    }
                                    else if (valueToken.Type == JTokenType.String)
                                    {
                                        prop.stringValue = valueToken.ToString();
                                    }
                                    else if (valueToken.Type == JTokenType.Integer)
                                    {
                                        prop.intValue = valueToken.ToObject<int>();
                                    }
                                    else if (valueToken.Type == JTokenType.Float)
                                    {
                                        prop.floatValue = valueToken.ToObject<float>();
                                    }
                                    else if (valueToken.Type == JTokenType.Boolean)
                                    {
                                        prop.boolValue = valueToken.ToObject<bool>();
                                    }
                                    
                                    so.ApplyModifiedProperties();
                                    modified = true;
                                    actionResult = $"Set property '{propertyName}' on {propComponentType}";
                                }
                                else
                                {
                                    actionResult = $"Property '{propertyName}' not found on {propComponentType}";
                                }
                            }
                            else
                            {
                                actionResult = $"Component not found: {propComponentType}";
                            }
                        }
                        break;
                    
                    default:
                        actionResult = "No action specified or unknown action";
                        break;
                }
                
                // Save if modified
                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"[MCP] Updated prefab: {prefabPath} - {actionResult}");
                }
                
                // Important: Unload the prefab contents
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                
                return new JObject
                {
                    ["success"] = true,
                    ["prefabPath"] = prefabPath,
                    ["modified"] = modified,
                    ["action"] = actionResult
                };
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
        
        // ==================== HELPER METHODS ====================
        
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        
        private static Color ParseColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return Color.white;
            
            if (!hexColor.StartsWith("#"))
                hexColor = "#" + hexColor;
            
            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
                return color;
            
            return Color.white;
        }
        private static JObject CopyAsset(JObject args)
        {
            string sourcePath = args["sourcePath"]?.ToString();
            string destPath = args["destPath"]?.ToString();
            
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destPath))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = "sourcePath and destPath are required"
                };
            }
            
            try
            {
                // Ensure destination directory exists
                string destDir = System.IO.Path.GetDirectoryName(destPath);
                if (!System.IO.Directory.Exists(destDir))
                {
                    System.IO.Directory.CreateDirectory(destDir);
                }
                
                // Copy the asset using Unity's AssetDatabase
                bool success = AssetDatabase.CopyAsset(sourcePath, destPath);
                
                if (success)
                {
                    AssetDatabase.Refresh();
                    Debug.Log($"[MCP] Copied asset from {sourcePath} to {destPath}");
                    
                    return new JObject
                    {
                        ["success"] = true,
                        ["sourcePath"] = sourcePath,
                        ["destPath"] = destPath
                    };
                }
                else
                {
                    return new JObject
                    {
                        ["success"] = false,
                        ["error"] = "AssetDatabase.CopyAsset returned false"
                    };
                }
            }
            catch (Exception e)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["error"] = e.Message
                };
            }
        }
    }
}

