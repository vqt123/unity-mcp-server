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
                    canvasRect.localScale = Vector3.one * 0.01f;
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
                tmp.fontSize = 32;
                tmp.alignment = TextAlignmentOptions.Center;
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
            canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
            
            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
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

