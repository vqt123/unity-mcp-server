using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ArenaGame.Client
{
    /// <summary>
    /// UI for managing hero inventory and party composition
    /// </summary>
    public class HeroInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform heroCardContainer;
        [SerializeField] private Button backButton;
        
        private HomeMenuUI homeMenu;
        private List<HeroCard> heroCards = new List<HeroCard>();
        
        public void Show(HomeMenuUI menu)
        {
            homeMenu = menu;
            
            if (panel == null)
            {
                CreateInventoryUI();
            }
            
            RefreshHeroCards();
            gameObject.SetActive(true);
        }
        
        private void CreateInventoryUI()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[HeroInventory] No canvas found!");
                return;
            }
            
            // Create full-screen panel
            panel = new GameObject("HeroInventoryPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            
            // Create header
            GameObject header = new GameObject("Header");
            header.transform.SetParent(panel.transform, false);
            
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.anchoredPosition = new Vector2(0, -50);
            headerRect.sizeDelta = new Vector2(0, 100);
            
            TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
            headerText.text = "HERO ROSTER";
            headerText.fontSize = 48;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = Color.white;
            
            // Create party info text
            GameObject partyInfo = new GameObject("PartyInfo");
            partyInfo.transform.SetParent(panel.transform, false);
            
            RectTransform partyRect = partyInfo.AddComponent<RectTransform>();
            partyRect.anchorMin = new Vector2(0f, 1f);
            partyRect.anchorMax = new Vector2(1f, 1f);
            partyRect.anchoredPosition = new Vector2(0, -120);
            partyRect.sizeDelta = new Vector2(0, 40);
            
            TextMeshProUGUI partyText = partyInfo.AddComponent<TextMeshProUGUI>();
            partyText.name = "PartyInfoText";
            partyText.text = "Party: 0/5 | Click hero to add/remove from party";
            partyText.fontSize = 24;
            partyText.alignment = TextAlignmentOptions.Center;
            partyText.color = Color.yellow;
            
            // Create scroll view for hero cards
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(panel.transform, false);
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.1f, 0.15f);
            scrollRect.anchorMax = new Vector2(0.9f, 0.75f);
            scrollRect.sizeDelta = Vector2.zero;
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            
            // Create viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            viewport.AddComponent<Image>();
            
            // Create content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 280);
            grid.spacing = new Vector2(20, 20);
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            heroCardContainer = content.transform;
            
            // Create back button
            GameObject backButtonObj = new GameObject("BackButton");
            backButtonObj.transform.SetParent(panel.transform, false);
            
            RectTransform backRect = backButtonObj.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0, 50);
            backRect.sizeDelta = new Vector2(200, 60);
            
            Image backImage = backButtonObj.AddComponent<Image>();
            backImage.color = new Color(0.8f, 0.2f, 0.2f);
            
            backButton = backButtonObj.AddComponent<Button>();
            backButton.onClick.AddListener(OnBackClicked);
            
            GameObject backText = new GameObject("Text");
            backText.transform.SetParent(backButtonObj.transform, false);
            
            RectTransform backTextRect = backText.AddComponent<RectTransform>();
            backTextRect.anchorMin = Vector2.zero;
            backTextRect.anchorMax = Vector2.one;
            backTextRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI backTMP = backText.AddComponent<TextMeshProUGUI>();
            backTMP.text = "BACK";
            backTMP.fontSize = 28;
            backTMP.alignment = TextAlignmentOptions.Center;
            backTMP.color = Color.white;
            
            Debug.Log("[HeroInventory] Created inventory UI");
        }
        
        private void RefreshHeroCards()
        {
            // Clear existing cards
            foreach (var card in heroCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            heroCards.Clear();
            
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[HeroInventory] PlayerDataManager not found!");
                return;
            }
            
            var inventory = PlayerDataManager.Instance.HeroInventory;
            
            // Create card for each unlocked hero
            foreach (string heroType in inventory.unlockedHeroes)
            {
                CreateHeroCard(heroType, inventory.IsInParty(heroType));
            }
            
            // Update party info text
            UpdatePartyInfo();
            
            Debug.Log($"[HeroInventory] Created {heroCards.Count} hero cards");
        }
        
        private void CreateHeroCard(string heroType, bool inParty)
        {
            GameObject cardObj = new GameObject($"HeroCard_{heroType}");
            cardObj.transform.SetParent(heroCardContainer, false);
            
            // Add card component
            HeroCard card = cardObj.AddComponent<HeroCard>();
            card.Setup(heroType, inParty, this);
            
            heroCards.Add(card);
        }
        
        public void OnHeroCardClicked(string heroType)
        {
            var inventory = PlayerDataManager.Instance.HeroInventory;
            
            if (inventory.IsInParty(heroType))
            {
                // Remove from party
                if (inventory.RemoveFromParty(heroType))
                {
                    Debug.Log($"[HeroInventory] Removed {heroType} from party");
                    PlayerDataManager.Instance.SaveData();
                    RefreshHeroCards();
                }
            }
            else
            {
                // Add to party
                if (inventory.AddToParty(heroType))
                {
                    Debug.Log($"[HeroInventory] Added {heroType} to party");
                    PlayerDataManager.Instance.SaveData();
                    RefreshHeroCards();
                }
            }
        }
        
        private void UpdatePartyInfo()
        {
            var inventory = PlayerDataManager.Instance.HeroInventory;
            TextMeshProUGUI partyText = panel.transform.Find("PartyInfo")?.GetComponent<TextMeshProUGUI>();
            
            if (partyText != null)
            {
                partyText.text = $"Party: {inventory.partyHeroes.Count}/{HeroInventoryData.MAX_PARTY_SIZE} | Click hero to add/remove from party";
            }
        }
        
        private void OnBackClicked()
        {
            Debug.Log("[HeroInventory] Back clicked");
            gameObject.SetActive(false);
            
            if (homeMenu != null)
            {
                homeMenu.Show();
            }
        }
    }
    
    /// <summary>
    /// Individual hero card component
    /// </summary>
    public class HeroCard : MonoBehaviour
    {
        private string heroType;
        private bool inParty;
        private HeroInventoryUI inventoryUI;
        private Image cardImage;
        private TextMeshProUGUI nameText;
        private GameObject partyIndicator;
        
        public void Setup(string type, bool isInParty, HeroInventoryUI ui)
        {
            heroType = type;
            inParty = isInParty;
            inventoryUI = ui;
            
            CreateCardUI();
        }
        
        private void CreateCardUI()
        {
            // Card background
            cardImage = gameObject.AddComponent<Image>();
            cardImage.color = inParty ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
            
            Button button = gameObject.AddComponent<Button>();
            button.onClick.AddListener(OnClick);
            
            // Hero name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(transform, false);
            
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.7f);
            nameRect.anchorMax = new Vector2(1f, 0.95f);
            nameRect.sizeDelta = Vector2.zero;
            
            nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = heroType;
            nameText.fontSize = 24;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            
            // Party indicator
            if (inParty)
            {
                partyIndicator = new GameObject("PartyIndicator");
                partyIndicator.transform.SetParent(transform, false);
                
                RectTransform indicatorRect = partyIndicator.AddComponent<RectTransform>();
                indicatorRect.anchorMin = new Vector2(0f, 0f);
                indicatorRect.anchorMax = new Vector2(1f, 0.2f);
                indicatorRect.sizeDelta = Vector2.zero;
                
                TextMeshProUGUI indicatorText = partyIndicator.AddComponent<TextMeshProUGUI>();
                indicatorText.text = "★ IN PARTY ★";
                indicatorText.fontSize = 20;
                indicatorText.alignment = TextAlignmentOptions.Center;
                indicatorText.color = Color.yellow;
            }
            
            // Stats preview (middle section)
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(transform, false);
            
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, 0.3f);
            statsRect.anchorMax = new Vector2(0.9f, 0.65f);
            statsRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = GetHeroStats(heroType);
            statsText.fontSize = 16;
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.color = new Color(0.9f, 0.9f, 0.9f);
        }
        
        private string GetHeroStats(string type)
        {
            // Simple stat display
            return type switch
            {
                "DefaultHero" => "Balanced\nDmg: 100\nAtkSpd: 3.3",
                "FastHero" => "Fast Attacker\nDmg: 100\nAtkSpd: 3.3",
                "TankHero" => "High Health\nDmg: 100\nAtkSpd: 2.0",
                "Archer" => "Range DPS\nDmg: 100\nAtkSpd: 3.3",
                "Mage" => "Magic DPS\nDmg: 100\nAtkSpd: 1.67",
                "Warrior" => "Melee Tank\nDmg: 100\nAtkSpd: 2.0",
                _ => "Hero"
            };
        }
        
        private void OnClick()
        {
            inventoryUI.OnHeroCardClicked(heroType);
        }
    }
}

