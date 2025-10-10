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
                    
                    // GameObject Management
                    case "unity_delete_gameobject":
                        return DeleteGameObject(args);
                    
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
    }
}

