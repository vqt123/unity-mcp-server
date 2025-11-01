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
        private bool isInitialized = false;
        
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        
        void Start()
        {
            // Store initial camera settings from scene
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            
            // Only set default angle if rotation is at default (0,0,0)
            if (Quaternion.Angle(transform.rotation, Quaternion.identity) < 0.01f)
            {
                transform.rotation = Quaternion.Euler(angle, 0, 0);
                initialRotation = transform.rotation;
            }
        }
        
        void LateUpdate()
        {
            if (GameSimulation.Instance == null) return;
            
            var world = GameSimulation.Instance.Simulation.World;
            
            // Calculate center of heroes
            Vector3 center = Vector3.zero;
            int heroCount = 0;
            
            foreach (var heroId in world.HeroIds)
            {
                if (!world.TryGetHero(heroId, out var hero)) continue;
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
                foreach (var heroId in world.HeroIds)
                {
                    if (!world.TryGetHero(heroId, out var hero)) continue;
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
                
                // Target position - offset from center based on initial camera offset
                Vector3 offsetFromInitial = initialPosition - Vector3.zero; // Offset from origin
                targetPosition = center + Vector3.up * adjustedHeight - Vector3.forward * adjustedHeight * 0.5f;
                
                // Only start following if camera was already following (not at initial scene position)
                // This prevents animating from scene-set camera to calculated position
                if (isInitialized)
                {
                    // Smooth move to follow heroes
                    transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
                }
                else
                {
                    // Check if camera is at its initial scene position
                    // If so, maintain it; otherwise start following
                    float distFromInitial = Vector3.Distance(transform.position, initialPosition);
                    if (distFromInitial < 0.1f)
                    {
                        // Camera is still at initial position - maintain scene settings
                        // Don't animate yet
                        return;
                    }
                    else
                    {
                        // Camera has moved (maybe by other system) - start following
                        isInitialized = true;
                        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
                    }
                }
            }
        }
    }
}

