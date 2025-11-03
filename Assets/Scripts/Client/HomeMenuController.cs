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
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
                Debug.Log("[HomeMenuController] Wired PlayButton");
            }
            else
            {
                Debug.LogError("[HomeMenuController] PlayButton not found!");
            }
            
            if (heroesButton != null)
            {
                heroesButton.onClick.AddListener(OnHeroesButtonClicked);
                Debug.Log("[HomeMenuController] Wired HeroesButton");
            }
            else
            {
                Debug.LogError("[HomeMenuController] HeroesButton not found!");
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
                Debug.Log("[HomeMenuController] Wired QuitButton");
            }
            else
            {
                Debug.LogError("[HomeMenuController] QuitButton not found!");
            }
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

