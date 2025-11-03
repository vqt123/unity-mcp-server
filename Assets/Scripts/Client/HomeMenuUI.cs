using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace ArenaGame.Client
{
    /// <summary>
    /// Main menu UI for home scene
    /// </summary>
    public class HomeMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button heroesButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI energyText;
        
        void Start()
        {
            // If no buttons assigned, create them all
            if (heroesButton == null && playButton == null && quitButton == null)
            {
                CreateMenuUI();
            }
            else
            {
                // Wire up existing buttons
                if (heroesButton != null)
                    heroesButton.onClick.AddListener(OnHeroesClicked);
                if (playButton != null)
                    playButton.onClick.AddListener(OnPlayClicked);
                if (quitButton != null)
                    quitButton.onClick.AddListener(OnQuitClicked);
                
                // Ensure replay button exists (might be missing from scene)
                if (replayButton == null)
                {
                    Debug.Log("[HomeMenu] Replay button missing - creating it");
                    CreateReplayButton();
                }
                
                if (replayButton != null)
                {
                    replayButton.onClick.AddListener(OnReplayClicked);
                    // Update replay button state
                    replayButton.interactable = ArenaReplayManager.HasReplay();
                    Debug.Log($"[HomeMenu] Replay button state: interactable={replayButton.interactable}, hasReplay={ArenaReplayManager.HasReplay()}");
                }
            }
            
            // Setup gold display
            SetupGoldDisplay();
            
            // Setup energy display
            SetupEnergyDisplay();
        }
        
        /// <summary>
        /// Create replay button and add it to existing menu structure
        /// </summary>
        private void CreateReplayButton()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[HomeMenu] Cannot create replay button - no Canvas found");
                return;
            }
            
            // Find the parent of existing buttons (should be MainCanvas)
            Transform buttonParent = null;
            if (playButton != null)
            {
                buttonParent = playButton.transform.parent;
            }
            else if (heroesButton != null)
            {
                buttonParent = heroesButton.transform.parent;
            }
            
            if (buttonParent == null)
            {
                Debug.LogError("[HomeMenu] Cannot find parent for replay button");
                return;
            }
            
            // Create replay button between Play and Heroes buttons
            replayButton = CreateButton(buttonParent, "REPLAY LAST ARENA", new Color(0.8f, 0.6f, 0.2f));
            
            // Position it after Play button
            if (playButton != null)
            {
                int playButtonIndex = playButton.transform.GetSiblingIndex();
                replayButton.transform.SetSiblingIndex(playButtonIndex + 1);
            }
            
            Debug.Log("[HomeMenu] Created replay button");
        }
        
        void OnEnable()
        {
            // Refresh replay button state when scene is enabled (e.g., returning from arena)
            if (replayButton != null)
            {
                replayButton.interactable = ArenaReplayManager.HasReplay();
                Debug.Log($"[HomeMenu] OnEnable - Replay button state updated: interactable={replayButton.interactable}, hasReplay={ArenaReplayManager.HasReplay()}");
            }
            
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged += UpdateGoldDisplay;
                UpdateGoldDisplay(GoldManager.Instance.CurrentGold);
            }
            
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.OnEnergyChanged += UpdateEnergyDisplay;
                UpdateEnergyDisplay(EnergyManager.Instance.CurrentEnergy);
            }
        }
        
        void OnDisable()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
            }
            
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.OnEnergyChanged -= UpdateEnergyDisplay;
            }
        }
        
        private void SetupGoldDisplay()
        {
            // Create gold display if not assigned
            if (goldText == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObj = new GameObject("Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                
                CreateGoldDisplay(canvas.transform);
            }
            else
            {
                UpdateGoldDisplay(GoldManager.Instance != null ? GoldManager.Instance.CurrentGold : 0);
            }
        }
        
        private void CreateGoldDisplay(Transform parent)
        {
            GameObject goldObj = new GameObject("GoldDisplay");
            goldObj.transform.SetParent(parent, false);
            
            RectTransform rect = goldObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(200f, 40f);
            
            TextMeshProUGUI tmp = goldObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Gold: 0";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.color = new Color(1f, 0.84f, 0f); // Gold color
            
            goldText = tmp;
        }
        
        private void UpdateGoldDisplay(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }
        
        private void SetupEnergyDisplay()
        {
            // Create energy display if not assigned
            if (energyText == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObj = new GameObject("Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                
                CreateEnergyDisplay(canvas.transform);
            }
            else
            {
                UpdateEnergyDisplay(EnergyManager.Instance != null ? EnergyManager.Instance.CurrentEnergy : 0);
            }
        }
        
        private void CreateEnergyDisplay(Transform parent)
        {
            GameObject energyObj = new GameObject("EnergyDisplay");
            energyObj.transform.SetParent(parent, false);
            
            RectTransform rect = energyObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -70f); // Below gold display
            rect.sizeDelta = new Vector2(250f, 40f);
            
            TextMeshProUGUI tmp = energyObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Energy: 30/30";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = new Color(0.3f, 0.8f, 1f); // Light blue/cyan color
            
            energyText = tmp;
        }
        
        private void UpdateEnergyDisplay(int energy)
        {
            if (energyText != null)
            {
                int maxEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.MaxEnergy : 30;
                energyText.text = $"Energy: {energy}/{maxEnergy}";
            }
        }
        
        private void CreateMenuUI()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                // Add EventSystem if needed
                if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    
#if ENABLE_INPUT_SYSTEM
                    // New Input System
                    eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                    // Legacy Input System
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
                }
            }
            
            // Create vertical layout for menu
            GameObject menuPanel = new GameObject("MenuPanel");
            menuPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = menuPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 500);
            
            VerticalLayoutGroup layout = menuPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            // Create title
            CreateText(menuPanel.transform, "ARENA SURVIVOR", 48);
            
            // Create spacing
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(menuPanel.transform, false);
            RectTransform spacerRect = spacer.AddComponent<RectTransform>();
            spacerRect.sizeDelta = new Vector2(400, 40);
            
            // Create buttons
            playButton = CreateButton(menuPanel.transform, "PLAY", new Color(0.2f, 0.8f, 0.2f));
            replayButton = CreateButton(menuPanel.transform, "REPLAY LAST ARENA", new Color(0.8f, 0.6f, 0.2f));
            heroesButton = CreateButton(menuPanel.transform, "HEROES", new Color(0.2f, 0.4f, 0.8f));
            quitButton = CreateButton(menuPanel.transform, "QUIT", new Color(0.8f, 0.2f, 0.2f));
            
            // Wire up buttons
            playButton.onClick.AddListener(OnPlayClicked);
            replayButton.onClick.AddListener(OnReplayClicked);
            heroesButton.onClick.AddListener(OnHeroesClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            
            // Disable replay button if no replay available
            if (!ArenaReplayManager.HasReplay())
            {
                replayButton.interactable = false;
            }
            
            Debug.Log("[HomeMenu] Created menu UI");
        }
        
        private GameObject CreateText(Transform parent, string text, float fontSize)
        {
            GameObject textObj = new GameObject("Text_" + text);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 80);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return textObj;
        }
        
        private Button CreateButton(Transform parent, string label, Color color)
        {
            GameObject buttonObj = new GameObject("Button_" + label);
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = color;
            
            Button button = buttonObj.AddComponent<Button>();
            
            // Add text to button
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return button;
        }
        
        private void OnPlayClicked()
        {
            Debug.Log("[HomeMenu] Play clicked - checking energy");
            
            // Check energy cost (5 energy to start arena)
            const int ARENA_ENERGY_COST = 5;
            
            if (EnergyManager.Instance == null)
            {
                Debug.LogError("[HomeMenu] EnergyManager not found!");
                return;
            }
            
            if (EnergyManager.Instance.CurrentEnergy < ARENA_ENERGY_COST)
            {
                Debug.LogWarning($"[HomeMenu] Not enough energy! Need {ARENA_ENERGY_COST}, have {EnergyManager.Instance.CurrentEnergy}");
                // TODO: Show error message to player
                return;
            }
            
            // Spend energy and start arena
            if (EnergyManager.Instance.SpendEnergy(ARENA_ENERGY_COST))
            {
                Debug.Log("[HomeMenu] Starting arena - loading game scene");
                SceneManager.LoadScene("ArenaGame");
            }
        }
        
        private void OnReplayClicked()
        {
            Debug.Log("[HomeMenu] Replay clicked - loading last arena replay");
            
            // Load replay data
            var commands = ArenaReplayManager.LoadReplay();
            if (commands == null || commands.Count == 0)
            {
                Debug.LogWarning("[HomeMenu] No replay data available");
                return;
            }
            
            // Load arena scene and set replay mode
            SceneManager.LoadScene("ArenaGame");
            
            // Note: Replay will be started by GameBootstrapper after scene loads
            // We need to pass the replay commands somehow - using a static variable for now
            ReplayStarter.SetReplayCommands(commands);
        }
        
        private void OnHeroesClicked()
        {
            Debug.Log("[HomeMenu] Heroes clicked - showing hero inventory");
            
            // Hide main menu
            gameObject.SetActive(false);
            
            // Show hero inventory UI
            HeroInventoryUI inventoryUI = FindFirstObjectByType<HeroInventoryUI>();
            if (inventoryUI == null)
            {
                GameObject invObj = new GameObject("HeroInventoryUI");
                inventoryUI = invObj.AddComponent<HeroInventoryUI>();
            }
            inventoryUI.gameObject.SetActive(true);
            inventoryUI.Show(this);
        }
        
        private void OnQuitClicked()
        {
            Debug.Log("[HomeMenu] Quit clicked");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}

