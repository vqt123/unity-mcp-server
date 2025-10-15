using UnityEngine;
using ArenaGame.Shared.Math;
using EntityId = ArenaGame.Shared.Entities.EntityId;
using Hero = ArenaGame.Shared.Entities.Hero;
using Enemy = ArenaGame.Shared.Entities.Enemy;
using Projectile = ArenaGame.Shared.Entities.Projectile;

namespace ArenaGame.Client
{
    /// <summary>
    /// Component that links a GameObject to a simulation entity
    /// Provides query access to simulation state
    /// </summary>
    public class EntityView : MonoBehaviour
    {
        public EntityId EntityId { get; set; }
        public bool IsHero { get; set; }
        
        public bool TryGetHero(out Hero hero)
        {
            hero = default;
            if (!IsHero || GameSimulation.Instance == null) return false;
            return GameSimulation.Instance.Simulation.World.TryGetHero(EntityId, out hero);
        }
        
        public bool TryGetEnemy(out Enemy enemy)
        {
            enemy = default;
            if (IsHero || GameSimulation.Instance == null) return false;
            return GameSimulation.Instance.Simulation.World.TryGetEnemy(EntityId, out enemy);
        }
        
        public bool TryGetProjectile(out Projectile projectile)
        {
            projectile = default;
            if (GameSimulation.Instance == null) return false;
            return GameSimulation.Instance.Simulation.World.TryGetProjectile(EntityId, out projectile);
        }
        
        public FixV2 GetSimulationPosition()
        {
            if (TryGetHero(out Hero hero)) return hero.Position;
            if (TryGetEnemy(out Enemy enemy)) return enemy.Position;
            if (TryGetProjectile(out Projectile proj)) return proj.Position;
            return FixV2.Zero;
        }
    }
}

