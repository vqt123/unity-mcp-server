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
    private int weaponTier = 1; // Current weapon tier
    private int weaponProjectileCount = 1;
    private float weaponAoeRadius = 0f;
    private bool weaponPiercing = false;
    
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
            
            // Load weapon tier 1 stats
            LoadWeaponTier(1);
        }
        else
        {
            Debug.LogError($"[Hero] Failed to find hero data for type: {type}");
        }
    }
    
    void LoadWeaponTier(int tier)
    {
        weaponTier = tier;
        WeaponData weaponData = ConfigManager.Instance.GetWeaponData(weaponName);
        WeaponTierData tierData = ConfigManager.Instance.GetWeaponTier(weaponName, tier);
        
        if (weaponData != null && tierData != null)
        {
            weaponDamage = tierData.damage;
            weaponShootCooldown = tierData.shootCooldown;
            weaponBulletSpeed = tierData.bulletSpeed;
            weaponBulletColor = weaponData.bulletColor.ToColor();
            weaponProjectileCount = tierData.projectileCount;
            weaponAoeRadius = tierData.aoeRadius;
            weaponPiercing = tierData.piercing;
            
            Debug.Log($"[Hero] {heroType} equipped {tierData.name} (Tier {tier}): {weaponDamage} dmg, {weaponProjectileCount}x projectiles, AOE: {weaponAoeRadius}");
        }
        else
        {
            Debug.LogError($"[Hero] Failed to load weapon tier: {weaponName} tier {tier}");
        }
    }
    
    public void UpgradeWeaponTier()
    {
        if (weaponTier < 4)
        {
            LoadWeaponTier(weaponTier + 1);
        }
        else
        {
            Debug.LogWarning($"[Hero] {heroType} weapon already at max tier!");
        }
    }
    
    public int GetWeaponTier()
    {
        return weaponTier;
    }
    
    public bool CanUpgradeWeapon()
    {
        return weaponTier < 4;
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
            // Check if this is a melee weapon (Sword)
            if (weaponName == "Sword")
            {
                // Melee attack - instant cleave damage in front of hero
                PerformMeleeAttack();
            }
            else
            {
                // Ranged attack - shoot projectiles
                Vector3 baseDirection = (closest.transform.position - transform.position).normalized;
                
                // Fire multiple projectiles based on weapon tier
                for (int i = 0; i < weaponProjectileCount; i++)
                {
                    // Calculate spread angle for multiple projectiles
                    float spreadAngle = 0f;
                    if (weaponProjectileCount > 1)
                    {
                        float totalSpread = 30f; // Total spread angle in degrees
                        float angleStep = totalSpread / (weaponProjectileCount - 1);
                        spreadAngle = -totalSpread / 2f + (angleStep * i);
                    }
                    
                    // Rotate direction by spread angle (around Y axis)
                    Quaternion rotation = Quaternion.Euler(0, spreadAngle, 0);
                    Vector3 direction = rotation * baseDirection;
                    
                    // Slight delay between projectiles for visual effect
                    float delay = i * 0.05f;
                    StartCoroutine(FireProjectile(direction, delay));
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
    
    void PerformMeleeAttack()
    {
        // Melee attack - deal damage in cone in front of hero
        Vector3 attackPosition = transform.position + transform.forward * (weaponAoeRadius * 0.5f);
        
        // Multiple swings for higher tiers
        for (int i = 0; i < weaponProjectileCount; i++)
        {
            float delay = i * 0.15f; // Slightly longer delay between swings
            StartCoroutine(PerformMeleeSwing(attackPosition, delay, i));
        }
    }
    
    System.Collections.IEnumerator PerformMeleeSwing(Vector3 attackPosition, float delay, int swingIndex)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        
        // Slight variation in swing position for multiple swings
        Vector3 swingOffset = Vector3.zero;
        if (weaponProjectileCount > 1)
        {
            // Alternate left/right for combo attacks
            float sideOffset = (swingIndex % 2 == 0) ? -0.5f : 0.5f;
            swingOffset = transform.right * sideOffset;
        }
        
        Vector3 finalPosition = attackPosition + swingOffset;
        
        // Find all enemies in melee range
        Collider[] hitColliders = Physics.OverlapSphere(finalPosition, weaponAoeRadius);
        
        int enemiesHit = 0;
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(weaponDamage);
                    enemiesHit++;
                }
            }
        }
        
        // Create visual slash effect
        CreateMeleeSlashEffect(finalPosition, weaponAoeRadius, weaponBulletColor);
        
        Debug.Log($"[Hero] {heroType} melee swing {swingIndex + 1} hit {enemiesHit} enemies");
    }
    
    void CreateMeleeSlashEffect(Vector3 position, float radius, Color color)
    {
        // Create particle burst for slash
        GameObject particleObj = new GameObject("Slash_Effect");
        particleObj.transform.position = position;
        
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 5f;
        main.startSize = 0.4f;
        main.startColor = color;
        main.maxParticles = 20;
        main.loop = false;
        main.duration = 0.2f;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 20)
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.1f;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        ps.Play();
        
        // Create ground ring
        CreateSlashRing(position, radius, color);
        
        Destroy(particleObj, 0.5f);
    }
    
    void CreateSlashRing(Vector3 position, float radius, Color color)
    {
        GameObject ring = new GameObject("Slash_Ring");
        ring.transform.position = position;
        
        LineRenderer lr = ring.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, 0f);
        lr.startWidth = 0.3f;
        lr.endWidth = 0.3f;
        lr.useWorldSpace = false;
        
        int segments = 32;
        lr.positionCount = segments + 1;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0.1f, z));
        }
        
        StartCoroutine(AnimateSlashRing(ring, radius));
    }
    
    System.Collections.IEnumerator AnimateSlashRing(GameObject ring, float finalRadius)
    {
        LineRenderer lr = ring.GetComponent<LineRenderer>();
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float currentRadius = finalRadius * (1f + t * 0.2f); // Slight expansion
            
            int segments = lr.positionCount - 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * currentRadius;
                float z = Mathf.Sin(angle) * currentRadius;
                lr.SetPosition(i, new Vector3(x, 0.1f, z));
            }
            
            Color startColor = lr.startColor;
            startColor.a = 1f - t;
            lr.startColor = startColor;
            lr.endColor = startColor;
            
            yield return null;
        }
        
        Destroy(ring);
    }
    
    System.Collections.IEnumerator FireProjectile(Vector3 direction, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        
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
                bulletScript.aoeRadius = weaponAoeRadius;
                bulletScript.piercing = weaponPiercing;
                bulletScript.bulletColor = weaponBulletColor; // For particle effects
            }
        }
    }
    
    public float GetCooldownPercent()
    {
        return Mathf.Clamp01((Time.time - lastShootTime) / weaponShootCooldown);
    }
    
    public void AddWeaponDamage(float bonus)
    {
        weaponDamage += bonus;
        Debug.Log($"[Hero] {heroType} weapon damage increased by {bonus}! New damage: {weaponDamage}");
    }
    
    public string GetWeaponTierName()
    {
        WeaponTierData tierData = ConfigManager.Instance.GetWeaponTier(weaponName, weaponTier);
        return tierData?.name ?? weaponName;
    }
    
    public void ApplyStatUpgrade(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.Health:
                maxHealth += value;
                currentHealth += value; // Also heal by the bonus amount
                Debug.Log($"[Hero] {heroType} max health increased by {value}! New max: {maxHealth}");
                break;
                
            case StatType.AttackSpeed:
                weaponShootCooldown *= (1f - value); // Reduce cooldown = faster attacks
                Debug.Log($"[Hero] {heroType} attack speed increased by {value * 100}%! New cooldown: {weaponShootCooldown}s");
                break;
                
            case StatType.MovementSpeed:
                // Movement speed not implemented yet, but we can store it for future
                Debug.Log($"[Hero] {heroType} movement speed increased by {value * 100}%! (Not yet implemented)");
                break;
        }
    }
}