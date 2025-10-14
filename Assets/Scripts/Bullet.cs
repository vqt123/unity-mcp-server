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
    public Color bulletColor = Color.white; // For particle effects
    
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
        // Create visual AOE effect
        CreateAoeParticleEffect(impactPoint);
        
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
    
    void CreateAoeParticleEffect(Vector3 position)
    {
        // Create particle system GameObject
        GameObject particleObj = new GameObject("AOE_Effect");
        particleObj.transform.position = position;
        
        // Add ParticleSystem component
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        
        // Stop the system first so we can configure it
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        // Configure main module
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = bulletColor;
        main.maxParticles = 50;
        main.loop = false;
        main.duration = 0.3f;
        
        // Configure emission module (burst)
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30)
        });
        
        // Configure shape module (sphere matching AOE radius)
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = aoeRadius;
        shape.radiusThickness = 1f; // Emit from surface
        
        // Configure renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Start the particle system after configuration
        ps.Play();
        
        // Add glow ring effect at ground level
        CreateAoeRing(position, aoeRadius, bulletColor);
        
        // Auto-destroy after effect finishes
        Destroy(particleObj, 1f);
        
        Debug.Log($"[Bullet] Created AOE particle effect at {position} with radius {aoeRadius}");
    }
    
    void CreateAoeRing(Vector3 position, float radius, Color color)
    {
        // Create a ring to show AOE radius on ground
        GameObject ring = new GameObject("AOE_Ring");
        ring.transform.position = position;
        
        // Create line renderer for the ring
        LineRenderer lr = ring.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, 0f); // Fade out
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.useWorldSpace = false;
        
        // Create circle points
        int segments = 32;
        lr.positionCount = segments + 1;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0.1f, z));
        }
        
        // Animate ring expanding and fading
        StartCoroutine(AnimateRing(ring, radius));
    }
    
    System.Collections.IEnumerator AnimateRing(GameObject ring, float finalRadius)
    {
        LineRenderer lr = ring.GetComponent<LineRenderer>();
        float duration = 0.4f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Expand ring
            float currentRadius = Mathf.Lerp(0f, finalRadius, t);
            
            // Update ring points
            int segments = lr.positionCount - 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * currentRadius;
                float z = Mathf.Sin(angle) * currentRadius;
                lr.SetPosition(i, new Vector3(x, 0.1f, z));
            }
            
            // Fade out
            Color startColor = lr.startColor;
            startColor.a = 1f - t;
            lr.startColor = startColor;
            lr.endColor = startColor;
            
            yield return null;
        }
        
        Destroy(ring);
    }
}