using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ArenaGame.Client
{
    /// <summary>
    /// UI for arena scene - exit button, gold display, etc.
    /// </summary>
    public class ArenaUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private GameObject upgradePanel;
        
        void Start()
        {
            SetupUI();
            
            // Wait a frame to ensure GoldManager is created
            StartCoroutine(SubscribeToGoldDelayed());
        }
        
        private System.Collections.IEnumerator SubscribeToGoldDelayed()
        {
            yield return null; // Wait one frame
            
            // Subscribe to gold changes
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged += UpdateGoldDisplay;
                UpdateGoldDisplay(GoldManager.Instance.CurrentGold);
                Debug.Log($"[ArenaUI] Subscribed to GoldManager, current gold: {GoldManager.Instance.CurrentGold}");
            }
            else
            {
                Debug.LogError("[ArenaUI] GoldManager.Instance is null! GameBootstrapper may not have run yet.");
            }
        }
        
        void OnDestroy()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
            }
        }
        
        private void SetupUI()
        {
            // Create canvas if it doesn't exist
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                // Create EventSystem if needed
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
            
            // Create exit button if not assigned
            if (exitButton == null)
            {
                CreateExitButton(canvas.transform);
            }
            else
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }
            
            // Create gold display if not assigned
            if (goldText == null)
            {
                CreateGoldDisplay(canvas.transform);
            }
        }
        
        private void CreateExitButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("ExitButton");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(150f, 50f);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red
            
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnExitClicked);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Exit";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            exitButton = button;
        }
        
        private void CreateGoldDisplay(Transform parent)
        {
            GameObject goldObj = new GameObject("GoldDisplay");
            goldObj.transform.SetParent(parent, false);
            
            RectTransform rect = goldObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(200f, 40f);
            
            TextMeshProUGUI tmp = goldObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Gold: 0";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Left;
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
        
        private void OnExitClicked()
        {
            Debug.Log("[ArenaUI] Exit button clicked - returning to main menu");
            SceneManager.LoadScene("HomeScene");
        }
    }
}

