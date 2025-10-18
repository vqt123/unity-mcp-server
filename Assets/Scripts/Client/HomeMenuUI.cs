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
        [SerializeField] private Button quitButton;
        
        void Start()
        {
            // If no buttons assigned, create them
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
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
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
            heroesButton = CreateButton(menuPanel.transform, "HEROES", new Color(0.2f, 0.4f, 0.8f));
            quitButton = CreateButton(menuPanel.transform, "QUIT", new Color(0.8f, 0.2f, 0.2f));
            
            // Wire up buttons
            playButton.onClick.AddListener(OnPlayClicked);
            heroesButton.onClick.AddListener(OnHeroesClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            
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
            Debug.Log("[HomeMenu] Play clicked - loading game scene");
            SceneManager.LoadScene("ArenaGame");
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

