using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    [InitializeOnLoad]
    public static class MCPServer
    {
        private static HttpListener listener;
        private static Thread serverThread;
        private static bool isRunning = false;
        private static int port = 8765;
        private static bool shouldRestart = false;
        
        // Command queue for main thread execution
        private class PendingRequest
        {
            public string tool;
            public JObject args;
            public ManualResetEvent resetEvent;
            public JObject response;
        }
        
        private static Queue<PendingRequest> pendingRequests = new Queue<PendingRequest>();
        private static object queueLock = new object();
        
        // Auto-start on Unity load
        static MCPServer()
        {
            EditorApplication.update += ProcessRequests;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            
            // Flag server to start on next update (works without Unity focus)
            shouldRestart = true;
        }
        
        private static void ProcessRequests()
        {
            // Check if server needs to be started/restarted (works without Unity focus)
            if (shouldRestart && !isRunning)
            {
                shouldRestart = false;
                StartServer();
            }
            
            // Process all pending requests on main thread
            lock (queueLock)
            {
                while (pendingRequests.Count > 0)
                {
                    var request = pendingRequests.Dequeue();
                    
                    try
                    {
                        Debug.Log($"[MCP] Executing: {request.tool}");
                        request.response = MCPTools.Execute(request.tool, request.args);
                        Debug.Log($"[MCP] Executed successfully");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[MCP] Execution error: {e.Message}");
                        request.response = CreateError(e.Message);
                    }
                    finally
                    {
                        request.resetEvent.Set();
                    }
                }
            }
        }
        
        private static void OnBeforeAssemblyReload()
        {
            Debug.Log("[MCP] Stopping server for assembly reload...");
            StopServer();
        }
        
        private static void OnAfterAssemblyReload()
        {
            Debug.Log("[MCP] Assembly reload complete - flagging server to restart...");
            // Flag server to restart on next update (works without Unity focus)
            shouldRestart = true;
        }
        
        public static void StartServer()
        {
            if (isRunning)
            {
                Debug.Log("[MCP] Server already running");
                return;
            }
            
            try
            {
                serverThread = new Thread(ServerLoop);
                serverThread.IsBackground = true;
                serverThread.Start();
                isRunning = true;
                
                Debug.Log($"[MCP] ✅ Server started on http://localhost:{port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Failed to start: {e.Message}");
            }
        }
        
        public static void StopServer()
        {
            if (!isRunning) return;
            
            isRunning = false;
            listener?.Stop();
            listener?.Close();
            
            // Clear pending requests
            lock (queueLock)
            {
                pendingRequests.Clear();
            }
            
            Debug.Log("[MCP] Server stopped");
        }
        
        private static void ServerLoop()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            
            Debug.Log("[MCP] Listening for requests...");
            
            while (isRunning)
            {
                try
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MCP] Server error: {e.Message}");
                }
            }
        }
        
        private static void HandleRequest(HttpListenerContext context)
        {
            try
            {
                // Read request
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }
                
                // Parse JSON
                var request = JObject.Parse(requestBody);
                string tool = request["tool"]?.ToString();
                var args = request["args"] as JObject ?? new JObject();
                
                Debug.Log($"[MCP] Received: {tool}");
                
                // Queue request for main thread
                var pendingRequest = new PendingRequest
                {
                    tool = tool,
                    args = args,
                    resetEvent = new ManualResetEvent(false),
                    response = null
                };
                
                lock (queueLock)
                {
                    pendingRequests.Enqueue(pendingRequest);
                }
                
                // Wait for main thread to process (30 second timeout)
                if (pendingRequest.resetEvent.WaitOne(30000))
                {
                    SendResponse(context, pendingRequest.response);
                }
                else
                {
                    Debug.LogError("[MCP] Request timeout");
                    SendResponse(context, CreateError("Timeout: Unity main thread didn't process request"));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Request error: {e.Message}");
                SendResponse(context, CreateError(e.Message));
            }
        }
        
        private static void SendResponse(HttpListenerContext context, JObject response)
        {
            try
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                
                var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
                context.Response.ContentLength64 = responseBytes.Length;
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                context.Response.OutputStream.Close();
                
                Debug.Log($"[MCP] Response sent: {response.ToString().Substring(0, Math.Min(100, response.ToString().Length))}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MCP] Send response error: {e.Message}");
            }
        }
        
        private static JObject CreateError(string message)
        {
            return new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
        }
    }
}
