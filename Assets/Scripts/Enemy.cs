using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 30f;
    public float currentHealth;
    public float damage = 5f;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    
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
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // Notify game manager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.EnemyKilled();
        }
        
        Destroy(gameObject);
    }
}