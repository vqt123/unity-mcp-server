using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        // Ensure ConfigManager is loaded
        ConfigManager.Instance.LoadConfigs();
        
        // Spawn all 3 heroes in the center with slight offsets
        SpawnHeroes();
    }
    
    void SpawnHeroes()
    {
        // GameManager now handles hero spawning dynamically
        // Heroes are selected by player at battle start and on level up
        Debug.Log("[Main] Hero spawning is now handled by GameManager");
    }
}
