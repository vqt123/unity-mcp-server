using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Sets up a game scene with all necessary managers
    /// Attach this to a GameObject in your scene to auto-setup
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject damageNumberPrefab;
        
        [Header("Settings")]
        [SerializeField] private bool autoSetup = true;
        
        void Awake()
        {
            if (!autoSetup) return;
            
            SetupManagers();
        }
        
        [ContextMenu("Setup Managers")]
        public void SetupManagers()
        {
            // GameSimulation
            if (FindFirstObjectByType<GameSimulation>() == null)
            {
                GameObject simObj = new GameObject("GameSimulation");
                simObj.AddComponent<GameSimulation>();
                Debug.Log("[Setup] Created GameSimulation");
            }
            
            // EntityVisualizer
            if (FindFirstObjectByType<EntityVisualizer>() == null)
            {
                GameObject vizObj = new GameObject("EntityVisualizer");
                var viz = vizObj.AddComponent<EntityVisualizer>();
                
                // Assign prefabs via reflection if available
                if (heroPrefab != null)
                {
                    var field = typeof(EntityVisualizer).GetField("heroPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(viz, heroPrefab);
                }
                if (enemyPrefab != null)
                {
                    var field = typeof(EntityVisualizer).GetField("enemyPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(viz, enemyPrefab);
                }
                if (projectilePrefab != null)
                {
                    var field = typeof(EntityVisualizer).GetField("projectilePrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(viz, projectilePrefab);
                }
                
                Debug.Log("[Setup] Created EntityVisualizer");
            }
            
            // GameInitializer
            if (FindFirstObjectByType<GameInitializer>() == null)
            {
                GameObject initObj = new GameObject("GameInitializer");
                initObj.AddComponent<GameInitializer>();
                Debug.Log("[Setup] Created GameInitializer");
            }
            
            // WaveManager
            if (FindFirstObjectByType<WaveManager>() == null)
            {
                GameObject waveObj = new GameObject("WaveManager");
                waveObj.AddComponent<WaveManager>();
                Debug.Log("[Setup] Created WaveManager");
            }
            
            // DamageNumberSpawner
            if (FindFirstObjectByType<DamageNumberSpawner>() == null)
            {
                GameObject dmgObj = new GameObject("DamageNumberSpawner");
                var spawner = dmgObj.AddComponent<DamageNumberSpawner>();
                
                if (damageNumberPrefab != null)
                {
                    var field = typeof(DamageNumberSpawner).GetField("damageNumberPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(spawner, damageNumberPrefab);
                }
                
                Debug.Log("[Setup] Created DamageNumberSpawner");
            }
            
            // CombatEffectsManager
            if (FindFirstObjectByType<CombatEffectsManager>() == null)
            {
                GameObject fxObj = new GameObject("CombatEffectsManager");
                fxObj.AddComponent<CombatEffectsManager>();
                Debug.Log("[Setup] Created CombatEffectsManager");
            }
            
            Debug.Log("[Setup] Scene setup complete!");
        }
    }
}

