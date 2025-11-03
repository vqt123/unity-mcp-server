using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Entities;
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
        private List<string> spawnedHeroTypes = new List<string>();
        private bool waitingForFirstHeroSpawn = false; // Track if we're waiting for first hero to spawn
        
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
            
            // Subscribe to events
            EventBus.Subscribe<HeroLevelUpEvent>(OnHeroLevelUp);
            EventBus.Subscribe<HeroSpawnedEvent>(OnHeroSpawned);
            
            Debug.Log("[UpgradeUI] Initialized and subscribed to HeroLevelUpEvent and HeroSpawnedEvent");
            
            // Arena starts with 0 heroes - trigger initial upgrade (hero selection)
            StartCoroutine(TriggerInitialLevelUpForNoHeroDelayed());
        }
        
        void OnDestroy()
        {
            EventBus.Unsubscribe<HeroLevelUpEvent>(OnHeroLevelUp);
            EventBus.Unsubscribe<HeroSpawnedEvent>(OnHeroSpawned);
            // Stop any running coroutines
            StopAllCoroutines();
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
        
        private System.Collections.IEnumerator TriggerInitialLevelUpForNoHeroDelayed()
        {
            // Wait a short delay to ensure everything is initialized
            yield return new WaitForSeconds(0.3f);
            
            // If no heroes are spawned yet, show hero selection panel
            Debug.Log("[UpgradeUI] Triggering initial level-up upgrade (hero selection)");
            
            // Use a dummy hero ID - the upgrade panel will handle showing heroes since none are spawned
            playerHeroId = EntityId.Invalid; // Will be set when first hero is chosen
            
            // Show upgrade panel with hero selection (1 of 2)
            ShowUpgradePanelForLevel1FirstUpgrade();
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
            Debug.Log($"[UpgradeUI] OnHeroSelected called: {heroType}");
            
            spawnedHeroTypes.Add(heroType);
            Debug.Log($"[UpgradeUI] Spawned hero types count: {spawnedHeroTypes.Count}");
            
            // Hide panel
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
                Debug.Log("[UpgradeUI] Upgrade panel hidden");
            }
            
            // Resume game FIRST so simulation can process the spawn command
            Time.timeScale = 1f;
            Debug.Log("[UpgradeUI] Resuming game (Time.timeScale = 1) so simulation can process spawn command");
            
            // Spawn hero via PartySpawner
            PartySpawner spawner = PartySpawner.Instance;
            if (spawner != null)
            {
                Debug.Log($"[UpgradeUI] Calling PartySpawner.SpawnSelectedHero({heroType})");
                spawner.SpawnSelectedHero(heroType);
            }
            else
            {
                Debug.LogError("[UpgradeUI] PartySpawner.Instance is null!");
            }
            
            // If this is the first hero, wait for it to spawn then trigger level up
            if (spawnedHeroTypes.Count == 1)
            {
                waitingForFirstHeroSpawn = true;
                Debug.Log("[UpgradeUI] First hero selected - waiting for spawn event (waitingForFirstHeroSpawn = true)");
                // Level up will be triggered by OnHeroSpawned event
            }
            else
            {
                Debug.Log("[UpgradeUI] Additional hero spawned");
            }
        }
        
        /// <summary>
        /// Event-driven: Hero spawned - check if we need to trigger initial level up
        /// </summary>
        private void OnHeroSpawned(ISimulationEvent evt)
        {
            Debug.Log($"[UpgradeUI] OnHeroSpawned event received, type: {evt.GetType().Name}");
            
            if (evt is HeroSpawnedEvent spawnEvent)
            {
                Debug.Log($"[UpgradeUI] HeroSpawnedEvent - HeroId: {spawnEvent.HeroId.Value}, HeroType: {spawnEvent.HeroType}, waitingForFirstHeroSpawn: {waitingForFirstHeroSpawn}");
                
                // If waiting for first hero spawn, trigger level up from 0 to 1
                if (waitingForFirstHeroSpawn)
                {
                    Debug.Log("[UpgradeUI] Processing first hero spawn - triggering level up");
                    waitingForFirstHeroSpawn = false;
                    
                    if (GameSimulation.Instance != null)
                    {
                        var world = GameSimulation.Instance.Simulation.World;
                        Debug.Log($"[UpgradeUI] GameSimulation found, current tick: {world.CurrentTick}");
                        
                        if (world.TryGetHero(spawnEvent.HeroId, out Hero hero))
                        {
                            Debug.Log($"[UpgradeUI] Hero found in world, current level: {hero.Level}");
                            
                            // Verify hero is at level 0
                            if (hero.Level == 0)
                            {
                                Debug.Log("[UpgradeUI] Hero is at level 0 - leveling up to 1");
                                
                                // Level up from 0 to 1
                                hero.Level = 1;
                                hero.CurrentXP = 0;
                                world.UpdateHero(spawnEvent.HeroId, hero);
                                
                                // Pause game before showing upgrade panel
                                Time.timeScale = 0f;
                                Debug.Log("[UpgradeUI] Paused game for upgrade panel");
                                
                                // Publish level up event via EventBus (which will trigger OnHeroLevelUp)
                                EventBus.Publish(new HeroLevelUpEvent
                                {
                                    Tick = world.CurrentTick,
                                    HeroId = spawnEvent.HeroId,
                                    NewLevel = 1
                                });
                                
                                Debug.Log($"[UpgradeUI] Hero leveled up from 0 to 1 - published HeroLevelUpEvent for hero {spawnEvent.HeroId.Value}");
                            }
                            else
                            {
                                Debug.LogWarning($"[UpgradeUI] Hero spawned but already at level {hero.Level}, expected 0");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[UpgradeUI] Hero {spawnEvent.HeroId.Value} not found in world!");
                        }
                    }
                    else
                    {
                        Debug.LogError("[UpgradeUI] GameSimulation.Instance is null!");
                    }
                }
                else
                {
                    Debug.Log($"[UpgradeUI] Hero spawned but not waiting for first hero (waitingForFirstHeroSpawn = false)");
                }
            }
            else
            {
                Debug.LogWarning($"[UpgradeUI] Received non-HeroSpawnedEvent: {evt.GetType().Name}");
            }
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
            if (evt is HeroLevelUpEvent levelUpEvent)
            {
                Debug.Log($"[UpgradeUI] Hero leveled up to {levelUpEvent.NewLevel}, spawned heroes: {spawnedHeroTypes.Count}");
                
                playerHeroId = levelUpEvent.HeroId;
                
                // Check upgrade progress to determine which panel to show
                PlayerDataManager dataMgr = PlayerDataManager.Instance;
                int upgradesChosen = dataMgr != null ? dataMgr.GetUpgradesAtLevel1() : 0;
                
                if (spawnedHeroTypes.Count == 0)
                {
                    // No heroes spawned yet - show first upgrade (hero selection)
                    ShowUpgradePanelForLevel1FirstUpgrade();
                }
                else if (levelUpEvent.NewLevel == 1 && upgradesChosen == 1)
                {
                    // Hero just leveled to 1 and first upgrade was chosen - show second upgrade
                    Debug.Log("[UpgradeUI] Showing second upgrade (2 of 2)");
                    ShowUpgradePanelForLevel1SecondUpgrade();
                }
                else
                {
                    // Normal level up
                    ShowUpgradePanel(levelUpEvent.HeroId);
                }
            }
        }
        
        private void ShowUpgradePanel(EntityId heroId)
        {
            if (upgradePanel == null) return;
            
            playerHeroId = heroId;
            
            // Get hero's actual level from the world
            int heroLevel = 1;
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(heroId, out Hero hero))
                {
                    heroLevel = hero.Level;
                }
            }
            
            // Check if this is level 1 and determine which upgrade (1 of 2 or 2 of 2)
            PlayerDataManager dataMgr = PlayerDataManager.Instance;
            bool isLevel1 = heroLevel == 1;
            int upgradesChosen = dataMgr != null ? dataMgr.GetUpgradesAtLevel1() : 0;
            
            if (isLevel1 && upgradesChosen == 0)
            {
                // First upgrade at level 1 - should already be handled by ShowUpgradePanelForLevel1FirstUpgrade
                // But if called from level-up event, handle it
                ShowUpgradePanelForLevel1FirstUpgrade();
                return;
            }
            else if (isLevel1 && upgradesChosen == 1)
            {
                // Second upgrade at level 1
                ShowUpgradePanelForLevel1SecondUpgrade();
                return;
            }
            else
            {
                // Normal level-up (not level 1)
                ShowNormalUpgradePanel(heroLevel);
            }
        }
        
        private void ShowUpgradePanelForLevel1FirstUpgrade()
        {
            if (upgradePanel == null) return;
            
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
                    subtitleText.text = "Choose an upgrade: (1 of 2)";
                }
            }
            
            // Get party heroes
            PlayerDataManager dataManager = PlayerDataManager.Instance;
            List<string> availableHeroes = new List<string>();
            
            if (dataManager != null)
            {
                var party = dataManager.HeroInventory.partyHeroes;
                foreach (string heroType in party)
                {
                    if (!spawnedHeroTypes.Contains(heroType))
                    {
                        availableHeroes.Add(heroType);
                    }
                }
            }
            
            // First upgrade: Show 3 heroes (or all available if less than 3)
            int heroesToShow = Mathf.Min(3, availableHeroes.Count);
            
            // Shuffle heroes
            List<string> shuffledHeroes = new List<string>(availableHeroes);
            for (int i = 0; i < shuffledHeroes.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffledHeroes.Count);
                string temp = shuffledHeroes[i];
                shuffledHeroes[i] = shuffledHeroes[randomIndex];
                shuffledHeroes[randomIndex] = temp;
            }
            
            // Show up to 3 heroes
            for (int i = 0; i < heroesToShow && i < upgradeButtons.Length; i++)
            {
                string heroType = shuffledHeroes[i];
                upgradeButtons[i].gameObject.SetActive(true);
                
                var buttonText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    HeroConfig config = HeroData.GetConfig(heroType);
                    buttonText.text = $"{heroType}\nHP: {config.MaxHealth.ToInt()} | DMG: {config.Damage.ToInt()}";
                }
                
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnHeroSelectedFromUpgrade(heroType));
            }
            
            // Hide unused buttons
            for (int i = heroesToShow; i < upgradeButtons.Length; i++)
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
            
            upgradePanel.SetActive(true);
            Debug.Log($"[UpgradeUI] Showing {heroesToShow} heroes for first upgrade (1 of 2)");
        }
        
        private void ShowUpgradePanelForLevel1SecondUpgrade()
        {
            if (upgradePanel == null) return;
            
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
                    subtitleText.text = "Choose an upgrade: (2 of 2)";
                }
            }
            
            // Get unspawned heroes and stat upgrades
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
            
            // Second upgrade: Randomly show either heroes OR stat upgrades
            string[] upgradeTypes = { "Damage", "AttackSpeed", "Health" };
            string[] upgradeLabels = { 
                "Increase Damage\n+10 DMG", 
                "Increase Attack Speed\n+0.5 ATK/SEC",
                "Increase Max Health\n+30 HP"
            };
            
            List<int> statIndices = new List<int> { 0, 1, 2 };
            for (int i = 0; i < statIndices.Count; i++)
            {
                int randomIndex = Random.Range(i, statIndices.Count);
                int temp = statIndices[i];
                statIndices[i] = statIndices[randomIndex];
                statIndices[randomIndex] = temp;
            }
            
            // Randomly decide: show heroes OR stat upgrades
            bool showHeroes = (unspawnedHeroes.Count > 0) && (Random.value > 0.5f);
            
            if (showHeroes && unspawnedHeroes.Count > 0)
            {
                // Show 1 random hero
                string randomHeroType = unspawnedHeroes[Random.Range(0, unspawnedHeroes.Count)];
                
                upgradeButtons[0].gameObject.SetActive(true);
                var buttonText = upgradeButtons[0].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    HeroConfig config = HeroData.GetConfig(randomHeroType);
                    buttonText.text = $"Add Hero: {randomHeroType}\nHP: {config.MaxHealth.ToInt()} | DMG: {config.Damage.ToInt()}";
                }
                
                upgradeButtons[0].onClick.RemoveAllListeners();
                upgradeButtons[0].onClick.AddListener(() => SpawnAdditionalHero(randomHeroType));
                
                // Show 2 stat upgrades
                for (int i = 0; i < 2 && i < statIndices.Count; i++)
                {
                    int statIndex = statIndices[i];
                    upgradeButtons[i + 1].gameObject.SetActive(true);
                    
                    var btnText = upgradeButtons[i + 1].GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = upgradeLabels[statIndex];
                    }
                    
                    string upgradeType = upgradeTypes[statIndex];
                    upgradeButtons[i + 1].onClick.RemoveAllListeners();
                    upgradeButtons[i + 1].onClick.AddListener(() => OnUpgradeSelected(upgradeType));
                }
                
                // Hide third button
                upgradeButtons[2].gameObject.SetActive(false);
                
                Debug.Log($"[UpgradeUI] Showing 1 hero + 2 stat upgrades for second upgrade (2 of 2)");
            }
            else
            {
                // Show 3 stat upgrades
                for (int i = 0; i < 3 && i < statIndices.Count; i++)
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
                
                Debug.Log($"[UpgradeUI] Showing 3 stat upgrades for second upgrade (2 of 2)");
            }
            
            upgradePanel.SetActive(true);
            Debug.Log("[UpgradeUI] Upgrade panel shown for second upgrade");
        }
        
        private void ShowNormalUpgradePanel(int heroLevel)
        {
            if (upgradePanel == null) return;
            
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
            
            // Normal upgrade: 1 upgrade (hero or stat)
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
            
            string[] upgradeTypes = { "Damage", "AttackSpeed", "Health" };
            string[] upgradeLabels = { 
                "Increase Damage\n+10 DMG", 
                "Increase Attack Speed\n+0.5 ATK/SEC",
                "Increase Max Health\n+30 HP"
            };
            
            List<int> statIndices = new List<int> { 0, 1, 2 };
            for (int i = 0; i < statIndices.Count; i++)
            {
                int randomIndex = Random.Range(i, statIndices.Count);
                int temp = statIndices[i];
                statIndices[i] = statIndices[randomIndex];
                statIndices[randomIndex] = temp;
            }
            
            // Show 1 upgrade: hero if available, otherwise stat
            if (unspawnedHeroes.Count > 0)
            {
                string randomHeroType = unspawnedHeroes[Random.Range(0, unspawnedHeroes.Count)];
                
                upgradeButtons[0].gameObject.SetActive(true);
                var buttonText = upgradeButtons[0].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    HeroConfig config = HeroData.GetConfig(randomHeroType);
                    buttonText.text = $"Add Hero: {randomHeroType}\nHP: {config.MaxHealth.ToInt()} | DMG: {config.Damage.ToInt()}";
                }
                
                upgradeButtons[0].onClick.RemoveAllListeners();
                upgradeButtons[0].onClick.AddListener(() => SpawnAdditionalHero(randomHeroType));
                
                // Hide other buttons
                for (int i = 1; i < upgradeButtons.Length; i++)
                {
                    upgradeButtons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                // Show 1 random stat upgrade
                int statIndex = statIndices[0];
                upgradeButtons[0].gameObject.SetActive(true);
                
                var buttonText = upgradeButtons[0].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = upgradeLabels[statIndex];
                }
                
                string upgradeType = upgradeTypes[statIndex];
                upgradeButtons[0].onClick.RemoveAllListeners();
                upgradeButtons[0].onClick.AddListener(() => OnUpgradeSelected(upgradeType));
                
                // Hide other buttons
                for (int i = 1; i < upgradeButtons.Length; i++)
                {
                    upgradeButtons[i].gameObject.SetActive(false);
                }
            }
            
            upgradePanel.SetActive(true);
            Debug.Log("[UpgradeUI] Normal upgrade panel shown");
        }
        
        private void OnHeroSelectedFromUpgrade(string heroType)
        {
            Debug.Log($"[UpgradeUI] Hero selected from first upgrade: {heroType}");
            
            // Track this as first upgrade BEFORE spawning hero
            PlayerDataManager dataMgr = PlayerDataManager.Instance;
            if (dataMgr != null)
            {
                dataMgr.IncrementUpgradesAtLevel1();
                Debug.Log($"[UpgradeUI] First upgrade tracked, upgrades chosen: {dataMgr.GetUpgradesAtLevel1()}");
            }
            
            // Hide panel first
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            // Select and spawn the hero - this will trigger level up
            OnHeroSelected(heroType);
        }
        
        private void OnUpgradeSelected(string upgradeType)
        {
            Debug.Log($"[UpgradeUI] Upgrade selected: {upgradeType}");
            
            // Get hero's actual level from the world
            int currentHeroLevel = 1;
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(playerHeroId, out Hero hero))
                {
                    currentHeroLevel = hero.Level;
                }
            }
            
            PlayerDataManager dataMgr = PlayerDataManager.Instance;
            bool isSecondUpgrade = false;
            
            if (currentHeroLevel == 1 && dataMgr != null)
            {
                int upgradesChosen = dataMgr.GetUpgradesAtLevel1();
                
                if (upgradesChosen == 0)
                {
                    // This shouldn't happen - stat upgrades shouldn't be chosen in first upgrade
                    Debug.LogWarning("[UpgradeUI] Stat upgrade selected in first upgrade (unexpected)");
                }
                else if (upgradesChosen == 1)
                {
                    // This is the second upgrade - increment and close
                    dataMgr.IncrementUpgradesAtLevel1();
                    isSecondUpgrade = true;
                }
            }
            
            ChooseUpgradeCommand cmd = new ChooseUpgradeCommand
            {
                HeroId = playerHeroId,
                UpgradeType = upgradeType,
                UpgradeTier = 1
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            
            // Close panel and resume game
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            Time.timeScale = 1f;
            Debug.Log($"[UpgradeUI] Upgrade chosen, game resumed ({(isSecondUpgrade ? "second" : "normal")} upgrade)");
        }
        
        public void SetPlayerHero(EntityId heroId)
        {
            playerHeroId = heroId;
            Debug.Log($"[UpgradeUI] Player hero set to ID: {heroId.Value}");
        }
    }
}


