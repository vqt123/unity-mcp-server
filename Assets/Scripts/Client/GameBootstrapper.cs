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
        
        [Header("Projectile FX Prefabs")]
        [Tooltip("Default projectile FX - used for Bow, Sword, and other default weapons")]
        [SerializeField] private GameObject projectileFXDefault;
        [Tooltip("Fireball FX - used for Firewand weapon")]
        [SerializeField] private GameObject projectileFXFireball;
        
        void Awake()
        {
            if (autoSetup)
            {
                SetupGame();
            }
        }
        
        private void SetupGame()
        {
            Debug.Log($"[GameBootstrapper] ========== SETUP START ==========");
            Debug.Log($"[GameBootstrapper] Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            
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
            
            // 1a. Check for replay mode
            var replayCommands = ReplayStarter.GetAndClearReplayCommands();
            var replayHashes = ReplayStarter.GetAndClearReplayHashes();
            int replayStopTick = ReplayStarter.GetAndClearReplayStopTick();
            if (replayCommands != null && replayCommands.Count > 0)
            {
                GameLogger.Log($"[Bootstrap] REPLAY MODE DETECTED - {replayCommands.Count} commands to replay, {replayHashes?.Count ?? 0} state hashes, stopTick={replayStopTick}");
                foreach (var cmd in replayCommands)
                {
                    GameLogger.Log($"[Bootstrap]   - Replay command: {cmd.GetType().Name} at tick {cmd.Tick}");
                }
                GameSimulation.Instance.StartReplay(replayCommands, replayHashes, replayStopTick);
                GameLogger.Log($"[Bootstrap] ✓ Started replay with {replayCommands.Count} commands, IsReplaying={GameSimulation.Instance.IsReplaying}");
            }
            else
            {
                // Start recording for new arena
                GameSimulation.Instance.StartRecording();
                GameLogger.Log("[Bootstrap] ✓ Started command recording (NOT replay mode)");
            }
            
            // 2. Create WeaponConfigDatabase (loads weapon configs from Resources)
            if (WeaponConfigDatabase.Instance == null)
            {
                GameObject weaponDbObj = new GameObject("WeaponConfigDatabase");
                weaponDbObj.AddComponent<WeaponConfigDatabase>();
                GameLogger.Log("[Bootstrap] ✓ Created WeaponConfigDatabase");
            }
            
            // 2a. Create HeroConfigDatabase (loads hero configs from Resources)
            if (HeroConfigDatabase.Instance == null)
            {
                GameObject heroDbObj = new GameObject("HeroConfigDatabase");
                heroDbObj.AddComponent<HeroConfigDatabase>();
                GameLogger.Log("[Bootstrap] ✓ Created HeroConfigDatabase");
            }
            
            // 2a1. Create EnemyConfigDatabase (loads enemy configs from Resources)
            if (EnemyConfigDatabase.Instance == null)
            {
                GameObject enemyDbObj = new GameObject("EnemyConfigDatabase");
                enemyDbObj.AddComponent<EnemyConfigDatabase>();
                GameLogger.Log("[Bootstrap] ✓ Created EnemyConfigDatabase");
            }
            
            // 2b. Register bridge so Shared assembly can use HeroConfigSO
            HeroConfigConverter.RegisterBridge();
            GameLogger.Log("[Bootstrap] ✓ Registered HeroConfig bridge");
            
            // 2c. Register weapon config bridge
            WeaponConfigBridge.RegisterBridge();
            GameLogger.Log("[Bootstrap] ✓ Registered WeaponConfig bridge");
            
            // 2d. Register hero level bridge
            HeroLevelBridge.RegisterBridge();
            GameLogger.Log("[Bootstrap] ✓ Registered HeroLevel bridge");
            
            // 3. Create EntityVisualizer (creates GameObjects from events)
            GameObject visualizerObj = new GameObject("EntityVisualizer");
            EntityVisualizer visualizer = visualizerObj.AddComponent<EntityVisualizer>();
            
            // Auto-load prefabs from Resources if not assigned
            if (projectilePrefab == null)
            {
                projectilePrefab = Resources.Load<GameObject>("Projectile");
                if (projectilePrefab != null)
                {
                    GameLogger.Log("[Bootstrap] Loaded Projectile prefab from Resources");
                }
            }
            
            // Load ProjectileFX prefabs if not assigned (try multiple paths)
            if (projectileFXDefault == null)
            {
                projectileFXDefault = Resources.Load<GameObject>("ProjectileFX");
                if (projectileFXDefault == null)
                {
                    projectileFXDefault = Resources.Load<GameObject>("ProjectileFXDefault");
                }
                if (projectileFXDefault != null)
                {
                    GameLogger.Log("[Bootstrap] Loaded ProjectileFX default prefab from Resources");
                }
            }
            
            if (projectileFXFireball == null)
            {
                projectileFXFireball = Resources.Load<GameObject>("ProjectileFXFireball");
                if (projectileFXFireball != null)
                {
                    GameLogger.Log("[Bootstrap] Loaded ProjectileFXFireball prefab from Resources");
                }
            }
            
            // Assign prefabs using public method
            visualizer.SetPrefabs(heroPrefab, enemyPrefab, projectilePrefab);
            visualizer.SetProjectileFXPrefabs(projectileFXDefault, projectileFXFireball);
            GameLogger.Log($"[Bootstrap] ✓ Created EntityVisualizer - Prefabs: Hero={heroPrefab!=null}, Enemy={enemyPrefab!=null}, Proj={projectilePrefab!=null}, FXDefault={projectileFXDefault!=null}, FXFireball={projectileFXFireball!=null}");
            
            // 4. Create PlayerDataManager if not already present (persistent - must be first)
            if (PlayerDataManager.Instance == null)
            {
                GameObject dataObj = new GameObject("PlayerDataManager");
                dataObj.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(dataObj);
                GameLogger.Log("[Bootstrap] ✓ Created PlayerDataManager");
            }
            
            // 5. Create GoldManager (tracks and awards gold)
            if (GoldManager.Instance == null)
            {
                GameObject goldObj = new GameObject("GoldManager");
                goldObj.AddComponent<GoldManager>();
                GameLogger.Log("[Bootstrap] ✓ Created GoldManager");
            }
            
            // 6. Create EnergyManager (manages energy regeneration)
            if (EnergyManager.Instance == null)
            {
                GameObject energyObj = new GameObject("EnergyManager");
                energyObj.AddComponent<EnergyManager>();
                GameLogger.Log("[Bootstrap] ✓ Created EnergyManager");
            }
            
            // 6a. Create GemsManager (manages gems currency)
            if (GemsManager.Instance == null)
            {
                GameObject gemsObj = new GameObject("GemsManager");
                gemsObj.AddComponent<GemsManager>();
                GameLogger.Log("[Bootstrap] ✓ Created GemsManager");
            }
            
            // 6b. Give starting rewards (300 gold, 100 gems) and set player level to 1
            if (GoldManager.Instance != null && PlayerDataManager.Instance != null)
            {
                // Give 300 gold
                GoldManager.Instance.AddGold(300);
                
                // Give 100 gems
                if (GemsManager.Instance != null)
                {
                    GemsManager.Instance.AddGems(100);
                }
                
                // Reset player level to 0 at arena start (player level is arena-specific)
                PlayerDataManager.Instance.SetPlayerLevel(0);
                PlayerDataManager.Instance.PlayerBlob.upgradesChosenAtLevel1 = 0; // Reset upgrade counter
                GameLogger.Log("[Bootstrap] ✓ Reset player level to 0 for arena start");
                
                GameLogger.Log("[Bootstrap] ✓ Gave starting rewards: 300 gold, 100 gems");
            }
            
            // 7. Create PartySpawner (spawns heroes from party)
            GameObject partySpawnerObj = new GameObject("PartySpawner");
            partySpawnerObj.AddComponent<PartySpawner>();
            GameLogger.Log("[Bootstrap] ✓ Created PartySpawner");
            
            // 8. Create WaveManager (spawns enemies)
            GameObject waveObj = new GameObject("WaveManager");
            waveObj.AddComponent<WaveManager>();
            GameLogger.Log("[Bootstrap] ✓ Created WaveManager");
            
            // 9. Create ArenaUI (exit button, gold display)
            GameObject arenaUIObj = new GameObject("ArenaUI");
            arenaUIObj.AddComponent<ArenaUI>();
            GameLogger.Log("[Bootstrap] ✓ Created ArenaUI");
            
            // 10. Create DamageNumberSpawner (floating damage numbers) - always create it
            GameObject dmgNumObj = new GameObject("DamageNumberSpawner");
            DamageNumberSpawner spawner = dmgNumObj.AddComponent<DamageNumberSpawner>();
            if (damageNumberPrefab != null)
            {
                var field = typeof(DamageNumberSpawner).GetField("damageNumberPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(spawner, damageNumberPrefab);
            }
                GameLogger.Log("[Bootstrap] ✓ Created DamageNumberSpawner");
            
            // 11. Create CombatEffectsManager (VFX & audio)
            GameObject effectsObj = new GameObject("CombatEffectsManager");
            effectsObj.AddComponent<CombatEffectsManager>();
            GameLogger.Log("[Bootstrap] ✓ Created CombatEffectsManager");
            
            // 9. Create CameraController if Main Camera exists
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<CameraController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController>();
                GameLogger.Log("[Bootstrap] ✓ Added CameraController to Main Camera");
            }
            
            // 10. Create SimulationDebugger for debug info
            GameObject debugObj = new GameObject("SimulationDebugger");
            debugObj.AddComponent<SimulationDebugger>();
            GameLogger.Log("[Bootstrap] ✓ Created SimulationDebugger");
            
            // 11. Create HeroUpgradeManager for gold-based upgrades
            GameObject heroUpgradeObj = new GameObject("HeroUpgradeManager");
            heroUpgradeObj.AddComponent<HeroUpgradeManager>();
            GameLogger.Log("[Bootstrap] ✓ Created HeroUpgradeManager");
            
            // 12. Create UpgradeUIManager for level-up upgrades
            GameObject upgradeObj = new GameObject("UpgradeUIManager");
            upgradeObj.AddComponent<UpgradeUIManager>();
            GameLogger.Log("[Bootstrap] ✓ Created UpgradeUIManager");
            
            // 13. Create CooldownUIManager for hero cooldown displays
            GameObject cooldownMgrObj = new GameObject("CooldownUIManager");
            cooldownMgrObj.AddComponent<CooldownUIManager>();
            GameLogger.Log("[Bootstrap] ✓ Created CooldownUIManager");
            
            // 14. Create WaveProgressUI for wave completion progress
            GameObject waveProgressObj = new GameObject("WaveProgressUI");
            waveProgressObj.AddComponent<WaveProgressUI>();
            GameLogger.Log("[Bootstrap] ✓ Created WaveProgressUI");
            
            // 15. Create XPBarUI for hero experience display
            GameObject xpBarObj = new GameObject("XPBarUI");
            xpBarObj.AddComponent<XPBarUI>();
            GameLogger.Log("[Bootstrap] ✓ Created XPBarUI");
            
            GameLogger.Log("[Bootstrap] ========== GAME SETUP COMPLETE ==========");
        }
    }
}

