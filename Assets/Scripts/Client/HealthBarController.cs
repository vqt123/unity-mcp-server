using UnityEngine;
using UnityEngine.UI;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;

namespace ArenaGame.Client
{
    /// <summary>
    /// Updates health bar based on simulation events
    /// </summary>
    public class HealthBarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Canvas healthBarCanvas;
        
        private EntityView entityView;
        private Camera mainCamera;
        
        void Start()
        {
            mainCamera = Camera.main;
            entityView = GetComponent<EntityView>();
            
            if (entityView == null)
            {
                Debug.LogWarning($"[HealthBar] No EntityView found on {gameObject.name}");
                enabled = false;
                return;
            }
            
            // Subscribe to damage events
            if (entityView.IsHero)
            {
                EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
            }
            else
            {
                EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            }
        }
        
        void OnDestroy()
        {
            if (entityView != null)
            {
                if (entityView.IsHero)
                {
                    EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
                }
                else
                {
                    EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
                }
            }
        }
        
        void LateUpdate()
        {
            // Update health bar
            UpdateHealthBar();
            
            // Billboard effect
            if (healthBarCanvas != null && mainCamera != null)
            {
                healthBarCanvas.transform.LookAt(mainCamera.transform);
                healthBarCanvas.transform.Rotate(0, 180, 0);
            }
        }
        
        private void OnHeroDamaged(ISimulationEvent evt)
        {
            if (evt is HeroDamagedEvent heroDmg && entityView != null)
            {
                if (heroDmg.HeroId == entityView.EntityId)
                {
                    UpdateHealthBar();
                }
            }
        }
        
        private void OnEnemyDamaged(ISimulationEvent evt)
        {
            if (evt is EnemyDamagedEvent enemyDmg && entityView != null)
            {
                if (enemyDmg.EnemyId == entityView.EntityId)
                {
                    UpdateHealthBar();
                }
            }
        }
        
        private void UpdateHealthBar()
        {
            if (entityView == null || healthBarFill == null) return;
            
            float healthPercent = 1f;
            
            if (entityView.IsHero && entityView.TryGetHero(out var hero))
            {
                healthPercent = (float)(hero.Health / hero.MaxHealth).ToDouble();
            }
            else if (!entityView.IsHero && entityView.TryGetEnemy(out var enemy))
            {
                healthPercent = (float)(enemy.Health / enemy.MaxHealth).ToDouble();
            }
            
            healthBarFill.fillAmount = Mathf.Clamp01(healthPercent);
        }
    }
}

