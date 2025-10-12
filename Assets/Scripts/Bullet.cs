using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private string targetTag;
    private float lifetime = 5f;
    
    public void Initialize(Vector3 dir, float spd, float dmg, string tag)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        targetTag = tag;
        
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Deal damage
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
    }
}