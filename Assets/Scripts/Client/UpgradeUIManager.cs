using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages upgrade UI and sends upgrade commands
    /// </summary>
    public class UpgradeUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private Button damageUpgradeButton;
        [SerializeField] private Button attackSpeedUpgradeButton;
        [SerializeField] private Button moveSpeedUpgradeButton;
        [SerializeField] private Button healthUpgradeButton;
        
        private EntityId playerHeroId;
        private bool upgradeAvailable = false;
        
        void Start()
        {
            // If no panel assigned, create one programmatically
            if (upgradePanel == null)
            {
                CreateUpgradePanel();
            }
            else
            {
                upgradePanel.SetActive(false);
            }
            
            // Wire up buttons
            if (damageUpgradeButton != null)
                damageUpgradeButton.onClick.AddListener(() => ChooseUpgrade("Damage"));
            if (attackSpeedUpgradeButton != null)
                attackSpeedUpgradeButton.onClick.AddListener(() => ChooseUpgrade("AttackSpeed"));
            if (moveSpeedUpgradeButton != null)
                moveSpeedUpgradeButton.onClick.AddListener(() => ChooseUpgrade("MoveSpeed"));
            if (healthUpgradeButton != null)
                healthUpgradeButton.onClick.AddListener(() => ChooseUpgrade("Health"));
            
            // Subscribe to level up events
            EventBus.Subscribe<HeroLevelUpEvent>(OnHeroLevelUp);
            
            Debug.Log("[UpgradeUI] Subscribed to HeroLevelUpEvent");
        }
        
        private void CreateUpgradePanel()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UpgradeUI] No Canvas found in scene!");
                return;
            }
            
            // Create panel
            upgradePanel = new GameObject("UpgradePanel");
            upgradePanel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = upgradePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            Image panelImage = upgradePanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            // Create vertical layout for buttons
            GameObject layoutObj = new GameObject("ButtonLayout");
            layoutObj.transform.SetParent(upgradePanel.transform, false);
            
            RectTransform layoutRect = layoutObj.AddComponent<RectTransform>();
            layoutRect.anchorMin = new Vector2(0.5f, 0.5f);
            layoutRect.anchorMax = new Vector2(0.5f, 0.5f);
            layoutRect.sizeDelta = new Vector2(400, 500);
            
            UnityEngine.UI.VerticalLayoutGroup layout = layoutObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            // Create title
            CreateText(layoutObj.transform, "LEVEL UP!", 40);
            CreateText(layoutObj.transform, "Choose an upgrade:", 24);
            
            // Create 3 buttons (stat upgrades + weapon upgrade option)
            damageUpgradeButton = CreateButton(layoutObj.transform, "Increase Damage (+10)");
            attackSpeedUpgradeButton = CreateButton(layoutObj.transform, "Increase Attack Speed (+0.5)");
            healthUpgradeButton = CreateButton(layoutObj.transform, "Increase Max Health (+30)");
            
            // TODO: Add weapon upgrade button dynamically based on current weapon tier
            
            upgradePanel.SetActive(false);
            Debug.Log("[UpgradeUI] Created upgrade panel programmatically with 3 choices");
        }
        
        private GameObject CreateText(Transform parent, string text, float fontSize)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return textObj;
        }
        
        private Button CreateButton(Transform parent, string label)
        {
            GameObject buttonObj = new GameObject("Button_" + label);
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            
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
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return button;
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<HeroLevelUpEvent>(OnHeroLevelUp);
        }
        
        private void OnHeroLevelUp(ISimulationEvent evt)
        {
            if (evt is HeroLevelUpEvent levelUpEvent)
            {
                Debug.Log($"[UpgradeUI] ===== LEVEL UP EVENT RECEIVED =====");
                Debug.Log($"[UpgradeUI] Hero {levelUpEvent.HeroId.Value} leveled up to level {levelUpEvent.NewLevel}");
                Debug.Log($"[UpgradeUI] Player hero ID: {playerHeroId.Value}");
                Debug.Log($"[UpgradeUI] IDs equal: {levelUpEvent.HeroId.Equals(playerHeroId)}");
                
                // Show upgrade panel for ANY hero (for now, since we only have one)
                Debug.Log($"[UpgradeUI] Showing upgrade panel!");
                ShowUpgradePanel(levelUpEvent.HeroId);
            }
        }
        
        public void ShowUpgradePanel(EntityId heroId)
        {
            Debug.Log($"[UpgradeUI] ShowUpgradePanel called for hero {heroId.Value}");
            Debug.Log($"[UpgradeUI] upgradePanel null? {upgradePanel == null}");
            
            playerHeroId = heroId;
            upgradeAvailable = true;
            
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
                Debug.Log($"[UpgradeUI] Upgrade panel activated! timeScale set to 0");
            }
            else
            {
                Debug.LogError("[UpgradeUI] upgradePanel is NULL!");
            }
            
            // Pause game
            Time.timeScale = 0f;
        }
        
        private void ChooseUpgrade(string upgradeType)
        {
            if (!upgradeAvailable) return;
            
            Debug.Log($"[UpgradeUI] Player chose upgrade: {upgradeType}");
            
            ChooseUpgradeCommand cmd = new ChooseUpgradeCommand
            {
                HeroId = playerHeroId,
                UpgradeType = upgradeType,
                UpgradeTier = 1
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            
            upgradeAvailable = false;
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            // Resume game
            Time.timeScale = 1f;
            Debug.Log($"[UpgradeUI] Upgrade chosen, resuming game (timeScale = 1)");
        }
        
        public void SetPlayerHero(EntityId heroId)
        {
            playerHeroId = heroId;
            Debug.Log($"[UpgradeUI] Player hero set to ID: {heroId.Value}");
        }
    }
}

