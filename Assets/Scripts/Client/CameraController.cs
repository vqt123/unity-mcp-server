using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Simple camera controller that follows the average position of heroes
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float height = 15f;
        [SerializeField] private float angle = 45f;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float minZoom = 10f;
        [SerializeField] private float maxZoom = 30f;
        
        private Vector3 targetPosition;
        
        void Start()
        {
            // Set initial camera angle
            transform.rotation = Quaternion.Euler(angle, 0, 0);
        }
        
        void LateUpdate()
        {
            if (GameSimulation.Instance == null) return;
            
            var world = GameSimulation.Instance.Simulation.World;
            
            // Calculate center of heroes
            Vector3 center = Vector3.zero;
            int heroCount = 0;
            
            foreach (var heroKvp in world.Heroes)
            {
                var hero = heroKvp.Value;
                if (hero.IsAlive)
                {
                    center += new Vector3(
                        (float)hero.Position.X.ToDouble(),
                        0f,
                        (float)hero.Position.Y.ToDouble()
                    );
                    heroCount++;
                }
            }
            
            if (heroCount > 0)
            {
                center /= heroCount;
                
                // Calculate distance based on spread
                float maxDist = 0f;
                foreach (var heroKvp in world.Heroes)
                {
                    var hero = heroKvp.Value;
                    if (hero.IsAlive)
                    {
                        Vector3 heroPos = new Vector3(
                            (float)hero.Position.X.ToDouble(),
                            0f,
                            (float)hero.Position.Y.ToDouble()
                        );
                        float dist = Vector3.Distance(center, heroPos);
                        if (dist > maxDist) maxDist = dist;
                    }
                }
                
                // Adjust height based on spread
                float adjustedHeight = Mathf.Clamp(height + maxDist * 0.5f, minZoom, maxZoom);
                
                // Target position
                targetPosition = center + Vector3.up * adjustedHeight - Vector3.forward * adjustedHeight * 0.5f;
                
                // Smooth move
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            }
        }
    }
}

