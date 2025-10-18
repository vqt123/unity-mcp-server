using UnityEngine;
using UnityEngine.UI;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
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
            
            if (fillImage == null)
            {
                Debug.LogError("[XPBar] No Image component found!");
                enabled = false;
                return;
            }
            
            Debug.Log($"[XPBar] Image configured - type: {fillImage.type}, fillMethod: {fillImage.fillMethod}");
            
            // Try to find hero after a delay
            Invoke(nameof(FindHero), 1f);
        }
        
        private void FindHero()
        {
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                
                if (world.HeroIds.Count > 0)
                {
                    heroId = world.HeroIds[0];
                    trackingHero = true;
                    Debug.Log($"[XPBar] Now tracking hero {heroId.Value}");
                }
                else
                {
                    Debug.LogWarning("[XPBar] No heroes found, retrying in 1 second");
                    Invoke(nameof(FindHero), 1f);
                }
            }
            else
            {
                Debug.LogWarning("[XPBar] GameSimulation not ready, retrying in 1 second");
                Invoke(nameof(FindHero), 1f);
            }
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