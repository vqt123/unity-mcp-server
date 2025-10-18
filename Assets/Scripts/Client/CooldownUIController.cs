using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Displays hero ability cooldowns
    /// </summary>
    public class CooldownUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image cooldownFill;
        [SerializeField] private TextMeshProUGUI cooldownText;
        
        private EntityId heroId;
        private bool trackingHero = false;
        private float lastFillAmount = -1f;
        
        void Start()
        {
            Debug.Log($"[CooldownUI] Start - cooldownFill: {(cooldownFill != null ? "OK" : "NULL")}, cooldownText: {(cooldownText != null ? "OK" : "NULL")}");
            
            // Try to find hero after a delay
            Invoke(nameof(FindHero), 1f);
        }
        
        private void FindHero()
        {
            Debug.Log($"[CooldownUI] FindHero - GameSimulation exists: {GameSimulation.Instance != null}");
            
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                Debug.Log($"[CooldownUI] World has {world.HeroIds.Count} heroes");
                
                if (world.HeroIds.Count > 0)
                {
                    heroId = world.HeroIds[0];
                    trackingHero = true;
                    Debug.Log($"[CooldownUI] Now tracking hero {heroId.Value}");
                }
                else
                {
                    Debug.LogWarning("[CooldownUI] No heroes found, retrying in 1 second");
                    Invoke(nameof(FindHero), 1f);
                }
            }
            else
            {
                Debug.LogWarning("[CooldownUI] GameSimulation not ready, retrying in 1 second");
                Invoke(nameof(FindHero), 1f);
            }
        }
        
        void Update()
        {
            if (!trackingHero || GameSimulation.Instance == null) return;
            
            var world = GameSimulation.Instance.Simulation.World;
            if (world.TryGetHero(heroId, out Hero hero))
            {
                UpdateCooldownDisplay(hero);
            }
        }
        
        private void UpdateCooldownDisplay(Hero hero)
        {
            var world = GameSimulation.Instance.Simulation.World;
            
            // Calculate cooldown percentage
            int ticksSinceLastShot = world.CurrentTick - hero.LastShotTick;
            float cooldownPercent = Mathf.Clamp01((float)ticksSinceLastShot / hero.ShotCooldownTicks);
            
            // Update fill
            if (cooldownFill != null)
            {
                float newFillAmount = 1f - cooldownPercent; // Drain as cooldown completes
                
                // Log when it changes significantly
                if (Mathf.Abs(newFillAmount - lastFillAmount) > 0.1f)
                {
                    Debug.Log($"[CooldownUI] fillAmount: {newFillAmount:F2}, percent: {cooldownPercent:F2}, visible: {cooldownFill.gameObject.activeInHierarchy}");
                    lastFillAmount = newFillAmount;
                }
                
                cooldownFill.fillAmount = newFillAmount;
                
                // Color: red when on cooldown, green when ready
                cooldownFill.color = cooldownPercent >= 1f ? Color.green : Color.red;
            }
            else if (Time.frameCount % 300 == 0) // Log every ~5 seconds at 60fps
            {
                Debug.LogWarning("[CooldownUI] cooldownFill is NULL - please assign in Inspector!");
            }
            
            // Update text
            if (cooldownText != null)
            {
                if (cooldownPercent >= 1f)
                {
                    cooldownText.text = "READY";
                    cooldownText.color = Color.green;
                }
                else
                {
                    float remainingSeconds = (hero.ShotCooldownTicks - ticksSinceLastShot) / 30f;
                    cooldownText.text = $"{remainingSeconds:F1}s";
                    cooldownText.color = Color.white;
                }
            }
        }
        
        public void SetHero(EntityId id)
        {
            heroId = id;
            trackingHero = true;
        }
    }
}

