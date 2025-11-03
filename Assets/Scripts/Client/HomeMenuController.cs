using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArenaGame.Client
{
    /// <summary>
    /// Simple home menu controller with public methods for button clicks
    /// </summary>
    public class HomeMenuController : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("[HomeMenuController] Starting");
            
            // Ensure PlayerDataManager exists
            if (PlayerDataManager.Instance == null)
            {
                GameObject dataObj = new GameObject("PlayerDataManager");
                dataObj.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(dataObj);
                Debug.Log("[HomeMenuController] Created PlayerDataManager");
            }
            
            // Ensure EnergyManager exists
            if (EnergyManager.Instance == null)
            {
                GameObject energyObj = new GameObject("EnergyManager");
                energyObj.AddComponent<EnergyManager>();
                Debug.Log("[HomeMenuController] Created EnergyManager");
            }
        }
        
        void Start()
        {
            // Find and wire up buttons
            UnityEngine.UI.Button playButton = GameObject.Find("PlayButton")?.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Button heroesButton = GameObject.Find("HeroesButton")?.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Button quitButton = GameObject.Find("QuitButton")?.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Button replayButton = GameObject.Find("ReplayButton")?.GetComponent<UnityEngine.UI.Button>();
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
                Debug.Log("[HomeMenuController] Wired PlayButton");
            }
            
            if (heroesButton != null)
            {
                heroesButton.onClick.AddListener(OnHeroesButtonClicked);
                Debug.Log("[HomeMenuController] Wired HeroesButton");
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
                Debug.Log("[HomeMenuController] Wired QuitButton");
            }
            
            // Create replay button if it doesn't exist
            if (replayButton == null)
            {
                replayButton = CreateReplayButton();
                Debug.Log("[HomeMenuController] Created ReplayButton");
            }
            
            if (replayButton != null)
            {
                replayButton.onClick.AddListener(OnReplayButtonClicked);
                bool hasReplay = ArenaReplayManager.HasReplay();
                replayButton.interactable = hasReplay;
                Debug.Log($"[HomeMenuController] Wired ReplayButton, hasReplay={hasReplay}, interactable={replayButton.interactable}");
            }
        }
        
        void OnEnable()
        {
            // Refresh replay button state when returning to scene
            UnityEngine.UI.Button replayButton = GameObject.Find("ReplayButton")?.GetComponent<UnityEngine.UI.Button>();
            if (replayButton != null)
            {
                bool hasReplay = ArenaReplayManager.HasReplay();
                replayButton.interactable = hasReplay;
                Debug.Log($"[HomeMenuController] OnEnable - ReplayButton state: hasReplay={hasReplay}, interactable={replayButton.interactable}");
            }
        }
        
        private UnityEngine.UI.Button CreateReplayButton()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[HomeMenuController] Cannot create replay button - no Canvas found");
                return null;
            }
            
            // Find the lowest button (QuitButton) to position replay button below it
            GameObject quitButtonObj = GameObject.Find("QuitButton");
            Transform buttonParent = quitButtonObj != null ? quitButtonObj.transform.parent : canvas.transform;
            
            // Create replay button
            GameObject replayObj = new GameObject("ReplayButton");
            replayObj.transform.SetParent(buttonParent, false);
            
            RectTransform rect = replayObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            
            // Position below QuitButton if it exists
            if (quitButtonObj != null)
            {
                RectTransform quitRect = quitButtonObj.GetComponent<RectTransform>();
                if (quitRect != null)
                {
                    // Position below QuitButton with spacing
                    rect.anchoredPosition = quitRect.anchoredPosition - new Vector2(0, quitRect.sizeDelta.y + 20);
                }
            }
            else
            {
                // Fallback: position at bottom center if no QuitButton found
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 50);
            }
            
            UnityEngine.UI.Image image = replayObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.8f, 0.6f, 0.2f);
            
            UnityEngine.UI.Button button = replayObj.AddComponent<UnityEngine.UI.Button>();
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(replayObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "REPLAY LAST ARENA";
            tmp.fontSize = 32;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return button;
        }
        
        public void OnReplayButtonClicked()
        {
            Debug.Log("[HomeMenuController] Replay button clicked");
            
            var (commands, stateHashes, stopTick) = ArenaReplayManager.LoadReplayWithHashes();
            if (commands == null || commands.Count == 0)
            {
                Debug.LogWarning("[HomeMenuController] No replay data available");
                return;
            }
            
            Debug.Log($"[HomeMenuController] Loading replay with {commands.Count} commands, {stateHashes?.Count ?? 0} state hashes, stopTick={stopTick}");
            ReplayStarter.SetReplayCommands(commands, stateHashes, stopTick);
            UnityEngine.SceneManagement.SceneManager.LoadScene("ArenaGame");
        }
        
        public void OnPlayButtonClicked()
        {
            Debug.Log("[HomeMenuController] Play button clicked - checking energy");
            
            // Check energy cost (5 energy to start arena)
            const int ARENA_ENERGY_COST = 5;
            
            // Ensure EnergyManager exists (fallback if not created by bootstrapper)
            if (EnergyManager.Instance == null)
            {
                GameObject energyObj = new GameObject("EnergyManager");
                energyObj.AddComponent<EnergyManager>();
                Debug.Log("[HomeMenuController] Created EnergyManager (fallback)");
            }
            
            if (EnergyManager.Instance.CurrentEnergy < ARENA_ENERGY_COST)
            {
                Debug.LogWarning($"[HomeMenuController] Not enough energy! Need {ARENA_ENERGY_COST}, have {EnergyManager.Instance.CurrentEnergy}");
                // TODO: Show error message to player
                return;
            }
            
            // Spend energy and start arena
            if (EnergyManager.Instance.SpendEnergy(ARENA_ENERGY_COST))
            {
                Debug.Log("[HomeMenuController] Starting arena - loading game scene");
                SceneManager.LoadScene("ArenaGame");
            }
        }
        
        public void OnHeroesButtonClicked()
        {
            Debug.Log("[HomeMenuController] Heroes button clicked!");
            
            // Find or create hero inventory UI
            HeroInventoryUI inventoryUI = FindFirstObjectByType<HeroInventoryUI>();
            if (inventoryUI == null)
            {
                Debug.Log("[HomeMenuController] Creating HeroInventoryUI");
                GameObject invObj = new GameObject("HeroInventoryUI");
                inventoryUI = invObj.AddComponent<HeroInventoryUI>();
            }
            
            inventoryUI.Show(null);
        }
        
        public void OnQuitButtonClicked()
        {
            Debug.Log("[HomeMenuController] Quit button clicked!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

