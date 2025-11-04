using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages cooldown UI elements - creates one for each hero
    /// </summary>
    public class CooldownUIManager : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private RectTransform cooldownContainer;
        [SerializeField] private GameObject cooldownPrefab;
        [SerializeField] private float cooldownSize = 60f;
        [SerializeField] private float spacing = 10f;
        
        private Dictionary<EntityId, GameObject> heroCooldowns = new Dictionary<EntityId, GameObject>();
        
        void Start()
        {
            Debug.Log("[CooldownUI] Starting CooldownUIManager");
            
            // Create UI container if needed
            if (cooldownContainer == null)
            {
                CreateCooldownContainer();
            }
            
            // Subscribe to hero spawn events
            EventBus.Subscribe<HeroSpawnedEvent>(OnHeroSpawned);
            
            Debug.Log("[CooldownUI] Subscribed to HeroSpawnedEvent");
        }
        
        void OnDestroy()
        {
            EventBus.Unsubscribe<HeroSpawnedEvent>(OnHeroSpawned);
        }
        
        private void CreateCooldownContainer()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[CooldownUI] No Canvas found!");
                return;
            }
            
            // Create container in bottom middle
            GameObject containerObj = new GameObject("CooldownContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            
            cooldownContainer = containerObj.AddComponent<RectTransform>();
            cooldownContainer.anchorMin = new Vector2(0.5f, 0f);
            cooldownContainer.anchorMax = new Vector2(0.5f, 0f);
            cooldownContainer.pivot = new Vector2(0.5f, 0f);
            cooldownContainer.anchoredPosition = new Vector2(0, 80);
            cooldownContainer.sizeDelta = new Vector2(400, 100);
            
            // Add horizontal layout group
            HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            
            Debug.Log("[CooldownUI] Created cooldown container");
        }
        
        private void OnHeroSpawned(ISimulationEvent evt)
        {
            if (evt is HeroSpawnedEvent spawnEvent)
            {
                Debug.Log($"[CooldownUI] Hero spawned: {spawnEvent.HeroId.Value}, creating cooldown UI");
                CreateCooldownForHero(spawnEvent.HeroId, spawnEvent.HeroType);
            }
        }
        
        private void CreateCooldownForHero(EntityId heroId, string heroType)
        {
            if (heroCooldowns.ContainsKey(heroId))
            {
                Debug.LogWarning($"[CooldownUI] Cooldown UI already exists for hero {heroId.Value}");
                return;
            }
            
            // Create cooldown UI element with vertical layout (icon on top, text below)
            GameObject cooldownObj = new GameObject($"Cooldown_{heroType}_{heroId.Value}");
            cooldownObj.transform.SetParent(cooldownContainer, false);
            
            RectTransform rect = cooldownObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cooldownSize + 20, cooldownSize + 30); // Extra space for text
            
            // Add vertical layout for icon + text
            VerticalLayoutGroup verticalLayout = cooldownObj.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5f;
            verticalLayout.childAlignment = TextAnchor.MiddleCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            
            // Create icon container
            GameObject iconContainer = new GameObject("Icon");
            iconContainer.transform.SetParent(cooldownObj.transform, false);
            RectTransform iconRect = iconContainer.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(cooldownSize, cooldownSize);
            
            // Add background circle
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(iconContainer.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = GetCircleSprite();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add fill image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(iconContainer.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = GetCircleSprite();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = false;
            fillImage.fillAmount = 0f;
            fillImage.color = Color.red;
            
            // Add hero name text
            GameObject textObj = new GameObject("HeroName");
            textObj.transform.SetParent(cooldownObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(cooldownSize + 20, 20);
            
            TMPro.TextMeshProUGUI nameText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            nameText.text = heroType;
            nameText.fontSize = 14;
            nameText.alignment = TMPro.TextAlignmentOptions.Center;
            nameText.color = Color.white;
            
            // Add CooldownRadialUI component to track this specific hero
            CooldownRadialUI cooldownUI = cooldownObj.AddComponent<CooldownRadialUI>();
            cooldownUI.SetHero(heroId, fillImage);
            
            heroCooldowns[heroId] = cooldownObj;
            
            Debug.Log($"[CooldownUI] Created cooldown UI for hero {heroId.Value}");
        }
        
        private Sprite cachedCircleSprite = null;
        
        private Sprite GetCircleSprite()
        {
            // Cache the sprite so we only create it once
            if (cachedCircleSprite != null)
            {
                return cachedCircleSprite;
            }
            
            // Create a simple white circle texture programmatically
            int size = 256;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    // Anti-aliased circle edge
                    float alpha = 1f - Mathf.Clamp01((distance - radius + 1f));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            
            // Create sprite from texture
            cachedCircleSprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect
            );
            
            Debug.Log("[CooldownUI] Created procedural circle sprite");
            return cachedCircleSprite;
        }
    }
}

