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
        private Image fillImage;
        private EntityId heroId;
        private bool trackingHero = false;
        
        /// <summary>
        /// Set the hero to track (called by CooldownUIManager)
        /// </summary>
        public void SetHero(EntityId id, Image image)
        {
            heroId = id;
            fillImage = image;
            trackingHero = true;
            Debug.Log($"[CooldownRadial] Tracking hero {heroId.Value}");
        }
        
        void Update()
        {
            if (!trackingHero || GameSimulation.Instance == null || fillImage == null)
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
            fillImage.fillAmount = newFillAmount;
            
            // Color: red when on cooldown, green when ready
            fillImage.color = cooldownPercent >= 1f ? Color.green : Color.red;
        }
    }
}