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
        
        void Start()
        {
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
                    Debug.Log($"[CooldownUI] Tracking hero {heroId.Value}");
                }
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
            // Calculate cooldown percentage
            int ticksSinceLastShot = world.CurrentTick - hero.LastShotTick;
            float cooldownPercent = Mathf.Clamp01((float)ticksSinceLastShot / hero.ShotCooldownTicks);
            
            // Update fill
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 1f - cooldownPercent; // Drain as cooldown completes
                
                // Color: red when on cooldown, green when ready
                cooldownFill.color = cooldownPercent >= 1f ? Color.green : Color.red;
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
        
        private SimulationWorld world => GameSimulation.Instance.Simulation.World;
    }
}

