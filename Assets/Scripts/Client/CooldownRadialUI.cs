using UnityEngine;
using UnityEngine.UI;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Displays hero ability cooldown as a radial fill indicator
    /// </summary>
    public class CooldownRadialUI : MonoBehaviour
    {
            [SerializeField] private Image fillImage;
        
        private EntityId heroId;
        private bool trackingHero = false;
        private float lastFillAmount = -1f;
        
        void Start()
        {
            Debug.Log("[CooldownRadial] Starting cooldown radial UI");
            
            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
            }
            
            if (fillImage == null)
            {
                Debug.LogError("[CooldownRadial] No Image component found!");
                enabled = false;
                return;
            }
            
            Debug.Log($"[CooldownRadial] Image configured - type: {fillImage.type}, fillMethod: {fillImage.fillMethod}");
            
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
                    Debug.Log($"[CooldownRadial] Now tracking hero {heroId.Value}");
                }
                else
                {
                    Debug.LogWarning("[CooldownRadial] No heroes found, retrying in 1 second");
                    Invoke(nameof(FindHero), 1f);
                }
            }
            else
            {
                Debug.LogWarning("[CooldownRadial] GameSimulation not ready, retrying in 1 second");
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
                UpdateCooldownDisplay(hero, world);
            }
        }
        
        private void UpdateCooldownDisplay(Hero hero, SimulationWorld world)
        {
            // Calculate cooldown percentage
            int ticksSinceLastShot = world.CurrentTick - hero.LastShotTick;
            float cooldownPercent = Mathf.Clamp01((float)ticksSinceLastShot / hero.ShotCooldownTicks);
            
            // Update fill (drain as cooldown completes)
            float newFillAmount = 1f - cooldownPercent;
            
            // Log when it changes significantly
            if (Mathf.Abs(newFillAmount - lastFillAmount) > 0.1f)
            {
                Debug.Log($"[CooldownRadial] fillAmount: {newFillAmount:F2}, cooldownPercent: {cooldownPercent:F2}");
                lastFillAmount = newFillAmount;
            }
            
            fillImage.fillAmount = newFillAmount;
            
            // Color: red when on cooldown, green when ready
            fillImage.color = cooldownPercent >= 1f ? Color.green : Color.red;
        }
    }
}