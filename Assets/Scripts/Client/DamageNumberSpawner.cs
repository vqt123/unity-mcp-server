using UnityEngine;
using TMPro;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;

namespace ArenaGame.Client
{
    /// <summary>
    /// Spawns floating damage numbers when entities take damage
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject damageNumberPrefab;
        
        [Header("Settings")]
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private Color heroDamageColor = Color.red;
        [SerializeField] private Color enemyDamageColor = Color.white;
        
        void OnEnable()
        {
            EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }
        
        private void OnHeroDamaged(ISimulationEvent evt)
        {
            if (evt is HeroDamagedEvent dmg && GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(dmg.HeroId, out var hero))
                {
                    Vector3 pos = new Vector3(
                        (float)hero.Position.X.ToDouble(),
                        0.5f,
                        (float)hero.Position.Y.ToDouble()
                    );
                    SpawnDamageNumber(pos, dmg.Damage.ToInt(), heroDamageColor);
                }
            }
        }
        
        private void OnEnemyDamaged(ISimulationEvent evt)
        {
            if (evt is EnemyDamagedEvent dmg && GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetEnemy(dmg.EnemyId, out var enemy))
                {
                    Vector3 pos = new Vector3(
                        (float)enemy.Position.X.ToDouble(),
                        0.5f,
                        (float)enemy.Position.Y.ToDouble()
                    );
                    SpawnDamageNumber(pos, dmg.Damage.ToInt(), enemyDamageColor);
                }
            }
        }
        
        private void SpawnDamageNumber(Vector3 position, int damage, Color color)
        {
            if (damageNumberPrefab == null) return;
            
            GameObject obj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            
            var tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
                tmp.color = color;
            }
            
            var floater = obj.AddComponent<FloatingNumber>();
            floater.speed = floatSpeed;
            floater.lifetime = lifetime;
        }
        
        private class FloatingNumber : MonoBehaviour
        {
            public float speed = 2f;
            public float lifetime = 1f;
            private float timer = 0f;
            
            void Update()
            {
                timer += Time.deltaTime;
                transform.position += Vector3.up * speed * Time.deltaTime;
                
                if (timer >= lifetime)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

