using UnityEngine;
using UnityEngine.UI;

public class Hero : MonoBehaviour
{
    [System.NonSerialized]
    public string heroType;
    [System.NonSerialized]
    public float maxHealth = 100f;
    [System.NonSerialized]
    public float currentHealth;
    [System.NonSerialized]
    public GameObject bulletPrefab;
    [System.NonSerialized]
    public string weaponName;
    
    // Weapon stats (loaded from WeaponTypes.json)
    private float weaponDamage;
    private float weaponShootCooldown;
    private float weaponBulletSpeed;
    private Color weaponBulletColor;
    
    private float lastShootTime;
    private Renderer heroRenderer;
    private UnityEngine.UI.Image cooldownRadial;
    private int heroIndex; // 0=left, 1=center, 2=right
    
    public void Initialize(string type, int index)
    {
        heroType = type;
        heroIndex = index;
        HeroData data = ConfigManager.Instance.GetHeroData(type);
        
        if (data != null)
        {
            maxHealth = data.maxHealth;
            weaponName = data.startingWeapon;
            
            // Set hero color
            heroRenderer = GetComponent<Renderer>();
            if (heroRenderer != null)
            {
                heroRenderer.material.color = data.color.ToColor();
            }
            
            // Load weapon stats from WeaponTypes.json
            WeaponData weaponData = ConfigManager.Instance.GetWeaponData(weaponName);
            if (weaponData != null)
            {
                weaponDamage = weaponData.damage;
                weaponShootCooldown = weaponData.shootCooldown;
                weaponBulletSpeed = weaponData.bulletSpeed;
                weaponBulletColor = weaponData.bulletColor.ToColor();
                
                Debug.Log($"[Hero] Initialized {type} with weapon '{weaponName}' - Damage: {weaponDamage}, Speed: {weaponBulletSpeed}, Cooldown: {weaponShootCooldown}s");
            }
            else
            {
                Debug.LogError($"[Hero] Failed to find weapon data for: {weaponName}");
            }
        }
        else
        {
            Debug.LogError($"[Hero] Failed to find hero data for type: {type}");
        }
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        lastShootTime = -weaponShootCooldown; // Can shoot immediately
        
        // Load bullet prefab from Resources folder (non-serialized fields must be loaded in code)
        bulletPrefab = Resources.Load<GameObject>("Bullet");
        
        // Create cooldown UI in Canvas
        CreateCooldownUI();
        
        Debug.Log($"[Hero] Started {heroType} - bulletPrefab: {(bulletPrefab != null ? bulletPrefab.name : "NULL")}, weapon: {weaponName}, cooldownUI: {(cooldownRadial != null ? "Created" : "NULL")}");
    }
    
    void CreateCooldownUI()
    {
        // Find the Canvas
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"[Hero] Canvas not found for {heroType}!");
            return;
        }
        
        // Create a white sprite for the UI
        Texture2D tex = new Texture2D(256, 256);
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float dx = (x - 128f) / 128f;
                float dy = (y - 128f) / 128f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, dist <= 1f ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        Sprite circleSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        
        // Create UI GameObject
        GameObject uiObj = new GameObject($"Cooldown_{heroType}");
        uiObj.transform.SetParent(canvas.transform, false);
        
        // Add Image component
        cooldownRadial = uiObj.AddComponent<Image>();
        cooldownRadial.sprite = circleSprite;
        cooldownRadial.color = weaponBulletColor;
        cooldownRadial.type = Image.Type.Filled;
        cooldownRadial.fillMethod = Image.FillMethod.Radial360;
        cooldownRadial.fillOrigin = (int)Image.Origin360.Top;
        cooldownRadial.fillAmount = 0f;
        
        // Position at bottom of screen
        RectTransform rectTransform = uiObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);
        
        // Count how many heroes exist to determine horizontal position
        int heroCount = GameObject.FindGameObjectsWithTag("Player").Length;
        float spacing = 120f;
        float totalWidth = (heroCount - 1) * spacing;
        float xPos = -totalWidth / 2f + heroIndex * spacing;
        
        // Anchor to bottom-center
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(xPos, 60f);
        
        Debug.Log($"[Hero] Created cooldown UI for {heroType} at position {xPos}, heroCount: {heroCount}, index: {heroIndex}");
    }
    
    void Update()
    {
        // Update cooldown UI
        if (cooldownRadial != null)
        {
            float percent = GetCooldownPercent();
            cooldownRadial.fillAmount = percent;
        }
        
        // Check if can shoot
        if (Time.time - lastShootTime >= weaponShootCooldown)
        {
            ShootAtClosestEnemy();
        }
    }
    
    void ShootAtClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (enemies.Length == 0) return;
        
        // Find closest enemy
        GameObject closest = null;
        float closestDist = Mathf.Infinity;
        
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }
        
        if (closest != null)
        {
            // Shoot bullet toward closest enemy
            Vector3 direction = (closest.transform.position - transform.position).normalized;
            
            if (bulletPrefab != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                
                // Set bullet color based on weapon
                Renderer bulletRenderer = bullet.GetComponent<Renderer>();
                if (bulletRenderer != null)
                {
                    bulletRenderer.material.color = weaponBulletColor;
                }
                
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize(direction, weaponBulletSpeed, weaponDamage, "Enemy", gameObject);
                }
            }
            
            lastShootTime = Time.time;
        }
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Hero health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("Hero died! Game Over");
        // Game over logic
        Time.timeScale = 0;
    }
    
    public float GetCooldownPercent()
    {
        return Mathf.Clamp01((Time.time - lastShootTime) / weaponShootCooldown);
    }
}