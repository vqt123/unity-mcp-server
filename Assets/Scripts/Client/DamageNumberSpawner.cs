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
        [SerializeField] private float floatSpeed = 3f;
        [SerializeField] private float lifetime = 1.5f;
        [SerializeField] private float fontSize = 64f; // Much bigger damage numbers
        [SerializeField] private Color heroDamageColor = Color.red;
        [SerializeField] private Color enemyDamageColor = Color.white;
        
        void Start()
        {
            // Ensure we subscribe - unsubscribe first to avoid duplicates
            EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            Debug.Log("[DamageNumberSpawner] Subscribed to damage events in Start");
        }
        
        void OnEnable()
        {
            // Subscribe when enabled
            EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            Debug.Log("[DamageNumberSpawner] Subscribed to damage events in OnEnable");
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
            if (evt is EnemyDamagedEvent dmg)
            {
                Vector3 pos = Vector3.zero;
                
                // Priority 1: Use EntityVisualizer position (most reliable - uses visual GameObject position)
                var visualizer = FindFirstObjectByType<EntityVisualizer>();
                if (visualizer != null)
                {
                    Vector3 visualPos = visualizer.GetEntityPosition(dmg.EnemyId);
                    if (visualPos != Vector3.zero)
                    {
                        pos = new Vector3(visualPos.x, 1.0f, visualPos.z);
                    }
                }
                
                // Priority 2: Use position from event (stored when damage is dealt)
                if (pos == Vector3.zero)
                {
                    pos = new Vector3(
                        (float)dmg.EnemyPosition.X.ToDouble(),
                        1.0f, // Higher up so it's more visible
                        (float)dmg.EnemyPosition.Y.ToDouble()
                    );
                }
                
                // Priority 3: Try simulation world as final fallback
                if (pos == Vector3.zero && GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetEnemy(dmg.EnemyId, out var enemy))
                {
                        pos = new Vector3(
                        (float)enemy.Position.X.ToDouble(),
                            1.0f,
                        (float)enemy.Position.Y.ToDouble()
                    );
                    }
                }
                
                // If we still don't have a position, log warning but don't spawn at origin
                if (pos == Vector3.zero)
                {
                    Debug.LogWarning($"[DamageNumberSpawner] Could not find position for enemy {dmg.EnemyId} - skipping damage number");
                    return;
                }
                
                // Debug log to verify position
                Debug.Log($"[DamageNumberSpawner] Spawning damage {dmg.Damage.ToInt()} at enemy position: {pos} (enemy {dmg.EnemyId}, attacker {dmg.AttackerId})");
                
                // ALWAYS spawn damage number at enemy position
                SpawnDamageNumber(pos, dmg.Damage.ToInt(), enemyDamageColor);
                }
            else
            {
                Debug.LogWarning($"[DamageNumberSpawner] Event is not EnemyDamagedEvent: {evt?.GetType().Name}");
            }
        }
        
        private void SpawnDamageNumber(Vector3 position, int damage, Color color)
        {
            GameObject obj = null;
            
            if (damageNumberPrefab != null)
            {
                obj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create damage text dynamically if prefab not assigned
                obj = CreateDamageTextObject();
                obj.transform.position = position;
            }
            
            var tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
                tmp.color = color;
                tmp.fontSize = fontSize; // Ensure fontSize is applied
                tmp.fontStyle = FontStyles.Bold; // Make it bold
            }
            else
            {
                // Add TextMeshPro if not found
                GameObject canvasObj = obj.transform.Find("Canvas")?.gameObject;
                if (canvasObj == null)
                {
                    canvasObj = new GameObject("Canvas");
                    canvasObj.transform.SetParent(obj.transform, false);
                    Canvas canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = Camera.main;
                    RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
                    canvasRect.sizeDelta = new Vector2(1f, 1f);
                    canvasRect.localScale = Vector3.one * 0.02f; // 2x bigger for visibility
                }
                
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(canvasObj.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                
                tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = damage.ToString();
                tmp.color = color;
                tmp.fontSize = fontSize; // Use configurable fontSize
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold; // Make it bold
            }
            
            var floater = obj.GetComponent<FloatingNumber>();
            if (floater == null)
            {
                floater = obj.AddComponent<FloatingNumber>();
            }
            floater.speed = floatSpeed;
            floater.lifetime = lifetime;
        }
        
        private GameObject CreateDamageTextObject()
        {
            GameObject obj = new GameObject("DamageText");
            
            // Create canvas for world space text
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(obj.transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1f, 1f);
            // Make bigger - reduce scale less so text appears larger
            canvasRect.localScale = Vector3.one * 0.02f; // 2x bigger than before
            
            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize; // Use configurable fontSize
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold; // Make it bold
            
            return obj;
        }
        
        private class FloatingNumber : MonoBehaviour
        {
            public float speed = 2f;
            public float lifetime = 1f;
            private float timer = 0f;
            private TextMeshProUGUI text;
            
            void Start()
            {
                text = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            void Update()
            {
                timer += Time.deltaTime;
                transform.position += Vector3.up * speed * Time.deltaTime;
                
                // Fade out over time
                if (text != null)
                {
                    float alpha = 1f - (timer / lifetime);
                    Color color = text.color;
                    color.a = alpha;
                    text.color = color;
                }
                
                if (timer >= lifetime)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

