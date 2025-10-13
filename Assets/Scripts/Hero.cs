using UnityEngine;

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
    
    public void Initialize(string type)
    {
        heroType = type;
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
        
        Debug.Log($"[Hero] Started {heroType} - bulletPrefab: {(bulletPrefab != null ? bulletPrefab.name : "NULL")}, weapon: {weaponName}");
    }
    
    void Update()
    {
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