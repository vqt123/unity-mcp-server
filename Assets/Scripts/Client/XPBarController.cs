using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Displays hero XP bar and level
    /// </summary>
    public class XPBarController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI xpText;
        
        private EntityId heroId;
        private bool trackingHero = false;
        
        void Start()
        {
            Debug.Log($"[XPBar] Start - xpBarFill: {(xpBarFill != null ? "OK" : "NULL")}, xpText: {(xpText != null ? "OK" : "NULL")}");
            
            // Try to find hero after a delay
            Invoke(nameof(FindHero), 1f);
        }
        
        private void FindHero()
        {
            Debug.Log($"[XPBar] FindHero - GameSimulation exists: {GameSimulation.Instance != null}");
            
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                Debug.Log($"[XPBar] World has {world.HeroIds.Count} heroes");
                
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
            if (!trackingHero || GameSimulation.Instance == null) return;
            
            var world = GameSimulation.Instance.Simulation.World;
            if (world.TryGetHero(heroId, out Hero hero))
            {
                UpdateXPDisplay(hero);
            }
        }
        
        private float lastXPPercent = -1f;
        
        private void UpdateXPDisplay(Hero hero)
        {
            // Calculate XP percentage
            float xpPercent = hero.XPToNextLevel > 0 ? (float)hero.CurrentXP / hero.XPToNextLevel : 0f;
            
            bool xpChanged = Mathf.Abs(xpPercent - lastXPPercent) > 0.01f;
            
            // Log when XP changes
            if (xpChanged)
            {
                Debug.Log($"[XPBar] XP CHANGED! Level: {hero.Level}, XP: {hero.CurrentXP}/{hero.XPToNextLevel} ({xpPercent:P0})");
            }
            
            // Log periodically
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"[XPBar] Level: {hero.Level}, XP: {hero.CurrentXP}/{hero.XPToNextLevel} ({xpPercent:P0})");
            }
            
            // Update fill
            if (xpBarFill != null)
            {
                xpBarFill.fillAmount = Mathf.Clamp01(xpPercent);
                
                // Log when we actually set it
                if (xpChanged)
                {
                    Debug.Log($"[XPBar] Set fillAmount to {xpBarFill.fillAmount:F2} (visible: {xpBarFill.gameObject.activeInHierarchy})");
                }
            }
            else if (Time.frameCount % 300 == 0)
            {
                Debug.LogWarning("[XPBar] xpBarFill is NULL - please assign in Inspector!");
            }
            
            // Update text
            if (xpText != null)
            {
                xpText.text = $"Level {hero.Level} - {hero.CurrentXP}/{hero.XPToNextLevel} XP";
            }
            
            // Update lastXPPercent AFTER all checks
            if (xpChanged)
            {
                lastXPPercent = xpPercent;
            }
        }
        
        public void SetHero(EntityId id)
        {
            heroId = id;
            trackingHero = true;
        }
    }
}

