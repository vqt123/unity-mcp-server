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
        Vector3 centerPosition = Vector3.zero;
        float spacing = 2f;
        
        // Get all hero types from config
        var heroTypes = ConfigManager.Instance.GetAllHeroes();
        
        // Spawn each hero type
        for (int i = 0; i < heroTypes.Count; i++)
        {
            // Calculate position (line them up horizontally)
            Vector3 position = centerPosition + new Vector3((i - 1) * spacing, 0, 0);
            
            // Create hero cube
            GameObject heroCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            heroCube.name = heroTypes[i].type;
            heroCube.transform.position = position;
            heroCube.tag = "Player";
            
            // Add Hero component and initialize
            Hero hero = heroCube.AddComponent<Hero>();
            hero.Initialize(heroTypes[i].type, i); // Pass index (0, 1, 2)
            
            Debug.Log($"[Main] Spawned {heroTypes[i].type} at {position}");
        }
    }
}
