using UnityEngine;
using UnityEngine.UI;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Events;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Displays hero XP as a horizontal fill bar
    /// </summary>
    public class XPBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        
        private EntityId heroId;
        private bool trackingHero = false;
        private float lastFillAmount = -1f;
        
        void Start()
        {
            Debug.Log("[XPBar] Starting XP bar UI");
            
            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
            }
            
            // If no image found, create the UI
            if (fillImage == null)
            {
                CreateXPBarUI();
            }
            
            if (fillImage == null)
            {
                Debug.LogError("[XPBar] Failed to create XP bar UI!");
                enabled = false;
                return;
            }
            
            Debug.Log($"[XPBar] Image configured - type: {fillImage.type}, fillMethod: {fillImage.fillMethod}");
            
            // Subscribe to hero spawn events (event-driven, no polling)
            EventBus.Subscribe<HeroSpawnedEvent>(OnHeroSpawned);
            
            // Check if hero already exists (for scene reloads/replay)
            CheckExistingHero();
        }
        
        void OnDestroy()
        {
            EventBus.Unsubscribe<HeroSpawnedEvent>(OnHeroSpawned);
        }
        
        /// <summary>
        /// Event-driven: Hero spawned - start tracking if not already tracking
        /// </summary>
        private void OnHeroSpawned(ISimulationEvent evt)
        {
            if (!trackingHero && evt is HeroSpawnedEvent spawnEvent)
            {
                heroId = spawnEvent.HeroId;
                trackingHero = true;
                Debug.Log($"[XPBar] Now tracking hero {heroId.Value} from spawn event");
            }
        }
        
        /// <summary>
        /// Check if a hero already exists (for scene reloads/replay)
        /// </summary>
        private void CheckExistingHero()
        {
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.HeroIds.Count > 0)
                {
                    heroId = world.HeroIds[0];
                    trackingHero = true;
                    Debug.Log($"[XPBar] Found existing hero {heroId.Value}");
                }
            }
        }
        
        private void CreateXPBarUI()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[XPBar] No Canvas found!");
                return;
            }
            
            // Create container at bottom of screen
            GameObject containerObj = new GameObject("XPBarContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, 0f);
            containerRect.anchorMax = new Vector2(0.9f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0, 50);
            containerRect.sizeDelta = new Vector2(0, 20);
            
            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(containerObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Create a simple sprite for the fill
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
            bgImage.sprite = sprite;
            
            // Create fill
            GameObject fillObj = new GameObject("XPBarFill");
            fillObj.transform.SetParent(containerObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = sprite;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;
            fillImage.color = new Color(0.2f, 0.8f, 1f, 1f); // Light blue/cyan color
            
            Debug.Log("[XPBar] Created XP bar UI");
        }
        
        void Update()
        {
            if (!trackingHero || GameSimulation.Instance == null)
                return;
                
            var world = GameSimulation.Instance.Simulation.World;
            if (world.TryGetHero(heroId, out Hero hero))
            {
                UpdateXPDisplay(hero);
            }
        }
        
        private void UpdateXPDisplay(Hero hero)
        {
            // Calculate XP percentage
            float xpPercent = hero.XPToNextLevel > 0 ? (float)hero.CurrentXP / hero.XPToNextLevel : 0f;
            float newFillAmount = Mathf.Clamp01(xpPercent);
            
            // Log when it changes significantly
            if (Mathf.Abs(newFillAmount - lastFillAmount) > 0.05f)
            {
                Debug.Log($"[XPBar] Level: {hero.Level}, XP: {hero.CurrentXP}/{hero.XPToNextLevel} ({xpPercent:P0}), fillAmount: {newFillAmount:F2}");
                lastFillAmount = newFillAmount;
            }
            
            fillImage.fillAmount = newFillAmount;
        }
    }
}