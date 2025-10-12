using UnityEngine;

public class Enemy : MonoBehaviour
{
    [System.NonSerialized]
    public float maxHealth = 30f;
    [System.NonSerialized]
    public float currentHealth;
    [System.NonSerialized]
    public float damage = 5f;
    [System.NonSerialized]
    public float moveSpeed = 1.5f;
    [System.NonSerialized]
    public float attackRange = 1.5f;
    [System.NonSerialized]
    public float attackCooldown = 1f;
    [System.NonSerialized]
    public GameObject bloodEffect; // Assign BloodEffect prefab
    
    private Transform hero;
    private float lastAttackTime;
    
    void Start()
    {
        currentHealth = maxHealth;
        GameObject heroObj = GameObject.FindGameObjectWithTag("Player");
        if (heroObj != null)
        {
            hero = heroObj.transform;
        }
        
        // Load blood effect from Resources folder (non-serialized fields must be loaded in code)
        bloodEffect = Resources.Load<GameObject>("BloodEffect");
        
        Debug.Log($"[Enemy] Loaded bloodEffect: {(bloodEffect != null ? bloodEffect.name : "NULL")}");
    }
    
    void Update()
    {
        if (hero == null) return;
        
        // Move toward hero
        float distToHero = Vector3.Distance(transform.position, hero.position);
        
        if (distToHero > attackRange)
        {
            // Move toward hero
            Vector3 direction = (hero.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Attack hero
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackHero();
            }
        }
    }
    
    void AttackHero()
    {
        if (hero == null) return;
        
        Hero heroScript = hero.GetComponent<Hero>();
        if (heroScript != null)
        {
            heroScript.TakeDamage(damage);
            lastAttackTime = Time.time;
        }
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        // Spawn blood effect
        if (bloodEffect != null)
        {
            GameObject blood = Instantiate(bloodEffect, transform.position, Quaternion.identity);
            Destroy(blood, 1f); // Destroy after 1 second
        }
        
        Debug.Log($"[Enemy] Took {amount} damage, health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("[Enemy] Dying!");
        
        // Notify game manager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.EnemyKilled();
        }
        
        Destroy(gameObject);
    }
}