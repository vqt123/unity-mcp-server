using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UpgradeType
{
    HeroSelection,
    WeaponTier,
    StatUpgrade
}

public enum StatType
{
    Health,
    AttackSpeed,
    MovementSpeed
}

[System.Serializable]
public class UpgradeChoice
{
    public UpgradeType type;
    public string name;
    public string description;
    public string heroType; // For hero selection
    public Hero targetHero; // For weapon/stat upgrade
    public StatType statType; // For stat upgrade
    public float statValue; // For stat upgrade
    
    // Hero selection constructor
    public UpgradeChoice(string heroTypeName)
    {
        type = UpgradeType.HeroSelection;
        heroType = heroTypeName;
        name = $"Select {heroTypeName}";
        description = $"Add {heroTypeName} to your party";
    }
    
    // Weapon tier constructor
    public UpgradeChoice(Hero hero, int nextTier)
    {
        type = UpgradeType.WeaponTier;
        targetHero = hero;
        WeaponTierData tierData = ConfigManager.Instance.GetWeaponTier(hero.weaponName, nextTier);
        name = tierData != null ? tierData.name : $"{hero.weaponName} Tier {nextTier}";
        description = tierData != null ? tierData.description : "Upgrade weapon";
    }
    
    // Stat upgrade constructor
    public UpgradeChoice(Hero hero, StatType stat, float value)
    {
        type = UpgradeType.StatUpgrade;
        targetHero = hero;
        statType = stat;
        statValue = value;
        
        string heroName = hero != null ? hero.heroType : "All Heroes";
        
        switch (stat)
        {
            case StatType.Health:
                name = $"+{value} Max Health";
                description = $"Increase {heroName}'s max health";
                break;
            case StatType.AttackSpeed:
                name = $"+{value * 100}% Attack Speed";
                description = $"Faster attacks for {heroName}";
                break;
            case StatType.MovementSpeed:
                name = $"+{value * 100}% Move Speed";
                description = $"Faster movement for {heroName}";
                break;
        }
    }
}

public class GameManager : MonoBehaviour
{
    public int killScore = 0;
    public TextMeshProUGUI scoreText;
    
    // XP System
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpPerLevel = 100; // XP needed for level 1->2
    public float xpScaling = 1.2f; // XP requirement increases by 20% per level
    public int xpPerKill = 10;
    
    // UI References
    private Image xpBarFill;
    private TextMeshProUGUI xpText;
    private GameObject upgradePanel;
    
    // Upgrade System
    private List<UpgradeChoice> currentUpgradeChoices = new List<UpgradeChoice>();
    private List<Hero> activeHeroes = new List<Hero>(); // Heroes currently in battle
    private List<string> availableHeroTypes = new List<string>(); // Hero types not yet selected
    private bool battleStarted = false;
    
    void Start()
    {
        Debug.Log("[GameManager] Starting...");
        
        if (scoreText == null)
        {
            Debug.LogError("[GameManager] scoreText is NULL! Assign it in inspector");
        }
        else
        {
            Debug.Log($"[GameManager] scoreText assigned: {scoreText.gameObject.name}");
        }
        
        // Find XP UI elements
        GameObject xpBarFillObj = GameObject.Find("XPBarFill");
        if (xpBarFillObj != null)
        {
            xpBarFill = xpBarFillObj.GetComponent<Image>();
            Debug.Log("[GameManager] Found XPBarFill");
        }
        else
        {
            Debug.LogError("[GameManager] XPBarFill not found!");
        }
        
        GameObject xpTextObj = GameObject.Find("XPText");
        if (xpTextObj != null)
        {
            xpText = xpTextObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("[GameManager] Found XPText");
        }
        else
        {
            Debug.LogError("[GameManager] XPText not found!");
        }
        
        // Find upgrade panel and setup buttons while it's active
        upgradePanel = GameObject.Find("UpgradePanel");
        if (upgradePanel != null)
        {
            // Setup button click handlers while panel is active
            SetupUpgradeButtons();
            
            // Now hide it
            upgradePanel.SetActive(false);
            Debug.Log("[GameManager] Found UpgradePanel and set up buttons");
        }
        
        // Initialize available hero types from config
        List<HeroData> allHeroData = ConfigManager.Instance.GetAllHeroes();
        foreach (HeroData heroData in allHeroData)
        {
            availableHeroTypes.Add(heroData.type);
        }
        Debug.Log($"[GameManager] Initialized with {availableHeroTypes.Count} available hero types");
        
        UpdateScoreUI();
        UpdateXPUI();
        
        // Show battle start hero selection
        ShowBattleStartSelection();
    }
    
