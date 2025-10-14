using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UpgradeRarity
{
    Common,
    Uncommon,
    Rare
}

[System.Serializable]
public class Upgrade
{
    public UpgradeRarity rarity;
    public string name;
    public string description;
    public float damageBonus;
    
    public Upgrade(UpgradeRarity r, float dmg)
    {
        rarity = r;
        damageBonus = dmg;
        name = $"{r}: +{dmg} Damage";
        description = $"Increases weapon damage by {dmg}";
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
    private List<Upgrade> currentUpgradeChoices = new List<Upgrade>();
    private List<Hero> allHeroes = new List<Hero>();
    
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
        
        // Find all heroes in the scene
        FindAllHeroes();
        
        UpdateScoreUI();
        UpdateXPUI();
    }
    
    void FindAllHeroes()
    {
        // Find all hero objects (tagged as Player)
        GameObject[] heroObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject heroObj in heroObjects)
        {
            Hero hero = heroObj.GetComponent<Hero>();
            if (hero != null)
            {
                allHeroes.Add(hero);
            }
        }
        Debug.Log($"[GameManager] Found {allHeroes.Count} heroes");
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
        ShowUpgradePanel();
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
    
    void ShowUpgradePanel()
    {
        // Find or create upgrade panel
        if (upgradePanel == null)
        {
            upgradePanel = GameObject.Find("UpgradePanel");
        }
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            // Generate 3 random upgrades
            GenerateUpgradeChoices();
        }
        else
        {
            Debug.LogError("[GameManager] UpgradePanel not found!");
        }
    }
    
    void GenerateUpgradeChoices()
    {
        currentUpgradeChoices.Clear();
        
        // Generate 3 random upgrades with different rarities
        // Common: 30% chance, +5 damage
        // Uncommon: 50% chance, +10 damage  
        // Rare: 20% chance, +20 damage
        
        for (int i = 0; i < 3; i++)
        {
            float roll = Random.Range(0f, 1f);
            Upgrade upgrade;
            
            if (roll < 0.3f)
            {
                upgrade = new Upgrade(UpgradeRarity.Common, 5f);
            }
            else if (roll < 0.8f)
            {
                upgrade = new Upgrade(UpgradeRarity.Uncommon, 10f);
            }
            else
            {
                upgrade = new Upgrade(UpgradeRarity.Rare, 20f);
            }
            
            currentUpgradeChoices.Add(upgrade);
        }
        
        // Update button texts to show the generated upgrades
        UpdateUpgradeButtonTexts();
        
        Debug.Log("[GameManager] Generated 3 upgrade choices");
    }
    
    void UpdateUpgradeButtonTexts()
    {
        for (int i = 0; i < 3 && i < currentUpgradeChoices.Count; i++)
        {
            GameObject buttonObj = GameObject.Find($"UpgradeButton{i + 1}");
            if (buttonObj != null)
            {
                TextMeshProUGUI textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    Upgrade upgrade = currentUpgradeChoices[i];
                    textComponent.text = upgrade.name;
                    
                    // Color based on rarity
                    switch (upgrade.rarity)
                    {
                        case UpgradeRarity.Common:
                            textComponent.color = Color.white;
                            break;
                        case UpgradeRarity.Uncommon:
                            textComponent.color = Color.green;
                            break;
                        case UpgradeRarity.Rare:
                            textComponent.color = new Color(1f, 0.5f, 0f); // Orange
                            break;
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
        
        Upgrade selectedUpgrade = currentUpgradeChoices[upgradeIndex];
        
        // Apply damage bonus to ALL heroes
        foreach (Hero hero in allHeroes)
        {
            if (hero != null)
            {
                hero.AddWeaponDamage(selectedUpgrade.damageBonus);
            }
        }
        
        Debug.Log($"[GameManager] Applied {selectedUpgrade.rarity} upgrade: +{selectedUpgrade.damageBonus} damage to all heroes!");
    }
}