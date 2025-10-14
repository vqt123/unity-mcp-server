using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private string targetTag;
    private float lifetime = 5f;
    private Rigidbody rb;
    
    public float aoeRadius = 0f; // Set by Hero for AOE weapons
    public bool piercing = false; // Set by Hero for piercing weapons
    
    public void Initialize(Vector3 dir, float spd, float dmg, string tag, GameObject shooter)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        targetTag = tag;
        
        // Add Rigidbody for physics-based movement
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        // Don't set collisionDetectionMode - let Unity use default
        rb.linearVelocity = direction * speed;
        
        // Ignore collision with shooter
        if (shooter != null)
        {
            Collider shooterCollider = shooter.GetComponent<Collider>();
            Collider bulletCollider = GetComponent<Collider>();
            if (shooterCollider != null && bulletCollider != null)
            {
                Physics.IgnoreCollision(bulletCollider, shooterCollider);
            }
        }
        
        Debug.Log($"[Bullet] Initialized with speed {speed}, velocity {rb.linearVelocity.magnitude}");
        
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Keep constant velocity (no physics drag)
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Bullet] Hit {other.name}, tag: {other.tag}");
        
        if (other.CompareTag(targetTag))
        {
            // Deal damage to hit target
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"[Bullet] Dealing {damage} damage to {other.name}");
                enemy.TakeDamage(damage);
            }
            
            // If AOE, deal damage to nearby enemies
            if (aoeRadius > 0f)
            {
                DealAoeDamage(other.transform.position);
            }
            
            // Destroy bullet unless it's piercing
            if (!piercing)
            {
                Destroy(gameObject);
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Bullet] Collision with {collision.gameObject.name}");
        
        if (collision.gameObject.CompareTag(targetTag))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"[Bullet] Dealing {damage} damage to {collision.gameObject.name} via collision");
                enemy.TakeDamage(damage);
            }
            
            // If AOE, deal damage to nearby enemies
            if (aoeRadius > 0f)
            {
                DealAoeDamage(collision.transform.position);
            }
            
            // Destroy bullet unless it's piercing
            if (!piercing)
            {
                Destroy(gameObject);
            }
        }
    }
    
    void DealAoeDamage(Vector3 impactPoint)
    {
        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, aoeRadius);
        
        Debug.Log($"[Bullet] AOE explosion! Radius: {aoeRadius}, hit {hitColliders.Length} colliders");
        
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag(targetTag))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Full damage in AOE (could reduce by distance if desired)
                    Debug.Log($"[Bullet] AOE damage {damage} to {col.name}");
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}