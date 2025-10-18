using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ArenaGame.Client
{
    /// <summary>
    /// Simple home menu manager that handles button clicks
    /// </summary>
    public class HomeMenuManager : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("[HomeMenu] Starting home menu setup");
            
            // Ensure PlayerDataManager exists
            if (PlayerDataManager.Instance == null)
            {
                GameObject dataObj = new GameObject("PlayerDataManager");
                dataObj.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(dataObj);
                Debug.Log("[HomeMenu] Created PlayerDataManager");
            }
            
            // Find and wire up buttons
            Button playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
            Button heroesButton = GameObject.Find("HeroesButton")?.GetComponent<Button>();
            Button quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
                Debug.Log("[HomeMenu] Play button wired");
            }
            
            if (heroesButton != null)
            {
                heroesButton.onClick.AddListener(OnHeroesClicked);
                Debug.Log("[HomeMenu] Heroes button wired");
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
                Debug.Log("[HomeMenu] Quit button wired");
            }
        }
        
        private void OnPlayClicked()
        {
            Debug.Log("[HomeMenu] Play button clicked - loading ArenaGame");
            SceneManager.LoadScene("ArenaGame");
        }
        
        private void OnHeroesClicked()
        {
            Debug.Log("[HomeMenu] Heroes button clicked - showing inventory");
            
            // Find or create hero inventory UI
            HeroInventoryUI inventoryUI = FindFirstObjectByType<HeroInventoryUI>();
            if (inventoryUI == null)
            {
                GameObject invObj = new GameObject("HeroInventoryUI");
                inventoryUI = invObj.AddComponent<HeroInventoryUI>();
            }
            
            inventoryUI.Show(null);
        }
        
        private void OnQuitClicked()
        {
            Debug.Log("[HomeMenu] Quit button clicked");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

