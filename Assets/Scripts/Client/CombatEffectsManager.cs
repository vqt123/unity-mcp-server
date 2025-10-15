using UnityEngine;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;

namespace ArenaGame.Client
{
    /// <summary>
    /// Spawns visual effects for combat events
    /// </summary>
    public class CombatEffectsManager : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        
        [Header("Audio")]
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        
        private AudioSource audioSource;
        
        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<HeroShootEvent>(OnHeroShoot);
            EventBus.Subscribe<HeroDamagedEvent>(OnEntityDamaged);
            EventBus.Subscribe<EnemyDamagedEvent>(OnEntityDamaged);
            EventBus.Subscribe<HeroKilledEvent>(OnEntityKilled);
            EventBus.Subscribe<EnemyKilledEvent>(OnEntityKilled);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<HeroShootEvent>(OnHeroShoot);
            EventBus.Unsubscribe<HeroDamagedEvent>(OnEntityDamaged);
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEntityDamaged);
            EventBus.Unsubscribe<HeroKilledEvent>(OnEntityKilled);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEntityKilled);
        }
        
        private void OnHeroShoot(ISimulationEvent evt)
        {
            if (evt is HeroShootEvent shoot && GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(shoot.HeroId, out var hero))
                {
                    Vector3 pos = ToVector3(hero.Position);
                    
                    // Muzzle flash
                    if (muzzleFlashPrefab != null)
                    {
                        var flash = Instantiate(muzzleFlashPrefab, pos, Quaternion.identity);
                        Destroy(flash, 0.2f);
                    }
                    
                    // Sound
                    if (shootSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(shootSound, 0.3f);
                    }
                }
            }
        }
        
        private void OnEntityDamaged(ISimulationEvent evt)
        {
            Vector3 pos = Vector3.zero;
            bool found = false;
            
            if (evt is HeroDamagedEvent heroDmg && GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(heroDmg.HeroId, out var hero))
                {
                    pos = ToVector3(hero.Position);
                    found = true;
                }
            }
            else if (evt is EnemyDamagedEvent enemyDmg && GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetEnemy(enemyDmg.EnemyId, out var enemy))
                {
                    pos = ToVector3(enemy.Position);
                    found = true;
                }
            }
            
            if (found)
            {
                // Hit effect
                if (hitEffectPrefab != null)
                {
                    var hit = Instantiate(hitEffectPrefab, pos, Quaternion.identity);
                    Destroy(hit, 1f);
                }
                
                // Sound
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound, 0.5f);
                }
            }
        }
        
        private void OnEntityKilled(ISimulationEvent evt)
        {
            // Death effect and sound handled here
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound, 0.7f);
            }
        }
        
        private Vector3 ToVector3(ArenaGame.Shared.Math.FixV2 pos)
        {
            return new Vector3((float)pos.X.ToDouble(), 0f, (float)pos.Y.ToDouble());
        }
    }
}

