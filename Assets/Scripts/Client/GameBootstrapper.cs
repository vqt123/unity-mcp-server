using UnityEngine;
using ArenaGame.Shared.Core;

namespace ArenaGame.Client
{
    /// <summary>
    /// Automatically sets up the game scene with all required managers
    /// Add this to an empty GameObject in your scene to auto-start the game
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Auto-Create Managers")]
        [SerializeField] private bool autoSetup = true;
        
        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject damageNumberPrefab;
        
        void Awake()
        {
            if (autoSetup)
            {
                SetupGame();
            }
        }
        
        private void SetupGame()
        {
            // 0. Initialize logging FIRST
            GameLogger.Initialize(new UnityLogger());
            GameLogger.Log("[Bootstrap] ========== GAME SETUP START ==========");
            GameLogger.Log("[Bootstrap] Logger initialized");
            
            // 1. Create GameSimulation (runs the simulation loop)
            if (GameSimulation.Instance == null)
            {
                GameObject simObj = new GameObject("GameSimulation");
                simObj.AddComponent<GameSimulation>();
                GameLogger.Log("[Bootstrap] ✓ Created GameSimulation");
            }
            
            // 2. Create EntityVisualizer (creates GameObjects from events)
            GameObject visualizerObj = new GameObject("EntityVisualizer");
            EntityVisualizer visualizer = visualizerObj.AddComponent<EntityVisualizer>();
            
            // Assign prefabs using public method
            visualizer.SetPrefabs(heroPrefab, enemyPrefab, projectilePrefab);
            GameLogger.Log($"[Bootstrap] ✓ Created EntityVisualizer - Prefabs: Hero={heroPrefab!=null}, Enemy={enemyPrefab!=null}, Proj={projectilePrefab!=null}");
            
            // 3. Create PlayerDataManager if not already present (persistent)
            if (PlayerDataManager.Instance == null)
            {
                GameObject dataObj = new GameObject("PlayerDataManager");
                dataObj.AddComponent<PlayerDataManager>();
                GameLogger.Log("[Bootstrap] ✓ Created PlayerDataManager");
            }
            
            // 4. Create PartySpawner (spawns heroes from party)
            GameObject partySpawnerObj = new GameObject("PartySpawner");
            partySpawnerObj.AddComponent<PartySpawner>();
            GameLogger.Log("[Bootstrap] ✓ Created PartySpawner");
            
            // 5. Create WaveManager (spawns enemies)
            GameObject waveObj = new GameObject("WaveManager");
            waveObj.AddComponent<WaveManager>();
            GameLogger.Log("[Bootstrap] ✓ Created WaveManager");
            
            // 6. Create DamageNumberSpawner (floating damage numbers)
            if (damageNumberPrefab != null)
            {
                GameObject dmgNumObj = new GameObject("DamageNumberSpawner");
                DamageNumberSpawner spawner = dmgNumObj.AddComponent<DamageNumberSpawner>();
                typeof(DamageNumberSpawner).GetField("damageNumberPrefab", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(spawner, damageNumberPrefab);
                GameLogger.Log("[Bootstrap] ✓ Created DamageNumberSpawner");
            }
            
            // 7. Create CombatEffectsManager (VFX & audio)
            GameObject effectsObj = new GameObject("CombatEffectsManager");
            effectsObj.AddComponent<CombatEffectsManager>();
            GameLogger.Log("[Bootstrap] ✓ Created CombatEffectsManager");
            
            // 8. Create CameraController if Main Camera exists
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<CameraController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController>();
                GameLogger.Log("[Bootstrap] ✓ Added CameraController to Main Camera");
            }
            
            // 9. Create SimulationDebugger for debug info
            GameObject debugObj = new GameObject("SimulationDebugger");
            debugObj.AddComponent<SimulationDebugger>();
            GameLogger.Log("[Bootstrap] ✓ Created SimulationDebugger");
            
            // 10. Create UpgradeUIManager for level-up upgrades
            GameObject upgradeObj = new GameObject("UpgradeUIManager");
            upgradeObj.AddComponent<UpgradeUIManager>();
            GameLogger.Log("[Bootstrap] ✓ Created UpgradeUIManager");
            
            // 11. Create CooldownUIManager for hero cooldown displays
            GameObject cooldownMgrObj = new GameObject("CooldownUIManager");
            cooldownMgrObj.AddComponent<CooldownUIManager>();
            GameLogger.Log("[Bootstrap] ✓ Created CooldownUIManager");
            
            // 12. Create WaveProgressUI for wave completion progress
            GameObject waveProgressObj = new GameObject("WaveProgressUI");
            waveProgressObj.AddComponent<WaveProgressUI>();
            GameLogger.Log("[Bootstrap] ✓ Created WaveProgressUI");
            
            GameLogger.Log("[Bootstrap] ========== GAME SETUP COMPLETE ==========");
        }
    }
}

