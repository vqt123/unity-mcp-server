using UnityEngine;

public class Hero : MonoBehaviour
{
    [System.NonSerialized]
    public float maxHealth = 100f;
    [System.NonSerialized]
    public float currentHealth;
    [System.NonSerialized]
    public float damage = 10f;
    [System.NonSerialized]
    public float shootCooldown = 1f;
    [System.NonSerialized]
    public GameObject bulletPrefab;
    [System.NonSerialized]
    public float bulletSpeed = 5f;
    
    private float lastShootTime;
    
    void Start()
    {
        currentHealth = maxHealth;
        lastShootTime = -shootCooldown; // Can shoot immediately
        
        // Load bullet prefab from Resources folder (non-serialized fields must be loaded in code)
        bulletPrefab = Resources.Load<GameObject>("Bullet");
        
        Debug.Log($"[Hero] Started - bulletPrefab: {(bulletPrefab != null ? bulletPrefab.name : "NULL")}, bulletSpeed: {bulletSpeed}");
    }
    
    void Update()
    {
        // Check if can shoot
        if (Time.time - lastShootTime >= shootCooldown)
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
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize(direction, bulletSpeed, damage, "Enemy", gameObject);
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
        return Mathf.Clamp01((Time.time - lastShootTime) / shootCooldown);
    }
}