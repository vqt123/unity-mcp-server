using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Data;
using System.Collections.Generic;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages upgrade UI and initial hero selection
    /// </summary>
    public class UpgradeUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private Button[] upgradeButtons = new Button[3];
        
        private EntityId playerHeroId;
        private bool hasSelectedInitialHero = false;
        private List<string> spawnedHeroTypes = new List<string>();
        
        void Start()
        {
            Debug.Log("[UpgradeUI] Starting UpgradeUIManager");
            
            // Create UI if not assigned
            if (upgradePanel == null)
            {
                CreateUpgradePanel();
            }
            else
            {
                upgradePanel.SetActive(false);
            }
            
            // Subscribe to level-up events
            EventBus.Subscribe<HeroLevelUpEvent>(OnHeroLevelUp);
            
            Debug.Log("[UpgradeUI] Initialized and subscribed to HeroLevelUpEvent");
            
            // Show hero selection at game start
            Invoke(nameof(ShowInitialHeroSelection), 0.2f);
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
            
            VerticalLayoutGroup layout = layoutObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            // Create title
            CreateText(layoutObj.transform, "LEVEL UP!", 40, "TitleText");
            CreateText(layoutObj.transform, "Choose an upgrade:", 24, "SubtitleText");
            
            // Create 3 buttons
            upgradeButtons[0] = CreateButton(layoutObj.transform, "Choice 1");
            upgradeButtons[1] = CreateButton(layoutObj.transform, "Choice 2");
            upgradeButtons[2] = CreateButton(layoutObj.transform, "Choice 3");
            
            upgradePanel.SetActive(false);
            Debug.Log("[UpgradeUI] Created upgrade panel programmatically");
        }
        
        private GameObject CreateText(Transform parent, string text, float fontSize, string objName = "Text")
        {
            GameObject textObj = new GameObject(objName);
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
            rect.sizeDelta = new Vector2(400, 80);
            
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
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return button;
        }
        
        void OnDestroy()
        {
            EventBus.Unsubscribe<HeroLevelUpEvent>(OnHeroLevelUp);
        }
        
        private void ShowInitialHeroSelection()
        {
            if (hasSelectedInitialHero) return;
            
            PlayerDataManager dataManager = PlayerDataManager.Instance;
            if (dataManager == null || dataManager.HeroInventory.partyHeroes.Count == 0)
            {
                Debug.LogError("[UpgradeUI] No heroes in party!");
                return;
            }
            
            var party = dataManager.HeroInventory.partyHeroes;
            Debug.Log($"[UpgradeUI] Showing initial hero selection from party of {party.Count}");
            
            // Get 3 random heroes from party (or less if party is smaller)
            List<string> heroChoices = new List<string>();
            List<string> shuffledParty = new List<string>(party);
            
            // Shuffle the party list
            for (int i = 0; i < shuffledParty.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffledParty.Count);
                string temp = shuffledParty[i];
                shuffledParty[i] = shuffledParty[randomIndex];
                shuffledParty[randomIndex] = temp;
            }
            
            // Take up to 3 heroes
            for (int i = 0; i < Mathf.Min(3, shuffledParty.Count); i++)
            {
                heroChoices.Add(shuffledParty[i]);
            }
            
            ShowHeroSelectionPanel(heroChoices);
        }
        
        private void ShowHeroSelectionPanel(List<string> heroChoices)
        {
            if (upgradePanel == null)
            {
                Debug.LogError("[UpgradeUI] Upgrade panel is null!");
                return;
            }
            
            // Pause game
            Time.timeScale = 0f;
            
            // Update panel title
            Transform titleTransform = upgradePanel.transform.Find("ButtonLayout/TitleText");
            if (titleTransform != null)
            {
                var titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = "Choose Your Hero";
                }
            }
            
            Transform subtitleTransform = upgradePanel.transform.Find("ButtonLayout/SubtitleText");
            if (subtitleTransform != null)
            {
                var subtitleText = subtitleTransform.GetComponent<TextMeshProUGUI>();
                if (subtitleText != null)
                {
                    subtitleText.text = "Select a hero to begin battle:";
                }
            }
            
            // Update buttons with hero choices
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                if (i < heroChoices.Count)
                {
                    string heroType = heroChoices[i];
                    upgradeButtons[i].gameObject.SetActive(true);
                    
                    // Update button text
                    var buttonText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        HeroConfig config = HeroData.GetConfig(heroType);
                        buttonText.text = $"{heroType}\nHP: {config.MaxHealth.ToInt()} | DMG: {config.Damage.ToInt()} | ATK SPD: {config.AttackSpeed.ToFloat():F1}";
                    }
                    
                    // Wire up button click
                    upgradeButtons[i].onClick.RemoveAllListeners();
                    upgradeButtons[i].onClick.AddListener(() => OnHeroSelected(heroType));
                }
                else
                {
                    upgradeButtons[i].gameObject.SetActive(false);
                }
            }
            
            upgradePanel.SetActive(true);
            Debug.Log("[UpgradeUI] Hero selection panel shown");
        }
        
        private void OnHeroSelected(string heroType)
        {
            Debug.Log($"[UpgradeUI] Hero selected: {heroType}");
            
            hasSelectedInitialHero = true;
            spawnedHeroTypes.Add(heroType);
            
            // Hide panel
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            // Spawn hero via PartySpawner
            PartySpawner spawner = PartySpawner.Instance;
            if (spawner != null)
            {
                spawner.SpawnSelectedHero(heroType);
            }
            else
            {
                Debug.LogError("[UpgradeUI] PartySpawner not found!");
            }
            
            // Unpause game
            Time.timeScale = 1f;
            Debug.Log("[UpgradeUI] Game started");
        }
        
        private void SpawnAdditionalHero(string heroType)
        {
            Debug.Log($"[UpgradeUI] Spawning additional hero: {heroType}");
            
            spawnedHeroTypes.Add(heroType);
            
            // Hide panel
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            // Spawn hero via PartySpawner
            PartySpawner spawner = PartySpawner.Instance;
            if (spawner != null)
            {
                spawner.SpawnAdditionalHero(heroType);
            }
            else
            {
                Debug.LogError("[UpgradeUI] PartySpawner not found!");
            }
            
            // Resume game
            Time.timeScale = 1f;
            Debug.Log("[UpgradeUI] Additional hero spawned, game resumed");
        }
        
        private void OnHeroLevelUp(ISimulationEvent evt)
        {
            if (hasSelectedInitialHero && evt is HeroLevelUpEvent levelUpEvent)
            {
                Debug.Log($"[UpgradeUI] Hero leveled up to {levelUpEvent.NewLevel}");
                ShowUpgradePanel(levelUpEvent.HeroId);
            }
        }
        
        private void ShowUpgradePanel(EntityId heroId)
        {
            if (upgradePanel == null) return;
            
            playerHeroId = heroId;
            
            // Pause game
            Time.timeScale = 0f;
            
            // Update panel title
            Transform titleTransform = upgradePanel.transform.Find("ButtonLayout/TitleText");
            if (titleTransform != null)
            {
                var titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = "LEVEL UP!";
                }
            }
            
            Transform subtitleTransform = upgradePanel.transform.Find("ButtonLayout/SubtitleText");
            if (subtitleTransform != null)
            {
                var subtitleText = subtitleTransform.GetComponent<TextMeshProUGUI>();
                if (subtitleText != null)
                {
                    subtitleText.text = "Choose an upgrade:";
                }
            }
            
            // Check if there are unspawned heroes from party
            PlayerDataManager dataManager = PlayerDataManager.Instance;
            List<string> unspawnedHeroes = new List<string>();
            
            if (dataManager != null)
            {
                var party = dataManager.HeroInventory.partyHeroes;
                foreach (string heroType in party)
                {
                    if (!spawnedHeroTypes.Contains(heroType))
                    {
                        unspawnedHeroes.Add(heroType);
                    }
                }
            }
            
            // Show upgrade choices (2 stat upgrades + 1 hero if available)
            string[] upgradeTypes = { "Damage", "AttackSpeed", "Health" };
            string[] upgradeLabels = { 
                "Increase Damage\n+10 DMG", 
                "Increase Attack Speed\n+0.5 ATK/SEC",
                "Increase Max Health\n+30 HP"
            };
            
            // Decide which 2 stat upgrades to show (randomize)
            List<int> statIndices = new List<int> { 0, 1, 2 };
            for (int i = 0; i < statIndices.Count; i++)
            {
                int randomIndex = Random.Range(i, statIndices.Count);
                int temp = statIndices[i];
                statIndices[i] = statIndices[randomIndex];
                statIndices[randomIndex] = temp;
            }
            
            // Show 2 stat upgrades
            for (int i = 0; i < 2; i++)
            {
                int statIndex = statIndices[i];
                upgradeButtons[i].gameObject.SetActive(true);
                
                var buttonText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = upgradeLabels[statIndex];
                }
                
                string upgradeType = upgradeTypes[statIndex];
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(upgradeType));
            }
            
            // Show hero choice if available
            if (unspawnedHeroes.Count > 0)
            {
                // Pick random unspawned hero
                string randomHeroType = unspawnedHeroes[Random.Range(0, unspawnedHeroes.Count)];
                
                upgradeButtons[2].gameObject.SetActive(true);
                var buttonText = upgradeButtons[2].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    HeroConfig config = HeroData.GetConfig(randomHeroType);
                    buttonText.text = $"Add Hero: {randomHeroType}\nHP: {config.MaxHealth.ToInt()} | DMG: {config.Damage.ToInt()}";
                }
                
                upgradeButtons[2].onClick.RemoveAllListeners();
                upgradeButtons[2].onClick.AddListener(() => SpawnAdditionalHero(randomHeroType));
                
                Debug.Log($"[UpgradeUI] Offering hero choice: {randomHeroType}");
            }
            else
            {
                // No more heroes, show 3rd stat upgrade
                int statIndex = statIndices[2];
                upgradeButtons[2].gameObject.SetActive(true);
                
                var buttonText = upgradeButtons[2].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = upgradeLabels[statIndex];
                }
                
                string upgradeType = upgradeTypes[statIndex];
                upgradeButtons[2].onClick.RemoveAllListeners();
                upgradeButtons[2].onClick.AddListener(() => OnUpgradeSelected(upgradeType));
                
                Debug.Log("[UpgradeUI] All heroes spawned - showing 3 stat upgrades");
            }
            
            upgradePanel.SetActive(true);
            Debug.Log("[UpgradeUI] Upgrade panel shown");
        }
        
        private void OnUpgradeSelected(string upgradeType)
        {
            Debug.Log($"[UpgradeUI] Upgrade selected: {upgradeType}");
            
            ChooseUpgradeCommand cmd = new ChooseUpgradeCommand
            {
                HeroId = playerHeroId,
                UpgradeType = upgradeType,
                UpgradeTier = 1
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            // Resume game
            Time.timeScale = 1f;
            Debug.Log("[UpgradeUI] Upgrade chosen, game resumed");
        }
        
        public void SetPlayerHero(EntityId heroId)
        {
            playerHeroId = heroId;
            Debug.Log($"[UpgradeUI] Player hero set to ID: {heroId.Value}");
        }
    }
}
