using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Sets up the home scene with menu and player data
    /// </summary>
    public class HomeSceneBootstrapper : MonoBehaviour
    {
        void Awake()
        {
            SetupHomeScene();
        }
        
        private void SetupHomeScene()
        {
            Debug.Log("[HomeBootstrap] Setting up home scene");
            
            // 1. Create PlayerDataManager (persistent - must be first)
            if (PlayerDataManager.Instance == null)
            {
                GameObject dataObj = new GameObject("PlayerDataManager");
                dataObj.AddComponent<PlayerDataManager>();
                Debug.Log("[HomeBootstrap] ✓ Created PlayerDataManager");
            }
            
            // 2. Create GoldManager (persistent - needed for gold display)
            if (GoldManager.Instance == null)
            {
                GameObject goldObj = new GameObject("GoldManager");
                goldObj.AddComponent<GoldManager>();
                Debug.Log("[HomeBootstrap] ✓ Created GoldManager");
            }
            
            // 3. Create EnergyManager (persistent - manages energy regeneration)
            if (EnergyManager.Instance == null)
            {
                GameObject energyObj = new GameObject("EnergyManager");
                energyObj.AddComponent<EnergyManager>();
                Debug.Log("[HomeBootstrap] ✓ Created EnergyManager");
            }
            
            // 4. Create Canvas if needed
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("[HomeBootstrap] ✓ Created Canvas");
            }
            
            // 4. Create EventSystem if needed
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
#if ENABLE_INPUT_SYSTEM
                // New Input System
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("[HomeBootstrap] ✓ Created EventSystem with InputSystemUIInputModule");
#else
                // Legacy Input System
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[HomeBootstrap] ✓ Created EventSystem with StandaloneInputModule");
#endif
            }
            
            // 4. Create HomeMenuUI
            GameObject menuObj = new GameObject("HomeMenuUI");
            menuObj.AddComponent<HomeMenuUI>();
            Debug.Log("[HomeBootstrap] ✓ Created HomeMenuUI");
            
            // 5. Set camera background
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.15f);
                Debug.Log("[HomeBootstrap] ✓ Set camera background");
            }
            
            Debug.Log("[HomeBootstrap] Home scene setup complete!");
        }
    }
}