    void ShowBattleStartSelection()
    {
        // Pause game
        Time.timeScale = 0f;
        
        // Show title and 3 hero choices
        ShowUpgradePanel("Choose Your Hero!", true);
        
        Debug.Log("[GameManager] Showing battle start hero selection");
    }
    
    void SetupUpgradeButtons()
    {
        // Find and setup button click handlers
        for (int i = 1; i <= 3; i++)
        {
            GameObject buttonObj = GameObject.Find($"UpgradeButton{i}");
            if (buttonObj != null)
            {
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    int upgradeIndex = i - 1; // Capture index for closure
                    button.onClick.AddListener(() => SelectUpgrade(upgradeIndex));
                    Debug.Log($"[GameManager] Setup click handler for UpgradeButton{i}");
                }
            }
        }
    }
    
    void Update()
    {
        // Each hero now manages its own cooldown UI
    }
    
    public void EnemyKilled()
    {
        killScore++;
        UpdateScoreUI();
        
        // Award XP
        AddXP(xpPerKill);
        
        Debug.Log($"[GameManager] Enemy killed! Score: {killScore}, XP: {currentXP}/{GetXPForNextLevel()}");
    }
    
    void AddXP(int amount)
    {
        currentXP += amount;
        
        // Check for level up
        int xpNeeded = GetXPForNextLevel();
        if (currentXP >= xpNeeded)
        {
            LevelUp();
        }
        
        UpdateXPUI();
    }
    
    int GetXPForNextLevel()
    {
        // Calculate XP needed for current level (scales exponentially)
        return Mathf.RoundToInt(xpPerLevel * Mathf.Pow(xpScaling, currentLevel - 1));
    }
    
    void LevelUp()
    {
        currentLevel++;
        currentXP = 0; // Reset XP for new level
        
        Debug.Log($"[GameManager] LEVEL UP! Now level {currentLevel}");
        
        // Pause game and show upgrade choices
        Time.timeScale = 0f;
        ShowUpgradePanel("LEVEL UP!", false);
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Kills: {killScore}";
        }
    }
    
    void UpdateXPUI()
    {
        int xpNeeded = GetXPForNextLevel();
        float fillPercent = (float)currentXP / xpNeeded;
        
        if (xpBarFill != null)
        {
            // Ensure the Image is set to Filled type
            if (xpBarFill.type != Image.Type.Filled)
            {
                xpBarFill.type = Image.Type.Filled;
                xpBarFill.fillMethod = Image.FillMethod.Horizontal;
                xpBarFill.fillOrigin = 0;
                Debug.Log("[GameManager] Set XPBarFill to Horizontal Filled type");
            }
            
            xpBarFill.fillAmount = fillPercent;
        }
        
        if (xpText != null)
        {
            xpText.text = $"Level {currentLevel} - {currentXP}/{xpNeeded} XP";
        }
    }
    
    void ShowUpgradePanel(string title, bool isBattleStart)
    {
        // Find or create upgrade panel
        if (upgradePanel == null)
        {
            upgradePanel = GameObject.Find("UpgradePanel");
        }
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            // Update title
            GameObject titleObj = GameObject.Find("UpgradeTitle");
            if (titleObj != null)
            {
                TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = title;
                }
            }
            
            // Generate upgrades
            GenerateUpgradeChoices(isBattleStart);
        }
        else
        {
            Debug.LogError("[GameManager] UpgradePanel not found!");
        }
    }
    
    void GenerateUpgradeChoices(bool isBattleStart)
    {
        currentUpgradeChoices.Clear();
        
        if (isBattleStart)
        {
            // Battle start: Show 3 random hero choices
            List<string> shuffledHeroes = new List<string>(availableHeroTypes);
            ShuffleList(shuffledHeroes);
            
            for (int i = 0; i < 3 && i < shuffledHeroes.Count; i++)
            {
                currentUpgradeChoices.Add(new UpgradeChoice(shuffledHeroes[i]));
            }
            
            Debug.Log("[GameManager] Generated 3 hero choices for battle start");
        }
        else
        {
            // Level up: Generate 3 random upgrade choices
            // Create pool of all possible upgrades
            List<UpgradeChoice> upgradePool = new List<UpgradeChoice>();
            
            // Add hero selections (if available)
            foreach (string heroType in availableHeroTypes)
            {
                upgradePool.Add(new UpgradeChoice(heroType));
            }
            
            // Add weapon tier upgrades for each hero who can upgrade
            foreach (Hero hero in activeHeroes)
            {
                if (hero != null && hero.CanUpgradeWeapon())
                {
                    upgradePool.Add(new UpgradeChoice(hero, hero.GetWeaponTier() + 1));
                }
            }
            
            // Add stat upgrades for each active hero
            foreach (Hero hero in activeHeroes)
            {
                if (hero != null)
                {
                    upgradePool.Add(new UpgradeChoice(hero, StatType.Health, 20f));
                    upgradePool.Add(new UpgradeChoice(hero, StatType.AttackSpeed, 0.15f)); // 15% faster
                }
            }
            
            // Shuffle and pick 3 random upgrades
            ShuffleList(upgradePool);
            
            for (int i = 0; i < 3 && i < upgradePool.Count; i++)
            {
                currentUpgradeChoices.Add(upgradePool[i]);
            }
            
            // If we don't have 3 upgrades, fill with stat upgrades
            while (currentUpgradeChoices.Count < 3 && activeHeroes.Count > 0)
            {
                Hero randomHero = activeHeroes[Random.Range(0, activeHeroes.Count)];
                currentUpgradeChoices.Add(new UpgradeChoice(randomHero, StatType.Health, 20f));
            }
            
            Debug.Log($"[GameManager] Generated {currentUpgradeChoices.Count} upgrade choices");
        }
        
        // Update button texts to show the generated upgrades
        UpdateUpgradeButtonTexts();
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    
    void UpdateUpgradeButtonTexts()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObj = GameObject.Find($"UpgradeButton{i + 1}");
            if (buttonObj != null)
            {
                TextMeshProUGUI textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    if (i < currentUpgradeChoices.Count)
                    {
                        UpgradeChoice choice = currentUpgradeChoices[i];
                        textComponent.text = choice.name + "\n" + choice.description;
                        
                        // Color based on type
                        if (choice.type == UpgradeType.HeroSelection)
                        {
                            textComponent.color = Color.cyan; // Cyan for hero selection
                        }
                        else if (choice.type == UpgradeType.WeaponTier)
                        {
                            textComponent.color = Color.yellow; // Yellow for weapon upgrades
                        }
                        else // StatUpgrade
                        {
                            textComponent.color = Color.green; // Green for stat upgrades
                        }
                        
                        buttonObj.SetActive(true);
                    }
                    else
                    {
                        // Hide unused buttons
                        buttonObj.SetActive(false);
                    }
                }
            }
        }
    }
    
    public void SelectUpgrade(int upgradeIndex)
    {
        Debug.Log($"[GameManager] Selected upgrade {upgradeIndex}");
        
        // Apply the upgrade (will implement)
        ApplyUpgrade(upgradeIndex);
        
        // Close panel and resume game
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }
    
    void ApplyUpgrade(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= currentUpgradeChoices.Count)
        {
            Debug.LogError($"[GameManager] Invalid upgrade index: {upgradeIndex}");
            return;
        }
        
        UpgradeChoice selectedChoice = currentUpgradeChoices[upgradeIndex];
        
        if (selectedChoice.type == UpgradeType.HeroSelection)
        {
            // Spawn new hero
            SpawnHero(selectedChoice.heroType);
            
            // Remove from available types
            availableHeroTypes.Remove(selectedChoice.heroType);
            
            Debug.Log($"[GameManager] Selected hero: {selectedChoice.heroType}. Remaining heroes: {availableHeroTypes.Count}");
            
            // Start battle if this was battle start selection
            if (!battleStarted)
            {
                battleStarted = true;
                Debug.Log("[GameManager] Battle started!");
            }
        }
        else if (selectedChoice.type == UpgradeType.WeaponTier)
        {
            // Upgrade weapon tier
            if (selectedChoice.targetHero != null)
            {
                selectedChoice.targetHero.UpgradeWeaponTier();
                Debug.Log($"[GameManager] Upgraded {selectedChoice.targetHero.heroType} weapon to tier {selectedChoice.targetHero.GetWeaponTier()}");
            }
        }
        else if (selectedChoice.type == UpgradeType.StatUpgrade)
        {
            // Apply stat upgrade
            if (selectedChoice.targetHero != null)
            {
                selectedChoice.targetHero.ApplyStatUpgrade(selectedChoice.statType, selectedChoice.statValue);
                Debug.Log($"[GameManager] Applied {selectedChoice.statType} upgrade to {selectedChoice.targetHero.heroType}");
            }
        }
    }
    
    void SpawnHero(string heroType)
    {
        // Find spawn position (spread heroes out)
        int heroCount = activeHeroes.Count;
        float spacing = 2f;
        Vector3 spawnPos = new Vector3((heroCount - 1) * spacing, 0.5f, 0);
        
        // Create hero cube
        GameObject heroCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        heroCube.name = heroType;
        heroCube.tag = "Player";
        heroCube.transform.position = spawnPos;
        
        // Add Hero component and initialize
        Hero hero = heroCube.AddComponent<Hero>();
        hero.Initialize(heroType, heroCount);
        
        // Add to active heroes
        activeHeroes.Add(hero);
        
        Debug.Log($"[GameManager] Spawned {heroType} at position {spawnPos}");
    }
}